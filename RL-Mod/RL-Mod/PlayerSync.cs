using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL2Multiplayer
{
    public struct PlayerStateData
    {
        public float PositionX;
        public float PositionY;
        public float VelocityX;
        public float VelocityY;
        public bool FacingRight;
        public bool Grounded;
        /// <summary>该玩家处于倒地观战状态（尸体停在原地，不参与移动同步）。</summary>
        public bool Downed;
        /// <summary>本地玩家 Animator 第 0 层当前状态的 shortNameHash（0 = 无动画数据）。</summary>
        public int AnimStateHash;
        /// <summary>当前状态的归一化时间（0..1，循环状态会 >1 累积）。</summary>
        public float AnimNormalizedTime;

        // ---- v1.6.3 全层动画同步 ----
        // 真机联机原来只发第 0 层：Player_Animator 有 12 层，攻击/虚空冲刺等动作
        // 都在更高层 —— 远端永远看不到。现在同步所有层的 (状态hash, 归一化时间)。
        public int[] LayerHashes;
        public float[] LayerTimes;
        // 动画参数按类型顺序快照（本地/远端用同一 Player_Animator，索引天然对齐）。
        // 参数驱动 blend tree 与层权重 —— 攻击方向、蓄力高度都靠它。
        public float[] FloatParams;
        public int[] IntParams;
        public bool[] BoolParams;
    }

    public class PlayerSync : MonoBehaviour
    {
        public static PlayerSync Instance { get; private set; }

        public MonoBehaviour LocalPlayer { get; private set; }
        public GameObject CachedLocalPlayerObject { get; private set; }

        public readonly Dictionary<int, GameObject> RemotePlayers = new Dictionary<int, GameObject>();
        public readonly Dictionary<int, RemotePlayerInterpolator> RemoteInterpolators = new Dictionary<int, RemotePlayerInterpolator>();

        /// <summary>已倒地的远程玩家。倒地的不参与"谁还活着"的统计，也用来把尸体画暗。</summary>
        public readonly HashSet<int> RemoteDowned = new HashSet<int>();

        private Type _playerControllerType;
        private string _playerControllerTypeName;

        private const float LerpSpeed = 20f;

        private float _captureRateLimit;
        private float _stateLogTimer;
        private int _capturedCheckCount;

        private void Awake()
        {
            Instance = this;
            SceneManager.sceneLoaded += OnSceneLoaded;

            Plugin.Log.LogInfo("[RL2MP] PlayerSync.Awake() - Instance set.");

            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPlayerJoin += OnRemotePlayerJoin;
                NetworkManager.Instance.OnPlayerLeave += OnRemotePlayerLeave;
                Plugin.Log.LogInfo("[RL2MP] PlayerSync subscribed to NetworkManager.OnPlayerJoin/Leave.");
            }
            else
            {
                Plugin.Log.LogWarning("[RL2MP] PlayerSync.Awake() but NetworkManager.Instance is null!");
            }
        }

        private void OnDestroy()
        {
            Plugin.Log.LogInfo("[RL2MP] PlayerSync.OnDestroy() - cleaning up.");
            SceneManager.sceneLoaded -= OnSceneLoaded;
            if (NetworkManager.Instance != null)
            {
                NetworkManager.Instance.OnPlayerJoin -= OnRemotePlayerJoin;
                NetworkManager.Instance.OnPlayerLeave -= OnRemotePlayerLeave;
            }
            ClearAllRemote();
            Plugin.Log.LogInfo("[RL2MP] PlayerSync destroyed.");
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.Log.LogInfo("[RL2MP] [SCENE] Loaded: '" + scene.name + "' (mode=" + mode
                + "). Clearing " + RemotePlayers.Count + " remote players, dropping local ref.");
            ClearAllRemote();
            LocalPlayer = null;
            CachedLocalPlayerObject = null;
            _playerControllerType = null;
            _playerControllerTypeName = null;
            _capturedCheckCount = 0;
            _localBodyAnimator = null;
            // 关键：克隆模板也必须丢弃。模板是 DontDestroyOnLoad 缓存的，
            // 不丢的话重开一局（换了新角色）后生成的分身还是上一把的旧外形。
            DiscardCloneTemplate("scene loaded: " + scene.name);

            // 地图同步：客机进完世界后标记"主机地图已生效"
            if (scene.name == "World") MapSync.OnWorldLoaded();
        }

        /// <summary>丢弃克隆模板与外观缓存，下次按当前本地玩家重建。</summary>
        private void DiscardCloneTemplate(string reason)
        {
            if (_cloneTemplate != null)
            {
                Destroy(_cloneTemplate);
                Plugin.Log.LogInfo("[RL2MP] [CLONE] 视觉模板已丢弃 (" + reason + ")。");
            }
            _cloneTemplate = null;
            _cloneTemplateSourceId = 0;
            _cloneFailed = false;
            _remoteSprite = null;
            _visualSourceRenderer = null;
        }

        /// <summary>收到对方角色外观（或外观更新）后，立刻给已有的分身换装。</summary>
        public void RefreshRemoteAppearance(int id)
        {
            GameObject go;
            if (!RemotePlayers.TryGetValue(id, out go) || go == null) return;

            var visual = go.transform.Find("Visual");
            if (visual == null)
            {
                // 还停在兜底白块阶段：正好本地模板可用的话就当场升级，顺手把外观套上
                var template = CachedLocalPlayerObject != null ? GetVisualCloneTemplate() : null;
                if (template != null)
                {
                    Plugin.Log.LogInfo("[RL2MP] [REMOTE] ID=" + id + " 收到外观时还是白块 -> 立刻升级为整树克隆。");
                    var sr = go.GetComponent<SpriteRenderer>();
                    if (sr != null) Destroy(sr);
                    go.transform.localScale = Vector3.one;
                    if (AttachCloneVisual(id, go, template)) return;
                }
                Plugin.Log.LogWarning("[RL2MP] [CHAR] 分身 " + id + " 没有 Visual 子对象，跳过外观刷新。");
                return;
            }
            CharacterSync.ApplyToClone(id, visual.gameObject);
        }

        private void ClearAllRemote()
        {
            int count = RemotePlayers.Count;
            foreach (var go in RemotePlayers.Values)
            {
                if (go != null) Destroy(go);
            }
            RemotePlayers.Clear();
            RemoteInterpolators.Clear();
            RemoteDowned.Clear();       // v1.7.9：换场景/重连时一并清倒地状态，避免残留幽灵尸体
            _downVisualPending.Clear();
            if (count > 0)
            {
                Plugin.Log.LogInfo("[RL2MP] [REMOTE] Cleared " + count + " remote player objects.");
            }
        }

        private void Update()
        {
            _captureRateLimit += Time.unscaledDeltaTime;
            EnsureLocalPlayerCaptured();
            UpdateRemoteSprites();

            // 低频检查：角色换没换（换继承人/重开），换了就重建分身外观
            _templateCheckTimer += Time.unscaledDeltaTime;
            if (_templateCheckTimer >= 0.5f)
            {
                _templateCheckTimer = 0f;
                CheckTemplateFreshness();
                TryUpgradeFallbackVisuals(); // 场景刚切完时先落地成白块的分身，这里补成真克隆
            }
        }

        /// <summary>
        /// 让远程玩家的贴图跟着本地玩家的**身体 renderer** 走（10Hz 刷新）。
        /// 注意：只能抄捕获时选定的那个 renderer（身体层），不能抄"当前激活的任意 renderer"——
        /// RL2 玩家身上随时激活的可能是武器挥砍/特效层，抄那个会让分身变成闪烁的攻击特效。
        /// </summary>
        private float _spriteRefreshTimer;
        private void UpdateRemoteSprites()
        {
            if (RemotePlayers.Count == 0) return;
            var src = _visualSourceRenderer;
            if (src == null || src.sprite == null) return;

            _spriteRefreshTimer += Time.unscaledDeltaTime;
            if (_spriteRefreshTimer < 0.1f) return;
            _spriteRefreshTimer = 0f;

            Sprite current = src.sprite;
            if (current == _remoteSprite) return;

            _remoteSprite = current;
            foreach (var kv in RemotePlayers)
            {
                if (kv.Value == null) continue;
                if (RemoteDowned.Contains(kv.Key)) continue; // 尸体保持最后一帧
                var sr = kv.Value.GetComponent<SpriteRenderer>();
                if (sr != null) sr.sprite = current;
            }
        }

        private void EnsureLocalPlayerCaptured()
        {
            // 倒地期间角色对象被停用，不能去重新捕获（FindObjectsOfType 找不到 inactive 对象，
            // 会把引用清空，导致心跳和状态上报全部失效）。
            if (DownedState.LocalDown) return;

            if (CachedLocalPlayerObject != null && CachedLocalPlayerObject.activeInHierarchy)
            {
                if (LocalPlayer == null || LocalPlayer.gameObject != CachedLocalPlayerObject)
                {
                    LocalPlayer = CachedLocalPlayerObject.GetComponent(_playerControllerType) as MonoBehaviour;
                    Plugin.Log.LogInfo("[RL2MP] [CAPTURE] Re-bound LocalPlayer ref (controller="
                        + (_playerControllerTypeName ?? "null") + ").");
                }
                return;
            }

            if (_playerControllerType == null)
            {
                _playerControllerType = AccessTools.TypeByName("PlayerController");
                if (_playerControllerType != null)
                {
                    _playerControllerTypeName = "PlayerController";
                    Plugin.Log.LogInfo("[RL2MP] [CAPTURE] Found type: PlayerController -> " + _playerControllerType.FullName);
                }
                else
                {
                    _playerControllerType = AccessTools.TypeByName("Player");
                    if (_playerControllerType != null)
                    {
                        _playerControllerTypeName = "Player";
                        Plugin.Log.LogInfo("[RL2MP] [CAPTURE] PlayerController not found, falling back to Player -> " + _playerControllerType.FullName);
                    }
                    else
                    {
                        _capturedCheckCount++;
                        if (_captureRateLimit > 2f)
                        {
                            _captureRateLimit = 0f;
                            Plugin.Log.LogWarning("[RL2MP] [CAPTURE] STILL cannot find PlayerController or Player type. "
                                + "Checked " + _capturedCheckCount + " frames total. Is game scene loaded?");
                        }
                        return;
                    }
                }
            }

            var found = UnityEngine.Object.FindObjectsOfType(_playerControllerType);
            if (found != null && found.Length > 0)
            {
                var mb = found[0] as MonoBehaviour;
                if (mb != null)
                {
                    bool firstTime = (CachedLocalPlayerObject == null);
                    LocalPlayer = mb;
                    CachedLocalPlayerObject = mb.gameObject;
                    if (firstTime)
                    {
                        Plugin.Log.LogInfo("[RL2MP] [CAPTURE] SUCCESS! Captured local player: name='"
                            + mb.gameObject.name + "', type=" + _playerControllerTypeName
                            + ", pos=(" + mb.transform.position.x.ToString("F1") + "," + mb.transform.position.y.ToString("F1") + ")");
                    }
                }
            }
            else
            {
                if (_captureRateLimit > 3f)
                {
                    _captureRateLimit = 0f;
                    Plugin.Log.LogInfo("[RL2MP] [CAPTURE] No " + _playerControllerTypeName
                        + " objects in scene. Waiting for game to start...");
                }
            }
        }

        public PlayerStateData GetLocalPlayerState()
        {
            var state = new PlayerStateData();

            // 倒地时不再读角色（对象已 inactive），停在倒下的位置，让队友看到的是尸体
            if (DownedState.LocalDown)
            {
                state.PositionX = DownedState.LastAlivePos.x;
                state.PositionY = DownedState.LastAlivePos.y;
                state.Downed = true;
                return state;
            }

            if (LocalPlayer == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [STATE] GetLocalPlayerState called but LocalPlayer is null!");
                return state;
            }

            state.PositionX = LocalPlayer.transform.position.x;
            state.PositionY = LocalPlayer.transform.position.y;

            try
            {
                var t = LocalPlayer.GetType();

                var velProp = GetVec2Prop(t, LocalPlayer, "Velocity");
                state.VelocityX = velProp.x;
                state.VelocityY = velProp.y;

                state.FacingRight = GetBoolProp(t, LocalPlayer, "IsFacingRight");
                state.Grounded = GetBoolProp(t, LocalPlayer, "IsGrounded");

                // 动画状态：v1.6.3 起同步所有层 + 全部参数（旧版只发第 0 层，
                // 攻击/冲刺在更高层，远端看不到）
                var anim = EnsureLocalAnimator();
                if (anim != null)
                {
                    int layers = Mathf.Min(anim.layerCount, 16);
                    var hashes = new int[layers];
                    var times = new float[layers];
                    for (int l = 0; l < layers; l++)
                    {
                        try
                        {
                            var info = anim.GetCurrentAnimatorStateInfo(l);
                            // v2.2.0：层权重为 0 的层（攻击/施法动画已经结束、层还没切回去）
                            // 一律发 0，让远端跳过。旧版会把"上一帧遗留的攻击状态 + t=0"
                            // 一直发过去，远端按一次性动画补播 → 分身无限重复攻击动作。
                            bool inactive = l > 0 && anim.GetLayerWeight(l) < 0.02f;
                            hashes[l] = inactive ? 0 : info.shortNameHash;
                            times[l] = inactive ? 0f : info.normalizedTime;
                        }
                        catch { hashes[l] = 0; times[l] = 0f; } // 无状态机/同步层
                    }
                    state.LayerHashes = hashes;
                    state.LayerTimes = times;
                    state.AnimStateHash = hashes.Length > 0 ? hashes[0] : 0;
                    state.AnimNormalizedTime = hashes.Length > 0 ? times[0] : 0f;

                    // 参数快照（float/int/bool 按 controller 参数表顺序；trigger 读不到，靠层状态 scrub 兜底）
                    try
                    {
                        var prms = anim.parameters;
                        int nf = 0, ni = 0, nb = 0;
                        for (int i = 0; i < prms.Length; i++)
                        {
                            switch (prms[i].type)
                            {
                                case AnimatorControllerParameterType.Float: nf++; break;
                                case AnimatorControllerParameterType.Int: ni++; break;
                                case AnimatorControllerParameterType.Bool: nb++; break;
                            }
                        }
                        var fp = new float[nf];
                        var ip = new int[ni];
                        var bp = new bool[nb];
                        int a = 0, b = 0, c = 0;
                        for (int i = 0; i < prms.Length; i++)
                        {
                            switch (prms[i].type)
                            {
                                case AnimatorControllerParameterType.Float: fp[a++] = anim.GetFloat(prms[i].name); break;
                                case AnimatorControllerParameterType.Int: ip[b++] = anim.GetInteger(prms[i].name); break;
                                case AnimatorControllerParameterType.Bool: bp[c++] = anim.GetBool(prms[i].name); break;
                            }
                        }
                        state.FloatParams = fp;
                        state.IntParams = ip;
                        state.BoolParams = bp;
                    }
                    catch { /* 参数读不到就跳过，层状态照发 */ }
                }

                // 状态日志单独节流（以前借用 _captureRateLimit，捕获成功后它不再清零 → 每帧刷屏）
                _stateLogTimer += Time.unscaledDeltaTime;
                if (_stateLogTimer > 10f && Math.Abs(state.PositionX) + Math.Abs(state.PositionY) > 0.01f)
                {
                    _stateLogTimer = 0f;
                    Plugin.Log.LogInfo("[RL2MP] [STATE] captured -> pos=(" + state.PositionX.ToString("F2") + "," + state.PositionY.ToString("F2")
                        + ") vx=" + state.VelocityX.ToString("F2") + " vy=" + state.VelocityY.ToString("F2")
                        + " facing=" + state.FacingRight + " grounded=" + state.Grounded);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [STATE] prop read exception: " + ex.Message);
            }

            return state;
        }

        private static Vector2 GetVec2Prop(Type t, object obj, string name)
        {
            var cur = t;
            while (cur != null)
            {
                var p = AccessTools.Property(cur, name);
                if (p != null)
                {
                    try
                    {
                        object v = p.GetValue(obj);
                        if (v is Vector2) return (Vector2)v;
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogWarning("[RL2MP] [PROP] " + cur.Name + "." + name + " getter threw: " + ex.Message);
                    }
                    return Vector2.zero;
                }
                cur = cur.BaseType;
            }
            return Vector2.zero;
        }

        private static bool GetBoolProp(Type t, object obj, string name)
        {
            var cur = t;
            while (cur != null)
            {
                var p = AccessTools.Property(cur, name);
                if (p != null)
                {
                    try
                    {
                        object v = p.GetValue(obj);
                        if (v is bool) return (bool)v;
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogWarning("[RL2MP] [PROP] " + cur.Name + "." + name + " getter threw: " + ex.Message);
                    }
                    return false;
                }
                cur = cur.BaseType;
            }
            return false;
        }

        /// <summary>
        /// 分层状态入口（v1.6.0）：完整包已拆成 位置/动画/标志 三个独立消息。
        /// 这个方法保留给本地调试模拟（DebugSim）和兼容调用使用。
        /// </summary>
        public void HandleRemotePlayerState(int id, float x, float y, float vx, float vy, bool facingRight, bool grounded, bool downed,
            int animHash = 0, float animTime = 0f)
        {
            HandleRemoteTransform(id, x, y, vx, vy, facingRight, grounded);
            HandleRemoteAnim(id, animHash, animTime);
            HandleRemoteFlags(id, downed);
        }

        /// <summary>确保远端分身对象存在，不存在则创建。返回是否是新创建的。</summary>
        private bool EnsureRemoteObject(int id, out GameObject go, out RemotePlayerInterpolator interpolator)
        {
            go = null;
            interpolator = null;
            bool createdNew = false;
            if (!RemotePlayers.TryGetValue(id, out go) || go == null)
            {
                Plugin.Log.LogInfo("[RL2MP] [REMOTE] Creating NEW remote player visual for ID=" + id
                    + " (RemotePlayers didn't contain it or obj was destroyed).");
                go = CreateRemotePlayerObject(id);
                RemotePlayers[id] = go;
                createdNew = true;
            }

            interpolator = go.GetComponent<RemotePlayerInterpolator>();
            if (interpolator == null)
            {
                Plugin.Log.LogInfo("[RL2MP] [REMOTE] Adding RemotePlayerInterpolator component to RemotePlayer_" + id);
                interpolator = go.AddComponent<RemotePlayerInterpolator>();
            }

            // v2.1.0：分身受击盒（怪物子弹/近战/尖刺能"碰到"对面玩家，伤害转发给真身）。
            // 幂等：重复调用只更新 OwnerId。
            RemoteHurtbox.Attach(go, id);

            // v1.7.9：若该 id 曾在我们没有分身时被标记倒地（downed 事件先到），现在补套。
            if (createdNew && RemoteDowned.Contains(id))
            {
                Plugin.Log.LogInfo("[RL2MP] [REMOTE] ID=" + id + " 新建分身时补套倒地外观（downed 曾早到）。");
                ApplyDownedVisual(go, true, id);
            }
            return createdNew;
        }

        /// <summary>位置/朝向包（20Hz，Sequenced）：只更新插值目标，不碰动画和倒地状态。</summary>
        public void HandleRemoteTransform(int id, float x, float y, float vx, float vy, bool facingRight, bool grounded)
        {
            GameObject go;
            RemotePlayerInterpolator interpolator;
            bool createdNew = EnsureRemoteObject(id, out go, out interpolator);

            // v2.4.0：尸体不再跟随位置包。
            // 旧版每个位置包都 SetTarget(..., freeze=false)，把刚冻结的尸体又解冻挪走，
            // 于是"死了还在飘"。现在倒地期间位置完全由尸体下落逻辑接管。
            if (!createdNew && RemoteDowned.Contains(id)) return;

            var newTarget = new Vector3(x, y, go.transform.position.z);
            if (createdNew)
            {
                Plugin.Log.LogInfo("[RL2MP] [REMOTE] ID=" + id + " target set to ("
                    + x.ToString("F2") + "," + y.ToString("F2") + ") facingRight=" + facingRight
                    + " (fresh target after creation).");
            }
            // v3.0.24：把包里带来的速度也交给插值器 —— 两包之间用它外推，
            // 分身不再"站住等下一包"，观战镜头也就不会一格一格地卡。
            interpolator.SetTarget(newTarget, facingRight, new Vector2(vx, vy));
        }

        /// <summary>动画包（10Hz）：分身不存在时静默丢弃，等位置包把它建出来。</summary>
        public void HandleRemoteAnim(int id, int[] layerHashes, float[] layerTimes, float[] floatParams, int[] intParams, bool[] boolParams)
        {
            GameObject go;
            RemotePlayerInterpolator interpolator;
            if (!RemotePlayers.TryGetValue(id, out go) || go == null) return;
            // v2.8.6：对端倒地后动画包会变成空数组，若继续灌进分身，
            // 镜像层会退化成"拿本机存活玩家的 Animator 逐帧套到尸体上"。
            // 源头挡掉，让尸体保持死亡瞬间的姿态；对方复活后会重新发包。
            if (RemoteDowned.Contains(id)) return;
            interpolator = go.GetComponent<RemotePlayerInterpolator>();
            if (interpolator == null) return;
            interpolator.SetAnimTarget(layerHashes, layerTimes, floatParams, intParams, boolParams);
        }

        /// <summary>旧签名（调试模拟等只用第 0 层的调用方）。</summary>
        public void HandleRemoteAnim(int id, int animHash, float animTime)
        {
            HandleRemoteAnim(id, new int[] { animHash }, new float[] { animTime }, null, null, null);
        }

        /// <summary>状态位包（变化时才发，Reliable）：倒地/起身。</summary>
        public void HandleRemoteFlags(int id, bool downed)
        {
            SetRemoteDowned(id, downed);
        }

        /// <summary>
        /// 远程玩家倒地：尸体停在当前格位置并画成半透明灰，站起来后恢复。
        /// 这里不改 active 状态——倒地的人仍然要被同步位置（复活时才知道他在哪）。
        /// </summary>
        public void SetRemoteDowned(int id, bool downed)
        {
            GameObject go;
            RemotePlayers.TryGetValue(id, out go); // v1.7.9：分身可能还没创建，不再提前 return

            bool was = RemoteDowned.Contains(id);
            if (downed == was && (go == null || !_downVisualPending.Contains(id))) return;

            // v1.7.9 关键修复：先无条件记录/清除状态。旧版在分身不存在时直接 return，
            // 而 downed 事件包只发一次 -> 状态永久丢失：尸体显示为活人、RemoteDowned
            // 集合为空导致长按 H 搜不到救援目标。现在状态先落地，分身一旦创建就补套外观。
            if (downed)
            {
                RemoteDowned.Add(id);
                if (go == null) _downVisualPending.Add(id); // 等分身创建后补套灰化/冻结
            }
            else
            {
                RemoteDowned.Remove(id);
                _downVisualPending.Remove(id);
            }

            if (go == null) return; // 状态已记住，外观等分身就绪再套

            ApplyDownedVisual(go, downed, id);
            Plugin.Log.LogInfo("[RL2MP] [REMOTE] id=" + id + (downed ? " is DOWN" : " is back UP")
                + " (downedCount=" + RemoteDowned.Count + "/" + RemotePlayers.Count + ")");
        }

        /// <summary>倒地意图已记录但分身当时不存在的 id，分身创建后补套外观。</summary>
        private readonly HashSet<int> _downVisualPending = new HashSet<int>();

        /// <summary>统一套用/恢复倒地外观（隐藏模型 + 冻结位置）。倒地者位置来自其稳定广播坐标。</summary>
        private void ApplyDownedVisual(GameObject go, bool downed, int id)
        {
            // v2.8.7（借鉴 rl2-coop Puppet.cs）：对端倒地时直接隐藏"Visual"模型子树，
            // 而不只是把 Animator.speed 设 0。旧版只停动画、停粒子，模型节点仍在场上，
            // 存活方画面上就一直挂着一具不动的尸体，还占着仇恨/遮挡视线。
            // 只隐藏 Visual 子节点，不隐藏 root —— root 上的 RemoteTargetProxy /
            // RemoteHurtbox / Interpolator 还要给观战 Follow、怪物仇恨、复活定位用。
            // 起身时恢复（复活/新局），与 rl2-coop "隐藏 animator.gameObject, 不销毁 root" 一致。
            SetVisualHidden(go, downed);

            var interp = go.GetComponent<RemotePlayerInterpolator>();
            // v2.8.6：把倒地状态传给镜像层，让它停掉"本机动画 → 尸体"的兜底镜像。
            if (interp != null) interp.Downed = downed;
            if (interp != null)
            {
                if (downed)
                    interp.SetTarget(go.transform.position, true, true); // freeze 在原地
                else
                    interp.SetTarget(go.transform.position, true, false);
            }
            // 克隆模式下没有根级 SpriteRenderer，且材质与本地玩家共享，改色会连累原体，
            // 因此不对克隆做逐 renderer 调色；倒地视觉靠冻结位置 + HUD/尸体标记表达。
            _downVisualPending.Remove(id);

            // v2.2.0：倒地/死亡时把分身"收干净"。
            // 之前只冻结位置，分身的 Animator 还在按最后一份网络状态补播、
            // 身上的粒子也还在发射 —— 表现就是"人已经倒了，对面画面上他还在不停攻击、
            // 法术特效一直挂着"。这里冻结动画 + 停掉所有粒子。
            try
            {
                var anims = go.GetComponentsInChildren<Animator>(true);
                for (int i = 0; i < anims.Length; i++)
                {
                    if (anims[i] != null) anims[i].speed = downed ? 0f : 1f;
                }
                if (downed)
                {
                    var pss = go.GetComponentsInChildren<ParticleSystem>(true);
                    for (int i = 0; i < pss.Length; i++)
                    {
                        if (pss[i] != null)
                            pss[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
            }
            catch { }
        }

        /// <summary>
        /// v2.8.7：隐藏/恢复分身"模型"本身（rl2-coop 观战原则：死亡方不占画面）。
        /// 整树克隆模式隐藏 Visual 子节点；白块兜底模式关掉 root 的 SpriteRenderer。
        /// 绝不 SetActive(false) root —— 观战 Follow、怪物仇恨锚点、复活定位都挂在 root 上。
        /// </summary>
        private void SetVisualHidden(GameObject go, bool hidden)
        {
            try
            {
                var visual = go.transform.Find("Visual");
                if (visual != null) { visual.gameObject.SetActive(!hidden); return; }
                // v3.0.4：整树克隆模式下渲染器可能挂在任意子节点（不一定叫 Visual，也不一定在 root）。
                // 旧版只查 root 的 GetComponents<SpriteRenderer> -> 克隆分身找不到任何 renderer ->
                // 倒地分身的模型一直在场上（用户反馈"主机的模型一直在，只是没动作"）。
                // 改为递归查所有子节点；不隐藏 root（观战 Follow / 仇恨锚点还要用）。
                var srs = go.GetComponentsInChildren<SpriteRenderer>(true);
                for (int i = 0; i < srs.Length; i++) if (srs[i] != null) srs[i].enabled = !hidden;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [REMOTE] SetVisualHidden(" + go.name + ") 失败: " + ex.Message);
            }
        }

        // ---------------- 远程玩家外观（从本地玩家身上抄 sprite / 缩放 / 排序层） ----------------
        private Sprite _remoteSprite;
        private Sprite _fallbackSprite;
        private SpriteRenderer _visualSourceRenderer; // 身体层 renderer，动画帧从这里同步
        private Animator _localBodyAnimator; // 本地玩家身体 Animator，动画状态从这里镜像给分身
        private float _remoteScaleX = 1f;
        private float _remoteScaleY = 1f;
        private int _remoteSortingLayerId;
        private int _remoteSortingOrder = 32000;
        private float _remoteZ;

        /// <summary>
        /// 找本地玩家的"身体"Animator（有 controller 且启用的第一个）。
        /// 找不到时返回 null，动画同步静默降级为待机。
        /// </summary>
        private Animator EnsureLocalAnimator()
        {
            if (_localBodyAnimator != null) return _localBodyAnimator;
            if (CachedLocalPlayerObject == null) return null;

            try
            {
                var anims = CachedLocalPlayerObject.GetComponentsInChildren<Animator>(true);
                foreach (var a in anims)
                {
                    if (a == null || a.runtimeAnimatorController == null) continue;
                    if (!a.enabled) continue;
                    _localBodyAnimator = a;
                    Plugin.Log.LogInfo("[RL2MP] [ANIM] 本地身体 Animator: '" + a.transform.name
                        + "' controller=" + a.runtimeAnimatorController.name
                        + " layers=" + a.layerCount
                        + " params=" + a.parameterCount);
                    return _localBodyAnimator;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ANIM] 扫描本地 Animator 异常: " + ex.Message);
            }
            return null;
        }

        /// <summary>供 RemoteAnimatorMirror 惰性获取本地身体 Animator。</summary>
        internal Animator GetLocalBodyAnimator()
        {
            return EnsureLocalAnimator();
        }

        /// <summary>
        /// 从本地玩家身上抓一份外观参数。只抓一次（抓到 sprite 就缓存）。
        /// 选片规则：**面积最大的 sprite**。RL2 玩家由多层 sprite 组成（身体/装备/武器特效），
        /// 随时激活的层往往是武器挥砍之类的特效，不能按"是否激活"选——身体层永远是最大的那张。
        /// </summary>
        private void TryCaptureVisualSource()
        {
            if (_remoteSprite != null) return;
            if (CachedLocalPlayerObject == null) return;

            try
            {
                var srs = CachedLocalPlayerObject.GetComponentsInChildren<SpriteRenderer>(true);
                SpriteRenderer best = null;
                float bestArea = 0f;
                var list = new System.Text.StringBuilder();

                foreach (var sr in srs)
                {
                    if (sr == null || sr.sprite == null) continue;
                    var r = sr.sprite.rect;
                    float area = r.width * r.height;
                    list.Append(sr.gameObject.name).Append("='").Append(sr.sprite.name)
                        .Append("' ").Append((int)r.width).Append("x").Append((int)r.height)
                        .Append(sr.gameObject.activeInHierarchy ? " [on] " : " [off] ");
                    if (area > bestArea)
                    {
                        bestArea = area;
                        best = sr;
                    }
                }

                if (best == null)
                {
                    Plugin.Log.LogInfo("[RL2MP] [SPRITE] 本地玩家身上没有可用 SpriteRenderer（共 " + srs.Length + " 个），继续用兜底白块。");
                    return;
                }

                _remoteSprite = best.sprite;
                _visualSourceRenderer = best;
                _remoteSortingLayerId = best.sortingLayerID;
                _remoteSortingOrder = best.sortingOrder;
                _remoteZ = CachedLocalPlayerObject.transform.position.z;

                var ls = best.transform.lossyScale;
                _remoteScaleX = Mathf.Max(0.01f, Mathf.Abs(ls.x));
                _remoteScaleY = Mathf.Max(0.01f, Mathf.Abs(ls.y));

                Plugin.Log.LogInfo("[RL2MP] [SPRITE] 候选: " + list + " | 选定 '"
                    + best.gameObject.name + "' (最大 " + (int)best.sprite.rect.width + "x"
                    + (int)best.sprite.rect.height + ")");
                Plugin.Log.LogInfo("[RL2MP] [SPRITE] 外观已捕获：sprite='" + best.sprite.name + "'"
                    + " sortLayer=" + best.sortingLayerName + "(" + _remoteSortingLayerId + ")"
                    + " sortOrder=" + _remoteSortingOrder
                    + " scale=(" + _remoteScaleX.ToString("F2") + "," + _remoteScaleY.ToString("F2") + ")"
                    + " z=" + _remoteZ.ToString("F2"));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SPRITE] 扫描本地 renderer 异常: " + ex.Message);
            }
        }

        private GameObject _cloneTemplate; // 剥离脚本后的玩家整树克隆模板
        private int _cloneTemplateSourceId; // 模板是从哪个本地玩家实例克隆的（InstanceID）
        private bool _cloneFailed;

        /// <summary>
        /// 本地角色一旦更换（重开一局 / 换继承人），旧模板就作废：
        /// 丢弃模板并销毁所有远程分身，下一个状态包会让它们用新模板重建。
        /// </summary>
        private float _templateCheckTimer;
        private void CheckTemplateFreshness()
        {
            if (RemotePlayers.Count == 0) return;
            if (_cloneTemplate == null || _cloneTemplateSourceId == 0) return;
            if (CachedLocalPlayerObject == null) return;
            if (CachedLocalPlayerObject.GetInstanceID() == _cloneTemplateSourceId) return;

            Plugin.Log.LogInfo("[RL2MP] [CLONE] 本地角色已更换（instanceId " + _cloneTemplateSourceId
                + " -> " + CachedLocalPlayerObject.GetInstanceID() + "），重建远程分身外观。");
            DiscardCloneTemplate("local character changed");
            ClearAllRemote(); // 状态包每 50ms 一次，马上会用新模板重建
        }

        /// <summary>
        /// 克隆本地玩家整棵视觉树，剥掉脚本/碰撞体/UI/音频，只留 Transform + Animator +
        /// Mesh/SkinnedMesh 渲染器 + 粒子。RL2 的角色是 SkinnedMeshRenderer 骨骼网格
        /// （dump 已确认），抄单张贴图抄不到身体，只有整树克隆才有骨架。
        /// 模板只做一次；之后每个远程玩家 Instantiate 一份。
        /// </summary>
        private GameObject GetVisualCloneTemplate()
        {
            if (_cloneTemplate != null)
            {
                // 场景没换但角色换了（死亡回出生点换继承人等）：旧模板作废，重建
                if (CachedLocalPlayerObject != null && CachedLocalPlayerObject.GetInstanceID() != _cloneTemplateSourceId)
                {
                    DiscardCloneTemplate("local player object changed in-scene");
                }
                else
                {
                    return _cloneTemplate;
                }
            }
            if (_cloneFailed) return null;
            if (CachedLocalPlayerObject == null) return null;

            GameObject original = CachedLocalPlayerObject;
            bool wasActive = original.activeSelf;
            try
            {
                // 先停用原体再克隆：避免克隆体上的脚本 Awake 抢静态单例
                if (wasActive) original.SetActive(false);
                var clone = UnityEngine.Object.Instantiate(original);
                clone.name = "RL2MP_PlayerVisualTemplate";

                int removed = StripClone(clone);
                clone.SetActive(false);
                UnityEngine.Object.DontDestroyOnLoad(clone);
                _cloneTemplate = clone;
                _cloneTemplateSourceId = original.GetInstanceID();

                Plugin.Log.LogInfo("[RL2MP] [CLONE] 视觉模板创建成功，剥离组件 " + removed
                    + " 个。子物体 " + clone.transform.childCount + " 个。");
            }
            catch (Exception ex)
            {
                _cloneFailed = true;
                Plugin.Log.LogWarning("[RL2MP] [CLONE] 克隆玩家视觉树失败，回退到 sprite 方案: " + ex.Message);
            }
            finally
            {
                if (wasActive && !original.activeSelf) original.SetActive(true);
            }
            return _cloneTemplate;
        }

        /// <summary>剥掉克隆体上除视觉/动画以外的所有组件，返回剥掉的数量。</summary>
        private static int StripClone(GameObject root)
        {
            int removed = 0;
            foreach (var t in root.GetComponentsInChildren<Transform>(true))
            {
                var comps = t.GetComponents<Component>();
                for (int i = comps.Length - 1; i >= 0; i--)
                {
                    var c = comps[i];
                    if (c == null) continue;
                    if (c is Transform) continue;
                    if (c is Animator) continue;
                    if (c is SkinnedMeshRenderer || c is MeshRenderer || c is MeshFilter) continue;
                    if (c is ParticleSystem || c is ParticleSystemRenderer) continue;
                    // LookController 必须保留：分身的职业套装/装备/五官全靠它，
                    // 而且它的 geo 引用是 [SerializeField]（预制体上配好的），
                    // 销毁了就再也重建不出正确外观。脚本逻辑由 CharacterSync 里 enabled=false 关掉。
                    if (c is LookController) continue;
                    // 其余全剥：脚本、碰撞体、刚体、Canvas、AudioSource、FMOD、TextMeshPro……
                    UnityEngine.Object.DestroyImmediate(c);
                    removed++;
                }
            }
            return removed;
        }

        private GameObject CreateRemotePlayerObject(int id)
        {
            Plugin.Log.LogInfo("[RL2MP] [REMOTE] CreateRemotePlayerObject(" + id + ")...");

            TryCaptureVisualSource();

            GameObject go = new GameObject("RemotePlayer_" + id);
            go.transform.position = new Vector3(0, 0, _remoteZ);
            go.transform.localScale = Vector3.one;

            // v2.8.5：给分身 root 挂一个极简 BaseCharacterController 代理。
            // 一方倒地后 EnemyAiFix 会把怪的 e.Target 指向存活方分身，而
            // EnemyController.Target setter 要 GetComponent<BaseCharacterController>()；
            // 分身是剥光脚本的视觉克隆，没有它 TargetController=null，怪物 AI 读
            // TargetController.Midpoint 时空引用崩溃 → 表现就是"怪不打存活方"。
            // 代理不初始化、不参与伤害，只提供坐标锚点（Midpoint 回退到 transform.position）。
            try
            {
                if (go.GetComponent<RemoteTargetProxy>() == null)
                    go.AddComponent<RemoteTargetProxy>();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[REMOTE] ID=" + id + " 挂 TargetProxy 失败: " + ex.Message);
            }

            // 首选：整树克隆（有完整骨架和动画）
            var template = GetVisualCloneTemplate();
            if (template != null)
            {
                AttachCloneVisual(id, go, template);
                return go;
            }

            // 兜底：单 sprite 方案。
            // 注意：这里多半发生在"场景刚切完、本地玩家还没被捕获"的瞬间（例如客机套用主机地图
            // 后强制重进 World），所以不能就此定终身——Update 里的 TryUpgradeFallbackVisuals()
            // 会在本地玩家出现后把这些白块自动升级成整树克隆。
            Plugin.Log.LogWarning("[RL2MP] [REMOTE] ID=" + id
                + " 创建时拿不到本地玩家模板（本地角色可能还没进场景），先放兜底白块，稍后自动升级。");
            go.transform.localScale = new Vector3(_remoteScaleX, _remoteScaleY, 1f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = _remoteSprite != null ? _remoteSprite : GetFallbackSprite();
            sr.sortingLayerID = _remoteSortingLayerId;
            sr.sortingOrder = _remoteSortingOrder + 1;
            sr.color = Color.white;

            Plugin.Log.LogInfo("[RL2MP] [REMOTE] ID=" + id + " 外观: "
                + (_remoteSprite != null ? "copied '" + _remoteSprite.name + "'" : "fallback 白块")
                + ", sorting=" + sr.sortingLayerName + "/" + sr.sortingOrder
                + ", scale=(" + go.transform.localScale.x.ToString("F2") + "," + go.transform.localScale.y.ToString("F2") + ")"
                + ", z=" + go.transform.position.z.ToString("F2"));

            return go;
        }

        /// <summary>
        /// 给 RemotePlayer_x 挂上"Visual"子物体（整树克隆 + 外观重建 + 动画镜像）。
        /// 已经是克隆体的话直接返回 false（幂等，可安全重复调用）。
        /// </summary>
        private bool AttachCloneVisual(int id, GameObject go, GameObject template)
        {
            if (go == null || template == null) return false;
            if (go.transform.Find("Visual") != null) return false; // 已经有克隆体了

            var clone = UnityEngine.Object.Instantiate(template);
            clone.name = "Visual";
            clone.transform.SetParent(go.transform, false);
            clone.transform.localPosition = Vector3.zero;

            // v2.8.5：修观战时"位置往前、动画却像倒着走"。
            // 游戏原生 Character.Flip() 翻的是 CharacterModel 子树（localScale.x = -1），
            // 整树克隆会把这个负 x 一并带进 Visual；而分身每帧又靠父 go 的 localScale.x
            // 按主机朝向翻面 —— 两套翻转叠加，模型朝向就和主机传来的带符号动画参数
            // （FacingSpeed/Speed）对不上，看起来就是"往右平移却播向后走"。
            // 把模板树里残留的负 x 规范成正，让翻面只由父 go 一处决定。
            NormalizeCloneScale(clone);

            // 外观必须先于 SetActive：LookController 的 OnEnable/Start 会注册事件、
            // 甚至等 PlayerController（分身没有），所以在激活前就关掉它的逻辑，
            // 只把它当成"外观数据容器"来用。
            var look = clone.GetComponentInChildren<LookController>(true);
            if (look != null) look.enabled = false;

            clone.SetActive(true); // Animator 从默认状态（待机）开始播

            // 用对方真实的 CharacterData 重建外观（职业套装/装备/五官/肤色）
            bool lookApplied = CharacterSync.ApplyToClone(id, clone);

            // 绑定动画/特效镜像：按相对路径把本地树上所有 Animator + 粒子系统配对到克隆体
            var mirror = go.GetComponent<RemoteAnimatorMirror>();
            if (mirror == null) mirror = go.AddComponent<RemoteAnimatorMirror>();
            if (CachedLocalPlayerObject != null) mirror.Bind(CachedLocalPlayerObject, clone);

            // 从白块升级上来的话，把翻面基准缩放从 0.77 之类重置成 1，避免被压扁
            var interp = go.GetComponent<RemotePlayerInterpolator>();
            if (interp != null) interp.SetVisualScale(1f);
            else go.transform.localScale = Vector3.one;

            Plugin.Log.LogInfo("[RL2MP] [REMOTE] ID=" + id + " 外观: 整树克隆（骨架网格 + Animator + 动画镜像）"
                + (lookApplied ? " + 对方角色外观" : "（等对方外观包，收到后会刷新）"));
            return true;
        }

        /// <summary>
        /// v2.8.5：把克隆视觉树里残留的负 x 缩放规范为正。
        /// 只处理 |x| 接近 1 的翻转节点（Character.Flip 用 FlipValue=(-1,1,1) 翻模型），
        /// 不动美术上刻意做镜像/缩放的子物体，避免改变外观。
        /// </summary>
        private static void NormalizeCloneScale(GameObject clone)
        {
            var all = clone.GetComponentsInChildren<Transform>(true);
            int fixedCount = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var t = all[i];
                if (t == null) continue;
                var s = t.localScale;
                // 只认"整体翻面"：x 为负且绝对值在 [0.9, 1.1]，y/z 为正
                if (s.x < 0f && Mathf.Abs(s.x) >= 0.9f && Mathf.Abs(s.x) <= 1.1f
                    && s.y > 0f && s.z > 0f)
                {
                    s.x = Mathf.Abs(s.x);
                    t.localScale = s;
                    fixedCount++;
                }
            }
            if (fixedCount > 0)
                Plugin.Log.LogInfo("[RL2MP] [REMOTE] 视觉模板朝向已规范化，修正 " + fixedCount
                    + " 个翻转节点（翻面改由分身 root 统一负责）");
        }

        /// <summary>
        /// 兜底白块 → 整树克隆的自动升级。
        ///
        /// 场景切换（尤其是客机套用主机地图后的强制重进 World）时，状态包往往先于
        /// 本地角色对象到达，那时 GetVisualCloneTemplate() 返回 null，分身只能退化成
        /// 一个 sortingOrder 很低的白块——玩家看到的就是"对方不见了"。
        /// 这里每 0.5 秒扫一遍降级的分身，一旦本地玩家出现就当场升级成真克隆。
        /// </summary>
        private void TryUpgradeFallbackVisuals()
        {
            if (RemotePlayers.Count == 0) return;
            if (CachedLocalPlayerObject == null) return; // 本地角色还没进场景，再等等

            var template = GetVisualCloneTemplate();
            if (template == null) return;

            List<int> pending = null;
            foreach (var kv in RemotePlayers)
            {
                if (kv.Value == null) continue;
                if (kv.Value.transform.Find("Visual") != null) continue;
                (pending ?? (pending = new List<int>())).Add(kv.Key);
            }
            if (pending == null) return;

            foreach (int id in pending)
            {
                GameObject go;
                if (!RemotePlayers.TryGetValue(id, out go) || go == null) continue;

                Plugin.Log.LogInfo("[RL2MP] [REMOTE] ID=" + id + " 兜底白块 -> 升级为整树克隆。");
                var sr = go.GetComponent<SpriteRenderer>();
                if (sr != null) Destroy(sr);
                go.transform.localScale = Vector3.one;
                if (!AttachCloneVisual(id, go, template))
                {
                    Plugin.Log.LogWarning("[RL2MP] [REMOTE] ID=" + id + " 升级失败，下次再试。");
                }
            }
        }

        private Sprite GetFallbackSprite()
        {
            if (_fallbackSprite != null) return _fallbackSprite;

            Plugin.Log.LogInfo("[RL2MP] [SPRITE] Creating 16x16 white fallback sprite (once)...");
            var tex = new Texture2D(16, 16, TextureFormat.RGBA32, false);
            for (int i = 0; i < 16; i++)
                for (int j = 0; j < 16; j++)
                    tex.SetPixel(i, j, Color.white);
            tex.Apply();
            _fallbackSprite = Sprite.Create(tex, new Rect(0, 0, 16, 16), new Vector2(0.5f, 0.5f), 16);
            return _fallbackSprite;
        }

        /// <summary>
        /// 诊断：把本地玩家整棵树的所有物体/组件/renderer 打出来，
        /// 用来确认"身体"到底画在什么组件上（SpriteRenderer / MeshRenderer / SkinnedMeshRenderer）。
        /// </summary>
        internal void DumpPlayerStructure()
        {
            if (CachedLocalPlayerObject == null)
            {
                Plugin.Log.LogInfo("[RL2MP] [DUMP] no local player captured.");
                return;
            }
            Plugin.Log.LogInfo("[RL2MP] [DUMP] ===== 玩家对象结构 begin: " + CachedLocalPlayerObject.name + " =====");
            DumpTransform(CachedLocalPlayerObject.transform, 0);
            Plugin.Log.LogInfo("[RL2MP] [DUMP] ===== 玩家对象结构 end =====");
        }

        private static void DumpTransform(Transform t, int depth)
        {
            var sb = new System.Text.StringBuilder();
            sb.Append(' ', depth * 2).Append(t.name).Append(t.gameObject.activeSelf ? "" : " [INACTIVE]");
            foreach (var c in t.GetComponents<Component>())
            {
                if (c == null) { sb.Append(" | <null-comp>"); continue; }
                var tn = c.GetType().Name;
                if (tn == "Transform") continue;

                var sr = c as SpriteRenderer;
                if (sr != null)
                {
                    var sp = sr.sprite;
                    sb.Append(" | SR").Append(sr.enabled ? "" : "(disabled)")
                      .Append(" layer=").Append(sr.sortingLayerName).Append('/').Append(sr.sortingOrder)
                      .Append(" sprite=").Append(sp != null ? sp.name + " " + (int)sp.rect.width + "x" + (int)sp.rect.height : "<null>")
                      .Append(sr.gameObject.activeInHierarchy ? "" : " [not-in-hierarchy]");
                    continue;
                }
                if (c is MeshRenderer) { sb.Append(" | MeshR").Append(((MeshRenderer)c).enabled ? "" : "(disabled)"); continue; }
                if (c is SkinnedMeshRenderer) { sb.Append(" | SkinnedMeshR"); continue; }
                if (tn == "Animator") sb.Append(" | Animator");
                else sb.Append(" | ").Append(tn);
            }
            Plugin.Log.LogInfo("[RL2MP] [DUMP] " + sb.ToString());

            for (int i = 0; i < t.childCount; i++)
                DumpTransform(t.GetChild(i), depth + 1);
        }

        private void OnRemotePlayerJoin(int id)
        {
            Plugin.Log.LogInfo("[RL2MP] [EVENT] OnRemotePlayerJoin id=" + id + ". Pre-trigger check -> RemotePlayers.Contains=" + RemotePlayers.ContainsKey(id));
            if (RemotePlayers.ContainsKey(id) && RemotePlayers[id] != null)
            {
                Plugin.Log.LogInfo("[RL2MP] [EVENT] RemotePlayer_" + id + " already exists, skipping HandleRemotePlayerState creation.");
                return;
            }
            Plugin.Log.LogInfo("[RL2MP] [EVENT] Calling HandleRemotePlayerState(" + id + ", 0,0,0,0,true,false) to force-create visual.");
            HandleRemotePlayerState(id, 0, 0, 0, 0, true, false, false);
        }

        private void OnRemotePlayerLeave(int id)
        {
            Plugin.Log.LogInfo("[RL2MP] [EVENT] OnRemotePlayerLeave id=" + id + ". Removing visual object.");
            GameObject go;
            if (RemotePlayers.TryGetValue(id, out go) && go != null)
            {
                Plugin.Log.LogInfo("[RL2MP] [EVENT] Destroying RemotePlayer_" + id + " GameObject.");
                Destroy(go);
            }
            else
            {
                Plugin.Log.LogInfo("[RL2MP] [EVENT] RemotePlayers[" + id + "] not found or already destroyed.");
            }
            RemotePlayers.Remove(id);
            RemoteInterpolators.Remove(id);
            Plugin.Log.LogInfo("[RL2MP] [EVENT] After leave -> RemotePlayers count=" + RemotePlayers.Count);
        }
    }

    public class RemotePlayerInterpolator : MonoBehaviour
    {
        private Vector3 _targetPos;
        private bool _targetFacingRight;
        private float _visualFacingScaleX;
        private bool _frozen;

        // v2.8.6：该分身的拥有者已倒地。镜像层据此停掉动画/粒子兜底同步，
        // 避免"尸体跟着存活方的动作一起动"。复活时会被置回 false。
        internal bool Downed;

        // v2.4.0：尸体下坠。旧版倒地直接冻结位置 —— 人死在空中，尸体就永远悬在半空。
        private bool _corpseFalling;
        private float _corpseVy;
        private float _corpseFallTime;

        // 网络送来的动画状态（由 RemoteAnimatorMirror 消费）—— v1.6.3 全层 + 参数
        internal int[] TargetLayerHashes = new int[0];
        internal float[] TargetLayerTimes = new float[0];
        internal float[] TargetFloatParams;
        internal int[] TargetIntParams;
        internal bool[] TargetBoolParams;
        /// <summary>收到动画包的时刻（unscaled）。镜像组件用它预测当前应处的动画时间。</summary>
        internal float AnimPacketTime;

        private const float LerpSpeed = 18f;

        /// <summary>v3.0.24：最近一次位置包带的速度（世界单位/秒）。位置包只有 20Hz，
        /// 两包之间分身原本是"站住等下一包"的台阶；观战镜头跟着分身走，台阶就变成一格一格的卡顿。
        /// 用包里本来就有的 vx/vy 在两包之间外推一小段，台阶就消了。</summary>
        private Vector2 _targetVel;
        /// <summary>v3.0.24：最近一次位置包的到达时刻（unscaled）。</summary>
        private float _targetAtUnscaled = -999f;
        /// <summary>v3.0.24：外推时长上限（秒）。对端急停/转向时不能一直冲出去。</summary>
        private const float MaxExtrapolate = 0.12f;
        /// <summary>v3.0.24：单次外推的最大位移（米）。被击飞时速度很大，不限幅会冲过头。</summary>
        private const float MaxLead = 2.5f;
        private const float MaxLeadSqr = MaxLead * MaxLead;

        private float _rateLimit;

        private void Start()
        {
            _targetPos = transform.position;
            _targetFacingRight = true;
            _visualFacingScaleX = Mathf.Max(0.01f, Mathf.Abs(transform.localScale.x));
            Plugin.Log.LogInfo("[RL2MP] [INTERP] Start() on " + gameObject.name + " initial pos=("
                + transform.position.x.ToString("F2") + "," + transform.position.y.ToString("F2") + ")");
        }

        public void SetTarget(Vector3 pos, bool facingRight)
        {
            SetTarget(pos, facingRight, Vector2.zero, false);
        }

        /// <summary>v3.0.24：带速度的位置包。速度用于两包之间外推，消除 20Hz 台阶感。</summary>
        public void SetTarget(Vector3 pos, bool facingRight, Vector2 vel)
        {
            SetTarget(pos, facingRight, vel, false);
        }

        /// <summary>freeze=true 时锁定在原地（倒地），不再插值移动。</summary>
        public void SetTarget(Vector3 pos, bool facingRight, bool freeze)
        {
            SetTarget(pos, facingRight, Vector2.zero, freeze);
        }

        public void SetTarget(Vector3 pos, bool facingRight, Vector2 vel, bool freeze)
        {
            _targetPos = pos;
            _targetFacingRight = facingRight;

            if (!freeze)
            {
                _targetVel = vel;
                _targetAtUnscaled = Time.unscaledTime;
            }
            else
            {
                // 倒地：位置交给尸体下坠逻辑，速度外推必须停掉，否则尸体会继续往前飘。
                _targetVel = Vector2.zero;
                _targetAtUnscaled = -999f;
            }

            if (freeze && !_frozen)
            {
                // 刚倒下：开始自由落体（尸体不该悬在空中）
                _corpseFalling = true;
                _corpseVy = 0f;
                _corpseFallTime = 0f;
            }
            else if (!freeze && _frozen)
            {
                // 复活：停止下坠
                _corpseFalling = false;
                _corpseVy = 0f;
            }

            _frozen = freeze;
        }

        /// <summary>尸体自由落体：脚下有非触发器碰撞体就停住；最多落 3 秒防掉出世界。</summary>
        private void TickCorpseFall(float dt)
        {
            if (!_corpseFalling) return;
            if (dt <= 0f) return;
            if (dt > 0.05f) dt = 0.05f;

            _corpseFallTime += dt;
            if (_corpseFallTime > 3f) { _corpseFalling = false; _corpseVy = 0f; return; }

            _corpseVy -= 45f * dt;
            if (_corpseVy < -30f) _corpseVy = -30f;

            Vector3 next = transform.position + new Vector3(0f, _corpseVy * dt, 0f);
            try
            {
                var hits = Physics2D.RaycastAll(new Vector2(next.x, next.y + 0.4f), Vector2.down, 1.0f);
                if (hits != null)
                {
                    for (int i = 0; i < hits.Length; i++)
                    {
                        var col = hits[i].collider;
                        if (col == null || col.isTrigger) continue;                          // 触发器不算地板
                        if (col.GetComponentInParent<BaseCharacterController>() != null) continue; // 别的角色
                        _corpseFalling = false;
                        _corpseVy = 0f;
                        return;
                    }
                }
            }
            catch { }

            transform.position = next;
        }

        /// <summary>记录网络送来的动画状态（hash=0 表示无数据，镜像组件会降级为待机/本地镜像）。</summary>
        public void SetAnimTarget(int hash, float time)
        {
            TargetLayerHashes = new int[] { hash };
            TargetLayerTimes = new float[] { time };
            AnimPacketTime = Time.unscaledTime;
        }

        /// <summary>v1.6.3：全层 + 参数快照（10Hz 网络包解析结果）。</summary>
        public void SetAnimTarget(int[] layerHashes, float[] layerTimes, float[] floatParams, int[] intParams, bool[] boolParams)
        {
            TargetLayerHashes = layerHashes ?? new int[0];
            TargetLayerTimes = layerTimes ?? new float[0];
            TargetFloatParams = floatParams;
            TargetIntParams = intParams;
            TargetBoolParams = boolParams;
            AnimPacketTime = Time.unscaledTime;
        }

        /// <summary>
        /// 兜底白块升级成整树克隆后，把"翻面用的基准缩放"重置成 1。
        /// 不重置的话，x 会继续沿用白块时期的 0.77，而 y 是 1 —— 分身会被压扁。
        /// </summary>
        public void SetVisualScale(float s)
        {
            _visualFacingScaleX = Mathf.Max(0.01f, Mathf.Abs(s));
            var scale = transform.localScale;
            scale.x = _targetFacingRight ? _visualFacingScaleX : -_visualFacingScaleX;
            scale.y = _visualFacingScaleX;
            transform.localScale = scale;
        }

        private void Update()
        {
            // v3.0.24：这里全部改用 unscaled 时间。
            // 受缩放的 Time.deltaTime 在 Time.timeScale=0 时是 0 —— 而 timeScale=0 不只会
            // 出现在暂停菜单（PauseCompat 会抢回 1），Boss 死亡定格（Cutscene 类型，PauseCompat
            // 明确不拦）和场景转场都会真的把它压到 0。那时候分身会原地冻住，
            // 观战镜头又是跟着分身走的 → 整个观战画面卡死不动（用户反馈的"观战视角特别卡"）。
            float dt = Time.unscaledDeltaTime;

            if (_frozen)
            {
                TickCorpseFall(dt);
                return;
            }

            // 目标点 = 收到的位置 + 速度外推（消除 20Hz 台阶感）。
            Vector3 goal = _targetPos;
            if (_targetAtUnscaled > 0f)
            {
                float age = Mathf.Clamp(Time.unscaledTime - _targetAtUnscaled, 0f, MaxExtrapolate);
                if (age > 0f)
                {
                    Vector3 lead = new Vector3(_targetVel.x, _targetVel.y, 0f) * age;
                    // 被击飞/弹射时速度能到几十单位每秒，外推会冲出好几米 —— 限幅，
                    // 否则分身会先飞出去再被下一包拽回来，抖得很明显。
                    if (lead.sqrMagnitude > MaxLeadSqr) lead = lead.normalized * MaxLead;
                    goal += lead;
                }
            }

            // Clamp01：dt*LerpSpeed 在低帧率下会 >1，Vector3.Lerp 会过冲，表现为来回抖。
            float t = Mathf.Clamp01(dt * LerpSpeed);
            transform.position = Vector3.Lerp(transform.position, goal, t);

            // 朝向直接翻面。之前用 Lerp 从 +1 过渡到 -1，途中会经过 0（宽度为零），
            // 看起来就是"原地转一下然后消失"——这就是你看到的现象之一。
            var scale = transform.localScale;
            float targetX = _targetFacingRight ? _visualFacingScaleX : -_visualFacingScaleX;
            if (Mathf.Abs(scale.x - targetX) > 0.001f)
            {
                scale.x = targetX;
                transform.localScale = scale;
            }

            _rateLimit += dt;
            if (_rateLimit > 3f)
            {
                _rateLimit = 0f;
                float dist = Vector3.Distance(transform.position, _targetPos);
                Plugin.Log.LogInfo("[RL2MP] [INTERP] " + gameObject.name + " -> current=("
                    + transform.position.x.ToString("F2") + "," + transform.position.y.ToString("F2")
                    + ") target=(" + _targetPos.x.ToString("F2") + "," + _targetPos.y.ToString("F2")
                    + ") dist=" + dist.ToString("F3") + " facingRight=" + _targetFacingRight);
            }
        }
    }

    /// <summary>
    /// 分身动画/特效镜像（v0.6.1 全量重写）。
    ///
    /// 之前只镜像"第一个 Animator 的第 0 层"，导致：
    /// - 攻击/法术动画不显示（它们在武器/FX 的独立 Animator 上，或在更高的层）
    /// - 冲刺残影卡顿（一次性短动画被漂移重对时逻辑反复重启）
    ///
    /// 现在的做法：
    /// - Animator：本地玩家树里**所有**有 controller 的 Animator，按相对路径配对到克隆体
    ///   （克隆是整树拷贝，路径一一对应），每个都镜像**所有层**的状态 + 参数（float/int/bool）。
    /// - ParticleSystem：同样按路径配对，同步播放/发射状态 —— 冲刺拖尾、法术特效、攻击火花全跟随。
    ///
    /// 同步策略：
    /// - 状态切换：立即 Play 新状态并对齐时间
    /// - 循环状态（跑/待机）：自由播放，归一化时间漂移超阈值才重对时
    /// - 一次性状态（冲刺/攻击）：只在检测到"本地重新开始"（时间回卷）时重播，
    ///   绝不中途重启 —— 这是残影卡顿的根因
    /// </summary>
    public class RemoteAnimatorMirror : MonoBehaviour
    {
        private class AnimPair { public Animator Local; public Animator Clone; }
        private class FxPair { public ParticleSystem Local; public ParticleSystem Clone; }

        private readonly List<AnimPair> _animPairs = new List<AnimPair>();
        private readonly List<FxPair> _fxPairs = new List<FxPair>();
        private RemotePlayerInterpolator _interp;

        /// <summary>
        /// v3.0.5：给特效主动桥用的主分身 Animator（_animPairs[0] 的克隆侧）。
        /// 原生 EffectTrigger.InvokeTrigger 需要一个 Animator 作为特效来源，
        /// 复用镜像已配对的主 Animator，保证特效跟随正确的身体。
        /// </summary>
        internal Animator PrimaryCloneAnimator
        {
            get { return _animPairs.Count > 0 ? _animPairs[0].Clone : null; }
        }
        private const float ResyncThreshold = 0.18f;       // 循环状态允许的时间漂移（本地全量镜像用）
        private const float OneShotRestartGap = 0.05f;     // 一次性状态判定"本地重新开始"的回卷幅度
        private const float NetLoopDrift = 0.3f;           // 网络循环动画允许的绝对时间漂移
        private const float NetLoopResyncGap = 0.5f;       // 网络循环动画两次重对时的最小间隔
        private const float NetOneShotResyncGap = 0.3f;    // 网络一次性动画两次重播的最小间隔
        private readonly float[] _lastResync = new float[16]; // 每层上次对网络动画执行 Play 的时刻
        // v1.8.3：每层上次收到的网络 normalizedTime，用来检测"对端重新触发同一次性动画"(t 回卷)。
        private readonly float[] _lastRemoteNorm = new float[16];
        private readonly bool[] _hasRemoteNorm = new bool[16];
        // v2.2.0：一次性动画重播看门狗。真正的信号不是"补播次数多"，而是
        // "补播了但网络送来的 t 几乎没前进"——说明对端把某个残留状态一直发过来
        // （攻击/施法层权重已归零但状态还没切走），继续补播只会让分身无限重复攻击。
        private readonly int[] _stuckHash = new int[16];
        private readonly int[] _noProgressCount = new int[16];
        private readonly float[] _lastReplayT = new float[16];
        private readonly bool[] _hasReplayT = new bool[16];
        private const int ReplayWinLimit = 5;
        // v2.5.4：对端层权重归零(hash=0)时，克隆体那层仍停在法术最后一帧且继续推进
        // normalizedTime（日志里 curN=95 / 245 就是这么来的）。这里显式把该层静音，
        // 下次真有状态到来再恢复权重，避免残留特效 + 每帧重试 Play 拖帧。
        private readonly bool[] _layerMuted = new bool[16];
        // 参数快照只需在新动画包到达时套用，不必每帧重复 109 次 Animator.Set*。
        private double _lastAppliedAnimPacketTime = -1d;
        // v2.7.1：法术只有一帧的根因——发送端一层只发 1 个包的 hash 就变成 0，
        // 我们收到第一个 hash=0 就立刻静音，施法动画被砍成一帧。
        // 改成连续 N 个包都为 0 才静音（10Hz ≈ 0.5s），让一次性法术播完。
        private readonly int[] _zeroHashPackets = new int[16];
        // v2.9.1：之前以"12 个包"为阈值，但计数写在每帧执行的 ApplyNetworkAnim() 里，
        // 实际是 12 帧≈0.2s（60fps），远小于 Lance 蓄力 0.55s —— 这正是"法术动态
        // 动画没结合"的根因。改成按“连续无状态的真实时长”判断，与帧率/包率都无关。
        private const float ZeroStateMuteSeconds = 1.2f;
        // 每层第一次收到 hash=0 的时刻；-1 表示当前不是零状态。
        private readonly float[] _zeroStateSince = new float[16];
        // v3.0.2：施法事件接管——收到 AbilityFx 事件后，该层在窗口期内由事件重放驱动，
        // ApplyNetworkAnim 跳过该层的逐层硬追，避免和事件 Play 打架（两套机制并存可回退）。
        private readonly float[] _fxTakeoverUntil = new float[16];
        private float _lastMuteLog;
        private float _disableListTimer;                   // 克隆体效果禁用列表重申计时
        private float _animDiagTimer;                      // 动画诊断日志节流
        private int _replayCount;                          // 诊断窗口内 Play(重播)次数

        /// <summary>
        /// 按相对路径把本地玩家树上的 Animator/ParticleSystem 配对到克隆体上。
        /// localRoot = 本地玩家根对象，cloneRoot = 克隆出来的"Visual"根。
        /// </summary>
        public void Bind(GameObject localRoot, GameObject cloneRoot)
        {
            if (localRoot == null || cloneRoot == null) return;

            // v2.5.4：分身重建后所有层状态/静音标记必须清空，否则新克隆体会继承
            // 上一次的"层已静音"判断，导致法术再次不显示。
            Array.Clear(_layerMuted, 0, _layerMuted.Length);
            Array.Clear(_stuckHash, 0, _stuckHash.Length);
            Array.Clear(_lastRemoteNorm, 0, _lastRemoteNorm.Length);
            Array.Clear(_hasRemoteNorm, 0, _hasRemoteNorm.Length);
            Array.Clear(_zeroHashPackets, 0, _zeroHashPackets.Length);
            Array.Clear(_fxTakeoverUntil, 0, _fxTakeoverUntil.Length);
            for (int i = 0; i < _zeroStateSince.Length; i++) _zeroStateSince[i] = -1f;
            _lastAppliedAnimPacketTime = -1d;

            try
            {
                // Animator 配对
                var cloneAnimMap = new Dictionary<string, Animator>();
                foreach (var ca in cloneRoot.GetComponentsInChildren<Animator>(true))
                {
                    if (ca != null) cloneAnimMap[PathOf(cloneRoot.transform, ca.transform)] = ca;
                }
                foreach (var la in localRoot.GetComponentsInChildren<Animator>(true))
                {
                    if (la == null || la.runtimeAnimatorController == null) continue;
                    Animator ca;
                    if (!cloneAnimMap.TryGetValue(PathOf(localRoot.transform, la.transform), out ca) || ca == null) continue;

                    ca.cullingMode = AnimatorCullingMode.AlwaysAnimate; // 离屏不冻结
                    _animPairs.Add(new AnimPair { Local = la, Clone = ca });
                }

                // 粒子特效配对
                var cloneFxMap = new Dictionary<string, ParticleSystem>();
                foreach (var cp in cloneRoot.GetComponentsInChildren<ParticleSystem>(true))
                {
                    if (cp != null) cloneFxMap[PathOf(cloneRoot.transform, cp.transform)] = cp;
                }
                foreach (var lp in localRoot.GetComponentsInChildren<ParticleSystem>(true))
                {
                    if (lp == null) continue;
                    ParticleSystem cp;
                    if (!cloneFxMap.TryGetValue(PathOf(localRoot.transform, lp.transform), out cp) || cp == null) continue;
                    _fxPairs.Add(new FxPair { Local = lp, Clone = cp });
                }

                Plugin.Log.LogInfo("[RL2MP] [ANIM] 镜像绑定完成: " + _animPairs.Count + " 个 Animator, "
                    + _fxPairs.Count + " 个粒子系统。");
                for (int i = 0; i < _animPairs.Count; i++)
                {
                    Plugin.Log.LogInfo("[RL2MP] [ANIM]   pair[" + i + "] " + _animPairs[i].Local.transform.name
                        + " layers=" + _animPairs[i].Local.layerCount
                        + " params=" + _animPairs[i].Local.parameterCount);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ANIM] Bind 失败: " + ex.Message);
            }
        }

        private static string PathOf(Transform root, Transform t)
        {
            var sb = new System.Text.StringBuilder();
            while (t != null && t != root)
            {
                if (sb.Length > 0) sb.Insert(0, '/');
                sb.Insert(0, t.name);
                t = t.parent;
            }
            return sb.ToString();
        }

        private void Update()
        {
            if (_animPairs.Count == 0 && _fxPairs.Count == 0) return;

            // 惰性接线：Bind 时 interpolator 可能还没加上（创建顺序）
            if (_interp == null) _interp = GetComponent<RemotePlayerInterpolator>();

            // 克隆体的 Animator 里内嵌了游戏自己的 EffectTriggerAnimBehaviour
            // （攻击伤害就是它触发的）—— 不禁用的话分身会真的造成二次伤害。
            // 用游戏自带的 per-animator 开关把它关掉；每秒重申一次防列表被清。
            _disableListTimer += Time.unscaledDeltaTime;
            if (_disableListTimer >= 1f)
            {
                _disableListTimer = 0f;
                EnsureCloneEffectsDisabled();
            }

            // v2.8.6：对端倒地后它的动画包会变空，TargetLayerHashes.Length==0 会走
            // 下面的 MirrorAnimator 兜底 —— 那是用"本机存活玩家"的 Animator 逐帧
            // 往尸体上套，正是"尸体跟着活人一起动"的根因。这里直接停掉镜像，
            // 尸体保持死亡瞬间的姿态；活人分身 Downed=false，完全不受影响。
            if (_interp != null && _interp.Downed) return;

            try
            {
                if (_interp != null && _interp.TargetLayerHashes != null && _interp.TargetLayerHashes.Length > 0)
                {
                    // 真实联机：网络送来所有层的状态 + 参数（v1.6.3）
                    ApplyNetworkAnim();
                }
                else
                {
                    // 调试模拟（或没收到网络动画数据）：全量实时镜像
                    for (int i = 0; i < _animPairs.Count; i++)
                    {
                        MirrorAnimator(_animPairs[i]);
                    }
                }

                MirrorFx();
            }
            catch (Exception ex)
            {
                // 场景切换瞬间对象可能失效，静默降级
                Plugin.Log.LogWarning("[RL2MP] [ANIM] mirror exception: " + ex.Message);
                enabled = false;
            }
        }

        /// <summary>
        /// 把克隆体上的所有 Animator 加进游戏的效果禁用列表。
        /// EffectManager 还没初始化时静默返回，下一秒重试。
        /// </summary>
        private void EnsureCloneEffectsDisabled()
        {
            try
            {
                if (!EffectManager.IsInitialized) return;
                for (int i = 0; i < _animPairs.Count; i++)
                {
                    var ca = _animPairs[i].Clone;
                    if (ca != null) EffectManager.AddAnimatorToDisableList(ca);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ANIM] 克隆体效果禁用失败: " + ex.Message);
            }
        }

        /// <summary>镜像单个 Animator 的参数 + 所有层状态。</summary>
        private void MirrorAnimator(AnimPair p)
        {
            var loc = p.Local;
            var cl = p.Clone;
            if (loc == null || cl == null) return;
            if (!loc.enabled || !loc.gameObject.activeInHierarchy) return;

            // 参数同步：驱动 blend tree（移动速度混合）和条件过渡
            var prms = loc.parameters;
            for (int i = 0; i < prms.Length; i++)
            {
                var prm = prms[i];
                switch (prm.type)
                {
                    case AnimatorControllerParameterType.Float: cl.SetFloat(prm.name, loc.GetFloat(prm.name)); break;
                    case AnimatorControllerParameterType.Int: cl.SetInteger(prm.name, loc.GetInteger(prm.name)); break;
                    case AnimatorControllerParameterType.Bool: cl.SetBool(prm.name, loc.GetBool(prm.name)); break;
                    // Trigger 读不到（Unity 不暴露），靠状态 scrub 兜底
                }
            }

            int layers = Mathf.Min(loc.layerCount, cl.layerCount);
            for (int l = 0; l < layers; l++)
            {
                var li = loc.GetCurrentAnimatorStateInfo(l);
                var ci = cl.GetCurrentAnimatorStateInfo(l);
                int hash = li.shortNameHash;
                float tgtN = li.normalizedTime;
                float curN = ci.normalizedTime;

                if (hash != ci.shortNameHash)
                {
                    // 状态切换：立即播新状态并对齐时间
                    cl.Play(hash, l, tgtN);
                    continue;
                }

                float tgtF = tgtN - Mathf.Floor(tgtN);
                float curF = curN - Mathf.Floor(curN);

                if (li.loop)
                {
                    // 循环状态：自由播放，漂移过大才重对时（每帧 Play 会打断过渡）
                    float drift = Mathf.Abs(tgtF - curF);
                    if (drift > ResyncThreshold && drift < 1f - ResyncThreshold)
                    {
                        cl.Play(hash, l, tgtN);
                    }
                }
                else
                {
                    // 一次性状态（冲刺/攻击）：绝不中途重启。两种情况需要重对时：
                    // a) 克隆体已经播完（normalizedTime 停在 1.0，小数部分归 0，
                    //    本地连击重开的时间回卷检测永远不触发 → 卡在最后一帧）
                    // b) 本地在克隆体没播完时重新开始了（连击快速连打）
                    bool cloneFinished = curN >= 0.99f;
                    bool localStillPlaying = tgtN < 0.99f;
                    bool localRestarted = (tgtF < curF) && (curF - tgtF) > OneShotRestartGap;
                    if ((cloneFinished && localStillPlaying) || localRestarted)
                    {
                        cl.Play(hash, l, tgtN);
                    }
                }
            }
        }

        /// <summary>层从"静音"恢复：对端重新起招时先把克隆体层权重还回去。</summary>
        private void EnsureLayerAudible(Animator cl, int layer)
        {
            if (layer <= 0 || !_layerMuted[layer]) return; // 第 0 层权重恒为 1，不能静音
            try { cl.SetLayerWeight(layer, 1f); } catch { }
            _layerMuted[layer] = false;
        }

        /// <summary>
        /// v3.0.2：施法事件重放（HKMP 式）。收到对端 AbilityFx 事件后，让分身
        /// 对应层从 0 播一次施法动画，并在窗口期内接管该层——期间 ApplyNetworkAnim
        /// 不再用逐层 hash 硬追该层，避免"一次性动画被反复 Play 拉回"的抽搐。
        /// 与逐层镜像并存：窗口过后自动交还，可单独回退（不收到事件则本方法从不调用）。
        /// </summary>
        internal void ReplayAbilityLayer(int layer, int hash)
        {
            if (_animPairs.Count == 0) return;
            var cl = _animPairs[0].Clone;
            if (cl == null || layer < 0 || layer >= cl.layerCount || hash == 0) return;
            try
            {
                // 施法层之前可能被静音（权重 0）：强制恢复，否则 Play 了也看不见。
                if (layer > 0)
                {
                    cl.SetLayerWeight(layer, 1f);
                    _layerMuted[layer] = false;
                }
                cl.Play(hash, layer, 0f);
                // 接管窗口：该层跳过硬追，让事件重放的动画自然播完。
                _fxTakeoverUntil[layer] = Time.unscaledTime + 1.5f;
                // 清掉该层旧的一次性动画看门狗状态，下次真正切换才不会被误判残留。
                _stuckHash[layer] = 0;
                _noProgressCount[layer] = 0;
                _hasReplayT[layer] = false;
                _zeroStateSince[layer] = -1f;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ANIM] 事件重放失败 layer=" + layer + ": " + ex.Message);
            }
        }

        /// <summary>
        /// 真实联机：网络送来所有层的状态 + 参数。
        ///
        /// 网络包是 10Hz 的，数据到手已落后一个网络延迟，直接拿"过期目标"比较会
        /// 频繁重对时（v1.6.2 修过的抽搐）。这里按"包到达时刻 + 已流逝时间"预测
        /// 当前应处的动画时间；循环态比绝对时间（不回卷），一次性态保留重启检测，
        /// 并按层加最小重对时间隔。
        /// </summary>
        private void ApplyNetworkAnim()
        {
            if (_animPairs.Count == 0 || _animPairs[0].Clone == null) return;
            var cl = _animPairs[0].Clone;

            // v2.5.4：参数只在"新动画包到达"时套用（10Hz）。旧版每帧重复 109 次
            // Animator.Set*，主机上 6 层以上时是实打实的主线程开销。
            // v2.9.1：newPacket 同时用作零状态计时的唯一时基 —— 一个网络包只记一次，
            // 不会因为渲染 60/144fps 就把 0.2s 误当成 1.2s。
            double packetTime = _interp.AnimPacketTime;
            bool newPacket = packetTime != _lastAppliedAnimPacketTime;
            if (newPacket)
            {
                _lastAppliedAnimPacketTime = packetTime;
                ApplyParams(cl);
            }

            var hashes = _interp.TargetLayerHashes;
            var times = _interp.TargetLayerTimes;
            if (hashes == null || hashes.Length == 0) return;

            // 预测此刻的归一化时间（最多预测 0.3s，丢包时别让预测飞出去）
            float rawAge = Time.unscaledTime - _interp.AnimPacketTime;
            // v2.2.0：超过 0.6s 没收到动画包 = 数据断流（对端倒地/正在重连/切场景）。
            // 这时候绝不能拿最后一份"正在攻击"的目标继续补播，否则分身会一直打。
            bool animStale = _interp.AnimPacketTime <= 0f || rawAge > 0.6f;
            float age = rawAge;
            if (age < 0f) age = 0f;
            if (age > 0.3f) age = 0.3f;

            float now = Time.unscaledTime;
            int layers = Mathf.Min(cl.layerCount, hashes.Length);
            layers = Mathf.Min(layers, _lastResync.Length);

            for (int l = 0; l < layers; l++)
            {
                // v3.0.2：该层正由 AbilityFx 事件重放驱动——跳过逐层硬追，
                // 否则事件刚 Play(0) 就被网络的归一化时间硬拉回去，法术又成抽搐。
                if (now < _fxTakeoverUntil[l]) continue;

                int hash = hashes[l];
                float t = times[l];
                if (hash == 0)
                {
                    // 该层已无数据（对端层权重归零）：顺手清掉看门狗锁
                    if (_stuckHash[l] != 0) { _stuckHash[l] = 0; _noProgressCount[l] = 0; _hasReplayT[l] = false; }
                    // v2.9.1：按真实时间计时，不再按帧/按包计数。
                    // 施法层常常只有很少几个包带 hash，随后权重归零但动画仍在播；
                    // 必须给足 1.2s 让 Lance 这类 0.55s 蓄力 + 施法完整结束。
                    if (newPacket)
                    {
                        if (_zeroStateSince[l] < 0f) _zeroStateSince[l] = now;
                        _zeroHashPackets[l]++; // 只用于日志，不再作为阈值
                    }
                    float zeroFor = _zeroStateSince[l] < 0f ? 0f : now - _zeroStateSince[l];
                    if (zeroFor >= ZeroStateMuteSeconds && l > 0 && !_layerMuted[l])
                    {
                        try
                        {
                            cl.SetLayerWeight(l, 0f);
                            // v2.8.1：只把权重设 0，状态机仍停在上一次施法状态并继续推进，
                            // normalizedTime 会涨到几十上百；下次同 hash 到来时
                            // hash 相同不再走"切换"分支，只能靠重播硬拉，画面就是抽搐。
                            // 顺便把该层复位到空状态，下次必然走正常切换。
                            cl.Play(0, l, 0f);
                        }
                        catch { }
                        _layerMuted[l] = true;
                        if (Time.unscaledTime - _lastMuteLog > 5f)
                        {
                            _lastMuteLog = Time.unscaledTime;
                            Plugin.Log.LogInfo("[RL2MP] [ANIM] 层 " + l + " 连续 "
                                + zeroFor.ToString("F2") + "s 无状态 -> 静音（法术结束）");
                        }
                    }
                    continue; // 该层无数据（无状态机/同步层）
                }
                _zeroHashPackets[l] = 0;  // 只用于日志
                _zeroStateSince[l] = -1f; // 对端又给状态了，取消静音倒计时

                if (_stuckHash[l] != 0 && hash != _stuckHash[l])
                {
                    // 对端换到新状态了，解锁
                    _stuckHash[l] = 0;
                    _noProgressCount[l] = 0;
                    _hasReplayT[l] = false;
                    _lastResync[l] = 0f;
                }

                AnimatorStateInfo ci;
                try { ci = cl.GetCurrentAnimatorStateInfo(l); }
                catch { continue; }

                bool loop = ci.loop;
                // v1.8.3：一次性动画(法术/攻击)也加 age 预测。旧版一次性不加，predN=t
                // 天然落后分身本地 60fps 进度 → 每帧都像"对端重新开始"→ 反复 Play 拉回，
                // 表现为法术一直卡顿/抽搐。加 age 后预测时间追上真实进度，消除误判。
                float predN = t + age;
                float predF = predN - Mathf.Floor(predN);

                if (hash != ci.shortNameHash)
                {
                    // 状态切换（含攻击/冲刺进入更高层）：立即播新状态并对齐时间
                    EnsureLayerAudible(cl, l);
                    cl.Play(hash, l, predN);
                    _lastResync[l] = now;
                    _lastRemoteNorm[l] = t;
                    _hasRemoteNorm[l] = true;
                    continue;
                }

                float curN = ci.normalizedTime;
                float curF = curN - Mathf.Floor(curN);

                if (loop)
                {
                    // 循环状态（跑/待机）：比绝对时间。绝对时间单调递增，没有回卷误判。
                    if (Mathf.Abs(predN - curN) > NetLoopDrift && now - _lastResync[l] >= NetLoopResyncGap)
                    {
                        EnsureLayerAudible(cl, l);
                        cl.Play(hash, l, predN);
                        _lastResync[l] = now;
                    }
                    _lastRemoteNorm[l] = t;
                    _hasRemoteNorm[l] = true;
                    continue;
                }

                // v1.8.3 一次性状态（法术/攻击/冲刺/翻滚）重对时判定：
                // 正确的"对端重新施法"信号是【网络送来的 t 回卷】(t 明显小于上一包)，
                // 而不是拿本地进度比(predF<curF)——后者因网络滞后会持续误判。
                bool remoteRestarted = _hasRemoteNorm[l] && (t < _lastRemoteNorm[l] - OneShotRestartGap);
                _lastRemoteNorm[l] = t;
                _hasRemoteNorm[l] = true;

                // a) 分身已播完(curN>=1)但预测目标还在播(predN<1)：补播对齐一次
                // b) 对端确实重新触发了同一动画(t 回卷)：从头对齐
                bool cloneFinished = curN >= 0.99f;
                bool targetStillPlaying = predN < 0.99f;
                bool needReplay = (cloneFinished && targetStillPlaying) || remoteRestarted;
                if (needReplay && !animStale && now - _lastResync[l] >= NetOneShotResyncGap)
                {
                    // 看门狗：补播了 t 却没前进 → 目标状态在原地打转，连续 5 次就锁住该层
                    bool noProgress = _hasReplayT[l] && Mathf.Abs(t - _lastReplayT[l]) < 0.05f;
                    _noProgressCount[l] = noProgress ? _noProgressCount[l] + 1 : 0;
                    _lastReplayT[l] = t;
                    _hasReplayT[l] = true;

                    if (_noProgressCount[l] >= ReplayWinLimit)
                    {
                        _stuckHash[l] = hash;
                        _noProgressCount[l] = 0;
                        Plugin.Log.LogWarning("[RL2MP] [ANIM] 分身第 " + l + " 层收到的状态停在 t="
                            + t.ToString("F3") + " 不动（hash=" + hash + "），判定为残留状态，"
                            + "停止补播直到状态变化。");
                        _lastRemoteNorm[l] = t;
                        continue;
                    }

                    EnsureLayerAudible(cl, l);
                    cl.Play(hash, l, predN);
                    _lastResync[l] = now;
                    _replayCount++;
                    // 节流诊断：法术/攻击层(l>=0)频繁重播时会打出来，便于定位卡顿
                    if (now - _animDiagTimer > 2f)
                    {
                        _animDiagTimer = now;
                        Plugin.Log.LogInfo("[RL2MP] [ANIM] replay layer=" + l + " hash=" + hash
                            + " loop=" + loop + " t=" + t.ToString("F3") + " predN=" + predN.ToString("F3")
                            + " curN=" + curN.ToString("F3") + " age=" + age.ToString("F3")
                            + " remoteRestarted=" + remoteRestarted + " replays/2s~" + _replayCount);
                        _replayCount = 0;
                    }
                }
            }
        }

        /// <summary>按类型顺序套用参数快照（本地/远端同一 Player_Animator，顺序天然对齐）。</summary>
        private void ApplyParams(Animator cl)
        {
            var fp = _interp.TargetFloatParams;
            var ip = _interp.TargetIntParams;
            var bp = _interp.TargetBoolParams;
            if (fp == null && ip == null && bp == null) return;

            try
            {
                var prms = cl.parameters;
                int a = 0, b = 0, c = 0;
                for (int i = 0; i < prms.Length; i++)
                {
                    switch (prms[i].type)
                    {
                        case AnimatorControllerParameterType.Float:
                            if (fp != null && a < fp.Length) cl.SetFloat(prms[i].nameHash, fp[a]);
                            a++;
                            break;
                        case AnimatorControllerParameterType.Int:
                            if (ip != null && b < ip.Length) cl.SetInteger(prms[i].nameHash, ip[b]);
                            b++;
                            break;
                        case AnimatorControllerParameterType.Bool:
                            if (bp != null && c < bp.Length) cl.SetBool(prms[i].nameHash, bp[c]);
                            c++;
                            break;
                    }
                }
            }
            catch { /* 参数表对不上时静默跳过 */ }
        }

        /// <summary>粒子特效播放状态同步：冲刺拖尾/法术光效/攻击火花。</summary>
        private void MirrorFx()
        {
            for (int i = 0; i < _fxPairs.Count; i++)
            {
                var lp = _fxPairs[i].Local;
                var cp = _fxPairs[i].Clone;
                if (cp == null) continue;

                try
                {
                    // v2.2.0：只跟随"停止"，绝不替对端"开场"。
                    //
                    // 旧实现看到本机粒子在发射就 cp.Play() —— 等于把**我自己**的法术特效
                    // 贴到对端分身上。持续性法术（火焰/护盾这类常驻发射器）结束后本机特效
                    // 被回收成 null，旧代码直接 continue，分身那份就一直喷下去：法术早停了，
                    // 对方画面上还在放。攻击动作也是同理，看着像"不停歇地攻击"。
                    //
                    // 分身自己的粒子由它的动画驱动（克隆体 Animator 已被加进
                    // EffectManager 的禁用列表，防止二次伤害），所以这里不需要也不该接管开始。
                    if (lp == null || !lp.isEmitting || !lp.gameObject.activeInHierarchy)
                    {
                        if (cp.isEmitting || cp.particleCount > 0)
                            cp.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                    }
                }
                catch (Exception)
                {
                    // 单个粒子对失效不影响其他
                }
            }
        }
    }
}
