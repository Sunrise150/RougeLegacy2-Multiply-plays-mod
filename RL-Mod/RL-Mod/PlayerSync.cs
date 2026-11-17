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

                // 动画状态（第 0 层）。读不到就保持 0，远端自动降级为待机。
                var anim = EnsureLocalAnimator();
                if (anim != null)
                {
                    var info = anim.GetCurrentAnimatorStateInfo(0);
                    state.AnimStateHash = info.shortNameHash;
                    state.AnimNormalizedTime = info.normalizedTime;
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
            return createdNew;
        }

        /// <summary>位置/朝向包（20Hz，Sequenced）：只更新插值目标，不碰动画和倒地状态。</summary>
        public void HandleRemoteTransform(int id, float x, float y, float vx, float vy, bool facingRight, bool grounded)
        {
            GameObject go;
            RemotePlayerInterpolator interpolator;
            bool createdNew = EnsureRemoteObject(id, out go, out interpolator);

            var newTarget = new Vector3(x, y, go.transform.position.z);
            if (createdNew)
            {
                Plugin.Log.LogInfo("[RL2MP] [REMOTE] ID=" + id + " target set to ("
                    + x.ToString("F2") + "," + y.ToString("F2") + ") facingRight=" + facingRight
                    + " (fresh target after creation).");
            }
            interpolator.SetTarget(newTarget, facingRight);
        }

        /// <summary>动画包（10Hz，Sequenced）：分身不存在时静默丢弃，等位置包把它建出来。</summary>
        public void HandleRemoteAnim(int id, int animHash, float animTime)
        {
            GameObject go;
            RemotePlayerInterpolator interpolator;
            if (!RemotePlayers.TryGetValue(id, out go) || go == null) return;
            interpolator = go.GetComponent<RemotePlayerInterpolator>();
            if (interpolator == null) return;
            interpolator.SetAnimTarget(animHash, animTime);
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
            if (!RemotePlayers.TryGetValue(id, out go) || go == null) return;

            bool was = RemoteDowned.Contains(id);
            if (downed == was) return;

            if (downed)
            {
                RemoteDowned.Add(id);
                var interp = go.GetComponent<RemotePlayerInterpolator>();
                if (interp != null) interp.SetTarget(go.transform.position, true, true);
            }
            else
            {
                RemoteDowned.Remove(id);
            }

            // 倒地变灰半透明；克隆模式下没有根级 SpriteRenderer，
            // 且 SkinnedMeshRenderer 材质与本地玩家共享，改色会连累原体——先跳过调色，
            // 倒地视觉靠冻结位置表达（SetTarget 不再更新）。
            var sr = go.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.color = downed
                    ? new Color(0.55f, 0.55f, 0.55f, 0.35f)
                    : new Color(1f, 1f, 1f, 0.95f);
            }

            Plugin.Log.LogInfo("[RL2MP] [REMOTE] id=" + id + (downed ? " is DOWN" : " is back UP")
                + " (downedCount=" + RemoteDowned.Count + "/" + RemotePlayers.Count + ")");
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

        // 网络送来的动画状态（由 RemoteAnimatorMirror 消费）
        internal int TargetAnimHash;
        internal float TargetAnimTime;

        private const float LerpSpeed = 18f;

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
            _targetPos = pos;
            _targetFacingRight = facingRight;
            _frozen = false;
        }

        /// <summary>freeze=true 时锁定在原地（倒地），不再插值移动。</summary>
        public void SetTarget(Vector3 pos, bool facingRight, bool freeze)
        {
            _targetPos = pos;
            _targetFacingRight = facingRight;
            _frozen = freeze;
        }

        /// <summary>记录网络送来的动画状态（hash=0 表示无数据，镜像组件会降级为待机/本地镜像）。</summary>
        public void SetAnimTarget(int hash, float time)
        {
            TargetAnimHash = hash;
            TargetAnimTime = time;
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
            if (_frozen) return;

            transform.position = Vector3.Lerp(transform.position, _targetPos, Time.deltaTime * LerpSpeed);

            // 朝向直接翻面。之前用 Lerp 从 +1 过渡到 -1，途中会经过 0（宽度为零），
            // 看起来就是"原地转一下然后消失"——这就是你看到的现象之一。
            var scale = transform.localScale;
            float targetX = _targetFacingRight ? _visualFacingScaleX : -_visualFacingScaleX;
            if (Mathf.Abs(scale.x - targetX) > 0.001f)
            {
                scale.x = targetX;
                transform.localScale = scale;
            }

            _rateLimit += Time.deltaTime;
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
        private const float ResyncThreshold = 0.18f;       // 循环状态允许的时间漂移
        private const float OneShotRestartGap = 0.05f;     // 一次性状态判定"本地重新开始"的回卷幅度
        private float _disableListTimer;                   // 克隆体效果禁用列表重申计时

        /// <summary>
        /// 按相对路径把本地玩家树上的 Animator/ParticleSystem 配对到克隆体上。
        /// localRoot = 本地玩家根对象，cloneRoot = 克隆出来的"Visual"根。
        /// </summary>
        public void Bind(GameObject localRoot, GameObject cloneRoot)
        {
            if (localRoot == null || cloneRoot == null) return;

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

            try
            {
                if (_interp != null && _interp.TargetAnimHash != 0)
                {
                    // 真实联机：网络只送了主 Animator 第 0 层，至少保证跑/跳跟随
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

        /// <summary>真实联机兜底：网络数据只有主 Animator 第 0 层。</summary>
        private void ApplyNetworkAnim()
        {
            if (_animPairs.Count == 0 || _animPairs[0].Clone == null) return;
            var cl = _animPairs[0].Clone;
            int hash = _interp.TargetAnimHash;
            float t = _interp.TargetAnimTime;

            var ci = cl.GetCurrentAnimatorStateInfo(0);
            if (hash != ci.shortNameHash)
            {
                cl.Play(hash, 0, t);
                return;
            }

            float curN = ci.normalizedTime;
            float curF = curN - Mathf.Floor(curN);
            float tgtF = t - Mathf.Floor(t);

            bool cloneFinished = curN >= 0.99f;
            bool localStillPlaying = t < 0.99f;
            bool localRestarted = (tgtF < curF) && (curF - tgtF) > OneShotRestartGap;
            bool drift = Mathf.Abs(tgtF - curF) > ResyncThreshold && Mathf.Abs(tgtF - curF) < 1f - ResyncThreshold;

            if ((cloneFinished && localStillPlaying) || localRestarted || drift)
            {
                cl.Play(hash, 0, t);
            }
        }

        /// <summary>粒子特效播放状态同步：冲刺拖尾/法术光效/攻击火花。</summary>
        private void MirrorFx()
        {
            for (int i = 0; i < _fxPairs.Count; i++)
            {
                var lp = _fxPairs[i].Local;
                var cp = _fxPairs[i].Clone;
                if (lp == null || cp == null) continue;

                try
                {
                    // 发射开关同步（有些特效靠开关 emission 做节流）
                    var le = lp.emission;
                    var ce = cp.emission;
                    if (le.enabled != ce.enabled) ce.enabled = le.enabled;

                    // 播放状态同步
                    if (lp.isEmitting && !cp.isEmitting)
                    {
                        cp.Play();
                    }
                    else if (!lp.isEmitting && cp.isEmitting)
                    {
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
