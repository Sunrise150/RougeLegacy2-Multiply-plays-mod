using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 协作模式：主机权威怪物位置 + 仇恨目标。
    ///
    /// 历史：
    ///   v2.0.0 每帧把本地怪 lerp 到主机坐标 → "轨迹不自然、抽搐、掉帧"（AI 与同步互相拉扯）。
    ///   v2.0.6 改成"只纠正不对抗" + 时间轴插值。
    ///   v2.3.0（本次）修掉三个仍然会导致抽搐的问题：
    ///
    ///   1) **插值时间轴算错**（抽搐的数学根因）。
    ///      旧实现只存 Prev/Cur 两个采样，渲染时刻取 `now - 0.09`，而 Cur 是"刚刚收到这包"
    ///      的时刻，Prev 是 `now - 0.0667`。于是 `renderAt = now - 0.09 < Prev.At`，
    ///      `f = clamp01(负数) = 0` —— 整包周期内 want 都等于 Prev.Pos（不动），
    ///      下一包到达时 want 突然跳到旧 Cur.Pos。**每 66.7ms 一次机械性跳变**，
    ///      位移越大跳得越明显，这就是"位置一直在抽搐"。
    ///      修法：存三个采样（S2/S1/S0），渲染时刻按历史轴找"真正包围它的那一对"再插值；
    ///      落在时间轴之外时钳住（滞后但不跳）。
    ///
    ///   2) **key 匹配率极低**。日志实测 `[EPOS] 客机权威条目=12 但无匹配活怪`，
    ///      说明客机算出来的 key 与主机发来的 key 大量对不上（房间半初始化导致
    ///      EnemyIndex 漂移）。对不上的怪完全跑本地 AI，与主机分歧 → 看着像"怪在原地抽"。
    ///      修法：包里带 EnemyType，key 未命中时退一步找"同类型 + 6m 内最近"的活怪，
    ///      命中后把 localKey→hostKey 记进映射表，后续 O(1) 直查，不再反复兜底。
    ///
    ///   3) **仇恨标签每包翻转**。主机端 DecideTarget 有黏性，客机端没有：TargetRemote↔
    ///      TargetHost 一变，渲染模式就在"本地 AI 主导"和"完全权威写入"之间硬切，
    ///      位置大范围跳动。修法：客机端给目标加 0.45s 迟滞，切换瞬间重置渲染基准。
    ///
    /// 只在协作 + InSession + Connected 生效；竞速/单人完全不碰。
    /// </summary>
    internal static class EnemyPositionSync
    {
        // 仇恨目标编码
        internal const byte TargetNone = 0;
        internal const byte TargetHost = 1;
        internal const byte TargetRemote = 2;

        private struct Sample
        {
            public Vector3 Pos;
            public double At;   // v2.5.0：主机时间轴时刻（原为本地 float）
        }

        private struct Entry
        {
            public Sample S2;      // 最老
            public Sample S1;
            public Sample S0;      // 最新
            public bool FacingRight;
            public byte Target;    // 已生效的目标（经过迟滞）
            public byte PendingTarget;
            public float PendingAt;
            public float RecvAt;
            // v2.6.0：主机敌人动画状态（借鉴 rl2-coop 的 EnemyState.AnimState/AnimTime），
            // 让客机看到的主怪物出招/收招与主机一致。
            public int AnimHash;
            public float AnimTime;
            public bool HasAnim;
            // 我们自己渲染出来的位置（不读 transform，避免把本地 AI 的位移算进来形成拉扯）
            public Vector3 Rendered;
            public bool HasRendered;
            // v3.0.11：动画回放去重。主机 15Hz 快照的 AnimTime 每包都在推进，
            // 旧版每帧拿它 Play 一次等于把动画反复拽回快照时间点 -> 客机看出招抽搐。
            // 只在 hash 真正变化时播放一次，之后交回 Animator 自然推进；偏差过大才软校正。
            public int PlayedHash;
            public float PlayedAt;       // 本机播放时刻
            public float PlayedAnimTime; // 播放时锚定的快照归一化时间
            // v3.0.11：朝向迟滞。连续 N 帧一致才翻面，挡掉单包 facing 抖动。
            public bool FacingStable;
            public float FacingPendingAt;
        }

        private static readonly Dictionary<int, Entry> _authority = new Dictionary<int, Entry>();
        // 客机：本地 key -> 主机 key。key 因为 EnemyIndex 漂移对不上时，靠
        // "同类型 + 位置接近"兜底认一次，认上之后写进这里，后续直接查表。
        private static readonly Dictionary<int, int> _keyMap = new Dictionary<int, int>();
        // v2.8.0：客机已接管的怪（rl2-coop 的 Puppetize）。本地 AI 停摆，
        // 位置/朝向/动画只由主机快照驱动。
        private static readonly HashSet<EnemyController> _puppetized = new HashSet<EnemyController>();
        private static float _lastPuppetLog;
        internal static bool IsPuppetized(EnemyController e)
        {
            return e != null && _puppetized.Contains(e);
        }
        // 主机侧：每只怪上一次的仇恨目标，用于"黏住"避免来回切（hysteresis）
        private static readonly Dictionary<int, byte> _lastTarget = new Dictionary<int, byte>();

        private static float _sendTimer;
        private const float SendInterval = 0.0667f;     // 15Hz
        private const float StaleTimeout = 0.75f;       // 超过这个时间没新包就不再用它
        private const float InterpDelay = 0.11f;        // 插值渲染延迟（≈1.6 个包间隔，吸收抖动）
        // v2.6.0：每只怪多带 animHash(4B)+animTime(4B)，28B×32=896B，仍稳在 MTU 内。
        private const int MaxEnemies = 32;
        private const float SwingAdvantage = 4f;        // v3.0.11：与权威端索敌一致提到 4m，减少目标翻转→朝向抽搐
        private const float TargetHoldTime = 0.45f;     // 客机端目标切换迟滞
        private const float ApproxMatchDist = 6f;       // 兜底匹配的最大距离
        private const float WriteEpsilon = 0.02f;       // 位移小于 2cm 不写 transform（省物理更新）
        private const float FacingHold = 0.12f;         // v3.0.11：朝向翻转迟滞（约 7 帧），挡单包抖动
        private const float AnimResyncThreshold = 0.35f; // v3.0.11：动画进度偏差超 35% 才软校正一次

        private static float _lastLog;
        private static float _lastPrune;
        private static int _statFrames;
        private static float _statAt;
        // v3.0.23：权威侧帧率统计。旧的 fps 日志只在 ClientApplyLate 打，而该函数在
        // 本机成为权威（主机倒地移交）后第一行就 return —— 恰好是"客机倒地后卡"的
        // 场景反而没有任何帧率数据。这里补一条权威侧计数，下一次真机日志就能确认
        // 掉帧是权威采样/广播造成，还是渲染侧问题。
        private static int _hostStatFrames;
        private static float _hostStatAt;
        private static readonly LiteNetLib.Utils.NetDataWriter _writer = new LiteNetLib.Utils.NetDataWriter();

        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp && DownedState.InSession
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        // ---------------- 主机：广播 ----------------

        /// <summary>主机：15Hz 广播活怪的权威位置 + 朝向 + 仇恨目标。由 PumpNetwork 驱动。</summary>
        internal static void HostTick()
        {
            if (!Active || NetworkManager.Instance == null || !RoomAuthority.ShouldDriveWorld) return;

            // v3.0.23：每帧计数（在 15Hz 节流之前），每 10s 打一次权威侧帧率。
            _hostStatFrames++;
            if (Time.unscaledTime - _hostStatAt >= 10f)
            {
                float hostFps = _hostStatFrames / (Time.unscaledTime - _hostStatAt);
                _hostStatAt = Time.unscaledTime;
                _hostStatFrames = 0;
                Plugin.Log.LogInfo("[RL2MP] [EPOS] 权威端统计: fps=" + hostFps.ToString("F1")
                    + " (isHost=" + NetworkManager.Instance.IsHost
                    + ", localDown=" + DownedState.LocalDown + ")");
            }

            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer < SendInterval) return;
            _sendTimer = 0f;

            try
            {
                var all = EnemySync.GetEnemiesForPosition();
                if (all == null) return;

                Vector3 selfPos = GetLocalPlayerPos();
                bool hasSelf = selfPos.sqrMagnitude > 0.0001f;

                // 第一遍：数有效怪（写 count 需要先知道数量）
                int count = 0;
                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];
                    if (e == null || e.IsDead) continue;
                    if (++count >= MaxEnemies) break;
                }
                if (count == 0) return;

                _writer.Reset();
                _writer.Put((byte)MsgType.EnemyPosition);
                // v2.5.0：打主机时间轴时间戳——客机按它采样，消除本机抖动/漂移导致的抽搐。
                _writer.Put(NetClock.HostNow);
                _writer.Put(count);

                int written = 0;
                for (int i = 0; i < all.Length && written < count; i++)
                {
                    var e = all[i];
                    if (e == null || e.IsDead) continue;

                    int key = EnemySync.BuildKey(e);
                    Vector3 p = e.transform.position;
                    byte target = DecideTarget(key, p, hasSelf, selfPos);

                    // v2.6.0：把主机当前动画状态也发出去（rl2-coop 同款），
                    // 客机据此纠正"怪物出招早晚/收招"的视觉差异。
                    int animHash = 0;
                    float animTime = 0f;
                    try
                    {
                        var anim = e.Animator;
                        if (anim != null && anim.isActiveAndEnabled)
                        {
                            // v3.0.4（借鉴 TUNIC Puppet.Awake）：权威端离屏怪的 Animator
                            // 默认会被 Unity culling 冻结，GetCurrentAnimatorStateInfo 返回
                            // 停更的旧状态 -> 广播出去的动画hash也是旧的 -> 客机看出招错位。
                            // 设一次 AlwaysAnimate（有内部去重，开销可忽略）。
                            if (anim.cullingMode != AnimatorCullingMode.AlwaysAnimate)
                                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                            var st = anim.GetCurrentAnimatorStateInfo(0);
                            animHash = st.fullPathHash;
                            animTime = st.normalizedTime;
                        }
                    }
                    catch { animHash = 0; animTime = 0f; }

                    _writer.Put(key);
                    _writer.Put((ushort)e.EnemyType);   // v2.3.0：兜底匹配用
                    _writer.Put(p.x);
                    _writer.Put(p.y);
                    _writer.Put(p.z);
                    _writer.Put(e.IsFacingRight);
                    _writer.Put(target);
                    _writer.Put(animHash);
                    _writer.Put(animTime);
                    written++;
                }

                NetworkManager.Instance.SendEnemyPosition(_writer);

                if (Time.unscaledTime - _lastLog > 8f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [EPOS] 广播怪物位置 enemies=" + written + "（15Hz，含仇恨目标）");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [EPOS] host tick failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>
        /// 主机决定这只怪在追谁：比较"到主机的距离"和"到各客机分身的距离"，取近的。
        /// 带黏性：原来追的人只要别落后 SwingAdvantage 米以上就不换，避免两边站位相近时每帧乱切。
        /// </summary>
        private static byte DecideTarget(int key, Vector3 enemyPos, bool hasSelf, Vector3 selfPos)
        {
            byte best = TargetNone;
            float bestDist = float.MaxValue;

            if (hasSelf)
            {
                float d = Vector3.Distance(enemyPos, selfPos);
                if (d < bestDist) { bestDist = d; best = TargetHost; }
            }

            var ps = PlayerSync.Instance;
            if (ps != null && ps.RemotePlayers != null && ps.RemotePlayers.Count > 0)
            {
                // v3.0.11：修旧版两个 bug——① 遇第一个更近的 remote 就 break，没取真正最近的；
                // ② 没过滤倒地分身。这里取所有未倒地分身里的最近者，与权威端 RetargetAuthorityPlayers 对齐。
                float nearestRemote = float.MaxValue;
                foreach (var kv in ps.RemotePlayers)
                {
                    if (ps.RemoteDowned.Contains(kv.Key)) continue;
                    var go = kv.Value;
                    if (go == null || !go.activeInHierarchy) continue;
                    float d = Vector3.Distance(enemyPos, go.transform.position);
                    if (d < nearestRemote) nearestRemote = d;
                }
                if (nearestRemote < bestDist - 0.05f) { bestDist = nearestRemote; best = TargetRemote; }
            }

            byte last;
            if (best != TargetNone && _lastTarget.TryGetValue(key, out last) && last != best)
            {
                // 只有明显更近才切换（黏性），否则沿用上一次的目标
                float lastDist = DistToTarget(last, enemyPos, selfPos);
                if (lastDist < bestDist + SwingAdvantage) best = last;
            }
            if (best != TargetNone) _lastTarget[key] = best;
            return best;
        }

        private static float DistToTarget(byte target, Vector3 enemyPos, Vector3 selfPos)
        {
            if (target == TargetHost) return Vector3.Distance(enemyPos, selfPos);
            var ps = PlayerSync.Instance;
            if (ps != null && ps.RemotePlayers != null)
            {
                foreach (var kv in ps.RemotePlayers)
                {
                    if (kv.Value != null) return Vector3.Distance(enemyPos, kv.Value.transform.position);
                }
            }
            return float.MaxValue;
        }

        private static Vector3 GetLocalPlayerPos()
        {
            // v2.4.0：本地玩家已死亡/观战时不再作为仇恨目标。
            // 旧版这里照样返回尸体坐标，于是主机死后怪物一直围着尸体打转；
            // 客机端这只怪的本地 AI 却在追活着的客机 —— 两套行为对冲，
            // 我们每帧把怪拉回主机位置、AI 又把它拽走，画面就是"围着尸体抽搐"。
            if (DownedState.LocalDown) return Vector3.zero;

            var ps = PlayerSync.Instance;
            if (ps == null || ps.LocalPlayer == null) return Vector3.zero;
            var go = ps.CachedLocalPlayerObject;
            if (go == null || !go.activeInHierarchy) return Vector3.zero;
            return ps.LocalPlayer.transform.position;
        }

        // ---------------- 客机：接收 ----------------

        /// <summary>客机：解析主机快照，写入三个采样（供时间轴插值渲染）。</summary>
        internal static void ApplySnapshot(LiteNetLib.Utils.NetDataReader reader)
        {
            try
            {
                // v2.5.0：包首是主机时间轴时间戳；用它而不是本地到达时间做插值轴。
                if (reader.AvailableBytes < 8) return;
                double packetHostTime = reader.GetDouble();
                float now = Time.unscaledTime;
                // 主机时间合法性兜底：离谱值退化为"用本地到达时刻对应的主机时刻"。
                if (!RLMod.Core.Protocol.Validation.ValidHostTime(packetHostTime, NetClock.HostNow))
                    packetHostTime = NetClock.HostNow;

                int n = reader.GetInt();
                if (n < 0 || n > 512) return;

                for (int i = 0; i < n; i++)
                {
                    int key = reader.GetInt();
                    ushort type = reader.GetUShort();
                    float x = reader.GetFloat(), y = reader.GetFloat(), z = reader.GetFloat();
                    bool facing = reader.GetBool();
                    byte target = reader.GetByte();
                    int animHash = reader.GetInt();      // v2.6.0
                    float animTime = reader.GetFloat();
                    if (float.IsNaN(animTime) || float.IsInfinity(animTime)) { animHash = 0; animTime = 0f; }

                    // v2.5.0：每只怪坐标合法性——坏值直接跳过这只，绝不污染渲染。
                    if (!RLMod.Core.Protocol.Validation.ValidCoord(x, y, z)) continue;

                    Entry en;
                    if (!_authority.TryGetValue(key, out en))
                    {
                        en = new Entry();
                        en.S0 = new Sample { Pos = new Vector3(x, y, z), At = packetHostTime };
                        en.Target = target;
                    }
                    else
                    {
                        // 三采样轮转：S2←S1←S0←新包（用包内主机时刻）
                        en.S2 = en.S1;
                        en.S1 = en.S0;
                        en.S0 = new Sample { Pos = new Vector3(x, y, z), At = packetHostTime };
                    }
                    en.FacingRight = facing;
                    en.AnimHash = animHash;
                    en.AnimTime = animTime;
                    en.HasAnim = animHash != 0;
                    en.RecvAt = now;
                    _authorityType[key] = type;

                    // 目标迟滞：候选要连续保持 TargetHoldTime 才真正切换，
                    // 避免主机端一包一变导致客机渲染模式反复翻转（抽搐）。
                    if (target == en.Target)
                    {
                        en.PendingTarget = TargetNone;
                        en.PendingAt = 0f;
                    }
                    else if (en.PendingTarget != target)
                    {
                        en.PendingTarget = target;
                        en.PendingAt = now;
                    }
                    else if (now - en.PendingAt >= TargetHoldTime)
                    {
                        en.Target = target;
                        en.PendingTarget = TargetNone;
                        en.PendingAt = 0f;
                        // 模式切换后按当前真实位置重新起算渲染，避免"AI 位置"和
                        // "权威轨迹"之间直接瞬移。
                        en.HasRendered = false;
                    }

                    _authority[key] = en;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [EPOS] apply snapshot failed: " + ex.Message);
            }
        }

        /// <summary>
        /// rl2-coop 同款：客机怪由主机快照驱动，本地 AI/物理必须停摆，
        /// 否则它自己的 Update 会继续推位置、出招、发弹，和权威位置对撞成抽搐。
        /// </summary>
        private static void Puppetize(EnemyController e)
        {
            if (e == null || _puppetized.Contains(e)) return;
            // v2.8.2：拆成两个独立 try。旧版一个 try 包住全部，LogicController 取值或
            // StopAllLogic 抛异常（部分怪没有 LogicController 组件，getter 会抛
            // MissingComponentException）时，后面的 corgi 关闭就被跳过，且被 catch{} 全吞
            // 没有任何日志——怪的 AI/物理继续本地跑，和权威位置对冲成抽搐，却查不到原因。
            bool logicStopped = false, corgiStopped = false;
            try
            {
                var logic = e.LogicController;
                if (logic != null)
                {
                    logic.StopAllLogic(true);
                    logic.enabled = false;
                    logicStopped = true;
                }
                else logicStopped = true;   // 没有 AI 组件，本来就不需要停
            }
            catch (Exception ex) { LogPuppetErr("LogicController", ex); }
            try
            {
                var corgi = e.ControllerCorgi;
                if (corgi != null) { corgi.enabled = false; corgiStopped = true; }
                else corgiStopped = true;
            }
            catch (Exception ex) { LogPuppetErr("ControllerCorgi", ex); }
            // v2.8.5：fireEvents 的关闭已移到 ClientApplyLate 循环里，对客机所有本地怪
            // 无条件执行（含未匹配权威条目的怪），见 DisableLocalFireEvents。
            // Puppetize 这里只负责停 AI/物理，职责拆开，避免漏关和重复。
            _puppetized.Add(e);
            if (Time.unscaledTime - _lastPuppetLog > 5f)
            {
                _lastPuppetLog = Time.unscaledTime;
                Plugin.Log.LogInfo("[RL2MP] [EPOS] 客机怪已交出 AI 控制权（Puppetize），累计 "
                    + _puppetized.Count + " 只 logic=" + logicStopped + " corgi=" + corgiStopped);
            }
        }

        // v2.8.5：关过动画事件的怪，避免每帧重复取 Animator。换房/换场景时随 ResetAll 清空。
        private static readonly HashSet<EnemyController> _fireEventsOff = new HashSet<EnemyController>();

        private static void DisableLocalFireEvents(EnemyController e)
        {
            if (e == null || _fireEventsOff.Contains(e)) return;
            try
            {
                var anim = e.Animator;
                if (anim == null) return;
                anim.fireEvents = false;
                _fireEventsOff.Add(e);
            }
            catch (Exception ex) { LogPuppetErr("Animator.fireEvents", ex); }
        }

        private static float _lastPuppetErrLog;
        private static void LogPuppetErr(string what, Exception ex)
        {
            if (Time.unscaledTime - _lastPuppetErrLog < 3f) return;
            _lastPuppetErrLog = Time.unscaledTime;
            Plugin.Log.LogWarning("[RL2MP] [EPOS] Puppetize 关闭 " + what + " 失败: " + Plugin.PatchError(ex));
        }

        /// <summary>
        /// 客机：LateUpdate 把权威轨迹平滑地叠到本地怪身上。由 NetworkManager.LateUpdate 驱动。
        /// 原则是"只纠正不对抗"：偏差小就不动（交给本地 AI），偏差大才限速收敛。
        /// </summary>
        internal static void ClientApplyLate()
        {
            if (!Active || NetworkManager.Instance == null || RoomAuthority.IsLocalAuthority) return;
            if (_authority.Count == 0) return;

            try
            {
                var all = EnemySync.GetEnemiesForPosition();
                if (all == null) return;

                float now = Time.unscaledTime;
                float dt = Mathf.Clamp(Time.unscaledDeltaTime, 0.001f, 0.05f);
                // v2.5.0：渲染时刻取自统一主机时间轴；延迟吸收抖动但不依赖本机帧抖动。
                double renderAt = NetClock.HostNow - InterpDelay;
                int applied = 0, approx = 0, total = 0;

                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];
                    if (e == null || e.IsDead) continue;
                    total++;

                    // v2.8.5：所有客机本地怪一律关掉动画事件，与"是否匹配上权威条目"无关。
                    // 未匹配/新刷出的怪若留着 fireEvents，播放主机同步的攻击动画时会在本地
                    // 生成真 hitbox 打中存活方真身（本地扣一次），同时权威端经 RemoteHurtbox
                    // 又转发一次 —— 叠加就是"一方倒地后另一方掉血双倍"。
                    // 这里只关动画事件，不停 AI/物理（v2.8.4 无条件 Puppetize 把二者也停了，
                    // 反而让未匹配怪僵住、引入更多问题），位置/插值逻辑完全不动。
                    DisableLocalFireEvents(e);

                    int localKey = EnemySync.BuildKey(e);
                    int hostKey;
                    Entry en;
                    if (!ResolveEntry(e, localKey, out hostKey, out en)) continue;
                    Puppetize(e);   // v2.8.0：本地 AI 停摆，视觉/位置完全听主机的
                    if (now - en.RecvAt > StaleTimeout) continue;
                    if (hostKey != localKey) approx++;

                    Vector3 want = Evaluate(en, renderAt);

                    // v2.4.0：不再区分"这只怪在追谁"，**所有**怪物一律按主机权威轨迹渲染。
                    //
                    // 旧版对"在追本地玩家"的怪放任本地 AI 主导（只在偏差 >2.2m 时硬拉），
                    // 于是本地 AI 每帧移动、同步逻辑每帧拉回，两种力互相抵消 ——
                    // 这就是反复出现的"客机画面抽搐"。两端 AI 的目标本来就不保证一致
                    // （尤其是主机死了以后：主机端怪围着尸体，客机端怪追活人），
                    // 唯一稳的做法是位置只信主机，AI 只负责动画/攻击判定。
                    if (!en.HasRendered)
                    {
                        // 首次同步：偏差很大直接吸附，避免从旧位置"飞"过去
                        en.Rendered = want;
                        en.HasRendered = true;
                        try { e.transform.position = want; } catch { }
                    }
                    else
                    {
                        // 低通滤掉残余抖动；系数随帧率走，绝不和 AI 抢
                        en.Rendered = Vector3.Lerp(en.Rendered, want, Mathf.Clamp01(dt * 30f));
                    }

                    Vector3 cur = e.transform.position;
                    if ((cur - en.Rendered).sqrMagnitude > WriteEpsilon * WriteEpsilon)
                    {
                        try { e.transform.position = en.Rendered; } catch { }
                    }

                    applied++;

                    // v3.0.11：朝向迟滞。索敌每 0.5s 可能换目标 -> 主机快照 facing 偶尔翻一下，
                    // 客机若逐帧硬 SetFacing 就会看到怪来回抽搐。连续 FacingHold 秒一致才翻面。
                    if (e.IsFacingRight != en.FacingRight)
                    {
                        if (!en.FacingStable)
                        {
                            en.FacingStable = true;
                            en.FacingPendingAt = now;
                        }
                        else if (now - en.FacingPendingAt >= FacingHold)
                        {
                            try { e.SetFacing(en.FacingRight); } catch { }
                            en.FacingStable = false;
                        }
                    }
                    else
                    {
                        en.FacingStable = false;
                    }

                    // v3.0.11：动画回放去重（客机抽搐主因）。
                    // 旧版每拿一包就 anim.Play(hash, 0, AnimTime%1)：15Hz 快照的 AnimTime 每包
                    // 都在推进，等于每 66ms 把动画拽回快照时刻；Hash 在两帧间抖动时更会反复重播
                    // -> 主机原生动画平滑，客机看成出招抽搐。
                    // 新策略：hash 真正变化才 Play 一次并锚定时间；同 hash 期间交回 Animator 自然
                    // 推进（PlayedAnimTime + 本机经过时间推算预期进度），只有偏差超过阈值才软校正。
                    if (en.HasAnim)
                    {
                        try
                        {
                            var anim = e.Animator;
                            if (anim != null && anim.isActiveAndEnabled)
                            {
                                bool hashChanged = en.PlayedHash != en.AnimHash;
                                bool needPlay = hashChanged;
                                if (!needPlay)
                                {
                                    // 同一段动画：估算"若按本机正常推进现在应到哪"，与快照锚点比对。
                                    float elapsed = now - en.PlayedAt;
                                    float expected = Mathf.Repeat(en.PlayedAnimTime + elapsed * anim.speed, 1f);
                                    float snapT = Mathf.Repeat(en.AnimTime, 1f);
                                    float diff = Mathf.Abs(expected - snapT);
                                    if (Mathf.Min(diff, 1f - diff) > AnimResyncThreshold)
                                        needPlay = true;   // 偏差过大（如中途换招/循环重置）才校正
                                }
                                if (needPlay)
                                {
                                    anim.Play(en.AnimHash, 0, Mathf.Repeat(en.AnimTime, 1f));
                                    en.PlayedHash = en.AnimHash;
                                    en.PlayedAt = now;
                                    en.PlayedAnimTime = Mathf.Repeat(en.AnimTime, 1f);
                                }
                            }
                        }
                        catch { }
                    }
                    _authority[hostKey] = en;
                }

                if (now - _lastPrune > 3f)
                {
                    _lastPrune = now;
                    PruneStale(now);
                }

                // v2.3.0：诊断统计。抽搐问题必须能区分"同步没匹配上"和"帧率在抖"，
                // 所以这里把 fps 和匹配率一起打出来，下次看日志就能定位。
                _statFrames++;
                if (now - _statAt >= 10f)
                {
                    float fps = _statFrames / (now - _statAt);
                    Plugin.Log.LogInfo("[RL2MP] [EPOS] 客机统计: fps=" + fps.ToString("F1")
                        + " 活怪=" + total + " 已同步=" + applied + "（兜底 " + approx + "）"
                        + " 权威条目=" + _authority.Count + " 映射=" + _keyMap.Count);
                    _statAt = now;
                    _statFrames = 0;
                    if (applied == 0 && total > 0)
                    {
                        Plugin.Log.LogWarning("[RL2MP] [EPOS] 客机有 " + total
                            + " 只活怪但一只都没匹配上（key 与位置都对不上，房间几何可能不一致）");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [EPOS] client apply failed: " + ex.Message);
            }
        }

        /// <summary>
        /// 时间轴插值：在 S2→S1→S0 之间找真正包围 renderAt 的那一对采样。
        /// 落在这条历史轴之外时钳住端点（宁可滞后一帧，也绝不跳变）。
        /// </summary>
        private static Vector3 Evaluate(Entry en, double renderAt)
        {
            if (en.S0.At <= 0.0001) return en.S0.Pos;
            if (renderAt >= en.S0.At) return en.S0.Pos;

            if (en.S1.At > 0.0001)
            {
                if (renderAt >= en.S1.At)
                {
                    double span = en.S0.At - en.S1.At;
                    double f = span > 0.0001 ? (renderAt - en.S1.At) / span : 1.0;
                    return Vector3.Lerp(en.S1.Pos, en.S0.Pos, Mathf.Clamp01((float)f));
                }
                if (en.S2.At > 0.0001 && renderAt >= en.S2.At)
                {
                    double span = en.S1.At - en.S2.At;
                    double f = span > 0.0001 ? (renderAt - en.S2.At) / span : 1.0;
                    return Vector3.Lerp(en.S2.Pos, en.S1.Pos, Mathf.Clamp01((float)f));
                }
                return en.S2.At > 0.0001 ? en.S2.Pos : en.S1.Pos;
            }

            return en.S0.Pos;
        }

        /// <summary>
        /// 找到这只本地怪对应的权威条目。
        /// 先走 localKey→hostKey 映射（命中率最高），未命中时按"同类型 + 近距离"兜底认一次。
        /// </summary>
        private static bool ResolveEntry(EnemyController e, int localKey, out int hostKey, out Entry en)
        {
            hostKey = localKey;

            int mapped;
            if (_keyMap.TryGetValue(localKey, out mapped))
            {
                if (_authority.TryGetValue(mapped, out en)) return true;
                _keyMap.Remove(localKey);   // 映射失效，重新兜底
            }

            if (_authority.TryGetValue(localKey, out en)) return true;

            // 兜底：同类型、6m 内最近的活条目。EnemyIndex 漂移时这是唯一能对上的依据。
            // 失败后 0.6s 内不再为这只怪重试，否则每帧 50 只怪 × 全部条目会白烧 CPU。
            float failAt;
            if (_approxFailAt.TryGetValue(localKey, out failAt) && Time.unscaledTime - failAt < 0.6f)
                return false;

            Vector3 p = e.transform.position;
            ushort type = (ushort)e.EnemyType;
            int bestKey = 0;
            float bestSqr = ApproxMatchDist * ApproxMatchDist;
            Entry best = default(Entry);
            bool found = false;

            foreach (var kv in _authority)
            {
                ushort candType;
                if (!_authorityType.TryGetValue(kv.Key, out candType) || candType != type) continue;
                var cand = kv.Value;
                float sqr = (cand.S0.Pos - p).sqrMagnitude;
                if (sqr > bestSqr) continue;
                bestSqr = sqr;
                bestKey = kv.Key;
                best = cand;
                found = true;
            }
            if (!found)
            {
                _approxFailAt[localKey] = Time.unscaledTime;
                return false;
            }

            _approxFailAt.Remove(localKey);
            _keyMap[localKey] = bestKey;
            hostKey = bestKey;
            en = best;
            return true;
        }

        // 兜底匹配失败的时间戳（localKey -> 时刻），用于给失败重试降温
        private static readonly Dictionary<int, float> _approxFailAt = new Dictionary<int, float>();

        // 主机包里的 EnemyType，与条目 key 一一对应（用于兜底匹配时校验类型）
        private static readonly Dictionary<int, ushort> _authorityType = new Dictionary<int, ushort>();

        private static readonly List<int> _pruneBuf = new List<int>(64);

        private static void PruneStale(float now)
        {
            if (_authority.Count <= 128) return;
            _pruneBuf.Clear();
            foreach (var kv in _authority)
            {
                if (now - kv.Value.RecvAt > 5f) _pruneBuf.Add(kv.Key);
            }
            for (int i = 0; i < _pruneBuf.Count; i++)
            {
                _authority.Remove(_pruneBuf[i]);
                _authorityType.Remove(_pruneBuf[i]);
            }
        }

        /// <summary>查询某只怪当前的权威仇恨目标（0=未知/1=主机/2=客机），供其他模块参考。</summary>
        /// <summary>
        /// v3.0.4（借鉴 TUNIC MonsterSync.MaybeHandoff 与 rl2-coop SceneHost 移交）：
        /// 世界驱动权易手时，同步翻转"本地怪 AI 控制权"。
        ///
        /// 根因：v2.8.0 起客机的怪被 Puppetize（logic/corgi 全关，只听权威快照），
        /// 且 _puppetized 只在 ResetAll（换场景）时清空。v3.0.0 让客机在主机倒地时
        /// 接管世界驱动，但没恢复本地怪 AI —— 新权威驱动的其实是一群"死怪"，
        /// 表现就是"怪不攻击客机 / 观战画面怪静止 / 机关采样停在旧值"。
        ///
        /// nowAuthority=true：本机刚成为权威 -> 恢复所有本地怪的 AI/物理/动画事件。
        /// nowAuthority=false：本机失去权威 -> 清快照缓冲，下一拍 ClientApplyLate 按新权威重建。
        /// </summary>
        internal static void OnAuthorityChanged(bool nowAuthority)
        {
            if (!Active) return;
            _authority.Clear();
            _authorityType.Clear();
            _keyMap.Clear();
            _approxFailAt.Clear();
            _lastTarget.Clear();

            if (!nowAuthority)
            {
                Plugin.Log.LogInfo("[RL2MP] [EPOS] 本机失去世界权威 -> 已清快照缓冲，等待新权威同步。");
                return;
            }

            int restored = 0;
            try
            {
                var all = EnemySync.GetEnemiesForPosition();
                if (all != null)
                    for (int i = 0; i < all.Length; i++)
                        if (Depuppetize(all[i])) restored++;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [EPOS] 权威接管恢复本地怪 AI 失败: " + ex.Message);
            }
            // v3.0.11：_fireEventsOff 含"未匹配权威条目但被关 fireEvents"的怪，Depuppetize 不覆盖它们。
            // 客机接管后这些怪永久 fireEvents=false → 播攻击动画但不生成真 hitbox = "怪不攻击客机"。
            // 这里对全场活怪无条件恢复 fireEvents（本机已是权威，本就该能生成 hitbox）。
            int fxRestored = 0;
            try
            {
                var all = EnemySync.GetEnemiesForPosition();
                if (all != null)
                    for (int i = 0; i < all.Length; i++)
                    {
                        var e = all[i];
                        if (e == null || e.IsDead) continue;
                        try
                        {
                            var anim = e.Animator;
                            if (anim != null && !anim.fireEvents)
                            {
                                anim.fireEvents = true;
                                anim.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                                fxRestored++;
                            }
                        }
                        catch { }
                    }
            }
            catch { }
            _fireEventsOff.Clear();
            Plugin.Log.LogInfo("[RL2MP] [EPOS] 本机接管世界权威 -> 恢复 " + restored
                + " 只本地怪的 AI/物理、" + fxRestored + " 只怪的攻击动画事件（之前被关停）。");
        }

        /// <summary>恢复单只被 Puppetize 的怪：与 Puppetize 对称地还原 AI/物理/动画事件。</summary>
        private static bool Depuppetize(EnemyController e)
        {
            if (e == null || !_puppetized.Contains(e)) return false;
            try
            {
                var logic = e.LogicController;
                if (logic != null) logic.enabled = true;
            }
            catch (Exception ex) { LogPuppetErr("restore LogicController", ex); }
            try
            {
                var corgi = e.ControllerCorgi;
                if (corgi != null) corgi.enabled = true;
            }
            catch (Exception ex) { LogPuppetErr("restore ControllerCorgi", ex); }
            try
            {
                var anim = e.Animator;
                if (anim != null)
                {
                    anim.fireEvents = true;   // 本机现在是权威，攻击动画要能生成本地真实 hitbox
                    anim.cullingMode = AnimatorCullingMode.AlwaysAnimate; // 离屏不被 culling 冻结
                }
            }
            catch (Exception ex) { LogPuppetErr("restore Animator", ex); }

            _puppetized.Remove(e);
            _fireEventsOff.Remove(e);
            return true;
        }

        internal static byte GetTarget(int key)
        {
            Entry en;
            if (_authority.TryGetValue(key, out en) && Time.unscaledTime - en.RecvAt <= StaleTimeout)
                return en.Target;
            return TargetNone;
        }

        internal static void ResetAll()
        {
            _authority.Clear();
            _authorityType.Clear();
            _keyMap.Clear();
            _approxFailAt.Clear();
            _lastTarget.Clear();
            _puppetized.Clear();
            _fireEventsOff.Clear();
            _sendTimer = 0f;
        }
    }
}
