using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
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

        /// <summary>倒地瞬间的位置，用于复活时把人放回原地。</summary>
        internal static Vector3 LastAlivePos;

        private static CinemachineVirtualCameraManager _vcam;
        private static Transform _lastFollow;
        private static float _refollowTimer;
        private static float _logTimer;
        private static float _stuckTimer;
        private static float _aggroClearTimer;
        private static float _roomCheckTimer;
        private static BaseRoom _spectatedRoom;
        // 观战期间被我们手动激活的房间（正常游戏里房间进出由门触发器管理，
        // 我们绕过了它，所以退出观战时要自己把这些房间关回去，否则里面的
        // 怪会一直活着跨房间追人 —— 这正是"下个房间的怪跑到上个房间打我"的来源）
        private static readonly List<BaseRoom> _activatedRooms = new List<BaseRoom>();

        internal static bool InSession
        {
            get
            {
                var nm = NetworkManager.Instance;
                if (nm == null) return false;
                return nm.Connected || Plugin.DebugMode;
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

        /// <summary>游戏切房间时会把镜头 Follow 改回玩家，这里定期纠一次，不打日志。</summary>
        private static void FollowSilent(Transform target)
        {
            if (target == null || !EnsureVcam()) return;
            try
            {
                _vcam.SetFollowTarget(target);
            }
            catch (Exception)
            {
                // 忽略：相机可能在切换中
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
                    if (e.Target != null) { e.Target = null; cleared++; }
                }
                if (cleared > 0)
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 清除 " + cleared + " 个敌人的仇恨（玩家已倒地）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] ClearEnemyAggro failed: " + ex.Message);
            }
        }

        // ---------------- 观战房间激活 ----------------

        /// <summary>
        /// RL2 的 World 场景里同一时刻只有一个房间 GameObject 是激活的
        /// （PlayerExit 会把旧房间 SetActive(false)）。主机走进下一个房间后，
        /// 客机本地那个房间根本没激活，镜头跟过去只能看到空场景。
        /// 这里把观战目标所在的房间在客机端激活 + 初始化（敌人/道具/背景/存档状态），
        /// 并让相机边界切过去 —— 等价于游戏的 PlacePlayerInRoom，但不挪动本地玩家。
        /// </summary>
        private static void EnsureSpectateRoom(Vector3 targetPos)
        {
            try
            {
                var room = FindRoomContaining(targetPos);
                if (room == null) return;
                if (_spectatedRoom == room) return;
                _spectatedRoom = room;

                if (!room.gameObject.activeSelf)
                {
                    // 复刻 PlacePlayerInRoom_End 的顺序：种子 -> 激活 -> 敌人/道具 -> 背景
                    try
                    {
                        var seedMethod = AccessTools.Method(room.GetType(), "GetSpecialPropSeed");
                        if (seedMethod != null)
                            RNGManager.SetSeed(RngID.SpecialProps_RoomSeed, (int)seedMethod.Invoke(room, null));
                    }
                    catch { /* 种子只是道具随机，失败无碍 */ }

                    room.gameObject.SetActive(true);
                    _activatedRooms.Add(room);

                    AccessTools.Method(room.GetType(), "InitializeSpawnControllerInstances")?.Invoke(room, null);
                    AccessTools.Method(room.GetType(), "InitializeBackgrounds")?.Invoke(room, null);
                    Plugin.Log.LogInfo("[RL2MP] [DOWN] 观战房间已激活: '" + room.name + "'");
                }

                // 按客机地图的房间状态加载（主机打死的敌人不会复活，开过的箱子保持开过）
                if (room.SaveController != null) room.SaveController.OnPlayerEnter_LoadStageData();

                // 相机边界切到该房间（否则 confiner 还锁在本地玩家房间）
                CameraController.UpdateRoomCameraSettings(new RoomViaDoorEventArgs(room, null));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] EnsureSpectateRoom failed: " + ex.Message);
            }
        }

        /// <summary>找出包含该坐标的房间（含未激活的；世界坐标，x/y 判定，容差 1m）。</summary>
        private static BaseRoom FindRoomContaining(Vector3 pos)
        {
            BaseRoom best = null;
            BaseRoom bestActive = null;
            var rooms = Resources.FindObjectsOfTypeAll<BaseRoom>();
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                if (r == null) continue;
                if (!r.gameObject.scene.IsValid()) continue; // 排除 prefab 资源
                try
                {
                    var b = r.Bounds;
                    Vector3 c = b.center;
                    Vector3 s = b.size;
                    if (Mathf.Abs(pos.x - c.x) > s.x * 0.5f + 1f) continue;
                    if (Mathf.Abs(pos.y - c.y) > s.y * 0.5f + 1f) continue;
                    if (best == null) best = r;
                    if (r.gameObject.activeSelf) bestActive = r;
                }
                catch { /* Bounds 可能在未初始化房间上抛异常 */ }
            }
            return bestActive != null ? bestActive : best;
        }

        // ---------------- 状态切换 ----------------

        internal static void GoDown(string reason)
        {
            if (LocalDown) return;
            LocalDown = true;
            AllowRealDeath = false;
            SpectateTargetId = -1;
            _stuckTimer = 0f;

            var ps = PS;
            var go = ps != null ? ps.CachedLocalPlayerObject : null;
            if (go != null)
            {
                LastAlivePos = go.transform.position;
                // 关键：把角色整个停掉。游戏靠 Update 里的血量检查推进死亡流程，
                // 对象不 active 就不会有人去开死亡界面。
                go.SetActive(false);
            }

            // 别让房间里的怪物对着尸体一直输出
            ClearEnemyAggro();
            _aggroClearTimer = 0f;
            _roomCheckTimer = 0f;
            _spectatedRoom = null;

            var target = EnsureTarget();
            if (target != null) Follow(target.transform);

            Plugin.Log.LogInfo("[RL2MP] [DOWN] local player DOWNED (" + reason + ") at ("
                + LastAlivePos.x.ToString("F1") + "," + LastAlivePos.y.ToString("F1") + "). "
                + "remoteAlive=" + AliveRemoteCount + "/" + RemoteCount
                + " spectating=" + (target != null ? target.name : "<none>"));
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
            go.transform.position = worldPos;

            try
            {
                var room = pc.CurrentlyInRoom;
                if (room != null && !room.gameObject.activeSelf) room.gameObject.SetActive(true);
            }
            catch { /* 房间引用异常不影响瞬移 */ }

            try
            {
                if (pc.ControllerCorgi != null) pc.ControllerCorgi.SetRaysParameters();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] Corgi 射线复位失败（已瞬移，仅射线未刷新）: " + ex.Message);
            }
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
        }

        internal static void Revive(string reason)
        {
            if (!LocalDown) return;
            LocalDown = false;
            SpectateTargetId = -1;
            _spectatedRoom = null;

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

                // 复活瞬间清一次仇恨，避免刚站起就被围殴秒杀
                ClearEnemyAggro();
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
            var ps = PS;
            var go = ps != null ? ps.CachedLocalPlayerObject : null;
            var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
            if (go == null || pc == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] QuickRespawn: 没有本地玩家引用，跳过。");
                return;
            }

            if (!go.activeSelf) go.SetActive(true);
            Vector3 pos = go.transform.position;

            try { pc.SetHealth(pc.ActualMaxHealth, additive: false, runEvents: true); }
            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] QuickRespawn 回血失败: " + ex.Message); }

            try { pc.ResetStates(); }
            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [DOWN] QuickRespawn 复位状态失败: " + ex.Message); }

            // v1.6.1：原地瞬移复活，不再走 PlacePlayerInRoom（会重刷怪物/复活死怪/造孤儿怪）
            RespawnInCurrentRoom(pc, go, pos);

            // 短暂无敌：避免站在怪堆里被连续判定死亡、加时刷屏
            try { pc.CharacterHitResponse.SetInvincibleTime(1.5f, additive: false, playInvincibility: false); }
            catch { /* 无敌只是保险，失败无碍 */ }

            ClearEnemyAggro();
            Race.OnLocalDeath();

            Plugin.Log.LogInfo("[RL2MP] [DOWN] 竞速死亡 -> 立即原地复活 (" + reason + ")，成绩加时。");
        }

        /// <summary>全员倒下 -> 放行真实死亡，让游戏正常走回城流程。</summary>
        internal static void TriggerRealDeath()
        {
            if (AllowRealDeath) return;
            AllowRealDeath = true;

            var ps = PS;
            if (ps == null || ps.LocalPlayer == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOWN] all down but no local player ref; cannot trigger real death.");
                return;
            }

            Plugin.Log.LogInfo("[RL2MP] [DOWN] ALL PLAYERS DOWN -> letting the real death through.");

            _spectatedRoom = null;
            RestoreSpectateRooms(null);
            var go = ps.CachedLocalPlayerObject;
            if (go != null && !go.activeSelf) go.SetActive(true);

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
                Plugin.Log.LogWarning("[RL2MP] [DOWN] trigger real death failed: " + ex.Message);
            }

            LocalDown = false;
        }

        internal static void ResetAll(string reason)
        {
            if (LocalDown || AllowRealDeath)
            {
                Plugin.Log.LogInfo("[RL2MP] [DOWN] reset (" + reason + ")");
            }
            LocalDown = false;
            AllowRealDeath = false;
            SpectateTargetId = -1;
            _vcam = null;
            _lastFollow = null;
            _spectatedRoom = null;

            // 会话重置：观战期间激活的房间全部关回去
            RestoreSpectateRooms(null);
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
            if (!LocalDown) return;

            var target = EnsureTarget();
            if (target != null)
            {
                _stuckTimer = 0f;

                // 目标每帧可能换人或者被销毁，这里持续纠正
                Follow(target.transform);

                // 主机换房间 -> 客机端激活对应房间 + 相机边界跟过去（每 0.5s 查一次，别每帧扫）
                _roomCheckTimer += Time.unscaledDeltaTime;
                if (_roomCheckTimer >= 0.5f)
                {
                    _roomCheckTimer = 0f;
                    EnsureSpectateRoom(target.transform.position);
                }

                _refollowTimer += Time.unscaledDeltaTime;
                if (_refollowTimer >= 1f)
                {
                    _refollowTimer = 0f;
                    FollowSilent(target.transform);
                }
            }
            else
            {
                // 一个能看的人都没有（队友全掉线/全退了）：别让人永远躺在地上
                _stuckTimer += Time.unscaledDeltaTime;
                if (_stuckTimer >= 8f)
                {
                    _stuckTimer = 0f;
                    Revive("no one left to spectate");
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

            // 新激活的房间会刷出新怪并重新锁定玩家，定期再清一遍
            _aggroClearTimer += Time.unscaledDeltaTime;
            if (_aggroClearTimer >= 2f)
            {
                _aggroClearTimer = 0f;
                ClearEnemyAggro();
            }

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

            if (RemoteCount > 0 && AliveRemoteCount == 0)
            {
                TriggerRealDeath();
            }
        }
    }

    /// <summary>挂在 RL2MP_Runtime 上，只负责每帧驱动 DownedState 和按键。</summary>
    public class DownedManager : MonoBehaviour
    {
        private void Awake()
        {
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
            DownedState.Tick();

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
                Plugin.Log.LogWarning("[RL2MP] [DOWN] PlayerController patch failed: " + ex.Message);
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
                    Plugin.Log.LogWarning("[RL2MP] [DOWN] BaseCharacterController patch failed: " + ex.Message);
                }
            }
        }

        private static bool KillCharacterPrefix(BaseCharacterController __instance)
        {
            try
            {
                // 敌人正常死
                if (!(__instance is PlayerController)) return true;

                if (!DownedState.InSession) return true;
                if (DownedState.AllowRealDeath) return true;

                // 竞速中：不做倒地观战，直接原地复活 + 加时
                if (Race.InRacePhase)
                {
                    DownedState.QuickRespawn("KillCharacter");
                    return false;
                }

                // 已经倒地了就不再做任何事（游戏可能每帧都往尸体上补刀）
                if (DownedState.LocalDown) return false;

                DownedState.GoDown("KillCharacter");
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
