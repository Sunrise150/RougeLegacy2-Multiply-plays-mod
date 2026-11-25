using System;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.2：在游戏内 ESC 暂停菜单里注入一个原生「联机中心」Tab。
    /// 做法：
    ///   1) patch PauseWindowController.Initialize 的 postfix —— 克隆一个现有 PauseTabButton，
    ///      改名/改文字为「联机中心」，WindowToDisplay=None（不打开任何原生子窗口），加入
    ///      m_pauseTabArray，并订阅 SetSelectedTab 让它参与 Tab 导航。
    ///   2) patch PauseWindowController.UpdateSelectedTab 的 postfix —— 当选中我们的 Tab 时
    ///      打开整屏联机中心（IMGUI），切到其它 Tab 时关闭。
    /// 这样鼠标点击 / 键盘 LB·RB 切 Tab 都能正常开关联机中心，无需再打开那个 IMGUI 小控制面板。
    /// 全程 try/catch，失败只打日志，不影响游戏原生暂停菜单。
    /// </summary>
    internal static class PauseMenuHubTab
    {
        private const string TabName = "RL2MP_PauseHubTab";
        private const string TabText = "联机中心";
        private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public;
        private static Type s_pauseType;
        private static volatile bool s_resetRequested;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                Type pauseType = AccessTools.TypeByName("PauseWindowController");
                if (pauseType == null)
                {
                    Plugin.Log.LogWarning("[PAUSETAB] PauseWindowController 未找到，无法注入暂停菜单 Tab。");
                    return;
                }

                var initMethod = AccessTools.Method(pauseType, "Initialize");
                if (initMethod != null)
                {
                    harmony.Patch(initMethod, postfix: new HarmonyMethod(typeof(PauseMenuHubTab).GetMethod("InitPostfix", StaticFlags)));
                    Plugin.Log.LogInfo("[PAUSETAB] 补丁 OK: PauseWindowController.Initialize postfix");
                }
                else Plugin.Log.LogWarning("[PAUSETAB] Initialize 未找到。");

                var updateMethod = AccessTools.Method(pauseType, "UpdateSelectedTab", new[] { typeof(bool) });
                if (updateMethod != null)
                {
                    harmony.Patch(updateMethod, postfix: new HarmonyMethod(typeof(PauseMenuHubTab).GetMethod("UpdateSelectedTabPostfix", StaticFlags)));
                    Plugin.Log.LogInfo("[PAUSETAB] 补丁 OK: PauseWindowController.UpdateSelectedTab postfix");
                }
                else Plugin.Log.LogWarning("[PAUSETAB] UpdateSelectedTab 未找到。");

                // 关闭联机中心时只置一个标志，真正的 UI 复位延迟到下一帧 Update 执行。
                // 绝不能在 OnGUI（IMGUI）里同步操作 Unity UI（窗口切换/布局重建），否则会卡死。
                s_pauseType = pauseType;
                MpHub.OnClosed = () => { s_resetRequested = true; };
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[PAUSETAB] 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>由 MpHub.TickDeferred() 从 ControlPanel.Update 调用（非 IMGUI 上下文）。
        /// 关闭联机中心后把暂停菜单切回 Options Tab，保证可以再次点开联机中心 Tab。</summary>
        internal static void FlushPendingReset()
        {
            if (!s_resetRequested) return;
            s_resetRequested = false;
            ResetToOptionsTab(s_pauseType);
        }

        /// <summary>关闭联机中心后，把暂停菜单切回 Options Tab（WindowID.Options=1）。</summary>
        private static void ResetToOptionsTab(Type pauseType)
        {
            try
            {
                if (pauseType == null) return;
                var controllers = UnityEngine.Object.FindObjectsOfType(pauseType);
                if (controllers == null || controllers.Length == 0) return;

                // 精确找 SetSelectedTab(WindowID) —— 参数是枚举，不是 int
                MethodInfo setSel = null;
                foreach (var m in pauseType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
                {
                    if (m.Name == "SetSelectedTab" && m.GetParameters().Length == 1
                        && m.GetParameters()[0].ParameterType.IsEnum)
                    {
                        setSel = m;
                        break;
                    }
                }
                if (setSel == null) return;
                object optionsVal = Enum.ToObject(setSel.GetParameters()[0].ParameterType, 1); // WindowID.Options
                foreach (var c in controllers)
                {
                    setSel.Invoke(c, new object[] { optionsVal });
                }
                Plugin.Log.LogInfo("[PAUSETAB] 联机中心关闭后已将暂停 Tab 复位到 Options");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[PAUSETAB] 复位 Tab 失败（不影响功能）: " + ex.Message);
            }
        }

        private static void InitPostfix(object __instance)
        {
            try
            {
                Type pauseType = __instance.GetType();
                FieldInfo arrField = AccessTools.Field(pauseType, "m_pauseTabArray");
                if (arrField == null) return;
                var arr = arrField.GetValue(__instance) as Array;
                if (arr == null || arr.Length == 0) return;

                // 换场景/重复 Initialize 时防重复注入
                foreach (var o in arr)
                {
                    var c = o as Component;
                    if (c != null && c.gameObject != null && c.gameObject.name == TabName) return;
                }

                Component template = arr.GetValue(0) as Component;
                if (template == null || template.gameObject == null || template.transform.parent == null) return;

                GameObject go = UnityEngine.Object.Instantiate(template.gameObject);
                go.name = TabName;
                go.transform.SetParent(template.transform.parent, false);
                go.transform.localScale = Vector3.one;

                var newTabComp = go.GetComponent(template.GetType());
                if (newTabComp == null) return;

                SetTabText(go, TabText);

                // WindowID.None = 0：UpdateSelectedTab 遇到 None 会 return，不开任何原生子窗口
                Traverse.Create(newTabComp).Property("WindowToDisplay").SetValue(0);

                // 订阅 OnSelectEvent -> SetSelectedTab(PauseTabButton)，让它参与 Tab 高亮/导航
                try
                {
                    Type tabType = template.GetType();
                    Type handlerType = AccessTools.TypeByName("PauseTabSelectedHandler");
                    var setSelMethod = AccessTools.Method(pauseType, "SetSelectedTab", new[] { tabType });
                    if (handlerType != null && setSelMethod != null)
                    {
                        Delegate handler = Delegate.CreateDelegate(handlerType, __instance, setSelMethod);
                        var prop = Traverse.Create(newTabComp).Property("OnSelectEvent");
                        var current = prop.GetValue() as Delegate;
                        prop.SetValue(Delegate.Combine(current, handler));
                    }
                }
                catch (Exception ex)
                {
                    Plugin.Log.LogWarning("[PAUSETAB] 订阅 OnSelectEvent 失败（不影响 Tab 显示）: " + ex.Message);
                }

                // 加入 m_pauseTabArray
                Type elemType = arr.GetType().GetElementType();
                Array newArr = Array.CreateInstance(elemType, arr.Length + 1);
                Array.Copy(arr, newArr, arr.Length);
                newArr.SetValue(newTabComp, arr.Length);
                arrField.SetValue(__instance, newArr);

                Plugin.Log.LogInfo("[PAUSETAB] 已向暂停菜单注入「" + TabText + "」Tab (共 " + newArr.Length + " 个)");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[PAUSETAB] 注入暂停 Tab 失败（不影响原菜单）: " + ex.Message);
            }
        }

        private static void SetTabText(GameObject go, string text)
        {
            foreach (var c in go.GetComponentsInChildren<Component>(true))
            {
                if (c == null) continue;
                string n = c.GetType().Name;
                if (n == "TextMeshProUGUI" || n == "TMP_Text" || n == "TextMeshPro")
                {
                    Traverse.Create(c).Property("text").SetValue(text);
                }
            }
        }

        private static void UpdateSelectedTabPostfix(object __instance)
        {
            try
            {
                Type pauseType = __instance.GetType();
                var arrField = AccessTools.Field(pauseType, "m_pauseTabArray");
                var idxField = AccessTools.Field(pauseType, "m_selectedTabIndex");
                if (arrField == null || idxField == null) return;
                var arr = arrField.GetValue(__instance) as Array;
                int idx = (int)idxField.GetValue(__instance);
                if (arr == null || idx < 0 || idx >= arr.Length) return;

                var tab = arr.GetValue(idx) as Component;
                bool isHub = tab != null && tab.gameObject != null && tab.gameObject.name == TabName;
                if (isHub)
                {
                    if (!MpHub.Visible) MpHub.Open();
                }
                else if (MpHub.Visible)
                {
                    MpHub.Close();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[PAUSETAB] UpdateSelectedTab 异常: " + ex.Message);
            }
        }
    }
}
