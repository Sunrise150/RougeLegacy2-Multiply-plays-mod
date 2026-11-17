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

        public static RaceGameMode Mode = RaceGameMode.Time;

        public static int RoomPoints = 25;        // 每探索一个新房间
        public static int EnemyPoints = 3;        // 每击杀一个普通怪物
        public static int BossPoints = 300;       // 每击败一个 Boss
        public static int RelicPenalty = 50;      // 每拿一个非任务遗物
        public static int DeathPenalty = 100;     // 每死一次
        public static int DecayPerMinute = 20;    // 每分钟自然衰减

        public const int MaxPoints = 100000;

        // ---------------- 本地积分 ----------------

        public static int LocalScore;
        public static int LocalRooms;
        public static int LocalKills;
        public static int LocalBosses;
        public static int LocalRelics;
        public static int LocalDeaths;

        // ---------------- 内部状态 ----------------

        private static readonly HashSet<string> _visitedRooms = new HashSet<string>();
        private static readonly HashSet<int> _killedEnemies = new HashSet<int>();

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

            _visitedRooms.Clear();
            _killedEnemies.Clear();
            _decayAccum = 0f;
            _sendTimer = 0f;
            _dirty = false;

            Plugin.Log.LogInfo("[RL2MP] [SCORE] 积分已清零（模式=" + ModeName(Mode)
                + " 房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
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
                LocalScore += EnemyPoints;
                _dirty = true;
            }
        }

        /// <summary>打死了一个 Boss（终点 Boss 也算分，然后才结算）。</summary>
        internal static void OnBossDefeated(int bossId)
        {
            if (Race.Phase != RacePhase.Racing) return;
            LocalBosses++;
            Add(BossPoints, "击败 Boss " + Race.BossName(bossId) + " (#" + LocalBosses + ")");
        }

        /// <summary>本地玩家死亡（由 Race.OnLocalDeath 调用）。</summary>
        internal static void OnDeath()
        {
            if (Race.Phase != RacePhase.Racing) return;
            LocalDeaths++;
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
            if (ded != 0) Add(-ded, "拾取遗物 " + type + " x" + deltaLevels);
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
            }

            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected || nm.LocalPlayerId < 0) return;

            nm.SendScoreState(nm.LocalPlayerId, LocalScore, LocalRooms, LocalKills, LocalBosses, LocalRelics, Race.LocalDeaths);
        }

        /// <summary>重连成功后立刻补发一次本地积分（绕过节流定时器）。</summary>
        internal static void ForcePushState()
        {
            SendState();
        }

        /// <summary>收到某个玩家的积分（主机自己也会在本地写槽位）。</summary>
        internal static void HandleState(int id, int score, int rooms, int kills, int bosses, int relics, int deaths)
        {
            var s = Roster.Get(id);
            s.Score = score;
            s.Rooms = rooms;
            s.Kills = kills;
            s.Bosses = bosses;
            s.Relics = relics;
            s.Deaths = deaths;
        }

        internal static void BroadcastConfig()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.IsHost) return;
            if (nm.Connected) nm.SendScoreConfig(Mode, RoomPoints, EnemyPoints, BossPoints, RelicPenalty, DeathPenalty, DecayPerMinute);

            Plugin.Log.LogInfo("[RL2MP] [SCORE] HOST 规则广播 -> 模式=" + ModeName(Mode)
                + " 房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
                + " 遗物-" + RelicPenalty + " 死亡-" + DeathPenalty
                + " 衰减-" + DecayPerMinute + "/分");
        }

        internal static void ApplyConfig(RaceGameMode mode, int room, int enemy, int boss, int relic, int death, int decay)
        {
            Mode = mode;
            RoomPoints = Mathf.Clamp(room, 0, MaxPoints);
            EnemyPoints = Mathf.Clamp(enemy, 0, MaxPoints);
            BossPoints = Mathf.Clamp(boss, 0, MaxPoints);
            RelicPenalty = Mathf.Clamp(relic, -MaxPoints, MaxPoints);
            DeathPenalty = Mathf.Clamp(death, -MaxPoints, MaxPoints);
            DecayPerMinute = Mathf.Clamp(decay, 0, MaxPoints);

            Plugin.Log.LogInfo("[RL2MP] [SCORE] 收到规则: 模式=" + ModeName(Mode)
                + " 房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
                + " 遗物-" + RelicPenalty + " 死亡-" + DeathPenalty
                + " 衰减-" + DecayPerMinute + "/分");
        }

        internal static void ResetDefaults()
        {
            Mode = RaceGameMode.Time;
            RoomPoints = 25;
            EnemyPoints = 3;
            BossPoints = 300;
            RelicPenalty = 50;
            DeathPenalty = 100;
            DecayPerMinute = 20;
            BroadcastConfig();
        }

        public static string ModeName(RaceGameMode m)
        {
            return m == RaceGameMode.Score ? "比得分" : "比用时";
        }

        // =====================================================================
        //  显示辅助
        // =====================================================================

        internal static string LocalBreakdown()
        {
            return "房间 " + LocalRooms + " · 怪 " + LocalKills + " · Boss " + LocalBosses
                + " · 遗物 " + LocalRelics + " · 死 " + LocalDeaths;
        }

        internal static string Breakdown(PlayerSlot s)
        {
            return "房间 " + s.Rooms + " · 怪 " + s.Kills + " · Boss " + s.Bosses
                + " · 遗物 " + s.Relics + " · 死 " + s.Deaths;
        }

        /// <summary>调试用：把当前规则打成一行（面板显示）。</summary>
        internal static string RuleLine()
        {
            return "房间+" + RoomPoints + " 怪+" + EnemyPoints + " Boss+" + BossPoints
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
                Plugin.Log.LogWarning("[RL2MP] [SCORE] " + label + " 补丁失败: " + ex.Message);
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
