using System.Collections.Generic;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>一个玩家的会话状态（主机 + 各客机各一份）。</summary>
    public class PlayerSlot
    {
        public int Id;
        public bool IsLocal;
        public bool IsHost;

        // 大厅
        public bool Ready;
        public bool InWorld;

        // 竞速
        public float FinishTime = -1f;   // <0 = 还没完成
        public bool Dnf;                 // 超时未完成，判为未完成
        public int Deaths;
        public float PenaltySeconds;

        // 得分
        public int Score;
        public int Rooms, Kills, Bosses, Relics;

        // 会话恢复（v1.6.0）：主机为每个客机生成 token，重连时校验；
        // Offline = 网络断了但仍在重连宽限期内（槽位保留，成绩不清）
        public int Token;
        public bool Offline;
        public float OfflineSince;

        public void ClearRaceState()
        {
            Ready = false;
            InWorld = false;
            FinishTime = -1f;
            Dnf = false;
            Deaths = 0;
            PenaltySeconds = 0f;
            Score = 0;
            Rooms = 0;
            Kills = 0;
            Bosses = 0;
            Relics = 0;
        }

        /// <summary>紧凑标签：主机 / P2 / P3 …</summary>
        public string Tag
        {
            get { return Id == 0 ? "主机" : ("P" + Id); }
        }

        /// <summary>界面显示名（自己显示为"你"）。</summary>
        public string Display
        {
            get { return IsLocal ? "你" : (Offline ? Tag + "(掉线)" : Tag); }
        }

        public bool Finished { get { return FinishTime >= 0f; } }
    }

    /// <summary>
    /// 会话内玩家名单（最多 4 人，理论上更多也行）。
    /// 原来 Race / Score 里那一堆 RemoteXxx 单值字段，都是为了"只有一个对手"设计的；
    /// 这里统一成按 playerId 索引的槽位，所有跨玩家状态都放这儿。
    /// </summary>
    internal static class Roster
    {
        public const int MaxPlayers = 4;

        private static readonly Dictionary<int, PlayerSlot> _slots = new Dictionary<int, PlayerSlot>();

        /// <summary>本地玩家的槽位（进会话/拿到 LocalPlayerId 后由 SyncSelf 建立）。</summary>
        public static PlayerSlot Local { get; private set; }

        /// <summary>根据 NetworkManager 的 LocalPlayerId 建立/刷新本地槽位。</summary>
        internal static void SyncSelf()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || nm.LocalPlayerId < 0)
            {
                Local = null;
                return;
            }

            var s = Get(nm.LocalPlayerId);
            s.IsLocal = true;
            s.IsHost = nm.IsHost;
            Local = s;
        }

        internal static PlayerSlot Get(int id)
        {
            PlayerSlot s;
            if (!_slots.TryGetValue(id, out s))
            {
                s = new PlayerSlot();
                s.Id = id;
                s.IsHost = id == 0;
                _slots[id] = s;
                Plugin.Log.LogInfo("[RL2MP] [ROSTER] 新增玩家槽位 id=" + id + "（当前 " + _slots.Count + " 人）");
            }
            return s;
        }

        internal static void Remove(int id)
        {
            if (_slots.Remove(id))
            {
                Plugin.Log.LogInfo("[RL2MP] [ROSTER] 移除玩家 id=" + id + "（剩余 " + _slots.Count + " 人）");
            }
            if (Local != null && Local.Id == id) Local = null;
        }

        internal static void Clear(string reason)
        {
            if (_slots.Count > 0)
            {
                Plugin.Log.LogInfo("[RL2MP] [ROSTER] 清空名单（" + reason + "），原 " + _slots.Count + " 人。");
            }
            _slots.Clear();
            Local = null;
        }

        internal static int Count { get { return _slots.Count; } }

        internal static IEnumerable<PlayerSlot> All { get { return _slots.Values; } }

        /// <summary>主机视角：除自己以外的所有人。</summary>
        internal static List<PlayerSlot> Others()
        {
            var list = new List<PlayerSlot>();
            foreach (var s in _slots.Values)
            {
                if (Local != null && s.Id == Local.Id) continue;
                list.Add(s);
            }
            list.Sort(delegate (PlayerSlot a, PlayerSlot b) { return a.Id.CompareTo(b.Id); });
            return list;
        }

        /// <summary>按比赛规则排序（名次）：完成者在前，未完成（DNF）在后。</summary>
        internal static List<PlayerSlot> Ranked(bool scoreMode)
        {
            var list = new List<PlayerSlot>(_slots.Values);
            list.Sort(delegate (PlayerSlot a, PlayerSlot b)
            {
                bool af = a.Finished && !a.Dnf;
                bool bf = b.Finished && !b.Dnf;
                if (af != bf) return af ? -1 : 1;

                if (af && bf)
                {
                    if (scoreMode)
                    {
                        if (a.Score != b.Score) return b.Score.CompareTo(a.Score);
                        return a.FinishTime.CompareTo(b.FinishTime);
                    }
                    return a.FinishTime.CompareTo(b.FinishTime);
                }

                // 都没完成：分数高的在前（比得分模式），否则按 id
                if (scoreMode && a.Score != b.Score) return b.Score.CompareTo(a.Score);
                return a.Id.CompareTo(b.Id);
            });
            return list;
        }

        /// <summary>所有非本机玩家都准备好了，并且都在游戏世界里。</summary>
        internal static bool AllOthersReady()
        {
            int n = 0;
            foreach (var s in _slots.Values)
            {
                if (Local != null && s.Id == Local.Id) continue;
                n++;
                if (!s.Ready || !s.InWorld) return false;
            }
            return n > 0;
        }

        internal static int ReadyCount()
        {
            int n = 0;
            foreach (var s in _slots.Values) if (s.Ready && s.InWorld) n++;
            return n;
        }

        internal static void ClearAllRaceState()
        {
            foreach (var s in _slots.Values) s.ClearRaceState();
        }

        internal static string NamesLine()
        {
            var sb = new System.Text.StringBuilder();
            foreach (var s in Ranked(false))
            {
                if (sb.Length > 0) sb.Append(" / ");
                sb.Append(s.Display);
            }
            return sb.ToString();
        }
    }
}
