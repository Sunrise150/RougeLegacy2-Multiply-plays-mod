using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL2Multiplayer
{
    public enum RacePhase
    {
        Idle = 0,           // 没在竞速
        WaitingReady = 1,   // 主机已定终点，等双方准备
        Countdown = 2,      // 3 秒倒计时
        Racing = 3,         // 竞速中
        Finished = 4,       // 已出胜负
    }

    /// <summary>
    /// 竞速模式状态机。设计原则与 DownedState 相同：
    /// 全静态字段，不依赖 GameObject 存活；网络收发都走 NetworkManager。
    ///
    /// 流程：
    ///   主机面板选终点 Boss  -> 广播 RaceConfig（双方进入 WaitingReady）
    ///   双方按"准备"         -> RaceReady 消息互相通知
    ///   主机看到两边都好了   -> 广播 RaceStart -> 双方 3 秒倒计时 -> Racing，右上角开始计时
    ///   谁先杀掉目标 Boss    -> RaceFinish 上报 -> 主机仲裁 -> RaceResult 广播
    ///   任意一方在面板按"结束对局"（主机）-> RaceEnd -> 双方断开
    ///
    /// 死亡惩罚：竞速期间死亡 = 立刻原地复活，但成绩加时（每次 +PenaltyPerDeath 秒）。
    /// </summary>
    public static class Race
    {
        /// <summary>每次死亡的加时（秒），主机可在面板里设定。</summary>
        public static float PenaltyPerDeath = 60f;
        /// <summary>第一个人到达终点后，其余玩家还有多少秒可以跑完（0 = 立刻结算）。</summary>
        public static float FinishGraceSeconds = 60f;

        /// <summary>本地：累计加时（秒）与死亡次数。</summary>
        public static float LocalPenaltySeconds;
        public static int LocalDeaths;

        private const float CountdownSeconds = 3f;

        public static RacePhase Phase = RacePhase.Idle;
        public static int TargetBossId;            // BossID 枚举值，主机设定
        public static float LocalFinishTime = -1f; // 本地完成用时（秒），-1=未完成
        public static int WinnerId = -1;           // -1=未定
        public static string ResultText = "";

        private static float _graceEndRealtime = -1f;  // 首位完成后的等待截止时刻

        /// <summary>本地准备状态（真正存在 Roster.Local 里，保证 N 人一致）。</summary>
        public static bool LocalReady
        {
            get { return Roster.Local != null && Roster.Local.Ready; }
            set { if (Roster.Local != null) Roster.Local.Ready = value; }
        }

        /// <summary>所有其他玩家都准备好了吗（含"必须在游戏世界里"）。</summary>
        public static bool RemoteReady { get { return Roster.AllOthersReady(); } }

        /// <summary>所有其他玩家都在游戏世界里吗。</summary>
        public static bool RemoteInWorld
        {
            get
            {
                int n = 0;
                foreach (var s in Roster.All)
                {
                    if (Roster.Local != null && s.Id == Roster.Local.Id) continue;
                    n++;
                    if (!s.InWorld) return false;
                }
                return n > 0;
            }
        }

        /// <summary>其他人里最好的完成用时（HUD/结算用，没有就 -1）。</summary>
        public static float RemoteFinishTime
        {
            get
            {
                float best = -1f;
                foreach (var s in Roster.All)
                {
                    if (Roster.Local != null && s.Id == Roster.Local.Id) continue;
                    if (!s.Finished) continue;
                    if (best < 0f || s.FinishTime < best) best = s.FinishTime;
                }
                return best;
            }
        }

        /// <summary>其他玩家死亡次数合计（HUD 简表用）。</summary>
        public static int RemoteDeaths
        {
            get
            {
                int n = 0;
                foreach (var s in Roster.All)
                {
                    if (Roster.Local != null && s.Id == Roster.Local.Id) continue;
                    n += s.Deaths;
                }
                return n;
            }
        }

        public static float RemotePenaltySeconds
        {
            get
            {
                float n = 0f;
                foreach (var s in Roster.All)
                {
                    if (Roster.Local != null && s.Id == Roster.Local.Id) continue;
                    n += s.PenaltySeconds;
                }
                return n;
            }
        }

        private static bool _reportedInWorld;      // 上次上报给主机的 inWorld 值
        private static float _autoEnterCooldown;   // 自动进图的最小间隔，防抖
        private static float _notInGameSince = -1f; // 准备好了但还没进游戏的起始时间
        private static bool _autoEnterWarned;

        /// <summary>
        /// 本地是否"真的在游戏里"：World 场景 + 已经捕获到本地玩家。
        /// 光看场景名不够——加载中的 World 还没有 PlayerController，进去了也跑不了。
        /// </summary>
        public static bool LocalInGame
        {
            get
            {
                try
                {
                    if (SceneManager.GetActiveScene().name != "World") return false;
                    return PlayerSync.Instance != null && PlayerSync.Instance.LocalPlayer != null;
                }
                catch
                {
                    return false;
                }
            }
        }

        internal static float CountdownEndRealtime;
        internal static float StartRealtime;

        /// <summary>竞速会话进行中（已配置终点、还没结束）。</summary>
        public static bool InSession
        {
            get { return Phase != RacePhase.Idle; }
        }

        /// <summary>正在竞速或倒计时（死亡惩罚生效的时间段）。</summary>
        public static bool InRacePhase
        {
            get { return Phase == RacePhase.Countdown || Phase == RacePhase.Racing; }
        }

        public static float Elapsed
        {
            get
            {
                if (Phase == RacePhase.Racing) return Time.realtimeSinceStartup - StartRealtime;
                if (Phase == RacePhase.Finished)
                {
                    if (LocalFinishTime >= 0f) return LocalFinishTime;
                    return BestFinish();
                }
                return -1f;
            }
        }

        /// <summary>所有人里最快的完成用时（没人完成返回 -1）。</summary>
        private static float BestFinish()
        {
            float best = -1f;
            foreach (var s in Roster.All)
            {
                if (!s.Finished || s.Dnf) continue;
                if (best < 0f || s.FinishTime < best) best = s.FinishTime;
            }
            return best;
        }

        /// <summary>把本局所有玩家状态清干净（换终点 / 开跑 / 重置时用，名单本身保留）。</summary>
        internal static void ResetRaceState()
        {
            Roster.SyncSelf();
            Roster.ClearAllRaceState();
            _reportedInWorld = false;
            LocalFinishTime = -1f;
            LocalPenaltySeconds = 0f;
            LocalDeaths = 0;
            WinnerId = -1;
            ResultText = "";
            _graceEndRealtime = -1f;
            Score.ResetForRace();
        }

        // ---------------- 主机：设定终点 ----------------

        public static void HostSetTargetBoss(int bossId)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] HostSetTargetBoss: 只有主机能设定终点。");
                return;
            }

            TargetBossId = bossId;
            ResetRaceState();
            Phase = RacePhase.WaitingReady;

            Plugin.Log.LogInfo("[RL2MP] [RACE] HOST 设定终点 Boss=" + BossName(bossId)
                + " (" + bossId + ") -> 广播 RaceConfig。");
            nm.SendRaceConfig(bossId);

            // 大厅状态一起发：客机收到 RaceConfig 会重置本地名单，主机这边补一份权威状态
            BroadcastLobby();

            // 规则包一起发：周期 + 累赘等级（客机世界与主机一致的另一半前提）
            RaceRulesSync.CaptureAndBroadcast();
        }

        // ---------------- 网络消息处理（NetworkManager 调进来） ----------------

        internal static void HandleRaceConfig(int bossId)
        {
            Plugin.Log.LogInfo("[RL2MP] [RACE] 收到终点配置 Boss=" + BossName(bossId)
                + " (" + bossId + ") -> 进入等待准备。");
            TargetBossId = bossId;
            ResetRaceState();
            if (Phase == RacePhase.Idle || Phase == RacePhase.Finished)
                Phase = RacePhase.WaitingReady;
        }

        public static void ToggleLocalReady()
        {
            if (Phase != RacePhase.WaitingReady)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 当前阶段不支持准备（Phase=" + Phase + "）。");
                return;
            }

            Roster.SyncSelf();
            var me = Roster.Local;
            if (me == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 还没有本地玩家 ID，无法准备（等连接建立）。");
                return;
            }

            me.Ready = !me.Ready;

            // 还在主菜单/选人界面按准备毫无意义——直接把他推进世界。
            // 进图并捕获到玩家后 Tick 会自动补发 ready(inWorld=true)。
            if (me.Ready && !LocalInGame)
            {
                var nmCheck = NetworkManager.Instance;
                if (nmCheck != null && nmCheck.IsHost)
                {
                    Plugin.Log.LogWarning("[RL2MP] [RACE] 主机自己还没在游戏世界里，请先进入游戏再准备。");
                }
                else
                {
                    AutoEnterWorld("按下准备时不在游戏世界中");
                }
            }

            _reportedInWorld = LocalInGame;
            me.InWorld = _reportedInWorld;

            var nm = NetworkManager.Instance;
            if (nm != null)
            {
                nm.SendRaceReady(me.Ready, me.InWorld);
                if (nm.IsHost) BroadcastLobby();
            }

            Plugin.Log.LogInfo("[RL2MP] [RACE] 本地准备状态 -> " + me.Ready
                + "（在游戏中=" + me.InWorld + "，名单 " + Roster.Count + " 人）");

            // 主机：所有人都准备好了就直接开跑
            if (nm != null && nm.IsHost && me.Ready && Roster.AllOthersReady())
            {
                StartRace();
            }
        }

        internal static void HandleRaceReady(int id, bool ready, bool inWorld)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return;

            var s = Roster.Get(id);
            s.Ready = ready;
            s.InWorld = inWorld;

            Plugin.Log.LogInfo("[RL2MP] [RACE] " + s.Tag + " 准备状态 -> " + ready
                + "（在游戏中=" + inWorld + "，已就绪 " + Roster.ReadyCount() + "/" + Roster.Count + "）");

            // 把大厅状态同步给所有客机（其他人也要看到谁准备好了）
            BroadcastLobby();

            if (ready && !inWorld)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] " + s.Tag
                    + " 已准备但还在主菜单/加载中 —— 等他进入游戏世界后会自动补发准备。");
                return;
            }

            if (ready && LocalReady && Phase == RacePhase.WaitingReady && Roster.AllOthersReady())
            {
                StartRace();
            }
        }

        /// <summary>HOST：把当前大厅状态（每人一份 ready/inWorld）广播给所有客机。</summary>
        internal static void BroadcastLobby()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost || !nm.Connected) return;
            nm.SendRaceLobby();
        }

        /// <summary>CLIENT：收到大厅状态里的一个条目。</summary>
        internal static void HandleLobbyEntry(int id, bool ready, bool inWorld, bool offline)
        {
            var s = Roster.Get(id);
            if (s.IsLocal) return;
            s.Ready = ready;
            s.InWorld = inWorld;
            s.Offline = offline;
        }

        /// <summary>
        /// 客机重连成功（恢复原槽位）后立刻补发本地状态：
        /// 准备状态 / 死亡加时 / 积分，让主机那边的名单马上回到断线前的样子。
        /// </summary>
        internal static void OnReconnected()
        {
            var nm = NetworkManager.Instance;
            if (nm == null) return;

            // 让 Tick 里"inWorld 状态变化补发 ready"的自动逻辑重新走一遍
            _reportedInWorld = false;
            Roster.SyncSelf();
            if (Roster.Local != null)
            {
                Roster.Local.Ready = LocalReady;
                Roster.Local.InWorld = LocalInGame;
            }

            if (LocalReady)
                nm.SendRaceReady(LocalReady, LocalInGame);

            if (LocalDeaths > 0 || LocalPenaltySeconds > 0f)
                nm.SendRacePenalty(nm.LocalPlayerId, LocalDeaths, LocalPenaltySeconds);

            Score.ForcePushState();

            Plugin.Log.LogInfo("[RL2MP] [RACE] 重连恢复：ready=" + LocalReady + " inWorld=" + LocalInGame
                + " deaths=" + LocalDeaths + " penalty=" + LocalPenaltySeconds.ToString("F0") + "s");
        }

        /// <summary>
        /// 客机把游戏推进 World。走游戏自己的转场，和 SeedSync 自动重进同一套 API：
        /// 进 World 时 GenerateNewSeed/InitializeRNG 的钩子会把种子强制成主机的。
        /// </summary>
        internal static void AutoEnterWorld(string reason)
        {
            try
            {
                var nm = NetworkManager.Instance;
                if (nm == null || nm.IsHost || !nm.Connected) return;

                if (_autoEnterCooldown > 0f) return;

                string cur = SceneManager.GetActiveScene().name;
                if (cur == "World") return;              // 已经在 World（只是玩家还没生成）
                if (SceneLoader_RL.IsLoading) return;     // 游戏自己正在转场

                _autoEnterCooldown = 5f;
                Plugin.Log.LogInfo("[RL2MP] [RACE] 客机不在 World（当前 '" + cur + "'，"
                    + reason + "）-> 自动进图，地图将使用主机种子。");
                SceneLoader_RL.LoadScene(SceneID.World, TransitionID.FadeToBlackWithLoading);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 自动进图失败: " + ex.Message
                    + " —— 请手动从主菜单进入游戏世界。");
            }
        }

        /// <summary>主机裁决：双方都就绪，广播 RaceStart 并本地进入倒计时。</summary>
        private static void StartRace()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return;

            Roster.SyncSelf();
            if (Roster.Count < 2)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 还没有客机连进来，无法开跑。");
                return;
            }
            if (!LocalInGame)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 你还在主菜单/加载中，无法开跑 —— 请先进入游戏世界。");
                return;
            }
            if (!LocalReady)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 你自己还没准备，无法开跑。");
                return;
            }
            if (!Roster.AllOthersReady())
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 还有人没进入游戏/没准备（已就绪 "
                    + Roster.ReadyCount() + "/" + Roster.Count + "），暂不开跑。");
                return;
            }

            Phase = RacePhase.Countdown;
            CountdownEndRealtime = Time.realtimeSinceStartup + CountdownSeconds;
            ResetRaceState();
            Score.BroadcastConfig();

            nm.SendRaceStart(CountdownSeconds);
            Plugin.Log.LogInfo("[RL2MP] [RACE] HOST 广播 RaceStart（" + Roster.Count + " 人参赛），"
                + CountdownSeconds + " 秒倒计时后开跑！");
        }

        internal static void HandleRaceStart(float countdownSeconds)
        {
            Phase = RacePhase.Countdown;
            CountdownEndRealtime = Time.realtimeSinceStartup + Mathf.Max(0.5f, countdownSeconds);
            ResetRaceState();
            Plugin.Log.LogInfo("[RL2MP] [RACE] 收到 RaceStart（" + Roster.Count + " 人参赛），"
                + countdownSeconds.ToString("F1") + " 秒倒计时后开跑！");
        }

        /// <summary>本地杀掉了目标 Boss。</summary>
        internal static void OnLocalBossDefeated(int bossId)
        {
            if (Phase != RacePhase.Racing) return;
            if (bossId != TargetBossId) return; // 杀的不是终点 Boss

            // 终点 Boss 本身也计入 Boss 击杀分（否则打法鼓励跳过 Boss 刷房）
            Score.OnBossDefeated(bossId);
            FinishLocal("终点 Boss 倒下");
        }

        /// <summary>
        /// v1.7.2：本地完成竞速的统一出口。
        /// 顺序很重要：先把本局金币折算成分（"最后把金币换成分"），再读总分上报，
        /// 否则金币分会漏在赛后结算之外。
        /// </summary>
        internal static void FinishLocal(string reason)
        {
            Roster.SyncSelf();
            var me = Roster.Local;
            if (me == null) return;
            if (me.Finished) return;  // 已经上报过了

            Score.ApplyGoldConversion(reason);

            float raw = Time.realtimeSinceStartup - StartRealtime;
            me.FinishTime = raw + LocalPenaltySeconds;
            me.Score = Score.LocalScore;
            LocalFinishTime = me.FinishTime;

            Plugin.Log.LogInfo("[RL2MP] [RACE] 本地完成竞速（" + reason + "）"
                + " 净用时 " + FormatTime(raw)
                + " + 死亡加时 " + FormatTime(LocalPenaltySeconds)
                + "（" + LocalDeaths + " 次） = " + FormatTime(me.FinishTime)
                + " | 得分 " + me.Score
                + "（含金币 " + Score.LocalGoldConverted + " -> +" + Score.LocalCoinPoints + "）"
                + " | 完赛 " + FinishedCount() + "/" + Roster.Count);

            var nm = NetworkManager.Instance;
            if (nm == null) return;

            if (nm.IsHost)
            {
                nm.SendRacePlayerFinish(me.Id, me.FinishTime, me.Score);
                CheckArbitrate();
            }
            else
            {
                nm.SendRaceFinish(me.FinishTime, me.Score);
            }
        }

        /// <summary>调试按钮：不等 Boss，直接把本地判为完赛（验证结算动画 / 胜负流程）。</summary>
        internal static void DebugForceLocalFinish()
        {
            if (Phase != RacePhase.Racing)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 调试完赛失败：当前不在比赛中（Phase=" + Phase + "）。");
                return;
            }
            Plugin.Log.LogInfo("[RL2MP] [RACE] 调试按钮：本地直接判定完赛。");
            FinishLocal("调试按钮");
        }

        // ---------------- 多人结算 ----------------

        /// <summary>HOST：收到某客机的完成上报。</summary>
        internal static void HandleClientFinish(int id, float elapsed, int score)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return;

            ApplyFinish(id, elapsed, score);
            // 转发给其他客机（他们也要看到谁完成了）
            nm.SendRacePlayerFinish(id, elapsed, score);
            CheckArbitrate();
        }

        /// <summary>收到"某人完成"的转发（客机侧）。</summary>
        internal static void HandleRemoteFinish(int id, float elapsed, int score)
        {
            ApplyFinish(id, elapsed, score);
            var nm = NetworkManager.Instance;
            if (nm != null && nm.IsHost) CheckArbitrate();
        }

        private static void ApplyFinish(int id, float elapsed, int score)
        {
            var s = Roster.Get(id);
            s.FinishTime = elapsed;
            s.Score = score;
            s.Dnf = false;

            Plugin.Log.LogInfo("[RL2MP] [RACE] " + s.Tag + " 完成竞速：用时 " + FormatTime(elapsed)
                + " 得分 " + score + "（完赛 " + FinishedCount() + "/" + Roster.Count + "）");
        }

        private static int FinishedCount()
        {
            int n = 0;
            foreach (var s in Roster.All) if (s.Finished) n++;
            return n;
        }

        /// <summary>HOST：检查能不能结算（全部完成，或首位完成后宽限期到点）。</summary>
        private static void CheckArbitrate()
        {
            if (WinnerId >= 0) return;
            if (Phase != RacePhase.Racing) return;

            if (FinishedCount() >= Roster.Count)
            {
                Arbitrate();
                return;
            }

            if (_graceEndRealtime >= 0f) return;

            _graceEndRealtime = Time.realtimeSinceStartup + Mathf.Max(0f, FinishGraceSeconds);
            Plugin.Log.LogInfo("[RL2MP] [RACE] 已有玩家到达终点，其余玩家还有 "
                + (int)FinishGraceSeconds + " 秒跑完（超时按未完成结算）。");

            // 客机也要显示这个倒计时
            var nmGrace = NetworkManager.Instance;
            if (nmGrace != null && nmGrace.Connected) nmGrace.SendRaceGrace(FinishGraceSeconds);

            if (FinishGraceSeconds <= 0f) MarkDnfAndArbitrate();
        }

        /// <summary>每帧：宽限期到点就结算。</summary>
        private static void TickArbitration()
        {
            if (Phase != RacePhase.Racing || WinnerId >= 0) return;
            if (_graceEndRealtime < 0f) return;
            if (Time.realtimeSinceStartup < _graceEndRealtime) return;

            MarkDnfAndArbitrate();
        }

        /// <summary>客机收到"宽限期开始"的通知。</summary>
        internal static void HandleGrace(float seconds)
        {
            _graceEndRealtime = Time.realtimeSinceStartup + Mathf.Max(0f, seconds);
            Plugin.Log.LogInfo("[RL2MP] [RACE] 其余玩家还有 " + (int)seconds + " 秒完赛（超时算未完成）。");
        }

        /// <summary>首位完成后剩余等待秒数（-1 = 还没开始等待）。</summary>
        public static float GraceRemaining
        {
            get
            {
                if (_graceEndRealtime < 0f) return -1f;
                float rem = _graceEndRealtime - Time.realtimeSinceStartup;
                return rem > 0f ? rem : 0f;
            }
        }

        /// <summary>已完赛人数（HUD 用）。</summary>
        public static int FinishedPlayers { get { return FinishedCount(); } }

        private static void MarkDnfAndArbitrate()
        {
            int dnf = 0;
            foreach (var s in Roster.All)
            {
                if (!s.Finished)
                {
                    s.Dnf = true;
                    dnf++;
                }
            }
            Plugin.Log.LogInfo("[RL2MP] [RACE] 等待超时：" + dnf + " 人未完成，按未完成结算。");
            Arbitrate();
        }

        /// <summary>HOST：按当前模式排名并广播结果。</summary>
        internal static void Arbitrate()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return;
            if (WinnerId >= 0) return;
            if (Roster.Count == 0) return;

            bool scoreMode = Score.Mode == RaceGameMode.Score;
            var ranked = Roster.Ranked(scoreMode);
            if (ranked.Count == 0) return;

            WinnerId = ranked[0].Id;
            Phase = RacePhase.Finished;
            ResultText = BuildResultText(ranked, scoreMode);

            Plugin.Log.LogInfo("[RL2MP] [RACE] HOST 结算（玩法=" + Score.StyleName(Score.Style)
                + " 判定=" + Score.ModeName(Score.Mode) + "）："
                + DescribeRanking(ranked));
            nm.SendRaceResult(ranked[0].Id, scoreMode, ranked);
        }

        internal static void HandleRaceResult(int winnerId, bool scoreMode, int[] ids,
            float[] times, int[] scores, bool[] dnf)
        {
            if (ids == null) return;

            for (int i = 0; i < ids.Length; i++)
            {
                var s = Roster.Get(ids[i]);
                s.FinishTime = times[i];
                s.Score = scores[i];
                s.Dnf = dnf[i];
            }

            WinnerId = winnerId;
            Phase = RacePhase.Finished;
            ResultText = BuildResultText(Roster.Ranked(scoreMode), scoreMode);
            Plugin.Log.LogInfo("[RL2MP] [RACE] 收到结果：" + ResultText);
        }

        private static string DescribeRanking(List<PlayerSlot> ranked)
        {
            var sb = new System.Text.StringBuilder();
            for (int i = 0; i < ranked.Count; i++)
            {
                if (i > 0) sb.Append(" > ");
                sb.Append(ranked[i].Tag);
                sb.Append(ranked[i].Dnf ? "(未完成)" : ("(" + FormatTime(ranked[i].FinishTime) + "/" + ranked[i].Score + ")"));
            }
            return sb.ToString();
        }

        private static string BuildResultText(List<PlayerSlot> ranked, bool scoreMode)
        {
            var me = Roster.Local;
            PlayerSlot win = ranked.Count > 0 ? ranked[0] : null;

            int myRank = 0;
            if (me != null)
            {
                for (int i = 0; i < ranked.Count; i++)
                {
                    if (ranked[i].Id == me.Id) { myRank = i + 1; break; }
                }
            }

            string head = (win != null && me != null && win.Id == me.Id)
                ? "你赢了！"
                : ((win != null ? win.Tag : "?") + " 赢了！");

            string detail = "";
            if (win != null)
            {
                detail = scoreMode
                    ? ("得分 " + win.Score + " / 用时 " + FormatTime(win.FinishTime))
                    : ("用时 " + FormatTime(win.FinishTime) + " / 得分 " + win.Score);
            }

            string mine = "";
            if (me != null && myRank > 1)
            {
                mine = "  你第 " + myRank + "/" + ranked.Count + " 名";
            }

            return head + " " + detail + mine;
        }

        // ---------------- 死亡加时 ----------------

        /// <summary>
        /// 本地玩家在竞速中死亡：立刻原地复活（由 DownedState.QuickRespawn 完成），
        /// 这里只负责记账 —— 每次 +PenaltyPerDeath 秒，直接加进最终成绩。
        /// </summary>
        internal static void OnLocalDeath()
        {
            if (Phase != RacePhase.Racing) return;

            LocalDeaths++;
            LocalPenaltySeconds += PenaltyPerDeath;
            Score.OnDeath();

            Roster.SyncSelf();
            var me = Roster.Local;
            if (me != null)
            {
                me.Deaths = LocalDeaths;
                me.PenaltySeconds = LocalPenaltySeconds;
            }

            Plugin.Log.LogInfo("[RL2MP] [RACE] 本地死亡 #" + LocalDeaths
                + " -> 成绩 +" + (int)PenaltyPerDeath + " 秒（累计加时 "
                + FormatTime(LocalPenaltySeconds) + "）· 积分 " + Score.LocalScore);

            var nm = NetworkManager.Instance;
            if (nm != null && nm.Connected) nm.SendRacePenalty(nm.LocalPlayerId, LocalDeaths, LocalPenaltySeconds);
        }

        /// <summary>HOST：设定每次死亡的加时秒数，并广播给客机。</summary>
        public static void HostSetPenalty(float seconds)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] HostSetPenalty: 只有主机能设定。");
                return;
            }
            PenaltyPerDeath = Mathf.Clamp(seconds, 0f, 600f);
            Plugin.Log.LogInfo("[RL2MP] [RACE] HOST 死亡加时设为 " + (int)PenaltyPerDeath + " 秒/次 -> 广播。");
            if (nm.Connected) nm.SendRacePenaltyConfig(PenaltyPerDeath);
        }

        internal static void HandleRacePenaltyConfig(float seconds)
        {
            PenaltyPerDeath = Mathf.Clamp(seconds, 0f, 600f);
            Plugin.Log.LogInfo("[RL2MP] [RACE] 收到死亡加时设置: " + (int)PenaltyPerDeath + " 秒/次。");
        }

        internal static void HandleRacePenalty(int id, int deaths, float accumSeconds)
        {
            var s = Roster.Get(id);
            s.Deaths = deaths;
            s.PenaltySeconds = accumSeconds;
            Plugin.Log.LogInfo("[RL2MP] [RACE] " + s.Tag + " 死亡 " + deaths + " 次，累计加时 "
                + FormatTime(accumSeconds) + "。");
        }

        /// <summary>主机面板"结束对局"：广播 RaceEnd -> 双方断开连接。</summary>
        public static void HostEndRace()
        {
            var nm = NetworkManager.Instance;
            if (nm == null) return;
            if (!nm.IsHost)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] 只有主机能结束对局。");
                return;
            }
            Plugin.Log.LogInfo("[RL2MP] [RACE] HOST 结束对局 -> 广播 RaceEnd -> 双方断开。");
            nm.SendRaceEnd();
            Reset("host ended race");
            nm.Stop();
        }

        internal static void HandleRaceEnd()
        {
            Plugin.Log.LogInfo("[RL2MP] [RACE] 主机结束了对局 -> 断开连接。");
            Reset("race ended by host");
            RaceRulesSync.Restore("race ended");
            var nm = NetworkManager.Instance;
            if (nm != null) nm.Stop();
        }

        internal static void Reset(string reason)
        {
            Plugin.Log.LogInfo("[RL2MP] [RACE] Reset (" + reason + ")");
            Phase = RacePhase.Idle;
            _autoEnterCooldown = 0f;
            ResetRaceState();
            // TargetBossId 保留：方便下一局继续用同一个终点
        }

        // ---------------- 每帧驱动（ControlPanel.Update 调用） ----------------

        internal static void Tick()
        {
            if (_autoEnterCooldown > 0f)
            {
                _autoEnterCooldown -= Time.unscaledDeltaTime;
            }

            // 兜底：准备了但迟迟进不去游戏（自动进图失败/卡加载），给明确提示
            if (Phase == RacePhase.WaitingReady && LocalReady && !LocalInGame)
            {
                if (_notInGameSince < 0f)
                {
                    _notInGameSince = Time.realtimeSinceStartup;
                }
                else if (!_autoEnterWarned && Time.realtimeSinceStartup - _notInGameSince > 12f)
                {
                    _autoEnterWarned = true;
                    Plugin.Log.LogWarning("[RL2MP] [RACE] 已准备 12 秒但仍不在游戏世界里 —— "
                        + "自动进图可能失败，请手动从主菜单进入游戏后再取消/重新准备。");
                }
            }
            else
            {
                _notInGameSince = -1f;
                _autoEnterWarned = false;
            }

            // 客机：准备之后才进到游戏里（自动进图/手动进图/加载完成），
            // 这里检测到状态变化就补发一次 ready(inWorld=true)，
            // 主机收到才会开始倒计时——否则会出现"主机开跑了、客机还在菜单"。
            if (LocalReady && Phase == RacePhase.WaitingReady)
            {
                var nm = NetworkManager.Instance;
                if (nm != null && !nm.IsHost && nm.Connected)
                {
                    bool inGame = LocalInGame;
                    if (inGame != _reportedInWorld)
                    {
                        _reportedInWorld = inGame;
                        Roster.SyncSelf();
                        if (Roster.Local != null) Roster.Local.InWorld = inGame;
                        nm.SendRaceReady(true, inGame);
                        Plugin.Log.LogInfo("[RL2MP] [RACE] 本地游戏状态变化 -> 补发 ready(inWorld="
                            + inGame + ")");
                    }
                }
            }

            if (Phase == RacePhase.Countdown && Time.realtimeSinceStartup >= CountdownEndRealtime)
            {
                Phase = RacePhase.Racing;
                StartRealtime = Time.realtimeSinceStartup;
                Plugin.Log.LogInfo("[RL2MP] [RACE] 开跑！计时开始。");
            }

            if (Phase == RacePhase.Racing)
            {
                TickArbitration();
            }
        }

        // ---------------- Boss 识别 ----------------

        /// <summary>PlayerSaveFlag（Defeated 类）-> BossID。</summary>
        internal static int SaveFlagToBossId(PlayerSaveFlag flag)
        {
            switch (flag)
            {
                case PlayerSaveFlag.CastleBoss_Defeated: return (int)BossID.Castle_Boss;
                case PlayerSaveFlag.BridgeBoss_Defeated: return (int)BossID.Bridge_Boss;
                case PlayerSaveFlag.ForestBoss_Defeated: return (int)BossID.Forest_Boss;
                case PlayerSaveFlag.StudyBoss_Defeated: return (int)BossID.Study_Boss;
                case PlayerSaveFlag.TowerBoss_Defeated: return (int)BossID.Tower_Boss;
                case PlayerSaveFlag.CaveBoss_Defeated: return (int)BossID.Cave_Boss;
                case PlayerSaveFlag.GardenBoss_Defeated: return (int)BossID.Garden_Boss;
                case PlayerSaveFlag.FinalBoss_Defeated: return (int)BossID.Final_Boss;
                default: return 0;
            }
        }

        public static readonly int[] BossChoices = new int[]
        {
            (int)BossID.Castle_Boss,
            (int)BossID.Bridge_Boss,
            (int)BossID.Forest_Boss,
            (int)BossID.Study_Boss,
            (int)BossID.Tower_Boss,
            (int)BossID.Cave_Boss,
            (int)BossID.Garden_Boss,
            (int)BossID.Final_Boss,
        };

        public static string BossName(int bossId)
        {
            switch ((BossID)bossId)
            {
                case BossID.Castle_Boss: return "拉麦";
                case BossID.Bridge_Boss: return "虚空兽";
                case BossID.Forest_Boss: return "拿玛";
                case BossID.Study_Boss: return "以诺";
                case BossID.Tower_Boss: return "以拿";
                case BossID.Cave_Boss: return "土八";
                case BossID.Garden_Boss: return "约拿";
                case BossID.Final_Boss: return "该隐";
                default: return "未知(" + bossId + ")";
            }
        }

        /// <summary>区域名（用于和 Boss 名一起显示，避免同名 Boss 分不清）。</summary>
        public static string BossBiome(int bossId)
        {
            switch ((BossID)bossId)
            {
                case BossID.Castle_Boss: return "城堡";
                case BossID.Bridge_Boss: return "大桥";
                case BossID.Forest_Boss: return "森林";
                case BossID.Study_Boss: return "书房";
                case BossID.Tower_Boss: return "高塔";
                case BossID.Cave_Boss: return "洞穴";
                case BossID.Garden_Boss: return "花园";
                case BossID.Final_Boss: return "最终";
                default: return "?";
            }
        }

        /// <summary>界面用标签：重名时带上区域（例如 嗣骨以拿(城堡) / 嗣骨以拿(高塔)）。</summary>
        public static string BossLabel(int bossId)
        {
            string n = BossName(bossId);
            int dup = 0;
            for (int i = 0; i < BossChoices.Length; i++)
            {
                if (BossName(BossChoices[i]) == n) dup++;
            }
            return dup > 1 ? n + "(" + BossBiome(bossId) + ")" : n;
        }

        public static string FormatTime(float seconds)
        {
            if (seconds < 0f) return "--:--";
            TimeSpan t = TimeSpan.FromSeconds(seconds);
            return string.Format("{0:00}:{1:00}.{2:00}", t.Minutes, t.Seconds, t.Milliseconds / 10);
        }
    }

    // ---------------- Harmony 补丁：Boss 被杀 ----------------

    internal static class RacePatches
    {
        internal static void Apply(Harmony harmony)
        {
            try
            {
                var m = AccessTools.Method(typeof(BossRoomController), "SetBossFlagDefeated");
                if (m == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [RACE] BossRoomController.SetBossFlagDefeated not found!");
                    return;
                }

                var postfix = new HarmonyMethod(typeof(RacePatches).GetMethod("SetBossFlagDefeatedPostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(m, postfix: postfix);
                Plugin.Log.LogInfo("[RL2MP] [RACE] Patched BossRoomController.SetBossFlagDefeated postfix OK.");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] SetBossFlagDefeated patch failed: " + Plugin.PatchError(ex));
            }
        }

        private static void SetBossFlagDefeatedPostfix(BossRoomController __instance)
        {
            try
            {
                var flagField = AccessTools.Field(typeof(BossRoomController), "m_bossSaveFlag");
                if (flagField == null) return;
                var flag = (PlayerSaveFlag)flagField.GetValue(__instance);
                int bossId = Race.SaveFlagToBossId(flag);
                if (bossId == 0)
                {
                    Plugin.Log.LogInfo("[RL2MP] [RACE] Boss 死亡但无法识别（flag=" + flag + "），忽略。");
                    return;
                }

                Plugin.Log.LogInfo("[RL2MP] [RACE] Boss 击杀: " + Race.BossName(bossId)
                    + " (racePhase=" + Race.Phase + ", target=" + Race.TargetBossId + ")");

                // 非终点 Boss：纯加分（终点 Boss 的加分在 OnLocalBossDefeated 里做，顺序无关）
                if (Race.Phase == RacePhase.Racing && bossId != Race.TargetBossId)
                    Score.OnBossDefeated(bossId);

                Race.OnLocalBossDefeated(bossId);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] SetBossFlagDefeatedPostfix failed: " + ex.Message);
            }
        }
    }

    // ---------------- 规则包同步：周期 + 累赘 ----------------

    /// <summary>
    /// 主客机存档进度不同的解决方案（规则包模式，不动磁盘存档）：
    ///
    /// - 主机广播「规则包」：自己的 NewGamePlusLevel + 全部累赘（Burden）等级
    /// - 客机收到后**在内存里覆写**自己 SaveManager.PlayerSaveData 的这两块数据
    ///   → 世界生成、敌人强度、高级 Boss 生成全部与主机一致
    /// - 磁盘保护：游戏保存 Player 数据时会序列化这个活对象（还是异步的），
    ///   所以 patch 了 SaveManager.CreateSaveFile（序列化那一刻），
    ///   写盘瞬间把真实值换回来、写完再套回主机值 → 客机存档零污染
    /// - 会话结束/掉线 → 内存恢复客机自己的值
    ///
    /// 技能树/装备/金币是"练度"，保留各自的，不做同步。
    /// </summary>
    internal static class RaceRulesSync
    {
        internal static bool Applied;

        private static int _hostNgPlus;
        private static int _hostHighestBeaten;
        private static readonly Dictionary<BurdenType, int> _hostBurdenLevels = new Dictionary<BurdenType, int>();

        // 客机自己的原值（应用前备份，恢复时写回）
        private static int _origNgPlus;
        private static int _origHighestBeaten;
        private static readonly Dictionary<BurdenType, int> _origBurdenLevels = new Dictionary<BurdenType, int>();

        private static bool _loggedOnce;

        /// <summary>
        /// 读「真实」累赘等级：**绕过 BurdenManager.GetBurdenLevel 的解锁过滤**。
        ///
        /// v1.0.1 修复：旧代码用 GetBurdenLevel()，而它内部有一句
        ///     if (... && IsBurdenUnlocked(burdenType)) return burden.CurrentLevel;  return 0;
        /// IsBurdenUnlocked 对 CastleBossUp/BridgeBossUp/BiomeUp… 这一整批累赘是看
        /// PlayerSaveData.HighestNGPlusBeaten 的。主机 NG+ 高、解锁齐 → 能读到 20+ 项；
        /// 客机 HighestNGPlusBeaten 低 → 这些累赘一律读成 0，于是客机只剩 12 个。
        /// 所以这里直接读 BurdenObj.CurrentLevel（只受 MaxLevel 影响）。
        /// </summary>
        private static int ReadBurdenLevelRaw(BurdenType t)
        {
            try
            {
                var obj = BurdenManager.GetBurden(t);
                if (obj == null) return 0;
                return obj.CurrentLevel;
            }
            catch { return 0; }
        }

        /// <summary>HOST：抓自己的周期 + 最高通关周期 + 全部累赘等级，打包广播。</summary>
        internal static void CaptureAndBroadcast()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return;

            try
            {
                int ngPlus = SaveManager.PlayerSaveData.NewGamePlusLevel;
                int highestBeaten = SaveManager.PlayerSaveData.HighestNGPlusBeaten;

                var burdens = new Dictionary<BurdenType, int>();
                int sum = 0;
                foreach (BurdenType t in Enum.GetValues(typeof(BurdenType)))
                {
                    if (t == BurdenType.None) continue;
                    int lv = ReadBurdenLevelRaw(t);
                    if (lv > 0)
                    {
                        burdens[t] = lv;
                        sum += lv;
                    }
                }

                Plugin.Log.LogInfo("[RL2MP] [RULES] HOST 规则包: NG+=" + ngPlus
                    + " HighestNG+Beaten=" + highestBeaten
                    + " 累赘=" + burdens.Count + " 项 / 总等级 " + sum + " -> 广播。");
                nm.SendRaceRules(ngPlus, highestBeaten, burdens);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] CaptureAndBroadcast failed: " + ex.Message);
            }
        }

        /// <summary>CLIENT：应用主机的规则包（内存覆写）。</summary>
        internal static void ApplyHostRules(int ngPlus, int highestBeaten, Dictionary<BurdenType, int> burdens)
        {
            ApplyLocal(ngPlus, highestBeaten, burdens);
        }

        /// <summary>
        /// 本地应用一套规则（主机自己 / 客机收到配置包都走这里）。
        /// 沙盒模式也复用这个方法：周目 + 统一档位累赘就是这么写进内存的。
        /// </summary>
        internal static void ApplyLocal(int ngPlus, int highestBeaten, Dictionary<BurdenType, int> burdens)
        {
            try
            {
                if (!Applied)
                {
                    // 首次应用：备份客机自己的值（全部累赘类型都备份，含 0 级）
                    _origNgPlus = SaveManager.PlayerSaveData.NewGamePlusLevel;
                    _origHighestBeaten = SaveManager.PlayerSaveData.HighestNGPlusBeaten;
                    _origBurdenLevels.Clear();
                    foreach (BurdenType t in Enum.GetValues(typeof(BurdenType)))
                    {
                        if (t == BurdenType.None) continue;
                        _origBurdenLevels[t] = ReadBurdenLevelRaw(t);
                    }
                    Applied = true;
                    Plugin.Log.LogInfo("[RL2MP] [RULES] 首次应用规则包（原值已备份: NG+=" + _origNgPlus
                        + " HighestNG+Beaten=" + _origHighestBeaten
                        + " 累赘总等级 " + SumLevels(_origBurdenLevels) + "）");
                }

                _hostNgPlus = ngPlus;
                _hostHighestBeaten = highestBeaten;
                _hostBurdenLevels.Clear();
                foreach (var kv in burdens) _hostBurdenLevels[kv.Key] = kv.Value;

                WriteValues(ngPlus, highestBeaten, burdens);

                if (!_loggedOnce)
                {
                    _loggedOnce = true;
                    // 用游戏自己的 GetBurdenLevel（带解锁过滤）复核一遍：
                    // 如果这里打出来的项数/总等级和主机一致，说明解锁门槛也跟上了。
                    int effCount = 0, effSum = 0;
                    foreach (BurdenType t in Enum.GetValues(typeof(BurdenType)))
                    {
                        if (t == BurdenType.None) continue;
                        int lv = BurdenManager.GetBurdenLevel(t);
                        if (lv > 0) { effCount++; effSum += lv; }
                    }
                    Plugin.Log.LogInfo("[RL2MP] [RULES] 生效复核: GetBurdenLevel 统计 -> "
                        + effCount + " 项 / 总等级 " + effSum
                        + "（与主机不一致就是解锁门槛没同步好）");
                    Plugin.Log.LogInfo("[RL2MP] [RULES] 已套用主机规则: NG+=" + ngPlus
                        + " HighestNG+Beaten=" + highestBeaten
                        + " 累赘 " + burdens.Count + " 项 / 总等级 " + SumLevels(burdens)
                        + "（技能树/装备保留客机自己的；写盘时会自动换回真值）");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] ApplyHostRules failed: " + ex.Message);
            }
        }

        private static int SumLevels(Dictionary<BurdenType, int> dict)
        {
            int s = 0;
            foreach (var kv in dict) s += kv.Value;
            return s;
        }

        /// <summary>把主机值写进内存对象。</summary>
        private static void WriteValues(int ngPlus, int highestBeaten, Dictionary<BurdenType, int> burdens)
        {
            var psd = SaveManager.PlayerSaveData;
            psd.NewGamePlusLevel = ngPlus;
            // 必须先解锁门槛再写等级：BurdenManager.GetBurdenStatGain / GetBurdenWeight
            // 也会检查 IsBurdenUnlocked，门槛不对的话累赘"装上了却不生效"。
            psd.HighestNGPlusBeaten = highestBeaten;

            foreach (BurdenType t in Enum.GetValues(typeof(BurdenType)))
            {
                if (t == BurdenType.None) continue;

                int want;
                if (!burdens.TryGetValue(t, out want)) want = 0; // 主机没有的累赘，客机也关掉

                try
                {
                    var obj = BurdenManager.GetBurden(t);
                    // ForceLevel 不做 clamp 也不查解锁，写的就是真实等级
                    if (obj != null) obj.ForceLevel(want);
                }
                catch { }
            }
        }

        /// <summary>会话结束/掉线：恢复客机自己的内存值。</summary>
        internal static void Restore(string reason)
        {
            if (!Applied) return;
            Applied = false;
            try
            {
                var psd = SaveManager.PlayerSaveData;
                psd.NewGamePlusLevel = _origNgPlus;
                psd.HighestNGPlusBeaten = _origHighestBeaten;
                foreach (var kv in _origBurdenLevels)
                {
                    try
                    {
                        var obj = BurdenManager.GetBurden(kv.Key);
                        if (obj != null) obj.ForceLevel(kv.Value);
                    }
                    catch { }
                }
                Plugin.Log.LogInfo("[RL2MP] [RULES] 已恢复客机自身规则 (" + reason + "): NG+=" + _origNgPlus
                    + " HighestNG+Beaten=" + _origHighestBeaten
                    + " 累赘总等级 " + SumLevels(_origBurdenLevels) + "。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] Restore failed: " + ex.Message);
            }
        }

        // ---------------- 磁盘存档保护补丁 ----------------

        private static void RestoreForSave()
        {
            var psd = SaveManager.PlayerSaveData;
            psd.NewGamePlusLevel = _origNgPlus;
            psd.HighestNGPlusBeaten = _origHighestBeaten;
            foreach (var kv in _origBurdenLevels)
            {
                try
                {
                    var obj = BurdenManager.GetBurden(kv.Key);
                    if (obj != null) obj.ForceLevel(kv.Value);
                }
                catch { }
            }
        }

        private static void ReapplyAfterSave()
        {
            var psd = SaveManager.PlayerSaveData;
            psd.NewGamePlusLevel = _hostNgPlus;
            psd.HighestNGPlusBeaten = _hostHighestBeaten;
            foreach (BurdenType t in Enum.GetValues(typeof(BurdenType)))
            {
                if (t == BurdenType.None) continue;
                int lv;
                if (!_hostBurdenLevels.TryGetValue(t, out lv)) lv = 0;
                try
                {
                    var obj = BurdenManager.GetBurden(t);
                    if (obj != null) obj.ForceLevel(lv);
                }
                catch { }
            }
        }

        /// <summary>
        /// patch SaveManager.CreateSaveFile（私有静态，序列化落盘的那一刻）。
        /// 写盘瞬间把客机真值换回来，序列化完成后重新套上主机值。
        /// 不这样做的话：SaveFileAsync 把 PlayerSaveData 活引用丢给后台 Task，
        /// 异步序列化读到的会是主机值 → 客机磁盘存档被污染。
        /// </summary>
        internal static void ApplySaveProtection(Harmony harmony)
        {
            try
            {
                var m = AccessTools.Method(typeof(SaveManager), "CreateSaveFile");
                if (m == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [RULES] SaveManager.CreateSaveFile not found; save protection DISABLED (客机存档可能被污染!).");
                    return;
                }

                var prefix = new HarmonyMethod(typeof(RaceRulesSync).GetMethod("CreateSaveFilePrefix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                var postfix = new HarmonyMethod(typeof(RaceRulesSync).GetMethod("CreateSaveFilePostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(m, prefix: prefix, postfix: postfix);
                Plugin.Log.LogInfo("[RL2MP] [RULES] Patched SaveManager.CreateSaveFile (存档保护 OK).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] save protection patch failed: " + Plugin.PatchError(ex));
            }

            // 读档侧：LoadGameDataAndUpdate(Player) 会把 SaveManager.PlayerSaveData
            // **整个换成磁盘里读出来的新对象**，我们在内存里覆写的 NG+/累赘/解锁门槛
            // 会一并消失（客机一进世界就又变回自己的 12 个累赘，就是这个原因）。
            try
            {
                var m2 = AccessTools.Method(typeof(SaveManager), "LoadGameDataAndUpdate",
                    new[] { typeof(int), typeof(SaveDataType) });
                if (m2 == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [RULES] SaveManager.LoadGameDataAndUpdate not found; "
                        + "客机读档后规则包可能需要重新下发。");
                    return;
                }
                var p2 = new HarmonyMethod(typeof(RaceRulesSync).GetMethod("LoadPlayerDataPostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(m2, postfix: p2);
                Plugin.Log.LogInfo("[RL2MP] [RULES] Patched SaveManager.LoadGameDataAndUpdate (读档后重套规则 OK).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] LoadGameDataAndUpdate patch failed: " + Plugin.PatchError(ex));
            }

            // 新周目同步：主机在游戏里开启 NG+（Charon 对话确认）时会调
            // PlayerSaveData.ResetForNewGamePlus —— 游戏随即清掉 Boss 标记、
            // StageSaveData.ForceResetWorld=true，进 World 后现场生成全新地图。
            // 我们在 postfix 里广播 NewGame 给客机 + 清"已推送地图"记录，
            // 这样主机一进新世界，MapSync 就会把新地图自动推给客机。
            try
            {
                var m3 = AccessTools.Method(typeof(PlayerSaveData), "ResetForNewGamePlus");
                if (m3 == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [RULES] PlayerSaveData.ResetForNewGamePlus not found; 新周目同步不可用.");
                    return;
                }
                var p3 = new HarmonyMethod(typeof(RaceRulesSync).GetMethod("ResetForNewGamePlusPostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(m3, postfix: p3);
                Plugin.Log.LogInfo("[RL2MP] [RULES] Patched PlayerSaveData.ResetForNewGamePlus (新周目同步 OK).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] ResetForNewGamePlus patch failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>HOST：自己开启新周目（NG+）后广播 + 重新下发规则包 + 通知 MapSync 重推新地图。</summary>
        private static void ResetForNewGamePlusPostfix(PlayerSaveData __instance)
        {
            try
            {
                var nm = NetworkManager.Instance;
                if (nm == null || !nm.IsHost || !nm.Connected) return;
                if (__instance != SaveManager.PlayerSaveData) return;

                int ngPlus = SaveManager.PlayerSaveData.NewGamePlusLevel;
                Plugin.Log.LogInfo("[RL2MP] [RULES] 主机开启了新周目 NG+=" + ngPlus
                    + " -> 通知客机重置竞速状态，新地图将在进入 World 后自动推送。");

                // 1) 客机：清掉自己的 Boss/黑箱标记（内存），与新周目语义一致
                nm.SendNewGame(ngPlus);
                // 2) 规则包重发（NG+ 等级刚变）
                CaptureAndBroadcast();
                // 3) 地图：把"每个客机只推一次"的记录清掉，进 World 后新图自动重推
                MapSync.ClearSentTracking();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] ResetForNewGamePlusPostfix failed: " + ex.Message);
            }
        }

        /// <summary>CLIENT：主机开了新周目 —— 把自己内存里上一周目的 Boss/黑箱标记清掉，
        /// 这样主机推来的新地图上 Boss 房才会正常刷 Boss（否则旧标记会判定"已击败"）。</summary>
        internal static void OnHostNewGame(int hostNgPlus)
        {
            try
            {
                var psd = SaveManager.PlayerSaveData;
                if (psd == null) return;

                PlayerSaveFlag[] bossFlags =
                {
                    PlayerSaveFlag.CastleBoss_Defeated, PlayerSaveFlag.BridgeBoss_Defeated,
                    PlayerSaveFlag.ForestBoss_Defeated, PlayerSaveFlag.StudyBoss_Defeated,
                    PlayerSaveFlag.TowerBoss_Defeated,  PlayerSaveFlag.CaveBoss_Defeated,
                    PlayerSaveFlag.GardenBoss_Defeated, PlayerSaveFlag.FinalBoss_Defeated,
                };
                foreach (var f in bossFlags) psd.SetFlag(f, false);

                PlayerSaveFlag[] chestFlags =
                {
                    PlayerSaveFlag.BlackChest_Weapon_Opened, PlayerSaveFlag.BlackChest_Helm_Opened,
                    PlayerSaveFlag.BlackChest_Chest_Opened,  PlayerSaveFlag.BlackChest_Cape_Opened,
                    PlayerSaveFlag.BlackChest_Trinket_Opened,
                };
                foreach (var f in chestFlags) psd.SetFlag(f, false);

                Plugin.Log.LogInfo("[RL2MP] [RULES] 同步新周目: 已清除本周目的 Boss/黑箱标记"
                    + "（主机的 NG+=" + hostNgPlus + " 规则包稍后自动到达）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] OnHostNewGame failed: " + ex.Message);
            }
        }

        /// <summary>
        /// 客机把 Player 存档从磁盘换成新对象之后，立刻把主机规则重新写上去。
        /// 新对象里的值就是客机真实值（磁盘永远只存真值，见 CreateSaveFile 保护），所以不用重备份。
        /// </summary>
        private static void LoadPlayerDataPostfix(SaveDataType saveType)
        {
            if (!Applied) return;
            if (saveType != SaveDataType.Player) return;
            if (SaveManager.PlayerSaveData == null) return;

            try
            {
                ReapplyAfterSave();
                Plugin.Log.LogInfo("[RL2MP] [RULES] 检测到客机重新读档 -> 已把主机规则重新套上: NG+="
                    + _hostNgPlus + " HighestNG+Beaten=" + _hostHighestBeaten
                    + " 累赘 " + _hostBurdenLevels.Count + " 项。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RULES] 读档后重套规则失败: " + ex.Message);
            }
        }

        private static void CreateSaveFilePrefix(object saveObject)
        {
            if (Applied && saveObject is PlayerSaveData) RestoreForSave();
            // 地图数据同理：客机写盘时不能用主机的地图覆盖自己的存档
            if (saveObject is StageSaveData && MapSync.Applied) MapSync.RestoreForSave();
        }

        private static void CreateSaveFilePostfix(object saveObject)
        {
            if (Applied && saveObject is PlayerSaveData) ReapplyAfterSave();
            if (saveObject is StageSaveData && MapSync.Applied) MapSync.ReapplyAfterSave();
        }
    }

    // ---------------- HUD ----------------

    /// <summary>竞速 HUD：右上角计时器 + 状态。独立于控制面板（面板可藏，HUD 常显）。</summary>
    public class RaceHud : MonoBehaviour
    {
        private static GUIStyle _title;
        private static GUIStyle _big;
        private static GUIStyle _line;
        private static GUIStyle _small;
        private static GUIStyle _scoreHot;
        private static GUIStyle _center;

        // ---------------- v1.7.3：可拖动 / 可缩放的计分板窗口 ----------------
        private const int HudWindowId = 543210;
        private const float BaseW = 340f;
        private const float BaseH = 238f;
        private const float MinW = 220f, MinH = 150f, MaxW = 980f, MaxH = 700f;

        private static Rect _rect = new Rect(-1f, -1f, BaseW, BaseH);
        private static bool _placed;
        private static bool _resizing;
        private static Vector2 _resizeMouse0;
        private static Vector2 _resizeSize0;
        private static GUIStyle _hudBg;
        private static float _saveDelay;
        private static Rect _lastSavedRect;
        private static float _hintUntil;

        private static BepInEx.Configuration.ConfigEntry<float> _cfgX, _cfgY, _cfgW, _cfgH;

        /// <summary>控制面板里的"调整计分板位置"：Idle 状态也把 HUD 画出来方便拖动。</summary>
        internal static bool ForceShow;

        private void OnGUI()
        {
            if (Race.Phase == RacePhase.Idle)
            {
                // 空闲时不画竞速 HUD，但死亡惩罚可能还在（竞速结束后不会）——直接返回
                if (_fx.Count > 0) { _fx.Clear(); _fxSeen.Clear(); }
                if (!ForceShow) return;
            }

            // 新一局开始：上一局的结算动画清掉，否则会立刻给"已完赛"的人重播旧成绩
            if (Race.Phase == RacePhase.WaitingReady || Race.Phase == RacePhase.Countdown)
            {
                if (_fx.Count > 0) { _fx.Clear(); _fxSeen.Clear(); }
            }

            try
            {
                EnsureStyles();
                EnsureLayout();
                GUI.matrix = Matrix4x4.identity;
                GUI.color = Color.white;
                // 比控制面板低一层，避免两个窗口叠在一起时挡住面板操作
                GUI.depth = 10;

                // 右下角缩放要在 Window 之前吃掉事件，否则会被 DragWindow 抢走
                HandleResize();
                _rect = GUI.Window(HudWindowId, _rect, DrawHudWindow, GUIContent.none, _hudBg);
                ClampOnScreen();
                DrawResizeGrip();
                ScheduleSave();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] HUD exception: " + ex.Message);
                enabled = false;
            }
        }

        /// <summary>窗口默认位置/尺寸（可拖动、可缩放，位置记在 BepInEx 配置里）。</summary>
        private static void EnsureLayout()
        {
            if (_placed) return;
            _placed = true;

            try
            {
                var cfg = Plugin.Instance != null ? Plugin.Instance.Config : null;
                if (cfg != null)
                {
                    _cfgX = cfg.Bind<float>("Hud", "ScoreX", -1f, "竞速计分板左上角 X（-1 表示默认靠右）");
                    _cfgY = cfg.Bind<float>("Hud", "ScoreY", 28f, "竞速计分板左上角 Y");
                    _cfgW = cfg.Bind<float>("Hud", "ScoreW", BaseW, "竞速计分板宽度");
                    _cfgH = cfg.Bind<float>("Hud", "ScoreH", BaseH, "竞速计分板高度");
                    _rect = new Rect(_cfgX.Value, _cfgY.Value, _cfgW.Value, _cfgH.Value);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RACE] HUD 配置读取失败: " + ex.Message);
            }

            if (_rect.width < MinW || _rect.height < MinH)
                _rect = new Rect(_rect.x, _rect.y, BaseW, BaseH);

            if (_rect.x < -0.5f) _rect.x = Screen.width - _rect.width - 10f;
            ClampOnScreen();
            _lastSavedRect = _rect;
            _hintUntil = Time.unscaledTime + 10f;
        }

        private static void ClampOnScreen()
        {
            _rect.width = Mathf.Clamp(_rect.width, MinW, Mathf.Min(MaxW, Mathf.Max(MinW, Screen.width - 16f)));
            _rect.height = Mathf.Clamp(_rect.height, MinH, Mathf.Min(MaxH, Mathf.Max(MinH, Screen.height - 16f)));
            _rect.x = Mathf.Clamp(_rect.x, 0f, Mathf.Max(0f, Screen.width - _rect.width));
            _rect.y = Mathf.Clamp(_rect.y, 0f, Mathf.Max(0f, Screen.height - _rect.height));
        }

        private static void HandleResize()
        {
            Event e = Event.current;
            if (e == null) return;
            Rect grip = new Rect(_rect.xMax - 18f, _rect.yMax - 18f, 18f, 18f);

            switch (e.type)
            {
                case EventType.MouseDown:
                    if (e.button == 0 && grip.Contains(e.mousePosition))
                    {
                        _resizing = true;
                        _resizeMouse0 = e.mousePosition;
                        _resizeSize0 = new Vector2(_rect.width, _rect.height);
                        e.Use();
                    }
                    break;
                case EventType.MouseDrag:
                    if (_resizing)
                    {
                        Vector2 d = e.mousePosition - _resizeMouse0;
                        _rect.width = Mathf.Clamp(_resizeSize0.x + d.x, MinW, MaxW);
                        _rect.height = Mathf.Clamp(_resizeSize0.y + d.y, MinH, MaxH);
                        e.Use();
                    }
                    break;
                case EventType.MouseUp:
                    if (_resizing) { _resizing = false; e.Use(); }
                    break;
            }
        }

        private static void DrawResizeGrip()
        {
            EnsureWhite();
            float x = _rect.xMax - 15f, y = _rect.yMax - 15f;
            Color prev = GUI.color;
            GUI.color = new Color(1f, 0.85f, 0.35f, _resizing ? 1f : 0.5f);
            for (int i = 1; i <= 3; i++)
            {
                float len = i * 4f;
                GUI.DrawTexture(new Rect(x + 12f - len, y + 12f, len, 1.5f), _white);
                GUI.DrawTexture(new Rect(x + 12f, y + 12f - len, 1.5f, len), _white);
            }
            GUI.color = prev;
        }

        private static void ScheduleSave()
        {
            if (_cfgX == null) return;
            if (_rect == _lastSavedRect) return;
            _saveDelay += Time.unscaledDeltaTime;
            if (_saveDelay < 0.6f) return;
            _saveDelay = 0f;
            _lastSavedRect = _rect;
            try
            {
                _cfgX.Value = _rect.x;
                _cfgY.Value = _rect.y;
                _cfgW.Value = _rect.width;
                _cfgH.Value = _rect.height;
            }
            catch { /* 配置写入失败不影响显示 */ }
        }

        /// <summary>窗口内容：整体按窗口尺寸等比缩放，布局坐标始终用 BaseW×BaseH。</summary>
        private static void DrawHudWindow(int id)
        {
            float w = _rect.width, h = _rect.height;
            float s = Mathf.Clamp(Mathf.Min(w / BaseW, h / BaseH), 0.45f, 2f);

            EnsureWhite();
            Color prev = GUI.color;
            // 半透明深色底：不挡地图
            GUI.color = new Color(0.02f, 0.04f, 0.07f, 0.72f);
            GUI.DrawTexture(new Rect(0f, 0f, w, h), _white);
            GUI.color = new Color(1f, 0.85f, 0.35f, 0.45f);
            GUI.DrawTexture(new Rect(0f, 0f, w, 1f), _white);
            GUI.DrawTexture(new Rect(0f, h - 1f, w, 1f), _white);
            GUI.DrawTexture(new Rect(0f, 0f, 1f, h), _white);
            GUI.DrawTexture(new Rect(w - 1f, 0f, 1f, h), _white);
            GUI.color = prev;

            GUI.BeginGroup(new Rect(0f, 0f, w, h));
            Matrix4x4 m0 = GUI.matrix;
            GUIUtility.ScaleAroundPivot(new Vector2(s, s), Vector2.zero);
            DrawHudContent();
            GUI.matrix = m0;
            GUI.EndGroup();

            // 顶部一整条都能拖（窗口不像 GUILayout.Window 那样自带标题栏）
            GUI.DragWindow(new Rect(0f, 0f, w, Mathf.Max(18f, 24f * s)));
        }

        private static void DrawHudContent()
        {
            float w = BaseW;
            bool coop = Score.Style == PlayStyle.CoOp;

            GUI.Label(new Rect(8f, 3f, w - 16f, 20f),
                coop ? "双人协作 · 存活时间"
                     : (Score.StyleName(Score.Style) + " · " + Score.ModeName(Score.Mode)
                        + (Race.Phase == RacePhase.Idle ? " · 预览（未开赛）" : " · 终点: " + Race.BossLabel(Race.TargetBossId))),
                _title);

            string timeText;
            if (Race.Phase == RacePhase.Racing || Race.Phase == RacePhase.Finished)
                timeText = Race.FormatTime(Race.Elapsed);
            else
                timeText = "--:--";

            // 协作模式：不比用时/得分、不排名，右上角只留计时器 + 救援状态
            if (coop)
            {
                GUI.Label(new Rect(8f, 22f, w - 16f, 58f), timeText, _big);
                DrawCoOpStatus(w);
                if (Time.unscaledTime < _hintUntil)
                {
                    Color prev = GUI.color;
                    GUI.color = new Color(1f, 0.85f, 0.35f, 0.85f);
                    GUI.Label(new Rect(8f, BaseH - 20f, w - 16f, 16f),
                        "拖动标题移动 · 右下角拖动缩放（位置会记住）", _small);
                    GUI.color = prev;
                }
                return;
            }

            bool scoreMode = Score.Mode == RaceGameMode.Score;
            var ranked = Roster.Ranked(scoreMode);
            const float rowH = 26f;

            GUI.Label(new Rect(8f, 20f, w - 16f, 34f), timeText, _big);

            string status;
            switch (Race.Phase)
            {
                case RacePhase.WaitingReady:
                    status = "准备 " + Roster.ReadyCount() + "/" + Roster.Count
                           + "   你:" + (Race.LocalReady ? "已准备" : "未准备");
                    break;
                case RacePhase.Countdown:
                    float left = Race.CountdownEndRealtime - Time.realtimeSinceStartup;
                    status = "倒计时 " + Mathf.CeilToInt(left) + " 秒 · " + Roster.Count + " 人参赛";
                    break;
                case RacePhase.Racing:
                    status = "完赛 " + Race.FinishedPlayers + "/" + Roster.Count;
                    if (Race.GraceRemaining >= 0f)
                        status += "   · 剩余玩家还有 " + Mathf.CeilToInt(Race.GraceRemaining) + " 秒";
                    break;
                case RacePhase.Finished:
                    status = Race.ResultText;
                    break;
                default:
                    status = "";
                    break;
            }
            GUI.Label(new Rect(8f, 54f, w - 16f, 34f), status, _line);

            float ry = 92f;
            for (int i = 0; i < ranked.Count && i < 4; i++)
            {
                var s = ranked[i];
                Rect row = new Rect(6f, ry, w - 12f, rowH);
                DrawAvatar(row, s);

                string line = (i + 1) + ". " + s.Display + "   " + SlotLine(s, scoreMode);
                GUI.Label(new Rect(row.x + 28f, ry - 2f, w - 46f, 18f), line, s.IsLocal ? _scoreHot : _line);
                ry += rowH;
            }

            // 完赛动画要在所有行之后画，才能盖在列表上面
            for (int i = 0; i < ranked.Count && i < 4; i++)
            {
                var s = ranked[i];
                if (!s.Finished || s.Dnf) continue;
                EnsureTallyFx(s);
                DrawTallyFx(new Rect(6f, 92f + i * rowH, w - 12f, rowH), s);
            }

            GUI.Label(new Rect(8f, ry + 2f, w - 16f, 18f),
                scoreMode
                    ? ("规则: " + Score.RuleLine())
                    : ("加时: 每死 +" + (int)Race.PenaltyPerDeath + " 秒（本次 +"
                        + Race.FormatTime(Race.LocalPenaltySeconds) + "）"),
                _small);

            if (Time.unscaledTime < _hintUntil)
            {
                Color prev = GUI.color;
                GUI.color = new Color(1f, 0.85f, 0.35f, 0.85f);
                GUI.Label(new Rect(8f, BaseH - 20f, w - 16f, 16f),
                    "拖动标题移动 · 右下角拖动缩放（位置会记住）", _small);
                GUI.color = prev;
            }
        }

        /// <summary>协作 HUD 状态行：观战中 / 观战目标。</summary>
        private static void DrawCoOpStatus(float w)
        {
            Color prev = GUI.color;
            float y = 84f;
            float lineH = 22f;

            if (CoOpRescue.LocalDown)
            {
                GUI.color = new Color(1f, 0.72f, 0.42f, 0.95f);
                GUI.Label(new Rect(8f, y, w - 16f, lineH), "你已倒下 · 观战中（[ ] 切换目标）", _line);
                y += lineH;

                GUI.color = new Color(0.78f, 0.9f, 1f, 0.85f);
                string who = DownedState.SpectateTargetId >= 0
                    ? "P" + DownedState.SpectateTargetId
                    : "无（队友全部倒下）";
                GUI.Label(new Rect(8f, y, w - 16f, lineH),
                    "正在观看 " + who + " · 队友存活 "
                    + DownedState.AliveRemoteCount + "/" + DownedState.RemoteCount, _small);
            }
            else
            {
                GUI.color = new Color(0.75f, 0.9f, 1f, 0.8f);
                GUI.Label(new Rect(8f, y, w - 16f, lineH),
                    "倒下后自动进入观战 · [ ] 切换观战目标", _small);
            }

            GUI.color = prev;
        }

        // ---------------- 头像 + 完赛统计动画（v1.7.2） ----------------

        private static Texture2D _white;
        private static GUIStyle _avatar;
        private static GUIStyle _tallyLine;
        private static GUIStyle _tallyTotal;

        private class TallyFx
        {
            public int Id;
            public float Start;
            public string[] Labels;
            public int[] Values;
            public int Total;
        }

        private static readonly Dictionary<int, TallyFx> _fx = new Dictionary<int, TallyFx>();
        private static readonly HashSet<int> _fxSeen = new HashSet<int>();

        private static void EnsureWhite()
        {
            if (_white != null) return;
            _white = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            _white.SetPixel(0, 0, Color.white);
            _white.Apply();
            _white.hideFlags = HideFlags.HideAndDontSave;
        }

        private static Color AvatarColor(int id)
        {
            switch (id)
            {
                case 0: return new Color(1f, 0.78f, 0.28f);      // 主机：金
                case 1: return new Color(0.40f, 0.76f, 1f);      // P2：蓝
                case 2: return new Color(0.55f, 0.95f, 0.55f);   // P3：绿
                default: return new Color(1f, 0.55f, 0.78f);     // P4：粉
            }
        }

        /// <summary>玩家图像（头像方块）：完赛后描金边。</summary>
        private static void DrawAvatar(Rect row, PlayerSlot s)
        {
            EnsureWhite();
            Rect av = new Rect(row.x + 3f, row.y + 2f, 22f, 22f);
            Color c = AvatarColor(s.Id);
            bool done = s.Finished && !s.Dnf;

            Color prev = GUI.color;
            GUI.color = new Color(c.r * 0.45f, c.g * 0.45f, c.b * 0.45f, 0.95f);
            GUI.DrawTexture(av, _white);

            GUI.color = done ? new Color(1f, 0.88f, 0.35f) : new Color(c.r * 0.85f, c.g * 0.85f, c.b * 0.85f, 0.9f);
            GUI.DrawTexture(new Rect(av.x, av.y, av.width, 2f), _white);
            GUI.DrawTexture(new Rect(av.x, av.yMax - 2f, av.width, 2f), _white);
            GUI.DrawTexture(new Rect(av.x, av.y, 2f, av.height), _white);
            GUI.DrawTexture(new Rect(av.xMax - 2f, av.y, 2f, av.height), _white);
            GUI.color = prev;

            GUI.Label(new Rect(av.x, av.y + 3f, av.width, 16f), s.IsLocal ? "你" : s.Tag, _avatar);
        }

        private static string FormatGold(int gold)
        {
            if (gold >= 1000000) return (gold / 1000000f).ToString("F1") + "M";
            if (gold >= 10000) return (gold / 1000f).ToString("F1") + "k";
            return gold.ToString();
        }

        private static string Signed(int v)
        {
            return v >= 0 ? ("+" + v) : v.ToString();
        }

        /// <summary>第一次看到某人完成时，给他建一份结算动画面板数据。</summary>
        private static void EnsureTallyFx(PlayerSlot s)
        {
            if (!_fxSeen.Add(s.Id)) return;

            // 本地玩家直接用本地累加器（网络包 0.5s 一次，刚完赛时明细可能还没刷到槽位里）
            int rooms = s.IsLocal ? Score.LocalRooms : s.Rooms;
            int kills = s.IsLocal ? Score.LocalKills : s.Kills;
            int bosses = s.IsLocal ? Score.LocalBosses : s.Bosses;
            int chests = s.IsLocal ? Score.LocalChests : s.Chests;
            int chestPts = s.IsLocal ? Score.LocalChestPoints : s.ChestPoints;
            int relics = s.IsLocal ? Score.LocalRelics : s.Relics;
            int deaths = s.IsLocal ? Score.LocalDeaths : s.Deaths;
            int coinPts = s.IsLocal ? Score.LocalCoinPoints : s.CoinPoints;
            int gold = s.IsLocal ? Score.LocalGoldConverted : s.GoldCollected;

            var labels = new List<string>();
            var values = new List<int>();

            if (rooms > 0) { labels.Add("探索房间 ×" + rooms); values.Add(rooms * Score.RoomPoints); }
            if (kills > 0) { labels.Add("击杀怪物 ×" + kills); values.Add(kills * Score.EnemyPoints); }
            if (bosses > 0) { labels.Add("击败 Boss ×" + bosses); values.Add(bosses * Score.BossPoints); }
            if (chests > 0) { labels.Add("开箱 ×" + chests); values.Add(chestPts); }
            if (coinPts != 0 || gold > 0)
            {
                labels.Add("金币 " + FormatGold(gold));
                values.Add(coinPts);
            }
            if (relics > 0) { labels.Add("遗物 ×" + relics); values.Add(-(relics * Score.RelicPenalty)); }
            if (deaths > 0) { labels.Add("死亡 ×" + deaths); values.Add(-(deaths * Score.DeathPenalty)); }

            int sum = 0;
            for (int i = 0; i < values.Count; i++) sum += values[i];
            int rest = s.Score - sum;
            if (rest != 0) { labels.Add("时间衰减 / 其他"); values.Add(rest); }

            if (labels.Count == 0) { labels.Add("完赛"); values.Add(s.Score); }

            var fx = new TallyFx();
            fx.Id = s.Id;
            fx.Start = Time.unscaledTime;
            fx.Labels = labels.ToArray();
            fx.Values = values.ToArray();
            fx.Total = s.Score;
            _fx[s.Id] = fx;

            Plugin.Log.LogInfo("[RL2MP] [RACE] 结算动画: " + s.Tag + " 总分 " + s.Score
                + " | " + string.Join(", ", labels.ToArray()));
        }

        /// <summary>
        /// 画结算面板：从该玩家头像位置滑到"图像正前方"（列表左侧），
        /// 然后逐行滚出加成项，总分跟着跳动。
        /// </summary>
        private static void DrawTallyFx(Rect row, PlayerSlot s)
        {
            TallyFx fx;
            if (!_fx.TryGetValue(s.Id, out fx)) return;

            float age = Time.unscaledTime - fx.Start;
            int n = fx.Labels.Length;
            const float revealStart = 0.30f;
            const float perLine = 0.20f;
            const float hold = 4.5f;
            float allShown = revealStart + n * perLine;
            float life = allShown + hold;

            float fade = age > life ? Mathf.Clamp01(1f - (age - life) / 0.7f) : 1f;
            if (fade <= 0f) return;

            float inT = Mathf.Clamp01(age / 0.30f);
            float ease = 1f - (1f - inT) * (1f - inT);

            float panelW = Mathf.Min(248f, BaseW - 12f);
            float panelH = 26f + (n + 1) * 16f + 8f;
            // v1.7.3：面板改在窗口内部滑动。窗口用 BeginGroup 裁剪，
            // 画到窗口外的部分会被切掉，所以终点固定在窗口左内侧。
            float finalX = 6f;
            float startX = row.xMax - 40f;        // 从该玩家的分数位置飞出来
            float px = Mathf.Lerp(startX, finalX, ease);
            float py = Mathf.Clamp(row.y + row.height * 0.5f - panelH * 0.5f,
                2f, Mathf.Max(2f, BaseH - panelH - 2f));

            Color prev = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, fade);
            EnsureWhite();
            Color bg = GUI.color;
            GUI.color = new Color(0.02f, 0.04f, 0.07f, 0.94f * fade);
            GUI.DrawTexture(new Rect(px, py, panelW, panelH), _white);
            GUI.color = new Color(1f, 0.86f, 0.32f, 0.7f * fade);
            GUI.DrawTexture(new Rect(px, py, panelW, 1.5f), _white);
            GUI.DrawTexture(new Rect(px, py + panelH - 1.5f, panelW, 1.5f), _white);
            GUI.DrawTexture(new Rect(px, py, 1.5f, panelH), _white);
            GUI.DrawTexture(new Rect(px + panelW - 1.5f, py, 1.5f, panelH), _white);
            GUI.color = bg;

            GUI.Label(new Rect(px + 8f, py + 3f, panelW - 16f, 16f),
                s.Display + " 完赛 · " + Race.FormatTime(s.FinishTime), _tallyLine);

            int shown = 0;
            int revealed = 0;
            for (int i = 0; i < n; i++)
            {
                float t = (age - revealStart - i * perLine) / 0.14f;
                if (t <= 0f) break;
                float a = Mathf.Clamp01(t);
                revealed++;

                Color lc = fx.Values[i] >= 0
                    ? new Color(0.75f, 1f, 0.78f, a * fade)
                    : new Color(1f, 0.66f, 0.60f, a * fade);
                GUI.color = lc;
                GUI.Label(new Rect(px + 10f, py + 22f + i * 16f, panelW - 20f, 15f),
                    fx.Labels[i], _tallyLine);
                GUI.Label(new Rect(px + panelW - 78f, py + 22f + i * 16f, 68f, 15f),
                    Signed(fx.Values[i]) + " 分", _tallyTotalRight);

                shown += fx.Values[i];
            }

            int live = shown;
            if (revealed >= n)
            {
                float tt = Mathf.Clamp01((age - allShown) / 0.35f);
                live = Mathf.RoundToInt(Mathf.Lerp(shown, fx.Total, tt));
            }

            GUI.color = new Color(1f, 0.86f, 0.32f, fade);
            GUI.Label(new Rect(px + 8f, py + panelH - 24f, 90f, 20f), "总分", _tallyTotal);
            GUI.Label(new Rect(px + 52f, py + panelH - 26f, panelW - 60f, 22f),
                live + " 分", _tallyTotal);
            GUI.color = prev;
        }

        private static GUIStyle _tallyTotalRight;

        private static string SlotLine(PlayerSlot s, bool scoreMode)
        {
            if (s.Finished && !s.Dnf)
            {
                return scoreMode
                    ? (s.Score + " 分 · " + Race.FormatTime(s.FinishTime) + " · 死 " + s.Deaths)
                    : (Race.FormatTime(s.FinishTime) + " · " + s.Score + " 分 · 死 " + s.Deaths);
            }
            if (s.Dnf) return "未完成 · " + s.Score + " 分";
            return "跑图中 · " + s.Score + " 分 · 死 " + s.Deaths;
        }

        private static void EnsureStyles()
        {
            if (_hudBg == null)
            {
                // 空样式：窗口背景由我们自己画（半透明，不挡地图）
                _hudBg = new GUIStyle(GUI.skin.box);
                _hudBg.normal.background = null;
                _hudBg.hover.background = null;
                _hudBg.active.background = null;
                _hudBg.focused.background = null;
                _hudBg.border = new RectOffset(0, 0, 0, 0);
                _hudBg.padding = new RectOffset(0, 0, 0, 0);
                _hudBg.margin = new RectOffset(0, 0, 0, 0);
            }
            if (_title == null)
            {
                _title = new GUIStyle(GUI.skin.label);
                _title.fontSize = 14;
                _title.fontStyle = FontStyle.Bold;
                _title.normal.textColor = new Color(1f, 0.85f, 0.3f);
            }
            if (_big == null)
            {
                _big = new GUIStyle(GUI.skin.label);
                _big.fontSize = 26;
                _big.fontStyle = FontStyle.Bold;
                _big.normal.textColor = Color.white;
                _big.alignment = TextAnchor.MiddleCenter;
            }
            if (_small == null)
            {
                _small = new GUIStyle(GUI.skin.label);
                _small.fontSize = 11;
                _small.normal.textColor = new Color(0.78f, 0.82f, 0.9f);
            }
            if (_scoreHot == null)
            {
                _scoreHot = new GUIStyle(GUI.skin.label);
                _scoreHot.fontSize = 14;
                _scoreHot.fontStyle = FontStyle.Bold;
                _scoreHot.normal.textColor = new Color(1f, 0.85f, 0.3f);
            }
            if (_line == null)
            {
                _line = new GUIStyle(GUI.skin.label);
                _line.fontSize = 12;
                _line.wordWrap = true;
                _line.normal.textColor = Color.white;
            }
            if (_center == null)
            {
                _center = new GUIStyle(GUI.skin.label);
                _center.fontSize = 18;
                _center.fontStyle = FontStyle.Bold;
                _center.normal.textColor = new Color(1f, 0.5f, 0.4f);
                _center.alignment = TextAnchor.MiddleCenter;
            }
            if (_avatar == null)
            {
                _avatar = new GUIStyle(GUI.skin.label);
                _avatar.fontSize = 11;
                _avatar.fontStyle = FontStyle.Bold;
                _avatar.alignment = TextAnchor.MiddleCenter;
                _avatar.normal.textColor = new Color(0.05f, 0.05f, 0.05f);
            }
            if (_tallyLine == null)
            {
                _tallyLine = new GUIStyle(GUI.skin.label);
                _tallyLine.fontSize = 11;
                _tallyLine.normal.textColor = Color.white;
            }
            if (_tallyTotalRight == null)
            {
                _tallyTotalRight = new GUIStyle(GUI.skin.label);
                _tallyTotalRight.fontSize = 11;
                _tallyTotalRight.alignment = TextAnchor.MiddleRight;
                _tallyTotalRight.normal.textColor = Color.white;
            }
            if (_tallyTotal == null)
            {
                _tallyTotal = new GUIStyle(GUI.skin.label);
                _tallyTotal.fontSize = 15;
                _tallyTotal.fontStyle = FontStyle.Bold;
                _tallyTotal.normal.textColor = new Color(1f, 0.86f, 0.32f);
            }
        }
    }
}
