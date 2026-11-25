using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v3.0.24：失焦不再把整个实例挂起。
    ///
    /// 真机现象：鼠标不在主机窗口上时，客机画面"非常糟糕"；鼠标移回去点一下主机窗口，
    /// 客机画面立刻好转。根因是两条独立的东西叠在一起：
    ///
    /// 1) Unity 独立程序的 Player Settings 里 "Run In Background" 默认是关的。
    ///    关着的时候，窗口一旦失去焦点，Unity 会【挂起整个主循环】：
    ///    Update 不再执行、Application.onBeforeRender 不再触发、也不再出帧。
    ///    我们整套联机逻辑恰恰都挂在这两个回调上：
    ///      · NetworkManager.Update -> PumpNetwork（收发 + 定时器）
    ///      · Application.onBeforeRender -> Bootstrap.Tick（暂停时的兜底收发）
    ///    所以失焦的那一方会彻底停止收发 —— 对端看到的就是画面冻住/极度卡顿；
    ///    鼠标点回主机窗口 → 恢复焦点 → 主循环复活 → 立刻好转。症状完全对上。
    ///    顺带：门触发器自愈（RoomFix.Tick -> DoorLinkFix.Tick）也挂在 DownedManager.Update 上，
    ///    失焦时同样停摆 —— 这就是"门触发器又失效了"的另一半原因。
    ///
    /// 2) 游戏自己还叠了一层：GameManager.OnApplicationFocus(false) ->
    ///    WindowManager.PauseWhenPossible(true) -> 弹出暂停菜单窗口，并把 Time.timeScale 置 0。
    ///    PauseCompat 已经把 timeScale 抢回 1（世界继续跑），但暂停菜单窗口仍会盖住画面，
    ///    而且 GameManager.IsGamePaused=true 会把输入切到菜单层 —— 观战时很难受。
    ///
    /// 做法（只做加法，不碰已验证的门/房间/选房逻辑）：
    ///   1) Application.runInBackground = true —— 失焦后 Unity 继续跑主循环，收发不断。
    ///   2) 联机中拦掉 GameManager.OnApplicationFocus 的"失焦即暂停"：只放行"获得焦点"，
    ///      失焦时什么都不做。ESC 暂停、手柄断开暂停走的是别的入口，完全不受影响；
    ///      单机也完全不受影响（仍按原生暂停）。
    /// </summary>
    internal static class BackgroundRun
    {
        /// <summary>补丁是否生效（供诊断日志用）。</summary>
        internal static bool Applied;

        /// <summary>最近一次焦点状态变化（诊断用）。</summary>
        internal static string LastFocus = "?";

        internal static void Apply(Harmony harmony)
        {
            // ---- 1) 让 Unity 失焦后继续跑主循环 ----
            try
            {
                Application.runInBackground = true;
                Plugin.Log.LogInfo("[RL2MP] [BG] Application.runInBackground = true"
                    + "（失焦不再挂起主循环，收发/门自愈不断）");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [BG] 设置 runInBackground 失败: " + Plugin.PatchError(ex));
            }

            // ---- 2) 联机中不让失焦把游戏暂停掉（否则弹出暂停菜单 + 抢输入）----
            try
            {
                var method = AccessTools.Method(typeof(GameManager), "OnApplicationFocus");
                if (method == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [BG] GameManager.OnApplicationFocus 未找到，"
                        + "失焦暂停不拦截（runInBackground 已生效）。");
                    return;
                }
                var prefix = new HarmonyMethod(typeof(BackgroundRunPatches).GetMethod("FocusPrefix",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
                harmony.Patch(method, prefix: prefix);
                Applied = true;
                Plugin.Log.LogInfo("[RL2MP] [BG] 补丁 OK: GameManager.OnApplicationFocus"
                    + "（联机失焦不暂停、不弹暂停菜单）");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [BG] 补丁失败: " + Plugin.PatchError(ex));
            }
        }
    }

    internal static class BackgroundRunPatches
    {
        /// <summary>
        /// Harmony prefix：返回 false 跳过原方法。
        /// 只在"联机中 + 失去焦点"时跳过；获得焦点、单机、断线重连中一律原样放行。
        /// </summary>
        internal static bool FocusPrefix(bool focus)
        {
            try
            {
                if (focus)
                {
                    if (BackgroundRun.LastFocus != "focus")
                    {
                        BackgroundRun.LastFocus = "focus";
                        Plugin.Log.LogInfo("[RL2MP] [BG] 窗口获得焦点。");
                    }
                    return true;
                }

                // 失焦。联机中不暂停。
                if (PauseCompat.ShouldKeepRunning())
                {
                    if (BackgroundRun.LastFocus != "lost")
                    {
                        BackgroundRun.LastFocus = "lost";
                        Plugin.Log.LogInfo("[RL2MP] [BG] 窗口失去焦点：联机中继续保持运行"
                            + "（不暂停、不弹菜单），收发与门自愈不断。");
                    }
                    return false;
                }

                BackgroundRun.LastFocus = "lost-single";
                return true;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [BG] FocusPrefix 异常: " + ex.Message);
                return true;
            }
        }
    }
}
