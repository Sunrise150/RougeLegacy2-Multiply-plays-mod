using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v3.0.20：门连接体检与修复。
    ///
    /// 反编译结论（这是“主机一死客机就永远出不去”的真正原因）：
    ///  · Door.ConnectedDoor 只在 <see cref="Door"/> 的 OnTriggerEnter2D 里赋值，
    ///    也就是“两间房的门碰撞体同时激活、发生物理重叠”的那一瞬间。链一旦没接上，
    ///    之后没有任何机制会补 —— 我们自己的“关旧房 / 开新房”正好会破坏这个前提。
    ///  · BaseRoom.PlayerExit 在 ConnectedDoor != null 但 ConnectedDoor.Room == null 时
    ///    【静默什么都不做】：不报错、不转场。玩家表现就是“走到门口没反应”。
    ///  · 门洞里的地面属于隔壁房间；隔壁房间没激活时门洞就是个洞，玩家走过去直接坠落，
    ///    被 RoomFix 救回门口，再走再掉 —— 真机日志里 3*Standard* 右门 x≈133 处反复坠落。
    ///  · 我们自己的兜底 CheckCrossedDoor / CheckHardExit 恰好把 ConnectedDoor 为空的门
    ///    全部 skip 掉：唯一必坏的门，正是兜底绝不会碰的门。
    ///
    /// 做法（尽量复用游戏自己的机制，不另造轮子）：
    ///  1) 触发器没建/没启用 → 调用原生 Door.OnPlayerEnterRoom（它负责建 exit trigger、
    ///     建门沿 ledge、并启用触发器），再补一次 PlayerEnterTrigger 让原生协程跑起来。
    ///  2) 门没连上 → 按门的网格坐标在本地找对面那扇门；对面门没有归属房间时，
    ///     用公开的 Door.SetRoom 补上；然后把两扇门互连（写 m_connectedDoor +
    ///     派发 m_doorConnectRelay），等价于 Door.OnTriggerEnter2D + Room.OnDoorConnect。
    ///  3) 本地根本没有隔壁房间 → 用游戏同款 BoxCollider2D 在门洞补一块地板
    ///     （Door.CreateLedge 就是这么做的），玩家不再掉进虚空，同时打醒目错误日志。
    ///  4) 每 5 秒输出一行“当前房间所有门”的体检数据，把神秘卡门变成可读日志。
    /// </summary>
    internal static class DoorLinkFix
    {
        private static FieldInfo _connectedDoorField;      // Door.m_connectedDoor
        private static FieldInfo _connectRelayField;      // Door.m_doorConnectRelay
        private static MethodInfo _doorEnterRoomMethod;   // Door.OnPlayerEnterRoom
        private static MethodInfo _playerEnterTrigger;    // PlayerTrigger.PlayerEnterTrigger
        private static FieldInfo _exitTriggerField;       // Door.m_exitTrigger

        private static float _healthTimer;
        private static float _nearDoorLogTime;
        private static readonly HashSet<int> _deadDoorReported = new HashSet<int>();
        private static readonly Dictionary<int, GameObject> _gapFillers = new Dictionary<int, GameObject>();

        // ---------------- 入口 ----------------

        /// <summary>由 RoomFix 的巡检每 0.5 秒调一次。</summary>
        internal static void Tick(float dt)
        {
            if (!DownedState.InSession) return;

            var ps = PlayerSync.Instance;
            var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
            if (pc == null || DownedState.LocalDown) return;

            BaseRoom room = null;
            try { room = pc.CurrentlyInRoom; } catch { }
            if (room == null) return;

            _healthTimer += dt;
            bool health = _healthTimer >= 5f;
            if (health) _healthTimer = 0f;

            try
            {
                var doors = room.Doors;
                if (doors == null) return;
                Vector3 playerPos = pc.transform.position;
                for (int i = 0; i < doors.Count; i++)
                {
                    var d = doors[i];
                    if (d == null) continue;
                    Inspect(room, d, health, playerPos);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 门体检异常: " + ex.Message);
            }
        }

        internal static void Reset()
        {
            _healthTimer = 0f;
            _nearDoorLogTime = 0f;
            _deadDoorReported.Clear();
            foreach (var kv in _gapFillers)
            {
                try { if (kv.Value != null) UnityEngine.Object.Destroy(kv.Value); } catch { }
            }
            _gapFillers.Clear();
        }

        // ---------------- 单扇门 ----------------

        private static void Inspect(BaseRoom room, Door door, bool health, Vector3 playerPos)
        {
            bool linked = door.ConnectedDoor != null && door.ConnectedDoor.Room != null;

            if (linked)
            {
                // 已连上：仍然要保证触发器处于武装状态（原生只在进房那一刻武装一次，
                // 中途被关掉的门不会自己恢复）。
                EnsureArmed(room, door);
                if (health) LogHealth(room, door, "正常");
                WarnIfDeadAtDoor(room, door, playerPos);
                return;
            }

            // ---- 未连上：先按原生方式武装，再尝试修复连接 ----
            EnsureArmed(room, door);

            Door neighbour = FindNeighbourDoor(room, door);
            if (neighbour != null)
            {
                BaseRoom nroom = neighbour.Room;
                if (nroom == null)
                {
                    BaseRoom owner = FindOwnerRoom(room, neighbour);
                    if (owner != null)
                    {
                        try { neighbour.SetRoom(owner); } catch { }
                        nroom = neighbour.Room;
                    }
                }
                if (nroom != null)
                {
                    LinkDoors(door, neighbour);
                    Plugin.Log.LogWarning("[RL2MP] [DOOR] 修复门连接：房间 '" + room.name
                        + "' 的门 " + door.Side + "-" + door.Number + " 之前是断链，"
                        + "已连到 '" + nroom.name + "'。");
                    return;
                }
            }

            // ---- 本地没有可连的隔壁房间：先保证玩家不会掉进虚空 ----
            FillGap(room, door);
            WarnIfDeadAtDoor(room, door, playerPos);
            int key = door.GetInstanceID();
            if (!_deadDoorReported.Contains(key))
            {
                _deadDoorReported.Add(key);
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 房间 '" + room.name + "' 的门 "
                    + door.Side + "-" + door.Number + "（中心 " + door.CenterPoint
                    + "）找不到可连接的隔壁房间：原生 PlayerExit 会静默失败。"
                    + "已用补地板挡住门洞，避免玩家反复坠落。");
            }
            if (health) LogHealth(room, door, "断链(已补地板)");
        }

        /// <summary>用原生 Door.OnPlayerEnterRoom 武装这扇门：建 exit trigger + 门沿 + 启用。</summary>
        internal static void ArmDoor(BaseRoom room, Door door)
        {
            if (door == null) return;
            EnsureArmed(room, door);
        }

        private static void EnsureArmed(BaseRoom room, Door door)
        {
            try
            {
                var trig = GetExitTrigger(door);
                bool need = trig == null || !trig.IsTriggerActive;
                if (!need) return;

                if (_doorEnterRoomMethod == null)
                    _doorEnterRoomMethod = AccessTools.Method(typeof(Door), "OnPlayerEnterRoom");
                if (_doorEnterRoomMethod != null)
                {
                    var args = new RoomViaDoorEventArgs(room, door);
                    _doorEnterRoomMethod.Invoke(door, new object[] { door, args });
                }

                // OnPlayerEnterRoom 内部 SetTriggerActive(true) 会 Reset() 清掉
                // IsPlayerInTrigger；玩家若正站在门里，必须补一次进入事件让原生协程启动。
                trig = GetExitTrigger(door);
                if (trig != null && trig.IsTriggerActive && !trig.IsPlayerInTrigger)
                {
                    try
                    {
                        var col = trig.GetComponent<Collider2D>();
                        var ps = PlayerSync.Instance;
                        var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                        if (col != null && pc != null && col.bounds.Contains(pc.transform.position))
                        {
                            if (_playerEnterTrigger == null)
                                _playerEnterTrigger = AccessTools.Method(typeof(PlayerTrigger), "PlayerEnterTrigger");
                            if (_playerEnterTrigger != null)
                            {
                                _playerEnterTrigger.Invoke(trig, null);
                                Plugin.Log.LogInfo("[RL2MP] [DOOR] 已重新武装并补发进入事件：门 "
                                    + door.Side + "-" + door.Number);
                            }
                        }
                    }
                    catch { }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 武装门失败: " + ex.Message);
            }
        }

        /// <summary>按网格坐标 / 门碰撞体位置在本地找对面那扇门（游戏用物理重叠连门，我们用几何找）。</summary>
        private static Door FindNeighbourDoor(BaseRoom room, Door door)
        {
            RoomSide want = Opposite(door.Side);
            Vector2 myCoord = door.Coordinates;
            bool myCoordValid = Mathf.Abs(myCoord.x) + Mathf.Abs(myCoord.y) > 0.05f;
            Vector2 myCenter = door.CenterPoint;
            bool myCenterValid = Mathf.Abs(myCenter.x) + Mathf.Abs(myCenter.y) > 0.5f;
            Bounds myBounds = DoorBounds(door);
            bool myBoundsValid = myBounds.size.x > 0.01f && myBounds.size.y > 0.01f;

            Door best = null;
            float bestScore = float.MaxValue;
            try
            {
                var rooms = Resources.FindObjectsOfTypeAll<BaseRoom>();
                for (int i = 0; i < rooms.Length; i++)
                {
                    var r = rooms[i];
                    if (r == null || ReferenceEquals(r, room)) continue;
                    if (!r.gameObject.scene.IsValid()) continue;
                    // 只认同一个场景里的房间：庄园/大厅的房间里也有门，坐标可能撞车。
                    if (r.gameObject.scene != room.gameObject.scene) continue;
                    var doors = r.Doors;
                    if (doors == null) continue;
                    for (int j = 0; j < doors.Count; j++)
                    {
                        var d2 = doors[j];
                        if (d2 == null || d2.Side != want) continue;
                        if (ReferenceEquals(d2, door)) continue;

                        float score = float.MaxValue;
                        if (myCoordValid)
                        {
                            Vector2 c2 = d2.Coordinates;
                            if (Mathf.Abs(c2.x) + Mathf.Abs(c2.y) > 0.05f)
                            {
                                score = (c2 - myCoord).magnitude;
                            }
                        }
                        if (score == float.MaxValue && myBoundsValid)
                        {
                            // 合并房这类门的 Coordinates/CenterPoint 都是 (0,0)，只能靠
                            // 门碰撞体的世界位置判断：成对的两扇门就在同一条边界上。
                            Bounds b2 = DoorBounds(d2);
                            if (b2.size.x > 0.01f && b2.size.y > 0.01f)
                            {
                                float dx = Mathf.Abs(b2.center.x - myBounds.center.x);
                                float dy = Mathf.Abs(b2.center.y - myBounds.center.y);
                                if (dx <= 3f && dy <= 3f) score = dx + dy;
                            }
                        }
                        if (score == float.MaxValue && myCenterValid)
                        {
                            Vector2 c2 = d2.CenterPoint;
                            if (Mathf.Abs(c2.x) + Mathf.Abs(c2.y) > 0.5f)
                                score = (c2 - myCenter).magnitude;
                        }
                        if (score == float.MaxValue) continue;

                        if (score < bestScore) { bestScore = score; best = d2; }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 查找隔壁门异常: " + ex.Message);
                return null;
            }
            return bestScore <= 3f ? best : null;
        }

        /// <summary>取门自身碰撞体的世界包围盒（门洞那圈地形）。</summary>
        private static Bounds DoorBounds(Door door)
        {
            try
            {
                var col = door.GetComponent<Collider2D>();
                if (col != null) return col.bounds;
            }
            catch { }
            return default(Bounds);
        }

        /// <summary>找出某个门所属的房间（Rooms 列表里包含它的那个房间）。</summary>
        private static BaseRoom FindOwnerRoom(BaseRoom current, Door door)
        {
            try
            {
                var rooms = Resources.FindObjectsOfTypeAll<BaseRoom>();
                for (int i = 0; i < rooms.Length; i++)
                {
                    var r = rooms[i];
                    if (r == null || !r.gameObject.scene.IsValid()) continue;
                    if (current != null && r.gameObject.scene != current.gameObject.scene) continue;
                    var doors = r.Doors;
                    if (doors == null) continue;
                    for (int j = 0; j < doors.Count; j++)
                        if (ReferenceEquals(doors[j], door)) return r;
                }
            }
            catch { }
            return null;
        }

        /// <summary>把两扇门互连，等价于游戏的 Door.OnTriggerEnter2D + Room.OnDoorConnect。</summary>
        private static void LinkDoors(Door a, Door b)
        {
            try
            {
                if (_connectedDoorField == null)
                    _connectedDoorField = AccessTools.Field(typeof(Door), "m_connectedDoor");
                if (_connectedDoorField == null) return;
                _connectedDoorField.SetValue(a, b);
                _connectedDoorField.SetValue(b, a);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 写 ConnectedDoor 失败: " + ex.Message);
                return;
            }

            // 让房间的连接簿记也补上（Room.OnDoorConnect -> AddConnectedRoom）
            try
            {
                if (_connectRelayField == null)
                    _connectRelayField = AccessTools.Field(typeof(Door), "m_doorConnectRelay");
                var relay = _connectRelayField != null ? _connectRelayField.GetValue(a) : null;
                if (relay != null)
                {
                    var relayType = relay.GetType();
                    MethodInfo dispatch = null;
                    var methods = relayType.GetMethods();
                    for (int i = 0; i < methods.Length; i++)
                    {
                        if (methods[i].Name != "Dispatch") continue;
                        var ps = methods[i].GetParameters();
                        if (ps.Length == 2) { dispatch = methods[i]; break; }
                    }
                    if (dispatch != null)
                    {
                        Type argsType = null;
                        var ga = relayType.GetGenericArguments();
                        if (ga.Length >= 2) argsType = ga[1];
                        if (argsType == null)
                            argsType = Type.GetType("DoorConnectEventArgs, Assembly-CSharp");
                        if (argsType != null)
                        {
                            var evt = Activator.CreateInstance(argsType, new object[] { a, b });
                            dispatch.Invoke(relay, new object[] { a, evt });
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 派发门连接事件失败（不影响连门）: " + ex.Message);
            }
        }

        /// <summary>隔壁房间本地不存在时，在门洞补一块地板，别让玩家掉进虚空。</summary>
        private static void FillGap(BaseRoom room, Door door)
        {
            // 只有左右门（横向门洞）需要补地板；上下门是竖井，补了反而挡路。
            if (door.Side != RoomSide.Left && door.Side != RoomSide.Right) return;

            int key = door.GetInstanceID();
            if (_gapFillers.ContainsKey(key)) return;

            try
            {
                Vector2 c = door.CenterPoint;
                if (Mathf.Abs(c.x) + Mathf.Abs(c.y) < 0.5f) return;

                float groundY = c.y;
                var hits = Physics2D.RaycastAll(new Vector2(c.x, c.y), Vector2.down, 14f);
                if (hits != null && hits.Length > 0)
                {
                    float best = float.MaxValue;
                    for (int i = 0; i < hits.Length; i++)
                    {
                        var col = hits[i].collider;
                        if (col == null || col.isTrigger) continue;
                        if (col.GetComponentInParent<BaseCharacterController>() != null) continue;
                        if (hits[i].distance < best) best = hits[i].distance;
                    }
                    if (best < float.MaxValue) groundY = c.y - best;
                }

                var go = new GameObject("RL2MP_DoorGap_" + door.Side + door.Number);
                int layer = LayerMask.NameToLayer("Platform_CollidesWithAll");
                if (layer >= 0) go.layer = layer;
                go.transform.position = new Vector3(c.x, groundY + 0.5f, 0f);
                var box = go.AddComponent<BoxCollider2D>();
                box.size = new Vector2(12f, 1f);
                _gapFillers[key] = go;
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 已在门洞补地板（位置 " + c.x.ToString("F1")
                    + "," + groundY.ToString("F1") + "），避免玩家坠入门洞。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 补门洞地板失败: " + ex.Message);
            }
        }

        // ---------------- 诊断 ----------------

        /// <summary>
        /// v3.0.24：玩家就站在门口、而这扇门的出口触发器仍然没武装 —— 这就是
        /// "门触发器失效"的现场。之前只有 5 秒一次的体检日志，且只在门已经坏掉时才有；
        /// 这里在"人已经走到门口却推不开"的当下就打一条带完整现场的警告，
        /// 真机日志里一眼能看出是哪扇门、什么状态。
        /// </summary>
        private static void WarnIfDeadAtDoor(BaseRoom room, Door door, Vector3 playerPos)
        {
            try
            {
                Vector2 c = door.CenterPoint;
                if (Mathf.Abs(c.x) + Mathf.Abs(c.y) < 0.5f) return;   // 坐标不可信的门（合并房）

                float dist = Vector2.Distance(new Vector2(playerPos.x, playerPos.y), c);
                if (dist > 3.5f) return;                              // 玩家不在这扇门口

                var trig = GetExitTrigger(door);
                if (trig != null && trig.IsTriggerActive) return;      // 触发器已武装，正常

                if (Time.unscaledTime - _nearDoorLogTime < 3f) return;
                _nearDoorLogTime = Time.unscaledTime;

                string linked = door.ConnectedDoor == null ? "无ConnectedDoor"
                    : (door.ConnectedDoor.Room == null ? "有门无Room"
                        : door.ConnectedDoor.Room.name);
                Plugin.Log.LogWarning("[RL2MP] [DOOR] 玩家就在门口但触发器没武装：房间 '"
                    + room.name + "' 门 " + door.Side + "-" + door.Number
                    + " 距离=" + dist.ToString("F1") + "m"
                    + " 触发器=" + (trig == null ? "无" : "有/关闭")
                    + " 对面=" + linked
                    + " —— 这就是'门触发器失效'的现场。");
            }
            catch { }
        }

        private static void LogHealth(BaseRoom room, Door door, string tag)
        {
            try
            {
                var trig = GetExitTrigger(door);
                string linked = door.ConnectedDoor == null ? "无ConnectedDoor"
                    : (door.ConnectedDoor.Room == null ? "对面门无Room"
                        : door.ConnectedDoor.Room.name);
                bool active = trig != null && trig.IsTriggerActive;
                bool inTrig = trig != null && trig.IsPlayerInTrigger;
                Plugin.Log.LogInfo("[RL2MP] [DOOR] 体检 '" + room.name + "' 门 "
                    + door.Side + "-" + door.Number + " 中心=" + door.CenterPoint
                    + " 对面=" + linked + " 触发器=" + (trig != null ? "有" : "无")
                    + (active ? "/启用" : "/关闭") + (inTrig ? "/人在内" : "") + " -> " + tag);
            }
            catch { }
        }

        private static PlayerTrigger GetExitTrigger(Door door)
        {
            try
            {
                if (_exitTriggerField == null)
                    _exitTriggerField = AccessTools.Field(typeof(Door), "m_exitTrigger");
                return _exitTriggerField != null ? _exitTriggerField.GetValue(door) as PlayerTrigger : null;
            }
            catch { return null; }
        }

        private static RoomSide Opposite(RoomSide side)
        {
            switch (side)
            {
                case RoomSide.Left: return RoomSide.Right;
                case RoomSide.Right: return RoomSide.Left;
                case RoomSide.Top: return RoomSide.Bottom;
                case RoomSide.Bottom: return RoomSide.Top;
            }
            return RoomSide.None;
        }
    }
}
