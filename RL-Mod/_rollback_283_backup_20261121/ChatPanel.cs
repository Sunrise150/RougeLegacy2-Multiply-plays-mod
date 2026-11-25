using System;
using System.Collections.Generic;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.4：联机文字聊天栏。
    ///   - 贴在屏幕最右侧，默认收起，只显示最近一条消息的缩略文字（有新消息时高亮）。
    ///   - 按回车展开（展开后输入框自动聚焦），再按回车发送消息，Esc 收起。
    ///   - 消息经 NetworkManager.SendChat 走主机中转广播。
    ///   - 用独立的暗金皮肤，绘制前后保存/还原 GUI.skin，绝不影响游戏或其它 UI。
    /// 由 ControlPanel.OnGUI 每帧转发绘制（OnGUI 必须在活着的 MonoBehaviour 里调）。
    /// </summary>
    internal static class ChatPanel
    {
        private class ChatLine
        {
            public string Tag;
            public string Text;
            public float At;      // realtimeSinceStartup，用于淡出
        }

        private const int MaxLines = 60;
        private static readonly List<ChatLine> _lines = new List<ChatLine>();
        private static bool _expanded;
        private static int _unread;
        private static string _input = "";
        private static Vector2 _scroll;
        private static string _focusName = "RL2MP_ChatInput";
        private static bool _wantFocus;   // 展开后下一帧 OnGUI 里聚焦输入框（GUI API 只能在 OnGUI 调）

        private static GUISkin _skin;
        private static GUISkin _savedSkin;
        private static bool _skinApplied;
        private static Texture2D _texPanel, _texField, _texSend, _texSendHover;
        private static GUIStyle _lineStyle, _tagStyle, _smallStyle;

        /// <summary>收到一条聊天（本地或网络）。</summary>
        internal static void Receive(string tag, string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            var line = new ChatLine
            {
                Tag = string.IsNullOrEmpty(tag) ? "?" : tag,
                Text = text,
                At = Time.realtimeSinceStartup
            };
            _lines.Add(line);
            if (_lines.Count > MaxLines) _lines.RemoveAt(0);
            if (!_expanded) _unread++;
            Plugin.Log.LogInfo("[CHAT] <" + line.Tag + "> " + text);
        }

        /// <summary>本地发送：先本地回显，再走网络。</summary>
        internal static void SendLocal()
        {
            string text = _input == null ? "" : _input.Trim();
            if (text.Length == 0) return;
            string tag = "我";
            Receive(tag, text);
            var nm = NetworkManager.Instance;
            if (nm != null && nm.Connected) nm.SendChat(text);
            _input = "";
        }

        /// <summary>回车键：收起时展开，展开时（若有焦点在输入框）由 OnGUI 处理发送；Esc 收起。</summary>
        internal static void ToggleExpanded()
        {
            _expanded = !_expanded;
            if (_expanded)
            {
                _unread = 0;
                _scroll = new Vector2(0f, float.MaxValue); // 展开时滚到底
                _wantFocus = true;
            }
            Plugin.Log.LogInfo("[CHAT] 展开 -> " + _expanded);
        }

        internal static bool Expanded { get { return _expanded; } }

        /// <summary>由 ControlPanel.Update 每帧调用：回车展开/发送，Esc 收起。
        /// 键盘统一在 Update 里处理，避免与 OnGUI 的 Event 系统双触发。</summary>
        internal static void HandleInput()
        {
            bool enter = Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter);
            if (!_expanded)
            {
                if (enter) ToggleExpanded();
                return;
            }
            // 展开态
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Collapse();
            }
            else if (enter)
            {
                SendLocal();
                _wantFocus = true;
            }
        }

        internal static void Collapse()
        {
            if (!_expanded) return;
            _expanded = false;
            _wantFocus = false;
        }

        // ---------------- 绘制 ----------------

        private static Texture2D MakeTex(Color c)
        {
            var t = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            t.SetPixel(0, 0, c);
            t.Apply();
            t.hideFlags = HideFlags.HideAndDontSave;
            return t;
        }

        private static void EnsureTextures()
        {
            if (_texPanel != null) return;
            _texPanel = MakeTex(new Color(0.08f, 0.06f, 0.05f, 0.90f));
            _texField = MakeTex(new Color(0.05f, 0.04f, 0.03f, 0.97f));
            _texSend = MakeTex(new Color(0.28f, 0.21f, 0.10f, 1f));
            _texSendHover = MakeTex(new Color(0.44f, 0.33f, 0.14f, 1f));
        }

        private static void EnsureSkin()
        {
            EnsureTextures();
            if (_skin == null)
            {
                _skin = UnityEngine.Object.Instantiate(GUI.skin);
                _skin.name = "RL2MP_ChatSkin";
                Color gold = new Color(0.93f, 0.78f, 0.42f);
                Color parchment = new Color(0.90f, 0.85f, 0.70f);

                _skin.box.normal.background = _texPanel;
                _skin.box.normal.textColor = parchment;
                _skin.box.padding = new RectOffset(8, 8, 6, 6);

                _skin.button.normal.background = _texSend;
                _skin.button.normal.textColor = gold;
                _skin.button.hover.background = _texSendHover;
                _skin.button.hover.textColor = Color.white;
                _skin.button.active.background = _texSendHover;
                _skin.button.fontStyle = FontStyle.Bold;
                _skin.button.alignment = TextAnchor.MiddleCenter;

                _skin.textField.normal.background = _texField;
                _skin.textField.normal.textColor = parchment;
                _skin.textField.focused.background = _texField;
                _skin.textField.focused.textColor = Color.white;
                _skin.textField.padding = new RectOffset(6, 4, 4, 4);
                _skin.scrollView.normal.background = _texPanel;
            }

            if (_lineStyle == null)
            {
                _lineStyle = new GUIStyle(GUI.skin.label);
                _lineStyle.fontSize = 13;
                _lineStyle.wordWrap = true;
                _lineStyle.normal.textColor = new Color(0.90f, 0.85f, 0.70f);
            }
            if (_tagStyle == null)
            {
                _tagStyle = new GUIStyle(_lineStyle);
                _tagStyle.normal.textColor = new Color(0.98f, 0.82f, 0.38f);
                _tagStyle.fontStyle = FontStyle.Bold;
            }
            if (_smallStyle == null)
            {
                _smallStyle = new GUIStyle(GUI.skin.label);
                _smallStyle.fontSize = 12;
                _smallStyle.normal.textColor = new Color(0.80f, 0.72f, 0.50f);
                _smallStyle.alignment = TextAnchor.MiddleRight;
            }
        }

        private static void ApplySkin()
        {
            if (!_skinApplied)
            {
                _savedSkin = GUI.skin;
                GUI.skin = _skin;
                _skinApplied = true;
            }
        }

        private static void RestoreSkin()
        {
            if (_skinApplied)
            {
                GUI.skin = _savedSkin;
                _skinApplied = false;
            }
        }

        /// <summary>由 ControlPanel.OnGUI 调用（在 MpHub 绘制之前）。</summary>
        internal static void Draw()
        {
            EnsureSkin();
            ApplySkin();
            try
            {
                if (_expanded) DrawExpanded();
                else DrawCollapsed();
                if (_wantFocus)
                {
                    GUI.FocusControl(_focusName);
                    _wantFocus = false;
                }
            }
            finally
            {
                RestoreSkin();
            }
        }

        private static void DrawCollapsed()
        {
            if (_lines.Count == 0) return;
            ChatLine last = _lines[_lines.Count - 1];

            // 收起态：屏幕最右侧一条窄提示，显示最近一条消息缩略
            float w = Mathf.Min(300f, Screen.width * 0.32f);
            float h = 30f;
            Rect r = new Rect(Screen.width - w - 6f, Screen.height - h - 8f, w, h);

            Color prev = GUI.color;
            GUI.color = _unread > 0
                ? new Color(1f, 0.92f, 0.62f, 0.96f)
                : new Color(0.86f, 0.80f, 0.62f, 0.72f);
            GUI.Box(r, "");
            string brief = "<" + last.Tag + "> " + last.Text;
            if (brief.Length > 34) brief = brief.Substring(0, 34) + "…";
            GUI.Label(new Rect(r.x + 8, r.y + 4, r.width - 60, r.height - 8), brief, _smallStyle);
            GUI.Label(new Rect(r.x + r.width - 54, r.y + 4, 50, r.height - 8), "回车聊天", _smallStyle);
            GUI.color = prev;

            // 点击提示条也能展开
            if (Event.current.type == EventType.MouseDown && r.Contains(Event.current.mousePosition))
            {
                ToggleExpanded();
                Event.current.Use();
            }
        }

        private static void DrawExpanded()
        {
            float w = Mathf.Min(360f, Screen.width * 0.42f);
            float h = Mathf.Min(300f, Screen.height * 0.55f);
            Rect panel = new Rect(Screen.width - w - 8f, Screen.height - h - 46f, w, h);

            GUI.Box(panel, "联机聊天  (Enter 发送 · Esc 收起)");

            Rect listRect = new Rect(panel.x + 10, panel.y + 26, panel.width - 20, panel.height - 66);
            GUILayout.BeginArea(listRect);
            _scroll = GUILayout.BeginScrollView(_scroll, false, false);
            int start = Mathf.Max(0, _lines.Count - MaxLines);
            for (int i = start; i < _lines.Count; i++)
            {
                ChatLine ln = _lines[i];
                GUILayout.BeginHorizontal();
                GUILayout.Label("<" + ln.Tag + ">", _tagStyle, GUILayout.Width(58));
                GUILayout.Label(ln.Text, _lineStyle);
                GUILayout.EndHorizontal();
            }
            GUILayout.EndScrollView();
            GUILayout.EndArea();

            // 输入框 + 发送按钮
            Rect inputRect = new Rect(panel.x + 10, panel.y + panel.height - 34, panel.width - 76, 26);
            Rect sendRect = new Rect(inputRect.x + inputRect.width + 6, inputRect.y, 60, 26);
            GUI.SetNextControlName(_focusName);
            _input = GUI.TextField(inputRect, _input);
            if (GUI.Button(sendRect, "发送")) SendLocal();
            // 键盘（回车发送 / Esc 收起）统一由 ControlPanel.Update -> HandleInput 处理
        }
    }
}
