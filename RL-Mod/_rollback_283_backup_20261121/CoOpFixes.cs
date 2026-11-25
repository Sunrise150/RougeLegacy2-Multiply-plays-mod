using System;
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

        internal static void Reset()
        {
            _tick = 0f;
        }

        internal static void Tick()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;

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
    }
}
