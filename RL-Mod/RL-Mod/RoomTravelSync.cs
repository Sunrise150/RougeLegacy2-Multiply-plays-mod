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
        /// <summary>RoomFix 的门兜底必须避开"我们正在跟随远端换房"的窗口，否则会双重过门。</summary>
        internal static bool IsApplyingRemote { get { return _applyingRemote; } }
        /// <summary>本端 PlayerExit 在此时间点之前需要判断是不是回声（不是一律抑制）。</summary>
        private static float _suppressUntil;
        private const float SuppressWindow = 0.8f;
        /// <summary>跟随前所在的房间坐标：若窗内 PlayerExit 的目标正好是它，就是弹回原房间的回声。</summary>
        private static Vector2? _suppressReturnCoords;
        /// <summary>v3.0.10：最近一次跟随进入的房间。静默窗内再广播它就是 ping-pong 回声。</summary>
        private static BaseRoom _lastFollowedRoom;

        // ---------------- 待应用队列（收包 -> LateUpdate） ----------------
        private static bool _pending;
        private static int _pendingSender;
        private static float _pendingX, _pendingY;
        private static float _pendingPX, _pendingPY;   // v2.3.2：目标房间内的实际世界坐标
        private static float _pendingAX, _pendingAY;   // v2.9.1：目标房间 transform 锚点
        private static byte _pendingSide;
        private static int _pendingNumber;
        private static ushort _pendingSeq;
        private static float _pendingAt;
        private static float _pendingNextTryAt;
        private static int _pendingTries;
        private const float PendingTtl = 4f;      // 4 秒还找不到/进不去就放弃这一包
        private const int MaxTries = 14;
        private const float RetryInterval = 0.25f;

        // v2.8.6：对端最后一次换房的权威坐标。即使本机已倒地（ApplyNow 会 Drop 掉
        // 换房应用）也照记，供 Downed 观战时精确定位并激活对端所在房间。
        internal static bool HasRemoteRoom;
        internal static Vector2 RemoteRoomCoords;
        internal static Vector3 RemoteRoomLanding;
        // v2.9.1：房间 transform 锚点。Room.Coordinates 对特殊/过渡/合并房间可能与
        // 玩家实际世界位置不一致（真机日志：广播 (144,45) 但落点 (112.5,17)），
        // 用 transform.position 做第二把钥匙（与 rl2-coop 的 RoomKey 同源思路）。
        internal static Vector2 RemoteRoomAnchor;
        internal static bool HasRemoteAnchor;
        // v3.0.22：权威端所在房间所属的 biome（BiomeType 枚举值）。
        // 跨区域（biome 切换）时，本地根本没有那个区域的房间对象 —— 原生
        // BiomeTransitionController 只在"本地玩家实际过门"的那一端实例化新 biome，
        // 倒地观战端不会执行这条路径，于是选房必然落空、镜头卡在初始地图。
        // 广播端把 biome 带上，观战端就能据此在本地补建目标区域的房间。
        internal static int RemoteBiome = -1;
        /// <summary>本地当前所在 biome（用于判断权威是否已经跨区）。</summary>
        internal static int LocalBiome = -1;

        /// <summary>
        /// 本地玩家当前所在 biome（实时读，不依赖广播）。
        /// 观战端已倒地、不会再广播换房包，LocalBiome 若只在广播时更新就会一直停在旧值，
        /// 导致"权威已跨区但本地 biome 没变"的误判。
        /// </summary>
        internal static int CurrentLocalBiome()
        {
            try
            {
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return LocalBiome;
                var room = pc.CurrentlyInRoom as BaseRoom;
                if (room == null) return LocalBiome;
                return (int)room.BiomeType;
            }
            catch { return LocalBiome; }
        }

        // ---------------- 本地过门意图（v2.3.2） ----------------
        /// <summary>已经过门、等新房间 PlacePlayerInRoom_End 就绪后再广播。</summary>
        private static bool _exitArmed;
        private static float _exitAt;
        private static BaseRoom _exitFromRoom;
        private static byte _exitSide;
        private static int _exitNumber;
        /// <summary>v3.0.20：门身份未知的标记值（RoomSide 合法值只有 0~3）。</summary>
        private const byte UnknownSide = 255;
        /// <summary>
        /// v3.0.23：跨区域过门的广播窗口。原生 BiomeTransitionController.Run 先跑
        /// FadeToBlackWithLoading 转场（淡入 ~1.5s + 整块 biome 实例化，几十间房/背景/
        /// 天空/天气的 BuildStage），最后才 PlacePlayerInRoom 触发我们的广播 postfix。
        /// 这段远超旧的 3s 窗口（v3.0.22 及以前一律 3s），导致跨区包永远发不出来、
        /// 观战端收不到 biome → 补建不触发、视角卡初始地图。跨区单独放宽到 45s。
        /// </summary>
        private const float BiomeExitTtl = 45f;
        private static bool _exitIsBiomeTransition;

        // ---------------- 序号去重 ----------------
        private static readonly Dictionary<int, ushort> _lastSeqBySender = new Dictionary<int, ushort>();
        private static ushort _sendSeq;

        // ---------------- 广播补发 ----------------
        private struct Resend
        {
            public float At;
            public float X, Y;
            public float PX, PY;      // v2.3.2：目标房间里的实际世界坐标
            public float AX, AY;      // v2.9.1：目标房间 transform 锚点
            public byte Side;
            public int Number;
            public ushort Seq;
            public int Biome;         // v3.0.22：目标房间所属 biome
        }
        private static readonly List<Resend> _resends = new List<Resend>(4);

        // ---------------- 限流刹车 ----------------
        private static float _burstStart;
        private static int _burstCount;
        private const int BurstLimit = 8;
        private const float BurstWindow = 3f;

        // ---------------- 换房门闩（v3.0.7，借鉴 Boss Room LoadEventCompleted） ----------------
        // 发起端广播后开闸，等所有在线对端回执"到位"才关闸。
        // 只做确认/诊断/救援补发，不改变任何现有解析与落点逻辑（那些已调稳，不动）。
        private static bool _gateOpen;
        private static ushort _gateSeq;
        private static int _gateOwner;
        private static float _gateSince;
        private static readonly HashSet<int> _gateWaiters = new HashSet<int>();
        private static readonly HashSet<int> _gateAcked = new HashSet<int>();
        private static int _gateFailCount;        // 回执"未到位"的对端数
        private static int _gateRescueCount;
        private const float GateTtl = 6f;          // 6 秒还没收齐就强制关闸，避免死等
        private const float GateRescueDelay = 1.2f; // 收到"未到位"回执后延迟多久救援补发
        private const int MaxGateRescues = 2;      // 最多救援补发两次

        // 挂起的救援补发（发起端重发权威包，让跟随端重新进入 pending 重试窗口）
        private static bool _rescuePending;
        private static float _rescueAt;
        private static float _rescueX, _rescueY, _rescuePX, _rescuePY, _rescueAX, _rescueAY;
        private static byte _rescueSide;
        private static int _rescueNumber;
        private static int _rescueBiome = -1;   // v3.0.22

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
                // v3.0.23：记下这是否是跨区域过门——跨区走黑屏加载，到 PlacePlayerInRoom_End
                // 可能几十秒，必须放宽广播窗口，否则包在发出前就被 3s 超时吞掉。
                _exitIsBiomeTransition = door != null && door.IsBiomeTransitionPoint;
                // v3.0.20：side=255 表示"未知门"。旧版默认 0 会被对端当成
                // "Left 0 号门"，把玩家拉到错误的入口/房间（真机日志：跟随到 (0,0)
                // 的过渡房，人掉出去被救回）。ConnectedDoor 为空时必须显式标未知。
                _exitSide = UnknownSide;
                _exitNumber = -1;
                if (door != null)
                {
                    var cd = door.ConnectedDoor;
                    if (cd != null)
                    {
                        _exitSide = (byte)cd.Side;
                        _exitNumber = cd.Number;
                    }
                    else
                    {
                        Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 过门时这扇门没有 ConnectedDoor"
                            + "（原生 PlayerExit 会静默失败），广播门身份标为未知。");
                    }

                    // v3.0.26：把原生 PlayerExit 的每一条分支条件都打出来。
                    // 原生 PlayerExit 有两条【静默失败】分支：
                    //   · door.ConnectedDoor == null      -> "You entered a Door whose Connected Door is null"
                    //   · door.ConnectedDoor.Room == null  -> "You entered a Door whose Connected Door's Room is null"
                    // 两条都只是 Debug.LogError 就返回，玩家表现就是"走到门口没反应"。
                    // 门触发器到底坏在哪一步，看这一行就够了。
                    try
                    {
                        var pc0 = PlayerSync.Instance != null
                            ? PlayerSync.Instance.LocalPlayer as PlayerController : null;
                        bool isDead = pc0 != null && pc0.IsDead;
                        float hp = pc0 != null ? pc0.CurrentHealth : -1f;
                        bool bigRoom = false;
                        try
                        {
                            var dr = cd != null ? cd.Room : null;
                            if (dr != null)
                                bigRoom = dr.Bounds.size.x > 160f || dr.Bounds.size.y > 90f
                                       || __instance.Bounds.size.x > 160f || __instance.Bounds.size.y > 90f;
                        }
                        catch { }
                        Plugin.Log.LogInfo("[RL2MP] [EXIT] 尝试过门：房=" + __instance.name
                            + " 门=" + door.Side + "-" + door.Number
                            + " 对面门=" + (cd != null ? "有" : "无")
                            + " 对面房=" + (cd != null && cd.Room != null ? cd.Room.name : "无")
                            + " 跨区=" + door.IsBiomeTransitionPoint
                            + " 大房转场=" + bigRoom
                            + " IsDead=" + isDead
                            + " 血量=" + hp.ToString("F0")
                            + " -> " + (cd == null ? "原生会静默失败(无ConnectedDoor)"
                                : (cd.Room == null ? "原生会静默失败(对面无Room)"
                                    : "正常换房")));
                    }
                    catch { }
                }
                else
                {
                    // v3.0.25：door == null 不是"过门"，而是游戏自己的【场景级 / 传送级】进入。
                    // 全库只有这五处会这样调 PlayerExit(null)：
                    //   · OnPlayManager.CreateInstanceOfBaseRoom —— 开局进城堡的第一间房
                    //   · Tunnel.EnterTunnel                      —— 隧道（分支/密室）
                    //   · ChallengeTunnelController.EnterTunnel   —— 漂流屋的挑战
                    //   · ChallengeTunnelController.ReturnToDriftHouse —— 从挑战回漂流屋
                    //   · MapController                           —— 地图传送
                    // 这五条都不是"从某扇门走进隔壁房"，按"门内侧"落人必然是错的：
                    //   · 开局那一次会把客机放到"离下一间房最近的门边"，而不是房间默认出生点
                    //     （原生 PlayerController.EnterRoom(room, null, room.PlayerSpawn.localPosition)
                    //      才是开局真正的落点 —— 用户反馈"开局为什么不是从默认出生点"）。
                    //   · 挑战那一次会把队友直接拖进挑战里（用户反馈"客机进入漂流屋，
                    //     主机竟然直接被传送到挑战里面去了"）。
                    //   · 地图传送也不该把队友一起拽过去。
                    // 开局进城堡由 SceneChangeSync 的【场景同步】负责：两端一起切 World 场景，
                    // 各自由原生 OnPlayManager 把人放在默认出生点 —— 不需要房间跟随。
                    // 所以这里直接【不触发房间跟随】，并清掉可能残留的过门意图。
                    _exitArmed = false;
                    _exitIsBiomeTransition = false;
                    Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 场景级/传送级进入（无门），"
                        + "不触发房间跟随（开局由场景同步负责，挑战/隧道/地图传送不该拖走队友）。");
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
            // v3.0.23：跨区域过门要走完整的黑屏加载 + biome 实例化，耗时远超普通过门。
            // 普通过门仍用 3s（挡住"根本没换房"的陈旧意图），跨区放宽到 45s。
            float ttl = _exitIsBiomeTransition ? BiomeExitTtl : 3f;
            if (Time.unscaledTime - _exitAt > ttl) { _exitArmed = false; _exitIsBiomeTransition = false; return; }
            if (!Active || room == null) { _exitArmed = false; _exitIsBiomeTransition = false; return; }

            try
            {
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return;      // 还没抓到角色，等下一次机会

                Vector3 wp = pc.transform.position;
                if (Mathf.Abs(wp.x) < 0.01f && Mathf.Abs(wp.y) < 0.01f) return;   // 位置未就绪

                Vector2 coords = RoomCoordsOf(room);

            // 回声：
            //   a) 目标就是"刚跟随弹回去的那间房"；
            //   b) v3.0.10：目标就是"刚跟随过来的那间房"——跟随落位后门触发器把
            //      PlacePlayerInRoom_End 再跑一次，旧版当成自己过门又广播出去，
            //      对端收到后再跟一次、再弹回来，形成日志里的双向 ping-pong
            //      （主机在 (96,9)<->(0,0) 横跳 5 次、两端 seq 各自独立递增）。
            //      跟随后的短静默窗内，任何指向"刚落位房间"的广播一律吞掉。
            //   c) 根本没换房（room 还是离开的那间）——延迟进房/大房间过渡会走这条。
            if (Time.unscaledTime < _suppressUntil)
            {
                bool isEcho = _suppressReturnCoords.HasValue
                    && CoordsMatch(coords, _suppressReturnCoords.Value.x, _suppressReturnCoords.Value.y);
                if (!isEcho && _lastFollowedRoom != null)
                {
                    Vector2 followedCoords = RoomCoordsOf(_lastFollowedRoom);
                    if (CoordsMatch(coords, followedCoords.x, followedCoords.y))
                        isEcho = true;
                }
                if (isEcho) { _exitArmed = false; return; }
            }
                if (_exitFromRoom != null && ReferenceEquals(_exitFromRoom, room))
                {
                    _exitArmed = false;      // 还在原来那间房（协程延迟/大房间过渡），不广播
                    return;
                }

                _exitArmed = false;
                bool wasBiome = _exitIsBiomeTransition;
                _exitIsBiomeTransition = false;

                ushort seq = ++_sendSeq;
                // v2.9.1：同时发房间 transform 锚点，避免特殊房间的 Coordinates 与
                // 玩家落点不在同一坐标系时接收端找不到目标房间。
                float ax = Mathf.RoundToInt(room.transform.position.x);
                float ay = Mathf.RoundToInt(room.transform.position.y);
                // v3.0.22：带上目标房间所属 biome。跨区域时对端本地没有那个区域的房间
                // 对象（原生 BiomeTransitionController 只在"本地玩家实际过门"的一端实例化），
                // 对端需要靠这个信息在本地补建，否则视角永远卡在初始地图。
                int roomBiome = (int)room.BiomeType;
                LocalBiome = roomBiome;
                BroadcastTravel(coords.x, coords.y, _exitSide, _exitNumber, seq, wp.x, wp.y, ax, ay, roomBiome);
                ScheduleResends(coords, _exitSide, _exitNumber, seq, wp.x, wp.y, ax, ay, roomBiome);
                OpenGate(seq, coords.x, coords.y, _exitSide, _exitNumber, wp.x, wp.y, ax, ay, roomBiome);  // v3.0.7：门闩

                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 广播换房 -> 房间(" + coords.x + "," + coords.y
                        + ") 锚点(" + ax.ToString("F0") + "," + ay.ToString("F0")
                        + ") 落点(" + wp.x.ToString("F1") + "," + wp.y.ToString("F1")
                        + ") 门 side=" + _exitSide + " num=" + _exitNumber + " seq=" + seq
                        + " biome=" + roomBiome + (wasBiome ? " [跨区域]" : "")
                        + " via=" + from);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] TryAnnounceEnteredRoom 异常: " + Plugin.PatchError(ex));
            }
        }

        private static void BroadcastTravel(float cx, float cy, byte side, int number, ushort seq,
            float px, float py, float ax, float ay, int biome = -1)
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
            w.Put(ax); w.Put(ay);
            // v3.0.22：目标房间所属 biome（-1 = 未知/不跨区）。
            w.Put(biome);
            nm.SendRoomTravel(w);
        }

        /// <summary>过门后补发两次：对端在加载中/丢包时也能在恢复后拿到（接收端幂等+序号去重）。</summary>
        private static void ScheduleResends(Vector2 coords, byte side, int number, ushort seq,
            float px, float py, float ax, float ay, int biome = -1)
        {
            _resends.Clear();
            float now = Time.unscaledTime;
            _resends.Add(new Resend { At = now + 0.35f, X = coords.x, Y = coords.y, PX = px, PY = py, AX = ax, AY = ay, Side = side, Number = number, Seq = seq, Biome = biome });
            _resends.Add(new Resend { At = now + 1.0f, X = coords.x, Y = coords.y, PX = px, PY = py, AX = ax, AY = ay, Side = side, Number = number, Seq = seq, Biome = biome });
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
                var curRoom = pc.CurrentlyInRoom as BaseRoom;
                if (curRoom != null && CoordsMatch(RoomCoordsOf(curRoom), r.X, r.Y))
                {
                    BroadcastTravel(r.X, r.Y, r.Side, r.Number, r.Seq, r.PX, r.PY, r.AX, r.AY, r.Biome);
                }
            }
        }

        // ---------------- 换房门闩：发起端 ----------------

        /// <summary>广播换房后开闸：等所有在线对端回执到位（Boss Room 式全员确认）。</summary>
        private static void OpenGate(ushort seq, float cx, float cy, byte side, int number,
            float px, float py, float ax, float ay, int biome = -1)
        {
            var nm = NetworkManager.Instance;
            if (nm == null) return;
            _gateOpen = true;
            _gateSeq = seq;
            _gateOwner = nm.LocalPlayerId;
            _gateSince = Time.unscaledTime;
            _gateAcked.Clear();
            _gateWaiters.Clear();
            // 主机等所有在线客机；客机只等主机（双方拓扑：客机的对端就是主机）
            if (nm.IsHost)
            {
                foreach (int cid in nm.ClientIds) if (cid != _gateOwner) _gateWaiters.Add(cid);
            }
            else
            {
                _gateWaiters.Add(0);   // HostId = 0
            }
            _gateRescueCount = 0;
            _gateFailCount = 0;
            _rescuePending = false;

            // 记下这包的权威数据，供"未到位"救援补发复用（用原 seq，接收端幂等去重不冲突）
            _rescueX = cx; _rescueY = cy; _rescuePX = px; _rescuePY = py;
            _rescueAX = ax; _rescueAY = ay; _rescueSide = side; _rescueNumber = number;
            _rescueBiome = biome;   // v3.0.22：救援补发也要带 biome（跨区域时对端要补建房间）

            if (_gateWaiters.Count == 0)   // 没有对端：单机，直接关闸
            {
                CloseGate("无在线对端");
                return;
            }
            Plugin.Log.LogInfo("[RL2MP] [GATE] 开闸 seq=" + seq + " 等待 " + _gateWaiters.Count
                + " 个对端到位 -> 房间(" + cx.ToString("F0") + "," + cy.ToString("F0") + ")");
        }

        /// <summary>收到对端回执。status: 0=未到位, 1=已到位, 2=已到位但本机倒地（只定位观战房间）。</summary>
        internal static void HandleAck(int ackerId, ushort seq, byte status, float rx, float ry)
        {
            if (!_gateOpen || seq != _gateSeq) return;   // 旧包/别的换房的回执，忽略
            if (!_gateWaiters.Contains(ackerId)) return;
            if (_gateAcked.Contains(ackerId)) return;    // 幂等
            _gateAcked.Add(ackerId);

            string who = ackerId == 0 ? "主机" : ("P" + ackerId);
            if (status == 0)
            {
                // 对端没进去。记下它实际卡在哪个房间（诊断），并安排救援补发。
                _gateFailCount++;
                Plugin.Log.LogWarning("[RL2MP] [GATE] " + who + " 未到位（实际房间 "
                    + rx.ToString("F0") + "," + ry.ToString("F0") + "），seq=" + seq);
                if (_gateRescueCount < MaxGateRescues)
                {
                    _rescuePending = true;
                    _rescueAt = Time.unscaledTime + GateRescueDelay;
                }
            }
            else
            {
                Plugin.Log.LogInfo("[RL2MP] [GATE] " + who + " 到位（status=" + status + "）seq=" + seq);
            }

            _gateWaiters.Remove(ackerId);
            if (_gateWaiters.Count == 0)
            {
                // 全部回执到齐才关闸；若有人未到位，仍关闸但保留 _rescuePending 让救援补发照常执行。
                CloseGate(_gateFailCount > 0 ? _gateFailCount + " 个对端未到位（已安排救援）" : "全员到位",
                    cancelRescue: _gateFailCount == 0);
            }
        }

        /// <summary>对端断线：从门闩等待集合移除（对应 Boss Room OnConnectionEvent 清理）。</summary>
        internal static void OnPeerGone(int playerId)
        {
            if (!_gateOpen) return;
            if (!_gateWaiters.Remove(playerId)) return;
            _gateAcked.Add(playerId);
            Plugin.Log.LogInfo("[RL2MP] [GATE] 对端 P" + playerId + " 断线，移出门闩等待集合。");
            if (_gateWaiters.Count == 0) CloseGate("对端断线");
        }

        private static void CloseGate(string reason, bool cancelRescue = true)
        {
            if (!_gateOpen) return;
            _gateOpen = false;
            if (cancelRescue) _rescuePending = false;   // 未到位关闸时保留救援补发
            Plugin.Log.LogInfo("[RL2MP] [GATE] 关闸 seq=" + _gateSeq + " 原因=" + reason
                + " 到位 " + _gateAcked.Count + " 个，救援补发 " + _gateRescueCount + " 次。");
            _gateWaiters.Clear();
            _gateAcked.Clear();
        }

        /// <summary>由 LateUpdate 每帧驱动：门闩超时关闸 + 救援补发。</summary>
        private static void TickGate(float now)
        {
            if (_gateOpen && now - _gateSince > GateTtl)
            {
                int missing = _gateWaiters.Count;
                Plugin.Log.LogWarning("[RL2MP] [GATE] seq=" + _gateSeq + " 等待 " + GateTtl
                    + "s 超时，仍有 " + missing + " 个对端未回执，强制关闸。");
                CloseGate("超时");
            }
            if (!_rescuePending || now < _rescueAt) return;
            _rescuePending = false;
            if (_gateRescueCount >= MaxGateRescues) return;
            if (!Active) return;
            _gateRescueCount++;
            // 用原 seq 重发权威包：接收端 seq 去重允许相同 seq 通过，会重新登记 pending 再试一轮。
            BroadcastTravel(_rescueX, _rescueY, _rescueSide, _rescueNumber, _gateSeq,
                _rescuePX, _rescuePY, _rescueAX, _rescueAY, _rescueBiome);
            Plugin.Log.LogWarning("[RL2MP] [GATE] 救援补发权威包（第 " + _gateRescueCount
                + " 次）seq=" + _gateSeq + "，让未到位对端重试落位。");
        }

        // ---------------- 接收端 ----------------

        /// <summary>收到其他端的换房广播：只登记，实际传送留给 LateUpdate。</summary>
        internal static void ApplyTravel(int senderId, ushort seq, float cx, float cy, byte side, int number,
            float px, float py, float ax, float ay, int biome = -1)
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

            // v2.8.6：无论本机是否倒地，都记下对端权威房间坐标。
            // 倒地端不会真的 PlacePlayerInRoom（会让尸体乱跑），但观战必须知道
            // 该激活哪个房间 —— 以前只靠分身坐标+已激活房间偏好，会锁在旧房间，
            // 表现就是"对端去了我没探索过的房间，观战画面全黑"。
            HasRemoteRoom = true;
            RemoteRoomCoords = new Vector2(cx, cy);
            RemoteRoomLanding = new Vector3(px, py, 0f);
            HasRemoteAnchor = Mathf.Abs(ax) + Mathf.Abs(ay) > 0.05f;
            RemoteRoomAnchor = new Vector2(ax, ay);
            // v3.0.22：权威端所在区域的 biome。跨区域时对端本地没有那批房间对象，
            // 观战端要靠它在本地补建（见 BiomeRoomBuilder）。
            RemoteBiome = biome;

            _pending = true;
            _pendingSender = senderId;
            _pendingX = cx; _pendingY = cy;
            _pendingPX = px; _pendingPY = py;
            _pendingAX = ax; _pendingAY = ay;
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

        /// <summary>
        /// 跟随端走到终态后给发起端回执。status: 0=未到位, 1=已到位, 2=已到位但本机倒地。
        /// rx/ry 报本机实际所在房间坐标供发起端诊断。
        /// </summary>
        private static void SendApplyAck(byte status)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            try
            {
                float rx = _pendingX, ry = _pendingY;   // 默认回报目标房间
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc != null)
                {
                    var room = pc.CurrentlyInRoom as BaseRoom;
                    if (room != null) { var c = RoomCoordsOf(room); rx = c.x; ry = c.y; }
                }
                var w = new LiteNetLib.Utils.NetDataWriter();
                w.Put((byte)MsgType.RoomTravelAck);
                w.Put(nm.LocalPlayerId);   // acker
                w.Put(_pendingSender);     // owner = 换房发起者
                w.Put(_pendingSeq);
                w.Put(status);
                w.Put(rx); w.Put(ry);
                nm.SendRoomTravelAck(w);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [GATE] 回执发送失败: " + ex.Message);
            }
        }

        /// <summary>
        /// v3.0.24：权威端所在区域本地没有时，触发原生 InstantiateBiome 补建。
        /// 观战端原本就接了这条（EnsureSpectateRoom），活着的跟随端这边也接上，
        /// 否则跨区域时跟随端永远解析不到目标房间（biome 过滤把本地旧区房间全排掉了）。
        /// </summary>
        internal static void TryBuildRemoteBiome()
        {
            int remote = RemoteBiome;
            if (remote < 0) return;
            // 本地正在转场/加载：原生自己会建，别插手（并发建同一块 biome 会写坏房间表）。
            if (SceneChangeSync.IsTransitioning) return;
            int local = CurrentLocalBiome();
            if (remote == local) return;      // 本地已经在同一区域了
            try
            {
                if (BiomeRoomBuilder.EnsureBiome(remote)
                    && Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 跟随端跨区域：已触发本地补建区域 "
                        + ((BiomeType)remote) + " 的房间");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 跟随端补建区域失败: " + ex.Message);
            }
        }

        /// <summary>由 NetworkManager.LateUpdate 每帧驱动：应用待处理的远端换房（失败会重试，不丢包）。</summary>
        internal static void Tick()
        {
            float now = Time.unscaledTime;
            VerifyLanding(now);   // 先做上一次跟随的落地自检
            TickResends(now);
            TickGate(now);        // v3.0.7：门闩超时关闸 + 救援补发

            if (!_pending) return;
            if (!Active) { _pending = false; return; }

            if (now - _pendingAt > PendingTtl)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 跟随超时放弃：房间(" + _pendingX + "," + _pendingY
                    + ") 重试 " + _pendingTries + " 次仍未成功。");
                SendApplyAck(0);   // v3.0.7：明确告诉发起端"我没进去"，触发其救援补发
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
            float ax = _pendingAX, ay = _pendingAY;
            byte side = _pendingSide;
            int number = _pendingNumber;

            ApplyResult result = ApplyNow(sender, cx, cy, side, number, px, py, ax, ay, now);
            if (result == ApplyResult.Retry && _pendingTries < MaxTries)
            {
                _pendingTries++;
                _pendingNextTryAt = now + RetryInterval;
                // v3.0.24：权威端跨区时，本地根本没有那个区域的房间对象 ——
                // biome 过滤会把本地旧区房间全排掉，ResolveTargetRoom 必然落空，
                // 跟随端只能一路重试到超时。观战端早就接了 BiomeRoomBuilder，
                // 活着的跟随端这里也接上。
                // 延到第 3 次重试再触发：给"跟随端自己也会过那扇门、原生会自己建这块 biome"
                // 留出时间，避免和原生 BiomeTransitionController 同时建同一块 biome。
                if (_pendingTries >= 3) TryBuildRemoteBiome();
                return;   // 保留 _pending，下一轮再试
            }
            // 终态：给发起端回执到位情况（v3.0.7 门闩）。倒地端只定位了观战房间，报 status=2。
            byte ackStatus;
            if (result == ApplyResult.Success) ackStatus = 1;
            else if (result == ApplyResult.Drop)
            {
                var p = PlayerSync.Instance;
                var pl = p != null ? p.LocalPlayer as PlayerController : null;
                ackStatus = (DownedState.LocalDown || (pl != null && pl.IsDead)) ? (byte)2 : (byte)0;
            }
            else ackStatus = 0;   // 重试次数耗尽仍未成功
            SendApplyAck(ackStatus);
            _pending = false;
        }

        private enum ApplyResult { Success, Retry, Drop }

        private static ApplyResult ApplyNow(int senderId, float cx, float cy, byte side, int number,
            float px, float py, float ax, float ay, float now)
        {
            try
            {
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return ApplyResult.Retry;              // 本地角色还没进场景：稍后再试

                bool haveAnchor = Mathf.Abs(ax) + Mathf.Abs(ay) > 0.05f;
                if (pc.IsDead || DownedState.LocalDown)
                {
                    // v2.9.1：倒地端不搬尸体，但必须把权威房间坐标修正为"实际找到的房间"。
                    // v3.0.1：改用统一稳定解析（落点优先），且**解析不到时返回 Retry 而不是 Drop**——
                    // 旧版只用锚点+坐标，过渡房 (144,45) 对不上就一次性放弃，权威坐标锁死，
                    // 观战房间定位失败（日志铁证：倒地 spectating 时出现"目标未匹配"后无后续）。
                    // 保留 pending 让对端补发 + 本端重试，直到房间生成/落点可解析。
                    BaseRoom downDest = ResolveTargetRoom(cx, cy, ax, ay, px, py, side, number, null);
                    if (downDest != null)
                    {
                        RemoteRoomCoords = RoomCoordsOf(downDest);
                        RemoteRoomAnchor = (Vector2)downDest.transform.position;
                        HasRemoteAnchor = true;
                        return ApplyResult.Drop;   // 已解析到权威房间，不需要搬尸体
                    }
                    // 解析不到：对端落点可能还没生成对应房间，缓冲等待（HKMP 门控思路）
                    if (Time.unscaledTime - _lastLog > 1f)
                    {
                        _lastLog = Time.unscaledTime;
                        LogThrottled("倒地观战：目标房间(" + cx + "," + cy + "/落点 "
                            + px.ToString("F1") + "," + py.ToString("F1") + ")尚未就绪，等待重试。", true);
                    }
                    return ApplyResult.Retry;
                }

                var cur = pc.CurrentlyInRoom;
                if (cur == null) return ApplyResult.Retry;             // 加载中：稍后再试

                var curRoom = cur as BaseRoom;
                Vector2 curCoords = RoomCoordsOf(cur);
                // v2.9.1：先按 transform 锚点判重。特殊房间的 Coordinates 可能和玩家
                // 世界坐标完全不在一个格网里，只比 Coordinates 会把"已到达"误判掉。
                if (haveAnchor ? CoordsMatch((Vector2)cur.transform.position, ax, ay)
                               : CoordsMatch(curCoords, cx, cy))
                    return ApplyResult.Success;   // 已经在目标房间（幂等）

                bool havePos = Mathf.Abs(px) + Mathf.Abs(py) > 0.05f;
                // v3.0.1：统一走稳定解析（借鉴 HKMP 稳定身份 + TUNIC 入口门键）：
                // 入口门/落点世界坐标 > 锚点 > 网格坐标。过渡房坐标两端不一致时，
                // 落点世界坐标是唯一可靠的稳定身份。
                BaseRoom dest = ResolveTargetRoom(cx, cy, ax, ay, px, py, side, number, curRoom);
                if (dest == null)
                {
                    LogThrottled("未找到目标房间(" + cx + "," + cy + " / 锚点 "
                        + ax.ToString("F0") + "," + ay.ToString("F0") + " / 落点 "
                        + px.ToString("F1") + "," + py.ToString("F1") + ")，稍后重试。", true);
                    return ApplyResult.Retry;                          // 房间可能还没生成
                }
                // 统一回写为找到的房间真实坐标，后面的 ALIVE/障碍 key 才不会错位。
                // v3.0.11：合并房 MergeRoom 没有 Coordinates（Room 独有），统一走 RoomCoordsOf 兜底。
                Vector2 destCoords = RoomCoordsOf(dest);
                cx = destCoords.x; cy = destCoords.y;
                RemoteRoomCoords = destCoords;
                RemoteRoomAnchor = (Vector2)dest.transform.position;
                HasRemoteAnchor = true;

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
                    RoomSide entrySide = (RoomSide)side;
                    bool verticalDoor = entrySide == RoomSide.Top || entrySide == RoomSide.Bottom;
                    // v3.0.25：先把落点算出来，再统一执行"退旧房 / 开新房 / 落人"。
                    // 旧版每种落点各写一遍同样的收尾代码，改动极易走样。
                    // 落点优先级（与原生一致）：
                    //   1) 对端上报的真实世界落点（垂直门 / 水平门）
                    //   2) 入口门内侧
                    //   3) 房间默认出生点 Room.PlayerSpawn（兜底）
                    bool haveLocal = false;
                    string how = "";
                    if (havePos && verticalDoor && TryRemoteLandingLocal(dest, px, py, out entranceLocal))
                    {
                        haveLocal = true; how = "垂直门-对端落点";
                    }
                    else if (havePos && !verticalDoor && TryWorldLandingLocal(dest, px, py, out entranceLocal))
                    {
                        haveLocal = true; how = "水平门-对端落点";
                    }
                    else if (TryGetEntranceLocal(dest, side, number, out entranceLocal))
                    {
                        haveLocal = true; how = "入口门内侧";
                    }
                    else if (TryGetSpawnLocal(dest, out entranceLocal))
                    {
                        haveLocal = true; how = "默认出生点(兜底)";
                    }

                    if (haveLocal)
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

                        // 3) 落位，走完整进入流程（刷怪/背景/存档/相机/进入事件）
                        dest.PlacePlayerInRoom(entranceLocal);
                        // 原生 EnterRoom(Door) 对 Bottom 门强制 19.5m/s 向上速度把人抛进房间；
                        // PlacePlayerInRoom(Vector3) 不带这个速度，人会被重力拉回下层 = 传送失败。
                        if (entrySide == RoomSide.Bottom)
                        {
                            try { pc.SetVelocity(pc.Velocity.x, 19.5f, false); } catch { }
                        }
                        _lastEntranceLocal = entranceLocal;
                        Plugin.Log.LogInfo("[RL2MP] [ROOMSYNC] 跟随落位方式：" + how
                            + " 本地(" + entranceLocal.x.ToString("F1") + "," + entranceLocal.y.ToString("F1") + ")");
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

                // v3.0.10：记住刚跟随进入的房间，静默窗内抑制对它的二次广播（防 ping-pong）
                _lastFollowedRoom = dest;

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
        private static BaseRoom _verifyDest;   // v3.0.11：合并房 MergeRoom 不是 Room，改用 BaseRoom
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
        /// <summary>
        /// v3.0.11（rl2-coop RL2World.GoToRoom 同款）：把对端报来的真实世界落点直接换算成本地坐标。
        /// 不做向下贴地、不做门中心推算 —— px/py 就是对端过门后实际站立的世界位置。
        /// 合并房、垂直门、上下叠房都适用。落点落在 Bounds 外时拒绝，交给后续兜底。
        /// </summary>
        /// <summary>
        /// v3.0.25：房间的可玩区是否包含该世界坐标。
        /// 放宽 3m —— 玩家可能正贴着门/站在上下叠房间的交界上。
        /// 房间 Bounds 本身不可信时（尺寸为 0）返回 true（判不了就别乱拒），
        /// 否则会把合并房这类边界异常的房间误判成"落点不在里面"。
        /// </summary>
        private static bool RoomContainsPoint(BaseRoom room, float wx, float wy)
        {
            if (room == null) return false;
            try
            {
                Bounds b = room.Bounds;
                if (b.size.x <= 0.1f || b.size.y <= 0.1f) return true;   // 判不了 -> 放行
                var p = new Vector3(wx, wy, 0f);
                // 注意：Unity 的 Bounds.Expand(Vector3) 返回 void —— 它是【原地】放大。
                // 必须先复制一份再放大，不能写成 b.Expand(...).Contains(...)。
                Bounds expanded = b;
                expanded.Expand(new Vector3(3f, 3f, 0f));
                return b.Contains(p) || expanded.Contains(p);
            }
            catch { return true; }
        }

        /// <summary>
        /// v3.0.25：房间的【默认出生点】（原生 Room.PlayerSpawn / PlayerRoomSpawn）。
        /// 场景级进入（开局进城堡的第一间房）走的就是原生
        /// PlayerController.EnterRoom(room, null, room.PlayerSpawn.transform.localPosition)，
        /// 跟随端必须用同一个落点，否则客机被放在门内侧而不是默认出生点。
        /// </summary>
        private static bool TryGetSpawnLocal(BaseRoom dest, out Vector3 local)
        {
            local = Vector3.zero;
            try
            {
                var room = dest as Room;
                if (room == null) return false;      // 合并房 MergeRoom 不是 Room，没有 PlayerSpawn
                var spawn = room.PlayerSpawn;
                if (spawn == null) return false;
                local = spawn.transform.localPosition;
                return true;
            }
            catch { return false; }
        }

        private static bool TryRemoteLandingLocal(BaseRoom dest, float px, float py, out Vector3 local)
        {
            local = Vector3.zero;
            if (dest == null) return false;
            if (float.IsNaN(px) || float.IsNaN(py) || float.IsInfinity(px) || float.IsInfinity(py)) return false;
            try
            {
                var world = new Vector3(px, py, dest.transform.position.z);
                Bounds b = dest.Bounds;
                // 放宽 3m 容差：对端落点可能贴着门触发器（房间边界外一点），仍算有效。
                if (b.size.x > 0.1f && b.size.y > 0.1f
                    && (world.x < b.min.x - 3f || world.x > b.max.x + 3f
                        || world.y < b.min.y - 3f || world.y > b.max.y + 3f))
                    return false;
                local = dest.transform.InverseTransformPoint(world);
                local.z = 0f;
                return true;
            }
            catch { return false; }
        }

        private static bool TryGetEntranceLocal(BaseRoom dest, byte side, int number, out Vector3 local)
        {
            local = Vector3.zero;
            if (dest == null) return false;
            try
            {
                RoomSide s = (RoomSide)side;
                if (s != RoomSide.Left && s != RoomSide.Right && s != RoomSide.Top && s != RoomSide.Bottom)
                    return false;   // None/Any：位置算不出来，交给兜底

                Vector2 doorCenter;
                // v3.0.11：优先用目标房间里真实门对象的 CenterPoint —— 合并房 MergeRoom
                // 没有 Coordinates/Size，按公式推必错；而门的 CenterPoint 是合并前按成员房
                // 算好后缓存的、与几何一致（Door.SetRoom 对 BaseRoom 不重算），是权威值。
                Door realDoor = FindDoor(dest, side, number);
                if (realDoor != null)
                {
                    doorCenter = realDoor.CenterPoint;
                }
                else
                {
                    var destRoom = dest as Room;
                    if (destRoom == null) return false;   // 合并房又找不到门：交给出生点兜底
                    Vector2Int size = destRoom.Size;
                    if (size.x <= 0 || size.y <= 0) return false;   // 房间尺寸没初始化
                    doorCenter = RoomUtility.GetDoorCenterPoint(destRoom.Coordinates, size,
                        new DoorLocation(s, number));
                }

                // 门中心正落在房间边界上；朝内侧缩 2 米，站到门内的落脚台上。
                // （门的退出触发器整格在房间外侧，落点在内侧不会被它误触发——这点很关键，
                //   否则一落地就被门弹回上一个房间，表现为"来回换房"。）
                const float inset = 2f;
                // v2.9.0：垂直方向房间高度只有 18，水平方向的 2m 内缩不足以让落点
                // 脱离门的退出触发格（落点还在格子里 → 一落地就被弹回上层）。加大到 4m。
                const float vInset = 4f;
                if (s == RoomSide.Left) doorCenter.x += inset;
                else if (s == RoomSide.Right) doorCenter.x -= inset;
                else if (s == RoomSide.Bottom) doorCenter.y += vInset;
                else if (s == RoomSide.Top) doorCenter.y -= vInset;

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

                Vector2 destC = RoomCoordsOf(dest);
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 跟随落点异常 pos=(" + p.x.ToString("F1") + ","
                    + p.y.ToString("F1") + ") 不在房间(" + destC.x + "," + destC.y
                    + ") 内，拉回入口。");
                if (!dest.gameObject.activeInHierarchy) dest.gameObject.SetActive(true);
                if (_lastEntranceLocal.HasValue && !_entranceRetried)
                {
                    _entranceRetried = true;
                    dest.PlacePlayerInRoom(_lastEntranceLocal.Value);
                    _verifyDest = dest;
                    _verifyAt = Time.unscaledTime + 0.6f;   // 再观察一次
                }
                else if (dest is Room && ((Room)dest).PlayerSpawn != null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 入口落点站不住，改投房间默认出生点。");
                    dest.PlacePlayerInRoom(((Room)dest).PlayerSpawn.transform.localPosition);
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
        private static BaseRoom FindRoomAt(Vector3 world)
        {
            try
            {
                for (int pass = 0; pass < 2; pass++)
                {
                    var rooms = GetAllRooms(pass == 1);
                    // v3.0.1：三档严格度。上下叠房间 Bounds 会重叠，直接外扩 2m 会命中
                    // 遍历顺序靠前的下层房 → 传送到错房间。先严格包含，再小外扩，最后才兜底。
                    // pass 0 严格 / pass 1 放宽 + 强制重建房间表。
                    // v3.0.9：落点可能被多个房间 Bounds 同时包含（合并房套着标准房）。
                    // 选【面积最小】的命中房，而不是遍历顺序第一个——否则会命中大合并房，
                    // 它的 Coordinates = pos - 0.5*UnitDimensions 是负值，两端对不上。
                    for (int strict = 0; strict < 2; strict++)
                    {
                        BaseRoom tightest = null;
                        float tightestArea = float.MaxValue;
                        for (int i = 0; i < rooms.Length; i++)
                        {
                            var r = rooms[i];
                            if (r == null) continue;
                            Bounds b;
                            try { b = r.Bounds; } catch { continue; }
                            if (b.size.x <= 0.1f || b.size.y <= 0.1f) continue;
                            bool hit;
                            if (strict == 0)
                            {
                                hit = b.Contains(world);                       // 严格：真实包含落点
                            }
                            else
                            {
                                var expanded = b;
                                expanded.Expand(new Vector3(pass == 0 ? 1f : 2f, pass == 0 ? 1f : 2f, 0f));
                                hit = expanded.Contains(world);
                            }
                            if (!hit) continue;
                            float area = b.size.x * b.size.y;
                            if (area < tightestArea) { tightestArea = area; tightest = r; }
                        }
                        if (tightest != null) return tightest;
                    }

                    BaseRoom best = null;
                    float bestSqr = 24f * 24f;
                    for (int i = 0; i < rooms.Length; i++)
                    {
                        var r = rooms[i];
                        if (r == null) continue;
                        Bounds b;
                        try { b = r.Bounds; } catch { continue; }
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
        private static bool TryWorldLandingLocal(BaseRoom dest, float px, float py, out Vector3 local)
        {
            local = Vector3.zero;
            if (dest == null) return false;
            try
            {
                Vector3 world = new Vector3(px, py, 0f);
                Bounds b;
                try { b = dest.Bounds; } catch { b = new Bounds(world, Vector3.zero); }
                bool boundsUsable = b.size.x > 0.1f && b.size.y > 0.1f;

                // v3.0.26：对端已经站在房间里了（原生 PlacePlayerInRoom 放在门内侧），
                // 【原样落地】才能保证两端位置一致。
                // 旧版这里无条件朝房间中心内缩 1.6m + 向下贴地，结果每进一次加长房间
                // （合并房，Bounds 很大、中心在整段房间中间）客机就会比主机深 1.6m 且高度不同
                // —— 用户反馈"加长房间传送失误，客机和主机的位置不一致"。
                // 现在只有落点【明显在房间外】（门格/房间外侧）时才做修正。
                bool inside = !boundsUsable
                    || (world.x >= b.min.x - 1f && world.x <= b.max.x + 1f
                        && world.y >= b.min.y - 1f && world.y <= b.max.y + 1f);
                if (!inside)
                {
                    // 朝房间中心内缩 1.6m（门格在房间外侧，直接落会踩在门触发器的格子上）
                    Vector2 dir = (Vector2)b.center - (Vector2)world;
                    if (dir.sqrMagnitude > 0.01f) world += (Vector3)(dir.normalized * 1.6f);

                    // 向下找地面（12m），命中就站在地面上方 1.1m
                    Vector3 ground;
                    if (TryGroundBelow(world, out ground)) world = ground;
                }

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

        /// <summary>
        /// v2.9.1：按房间 GameObject 的世界锚点找房间（rl2-coop RoomKey 同源思路）。
        /// 对特殊/过渡/合并房间，这比 Room.Coordinates 稳定得多。
        /// </summary>
        internal static BaseRoom FindRoomByAnchor(float ax, float ay)
        {
            if (Mathf.Abs(ax) + Mathf.Abs(ay) < 0.05f) return null;
            try
            {
                BaseRoom found = SearchRoomsByAnchor(GetAllRooms(false), ax, ay);
                if (found != null) return found;
                found = SearchRoomsByAnchor(GetAllRooms(true), ax, ay);
                return found;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] FindRoomByAnchor 异常: " + ex.Message);
                return null;
            }
        }

        private static BaseRoom SearchRoomsByAnchor(BaseRoom[] rooms, float ax, float ay)
        {
            if (rooms == null) return null;
            // v3.0.23：与 SearchRooms 对齐的 biome 过滤。锚点是 ResolveTargetRoom 的第一
            // 优先级，各 biome 内部坐标系相近，跨区域时新区的锚点会命中旧区某间房；
            // 一旦命中就直接返回，观战端便锁死在旧区、永远不触发补建。
            int filterBiome = RemoteBiome;
            BaseRoom best = null;
            float bestD = float.MaxValue;
            var target = new Vector2(ax, ay);
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                if (r == null) continue;
                if (filterBiome >= 0 && (int)r.BiomeType != filterBiome) continue;
                float d = ((Vector2)r.transform.position - target).sqrMagnitude;
                if (d < bestD) { bestD = d; best = r; }
            }
            return bestD <= 9f ? best : null;   // 3m 容差（锚点发的是取整值）
        }

        private static BaseRoom FindRoomByCoords(float cx, float cy)
        {
            try
            {
                BaseRoom found = SearchRooms(GetAllRooms(false), cx, cy);
                if (found != null) return found;

                // 缓存过期（换层时 Room 对象被销毁会留下 null 引用 / 新房间还没入表）：
                // 无条件重建一次再找——旧版只在"数量不同"时才重建，数量相同但对象换了就永远找不到。
                found = SearchRooms(GetAllRooms(true), cx, cy);
                if (found != null) return found;

                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    var rooms = _roomCache ?? new BaseRoom[0];
                    BaseRoom nearest = null;
                    float bestD = float.MaxValue;
                    for (int i = 0; i < rooms.Length; i++)
                    {
                        var r = rooms[i];
                        if (r == null) continue;
                        var rr = r as Room;
                        Vector2 rc = rr != null ? rr.Coordinates : (Vector2)r.transform.position;
                        float d = (rc - new Vector2(cx, cy)).sqrMagnitude;
                        if (d < bestD) { bestD = d; nearest = r; }
                    }
                    Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] 目标(" + cx + "," + cy + ")未匹配：场景内有效房间="
                        + rooms.Length + " 个，最近=" + (nearest != null ? "(" + RoomCoordsOf(nearest).x + ","
                            + RoomCoordsOf(nearest).y + ") d²=" + bestD.ToString("F1") : "<none>"));
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ROOMSYNC] FindRoomByCoords 异常: " + ex.Message);
            }
            return null;
        }

        /// <summary>v3.0.11：房间稳定坐标。标准房用 Room.Coordinates，合并房 MergeRoom 没有该属性，
        /// 退回 transform.position（与它广播时发的 coords/锚点同一坐标系）。</summary>
        /// <summary>v3.0.26：房间网格坐标（Room 用 Coordinates，合并房等其它类型用 transform 位置）。</summary>
        internal static Vector2 RoomCoordsOf(BaseRoom r)
        {
            var rr = r as Room;
            return rr != null ? rr.Coordinates : (Vector2)r.transform.position;
        }

        private static BaseRoom SearchRooms(BaseRoom[] rooms, float cx, float cy)
        {
            if (rooms == null) return null;
            var target = new Vector2(cx, cy);
            // v3.0.22：网格坐标是【每个 biome 内部】的坐标系，跨区域时同坐标可能
            // 指向完全不同的房间。权威端带了 biome 时只在同一区域内匹配，
            // 否则会命中初始区域里同坐标的旧房间。
            int filterBiome = RemoteBiome;
            // 第一档：标准房的 Room.Coordinates（左下角），正常房间最准的键
            BaseRoom best = null;
            float bestD = float.MaxValue;
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i] as Room;
                if (r == null) continue;
                if (filterBiome >= 0 && (int)r.BiomeType != filterBiome) continue;
                float d = (r.Coordinates - target).sqrMagnitude;
                if (d < bestD) { bestD = d; best = r; }
            }
            if (bestD <= 4f) return best;   // 2m 容差
            // 第二档：合并房 MergeRoom 没有 Coordinates，广播端发的 coords 就是 transform.position
            best = null; bestD = float.MaxValue;
            for (int i = 0; i < rooms.Length; i++)
            {
                var r = rooms[i];
                if (r == null) continue;
                if (filterBiome >= 0 && (int)r.BiomeType != filterBiome) continue;
                float d = ((Vector2)r.transform.position - target).sqrMagnitude;
                if (d < bestD) { bestD = d; best = r; }
            }
            return bestD <= 9f ? best : null;   // 3m 容差（锚点发的是取整值）
        }

        /// <summary>
        /// v3.0.1（借鉴 HKMP 场景身份 + TUNIC 入口门稳定键）：
        /// 用一个统一入口解析"对端报来的目标房间"。关键改动是**落点世界坐标优先**——
        /// 过渡房间 '1*Transition*' 的 Room.Coordinates / transform 锚点在两端算出来不一致
        /// （真机日志：广播 (144,45)，但本端 45 个房间最近只有 (128,0)），
        /// 而玩家实际落点世界坐标是两端共享的同一个 Physical 位置，才是稳定身份。
        /// 顺序：入口门(side/number) → 落点世界坐标 → 锚点 → 网格坐标。
        /// 找不到返回 null，调用方据此走"缓冲门控重试"而不是丢弃。
        /// </summary>
        private static BaseRoom ResolveTargetRoom(float cx, float cy, float ax, float ay,
            float px, float py, byte side, int number, BaseRoom exclude)
        {
            bool havePos = Mathf.Abs(px) + Mathf.Abs(py) > 0.05f;
            bool haveAnchor = Mathf.Abs(ax) + Mathf.Abs(ay) > 0.05f;

            // v3.0.9：锚点（room.transform.position 取整）提到第一位。
            // 依据 rl2-coop 的 RoomKey（RL2World.cs:81）——它就是用 transform.position
            // 取整 + BiomeType 做跨端房间身份，是经另一个 mod 真机验证的稳定键。
            // 旧版把"落点世界坐标"排第一，FindRoomAt 的"24m 内最近中心"兜底会命中
            // Bounds 覆盖落点的【合并房】；而 Room.Coordinates = transform.position -
            // 0.5*UnitDimensions（Room.cs:161），合并房 UnitDimensions 大 → Coordinates
            // 变成 (64,-36) 这种负值。真机铁证：主机广播房间(96,9)锚点(96,9)，
            // 客机却跟随到 (64,-36)/(96,-36)，并打"目标(96,9)未匹配 最近(96,18)"。
            // 锚点是两端共享的同一 transform.position，精确无歧义，必须优先。
            // v3.0.25：锚点命中还得看【对端是不是真的站在这间房里】。
            // 不同房间的坐标/锚点会撞车（漂流屋 vs 挑战房、合并房 vs 成员房），
            // 光凭锚点会命中同坐标的邻居 -> 跟随端被传进隔壁那间。
            // 用户真机反馈："客机进入漂流屋，主机竟然直接被传送到挑战里面去了"。
            BaseRoom anchorHit = null;
            BaseRoom dest = haveAnchor ? FindRoomByAnchor(ax, ay) : null;
            if (dest != null && !ReferenceEquals(dest, exclude) && !IsTransitionRoom(dest))
            {
                if (!havePos || RoomContainsPoint(dest, px, py))
                    return dest;
                anchorHit = dest;   // 命中但落点不在里面 —— 留作最后兜底
            }

            // 2) 落点世界坐标：锚点失效时（过渡房两端 transform 不一致）的兜底
            dest = havePos ? FindRoomAt(new Vector3(px, py, 0f)) : null;
            if (dest != null && !ReferenceEquals(dest, exclude) && !IsTransitionRoom(dest)) return dest;

            // 3) 网格坐标（正常标准房间最准；特殊房间这里会落空）
            dest = FindRoomByCoords(cx, cy);
            if (dest != null && !ReferenceEquals(dest, exclude) && !IsTransitionRoom(dest)) return dest;

            // 3b) v3.0.25：锚点命中但落点不在里面的那间房，作为"门兜底"之前的最后手段。
            // 比按门号瞎猜准得多（门号在断链/合并房里经常指到别的房间）。
            if (anchorHit != null) return anchorHit;

            // 4) 最后手段：入口门——遍历所有房间，找有匹配 side/number 门且离落点最近的一间
            if (havePos)
            {
                BaseRoom[] all = GetAllRooms(true);
                BaseRoom best = null;
                float bestD = float.MaxValue;
                var landing = new Vector2(px, py);
                for (int i = 0; i < all.Length; i++)
                {
                    var r = all[i];
                    if (r == null || ReferenceEquals(r, exclude)) continue;
                    // v3.0.20：过渡房（*Transition*）的 Bounds/坐标都是坏的，跟随进去
                    // 只会让人掉进空洞再被救回（真机日志：跟随到房间(0,0) 后人掉出 1*Transition*）。
                    if (IsTransitionRoom(r)) continue;
                    Door d = FindDoor(r, side, number);
                    if (d == null) continue;
                    float dd = ((Vector2)r.transform.position - landing).sqrMagnitude;
                    if (dd < bestD) { bestD = dd; best = r; }
                }
                if (best != null) return best;
            }
            return null;
        }

        /// <summary>v3.0.20：过渡房判定。过渡房只是换楼层/换场景的过道，Bounds 与坐标都不可信。</summary>
        private static bool IsTransitionRoom(BaseRoom room)
        {
            try
            {
                if (room == null) return false;
                string n = room.name;
                return !string.IsNullOrEmpty(n)
                    && n.IndexOf("Transition", StringComparison.OrdinalIgnoreCase) >= 0;
            }
            catch { return false; }
        }

        /// <summary>
        /// 全量房间表（含未激活）。Resources.FindObjectsOfTypeAll 要遍历所有已加载对象，
        /// 每次换房都扫一遍太贵，这里按场景缓存；换场景或缓存里出现已销毁对象时重建。
        /// </summary>
        // v3.0.11：房间发现一律用 BaseRoom。合并房 MergeRoom 继承自 BaseRoom 而非 Room，
        // 旧版 FindObjectsOfTypeAll<Room>() 完全看不到它 —— 玩家跳进加长合成房后，
        // 跟随端在标准房里怎么找都找不到目标，只能落到最近的相邻房间（"掉到下一个房间"）。
        // 与 rl2-coop 的 RoomKey(BaseRoom) 同源：身份只看 transform.position，不看 Room.Coordinates。
        private static BaseRoom[] _roomCache;
        private static int _roomCacheScene = int.MinValue;

        /// <summary>
        /// v3.0.22：房间缓存失效。跨区域补建房间后必须调用 ——
        /// 新建的房间对象不在旧缓存里，否则 FindRoomByCoords 仍会报"未匹配"。
        /// </summary>
        internal static void InvalidateRoomCache()
        {
            _roomCache = null;
            _roomCacheScene = int.MinValue;
        }

        private static BaseRoom[] GetAllRooms(bool force = false)
        {            var sc = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
            if (!force && _roomCache != null && sc.handle == _roomCacheScene && !HasDeadEntry(_roomCache))
                return _roomCache;

            var all = Resources.FindObjectsOfTypeAll<BaseRoom>();
            int n = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var r = all[i];
                if (r == null) continue;
                if (!r.gameObject.scene.IsValid()) continue;   // 排除 prefab / 资源
                if (IsMergedAwayMember(r)) continue;           // v3.0.25：排除已被合并掉的成员房
                n++;
            }
            var arr = new BaseRoom[n];
            int k = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var r = all[i];
                if (r == null) continue;
                if (!r.gameObject.scene.IsValid()) continue;
                if (IsMergedAwayMember(r)) continue;
                arr[k++] = r;
            }
            _roomCache = arr;
            _roomCacheScene = sc.handle;
            return arr;
        }

        /// <summary>
        /// v3.0.25：这个房间是不是已经被合并进 MergeRoom 了。
        ///
        /// 原生 MergeRoomTools.AddRooms -> CopyRoom 会把成员房的 Room 组件销毁掉
        /// （只留 Transform / Collider2D / RoomEnemyManager），成员房 GameObject 本身
        /// 还会作为子物体留在合并房下面。Destroy 是延迟到帧末执行的，
        /// 在那一小段窗口里 Resources.FindObjectsOfTypeAll 仍可能扫到这些成员房，
        /// 它们的坐标与合并房重叠 -> 房间解析命中成员房而不是合并房
        /// -> "在对面的加长房间的传送依然出现了传送错误"。
        ///
        /// 判据：被登记过合并的成员房，其 GridPointManager.MergeWithGridPointManagers 非空
        /// （原生 SetMergeWithRooms 会把它和要合并的同伴互相登记）。
        /// 合并房本身是 MergeRoom（继承 BaseRoom 而不是 Room），不会被这里误伤。
        /// </summary>
        private static bool IsMergedAwayMember(BaseRoom r)
        {
            var rr = r as Room;
            if (rr == null) return false;
            try
            {
                var gpm = rr.GridPointManager;
                if (gpm == null) return false;
                var list = gpm.MergeWithGridPointManagers;
                return list != null && list.Count > 0;
            }
            catch { return false; }
        }

        private static bool HasDeadEntry(BaseRoom[] rooms)
        {
            for (int i = 0; i < rooms.Length; i++)
            {
                if (rooms[i] == null) return true;   // Unity 假 null（对象已销毁）
            }
            return false;
        }

        private static Door FindDoor(BaseRoom room, byte side, int number)
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
            _lastFollowedRoom = null;   // v3.0.10
            _burstStart = 0f;
            _burstCount = 0;
            HasRemoteAnchor = false;
            RemoteRoomAnchor = Vector2.zero;
            _pendingAX = 0f; _pendingAY = 0f;
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
            // v3.0.7：清空换房门闩状态
            _gateOpen = false;
            _gateWaiters.Clear();
            _gateAcked.Clear();
            _rescuePending = false;
            _gateRescueCount = 0;
            // v3.0.22：清掉跨区域状态，避免换局后拿旧的 biome 去补建。
            RemoteBiome = -1;
            LocalBiome = -1;
            BiomeRoomBuilder.ResetState();
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
