using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>比赛判定方式：比用时（默认，死亡加时计入） / 比得分。</summary>
    public enum RaceGameMode
    {
        Time = 0,
        Score = 1,
    }

    /// <summary>
    /// 玩法维度（v1.7.6）：竞速 = 各跑各的、先到终点者胜；
    /// 协作 = 两人同图、一起过门、倒地救援（玩法行为将在后续版本逐步启用）。
    /// 与判定方式正交：两种玩法都能选比用时 / 比得分。
    /// </summary>
    public enum PlayStyle
    {
        Race = 0,
        CoOp = 1,
    }

    /// <summary>
    /// 得分系统：给竞速加一层"行为计分"，避免"用命换时间"的莽夫打法。
    ///
    /// 计分项（全部主机可调，实时广播给客机）：
    ///   + 每探索一个新房间（按房间去重，来回走不会刷分）
    ///   + 每击杀一个普通怪物（Boss 单独算）
    ///   + 每击败一个 Boss（终点 Boss 也算）
    ///   - 每多拿一个非任务遗物（Lily 系列 / 龙钥匙 / 任务类不算）
    ///   - 每死亡一次
    ///   - 随时间自然衰减（每分钟扣固定分，逼你跑起来）
    ///
    /// 两种比赛模式：
    ///   Time  —— 原有玩法：谁先打死终点 Boss，用时（含死亡加时）短的赢；得分仅作展示
    ///   Score —— 终点 Boss 被打死后结束，直接比总分
    /// </summary>
    internal static class Score
    {
        // ---------------- 规则配置（主机权威） ----------------

        // 玩法（主机权威，与判定方式正交）。v1.7.6 先只做切换/同步/显示，
        // 协作的行为规则在后续版本按 Score.Style == PlayStyle.CoOp 分支逐步启用。
        public static PlayStyle Style = PlayStyle.Race;

        public static RaceGameMode Mode = RaceGameMode.Time;

        public static int RoomPoints = 25;        // 每探索一个新房间
        public static int EnemyPoints = 3;        // 每击杀一个普通怪物
        public static int BossPoints = 300;       // 每击败一个 Boss
        public static int RelicPenalty = 50;      // 每拿一个非任务遗物
        public static int DeathPenalty = 100;     // 每死一次
        public static int DecayPerMinute = 20;    // 每分钟自然衰减

        // 开箱加分（v1.7.2）
        public static int ChestBronzePoints = 40;   // 普通（铜）箱
        public static int ChestSilverPoints = 80;   // 银箱
        public static int ChestGoldPoints = 150;    // 金箱
        public static int ChestFairyPoints = 200;   // 妖精（带锁）箱

        // 金币折算（v1.7.2）：完赛时把本局收集的金币折算成分。
        // 周目越高金币收益越高，所以兑换门槛按周目线性上涨（GoldNgPlusStep / 周目）。
        public static int GoldPerPoint = 10000;     // NG+0：多少金币 = 1 分
        public static float GoldNgPlusStep = 0.35f; // 每多一个周目，所需金币 +35%

        public const int MaxPoints = 100000;

        // ---------------- 本地积分 ----------------

        public static int LocalScore;
        public static int LocalRooms;
        public static int LocalKills;
        public static int LocalBosses;
        public static int LocalRelics;
        public static int LocalDeaths;
        public static int LocalChests;
        public static int LocalChestPoints;
        public static int LocalCoinPoints;
        public static int LocalGoldConverted;
        public static bool LocalGoldApplied;

        // 明细分桶（结算动画面板逐行展示）
        public static int LocalRoomPoints;
        public static int LocalKillPoints;
        public static int LocalBossPoints;
        public static int LocalRelicPoints;
        public static int LocalDeathPoints;
        public static int LocalDecayPoints;

        // ---------------- 内部状态 ----------------

        private static readonly HashSet<string> _visitedRooms = new HashSet<string>();
        private static readonly HashSet<int> _killedEnemies = new HashSet<int>();
        // 箱子对象是池化的（同一个实例会被不同房间复用），所以按"房间 + 箱子序号 + 坐标"去重，
        // 不能用 GetInstanceID。
        private static readonly HashSet<string> _chestsOpened = new HashSet<string>();

        private static float _decayAccum;
        private static float _sendTimer;
        private static bool _dirty;

        /// <summary>任务/特殊类遗物：拿到不扣分（森林开门用的 Lily、龙钥匙、调试用 GodMode 等）。</summary>
        private static readonly HashSet<string> QuestRelics = new HashSet<string>
        {
            "Lily1", "Lily2", "Lily3",
            "DragonKeyBlack", "DragonKeyWhite",
            "GodMode",
        };

        // =====================================================================
        //  生命周期
        // =====================================================================

        /// <summary>开跑时调用：清零双方积分，重置房间/怪物去重表。</summary>
        internal static void ResetForRace()
        {
            LocalScore = 0;
            LocalRooms = 0;
            LocalKills = 0;
            LocalBosses = 0;
            LocalRelics = 0;
            LocalDeaths = 0;
            LocalChests = 0;
            LocalChestPoints = 0;
            LocalCoinPoints = 0;
            LocalGoldConverted = 0;
            LocalGoldApplied = false;
            LocalRoomPoints = 0;
            LocalKillPoints = 0;
            LocalBossPoints = 0;
            LocalRelicPoints = 0;
            LocalDeathPoints = 0;
            LocalDecayPoints = 0;

            _visitedRooms.Clear();
            _killedEnemies.Clear();
            _chestsOpened.Clear();
            _decayAccum = 0f;
            _sendTimer = 0f;
            _dirty = false;

            Plugin.Log.LogInfo("[RL2MP] [SCORE] 积分已清零（玩法=" + StyleName(Style)
                + " 判定=" + ModeName(Mode)
                + " 房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
                + " 箱子+" + ChestBronzePoints + "/" + ChestSilverPoints + "/" + ChestGoldPoints
                + "/" + ChestFairyPoints
                + " 金币 " + EffectiveGoldPerPoint() + "=1分"
                + " 遗物-" + RelicPenalty + " 死亡-" + DeathPenalty
                + " 衰减-" + DecayPerMinute + "/分）");
        }

        /// <summary>每帧驱动（ControlPanel.Update 调用）。</summary>
        internal static void Tick()
        {
            if (Race.Phase != RacePhase.Racing)
            {
                _dirty = false;
                return;
            }

            float dt = Time.unscaledDeltaTime;

            if (DecayPerMinute > 0)
            {
                _decayAccum += dt;
                while (_decayAccum >= 60f)
                {
                    _decayAccum -= 60f;
                    LocalDecayPoints -= DecayPerMinute;
                    Add(-DecayPerMinute, "时间衰减");
                }
            }
            else
            {
                _decayAccum = 0f;
            }

            _sendTimer += dt;
            if (_dirty && _sendTimer >= 0.5f)
            {
                _sendTimer = 0f;
                _dirty = false;
                SendState();
            }
        }

        private static void Add(int delta, string reason)
        {
            LocalScore += delta;
            _dirty = true;
            Plugin.Log.LogInfo("[RL2MP] [SCORE] " + reason + " " + (delta >= 0 ? "+" : "") + delta
                + " -> " + LocalScore);
        }

        // =====================================================================
        //  计分入口
        // =====================================================================

        /// <summary>进入了一个房间（按房间去重，来回走不重复计分）。</summary>
        internal static void OnRoomEntered(BaseRoom room)
        {
            if (Race.Phase != RacePhase.Racing || room == null) return;

            string key = RoomKey(room);
            if (!_visitedRooms.Add(key)) return;

            LocalRooms++;
            LocalRoomPoints += RoomPoints;
            if (RoomPoints != 0) Add(RoomPoints, "探索新房间 '" + room.gameObject.name + "' (#" + LocalRooms + ")");
            else _dirty = true;
        }

        private static string RoomKey(BaseRoom room)
        {
            Vector3 p = room.transform.position;
            return room.BiomeControllerIndex + "|" + room.gameObject.name
                + "|" + Mathf.RoundToInt(p.x) + "," + Mathf.RoundToInt(p.y);
        }

        /// <summary>打死了一个怪（Boss 不算在这里）。</summary>
        internal static void OnEnemyKilled(EnemyController enemy)
        {
            if (Race.Phase != RacePhase.Racing || enemy == null) return;
            if (enemy.IsBoss) return;
            if (!_killedEnemies.Add(enemy.GetInstanceID())) return;

            LocalKills++;
            if (EnemyPoints != 0)
            {
                LocalKillPoints += EnemyPoints;
                LocalScore += EnemyPoints;
                _dirty = true;
            }
        }

        /// <summary>打死了一个 Boss（终点 Boss 也算分，然后才结算）。</summary>
        internal static void OnBossDefeated(int bossId)
        {
            if (Race.Phase != RacePhase.Racing) return;
            LocalBosses++;
            LocalBossPoints += BossPoints;
            Add(BossPoints, "击败 Boss " + Race.BossName(bossId) + " (#" + LocalBosses + ")");
        }

        /// <summary>打开一个箱子（v1.7.2）。按箱子品质加分，同一个箱子只算一次。</summary>
        internal static void OnChestOpened(ChestObj chest)
        {
            if (Race.Phase != RacePhase.Racing || chest == null) return;

            string key;
            try
            {
                Vector3 p = chest.transform.position;
                key = (chest.Room != null ? RoomKey(chest.Room) : "?")
                    + "#" + chest.ChestIndex
                    + "@" + Mathf.RoundToInt(p.x) + "," + Mathf.RoundToInt(p.y);
            }
            catch { return; }

            if (!_chestsOpened.Add(key)) return;   // 重复回调（存档重载 / 重复交互 / 池对象复用前的旧记录）

            int pts = ChestPointsFor(chest.ChestType);
            LocalChests++;
            if (pts != 0)
            {
                LocalChestPoints += pts;
                Add(pts, "开箱[" + chest.ChestType + "] (#" + LocalChests + ")");
            }
            else
            {
                _dirty = true;
                Plugin.Log.LogInfo("[RL2MP] [SCORE] 开箱[" + chest.ChestType + "] 不计分。");
            }
        }

        /// <summary>箱子分值表（Boss 箱/黑箱由 Boss 击杀分覆盖，不再重复加）。</summary>
        internal static int ChestPointsFor(ChestType type)
        {
            switch (type)
            {
                case ChestType.Bronze: return ChestBronzePoints;
                case ChestType.Silver: return ChestSilverPoints;
                case ChestType.Gold: return ChestGoldPoints;
                case ChestType.Fairy: return ChestFairyPoints;
                default: return 0;
            }
        }

        /// <summary>本地玩家死亡（由 Race.OnLocalDeath 调用）。</summary>
        internal static void OnDeath()
        {
            if (Race.Phase != RacePhase.Racing) return;
            LocalDeaths++;
            LocalDeathPoints -= DeathPenalty;
            if (DeathPenalty != 0) Add(-DeathPenalty, "死亡 #" + LocalDeaths);
            else _dirty = true;
        }

        /// <summary>拿了一个遗物（等级增长）。任务类不扣分。</summary>
        internal static void OnRelicGained(RelicType type, int deltaLevels)
        {
            if (Race.Phase != RacePhase.Racing || deltaLevels <= 0) return;

            if (IsQuestRelic(type))
            {
                Plugin.Log.LogInfo("[RL2MP] [SCORE] 任务类遗物 " + type + " x" + deltaLevels + " -> 不扣分。");
                return;
            }

            LocalRelics += deltaLevels;
            int ded = RelicPenalty * deltaLevels;
            LocalRelicPoints -= ded;
            if (ded != 0) Add(-ded, "拾取遗物 " + type + " x" + deltaLevels);
            else _dirty = true;
        }

        // =====================================================================
        //  金币折算（完赛时结算，v1.7.2）
        // =====================================================================

        /// <summary>当前周目（NG+ 等级，0 = 一周目）。</summary>
        internal static int CurrentNgPlus()
        {
            try { return Mathf.Max(0, SaveManager.PlayerSaveData.NewGamePlusLevel); }
            catch { return 0; }
        }

        /// <summary>
        /// 当前生效的兑换门槛：NG+0 为 GoldPerPoint，每多一个周目 +GoldNgPlusStep。
        /// 周目越高金币收益越高（怪掉更多、箱子里更多），门槛同比上涨才能保持"金币分"的权重。
        /// </summary>
        internal static int EffectiveGoldPerPoint()
        {
            int ng = CurrentNgPlus();
            float f = GoldPerPoint * (1f + GoldNgPlusStep * ng);
            return Mathf.Clamp(Mathf.RoundToInt(f), 100, 10000000);
        }

        /// <summary>本局收集到的金币（存档里的 GoldCollected 就是"本局收集"）。</summary>
        internal static int CollectedGold()
        {
            try { return Mathf.Max(0, SaveManager.PlayerSaveData.GoldCollected); }
            catch { return 0; }
        }

        /// <summary>
        /// 完赛结算：把本局金币折算成分加进总分（只加一次）。
        /// 在"本地玩家完成"时调用，所以 DNF 玩家没有金币分。
        /// </summary>
        internal static void ApplyGoldConversion(string reason)
        {
            if (LocalGoldApplied) return;
            LocalGoldApplied = true;

            int gold = CollectedGold();
            int per = EffectiveGoldPerPoint();
            int pts = per > 0 ? gold / per : 0;

            LocalGoldConverted = gold;
            LocalCoinPoints = pts;

            Plugin.Log.LogInfo("[RL2MP] [SCORE] 金币结算(" + reason + ")：本局收集 " + gold
                + " / 每分 " + per + "（NG+=" + CurrentNgPlus() + "）-> +" + pts + " 分");

            if (pts != 0) Add(pts, "金币折算 " + gold + " / " + per);
            else _dirty = true;
        }

        internal static bool IsQuestRelic(RelicType t)
        {
            string n = t.ToString();
            if (n == "None") return true;
            if (n.EndsWith("Used")) return true;   // ExtraLifeUsed / *ChallengeUsed 之类
            return QuestRelics.Contains(n);
        }

        // =====================================================================
        //  网络同步
        // =====================================================================

        internal static void SendState()
        {
            // 先把本地槽位刷新（结算/大盘都读槽位，单人调试时也要对）
            Roster.SyncSelf();
            var me = Roster.Local;
            if (me != null)
            {
                me.Score = LocalScore;
                me.Rooms = LocalRooms;
                me.Kills = LocalKills;
                me.Bosses = LocalBosses;
                me.Relics = LocalRelics;
                me.Deaths = Race.LocalDeaths;
                me.Chests = LocalChests;
                me.ChestPoints = LocalChestPoints;
                me.CoinPoints = LocalCoinPoints;
                me.GoldCollected = LocalGoldConverted;
            }

            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected || nm.LocalPlayerId < 0) return;

            nm.SendScoreState(nm.LocalPlayerId, LocalScore, LocalRooms, LocalKills, LocalBosses,
                LocalRelics, Race.LocalDeaths, LocalChests, LocalChestPoints, LocalCoinPoints, LocalGoldConverted);
        }

        /// <summary>重连成功后立刻补发一次本地积分（绕过节流定时器）。</summary>
        internal static void ForcePushState()
        {
            SendState();
        }

        /// <summary>收到某个玩家的积分（主机自己也会在本地写槽位）。</summary>
        internal static void HandleState(int id, int score, int rooms, int kills, int bosses, int relics, int deaths,
            int chests, int chestPoints, int coinPoints, int goldCollected)
        {
            var s = Roster.Get(id);
            s.Score = score;
            s.Rooms = rooms;
            s.Kills = kills;
            s.Bosses = bosses;
            s.Relics = relics;
            s.Deaths = deaths;
            s.Chests = chests;
            s.ChestPoints = chestPoints;
            s.CoinPoints = coinPoints;
            s.GoldCollected = goldCollected;
        }

        internal static void BroadcastConfig()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return;
            if (nm.Connected)
                nm.SendScoreConfig(Mode, RoomPoints, EnemyPoints, BossPoints, RelicPenalty, DeathPenalty,
                    DecayPerMinute, ChestBronzePoints, ChestSilverPoints, ChestGoldPoints, ChestFairyPoints,
                    GoldPerPoint, GoldNgPlusStep, Style);

            Plugin.Log.LogInfo("[RL2MP] [SCORE] HOST 规则广播 -> 玩法=" + StyleName(Style)
                + " 判定=" + ModeName(Mode)
                + " 房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
                + " 箱子+" + ChestBronzePoints + "/" + ChestSilverPoints + "/" + ChestGoldPoints + "/" + ChestFairyPoints
                + " 金币 " + GoldPerPoint + "(" + CurrentNgPlus() + "周目 -> " + EffectiveGoldPerPoint() + ")=1分"
                + " 遗物-" + RelicPenalty + " 死亡-" + DeathPenalty
                + " 衰减-" + DecayPerMinute + "/分");
        }

        internal static void ApplyConfig(RaceGameMode mode, int room, int enemy, int boss, int relic, int death, int decay,
            int chestBronze, int chestSilver, int chestGold, int chestFairy, int goldPerPoint, float goldNgPlusStep,
            PlayStyle style = PlayStyle.Race)
        {
            Mode = mode;
            Style = style;
            RoomPoints = Mathf.Clamp(room, 0, MaxPoints);
            EnemyPoints = Mathf.Clamp(enemy, 0, MaxPoints);
            BossPoints = Mathf.Clamp(boss, 0, MaxPoints);
            RelicPenalty = Mathf.Clamp(relic, -MaxPoints, MaxPoints);
            DeathPenalty = Mathf.Clamp(death, -MaxPoints, MaxPoints);
            DecayPerMinute = Mathf.Clamp(decay, 0, MaxPoints);
            ChestBronzePoints = Mathf.Clamp(chestBronze, 0, MaxPoints);
            ChestSilverPoints = Mathf.Clamp(chestSilver, 0, MaxPoints);
            ChestGoldPoints = Mathf.Clamp(chestGold, 0, MaxPoints);
            ChestFairyPoints = Mathf.Clamp(chestFairy, 0, MaxPoints);
            GoldPerPoint = Mathf.Clamp(goldPerPoint, 100, 10000000);
            GoldNgPlusStep = Mathf.Clamp(goldNgPlusStep, 0f, 5f);

            Plugin.Log.LogInfo("[RL2MP] [SCORE] 收到规则: 玩法=" + StyleName(Style)
                + " 判定=" + ModeName(Mode)
                + " 房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
                + " 箱子+" + ChestBronzePoints + "/" + ChestSilverPoints + "/" + ChestGoldPoints + "/" + ChestFairyPoints
                + " 金币 " + GoldPerPoint + "=1分(周目递增 " + (GoldNgPlusStep * 100f).ToString("F0") + "%)"
                + " 遗物-" + RelicPenalty + " 死亡-" + DeathPenalty
                + " 衰减-" + DecayPerMinute + "/分");
        }

        internal static void ResetDefaults()
        {
            Style = PlayStyle.Race;
            Mode = RaceGameMode.Time;
            RoomPoints = 25;
            EnemyPoints = 3;
            BossPoints = 300;
            RelicPenalty = 50;
            DeathPenalty = 100;
            DecayPerMinute = 20;
            ChestBronzePoints = 40;
            ChestSilverPoints = 80;
            ChestGoldPoints = 150;
            ChestFairyPoints = 200;
            GoldPerPoint = 10000;
            GoldNgPlusStep = 0.35f;
            BroadcastConfig();
        }

        public static string ModeName(RaceGameMode m)
        {
            return m == RaceGameMode.Score ? "比得分" : "比用时";
        }

        public static string StyleName(PlayStyle s)
        {
            return s == PlayStyle.CoOp ? "双人协作" : "竞速";
        }

        // =====================================================================
        //  显示辅助
        // =====================================================================

        internal static string LocalBreakdown()
        {
            return "房间 " + LocalRooms + " · 怪 " + LocalKills + " · Boss " + LocalBosses
                + " · 箱 " + LocalChests + " · 遗物 " + LocalRelics + " · 死 " + LocalDeaths;
        }

        internal static string Breakdown(PlayerSlot s)
        {
            return "房间 " + s.Rooms + " · 怪 " + s.Kills + " · Boss " + s.Bosses
                + " · 箱 " + s.Chests + " · 遗物 " + s.Relics + " · 死 " + s.Deaths;
        }

        /// <summary>调试用：把当前规则打成一行（面板显示）。</summary>
        internal static string RuleLine()
        {
            return "房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
                + " 箱+" + ChestBronzePoints + "/" + ChestSilverPoints + "/" + ChestGoldPoints
                + " 金币" + (EffectiveGoldPerPoint() / 1000) + "k=1分"
                + " 遗物-" + RelicPenalty + " 死亡-" + DeathPenalty + " 衰减-" + DecayPerMinute + "/分";
        }
    }

    // ---------------- Harmony 补丁：房间 / 怪物 / 遗物 ----------------

    internal static class ScorePatches
    {
        internal static void Apply(Harmony harmony)
        {
            TryPatch(harmony,
                AccessTools.Method(typeof(PlayerController), "EnterRoom",
                    new Type[] { typeof(BaseRoom), typeof(Door), typeof(Vector3) }),
                "EnterRoomPostfix", "房间探索计分");

            TryPatch(harmony,
                AccessTools.Method(typeof(EnemyController), "KillCharacter",
                    new Type[] { typeof(GameObject), typeof(bool) }),
                null, "怪物击杀计分", postfixName: "EnemyKillPostfix");

            TryPatch(harmony,
                AccessTools.Method(typeof(RelicObj), "SetLevel",
                    new Type[] { typeof(int), typeof(bool), typeof(bool) }),
                "RelicSetLevelPrefix", "遗物扣分", postfixName: "RelicSetLevelPostfix");

            // 开箱加分：只挂私有的 OpenChest（玩家交互触发），不挂 ForceOpenChest
            // —— 后者在房间存档重载时会被调用，用来"还原已开过的箱子"，不该加分。
            TryPatch(harmony,
                AccessTools.Method(typeof(ChestObj), "OpenChest", new Type[0]),
                null, "开箱计分", postfixName: "ChestOpenPostfix");
        }

        private static void TryPatch(Harmony harmony, MethodInfo target, string prefixName,
            string label, string postfixName = null)
        {
            try
            {
                if (target == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [SCORE] " + label + ": 目标方法没找到，跳过。");
                    return;
                }

                HarmonyMethod pre = null, post = null;
                if (!string.IsNullOrEmpty(prefixName))
                    pre = new HarmonyMethod(typeof(ScorePatches).GetMethod(prefixName,
                        BindingFlags.Static | BindingFlags.NonPublic));
                if (!string.IsNullOrEmpty(postfixName))
                    post = new HarmonyMethod(typeof(ScorePatches).GetMethod(postfixName,
                        BindingFlags.Static | BindingFlags.NonPublic));

                harmony.Patch(target, prefix: pre, postfix: post);
                Plugin.Log.LogInfo("[RL2MP] [SCORE] " + label + " 补丁 OK (" + target.Name + ").");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SCORE] " + label + " 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        // 玩家进入房间（所有换路方式最终都走这里：门、传送、复活放置）
        private static void EnterRoomPostfix(BaseRoom room)
        {
            try { Score.OnRoomEntered(room); }
            catch (Exception ex) { Plugin.Log.LogWarning("[RL2MP] [SCORE] EnterRoomPostfix: " + ex.Message); }
        }

        // 怪物死亡
        private static void EnemyKillPostfix(EnemyController __instance)
        {
            try { Score.OnEnemyKilled(__instance); }
            catch { /* 计分失败不能影响游戏 */ }
        }

        // 遗物等级增长（拾取 / 购买 / 奖励都会走 SetLevel）
        private static void RelicSetLevelPrefix(RelicObj __instance, ref int __state)
        {
            try { __state = __instance.Level; } catch { __state = 0; }
        }

        // 玩家开箱：OpenChest 的 postfix（IsOpen 已置 true，ChestType 已确定）
        private static void ChestOpenPostfix(ChestObj __instance)
        {
            try
            {
                // 上锁的妖精箱被拒绝时会提前 return（只播"锁链响"），IsOpen 仍是 false
                if (__instance != null && __instance.IsOpen) Score.OnChestOpened(__instance);
            }
            catch { /* 计分失败不能影响游戏 */ }
        }

        private static void RelicSetLevelPostfix(RelicObj __instance, int __state)
        {
            try
            {
                int now = __instance.Level;
                if (now > __state) Score.OnRelicGained(__instance.RelicType, now - __state);
            }
            catch { /* 计分失败不能影响游戏 */ }
        }
    }
}
