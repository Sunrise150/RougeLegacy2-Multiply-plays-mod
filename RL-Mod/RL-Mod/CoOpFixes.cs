using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 暂停菜单期间"打不掉血"的修复（v2.2.1）。
    ///
    /// 游戏原生逻辑：PlayerHitResponse.IsInvincible 除了真实无敌帧之外，还会在
    /// "Game 输入映射被禁用" 时返回 true —— 这是单机给暂停菜单的免伤保护。
    ///
    /// 但联机时 PauseCompat 故意拦掉了"打开菜单就冻结世界"，所以按 ESC 后怪物还在动、
    /// 还在攻击；而玩家被这条保护判定成无敌 → 不掉血、没伤害数字、没屏幕反馈。
    /// 同时 ProjectileManager 里也有 if (playerController.IsInvincible) return，
    /// 所以那段时间连自己射出去的弹体都会被吞。
    ///
    /// 修法：只在联机会话里，把这条属性改成"只看真实无敌帧计时器"，
    /// 和基类 BaseCharacterHitResponse.IsInvincible 完全一致。单机流程原样不动。
    /// </summary>
    internal static class PauseDamageFix
    {
        private static FieldInfo _invincibleTimer;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var getter = AccessTools.Method(typeof(PlayerHitResponse), "get_IsInvincible");
                _invincibleTimer = AccessTools.Field(typeof(BaseCharacterHitResponse), "m_invincibilityTimer");

                if (getter == null || _invincibleTimer == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [PAUSE] IsInvincible 补丁跳过（getter="
                        + (getter != null) + " timerField=" + (_invincibleTimer != null) + "）");
                    return;
                }

                harmony.Patch(getter, prefix: new HarmonyMethod(typeof(PauseDamageFix).GetMethod(
                    "IsInvinciblePrefix", BindingFlags.Static | BindingFlags.NonPublic)));

                Plugin.Log.LogInfo("[RL2MP] [PAUSE] 暂停菜单免伤 补丁 OK: 联机时只认真实无敌帧。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [PAUSE] 免伤补丁失败: " + ex.Message);
            }
        }

        /// <summary>
        /// 只接管"因为菜单打开而判无敌"的那部分；真实无敌帧仍然生效。
        /// 单机（非联机会话）直接返回 true，走原版逻辑。
        /// </summary>
        private static bool IsInvinciblePrefix(PlayerHitResponse __instance, ref bool __result)
        {
            if (!DownedState.InSession) return true;
            if (__instance == null || _invincibleTimer == null) return true;

            try
            {
                __result = (float)_invincibleTimer.GetValue(__instance) > 0f;
                return false;
            }
            catch
            {
                return true;
            }
        }
    }

    /// <summary>
    /// 怪物索敌目标修复（v2.2.1）。
    ///
    /// EnemyController.Target 只在 Start() 里设一次：m_target = PlayerManager.GetPlayer()。
    /// 但怪物是**对象池复用**的 —— 同一个实例被借出/归还很多次，Start() 不会再跑。
    /// 于是玩家换角色、复活、重开一局之后，池里那些怪手里的 Target 还指着
    /// 已经被销毁的旧玩家对象：TargetController 为空、ForceFaceTarget / OrientToTarget
    /// 全是空引用或原地发呆 —— 表现就是"索敌 AI 很怪"：怪不追人、对着空气转身、
    /// 站在那儿不动。
    ///
    /// 修法：每秒巡检一次，Target 变成 Unity 假 null（对象已销毁）就重新指回本地玩家。
    /// 只在 Target 为 null 时动手，不动 BOSS 那种故意把 Target 设成"房间中心"的情况。
    /// </summary>
    internal static class EnemyAiFix
    {
        private static float _tick;
        private static float _lastLog;

        // v3.0.8：权威端活体玩家表索敌（借鉴 Boss Room 的 IdleAIState.DetectFoes）。
        // 每只怪从"本地真身 + 未倒地的远端分身"里挑最近的活人当 Target，
        // 这样离客机更近的怪会主动追打客机分身（命中走 RemoteHurtbox 转发给真身）。
        private static float _retargetTick;
        private static float _lastRetargetLog;
        private const float RetargetInterval = 0.25f;  // v3.0.11：0.5s→0.25s，分身更快抢到仇恨
        // v3.0.11：4m→1m。上轮为压客机转身抽搐把它从 2.5 提到 4，副作用是分身必须近 4m 以上
        // 才能抢到仇恨，贴身走位时分身永远选不上（"怪不攻击客机"的直接原因之一）。
        // 抽搐已改由 EnemyPositionSync 的朝向迟滞 + 动画回放去重解决，这里放开让分身能被打。
        private const float SwitchAdvantage = 1f;
        private struct TargetCandidate
        {
            internal GameObject Go;
            internal Vector3 Pos;
        }
        private static readonly List<TargetCandidate> _candidates = new List<TargetCandidate>(4);

        internal static void Reset()
        {
            _tick = 0f;
            _retargetTick = 0f;
        }

        internal static void Tick()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;

            // v3.0.8：活体玩家表索敌。独立于下面每秒一次的"目标对象已销毁"巡检，
            // 0.5s 一次即可（仇恨切换有迟滞，不需要逐帧）。只在世界权威端 + 本地玩家
            // 站着时做：非权威端的怪是 Puppetize 木偶（AI/物理已停、fireEvents 已关），
            // 在它身上改 Target 既无伤害意义又会与权威快照对冲。
            _retargetTick += Time.unscaledDeltaTime;
            if (_retargetTick >= RetargetInterval)
            {
                _retargetTick = 0f;
                UnityEngine.GameObject local = null;
                try { local = PlayerManager.GetPlayer(); } catch { }
                bool localAlive = false;
                try { localAlive = local != null && local.activeInHierarchy && !DownedState.LocalDown; } catch { }
                if (localAlive && RoomAuthority.ShouldDriveWorld)
                    RetargetAuthorityPlayers(local);
            }

            _tick += Time.unscaledDeltaTime;
            if (_tick < 1f) return;
            _tick = 0f;

            try
            {
                // v2.3.1：Lineage/Town 等"还没有可控角色"的场景里，PlayerManager 内部状态
                // 可能为空，GetPlayer 会直接抛 NRE（真机日志：[AI] 目标巡检失败）。
                // 这里自己兜住，别让一条巡检把整个 LateUpdate 链路带崩。
                UnityEngine.GameObject player = null;
                try { player = PlayerManager.GetPlayer(); }
                catch { player = null; }

                bool localAlive = false;
                try { localAlive = player != null && player.activeInHierarchy; } catch { }

                // v2.4.0：本地玩家死亡/观战时，把怪物仇恨转给活着的队友。
                // 旧版这里直接 return —— 于是主机死后，主机端（权威端）的怪仍然锁定
                // 已经隐藏的本地玩家，一直围着尸体打转；客机端那只怪的本地 AI 却在追
                // 活着的客机，两边位置对冲，画面就是"围着尸体抽搐"。
                if (DownedState.LocalDown || !localAlive)
                {
                    var alt = PickAliveRemote();
                    if (alt == null) return;   // 没有活人可追，别乱改（否则怪彻底失去目标）

                    var allDown = EnemySync.GetEnemiesForPosition();
                    if (allDown == null) return;
                    int moved = 0;
                    for (int i = 0; i < allDown.Length; i++)
                    {
                        var e = allDown[i];
                        if (e == null || e.IsDead) continue;
                        try
                        {
                            // 只改"原本锁定本地玩家"的怪，不碰 BOSS 那种把 Target 设成房间中心的
                            if (e.Target == null || (player != null && e.Target == player))
                            {
                                e.Target = alt;
                                moved++;
                            }
                        }
                        catch { }
                    }
                    if (moved > 0 && Time.unscaledTime - _lastLog > 5f)
                    {
                        _lastLog = Time.unscaledTime;
                        Plugin.Log.LogInfo("[RL2MP] [AI] 本地玩家已倒下，把 " + moved
                            + " 只怪的仇恨转给队友 '" + alt.name + "'。");
                    }
                    return;
                }

                var all = EnemySync.GetEnemiesForPosition();
                if (all == null) return;

                int fixedCount = 0;
                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];
                    if (e == null || e.IsDead) continue;
                    if (e.Target == null)   // Unity 假 null = 目标对象已被销毁
                    {
                        try { e.Target = player; fixedCount++; }
                        catch { }
                    }
                }

                if (fixedCount > 0 && Time.unscaledTime - _lastLog > 5f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [AI] 修复 " + fixedCount
                        + " 只怪的目标（原来指向已销毁的旧玩家对象）。");
                }
            }
            catch (Exception ex)
            {
                if (Time.unscaledTime - _lastLog > 5f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogWarning("[RL2MP] [AI] 目标巡检失败: " + ex.Message);
                }
            }
        }

        /// <summary>
        /// v3.0.8：权威端活体玩家表索敌。
        ///
        /// 单机原版里怪物只在 Start() 锁一次 PlayerManager.GetPlayer()，永远只认本地真身。
        /// 联机后权威端（通常是主机）的怪因此只追主机、只朝主机出招，客机分身哪怕站在
        /// 怪旁边也不会被主动攻击——除非碰巧蹭进 hitbox。Boss Room 的 IdleAIState 做法是
        /// 遍历服务端维护的活体玩家表、取最近的目标。这里照做：
        ///   候选 = 本地真身（活着时） + 每个未倒地、active 的远端分身；
        ///   每只活怪选水平距离最近的候选当 Target，带 2.5m 迟滞防来回横跳。
        /// 分身 root 上已挂 RemoteTargetProxy（提供有效 Midpoint），命中由 RemoteHurtbox
        /// 转发给真身，所以"怪打分身"和"怪打真身"走的是同一套伤害链路。
        ///
        /// 只改 Target（AI 追谁），绝不碰 transform/位置/动画——那些由 EnemyPositionSync
        /// 的权威快照独立驱动，两边职责分开，避免动到已调稳的位置同步。
        /// BOSS 那种把 Target 设成房间中心的怪也要跳过：只处理 Target 是本地真身或某个
        /// 远端分身的怪，其余一律不碰。
        /// </summary>
        private static void RetargetAuthorityPlayers(UnityEngine.GameObject localPlayer)
        {
            var ps = PlayerSync.Instance;
            if (ps == null) return;

            // 1) 收集活体候选
            _candidates.Clear();
            if (localPlayer != null && localPlayer.activeInHierarchy)
                _candidates.Add(new TargetCandidate { Go = localPlayer, Pos = localPlayer.transform.position });
            if (ps.RemotePlayers != null)
            {
                foreach (var kv in ps.RemotePlayers)
                {
                    if (ps.RemoteDowned.Contains(kv.Key)) continue;   // 倒地的不作为仇恨目标
                    var go = kv.Value;
                    if (go == null) continue;
                    try { if (!go.activeInHierarchy) continue; } catch { continue; }
                    _candidates.Add(new TargetCandidate { Go = go, Pos = go.transform.position });
                }
            }
            if (_candidates.Count <= 1) return;   // 只有一个活人：保持原样（原生就锁本地真身）

            var all = EnemySync.GetEnemiesForPosition();
            if (all == null) return;

            int changed = 0;
            for (int i = 0; i < all.Length; i++)
            {
                var e = all[i];
                if (e == null || e.IsDead) continue;
                try
                {
                    var cur = e.Target;
                    // 只处理"锁着某个玩家"的怪。BOSS 把 Target 设成房间中心/其他对象的不碰。
                    if (cur != null && !IsPlayerTarget(cur)) continue;

                    Vector3 ep = e.transform.position;
                    // 当前目标的距离（线性米）。为空/已销毁记为无穷远。
                    float curDist = float.MaxValue;
                    if (cur != null) curDist = Mathf.Sqrt(FlatDistSqr(ep, cur.transform.position));

                    // 找最近的候选
                    int best = -1;
                    float bestDist = float.MaxValue;
                    for (int c = 0; c < _candidates.Count; c++)
                    {
                        float dist = Mathf.Sqrt(FlatDistSqr(ep, _candidates[c].Pos));
                        if (dist < bestDist) { bestDist = dist; best = c; }
                    }
                    if (best < 0) continue;
                    // 迟滞：新目标必须比当前近 SwitchAdvantage 米以上才切换，
                    // 避免两个玩家站位相近时怪每 0.5s 来回横跳。
                    if (cur != null && curDist <= bestDist + SwitchAdvantage) continue;
                    if (cur == _candidates[best].Go) continue;

                    e.Target = _candidates[best].Go;   // setter 会用 GetComponent 取到代理的 BaseCharacterController
                    changed++;
                }
                catch { /* 单只怪异常不影响其他怪 */ }
            }

            if (changed > 0 && Time.unscaledTime - _lastRetargetLog > 6f)
            {
                _lastRetargetLog = Time.unscaledTime;
                Plugin.Log.LogInfo("[RL2MP] [AI] 权威端活体玩家表：把 " + changed
                    + " 只怪的仇恨指向最近的活人（候选 " + _candidates.Count + " 个）。");
            }
        }

        /// <summary>当前 Target 是否是我们的人（本地真身或某个远端分身）。</summary>
        private static bool IsPlayerTarget(UnityEngine.GameObject t)
        {
            if (t == null) return false;
            for (int c = 0; c < _candidates.Count; c++)
                if (_candidates[c].Go == t) return true;
            return false;
        }

        /// <summary>只比 x/y（2D 横版），忽略 z。</summary>
        private static float FlatDistSqr(Vector3 a, Vector3 b)
        {
            float dx = a.x - b.x, dy = a.y - b.y;
            return dx * dx + dy * dy;
        }

        /// <summary>挑一个还活着的远程玩家分身（用于本地玩家倒地后接管怪物仇恨）。</summary>
        private static UnityEngine.GameObject PickAliveRemote()
        {
            try
            {
                var ps = PlayerSync.Instance;
                if (ps == null || ps.RemotePlayers == null) return null;
                foreach (var kv in ps.RemotePlayers)
                {
                    var go = kv.Value;
                    if (go == null) continue;
                    if (ps.RemoteDowned.Contains(kv.Key)) continue;
                    try { if (!go.activeInHierarchy) continue; } catch { continue; }
                    return go;
                }
            }
            catch { }
            return null;
        }

        // ---------------- v3.0.11：怪物攻击距离判定 patch ----------------

        private static FieldInfo _lcEnemyField;

        /// <summary>
        /// 反编译铁证：LogicController.IsInRange_Standard 判定 Close/Medium/Far 时，
        /// 用 m_rangeRect.Contains(PlayerManager.GetPlayerController().Midpoint) —— **只读本地真身**。
        /// 于是就算 Target 已被改指向客机分身，只要主机真身离得远，怪就进 Wander 根本不出招
        /// （"怪不攻击客机"的头号根因，光改 Target 无效）。
        ///
        /// 修法：联机会话里把判定改成"任一活玩家（真身 + 未倒地分身）在 rangeRect 内"。
        /// 单机/非会话完全走原版。只改判定，不碰 Target/位置/动画。
        /// </summary>
        internal static void Apply(Harmony harmony)
        {
            try
            {
                var method = AccessTools.Method(typeof(LogicController), "IsInRange_Standard");
                _lcEnemyField = AccessTools.Field(typeof(LogicController), "m_enemyController");
                if (method == null || _lcEnemyField == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [AI] IsInRange patch 跳过（method="
                        + (method != null) + " enemyField=" + (_lcEnemyField != null) + "）");
                    return;
                }
                harmony.Patch(method, prefix: new HarmonyMethod(
                    typeof(EnemyAiFix).GetMethod("IsInRangePrefix", BindingFlags.Static | BindingFlags.NonPublic)));
                Plugin.Log.LogInfo("[RL2MP] [AI] IsInRange patch OK：攻击距离按所有活玩家判定。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [AI] IsInRange patch 失败: " + ex.Message);
            }
        }

        private static bool IsInRangePrefix(LogicController __instance, float rangeRadius, ref bool __result)
        {
            // 非联机会话：交给原版
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected || Score.Style != PlayStyle.CoOp
                || !DownedState.InSession)
                return true;

            try
            {
                var enemy = _lcEnemyField.GetValue(__instance) as EnemyController;
                if (enemy == null) return true;
                Vector3 mid = enemy.Midpoint;
                float halfH = rangeRadius * 0.6f;
                var rect = new Rect(mid.x - rangeRadius, mid.y - halfH, rangeRadius * 2f, halfH * 2f);

                // 1) 本地真身（活着时）
                var ps = PlayerSync.Instance;
                var localPc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (localPc != null && !DownedState.LocalDown)
                {
                    try { if (rect.Contains((Vector2)localPc.Midpoint)) { __result = true; return false; } }
                    catch { }
                }

                // 2) 未倒地、active 的远端分身（它们的 hurtbox 能把伤害转发给真身）
                if (ps != null && ps.RemotePlayers != null)
                {
                    foreach (var kv in ps.RemotePlayers)
                    {
                        if (ps.RemoteDowned.Contains(kv.Key)) continue;
                        var go = kv.Value;
                        if (go == null) continue;
                        try
                        {
                            if (!go.activeInHierarchy) continue;
                            if (rect.Contains((Vector2)go.transform.position)) { __result = true; return false; }
                        }
                        catch { }
                    }
                }

                __result = false;
                return false;
            }
            catch
            {
                return true;   // 任何异常退回原版，绝不破坏单机行为
            }
        }
    }
}
