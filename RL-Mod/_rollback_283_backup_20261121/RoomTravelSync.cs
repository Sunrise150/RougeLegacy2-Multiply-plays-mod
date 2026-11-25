using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 协作模式：全员换房（一个人进房，所有人进房）。
    ///
    /// v2.1.0 可靠性重写（用户反馈"传送有时候有用有时候没用"）：
    ///   原来有三个会静默丢包的点，全都是单次机会，错过就再也不会跟随：
    ///     1) Tick 里先把 _pending 清掉再尝试应用 —— 应用失败的包直接消失；
    ///     2) _suppressUntil 窗口内的包被 ApplyNow 直接 return 丢弃；
    ///     3) 广播只发一次，对端正好在加载（当前房间为 null）或丢包就彻底漏掉。
    ///   现在改成"不丢包 + 重发 + 幂等"：
    ///     a) _pending 只有"应用成功"或"确定不可重试"才清；期间每 0.25s 重试一次，
    ///        最长 4 秒（房间生成/加载完成通常远小于这个时间）。
    ///     b) 过门广播带自增序号 seq，并在 +0.35s / +1.0s 各补发一次（接收端幂等+序号去重），
    ///        对端在加载中也能在恢复后拿到最新一条。
    ///     c) 抑制窗从 1.8s 缩到 0.8s，并且不再"窗内一律不广播"，而是判断
    ///        "这次 PlayerExit 的目标是不是刚离开的那个房间"——只有弹回原房间才算回声，
    ///        玩家刚跟随落地又立刻走向别的房间仍会正常广播。
    ///     d) 房间表缓存遇到已销毁的 Room 引用会强制重建（换层时对象会被销毁重建）。
    ///
    /// 历史要点（仍有效）：
    ///   - 不能把玩家直接放门上：门触发器在之后某一帧触发 OnPlayerExitRoom → 又广播回去（ping-pong）。
    ///     落点按 RoomUtility.GetDoorCenterPoint 算，再朝房间内侧缩 2m。
    ///   - 绕开原生 BaseRoom.PlayerExit 会让目标房间拿不到退房阶段数据，
    ///     PlacePlayerInRoom_End 里 OnPlayerEnter_LoadStageData 抛 NRE，所以补做 BroadcastPlayerExitRoomEvents。
    ///   - 收包只在 LateUpdate 应用（收包回调里改场景容易重入卡死）。
    ///
    /// 只在协作 + Connected 时生效；竞速/单人完全不碰。
    /// </summary>
    internal static class RoomTravelSync
    {
        /// <summary>协作全员换房开关（仅 UI 显示用；v2.0.1 起由玩法模式自动决定）。</summary>
        internal static bool Enabled
        {
            get { return Plugin.CoOpRoomSyncCfg != null && Plugin.CoOpRoomSyncCfg.Value; }
            set { if (Plugin.CoOpRoomSyncCfg != null) Plugin.CoOpRoomSyncCfg.Value = value; }
        }

        internal static bool Active
        {
            get
            {
                return Score.Style == PlayStyle.CoOp && DownedState.InSession
                       && NetworkManager.Instance != null && NetworkManager.Instance.Connected;
            }
        }

        // ---------------- 回声抑制 ----------------
        private static bool _applyingRemote;
        /// <summary>本端 PlayerExit 在此时间点之前需要判断是不是回声（不是一律抑制）。</summary>
        private static float _suppressUntil;
        private const float SuppressWindow = 0.8f;
        /// <summary>跟随前所在的房间坐标：若窗内 PlayerExit 的目标正好是它，就是弹回原房间的回声。</summary>
        private static Vector2? _suppressReturnCoords;

        // ---------------- 待应用队列（收包 -> LateUpdate） ----------------
        private static bool _pending;
        private static int _pendingSender;
        private static float _pendingX, _pendingY;
        private static float _pendingPX, _pendingPY;   // v2.3.2：目标房间内的实际世界坐标
        private static byte _pendingSide;
        private static int _pendingNumber;
        private static ushort _pendingSeq;
        private static float _pendingAt;
        private static float _pendingNextTryAt;
        private static int _pendingTries;
        private const float PendingTtl = 4f;      // 4 秒还找不到/进不去就放弃这一包
        private const int MaxTries = 14;
        private const float RetryInterval = 0.25f;

        // ---------------- 本地过门意图（v2.3.2） ----------------
        /// <summary>已经过门、等新房间 PlacePlayerInRoom_End 就绪后再广播。</summary>
        private static bool _exitArmed;
        private static float _exitAt;
        private static BaseRoom _exitFromRoom;
        private static byte _exitSide;
        private static int _exitNumber;

        // ---------------- 序号去重 ----------------
        private static readonly Dictionary<int, ushort> _lastSeqBySender = new Dictionary<int, ushort>();
        private static ushort _sendSeq;

        // ---------------- 广播补发 ----------------
        private struct Resend
        {
            public float At;
            public float X, Y;
            public float PX, PY;      // v2.3.2：目标房间里的实际世界坐标
            public byte Side;
            public int Number;
            public ushort Seq;
        }
        private static readonly List<Resend> _resends = new List<Resend>(4);

        // ---------------- 限流刹车 ----------------
        private static float _burstStart;
        private static int _burstCount;
        private const int BurstLimit = 8;
        private const float BurstWindow = 3f;

        private static float _lastLog;

        private static void LogThrottled(string msg, bool warn = false)
        {
            if (Time.unscaledTime - _lastLog < 0.5f) return;
            _lastLog = Time.unscaledTime;
            if (warn) Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] " + msg);
            else Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] " + msg);
        }

        // ---------------- 发送端 ----------------

        /// <summary>
        /// patch PlayerExit 的 prefix：只记录"本地玩家要过门了"，不在这里广播。
        ///
        /// v2.3.2：**这里是"没一起传送"的根因**。此刻目标房间往往还没激活，
        /// `door.ConnectedDoor.Room.Coordinates` 是未初始化的 (0,0)（真机日志：
        /// `广播过门 -> 房间(0,0) 门 side=3 num=0`，而人明明是从 (0,0) 的右门进了 (64,0)）。
        /// 对端拿到 (0,0) 就会认为"已经在那间房了"，直接幂等跳过 —— 于是谁也没跟过来。
        /// 正确时机是新房间 PlacePlayerInRoom 跑完（那边坐标和落点都是真的）。
        /// </summary>
        private static void PlayerExitPrefix(BaseRoom __instance, Door door)
        {
            if (_applyingRemote) return;

            // 本端主动换房优先：正在重试的远端换房请求作废，否则会把人拽回去
            _pending = false;
            if (!Active) return;

            try
            {
                _exitArmed = true;
                _exitAt = Time.unscaledTime;
                _exitFromRoom = __instance;
                _exitSide = 0;
                _exitNumber = 0;
                if (door != null)
                {
                    var cd = door.ConnectedDoor;
                    if (cd != null)
                    {
                        _exitSide = (byte)cd.Side;
                        _exitNumber = cd.Number;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] PlayerExitPrefix 异常: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>新房间 PlacePlayerInRoom_End 之后调用：此时坐标/落点才可信，在这里广播。</summary>
        private static void PlacePlayerInRoomEndPostfix(BaseRoom __instance, Door door)
        {
            TryAnnounceEnteredRoom(__instance, "postfix");
        }

        /// <summary>
        /// 本地玩家真正落进一间新房间后，把"目标房间 + 实际世界坐标"广播出去。
        /// 由 PlacePlayerInRoom_End postfix 和 RoomFix 的异常兜底两处调用，
        /// `_exitArmed` 保证只发一次。
        /// </summary>
        internal static void TryAnnounceEnteredRoom(BaseRoom room, string from)
        {
            if (_applyingRemote || !_exitArmed) return;
            if (Time.unscaledTime - _exitAt > 3f) { _exitArmed = false; return; }
            if (!Active || room == null) { _exitArmed = false; return; }

            try
            {
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return;      // 还没抓到角色，等下一次机会

                Vector3 wp = pc.transform.position;
                if (Mathf.Abs(wp.x) < 0.01f && Mathf.Abs(wp.y) < 0.01f) return;   // 位置未就绪

                var dest = room as Room;
                Vector2 coords = dest != null ? dest.Coordinates : (Vector2)room.transform.position;

                // 回声：
                //   a) 目标就是"刚跟随弹回去的那间房"；
                //   b) 根本没换房（room 还是离开的那间）——延迟进房/大房间过渡会走这条。
                if (Time.unscaledTime < _suppressUntil && _suppressReturnCoords.HasValue
                    && CoordsMatch(coords, _suppressReturnCoords.Value.x, _suppressReturnCoords.Value.y))
                {
                    _exitArmed = false;
                    return;
                }
                if (_exitFromRoom != null && ReferenceEquals(_exitFromRoom, room))
                {
                    _exitArmed = false;      // 还在原来那间房（协程延迟/大房间过渡），不广播
                    return;
                }

                _exitArmed = false;

                ushort seq = ++_sendSeq;
                BroadcastTravel(coords.x, coords.y, _exitSide, _exitNumber, seq, wp.x, wp.y);
                ScheduleResends(coords, _exitSide, _exitNumber, seq, wp.x, wp.y);

                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 广播换房 -> 房间(" + coords.x + "," + coords.y
                        + ") 落点(" + wp.x.ToString("F1") + "," + wp.y.ToString("F1")
                        + ") 门 side=" + _exitSide + " num=" + _exitNumber + " seq=" + seq
                        + " via=" + from);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] TryAnnounceEnteredRoom 异常: " + Plugin.PatchError(ex));
            }
        }

        private static void BroadcastTravel(float cx, float cy, byte side, int number, ushort seq,
            float px, float py)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            var w = new LiteNetLib.Utils.NetDataWriter();
            w.Put((byte)MsgType.RoomTravel);
            w.Put(nm.LocalPlayerId);
            w.Put(seq);
            w.Put(cx); w.Put(cy);
            w.Put(px); w.Put(py);
            w.Put(side); w.Put(number);
            nm.SendRoomTravel(w);
        }

        /// <summary>过门后补发两次：对端在加载中/丢包时也能在恢复后拿到（接收端幂等+序号去重）。</summary>
        private static void ScheduleResends(Vector2 coords, byte side, int number, ushort seq,
            float px, float py)
        {
            _resends.Clear();
            float now = Time.unscaledTime;
            _resends.Add(new Resend { At = now + 0.35f, X = coords.x, Y = coords.y, PX = px, PY = py, Side = side, Number = number, Seq = seq });
            _resends.Add(new Resend { At = now + 1.0f, X = coords.x, Y = coords.y, PX = px, PY = py, Side = side, Number = number, Seq = seq });
        }

        private static void TickResends(float now)
        {
            if (_resends.Count == 0) return;
            for (int i = _resends.Count - 1; i >= 0; i--)
            {
                var r = _resends[i];
                if (now < r.At) continue;
                _resends.RemoveAt(i);

                // 玩家已经又去了别处（新的广播会带更大 seq），这条旧补发就别再拉别人了
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) continue;
                var curRoom = pc.CurrentlyInRoom as Room;
                if (curRoom != null && CoordsMatch(curRoom.Coordinates, r.X, r.Y))
                {
                    BroadcastTravel(r.X, r.Y, r.Side, r.Number, r.Seq, r.PX, r.PY);
                }
            }
        }

        // ---------------- 接收端 ----------------

        /// <summary>收到其他端的换房广播：只登记，实际传送留给 LateUpdate。</summary>
        internal static void ApplyTravel(int senderId, ushort seq, float cx, float cy, byte side, int number,
            float px, float py)
        {
            if (!Active || NetworkManager.Instance == null) return;
            if (senderId == NetworkManager.Instance.LocalPlayerId) return;

            // 序号去重：只丢弃"比已处理的更旧"的包。相同的 seq 是补发包，允许通过
            // （应用失败重试超时后，补发是最后一次自救机会；已在目标房间时应用是幂等的）
            ushort last;
            if (_lastSeqBySender.TryGetValue(senderId, out last) && seq != last)
            {
                if (!IsNewerSeq(seq, last)) return;
            }
            _lastSeqBySender[senderId] = seq;

            _pending = true;
            _pendingSender = senderId;
            _pendingX = cx; _pendingY = cy;
            _pendingPX = px; _pendingPY = py;
            _pendingSide = side; _pendingNumber = number;
            _pendingSeq = seq;
            _pendingAt = Time.unscaledTime;
            _pendingNextTryAt = 0f;
            _pendingTries = 0;
        }

        private static bool IsNewerSeq(ushort seq, ushort last)
        {
            if (last == 0) return true;
            return unchecked((ushort)(seq - last)) < 0x8000;
        }

        /// <summary>由 NetworkManager.LateUpdate 每帧驱动：应用待处理的远端换房（失败会重试，不丢包）。</summary>
        internal static void Tick()
        {
            float now = Time.unscaledTime;
            VerifyLanding(now);   // 先做上一次跟随的落地自检
            TickResends(now);

            if (!_pending) return;
            if (!Active) { _pending = false; return; }

            if (now - _pendingAt > PendingTtl)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 跟随超时放弃：房间(" + _pendingX + "," + _pendingY
                    + ") 重试 " + _pendingTries + " 次仍未成功。");
                _pending = false;
                return;
            }

            if (now < _pendingNextTryAt) return;

            // 正在消化上一次跟随：延后，而不是丢包（旧版就是在这里丢的）
            if (now < _suppressUntil)
            {
                _pendingNextTryAt = _suppressUntil + 0.05f;
                return;
            }
            if (_applyingRemote)
            {
                _pendingNextTryAt = now + 0.05f;
                return;
            }

            int sender = _pendingSender;
            float cx = _pendingX, cy = _pendingY;
            float px = _pendingPX, py = _pendingPY;
            byte side = _pendingSide;
            int number = _pendingNumber;

            ApplyResult result = ApplyNow(sender, cx, cy, side, number, px, py, now);
            if (result == ApplyResult.Retry && _pendingTries < MaxTries)
            {
                _pendingTries++;
                _pendingNextTryAt = now + RetryInterval;
                return;   // 保留 _pending，下一轮再试
            }
            _pending = false;
        }

        private enum ApplyResult { Success, Retry, Drop }

        private static ApplyResult ApplyNow(int senderId, float cx, float cy, byte side, int number,
            float px, float py, float now)
        {
            try
            {
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return ApplyResult.Retry;              // 本地角色还没进场景：稍后再试
                if (pc.IsDead || DownedState.LocalDown) return ApplyResult.Drop;

                var cur = pc.CurrentlyInRoom;
                if (cur == null) return ApplyResult.Retry;             // 加载中：稍后再试

                var curRoom = cur as Room;
                Vector2 curCoords = curRoom != null ? curRoom.Coordinates : (Vector2)cur.transform.position;
                if (CoordsMatch(curCoords, cx, cy)) return ApplyResult.Success;   // 已经在目标房间（幂等）

                // v2.3.2：优先用对端"实际落地的世界坐标"定位房间。
                // 房间坐标 cx/cy 在旧版里经常是未初始化的 (0,0)，靠它永远找不着。
                bool havePos = Mathf.Abs(px) + Mathf.Abs(py) > 0.05f;
                Room dest = havePos ? FindRoomAt(new Vector3(px, py, 0f)) : null;
                bool byPos = dest != null;
                if (dest == null) dest = FindRoomByCoords(cx, cy);
                if (dest == null)
                {
                    LogThrottled("未找到目标房间(" + cx + "," + cy + " / 落点 "
                        + px.ToString("F1") + "," + py.ToString("F1") + ")，稍后重试。", true);
                    return ApplyResult.Retry;                          // 房间可能还没生成
                }
                if (byPos) { cx = dest.Coordinates.x; cy = dest.Coordinates.y; }

                // 限流刹车：短时间内疯狂换房必然是回声，强制停一段时间
                if (now - _burstStart > BurstWindow) { _burstStart = now; _burstCount = 0; }
                if (++_burstCount > BurstLimit)
                {
                    _suppressUntil = now + 3f;
                    Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] " + BurstWindow + "s 内换房超过 " + BurstLimit
                        + " 次，判定为回声循环，暂停跟随 3 秒。");
                    return ApplyResult.Drop;
                }

                _suppressUntil = now + SuppressWindow;
                _suppressReturnCoords = curCoords;   // 弹回这里 = 回声
                _entranceRetried = false;
                _applyingRemote = true;
                bool viaDoor = false;
                try
                {
                    // 落点按"入口那扇门"算，再朝房间内侧缩 2 米（详见 TryGetEntranceLocal）
                    Vector3 entranceLocal;
                    // v2.3.2：首选对端报来的实际落点（世界坐标）。它一定在目标房间的实地附近，
                    // 比"按门推算"更准，也不依赖未激活房间的门数据。
                    if (havePos && TryWorldLandingLocal(dest, px, py, out entranceLocal))
                    {
                        viaDoor = true;

                        try { cur.BroadcastPlayerExitRoomEvents(null); }
                        catch (Exception e) { Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 退房收尾异常: " + e.Message); }
                        try { cur.gameObject.SetActive(false); } catch { }
                        if (!dest.gameObject.activeInHierarchy) dest.gameObject.SetActive(true);
                        dest.PlacePlayerInRoom(entranceLocal);
                        _lastEntranceLocal = entranceLocal;
                    }
                    else if (TryGetEntranceLocal(dest, side, number, out entranceLocal))
                    {
                        viaDoor = true;

                        // 1) 原生退房收尾：存当前房间阶段数据 + 派发退房事件
                        //    （少了这步，目标房间 PlacePlayerInRoom_End 里
                        //      SaveController.OnPlayerEnter_LoadStageData 会抛 NRE）
                        try { cur.BroadcastPlayerExitRoomEvents(null); }
                        catch (Exception e) { Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 退房收尾异常: " + e.Message); }

                        // 2) 停旧房、激活新房：先把地板/碰撞体准备好再放人
                        try { cur.gameObject.SetActive(false); } catch { }
                        if (!dest.gameObject.activeInHierarchy) dest.gameObject.SetActive(true);

                        // 3) 门口落位，走完整进入流程（刷怪/背景/存档/相机/进入事件）
                        dest.PlacePlayerInRoom(entranceLocal);
                        _lastEntranceLocal = entranceLocal;
                    }
                    else if (dest.PlayerSpawn != null)
                    {
                        // 兜底：房间尺寸数据异常（例如没初始化）才退回默认出生点
                        try { cur.BroadcastPlayerExitRoomEvents(null); }
                        catch (Exception e) { Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 退房收尾异常: " + e.Message); }
                        try { cur.gameObject.SetActive(false); } catch { }
                        if (!dest.gameObject.activeInHierarchy) dest.gameObject.SetActive(true);
                        dest.PlacePlayerInRoom(dest.PlayerSpawn.transform.localPosition);
                        _lastEntranceLocal = dest.PlayerSpawn.transform.localPosition;
                    }
                    else
                    {
                        Door targetDoor = FindDoor(dest, side, number);
                        if (targetDoor == null)
                        {
                            LogThrottled("目标房间(" + cx + "," + cy + ")既无门也无出生点，跳过跟随。", true);
                            return ApplyResult.Drop;
                        }
                        try { cur.BroadcastPlayerExitRoomEvents(null); }
                        catch (Exception e) { Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 退房收尾异常: " + e.Message); }
                        cur.gameObject.SetActive(false);
                        dest.gameObject.SetActive(true);
                        dest.PlacePlayerInRoom(targetDoor);
                        _lastEntranceLocal = null;
                    }
                }
                finally
                {
                    _applyingRemote = false;
                }

                // 落地自检：0.5 秒后若人不在目标房间里（掉出房间/卡墙外），拉回入口
                _verifyDest = dest;
                _verifyAt = Time.unscaledTime + 0.5f;

                RoomFix.RequestVerify();   // 让 RoomFix 重新体检相机/空怪
                int tries = _pendingTries;
                Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 已跟随 P" + senderId + " 换房 -> 房间("
                    + cx + "," + cy + ")" + (viaDoor ? " 入口落位" : " 出生点兜底")
                    + (tries > 0 ? ("（重试 " + tries + " 次后成功）") : ""));
                return ApplyResult.Success;
            }
            catch (Exception ex)
            {
                _applyingRemote = false;
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] ApplyNow 异常: " + Plugin.PatchError(ex));
                return ApplyResult.Retry;
            }
        }

        // ---------------- 落地自检 ----------------
        private static Room _verifyDest;
        private static float _verifyAt;
        /// <summary>上一次跟随用的落点（房间本地坐标），自检失败时优先回到它而不是出生点。</summary>
        private static Vector3? _lastEntranceLocal;
        /// <summary>入口落点已经重试过一次（还是掉出去就改投出生点，避免反复掉同一个坑）。</summary>
        private static bool _entranceRetried;

        /// <summary>
        /// 算出"从 side/number 这扇门进来时"玩家应站的位置（目标房间本地坐标）。
        /// 只依赖 Room.Coordinates + Room.Size + 门的 side/number，不依赖门对象是否激活，
        /// 因此不会像 Door.CenterPoint 那样在未激活房间里是 (0,0) 而把玩家丢进虚空。
        /// </summary>
        private static bool TryGetEntranceLocal(Room dest, byte side, int number, out Vector3 local)
        {
            local = Vector3.zero;
            if (dest == null) return false;
            try
            {
                RoomSide s = (RoomSide)side;
                if (s != RoomSide.Left && s != RoomSide.Right && s != RoomSide.Top && s != RoomSide.Bottom)
                    return false;   // None/Any：位置算不出来，交给兜底

                Vector2Int size = dest.Size;
                if (size.x <= 0 || size.y <= 0) return false;   // 房间尺寸没初始化

                var dloc = new DoorLocation(s, number);
                Vector2 doorCenter = RoomUtility.GetDoorCenterPoint(dest.Coordinates, size, dloc);

                // 门中心正落在房间边界上；朝内侧缩 2 米，站到门内的落脚台上。
                // （门的退出触发器整格在房间外侧，落点在内侧不会被它误触发——这点很关键，
                //   否则一落地就被门弹回上一个房间，表现为"来回换房"。）
                const float inset = 2f;
                if (s == RoomSide.Left) doorCenter.x += inset;
                else if (s == RoomSide.Right) doorCenter.x -= inset;
                else if (s == RoomSide.Bottom) doorCenter.y += inset;
                else if (s == RoomSide.Top) doorCenter.y -= inset;

                var world = new Vector3(doorCenter.x, doorCenter.y, dest.transform.position.z);
                local = dest.transform.InverseTransformPoint(world);
                local.z = 0f;
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 计算入口落点失败: " + ex.Message);
                return false;
            }
        }

        private static void VerifyLanding(float now)
        {
            if (_verifyDest == null || now < _verifyAt) return;
            var dest = _verifyDest;
            _verifyDest = null;

            try
            {
                if (dest == null) return;
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null || pc.IsDead || DownedState.LocalDown) return;

                Bounds b = dest.Bounds;
                Vector3 p = pc.transform.position;
                bool inside = p.x >= b.min.x - 3f && p.x <= b.max.x + 3f
                              && p.y >= b.min.y - 3f && p.y <= b.max.y + 3f;
                if (inside) return;

                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 跟随落点异常 pos=(" + p.x.ToString("F1") + ","
                    + p.y.ToString("F1") + ") 不在房间(" + dest.Coordinates.x + "," + dest.Coordinates.y
                    + ") 内，拉回入口。");
                if (!dest.gameObject.activeInHierarchy) dest.gameObject.SetActive(true);
                if (_lastEntranceLocal.HasValue && !_entranceRetried)
                {
                    _entranceRetried = true;
                    dest.PlacePlayerInRoom(_lastEntranceLocal.Value);
                    _verifyDest = dest;
                    _verifyAt = Time.unscaledTime + 0.6f;   // 再观察一次
                }
                else if (dest.PlayerSpawn != null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 入口落点站不住，改投房间默认出生点。");
                    dest.PlacePlayerInRoom(dest.PlayerSpawn.transform.localPosition);
                }
                RoomFix.RequestVerify();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 落地自检异常: " + ex.Message);
            }
        }

        private static bool CoordsMatch(Vector2 a, float cx, float cy)
        {
            return Mathf.Abs(a.x - cx) < 1.5f && Mathf.Abs(a.y - cy) < 1.5f;
        }

        /// <summary>
        /// v2.3.2：按世界坐标找房间（优先用对端真实落点，而不是可能为 (0,0) 的房间坐标）。
        /// 先按 Bounds 命中，命中不到就退一步找"离该点最近且在 24m 内"的房间。
        /// </summary>
        private static Room FindRoomAt(Vector3 world)
        {
            try
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    var rooms = GetAllRooms(pass == 1);
                    Room best = null;
                    float bestSqr = 24f * 24f;
                    for (int i = 0; i < rooms.Length; i++)
                    {
                        var r = rooms[i];
                        if (r == null) continue;
                        Bounds b;
                        try { b = r.Bounds; } catch { continue; }

                        if (b.size.x > 0.1f && b.size.y > 0.1f)
                        {
                            var expanded = b;
                            expanded.Expand(new Vector3(2f, 2f, 0f));
                            if (expanded.Contains(world)) return r;
                        }

                        // 未激活房间 Bounds 可能是 0 尺寸：退一步用中心距离兜底
                        float sqr = ((Vector2)b.center - (Vector2)world).sqrMagnitude;
                        if (sqr < bestSqr) { bestSqr = sqr; best = r; }
                    }
                    if (best != null) return best;
                }
            }
            catch { }
            return null;
        }

        /// <summary>
        /// 把对端报来的世界落点换算成目标房间的本地坐标，并做一次"贴地"修正：
        /// 落点常常正好在门的格子里（房间外侧一点），先朝房间中心内缩，再向下找地面。
        /// </summary>
        private static bool TryWorldLandingLocal(Room dest, float px, float py, out Vector3 local)
        {
            local = Vector3.zero;
            if (dest == null) return false;
            try
            {
                Vector3 world = new Vector3(px, py, 0f);
                Bounds b;
                try { b = dest.Bounds; } catch { b = new Bounds(world, Vector3.zero); }

                // 朝房间中心内缩 1.6m（门格在房间外侧，直接落会踩在门触发器的格子上）
                Vector2 dir = (Vector2)b.center - (Vector2)world;
                if (dir.sqrMagnitude > 0.01f) world += (Vector3)(dir.normalized * 1.6f);

                // 向下找地面（12m），命中就站在地面上方 1.1m
                Vector3 ground;
                if (TryGroundBelow(world, out ground)) world = ground;

                local = dest.transform.InverseTransformPoint(world);
                return true;
            }
            catch { }
            return false;
        }

        /// <summary>从给定世界位置向下打射线找可站立的地面；找不到返回 false。</summary>
        private static bool TryGroundBelow(Vector3 at, out Vector3 ground)
        {
            ground = Vector3.zero;
            try
            {
                var origin = new Vector2(at.x, at.y + 1.5f);
                var hits = Physics2D.RaycastAll(origin, Vector2.down, 14f);
                if (hits == null || hits.Length == 0) return false;
                float bestY = float.NegativeInfinity;
                bool found = false;
                for (int i = 0; i < hits.Length; i++)
                {
                    var col = hits[i].collider;
                    if (col == null || col.isTrigger) continue;
                    var mb = col.GetComponentInParent<BaseCharacterController>();
                    if (mb != null) continue;
                    float y = hits[i].point.y;
                    if (y > bestY) { bestY = y; found = true; }
                }
                if (!found) return false;
                ground = new Vector3(at.x, bestY + 1.1f, at.z);
                return true;
            }
            catch { }
            return false;
        }

        private static Room FindRoomByCoords(float cx, float cy)
        {
            try
            {
                Room found = SearchRooms(GetAllRooms(false), cx, cy);
                if (found != null) return found;

                // 缓存过期（换层时 Room 对象被销毁会留下 null 引用 / 新房间还没入表）：
                // 无条件重建一次再找——旧版只在"数量不同"时才重建，数量相同但对象换了就永远找不到。
                found = SearchRooms(GetAllRooms(true), cx, cy);
                if (found != null) return found;

                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    var rooms = _roomCache ?? new Room[0];
                    Room nearest = null;
                    float bestD = float.MaxValue;
                    for (int i = 0; i < rooms.Length; i++)
                    {
                        var r = rooms[i];
                        if (r == null) continue;
                        float d = (r.Coordinates - new Vector2(cx, cy)).sqrMagnitude;
                        if (d < bestD) { bestD = d; nearest = r; }
                    }
                    Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 目标(" + cx + "," + cy + ")未匹配：场景内有效房间="
                        + rooms.Length + " 个，最近=" + (nearest != null ? "(" + nearest.Coordinates.x + ","
                            + nearest.Coordinates.y + ") d²=" + bestD.ToString("F1") : "<none>"));
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] FindRoomByCoords 异常: " + ex.Message);
            }
            return null;
        }

        private static Room SearchRooms(Room[] rooms, float cx, float cy)
        {
            if (rooms == null) return null;
            Room best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                if (r == null) continue;
                float d = (r.Coordinates - new Vector2(cx, cy)).sqrMagnitude;
                if (d < bestD) { bestD = d; best = r; }
            }
            return bestD <= 4f ? best : null;   // 2m 容差
        }

        /// <summary>
        /// 全量房间表（含未激活）。Resources.FindObjectsOfTypeAll 要遍历所有已加载对象，
        /// 每次换房都扫一遍太贵，这里按场景缓存；换场景或缓存里出现已销毁对象时重建。
        /// </summary>
        private static Room[] _roomCache;
        private static int _roomCacheScene = int.MinValue;

        private static Room[] GetAllRooms(bool force = false)
        {
            var sc = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!force && _roomCache != null && sc.handle == _roomCacheScene && !HasDeadEntry(_roomCache))
                return _roomCache;

            var all = Resources.FindObjectsOfTypeAll<Room>();
            int n = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var r = all[i];
                if (r == null) continue;
                if (!r.gameObject.scene.IsValid()) continue;   // 排除 prefab / 资源
                n++;
            }
            var arr = new Room[n];
            int k = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var r = all[i];
                if (r == null) continue;
                if (!r.gameObject.scene.IsValid()) continue;
                arr[k++] = r;
            }
            _roomCache = arr;
            _roomCacheScene = sc.handle;
            return arr;
        }

        private static bool HasDeadEntry(Room[] rooms)
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i] == null) return true;   // Unity 假 null（对象已销毁）
            }
            return false;
        }

        private static Door FindDoor(Room room, byte side, int number)
        {
            try
            {
                // 房间可能未激活，Doors 属性默认不含未激活子物体，这里用带 true 的全量取兜底
                Door[] all = room.GetComponentsInChildren<Door>(true);
                for (int i = 0; i < all.Length; i++)
                {
                    var d = all[i];
                    if (d == null) continue;
                    if ((byte)d.Side == side && d.Number == number) return d;
                }
                for (int i = 0; i < all.Length; i++)
                {
                    var d = all[i];
                    if (d == null) continue;
                    if ((byte)d.Side == side) return d;
                }
                var doors = room.Doors;
                if (doors != null)
                {
                    for (int i = 0; i < doors.Count; i++)
                    {
                        var d = doors[i];
                        if (d == null) continue;
                        if ((byte)d.Side == side && d.Number == number) return d;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] FindDoor 异常: " + ex.Message);
            }
            return null;
        }

        internal static void Reset(string reason)
        {
            _pending = false;
            _applyingRemote = false;
            _suppressUntil = 0f;
            _suppressReturnCoords = null;
            _burstStart = 0f;
            _burstCount = 0;
            _verifyDest = null;
            _verifyAt = 0f;
            _lastEntranceLocal = null;
            _entranceRetried = false;
            _roomCache = null;
            _roomCacheScene = int.MinValue;
            _pendingTries = 0;
            _pendingNextTryAt = 0f;
            _lastSeqBySender.Clear();
            _resends.Clear();
            _exitArmed = false;
            _exitFromRoom = null;
            _exitAt = 0f;
        }

        private static MethodInfo _playerExitMethod;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                _playerExitMethod = AccessTools.Method(typeof(BaseRoom), "PlayerExit", new Type[] { typeof(Door) });
                if (_playerExitMethod == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] BaseRoom.PlayerExit 未找到，全员换房不可用。");
                    return;
                }
                var pre = new HarmonyMethod(typeof(RoomTravelSync).GetMethod("PlayerExitPrefix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(_playerExitMethod, prefix: pre);
                Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 补丁 OK: BaseRoom.PlayerExit prefix（全员换房，协作自动生效）。");

                // v2.3.2：真正广播在新房间 PlacePlayerInRoom_End 之后（那时坐标/落点才可信）
                try
                {
                    var enterEnd = AccessTools.Method(typeof(BaseRoom), "PlacePlayerInRoom_End",
                        new Type[] { typeof(Door) });
                    if (enterEnd != null)
                    {
                        var post = new HarmonyMethod(typeof(RoomTravelSync).GetMethod(
                            "PlacePlayerInRoomEndPostfix", BindingFlags.Static | BindingFlags.NonPublic));
                        harmony.Patch(enterEnd, postfix: post);
                        Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 补丁 OK: PlacePlayerInRoom_End postfix（换房广播时机）。");
                    }
                }
                catch (Exception e2)
                {
                    Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] PlacePlayerInRoom_End postfix 失败: " + e2.Message);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 补丁失败: " + Plugin.PatchError(ex));
            }
        }
    }
}
