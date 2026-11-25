using System;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.9.0 整屏「联机中心」。替代原来可拖动的小弹窗，做成占满屏幕的分页界面：
    ///   连接 / 玩法规则 / 配置档 / 说明
    /// 由主菜单原生按钮或 F7 打开。绘制逻辑由 ControlPanel.OnGUI 转发（OnGUI 必须在活着的
    /// MonoBehaviour 里调，MpHub 本身只是静态绘制器）。
    /// </summary>
    internal static class MpHub
    {
        internal static bool Visible;

        private enum Tab { Connection = 0, Rules = 1, Profiles = 2, Saves = 3, Help = 4 }
        private static readonly string[] TabNames = { "连接", "玩法规则", "配置档", "存档", "说明" };
        private static Tab _tab = Tab.Connection;

        private static Rect _windowRect = new Rect(0, 0, 0, 0);
        private static bool _layoutReady;
        private static Vector2 _scroll;
        private static GUIStyle _titleStyle;
        private static GUIStyle _hStyle;
        private static GUIStyle _smallStyle;
        private static GUIStyle _bodyStyle;
        private static GUIStyle _tabOnStyle;
        private static GUIStyle _tabOffStyle;

        // v1.9.1 暗金哥特皮肤：程序化纯色纹理 + 临时替换 GUI.skin
        private static GUISkin _skin;
        private static Texture2D _texWindow, _texBox, _texButton, _texButtonHover, _texButtonActive;
        private static Texture2D _texField, _texToolbar, _texToolbarOn, _texScroll;
        private static GUISkin _savedSkin;
        private static bool _skinApplied;

        // 连接页输入缓存（与 ControlPanel 分离，避免耦合实例）
        private static string _ipInput = "127.0.0.1";
        private static int _presetIndex;
        private static readonly string[] _presets = { "127.0.0.1", "localhost" };
        private static bool _ipLoaded;

        // 配置档页输入
        private static string _newProfileName = "预设 1";
        private static string _renameBuffer = "";
        private static int _renamingIndex = -1;
        private static string _profileMsg = "";

        /// <summary>关闭时的可选回调（由暂停菜单 Tab 注册，用于把选中 Tab 复位到 Options）。</summary>
        internal static Action OnClosed;

        internal static void Open()
        {
            Visible = true;
            if (!_ipLoaded)
            {
                _ipInput = Plugin.HostAddress;
                _ipLoaded = true;
            }
            Plugin.Log.LogInfo("[HUB] 联机中心打开");
        }

        internal static void Close()
        {
            Visible = false;
            Plugin.Log.LogInfo("[HUB] 联机中心关闭");
            try
            {
                if (OnClosed != null) OnClosed();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[HUB] OnClosed 回调异常: " + ex.Message);
            }
        }

        /// <summary>由 ControlPanel.Update 每帧调用一次（非 IMGUI 上下文），处理延迟的 UI 复位。
        /// 绝不在 OnGUI 期间操作 Unity UI（窗口切换/布局重建），否则会卡死。</summary>
        internal static void TickDeferred()
        {
            PauseMenuHubTab.FlushPendingReset();
        }

        internal static void Toggle()
        {
            if (Visible) Close(); else Open();
        }

        /// <summary>由 ControlPanel.OnGUI 在每帧调用。GUI 全局状态复位后调用。</summary>
        internal static void Draw()
        {
            if (!Visible) return;

            EnsureTextures();
            EnsureStyles();
            ApplySkin();

            int w = Screen.width, h = Screen.height;
            float margin = Mathf.Min(40f, w * 0.04f);
            Rect target = new Rect(margin, margin, Mathf.Max(320f, w - margin * 2f), Mathf.Max(240f, h - margin * 2f));
            if (!_layoutReady || _windowRect.width <= 1f)
            {
                _windowRect = target;
                _layoutReady = true;
            }
            else
            {
                // 屏幕尺寸变化时跟随
                _windowRect.x = Mathf.Clamp(_windowRect.x, 0, Mathf.Max(0, w - 80));
                _windowRect.y = Mathf.Clamp(_windowRect.y, 0, Mathf.Max(0, h - 80));
                _windowRect.width = Mathf.Clamp(_windowRect.width, 320f, w);
                _windowRect.height = Mathf.Clamp(_windowRect.height, 240f, h);
            }

            try
            {
                // 柔和暗幕：只轻微压暗背景，不再整屏死黑（配合暗金面板本身即可读）
                GUI.color = new Color(0f, 0f, 0f, 0.35f);
                GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), Texture2D.whiteTexture);
                GUI.color = Color.white;

                _windowRect = GUI.Window(778899, _windowRect, DrawWindowBody,
                    "  RL2MP 联机中心   ·   v" + Plugin.VersionText);
            }
            finally
            {
                RestoreSkin();
            }
        }

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
            if (_texWindow != null) return;
            // Rogue Legacy 2 风格：深棕黑底 + 黄铜金边 + 羊皮纸金文字
            _texWindow = MakeTex(new Color(0.09f, 0.07f, 0.06f, 0.97f));
            _texBox = MakeTex(new Color(0.14f, 0.11f, 0.09f, 0.92f));
            _texButton = MakeTex(new Color(0.20f, 0.15f, 0.10f, 0.98f));
            _texButtonHover = MakeTex(new Color(0.32f, 0.24f, 0.12f, 1f));
            _texButtonActive = MakeTex(new Color(0.42f, 0.31f, 0.14f, 1f));
            _texField = MakeTex(new Color(0.06f, 0.05f, 0.04f, 0.98f));
            _texToolbar = MakeTex(new Color(0.15f, 0.11f, 0.08f, 0.98f));
            _texToolbarOn = MakeTex(new Color(0.45f, 0.33f, 0.13f, 1f));
            _texScroll = MakeTex(new Color(0.11f, 0.08f, 0.06f, 0.95f));
        }

        private static void ApplySkin()
        {
            EnsureTextures();
            if (_skin == null)
            {
                _skin = UnityEngine.Object.Instantiate(GUI.skin);
                _skin.name = "RL2MP_DarkGold";

                Color gold = new Color(0.93f, 0.78f, 0.42f);      // 黄铜金文字
                Color parchment = new Color(0.90f, 0.85f, 0.70f); // 羊皮纸正文

                // 窗口
                _skin.window.normal.background = _texWindow;
                _skin.window.normal.textColor = gold;
                _skin.window.fontStyle = FontStyle.Bold;
                _skin.window.fontSize = 16;
                _skin.window.alignment = TextAnchor.UpperCenter;
                _skin.window.padding = new RectOffset(12, 12, 28, 12);

                // 分组框
                _skin.box.normal.background = _texBox;
                _skin.box.normal.textColor = parchment;
                _skin.box.padding = new RectOffset(8, 8, 6, 6);

                // 按钮
                _skin.button.normal.background = _texButton;
                _skin.button.normal.textColor = gold;
                _skin.button.hover.background = _texButtonHover;
                _skin.button.hover.textColor = new Color(1f, 0.90f, 0.60f);
                _skin.button.active.background = _texButtonActive;
                _skin.button.active.textColor = Color.white;
                _skin.button.focused.textColor = gold;
                _skin.button.fontStyle = FontStyle.Bold;
                _skin.button.alignment = TextAnchor.MiddleCenter;

                // 输入框
                _skin.textField.normal.background = _texField;
                _skin.textField.normal.textColor = parchment;
                _skin.textField.focused.background = _texField;
                _skin.textField.focused.textColor = Color.white;
                _skin.textField.padding = new RectOffset(6, 6, 4, 4);

                // Toggle / 标签 / 滚动视图（Toolbar 内部复用 button 样式，已在上设置）
                _skin.toggle.normal.textColor = parchment;
                _skin.label.normal.textColor = parchment;
                _skin.scrollView.normal.background = _texScroll;
            }
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

        private static void EnsureStyles()
        {
            Color gold = new Color(0.95f, 0.80f, 0.44f);
            Color parchment = new Color(0.90f, 0.85f, 0.72f);
            if (_titleStyle == null)
            {
                _titleStyle = new GUIStyle(GUI.skin.label);
                _titleStyle.fontSize = 19;
                _titleStyle.fontStyle = FontStyle.Bold;
                _titleStyle.normal.textColor = gold;
            }
            if (_hStyle == null)
            {
                _hStyle = new GUIStyle(GUI.skin.label);
                _hStyle.fontSize = 15;
                _hStyle.fontStyle = FontStyle.Bold;
                _hStyle.normal.textColor = gold;
            }
            if (_smallStyle == null)
            {
                _smallStyle = new GUIStyle(GUI.skin.label);
                _smallStyle.fontSize = 12;
                _smallStyle.wordWrap = true;
                _smallStyle.normal.textColor = parchment;
            }
            if (_bodyStyle == null)
            {
                _bodyStyle = new GUIStyle(GUI.skin.label);
                _bodyStyle.fontSize = 13;
                _bodyStyle.wordWrap = true;
                _bodyStyle.normal.textColor = parchment;
            }
            if (_tabOnStyle == null)
            {
                _tabOnStyle = new GUIStyle(GUI.skin.button);
                _tabOnStyle.normal.background = _texToolbarOn;
                _tabOnStyle.normal.textColor = new Color(1f, 0.95f, 0.75f);
                _tabOnStyle.fontStyle = FontStyle.Bold;
                _tabOnStyle.fontSize = 14;
            }
            if (_tabOffStyle == null)
            {
                _tabOffStyle = new GUIStyle(GUI.skin.button);
                _tabOffStyle.normal.background = _texToolbar;
                _tabOffStyle.normal.textColor = gold;
                _tabOffStyle.fontStyle = FontStyle.Bold;
                _tabOffStyle.fontSize = 14;
                _tabOffStyle.hover.background = _texButtonHover;
                _tabOffStyle.hover.textColor = Color.white;
            }
        }

        private static void DrawWindowBody(int id)
        {
            // 只允许拖标题栏一小块，避免误拖整个界面
            GUI.DragWindow(new Rect(0, 0, _windowRect.width - 120f, 24f));

            GUILayout.BeginVertical();

            // 右上角关闭
            GUILayout.BeginHorizontal();
            GUILayout.Label("联机中心", _titleStyle);
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("关闭 (F7)", GUILayout.Width(110), GUILayout.Height(26)))
            {
                Close();
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                return;
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            // 自定义分页按钮：选中态用金底，风格统一
            GUILayout.BeginHorizontal();
            for (int i = 0; i < TabNames.Length; i++)
            {
                GUIStyle tabStyle = i == (int)_tab ? _tabOnStyle : _tabOffStyle;
                if (GUILayout.Button(TabNames[i], tabStyle, GUILayout.Height(30)) && i != (int)_tab)
                {
                    _tab = (Tab)i;
                    _scroll = Vector2.zero;
                }
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            _scroll = GUILayout.BeginScrollView(_scroll, false, true);
            switch (_tab)
            {
                case Tab.Connection: DrawConnection(); break;
                case Tab.Rules: DrawRules(); break;
                case Tab.Profiles: DrawProfiles(); break;
                case Tab.Saves: SaveInfoView.Draw(_hStyle, _bodyStyle, _smallStyle); break;
                case Tab.Help: DrawHelp(); break;
            }
            GUILayout.EndScrollView();

            GUILayout.EndVertical();
        }

        // ---------------- 连接页 ----------------

        private static void DrawConnection()
        {
            GUILayout.Label("── 网络状态 ──", _hStyle);
            GUILayout.Label("状态: " + ControlPanel.GetStatusText());
            GUILayout.Space(6);

            GUILayout.Label("主机 IP 地址（客机连接时用）:", _hStyle);
            GUILayout.BeginHorizontal();
            _ipInput = GUILayout.TextField(_ipInput, GUILayout.Width(220));
            if (GUILayout.Button("保存 IP", GUILayout.Width(90)))
            {
                string trimmed = _ipInput == null ? "" : _ipInput.Trim();
                Plugin.HostAddressCfg.Value = trimmed;
                _ipInput = trimmed;
                Plugin.Log.LogInfo("[HUB] 保存 IP -> '" + trimmed + "'");
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("快速选择:", GUILayout.Width(70));
            for (int i = 0; i < _presets.Length; i++)
            {
                GUIStyle ps = i == _presetIndex ? _tabOnStyle : _tabOffStyle;
                if (GUILayout.Button(_presets[i], ps, GUILayout.Width(110)) && i != _presetIndex)
                {
                    _presetIndex = i;
                    _ipInput = _presets[_presetIndex];
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(8);

            GUILayout.Label("── 启动 / 连接 ──", _hStyle);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("启动主机 (小键盘2)", GUILayout.Height(36)))
            {
                if (NetworkManager.Instance != null) NetworkManager.Instance.StartHost();
            }
            if (GUILayout.Button("连接主机 (小键盘3)", GUILayout.Height(36)))
            {
                if (NetworkManager.Instance != null) NetworkManager.Instance.StartClient();
            }
            if (GUILayout.Button("停止网络 (小键盘4)", GUILayout.Height(36)))
            {
                if (NetworkManager.Instance != null) NetworkManager.Instance.Stop();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);

            GUILayout.Label("── 暂停行为 ──", _hStyle);
            GUILayout.Label("联机时打开 ESC 菜单不会冻结游戏世界（已强制开启），怪物/血量继续同步。", _smallStyle);
            GUILayout.Space(10);

            // v2.0.1：协作"同世界"——协作模式自动开启，无需开关
            GUILayout.Label("── 协作同世界（双人协作模式自动开启）──", _hStyle);
            GUILayout.Label("● 全员换房：任一人过门，所有人一起进新房", _smallStyle);
            GUILayout.Label("● 全员场景切换：一起回家升级 / 一起上船开下一把", _smallStyle);
            GUILayout.Label("● Boss 进度：以主机为准，客机门状态自动对齐（收益仍各自存档）", _smallStyle);
            GUILayout.Label("● 怪物位置：主机权威实时同步", _smallStyle);
            GUILayout.Label("以上仅在「玩法=双人协作」时生效；竞速/单人完全不受影响。", _smallStyle);
            GUILayout.Space(10);

            // v1.9.4：在线玩家列表 + 房主踢人（含幽灵槽位清理）
            DrawPlayerKickList();

            GUILayout.Label("── 调试 ──", _hStyle);
            GUILayout.BeginHorizontal();
            bool dbg = GUILayout.Toggle(Plugin.DebugMode, Plugin.DebugMode ? " Debug 模拟: 开" : " Debug 模拟: 关", GUILayout.Width(180));
            if (dbg != Plugin.DebugMode)
            {
                Plugin.DebugModeCfg.Value = dbg;
                Plugin.SetDebugMode(dbg);
                if (NetworkManager.Instance != null)
                {
                    if (dbg) NetworkManager.Instance.StartDebugSim();
                    else NetworkManager.Instance.StopDebugSim();
                }
            }
            if (GUILayout.Button("测试倒地 F9", GUILayout.Width(120))) DownedState.GoDown("HUB test");
            if (GUILayout.Button("复活 F10", GUILayout.Width(100))) DownedState.Revive("HUB test");
            GUILayout.EndHorizontal();
        }

        // ---------------- v1.9.4：在线玩家 + 房主踢人 ----------------

        private static void DrawPlayerKickList()
        {
            var nm = NetworkManager.Instance;
            GUILayout.Label("── 在线玩家 / 房主管理 ──", _hStyle);
            if (nm == null || !nm.Connected)
            {
                GUILayout.Label("（未联机）", _smallStyle);
                GUILayout.Space(6);
                return;
            }

            bool host = nm.IsHost;
            foreach (var slot in Roster.All)
            {
                if (slot == null) continue;
                GUILayout.BeginHorizontal();
                string status = slot.IsLocal ? "（你）" : (slot.Offline ? "（掉线/幽灵）" : "");
                string ip = string.IsNullOrEmpty(slot.Ip) ? "" : "  " + slot.Ip;
                GUILayout.Label(slot.Tag + status + ip, _bodyStyle, GUILayout.MinWidth(140));

                if (host && !slot.IsLocal)
                {
                    if (slot.Offline)
                    {
                        // 幽灵槽位：直接清理
                        if (GUILayout.Button("清除", GUILayout.Width(64)))
                        {
                            nm.RemoveGhost(slot.Id);
                            Plugin.Log.LogInfo("[HUB] 清除幽灵槽位 P" + slot.Id);
                        }
                    }
                    else
                    {
                        if (GUILayout.Button("踢出", GUILayout.Width(64)))
                        {
                            nm.KickPlayer(slot.Id);
                            Plugin.Log.LogInfo("[HUB] 踢出 P" + slot.Id);
                        }
                    }
                }
                GUILayout.EndHorizontal();
            }
            if (!host)
                GUILayout.Label("只有房主能踢人。", _smallStyle);
            GUILayout.Space(6);
        }

        // ---------------- 规则页（复用 ControlPanel 的静态绘制） ----------------

        private static void DrawRules()
        {
            ControlPanel.DrawRaceSection();
            GUILayout.Space(10);
            ControlPanel.DrawNewGameHint();
        }

        // ---------------- 配置档页 ----------------

        private static void DrawProfiles()
        {
            GUILayout.Label("把当前整套联机规则存成预设，之后一键载入。主机载入会自动广播给客机。", _smallStyle);
            GUILayout.Space(8);

            GUILayout.Label("── 保存当前为新配置档 ──", _hStyle);
            GUILayout.BeginHorizontal();
            _newProfileName = GUILayout.TextField(_newProfileName, GUILayout.Width(220));
            if (GUILayout.Button("保存当前设置", GUILayout.Width(140)))
            {
                ConfigProfiles.SaveNew(ConfigProfiles.CaptureCurrent(_newProfileName));
                _profileMsg = "已保存 '" + _newProfileName + "'";
            }
            GUILayout.EndHorizontal();
            if (!string.IsNullOrEmpty(_profileMsg))
            {
                GUILayout.Label(_profileMsg, _smallStyle);
            }
            GUILayout.Space(10);

            GUILayout.Label("── 已有配置档 ──", _hStyle);
            var store = ConfigProfiles.Store;
            if (store == null || store.profiles == null || store.profiles.Length == 0)
            {
                GUILayout.Label("（还没有配置档，先在上面保存一套）", _smallStyle);
                return;
            }

            for (int i = 0; i < store.profiles.Length; i++)
            {
                var p = store.profiles[i];
                GUILayout.BeginVertical(GUI.skin.box);
                GUILayout.BeginHorizontal();
                if (_renamingIndex == i)
                {
                    _renameBuffer = GUILayout.TextField(_renameBuffer, GUILayout.Width(200));
                    if (GUILayout.Button("确定", GUILayout.Width(60)))
                    {
                        p.name = string.IsNullOrEmpty(_renameBuffer) ? p.name : _renameBuffer;
                        ConfigProfiles.Overwrite(i, p);
                        _renamingIndex = -1;
                    }
                    if (GUILayout.Button("取消", GUILayout.Width(60))) _renamingIndex = -1;
                }
                else
                {
                    GUILayout.Label("[" + (i + 1) + "] " + (p.name ?? "未命名"), _hStyle);
                    GUILayout.FlexibleSpace();
                }
                GUILayout.EndHorizontal();

                GUILayout.Label("玩法:" + Score.StyleName((PlayStyle)p.style)
                    + "  判定:" + Score.ModeName((RaceGameMode)p.mode)
                    + "  终点:" + Race.BossLabel(p.targetBossId)
                    + "  房间+" + p.roomPoints + " 怪+" + p.enemyPoints + " Boss+" + p.bossPoints
                    + "  死亡-" + p.deathPenalty + "  加时" + ((int)p.penaltyPerDeath) + "s"
                    + "  IP=" + p.hostAddress, _smallStyle);

                GUILayout.BeginHorizontal();
                if (GUILayout.Button("载入", GUILayout.Width(70)))
                {
                    bool ok = ConfigProfiles.Apply(i);
                    _profileMsg = ok ? "已载入 '" + p.name + "'" : "载入失败";
                }
                if (GUILayout.Button("用当前覆盖", GUILayout.Width(110)))
                {
                    var cur = ConfigProfiles.CaptureCurrent(p.name);
                    ConfigProfiles.Overwrite(i, cur);
                    _profileMsg = "已覆盖 '" + p.name + "'";
                }
                if (GUILayout.Button("重命名", GUILayout.Width(80)))
                {
                    _renamingIndex = i;
                    _renameBuffer = p.name ?? "";
                }
                if (GUILayout.Button("删除", GUILayout.Width(70)))
                {
                    ConfigProfiles.Delete(i);
                    _profileMsg = "已删除第 " + (i + 1) + " 套";
                    GUILayout.EndHorizontal();
                    GUILayout.EndVertical();
                    break; // 数组已变，重绘下一帧
                }
                GUILayout.EndHorizontal();
                GUILayout.EndVertical();
                GUILayout.Space(4);
            }
        }

        // ---------------- 说明页 ----------------

        private static void DrawHelp()
        {
            GUILayout.Label("── 玩法切换 ──", _hStyle);
            GUILayout.Label("竞速: 各跑各的，先到终点 Boss 者胜；可选比用时 / 比得分。", _smallStyle);
            GUILayout.Label("双人协作: 两人同图，共享怪物血量，倒地后靠近长按 H 救援；只显示时间不比得分。", _smallStyle);
            GUILayout.Space(8);

            GUILayout.Label("── 联机暂停 ──", _hStyle);
            GUILayout.Label("联机时打开 ESC 菜单不会冻结游戏世界（已强制开启，无需设置）。", _smallStyle);
            GUILayout.Label("这样即使一方在看菜单/调设置，怪物血量、AI、快照对账都继续进行，避免不同步。", _smallStyle);
            GUILayout.Space(8);

            GUILayout.Label("── 公平开局 ──", _hStyle);
            ControlPanel.DrawNewGameHint();
            GUILayout.Space(8);

            GUILayout.Label("── 热键 ──", _hStyle);
            GUILayout.Label("F7 / 小键盘1: 开关联机中心    小键盘2: 启动主机    小键盘3: 连接主机    小键盘4: 停止网络", _smallStyle);
            GUILayout.Label("F9: 测试倒地    F10: 复活    F11: 标记队友倒地    小键盘6: Dump 玩家结构", _smallStyle);
        }
    }
}
