using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.4.0：协作模式死亡流程 = **观战**（扶起机制已按用户要求删除）。
    ///
    /// 旧版（v1.7.8~v2.3.2）是"倒地待救 + 长按 H 救援 + 血上限惩罚"，
    /// 真机上问题很多：尸体可能停在半空够不到、怪物围着尸体把救人的人打死、
    /// 两端对"谁倒地了"的认知还会分叉。用户明确要求删掉扶起，改成观战。
    ///
    /// 新规则：
    ///   1) 死亡 -> 立刻进入观战（镜头跟队友，可 [ ] 切换，能看到队友和怪物）。
    ///   2) 观战期间本地角色隐藏、不参与任何同步写入。
    ///   3) 队友也死光 -> 结束本局，两端各自走原生死亡流程回城。
    ///   4) 断线 -> 2 秒后自动退出观战（否则永远躺尸）。
    ///
    /// 对外仍保留 RescueProgress / RescueTargetId / SafeCondition / SelfRescueRemaining /
    /// PenaltyRatio 这几个只读成员（值恒为"无救援"），避免 HUD 代码大改。
    /// </summary>
    internal static class CoOpRescue
    {
        // ---------------- 状态 ----------------

        /// <summary>本地玩家正在观战（协作模式死亡后）。</summary>
        internal static bool LocalDown;

        /// <summary>死亡瞬间的位置（仅用于日志/尸体摆放参考）。</summary>
        internal static Vector3 CorpsePos;

        /// <summary>v2.0.3：全员倒下持续计时，确认后结束本局。</summary>
        private static float _allDownElapsed;

        /// <summary>
        /// v2.8.8：本局已确认全员倒下（本地判定或收到 RunOver）。置位后即使对端
        /// 先走真死广播了 downed=0，也不再把全灭计时清零 —— 否则一端卡在观战不回城。
        /// </summary>
        private static bool _runOverLatched;

        /// <summary>会话丢失（断网/主机结束对局）后的退出观战计时。</summary>
        private static float _lostSessionElapsed;

        /// <summary>HUD 兼容：已无救援机制，恒为 0/-1/false。</summary>
        internal static float RescueProgress { get { return 0f; } }
        internal static int RescueTargetId { get { return -1; } }
        internal static bool SafeCondition { get { return false; } }
        internal static float SelfRescueRemaining { get { return -1f; } }
        internal static float PenaltyRatio { get { return 0f; } }
        internal static int HealthPenaltyStacks;

        /// <summary>当前是否处于协作玩法且联机对局中。</summary>
        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp && DownedState.InSession; }
        }

        /// <summary>兼容占位：旧版在这里补 ActualMaxHealth（血上限惩罚），现已取消。</summary>
        internal static void Apply(Harmony harmony)
        {
            if (Score.Style == PlayStyle.CoOp)
                Plugin.Log.LogInfo("[RL2MP] [COOP] 协作模式：扶起/救援机制已移除，死亡直接进入观战。");
        }

        // ---------------- 死亡 -> 观战 ----------------

        internal static void OnLocalDown(string reason)
        {
            if (LocalDown) return;
            LocalDown = true;
            _allDownElapsed = 0f;
            _lostSessionElapsed = 0f;

            try
            {
                var ps = PlayerSync.Instance;
                var go = ps != null ? ps.CachedLocalPlayerObject : null;
                CorpsePos = DownedState.DeathPos != Vector3.zero && go != null
                    ? DownedState.DeathPos
                    : (go != null ? go.transform.position : Vector3.zero);
            }
            catch { }

            // 复用观战链路：隐藏角色、停物理、广播 downed、清仇恨、镜头切到队友。
            DownedState.GoDown(reason + " [coop]");

            Plugin.Log.LogInfo("[RL2MP] [COOP] 本地死亡 -> 进入观战 (" + reason + ") at ("
                + CorpsePos.x.ToString("F1") + "," + CorpsePos.y.ToString("F1")
                + ")，队友存活 " + DownedState.AliveRemoteCount + "/" + DownedState.RemoteCount
                + "。按 [ ] 切换观战目标。");
        }

        /// <summary>退出观战，让本地角色重新站起来（断线兜底 / 调试用）。</summary>
        internal static void StandUp(bool restorePenalty, string reason)
        {
            if (!LocalDown) return;
            LocalDown = false;
            _allDownElapsed = 0f;
            _lostSessionElapsed = 0f;

            // Revive 要求 DownedState.LocalDown==true 才会执行；观战期间它一直是 true。
            DownedState.Revive("coop:" + reason);
            Plugin.Log.LogInfo("[RL2MP] [COOP] 退出观战 (" + reason + ")。");
        }

        // ---------------- 每帧驱动 ----------------

        internal static void Tick()
        {
            if (!LocalDown) return;

            // 会话丢失（断网 / 主机结束对局）：2 秒后退出观战，
            // 否则镜头跟着一个不存在的分身，画面永久卡住。
            if (!DownedState.InSession)
            {
                _lostSessionElapsed += Time.unscaledDeltaTime;
                if (_lostSessionElapsed >= 2f)
                    StandUp(false, "network lost");
                return;
            }
            _lostSessionElapsed = 0f;

            CheckAllDown();
        }

        /// <summary>
        /// 全员倒下（本地观战中 + 所有远端都不活着）-> 结束本局。
        /// 两端各自放行真实死亡，走游戏原生 死亡窗口 -> Lineage -> Town 流程。
        /// 持续 1.5s 确认，避免状态包抖动导致误判。
        ///
        /// v2.4.1：判定结果要**广播**（RunOver）。旧版是两端各自判：
        /// 一旦一端的 downed 状态没收到（这次就是），另一端还在观战、这一端已经真死，
        /// 两边对"这局结束了没有"的认知完全错开，最后卡死。
        /// </summary>
        private static void CheckAllDown()
        {
            // v2.8.8：已闭锁的全灭不再被状态抖动撤销 —— 一端先真死后广播 downed=0，
            // 旧版会把这里的计时清回 0，另一端就永远等不到 1.5s，卡在观战不回城。
            if (_runOverLatched) return;

            if (DownedState.RemoteCount <= 0 || DownedState.AliveRemoteCount > 0)
            {
                _allDownElapsed = 0f;
                return;
            }

            _allDownElapsed += Time.unscaledDeltaTime;
            if (_allDownElapsed < 1.5f) return;
            _allDownElapsed = 0f;

            Plugin.Log.LogInfo("[RL2MP] [COOP] 全员倒下 -> 结束本局（放行真实死亡，两端一起回城）。");
            _runOverLatched = true;
            LocalDown = false;

            // 先叫停对端，再处理自己：对端收到后也会走同一套原生死亡
            try
            {
                var nm = NetworkManager.Instance;
                if (nm != null) nm.SendRunOver("all down at local");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [COOP] 广播 RunOver 失败: " + ex.Message);
            }

            DownedState.TriggerRealDeath();
        }

        // ---------------- 兼容占位（协议仍在，但不再有救援语义）----------------

        internal static void OnRescueReceived(int targetId) { }
        internal static void OnRescueBroadcast(int targetId) { }

        /// <summary>收到对端 RunOver 时闭锁本端全灭判定，避免被反弹的 downed=0 撤销。</summary>
        internal static void OnRunOver()
        {
            _runOverLatched = true;
        }

        internal static void ResetAll(string reason)
        {
            LocalDown = false;
            HealthPenaltyStacks = 0;
            _allDownElapsed = 0f;
            _lostSessionElapsed = 0f;
            _runOverLatched = false;
            CorpsePos = Vector3.zero;
            Plugin.Log.LogInfo("[RL2MP] [COOP] state reset (" + reason + ").");
        }
    }
}
