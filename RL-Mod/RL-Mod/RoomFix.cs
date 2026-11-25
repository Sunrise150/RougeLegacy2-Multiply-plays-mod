using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using MoreMountains.CorgiEngine;
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
        // v3.0.20：建触发器改走原生 Door.OnPlayerEnterRoom（见 DoorLinkFix.ArmDoor），
        // 不再反射单建 CreateExitTrigger —— 那样不会置 m_hasExitTriggerBeenCreated，
        // 原生之后再进房会重复创建一个永不启用的孤儿触发器。
        // v3.0.17：触发器已经 active、但玩家站在里面 IsPlayerInTrigger 仍为 false 时，
        // Unity 不会再补 OnTriggerEnter2D。手动调这个私有方法重建 Door 的原生协程。
        private static readonly MethodInfo PlayerEnterTriggerMethod =
            AccessTools.Method(typeof(PlayerTrigger), "PlayerEnterTrigger", new Type[0]);
        private static readonly MethodInfo BroadcastEnterMethod =
            AccessTools.Method(typeof(BaseRoom), "BroadcastPlayerEnterRoomEvents", new Type[] { typeof(Door) });
        // v3.0.11：这两个是 BaseRoom 的私有实例方法。旧版用 room.GetType()（派生类）去取，
        // AccessTools.Method 默认不向上找基类私有方法 → 恒 null → 怪/背景从没被初始化（黑房/空房）。
        private static readonly MethodInfo SpawnInitMethod =
            AccessTools.Method(typeof(BaseRoom), "InitializeSpawnControllerInstances");
        private static readonly MethodInfo BgInitMethod =
            AccessTools.Method(typeof(BaseRoom), "InitializeBackgrounds");

        private static float _tick;
        private static float _offRoomTime;
        private static float _voidTime;
        private static int _voidTries;
        private static float _saveLogTime;
        private static float _doorHoldTime;
        private static Door _doorHold;
        private static Vector3 _doorHoldPos;
        // v2.9.1：原生门触发器彻底没触发时的安全兜底。只在玩家确实越过门边界、
        // 且持续卡住 0.8s 才调用一次 PlayerExit；避免与原生触发器同帧双重过门。
        private static Door _hardDoor;
        private static float _hardDoorTime;
        private static Vector3 _hardDoorPos;
        // v3.0.19：出门意图锁存（借鉴 RevivalSync 的“边沿检测 + 未见释放看门狗”）。
        // 玩家越过门边界后水平速度立刻归零变成垂直下坠，旧逻辑每 tick 重判
        // outward(vel.x) 会立刻清零计时，兜底 0.8s 永远攒不满；锁存后只要人还在
        // 门外就持续累计，救援也让位。
        private static bool _hardDoorLatched;
        private static float _hardDoorLatchUntil;
        private static float _manualExitUntil;
        // v3.0.16：最近一次"玩家正在门口越界尝试出门"的时刻。掉坑救援在这个窗口内
        // 必须让位——否则 0.45s 就把人瞬移回门内，兜底 0.8s 永远等不到，形成
        // "走到门口→被救回→再走"死循环（真机日志 5 次掉出全在房间边界）。
        private static float _doorExitAttemptUntil;
        // v3.0.17：我们自己为“卡门状态锁”打开 DisableDoorBlock 的截止时刻。
        // 只回收自己设的，不碰 Roll/Lance/Scythe 等技能自己开的门放行。
        private static float _roomDoorBlockUntil;
        private static BaseRoom _lastMarkRoom;
        private static BaseRoom _lastSeenRoom;
        private static BaseRoom _verifyRoom;
        private static float _verifyAt;
        private static bool _verifyCameraDone;
        private static int _verifyAttempts;
        private static float _logTime;

        /// <summary>
        /// v3.0.24：自己记录的"进房时刻"（unscaled）。
        /// 原生 PlayerController.TimeEnteredRoom 配合 Time.time 一起用是脆的：
        /// Time.time 是【受 timeScale 缩放】的时钟 —— Time.timeScale 一旦被压到 0
        /// （Boss 死亡定格的 Cutscene 类型 PauseCompat 明确不拦；RLTimeScale 又会把
        /// 9 个类型取最小值写回，只要有一项残留 0 就永远是 0），
        /// Time.time 就彻底停住，Time.time - TimeEnteredRoom 恒等于 0，
        /// "进房超过 1 秒"这个条件永远不成立 -> 门触发器自愈
        /// （ActivateDoorTriggers / PokeStubbedDoorTriggers / MarkEntered）被整体跳过
        /// -> "门触发器又失效了"。
        /// unscaledTime 是全局单调时钟，不受 timeScale 影响，用来兜底。
        /// </summary>
        private static float _roomEnteredAtUnscaled = -1f;
        /// <summary>v3.0.26：落位诊断的节流时间戳。</summary>
        private static float _spawnLogTime = -99f;

        /// <summary>
        /// "当前玩家在这间房待了多久（秒）"。
        /// scaled（Time.time - TimeEnteredRoom）精确但会被 timeScale=0 冻住；
        /// unscaled（自计时）不受缩放影响、但最多有 0.5s 的巡检误差。两者取较大值：
        /// 正常情况两者一致；timeScale 卡 0 时 unscaled 继续走，门自愈照常工作。
        /// </summary>
        private static float EnteredForSeconds(PlayerController pc)
        {
            float scaled = -1f;
            try
            {
                float tEnter = pc.TimeEnteredRoom;
                float tNow = Time.time;
                if (tNow >= tEnter) scaled = tNow - tEnter;
            }
            catch { }
            float unscaled = _roomEnteredAtUnscaled > 0f
                ? (Time.unscaledTime - _roomEnteredAtUnscaled) : -1f;

            if (scaled >= 0f && unscaled >= 0f) return Mathf.Max(scaled, unscaled);
            if (scaled >= 0f) return scaled;
            if (unscaled >= 0f) return unscaled;
            return 99f;   // 完全不知道：沿用旧版的"当作很久"兜底
        }

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
                            if (trig == null)
                            {
                                // v3.0.20：走原生 Door.OnPlayerEnterRoom（建触发器 + 门沿 + 启用），
                                // 不再反射单调 CreateExitTrigger（不置 m_hasExitTriggerBeenCreated，
                                // 会留下孤儿触发器）。
                                DoorLinkFix.ArmDoor(room, d);
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
                    if (SpawnInitMethod != null) SpawnInitMethod.Invoke(room, null);
                    else Warn("房间刷怪初始化失败: 找不到 BaseRoom.InitializeSpawnControllerInstances");
                }
                catch (Exception ex) { Warn("房间刷怪初始化失败: " + ex.Message); }

                try
                {
                    if (BgInitMethod != null) BgInitMethod.Invoke(room, null);
                    else Warn("房间背景初始化失败: 找不到 BaseRoom.InitializeBackgrounds");
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
                        // v3.0.15：合并房的门触发器可能从未创建（进房没派发 PlayerEnterRoom，
                        // 或 _End 中途异常）。旧版 trig==null 直接 continue → 门永远没触发器，
                        // 原生 GetOnTriggerStayExitRoom 永远不触发 → 玩家掉出房间。
                        // v3.0.20：改用原生 Door.OnPlayerEnterRoom 补建（它同时建门沿 ledge
                        // 并置 m_hasExitTriggerBeenCreated）。旧版反射单调 CreateExitTrigger
                        // 不置那个标志，之后原生再进房会重复建一个永不启用的孤儿触发器。
                        if (trig == null)
                        {
                            DoorLinkFix.ArmDoor(room, d);
                            trig = ExitTriggerField != null ? ExitTriggerField.GetValue(d) as PlayerTrigger : null;
                        }
                        if (trig == null) continue;
                        // v3.0.17：SetTriggerActive(true) 内部会 Reset() 清掉
                        // IsPlayerInTrigger。对已经 active 的触发器再“强制重开”，
                        // 恰好把 CheckCrossedDoor 要用的判定条件清掉，门就更过不去。
                        // 只激活关闭的；已 active 的交由 PokeStubbedDoorTriggers 补发进入事件。
                        if (trig.IsTriggerActive) continue;
                        trig.SetTriggerActive(true);
                        fixedCount++;
                    }
                    catch { /* 单个门失败不影响其他门 */ }
                }
            }
            catch (Exception ex) { Warn("ActivateDoorTriggers: " + ex.Message); }
            return fixedCount;
        }

        /// <summary>
        /// v3.0.17：触发器已经 active、玩家也站在里面，但 IsPlayerInTrigger 为 false 时，
        /// Unity 不会补发 OnTriggerEnter2D，Door 的 OnPlayerStayExitTrigger 协程永远不启动。
        /// 手动调私有 PlayerEnterTrigger()，等价于玩家重新踏入触发器，启动原生换房流程。
        /// 这比“强制重开触发器”安全——后者会把 IsPlayerInTrigger 自己清掉。
        /// </summary>
        private static int PokeStubbedDoorTriggers(BaseRoom room, Vector3 worldPos)
        {
            int poked = 0;
            if (room == null || PlayerEnterTriggerMethod == null) return 0;
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
                        if (trig == null || !trig.IsTriggerActive || trig.IsPlayerInTrigger) continue;
                        var col = trig.GetComponent<Collider2D>();
                        if (col == null || !col.bounds.Contains(worldPos)) continue;
                        PlayerEnterTriggerMethod.Invoke(trig, null);
                        poked++;
                    }
                    catch { /* 单个触发器失败不影响其它门 */ }
                }
            }
            catch (Exception ex) { Warn("PokeStubbedDoorTriggers: " + ex.Message); }
            return poked;
        }

        /// <summary>
        /// v3.0.17：原生 Door.GetOnTriggerStayExitRoom 对 Stunned / MovementDisabled
        /// 走的是“把玩家钳回门口、速度清零”的分支，根本不转场。复活之外的路径
        /// （受击、冲刺、剧情状态）也可能卡在这里。卡门 1.5s 后清一次状态，并临时
        /// 打开 DisableDoorBlock 给门判定放行；只回收本方法自己设的放行窗。
        /// </summary>
        private static void ClearDoorBlockState(PlayerController pc)
        {
            if (pc == null) return;
            try
            {
                var cond = pc.ConditionState;
                bool locked = cond == CharacterStates.CharacterConditions.Stunned
                    || cond == CharacterStates.CharacterConditions.DisableHorizontalMovement
                    || pc.CharacterMove.MovementDisabled;
                if (!locked) return;

                pc.ResetStates();
                pc.DisableDoorBlock = true;
                _roomDoorBlockUntil = Time.unscaledTime + 1.5f;
                Plugin.Log.LogWarning("[RL2MP] [ROOM] 玩家卡门且移动状态被锁（"
                    + cond + "），已复位并临时放行门判定。");
            }
            catch (Exception ex) { Warn("清除门状态锁失败: " + ex.Message); }
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

            // v3.0.17：回收本类自己开的门放行窗；不碰 Roll/Lance/Scythe 自己设的。
            if (_roomDoorBlockUntil > 0f && Time.unscaledTime > _roomDoorBlockUntil)
            {
                _roomDoorBlockUntil = 0f;
                try { pc.DisableDoorBlock = false; } catch { }
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
                // v3.0.24：记录自己的进房时刻（unscaled），跨场景加载后 Time.time 归零时兜底。
                _roomEnteredAtUnscaled = Time.unscaledTime;

                // v3.0.26：落位诊断 —— 玩家实际站在哪、房间的默认出生点在哪。
                // 用户反馈"开局不是从默认出生点，而是出生在离下一个房间最近的位置"，
                // 这一行能直接看出是原生落位就这样，还是被跟随传送挪过。
                if (Time.unscaledTime - _spawnLogTime > 3f)
                {
                    _spawnLogTime = Time.unscaledTime;
                    try
                    {
                        var rr = room as Room;
                        var spawn = rr != null ? rr.PlayerSpawn : null;
                        string spawnInfo = spawn != null
                            ? "出生点(" + spawn.transform.localPosition.x.ToString("F1") + ","
                                + spawn.transform.localPosition.y.ToString("F1") + ")"
                            : "出生点=无";
                        Plugin.Log.LogInfo("[RL2MP] [ROOM] 落位诊断：房间 '" + room.name
                            + "' 类型=" + room.GetType().Name
                            + " 玩家在(" + pos.x.ToString("F1") + "," + pos.y.ToString("F1") + ")"
                            + " " + spawnInfo
                            + " biome=" + room.BiomeType
                            + " 房坐标=" + RoomTravelSync.RoomCoordsOf(room));
                    }
                    catch { }
                }
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
            float enteredFor = EnteredForSeconds(pc);
            int fixedDoors = enteredFor > 1f ? ActivateDoorTriggers(room, false) : 0;
            if (fixedDoors > 0)
                Plugin.Log.LogInfo("[RL2MP] [ROOM] 房间 '" + room.name + "' 重新激活 "
                    + fixedDoors + " 个门的出口触发器。");

            // v3.0.17：触发器 active 但 IsPlayerInTrigger 为 false 时，Unity 不会补发
            // OnTriggerEnter2D。手动补发一次，让 Door 的原生协程重新跑起来。
            int poked = enteredFor > 1f ? PokeStubbedDoorTriggers(room, pos) : 0;
            if (poked > 0 && Time.unscaledTime - _logTime > 5f)
            {
                _logTime = Time.unscaledTime;
                Plugin.Log.LogInfo("[RL2MP] [ROOM] 房间 '" + room.name + "' 补发 "
                    + poked + " 个门触发器的进入事件。");
            }

            // ---- 1b) "进入房间"事件没跑完（PlacePlayerInRoom_End 中途异常/被打断）----
            // 这种情况下 room.IsPlayerInRoom 还是 false，门也永远停在关闭状态。
            // 每个房间只补一次，避免刷事件。
            if (_lastMarkRoom != room && !IsRoomEntered(room))
            {
                float sinceEnter = EnteredForSeconds(pc);
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
            CheckHardExit(room, pc, pos, dt);

            // ---- 2b) 门连接体检与修复（v3.0.20）----
            // 原生 PlayerExit 在 ConnectedDoor.Room == null 时静默失败，玩家会走进
            // 没有地板的门洞坠落。这里每 0.5s 检查一次当前房间的门：能连就修好，
            // 连不上就先在门洞补地板，保证玩家不会再掉进虚空。
            DoorLinkFix.Tick(dt);

            // ---- 3) 掉出房间 / 掉下地板：救回房间内，救不回来才按摔死处理 ----
            // v2.4.0：**彻底删掉"脚下没有地面"这条判定**。
            // 那一版只要"脚下 6m 内没地板 && vy<-1"就认定掉虚空，而玩家跳跃、从高台落下、
            // 站在单向平台上全都满足 —— 真机日志里就是正常走路被搬走：
            //     '2*Standard*'（75.7,12.2）-> 已救回到地面 80.0,11.1
            // 用户看到的就是"把我传到房间中心/别的门口去了"。
            // 现在只认"真的掉到房间底部以下"，其它一律不管（横向出界是过门逻辑的事）。
            bool belowFloor = false;
            try
            {
                var b = room.Bounds;
                if (b.size.y > 0.1f && pos.y < b.min.y - 3f) belowFloor = true;
            }
            catch { }

            if (!belowFloor)
            {
                _offRoomTime = 0f;
                _voidTime = 0f;
                _voidTries = 0;
            }
            else
            {
                _offRoomTime += dt;
                // v3.0.16：玩家正在门口尝试出门时暂缓掉坑救援。
                // 掉出判定 y < min.y-3 在门口下方平台也会成立，0.45s 救援抢先把人
                // 瞬移回门内 → 兜底过门永远等不到 → 反复"走到门口被救回"。
                // 出门尝试窗口(1s)内只累计、不救援，给 CheckHardExit 的 0.8s 兜底让路。
                if (Time.unscaledTime < _doorExitAttemptUntil)
                {
                    _voidTime = 0f;
                    return;
                }
                _voidTime += dt;
                if (_voidTime >= 0.45f)
                {
                    _voidTime = 0f;
                    if (_voidTries < 4 && RescueFromVoid(pc, room, pos))
                    {
                        RequestVerify();
                        return;
                    }

                    _voidTries++;
                    if (_voidTries >= 4 && _offRoomTime >= 2f)
                    {
                        _offRoomTime = 0f;
                        _voidTries = 0;
                        Plugin.Log.LogWarning("[RL2MP] [ROOM] 玩家反复掉出房间 '" + room.name
                            + "'（位置 " + pos.x.ToString("F1") + "," + pos.y.ToString("F1")
                            + "），按摔死处理。");
                        try { pc.KillCharacter(null, true); }
                        catch (Exception ex) { Warn("KillCharacter(掉出房间) 失败: " + ex.Message); }
                    }
                }
            }
        }

        // ---------------- 掉出房间救援 ----------------

        /// <summary>
        /// 把掉出房间/掉下地板的玩家放回房间内的实地。
        /// 候选点顺序：离玩家最近的门内侧（2.5m）→ 房间中心 → 房间根节点；
        /// 每个候选点向下打 12m 射线，命中第一个非触发器、非角色碰撞体就用它当地面。
        /// </summary>
        private static bool RescueFromVoid(PlayerController pc, BaseRoom room, Vector3 from)
        {
            Vector3 spot;
            if (!TryFindSafeSpot(room, from, out spot)) return false;
            try
            {
                pc.transform.position = spot;
                try { pc.ControllerCorgi.SetLastStandingPosition(spot); } catch { }
                try { pc.SetVelocity(0f, 0f, false); } catch { }
                try { pc.ResetStates(); } catch { }
                Plugin.Log.LogWarning("[RL2MP] [ROOM] 玩家掉出房间 '" + room.name + "'（"
                    + from.x.ToString("F1") + "," + from.y.ToString("F1") + "），已救回到地面 "
                    + spot.x.ToString("F1") + "," + spot.y.ToString("F1"));
                return true;
            }
            catch (Exception ex)
            {
                Warn("救援落位失败: " + ex.Message);
                return false;
            }
        }

        private static bool TryFindSafeSpot(BaseRoom room, Vector3 from, out Vector3 spot)
        {
            spot = Vector3.zero;
            try
            {
                Bounds bounds = room.Bounds;
                Vector3 center = bounds.center;
                var candidates = new List<Vector3>();

                // v2.4.0：门内侧优先（用户明确要求"掉下去要回入口，不是回房间中部"）。
                // 之前把"保持当前水平位置找地面"放第一是为了防误判，但误判源（noGround）
                // 已经删掉了，现在触发救援就是真掉到房间底部以下，回入口最符合直觉。
                try
                {
                    var doors = room.Doors;
                    if (doors != null)
                    {
                        var list = new List<Door>();
                        for (int i = 0; i < doors.Count; i++) if (doors[i] != null) list.Add(doors[i]);
                        list.Sort(delegate (Door a, Door b)
                        {
                            return ((Vector2)a.CenterPoint - (Vector2)from).sqrMagnitude
                                .CompareTo(((Vector2)b.CenterPoint - (Vector2)from).sqrMagnitude);
                        });
                        for (int i = 0; i < list.Count; i++)
                        {
                            Vector2 c = list[i].CenterPoint;
                            if (c.sqrMagnitude < 1f) continue;   // 门还没初始化
                            Vector2 dir = (Vector2)center - c;
                            if (dir.sqrMagnitude < 0.01f) dir = Vector2.up;
                            candidates.Add(new Vector3(c.x, c.y, 0f) + (Vector3)(dir.normalized * 2.5f));
                        }
                    }
                }
                catch { }

                // 其次：保持当前水平位置，只往下找地面
                candidates.Add(new Vector3(from.x, from.y, 0f));

                // 最后：房间中心 / 房间根
                candidates.Add(center);
                candidates.Add(room.transform.position);

                for (int i = 0; i < candidates.Count; i++)
                {
                    Vector3 c = candidates[i];
                    // 只有"保持原位"这个候选不做 InsideRoom 检查（它的目的就是原地找地面）
                    bool isInPlace = Mathf.Abs(c.x - from.x) < 0.01f && Mathf.Abs(c.y - from.y) < 0.01f;
                    if (!isInPlace && !InsideRoom(room, c, 2f)) continue;
                    Vector3 ground;
                    if (TrySampleGround(c, out ground)) { spot = ground; return true; }
                }

                if (bounds.size.x > 0.1f && bounds.size.y > 0.1f)
                {
                    spot = center;
                    return true;
                }
            }
            catch { }
            return false;
        }

        /// <summary>从给定位置向下找地面；找到返回落点（地面上方 1.1m）。</summary>
        private static bool TrySampleGround(Vector3 at, out Vector3 ground)
        {
            ground = Vector3.zero;
            try
            {
                var origin = new Vector2(at.x, at.y + 1.5f);
                var hits = Physics2D.RaycastAll(origin, Vector2.down, 12f);
                if (hits == null || hits.Length == 0) return false;

                float best = float.MaxValue;
                for (int i = 0; i < hits.Length; i++)
                {
                    var col = hits[i].collider;
                    if (col == null || col.isTrigger) continue;                       // 触发器不是地板
                    if (col.GetComponentInParent<BaseCharacterController>() != null) continue; // 角色本身
                    if (hits[i].distance < best) best = hits[i].distance;
                }
                if (best >= float.MaxValue) return false;

                ground = new Vector3(at.x, origin.y - best + 1.1f, 0f);
                return true;
            }
            catch { return false; }
        }

        private static void CheckCrossedDoor(BaseRoom room, PlayerController pc, Vector3 worldPos, float dt)
        {
            // 刚进房间的头两秒一定在门口附近（PlacePlayerInRoom 的落点就在门中心旁边），
            // 这时候"越线"是正常的，不能当成卡门处理，否则会被弹回上一个房间。
            float sinceEnter = EnteredForSeconds(pc);
            if (sinceEnter < 2f) { _doorHoldTime = 0f; _doorHold = null; return; }

            // v3.0.15：坐标系修复。Door.CenterPoint 是【世界坐标】，原生 Door.GetOnTriggerStayExitRoom
            // 直接拿玩家世界坐标(根节点 localPosition==position)跟它比。旧版用 InverseTransformPoint
            // 得到房间局部坐标去比世界坐标，数值上差一个 room.transform.position（合并房 224），
            // crossed/outward 恒 false —— 所有门的越线兜底全瞎。改为世界坐标，与原生对齐。
            Vector2 lp = new Vector2(worldPos.x, worldPos.y);

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
                + "' 的越界位置停留 1.5 秒仍未转场（门触发器可能失效），补发进入事件并清状态锁。");
            // v3.0.17：绝不再 force 重开 —— SetTriggerActive(true) 会 Reset()
            // 清掉 IsPlayerInTrigger，把上面的判定条件自己抹掉。
            ActivateDoorTriggers(room, false);
            PokeStubbedDoorTriggers(room, worldPos);
            ClearDoorBlockState(pc);
        }

        /// <summary>
        /// v2.9.1：主机倒地后存活方在垂直房间里反复掉地板、原生门触发器一次都没触发时，
        /// 只重开触发器已经救不回来。这里做最后一层兜底：确认玩家真的越过了某扇门、
        /// 且朝门外移动并卡住 0.8 秒，才调用一次原生 PlayerExit（会走正常换房广播）。
        /// 与原生触发器同帧抢跑的风险通过"必须持续卡住 + 2 秒冷却 + 避开远端跟随窗口"压到最低。
        /// </summary>
        private static void CheckHardExit(BaseRoom room, PlayerController pc, Vector3 worldPos, float dt)
        {
            if (Time.unscaledTime < _manualExitUntil) return;
            if (RoomTravelSync.IsApplyingRemote || DownedState.LocalDown || pc.IsDead || !RoomTravelSync.Active)
            {
                _hardDoor = null; _hardDoorTime = 0f; _hardDoorLatched = false;
                return;
            }

            float sinceEnter = EnteredForSeconds(pc);
            if (sinceEnter < 2f) { _hardDoor = null; _hardDoorTime = 0f; _hardDoorLatched = false; return; }

            // v3.0.15：坐标系修复，同 CheckCrossedDoor —— CenterPoint 是世界坐标，必须用世界坐标比。
            Vector2 lp = new Vector2(worldPos.x, worldPos.y);

            bool belowFloor = false;
            try { belowFloor = worldPos.y < room.Bounds.min.y - 2f; } catch { }

            Vector2 vel;
            try { vel = pc.Velocity; } catch { vel = Vector2.zero; }

            Door hit = null;
            if (_hardDoorLatched)
            {
                // v3.0.19：出门意图已锁存——越界后水平速度归零、改垂直下坠也不再清零。
                // 只在“回到门内 / 离门太远 / 超时 / 换房”时才撤销，等价于 RevivalSync
                // 对“未见到的释放”持有的那一小段闩锁。
                hit = _hardDoor;
                if (hit == null || Time.unscaledTime >= _hardDoorLatchUntil
                    || !StillPastDoor(hit, worldPos))
                {
                    _hardDoorLatched = false;
                    _hardDoor = null;
                    _hardDoorTime = 0f;
                    return;
                }
            }
            else
            {
                try
                {
                    var doors = room.Doors;
                    if (doors == null) return;
                    for (int i = 0; i < doors.Count; i++)
                    {
                        var d = doors[i];
                        if (d == null || d.ConnectedDoor == null || d.ConnectedDoor.Room == null) continue;
                        Vector2 c = d.CenterPoint;
                        bool outward = false;
                        bool nearDoor = true;
                        switch (d.Side)
                        {
                            case RoomSide.Left:
                                outward = lp.x < c.x - 1.5f && vel.x < -0.5f;
                                // v3.0.18：侧门不再要求 y 靠近门中心。主机倒地后存活方常沿
                                // 房间低层（比门中心低十几米）绕到侧边界，旧 nearDoor 判定
                                // 把这种合法出门过滤掉，原生触发器和兜底都不会触发。
                                nearDoor = belowFloor || Mathf.Abs(lp.y - c.y) <= 4f
                                    || lp.x < c.x - 3f;
                                break;
                            case RoomSide.Right:
                                outward = lp.x > c.x + 1.5f && vel.x > 0.5f;
                                // v3.0.18：同上，低层绕行到右侧边界也算“在门口”。
                                nearDoor = belowFloor || Mathf.Abs(lp.y - c.y) <= 4f
                                    || lp.x > c.x + 3f;
                                break;
                            case RoomSide.Top:
                                outward = lp.y > c.y + 1.5f && vel.y > 0.5f;
                                nearDoor = Mathf.Abs(lp.x - c.x) <= 4f;
                                break;
                            case RoomSide.Bottom:
                                outward = lp.y < c.y - 1.5f && vel.y < -0.5f;
                                nearDoor = Mathf.Abs(lp.x - c.x) <= 4f;
                                break;
                        }
                        if (outward && nearDoor) { hit = d; break; }
                    }
                }
                catch { return; }

                if (hit == null) { _hardDoor = null; _hardDoorTime = 0f; return; }
                _hardDoorLatched = true;
                _hardDoorLatchUntil = Time.unscaledTime + 2f;
                _hardDoor = hit;
                _hardDoorTime = 0f;
                _hardDoorPos = worldPos;
            }

            // v3.0.19：锁存期间每拍刷新救援让位窗。旧版只在“检测到速度”的 tick 设
            // 1 秒，玩家越界后速度归零就再也不刷新，0.45s 救援把人拉回门内 → 死循环。
            _doorExitAttemptUntil = Time.unscaledTime + 0.6f;
            // v3.0.16：旧版用"位移>3m 清零"判定玩家离开门洞，但 Tick 是 0.5s 一次、
            // 跑步速度 15.6m/s ⇒ 每 tick 位移 ~7.8m，平方 61 恒 >9 → 计时每 tick 都清零，
            // 兜底 PlayerExit 对移动中的玩家永远不会触发（真机日志只有救援、从无兜底过门）。
            // 改为：只在玩家【离开这扇门的门口区域】时才清零；仍在门外越界区就持续累计。
            // nearDoor/outward 上面每 tick 已经重算过，这里只需判断离门中心的距离是否失控。
            try
            {
                Vector2 cNow = hit.CenterPoint;
                // v3.0.18：侧门只看水平距离。低层绕行时玩家 y 与门中心差十几米，
                // 用二维距离会 >8f 而每 tick 清零计时，兜底 PlayerExit 永远攒不满。
                float doorDist = (hit.Side == RoomSide.Left || hit.Side == RoomSide.Right)
                    ? Mathf.Abs(worldPos.x - cNow.x)
                    : Vector2.Distance(new Vector2(worldPos.x, worldPos.y), cNow);
                if (doorDist > 8f)
                {
                    _hardDoorTime = 0f;
                    _hardDoorPos = worldPos;
                    return;
                }
            }
            catch { }

            _hardDoorTime += dt;
            if (_hardDoorTime < 0.8f) return;

            _hardDoor = null;
            _hardDoorTime = 0f;
            _hardDoorLatched = false;
            _hardDoorLatchUntil = 0f;
            _manualExitUntil = Time.unscaledTime + 2f;
            // v3.0.17：PlayerExit 只查 IsDead，但原生触发器还会被 Stunned/MovementDisabled
            // 钳住；兜底前也清一次状态，避免过门后又被原生逻辑拉回门口。
            ClearDoorBlockState(pc);
            Plugin.Log.LogWarning("[RL2MP] [ROOM] 门 '" + hit.Side + "-" + hit.Number
                + "' 的原生触发器未生效（玩家越界 0.8s），执行兜底 PlayerExit。");
            try { room.PlayerExit(hit); }
            catch (Exception ex) { Warn("兜底 PlayerExit 失败: " + ex.Message); }
        }

        /// <summary>
        /// v3.0.19：锁存期间只确认“人还在门外”，不再要求水平速度。
        /// 越界后玩家常立刻变成 vx=0 的垂直下坠，逐 tick 判速度会让兜底永远失效。
        /// </summary>
        private static bool StillPastDoor(Door door, Vector3 worldPos)
        {
            try
            {
                Vector2 c = door.CenterPoint;
                switch (door.Side)
                {
                    case RoomSide.Left:   return worldPos.x < c.x - 1.0f;
                    case RoomSide.Right:  return worldPos.x > c.x + 1.0f;
                    case RoomSide.Top:    return worldPos.y > c.y + 1.0f;
                    case RoomSide.Bottom: return worldPos.y < c.y - 1.0f;
                }
            }
            catch { }
            return false;
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
            _voidTime = 0f;
            _voidTries = 0;
            _doorHoldTime = 0f;
            _doorHold = null;
            _hardDoor = null;
            _hardDoorTime = 0f;
            _hardDoorLatched = false;
            _hardDoorLatchUntil = 0f;
            _manualExitUntil = 0f;
            _doorExitAttemptUntil = 0f;
            _roomDoorBlockUntil = 0f;
            _lastMarkRoom = null;
            _lastSeenRoom = null;
            // v3.0.24：进房时刻也一起清，下一次巡检重新记。
            _roomEnteredAtUnscaled = -1f;
            // v3.0.20：门体检的补地板/断链记录也要清，避免换局后残留旧门洞地板。
            try { DoorLinkFix.Reset(); } catch { }
            _verifyRoom = null;
            _verifyCameraDone = false;
            _verifyAttempts = 0;
            _tick = 0f;
        }

        /// <summary>复活/手动传送后调用：让下一次巡检重新做一遍房间体检（相机 + 空怪）。</summary>
        internal static void RequestVerify()
        {
            _lastSeenRoom = null;
            // v3.0.24：房间被重置时重新计时（否则新房间沿用旧房间的进房时刻）。
            _roomEnteredAtUnscaled = -1f;
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

                // v2.2.0：把 NRE 拦在**它自己的源头**。
                // 以前只兜 PlacePlayerInRoom_End，异常已经跑出 OnPlayerEnter_LoadStageData，
                // 后面的「相机 → 进入房间事件 → 门触发器」全被跳过：视角卡住 + 门全关 +
                // 房间半初始化（客机过门掉虚空就是这么来的）。在源头吞掉，后续步骤照常跑。
                var saveTarget = AccessTools.Method(typeof(RoomSaveController), "OnPlayerEnter_LoadStageData");
                if (saveTarget != null)
                {
                    harmony.Patch(saveTarget, finalizer: new HarmonyMethod(typeof(RoomFix).GetMethod(
                        "LoadStageDataFinalizer", BindingFlags.Static | BindingFlags.NonPublic)));
                    Plugin.Log.LogInfo("[RL2MP] [ROOM] OnPlayerEnter_LoadStageData 异常就地吞掉 补丁 OK.");
                }

                // v2.2.0：进房落位自检 —— 原生 EnterRoom 用 Door.CenterPoint 算落点，
                // 房间没初始化时 CenterPoint 是 0，人会被放到房间外/虚空里。
                var enter = AccessTools.Method(typeof(PlayerController), "EnterRoom",
                    new Type[] { typeof(BaseRoom), typeof(Door), typeof(Vector3) });
                if (enter != null)
                {
                    harmony.Patch(enter, postfix: new HarmonyMethod(typeof(RoomFix).GetMethod(
                        "EnterRoomPostfix", BindingFlags.Static | BindingFlags.NonPublic)));
                    Plugin.Log.LogInfo("[RL2MP] [ROOM] EnterRoom 落点自检 补丁 OK.");
                }
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

            // v2.3.2：异常被吞后 PlacePlayerInRoom_End 的 postfix 不一定还会跑，
            // 这里补一次"换房广播"（内部用 _exitArmed 去重，没在过门时什么也不做）。
            try { RoomTravelSync.TryAnnounceEnteredRoom(__instance, "roomfix-finalizer"); }
            catch { }

            // 吞掉异常：不吞的话 PlacePlayerInRoom 直接中断，房间状态更糟
            return null;
        }

        /// <summary>
        /// RoomSaveController.OnPlayerEnter_LoadStageData 的 finalizer：就地吞异常。
        ///
        /// 这条 NRE 是客机房间半初始化的总源头：它一抛，PlacePlayerInRoom_End 里
        /// 后面的 CameraController.UpdateRoomCameraSettings（视角卡住）和
        /// BroadcastPlayerEnterRoomEvents（门全部停在关闭状态）都不会执行。
        /// 吞在这里，后续步骤就能照常跑完。异常原文（含调用栈）打到日志里，便于定位空引用。
        /// </summary>
        private static Exception LoadStageDataFinalizer(RoomSaveController __instance, Exception __exception)
        {
            if (__exception == null) return null;

            if (Time.unscaledTime - _saveLogTime > 5f)
            {
                _saveLogTime = Time.unscaledTime;
                string rn = "null";
                try { if (__instance != null && __instance.Room != null) rn = __instance.Room.name; } catch { }
                Plugin.Log.LogWarning("[RL2MP] [ROOM] 房间 '" + rn
                    + "' 存档状态加载抛异常，已就地吞掉并继续相机/门初始化: " + __exception);
            }
            return null;
        }

        /// <summary>
        /// PlayerController.EnterRoom 的 postfix：进房后确认脚下真的有地。
        /// 原生落点用 door.CenterPoint 计算，房间没初始化时它是 (0,0)，
        /// 人就被放到房间外或悬空处 —— 直接掉虚空。这里发现脚下 12m 内没有地板就救回来。
        /// </summary>
        private static void EnterRoomPostfix(PlayerController __instance, BaseRoom room)
        {
            try
            {
                if (__instance == null || room == null) return;
                if (!DownedState.InSession) return;   // 单机/回城流程一律不干预
                if (!(room is Room)) return;          // 城堡/庄园等非关卡房间不碰

                Vector3 pos = __instance.transform.position;
                Vector3 ground;
                if (TrySampleGround(pos, out ground)) return;

                RescueFromVoid(__instance, room, pos);
            }
            catch { /* 落点自检失败绝不能影响正常进房 */ }
        }

        private static int CountDoorsSafe(BaseRoom room)
        {
            try { return room != null && room.Doors != null ? room.Doors.Count : 0; }
            catch { return 0; }
        }
    }
}
