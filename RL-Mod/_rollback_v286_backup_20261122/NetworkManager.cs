using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL2Multiplayer
{
    public enum MsgType : byte
    {
        Welcome = 1,
        PlayerState = 2,
        LevelSeed = 3,
        // 竞速模式
        RaceConfig = 10,   // 主机 -> 客机：终点 Boss（byte bossId）
        RaceReady = 11,    // 双向：准备状态（bool）
        RaceStart = 12,    // 主机 -> 客机：开始倒计时（float seconds）
        RaceFinish = 13,   // 客机 -> 主机：完成用时（float elapsed）；主机本地完成直接仲裁
        RaceResult = 14,   // 主机 -> 客机：胜负结果（int winnerId, float winTime）
        RaceEnd = 15,      // 主机 -> 客机：结束对局，双方断开
        RaceRules = 16,    // 主机 -> 客机：规则包（周期 + 累赘等级），客机内存覆写
        MapData = 17,      // 主机 -> 客机：地图数据分块（StageSaveData 序列化后的第 N 块）
        CharacterData = 18,// 双向：角色外观（int playerId + byte[] CharacterData）
        NewGame = 19,      // 主机 -> 客机：主机开启了新周目（NG+）
        RacePenaltyConfig = 20, // 主机 -> 客机：每次死亡的加时秒数
        RacePenalty = 21,       // 双向：某玩家死亡次数 + 累计加时（HUD 显示）
        ScoreConfig = 22,       // 主机 -> 客机：计分规则（模式 + 6 个分值）
        ScoreState = 23,        // 双向：当前积分与明细
        RaceLobby = 24,         // 主机 -> 客机：大厅状态（每人 ready/inWorld）
        RacePlayerFinish = 25,  // 主机 -> 客机：某个玩家完成了竞速
        CharRequest = 26,       // 主机 -> 客机：请把你的角色外观再发一遍（有人中途加入）
        RaceGrace = 27,         // 主机 -> 客机：首位完成后剩余等待秒数
        Hello = 28,             // 客机 -> 主机：连接握手（int playerId, int token；首次 playerId=-1）
        PlayerAnim = 29,        // 双向：动画状态（10Hz，Sequenced）
        PlayerFlags = 30,       // 双向：状态位 downed（变化时才发，Reliable）
        Projectile = 31,        // 双向：弹体发射事件（终态快照，ReliableUnordered）
        CoOpRescue = 32,        // 双向：协作救援——救人者 -> 主机 -> 被救者(targetId)，ReliableOrdered
        EnemyDamage = 33,       // 双向：协作共享血量——敌人受玩家攻击的伤害(key, dmg)，ReliableOrdered
        EnemyDied = 34,         // 双向：协作共享血量——敌人死亡(key)，ReliableOrdered
        EnemyStateSnapshot = 35,// 主机 -> 客机：协作共享血量——全量状态快照(敌人 key->hpPct + 死亡 key)，Sequenced
        DashFx = 36,            // 双向：冲刺/虚冲特效事件(ownerId + isVoid)，ReliableOrdered——补 Trigger 与限时特效豁免
        Chat = 37,              // 双向：文字聊天(int playerId + string text)，主机中转广播，ReliableOrdered
        PlayerKicked = 38,      // 主机 -> 客机：你被踢了(string reason)，客机收到后自行 Stop，ReliableOrdered
        // v2.0.0：协作"同世界"三件套
        EnemyPosition = 39,     // 主机 -> 客机：怪物权威快照(count + [key,type,x,y,z,facing,target,animHash,animTime])，Sequenced 15Hz
        RoomTravel = 40,        // 双向：全员换房——过门者 -> 主机 -> 其他端(room坐标 + 目标门 side/number)，ReliableOrdered
        SceneChange = 41,       // 主机 -> 客机：场景切换(SceneID int + TransitionID int)，客机跟随，ReliableOrdered
        BossProgress = 42,      // v2.0.1：主机 -> 客机：Boss 击败权威位图(ushort)，客机内存对齐门状态，ReliableOrdered
        SkillTree = 43,         // v2.0.2：协作技能树共享——任一方加点(type+level 增量 / 全量表)，取 max 并集写各自存档，ReliableOrdered
        PlayerHurt = 44,        // v2.1.0：分身受击转发——被击中的分身 -> 主机 -> 真身持有者(targetId,dmg,stun,srcX,srcY)，ReliableOrdered
        // v2.4.1：死亡/结束本局一致性
        PlayerDown = 45,        // 双向：倒地状态心跳(id + downed)，0.3s 一发，ReliableOrdered。
                                // 与位置包解耦：GoDown 会把角色 SetActive(false)，旧版 downed 标志
                                // 挂在位置包里，一旦发包路径被"本地玩家引用"检查挡掉就永久丢状态
                                // （实测客机死亡后主机一直显示 alive=1/1，两端对"全灭"判断分叉）。
        RunOver = 46,           // 双向：本局结束（全员倒下）。任一端判定后广播，另一端收到直接
                                // 放行真实死亡，避免"一端卡在观战、另一端还在打"的死局。
        // v2.5.0：统一时钟轴（借鉴 rl2-coop 的 NTP 式 ClockSync）
        TimePing = 47,          // 客机 -> 主机：时钟探测(double 本地单调时刻)，非可靠
        TimePong = 48,          // 主机 -> 客机：回显 clientTime + 主机时刻(double)，非可靠
        EnemyAlive = 49,        // 主机 -> 客机：房间怪物存活权威位/键集（低频可靠），解决活怪数不一致
        ObstaclePose = 50,      // 主机 -> 客机：移动障碍物权威位置(15Hz Sequenced，带主机时间戳)
        // v2.7.0：视觉弹提前消失（火墙/持续法术停止时 FlagForDestruction/SetActive(false)）
        // 之前只同步了发射，对端那颗弹会停在最后一帧永不消失。
        ProjectileDespawn = 51, // 双向：ownerId + uint spawnId，ReliableOrdered（与发射同通道保序）
        // v2.8.6：联机显示名，独立消息，不改任何现有包格式（协议仍 3000）
        PlayerName = 52,        // 双向：senderId + 名字；主机收到后中转给其他客机
    }

    public class NetworkManager : MonoBehaviour, INetEventListener
    {
        public static NetworkManager Instance { get; private set; }

        private const int Port = 9050;
        private const string Key = "RL2MP";
        private const float SendInterval = 0.05f;
        private const int HostId = 0;

        /// <summary>
        /// 协议版本（v2.1.0 = 2100）。任何包格式改动都必须 +1。
        ///
        /// 为什么必须校验：换房包在 2.1.0 里加了 seq 字段，如果一端还是旧版，
        /// 旧版按旧格式读就会把 float 的字节读串，坐标变成 2.38E-41 这种垃圾值，
        /// 表现是"人物位置严重不匹配 + 传送没反应"，极难从现象反推原因。
        /// 这里在握手时就比对，不一致直接拒绝并说明原因。
        /// </summary>
        internal const int ProtocolVersion = 3000;

        private static string FmtProto(int p)
        {
            if (p <= 0) return "未知";
            return (p / 1000) + "." + (p / 100 % 10) + "." + (p % 100);
        }

        public bool IsHost { get; private set; }
        public bool Connected { get { return _netManager != null && _netManager.IsRunning; } }
        /// <summary>
        /// "已入局"标志：从开局/收到 Welcome 开始为 true，只有主动 Stop() 才清掉。
        /// 断线重连期间 Connected 仍是 true，但主机结束对局/网络彻底断了之后 Connected 变 false，
        /// 此时如果还按 Connected 判断"是否联机中"，玩家一死就会被当成单机真死（回城堡）。
        /// 死亡拦截改用这个标志，保证整局内死亡都走复活。
        /// </summary>
        public bool SessionActive { get; private set; }
        public int LocalPlayerId { get; private set; } = -1;

        public event Action<int> OnPlayerJoin;
        public event Action<int> OnPlayerLeave;

        /// <summary>v2.5.0 协议校验：仅供 NetValidation 反查连接绑定的玩家 ID（反冒充）。</summary>
        internal bool TryGetBoundPlayerId(int netPeerId, out int playerId)
        {
            return _peerToPlayerId.TryGetValue(netPeerId, out playerId);
        }

        /// <summary>HOST：已连接的客机 ID 列表（竞速仲裁用）。</summary>
        public IEnumerable<int> ClientIds { get { return _clients.Keys; } }

        private NetManager _netManager;
        private NetPeer _serverPeer;

        // key = 游戏层玩家 ID（1,2,3...），value = 对应的网络 peer。
        // 注意：绝不能用 peer.Id 当玩家 ID —— LiteNetLib 给第一个连上来的 peer
        // 分配的 Id 就是 0，会和主机固定 ID（HostId=0）撞车，导致双方把自己的
        // 状态包当成回声丢弃，画面里永远看不到对方。
        private readonly Dictionary<int, NetPeer> _clients = new Dictionary<int, NetPeer>();
        // 网络层 peer.Id -> 游戏层玩家 ID 的反查表（断开连接/转发时用）
        private readonly Dictionary<int, int> _peerToPlayerId = new Dictionary<int, int>();
        private int _nextPlayerId = 1; // 主机给客机分配 ID 的计数器，0 永远保留给主机
        private readonly HashSet<int> _knownPlayerIds = new HashSet<int>();

        private int _hostMasterSeed = -1;
        private float _seedPollTimer;

        private bool _debugSimActive;
        private readonly List<int> _debugFakeIds = new List<int>();
        private float _debugMoveTimer;
        private readonly Dictionary<int, Vector3> _debugFakeOffsets = new Dictionary<int, Vector3>();
        private readonly Dictionary<int, bool> _debugFakeFacing = new Dictionary<int, bool>();
        private readonly Dictionary<int, float> _debugFakeLastX = new Dictionary<int, float>();

        private float _rateLimitTimer;
        private int _sendCountSinceLastLog;
        private int _recvCountSinceLastLog;
        // v2.5.2：relay 日志专用节流——原来共用 _rateLimitTimer（每 2s 清零），
        // 导致 20Hz PlayerState 每条都刷一行，真机磁盘 I/O 拖慢帧率（主机日志刷了 93 条）。
        private int _relayCountSinceLastLog;

        private float _lastPosX = float.NaN;
        private float _lastPosY = float.NaN;

        // ---------------- 会话恢复（v1.6.0） ----------------
        // 断线不再销毁会话：主机把槽位标记为 Offline（ghost），保留分数/用时，
        // 在宽限期内收到带相同 token 的重连就恢复原 playerId。
        public static float ReconnectGraceSeconds = 90f;   // 主机保留槽位时长（面板可调）
        private const float ClientReconnectBudget = 45f;   // 客机重连总尝试时长
        private const float PendingHelloTimeout = 5f;      // 主机等待 Hello 的兜底超时（旧客户端兼容）

        private static readonly System.Random _rng = new System.Random();

        private class GhostSlot { public int PlayerId; public int Token; public float At; public string Ip; }
        private class PendingPeer { public NetPeer Peer; public float At; }

        private readonly Dictionary<int, GhostSlot> _ghosts = new Dictionary<int, GhostSlot>();
        private readonly Dictionary<int, PendingPeer> _pendingPeers = new Dictionary<int, PendingPeer>();
        private float _ghostSweepTimer;

        private int _sessionToken;            // 客机侧：主机下发的会话 token
        private int _resumeId = -1;           // 客机侧：上次会话拿到的 playerId（重连握手用）
        private bool _reconnecting;           // 客机侧：正在重连（会话未销毁）
        private bool _manualStop;             // 主动 Stop() 时置位，禁止自动重连
        private float _reconnectElapsed;
        private float _reconnectWaitTimer;
        private int _reconnectAttempt;

        // 分层发包计时（位置 20Hz / 动画 10Hz）
        private float _transformSendTimer;
        private float _animSendTimer;
        private bool _lastSentDowned;
        // v1.7.9：downed 事件包周期性重发，修复"尸体消失 / 长按 H 找不到目标"。
        // 旧版只在状态翻转瞬间发一次 Reliable；若那一刻接收端分身尚未创建，
        // SetRemoteDowned 会直接 return 丢弃状态，之后永不补发 -> 尸体一直显示成活人、
        // RemoteDowned 集合也为空（救援目标搜不到）。改为倒地期间每 0.5s 重发一次。
        private float _downResendTimer;
        private const float DownResendInterval = 0.5f;

        // v2.4.1：倒地状态独立心跳。上一版依赖位置包里顺带发的 PlayerFlags，
        // 而位置包的前置条件里有 PlayerSync.LocalPlayer != null —— 死亡瞬间角色被
        // SetActive(false)，加上 TriggerRealDeath 失败的连锁，状态就再也没送到对端。
        // 这里彻底解耦：只要拿到了 playerId，就每 0.3s 发一条 4 字节的 PlayerDown。
        private float _downBeatTimer;
        private const float DownBeatInterval = 0.3f;
        private bool _lastDownBeat;   // 仅用于日志去重
        private bool _lastRunOverSent;

        public bool IsReconnecting { get { return _reconnecting; } }
        public int ReconnectAttempt { get { return _reconnectAttempt; } }
        public float ReconnectElapsed { get { return _reconnectElapsed; } }

        private void Awake()
        {
            Instance = this;
            Plugin.Log.LogInfo("[RL2MP] NetworkManager.Awake() - Instance set.");
        }

        private void Update()
        {
            PumpNetwork();
        }

        /// <summary>
        /// v2.0.0：怪物权威位置在 AI 的 Update 之后覆盖，避免被 AI 下一帧算出的位置打架。
        /// LateUpdate 在所有 Update 完成后执行，正好适合做视觉/碰撞位置对齐。
        /// </summary>
        private void LateUpdate()
        {
            if (EnemyPositionSync.Active) EnemyPositionSync.ClientApplyLate();
            if (EnemyAliveSync.Active) EnemyAliveSync.ClientTick(); // v2.5.0：按权威存活集补杀
            ObstacleSync.ClientApplyLate();  // v2.5.0：权威障碍物位置覆盖（与 AI/Update 对抗）
            RoomTravelSync.Tick();   // v2.0.4：远端换房延后到 LateUpdate 应用（避免收包回调里改场景重入卡死）
            PlayerHurtSync.Tick();   // v2.1.0：血量变化兜底监控（Harmony 补丁失效时仍能去重）
            EnemyAiFix.Tick();       // v2.2.1：怪物目标指向已销毁玩家时自动修复（对象池复用）
            SceneChangeSync.Tick();  // v2.3.2：场景加载看门狗（请求了但卡住时自动重试一次）
        }

        /// <summary>
        /// 网络收发泵。v1.8.0：除了 MonoBehaviour.Update，还由 Bootstrap.Tick(onBeforeRender)
        /// 驱动一次——游戏暂停(Time.timeScale=0)时若不保险，可能漏收发。用 frameCount 守卫
        /// 保证同一帧只泵一次，不重复发包/重入。
        /// </summary>
        private int _lastPumpFrame = -1;
        internal void PumpNetwork()
        {
            if (_lastPumpFrame == Time.frameCount) return;
            _lastPumpFrame = Time.frameCount;

            PauseCompat.Tick(Time.unscaledDeltaTime);

            _rateLimitTimer += Time.unscaledDeltaTime;
            if (_rateLimitTimer >= 2f)
            {
                _rateLimitTimer = 0f;
                if (_sendCountSinceLastLog > 0 || _recvCountSinceLastLog > 0)
                {
                    Plugin.Log.LogInfo("[RL2MP] [STATS] last 2s -> sent=" + _sendCountSinceLastLog
                        + " recv=" + _recvCountSinceLastLog
                        + " relayed=" + _relayCountSinceLastLog
                        + " knownPlayers=" + _knownPlayerIds.Count
                        + " localId=" + LocalPlayerId);
                }
                _sendCountSinceLastLog = 0;
                _recvCountSinceLastLog = 0;
                _relayCountSinceLastLog = 0;
            }

            if (_debugSimActive)
            {
                UpdateDebugSim();
                return;
            }

            if (_netManager == null || !_netManager.IsRunning) return;

            _netManager.PollEvents();

            // 客机重连中：只跑重连循环，暂停一切同步（地图/外观/状态包都不发）
            if (!IsHost && _reconnecting)
            {
                TickReconnect();
                return;
            }

            if (IsHost && _hostMasterSeed == -1)
            {
                // 1 秒一次就够——以前每帧调，日志被 "GetCurrentSeed returned -1" 刷爆
                _seedPollTimer += Time.unscaledDeltaTime;
                if (_seedPollTimer >= 1f)
                {
                    _seedPollTimer = 0f;
                    TryCaptureMasterSeed();
                }
            }

            if (IsHost)
            {
                GhostSweep();
                TickPendingHellos();
                MapSync.HostTick();
                EnemySync.HostTick();   // v1.8.0：协作共享血量状态快照（权威对账）
                EnemyAliveSync.HostTick();    // v2.5.0：协作怪物存活权威集（1Hz）
                ObstacleSync.HostTick();      // v2.5.0：移动障碍物权威位置（15Hz）
                EnemyPositionSync.HostTick(); // v2.0.0：协作怪物权威位置（20Hz）
                BossProgressSync.HostTick();  // v2.0.1：Boss 击败权威位图（主机说了算）
                SkillTreeSync.HostTick();     // v2.0.2：协作技能树全量兜底（中途加入/漏包补齐）
            }
            else
            {
                MapSync.ClientTick();
                // v2.5.0：统一时钟轴——客机周期性发 TimePing 估计主机 offset。
                double ct;
                if (SessionActive && NetClock.ShouldPing(out ct)) SendTimePing(ct);
            }

            // 角色外观：本地角色变了（换继承人/换装备）就补发
            CharacterSync.Tick();

            // 分层发包（v1.6.0）：位置 20Hz（Sequenced）+ 动画 10Hz（Sequenced）
            // + downed 变化时才发的 Reliable 标志包，带宽比原先一体的 20Hz 大包省一半以上
            // v2.4.1：倒地心跳独立于位置包，不受 LocalPlayer 引用影响
            if (LocalPlayerId >= 0)
            {
                _downBeatTimer += Time.unscaledDeltaTime;
                if (_downBeatTimer >= DownBeatInterval)
                {
                    _downBeatTimer = 0f;
                    SendPlayerDownBeat(DownedState.LocalDown);
                }
            }

            if (LocalPlayerId >= 0 && PlayerSync.Instance != null && PlayerSync.Instance.LocalPlayer != null)
            {
                _transformSendTimer += Time.unscaledDeltaTime;
                if (_transformSendTimer >= SendInterval)
                {
                    _transformSendTimer = 0f;
                    SendTransformPacket();
                }

                _animSendTimer += Time.unscaledDeltaTime;
                // v1.6.4：20Hz。10Hz 对枪手这类高频射击动作采样太稀，远端分身
                // 攻击动画 scrub 拖影/抽搐。包约 300 字节，20Hz 双向 <12KB/s，可接受。
                if (_animSendTimer >= 0.05f)
                {
                    _animSendTimer = 0f;
                    SendAnimPacket();
                }
            }
            else if (Connected && LocalPlayerId < 0)
            {
                // 正在等 Welcome 消息
            }
        }

        private void UpdateDebugSim()
        {
            if (PlayerSync.Instance == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [DEBUG SIM] PlayerSync.Instance is null. Waiting...");
                return;
            }
            if (PlayerSync.Instance.LocalPlayer == null)
            {
                // 游戏还没开始，本地玩家还没捕获到，静默等待
                Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] Waiting for local player capture...");
                return;
            }

            _debugMoveTimer += Time.unscaledDeltaTime;
            if (_debugMoveTimer < 0.05f) return;
            _debugMoveTimer = 0f;

            // 跟随模式：测试玩家完全复制本地玩家的操作状态（位置/速度/朝向/接地/倒地/动画），
            // 只保留一个固定横向偏移方便区分。这样能直观检验插值与动画同步的流畅度。
            // （旧版是 PingPong 自主巡逻，动画和移动对不上，测不了流畅度。）
            var local = PlayerSync.Instance.GetLocalPlayerState();

            foreach (int id in _debugFakeIds)
            {
                Vector3 offset;
                if (!_debugFakeOffsets.TryGetValue(id, out offset))
                {
                    offset = new Vector3(1.5f * id, 0f, 0f);
                    _debugFakeOffsets[id] = offset;
                }

                float x = local.PositionX + offset.x;
                float y = local.PositionY + offset.y;

                // 动画数据传 0：让分身镜像组件走"本地全量实时镜像"（所有 Animator/所有层/特效），
                // 走网络路径的话只有主 Animator 第 0 层，攻击法术动画会丢。
                PlayerSync.Instance.HandleRemotePlayerState(id, x, y,
                    local.VelocityX, local.VelocityY,
                    local.FacingRight, local.Grounded, local.Downed,
                    0, 0f);
            }

            // 每 2 秒打一次 debug sim 摘要
            if (_rateLimitTimer < 0.1f)
            {
                string ids = string.Join(",", _debugFakeIds);
                Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] tick -> fakeIds=[" + ids
                    + "] localPos=(" + local.PositionX.ToString("F1") + "," + local.PositionY.ToString("F1")
                    + ") animHash=" + local.AnimStateHash
                    + " animTime=" + local.AnimNormalizedTime.ToString("F2")
                    + " remoteObjs=" + PlayerSync.Instance.RemotePlayers.Count);
            }
        }

        /// <summary>最近一次网络层异常（面板显示用）。null = 无错误。</summary>
        public static string LastError { get; private set; }

        private static void ReportNetError(string where, Exception ex)
        {
            LastError = where + ": " + ex.GetType().Name + ": " + ex.Message;
            // 双通道记录：BepInEx 日志 + Unity 日志（Player.log），确保至少一处能落地
            Plugin.Log.LogError("[RL2MP] [NET ERROR] " + where + " -> " + ex.GetType().FullName + ": " + ex.Message + "\n" + ex.StackTrace);
            UnityEngine.Debug.LogError("[RL2MP] [NET ERROR] " + where + " -> " + ex);
        }

        private bool TryCreateNetManager()
        {
            Plugin.Log.LogInfo("[RL2MP] [NET] Creating LiteNetLib NetManager...");
            try
            {
                _netManager = new NetManager(this)
                {
                    AutoRecycle = true,
                    // 心跳参数调优（v1.6.0）：默认值在网络抖动时太容易误判断线
                    UpdateTime = 10,             // 默认 15ms，发包节拍更稳
                    PingInterval = 500,          // 默认 1000ms
                    DisconnectTimeout = 8000,    // 默认 5000ms，抖一下就掉
                    ReconnectDelay = 500,
                    MaxConnectAttempts = 8,
                    EnableStatistics = true,
                };
                Plugin.Log.LogInfo("[RL2MP] [NET] NetManager created OK (timeout=8s, ping=0.5s).");
                return true;
            }
            catch (Exception ex)
            {
                _netManager = null;
                ReportNetError("NetManager ctor", ex);
                return false;
            }
        }

        public void StartHost()
        {
            Plugin.Log.LogInfo("[RL2MP] StartHost() called. Connected=" + Connected);

            if (Connected)
            {
                Plugin.Log.LogWarning("[RL2MP] [START HOST] Network already running (IsHost=" + IsHost + "). Abort.");
                return;
            }
            if (_debugSimActive)
            {
                Plugin.Log.LogWarning("[RL2MP] [START HOST] Debug sim active. Stop debug sim first. Abort.");
                return;
            }

            if (!TryCreateNetManager()) return;

            Plugin.Log.LogInfo("[RL2MP] [START HOST] NetManager.Start(port=" + Port + ")...");
            try
            {
                if (!_netManager.Start(Port))
                {
                    Plugin.Log.LogError("[RL2MP] [START HOST] FAILED to bind port " + Port + " - may be in use or blocked by firewall.");
                    LastError = "端口 " + Port + " 绑定失败（被占用或防火墙拦截）";
                    _netManager = null;
                    return;
                }
            }
            catch (Exception ex)
            {
                _netManager = null;
                ReportNetError("NetManager.Start(" + Port + ")", ex);
                return;
            }

            IsHost = true;
            LocalPlayerId = HostId;
            _knownPlayerIds.Clear();
            _knownPlayerIds.Add(HostId);
            _peerToPlayerId.Clear();
            _nextPlayerId = 1;
            _ghosts.Clear();
            _pendingPeers.Clear();
            _manualStop = false;
            SessionActive = true;
            Roster.SyncSelf();

            Plugin.Log.LogInfo("[RL2MP] [START HOST] SUCCESS! IsHost=true, LocalPlayerId=" + LocalPlayerId
                + ", listening on 0.0.0.0:" + Port
                + ", knownPlayers=[" + string.Join(",", _knownPlayerIds) + "]");
        }

        public void StartClient()
        {
            Plugin.Log.LogInfo("[RL2MP] StartClient() called. Connected=" + Connected + ", targetHost='" + Plugin.HostAddress + "', port=" + Port);

            if (Connected)
            {
                Plugin.Log.LogWarning("[RL2MP] [START CLIENT] Network already running (IsHost=" + IsHost + "). Abort.");
                return;
            }
            if (_debugSimActive)
            {
                Plugin.Log.LogWarning("[RL2MP] [START CLIENT] Debug sim active. Stop debug sim first. Abort.");
                return;
            }

            string target = Plugin.HostAddress;
            if (string.IsNullOrWhiteSpace(target))
            {
                Plugin.Log.LogError("[RL2MP] [START CLIENT] HostAddress is empty! Abort.");
                return;
            }

            Plugin.Log.LogInfo("[RL2MP] [START CLIENT] Creating NetManager...");
            if (!TryCreateNetManager()) return;

            Plugin.Log.LogInfo("[RL2MP] [START CLIENT] NetManager.Start()... (no listen port for client)");
            try
            {
                _netManager.Start();
            }
            catch (Exception ex)
            {
                _netManager = null;
                ReportNetError("NetManager.Start (client)", ex);
                return;
            }

            Plugin.Log.LogInfo("[RL2MP] [START CLIENT] NetManager.Connect('" + target + "', " + Port + ", key='" + Key + "')...");
            try
            {
                _netManager.Connect(target, Port, Key);
            }
            catch (Exception ex)
            {
                _netManager = null;
                ReportNetError("NetManager.Connect(" + target + ":" + Port + ")", ex);
                return;
            }

            IsHost = false;
            _knownPlayerIds.Clear();
            _ghosts.Clear();
            _pendingPeers.Clear();
            _manualStop = false;
            _reconnecting = false;
            _reconnectAttempt = 0;
            _reconnectElapsed = 0f;
            _resumeId = -1;      // 新会话：不带旧身份
            _sessionToken = 0;
            _lastSentDowned = false;
            NetClock.Reset();
            ResetRunOverFlag();

            Plugin.Log.LogInfo("[RL2MP] [START CLIENT] Connect request sent to " + target + ":" + Port + ". Waiting for Welcome...");
        }

        public void Stop()
        {
            Plugin.Log.LogInfo("[RL2MP] Stop() called. _netManager=" + (_netManager != null ? "exists" : "null") + ", debugSimActive=" + _debugSimActive);

            _manualStop = true;
            SessionActive = false;
            _reconnecting = false;
            _ghosts.Clear();
            _pendingPeers.Clear();
            _resumeId = -1;
            _sessionToken = 0;
            if (_netManager != null)
            {
                foreach (int id in new List<int>(_knownPlayerIds))
                {
                    if (id != LocalPlayerId)
                    {
                        Plugin.Log.LogInfo("[RL2MP] [STOP] Raising OnPlayerLeave for id=" + id);
                        var handler = OnPlayerLeave;
                        if (handler != null) handler(id);
                    }
                }
                _knownPlayerIds.Clear();

                _clients.Clear();
                _peerToPlayerId.Clear();
                _nextPlayerId = 1;
                _serverPeer = null;

                try
                {
                    Plugin.Log.LogInfo("[RL2MP] [STOP] Calling _netManager.Stop()...");
                    _netManager.Stop();
                    Plugin.Log.LogInfo("[RL2MP] [STOP] _netManager.Stop() returned OK.");
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning("[RL2MP] [STOP] _netManager.Stop() threw: " + ex.Message);
                }
                _netManager = null;
            }

            if (_debugSimActive)
            {
                Plugin.Log.LogInfo("[RL2MP] [STOP] Debug sim was active, stopping too.");
                StopDebugSim();
            }

            IsHost = false;
            LocalPlayerId = -1;

            // 会话结束：客机恢复自己的周期/累赘内存值
            RaceRulesSync.Restore("network stopped");
            // 客机恢复自己的地图数据（防存档污染）
            MapSync.Restore("network stopped");
            // v2.0.1：客机恢复自己的 Boss 门进度（内存），不污染源存档
            BossProgressSync.Restore("network stopped");
            BossProgressSync.ResetHost();
            SkillTreeSync.Reset();  // v2.0.2：只清标志；技能树是永久共享元进度，不断线还原
            RoomTravelSync.Reset("network stopped");  // v2.0.4：清待应用换房 + 回声窗口
            PlayerHurtSync.Reset("network stopped");  // v2.1.0：清受击去重时间戳
            CharacterSync.Clear("network stopped");

            // 防御：如果游戏输入映射（Game 层）被某个流程留在了禁用状态，
            // ESC 暂停菜单会打不开（WindowManager.CanPauseNow 要求 Game map 已启用），
            // 用户就"无法返回主菜单"。断线时统一复位。
            try
            {
                if (!RewiredMapController.IsMapEnabled(GameInputMode.Game))
                {
                    RewiredMapController.SetCurrentMapEnabled(true);
                    Plugin.Log.LogInfo("[RL2MP] [STOP] Game 输入映射原本处于禁用状态 -> 已重新启用。");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [STOP] 输入映射复位失败: " + ex.Message);
            }

            Roster.Clear("network stopped");
            Plugin.Log.LogInfo("[RL2MP] [STOP] Network fully stopped. LocalPlayerId reset to -1.");
        }

        // ---------------- 分层状态包（v1.6.0） ----------------
        // 旧版一个 PlayerState 大包 20Hz 全量发；现在拆三层：
        //   位置 20Hz Sequenced（丢旧保新，乱序不回跳）
        //   动画 10Hz Sequenced（变化慢，带宽减半）
        //   downed 变化时一次 Reliable（事件型，绝不丢）
        public void SendPlayerState()
        {
            if (PlayerSync.Instance == null || PlayerSync.Instance.LocalPlayer == null) return;
            var p = PlayerSync.Instance.GetLocalPlayerState();
            SendTransformPacketInner(p);
            SendAnimPacketInner(p);
            _transformSendTimer = 0f;
            _animSendTimer = 0f;
        }

        private void SendTransformPacket()
        {
            if (PlayerSync.Instance == null || PlayerSync.Instance.LocalPlayer == null) return;
            SendTransformPacketInner(PlayerSync.Instance.GetLocalPlayerState());
        }

        private void SendAnimPacket()
        {
            if (PlayerSync.Instance == null || PlayerSync.Instance.LocalPlayer == null) return;
            SendAnimPacketInner(PlayerSync.Instance.GetLocalPlayerState());
        }

        private void SendTransformPacketInner(PlayerStateData p)
        {
            // 节流日志：位置变化幅度小时只在摘要里体现，变化大时立即打
            if (float.IsNaN(_lastPosX)
                || Mathf.Abs(p.PositionX - _lastPosX) > 2f
                || Mathf.Abs(p.PositionY - _lastPosY) > 2f)
            {
                _lastPosX = p.PositionX;
                _lastPosY = p.PositionY;
                Plugin.Log.LogInfo("[RL2MP] [SEND] Position update -> (" + p.PositionX.ToString("F2") + "," + p.PositionY.ToString("F2")
                    + ") vx=" + p.VelocityX.ToString("F2") + " vy=" + p.VelocityY.ToString("F2")
                    + " facingRight=" + p.FacingRight + " grounded=" + p.Grounded);
            }

            var w = new NetDataWriter();
            w.Put((byte)MsgType.PlayerState);
            w.Put(LocalPlayerId);
            w.Put(p.PositionX); w.Put(p.PositionY);
            w.Put(p.VelocityX); w.Put(p.VelocityY);
            w.Put(p.FacingRight);
            w.Put(p.Grounded);
            SendToPeers(w, DeliveryMethod.Sequenced);
            _sendCountSinceLastLog++;

            // downed 状态：变化时立即发（Reliable）；此外倒地期间每 0.5s 重发一次，
            // 保证迟到创建的分身 / 曾丢包的一端最终也能同步到"倒地"，尸体不会消失，
            // 救援端也能一直搜到目标。起身（false）只发一次即可。
            _downResendTimer += Time.unscaledDeltaTime;
            bool downedChanged = p.Downed != _lastSentDowned;
            bool downedResend = p.Downed && _downResendTimer >= DownResendInterval;
            if (downedChanged || downedResend)
            {
                _downResendTimer = 0f;
                _lastSentDowned = p.Downed;
                var wf = new NetDataWriter();
                wf.Put((byte)MsgType.PlayerFlags);
                wf.Put(LocalPlayerId);
                wf.Put(p.Downed);
                SendToPeers(wf, DeliveryMethod.ReliableOrdered);
            }
        }

        private void SendAnimPacketInner(PlayerStateData p)
        {
            // v1.6.3：全层动画 + 参数。Player_Animator 有 12 层，攻击/冲刺在更高层；
            // 参数驱动 blend tree。两台真机上旧版只发第 0 层，远端看不到攻击/冲刺动作。
            var w = new NetDataWriter();
            w.Put((byte)MsgType.PlayerAnim);
            w.Put(LocalPlayerId);
            WriteAnimPayload(w, p);
            SendToPeers(w, DeliveryMethod.Sequenced);
            _sendCountSinceLastLog++;
        }

        /// <summary>动画载荷（层状态 + 参数快照）。发送与主机中转共用。</summary>
        private static void WriteAnimPayload(NetDataWriter w, PlayerStateData p)        {
            int layers = p.LayerHashes != null ? Mathf.Min(p.LayerHashes.Length, 16) : 0;
            w.Put((byte)layers);
            for (int l = 0; l < layers; l++)
            {
                w.Put(p.LayerHashes[l]);
                w.Put(p.LayerTimes[l]);
            }

            int nf = p.FloatParams != null ? Mathf.Min(p.FloatParams.Length, 255) : 0;
            w.Put((byte)nf);
            for (int i = 0; i < nf; i++) w.Put(p.FloatParams[i]);

            int ni = p.IntParams != null ? Mathf.Min(p.IntParams.Length, 255) : 0;
            w.Put((byte)ni);
            for (int i = 0; i < ni; i++) w.Put(p.IntParams[i]);

            int nb = p.BoolParams != null ? Mathf.Min(p.BoolParams.Length, 255) : 0;
            w.Put((byte)nb);
            int bytes = (nb + 7) / 8;
            for (int k = 0; k < bytes; k++)
            {
                int bits = 0;
                for (int j = 0; j < 8 && k * 8 + j < nb; j++)
                    if (p.BoolParams[k * 8 + j]) bits |= 1 << j;
                w.Put((byte)bits);
            }
        }

        /// <summary>解析动画载荷为 PlayerStateData 的动画字段（接收端 + 中转复用）。</summary>
        private static PlayerStateData ReadAnimPayload(NetDataReader reader)
        {
            var p = new PlayerStateData();
            int layers = reader.GetByte();
            if (layers > 0 && layers <= 16)
            {
                var lh = new int[layers];
                var lt = new float[layers];
                for (int l = 0; l < layers; l++)
                {
                    lh[l] = reader.GetInt();
                    lt[l] = reader.GetFloat();
                }
                p.LayerHashes = lh;
                p.LayerTimes = lt;
            }
            else
            {
                // 层数异常：剩余字节不用管，Sequenced 包按序读坏一包无碍
                layers = Mathf.Max(layers % 256, 0);
                if (layers > 16) layers = 16;
                for (int l = 0; l < layers; l++) { reader.GetInt(); reader.GetFloat(); }
            }

            int nf = reader.GetByte();
            if (nf > 0 && nf <= 255)
            {
                var fp = new float[nf];
                for (int i = 0; i < nf; i++) fp[i] = reader.GetFloat();
                p.FloatParams = fp;
            }

            int ni = reader.GetByte();
            if (ni > 0 && ni <= 255)
            {
                var ip = new int[ni];
                for (int i = 0; i < ni; i++) ip[i] = reader.GetInt();
                p.IntParams = ip;
            }

            int nb = reader.GetByte();
            if (nb > 0 && nb <= 255)
            {
                var bp = new bool[nb];
                int bytes = (nb + 7) / 8;
                for (int k = 0; k < bytes; k++)
                {
                    int bits = reader.GetByte();
                    for (int j = 0; j < 8 && k * 8 + j < nb; j++)
                        bp[k * 8 + j] = (bits & (1 << j)) != 0;
                }
                p.BoolParams = bp;
            }
            return p;
        }

        /// <summary>按当前身份发到一个或所有对端（主机 -> 全部客机；客机 -> 主机）。</summary>
        private void SendToPeers(NetDataWriter w, DeliveryMethod method)
        {
            if (IsHost)
            {
                foreach (var c in _clients.Values)
                {
                    if (c != null && c.ConnectionState == ConnectionState.Connected)
                        c.Send(w, method);
                }
            }
            else if (_serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
            {
                _serverPeer.Send(w, method);
            }
        }

        /// <summary>
        /// v2.4.1：倒地状态心跳（0.3s）。不依赖位置包与本地角色引用，
        /// 保证"我死了"这件事一定能送达对端，两端对全灭的判断不会分叉。
        /// </summary>
        private void SendPlayerDownBeat(bool downed)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.PlayerDown);
            w.Put(LocalPlayerId);
            w.Put(downed);
            SendToPeers(w, DeliveryMethod.ReliableOrdered);

            if (downed != _lastDownBeat)
            {
                _lastDownBeat = downed;
                Plugin.Log.LogInfo("[RL2MP] [DOWN] 本端倒地状态切换 -> " + downed
                    + "（已广播 PlayerDown 心跳 id=" + LocalPlayerId + "）");
            }
        }

        /// <summary>v2.4.1：状态翻转时立即补发一条（0.3s 心跳之外的即时同步）。</summary>
        internal void SendDownedNow(bool downed)
        {
            if (LocalPlayerId < 0) return;
            _downBeatTimer = 0f;
            SendPlayerDownBeat(downed);
        }

        /// <summary>
        /// v2.4.1：广播"本局结束"。任一端判定全员倒下后调用：
        /// 主机 -> 所有客机；客机 -> 主机（主机再转发给其他客机）。
        /// </summary>
        internal void SendRunOver(string reason)
        {
            if (_lastRunOverSent) return;
            _lastRunOverSent = true;

            var w = new NetDataWriter();
            w.Put((byte)MsgType.RunOver);
            w.Put(LocalPlayerId);
            w.Put(reason ?? "");
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
            Plugin.Log.LogInfo("[RL2MP] [RUNOVER] 已广播本局结束 -> 所有对端（" + reason + "）");
        }

        /// <summary>新对局/重连时复位只发一次的标志。</summary>
        internal void ResetRunOverFlag()
        {
            _lastRunOverSent = false;
            _lastDownBeat = false;
            _downBeatTimer = 0f;
        }

        /// <summary>
        /// 协作救援：救人者长按 H 完成后调用。带上被救者 playerId，发给主机；
        /// 主机收到后按 targetId 定向转交给被救者本人（见 OnNetworkReceive 的 CoOpRescue 分支）。
        /// </summary>
        internal void SendCoOpRescue(int targetId)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.CoOpRescue);
            w.Put(targetId);
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
            Plugin.Log.LogInfo("[RL2MP] [COOP] 发出救援请求 -> targetId=" + targetId
                + " (isHost=" + IsHost + ")");
        }

        /// <summary>协作共享血量：把已经写好的 EnemyDamage / EnemyDied 包发出（主机中转给其他端）。</summary>
        internal void SendEnemyDamage(NetDataWriter w)
        {
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>v1.8.0：主机广播敌人血量全量快照（Sequenced——新快照覆盖旧的，暂停/丢包后用最新一帧对账）。</summary>
        internal void SendEnemySnapshot(NetDataWriter w)
        {
            if (IsHost) SendToPeers(w, DeliveryMethod.Sequenced);
        }

        /// <summary>v2.0.0：全员换房——过门者广播房间坐标+门信息，主机中转给其他端跟随。</summary>
        internal void SendRoomTravel(NetDataWriter w)
        {
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>v2.0.0：全员场景切换广播（任一端触发，主机中转给其他端）。</summary>
        internal void SendSceneChange(NetDataWriter w)
        {
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>v2.0.0：主机广播怪物权威位置快照（Sequenced，20Hz）。</summary>
        internal void SendEnemyPosition(NetDataWriter w)
        {
            if (IsHost) SendToPeers(w, DeliveryMethod.Sequenced);
        }

        /// <summary>v2.5.0：主机广播移动障碍物权威位置（Sequenced，15Hz）。</summary>
        internal void SendObstaclePose(NetDataWriter w)
        {
            if (IsHost) SendToPeers(w, DeliveryMethod.Sequenced);
        }

        /// <summary>v2.5.0：主机广播房间怪物存活权威集（ReliableOrdered）。</summary>
        internal void SendEnemyAlive(NetDataWriter w)
        {
            if (IsHost) SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>客机发时钟探测（非可靠）；主机回 pong（非可靠）。</summary>
        internal void SendTimePing(double clientTime)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.TimePing);
            w.Put(clientTime);
            if (_serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
                _serverPeer.Send(w, DeliveryMethod.Unreliable);
        }

        private void ReplyTimePong(NetPeer peer, double clientTime)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.TimePong);
            w.Put(clientTime);            // echo, so guest computes rtt
            w.Put(NetClock.HostNow);      // host authority timestamp
            peer.Send(w, DeliveryMethod.Unreliable);
        }

        /// <summary>v2.0.1：主机广播 Boss 击败权威位图（ReliableOrdered，保证中途加入者也拿到）。</summary>
        internal void SendBossProgress(NetDataWriter w)
        {
            if (IsHost) SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>v2.1.0：分身被击中——把伤害转发给分身背后的真身（主机中转）。</summary>
        internal void SendPlayerHurt(NetDataWriter w)
        {
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>v1.8.4：冲刺/虚冲特效事件——ReliableOrdered，保证对端一定收到并补放残影。</summary>
        internal void SendDashFx(NetDataWriter w)
        {
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>HOST：等待 Hello 超时的兜底——旧版本客户端不会发 Hello，按新玩家注册。</summary>
        private void TickPendingHellos()
        {
            if (_pendingPeers.Count == 0) return;

            float now = Time.realtimeSinceStartup;
            List<int> timedOut = null;
            foreach (var kv in _pendingPeers)
            {
                if (now - kv.Value.At < PendingHelloTimeout) continue;
                if (kv.Value.Peer == null || kv.Value.Peer.ConnectionState != ConnectionState.Connected) continue;
                if (timedOut == null) timedOut = new List<int>();
                timedOut.Add(kv.Key);
            }
            if (timedOut == null) return;

            foreach (int peerId in timedOut)
            {
                PendingPeer pp;
                if (!_pendingPeers.TryGetValue(peerId, out pp)) continue;
                Plugin.Log.LogWarning("[RL2MP] [NET] peerId=" + peerId + " 在 " + PendingHelloTimeout
                    + "s 内未完成 Hello 握手（旧版本客户端？）-> 按新玩家注册。");
                RegisterPeer(pp.Peer, AllocPlayerId(), _rng.Next(1, int.MaxValue), false);
            }
        }

        private void TryCaptureMasterSeed()
        {
            try
            {
                var getCur = AccessTools.Method(typeof(RNGSeedManager), "GetCurrentSeed", new[] { typeof(string) });
                if (getCur == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [SEED] GetCurrentSeed(string) method not found on RNGSeedManager.");
                    return;
                }

                int seed = (int)getCur.Invoke(null, new object[] { "World" });
                if (seed > 0)
                {
                    _hostMasterSeed = seed;
                    Plugin.Log.LogInfo("[RL2MP] [SEED] Master Seed captured: " + seed + " (0x" + seed.ToString("X") + ")");
                    BroadcastLevelSeed(seed);
                }
                // 拿不到就静默等下一次轮询（以前每帧打一行 "-1 waiting"，日志会爆掉）
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SEED] Exception: " + ex.Message + " at " + ex.StackTrace);
            }
        }

        /// <summary>
        /// HOST：SeedSync 的 GenerateNewSeed postfix 在游戏每次 roll 新 World seed 后调用这里。
        /// seed 变化（开新局/重开）才广播，避免每帧刷包。
        /// </summary>
        public void OnWorldSeedGenerated(int seed)
        {
            if (seed == _hostMasterSeed) return;

            Plugin.Log.LogInfo("[RL2MP] [SEED] HOST World seed changed: "
                + (_hostMasterSeed == -1 ? "<none>" : _hostMasterSeed.ToString()) + " -> " + seed
                + " (0x" + seed.ToString("X") + ") -> broadcasting.");
            _hostMasterSeed = seed;
            BroadcastLevelSeed(seed);
        }

        /// <summary>
        /// 客机自动重进：主机重开新局后，延迟片刻走游戏自己的转场流程重新加载 World。
        /// 新一局的种子会被 SeedSync 的钩子强制成主机的种子，地图即与主机一致。
        /// 注意：重进会丢弃客机当前这一局的进度（跟着主机开新局）。
        /// </summary>
        private IEnumerator ClientAutoReEnterCo(int newSeed)
        {
            Plugin.Log.LogInfo("[RL2MP] [SEED] 主机已重开新局（seed=" + newSeed
                + " / 0x" + newSeed.ToString("X") + "），1.5 秒后自动重进世界...");
            yield return new WaitForSecondsRealtime(1.5f);

            if (!Connected || IsHost)
            {
                Plugin.Log.LogInfo("[RL2MP] [SEED] 会话已结束，取消自动重进。");
                yield break;
            }

            var active = SceneManager.GetActiveScene();
            if (active.name != "World")
            {
                Plugin.Log.LogInfo("[RL2MP] [SEED] 当前不在 World 场景（'" + active.name
                    + "'），跳过重进——下次进图会自动用新种子。");
                yield break;
            }

            if (SceneLoader_RL.IsLoading)
            {
                // 客机可能刚好在自己进图的转场中。放弃就再也对不齐了（种子不会重复下发），
                // 所以这里短时重试，等它加载完再重进。
                Plugin.Log.LogInfo("[RL2MP] [SEED] 游戏正在转场，等待加载完成后重进...");
                for (int i = 0; i < 10; i++)
                {
                    yield return new WaitForSecondsRealtime(1f);
                    if (!Connected || IsHost) yield break;
                    if (!SceneLoader_RL.IsLoading) break;
                }
                if (SceneLoader_RL.IsLoading)
                {
                    Plugin.Log.LogWarning("[RL2MP] [SEED] 转场持续过久，放弃自动重进（下次进图会用新种子）。");
                    yield break;
                }
            }

            Plugin.Log.LogInfo("[RL2MP] [SEED] 正在重进 World（FadeToBlackWithLoading 转场）...");
            // v2.0.2：这是程序内部为对齐种子发起的重进，不能广播出去，否则两端场景回声无限加载
            SceneChangeSync.SuppressBroadcast = true;
            try { SceneLoader_RL.LoadScene(SceneID.World, TransitionID.FadeToBlackWithLoading); }
            finally { SceneChangeSync.SuppressBroadcast = false; }
        }

        private void BroadcastLevelSeed(int seed)
        {
            if (!IsHost || _clients.Count == 0)
            {
                Plugin.Log.LogInfo("[RL2MP] [SEED] Broadcast skipped: IsHost=" + IsHost + " clients.Count=" + _clients.Count);
                return;
            }

            var w = new NetDataWriter();
            w.Put((byte)MsgType.LevelSeed);
            w.Put(seed);

            foreach (var c in _clients.Values)
            {
                c.Send(w, DeliveryMethod.ReliableOrdered);
            }

            Plugin.Log.LogInfo("[RL2MP] [SEED] Broadcast LevelSeed=" + seed + " (0x" + seed.ToString("X") + ") to " + _clients.Count + " clients.");
        }

        // ---------------- 竞速模式消息 ----------------

        public void SendRaceConfig(int bossId)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceConfig);
            w.Put((byte)bossId);
            SendToAllReliable(w);
        }

        /// <summary>只发给某个新连上的客机：让他知道当前终点 Boss（不影响其他人）。</summary>
        public void SendRaceConfigTo(NetPeer peer, int bossId)
        {
            if (peer == null) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceConfig);
            w.Put((byte)bossId);
            peer.Send(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>死亡加时设置（秒/次）。</summary>
        public void SendRacePenaltyConfig(float seconds)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RacePenaltyConfig);
            w.Put(seconds);
            SendToAllReliable(w);
        }

        /// <summary>本地死亡记账：次数 + 累计加时秒数。</summary>
        public void SendRacePenalty(int playerId, int deaths, float accumSeconds)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RacePenalty);
            w.Put(playerId);
            w.Put(deaths);
            w.Put(accumSeconds);
            SendToAllReliable(w, false);
        }

        public void SendRaceReady(bool ready, bool inWorld)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceReady);
            w.Put(LocalPlayerId);
            w.Put(ready);
            // 关键：还要告诉主机"我是否真的在游戏世界里"。
            // 否则客机停主菜单按一下准备，主机也会照常倒计时开跑，
            // 结果就是"只有主机进入了，客机没进"。
            w.Put(inWorld);
            SendToAllReliable(w);
        }

        public void SendRaceStart(float countdownSeconds)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceStart);
            w.Put(countdownSeconds);
            SendToAllReliable(w);
        }

        public void SendRaceFinish(float elapsed, int score)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceFinish);
            w.Put(LocalPlayerId);
            w.Put(elapsed);
            w.Put(score);
            SendToAllReliable(w);
        }

        /// <summary>HOST -> 所有客机：某个玩家完成了竞速（含主机自己）。</summary>
        public void SendRacePlayerFinish(int playerId, float elapsed, int score)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RacePlayerFinish);
            w.Put(playerId);
            w.Put(elapsed);
            w.Put(score);
            SendToAllReliable(w, false);
        }

        /// <summary>HOST -> 所有客机：首位完成后的剩余等待秒数（HUD 倒计时用）。</summary>
        public void SendRaceGrace(float seconds)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceGrace);
            w.Put(seconds);
            SendToAllReliable(w, false);
        }

        /// <summary>HOST -> 所有客机：大厅状态（谁准备好了、谁在游戏里、谁掉线中）。</summary>
        public void SendRaceLobby()
        {
            if (!IsHost) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceLobby);
            var list = new List<PlayerSlot>(Roster.All);
            w.Put(list.Count);
            for (int i = 0; i < list.Count; i++)
            {
                w.Put(list[i].Id);
                w.Put(list[i].Ready);
                w.Put(list[i].InWorld);
                w.Put(list[i].Offline);
            }
            SendToAllReliable(w, false);
        }

        public void SendRaceResult(int winnerId, bool scoreMode, List<PlayerSlot> ranked)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceResult);
            w.Put(winnerId);
            w.Put(scoreMode);
            w.Put(ranked.Count);
            for (int i = 0; i < ranked.Count; i++)
            {
                w.Put(ranked[i].Id);
                w.Put(ranked[i].FinishTime);
                w.Put(ranked[i].Score);
                w.Put(ranked[i].Dnf);
            }
            SendToAllReliable(w);
        }

        /// <summary>计分规则（主机权威）。v1.7.2 追加：开箱分值 4 档 + 金币折算。</summary>
        public void SendScoreConfig(RaceGameMode mode, int room, int enemy, int boss, int relic, int death, int decay,
            int chestBronze, int chestSilver, int chestGold, int chestFairy, int goldPerPoint, float goldNgPlusStep,
            PlayStyle style)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.ScoreConfig);
            w.Put((byte)mode);
            w.Put(room);
            w.Put(enemy);
            w.Put(boss);
            w.Put(relic);
            w.Put(death);
            w.Put(decay);
            w.Put(chestBronze);
            w.Put(chestSilver);
            w.Put(chestGold);
            w.Put(chestFairy);
            w.Put(goldPerPoint);
            w.Put(goldNgPlusStep);
            w.Put((byte)style); // v1.7.6：玩法维度，放末尾保证旧字段位置不变
            SendToAllReliable(w, false);
        }

        /// <summary>当前积分（0.5 秒节流发送）。v1.7.2 追加：箱子 / 金币折算明细。</summary>
        public void SendScoreState(int playerId, int score, int rooms, int kills, int bosses, int relics, int deaths,
            int chests, int chestPoints, int coinPoints, int goldCollected)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.ScoreState);
            w.Put(playerId);
            w.Put(score);
            w.Put(rooms);
            w.Put(kills);
            w.Put(bosses);
            w.Put(relics);
            w.Put(deaths);
            w.Put(chests);
            w.Put(chestPoints);
            w.Put(coinPoints);
            w.Put(goldCollected);
            SendToAllReliable(w, false);
        }

        public void SendRaceEnd()
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceEnd);
            SendToAllReliable(w);
        }

        public void SendRaceRules(int ngPlus, int highestNgPlusBeaten, Dictionary<BurdenType, int> burdens)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RaceRules);
            w.Put(ngPlus);
            // 解锁门槛也一起送：客机 BurdenManager.GetBurdenLevel 对"未解锁"的累赘直接返回 0，
            // 不同步这个值的话 BossUp / BiomeUp 那一整批累赘在客机上等于不存在。
            w.Put(highestNgPlusBeaten);
            w.Put(burdens.Count);
            foreach (var kv in burdens)
            {
                w.Put((byte)kv.Key);
                w.Put(kv.Value);
            }
            SendToAllReliable(w);
        }

        /// <summary>新周目通知包：主机开启了 NG+，让客机同步重置自己的竞速相关状态。</summary>
        public void SendNewGame(int ngPlus)
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.NewGame);
            w.Put(ngPlus);
            SendToAllReliable(w);
        }

        internal void SendToAllReliable(NetDataWriter w, bool log = true)        {
            int sent = 0;
            if (IsHost)
            {
                foreach (var c in _clients.Values)
                {
                    if (c.ConnectionState == ConnectionState.Connected)
                    {
                        c.Send(w, DeliveryMethod.ReliableOrdered);
                        sent++;
                    }
                }
            }
            else if (_serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
            {
                _serverPeer.Send(w, DeliveryMethod.ReliableOrdered);
                sent++;
            }
            if (log) Plugin.Log.LogInfo("[RL2MP] [NET] SendRace msg sent to " + sent + " peer(s).");
        }

        /// <summary>只发给某个客机（点名发，别的人不受影响）。</summary>
        public bool SendToPlayer(int playerId, NetDataWriter w)
        {
            NetPeer peer;
            if (!_clients.TryGetValue(playerId, out peer)) return false;
            if (peer == null || peer.ConnectionState != ConnectionState.Connected) return false;
            peer.Send(w, DeliveryMethod.ReliableOrdered);
            return true;
        }

        // ---------------- v1.9.4：聊天 + 房主踢人 ----------------

        /// <summary>
        /// v2.8.6：广播本机的自定义显示名。客机发给主机（主机再中转），主机直接发全体。
        /// 使用独立的 PlayerName 消息，不动 Hello/Welcome/Chat 格式，协议版本保持 3000。
        /// </summary>
        internal void SendPlayerName()
        {
            if (!Connected || LocalPlayerId < 0) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.PlayerName);
            w.Put(LocalPlayerId);
            w.Put(Plugin.PlayerName);
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
            Plugin.Log.LogInfo("[RL2MP] [NET] -> PlayerName '" + Plugin.PlayerName + "'");
        }

        /// <summary>发送一条聊天文本。客机发给主机、主机中转广播；主机本地直接发全体。</summary>
        internal void SendChat(string text)
        {
            if (string.IsNullOrEmpty(text) || !Connected) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.Chat);
            w.Put(LocalPlayerId);
            w.Put(text);
            SendToPeers(w, DeliveryMethod.ReliableOrdered);
        }

        /// <summary>HOST：把某客机踢出房间。通知对方（让它走完整 Stop），并立即清掉本端槽位/ghost。</summary>
        internal bool KickPlayer(int playerId)
        {
            if (!IsHost || playerId == HostId) return false;

            NetPeer peer = null;
            _clients.TryGetValue(playerId, out peer);

            // 先通知被踢者，让它自己走 Stop（给 Reliable 送达留出机会）
            if (peer != null && peer.ConnectionState == ConnectionState.Connected)
            {
                try
                {
                    var w = new NetDataWriter();
                    w.Put((byte)MsgType.PlayerKicked);
                    w.Put("房主将你移出了房间");
                    peer.Send(w, DeliveryMethod.ReliableOrdered);
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning("[RL2MP] [KICK] 通知被踢者失败: " + ex.Message);
                }
            }

            // 本端清理：立即断开并移除槽位（不进 ghost 宽限期）
            if (peer != null)
            {
                try { peer.Disconnect(); }
                catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [KICK] disconnect 异常: " + ex.Message); }
                int netId = peer.Id;
                _peerToPlayerId.Remove(netId);
            }
            _clients.Remove(playerId);
            _ghosts.Remove(playerId);
            _pendingPeers.Clear();
            _knownPlayerIds.Remove(playerId);
            Roster.Remove(playerId);

            var handler = OnPlayerLeave;
            if (handler != null) handler(playerId);
            Race.BroadcastLobby();

            Plugin.Log.LogInfo("[RL2MP] [KICK] 房主踢出 playerId=" + playerId
                + "，剩余客户端 " + _clients.Count + " 个。");
            return true;
        }

        /// <summary>
        /// HOST：拒绝协议版本不一致的客机。
        /// 延迟断开是为了让 ReliableOrdered 的提示包先发出去——立刻 Disconnect 会连包带连接一起丢，
        /// 客机只看到"莫名其妙断线"，然后无限自动重连。
        /// </summary>
        private void RejectIncompatible(NetPeer peer, int theirProto)
        {
            Plugin.Log.LogWarning("[RL2MP] [NET] 拒绝连接：mod 协议不一致（对方 " + FmtProto(theirProto)
                + " / 本机 " + FmtProto(ProtocolVersion) + "）。");
            try
            {
                if (peer != null && peer.ConnectionState == ConnectionState.Connected)
                {
                    var w = new NetDataWriter();
                    w.Put((byte)MsgType.PlayerKicked);
                    w.Put("mod 版本不一致（主机 " + FmtProto(ProtocolVersion) + " / 客机 " + FmtProto(theirProto)
                          + "）。请把两端的 RL-Mod.dll 换成同一版本。");
                    peer.Send(w, DeliveryMethod.ReliableOrdered);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [NET] 发送版本提示失败: " + ex.Message);
            }

            StartCoroutine(DisconnectPeerAfter(peer, 0.35f));
        }

        private System.Collections.IEnumerator DisconnectPeerAfter(NetPeer peer, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            try { if (peer != null) peer.Disconnect(); } catch { }
        }

        /// <summary>HOST：清理一个掉线 ghost 槽位（幽灵玩家也能手动移除）。</summary>
        internal bool RemoveGhost(int playerId)
        {
            if (!IsHost || playerId == HostId) return false;
            bool removed = _ghosts.Remove(playerId);
            if (_clients.ContainsKey(playerId)) return false; // 还在线，不能用这个入口
            _knownPlayerIds.Remove(playerId);
            Roster.Remove(playerId);
            var leaveHandler = OnPlayerLeave;
            if (leaveHandler != null) leaveHandler(playerId);
            Race.BroadcastLobby();
            Plugin.Log.LogInfo("[RL2MP] [KICK] 房主清理幽灵槽位 playerId=" + playerId + " (hadGhost=" + removed + ")");
            return removed;
        }

        /// <summary>
        /// 弹体发射事件：v2.7.0 起与 despawn 一起走 ReliableOrdered。
        /// 原来用 ReliableUnordered，despawn 可能早于 spawn 到达，对端会丢掉消失通知
        /// （火墙残留最后一个火球就是这个原因之一）。
        /// </summary>
        internal void SendProjectile(NetDataWriter w)
        {
            if (IsHost)
            {
                foreach (var c in _clients.Values)
                {
                    if (c != null && c.ConnectionState == ConnectionState.Connected)
                        c.Send(w, DeliveryMethod.ReliableOrdered);
                }
            }
            else if (_serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
            {
                _serverPeer.Send(w, DeliveryMethod.ReliableOrdered);
            }
        }

        /// <summary>弹体提前消失：主机中转给其他端，必须可靠且与发射同通道保序。</summary>
        internal void SendProjectileDespawn(NetDataWriter w)
        {
            if (IsHost)
            {
                foreach (var c in _clients.Values)
                {
                    if (c != null && c.ConnectionState == ConnectionState.Connected)
                        c.Send(w, DeliveryMethod.ReliableOrdered);
                }
            }
            else if (_serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
            {
                _serverPeer.Send(w, DeliveryMethod.ReliableOrdered);
            }
        }

        /// <summary>HOST：把一条消息转发给除 excludePlayerId 之外的所有客机（多人局里互通状态用）。</summary>
        internal void RelayToOtherClients(int excludePlayerId, Action<NetDataWriter> fill)
        {
            if (!IsHost || _clients.Count == 0) return;

            var w = new NetDataWriter();
            fill(w);

            int sent = 0;
            foreach (var kv in _clients)
            {
                if (kv.Key == excludePlayerId) continue;
                var c = kv.Value;
                if (c != null && c.ConnectionState == ConnectionState.Connected)
                {
                    c.Send(w, DeliveryMethod.ReliableOrdered);
                    sent++;
                }
            }
        }

        /// <summary>HOST：当前连着的客机数量（地图推送等逻辑用）。</summary>
        internal int ClientCount { get { return _clients.Count; } }
        internal bool HasClients { get { return _clients.Count > 0; } }

        /// <summary>当前链路平均 RTT（毫秒）。主机取所有客机均值，客机取主机链路。无连接返回 -1。</summary>
        public int GetAveragePing()
        {
            try
            {
                if (IsHost)
                {
                    long sum = 0; int n = 0;
                    foreach (var c in _clients.Values)
                    {
                        if (c == null || c.ConnectionState != ConnectionState.Connected) continue;
                        sum += c.Ping; n++;
                    }
                    return n > 0 ? (int)(sum / n) : -1;
                }
                if (_serverPeer != null && _serverPeer.ConnectionState == ConnectionState.Connected)
                    return _serverPeer.Ping;
            }
            catch { }
            return -1;
        }

        /// <summary>HOST：把包转发给除 senderId 以外的其他客机（多客机同步用）。</summary>
        internal void ForwardToOtherClients(int senderId, NetDataWriter w)
        {
            if (!IsHost) return;
            foreach (var kv in _clients)
            {
                if (kv.Key == senderId) continue;
                if (kv.Value != null && kv.Value.ConnectionState == ConnectionState.Connected)
                {
                    kv.Value.Send(w, DeliveryMethod.ReliableOrdered);
                }
            }
        }

        public void OnConnectionRequest(ConnectionRequest request)
        {
            if (IsHost)
            {
                // 最多 4 人（含主机），满了直接拒
                if (_clients.Count >= Roster.MaxPlayers - 1)
                {
                    Plugin.Log.LogWarning("[RL2MP] [NET] 房间已满（" + (_clients.Count + 1) + "/"
                        + Roster.MaxPlayers + "），拒绝 " + request.RemoteEndPoint);
                    request.Reject();
                    return;
                }

                Plugin.Log.LogInfo("[RL2MP] [NET] OnConnectionRequest from " + request.RemoteEndPoint + " -> AcceptIfKey('" + Key + "')");
                request.AcceptIfKey(Key);
            }
            else
            {
                Plugin.Log.LogInfo("[RL2MP] [NET] OnConnectionRequest from " + request.RemoteEndPoint + " -> ignored (we are client).");
                request.Reject();
            }
        }

        public void OnPeerConnected(NetPeer peer)
        {
            Plugin.Log.LogInfo("[RL2MP] [NET] OnPeerConnected peerId=" + peer.Id + ", ep=" + peer.Address + ":" + peer.Port + ", IsHost=" + IsHost);

            if (IsHost)
            {
                // v1.6.0：主机先不分配 ID，等客机的 Hello 握手（带 playerId + token），
                // 这样重连的客机可以恢复原来的槽位，而不是被当成新玩家。
                _pendingPeers[peer.Id] = new PendingPeer { Peer = peer, At = Time.realtimeSinceStartup };
                Plugin.Log.LogInfo("[RL2MP] [NET] -> Waiting for Hello handshake from peerId=" + peer.Id);
            }
            else
            {
                _serverPeer = peer;
                SendHello();
            }
        }

        /// <summary>客机 -> 主机：握手包（playerId + token + 协议版本）。首次连接 playerId=-1。</summary>
        private void SendHello()
        {
            var w = new NetDataWriter();
            w.Put((byte)MsgType.Hello);
            w.Put(_resumeId);
            w.Put(_sessionToken);
            w.Put(ProtocolVersion);
            if (_serverPeer != null) _serverPeer.Send(w, DeliveryMethod.ReliableOrdered);
            Plugin.Log.LogInfo("[RL2MP] [NET] -> Hello sent (resumeId=" + _resumeId + ", token=" + _sessionToken
                + ", proto=" + FmtProto(ProtocolVersion) + ")");
        }

        /// <summary>给客机分配一个新的游戏层玩家 ID（跳过已占用的）。</summary>
        private int AllocPlayerId()
        {
            int assignedId = _nextPlayerId;
            while (_knownPlayerIds.Contains(assignedId) || _clients.ContainsKey(assignedId))
            {
                assignedId++;
            }
            _nextPlayerId = assignedId + 1;
            return assignedId;
        }

        /// <summary>HOST：处理 Hello 握手——token 匹配则恢复 ghost 槽位，否则按新玩家分配。</summary>
        private void HandleHello(NetPeer peer, int requestedId, int token)
        {
            if (_peerToPlayerId.ContainsKey(peer.Id))
            {
                return; // 这个 peer 已经注册过了（重复 Hello），忽略
            }

            int assignedId;
            int useToken;
            bool resumed = false;

            string peerIp = peer.Address != null ? peer.Address.ToString() : "";
            GhostSlot ghost;

            if (requestedId > 0 && token != 0
                && _ghosts.TryGetValue(requestedId, out ghost)
                && ghost.Token == token)
            {
                // 重连恢复：宽限期内带相同 token 回来
                assignedId = requestedId;
                useToken = token;
                resumed = true;
                _ghosts.Remove(requestedId);
                Plugin.Log.LogInfo("[RL2MP] [NET] -> RESUMED session: playerId=" + assignedId
                    + " reconnected within grace period (slot restored).");
            }
            else if (!string.IsNullOrEmpty(peerIp) && FindGhostByIp(peerIp, out ghost))
            {
                // v1.9.4：token 失配/丢失（例如游戏卡死后强退、进程重启导致 token 没保住），
                // 但同一 IP 在宽限期内有掉线槽位 -> 判定为同一台机器重连，恢复原槽位，
                // 避免主机多出一个幽灵玩家（原本会分配新 id，旧 ghost 还挂在名单上）。
                assignedId = ghost.PlayerId;
                useToken = ghost.Token;
                resumed = true;
                _ghosts.Remove(ghost.PlayerId);
                Plugin.Log.LogInfo("[RL2MP] [NET] -> RESUMED by IP match: playerId=" + assignedId
                    + " ip=" + peerIp + " (token mismatch, restored slot to avoid ghost player).");
            }
            else
            {
                assignedId = AllocPlayerId();
                useToken = _rng.Next(1, int.MaxValue);
                if (requestedId > 0 && token != 0)
                {
                    Plugin.Log.LogInfo("[RL2MP] [NET] -> Resume REJECTED for playerId=" + requestedId
                        + " (no ghost slot / token mismatch) -> assigned fresh id=" + assignedId);
                }
            }

            RegisterPeer(peer, assignedId, useToken, resumed);
        }

        /// <summary>HOST：按 IP 找宽限期内掉线的 ghost 槽位（token 丢失时的重连兜底）。</summary>
        private bool FindGhostByIp(string ip, out GhostSlot found)
        {
            found = null;
            foreach (var kv in _ghosts)
            {
                if (kv.Value != null && string.Equals(kv.Value.Ip, ip, StringComparison.Ordinal))
                {
                    found = kv.Value;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// HOST：把一个已握手的 peer 正式注册进会话，并发 Welcome + 全量状态重放。
        /// resumed=true 表示是断线重连（槽位、分数都在），false 表示全新加入。
        /// </summary>
        private void RegisterPeer(NetPeer peer, int assignedId, int token, bool resumed)
        {
            _clients[assignedId] = peer;
            _peerToPlayerId[peer.Id] = assignedId;
            _pendingPeers.Remove(peer.Id);

            var slot = Roster.Get(assignedId);
            slot.Token = token;
            slot.Offline = false;
            slot.OfflineSince = 0f;
            slot.Ip = peer.Address != null ? peer.Address.ToString() : "";
            if (!resumed)
            {
                slot.ClearRaceState();
                Roster.SyncSelf();
            }

            var w = new NetDataWriter();
            w.Put((byte)MsgType.Welcome);
            w.Put(assignedId);
            w.Put(token);
            w.Put((byte)(resumed ? 1 : 0));
            w.Put(ProtocolVersion);
            w.Put(NetClock.HostNow);   // v2.5.0：主机时钟粗种子（客机 Seed 单程估计）
            peer.Send(w, DeliveryMethod.ReliableOrdered);
            Plugin.Log.LogInfo("[RL2MP] [NET] -> Welcome sent. assignedId=" + assignedId
                + " token=" + token + " resumed=" + resumed
                + ". Now " + _clients.Count + " client(s).");

            if (_hostMasterSeed > 0)
            {
                var w2 = new NetDataWriter();
                w2.Put((byte)MsgType.LevelSeed);
                w2.Put(_hostMasterSeed);
                peer.Send(w2, DeliveryMethod.ReliableOrdered);
                Plugin.Log.LogInfo("[RL2MP] [NET] -> LevelSeed=" + _hostMasterSeed + " sent to new client.");
            }
            else
            {
                Plugin.Log.LogInfo("[RL2MP] [NET] -> LevelSeed not yet captured. Will broadcast later.");
            }

            // 规则包（周期 + 累赘）也随连接发给新客机
            RaceRulesSync.CaptureAndBroadcast();

            // 竞速进行中：把死亡加时规则补发给新客机
            if (Race.InSession)
            {
                var w3 = new NetDataWriter();
                w3.Put((byte)MsgType.RacePenaltyConfig);
                w3.Put(Race.PenaltyPerDeath);
                peer.Send(w3, DeliveryMethod.ReliableOrdered);
            }

            // 名单里先建好这个玩家的槽位（大厅/计分都按 id 索引）
            Roster.SyncSelf();

            // v2.8.6：新客机一进来就告诉它主持人 + 已在场其他客机的名字
            {
                var nw = new NetDataWriter();
                nw.Put((byte)MsgType.PlayerName);
                nw.Put(LocalPlayerId);
                nw.Put(Plugin.PlayerName);
                peer.Send(nw, DeliveryMethod.ReliableOrdered);
                foreach (var otherSlot in Roster.All)
                {
                    if (otherSlot.Id == LocalPlayerId || otherSlot.Id == assignedId) continue;
                    if (string.IsNullOrEmpty(otherSlot.Name)) continue;
                    var ow = new NetDataWriter();
                    ow.Put((byte)MsgType.PlayerName);
                    ow.Put(otherSlot.Id);
                    ow.Put(otherSlot.Name);
                    peer.Send(ow, DeliveryMethod.ReliableOrdered);
                }
            }

            // 计分规则 + 当前积分（新客机进来看板立刻是对的）
            SendScoreConfig(Score.Mode, Score.RoomPoints, Score.EnemyPoints, Score.BossPoints,
                Score.RelicPenalty, Score.DeathPenalty, Score.DecayPerMinute,
                Score.ChestBronzePoints, Score.ChestSilverPoints, Score.ChestGoldPoints, Score.ChestFairyPoints,
                Score.GoldPerPoint, Score.GoldNgPlusStep, Score.Style);
            if (Race.InSession)
            {
                var w4 = new NetDataWriter();
                w4.Put((byte)MsgType.ScoreState);
                w4.Put(LocalPlayerId);
                w4.Put(Score.LocalScore);
                w4.Put(Score.LocalRooms);
                w4.Put(Score.LocalKills);
                w4.Put(Score.LocalBosses);
                w4.Put(Score.LocalRelics);
                w4.Put(Race.LocalDeaths);
                w4.Put(Score.LocalChests);
                w4.Put(Score.LocalChestPoints);
                w4.Put(Score.LocalCoinPoints);
                w4.Put(Score.LocalGoldConverted);
                peer.Send(w4, DeliveryMethod.ReliableOrdered);
            }

            // 终点 Boss：中途连进来的客机也要知道这一局打哪个 Boss
            if (Race.TargetBossId != 0) SendRaceConfigTo(peer, Race.TargetBossId);

            // 大厅状态：新客机要知道谁已经准备好
            SendRaceLobby();

            // 已有客机把外观再发一遍：否则新来的看不到他们长什么样
            var wChar = new NetDataWriter();
            wChar.Put((byte)MsgType.CharRequest);
            foreach (var kv in _clients)
            {
                if (kv.Key == assignedId) continue;
                if (kv.Value != null && kv.Value.ConnectionState == ConnectionState.Connected)
                    kv.Value.Send(wChar, DeliveryMethod.ReliableOrdered);
            }

            // 角色外观：让新客机看到的分身长得像我（主机自己的外观）
            CharacterSync.SendLocalCharacter();

            if (_knownPlayerIds.Add(assignedId))
            {
                Plugin.Log.LogInfo("[RL2MP] [NET] -> KnownPlayers updated: [" + string.Join(",", _knownPlayerIds) + "]");
                var handler = OnPlayerJoin;
                if (handler != null) handler(assignedId);
            }
        }

        /// <summary>HOST：定期扫描 ghost 槽位，宽限超时的按规则清理。</summary>
        private void GhostSweep()
        {
            if (_ghosts.Count == 0) return;

            _ghostSweepTimer += Time.unscaledDeltaTime;
            if (_ghostSweepTimer < 0.5f) return;
            _ghostSweepTimer = 0f;

            float now = Time.realtimeSinceStartup;
            List<int> expired = null;
            foreach (var kv in _ghosts)
            {
                if (now - kv.Value.At < ReconnectGraceSeconds) continue;
                if (expired == null) expired = new List<int>();
                expired.Add(kv.Key);
            }
            if (expired == null) return;

            foreach (int id in expired)
            {
                _ghosts.Remove(id);
                var slot = Roster.Get(id);
                slot.Offline = false;

                if (slot.Finished)
                {
                    // 已经跑完的：成绩有效，槽位保留到本局结束，只是回不来了
                    Plugin.Log.LogInfo("[RL2MP] [NET] ghost id=" + id + " 超时未归（已完成，成绩保留）。");
                    continue;
                }

                Plugin.Log.LogInfo("[RL2MP] [NET] ghost id=" + id + " 超时未归（" + ReconnectGraceSeconds
                    + "s）-> 标记 DNF 并移出名单。");
                slot.Dnf = true;
                slot.ClearRaceState();
                _clients.Remove(id);
                if (_knownPlayerIds.Remove(id))
                {
                    var handler = OnPlayerLeave;
                    if (handler != null) handler(id);
                }
                Roster.Remove(id);
                Race.BroadcastLobby();
            }
        }

        public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
        {
            Plugin.Log.LogInfo("[RL2MP] [NET] OnPeerDisconnected peerId=" + peer.Id
                + " ep=" + peer.Address + ":" + peer.Port
                + " reason=" + disconnectInfo.Reason);

            if (IsHost)
            {
                int id;
                if (!_peerToPlayerId.TryGetValue(peer.Id, out id))
                {
                    // 未知 peer（可能还没走完 Hello 握手就断了）：清掉 pending 就行
                    _pendingPeers.Remove(peer.Id);
                    Plugin.Log.LogInfo("[RL2MP] [NET] -> 未知 peer 断开（未完成握手），剩余 clients=" + _clients.Count);
                    return;
                }
                _peerToPlayerId.Remove(peer.Id);
                _clients.Remove(id);

                // v1.6.0：不销毁槽位。标记 Offline 并进入重连宽限期，
                // 分数/用时/死亡数全部保留，带相同 token 回来就恢复。
                var slot = Roster.Get(id);
                slot.Offline = true;
                slot.OfflineSince = Time.realtimeSinceStartup;
                slot.Ready = false;
                slot.InWorld = false;
                _ghosts[id] = new GhostSlot { PlayerId = id, Token = slot.Token, At = Time.realtimeSinceStartup, Ip = slot.Ip };

                Plugin.Log.LogInfo("[RL2MP] [NET] -> 客机 playerId=" + id
                    + " 掉线 -> 槽位进入宽限期（" + ReconnectGraceSeconds + "s），成绩保留。");

                // 让其他人立刻看到"XX(掉线)"，也把 ready=false 传下去（防止开着局的误判）
                Race.BroadcastLobby();
            }
            else
            {
                _serverPeer = null;
                if (_manualStop)
                {
                    Plugin.Log.LogWarning("[RL2MP] [NET] -> 主动断开，走完整 Stop()。");
                    Stop();
                    return;
                }

                // v1.6.0：被动断线 -> 不销毁会话，进入自动重连。
                // Roster / Race / Score / HUD 全部保留，重连成功后恢复原 playerId。
                BeginReconnect();
            }
        }

        // ---------------- 客机自动重连 ----------------

        /// <summary>被动断线时进入重连状态：保留会话数据，只断了传输层。</summary>
        private void BeginReconnect()
        {
            _reconnecting = true;
            _reconnectAttempt = 0;
            _reconnectElapsed = 0f;
            _reconnectWaitTimer = 0.5f;   // 第一次尝试前稍微等一下
            _resumeId = LocalPlayerId >= 0 ? LocalPlayerId : _resumeId;

            Plugin.Log.LogWarning("[RL2MP] [NET] -> 与主机断开，开始自动重连（resumeId=" + _resumeId
                + "，最多尝试 " + ClientReconnectBudget + "s）...");
        }

        /// <summary>退避重连循环：1s -> 2s -> 4s -> 6s -> ...，总预算 45s。</summary>
        private void TickReconnect()
        {
            _reconnectElapsed += Time.unscaledDeltaTime;
            if (_reconnectElapsed >= ClientReconnectBudget)
            {
                Plugin.Log.LogError("[RL2MP] [NET] 重连失败：超过 " + ClientReconnectBudget + "s 仍未连上主机。");
                LastError = "重连失败（超时），已与主机断开";
                Stop();
                return;
            }

            _reconnectWaitTimer -= Time.unscaledDeltaTime;
            if (_reconnectWaitTimer > 0f) return;

            _reconnectAttempt++;
            // 1, 2, 4, 6, 6, 6 ...（封顶 6s，太长容易错过主机恰好开着的窗口）
            float delay = Mathf.Min(1f * (1 << Mathf.Min(_reconnectAttempt - 1, 2)) + (_reconnectAttempt > 3 ? 2f : 0f), 6f);
            _reconnectWaitTimer = delay;

            string target = Plugin.HostAddress;
            if (string.IsNullOrWhiteSpace(target))
            {
                Plugin.Log.LogError("[RL2MP] [NET] 重连失败：主机地址为空。");
                LastError = "重连失败：主机地址为空";
                Stop();
                return;
            }

            try
            {
                _netManager.Connect(target, Port, Key);
                Plugin.Log.LogInfo("[RL2MP] [NET] 重连尝试 #" + _reconnectAttempt + " -> " + target + ":" + Port
                    + "（已用时 " + _reconnectElapsed.ToString("F1") + "s，下次间隔 " + delay + "s）");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [NET] 重连尝试 #" + _reconnectAttempt + " 异常: " + ex.Message);
            }
        }

        /// <summary>客机重连成功（收到 resumed Welcome）后的本地恢复动作。</summary>
        private void OnResumedAfterReconnect()
        {
            _reconnecting = false;
            _reconnectAttempt = 0;
            _reconnectElapsed = 0f;
            _lastSentDowned = false;
            ResetRunOverFlag();

            Plugin.Log.LogWarning("[RL2MP] [NET] *** 重连成功！恢复原 playerId=" + LocalPlayerId + "，槽位已还原。 ***");

            // 重新上报本地状态：外观 / 准备状态 / 死亡加时 / 积分
            CharacterSync.SendLocalCharacter();
            Race.OnReconnected();
        }

        public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, byte channelNumber, DeliveryMethod deliveryMethod)
        {
            try
            {
                // v2.5.0: 空包/畸形包绝不进分发（coop 安全网第一层）。
                if (reader.AvailableBytes < 1) return;
                byte type = reader.GetByte();
                _recvCountSinceLastLog++;

                if (type == (byte)MsgType.Hello && IsHost)
                {
                    int requestedId = reader.GetInt();
                    int token = reader.GetInt();
                    int peerProto = reader.AvailableBytes >= 4 ? reader.GetInt() : 0;
                    if (peerProto != ProtocolVersion)
                    {
                        RejectIncompatible(peer, peerProto);
                        return;
                    }
                    HandleHello(peer, requestedId, token);
                }
                else if (type == (byte)MsgType.Welcome)
                {
                    int assignedId = reader.GetInt();
                    int token = reader.GetInt();
                    bool resumed = reader.GetByte() != 0;
                    int hostProto = reader.AvailableBytes >= 4 ? reader.GetInt() : 0;
                    if (hostProto != ProtocolVersion)
                    {
                        Plugin.Log.LogError("[RL2MP] [NET] 主机 mod 协议不一致（主机 " + FmtProto(hostProto)
                            + " / 本机 " + FmtProto(ProtocolVersion) + "），断开连接。");
                        ChatPanel.Receive("系统", "联机失败：两端 mod 版本不一致（主机 " + FmtProto(hostProto)
                            + " / 本机 " + FmtProto(ProtocolVersion) + "）。请把两端的 RL-Mod.dll 换成同一版本。");
                        _manualStop = true;   // 不要自动重连，否则会无限重试
                        Stop();
                        return;
                    }

                    // v2.5.0：主机时钟粗种子（旧主机无此字段则跳过，ping/pong 仍会收敛）。
                    if (reader.AvailableBytes >= 8)
                    {
                        double hostClock = reader.GetDouble();
                        if (NetValidation.IsFiniteD(hostClock))
                        {
                            NetClock.SeedFromWelcome(hostClock, _serverPeer != null ? _serverPeer.Ping : 0f);
                            NetClock.ResetPings();
                        }
                    }

                    bool wasReconnecting = _reconnecting;
                    _sessionToken = token;
                    _resumeId = assignedId;
                    LocalPlayerId = assignedId;
                    SessionActive = true;
                    _knownPlayerIds.Add(assignedId);
                    Roster.SyncSelf();
                    Roster.Get(assignedId).Offline = false;
                    Plugin.Log.LogInfo("[RL2MP] [NET] RECV Welcome -> LocalPlayerId=" + LocalPlayerId
                        + " token=" + token + " resumed=" + resumed
                        + ", knownPlayers=[" + string.Join(",", _knownPlayerIds) + "]");

                    if (!IsHost)
                    {
                        if (wasReconnecting && resumed)
                        {
                            // 重连成功：槽位是原来的，补发本地状态，不清名单
                            OnResumedAfterReconnect();
                        }
                        else
                        {
                            if (wasReconnecting)
                            {
                                // 想恢复但主机判为全新加入（宽限超时/token 失效）：名单重置
                                Plugin.Log.LogWarning("[RL2MP] [NET] 重连被判为全新会话 -> 重置本地名单。");
                                Roster.Clear("resume rejected");
                            }
                            _reconnecting = false;
                            _reconnectAttempt = 0;
                            _reconnectElapsed = 0f;
                            _lastSentDowned = false;
                            ResetRunOverFlag();
                            // 客机也把自己的外观发过去，主机才能正确显示我的分身
                            CharacterSync.SendLocalCharacter();
                        }
                        // v2.8.6：握手完成后立刻报上自己的自定义名字（主机收到会中转给其他客机）
                        SendPlayerName();
                    }
                }
                else if (type == (byte)MsgType.LevelSeed)
                {
                    int seed = reader.GetInt();
                    Plugin.Log.LogInfo("[RL2MP] [NET] RECV LevelSeed=" + seed + " (0x" + seed.ToString("X") + ") -> SeedSync.OnClientReceivedSeed()");
                    if (SeedSync.OnClientReceivedSeed(seed))
                    {
                        // 主机重开了一局（种子变了）：自动重进世界，跟上新地图
                        StartCoroutine(ClientAutoReEnterCo(seed));
                    }
                }
                else if (type == (byte)MsgType.TimePing)
                {
                    // v2.5.0：客机时钟探测——主机立即回 pong（不信任包内任何 id）。
                    double clientTime = reader.GetDouble();
                    if (IsHost && NetValidation.IsFiniteD(clientTime))
                        ReplyTimePong(peer, clientTime);
                }
                else if (type == (byte)MsgType.TimePong)
                {
                    // v2.5.0：客机收 pong——喂入 ClockSync（合法性在 Core 内校验）。
                    if (!IsHost)
                    {
                        double clientTime = reader.GetDouble();
                        double hostTime = reader.GetDouble();
                        NetClock.OnPong(clientTime, hostTime);
                    }
                }
                else if (type == (byte)MsgType.PlayerState)
                {
                    int claimed = reader.GetInt();
                    int id;
                    if (!NetValidation.ResolveSender(this, peer, claimed, out id)) return;
                    float x = reader.GetFloat();
                    float y = reader.GetFloat();
                    float vx = reader.GetFloat();
                    float vy = reader.GetFloat();
                    bool facingRight = reader.GetBool();
                    bool grounded = reader.GetBool();

                    // v2.5.0: 坐标/速度合法性——坏包绝不进渲染层（防 2.38E-41 幻影坐标重演）。
                    if (!NetValidation.Coord(x, y, "PlayerState")) return;
                    vx = NetValidation.ClampVelocity(vx);
                    vy = NetValidation.ClampVelocity(vy);

                    if (IsHost && id != HostId)
                    {
                        var fw = new NetDataWriter();
                        fw.Put((byte)MsgType.PlayerState);
                        fw.Put(id);
                        fw.Put(x); fw.Put(y);
                        fw.Put(vx); fw.Put(vy);
                        fw.Put(facingRight);
                        fw.Put(grounded);

                        int relayedTo = 0;
                        foreach (var kv in _clients)
                        {
                            var c = kv.Value;
                            // 不回发给发送者本人（kv.Key 是游戏层玩家 ID）
                            if (kv.Key == id) continue;
                            if (c.ConnectionState == ConnectionState.Connected)
                            {
                                c.Send(fw, DeliveryMethod.Sequenced);
                                relayedTo++;
                            }
                        }
                        // v2.5.2：不再每条都打日志（20Hz×两端=每秒数十行，真机刷屏拖慢帧率）。
                        // 只累计，由下方 STATS 每 2 秒汇总一行。
                        _relayCountSinceLastLog++;
                    }

                    if (id == LocalPlayerId)
                    {
                        // 收到的是自己的回声（host 转发回来给自己的），忽略
                        return;
                    }

                    if (_knownPlayerIds.Add(id))
                    {
                        Plugin.Log.LogInfo("[RL2MP] [NET] RECV PlayerState from NEW player id=" + id
                            + " -> firing OnPlayerJoin. knownPlayers=[" + string.Join(",", _knownPlayerIds) + "]");
                        var handler = OnPlayerJoin;
                        if (handler != null) handler(id);
                    }

                    if (PlayerSync.Instance != null)
                    {
                        PlayerSync.Instance.HandleRemoteTransform(id, x, y, vx, vy, facingRight, grounded);
                    }
                }
                else if (type == (byte)MsgType.PlayerAnim)
                {
                    int id = reader.GetInt();
                    var p = ReadAnimPayload(reader);

                    if (IsHost && id != HostId)
                    {
                        RelayToOtherClients(id, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)MsgType.PlayerAnim);
                            rw.Put(id);
                            WriteAnimPayload(rw, p);
                        });
                    }

                    if (id != LocalPlayerId && PlayerSync.Instance != null)
                    {
                        PlayerSync.Instance.HandleRemoteAnim(id, p.LayerHashes, p.LayerTimes,
                            p.FloatParams, p.IntParams, p.BoolParams);
                    }
                }
                else if (type == (byte)MsgType.PlayerFlags)
                {
                    int id = reader.GetInt();
                    bool downed = reader.GetBool();

                    if (IsHost && id != HostId)
                    {
                        RelayToOtherClients(id, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)MsgType.PlayerFlags);
                            rw.Put(id);
                            rw.Put(downed);
                        });
                    }

                    if (id != LocalPlayerId && PlayerSync.Instance != null)
                    {
                        PlayerSync.Instance.HandleRemoteFlags(id, downed);
                    }
                }
                else if (type == (byte)MsgType.PlayerDown)
                {
                    int id = reader.GetInt();
                    bool downed = reader.GetBool();

                    if (IsHost && id != HostId)
                    {
                        RelayToOtherClients(id, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)MsgType.PlayerDown);
                            rw.Put(id);
                            rw.Put(downed);
                        });
                    }

                    if (id != LocalPlayerId && PlayerSync.Instance != null)
                    {
                        PlayerSync.Instance.HandleRemoteFlags(id, downed);
                    }
                }
                else if (type == (byte)MsgType.RunOver)
                {
                    int fromId = reader.GetInt();
                    string reason = reader.GetString();

                    // 主机收到客机的结束请求后要转发给其他客机，否则 3 人以上对局会分叉
                    if (IsHost && fromId != HostId)
                    {
                        RelayToOtherClients(fromId, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)MsgType.RunOver);
                            rw.Put(fromId);
                            rw.Put(reason ?? "");
                        });
                    }

                    Plugin.Log.LogInfo("[RL2MP] [RUNOVER] 收到对端(id=" + fromId + ")的本局结束通知：" + reason);
                    DownedState.OnRunOverReceived(fromId);
                }
                else if (type == (byte)MsgType.CoOpRescue)
                {
                    int targetId = reader.GetInt();
                    if (IsHost)
                    {
                        // 主机中转给所有客机：被救者本人起身，其他端清掉尸体标志
                        var fw = new NetDataWriter();
                        fw.Put((byte)MsgType.CoOpRescue);
                        fw.Put(targetId);
                        SendToPeers(fw, DeliveryMethod.ReliableOrdered);
                    }
                    // 被救者本人（主机自己或收到转发的客机）起身；其他端同步状态
                    CoOpRescue.OnRescueReceived(targetId);
                    CoOpRescue.OnRescueBroadcast(targetId);
                    Plugin.Log.LogInfo("[RL2MP] [COOP] RECV CoOpRescue targetId=" + targetId
                        + " (isHost=" + IsHost + ", localId=" + LocalPlayerId + ")");
                }
                else if (type == (byte)MsgType.EnemyDamage || type == (byte)MsgType.EnemyDied)
                {
                    int claimed = reader.GetInt();
                    int senderId;
                    if (!NetValidation.ResolveSender(this, peer, claimed, out senderId)) return;
                    int key = reader.GetInt();
                    bool died = type == (byte)MsgType.EnemyDied;
                    float dmg = died ? 0f : reader.GetFloat();
                    // v2.2.0：附带怪物类型 + 世界坐标 —— 两端房间刷怪序号漂移时按
                    // "同类型且位置最近"兜底匹配，避免伤害被静默丢弃（打不动怪）。
                    int etype = reader.GetInt();
                    float px = reader.GetFloat();
                    float py = reader.GetFloat();

                    // v2.5.0: 伤害与坐标校验——NaN/负数/超上限/幻影坐标一律丢弃。
                    if (!died && !NetValidation.Damage(dmg, "EnemyDamage")) return;
                    if (!NetValidation.Coord(px, py, "EnemyDamage")) return;

                    // 主机中转给其他客机（不回发送者本人）
                    if (IsHost)
                    {
                        RelayToOtherClients(senderId, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)type);
                            rw.Put(senderId);
                            rw.Put(key);
                            if (!died) rw.Put(dmg);
                            rw.Put(etype);
                            rw.Put(px);
                            rw.Put(py);
                        });
                    }

                    // 发送者自己不应用（它本地已由原生伤害结算扣过血）
                    if (senderId != LocalPlayerId && EnemySync.Active)
                    {
                        if (died) EnemySync.ApplyRemoteDeath(key, etype, px, py);
                        else EnemySync.ApplyRemoteDamage(key, dmg, etype, px, py);
                    }
                }
                else if (type == (byte)MsgType.EnemyStateSnapshot)
                {
                    // v1.8.0：主机权威血量快照——客机用它对账（晚加入 / 暂停错过事件 / 丢包都能追上）
                    if (!IsHost && EnemySync.Active) EnemySync.ApplySnapshot(reader);
                }
                else if (type == (byte)MsgType.EnemyAlive)
                {
                    // v2.5.0：主机权威存活集——客机据此补杀自己多出来的活怪。
                    if (!IsHost && EnemyAliveSync.Active) EnemyAliveSync.ApplyAlive(reader);
                }
                else if (type == (byte)MsgType.ObstaclePose)
                {
                    // v2.5.0：主机权威障碍物位置——客机存起来，LateUpdate 覆盖。
                    if (!IsHost && ObstacleSync.Active) ObstacleSync.ApplySnapshot(reader);
                }
                else if (type == (byte)MsgType.EnemyPosition)
                {
                    // v2.0.0：主机权威怪物位置——客机按 key 存字典，LateUpdate 覆盖本地怪 transform
                    if (!IsHost && EnemyPositionSync.Active) EnemyPositionSync.ApplySnapshot(reader);
                }
                else if (type == (byte)MsgType.RoomTravel)
                {
                    // v2.3.2：全员换房——带 seq（补发/去重）+ 对端实际落点世界坐标（px,py）。
                    // 房间坐标(cx,cy)在目标房间未激活时可能是 (0,0)，所以接收端优先用落点找房间。
                    int claimed = reader.GetInt();
                    int senderId;
                    if (!NetValidation.ResolveSender(this, peer, claimed, out senderId)) return;
                    ushort seq = reader.GetUShort();
                    float cx = reader.GetFloat();
                    float cy = reader.GetFloat();
                    float px = reader.GetFloat();
                    float py = reader.GetFloat();
                    byte side = reader.GetByte();
                    int number = reader.GetInt();

                    // v2.5.0: 换房坐标必须合法——这是历史"幻影坐标 2.38E-41"的源头。
                    if (!NetValidation.Coord(cx, cy, "RoomTravel.room")) return;
                    if (!NetValidation.Coord(px, py, "RoomTravel.landing")) return;
                    if (side > 3 || number < 0 || number > 8) { NetValidation.Warn("RoomTravel 门参数非法 -> 丢包"); return; }

                    if (IsHost && senderId != LocalPlayerId)
                    {
                        var rw = new NetDataWriter();
                        rw.Put((byte)MsgType.RoomTravel);
                        rw.Put(senderId);
                        rw.Put(seq);
                        rw.Put(cx); rw.Put(cy);
                        rw.Put(px); rw.Put(py);
                        rw.Put(side); rw.Put(number);
                        ForwardToOtherClients(senderId, rw);
                    }
                    RoomTravelSync.ApplyTravel(senderId, seq, cx, cy, side, number, px, py);
                }
                else if (type == (byte)MsgType.SceneChange)
                {
                    // v2.0.0：全员场景切换——任一端触发，主机中转，所有其他端跟随
                    int senderId = reader.GetInt();
                    int sceneId = reader.GetInt();
                    int transitionId = reader.GetInt();
                    if (IsHost && senderId != LocalPlayerId)
                    {
                        var sw = new NetDataWriter();
                        sw.Put((byte)MsgType.SceneChange);
                        sw.Put(senderId);
                        sw.Put(sceneId); sw.Put(transitionId);
                        ForwardToOtherClients(senderId, sw);
                    }
                    if (senderId != LocalPlayerId) SceneChangeSync.FollowHost(senderId, sceneId, transitionId);
                }
                else if (type == (byte)MsgType.BossProgress)
                {
                    // v2.0.1：主机权威 Boss 位图——客机内存对齐门状态（不动自己存档收益）
                    ushort mask = reader.GetUShort();
                    if (!IsHost) BossProgressSync.ApplySnapshot(mask);
                }
                else if (type == (byte)MsgType.SkillTree)
                {
                    // v2.0.2：协作技能树共享——取 max 并集，写各自存档
                    SkillTreeSync.OnReceive(reader);
                }
                else if (type == (byte)MsgType.PlayerHurt)
                {
                    // v2.1.0：分身受击——被击中的分身所在端转发给真身持有者，主机顺带中转（3 人局）
                    int claimed = reader.GetInt();
                    int senderId;
                    if (!NetValidation.ResolveSender(this, peer, claimed, out senderId)) return;
                    int targetId = reader.GetInt();
                    float hDmg = reader.GetFloat();
                    float hStun = reader.GetFloat();
                    float hSx = reader.GetFloat();
                    float hSy = reader.GetFloat();

                    // v2.5.0: 伤害/眩晕/来源坐标校验。
                    if (!NetValidation.Damage(hDmg, "PlayerHurt")) return;
                    hStun = NetValidation.ClampAmount(hStun);
                    if (!NetValidation.Coord(hSx, hSy, "PlayerHurt")) return;
                    if (!NetValidation.PlayerId(targetId, "PlayerHurt.targetId")) return;

                    if (IsHost && senderId != LocalPlayerId && targetId != LocalPlayerId
                        && targetId != senderId)
                    {
                        RelayToOtherClients(senderId, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)MsgType.PlayerHurt);
                            rw.Put(senderId);
                            rw.Put(targetId);
                            rw.Put(hDmg);
                            rw.Put(hStun);
                            rw.Put(hSx);
                            rw.Put(hSy);
                        });
                    }

                    PlayerHurtSync.OnReceive(senderId, targetId, hDmg, hStun, hSx, hSy);
                }
                else if (type == (byte)MsgType.DashFx)
                {
                    // v1.8.4：冲刺/虚冲特效事件——主机中转，其他端补放残影
                    int dashOwner = reader.GetInt();
                    bool dashVoid = reader.GetBool();
                    if (IsHost && dashOwner != HostId)
                    {
                        var dw = new NetDataWriter();
                        dw.Put((byte)MsgType.DashFx);
                        dw.Put(dashOwner);
                        dw.Put(dashVoid);
                        ForwardToOtherClients(dashOwner, dw);
                    }
                    if (dashOwner != LocalPlayerId) DashFxSync.PlayRemoteDash(dashOwner, dashVoid);
                }
                else if (type == (byte)MsgType.Projectile)
                {
                    int id = reader.GetInt();
                    uint spawnId = reader.GetUInt();  // v2.7.0：跨端关联发射/消失
                    string pname = reader.GetString();
                    float px = reader.GetFloat(); float py = reader.GetFloat(); float pz = reader.GetFloat();
                    float eulerZ = reader.GetFloat();
                    float scaleX = reader.GetFloat();
                    float hx = reader.GetFloat(); float hy = reader.GetFloat();
                    float speed = reader.GetFloat();
                    float accX = reader.GetFloat(); float accY = reader.GetFloat();
                    bool snap = reader.GetBool();
                    float snapLx = 0f, snapLy = 0f, snapLz = 0f, snapRotZ = 0f;
                    if (snap)
                    {
                        snapLx = reader.GetFloat(); snapLy = reader.GetFloat(); snapLz = reader.GetFloat();
                        snapRotZ = reader.GetFloat();
                    }
                    float lifespan = reader.GetFloat();

                    if (IsHost && id != HostId)
                    {
                        RelayToOtherClients(id, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)MsgType.Projectile);
                            rw.Put(id);
                            rw.Put(spawnId);
                            rw.Put(pname);
                            rw.Put(px); rw.Put(py); rw.Put(pz);
                            rw.Put(eulerZ);
                            rw.Put(scaleX);
                            rw.Put(hx); rw.Put(hy);
                            rw.Put(speed);
                            rw.Put(accX); rw.Put(accY);
                            rw.Put(snap);
                            if (snap)
                            {
                                rw.Put(snapLx); rw.Put(snapLy); rw.Put(snapLz);
                                rw.Put(snapRotZ);
                            }
                            rw.Put(lifespan);
                        });
                    }

                    if (id != LocalPlayerId)
                    {
                        ProjectileSync.SpawnRemoteVisual(id, spawnId, pname, px, py, pz, eulerZ, scaleX,
                            hx, hy, speed, accX, accY, snap, snapLx, snapLy, snapLz, snapRotZ, lifespan);
                    }
                }
                else if (type == (byte)MsgType.ProjectileDespawn)
                {
                    int id = reader.GetInt();
                    uint spawnId = reader.GetUInt();
                    if (IsHost && id != HostId)
                    {
                        var dw = new NetDataWriter();
                        dw.Put((byte)MsgType.ProjectileDespawn);
                        dw.Put(id);
                        dw.Put(spawnId);
                        ForwardToOtherClients(id, dw);
                    }
                    if (id != LocalPlayerId) ProjectileSync.HandleRemoteDespawn(id, spawnId);
                }
                else if (type == (byte)MsgType.RaceConfig)
                {
                    int bossId = reader.GetByte();
                    Race.HandleRaceConfig(bossId);
                }
                else if (type == (byte)MsgType.RacePenaltyConfig)
                {
                    float sec = reader.GetFloat();
                    Race.HandleRacePenaltyConfig(sec);
                }
                else if (type == (byte)MsgType.RacePenalty)
                {
                    int pid = reader.GetInt();
                    int deaths = reader.GetInt();
                    float accum = reader.GetFloat();
                    Race.HandleRacePenalty(pid, deaths, accum);
                    RelayToOtherClients(pid, delegate (NetDataWriter rw)
                    {
                        rw.Put((byte)MsgType.RacePenalty);
                        rw.Put(pid);
                        rw.Put(deaths);
                        rw.Put(accum);
                    });
                }
                else if (type == (byte)MsgType.RaceReady)
                {
                    int pid = reader.GetInt();
                    bool ready = reader.GetBool();
                    bool inWorld = reader.GetBool();
                    // 只接受真正连接着的客机上报的 id（主机自己不需要走网络）
                    if (IsHost && (pid <= 0 || !_clients.ContainsKey(pid)))
                    {
                        Plugin.Log.LogWarning("[RL2MP] [NET] 丢弃非法 RaceReady（id=" + pid + "）");
                    }
                    else
                    {
                        Race.HandleRaceReady(pid, ready, inWorld);
                    }
                }
                else if (type == (byte)MsgType.RaceLobby)
                {
                    int count = reader.GetInt();
                    for (int i = 0; i < count; i++)
                    {
                        int pid = reader.GetInt();
                        bool ready = reader.GetBool();
                        bool inWorld = reader.GetBool();
                        bool offline = reader.GetBool();
                        Race.HandleLobbyEntry(pid, ready, inWorld, offline);
                    }
                }
                else if (type == (byte)MsgType.RaceStart)
                {
                    float cd = reader.GetFloat();
                    Race.HandleRaceStart(cd);
                }
                else if (type == (byte)MsgType.RaceFinish)
                {
                    int pid = reader.GetInt();
                    float elapsed = reader.GetFloat();
                    int score = reader.GetInt();
                    if (IsHost && pid > 0) Race.HandleClientFinish(pid, elapsed, score);
                }
                else if (type == (byte)MsgType.RaceGrace)
                {
                    float sec = reader.GetFloat();
                    Race.HandleGrace(sec);
                }
                else if (type == (byte)MsgType.RacePlayerFinish)
                {
                    int pid = reader.GetInt();
                    float elapsed = reader.GetFloat();
                    int score = reader.GetInt();
                    Race.HandleRemoteFinish(pid, elapsed, score);
                }
                else if (type == (byte)MsgType.RaceResult)
                {
                    int winnerId = reader.GetInt();
                    bool scoreMode = reader.GetBool();
                    int count = reader.GetInt();
                    int[] ids = new int[count];
                    float[] times = new float[count];
                    int[] scores = new int[count];
                    bool[] dnf = new bool[count];
                    for (int i = 0; i < count; i++)
                    {
                        ids[i] = reader.GetInt();
                        times[i] = reader.GetFloat();
                        scores[i] = reader.GetInt();
                        dnf[i] = reader.GetBool();
                    }
                    Race.HandleRaceResult(winnerId, scoreMode, ids, times, scores, dnf);
                }
                else if (type == (byte)MsgType.ScoreConfig)
                {
                    RaceGameMode mode = (RaceGameMode)reader.GetByte();
                    int roomP = reader.GetInt();
                    int enemyP = reader.GetInt();
                    int bossP = reader.GetInt();
                    int relicP = reader.GetInt();
                    int deathP = reader.GetInt();
                    int decayP = reader.GetInt();
                    int chestB = reader.GetInt();
                    int chestS = reader.GetInt();
                    int chestG = reader.GetInt();
                    int chestF = reader.GetInt();
                    int goldPerPoint = reader.GetInt();
                    float goldStep = reader.GetFloat();
                    // v1.7.6：玩法维度放包末尾；旧版主机没有这个字节，读不到就保持默认竞速
                    PlayStyle style = PlayStyle.Race;
                    try { style = (PlayStyle)reader.GetByte(); }
                    catch { /* 旧版协议兼容 */ }
                    Score.ApplyConfig(mode, roomP, enemyP, bossP, relicP, deathP, decayP,
                        chestB, chestS, chestG, chestF, goldPerPoint, goldStep, style);
                }
                else if (type == (byte)MsgType.ScoreState)
                {
                    int pid = reader.GetInt();
                    int sc = reader.GetInt();
                    int rm = reader.GetInt();
                    int kl = reader.GetInt();
                    int bs = reader.GetInt();
                    int rl = reader.GetInt();
                    int dt = reader.GetInt();
                    int ch = reader.GetInt();
                    int chp = reader.GetInt();
                    int cp = reader.GetInt();
                    int gc = reader.GetInt();
                    Score.HandleState(pid, sc, rm, kl, bs, rl, dt, ch, chp, cp, gc);
                    RelayToOtherClients(pid, delegate (NetDataWriter rw)
                    {
                        rw.Put((byte)MsgType.ScoreState);
                        rw.Put(pid);
                        rw.Put(sc);
                        rw.Put(rm);
                        rw.Put(kl);
                        rw.Put(bs);
                        rw.Put(rl);
                        rw.Put(dt);
                        rw.Put(ch);
                        rw.Put(chp);
                        rw.Put(cp);
                        rw.Put(gc);
                    });
                }
                else if (type == (byte)MsgType.RaceEnd)
                {
                    Race.HandleRaceEnd();
                }
                else if (type == (byte)MsgType.RaceRules)
                {
                    int ngPlus = reader.GetInt();
                    int highestBeaten = reader.GetInt();
                    int count = reader.GetInt();
                    var burdens = new Dictionary<BurdenType, int>();
                    for (int i = 0; i < count; i++)
                    {
                        BurdenType bt = (BurdenType)reader.GetByte();
                        int lv = reader.GetInt();
                        if (bt != BurdenType.None) burdens[bt] = lv;
                    }
                    Plugin.Log.LogInfo("[RL2MP] [NET] RECV RaceRules: NG+=" + ngPlus
                        + " HighestNG+Beaten=" + highestBeaten + " burdens=" + count);
                    RaceRulesSync.ApplyHostRules(ngPlus, highestBeaten, burdens);
                }
                else if (type == (byte)MsgType.MapData)
                {
                    int totalChunks = reader.GetInt();
                    int index = reader.GetInt();
                    int totalBytes = reader.GetInt();
                    byte[] chunk = reader.GetBytesWithLength();
                    MapSync.OnChunk(totalChunks, index, totalBytes, chunk);
                }
                else if (type == (byte)MsgType.CharRequest)
                {
                    Plugin.Log.LogInfo("[RL2MP] [CHAR] 主机要求补发外观 -> 重发。");
                    CharacterSync.SendLocalCharacter();
                }
                else if (type == (byte)MsgType.CharacterData)
                {
                    int pid = reader.GetInt();
                    byte[] bytes = reader.GetBytesWithLength();
                    CharacterSync.OnReceive(pid, bytes);

                    // 主机转发给其他客机：4 人局里每个人都要看到其他人的外观
                    if (IsHost && pid != LocalPlayerId)
                    {
                        RelayToOtherClients(pid, delegate (NetDataWriter rw)
                        {
                            rw.Put((byte)MsgType.CharacterData);
                            rw.Put(pid);
                            rw.PutBytesWithLength(bytes);
                        });
                    }
                }
                else if (type == (byte)MsgType.NewGame)
                {
                    int ng = reader.GetInt();
                    Plugin.Log.LogInfo("[RL2MP] [NET] RECV NewGame: 主机开启了新周目 NG+=" + ng);
                    RaceRulesSync.OnHostNewGame(ng);
                }
                else if (type == (byte)MsgType.Chat)
                {
                    int claimed = reader.GetInt();
                    int senderId;
                    if (!NetValidation.ResolveSender(this, peer, claimed, out senderId)) return;
                    string text = NetValidation.Text(reader.GetString());
                    // 主机中转给其他客机
                    if (IsHost && senderId != LocalPlayerId)
                    {
                        var cw = new NetDataWriter();
                        cw.Put((byte)MsgType.Chat);
                        cw.Put(senderId);
                        cw.Put(text);
                        ForwardToOtherClients(senderId, cw);
                    }
                    var chatSlot = Roster.Get(senderId);
                    string tag = string.IsNullOrEmpty(chatSlot.Name)
                        ? (senderId == HostId ? "主机" : ("P" + senderId)) : chatSlot.Name;
                    ChatPanel.Receive(tag, text);
                }
                else if (type == (byte)MsgType.PlayerName)
                {
                    int claimed = reader.GetInt();
                    int senderId;
                    if (!NetValidation.ResolveSender(this, peer, claimed, out senderId)) return;
                    string name = NetValidation.Text(reader.GetString());
                    if (name != null && name.Length > 16) name = name.Substring(0, 16);
                    if (string.IsNullOrEmpty(name)) return;
                    Roster.Get(senderId).Name = name;
                    Plugin.Log.LogInfo("[RL2MP] [NET] RECV PlayerName P" + senderId + " -> '" + name + "'");
                    // 主机中转给其他客机；客机侧 ResolveSender 信任主机盖章的 id
                    if (IsHost && senderId != LocalPlayerId)
                    {
                        var nw = new NetDataWriter();
                        nw.Put((byte)MsgType.PlayerName);
                        nw.Put(senderId);
                        nw.Put(name);
                        ForwardToOtherClients(senderId, nw);
                    }
                }
                else if (type == (byte)MsgType.PlayerKicked)
                {
                    string reason = reader.GetString();
                    Plugin.Log.LogWarning("[RL2MP] [NET] 被房主踢出: " + reason);
                    ChatPanel.Receive("系统", "你被房主移出房间：" + reason);
                    _manualStop = true; // 被踢后不要自动重连
                    Stop();
                }
                else
                {
                    Plugin.Log.LogWarning("[RL2MP] [NET] RECV unknown msg type byte=" + type + " from peerId=" + peer.Id);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[RL2MP] [NET] OnNetworkReceive EXCEPTION: " + ex.Message + "\n" + ex.StackTrace);
            }
        }

        public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
        {
            Plugin.Log.LogWarning("[RL2MP] [NET] OnNetworkError endpoint=" + endPoint + " socketError=" + socketError);
        }

        public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
        {
            Plugin.Log.LogInfo("[RL2MP] [NET] OnNetworkReceiveUnconnected from " + remoteEndPoint + " type=" + messageType + " (ignored)");
        }

        public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
        {
            // 这个回调每 500ms 一次，太吵。只在 latency 突变时打
            if (latency > 200)
            {
                Plugin.Log.LogInfo("[RL2MP] [NET] Latency high: peerId=" + peer.Id + " latency=" + latency + "ms");
            }
        }

        public void StartDebugSim()
        {
            Plugin.Log.LogInfo("[RL2MP] StartDebugSim() called. Connected=" + Connected + ", debugSimActive=" + _debugSimActive);

            if (_debugSimActive)
            {
                Plugin.Log.LogWarning("[RL2MP] [DEBUG SIM] Already active. Abort.");
                return;
            }
            if (Connected)
            {
                Plugin.Log.LogWarning("[RL2MP] [DEBUG SIM] Network is running. Stop it first. Abort.");
                return;
            }

            _debugSimActive = true;
            _debugFakeIds.Clear();
            _debugFakeOffsets.Clear();
            _debugFakeFacing.Clear();
            _knownPlayerIds.Clear();

            Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] State cleared. Adding first fake player...");
            DebugAddFakePlayer();

            Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] STARTED. Will simulate " + _debugFakeIds.Count + " fake player(s).");
        }

        public void StopDebugSim()
        {
            Plugin.Log.LogInfo("[RL2MP] StopDebugSim() called. Current active=" + _debugSimActive);
            if (!_debugSimActive)
            {
                Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] Not active. Nothing to do.");
                return;
            }

            _debugSimActive = false;

            Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] Removing " + _debugFakeIds.Count + " fake players...");
            foreach (int id in new List<int>(_debugFakeIds))
            {
                if (_knownPlayerIds.Remove(id))
                {
                    Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] -> Firing OnPlayerLeave for id=" + id);
                    var handler = OnPlayerLeave;
                    if (handler != null) handler(id);
                }
            }
            _debugFakeIds.Clear();
            _debugFakeOffsets.Clear();
            _debugFakeFacing.Clear();
            _knownPlayerIds.Clear();

            Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] STOPPED.");
        }

        public void DebugAddFakePlayer()
        {
            Plugin.Log.LogInfo("[RL2MP] DebugAddFakePlayer() called. active=" + _debugSimActive);
            if (!_debugSimActive)
            {
                Plugin.Log.LogWarning("[RL2MP] [DEBUG SIM] Not active. Add called but no-op.");
                return;
            }

            int nextId = 1;
            while (_knownPlayerIds.Contains(nextId)) nextId++;

            _debugFakeIds.Add(nextId);
            _debugFakeOffsets[nextId] = new Vector3(1.5f, 0f, 0f);
            _debugFakeFacing[nextId] = true;
            _knownPlayerIds.Add(nextId);

            Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] Added fake player ID=" + nextId
                + ". Total fake=" + _debugFakeIds.Count
                + ", knownPlayers=[" + string.Join(",", _knownPlayerIds) + "]");

            var handler = OnPlayerJoin;
            if (handler != null) handler(nextId);
            Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] OnPlayerJoin fired for id=" + nextId + ".");
        }

        public void DebugClearFakePlayers()
        {
            Plugin.Log.LogInfo("[RL2MP] DebugClearFakePlayers() called. Current count=" + _debugFakeIds.Count);
            for (int i = _debugFakeIds.Count - 1; i >= 0; i--)
            {
                int id = _debugFakeIds[i];
                if (_knownPlayerIds.Remove(id))
                {
                    Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] -> OnPlayerLeave for id=" + id);
                    var handler = OnPlayerLeave;
                    if (handler != null) handler(id);
                }
            }
            _debugFakeIds.Clear();
            _debugFakeOffsets.Clear();
            _debugFakeFacing.Clear();
            Plugin.Log.LogInfo("[RL2MP] [DEBUG SIM] All fake players cleared. knownPlayers=[" + string.Join(",", _knownPlayerIds) + "]");
        }
    }
}
