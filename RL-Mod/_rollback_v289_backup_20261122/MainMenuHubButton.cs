using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.0：在主菜单注入一个原生「联机中心」按钮（和开始游戏/选择存档那一栏同排）。
    /// 做法：
    ///   1) patch MainMenuWindowController.Initialize 的 postfix —— 克隆一个现有 MainMenuButton，
    ///      放到菜单按钮组下方，改文字为「联机中心」，改名为固定标识。
    ///   2) patch MainMenuButton.ExecuteButton 的 prefix —— 若是我们的按钮，打开整屏联机中心并
    ///      跳过原菜单跳转逻辑。
    /// 全程 try/catch，失败只打日志，不影响游戏原菜单。
    /// </summary>
    internal static class MainMenuHubButton
    {
        private const string ButtonName = "RL2MP_HubButton";
        private const string ButtonText = "联机中心";

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var mmType = AccessTools.TypeByName("MainMenuWindowController");
                var initMethod = mmType != null ? AccessTools.Method(mmType, "Initialize") : null;
                if (initMethod != null)
                {
                    var postfix = new HarmonyMethod(typeof(MainMenuHubButton).GetMethod("InitializePostfix",
                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
                    harmony.Patch(initMethod, postfix: postfix);
                    Plugin.Log.LogInfo("[MENUBTN] 补丁 OK: MainMenuWindowController.Initialize postfix");
                }
                else
                {
                    Plugin.Log.LogWarning("[MENUBTN] MainMenuWindowController.Initialize 未找到。");
                }

                var btnType = AccessTools.TypeByName("MainMenuButton");
                var execMethod = btnType != null ? AccessTools.Method(btnType, "ExecuteButton") : null;
                if (execMethod != null)
                {
                    var prefix = new HarmonyMethod(typeof(MainMenuHubButton).GetMethod("ExecuteButtonPrefix",
                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
                    harmony.Patch(execMethod, prefix: prefix);
                    Plugin.Log.LogInfo("[MENUBTN] 补丁 OK: MainMenuButton.ExecuteButton prefix");
                }
                else
                {
                    Plugin.Log.LogWarning("[MENUBTN] MainMenuButton.ExecuteButton 未找到。");
                }

                // v1.9.2：拦截 OnSelect，避免克隆按钮一直显示"光标划过"的选中高亮动画。
                // MainMenuButton 只实现了 OnPointerEnter->OnSelect（激活 indicator），
                // 没有 OnPointerExit，鼠标移开后高亮不会消失，看起来像一直被悬停。
                var selectMethod = btnType != null ? AccessTools.Method(btnType, "OnSelect") : null;
                if (selectMethod != null)
                {
                    var selectPrefix = new HarmonyMethod(typeof(MainMenuHubButton).GetMethod("OnSelectPrefix",
                        BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));
                    harmony.Patch(selectMethod, prefix: selectPrefix);
                    Plugin.Log.LogInfo("[MENUBTN] 补丁 OK: MainMenuButton.OnSelect prefix（去高亮）");
                }
                else
                {
                    Plugin.Log.LogWarning("[MENUBTN] MainMenuButton.OnSelect 未找到，无法去除高亮。");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[MENUBTN] 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        private static void InitializePostfix(object __instance)
        {
            try
            {
                if (__instance == null) return;
                Type mmType = __instance.GetType();

                // 取私有 List<MainMenuButton> m_menuButtonList
                FieldInfo listField = AccessTools.Field(mmType, "m_menuButtonList");
                if (listField == null) return;
                var list = listField.GetValue(__instance) as IList;
                if (list == null || list.Count == 0) return;

                // 已经注入过则跳过（换场景重进主菜单时列表里会有我们的按钮）
                foreach (var o in list)
                {
                    var b = o as Component;
                    if (b != null && b.gameObject != null && b.gameObject.name == ButtonName) return;
                }

                // 找一个模板按钮（优先 SelectProfile，其次第一个）
                Component template = null;
                Component first = null;
                foreach (var o in list)
                {
                    var b = o as Component;
                    if (b == null) continue;
                    if (first == null) first = b;
                    var selectorValue = Traverse.Create(b).Property("SelectorType").GetValue();
                    // MainMenuSelectionType.SelectProfile == 2
                    if (selectorValue != null && Convert.ToInt32(selectorValue) == 2)
                    {
                        template = b;
                        break;
                    }
                }
                if (template == null) template = first;
                if (template == null) return;
                if (template.gameObject == null || template.transform.parent == null) return;

                // 推算按钮垂直间距：用列表里相邻两个按钮的 anchoredPosition.y 差
                float spacing = 56f;
                float prevY = 0f; bool hasPrev = false;
                foreach (var o in list)
                {
                    var b = o as Component;
                    if (b == null) continue;
                    var rt = b.GetComponent<RectTransform>();
                    if (rt == null) continue;
                    if (hasPrev)
                    {
                        float d = Mathf.Abs(prevY - rt.anchoredPosition.y);
                        if (d > 8f && d < 300f) { spacing = d; break; }
                    }
                    prevY = rt.anchoredPosition.y;
                    hasPrev = true;
                }

                // 找到菜单里最下面的按钮，放到它下方一个间距处
                Component bottom = null; float bottomY = float.MaxValue;
                foreach (var o in list)
                {
                    var b = o as Component;
                    if (b == null) continue;
                    var rt = b.GetComponent<RectTransform>();
                    if (rt == null) continue;
                    if (rt.anchoredPosition.y < bottomY) { bottomY = rt.anchoredPosition.y; bottom = b; }
                }

                GameObject go = UnityEngine.Object.Instantiate(template.gameObject);
                go.name = ButtonName;
                go.transform.SetParent(template.transform.parent, false);
                go.transform.localScale = Vector3.one;

                // 位置：最下按钮再往下一个间距；若布局组自动排列则此设置会被覆盖，无妨
                RectTransform newRt = go.GetComponent<RectTransform>();
                RectTransform bottomRt = bottom != null ? bottom.GetComponent<RectTransform>() : null;
                if (newRt != null && bottomRt != null)
                {
                    newRt.anchoredPosition = new Vector2(bottomRt.anchoredPosition.x, bottomRt.anchoredPosition.y - spacing);
                    newRt.localPosition = new Vector3(bottomRt.localPosition.x, bottomRt.localPosition.y - spacing, bottomRt.localPosition.z);
                }

                // 改文字：用 Traverse 反射设 TMP_Text.text，避免直接引用 TMPro 程序集
                var btnComp = go.GetComponent(template.GetType());
                if (btnComp != null)
                {
                    var textObj = Traverse.Create(btnComp).Property("Text").GetValue();
                    if (textObj != null)
                    {
                        Traverse.Create(textObj).Property("text").SetValue(ButtonText);
                    }
                    // 关闭选中指示器的初始高亮（MainMenuButton.Initialize 会 SetActive(false)）
                    var indicator = Traverse.Create(btnComp).Field("m_selectedIndicator").GetValue<GameObject>();
                    if (indicator != null) indicator.SetActive(false);
                }
                // 保证可交互
                Traverse.Create(btnComp).Property("Interactable").SetValue(true);

                // v1.9.4：挂上自研悬停控制器——进入点亮、移开熄灭（原生 MainMenuButton 只有 enter 没有 exit）
                if (go.GetComponent<HubButtonHover>() == null)
                {
                    go.AddComponent<HubButtonHover>();
                }

                Plugin.Log.LogInfo("[MENUBTN] 主菜单已注入「" + ButtonText + "」按钮 (spacing=" + spacing.ToString("F1") + ")");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[MENUBTN] 注入主菜单按钮失败（不影响原菜单）: " + ex.Message);
            }
        }

        /// <summary>
        /// v1.9.2：联机中心按钮不参与原菜单的选中高亮。
        /// 原因：OnPointerEnter -> OnSelect 会激活金色指示器动画，而 MainMenuButton 没有
        /// OnPointerExit，鼠标移开后高亮不消失，看起来像光标一直停在按钮上。
        /// 同时跳过 MenuButtonSelected 回调，避免打乱主菜单键盘导航的选中索引。
        /// 返回 false 只针对我们的按钮；点击仍由 ExecuteButtonPrefix 正常处理。
        /// </summary>
        private static bool OnSelectPrefix(object __instance)
        {
            try
            {
                var comp = __instance as Component;
                if (comp != null && comp.gameObject != null && comp.gameObject.name == ButtonName)
                {
                    // 强制关掉可能已激活的指示器（双重保险）
                    var indicator = Traverse.Create(__instance).Field("m_selectedIndicator").GetValue<GameObject>();
                    if (indicator != null) indicator.SetActive(false);
                    return false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[MENUBTN] OnSelectPrefix 异常: " + ex.Message);
            }
            return true;
        }

        private static bool ExecuteButtonPrefix(object __instance)
        {
            try
            {
                var comp = __instance as Component;
                if (comp != null && comp.gameObject != null && comp.gameObject.name == ButtonName)
                {
                    // 可交互性检查
                    bool interactable = Traverse.Create(__instance).Property("Interactable").GetValue<bool>();
                    if (!interactable) return false;
                    Plugin.Log.LogInfo("[MENUBTN] 「联机中心」按钮被点击 -> 打开整屏界面");
                    MpHub.Open();
                    return false; // 跳过原菜单跳转
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[MENUBTN] ExecuteButtonPrefix 异常: " + ex.Message);
            }
            return true;
        }
    }
}
