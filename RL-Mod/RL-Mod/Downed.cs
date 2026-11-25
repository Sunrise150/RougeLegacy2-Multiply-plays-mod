using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL2Multiplayer
{
    /// <summary>
    /// 倒地 / 观战状态。全部是静态字段，不依赖任何 GameObject 存活：
    /// 即使运行对象被游戏清掉重建，状态也不会丢。
    /// </summary>
    internal static class DownedState
    {
        /// <summary>本地玩家已经倒地（角色被隐藏，镜头在观战）。</summary>
        internal static bool LocalDown;

        /// <summary>全员倒下后放行真实死亡，避免被自己的 prefix 无限拦截。</summary>
        internal static bool AllowRealDeath;

        /// <summary>正在观战的目标玩家 ID，-1 表示还没选。</summary>
        internal static int SpectateTargetId = -1;

        /// <summary>
        /// v3.0.4：权威易手后强制重新解析/激活观战房间。
        /// 换房包驱动的 RemoteRoomCoords 在权威切换瞬间可能还是旧的，
        /// 需要让观战维护下一拍强制重查一次，避免房间坐标锁死在易手前的值。
        /// </summary>
        internal static bool ForceSpectateRoomRefresh;

        /// <summary>
        /// v3.0.22：稳定的 MonoBehaviour 宿主（Plugin 创建的 DownedManager），
        /// 供需要跑协程的模块使用（例如跨区域时在本地补建房间）。
        /// </summary>
        internal static MonoBehaviour HostBehaviour;

        /// <summary>倒地瞬间的位置，用于复活时把人放回原地。</summary>
        internal static Vector3 LastAlivePos;

        /// <summary>本次死亡的坐标（KillCharacter 被拦截的那一帧记录，v1.7.2）。</summary>
        internal static Vector3 DeathPos = Vector3.zero;

        /// <summary>
        /// 本地玩家最后一次"稳站地面"的坐标。v1.7.2 由我们自己每 0.2s 采样，
        /// 不再依赖 Corgi 的 LastStandingPosition —— 后者只在特定图层的落脚面上更新，
        /// 实测进房间后长时间不刷新，导致第一次死亡被送回房间入口（上次的目标点）。
        /// </summary>
        internal static Vector3 TrackedGroundedPos = Vector3.zero;

        private static float _trackTimer;

        private static CinemachineVirtualCameraManager _vcam;
        private static Transform _lastFollow;
        private static float _logTimer;
        private static float _stuckTimer;
        private static float _aggroClearTimer;
        private static float _roomCheckTimer;
        private static BaseRoom _spectatedRoom;
        // v3.0.16：刚离开的观战房间 + 黏滞截止时刻。真机日志显示观战会在
        // '15*Standard*'<->'5*Standard*' 之间反复横跳，每次都关房 → 存活方所在房
        // 反复被 SetActive(false)。切走后 1.5s 内不再切回旧房，掐断摇摆。
        private static BaseRoom _spectateJustLeft;
        private static float _spectateStickyUntil;
        // v3.0.16：观战跳过关房日志的限流计时（独立于 _logTimer 的相机诊断日志）
        private static float _skipCloseLogTime;
        // v3.0.22：观战端跨区域补建房间的日志节流
        private static float _biomeBuildLogTime;
        // v3.0.19：观战换房滞回。相邻房的 Room.Bounds（含 10m letterbox）互相重叠，
        // 单拍“包含目标”会让观战在两房之间高频横跳；必须连续两拍确认才换。
        private static BaseRoom _spectateSwitchCandidate;
        private static int _spectateSwitchVotes;
        /// <summary>复活后仇恨压制窗截止时刻（unscaled）：窗内每 0.5s 再清一次索敌。</summary>
        private static float _aggroSuppressionUntil;
        private static float _aggroFailLogTime;
        /// <summary>倒地观战期间冻结角色物理的计时（防坠落速度累积）。</summary>
        private static float _freezeTimer;
        /// <summary>v2.4.1：真实死亡触发尝试次数与下次重试时刻（失败不再直接放弃）。</summary>
        private static int _realDeathAttempts;
        private static float _realDeathRetryAt;
        /// <summary>复活后临时打开 DisableDoorBlock 的截止时刻：门判定原本会因
        /// MovementDisabled/Stunned 把玩家钳回门口，复活瞬间状态可能还没干净。</summary>
        private static float _doorBlockUntil;
        // 观战期间被我们手动激活的房间（正常游戏里房间进出由门触发器管理，
        // 我们绕过了它，所以退出观战时要自己把这些房间关回去，否则里面的
        // 怪会一直活着跨房间追人 —— 这正是"下个房间的怪跑到上个房间打我"的来源）
        private static readonly List<BaseRoom> _activatedRooms = new List<BaseRoom>();
        // v2.8.7：已完成观战初始化的房间（含原生已激活的），避免同一个房间反复刷怪/建背景。
        private static readonly HashSet<BaseRoom> _spectateInitRooms = new HashSet<BaseRoom>();

        // v3.0.11：观战房初始化/相机的反射缓存。
        // 关键修复：PlacePlayerInRoom_Start/_End、InitializeSpawnControllerInstances、
        // InitializeBackgrounds、BroadcastPlayerEnterRoomEvents 都是【BaseRoom 的私有实例方法】，
        // 旧版用 room.GetType()（=Room/MergeRoom 派生类）去取——Harmony AccessTools.Method
        // 默认不向上找基类私有方法，反射恒为 null，于是怪/背景/进入事件全没初始化 → 黑房。
        // 显式用 typeof(BaseRoom) 取，一次缓存复用。
        // v3.0.21：设施补刷的反射缓存。InitializeSpawnControllerInstances
        // 会把敌人的 EnemyInstance 一起重新借出并把 Target 指回本地尸体（主机倒地观战时
        // 就是 inactive 的尸体），导致"怪不打存活方"。所以设施补刷只碰
        // PropSpawnController / ChestSpawnController。这两个 Init 是 public，
        // 但用反射调用可避免直接依赖原生类型签名，和本项目其它补丁风格一致。
        private static MethodInfo _propInitInstance;
        private static MethodInfo _chestInitInstance;
        private static MethodInfo _roomStartInit;
        private static MethodInfo _roomSpawnInit;
        private static MethodInfo _roomBgInit;
        private static MethodInfo _roomBroadcastEnter;
        private static PropertyInfo _roomCinemachineProp;
        private static FieldInfo _roomEventArgsField;
        private static MethodInfo _vcamSetActive;
        private static PropertyInfo _roomVcamProp;
        private static MethodInfo _vcamInternalUpdate;
        private static MethodInfo _brainManualUpdate;

        private static void EnsureRoomReflection()
        {
            if (_roomStartInit != null) return;
            _roomStartInit = AccessTools.Method(typeof(BaseRoom), "PlacePlayerInRoom_Start");
            _roomSpawnInit = AccessTools.Method(typeof(BaseRoom), "InitializeSpawnControllerInstances");
            _roomBgInit = AccessTools.Method(typeof(BaseRoom), "InitializeBackgrounds");
            _roomBroadcastEnter = AccessTools.Method(typeof(BaseRoom), "BroadcastPlayerEnterRoomEvents");
            _roomCinemachineProp = AccessTools.Property(typeof(BaseRoom), "CinemachineCamera");
            _roomEventArgsField = AccessTools.Field(typeof(BaseRoom), "m_roomViaDoorEventArgs");

            // v3.0.21：设施专用反射（不碰敌人）
            _propInitInstance = AccessTools.Method(typeof(PropSpawnController), "InitializePropInstance");
            _chestInitInstance = AccessTools.Method(typeof(ChestSpawnController), "InitializeChestInstance");
        }

        /// <summary>
        /// v3.0.21：只重新借出/初始化房间的 Prop（天赋·武器切换点、治疗树）和 Chest，
        /// 刻意跳过 EnemySpawnController —— 后者的 InitializeEnemyInstance 会把怪物的
        /// Target 重置成 PlayerManager.GetPlayer()（观战端就是 inactive 的本地尸体），
        /// 直接把"怪打存活方"又改坏。原生 InitializeSpawnControllerInstances 是三者一起做，
        /// 这里按类型拆开，做到"补设施不碰怪"。
        /// </summary>
        private static int EnsureRoomProps(BaseRoom room)
        {
            int count = 0;
            if (room == null) return 0;
            try
            {
                EnsureRoomReflection();
                var mgr = room.SpawnControllerManager;   // 强类型，和 RoomFix.CountMissingEnemies 一致
                if (mgr == null) return 0;

                var props = mgr.PropSpawnControllers;
                if (props != null && _propInitInstance != null)
                {
                    for (int i = 0; i < props.Length; i++)
                    {
                        var sc = props[i];
                        if (sc == null) continue;
                        try { _propInitInstance.Invoke(sc, null); count++; }
                        catch { /* 单个道具失败不影响其它 */ }
                    }
                }
                var chests = mgr.ChestSpawnControllers;
                if (chests != null && _chestInitInstance != null)
                {
                    for (int i = 0; i < chests.Length; i++)
                    {
                        var sc = chests[i];
                        if (sc == null) continue;
                        try { _chestInitInstance.Invoke(sc, null); count++; }
                        catch { }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] 补刷房间设施失败: " + ex.Message);
            }
            return count;
        }

        internal static bool InSession
        {
            get
            {
                var nm = NetworkManager.Instance;
                if (nm == null) return false;
                // 用"已入局"标志而不是 Connected：断线重连、主机结束对局后网络会停，
                // 但整局仍算联机对局，死亡必须继续走复活，不能掉回单机真死。
                return nm.SessionActive || nm.Connected || Plugin.DebugMode;
            }
        }

        // ---------------- 镜头 ----------------

        private static CinemachineVirtualCameraManager FindVcam()
        {
            try
            {
                var all = Resources.FindObjectsOfTypeAll(typeof(CinemachineVirtualCameraManager));
                CinemachineVirtualCameraManager first = null;
                for (int i = 0; i < all.Length; i++)
                {
                    var mgr = all[i] as CinemachineVirtualCameraManager;
                    if (mgr == null) continue;
                    if (!mgr.gameObject.scene.IsValid()) continue; // 排除资源
                    if (mgr.IsActiveVirtualCamera) return mgr;
                    if (first == null) first = mgr;
                }
                return first;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] FindVcam failed: " + ex.Message);
                return null;
            }
        }

        private static bool EnsureVcam()
        {
            if (_vcam != null) return true;
            _vcam = FindVcam();
            if (_vcam == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] No CinemachineVirtualCameraManager in scene; camera not moved.");
                return false;
            }
            return true;
        }

        /// <summary>切换跟随时记一条日志；同一个目标重复调用不重复打。</summary>
        internal static void Follow(Transform target)
        {
            if (target == null) return;
            if (!EnsureVcam()) return;
            if (_lastFollow == target) return;

            _lastFollow = target;
            try
            {
                _vcam.SetFollowTarget(target);
                Plugin.Log.LogInfo("[RL2MP] [DOWN] camera follow -> '" + target.gameObject.name + "'");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] SetFollowTarget failed: " + ex.Message);
            }
        }

        /// <summary>
        /// v2.8.3：把"当前活动的虚拟相机"的 Follow 指向观战目标。
        /// 每个房间有专属 vcam（含自己的 confiner/lens）；换房时游戏激活新房间的 vcam，
        /// 并把它的 Follow 设回本地玩家（已隐藏的尸体）。缓存的 _vcam 指向旧房间，改它无效
        /// ——这正是"半屏/失真/黑屏"的成因。rl2-coop 的做法是每帧重设活动相机，照搬。
        /// 全程反射，不静态引用 Cinemachine.dll（v2.5.0 type load 教训）。
        /// </summary>
        private static PropertyInfo _brainProp;
        private static PropertyInfo _activeVcamProp;
        private static PropertyInfo _vcamFollowProp;

        private static void FollowActiveVcam(Transform target)
        {
            if (target == null) return;
            try
            {
                if (_brainProp == null)
                    _brainProp = AccessTools.Property(typeof(CameraController), "CinemachineBrain");
                if (_brainProp == null) return;
                object brain = _brainProp.GetValue(null, null);
                if (brain == null) return;

                Type brainType = brain.GetType();
                if (_activeVcamProp == null || _activeVcamProp.DeclaringType != brainType)
                    _activeVcamProp = AccessTools.Property(brainType, "ActiveVirtualCamera");
                if (_activeVcamProp == null) return;
                object vcam = _activeVcamProp.GetValue(brain, null);
                if (vcam == null) return;

                Type vcamType = vcam.GetType();
                if (_vcamFollowProp == null || !_vcamFollowProp.DeclaringType.IsAssignableFrom(vcamType))
                    _vcamFollowProp = AccessTools.Property(vcamType, "Follow");
                if (_vcamFollowProp == null) return;

                object current = _vcamFollowProp.GetValue(vcam, null);
                if (!ReferenceEquals(current, target))
                    _vcamFollowProp.SetValue(vcam, target, null);
            }
            catch { /* 相机切换中：下一帧再纠 */ }
        }

        /// <summary>游戏切房间时会把镜头 Follow 改回玩家，这里每帧纠一次，不打日志。</summary>
        private static float _nextCamDiagAt;
        private static void FollowSilent(Transform target)
        {
            if (target == null) return;
            FollowActiveVcam(target);
            // v3.0.14：每 3 秒打一次观战相机诊断（只观测，绝不修改任何相机/房间状态）。
            // 只在观战期间走到这里，所以日志干净、不污染正常游戏。
            if (Time.unscaledTime >= _nextCamDiagAt)
            {
                _nextCamDiagAt = Time.unscaledTime + 3f;
                DumpSpectateDiagnostics(_spectatedRoom, target);
            }
        }

        /// <summary>
        /// v3.0.14：观战半黑屏诊断日志。用户提了五个 Unity 相机排查方向
        /// （Culling Mask / Clear Flags / Depth / Viewport Rect / Camera Stacking）——
        /// 反编译确认 RL2 是 Built-in 管线，第五项不适用；观战全程复用房间原生 vcam，
        /// mod 从未新建相机或改过这些属性。所以这里打印 brain 实际输出相机的真值供核对，
        /// 并补三个反编译锁定的真凶候选：confiner IsValid、活动 vcam 归属房间、letterbox 黑块。
        /// 全部反射 + 分级 try/catch，任何一个字段取不到都不影响观战主流程。
        /// </summary>
        private static void DumpSpectateDiagnostics(BaseRoom room, Transform target)
        {
            var log = new System.Text.StringBuilder("[RL2MP] [CamDiag]");
            try
            {
                log.Append(" room=").Append(room != null ? room.name : "<null>");
                log.Append(" target=").Append(target != null ? target.name : "<null>");

                // 1) Brain 实际输出相机：用户提的 Culling Mask / Clear Flags / Depth / Viewport Rect 真值
                try
                {
                    Camera outCam = GetBrainOutputCamera();
                    if (outCam != null)
                        log.Append(" | cam=").Append(outCam.name)
                           .Append(" cull=").Append(outCam.cullingMask)
                           .Append(" clear=").Append(outCam.clearFlags)
                           .Append(" depth=").Append(outCam.depth.ToString("F2"))
                           .Append(" rect=").Append(outCam.rect)
                           .Append(" ortho=").Append(outCam.orthographic)
                           .Append(" size=").Append(outCam.orthographicSize.ToString("F2"));
                    else
                        log.Append(" | outCam=<null>");
                }
                catch (Exception e) { log.Append(" | camErr=").Append(e.Message); }

                // 2) 活动 vcam 是谁的、Follow 指向谁（真凶候选：活动 vcam 不是观战房的）
                try
                {
                    object activeVcam = GetActiveVirtualCamera();
                    if (activeVcam != null)
                    {
                        var comp = activeVcam as UnityEngine.Component;
                        if (comp != null)
                        {
                            log.Append(" | vcam=").Append(comp.gameObject.name);
                            BaseRoom owner = null;
                            try { owner = comp.GetComponentInParent<BaseRoom>(); } catch { }
                            log.Append(" owner=").Append(owner != null ? owner.name : "<none>");
                        }
                        else log.Append(" | vcam=").Append(activeVcam.GetType().Name);

                        var followProp = AccessTools.Property(activeVcam.GetType(), "Follow");
                        object follow = followProp != null ? followProp.GetValue(activeVcam, null) : null;
                        log.Append(" follow=").Append(follow != null ? follow.ToString() : "<null>");
                    }
                    else log.Append(" | vcam=<null>");
                }
                catch (Exception e) { log.Append(" | vcamErr=").Append(e.Message); }

                // 3) 活动 vcam 的 confiner：IsValid / boundingShape active / bounds（头号真凶）
                try
                {
                    object activeVcam = GetActiveVirtualCamera();
                    var comp = activeVcam as UnityEngine.Component;
                    UnityEngine.Component confiner = comp != null
                        ? comp.GetComponent("CinemachineConfiner_RL") : null;
                    if (confiner != null)
                    {
                        var validProp = AccessTools.Property(confiner.GetType(), "IsValid");
                        bool valid = validProp != null && (bool)validProp.GetValue(confiner, null);
                        var shapeField = AccessTools.Field(confiner.GetType(), "m_BoundingShape2D");
                        var shape = shapeField != null ? shapeField.GetValue(confiner) as Collider2D : null;
                        bool shapeActive = shape != null && shape.gameObject.activeInHierarchy;
                        string bounds = shape != null
                            ? shape.bounds.size.x.ToString("F1") + "x" + shape.bounds.size.y.ToString("F1")
                            : "<null>";
                        log.Append(" | confiner valid=").Append(valid)
                           .Append(" shapeActive=").Append(shapeActive)
                           .Append(" bounds=").Append(bounds);
                    }
                    else log.Append(" | confiner=<none>");
                }
                catch (Exception e) { log.Append(" | confErr=").Append(e.Message); }

                // 4) 同时 active 的房间数（多房并存 → confiner 在错边界间摇摆）
                try
                {
                    var all = Resources.FindObjectsOfTypeAll<BaseRoom>();
                    int active = 0;
                    string names = "";
                    for (int i = 0; i < all.Length; i++)
                    {
                        var r = all[i];
                        if (r == null || !r.gameObject.scene.IsValid()) continue;
                        if (!r.gameObject.activeInHierarchy) continue;
                        active++;
                        if (names.Length < 140) names += r.name + ",";
                    }
                    log.Append(" | activeRooms=").Append(active).Append(" [").Append(names).Append("]");
                }
                catch (Exception e) { log.Append(" | roomCntErr=").Append(e.Message); }

                // 5) RoomLetterbox 黑块：由我们广播的 PlayerEnterRoom 触发，异常房间会生成遮挡块（看似半黑）
                try
                {
                    if (room != null)
                    {
                        int blocks = 0;
                        string detail = "";
                        var t = room.gameObject.transform;
                        for (int i = 0; i < t.childCount; i++)
                        {
                            var c = t.GetChild(i);
                            if (!c.gameObject.activeSelf) continue;
                            int layer = c.gameObject.layer;
                            if (layer != 5 && layer != 23) continue;   // RoomLetterboxController 固定用 5/23
                            blocks++;
                            if (detail.Length < 180)
                                detail += c.gameObject.name + "@" + c.position + " s=" + c.localScale + ";";
                        }
                        log.Append(" | letterbox=").Append(blocks).Append(" [").Append(detail).Append("]");
                    }
                }
                catch (Exception e) { log.Append(" | letterboxErr=").Append(e.Message); }
            }
            catch (Exception ex)
            {
                log.Append(" | DIAG_ERR=").Append(ex.Message);
            }
            Plugin.Log.LogInfo(log.ToString());
        }

        private static Camera GetBrainOutputCamera()
        {
            if (_brainProp == null)
                _brainProp = AccessTools.Property(typeof(CameraController), "CinemachineBrain");
            object brain = _brainProp != null ? _brainProp.GetValue(null, null) : null;
            if (brain == null) return null;
            try
            {
                var prop = AccessTools.Property(brain.GetType(), "OutputCamera");
                if (prop != null)
                {
                    Camera c = prop.GetValue(brain, null) as Camera;
                    if (c != null) return c;
                }
            }
            catch { }
            // 兜底：CinemachineBrain 通常就挂在实际输出相机上，直接取其 Camera 组件
            try
            {
                var comp = brain as UnityEngine.Component;
                if (comp != null) return comp.GetComponent<Camera>();
            }
            catch { }
            return null;
        }

        private static object GetActiveVirtualCamera()
        {
            if (_brainProp == null)
                _brainProp = AccessTools.Property(typeof(CameraController), "CinemachineBrain");
            object brain = _brainProp != null ? _brainProp.GetValue(null, null) : null;
            if (brain == null) return null;
            if (_activeVcamProp == null || _activeVcamProp.DeclaringType != brain.GetType())
                _activeVcamProp = AccessTools.Property(brain.GetType(), "ActiveVirtualCamera");
            return _activeVcamProp != null ? _activeVcamProp.GetValue(brain, null) : null;
        }

        /// <summary>
        /// v3.0.11：把指定房间的虚拟相机提升为 CinemachineBrain 的活动相机。
        /// 根因：每个房间有自己的 vcam，confiner 绑在房间根 Collider2D 上；
        /// CinemachineConfiner_RL.IsValid 要求 bounding shape activeInHierarchy。
        /// 旧房被 SetActive(false) 后若 Brain 还激活着它的 vcam，confiner 直接失效
        /// → 相机无边界 → 黑边/半屏/全黑。仅改 Follow 不够，必须把目标房 vcam 提到顶部。
        /// 全程反射，不静态引用 Cinemachine.dll。
        /// </summary>
        private static void PromoteSpectateCamera(BaseRoom room)
        {
            if (room == null) return;
            try
            {
                EnsureRoomReflection();
                if (_roomCinemachineProp == null) return;
                object mgr = _roomCinemachineProp.GetValue(room, null);
                if (mgr == null) return;
                Type mgrType = mgr.GetType();
                if (_vcamSetActive == null || _vcamSetActive.DeclaringType != mgrType)
                    _vcamSetActive = AccessTools.Method(mgrType, "SetIsActiveCamera");
                if (_vcamSetActive == null) return;
                _vcamSetActive.Invoke(mgr, new object[] { true });

                // 立即刷新相机状态，避免等到下一帧 Brain 切换时的黑屏窗口
                if (_roomVcamProp == null || _roomVcamProp.DeclaringType != mgrType)
                    _roomVcamProp = AccessTools.Property(mgrType, "VirtualCamera");
                if (_roomVcamProp != null)
                {
                    object vcam = _roomVcamProp.GetValue(mgr, null);
                    if (vcam != null)
                    {
                        if (_vcamInternalUpdate == null || _vcamInternalUpdate.DeclaringType != vcam.GetType())
                            _vcamInternalUpdate = AccessTools.Method(vcam.GetType(), "InternalUpdateCameraState");
                        if (_vcamInternalUpdate != null)
                            _vcamInternalUpdate.Invoke(vcam, new object[] { Vector3.up, 1f });
                    }
                }
                // Brain 手动刷新一次（原生 OnPlayerEnterRoom 同款）
                if (_brainProp == null)
                    _brainProp = AccessTools.Property(typeof(CameraController), "CinemachineBrain");
                object brain = _brainProp != null ? _brainProp.GetValue(null, null) : null;
                if (brain != null)
                {
                    if (_brainManualUpdate == null || _brainManualUpdate.DeclaringType != brain.GetType())
                        _brainManualUpdate = AccessTools.Method(brain.GetType(), "ManualUpdate");
                    if (_brainManualUpdate != null) _brainManualUpdate.Invoke(brain, null);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战相机提升失败: " + ex.Message);
            }
        }

        // ---------------- 观战目标 ----------------

        private static PlayerSync PS { get { return PlayerSync.Instance; } }

        internal static int RemoteCount
        {
            get { var ps = PS; return ps == null ? 0 : ps.RemotePlayers.Count; }
        }

        internal static int AliveRemoteCount
        {
            get
            {
                var ps = PS;
                if (ps == null) return 0;
                int n = 0;
                foreach (var kv in ps.RemotePlayers)
                {
                    if (kv.Value == null) continue;
                    if (!ps.RemoteDowned.Contains(kv.Key)) n++;
                }
                return n;
            }
        }

        /// <summary>按 ID 顺序循环选一个还活着的远程玩家。</summary>
        internal static GameObject CycleTarget(int direction)
        {
            var ps = PS;
            if (ps == null || ps.RemotePlayers.Count == 0) return null;

            var ids = new List<int>();
            foreach (var kv in ps.RemotePlayers)
            {
                if (kv.Value == null) continue;
                if (ps.RemoteDowned.Contains(kv.Key)) continue;
                ids.Add(kv.Key);
            }
            if (ids.Count == 0) return null;
            ids.Sort();

            int idx = ids.IndexOf(SpectateTargetId);
            if (idx < 0) idx = direction >= 0 ? -1 : 0;
            idx = ((idx + (direction >= 0 ? 1 : -1)) % ids.Count + ids.Count) % ids.Count;

            SpectateTargetId = ids[idx];
            return ps.RemotePlayers[SpectateTargetId];
        }

        /// <summary>当前观战目标如果没了（死亡/离场），自动换下一个。</summary>
        internal static GameObject EnsureTarget()
        {
            var ps = PS;
            if (ps == null) return null;

            GameObject go;
            if (SpectateTargetId >= 0 && ps.RemotePlayers.TryGetValue(SpectateTargetId, out go) && go != null
                && !ps.RemoteDowned.Contains(SpectateTargetId))
            {
                return go;
            }
            return CycleTarget(1);
        }

        // ---------------- 敌人仇恨 ----------------

        /// <summary>
        /// 倒地后本地玩家被隐藏，但房间里已有的怪物仍锁定着玩家对象，
        /// 会对着尸体位置持续攻击（画面很难看）。清掉所有敌人的 Target。
        /// </summary>
        internal static void ClearEnemyAggro()
        {
            try
            {
                var enemies = UnityEngine.Object.FindObjectsOfType<EnemyController>();
                int cleared = 0;
                for (int i = 0; i < enemies.Length; i++)
                {
                    var e = enemies[i];
                    if (e == null) continue;
                    // 场景切换瞬间敌人可能正在销毁，访问 Target 会抛 NRE —— 逐个隔离
                    try
                    {
                        if (e.Target != null) { e.Target = null; cleared++; }
                    }
                    catch { }
                }
                if (cleared > 0)
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 清除 " + cleared + " 个敌人的仇恨（玩家已倒地）。");
            }
            catch (Exception ex)
            {
                // 场景卸载时 FindObjectsOfType 本身可能抛异常；保护窗内每 0.5s 调一次，
                // 不能刷屏 —— 只在间隔超过 5s 时记录一次
                if (Time.unscaledTime - _aggroFailLogTime > 5f)
                {
                    _aggroFailLogTime = Time.unscaledTime;
                    Plugin.Log.LogWarning("[RL2MP] [DOWN] ClearEnemyAggro failed: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// 把本地角色的物理速度清零（倒地观战期间每 0.1s 调一次）。
        /// 角色虽然被 SetActive(false)，但 Corgi 的 _velocity 字段还会保留死亡瞬间的值
        /// （坠落/击退/虚空冲刺），复活时原样带回 —— 这是"复活后速度很快"的来源之一。
        /// </summary>
        private static void FreezeLocalBody()
        {
            try
            {
                var ps = PS;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return;
                pc.SetVelocity(0f, 0f, false);
                if (pc.ControllerCorgi != null) pc.ControllerCorgi.SetForce(Vector2.zero);
                if (pc.CharacterDash != null && pc.CharacterDash.IsDashing) pc.CharacterDash.StopDash();
            }
            catch { }
        }

        // ---------------- 复活点采样 ----------------

        /// <summary>
        /// 每帧（节流 0.2s）采样：活着、站在地上、在房间里时记一笔坐标。
        /// 复活时"掉坑/掉刺/在空气墙外死亡"就靠这个点兜底。
        /// </summary>
        internal static void TrackBody()
        {
            if (LocalDown) return;
            _trackTimer += Time.unscaledDeltaTime;
            if (_trackTimer < 0.2f) return;
            _trackTimer = 0f;

            try
            {
                var ps = PS;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return;
                if (pc.CurrentHealth <= 0f) return;

                var corgi = pc.ControllerCorgi;
                if (corgi == null || corgi.State == null) return;
                if (!corgi.State.IsGrounded) return;
                if (pc.CurrentlyInRoom == null) return;

                TrackedGroundedPos = pc.transform.position;
            }
            catch { /* 采样失败无所谓，还有死亡坐标兜底 */ }
        }

        /// <summary>
        /// v1.7.2：复活后把周围怪物的目标重新绑回本地玩家。
        ///
        /// 旧逻辑在复活时把敌人的 Target 清成 null，但敌人只在 Awake/初始化时
        /// （EnemyController.Start 里 m_target = PlayerManager.GetPlayer()）拿一次目标，
        /// 之后没有任何地方会重新索敌 —— 结果就是复活后怪彻底失去目标：
        /// 移动回到默认巡逻轨迹（"不跟人物走了"）、AI 的攻击分支全部因为 !Target 短路，
        /// 表现成"怪不打人了"。这里按游戏自己的赋值方式补回来。
        /// </summary>
        internal static void RebindEnemyTargets()
        {
            var ps = PS;
            var playerGo = ps != null ? ps.CachedLocalPlayerObject : null;
            if (playerGo == null) return;

            int fixedCount = 0;
            try
            {
                var enemies = UnityEngine.Object.FindObjectsOfType<EnemyController>();
                for (int i = 0; i < enemies.Length; i++)
                {
                    var e = enemies[i];
                    if (e == null) continue;
                    try
                    {
                        if (e.Target == null)
                        {
                            e.Target = playerGo;
                            fixedCount++;
                        }
                    }
                    catch { /* 单个敌人异常不能影响其他敌人 */ }
                }
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _aggroFailLogTime > 5f)
                {
                    _aggroFailLogTime = Time.unscaledTime;
                    Plugin.Log.LogWarning("[RL2MP] [DOWN] RebindEnemyTargets failed: " + ex.Message);
                }
                return;
            }

            if (fixedCount > 0)
                Plugin.Log.LogInfo("[RL2MP] [DOWN] 复活后重新索敌：" + fixedCount + " 个怪物重新锁定本地玩家。");
        }

        // ---------------- 观战房间激活 ----------------

        internal static void ClearSpectateRoomSelection()
        {
            _spectatedRoom = null;
            _spectateSwitchCandidate = null;
            _spectateSwitchVotes = 0;
        }

        /// <summary>
        /// RL2 的 World 场景里同一时刻只有一个房间 GameObject 是激活的
        /// （PlayerExit 会把旧房间 SetActive(false)）。主机走进下一个房间后，
        /// 客机本地那个房间根本没激活，镜头跟过去只能看到空场景。
        /// 这里把观战目标所在的房间在客机端激活 + 初始化（敌人/道具/背景/存档状态），
        /// 并让相机边界切过去 —— 等价于游戏的 PlacePlayerInRoom，但不挪动本地玩家。
        /// </summary>
        private static void EnsureSpectateRoom(Vector3 targetPos, Vector2? hostGridCoords = null,
            Vector2? hostAnchor = null, BaseRoom targetRoom = null)
        {
            try
            {
                // v3.0.18：权威换房包给出的房间是“存活方真正所在房间”的直接证据，
                // 优先级高于按坐标猜房。真机日志里 '14*Standard*'<->'9*Standard*' 横跳，
                // 而权威坐标始终是 (160,-18)=14*Standard*，正是坐标猜测选错的房。
                // 过渡房 (*Transition*) 全程排除。
                BaseRoom anchorRoom = hostAnchor.HasValue
                    ? RoomTravelSync.FindRoomByAnchor(hostAnchor.Value.x, hostAnchor.Value.y) : null;
                if (IsTransitionRoom(anchorRoom)) anchorRoom = null;
                BaseRoom gridRoom = hostGridCoords.HasValue
                    ? FindRoomByGridCoords(hostGridCoords.Value) : null;
                if (IsTransitionRoom(gridRoom)) gridRoom = null;
                BaseRoom authRoom = anchorRoom ?? gridRoom;

                // v3.0.22：跨区域（biome）时本地根本没有权威端所在区域的房间对象 ——
                // 原生 BiomeTransitionController 只在"本地玩家实际过门"的一端实例化新区域，
                // 倒地观战端从没过那扇门 → 选房必然落空（真机日志 "目标(96,-180)未匹配：
                // 场景内有效房间=46 个"）→ 观战视角卡在初始地图。
                // 这里在本地把权威端所在区域的房间补建出来（原生 InstantiateBiome），
                // 补建完由下面的既有选房逻辑接管。
                if (authRoom == null && gridRoom == null)
                {
                    int remoteBiome = RoomTravelSync.RemoteBiome;
                    int localBiome = RoomTravelSync.CurrentLocalBiome();
                    if (remoteBiome >= 0 && remoteBiome != localBiome)
                    {
                        // 触发补建（内部有节流与去重，不会每帧重复跑协程）。
                        if (BiomeRoomBuilder.EnsureBiome(remoteBiome))
                        {
                            // 补建是异步的（原生协程），这一拍先按现有逻辑选房，
                            // 下一拍房间对象就位了，选房自然命中。
                            if (Time.unscaledTime - _biomeBuildLogTime > 3f)
                            {
                                _biomeBuildLogTime = Time.unscaledTime;
                                Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战端跨区域：已触发本地补建区域 "
                                    + ((BiomeType)remoteBiome) + " 的房间");
                            }
                        }
                    }
                }

                // v3.0.19：判“包含目标”必须用房间【可玩区】（confiner/letterbox 内），
                // 不能用 Room.Bounds —— 后者把四周 10m letterbox 也算进去，相邻房会
                // 同时“包含”分身。真机日志 4*Mandatory*<->3*Standard* 每 1~2 秒横跳，
                // 正是 v3.0.18 的 authContainsTarget 拿宽 Bound 判包含后绕过了防横跳守卫。
                bool authContainsTarget = PlayableContains(authRoom, targetPos);
                bool currentContainsTarget = PlayableContains(_spectatedRoom, targetPos);

                // 权威房确实包着目标 -> 直接用它；否则退回分身坐标反查（合并房里更贴合）。
                var room = (targetRoom != null && !IsTransitionRoom(targetRoom) ? targetRoom : null)
                           ?? (authContainsTarget ? authRoom : null)
                           ?? FindRoomContaining(targetPos)
                           ?? anchorRoom
                           ?? gridRoom;
                if (room == null) return;

                // v3.0.18/19：目标仍在当前观战房可玩区内就钉住当前房。合并房与成员房
                // Bounds 重叠，FindRoomContaining 按"面积最小"选时会在相邻房之间反复切换，
                // 每次换房都关掉上一间，房间里的 Prop（补给/装备切换点、箱子、商店）
                // 随对象池归还后再也不补 —— 表现就是"客机看得见，主机观战空白"。
                if (_spectatedRoom != null && !ReferenceEquals(room, _spectatedRoom))
                {
                    if (currentContainsTarget)
                    {
                        room = _spectatedRoom;
                        _spectateSwitchCandidate = null;
                        _spectateSwitchVotes = 0;
                    }
                    else
                    {
                        // v3.0.19：借鉴 RevivalSync 的边沿/有界去重——连续两拍（约 1 秒）
                        // 都指向同一个新房才换，挡住权威坐标与分身插值抖动。
                        if (!ReferenceEquals(room, _spectateSwitchCandidate))
                        {
                            _spectateSwitchCandidate = room;
                            _spectateSwitchVotes = 1;
                        }
                        else _spectateSwitchVotes++;
                        if (_spectateSwitchVotes < 2) room = _spectatedRoom;
                    }
                }
                else
                {
                    _spectateSwitchCandidate = null;
                    _spectateSwitchVotes = 0;
                }

                // v3.0.16：黏滞窗内不切回刚离开的房间，止住两房横跳反复开关房。
                // 只在"目标分身仍在当前房内"时生效——分身真进了新房则必须立刻跟上，
                // 不能因为黏滞把镜头锁在旧房。
                if (_spectatedRoom != null && !ReferenceEquals(room, _spectatedRoom)
                    && ReferenceEquals(room, _spectateJustLeft)
                    && Time.unscaledTime < _spectateStickyUntil)
                {
                    bool targetInCur = false;
                    try { targetInCur = _spectatedRoom.Bounds.Contains(targetPos); } catch { }
                    if (targetInCur) room = _spectatedRoom;
                }

                bool roomChanged = (_spectatedRoom != room);
                if (roomChanged)
                {
                    BaseRoom previous = _spectatedRoom;
                    _spectatedRoom = room;
                    _spectateSwitchCandidate = null;
                    _spectateSwitchVotes = 0;
                    // 记下刚离开的房，1.5s 内不切回（防止横跳）。
                    if (previous != null)
                    {
                        _spectateJustLeft = previous;
                        _spectateStickyUntil = Time.unscaledTime + 1.5f;
                    }

                    // v3.0.10：切换观战房间时，把我们上一间手动激活的房间关回去。
                    // 旧版只在退出观战(RestoreSpectateRooms)时统一关，观战期间跨房间
                    // 一路累积 active（真机日志：8*Standard*→5*Mandatory*→4*Standard*
                    // →29*Mandatory* 四个房间同时开着）。RL2 同一时刻只该有一个房间 active，
                    // 多房并存让相机 confiner 在错边界间摇摆 -> 黑屏/半屏/房间看着不对。
                    // 只关"我们自己激活且没被本地玩家占用"的旧房，绝不碰官方激活的房间。
                    if (previous != null && !ReferenceEquals(previous, room)
                        && _activatedRooms.Contains(previous))
                    {
                        BaseRoom localRoom = null;
                        var psc = PS != null ? PS.LocalPlayer as PlayerController : null;
                        try { if (psc != null) localRoom = psc.CurrentlyInRoom; } catch { }

                        // v3.0.16（真机铁证）：倒地端观战选房会在相邻房之间摇摆
                        //（主机日志“关闭上一房间 X”紧跟客机日志“掉出房间 X”：
                        // 5*/11*/9*/20*Standard* 四间逐一对应）。根因是这里的
                        // SetActive(false) 把【存活方（观战目标分身）正踩着的房间】关掉了——
                        // 房间一关，地板 Collider2D 与门出口触发器一起失效，
                        // 存活方当场掉出房间/门卡死，被 RoomFix 救回后重复。
                        // 旧守卫只查本地尸体 localRoom，完全没查观战目标，故漏掉。
                        // 新守卫：只要观战目标分身当前坐标仍在 previous 的 Bounds 内，
                        // 就绝不关它。宁可多开一间房（相机另有问题），也不能让存活方掉坑。
                        bool targetStillInPrevious = false;
                        try
                        {
                            var pb = previous.Bounds;
                            if (pb.size.x > 0.1f && pb.size.y > 0.1f && pb.Contains(targetPos))
                                targetStillInPrevious = true;
                        }
                        catch { }

                        if (!ReferenceEquals(previous, localRoom) && !targetStillInPrevious)
                        {
                            try
                            {
                                if (previous.gameObject.activeSelf) previous.gameObject.SetActive(false);
                                // v3.0.18：房间关闭时 Prop 实例已归还对象池。清掉初始化标记，
                                // 下次再进这间房才会重新走完整的种子/道具/存档初始化。
                                _spectateInitRooms.Remove(previous);
                                Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战换房，关闭上一房间: '" + previous.name + "'");
                            }
                            catch { /* 场景可能已卸载 */ }
                        }
                        else if (targetStillInPrevious)
                        {
                            // 保留 active，但仍从待恢复列表移除：存活方自己过门时会 SetActive(false)，
                            // 我们不再在退出观战时重复关它。
                            if (Time.unscaledTime - _skipCloseLogTime > 2f)
                            {
                                _skipCloseLogTime = Time.unscaledTime;
                                Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战跳过关房 '" + previous.name
                                    + "'：存活方仍在该房间内（关掉会让它掉出/卡门）。");
                            }
                        }
                        _activatedRooms.Remove(previous);
                    }

                    // v3.0.18：清理更早以前残留的手动激活房。真机日志 activeRooms=3~4，
                    // 'Merge Room 1' 在切到 '14*Standard*' 后仍常驻 active，多房并存让
                    // 相机 confiner 和房间内容互相干扰（观战画面出现空白/错房间）。
                    // 只关我们自己激活过、且目标与本地尸体都不在的房间。
                    try
                    {
                        BaseRoom curLocalRoom = null;
                        var psc2 = PS != null ? PS.LocalPlayer as PlayerController : null;
                        try { if (psc2 != null) curLocalRoom = psc2.CurrentlyInRoom; } catch { }

                        var stale = new List<BaseRoom>();
                        foreach (var r in _activatedRooms)
                        {
                            if (r == null || ReferenceEquals(r, room) || ReferenceEquals(r, curLocalRoom)) continue;
                            bool containsTarget = false;
                            try
                            {
                                var rb = r.Bounds;
                                containsTarget = rb.size.x > 0.1f && rb.size.y > 0.1f && rb.Contains(targetPos);
                            }
                            catch { }
                            if (!containsTarget) stale.Add(r);
                        }
                        for (int i = 0; i < stale.Count; i++)
                        {
                            try
                            {
                                if (stale[i].gameObject.activeSelf) stale[i].gameObject.SetActive(false);
                                Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战清理残留房间: '" + stale[i].name + "'");
                            }
                            catch { /* 场景可能已卸载 */ }
                            _activatedRooms.Remove(stale[i]);
                            _spectateInitRooms.Remove(stale[i]);
                        }
                    }
                    catch { }

                    // v2.8.7（借鉴 rl2-coop 观战）：房间"被原生提前激活"不等于内容已生成。
                    // 旧版只在 !activeSelf 时才做 种子/激活/敌人/背景 初始化 —— 存活方
                    // 跨 biome 换房时，新房间对象可能已被 SceneLoader 预激活，但
                    // InitializeSpawnController/Background 还没跑（对端走的是完整换房流程），
                    // 于是观战端拿到的就是一间空房/黑屏，而且永远不再初始化。
                    // 改为：**首次进入观战视野的房间，无论 active 与否都完整初始化一次**，
                    // 用 _spectateInitRooms 去重，避免每个房间反复刷怪。
                    if (!_spectateInitRooms.Contains(room))
                    {
                        _spectateInitRooms.Add(room);
                        EnsureRoomReflection();

                        // v3.0.11：PlacePlayerInRoom_Start —— 原生进房第一步置 IsPlayerInRoom=true。
                        // 旧版漏了这步，部分监听者读 IsPlayerInRoom 拿到 false → 房间内容不激活。
                        if (_roomStartInit != null)
                        {
                            try { _roomStartInit.Invoke(room, null); }
                            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] PlacePlayerInRoom_Start 失败: " + ex.Message); }
                        }

                        // 复刻 PlacePlayerInRoom_End 的顺序：种子 -> 激活 -> 敌人/道具 -> 背景
                        try
                        {
                            // GetSpecialPropSeed 在 BaseRoom 是 abstract，实现在派生类上，用 room.GetType() 正确
                            var seedMethod = AccessTools.Method(room.GetType(), "GetSpecialPropSeed");
                            if (seedMethod != null)
                                RNGManager.SetSeed(RngID.SpecialProps_RoomSeed, (int)seedMethod.Invoke(room, null));
                        }
                        catch { /* 种子只是道具随机，失败无碍 */ }

                        bool wasActive = room.gameObject.activeSelf;
                        if (!wasActive)
                        {
                            room.gameObject.SetActive(true);
                            _activatedRooms.Add(room);   // 只有我们激活的，重置时才关回去
                        }

                        // v3.0.21：设施（天赋/武器切换点、治疗树）和怪必须分开补。
                        // 旧版把两者绑在 CountMissingEnemies>0 一个条件上：怪齐全但
                        // Prop 已随关房归还池时，InitializeSpawnControllerInstances 不跑，
                        // 设施永远借不回来 —— 观战画面就只剩地板，看不到切换点/生命树。
                        // 现在无条件先补 Prop/Chest（EnsureRoomProps 刻意不碰敌人），
                        // 只有确实缺怪时才调完整的 InitializeSpawnControllerInstances。
                        try
                        {
                            int props = EnsureRoomProps(room);
                            if (props > 0)
                                Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战房间补刷设施: '" + room.name
                                    + "' " + props + " 个 Prop/Chest");
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战补刷设施失败: " + ex.Message);
                        }
                        try
                        {
                            if (RoomFix.CountMissingEnemies(room) > 0 && _roomSpawnInit != null)
                            {
                                _roomSpawnInit.Invoke(room, null);
                                Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战房间补刷怪: '" + room.name + "'");
                            }
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战补刷怪失败: " + ex.Message);
                        }
                        // 背景初始化单独 try：抛异常也不能连累后面的存档/相机
                        if (_roomBgInit != null)
                        {
                            try { _roomBgInit.Invoke(room, null); }
                            catch (Exception ex)
                            {
                                Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战背景初始化失败: " + ex.Message);
                            }
                        }
                        Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战房间已激活: '" + room.name
                            + "' (wasActive=" + wasActive + ")");
                    }

                    // 按客机地图的房间状态加载（主机打死的敌人不会复活，开过的箱子保持开过）。
                    // v2.4.0：单独 try —— 这一步在客机端会抛 NRE（房间半初始化，
                    // RoomSaveController.OnPlayerEnter_LoadStageData 里读 null）。
                    // 旧版把它和下面的相机设置写在同一个 try 里，一抛异常相机就不切了，
                    // 表现就是"观战时画面被锁在旧房间边界、露黑边/空白"。
                    try
                    {
                        if (room.SaveController != null) room.SaveController.OnPlayerEnter_LoadStageData();
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战房间存档加载失败（不影响观战）: " + ex.Message);
                    }
                }

                // 相机边界切到该房间（否则 confiner 还锁在本地玩家房间）。
                // 只在房间变化时设一次：本地玩家已 SetActive(false)，游戏不会再为它改 confiner，
                // 定期重设反而会让画面每 2 秒抖一下。
                if (roomChanged)
                {
                    // v3.0.15：确保选中的房间一定是 active。真机日志实锤：观战在合并房/成员房
                    // 之间摇摆时，旧房被 SetActive(false)，再次选中它时因 _spectateInitRooms 去重
                    // 跳过了激活分支 → 房间仍 inactive，其 vcam/门/内容全失效 = 摄像机停在上一间。
                    // 无条件补一次激活（幂等：已 active 的节点 SetActive(true) 无副作用）。
                    try
                    {
                        if (!room.gameObject.activeSelf)
                        {
                            room.gameObject.SetActive(true);
                            if (!_activatedRooms.Contains(room)) _activatedRooms.Add(room);
                            Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战重新激活房间: '" + room.name + "'");

                            // v3.0.21：重激活只补 Prop/Chest，不调完整 InitializeSpawnControllerInstances
                            //（那会把存活方正在打的怪重新借出并把 Target 指回本地尸体）。
                            try
                            {
                                int props = EnsureRoomProps(room);
                                if (props > 0)
                                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战重激活后补刷房间道具: '"
                                        + room.name + "' " + props + " 个 Prop/Chest");
                            }
                            catch (Exception ex)
                            {
                                Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战重激活补道具失败: " + ex.Message);
                            }
                        }
                    }
                    catch { /* 场景卸载中 */ }

                    // v3.0.11：用真实门构造事件参数（部分监听者读 Door.Side，null 会 NRE 被吞 → 内容不加载）
                    Door eventDoor = RoomFix.FindNearestDoor(room, targetPos);
                    try { CameraController.UpdateRoomCameraSettings(new RoomViaDoorEventArgs(room, eventDoor)); }
                    catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战相机设置失败: " + ex.Message); }

                    // v3.0.11（黑屏根因修复）：原生 UpdateRoomCameraSettings 只设了 lens/zoom，
                    // SetIsActiveCamera 是否生效取决于房间 vcam 是否 active。旧房被我们
                    // SetActive(false) 后 Brain 可能仍激活着它的 vcam，而它的 confiner 已失效。
                    // 这里显式把观战房的 vcam 提到 Brain 顶部并立即 InternalUpdate，掐掉黑屏窗口。
                    PromoteSpectateCamera(room);

                    // v3.0.11：补 BroadcastPlayerEnterRoomEvents（原生 PlacePlayerInRoom_End 最后一步）。
                    // 缺这步 → PlayerEnterRoom relay 不派发，部分房间内容（道具/机关）不激活。
                    // 广播前先确保 m_roomViaDoorEventArgs 已初始化（原生 _End 里做的），否则 broadcast 内部 NRE。
                    if (_roomBroadcastEnter != null)
                    {
                        try
                        {
                            // v3.0.21：原生 PlacePlayerInRoom_End 对已存在的事件参数是
                            // Initialise(this, door) —— 即把 Room 更新成【当前房间】。
                            // 旧版只在 null 时 new，换房后 m_roomViaDoorEventArgs 仍指向
                            // 上一间房，Broadcast 派发时 PlayerEnterRoom relay 带的是旧房间，
                            // 新房里的 BaseSpecialRoomController 因此不触发
                            // SpecialRoomInitializedRelay → 天赋/武器切换点、治疗树不显示。
                            var args = _roomEventArgsField != null
                                ? _roomEventArgsField.GetValue(room) as RoomViaDoorEventArgs : null;
                            if (args == null)
                            {
                                args = new RoomViaDoorEventArgs(room, eventDoor);
                                if (_roomEventArgsField != null)
                                    _roomEventArgsField.SetValue(room, args);
                            }
                            else
                            {
                                args.Initialise(room, eventDoor);
                            }
                            _roomBroadcastEnter.Invoke(room, new object[] { eventDoor });
                            Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战派发 PlayerEnterRoom: '" + room.name + "'");
                        }
                        catch (Exception ex)
                        {
                            Plugin.Log.LogWarning("[RL2MP] [DOWN] 观战广播 PlayerEnterRoom 失败: " + ex.Message);
                        }
                    }

                    // v3.0.14：换房瞬间强制打一条诊断（此时最容易出现 confiner 失效/错房 vcam），
                    // 并把节流计时推后 3 秒，避免紧接着 FollowSilent 又重复打一条。
                    _nextCamDiagAt = Time.unscaledTime + 3f;
                    GameObject diagTarget = null;
                    try
                    {
                        var psDiag = PS;
                        if (psDiag != null && SpectateTargetId >= 0)
                            psDiag.RemotePlayers.TryGetValue(SpectateTargetId, out diagTarget);
                    }
                    catch { }
                    DumpSpectateDiagnostics(room, diagTarget != null ? diagTarget.transform : null);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] EnsureSpectateRoom failed: " + ex.Message);
            }
        }

        /// <summary>
        /// v2.8.3：用主机权威房间网格坐标精确定位房间。
        /// 旧版用"存活怪平均世界坐标"猜房间，怪分散在相邻房间时平均点落到第三间，
        /// 观战房间在 7*Bonus*/16*Trap* 之间反复横跳，confiner 锁在旧房间边界 →
        /// 半屏/失真/主机到客机没去过的房间时全黑。主机 ALIVE 包里就带精确房间坐标，直接用。
        /// </summary>
        /// <summary>
        /// v3.0.13：过渡房判定。'1*Transition*' 这类房间是场景切换的中转壳，
        /// Bounds/相机配置异常，作观战目标会让 confiner 锁到错边界 = 半黑屏/全黑，
        /// 且 Coordinates=(0,0) 会被网格选房误命中。观战选房一律排除。
        /// </summary>
        private static bool IsTransitionRoom(BaseRoom room)
        {
            if (room == null) return false;
            try
            {
                string n = room.name;
                return !string.IsNullOrEmpty(n)
                    && n.IndexOf("Transition", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// v3.0.19：目标是否落房间的【可玩区】内。优先读房间 vcam 的 confiner
        /// bounding shape（真机日志里 32x36 / 64x18，正是相机实际能显示的区域）；
        /// 读不到时退化为把 Room.Bounds 每边缩掉 10m（letterbox 宽度）。
        /// 不能用原始 Room.Bounds：它包含 letterbox，相邻房会同时命中。
        /// </summary>
        private static bool PlayableContains(BaseRoom room, Vector3 pos)
        {
            if (room == null) return false;
            Bounds b = default(Bounds);
            bool have = false;
            try
            {
                EnsureRoomReflection();
                var mgr = _roomCinemachineProp != null
                    ? _roomCinemachineProp.GetValue(room, null) as CinemachineVirtualCameraManager
                    : null;
                if (mgr != null && mgr.gameObject != null)
                {
                    var confiner = mgr.gameObject.GetComponent("CinemachineConfiner_RL");
                    if (confiner != null)
                    {
                        var field = AccessTools.Field(confiner.GetType(), "m_BoundingShape2D");
                        var shape = field != null ? field.GetValue(confiner) as Collider2D : null;
                        if (shape != null)
                        {
                            b = shape.bounds;
                            have = b.size.x > 0.1f && b.size.y > 0.1f;
                        }
                    }
                }
            }
            catch { }
            if (!have)
            {
                try
                {
                    var rb = room.Bounds;
                    b = new Bounds(rb.center, new Vector3(
                        Mathf.Max(0f, rb.size.x - 20f), Mathf.Max(0f, rb.size.y - 20f), 1f));
                    have = b.size.x > 0.1f && b.size.y > 0.1f;
                }
                catch { }
            }
            return have
                && Mathf.Abs(pos.x - b.center.x) <= b.size.x * 0.5f + 0.5f
                && Mathf.Abs(pos.y - b.center.y) <= b.size.y * 0.5f + 0.5f;
        }

        private static BaseRoom FindRoomByGridCoords(Vector2 coords)
        {
            try
            {
                // v3.0.11：合并房 MergeRoom 不是 Room，旧版 FindObjectsOfTypeAll<Room>()
                // 看不到它，观战主路径若锚点失效就只能锁到相邻标准房（半屏/黑房）。
                var rooms = Resources.FindObjectsOfTypeAll<BaseRoom>();
                BaseRoom best = null;
                float bestD = float.MaxValue;
                // v3.0.22：网格坐标是【每个 biome 内部】的坐标系 —— 不同区域的
                // (96,-180) 可能指向完全不同的房间。跨区域时如果不限 biome，
                // 就会命中初始区域里同坐标的旧房间（视角跟着"看起来对、其实错"的房）。
                // 权威端带了 biome 时，只在同一个 biome 内匹配。
                int filterBiome = RoomTravelSync.RemoteBiome;
                for (int i = 0; i < rooms.Length; i++)
                {
                    var r = rooms[i];
                    if (r == null || !r.gameObject.scene.IsValid()) continue; // 排除 prefab
                    if (IsTransitionRoom(r)) continue; // v3.0.13：过渡房不参与观战选房
                    if (filterBiome >= 0 && (int)r.BiomeType != filterBiome) continue;
                    var rr = r as Room;
                    Vector2 rc = rr != null ? rr.Coordinates : (Vector2)r.transform.position;
                    float d = (rc - coords).sqrMagnitude;
                    if (d < bestD) { bestD = d; best = r; }
                }
                // 网格坐标是整数，1 格外就算没匹配上（不让"最近的错房间"混进来）
                return bestD <= 1f ? best : null;
            }
            catch { return null; }
        }

        /// <summary>找出包含该坐标的房间（含未激活的；世界坐标，x/y 判定，容差 1m）。</summary>
        private static BaseRoom FindRoomContaining(Vector3 pos)
        {
            // v3.0.9：合并房 Bounds 覆盖多个标准房，旧的"偏好 active 房"会命中大合并房，
            // 观战相机 confiner 跟着锁到错误边界 → 黑屏/半屏。改为优先【最贴合】的房：
            // 命中房里 Bounds 面积最小的（通常就是玩家实际所在的那一间标准房），
            // 面积相同再看是否 active。与门/换房问题的合并房根因一致。
            BaseRoom best = null;
            float bestArea = float.MaxValue;
            var rooms = Resources.FindObjectsOfTypeAll<BaseRoom>();
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                if (r == null) continue;
                if (!r.gameObject.scene.IsValid()) continue; // 排除 prefab 资源
                if (IsTransitionRoom(r)) continue; // v3.0.13：过渡房不参与观战选房
                try
                {
                    var b = r.Bounds;
                    Vector3 c = b.center;
                    Vector3 s = b.size;
                    if (Mathf.Abs(pos.x - c.x) > s.x * 0.5f + 1f) continue;
                    if (Mathf.Abs(pos.y - c.y) > s.y * 0.5f + 1f) continue;
                    float area = Mathf.Max(0f, s.x) * Mathf.Max(0f, s.y);
                    // 面积明显更小优先；面积接近时 active 优先（避免未激活空房抢赢）
                    bool better = area < bestArea - 1f
                        || (Mathf.Abs(area - bestArea) <= 1f && r.gameObject.activeSelf
                            && (best == null || !best.gameObject.activeSelf));
                    if (best == null || better) { best = r; bestArea = area; }
                }
                catch { /* Bounds 可能在未初始化房间上抛异常 */ }
            }
            return best;
        }

        // ---------------- 状态切换 ----------------

        internal static void GoDown(string reason)
        {
            if (LocalDown) return;
            LocalDown = true;
            AllowRealDeath = false;
            SpectateTargetId = -1;
            _stuckTimer = 0f;

            // 倒地瞬间就把速度清掉，别让坠落/击退速度在 Corgi 里挂一整段观战时间
            FreezeLocalBody();

            var ps = PS;
            var go = ps != null ? ps.CachedLocalPlayerObject : null;
            if (go != null)
            {
                // v1.6.2：优先用 Corgi 记录的"最后站立位置"。死亡瞬间的坐标可能在
                // 坑底/空中/尖刺里 —— 用它复活会立刻再死一次，无限循环的表现就是
                // "死了之后一直没有站起来"。
                var pcDown = ps != null ? ps.LocalPlayer as PlayerController : null;
                try
                {
                    if (DeathPos != Vector3.zero) LastAlivePos = DeathPos;
                    else
                    {
                        var lsp = (pcDown != null && pcDown.ControllerCorgi != null)
                            ? pcDown.ControllerCorgi.LastStandingPosition
                            : Vector3.zero;
                        LastAlivePos = lsp != Vector3.zero ? lsp : go.transform.position;
                    }
                }
                catch { LastAlivePos = go.transform.position; }
                // 关键：把角色整个停掉。游戏靠 Update 里的血量检查推进死亡流程，
                // 对象不 active 就不会有人去开死亡界面。
                go.SetActive(false);
            }

            // 别让房间里的怪物对着尸体一直输出
            ClearEnemyAggro();
            _aggroClearTimer = 0f;
            _roomCheckTimer = 0f;
            ClearSpectateRoomSelection();

            var target = EnsureTarget();
            if (target != null) Follow(target.transform);

            // v2.4.1：立刻把"我倒了"发出去，不等 0.3s 心跳。
            // 全灭判定就靠这个状态，晚 1 秒都可能让另一端多打一轮。
            try
            {
                var nm = NetworkManager.Instance;
                if (nm != null) nm.SendDownedNow(true);
            }
            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] 广播倒地状态失败: " + ex.Message); }

            Plugin.Log.LogInfo("[RL2MP] [DOWN] local player DOWNED (" + reason + ") at ("
                + LastAlivePos.x.ToString("F1") + "," + LastAlivePos.y.ToString("F1") + "). "
                + "remoteAlive=" + AliveRemoteCount + "/" + RemoteCount
                + " spectating=" + (target != null ? target.name : "<none>"));
        }

        /// <summary>
        /// v1.7.2：选出复活坐标。
        ///  1) 死亡坐标在房间内 -> 就用它（用户要的"死在哪儿、活在哪"）；
        ///     但如果最后站稳点离得很近（&lt;3m），用站稳点，避免正好复活在尖刺尖上/墙里。
        ///  2) 死亡坐标在房间外（掉坑、掉虚空）-> 用最后站稳点；它也不在房间内则钳进房间。
        /// </summary>
        private static Vector3 PickRespawnPos(BaseRoom room, Vector3 deathPos)
        {
            Vector3 pos = deathPos;

            bool deathInside = InsideRoom(room, deathPos, 0.5f);
            if (deathInside)
            {
                Vector3 g = TrackedGroundedPos;
                if (g != Vector3.zero && InsideRoom(room, g, 0.5f))
                {
                    float dx = Mathf.Abs(g.x - deathPos.x);
                    float dy = Mathf.Abs(g.y - deathPos.y);
                    if (dx <= 3f && dy <= 3f) pos = g;
                }
            }
            else
            {
                Vector3 g = TrackedGroundedPos;
                if (g != Vector3.zero && InsideRoom(room, g, 0.5f))
                {
                    pos = g;
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 死亡点不在房间内，改用最后站稳点 ("
                        + g.x.ToString("F1") + "," + g.y.ToString("F1") + ")。");
                }
                else if (room != null)
                {
                    pos = ClampIntoRoom(room, deathPos);
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 没有可用的站稳点，把死亡点钳进房间 ("
                        + pos.x.ToString("F1") + "," + pos.y.ToString("F1") + ")。");
                }
            }

            return room != null ? ClampIntoRoom(room, pos) : pos;
        }

        private static bool InsideRoom(BaseRoom room, Vector3 worldPos, float margin)
        {
            if (room == null) return false;
            try
            {
                Bounds b = room.Bounds;
                if (b.size.x <= 0.01f || b.size.y <= 0.01f) return false;
                return worldPos.x >= b.min.x + margin && worldPos.x <= b.max.x - margin
                    && worldPos.y >= b.min.y + margin && worldPos.y <= b.max.y - margin;
            }
            catch { return false; }
        }

        private static Vector3 ClampIntoRoom(BaseRoom room, Vector3 worldPos)
        {
            Vector3 pos = worldPos;
            try
            {
                Bounds b = room.Bounds;
                const float margin = 1.5f;
                if (b.size.x > 3f && b.size.y > 3f)
                {
                    pos.x = Mathf.Clamp(pos.x, b.min.x + margin, b.max.x - margin);
                    pos.y = Mathf.Clamp(pos.y, b.min.y + margin, b.max.y - margin);
                }
            }
            catch { }
            return pos;
        }

        /// <summary>
        /// v1.6.1 关键修复：复活时**绝对不能**调 room.PlacePlayerInRoom()。
        /// 反编译确认它的 End 阶段会执行：
        ///   InitializeSpawnControllerInstances() —— 对每个怪刷怪器重新 GetEnemyFromPool()：
        ///     活着的怪已"借出"不在池里 -> 池直接 CreateNew() 再造一只（怪物两倍）；
        ///     死掉的怪已归还池 -> 取回同一个实例 ResetCharacter()（全房间怪物复活）。
        ///   SaveController.OnPlayerEnter_LoadStageData() —— 重载房间存档状态。
        /// 新造出来的怪会覆盖 controller 的 EnemyInstance 引用，旧的那只变成"孤儿"：
        /// 房间关闭时没人把它归还/禁用，它就一直活着跨房间追玩家。
        /// 玩家复活根本没换房间，只需要：原地瞬移 + Corgi 射线复位 + 兜底激活房间。
        /// </summary>
        private static void RespawnInCurrentRoom(PlayerController pc, GameObject go, Vector3 worldPos)
        {
            BaseRoom room = null;
            try { room = pc != null ? pc.CurrentlyInRoom : null; } catch { }
            // CurrentlyInRoom 丢失时按坐标反查房间，否则后续门触发器/相机都无从对齐
            if (room == null)
            {
                try { room = FindRoomContaining(worldPos); } catch { }
            }

            Vector3 pos = worldPos;
            bool usedDeath = true;
            try
            {
                Vector3 picked = PickRespawnPos(room, worldPos);
                usedDeath = (picked - worldPos).sqrMagnitude < 0.01f;
                pos = picked;
            }
            catch { }

            Plugin.Log.LogInfo("[RL2MP] [DOWN] 复活落点 (" + pos.x.ToString("F1") + "," + pos.y.ToString("F1")
                + ") 来源=" + (usedDeath ? "死亡点" : "最后站稳点")
                + " 传入=(" + worldPos.x.ToString("F1") + "," + worldPos.y.ToString("F1") + ")"
                + " 房间=" + (room != null ? room.name : "<none>"));

            // 走游戏自己的 EnterRoom：只负责 CurrentlyInRoom / LastStandingPosition /
            // TimeEnteredRoom / 房间激活 / 射线复位协程，不会重刷怪（那是 PlacePlayerInRoom 的副作用）。
            // 必须在 EnterRoom 之前记录：EnterRoom 会把关闭的房间 SetActive(true)。
            bool wasActive = true;
            try { wasActive = room != null && room.gameObject.activeInHierarchy; } catch { }

            bool entered = false;
            if (room != null)
            {
                try
                {
                    pc.EnterRoom(room, null, room.transform.InverseTransformPoint(pos));
                    entered = true;
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning("[RL2MP] [DOWN] EnterRoom 失败（退回直接瞬移）: " + ex.Message);
                }
            }
            if (!entered) go.transform.position = pos;

            // v1.7.3 关键修复：EnterRoom 只做"瞬移 + 记房间"，不广播"玩家进入房间"事件，
            // 而门的出口触发器恰恰只在收到该事件时才 SetTriggerActive(true)。
            // 不补这一下，只要有一次换房被打断（门口死亡/瞬移/掉坑），全场门都会停在
            // 关闭状态 —— 玩家站在门口按方向键毫无反应，就是"无法进入下一个房间"。
            //
            // v1.7.4：如果房间本来已经被 SetActive(false)（在门口/换房瞬间死亡就会这样），
            // EnterRoom 会把它重新激活，但**不会**借出敌人、不会切相机、不会广播事件 ——
            // 这正是"回到有怪的房间：视角动不了 + 怪全没了"的成因。这种情况必须把
            // PlacePlayerInRoom_End 的剩余步骤补完（怪/背景/存档/相机/事件）。
            if (room != null)
            {
                if (!wasActive)
                {
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 复活时房间 '" + room.name
                        + "' 处于关闭状态（怪已被回收），补完房间初始化。");
                    RoomFix.CompleteRoomActivation(room, pos, true);
                }
                else
                {
                    RoomFix.MarkEntered(room, null);
                }

                // 复活后重新体检一次（相机 + 是否有怪被回收成空房间）
                RoomFix.RequestVerify();
            }

            // 清零速度与外力 —— 这是"复活后移动速度过快、掉出空气墙"的根因：
            // 死亡瞬间角色可能带着坠落速度（实测日志里 vy 到过 -38）、击退或虚空冲刺速度，
            // 不清掉就会在复活后被保留，一个物理步就能穿过地板/空气墙，门触发器同样会被隧穿。
            try { pc.SetVelocity(0f, 0f, false); } catch { }
            try { if (pc.ControllerCorgi != null) pc.ControllerCorgi.SetForce(Vector2.zero); } catch { }
            try { pc.ExternalKnockbackMod = Vector2.one; pc.InternalKnockbackMod = Vector2.one; } catch { }
            try
            {
                if (pc.CharacterDash != null && pc.CharacterDash.IsDashing)
                    pc.CharacterDash.StopDash();
            }
            catch { }

            try
            {
                if (pc.ControllerCorgi != null) pc.ControllerCorgi.SetRaysParameters();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] Corgi 射线复位失败（已瞬移，仅射线未刷新）: " + ex.Message);
            }

            // 相机对齐房间：复活点可能在房间边缘，不刷新的话相机会停在上一段的边界
            try
            {
                if (room != null)
                    CameraController.UpdateRoomCameraSettings(new RoomViaDoorEventArgs(room, null));
            }
            catch { /* 相机失败不影响复活 */ }

            // 复活后 2 秒内不对门口做"眩晕/禁移"拦截：门判定里 ConditionState=Stunned 或
            // MovementDisabled 会把玩家钳回门口、拒绝换房，而刚复活那一两帧状态未必干净。
            try
            {
                pc.DisableDoorBlock = true;
                _doorBlockUntil = Time.unscaledTime + 2f;
            }
            catch { }
        }

        /// <summary>退出观战时把观战期间手动激活的房间关回去（玩家当前所在房间除外）。</summary>
        private static void RestoreSpectateRooms(PlayerController pc)
        {
            BaseRoom current = null;
            try { current = pc != null ? pc.CurrentlyInRoom : null; } catch { }

            for (int i = 0; i < _activatedRooms.Count; i++)
            {
                var r = _activatedRooms[i];
                if (r == null) continue;
                if (r == current) continue;
                try
                {
                    if (r.gameObject.activeSelf)
                    {
                        r.gameObject.SetActive(false);
                        Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战房间已恢复关闭: '" + r.name + "'");
                    }
                }
                catch { /* 场景可能已在卸载，忽略 */ }
            }
            _activatedRooms.Clear();
            _spectateInitRooms.Clear();   // v2.8.7：新一段观战允许重新初始化房间
        }

        internal static void Revive(string reason)
        {
            if (!LocalDown) return;
            LocalDown = false;
            // 复活后必须关掉真死放行，否则下一次死亡会直接走游戏原生死亡（回城堡）
            AllowRealDeath = false;
            _realDeathAttempts = 0;
            _realDeathRetryAt = 0f;
            SpectateTargetId = -1;
            ClearSpectateRoomSelection();

            // v2.4.1：站起来也要告诉对端，否则对面一直把我当尸体，永远不判全灭
            try
            {
                var nm = NetworkManager.Instance;
                if (nm != null) nm.SendDownedNow(false);
            }
            catch { }

            // v2.8.8：主机倒地期间 BossProgressSync.HostTick 停播了权威位图；
            // 复活这一刻必须强制重发当前 mask，否则客机可能还停在"主机倒地前的旧位图"上，
            // 与主机当前 Boss 门状态不一致 → 进出门口出错。
            // _lastSentMask 重置为 MaxValue 会让下一拍 HostTick 无条件广播一次。
            try { BossProgressSync.ResetHost(); } catch { }

            var ps = PS;
            var go = ps != null ? ps.CachedLocalPlayerObject : null;
            if (go != null)
            {
                if (!go.activeSelf) go.SetActive(true);

                var pc = ps.LocalPlayer as PlayerController;
                if (pc != null)
                {
                    // 必须回满血：血量还是 0 的话游戏下一帧又会尝试杀角色，
                    // 直接进"复活 -> 立刻再倒地"死循环。
                    try
                    {
                        // v1.7.4：观战/真实死亡路径会真的置 IsDead，之后门全部失效（PlayerExit 首行就 return）
                        pc.ResetIsDead();
                        pc.SetHealth(pc.ActualMaxHealth, additive: false, runEvents: true);
                        Plugin.Log.LogInfo("[RL2MP] [DOWN] 复活回满血: " + pc.ActualMaxHealth);
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogWarning("[RL2MP] [DOWN] 复活回血失败: " + ex.Message);
                    }

                    // 关键：复位被死亡拦截卡住的状态。血量归零的那次伤害会把角色
                    // 打进眩晕/禁止移动状态，正常死亡流程会重置它们，但我们把
                    // KillCharacter 拦掉了 —— 不复位的话角色冻在原地、门也不触发
                    // （Door.GetOnTriggerStayExitRoom 见到 Stunned/MovementDisabled 直接拒绝换房）。
                    try
                    {
                        pc.ResetStates();
                        Plugin.Log.LogInfo("[RL2MP] [DOWN] 已复位角色状态（ConditionState/眩晕/翻转锁）");
                    }
                    catch (Exception ex)
                    {
                        Plugin.Log.LogWarning("[RL2MP] [DOWN] 复位角色状态失败: " + ex.Message);
                    }
                }

                // v1.6.1：原地瞬移复活，不再走 PlacePlayerInRoom（会重刷怪物/复活死怪）
                RespawnInCurrentRoom(pc, go, LastAlivePos);

                // v1.7.2：观战期间尸体被隐藏，复活后把怪物重新锁回玩家（否则一辈子不打人）
                _aggroSuppressionUntil = 0f;
                RebindEnemyTargets();
                // 观战期间激活过的房间全部关回去（防止那些房间的怪跨房间追人）
                RestoreSpectateRooms(pc);
                Follow(go.transform);
            }

            Plugin.Log.LogInfo("[RL2MP] [DOWN] local player REVIVED (" + reason + ").");
        }

        /// <summary>
        /// 竞速模式专用：死亡即刻原地复活，不去观战、不换镜头。
        /// 加时记账由 Race.OnLocalDeath 负责，这里只负责把角色"修好"并放回原位：
        /// 回满血 + 复位状态机（否则会冻住、门不触发）+ 房间重置（Corgi 射线复位，防穿地板）。
        /// </summary>
        internal static void QuickRespawn(string reason)
        {
            // 已经死过一次的残留状态一律清掉：AllowRealDeath 如果还留着 true，
            // 之后每一次死亡都会直接放行成真死（"客机死亡后直接结束/回城堡"就是这个）。
            AllowRealDeath = false;
            LocalDown = false;

            var ps = PS;
            var go = ps != null ? ps.CachedLocalPlayerObject : null;
            var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
            if (go == null || pc == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] QuickRespawn: 没有本地玩家引用，跳过。");
                return;
            }

            if (!go.activeSelf) go.SetActive(true);

            // v1.7.2：复活点用"死亡瞬间坐标"（拦截 KillCharacter 时记下的），
            // 只有死亡点在房间外（掉坑/虚空）才退回最后站稳点 —— 见 PickRespawnPos。
            // 旧版直接用 Corgi.LastStandingPosition，而它进房间后常常长时间不刷新，
            // 第一次死亡会把玩家送回房间入口，第二次才"正常"。
            Vector3 pos = DeathPos;
            if (pos == Vector3.zero) pos = go.transform.position;

            // v1.7.4：BaseRoom.PlayerExit 的第一行是 if (playerController.IsDead) return;
            // 只回血 + ResetStates 不会清 IsDead —— 人看着活着，但**所有门永久失效**，
            // 就是"客机无法进入下一个房间"。必须显式复位。
            try { pc.ResetIsDead(); }
            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] QuickRespawn 清 IsDead 失败: " + ex.Message); }

            try { pc.SetHealth(pc.ActualMaxHealth, additive: false, runEvents: true); }
            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] QuickRespawn 回血失败: " + ex.Message); }

            try { pc.ResetStates(); }
            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] QuickRespawn 复位状态失败: " + ex.Message); }

            // v1.6.1：原地瞬移复活，不再走 PlacePlayerInRoom（会重刷怪物/复活死怪/造孤儿怪）
            RespawnInCurrentRoom(pc, go, pos);

            // 短暂无敌：避免站在怪堆里被连续判定死亡、加时刷屏
            try { pc.CharacterHitResponse.SetInvincibleTime(1.5f, additive: false, playInvincibility: false); }
            catch { /* 无敌只是保险，失败无碍 */ }

            // v1.7.2：不再清空敌人 Target（清空后游戏没有任何地方会重新索敌 ->
            // "怪不打人、也不跟人走"）。改为把丢了目标的怪重新锁回本地玩家。
            _aggroSuppressionUntil = 0f;
            RebindEnemyTargets();
            Race.OnLocalDeath();

            Plugin.Log.LogInfo("[RL2MP] [DOWN] 竞速死亡 -> 立即原地复活 (" + reason + ")，成绩加时。");
        }

        /// <summary>全员倒下 -> 放行真实死亡，让游戏正常走回城流程。</summary>
        internal static void TriggerRealDeath()
        {
            if (!AllowRealDeath)
            {
                AllowRealDeath = true;
                _realDeathAttempts = 0;
                _realDeathRetryAt = 0f;

                Plugin.Log.LogInfo("[RL2MP] [DOWN] ALL PLAYERS DOWN -> letting the real death through.");

                ClearSpectateRoomSelection();
                RestoreSpectateRooms(null);

                // 角色必须重新 active：GoDown 里把整个人 SetActive(false) 了，
                // 原生 KillCharacter 会遍历 Animator 参数 / 停能力 / 派发事件，
                // 对象处于 inactive 时其中一步抛异常就会让"死亡窗口"永远打不开。
                try
                {
                    var ps0 = PS;
                    var go0 = ps0 != null ? ps0.CachedLocalPlayerObject : null;
                    if (go0 != null && !go0.activeSelf) go0.SetActive(true);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning("[RL2MP] [DOWN] 重新激活角色失败: " + ex.Message);
                }
            }

            TryFireRealDeath();
        }

        /// <summary>
        /// v2.4.1：真正去触发原生死亡，并且**可重试**。
        ///
        /// 上一版的问题：catch 之后直接 `LocalDown = false`，把唯一的接管通道关掉，
        /// 而死亡窗口根本没打开 —— 表现就是"客机卡死在游戏里，既不能动也不能回城"。
        /// 现在失败会保留观战状态并定时重试，三级兜底：
        ///   1) 原生 KillCharacter；
        ///   2) 手动打开 PlayerDeath 窗口；
        ///   3) 直接切 Lineage。
        /// </summary>
        private static void TryFireRealDeath()
        {
            _realDeathAttempts++;
            _realDeathRetryAt = 0f;

            var ps = PS;
            if (ps == null || ps.LocalPlayer == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] all down but no local player ref; cannot trigger real death.");
                _realDeathRetryAt = Time.unscaledTime + 0.5f;
                return;
            }

            if (IsDeathWindowOpen())
            {
                FinishRealDeath("already open");
                return;
            }

            try
            {
                var m = AccessTools.Method(ps.LocalPlayer.GetType(), "KillCharacter");
                if (m != null)
                {
                    var ps2 = m.GetParameters();
                    object[] args = ps2.Length == 2 ? new object[] { null, true } : new object[ps2.Length];
                    m.Invoke(ps.LocalPlayer, args);
                }
                else
                {
                    Plugin.Log.LogWarning("[RL2MP] [DOWN] KillCharacter not found on " + ps.LocalPlayer.GetType().Name);
                }
            }
            catch (Exception ex)
            {
                // 反射调用抛的是 TargetInvocationException，真正的原因藏在 InnerException。
                // 上一版只打了 ex.Message（"Exception has been thrown by the target of an invocation."），
                // 等于什么都没说，这才导致这次卡死查不出根因。
                Plugin.Log.LogWarning("[RL2MP] [DOWN] trigger real death failed (attempt "
                    + _realDeathAttempts + "): " + DescribeException(ex));
            }

            if (IsDeathWindowOpen())
            {
                FinishRealDeath("KillCharacter");
                return;
            }

            // 兜底 1：原生流程可能已经把 IsDead 设成 true，但窗口那一步没走到
            ForceOpenDeathWindow();
            if (IsDeathWindowOpen())
            {
                FinishRealDeath("forced window");
                return;
            }

            // 兜底 2：死亡窗口彻底打不开 -> 直接回 Lineage，至少不把人锁在空白世界里
            if (_realDeathAttempts >= 3)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] 三次尝试仍未进入死亡流程 -> 强制切换到 Lineage。");
                HardReturnToLineage();
                FinishRealDeath("hard lineage");
                return;
            }

            _realDeathRetryAt = Time.unscaledTime + 0.6f;
            Plugin.Log.LogWarning("[RL2MP] [DOWN] 死亡流程未启动，0.6s 后重试（第 "
                + _realDeathAttempts + " 次）。观战保持，不会卡死。");
        }

        private static void FinishRealDeath(string how)
        {
            _realDeathRetryAt = 0f;
            LocalDown = false;
            CoOpRescue.LocalDown = false;
            SpectateTargetId = -1;
            Plugin.Log.LogInfo("[RL2MP] [DOWN] 已进入原生死亡流程（" + how + "），角色交还给游戏。");
        }

        /// <summary>v2.4.1：收到对端"本局结束"通知。只在本端也已倒下时跟随，避免误杀活人。</summary>
        internal static void OnRunOverReceived(int fromId)
        {
            if (Score.Style != PlayStyle.CoOp) return;

            bool meDown = LocalDown || IsDeathWindowOpen();
            if (!meDown)
            {
                Plugin.Log.LogWarning("[RL2MP] [RUNOVER] 对端(id=" + fromId
                    + ")报告全员倒下，但本端仍在存活状态 -> 忽略（不杀活人）。");
                return;
            }

            Plugin.Log.LogInfo("[RL2MP] [RUNOVER] 跟随对端结束本局（id=" + fromId + "）。");
            // v2.8.8：闭锁本端全灭判定，避免对端先真死的 downed=0 反弹把计时清掉后自己卡住。
            CoOpRescue.OnRunOver();
            CoOpRescue.LocalDown = false;
            TriggerRealDeath();
        }

        private static bool IsDeathWindowOpen()
        {
            try
            {
                var t = AccessTools.TypeByName("RL_Windows.WindowManager");
                if (t == null) return false;
                var m = t.GetMethod("GetIsWindowOpen", new Type[] { typeof(WindowID) });
                if (m == null) return false;
                object r = m.Invoke(null, new object[] { WindowID.PlayerDeath });
                return r is bool && (bool)r;
            }
            catch { return false; }
        }

        private static void ForceOpenDeathWindow()
        {
            try
            {
                var t = AccessTools.TypeByName("RL_Windows.WindowManager");
                if (t == null) return;
                var m = t.GetMethod("SetWindowIsOpen", new Type[] { typeof(WindowID), typeof(bool) });
                if (m == null) return;
                m.Invoke(null, new object[] { WindowID.PlayerDeath, true });
                Plugin.Log.LogInfo("[RL2MP] [DOWN] 已手动打开 PlayerDeath 窗口（兜底）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] 手动打开死亡窗口失败: " + DescribeException(ex));
            }
        }

        private static void HardReturnToLineage()
        {
            try
            {
                try { SaveManager.PlayerSaveData.IsDead = true; } catch { }
                SceneLoader_RL.LoadScene(SceneID.Lineage, TransitionID.FadeToBlackWithLoading);
                Plugin.Log.LogWarning("[RL2MP] [DOWN] 已强制切场景 -> Lineage。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] 强制切 Lineage 失败: " + DescribeException(ex));
            }
        }

        /// <summary>把异常链（含 TargetInvocationException 的内层）拼成可读文本。</summary>
        private static string DescribeException(Exception ex)
        {
            var sb = new System.Text.StringBuilder();
            int depth = 0;
            while (ex != null && depth < 4)
            {
                if (depth > 0) sb.Append(" <- ");
                sb.Append(ex.GetType().Name).Append(": ").Append(ex.Message);
                if (!string.IsNullOrEmpty(ex.StackTrace))
                    sb.Append("\n    ").Append(ex.StackTrace.Replace("\n", "\n    "));
                ex = ex.InnerException;
                depth++;
            }
            return sb.ToString();
        }

        internal static void ResetAll(string reason)
        {
            if (LocalDown || AllowRealDeath)
            {
                Plugin.Log.LogInfo("[RL2MP] [DOWN] reset (" + reason + ")");
            }
            LocalDown = false;
            AllowRealDeath = false;
            _realDeathAttempts = 0;
            _realDeathRetryAt = 0f;
            SpectateTargetId = -1;
            _vcam = null;
            _lastFollow = null;
            ClearSpectateRoomSelection();
            _aggroSuppressionUntil = 0f;
            _freezeTimer = 0f;
            // 换场景 = 换楼层，旧的死亡点/落脚点在新区里没有意义
            DeathPos = Vector3.zero;
            TrackedGroundedPos = Vector3.zero;
            RoomFix.Reset(reason);

            // 会话重置：观战期间激活的房间全部关回去
            RestoreSpectateRooms(null);

            // v1.7.8：协作倒地/血上限惩罚一并清理（换场景=换楼层/回城，属于新的一段）。
            CoOpRescue.ResetAll(reason);
            // v1.7.9：共享血量的防循环标志一并清理。
            EnemySync.ResetAll();
            // v1.8.4：冲刺特效豁免集合一并清理。
            DashFxSync.ResetAll();
            // v2.0.0：怪物权威位置字典一并清理（换场景=全新一层，旧 key 不再适用）。
            EnemyPositionSync.ResetAll();
            EnemyAliveSync.ResetAll();  // v2.5.0：存活权威集（换场景全新）
            ObstacleSync.ResetAll();    // v2.5.0：障碍物位置字典
        }

        /// <summary>调试用：把所有远程玩家标记为倒地，用来验证"全员倒下 -> 真实死亡"。</summary>
        internal static void DebugDownAllRemotes()
        {
            var ps = PS;
            if (ps == null) return;
            foreach (var kv in ps.RemotePlayers)
            {
                if (kv.Value == null) continue;
                ps.SetRemoteDowned(kv.Key, true);
            }
            Plugin.Log.LogInfo("[RL2MP] [DOWN] debug: all remotes marked down. alive="
                + AliveRemoteCount + "/" + RemoteCount);
        }

        /// <summary>每帧被 DownedManager 调用：维持镜头 + 检查是否该真死。</summary>
        internal static void Tick()
        {
            // 复活后临时关闭门口拦截，时间到了自动恢复
            if (_doorBlockUntil > 0f && Time.unscaledTime > _doorBlockUntil)
            {
                _doorBlockUntil = 0f;
                try
                {
                    var ps0 = PS;
                    var pc0 = ps0 != null ? ps0.LocalPlayer as PlayerController : null;
                    if (pc0 != null) pc0.DisableDoorBlock = false;
                }
                catch { }
            }

            // 倒地期间持续冻结尸体物理：角色还活着（我们拦掉了真死），重力会一直给
            // Corgi 加速，实测死亡几秒后 vy 能累到 -40。复活时若不清零，角色会以这个
            // 速度穿地板、穿空气墙出去，门触发器也来不及响应 —— 就是"复活后速度很快"
            // 的直接来源。这里每 0.1s 清一次，从源头掐断。
            if (LocalDown)
            {
                _freezeTimer += Time.unscaledDeltaTime;
                if (_freezeTimer >= 0.1f)
                {
                    _freezeTimer = 0f;
                    FreezeLocalBody();
                }
            }

            // v1.6.2：复活保护窗。QuickRespawn 只在起立瞬间清一次仇恨，
            // 怪物 1.5s 无敌一过就重新索敌 -> 刚站起又被围殴秒杀，看起来像
            // "复活了但马上又死"。保护窗内每 0.5s 再清一次。
            // v2.4.0：观战期间不走这段（此时仇恨已经转给活着的队友，清掉反而让怪发呆）。
            if (!LocalDown && Time.unscaledTime < _aggroSuppressionUntil)
            {
                _aggroClearTimer += Time.unscaledDeltaTime;
                if (_aggroClearTimer >= 0.5f)
                {
                    _aggroClearTimer = 0f;
                    ClearEnemyAggro();
                }
                return;
            }

            if (!LocalDown) return;

            // v2.4.0：协作模式也走同一套观战维护（镜头跟随队友 / 切换目标 / 激活观战房间）。
            // 旧版这里对协作 early-return，因为当时的协作是"倒地待救"，镜头要钉在自己尸体上；
            // 扶起机制删除后，协作死亡 = 观战，必须让下面这段跑起来才能看到队友和怪物。
            var target = EnsureTarget();
            if (target != null)
            {
                _stuckTimer = 0f;

                // 目标每帧可能换人或者被销毁，这里持续纠正
                Follow(target.transform);

                // 主机换房间 -> 客机端激活对应房间 + 相机边界跟过去（每 0.5s 查一次，别每帧扫）
                _roomCheckTimer += Time.unscaledDeltaTime;
                // v3.0.4：权威易手后立刻强制重解析一次，不等 0.5s 节流。
                bool forceRoom = ForceSpectateRoomRefresh;
                if (forceRoom) ForceSpectateRoomRefresh = false;
                if (_roomCheckTimer >= 0.5f || forceRoom)
                {
                    _roomCheckTimer = 0f;
                    // v2.8.6：换房包的权威坐标优先。EnemyAliveSync.HostRoomCoords 只在
                    // 客机端计算（主机广播、客机接收），所以主机倒地观战客机时它永远是
                    // 无效的 —— 只能拿分身坐标猜房间，再被 bestActive 偏好锁在旧房间，
                    // 表现就是"客机去了主机没探索的房间，主机观战全黑"。
                    Vector2? hostGrid = RoomTravelSync.HasRemoteRoom
                        ? (Vector2?)RoomTravelSync.RemoteRoomCoords
                        : (EnemyAliveSync.HasHostRoomCoords ? (Vector2?)EnemyAliveSync.HostRoomCoords : null);
                    Vector2? hostAnchor = RoomTravelSync.HasRemoteAnchor
                        ? (Vector2?)RoomTravelSync.RemoteRoomAnchor
                        : null;
                    Vector3 roomProbe = RoomTravelSync.HasRemoteRoom
                        ? RoomTravelSync.RemoteRoomLanding
                        : (EnemyAliveSync.HasHostAliveCenter
                            ? EnemyAliveSync.HostAliveCenter
                            : target.transform.position);

                    // v3.0.13：真机铁证 [AUTHROOM] 权威坐标=(0,0) → FindRoomByGridCoords((0,0))
                    // 命中 '1*Transition*' 过渡房（Bounds 异常）→ confiner 锁错边界 = 半黑屏，
                    // 且全程卡在过渡房。权威网格/锚点为 (0,0) 时一律视为无效。
                    // 观战目标分身的世界坐标是准的（日志 [INTERP] current=(61,7) 等），
                    // 用它做落点 probe，FindRoomContaining 就能命中分身真实所在房。
                    // 注意：分身是 StripClone 后的纯视觉克隆，没有 PlayerController，
                    // 因此不能读 CurrentlyInRoom，只能用世界坐标反查。
                    if (hostGrid.HasValue && Mathf.Abs(hostGrid.Value.x) + Mathf.Abs(hostGrid.Value.y) < 0.05f)
                        hostGrid = null;
                    if (hostAnchor.HasValue && Mathf.Abs(hostAnchor.Value.x) + Mathf.Abs(hostAnchor.Value.y) < 0.05f)
                        hostAnchor = null;
                    Vector3 spectateProbe = target.transform.position;
                    EnsureSpectateRoom(spectateProbe, hostGrid, hostAnchor);
                }

                // v2.8.3：每当房间切换，游戏会激活新房间的专属 vcam 并把 Follow 改回本地玩家
                //（已隐藏的尸体）。旧版 1 秒才纠一次，中间画面跟着尸体/confiner 锁旧边界 →
                // 半屏、失真、黑屏。改为每帧重设到观战目标（只写一个引用，无日志、开销极小）。
                FollowSilent(target.transform);
            }
            else
            {
                // 一个能观战的人都没有（队友全退了）：别让人永远躺在观战里。
                _stuckTimer += Time.unscaledDeltaTime;
                if (_stuckTimer >= 8f)
                {
                    _stuckTimer = 0f;
                    if (Score.Style == PlayStyle.CoOp)
                    {
                        // 协作：队友全退了，本局作废 —— 自己走原生死亡回城，而不是原地复活
                        // （复活会变成"一个人继续打"，进度和结算都说不清）。
                        Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战目标全部离开（协作模式）-> 结束本局。");
                        CoOpRescue.LocalDown = false;
                        TriggerRealDeath();
                    }
                    else
                    {
                        Revive("no one left to spectate");
                    }
                    return;
                }
            }

            _logTimer += Time.unscaledDeltaTime;
            if (_logTimer >= 5f)
            {
                _logTimer = 0f;
                Plugin.Log.LogInfo("[RL2MP] [DOWN] spectating id=" + SpectateTargetId
                    + " aliveRemote=" + AliveRemoteCount + "/" + RemoteCount
                    + " target=" + (target != null ? target.name : "<none>")
                    + " room=" + (_spectatedRoom != null ? _spectatedRoom.name : "<none>"));
            }

            // v2.4.0：**不再**定期清仇恨。
            // 旧版每 2 秒把怪的 Target 清成 null（防"怪对着尸体打"）。但扶起机制删除后
            // 仇恨会由 EnemyAiFix 转给活着的队友，定时清空只会让怪反复"发呆 → 重新锁定"，
            // 在画面上又是一轮抽搐。清理动作现在只在 GoDown / 复活窗里做一次。

            if (!InSession)
            {
                // 主机断线/结束对局时自己还躺着：没人可观战，尽快站起来，
                // 否则角色一直隐藏、视角跟着已经不存在的分身，游戏像卡死一样。
                _stuckTimer += Time.unscaledDeltaTime;
                if (_stuckTimer >= 2f)
                {
                    _stuckTimer = 0f;
                    Revive("network lost");
                }
                return;
            }

            // v2.4.1：全灭判定统一交给 CoOpRescue（1.5s 确认 + 广播 RunOver）。
            // 旧版这里还有一条"RemoteCount>0 && AliveRemoteCount==0 立刻真死"，
            // 会和 CoOpRescue 的确认窗打架：一端刚收到 downed 状态就立刻杀进程，
            // 另一端还没收到结束通知，两端状态就分叉了。
            // 这里只负责把上一步失败的"真实死亡"重试推进。
            if (AllowRealDeath && _realDeathRetryAt > 0f && Time.unscaledTime >= _realDeathRetryAt)
            {
                TryFireRealDeath();
            }
        }
    }

    /// <summary>挂在 RL2MP_Runtime 上，只负责每帧驱动 DownedState 和按键。</summary>
    public class DownedManager : MonoBehaviour
    {
        private void Awake()
        {
            // v3.0.22：登记为稳定宿主，供 BiomeRoomBuilder 跑原生协程。
            DownedState.HostBehaviour = this;
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            // 换场景 = 重新进城堡/回城，倒地状态不应该跨场景保留
            DownedState.ResetAll("sceneLoaded:" + scene.name);
        }

        private void Update()
        {
            DownedState.TrackBody();
            DownedState.Tick();
            RoomFix.Tick();
            CoOpRescue.Tick();

            // v2.4.0：协作模式死亡也是观战了，[ ] 切换必须照常可用。
            // 旧版这里对 CoOpRescue.LocalDown 直接 return（那时候协作是"倒地待救"，
            // 镜头钉在自己尸体上，没有切换的必要），删掉扶起后就变成了"观战但换不了目标"。
            if (!DownedState.LocalDown) return;

            if (Input.GetKeyDown(KeyCode.LeftBracket))
            {
                var t = DownedState.CycleTarget(-1);
                Plugin.Log.LogInfo("[RL2MP] [DOWN] cycle(-) -> " + (t != null ? t.name : "<none>"));
            }
            if (Input.GetKeyDown(KeyCode.RightBracket))
            {
                var t = DownedState.CycleTarget(1);
                Plugin.Log.LogInfo("[RL2MP] [DOWN] cycle(+) -> " + (t != null ? t.name : "<none>"));
            }
        }
    }

    /// <summary>
    /// 拦截玩家死亡。
    /// 注意不能只打 BaseCharacterController：PlayerController 重写了 KillCharacter，
    /// 先调 base（置 IsDead）再 Broadcast(EventIDs 44) + 触发 m_onPlayerDeathRelay，
    /// 死亡界面就是被这个事件推走的。所以必须直接拦 PlayerController 的那份重写。
    /// </summary>
    internal static class DownedPatches
    {
        internal static void Apply(Harmony harmony)
        {
            var prefix = new HarmonyMethod(typeof(DownedPatches).GetMethod("KillCharacterPrefix",
                BindingFlags.Static | BindingFlags.NonPublic));

            bool patched = false;
            try
            {
                var m = AccessTools.Method(typeof(PlayerController), "KillCharacter",
                    new Type[] { typeof(GameObject), typeof(bool) });
                if (m != null)
                {
                    harmony.Patch(m, prefix: prefix);
                    patched = true;
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] Patched PlayerController.KillCharacter (downed/spectate).");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] PlayerController patch failed: " + Plugin.PatchError(ex));
            }

            if (!patched)
            {
                try
                {
                    var m = AccessTools.Method(typeof(BaseCharacterController), "KillCharacter",
                        new Type[] { typeof(GameObject), typeof(bool) });
                    if (m != null)
                    {
                        harmony.Patch(m, prefix: prefix);
                        Plugin.Log.LogInfo("[RL2MP] [DOWN] Fallback: patched BaseCharacterController.KillCharacter.");
                    }
                    else
                    {
                        Plugin.Log.LogWarning("[RL2MP] [DOWN] KillCharacter not found; downed mode disabled.");
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning("[RL2MP] [DOWN] BaseCharacterController patch failed: " + Plugin.PatchError(ex));
                }
            }
        }

        private static bool KillCharacterPrefix(BaseCharacterController __instance)
        {
            try
            {
                // 敌人正常死
                if (!(__instance is PlayerController)) return true;

                // 记录死亡坐标：复活点优先用它（"死在原地、原地复活"）。
                // 只有落在房间外（掉坑/虚空）时才退回最后站稳点。
                try { DownedState.DeathPos = __instance.transform.position; } catch { }

                if (!DownedState.InSession)
                {
                    // 放行真实死亡：整局都不在联机会话里（单机 / 未开房）
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 放行真实死亡：不在联机会话中（单机或已开局前）。");
                    return true;
                }
                if (DownedState.AllowRealDeath)
                {
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 放行真实死亡：AllowRealDeath=true（全员倒下/已结束对局）。");
                    return true;
                }

                // v1.7.8：按玩法分流。
                //   协作（CoOp）：不立即复活，留尸体待救 + 血上限惩罚（CoOpRescue）。
                //   竞速（Race）：保持 v1.6.4 的立即原地复活 + 加时。
                // 注意这里两条路都必须拦下真死（return false），否则会走游戏原生死亡回城。
                if (Score.Style == PlayStyle.CoOp)
                {
                    CoOpRescue.OnLocalDown("KillCharacter");
                }
                else
                {
                    // 竞速：联机期间死亡一律立即原地复活。
                    // 非竞速阶段的死亡 QuickRespawn 内部 OnLocalDeath 会因
                    // Phase != Racing 跳过加时记账，不污染成绩。
                    DownedState.QuickRespawn("KillCharacter");
                }
                return false;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] prefix exception: " + ex.Message);
                return true;
            }
        }
    }
}
