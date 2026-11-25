using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.0：联机时"暂停菜单照常打开，但游戏世界不冻结"。
    ///
    /// 背景：
    ///   按 ESC -> WindowManager.PauseWhenPossible -> GameManager.SetIsPaused(true)
    ///        -> RLTimeScale.SetTimeScale(TimeScaleType.Game, 0f) -> Time.timeScale = 0
    ///   网络层本身用 unscaledDeltaTime，收发不会断；真正被冻结的是游戏逻辑
    ///   （怪物 AI、死亡协程、血条更新、快照对账都走 scaled time），
    ///   于是表现为"一方暂停后怪不同步 / 打了半血的怪对端看到满血"。
    ///
    /// 方案：
    ///   只拦 TimeScaleType.Game 且 value<=0 的那一次调用，联机(Connected)时把它
    ///   改成 1f。GameManager.IsGamePaused 仍为 true —— 暂停菜单照常弹出、输入映射
    ///   切到 Window、可以返回主界面/改设置；但 Time.timeScale 保持 1，世界继续跑。
    ///   单机(未连接)完全不受影响，仍可正常暂停。
    /// </summary>
    internal static class PauseCompat
    {
        /// <summary>由 BepInEx 配置绑定；联机时是否保持世界运行（默认开）。</summary>
        internal static bool KeepWorldRunning = true;

        private static int _suppressedTotal;
        private static float _logTimer;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var type = typeof(RLTimeScale);
                var method = AccessTools.Method(type, "SetTimeScale");
                if (method == null)
                {
                    Plugin.Log.LogWarning("[PAUSE] RLTimeScale.SetTimeScale 未找到，跳过暂停兼容补丁。");
                    return;
                }
                var prefix = new HarmonyMethod(typeof(PauseCompatPatches).GetMethod("SetTimeScalePrefix",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
                harmony.Patch(method, prefix: prefix);
                Plugin.Log.LogInfo("[PAUSE] 补丁 OK: RLTimeScale.SetTimeScale (联机暂停不冻结世界)");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[PAUSE] 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>
        /// v1.9.1：是否应拦截暂停。联机(Connected)时强制生效——不再受任何 UI 开关影响，
        /// 杜绝误触导致"暂停冻结世界、怪物不同步"。单机(未连接)不受影响。
        /// </summary>
        internal static bool ShouldKeepRunning()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return false;

            // v2.3.2：转场/加载期间一律不拦。游戏自己的过渡（fade + 加载屏）对时间缩放有依赖，
            // 我们把它按 1 强塞回去会让过渡协程永远走不完 —— 真机现象就是
            // "LoadScene 已经发起、主循环还在跑、但画面永远停在加载中"。
            if (SceneChangeSync.IsTransitioning) return false;

            return true;
        }

        /// <summary>诊断用：从网络泵每帧调一次。首次拦截立即打日志，之后节流汇总。</summary>
        internal static void Tick(float unscaledDelta)
        {
            _logTimer += unscaledDelta;
            if (_logTimer < 10f) return;
            _logTimer = 0f;
            if (_suppressedTotal > 0)
            {
                Plugin.Log.LogInfo("[PAUSE] 最近 10s 拦截了 " + _suppressedTotal
                    + " 次游戏暂停冻结（菜单打开但世界继续运行）。累计 "
                    + TotalSuppressed + " 次。");
                _suppressedTotal = 0;
            }
        }

        internal static int TotalSuppressed;

        internal static void NoteSuppressed()
        {
            _suppressedTotal++;
            TotalSuppressed++;
            // 首次拦截立即打一条，确保日志里一定能看到（不再依赖 Tick 节流）
            if (TotalSuppressed == 1)
                Plugin.Log.LogInfo("[PAUSE] 首次拦截暂停冻结成功：联机中 ESC 菜单打开但游戏世界继续运行。");
        }
    }

    internal static class PauseCompatPatches
    {
        // Harmony prefix：TimeScaleType 与 float 都按值传，用 ref 改 value 即可生效。
        internal static void SetTimeScalePrefix(TimeScaleType timeScaleType, ref float value)
        {
            try
            {
                // v2.8.6：恢复只拦 Game 类型。v2.8.5 放开到所有 value<=0 后，
                // Boss 死亡定格（Cutscene=0，BaseAIScript.DeathAnim）也被强行改成 1，
                // 两端同时出现严重卡房间/时序错乱 —— 这是唯一的全局改动，优先回退。
                // Cutscene 残留的 0 仍由 SceneChangeSync 看门狗的 RLTimeScale.Reset() 兜底。
                if (timeScaleType == TimeScaleType.Game && value <= 0f && PauseCompat.ShouldKeepRunning())
                {
                    value = 1f;
                    PauseCompat.NoteSuppressed();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[PAUSE] prefix 异常: " + ex.Message);
            }
        }
    }
}
