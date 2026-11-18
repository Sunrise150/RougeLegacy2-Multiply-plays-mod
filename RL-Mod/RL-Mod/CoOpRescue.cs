using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.7.8 协作专属行为：留尸体 + 长按 H 救援 + 血上限惩罚。
    ///
    /// 只在 Score.Style == PlayStyle.CoOp 时生效；竞速模式行为完全不变。
    /// 规则（参数集中在本类顶部常量，方便调）：
    ///   1) 协作模式死亡：不立即复活，角色留在原地"倒地待救"（复用 DownedState.LocalDown
    ///      的隐藏/观战链路，但不走竞速的即时复活与加时）。
    ///   2) 倒地瞬间血上限永久 -1/20（惩罚栈 +1）；队友长按 H 救起 -> 惩罚栈 -1（恢复上限）。
    ///   3) 救援窗口：房间内无怪且队友在房间内时给 60 秒自救倒计时；超时自行爬起，
    ///      惩罚保留（不减栈）——这就是"逼队友来救"的动机。
    ///   4) 房间有怪时不启动自救倒计时，但队友仍可强行救援；另设绝对兜底防永久卡死。
    ///
    /// 技术要点：
    ///   - 血上限用 Harmony postfix 补 PlayerController.get_ActualMaxHealth，按惩罚栈动态扣减。
    ///     MaxHealthMod 是 protected set 且 InitializeHealthMods 会清零，直接改字段不可靠。
    ///   - 救援请求走网络消息，避免"救人者本地起身、被救者浑然不知"的分裂。
    /// </summary>
    internal static class CoOpRescue
    {
        // ---------------- 可调参数 ----------------
        internal const float HealthPenaltyPerDown = 0.05f;   // 每次倒地扣 1/20 血上限
        internal const int MaxPenaltyStacks = 12;             // 上限保护：最多扣 60%，血下限 40%
        internal const float RescueHoldSeconds = 2.0f;        // 长按 H 多久算救起
        internal const float RescueRange = 5.0f;              // 救人者与尸体的最大距离（世界单位，v1.7.9 从 3 放宽：3 太小真机够不到）
        internal const float SafeWindowSeconds = 60f;         // 无怪+队友在房 -> 60 秒后自救
        internal const float AbsoluteTimeoutSeconds = 210f;   // 绝对兜底：3.5 分钟必起身，防卡死
        internal const float SameRoomRange = 24f;             // 判定"队友在同一房间"的距离阈值

        // ---------------- 状态（全静态，不依赖 GameObject 存活）----------------

        /// <summary>当前血上限惩罚层数（倒地 +1，被队友救起 -1）。</summary>
        internal static int HealthPenaltyStacks;

        /// <summary>本地玩家是否处于"协作倒地待救"（区别于竞速观战的 DownedState.LocalDown）。</summary>
        internal static bool LocalDown;

        /// <summary>倒地瞬间的位置（尸体停留点）。</summary>
        internal static Vector3 CorpsePos;

        /// <summary>当前救援输入进度（0=没按 / 1=救起），仅用于 HUD 显示。</summary>
        internal static float RescueProgress;

        /// <summary>本地正在施救的目标玩家 ID（-1 无）。</summary>
        internal static int RescueTargetId = -1;

        /// <summary>上次已打日志的救援目标，避免"发现目标"日志每帧刷屏。</summary>
        private static int _lastLoggedRescueTarget = -1;

        /// <summary>自救倒计时是否已满足"安全"条件、已累计的秒数。</summary>
        private static float _safeElapsed;

        /// <summary>本次倒地的绝对计时（unscaled）。</summary>
        private static float _downElapsed;

        /// <summary>会话丢失（断网/主机结束对局）后的起身计时，防永久躺尸。</summary>
        private static float _lostSessionElapsed;

        /// <summary>房间/队友条件复查节流。</summary>
        private static float _checkTimer;

        /// <summary>上次判定到的安全条件结果（给 HUD 用）。</summary>
        internal static bool SafeCondition;

        /// <summary>当前是否处于协作玩法且联机对局中。</summary>
        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp && DownedState.InSession; }
        }

        /// <summary>血上限惩罚比例（0~0.6）。</summary>
        internal static float PenaltyRatio
        {
            get { return Mathf.Min(HealthPenaltyStacks, MaxPenaltyStacks) * HealthPenaltyPerDown; }
        }

        // ---------------- 血上限补丁 ----------------

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var m = AccessTools.Method(typeof(PlayerController), "get_ActualMaxHealth");
                if (m == null)
                    m = AccessTools.Property(typeof(PlayerController), "ActualMaxHealth").GetGetMethod(true);
                if (m == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [COOP] ActualMaxHealth getter not found; health penalty disabled.");
                    return;
                }
                var post = new HarmonyMethod(typeof(CoOpRescue).GetMethod("ActualMaxHealthPostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(m, postfix: post);
                Plugin.Log.LogInfo("[RL2MP] [COOP] Patched PlayerController.ActualMaxHealth (health penalty).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [COOP] health patch failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>倒地惩罚直接作用在 ActualMaxHealth 的计算结果上，任何时机重算都生效。</summary>
        private static void ActualMaxHealthPostfix(ref int __result)
        {
            if (Score.Style != PlayStyle.CoOp) return;
            int stacks = Mathf.Min(HealthPenaltyStacks, MaxPenaltyStacks);
            if (stacks <= 0) return;
            float mult = 1f - stacks * HealthPenaltyPerDown;
            if (mult < 0.4f) mult = 0.4f;
            __result = Mathf.Max(1, Mathf.CeilToInt(__result * mult));
        }

        // ---------------- 倒地 / 起身 ----------------

        /// <summary>
        /// 协作模式死亡：交给 DownedState.GoDown 走隐藏角色 + 观战链路（重建对象也丢不了状态），
        /// 但不做竞速的即时复活/加时。这里额外施加血上限惩罚并记录尸体位置。
        /// </summary>
        internal static void OnLocalDown(string reason)
        {
            if (LocalDown) return;
            LocalDown = true;
            _safeElapsed = 0f;
            _downElapsed = 0f;
            _lostSessionElapsed = 0f;
            _checkTimer = 0f;
            RescueProgress = 0f;
            RescueTargetId = -1;
            SafeCondition = false;

            try
            {
                var ps = PlayerSync.Instance;
                var go = ps != null ? ps.CachedLocalPlayerObject : null;
                CorpsePos = DownedState.DeathPos != Vector3.zero && go != null
                    ? DownedState.DeathPos
                    : (go != null ? go.transform.position : Vector3.zero);
            }
            catch { }

            // 复用现有倒地链路：隐藏角色、停物理、广播 downed=true 给队友（尸体可见）、清仇恨。
            // GoDown 内部会设 DownedState.LocalDown=true 并 SetActive(false)。
            DownedState.GoDown(reason + " [coop]");

            // GoDown 会把镜头切到观战队友；协作倒地应留在自己的尸体附近等队友跑来救，
            // 所以把镜头切回本地角色 transform（对象虽 SetActive(false)，transform 仍有效）。
            try
            {
                var ps2 = PlayerSync.Instance;
                var go2 = ps2 != null ? ps2.CachedLocalPlayerObject : null;
                if (go2 != null) DownedState.Follow(go2.transform);
            }
            catch { }

            HealthPenaltyStacks = Mathf.Min(HealthPenaltyStacks + 1, MaxPenaltyStacks);
            Plugin.Log.LogInfo("[RL2MP] [COOP] 本地倒地待救 (" + reason + ") at ("
                + CorpsePos.x.ToString("F1") + "," + CorpsePos.y.ToString("F1")
                + ")，血上限惩罚 -> -" + (PenaltyRatio * 100f).ToString("F0") + "% ("
                + HealthPenaltyStacks + " 层)。");
        }

        /// <summary>
        /// 起身：被队友救起（restorePenalty=true，回一层血上限）或自救/兜底（惩罚保留）。
        /// 复用 DownedState.Revive 完成回满血 + 复位状态 + 原位复活 + 房间修复 + 镜头回本地。
        /// </summary>
        internal static void StandUp(bool restorePenalty, string reason)
        {
            if (!LocalDown) return;
            LocalDown = false;
            RescueProgress = 0f;
            RescueTargetId = -1;

            if (restorePenalty && HealthPenaltyStacks > 0)
            {
                HealthPenaltyStacks--;
                Plugin.Log.LogInfo("[RL2MP] [COOP] 被队友救起，血上限恢复一层 -> -"
                    + (PenaltyRatio * 100f).ToString("F0") + "% (" + HealthPenaltyStacks + " 层)。");
            }

            // Revive 要求 DownedState.LocalDown==true 才执行；协作倒地时它本来就是 true。
            DownedState.Revive("coop:" + reason);

            Plugin.Log.LogInfo("[RL2MP] [COOP] 起身 (" + reason + ")，惩罚"
                + (restorePenalty ? "已恢复一层" : "保留") + "。");
        }

        // ---------------- 每帧驱动（由 DownedManager.Update 调用）----------------

        internal static void Tick()
        {
            if (!LocalDown)
            {
                if (Active) TickRescuer();
                return;
            }

            // 会话丢失（断网/主机结束对局）：2 秒后自行爬起，惩罚保留。
            // 注意不能靠 DownedState.Tick 的断网兜底——协作倒地已被我们在那边 early-return。
            if (!DownedState.InSession)
            {
                _lostSessionElapsed += Time.unscaledDeltaTime;
                if (_lostSessionElapsed >= 2f)
                    StandUp(false, "network lost");
                return;
            }
            _lostSessionElapsed = 0f;

            TickDowned();
        }

        /// <summary>倒地者：维持尸体 + 计时救援窗口 / 绝对兜底，到点自行爬起。</summary>
        private static void TickDowned()
        {
            _downElapsed += Time.unscaledDeltaTime;

            // 绝对兜底：无论什么情况，到点必起身，惩罚保留。防止永远卡在观战。
            if (_downElapsed >= AbsoluteTimeoutSeconds)
            {
                StandUp(false, "绝对兜底超时");
                return;
            }

            _checkTimer += Time.unscaledDeltaTime;
            if (_checkTimer >= 0.25f)
            {
                _checkTimer = 0f;
                SafeCondition = IsSafeWindow();
            }

            if (SafeCondition)
            {
                _safeElapsed += Time.unscaledDeltaTime;
                if (_safeElapsed >= SafeWindowSeconds)
                {
                    StandUp(false, "安全救援窗口超时（惩罚保留）");
                    return;
                }
            }
            else
            {
                // 有怪 / 队友不在房间：不累计自救倒计时
                _safeElapsed = 0f;
            }
        }

        /// <summary>
        /// 安全救援窗口条件：当前房间内没有活着的敌人，且至少一名队友在同一房间/附近。
        /// 房间有怪时不启动自救（逼队友冒险来救或强行救），但救援输入不受此限制。
        /// </summary>
        private static bool IsSafeWindow()
        {
            try
            {
                var ps = PlayerSync.Instance;
                if (ps == null) return false;

                // 队友在附近（非倒地、非自己）
                bool teammateNear = false;
                foreach (var kv in ps.RemotePlayers)
                {
                    if (kv.Value == null) continue;
                    if (ps.RemoteDowned.Contains(kv.Key)) continue;
                    float d = Vector2.Distance(
                        new Vector2(kv.Value.transform.position.x, kv.Value.transform.position.y),
                        new Vector2(CorpsePos.x, CorpsePos.y));
                    if (d <= SameRoomRange) { teammateNear = true; break; }
                }
                if (!teammateNear) return false;

                // 当前房间无活敌
                var enemies = UnityEngine.Object.FindObjectsOfType<EnemyController>();
                for (int i = 0; i < enemies.Length; i++)
                {
                    var e = enemies[i];
                    if (e == null) continue;
                    if (e.IsDead) continue;
                    float d = Vector2.Distance(
                        new Vector2(e.transform.position.x, e.transform.position.y),
                        new Vector2(CorpsePos.x, CorpsePos.y));
                    if (d <= SameRoomRange) return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _checkTimer > 5f)
                    Plugin.Log.LogWarning("[RL2MP] [COOP] IsSafeWindow failed: " + ex.Message);
                return false;
            }
        }

        /// <summary>救人者：找附近倒地队友，长按 H 累计进度，满则发救援请求。</summary>
        private static void TickRescuer()
        {
            var ps = PlayerSync.Instance;
            if (ps == null || ps.LocalPlayer == null)
            {
                RescueProgress = 0f;
                RescueTargetId = -1;
                return;
            }

            Vector3 myPos = ps.LocalPlayer.transform.position;
            int target = -1;
            float best = RescueRange;

            foreach (var kv in ps.RemotePlayers)
            {
                if (kv.Value == null) continue;
                if (!ps.RemoteDowned.Contains(kv.Key)) continue;
                float d = Vector2.Distance(
                    new Vector2(kv.Value.transform.position.x, kv.Value.transform.position.y),
                    new Vector2(myPos.x, myPos.y));
                if (d < best)
                {
                    best = d;
                    target = kv.Key;
                }
            }

            if (target < 0)
            {
                RescueProgress = 0f;
                RescueTargetId = -1;
                _lastLoggedRescueTarget = -1;
                return;
            }

            RescueTargetId = target;
            if (target != _lastLoggedRescueTarget)
            {
                _lastLoggedRescueTarget = target;
                if (target >= 0)
                    Plugin.Log.LogInfo("[RL2MP] [COOP] 发现可救援目标 id=" + target
                        + " 距离=" + best.ToString("F1") + "，长按 H " + RescueHoldSeconds + " 秒救起。");
            }
            bool holding = Input.GetKey(KeyCode.H);
            if (holding)
            {
                float prev = RescueProgress;
                RescueProgress += Time.unscaledDeltaTime / RescueHoldSeconds;
                // 首次按住时打一条日志，真机可确认"按键 -> 目标 -> 进度"链路已通
                if (prev <= 0f)
                    Plugin.Log.LogInfo("[RL2MP] [COOP] 开始长按 H 救援 target=" + target
                        + " 距离=" + best.ToString("F1") + "，持续 " + RescueHoldSeconds + "s。");
                if (RescueProgress >= 1f)
                {
                    RescueProgress = 0f;
                    var nm = NetworkManager.Instance;
                    if (nm != null) nm.SendCoOpRescue(target);
                    else Plugin.Log.LogWarning("[RL2MP] [COOP] 救援完成但无 NetworkManager，无法上报。");
                }
            }
            else
            {
                // 松开 H 重置进度（必须持续长按）
                RescueProgress = 0f;
            }
        }

        /// <summary>收到"你被救起"的消息：只有本地确实处于倒地状态才起身。</summary>
        internal static void OnRescueReceived(int targetId)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || targetId != nm.LocalPlayerId) return;
            if (!LocalDown) return;
            StandUp(true, "队友长按H救援");
        }

        /// <summary>队友被救起后，其他端把它的 downed 标志清掉（尸体恢复为活人）。</summary>
        internal static void OnRescueBroadcast(int targetId)
        {
            var ps = PlayerSync.Instance;
            if (ps == null) return;
            if (targetId == (NetworkManager.Instance != null ? NetworkManager.Instance.LocalPlayerId : -99)) return;
            ps.SetRemoteDowned(targetId, false);
        }

        /// <summary>自救倒计时剩余秒数（HUD 用）；未处于安全窗口返回 -1。</summary>
        internal static float SelfRescueRemaining
        {
            get { return SafeWindowSeconds - _safeElapsed; }
        }

        // ---------------- 重置 ----------------

        internal static void ResetAll(string reason)
        {
            LocalDown = false;
            HealthPenaltyStacks = 0;
            RescueProgress = 0f;
            RescueTargetId = -1;
            _lastLoggedRescueTarget = -1;
            _safeElapsed = 0f;
            _downElapsed = 0f;
            _lostSessionElapsed = 0f;
            _checkTimer = 0f;
            SafeCondition = false;
            CorpsePos = Vector3.zero;
            Plugin.Log.LogInfo("[RL2MP] [COOP] state reset (" + reason + ").");
        }
    }
}
