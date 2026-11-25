using System;
using System.Reflection;
using HarmonyLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 施法动画事件重放 + 特效主动同步（v3.0.5，HKMP 式"事件 → 本地重放"架构）。
    ///
    /// 为什么需要它（根因，不是调参能解决的）：
    ///   旧的逐层动画镜像把"一次性施法状态"也当成持续状态硬追——每 10Hz 发 hash +
    ///   normalizedTime，接收端拿 predN 反复 cl.Play()。一次性动画播完后状态机停在末帧，
    ///   normalizedTime 一路涨到 200+，于是 replay 每帧硬拉，画面就是抽搐/断层，
    ///   法术"动态动画没结合"。调静音阈值、调预测窗口都只是缓解。
    ///
    /// HKMP 的做法：不传 Animator 状态，只在动作事件触发时发一个轻量 clipId；
    ///   接收端从 0 重放一次本地动画 + 本地粒子。传的是"发生了什么"，不是"现在第几帧"。
    ///
    /// RL2 落点（两段）：
    ///   1) 动画：patch AbilityAnimBehaviour.OnStateEnter，本地玩家进入施法状态就广播
    ///      layer + shortNameHash；接收端 cl.Play(hash, layer, 0) 从头重放，
    ///      并在 1.5s 接管窗口内让逐层镜像跳过该层，避免两套机制打架。
    ///   2) 特效（v3.0.5 补）：只重放动画不够——游戏特效靠 EffectTriggerAnimBehaviour
    ///      在分身 Animator 上再次触发，但它会被 EffectManager 的禁用列表吞掉，
    ///      且 AnimBehaviourCondition 依赖的参数（尤其 Trigger）根本传不过网络。
    ///      这正是 HKMP 为每类 clip 单独实现 IAnimationEffect、收到事件后主动
    ///      Play() 的原因。这里 patch EffectTriggerAnimBehaviour.InvokeTrigger，
    ///      本地玩家触发的每个动画特效都把 EffectTriggerEntry 关键字段打包广播；
    ///      接收端重建 entry，直接调用原生 EffectTrigger.InvokeTrigger 在分身上播放。
    ///      特效与分身 Animator 的禁用列表/参数条件彻底解耦。
    /// </summary>
    internal static class AbilityFxSync
    {
        /// <summary>事件接管窗口：覆盖蓄力(Tell)+出手(Attack)的完整施法时长。</summary>
        private const float FxTakeoverSeconds = 1.5f;
        /// <summary>分身 Animator 特效豁免窗口（仅特效桥未就绪时的旧路径用）。</summary>
        private const float GraceSeconds = 1.7f;

        /// <summary>特效主动桥是否成功 patch（失败时回退旧的 GrantGrace 路径）。</summary>
        private static bool _effectBridgeReady;
        private static float _lastLog;
        private static float _lastFxLog;
        /// <summary>
        /// v3.0.6：本地施法窗口。原本 patch InvokeTrigger 后把所有动画特效都同步了，
        /// 真机日志显示 99% 是 FootstepDust/LandedDust/GroundDirectionalDust 这类环境灰尘
        /// —— 刷屏、NRE、还和投射物自带特效叠成"毒弹两发"。HKMP 的做法是只为施法 clip
        /// 发事件。这里用 OnStateEnterPostfix（仅在真身进入 Tell/Attack 时触发）打一个
        /// 时间窗，特效桥只在这个窗口内发包，从根本上排除跑/跳/落地灰尘。
        /// </summary>
        private static float _localCastUntil;
        /// <summary>
        /// v3.0.12：虚冲特效窗口。虚冲专属残影(DashSilhouette/DashLines)靠 Animator 的
        /// VoidDash bool 条件触发，而网络参数快照会在 10Hz 把分身 bool 覆盖回 false、
        /// 层 scrub 会把 dash 特效状态硬拉回 —— 分身永远播不出虚冲残影。
        /// 走 HKMP 式主动桥：虚冲时开这个窗口，放行真身 dash 特效经 SpellFx 广播，
        /// 接收端直接 InvokeTrigger 在分身上播放，彻底与 Animator 参数/状态机解耦。
        /// </summary>
        private static float _dashFxUntil;
        /// <summary>同一特效名的最小重发间隔，消除 OnStateEnter+Update 双触发导致的双发。</summary>
        private static readonly System.Collections.Generic.Dictionary<string, float> _lastFxSent =
            new System.Collections.Generic.Dictionary<string, float>();
        private static readonly System.Collections.Generic.Dictionary<string, float> _lastFxPlayed =
            new System.Collections.Generic.Dictionary<string, float>();
        // v3.0.17：接收去重必须带 OwnerId。旧版只按特效名去重，两个玩家 0.08s 内
        // 放同名特效会互吞 —— 表现就是“其中一人的特效偶尔没出现”。
        private static readonly System.Collections.Generic.Dictionary<string, float> _lastCodeFxSent =
            new System.Collections.Generic.Dictionary<string, float>();
        private static readonly System.Collections.Generic.Dictionary<string, float> _lastCodeFxPlayed =
            new System.Collections.Generic.Dictionary<string, float>();
        /// <summary>动画桥 InvokeTrigger 执行期间，中央 PlayEffect 桥必须让路，避免同发特效双份。</summary>
        private static float _animFxNestingUntil;
        /// <summary>正在对端分身上重放 CodeFx，防止中央桥把它再发回去形成回声。</summary>
        private static bool _playingRemoteCodeFx;
        /// <summary>
        /// v3.0.10：动画事件通道去重。真机日志显示一次毒弹施法，OnStateEnter 连发 12 次
        /// 同一个 (layer,hash)，对端每次都 ReplayAbilityLayer->Play(hash,0) 从头播，
        /// 于是施法特效被叠加成"两发毒弹"。按 (layer,hash) 记录上次发包时刻，
        /// 极短窗口内只发一次；连招/不同状态 hash 不同，不受影响。
        /// key = layer<<32 | (uint)hash。
        /// </summary>
        private static readonly System.Collections.Generic.Dictionary<long, float> _lastAnimFxSent =
            new System.Collections.Generic.Dictionary<long, float>();
        private const float AnimFxDedupWindow = 0.35f;
        /// <summary>
        /// v3.0.9：发送端按 animator layer 记录本次施法已广播的特效名，OnStateExit 时
        /// 逐个广播取消（对应 Boss Room m_ActiveNodes 里登记的 effect）。
        /// </summary>
        private static readonly System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<string>> _fxByLayer =
            new System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<string>>();
        /// <summary>接收端按 Owner 记录正在播放的特效名，供取消事件精确 stop。</summary>
        private static readonly System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<string>> _remoteFxByOwner =
            new System.Collections.Generic.Dictionary<int, System.Collections.Generic.HashSet<string>>();

        internal static bool Active
        {
            get
            {
                var nm = NetworkManager.Instance;
                return nm != null && nm.Connected;
            }
        }

        /// <summary>
        /// v3.0.12：DashFxSync 在确认本次是虚空冲刺时调用，开一个覆盖虚冲全程的窗口。
        /// 期间 EffectTriggerAnimBehaviour.InvokeTrigger 里的 dash 类特效允许走主动桥发包。
        /// 2.4s 覆盖虚冲按住方向键时的双倍时长（基础 ~1.2s）。
        /// </summary>
        internal static void OpenDashFxWindow()
        {
            _dashFxUntil = Time.unscaledTime + 2.4f;
        }

        internal static void Apply(Harmony harmony)
        {
            // v3.0.3-hotfix：整段包在 try 里，任何反射/patch 异常都必须就地吞掉。
            // v3.0.2 曾因这里抛异常冒泡到 Plugin.Awake，中断了后续整条 patch 链，
            // 导致 MainMenuHubButton.Apply 没执行 —— 主菜单「联机中心」按钮消失。
            try
            {
                var t = AccessTools.TypeByName("AbilityAnimBehaviour");
                if (t == null)
                {
                    LogW("[ABILFX] 未找到 AbilityAnimBehaviour 类型，跳过施法事件重放（不影响其它功能）");
                    return;
                }
                // 显式带参数类型：签名不符时返回 null 而非 patch 错方法导致 postfix 注入失败。
                var m = AccessTools.Method(t, "OnStateEnter",
                    new[] { typeof(Animator), typeof(AnimatorStateInfo), typeof(int) });
                if (m == null)
                {
                    LogW("[ABILFX] 未找到 OnStateEnter(Animator,AnimatorStateInfo,int)，跳过施法事件重放");
                    return;
                }
                TryPatch(harmony, m, "OnStateEnterPrefix", "施法事件发送+窗口", postfixName: "OnStateEnterPostfix");

                // v3.0.9：施法动画节点退出 -> 广播特效取消（Boss Room OnStateExit Shutdown 式）。
                var exitM = AccessTools.Method(t, "OnStateExit",
                    new[] { typeof(Animator), typeof(AnimatorStateInfo), typeof(int) });
                if (exitM != null)
                    TryPatch(harmony, exitM, null, "施法特效取消发送", postfixName: "OnStateExitPostfix");

                // v3.0.5：特效主动桥——与动画事件同一入口注册，失败独立隔离。
                PatchEffectBridge(harmony);
                // v3.0.17：代码直调 EffectManager.PlayEffect 的中央桥。即使动画桥
                // 反射失败，这条仍要注册——技能代码直接调的特效本来就走它。
                PatchCodeFxBridge(harmony);
            }
            catch (Exception ex)
            {
                LogW("[ABILFX] 初始化异常，已隔离（不影响其它功能）: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>
        /// patch 游戏私有的 EffectTriggerAnimBehaviour.InvokeTrigger(GameObject, Animator, EffectTriggerEntry, int)。
        /// 这个方法就是所有"动画驱动特效"的统一出口：本地玩家每放一个新特效，我们就在 postfix
        /// 里把它打包广播，接收端直接重建 entry 调用原生 EffectTrigger.InvokeTrigger。
        /// </summary>
        private static void PatchEffectBridge(Harmony harmony)
        {
            try
            {
                var t = AccessTools.TypeByName("EffectTriggerAnimBehaviour");
                if (t == null)
                {
                    LogW("[ABILFX] 未找到 EffectTriggerAnimBehaviour，特效桥不可用（回退旧豁免路径）");
                    return;
                }
                var m = AccessTools.Method(t, "InvokeTrigger",
                    new[] { typeof(GameObject), typeof(Animator), typeof(EffectTriggerEntry), typeof(int) });
                if (m == null)
                {
                    LogW("[ABILFX] 未找到 EffectTriggerAnimBehaviour.InvokeTrigger，特效桥不可用");
                    return;
                }
                var post = new HarmonyMethod(typeof(AbilityFxSync).GetMethod("EffectTriggerInvokePostfix",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                if (post == null)
                {
                    LogW("[ABILFX] 特效桥 postfix 反射失败");
                    return;
                }
                var pre = new HarmonyMethod(typeof(AbilityFxSync).GetMethod("EffectTriggerInvokePrefix",
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                harmony.Patch(m, prefix: pre, postfix: post);
                _effectBridgeReady = true;
                Log("[ABILFX] 补丁 OK: 特效主动桥(EffectTriggerAnimBehaviour.InvokeTrigger)");
            }
            catch (Exception ex)
            {
                _effectBridgeReady = false;
                LogW("[ABILFX] 特效主动桥注册失败，回退旧豁免路径: " + Plugin.PatchError(ex));
            }
        }

        // ---------------- 发送端 ----------------

        /// <summary>
        /// v3.0.17：施法窗口必须在 OnStateEnter 的 prefix 打开。旧版放在 postfix，
        /// 同一动画状态里若 EffectTriggerAnimBehaviour.OnStateEnter 排在 AbilityAnimBehaviour
        /// 前面，EffectStartType.None 的首帧特效会在窗口打开前被过滤掉 —— 这正是
        /// “有些法术特效偶尔就是不出现”的时序根因。prefix 只打窗口，发包仍由 postfix 负责。
        /// </summary>
        internal static void OnStateEnterPrefix(AbilityAnimBehaviour __instance, Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            try
            {
                if (!Active || DownedState.LocalDown) return;
                if (__instance == null || animator == null) return;
                if (stateInfo.shortNameHash == 0 || layerIndex < 0 || layerIndex > 15) return;
                var ps = PlayerSync.Instance;
                if (ps == null || ps.LocalPlayer == null) return;
                if (animator.transform.root != ps.LocalPlayer.transform.root) return;
                _localCastUntil = Time.unscaledTime + FxTakeoverSeconds;
            }
            catch { }
        }

        /// <summary>
        /// AbilityAnimBehaviour.OnStateEnter postfix：本地玩家进入任一施法状态
        /// (TellIntro/Tell/Attack_Intro/Attack/Exit) 就广播一个轻量事件。
        /// 只发 stateInfo.shortNameHash —— 它和逐层镜像采样的 hash 是同一个值，
        /// 接收端可以直接 cl.Play(hash, layer, 0) 命中同一状态。
        /// </summary>
        internal static void OnStateEnterPostfix(AbilityAnimBehaviour __instance, Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            try
            {
                if (!Active) return;
                if (__instance == null || animator == null) return;

                // v3.0.13：倒地观战时本地真身是 inactive 尸体，其 Animator 仍在被
                // 观战/重放驱动，OnStateEnter 会当成"我又施法了"再发一遍 → 对端
                // 收到后重放，主机画面出现两份施法（日志：发送后又重放同 hash）。
                // 倒地端不产生任何施法发送；取消包(OnStateExit)不挡，保证能清理残留。
                if (DownedState.LocalDown) return;

                var nm = NetworkManager.Instance;
                var ps = PlayerSync.Instance;
                if (nm == null || ps == null || ps.LocalPlayer == null) return;

                // 只同步本地玩家：分身克隆树上也有 AbilityAnimBehaviour，
                // 它播动画时同样会触发 OnStateEnter，不过滤会形成回声风暴。
                if (animator.transform.root != ps.LocalPlayer.transform.root) return;

                int hash = stateInfo.shortNameHash;
                if (hash == 0) return;
                if (layerIndex < 0 || layerIndex > 15) return;

                // v3.0.10：同一 (层,状态) 的重复 OnStateEnter 去重。这是毒弹双发根因——
                // 一次施法游戏连续 enter 12 次同一状态，旧版每次都发 AbilityFx，
                // 对端每收一次就从头重放一次施法动画 -> 特效叠成两发。
                // 窗口 0.35s 足够覆盖单次施法的重复 enter，又不会吞掉连招的下一发
                //（连招是不同 hash 或间隔 > 0.35s）。
                long animKey = ((long)layerIndex << 32) | (uint)hash;
                float lastAnimT;
                if (_lastAnimFxSent.TryGetValue(animKey, out lastAnimT)
                    && Time.unscaledTime - lastAnimT < AnimFxDedupWindow) return;
                _lastAnimFxSent[animKey] = Time.unscaledTime;

                byte state = (byte)__instance.AnimatorStateType;

                // v3.0.6：打施法窗口——只有真身进入施法状态后的这段时间，特效桥才允许发包。
                // 这样跑/跳/落地/冲刺时的环境特效（CurrentAnimatorState 为 null）不会同步。
                _localCastUntil = Time.unscaledTime + FxTakeoverSeconds;

                var w = new NetDataWriter();
                w.Put((byte)MsgType.AbilityFx);
                w.Put(nm.LocalPlayerId);
                w.Put((byte)layerIndex);
                w.Put(hash);
                w.Put(state);
                nm.SendAbilityFx(w);

                if (Time.unscaledTime - _lastLog > 1f)
                {
                    _lastLog = Time.unscaledTime;
                    Log("[ABILFX] 发送 layer=" + layerIndex + " hash=" + hash + " state=" + state);
                }
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _lastLog > 2f)
                {
                    _lastLog = Time.unscaledTime;
                    LogW("[ABILFX] 发送失败: " + Plugin.PatchError(ex));
                }
            }
        }

        /// <summary>
        /// v3.0.9：施法动画节点退出 postfix（借鉴 Boss Room AnimatorTriggeredSpecialFX
        /// 的 OnStateExit -> Shutdown）。把该层本次施法登记的所有特效名广播取消，
        /// 接收端停掉分身上的同名特效，避免法术播完后粒子残留/循环特效不停。
        /// 只处理本地真身；分身/怪物的 OnStateExit 一律忽略，防回声。
        /// </summary>
        internal static void OnStateExitPostfix(AbilityAnimBehaviour __instance, Animator animator,
            AnimatorStateInfo stateInfo, int layerIndex)
        {
            try
            {
                if (!_effectBridgeReady || !Active) return;
                if (__instance == null || animator == null) return;

                var nm = NetworkManager.Instance;
                var ps = PlayerSync.Instance;
                if (nm == null || ps == null || ps.LocalPlayer == null) return;
                if (animator.transform.root != ps.LocalPlayer.transform.root) return;

                System.Collections.Generic.HashSet<string> layerSet;
                if (!_fxByLayer.TryGetValue(layerIndex, out layerSet) || layerSet.Count == 0) return;

                // 逐个发取消包。特效名一般是 1-2 个，开销可忽略。
                foreach (string fxName in layerSet)
                {
                    // v3.0.12：虚冲残影也要能取消，不能再用含 Dash 的黑名单拦掉。
                    if (IsPureEnvironmentFx(fxName)) continue;
                    var w = new NetDataWriter();
                    w.Put((byte)MsgType.SpellFxStop);
                    w.Put(nm.LocalPlayerId);
                    w.Put(fxName);
                    nm.SendSpellFxStop(w);
                }
                // 该层的施法结束了，清空登记，下一次施法重新累积。
                _fxByLayer.Remove(layerIndex);

                if (Time.unscaledTime - _lastFxLog > 1f)
                {
                    _lastFxLog = Time.unscaledTime;
                    Log("[ABILFX] 发送特效取消 layer=" + layerIndex + " n=" + layerSet.Count);
                }
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _lastFxLog > 2f)
                {
                    _lastFxLog = Time.unscaledTime;
                    LogW("[ABILFX] 取消发送失败: " + Plugin.PatchError(ex));
                }
            }
        }

        /// <summary>
        /// v3.0.5：动画特效出口 postfix。把本地玩家触发的 EffectTriggerEntry 打包广播。
        /// 只取 InvokeTrigger 真正会用到的字段（EffectCategory/EffectStartType 等不参与播放），
        /// 方向用与原生实现完全相同的 GetEffectDirectionFromObject 重算，保证两端一致。
        /// </summary>
        internal static void EffectTriggerInvokePostfix(GameObject parentObj, Animator animator,
            EffectTriggerEntry entry, int layerIndex)
        {
            try
            {
                if (!_effectBridgeReady || !Active) return;
                if (entry == null || string.IsNullOrEmpty(entry.EffectTypeName)) return;
                if (animator == null || parentObj == null) return;

                // v3.0.13：倒地尸体动画仍会触发特效发包，形成双份特效回声。倒地端不发。
                if (DownedState.LocalDown) return;

                // v3.0.6 白名单：只在真身施法窗口内发包。游戏用 AbilityAnimBehaviourManager
                // 标记当前是否处于 Tell/Attack/Exit —— 走路/跳跃/落地的灰尘此时为 null，
                // 直接排除。这是 HKMP "只为注册的施法 clip 发事件" 的 RL2 对应实现。
                // v3.0.12：虚冲窗口由 DashFxSync 显式打开，走同一条主动桥；虚冲期间
                // CurrentAnimatorState 可能为 null，不能用施法条件卡它。
                float winNow = Time.unscaledTime;
                bool inCast = winNow < _localCastUntil;
                bool inDash = winNow < _dashFxUntil;
                if (!inCast && !inDash) return;
                if (inCast && AbilityAnimBehaviourManager.CurrentAnimatorState == null) return;

                var nm = NetworkManager.Instance;
                var ps = PlayerSync.Instance;
                if (nm == null || ps == null || ps.LocalPlayer == null) return;

                // 只同步本地真身触发的特效；分身/怪物/掉落物的一律不发，防回声风暴。
                if (animator.transform.root != ps.LocalPlayer.transform.root) return;

                // 去重：同一特效在 0.08s 内只发一次。EffectTriggerAnimBehaviour 的
                // OnStateEnter 和 OnStateUpdate 会对同一 entry 各调一次 InvokeTrigger，
                // 不去重就是"一次施法两发特效"。
                string fxName = entry.EffectTypeName;
                // v3.0.12：虚冲残影名含 Dash，旧版被 IsEnvironmentFx 一刀切吞掉。
                // 现在是：纯环境灰尘/死亡/跳跃/落地永远过滤；Dash 类只在虚冲窗口放行，
                // 普通冲刺仍不同步（用户明确说普通冲刺没有特效动画）。
                if (IsPureEnvironmentFx(fxName)) return;
                if (IsDashVisualFx(fxName) && !inDash) return;
                float nowT = Time.unscaledTime;
                float lastT;
                if (_lastFxSent.TryGetValue(fxName, out lastT) && nowT - lastT < 0.08f) return;
                _lastFxSent[fxName] = nowT;

                byte flips = 0;
                if (entry.LeftFlip) flips |= 1;
                if (entry.RightFlip) flips |= 2;
                if (entry.UpFlip) flips |= 4;
                if (entry.DownFlip) flips |= 8;

            var data = new SpellFxData
            {
                OwnerId = nm.LocalPlayerId,
                EffectName = fxName,
                    PositionOffset = entry.PositionOffset,
                    OffsetType = (byte)entry.OffsetType,
                    FollowTargetType = (byte)entry.FollowTargetType,
                    DeriveScale = (byte)entry.DeriveScale,
                    BaseScaleToDerive = entry.BaseScaleToDerive,
                    DeriveRotation = (byte)entry.DeriveRotation,
                    DeriveRotationFromHeading = entry.DeriveRotationFromHeading,
                    DurationType = (int)entry.DurationType,
                    Duration = entry.Duration,
                    EffectStopType = (byte)entry.EffectStopType,
                    Direction = (int)EffectTrigger.GetEffectDirectionFromObject(parentObj, entry.TriggerDirection),
                    AnimatorLayer = (byte)Mathf.Clamp(entry.AnimatorLayer, 0, 255),
                    DestroyOnRoomChange = entry.DestroyOnRoomChange,
                    StartingPosLerp = entry.StartingPosLerp,
                    LeftRotationZ = entry.LeftRotationZ,
                    RightRotationZ = entry.RightRotationZ,
                    UpRotationZ = entry.UpRotationZ,
                    DownRotationZ = entry.DownRotationZ,
                    Flips = flips,
                    DeriveFacing = (byte)entry.DeriveFacing
                };

                var w = new NetDataWriter();
                w.Put((byte)MsgType.SpellFx);
                data.Write(w);
                nm.SendSpellFx(w);

                // v3.0.9：登记到该动画层，等 OnStateExit 广播取消。用 entry.AnimatorLayer
                // （特效自己绑定的层），与 OnStateExit 的 layerIndex 同源，才能正确配对。
                int fxLayer = entry.AnimatorLayer;
                System.Collections.Generic.HashSet<string> layerSet;
                if (!_fxByLayer.TryGetValue(fxLayer, out layerSet))
                {
                    layerSet = new System.Collections.Generic.HashSet<string>();
                    _fxByLayer[fxLayer] = layerSet;
                }
                layerSet.Add(fxName);

                if (Time.unscaledTime - _lastFxLog > 1f)
                {
                    _lastFxLog = Time.unscaledTime;
                    Log("[ABILFX] 发送特效 " + data.EffectName + " layer=" + layerIndex);
                }
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _lastFxLog > 2f)
                {
                    _lastFxLog = Time.unscaledTime;
                    LogW("[ABILFX] 特效发送失败: " + Plugin.PatchError(ex));
                }
            }
        }

        /// <summary>
        /// v3.0.17：动画桥前缀。它随后会调用 EffectTrigger.InvokeTrigger ->
        /// EffectManager.PlayEffect。这条路径已经由 SpellFx 事件同步，中央桥必须让路，
        /// 否则同一发粒子会被发两遍（老问题“毒弹两发”换条路径复发）。
        /// </summary>
        private static void EffectTriggerInvokePrefix()
        {
            _animFxNestingUntil = Time.unscaledTime + 0.1f;
        }

        /// <summary>
        /// v3.0.17：给 EffectManager.PlayEffect 打 postfix，补动画桥覆盖不到的
        /// “代码直调特效”。反编译确认 CannonBallMuzzle_Effect / CloakPuff_Effect /
        /// LanceChargingUp_Effect / CriticalSuccess_Effect 等技能视觉都是代码直接
        /// PlayEffect，根本不经过 EffectTriggerAnimBehaviour —— 只同步动画桥时，
        /// 这类特效在对端永远不出现。这里只转发“源对象根部是本地真身”的特效。
        /// </summary>
        private static void PatchCodeFxBridge(Harmony harmony)
        {
            try
            {
                var m = AccessTools.Method(typeof(EffectManager), "PlayEffect",
                    new[] { typeof(GameObject), typeof(Animator), typeof(string), typeof(Vector3),
                            typeof(float), typeof(EffectStopType), typeof(EffectTriggerDirection) });
                if (m == null)
                {
                    LogW("[ABILFX] 未找到 EffectManager.PlayEffect，代码直调特效不同步");
                    return;
                }
                TryPatch(harmony, m, null, "代码直调特效中央桥", postfixName: "PlayEffectPostfix");
            }
            catch (Exception ex)
            {
                LogW("[ABILFX] 代码直调特效桥注册失败: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>EffectManager.PlayEffect postfix：本地真身触发的代码直调特效 -> CodeFx 包。</summary>
        internal static void PlayEffectPostfix(GameObject source, Animator sourceAnimator, string effectName,
            Vector3 effectPosition, float duration, EffectStopType stopType, EffectTriggerDirection direction)
        {
            try
            {
                if (!Active || string.IsNullOrEmpty(effectName)) return;
                // 动画桥路径由 SpellFx 负责；远端重放不能回声。
                if (_playingRemoteCodeFx || Time.unscaledTime < _animFxNestingUntil) return;
                if (DownedState.LocalDown) return;
                if (IsPureEnvironmentFx(effectName) || IsCodeFxBlacklisted(effectName)) return;
                if (float.IsNaN(effectPosition.x) || float.IsNaN(effectPosition.y) || float.IsNaN(effectPosition.z)
                    || float.IsInfinity(effectPosition.x) || float.IsInfinity(effectPosition.y)
                    || float.IsInfinity(effectPosition.z)) return;

                var nm = NetworkManager.Instance;
                var ps = PlayerSync.Instance;
                if (nm == null || ps == null || ps.LocalPlayer == null) return;
                if (source == null || source.transform.root != ps.LocalPlayer.transform.root) return;

                float now = Time.unscaledTime;
                string key = nm.LocalPlayerId + ":" + effectName;
                float last;
                if (_lastCodeFxSent.TryGetValue(key, out last) && now - last < 0.06f) return;
                _lastCodeFxSent[key] = now;

                var d = new CodeFxData
                {
                    OwnerId = nm.LocalPlayerId,
                    EffectName = effectName,
                    Position = effectPosition,
                    Duration = float.IsNaN(duration) || float.IsInfinity(duration)
                        ? 0f : Mathf.Clamp(duration, -10f, 100000f),
                    StopType = (byte)stopType,
                    Direction = (int)direction
                };
                var w = new NetDataWriter();
                w.Put((byte)MsgType.CodeFx);
                d.Write(w);
                nm.SendCodeFx(w);

                if (now - _lastFxLog > 1f)
                {
                    _lastFxLog = now;
                    Log("[ABILFX] 发送代码特效 " + effectName);
                }
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _lastFxLog > 2f)
                {
                    _lastFxLog = Time.unscaledTime;
                    LogW("[ABILFX] 代码特效发送失败: " + Plugin.PatchError(ex));
                }
            }
        }

        // ---------------- 接收端 ----------------

        /// <summary>收到远端施法事件：分身对应层从 0 重放 + 特效豁免。</summary>
        internal static void PlayRemoteFx(int ownerId, int layer, int hash, byte state)
        {
            var ps = PlayerSync.Instance;
            if (ps == null) return;
            GameObject go;
            if (!ps.RemotePlayers.TryGetValue(ownerId, out go) || go == null) return;

            var mirror = go.GetComponent<RemoteAnimatorMirror>();
            if (mirror == null) return;

            mirror.ReplayAbilityLayer(layer, hash);

            // v3.0.5：特效已由 SpellFx 事件主动播放，不再靠分身 Animator 状态机重放。
            // 只有特效桥 patch 失败（旧路径）时才限时放行 state 里的 EffectTrigger，
            // 否则同一发法术会播两遍粒子。
            if (!_effectBridgeReady)
            {
                float until = Time.unscaledTime + GraceSeconds;
                var animators = go.GetComponentsInChildren<Animator>(true);
                for (int i = 0; i < animators.Length; i++)
                {
                    if (animators[i] != null) DashFxSync.GrantGrace(animators[i], until);
                }
            }

            if (Time.unscaledTime - _lastLog > 1f)
            {
                _lastLog = Time.unscaledTime;
                Log("[ABILFX] 重放 P" + ownerId + " layer=" + layer + " hash=" + hash
                    + " state=" + state + " bridge=" + _effectBridgeReady);
            }
        }

        /// <summary>
        /// v3.0.5：收到远端动画特效事件——重建 EffectTriggerEntry，用原生
        /// EffectTrigger.InvokeTrigger 在分身上主动生成特效。
        /// 这是 HKMP "收到 clip 事件后主动 effect.Play()" 的 RL2 对应实现：
        /// 不经过 EffectTriggerAnimBehaviour，因此不受分身禁用列表、参数条件、Trigger 不可传影响。
        /// </summary>
        internal static void PlayRemoteSpellFx(SpellFxData d)
        {
            if (d == null || string.IsNullOrEmpty(d.EffectName)) return;
            // 接收端同样去重：防止中转/重发导致同一特效在分身上叠两遍。
            // v3.0.12：虚冲残影名含 Dash，必须放行；纯环境灰尘仍过滤。
            if (IsPureEnvironmentFx(d.EffectName)) return;
            float nowR = Time.unscaledTime;
            // v3.0.17：去重 key 加 OwnerId，否则两个玩家同帧放同名特效会互吞。
            string playKey = d.OwnerId + ":" + d.EffectName;
            float lastR;
            if (_lastFxPlayed.TryGetValue(playKey, out lastR) && nowR - lastR < 0.08f) return;
            _lastFxPlayed[playKey] = nowR;

            var ps = PlayerSync.Instance;
            if (ps == null) return;
            GameObject clone;
            if (!ps.RemotePlayers.TryGetValue(d.OwnerId, out clone) || clone == null) return;

            var entry = new EffectTriggerEntry
            {
                EffectTypeName = d.EffectName,
                PositionOffset = d.PositionOffset,
                OffsetType = (EffectOffsetType)d.OffsetType,
                FollowTargetType = (EffectTargetType)d.FollowTargetType,
                DeriveScale = (EffectTargetType)d.DeriveScale,
                BaseScaleToDerive = d.BaseScaleToDerive,
                DeriveRotation = (EffectTargetType)d.DeriveRotation,
                DeriveRotationFromHeading = d.DeriveRotationFromHeading,
                DurationType = (EffectDurationType)d.DurationType,
                Duration = d.Duration,
                EffectStopType = (EffectStopType)d.EffectStopType,
                TriggerDirection = (EffectTriggerDirection)d.Direction,
                AnimatorLayer = d.AnimatorLayer,
                DestroyOnRoomChange = d.DestroyOnRoomChange,
                StartingPosLerp = d.StartingPosLerp,
                LeftRotationZ = d.LeftRotationZ,
                RightRotationZ = d.RightRotationZ,
                UpRotationZ = d.UpRotationZ,
                DownRotationZ = d.DownRotationZ,
                LeftFlip = (d.Flips & 1) != 0,
                RightFlip = (d.Flips & 2) != 0,
                UpFlip = (d.Flips & 4) != 0,
                DownFlip = (d.Flips & 8) != 0,
                DeriveFacing = (EffectTargetType)d.DeriveFacing,
                EffectStartType = EffectStartType.None,
                NormalizedStartTime = 0f
            };

            Animator anim = null;
            var mirror = clone.GetComponent<RemoteAnimatorMirror>();
            if (mirror != null) anim = mirror.PrimaryCloneAnimator;
            if (anim == null) anim = clone.GetComponentInChildren<Animator>(true);
            GameObject parent = anim != null ? anim.gameObject : clone;

            Vector3 mid = parent.transform.position;
            try
            {
                var m = parent.GetComponent<IMidpointObj>();
                if (m != null) mid = m.Midpoint;
            }
            catch { }

            // 用发送端算好的方向，不在分身上重算：分身被 strip 过、没有 IEffectVelocity，
            // 重算只能靠 localScale 猜朝向，快速移动/空中攻击时会和本体方向相反。
            var dir = (EffectTriggerDirection)d.Direction;
            try
            {
                // PlayEffect 在 effectName 不在 EffectLibrary 时会抛异常；
                // 单个畸形/版本不匹配的名字绝不能把整个收包循环炸掉。
                EffectTrigger.InvokeTrigger(entry, parent, parent, mid, mid, dir, anim);

                // v3.0.9：登记该 Owner 正在播放的特效名，供取消事件精确 stop。
                System.Collections.Generic.HashSet<string> ownerSet;
                if (!_remoteFxByOwner.TryGetValue(d.OwnerId, out ownerSet))
                {
                    ownerSet = new System.Collections.Generic.HashSet<string>();
                    _remoteFxByOwner[d.OwnerId] = ownerSet;
                }
                ownerSet.Add(d.EffectName);
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _lastFxLog > 2f)
                {
                    _lastFxLog = Time.unscaledTime;
                    LogW("[ABILFX] 播放特效异常 " + d.EffectName + ": " + Plugin.PatchError(ex));
                }
                return;
            }

            if (Time.unscaledTime - _lastFxLog > 1f)
            {
                _lastFxLog = Time.unscaledTime;
                Log("[ABILFX] 播放远端特效 P" + d.OwnerId + " " + d.EffectName);
            }
        }

        /// <summary>
        /// v3.0.17：收到代码直调特效事件——用对端分身作 Source 调原生 PlayEffect。
        /// Source 传分身根而不是某个子物体，StopRemoteSpellFx 的 root 比对才能命中。
        /// </summary>
        internal static void PlayRemoteCodeFx(CodeFxData d)
        {
            if (d == null || string.IsNullOrEmpty(d.EffectName)) return;
            if (IsPureEnvironmentFx(d.EffectName) || IsCodeFxBlacklisted(d.EffectName)) return;

            float now = Time.unscaledTime;
            string playKey = d.OwnerId + ":" + d.EffectName;
            float last;
            if (_lastCodeFxPlayed.TryGetValue(playKey, out last) && now - last < 0.08f) return;
            _lastCodeFxPlayed[playKey] = now;

            var ps = PlayerSync.Instance;
            if (ps == null) return;
            GameObject clone;
            if (!ps.RemotePlayers.TryGetValue(d.OwnerId, out clone) || clone == null) return;

            Animator anim = null;
            var mirror = clone.GetComponent<RemoteAnimatorMirror>();
            if (mirror != null) anim = mirror.PrimaryCloneAnimator;
            if (anim == null) anim = clone.GetComponentInChildren<Animator>(true);

            _playingRemoteCodeFx = true;
            try
            {
                EffectManager.PlayEffect(clone, anim, d.EffectName, d.Position, d.Duration,
                    (EffectStopType)d.StopType, (EffectTriggerDirection)d.Direction);

                // 与动画特效共用取消登记：OnStateExit 发来的 SpellFxStop 也能停掉它。
                System.Collections.Generic.HashSet<string> ownerSet;
                if (!_remoteFxByOwner.TryGetValue(d.OwnerId, out ownerSet))
                {
                    ownerSet = new System.Collections.Generic.HashSet<string>();
                    _remoteFxByOwner[d.OwnerId] = ownerSet;
                }
                ownerSet.Add(d.EffectName);

                if (now - _lastFxLog > 1f)
                {
                    _lastFxLog = now;
                    Log("[ABILFX] 播放远端代码特效 P" + d.OwnerId + " " + d.EffectName);
                }
            }
            catch (Exception ex)
            {
                if (now - _lastFxLog > 2f)
                {
                    _lastFxLog = now;
                    LogW("[ABILFX] 播放代码特效异常 " + d.EffectName + ": " + Plugin.PatchError(ex));
                }
            }
            finally { _playingRemoteCodeFx = false; }
        }

        /// <summary>v3.0.17：代码直调特效线格式（字段顺序即协议顺序）。</summary>
        internal sealed class CodeFxData
        {
            public int OwnerId;
            public string EffectName;
            public Vector3 Position;
            public float Duration;
            public byte StopType;
            public int Direction;

            internal void Write(NetDataWriter w)
            {
                w.Put(OwnerId);
                w.Put(EffectName ?? string.Empty);
                w.Put(Position.x);
                w.Put(Position.y);
                w.Put(Position.z);
                w.Put(Duration);
                w.Put(StopType);
                w.Put(Direction);
            }

            internal static CodeFxData Read(NetDataReader r)
            {
                try
                {
                    var d = new CodeFxData
                    {
                        OwnerId = r.GetInt(),
                        EffectName = r.GetString(),
                        Position = new Vector3(r.GetFloat(), r.GetFloat(), r.GetFloat()),
                        Duration = r.GetFloat(),
                        StopType = r.GetByte(),
                        Direction = r.GetInt()
                    };
                    if (string.IsNullOrEmpty(d.EffectName) || d.EffectName.Length > 128) return null;
                    if (float.IsNaN(d.Position.x) || float.IsNaN(d.Position.y) || float.IsNaN(d.Position.z)
                        || float.IsInfinity(d.Position.x) || float.IsInfinity(d.Position.y)
                        || float.IsInfinity(d.Position.z)) return null;
                    if (float.IsNaN(d.Duration) || float.IsInfinity(d.Duration)) d.Duration = 0f;
                    return d;
                }
                catch { return null; }
            }
        }

        /// <summary>
        /// v3.0.9：收到远端特效取消事件——停掉分身上由该 Owner 触发的同名特效。
        /// 用 EffectManager.GetEffectList 拿到对象池里的所有同名实例，逐个检查 Source
        /// 是否在该 Owner 的分身树内，命中才 Stop。这样不会误停本地真身的特效，
        /// 也不影响其他玩家/怪物发的同名特效（Boss Room 的 Shutdown 语义）。
        /// </summary>
        internal static void StopRemoteSpellFx(int ownerId, string effectName)
        {
            if (string.IsNullOrEmpty(effectName)) return;
            var ps = PlayerSync.Instance;
            if (ps == null) return;
            GameObject clone;
            if (!ps.RemotePlayers.TryGetValue(ownerId, out clone) || clone == null) return;

            // 从登记里移除（无论是否找得到实例，都要清掉，防止残留记录越积越多）。
            System.Collections.Generic.HashSet<string> ownerSet;
            if (!_remoteFxByOwner.TryGetValue(ownerId, out ownerSet)) return;
            ownerSet.Remove(effectName);

            int stopped = 0;
            try
            {
                var list = EffectManager.GetEffectList(effectName);
                if (list == null) return;
                // 倒序遍历：Stop 可能让池对象从 list 中移除/回池。
                for (int i = list.Count - 1; i >= 0; i--)
                {
                    var fx = list[i];
                    if (fx == null || !fx.gameObject.activeSelf) continue;
                    var src = fx.Source;
                    if (src == null) continue;
                    if (src.transform.root != clone.transform.root) continue;  // 只停这个分身发的
                    try
                    {
                        EffectManager.StopEffect(fx, EffectStopType.Gracefully);
                        stopped++;
                    }
                    catch { /* 单个特效 Stop 失败不影响其它 */ }
                }
            }
            catch (Exception ex)
            {
                LogW("[ABILFX] 取消特效异常 " + effectName + ": " + ex.Message);
            }

            if (stopped > 0 && Time.unscaledTime - _lastFxLog > 1f)
            {
                _lastFxLog = Time.unscaledTime;
                Log("[ABILFX] 停止远端特效 P" + ownerId + " " + effectName + " n=" + stopped);
            }
        }

        /// <summary>
        /// 纯环境/移动特效黑名单：每一端本地动画自然驱动，跨端同步只会刷屏、叠加和 NRE。
        /// v3.0.12：把 "Dash" 从这里移出——虚冲残影(DashSilhouette/DashLines)是虚冲专属视觉，
        /// 必须跨端同步；普通冲刺灰尘仍由 EndsWith("Dust_Effect") / Footstep 拦掉。
        /// </summary>
        private static bool IsPureEnvironmentFx(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            const StringComparison cmp = StringComparison.OrdinalIgnoreCase;
            return name.EndsWith("Dust_Effect", cmp)
                || name.IndexOf("Footstep", cmp) >= 0
                || name.IndexOf("Death", cmp) >= 0
                || name.IndexOf("Jump", cmp) >= 0
                || name.IndexOf("Land", cmp) >= 0;
        }

        /// <summary>虚冲专属视觉特效（残影/虚线）。发送端只在虚冲窗口放行。</summary>
        private static bool IsDashVisualFx(string name)
        {
            if (string.IsNullOrEmpty(name)) return false;
            const StringComparison cmp = StringComparison.OrdinalIgnoreCase;
            return name.IndexOf("Dash", cmp) >= 0;
        }

        /// <summary>
        /// v3.0.17：中央 PlayEffect 桥的额外黑名单。相机震动/屏幕闪/UI/彩蛋类特效
        /// 是本机表现，跨端重放只会让对端镜头乱抖，不同步。
        /// </summary>
        private static bool IsCodeFxBlacklisted(string name)
        {
            if (string.IsNullOrEmpty(name)) return true;
            const StringComparison cmp = StringComparison.OrdinalIgnoreCase;
            // 注意不要写宽泛的 "UI" 子串匹配——Liquid/Suit/Build 这类特效名会被误杀。
            // UI 特效的 Source 本来就不是玩家分身，中央桥的 root 过滤已经挡掉了。
            return name.IndexOf("CameraShake", cmp) >= 0
                || name.IndexOf("Screen", cmp) >= 0
                || name.IndexOf("Fart", cmp) >= 0
                || name.IndexOf("Purchase", cmp) >= 0;
        }

        internal static void ResetAll()
        {
            // 豁免字典由 DashFxSync.ResetAll 统一清理；本类无跨会话状态。
            _lastFxSent.Clear();
            _lastFxPlayed.Clear();
            _lastCodeFxSent.Clear();
            _lastCodeFxPlayed.Clear();
            _lastAnimFxSent.Clear();
            _fxByLayer.Clear();
            _remoteFxByOwner.Clear();
            _localCastUntil = 0f;
            _dashFxUntil = 0f;
            _animFxNestingUntil = 0f;
            _playingRemoteCodeFx = false;
        }

        /// <summary>
        /// 动画特效事件载荷。字段顺序即线协议顺序，两端必须同版本（协议 3007）。
        /// </summary>
        internal sealed class SpellFxData
        {
            public int OwnerId;
            public string EffectName;
            public Vector2 PositionOffset;
            public byte OffsetType;
            public byte FollowTargetType;
            public byte DeriveScale;
            public float BaseScaleToDerive;
            public byte DeriveRotation;
            public bool DeriveRotationFromHeading;
            public int DurationType;
            public float Duration;
            public byte EffectStopType;
            public int Direction;
            public byte AnimatorLayer;
            public bool DestroyOnRoomChange;
            public float StartingPosLerp;
            public float LeftRotationZ;
            public float RightRotationZ;
            public float UpRotationZ;
            public float DownRotationZ;
            public byte Flips;
            public byte DeriveFacing;

            internal void Write(NetDataWriter w)
            {
                w.Put(OwnerId);
                w.Put(EffectName ?? string.Empty);
                w.Put(PositionOffset.x);
                w.Put(PositionOffset.y);
                w.Put(OffsetType);
                w.Put(FollowTargetType);
                w.Put(DeriveScale);
                w.Put(BaseScaleToDerive);
                w.Put(DeriveRotation);
                w.Put(DeriveRotationFromHeading);
                w.Put(DurationType);
                w.Put(Duration);
                w.Put(EffectStopType);
                w.Put(Direction);
                w.Put(AnimatorLayer);
                w.Put(DestroyOnRoomChange);
                w.Put(StartingPosLerp);
                w.Put(LeftRotationZ);
                w.Put(RightRotationZ);
                w.Put(UpRotationZ);
                w.Put(DownRotationZ);
                w.Put(Flips);
                w.Put(DeriveFacing);
            }

            internal static SpellFxData Read(NetDataReader r)
            {
                try
                {
                    var d = new SpellFxData
                    {
                        OwnerId = r.GetInt(),
                        EffectName = r.GetString(),
                        PositionOffset = new Vector2(r.GetFloat(), r.GetFloat()),
                        OffsetType = r.GetByte(),
                        FollowTargetType = r.GetByte(),
                        DeriveScale = r.GetByte(),
                        BaseScaleToDerive = r.GetFloat(),
                        DeriveRotation = r.GetByte(),
                        DeriveRotationFromHeading = r.GetBool(),
                        DurationType = r.GetInt(),
                        Duration = r.GetFloat(),
                        EffectStopType = r.GetByte(),
                        Direction = r.GetInt(),
                        AnimatorLayer = r.GetByte(),
                        DestroyOnRoomChange = r.GetBool(),
                        StartingPosLerp = r.GetFloat(),
                        LeftRotationZ = r.GetFloat(),
                        RightRotationZ = r.GetFloat(),
                        UpRotationZ = r.GetFloat(),
                        DownRotationZ = r.GetFloat(),
                        Flips = r.GetByte(),
                        DeriveFacing = r.GetByte()
                    };
                    return string.IsNullOrEmpty(d.EffectName) ? null : d;
                }
                catch
                {
                    return null;
                }
            }
        }

        private static void TryPatch(Harmony harmony, MethodInfo target, string prefixName,
            string label, string postfixName = null)
        {
            try
            {
                if (target == null)
                {
                    LogW("[ABILFX] 补丁目标为空: " + label);
                    return;
                }
                HarmonyMethod pre = null, post = null;
                if (!string.IsNullOrEmpty(prefixName))
                    pre = new HarmonyMethod(typeof(AbilityFxSync).GetMethod(prefixName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                if (!string.IsNullOrEmpty(postfixName))
                    post = new HarmonyMethod(typeof(AbilityFxSync).GetMethod(postfixName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                harmony.Patch(target, prefix: pre, postfix: post);
                Log("[ABILFX] 补丁 OK: " + label);
            }
            catch (Exception ex)
            {
                LogW("[ABILFX] " + label + " 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        private static void Log(string m) { Plugin.Log.LogInfo("[RL2MP] " + m); }
        private static void LogW(string m) { Plugin.Log.LogWarning("[RL2MP] " + m); }
    }
}
