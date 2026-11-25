using System;
using System.IO;
using System.Xml.Serialization;
using BepInEx;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.0：联机配置档。把当前整套联机/竞速规则存成 JSON（BepInEx/config/RL2MP_Profiles.json），
    /// 支持多套预设的保存 / 载入 / 重命名 / 删除。主机载入后会自动把规则广播给客机。
    /// 用 UnityEngine.JsonUtility 序列化，无第三方依赖。
    /// </summary>
    [Serializable]
    public class RL2MPProfileData
    {
        public string name = "未命名";

        public int style;            // PlayStyle: 0=Race, 1=CoOp
        public int mode;             // RaceGameMode: 0=Time, 1=Score
        public int roomPoints;
        public int enemyPoints;
        public int bossPoints;
        public int relicPenalty;
        public int deathPenalty;
        public int decayPerMinute;
        public int chestBronze;
        public int chestSilver;
        public int chestGold;
        public int chestFairy;
        public int goldPerPoint;
        public float goldNgPlusStep;
        public float penaltyPerDeath;
        public float finishGrace;
        public float reconnectGrace;
        public int targetBossId;
        public string hostAddress = "127.0.0.1";
        public bool keepWorldRunning = true;
    }

    [Serializable]
    public class RL2MPProfileStore
    {
        public RL2MPProfileData[] profiles;
    }

    internal static class ConfigProfiles
    {
        internal const int MaxProfiles = 12;

        private static string ConfigDir
        {
            get
            {
                try { return Paths.ConfigPath; }
                catch { return Path.Combine(Application.dataPath, "..", "BepInEx", "config"); }
            }
        }

        private static string FilePath { get { return Path.Combine(ConfigDir, "RL2MP_Profiles.xml"); } }

        private static RL2MPProfileStore _store;

        internal static RL2MPProfileStore Store
        {
            get
            {
                if (_store == null) Load();
                return _store;
            }
        }

        internal static void Load()
        {
            try
            {
                _store = null;
                if (File.Exists(FilePath))
                {
                    var ser = new XmlSerializer(typeof(RL2MPProfileStore));
                    using (var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read))
                    {
                        _store = ser.Deserialize(fs) as RL2MPProfileStore;
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[PROFILE] 读取配置档失败: " + ex.Message);
                _store = null;
            }
            if (_store == null || _store.profiles == null)
            {
                _store = new RL2MPProfileStore();
                _store.profiles = new RL2MPProfileData[0];
            }
        }

        private static void Persist()
        {
            try
            {
                if (!Directory.Exists(ConfigDir)) Directory.CreateDirectory(ConfigDir);
                var ser = new XmlSerializer(typeof(RL2MPProfileStore));
                using (var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write))
                {
                    ser.Serialize(fs, _store);
                }
                Plugin.Log.LogInfo("[PROFILE] 已保存 " + _store.profiles.Length + " 套配置档 -> " + FilePath);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[PROFILE] 写盘失败: " + ex.Message);
            }
        }

        /// <summary>从当前运行时静态字段抓取一份快照。</summary>
        internal static RL2MPProfileData CaptureCurrent(string name)
        {
            return new RL2MPProfileData
            {
                name = string.IsNullOrEmpty(name) ? "未命名" : name,
                style = (int)Score.Style,
                mode = (int)Score.Mode,
                roomPoints = Score.RoomPoints,
                enemyPoints = Score.EnemyPoints,
                bossPoints = Score.BossPoints,
                relicPenalty = Score.RelicPenalty,
                deathPenalty = Score.DeathPenalty,
                decayPerMinute = Score.DecayPerMinute,
                chestBronze = Score.ChestBronzePoints,
                chestSilver = Score.ChestSilverPoints,
                chestGold = Score.ChestGoldPoints,
                chestFairy = Score.ChestFairyPoints,
                goldPerPoint = Score.GoldPerPoint,
                goldNgPlusStep = Score.GoldNgPlusStep,
                penaltyPerDeath = Race.PenaltyPerDeath,
                finishGrace = Race.FinishGraceSeconds,
                reconnectGrace = NetworkManager.ReconnectGraceSeconds,
                targetBossId = Race.TargetBossId,
                hostAddress = Plugin.HostAddress,
                keepWorldRunning = PauseCompat.KeepWorldRunning
            };
        }

        internal static void SaveNew(RL2MPProfileData data)
        {
            var list = new System.Collections.Generic.List<RL2MPProfileData>(Store.profiles);
            if (list.Count >= MaxProfiles)
            {
                Plugin.Log.LogWarning("[PROFILE] 已达上限 " + MaxProfiles + " 套，无法再存。");
                return;
            }
            list.Add(data);
            _store.profiles = list.ToArray();
            Persist();
        }

        internal static void Overwrite(int index, RL2MPProfileData data)
        {
            if (index < 0 || index >= Store.profiles.Length) return;
            _store.profiles[index] = data;
            Persist();
        }

        internal static void Delete(int index)
        {
            if (index < 0 || index >= Store.profiles.Length) return;
            var list = new System.Collections.Generic.List<RL2MPProfileData>(Store.profiles);
            list.RemoveAt(index);
            _store.profiles = list.ToArray();
            Persist();
        }

        /// <summary>载入配置档到运行时；主机自动广播。返回是否成功。</summary>
        internal static bool Apply(int index)
        {
            if (index < 0 || index >= Store.profiles.Length) return false;
            RL2MPProfileData p = Store.profiles[index];
            try
            {
                Score.Style = (PlayStyle)Mathf.Clamp(p.style, 0, 1);
                Score.Mode = (RaceGameMode)Mathf.Clamp(p.mode, 0, 1);
                Score.RoomPoints = Mathf.Clamp(p.roomPoints, 0, 5000);
                Score.EnemyPoints = Mathf.Clamp(p.enemyPoints, 0, 500);
                Score.BossPoints = Mathf.Clamp(p.bossPoints, 0, 20000);
                Score.RelicPenalty = p.relicPenalty;
                Score.DeathPenalty = p.deathPenalty;
                Score.DecayPerMinute = Mathf.Clamp(p.decayPerMinute, 0, 2000);
                Score.ChestBronzePoints = Mathf.Clamp(p.chestBronze, 0, 5000);
                Score.ChestSilverPoints = Mathf.Clamp(p.chestSilver, 0, 5000);
                Score.ChestGoldPoints = Mathf.Clamp(p.chestGold, 0, 5000);
                Score.ChestFairyPoints = Mathf.Clamp(p.chestFairy, 0, 5000);
                Score.GoldPerPoint = Mathf.Clamp(p.goldPerPoint, 1000, 1000000);
                Score.GoldNgPlusStep = Mathf.Clamp(p.goldNgPlusStep, 0f, 5f);

                Race.PenaltyPerDeath = Mathf.Max(0f, p.penaltyPerDeath);
                Race.FinishGraceSeconds = Mathf.Max(0f, p.finishGrace);
                NetworkManager.ReconnectGraceSeconds = Mathf.Max(0f, p.reconnectGrace);

                if (!string.IsNullOrEmpty(p.hostAddress))
                {
                    Plugin.HostAddressCfg.Value = p.hostAddress;
                }

                // v1.9.1：联机暂停不冻结世界已改为强制生效，不再受此开关影响。
                // 保留字段仅为兼容旧配置档读写，不再写回运行时。

                // 终点 Boss：走主机设定接口（会广播 RaceConfig）；非主机只本地记录
                var nm = NetworkManager.Instance;
                if (nm != null && nm.IsHost)
                {
                    Race.HostSetTargetBoss(p.targetBossId);
                    Score.BroadcastConfig();
                }
                else
                {
                    Race.TargetBossId = p.targetBossId;
                }

                Plugin.Log.LogInfo("[PROFILE] 已载入配置档 '" + p.name + "'");
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[PROFILE] 载入失败: " + ex.Message);
                return false;
            }
        }
    }
}
