using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 房间进出兜底修复（v1.7.3）。
    ///
    /// 反编译确认的门机制（Door / BaseRoom）：
    ///   PlayerExit(door)
    ///     -> BroadcastPlayerExitRoomEvents()  -> 本房间**所有门** OnPlayerExitRoom
    ///        -> m_exitTrigger.SetTriggerActive(false)   ← 出口触发器被关掉
    ///     -> room.PlacePlayerInRoom(connectedDoor)
    ///        -> EnterRoom(...)
    ///        -> PlacePlayerInRoom_End() -> BroadcastPlayerEnterRoomEvents()
    ///           -> 新房间所有门 OnPlayerEnterRoom -> SetTriggerActive(true)  ← 重新打开
    ///
    /// 关键：PlayerController.EnterRoom(room, door, localPos) 只是**瞬移 + 记房间**，
    /// 既不置 room.IsPlayerInRoom，也不广播 PlayerEnterRoom。
    /// 我们复活时为了避开 PlacePlayerInRoom 的重刷怪副作用，走的正是这条路 ——
    /// 于是只要有一次门切换只完成了"退出"没完成"进入"（玩家在门口死亡、被瞬移、
    /// ConnectedDoor 为空提前 return、掉坑打断……），全场门触发器就停在关闭状态：
    /// 玩家贴到门口、按方向键，一点反应都没有。这就是"客机无法进入下一个房间"。
    ///
    /// 本类做四件事：
    ///  1) MarkEntered：复活后补一次"进入房间"广播（+ 兜底直接激活门触发器）；
    ///  2) Tick：每 0.5s 巡检当前房间，把没激活的门触发器重新激活（自愈）；
    ///  3) 越线没换房：玩家已经站到门的越界判定里却迟迟没转场，手动 PlayerExit；
    ///  4) 掉出房间：离开房间 10m 以上持续 3 秒（掉虚空、房卡没了），按摔死处理。
    /// </summary>
    internal static class RoomFix
    {
        private static readonly FieldInfo IsPlayerInRoomField =
            AccessTools.Field(typeof(BaseRoom), "<IsPlayerInRoom>k__BackingField");
        private static readonly FieldInfo ExitTriggerField =
            AccessTools.Field(typeof(Door), "m_exitTrigger");
        private static readonly MethodInfo CreateExitTriggerMethod =
            AccessTools.Method(typeof(Door), "CreateExitTrigger", new Type[0]);
        private static readonly MethodInfo BroadcastEnterMethod =
            AccessTools.Method(typeof(BaseRoom), "BroadcastPlayerEnterRoomEvents", new Type[] { typeof(Door) });

        private static float _tick;
        private static float _offRoomTime;
        private static float _doorHoldTime;
        private static Door _doorHold;
        private static Vector3 _doorHoldPos;
        private static BaseRoom _lastMarkRoom;
        private static BaseRoom _lastSeenRoom;
        private static BaseRoom _verifyRoom;
        private static float _verifyAt;
        private static bool _verifyCameraDone;
        private static int _verifyAttempts;
        private static float _logTime;

        private static void Warn(string msg)
        {
            if (Time.unscaledTime - _logTime < 5f) return;
            _logTime = Time.unscaledTime;
            Plugin.Log.LogWarning("[RL2MP] [ROOM] " + msg);
        }

        // ---------------- 对外：复活 / 进房后补一次"进入房间" ----------------

        /// <summary>
        /// 补一次"玩家已进入该房间"的收尾工作。
        ///
        /// 不走 BroadcastPlayerEnterRoomEvents：那会给全工程 76 个 PlayerEnterRoom 监听者
        /// 广播一次带 door=null 的事件，别人读 args.Door.Side 就会 NRE。
        /// 我们只做真正必要、且完全等价的两步：
        ///   1) room.IsPlayerInRoom = true（PlacePlayerInRoom_Start 做的就是这个）；
        ///   2) 房间每扇门的出口触发器 —— 没有就按 Door.OnPlayerEnterRoom 的方式创建，
        ///      有但被关掉就 SetTriggerActive(true)。
        /// 不用 PlacePlayerInRoom，是因为它会 InitializeSpawnControllerInstances() 重刷怪。
        /// </summary>
        internal static void MarkEntered(BaseRoom room, Door door)
        {
            if (room == null) return;

            set_IsPlayerInRoom(room, true);

            int created = 0, activated = 0;
            try
            {
                var doors = room.Doors;
                if (doors != null)
                {
                    for (int i = 0; i < doors.Count; i++)
                    {
                        var d = doors[i];
                        if (d == null) continue;
                        try
                        {
                            var trig = ExitTriggerField != null ? ExitTriggerField.GetValue(d) as PlayerTrigger : null;
                            if (trig == null && CreateExitTriggerMethod != null)
                            {
                                CreateExitTriggerMethod.Invoke(d, null);
                                created++;
                                trig = ExitTriggerField != null ? ExitTriggerField.GetValue(d) as PlayerTrigger : null;
                            }
                            if (trig == null || trig.IsTriggerActive) continue;
                            trig.SetTriggerActive(true);
                            activated++;
                        }
                        catch { /* 单个门失败不影响其他门 */ }
                    }
                }
            }
            catch (Exception ex) { Warn("MarkEntered: " + ex.Message); }

            if (created > 0 || activated > 0)
                Plugin.Log.LogInfo("[RL2MP] [ROOM] 已补进入房间 '" + room.name
                    + "'：创建 " + created + " 个 / 恢复 " + activated + " 个门的出口触发器。");

            // 相机也要一起对齐：只补门不补相机 = 玩家能在新房间走，但画面还锁在
            // 上一个房间的 confiner 里（"视角动不了"）。
            if (door == null) door = FindNearestDoor(room, room.transform.position);
            ApplyCamera(room, door);
        }

        /// <summary>
        /// 把"房间对象已被 SetActive(true) 但初始化没跑完"的房间补成可用状态。
        ///
        /// 顺序完全照 BaseRoom.PlacePlayerInRoom_End：
        ///   种子 -> 借出道具/怪物/箱子 -> 背景 -> 房间存档状态 -> 相机 -> 进入事件。
        /// 每一步单独 try：原版里 InitializeBackgrounds() 在背景池缺资源时会**直接 throw**，
        /// 一旦抛出，后面的 OnPlayerEnter_LoadStageData / CameraController.UpdateRoomCameraSettings /
        /// BroadcastPlayerEnterRoomEvents 全部不会执行。表现就是这套症状三连：
        ///   · 视角卡住（相机还指着上一个房间的 confiner）
        ///   · 房间里一个怪都没有（怪在房间 OnDisable 时已归还池，没有重新借出）
        ///   · 出门没反应（门出口触发器只在进入事件里被打开）
        /// </summary>
        internal static void CompleteRoomActivation(BaseRoom room, Vector3 worldPos, bool spawnEnemies, bool broadcastEnter = true)
        {
            if (room == null) return;

            Door door = FindNearestDoor(room, worldPos);

            if (spawnEnemies)
            {
                try
                {
                    var seedMethod = AccessTools.Method(room.GetType(), "GetSpecialPropSeed");
                    if (seedMethod != null)
                        RNGManager.SetSeed(RngID.SpecialProps_RoomSeed, (int)seedMethod.Invoke(room, null));
                }
                catch (Exception ex) { Warn("房间种子设置失败: " + ex.Message); }

                try
                {
                    AccessTools.Method(room.GetType(), "InitializeSpawnControllerInstances")?.Invoke(room, null);
                }
                catch (Exception ex) { Warn("房间刷怪初始化失败: " + ex.Message); }

                try
                {
                    AccessTools.Method(room.GetType(), "InitializeBackgrounds")?.Invoke(room, null);
                }
                catch (Exception ex) { Warn("房间背景初始化失败: " + ex.Message); }
            }

            try
            {
                if (room.SaveController != null) room.SaveController.OnPlayerEnter_LoadStageData();
            }
            catch (Exception ex) { Warn("房间存档状态加载失败: " + ex.Message); }

            ApplyCamera(room, door);

            try
            {
                if (broadcastEnter && door != null) BroadcastEnterMethod?.Invoke(room, new object[] { door });
            }
            catch (Exception ex) { Warn("补广播进入房间失败: " + ex.Message); }

            ActivateDoorTriggers(room, true);
            set_IsPlayerInRoom(room, true);

            Plugin.Log.LogInfo("[RL2MP] [ROOM] 房间 '" + room.name + "' 已补完初始化（怪/背景/存档/相机/事件）。");
        }

        /// <summary>把相机切到指定房间（幂等，重复调用只是重设 confiner/zoom）。</summary>
        internal static void ApplyCamera(BaseRoom room, Door door)
        {
            if (room == null) return;
            try
            {
                CameraController.UpdateRoomCameraSettings(new RoomViaDoorEventArgs(room, door));
            }
            catch (Exception ex) { Warn("更新相机房间边界失败: " + ex.Message); }
        }

        /// <summary>取离给定坐标最近的门（用于事件参数，不能传 null —— 部分监听者会读 Door.Side）。</summary>
        internal static Door FindNearestDoor(BaseRoom room, Vector3 worldPos)
        {
            Door best = null;
            float bestD = float.MaxValue;
            try
            {
                var doors = room != null ? room.Doors : null;
                if (doors == null) return null;
                for (int i = 0; i < doors.Count; i++)
                {
                    var d = doors[i];
                    if (d == null) continue;
                    float dist = ((Vector2)d.CenterPoint - (Vector2)worldPos).sqrMagnitude;
                    if (dist < bestD) { bestD = dist; best = d; }
                }
            }
            catch { }
            return best;
        }

        /// <summary>
        /// 房间"有怪应该在场但没借出"的数量。房间 OnDisable 时 EnemySpawnController
        /// 会把 EnemyInstance 置 null 并归还池；重新激活后必须 InitializeSpawnControllerInstances
        /// 才会重新借出。没跑这一步，房间就是空的。
        /// </summary>
        internal static int CountMissingEnemies(BaseRoom room)
        {
            int missing = 0;
            try
            {
                var mgr = room != null ? room.SpawnControllerManager : null;
                if (mgr == null || mgr.EnemySpawnControllers == null) return 0;
                var list = mgr.EnemySpawnControllers;
                for (int i = 0; i < list.Length; i++)
                {
                    var esc = list[i];
                    if (esc == null) continue;
                    try
                    {
                        if (!esc.ShouldSpawn) continue;
                        if (esc.IsDead) continue;
                        if (esc.EnemyInstance == null) missing++;
                    }
                    catch { }
                }
            }
            catch { }
            return missing;
        }

        private static void set_IsPlayerInRoom(BaseRoom room, bool value)
        {
            try
            {
                if (IsPlayerInRoomField != null) { IsPlayerInRoomField.SetValue(room, value); return; }
                var prop = AccessTools.Property(typeof(BaseRoom), "IsPlayerInRoom");
                var setter = prop != null ? prop.GetSetMethod(true) : null;
                if (setter != null) setter.Invoke(room, new object[] { value });
            }
            catch (Exception ex) { Warn("设置 IsPlayerInRoom 失败: " + ex.Message); }
        }

        /// <summary>直接把房间里所有门的出口触发器打开（巡检用）。</summary>
        private static int ActivateDoorTriggers(BaseRoom room, bool force)
        {
            if (room == null) return 0;
            int fixedCount = 0;
            try
            {
                var doors = room.Doors;
                if (doors == null) return 0;
                for (int i = 0; i < doors.Count; i++)
                {
                    var d = doors[i];
                    if (d == null) continue;
                    try
                    {
                        var trig = ExitTriggerField != null ? ExitTriggerField.GetValue(d) as PlayerTrigger : null;
                        if (trig == null) continue;
                        if (!force && trig.IsTriggerActive) continue;
                        trig.SetTriggerActive(true);
                        fixedCount++;
                    }
                    catch { /* 单个门失败不影响其他门 */ }
                }
            }
            catch (Exception ex) { Warn("ActivateDoorTriggers: " + ex.Message); }
            return fixedCount;
        }

        // ---------------- 周期巡检 ----------------

        internal static void Tick()
        {
            _tick += Time.unscaledDeltaTime;
            if (_tick < 0.5f) return;
            float dt = _tick;
            _tick = 0f;

            try { TickInner(dt); }
            catch (Exception ex) { Warn("巡检异常: " + ex.Message); }
        }

        private static void TickInner(float dt)
        {
            // 只在联机会话里干预：单机流程（回城堡、挑战房、剧情锁门）一律不碰，
            // 否则 ResetIsDead / 强开门会破坏原生玩法。
            if (!DownedState.InSession) return;

            var ps = PlayerSync.Instance;
            var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
            if (pc == null || DownedState.LocalDown || pc.CurrentHealth <= 0f)
            {
                _offRoomTime = 0f;
                _doorHoldTime = 0f;
                _doorHold = null;
                return;
            }

            // ---- 0) IsDead 卡住 = 门全部失效 ----
            // BaseRoom.PlayerExit 的第一行是 if (playerController.IsDead) return;
            // 我们的复活只回血 + ResetStates（不改 IsDead），一旦某条路径真的置过
            // IsDead（倒地观战、真实死亡、跑到挑战房等），人就永远进不了下一扇门。
            try
            {
                if (pc.IsDead && !DownedState.AllowRealDeath)
                {
                    pc.ResetIsDead();
                    Plugin.Log.LogWarning("[RL2MP] [ROOM] 玩家 IsDead 仍为 true（门会全部失效），已强制复位。");
                }
            }
            catch { }

            BaseRoom room = null;
            try { room = pc.CurrentlyInRoom; } catch { }
            if (room == null)
            {
                _offRoomTime = 0f;
                return;
            }

            Vector3 pos = pc.transform.position;

            // ---- 0b) 换房后的一次性体检（0.8s：原版 PlacePlayerInRoom_End 这时应该跑完了）----
            if (_lastSeenRoom != room)
            {
                _lastSeenRoom = room;
                _verifyRoom = room;
                _verifyAt = Time.unscaledTime + 0.8f;
                _verifyCameraDone = false;
                _verifyAttempts = 0;
                _offRoomTime = 0f;
            }

            if (_verifyRoom == room && Time.unscaledTime >= _verifyAt)
            {
                // 相机：换房被打断时它会停在上一个房间的 confiner 上 —— 玩家能走，
                // 画面不跟，就是"视角动不了"。重设一次是幂等的。
                if (!_verifyCameraDone)
                {
                    _verifyCameraDone = true;
                    ApplyCamera(room, FindNearestDoor(room, pos));
                }

                // 空房间：房间 OnDisable 时敌人被归还池并把 EnemyInstance 置 null，
                // 重新激活后必须 InitializeSpawnControllerInstances 才会借回来。
                // 如果 PlacePlayerInRoom_End 中途抛异常（背景池缺资源会直接 throw），
                // 这一步就永远没跑 —— 房间看着干净，其实一只怪都没有。
                int missing = CountMissingEnemies(room);
                if (missing > 0 && room.gameObject.activeInHierarchy && _verifyAttempts < 2)
                {
                    _verifyAttempts++;
                    Plugin.Log.LogWarning("[RL2MP] [ROOM] 房间 '" + room.name + "' 有 "
                        + missing + " 个怪没被初始化（空房间），补刷第 " + _verifyAttempts + " 次。");
                    CompleteRoomActivation(room, pos, true, false);
                }
                else if (missing > 0 && _verifyAttempts >= 2 && Time.unscaledTime - _logTime > 5f)
                {
                    _logTime = Time.unscaledTime;
                    Plugin.Log.LogWarning("[RL2MP] [ROOM] 房间 '" + room.name + "' 仍有 "
                        + missing + " 个怪借不出来（对象池可能耗尽），不再重试。");
                }
            }

            // ---- 1) 门触发器自愈：没激活的直接打开 ----
            // 房间刚激活/存档重载时门触发器可能是关的，玩家会"走门口没反应"。
            // 给 1 秒缓冲：PlacePlayerInRoom 的落点就在门口，刚进房别乱动门状态。
            float enteredFor = 99f;
            try { enteredFor = Time.time - pc.TimeEnteredRoom; } catch { }
            int fixedDoors = enteredFor > 1f ? ActivateDoorTriggers(room, false) : 0;
            if (fixedDoors > 0)
                Plugin.Log.LogInfo("[RL2MP] [ROOM] 房间 '" + room.name + "' 重新激活 "
                    + fixedDoors + " 个门的出口触发器。");

            // ---- 1b) "进入房间"事件没跑完（PlacePlayerInRoom_End 中途异常/被打断）----
            // 这种情况下 room.IsPlayerInRoom 还是 false，门也永远停在关闭状态。
            // 每个房间只补一次，避免刷事件。
            if (_lastMarkRoom != room && !IsRoomEntered(room))
            {
                float sinceEnter = 99f;
                try { sinceEnter = Time.time - pc.TimeEnteredRoom; } catch { }
                if (sinceEnter > 1f)
                {
                    _lastMarkRoom = room;
                    MarkEntered(room, null);
                }
            }

            // ---- 2) 越线却没换房：只记录，绝不主动 PlayerExit ----
            // 主动调用会和游戏自己的 GetOnTriggerStayExitRoom 抢着转场（双重 PlayerExit），
            // 直接把房间/相机/存档写成错乱状态（"视角卡住 + 回来怪全没了"）。
            CheckCrossedDoor(room, pc, pos, dt);

            // ---- 3) 掉出房间太久：按摔死处理（走 KillCharacter -> 我们的原地复活）----
            if (InsideRoom(room, pos, 10f))
            {
                _offRoomTime = 0f;
            }
            else
            {
                _offRoomTime += dt;
                if (_offRoomTime >= 3f)
                {
                    _offRoomTime = 0f;
                    Plugin.Log.LogWarning("[RL2MP] [ROOM] 玩家离开房间 '" + room.name
                        + "' 超过 3 秒（位置 " + pos.x.ToString("F1") + "," + pos.y.ToString("F1")
                        + "），按摔死处理。");
                    try { pc.KillCharacter(null, true); }
                    catch (Exception ex) { Warn("KillCharacter(掉出房间) 失败: " + ex.Message); }
                }
            }
        }

        private static void CheckCrossedDoor(BaseRoom room, PlayerController pc, Vector3 worldPos, float dt)
        {
            // 刚进房间的头两秒一定在门口附近（PlacePlayerInRoom 的落点就在门中心旁边），
            // 这时候"越线"是正常的，不能当成卡门处理，否则会被弹回上一个房间。
            float sinceEnter = 99f;
            try { sinceEnter = Time.time - pc.TimeEnteredRoom; } catch { }
            if (sinceEnter < 2f) { _doorHoldTime = 0f; _doorHold = null; return; }

            Vector3 lp;
            try { lp = room.transform.InverseTransformPoint(worldPos); }
            catch { return; }

            Door hit = null;
            try
            {
                var doors = room.Doors;
                if (doors == null) return;
                for (int i = 0; i < doors.Count; i++)
                {
                    var d = doors[i];
                    if (d == null) continue;
                    try
                    {
                        if (d.ConnectedDoor == null || d.ConnectedDoor.Room == null) continue;

                        // 必须真的站在门的出口触发器里 —— 等价于游戏 GetOnTriggerStayExitRoom
                        // 的"已经到门口"前提。没有这一条，玩家在房间顶部跳一下都会被误判成
                        // "想出门"。
                        var trig = ExitTriggerField != null ? ExitTriggerField.GetValue(d) as PlayerTrigger : null;
                        if (trig == null || !trig.IsTriggerActive || !trig.IsPlayerInTrigger) continue;

                        Vector2 c = d.CenterPoint;
                        bool crossed;
                        switch (d.Side)
                        {
                            case RoomSide.Left:
                                crossed = lp.x < c.x - Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE;
                                break;
                            case RoomSide.Right:
                                crossed = lp.x > c.x + Room_EV.TRANSITION_LEFT_RIGHT_DISTANCE;
                                break;
                            case RoomSide.Top:
                                crossed = lp.y > c.y + Room_EV.TRANSITION_TOP_DISTANCE;
                                break;
                            case RoomSide.Bottom:
                                crossed = lp.y < c.y + Room_EV.TRANSITION_BOTTOM_DISTANCE;
                                break;
                            default:
                                crossed = false;
                                break;
                        }
                        if (!crossed) continue;
                        // 只认"贴着门"：距离门中心 4m 内，避免房间中部被误判
                        if (Vector2.Distance(new Vector2(lp.x, lp.y), c) > 4f) continue;
                        hit = d;
                        break;
                    }
                    catch { }
                }
            }
            catch { return; }

            if (hit == null)
            {
                _doorHoldTime = 0f;
                _doorHold = null;
                return;
            }

            if (_doorHold != hit)
            {
                _doorHold = hit;
                _doorHoldTime = 0f;
                _doorHoldPos = worldPos;
                return;
            }

            // 玩家还在走动（1.5m 以上位移）说明只是路过门洞，不是卡住
            if ((worldPos - _doorHoldPos).sqrMagnitude > 2.25f)
            {
                _doorHoldTime = 0f;
                _doorHoldPos = worldPos;
                return;
            }

            _doorHoldTime += dt;
            if (_doorHoldTime < 1.5f) return;

            _doorHoldTime = 0f;
            _doorHold = null;
            // 只诊断，不代替游戏转场：游戏的 Door 触发器自己会调 PlayerExit。
            // 我们再调一次 = 同一帧两个 PlacePlayerInRoom，房间/相机/存档全乱。
            Warn("玩家已在门 '" + hit.Side + "-" + hit.Number
                + "' 的越界位置停留 1.5 秒仍未转场（门触发器可能失效），已重开触发器，等待游戏自行转场。");
            ActivateDoorTriggers(room, true);
        }

        private static bool IsRoomEntered(BaseRoom room)
        {
            if (IsPlayerInRoomField == null) return true;   // 读不到就别乱补
            try { return (bool)IsPlayerInRoomField.GetValue(room); }
            catch { return true; }
        }

        private static bool InsideRoom(BaseRoom room, Vector3 worldPos, float margin)        {
            try
            {
                Bounds b = room.Bounds;
                if (b.size.x <= 0.01f || b.size.y <= 0.01f) return true; // 拿不到边界就不判
                return worldPos.x >= b.min.x - margin && worldPos.x <= b.max.x + margin
                    && worldPos.y >= b.min.y - margin && worldPos.y <= b.max.y + margin;
            }
            catch { return true; }
        }

        internal static void Reset(string reason)
        {
            _offRoomTime = 0f;
            _doorHoldTime = 0f;
            _doorHold = null;
            _lastMarkRoom = null;
            _lastSeenRoom = null;
            _verifyRoom = null;
            _verifyCameraDone = false;
            _verifyAttempts = 0;
            _tick = 0f;
        }

        /// <summary>复活/手动传送后调用：让下一次巡检重新做一遍房间体检（相机 + 空怪）。</summary>
        internal static void RequestVerify()
        {
            _lastSeenRoom = null;
            _verifyRoom = null;
            _verifyCameraDone = false;
            _verifyAttempts = 0;
        }

        // ---------------- 补丁：PlacePlayerInRoom_End 异常兜底 ----------------

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var target = AccessTools.Method(typeof(BaseRoom), "PlacePlayerInRoom_End",
                    new Type[] { typeof(Door) });
                if (target == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [ROOM] PlacePlayerInRoom_End 没找到，跳过异常兜底补丁。");
                    return;
                }

                var finalizer = new HarmonyMethod(typeof(RoomFix).GetMethod(
                    "PlacePlayerInRoomEndFinalizer", BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(target, finalizer: finalizer);
                Plugin.Log.LogInfo("[RL2MP] [ROOM] PlacePlayerInRoom_End 异常兜底 补丁 OK."
                    + " 方法=" + target.DeclaringType.Name + "." + target.Name);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOM] 异常兜底补丁失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 原版 PlacePlayerInRoom_End 任何一步抛异常，后面的步骤就全部作废：
        ///   InitializeSpawnControllerInstances（怪）-> InitializeBackgrounds（会主动 throw！
        ///   背景池找不到资源时）-> OnPlayerEnter_LoadStageData（房间状态）-> 相机 -> 进入事件。
        /// 于是出现"房间没怪 + 视角卡住 + 门全关"的三连症状。
        /// 这里兜住异常，并把怪之后的几步补做掉，保证房间至少是可玩状态。
        /// </summary>
        private static Exception PlacePlayerInRoomEndFinalizer(BaseRoom __instance, Door door, Exception __exception)
        {
            if (__exception == null) return null;

            Plugin.Log.LogWarning("[RL2MP] [ROOM] PlacePlayerInRoom_End 抛异常（房间会半初始化）: "
                + __exception.GetType().Name + ": " + __exception.Message);

            try
            {
                if (__instance.SaveController != null) __instance.SaveController.OnPlayerEnter_LoadStageData();
            }
            catch (Exception e) { Warn("兜底加载房间存档失败: " + e.Message); }

            Door d = door;
            if (d == null) d = CountDoorsSafe(__instance) > 0 ? FindNearestDoor(__instance, __instance.transform.position) : null;
            ApplyCamera(__instance, d);

            try
            {
                if (d != null && BroadcastEnterMethod != null) BroadcastEnterMethod.Invoke(__instance, new object[] { d });
            }
            catch (Exception e) { Warn("兜底广播进入房间失败: " + e.Message); }

            ActivateDoorTriggers(__instance, true);
            set_IsPlayerInRoom(__instance, true);

            // 吞掉异常：不吞的话 PlacePlayerInRoom 直接中断，房间状态更糟
            return null;
        }

        private static int CountDoorsSafe(BaseRoom room)
        {
            try { return room != null && room.Doors != null ? room.Doors.Count : 0; }
            catch { return 0; }
        }
    }
}
