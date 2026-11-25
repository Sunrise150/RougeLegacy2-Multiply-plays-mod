using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LiteNetLib.Utils;
using MoreMountains.CorgiEngine;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 虚冲/冲刺特效同步（v1.8.4）。
    ///
    /// 问题：虚冲残影靠 Animator 的 Dashing_Effect (Trigger) 驱动动画状态里的
    /// EffectTriggerAnimBehaviour 播放。但：
    ///   1) Trigger 是 Unity 不暴露读取的参数，网络动画同步只传 float/int/bool + 层状态 scrub，
    ///      Trigger 根本传不过去 → 分身不会进入 dash 特效状态。
    ///   2) 即便进入，分身 Animator 被 PlayerSync 加进 EffectManager 全局特效禁用列表
    ///      （当初为防分身攻击二次伤害），EffectTriggerAnimBehaviour.OnStateEnter 里
    ///      AnimatorEffectsDisabled(animator)==true 直接 return → 残影被吞。
    ///   3) EffectManager.PlayEffect 生成的是动态特效对象，MirrorFx 只镜像分身自带粒子，覆盖不到。
    ///
    /// 方案：专用冲刺特效事件（事件型，ReliableOrdered，必定到达）。
    ///   - 发送端 patch CharacterDash_RL.StartDash postfix：本地玩家真的冲刺了就广播 ownerId。
    ///   - 接收端：给分身所有 Animator 补 SetTrigger("Dashing_Effect") + SetBool("VoidDash")，
    ///     并把这些 Animator 加入"限时特效豁免集合"（约 1.2s，覆盖 dash 状态与残影窗口）。
    ///   - patch EffectManager.AnimatorEffectsDisabled prefix：豁免集合内的 Animator 返回 false
    ///     （放行特效）；过期自动移除。窗口过后分身恢复禁用，攻击二次伤害防护不受影响。
    ///
    /// 只在联机且协作/竞速都生效（纯视觉，不改变任何伤害/判定）。
    /// </summary>
    internal static class DashFxSync
    {
        /// <summary>分身 Animator -> 豁免到期时刻(unscaledTime)。</summary>
        private static readonly Dictionary<Animator, float> _grace = new Dictionary<Animator, float>();

        /// <summary>一次冲刺特效放行窗口（覆盖 dash 状态进入 + 残影发射）。</summary>
        private const float GraceSeconds = 1.2f;

        private static float _lastLog;
        /// <summary>
        /// v3.0.13：冲刺发送端去重。真机日志一次虚冲连发 10+ 次 [DASHFX]（InitiateDash
        /// 在一帧内被状态机多次调用），每次都 SetTrigger/续期豁免/发 SpellFx →
        /// 分身动画状态被反复重启=卡顿瞬移，特效被叠成十几份。窗口内只发一次。
        /// </summary>
        private static float _lastDashSent;
        private const float DashDedupWindow = 0.4f;

        internal static bool Active
        {
            get
            {
                var nm = NetworkManager.Instance;
                return nm != null && nm.Connected;
            }
        }

        internal static void Apply(Harmony harmony)
        {
            TryPatch(harmony,
                AccessTools.Method(typeof(CharacterDash_RL), "InitiateDash"),
                null, "冲刺事件发送", postfixName: "InitiateDashPostfix");

            // prefix 拦截 AnimatorEffectsDisabled：豁免集合内返回 false
            var target = AccessTools.Method(typeof(EffectManager), "AnimatorEffectsDisabled");
            TryPatch(harmony, target, "AnimatorEffectsDisabledPrefix", "冲刺特效豁免");
        }

        // ---------------- 发送端 ----------------

        /// <summary>InitiateDash postfix：本地玩家发起冲刺 → 广播特效事件。
        /// 用 InitiateDash 而非 StartDash：StartDash 的 IsDashing 此刻因协程尚未首帧执行仍为 false，
        /// 而能走到 InitiateDash postfix 末尾说明冲刺已真正启动（其内部有条件守卫）。</summary>
        internal static void InitiateDashPostfix(CharacterDash_RL __instance)
        {
            try
            {
                if (!Active) return;
                if (__instance == null) return;

                // v3.0.13：倒地尸体动画仍会触发冲刺发包 → 对端重放形成双份/连发。倒地端不发。
                if (DownedState.LocalDown) return;

                // v3.0.13：发送端去重——一次真实冲刺只发一包，掐断连发。
                // 0.4s 对内：真人最快连续冲刺间隔也 > 0.4s；对状态机同帧多次调用足够。
                float nowDash = Time.unscaledTime;
                if (nowDash - _lastDashSent < DashDedupWindow) return;

                var nm = NetworkManager.Instance;
                var ps = PlayerSync.Instance;
                if (nm == null || ps == null || ps.LocalPlayer == null) return;

                // 只同步本地玩家自己的冲刺
                var localRoot = ps.LocalPlayer.transform.root;
                if (__instance.transform.root != localRoot) return;

                // IsVoidDashing 此刻恒 false（依赖已冲距离），改看传家宝等级
                bool isVoid = false;
                try { isVoid = SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockVoidDash) > 0; }
                catch { }

                // v3.0.12：虚冲开主动桥窗口。虚冲专属残影(DashSilhouette/DashLines)靠
                // VoidDash bool 条件触发，而网络参数快照 10Hz 会把分身 bool 覆盖回 false、
                // 层 scrub 会把 dash 特效状态硬拉回 —— 单靠 SetBool/SetTrigger 分身永远播不出。
                // 打开窗口后，真身 dash 特效走 AbilityFxSync 已验证的 SpellFx 主动桥，
                // 接收端直接 InvokeTrigger 在分身上播，彻底绕开 Animator 参数/状态机。
                // 下面的 SetBool/SetTrigger 路径保留做双保险（特效桥未就绪时仍可兜底）。
                if (isVoid)
                {
                    try { AbilityFxSync.OpenDashFxWindow(); }
                    catch { /* 主动桥不可用时仍有 trigger/bool 兜底 */ }
                }

                // 确认是本地真身的有效冲刺，此刻才记下去重时间戳。
                _lastDashSent = nowDash;

                var w = new NetDataWriter();
                w.Put((byte)MsgType.DashFx);
                w.Put(nm.LocalPlayerId);
                w.Put(isVoid);
                nm.SendDashFx(w);
            }
            catch (Exception ex)
            {
                LogErr("[DASHFX] 发送失败: " + Plugin.PatchError(ex));
            }
        }

        // ---------------- 接收端 ----------------

        /// <summary>收到远端冲刺事件：给分身补 trigger + 限时豁免特效禁用。</summary>
        internal static void PlayRemoteDash(int ownerId, bool isVoid)
        {
            var ps = PlayerSync.Instance;
            if (ps == null) return;
            GameObject clone;
            if (!ps.RemotePlayers.TryGetValue(ownerId, out clone) || clone == null)
            {
                LogErr("[DASHFX] 收到冲刺 id=" + ownerId + " 但分身未创建，跳过");
                return;
            }

            float now = Time.unscaledTime;
            int n = 0;
            var animators = clone.GetComponentsInChildren<Animator>(true);
            for (int i = 0; i < animators.Length; i++)
            {
                var a = animators[i];
                if (a == null) continue;
                try
                {
                    // 补 Trigger（网络动画同步传不了 trigger）
                    a.SetTrigger("Dashing_Effect");
                    if (isVoid) a.SetBool("VoidDash", true);
                    // 加入限时豁免：让 dash 状态里的 EffectTriggerAnimBehaviour 能播残影
                    _grace[a] = now + GraceSeconds;
                    n++;
                }
                catch { }
            }
            Log("[DASHFX] 远端冲刺 id=" + ownerId + " void=" + isVoid + " 豁免 animator=" + n);
        }

        /// <summary>EffectManager.AnimatorEffectsDisabled prefix：豁免集合内返回 false（放行特效）。</summary>
        internal static bool AnimatorEffectsDisabledPrefix(Animator animator, ref bool __result)
        {
            if (animator == null) return true;
            float now = Time.unscaledTime;
            float until;
            if (_grace.TryGetValue(animator, out until))
            {
                if (now < until)
                {
                    __result = false; // 放行：分身这一下 dash 特效可以播
                    return false;     // 跳过原方法
                }
                _grace.Remove(animator);   // 过期移除，恢复正常禁用（攻击伤害防护不失效）
            }
            return true;
        }

        /// <summary>
        /// v3.0.2：供 AbilityFxSync 复用同一套特效豁免（施法 state 里的 EffectTrigger
        /// 也要能在分身上发射粒子）。窗口可自定义，覆盖施法完整时长。
        /// </summary>
        internal static void GrantGrace(Animator animator, float until)
        {
            if (animator == null) return;
            _grace[animator] = until;
        }

        internal static void ResetAll()
        {
            _grace.Clear();
            _lastDashSent = 0f;
        }

        private static void TryPatch(Harmony harmony, MethodInfo target, string prefixName,
            string label, string postfixName = null)
        {
            try
            {
                if (target == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [DASHFX] 补丁目标为空: " + label);
                    return;
                }
                HarmonyMethod pre = null, post = null;
                if (!string.IsNullOrEmpty(prefixName))
                    pre = new HarmonyMethod(typeof(DashFxSync).GetMethod(prefixName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                if (!string.IsNullOrEmpty(postfixName))
                    post = new HarmonyMethod(typeof(DashFxSync).GetMethod(postfixName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                harmony.Patch(target, prefix: pre, postfix: post);
                Plugin.Log.LogInfo("[RL2MP] [DASHFX] 补丁 OK: " + label);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [DASHFX] " + label + " 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        private static void Log(string msg)
        {
            float now = Time.unscaledTime;
            if (now - _lastLog < 0.5f) return; // 节流，避免连续冲刺刷屏
            _lastLog = now;
            Plugin.Log.LogInfo("[RL2MP] " + msg);
        }

        private static void LogErr(string msg)
        {
            float now = Time.unscaledTime;
            if (now - _lastLog < 0.5f) return;
            _lastLog = now;
            Plugin.Log.LogWarning("[RL2MP] " + msg);
        }
    }
}
