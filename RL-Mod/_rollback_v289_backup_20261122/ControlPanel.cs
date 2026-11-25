using System;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 控制面板 + 热键。挂在 Bootstrap 创建的 RL2MP_Runtime 对象上，
    /// 不再依赖 Plugin 组件（Plugin 组件会被 RL2 启动流程销毁）。
    /// </summary>
    public class ControlPanel : MonoBehaviour
    {
        public static ControlPanel Instance { get; private set; }

        private bool _uiVisible = true;
        private Rect _windowRect = new Rect(20, 20, 360, 380);
        private string _ipInput = "127.0.0.1";
        private int _presetIndex = 0;
        private bool _debugToggle;

        private float _heartbeatTimer;
        private int _heartbeatCount;

        private static GUIStyle _errorStyle;

        private string _lastGuiError;
        private int _guiErrorCount;

        private readonly string[] _ipPresets = new string[]
        {
            "127.0.0.1",
            "localhost",
        };

        private void Awake()
        {
            Instance = this;
            _ipInput = Plugin.HostAddress;
            _debugToggle = Plugin.DebugMode;
            Plugin.Log.LogInfo("[RL2MP] [PANEL] ControlPanel.Awake() - panel component is alive. frame=" + Time.frameCount);
        }

        private void OnDestroy()
        {
            Plugin.Log.LogInfo("[RL2MP] [PANEL] ControlPanel.OnDestroy() - panel component destroyed. frame=" + Time.frameCount);
        }

        private void Update()
        {
            _heartbeatTimer += Time.unscaledDeltaTime;
            if (_heartbeatTimer >= 5f)
            {
                _heartbeatTimer = 0f;
                _heartbeatCount++;
                Plugin.Log.LogInfo("[RL2MP] [HEARTBEAT] #" + _heartbeatCount
                    + " panel alive. uiVisible=" + _uiVisible
                    + " screen=" + Screen.width + "x" + Screen.height
                    + " NetworkManager.Instance=" + (NetworkManager.Instance != null)
                    + " PlayerSync.Instance=" + (PlayerSync.Instance != null));
            }

            Race.Tick();
            Score.Tick();

            // v1.9.2：处理联机中心关闭后的延迟 UI 复位（必须非 IMGUI 上下文）
            MpHub.TickDeferred();

            if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.F7))
            {
                // v1.9.0：改为开关整屏联机中心（原来是开关小弹窗）
                MpHub.Toggle();
                Plugin.Log.LogInfo("[RL2MP] [KEY] 联机中心切换 -> " + MpHub.Visible);
            }
            if (Input.GetKeyDown(KeyCode.Escape) && MpHub.Visible)
            {
                MpHub.Close();
            }

            // v1.9.4：聊天栏键盘（回车展开/发送、Esc 收起）。MpHub 打开时不响应。
            if (!MpHub.Visible)
            {
                ChatPanel.HandleInput();
            }
            else if (ChatPanel.Expanded)
            {
                ChatPanel.Collapse();
            }

            if (Input.GetKeyDown(KeyCode.Keypad2))
            {
                Plugin.Log.LogInfo("[RL2MP] [KEY] Keypad2 -> StartHost()");
                if (NetworkManager.Instance != null) NetworkManager.Instance.StartHost();
            }

            if (Input.GetKeyDown(KeyCode.Keypad3))
            {
                Plugin.Log.LogInfo("[RL2MP] [KEY] Keypad3 -> StartClient() (target='" + Plugin.HostAddress + "')");
                if (NetworkManager.Instance != null) NetworkManager.Instance.StartClient();
            }

            if (Input.GetKeyDown(KeyCode.Keypad4))
            {
                Plugin.Log.LogInfo("[RL2MP] [KEY] Keypad4 -> Stop()");
                if (NetworkManager.Instance != null) NetworkManager.Instance.Stop();
            }

            // 测试用：F9 强制倒地（走真实补丁链路），F10 复活
            if (Input.GetKeyDown(KeyCode.Keypad6))
            {
                Plugin.Log.LogInfo("[RL2MP] [KEY] Keypad6 -> dump player structure");
                if (PlayerSync.Instance != null) PlayerSync.Instance.DumpPlayerStructure();
            }
            if (Input.GetKeyDown(KeyCode.F9))
            {
                Plugin.Log.LogInfo("[RL2MP] [KEY] F9 -> force down (test)");
                DownedState.GoDown("F9 test");
            }

            if (Input.GetKeyDown(KeyCode.F10))
            {
                Plugin.Log.LogInfo("[RL2MP] [KEY] F10 -> revive (test)");
                DownedState.Revive("F10 test");
            }

            if (Input.GetKeyDown(KeyCode.F11))
            {
                Plugin.Log.LogInfo("[RL2MP] [KEY] F11 -> mark all remotes down (test)");
                DownedState.DebugDownAllRemotes();
            }
        }

        private void OnGUI()
        {
            // v1.9.2：不再绘制旧的 IMGUI 诊断条和小控制面板（太丑）。
            // 界面统一收敛到整屏联机中心 MpHub，由 F7 / 主菜单按钮 / ESC 暂停菜单 Tab 打开。
            // 这里只保留 GUI 状态复位 + 转发绘制，热键与 Tick 仍在 Update 里正常运行。

            // 游戏本体可能改过 GUI 的全局状态，这里强制复位，避免联机中心被画到屏幕外/透明
            GUI.matrix = Matrix4x4.identity;
            GUI.color = Color.white;
            GUI.depth = 0;

            // v1.9.4：聊天栏始终绘制（收起且无消息时完全不画，有消息时右下角缩略提示）
            try
            {
                ChatPanel.Draw();
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAT] 绘制异常: " + ex.Message);
            }

            if (!MpHub.Visible) return;

            try
            {
                MpHub.Draw();
            }
            catch (Exception ex)
            {
                string msg = ex.GetType().Name + ": " + ex.Message;
                if (msg != _lastGuiError)
                {
                    _lastGuiError = msg;
                    _guiErrorCount = 0;
                }
                _guiErrorCount++;
                if (_guiErrorCount == 1)
                {
                    Plugin.Log.LogError("[RL2MP] [ONGUI] GUI error: " + msg + "\n" + ex.StackTrace);
                }
                GUI.color = Color.white;
                float w = Mathf.Min(Screen.width - 8f, 1100f);
                GUI.Box(new Rect(2, 24, w, 46), "");
                if (_errorStyle == null) EnsureStyles();
                GUI.Label(new Rect(8, 28, w - 12, 40), "RL2MP 界面错误 (" + _guiErrorCount + "x)\n" + msg, _errorStyle);
            }
        }

        private static void EnsureStyles()
        {
            if (_errorStyle == null)
            {
                _errorStyle = new GUIStyle(GUI.skin.label);
                _errorStyle.fontSize = 13;
                _errorStyle.wordWrap = true;
                _errorStyle.normal.textColor = Color.red;
            }
        }

        private void DrawWindow(int id)
        {
            GUI.DragWindow(new Rect(0, 0, 10000, 20));

            GUILayout.BeginVertical();

            GUILayout.Label("网络状态: " + GetStatusText());

            GUILayout.Space(6);
            GUILayout.Label("主机 IP 地址:");
            GUILayout.BeginHorizontal();
            _ipInput = GUILayout.TextField(_ipInput, GUILayout.Width(180));
            if (GUILayout.Button("保存", GUILayout.Width(60)))
            {
                string trimmed = _ipInput.Trim();
                Plugin.HostAddressCfg.Value = trimmed;
                _ipInput = trimmed;
                Plugin.Log.LogInfo("[RL2MP] [UI] Save IP -> '" + trimmed + "'");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(4);
            GUILayout.BeginHorizontal();
            GUILayout.Label("快速选择:", GUILayout.Width(65));
            int newPreset = GUILayout.Toolbar(_presetIndex, _ipPresets);
            if (newPreset != _presetIndex)
            {
                _presetIndex = newPreset;
                _ipInput = _ipPresets[_presetIndex];
                Plugin.Log.LogInfo("[RL2MP] [UI] Preset: index=" + newPreset + " value='" + _ipInput + "'");
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(6);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("小键盘2 启动主机", GUILayout.Height(28)))
            {
                Plugin.Log.LogInfo("[RL2MP] [UI] '启动主机' -> StartHost()");
                if (NetworkManager.Instance != null) NetworkManager.Instance.StartHost();
            }
            if (GUILayout.Button("小键盘3 连接主机", GUILayout.Height(28)))
            {
                Plugin.Log.LogInfo("[RL2MP] [UI] '连接主机' -> StartClient() (target='" + Plugin.HostAddress + "')");
                if (NetworkManager.Instance != null) NetworkManager.Instance.StartClient();
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("小键盘4 停止网络", GUILayout.Height(24)))
            {
                Plugin.Log.LogInfo("[RL2MP] [UI] '停止网络' -> Stop()");
                if (NetworkManager.Instance != null) NetworkManager.Instance.Stop();
            }

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Label("Debug 模式（无网络模拟远程玩家）:", GUILayout.Width(230));
            bool toggled = GUILayout.Toggle(_debugToggle, _debugToggle ? "开启" : "关闭", GUILayout.Width(60));
            if (toggled != _debugToggle)
            {
                _debugToggle = toggled;
                Plugin.DebugModeCfg.Value = toggled;
                Plugin.SetDebugMode(toggled);
                Plugin.Log.LogInfo("[RL2MP] [UI] Debug toggled -> " + toggled);
                if (toggled)
                {
                    if (NetworkManager.Instance != null) NetworkManager.Instance.StartDebugSim();
                }
                else
                {
                    if (NetworkManager.Instance != null) NetworkManager.Instance.StopDebugSim();
                }
            }
            GUILayout.EndHorizontal();

            if (Plugin.DebugMode)
            {
                GUILayout.Space(4);
                GUILayout.Label("  Debug 运行中 · 模拟玩家 ID=1");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("  添加模拟玩家  "))
                {
                    Plugin.Log.LogInfo("[RL2MP] [UI] '添加模拟玩家' -> DebugAddFakePlayer()");
                    if (NetworkManager.Instance != null) NetworkManager.Instance.DebugAddFakePlayer();
                }
                if (GUILayout.Button("  清除模拟玩家  "))
                {
                    Plugin.Log.LogInfo("[RL2MP] [UI] '清除模拟玩家' -> DebugClearFakePlayers()");
                    if (NetworkManager.Instance != null) NetworkManager.Instance.DebugClearFakePlayers();
                }
                if (GUILayout.Button("  Dump玩家结构  "))
                {
                    Plugin.Log.LogInfo("[RL2MP] [UI] 'Dump玩家结构' -> DumpPlayerStructure()");
                    if (PlayerSync.Instance != null) PlayerSync.Instance.DumpPlayerStructure();
                }
                GUILayout.EndHorizontal();
            }

            GUILayout.Space(8);
            GUILayout.Label(DownedState.LocalDown
                ? "倒地观战中 · 目标=" + (DownedState.SpectateTargetId >= 0 ? DownedState.SpectateTargetId.ToString() : "无")
                  + " · 队友存活 " + DownedState.AliveRemoteCount + "/" + DownedState.RemoteCount
                : "状态正常（F9 可测试倒地）");
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("F9 测试倒地"))
            {
                DownedState.GoDown("UI button");
            }
            if (GUILayout.Button("F10 复活"))
            {
                DownedState.Revive("UI button");
            }
            if (GUILayout.Button("切换观战目标"))
            {
                var t = DownedState.CycleTarget(1);
                Plugin.Log.LogInfo("[RL2MP] [UI] spectate cycle -> " + (t != null ? t.name : "<none>"));
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("F11 标记所有队友倒地（测全员倒下）"))
            {
                DownedState.DebugDownAllRemotes();
            }

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            if (GUILayout.Button(RaceHud.ForceShow ? "收起计分板预览" : "调整计分板位置", GUILayout.Width(130)))
            {
                RaceHud.ForceShow = !RaceHud.ForceShow;
            }
            GUILayout.Label("（显示后可拖动窗口、拖右下角缩放，位置自动记住）");
            GUILayout.EndHorizontal();

            GUILayout.Space(8);
            GUILayout.BeginHorizontal();
            GUILayout.Label("小键盘1 / F7 显示隐藏  |", GUILayout.Width(155));
            GUILayout.Label("小键盘2 主机  3 客户端  4 停止");
            GUILayout.EndHorizontal();

            DrawRaceSection();
            DrawNewGameHint();

            GUILayout.EndVertical();
        }

        // ---------------- 新周目说明 ----------------

        internal static void DrawNewGameHint()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;

            GUILayout.Space(10);
            GUILayout.Label("══ 公平开局（新周目） ══");
            GUILayout.Label("1. 双方各自在自己的存档里配好装备/技能树;");
            GUILayout.Label("2. 主机在游戏里找 Charon 开启新周目(NG+);");
            GUILayout.Label("3. 新地图会自动同步给客机, 双方 Boss/黑箱");
            GUILayout.Label("   标记同时清零 -> 公平起跑。");
        }

        /// <summary>一行 [标签 - 值 +] 的整数调节器（主机计分规则用）。</summary>
        private static bool ScoreIntRow(string label, ref int value, int step, int min, int max, string suffix)
        {
            bool changed = false;
            GUILayout.BeginHorizontal();
            GUILayout.Label(label + suffix, GUILayout.Width(104));
            if (GUILayout.Button("-", GUILayout.Width(24)))
            {
                value = Mathf.Max(min, value - step);
                changed = true;
            }
            GUILayout.Label(value.ToString(), GUILayout.Width(56));
            if (GUILayout.Button("+", GUILayout.Width(24)))
            {
                value = Mathf.Min(max, value + step);
                changed = true;
            }
            GUILayout.EndHorizontal();
            return changed;
        }

        internal static void DrawRaceSection()
        {
            var nm = NetworkManager.Instance;
            bool netReady = nm != null && nm.Connected;
            if (!netReady) return;

            Roster.SyncSelf();

            GUILayout.Space(10);
            GUILayout.Label("══ 竞速模式（最多 " + Roster.MaxPlayers + " 人） ══");

            if (!nm.IsHost)
            {
                // ---------------- 客机视角 ----------------
                if (Race.InSession)
                {
                    GUILayout.Label("终点: " + Race.BossLabel(Race.TargetBossId));
                    DrawRosterTable(false);

                    if (!Race.LocalInGame)
                    {
                        GUILayout.Label("⚠ 你还没进入游戏世界，按准备会自动进图");
                    }
                    if (Race.Phase == RacePhase.WaitingReady)
                    {
                        string label = Race.LocalReady
                            ? "取消准备"
                            : (Race.LocalInGame ? "我准备好了" : "进入游戏并准备");
                        if (GUILayout.Button(label, GUILayout.Height(26)))
                        {
                            Race.ToggleLocalReady();
                        }
                    }
                    if (Race.Phase == RacePhase.Racing
                        && GUILayout.Button("调试: 直接判定我完赛（胜利）", GUILayout.Height(22)))
                    {
                        Race.DebugForceLocalFinish();
                    }
                }
                else
                {
                    GUILayout.Label("等待主机设定终点 Boss...");
                }

                GUILayout.Label("玩法: " + Score.StyleName(Score.Style) + " · 判定: " + Score.ModeName(Score.Mode)
                    + " · " + Score.RuleLine());
                GUILayout.Label(Score.Mode == RaceGameMode.Score
                    ? "死亡立即原地复活，并扣 " + Score.DeathPenalty + " 分"
                    : "死亡立即原地复活，成绩 +" + (int)Race.PenaltyPerDeath + " 秒/次");
                return;
            }

            // ---------------- 主机视角：规则 ----------------
            // v1.7.6：玩法维度（竞速 / 协作）与判定方式正交，只允许开赛前切换
            GUILayout.BeginHorizontal();
            GUILayout.Label("玩法:", GUILayout.Width(40));
            bool canSwitchStyle = Race.Phase == RacePhase.Idle || Race.Phase == RacePhase.WaitingReady;
            GUIStyle raceStyle = Score.Style == PlayStyle.Race ? GUI.skin.box : GUI.skin.button;
            if (GUILayout.Button("竞速", raceStyle, GUILayout.Width(70)) && canSwitchStyle
                && Score.Style != PlayStyle.Race)
            {
                Score.Style = PlayStyle.Race;
                Score.BroadcastConfig();
                Plugin.Log.LogInfo("[RL2MP] [UI] HOST 切换玩法 -> 竞速");
            }
            GUIStyle coopStyle = Score.Style == PlayStyle.CoOp ? GUI.skin.box : GUI.skin.button;
            if (GUILayout.Button("双人协作", coopStyle, GUILayout.Width(80)) && canSwitchStyle
                && Score.Style != PlayStyle.CoOp)
            {
                Score.Style = PlayStyle.CoOp;
                Score.BroadcastConfig();
                Plugin.Log.LogInfo("[RL2MP] [UI] HOST 切换玩法 -> 双人协作");
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(canSwitchStyle
                ? (Score.Style == PlayStyle.CoOp
                    ? "协作: 两人同图、齐门过房、倒地救援（行为规则后续版本启用，当前同竞速）"
                    : "竞速: 各跑各的，先到终点者胜")
                : "玩法已锁定（比赛进行中不可切换，回大厅后可改）");

            // 协作模式不比用时/得分，判定方式与计分规则整块隐藏
            if (Score.Style == PlayStyle.Race)
            {
            GUILayout.BeginHorizontal();
            GUILayout.Label("判定方式:", GUILayout.Width(64));
            GUIStyle timeStyle = Score.Mode == RaceGameMode.Time ? GUI.skin.box : GUI.skin.button;
            if (GUILayout.Button("比用时", timeStyle, GUILayout.Width(76)))
            {
                if (Score.Mode != RaceGameMode.Time)
                {
                    Score.Mode = RaceGameMode.Time;
                    Score.BroadcastConfig();
                }
            }
            GUIStyle scoreStyle = Score.Mode == RaceGameMode.Score ? GUI.skin.box : GUI.skin.button;
            if (GUILayout.Button("比得分", scoreStyle, GUILayout.Width(76)))
            {
                if (Score.Mode != RaceGameMode.Score)
                {
                    Score.Mode = RaceGameMode.Score;
                    Score.BroadcastConfig();
                }
            }
            GUILayout.EndHorizontal();
            GUILayout.Label(Score.Mode == RaceGameMode.Score
                ? "终点 Boss 倒下即结束 -> 比总分（死得多、拿遗物多都会掉分）"
                : "先打死终点 Boss 者胜 -> 比用时（死亡加时计入）");

            if (Score.Mode == RaceGameMode.Time)
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label("死亡加时:", GUILayout.Width(60));
                if (GUILayout.Button("-15s", GUILayout.Width(44)))
                    Race.HostSetPenalty(Race.PenaltyPerDeath - 15f);
                GUILayout.Label(((int)Race.PenaltyPerDeath) + " 秒/次", GUILayout.Width(70));
                if (GUILayout.Button("+15s", GUILayout.Width(44)))
                    Race.HostSetPenalty(Race.PenaltyPerDeath + 15f);
                if (GUILayout.Button("60", GUILayout.Width(36)))
                    Race.HostSetPenalty(60f);
                if (GUILayout.Button("120", GUILayout.Width(40)))
                    Race.HostSetPenalty(120f);
                GUILayout.EndHorizontal();
                GUILayout.Label("死亡立即原地复活，加时直接计入最终成绩");
            }
            else
            {
                GUILayout.Label("计分规则（改动立即同步给所有客机）:");
                if (ScoreIntRow("探索新房间 +", ref Score.RoomPoints, 5, 0, 5000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("击杀怪物 +", ref Score.EnemyPoints, 1, 0, 500, "")) Score.BroadcastConfig();
                if (ScoreIntRow("击败 Boss +", ref Score.BossPoints, 50, 0, 20000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("开普通箱 +", ref Score.ChestBronzePoints, 10, 0, 5000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("开银箱 +", ref Score.ChestSilverPoints, 10, 0, 5000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("开金箱 +", ref Score.ChestGoldPoints, 10, 0, 5000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("开妖精箱 +", ref Score.ChestFairyPoints, 10, 0, 5000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("拾取遗物 -", ref Score.RelicPenalty, 10, 0, 5000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("死亡一次 -", ref Score.DeathPenalty, 25, 0, 10000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("每分钟衰减 -", ref Score.DecayPerMinute, 5, 0, 2000, "")) Score.BroadcastConfig();
                if (ScoreIntRow("金币/分(NG+0) ", ref Score.GoldPerPoint, 1000, 1000, 1000000, "")) Score.BroadcastConfig();
                GUILayout.Label("完赛时把本局金币折算成分：NG+0 为 " + Score.GoldPerPoint
                    + " 金币 = 1 分，当前周目(NG+" + Score.CurrentNgPlus() + ") 实际 "
                    + Score.EffectiveGoldPerPoint() + " 金币 = 1 分");
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("恢复默认", GUILayout.Width(80))) Score.ResetDefaults();
                GUILayout.Label("同一房间/同一个箱子反复进不重复计分");
                GUILayout.EndHorizontal();
            }
            } // if (Score.Style == PlayStyle.Race)

            // 首位完成后给其他人的宽限时间（0 = 立刻结算）
            GUILayout.BeginHorizontal();
            GUILayout.Label("完赛宽限:", GUILayout.Width(60));
            if (GUILayout.Button("-15s", GUILayout.Width(44)))
                Race.FinishGraceSeconds = Mathf.Max(0f, Race.FinishGraceSeconds - 15f);
            GUILayout.Label(((int)Race.FinishGraceSeconds) + " 秒", GUILayout.Width(70));
            if (GUILayout.Button("+15s", GUILayout.Width(44)))
                Race.FinishGraceSeconds = Mathf.Min(600f, Race.FinishGraceSeconds + 15f);
            if (GUILayout.Button("0", GUILayout.Width(30)))
                Race.FinishGraceSeconds = 0f;
            GUILayout.EndHorizontal();
            GUILayout.Label("第一个到终点后，其他人还有多久可以跑完（超时算未完成）");

            // 断线重连宽限（主机侧保留掉线玩家槽位的时长）
            GUILayout.BeginHorizontal();
            GUILayout.Label("重连宽限:", GUILayout.Width(60));
            if (GUILayout.Button("-15s", GUILayout.Width(44)))
                NetworkManager.ReconnectGraceSeconds = Mathf.Max(15f, NetworkManager.ReconnectGraceSeconds - 15f);
            GUILayout.Label(((int)NetworkManager.ReconnectGraceSeconds) + " 秒", GUILayout.Width(70));
            if (GUILayout.Button("+15s", GUILayout.Width(44)))
                NetworkManager.ReconnectGraceSeconds = Mathf.Min(300f, NetworkManager.ReconnectGraceSeconds + 15f);
            if (GUILayout.Button("90", GUILayout.Width(36)))
                NetworkManager.ReconnectGraceSeconds = 90f;
            GUILayout.EndHorizontal();
            GUILayout.Label("有人掉线后，主机保留他的分数/用时多久（期间他可带原身份重连）");

            GUILayout.Label("选择终点 Boss（立即广播给客机）:");
            for (int row = 0; row < Race.BossChoices.Length; row += 4)
            {
                GUILayout.BeginHorizontal();
                for (int i = row; i < Mathf.Min(row + 4, Race.BossChoices.Length); i++)
                {
                    int bossId = Race.BossChoices[i];
                    GUIStyle style = bossId == Race.TargetBossId ? GUI.skin.box : GUI.skin.button;
                    if (GUILayout.Button(Race.BossLabel(bossId), style, GUILayout.Height(24)))
                    {
                        Plugin.Log.LogInfo("[RL2MP] [UI] HOST 选终点 -> " + Race.BossLabel(bossId));
                        Race.HostSetTargetBoss(bossId);
                    }
                }
                GUILayout.EndHorizontal();
            }

            if (!Race.InSession) return;

            // ---------------- 主机视角：大厅 ----------------
            DrawRosterTable(true);

            if (!Race.LocalInGame)
            {
                GUILayout.Label("⚠ 你还没进入游戏世界，无法开跑");
            }

            if (Race.Phase == RacePhase.WaitingReady && GUILayout.Button("我也准备好了", GUILayout.Height(26)))
            {
                Race.ToggleLocalReady();
            }

            if (Race.Phase == RacePhase.Racing)
            {
                if (Race.GraceRemaining >= 0f)
                {
                    GUILayout.Label("已有人在终点，剩余玩家还有 "
                        + Mathf.CeilToInt(Race.GraceRemaining) + " 秒（超时判未完成）");
                }
                if (GUILayout.Button("立即结算（不等人）", GUILayout.Height(22)))
                {
                    Plugin.Log.LogInfo("[RL2MP] [UI] HOST 立即结算");
                    foreach (var sl in Roster.All)
                    {
                        if (!sl.Finished) sl.Dnf = true;
                    }
                    Race.Arbitrate();
                }
                if (GUILayout.Button("调试: 直接判定我完赛（胜利）", GUILayout.Height(22)))
                {
                    Race.DebugForceLocalFinish();
                }
            }

            if (GUILayout.Button("结束对局并断开所有人", GUILayout.Height(24)))
            {
                Race.HostEndRace();
            }
        }

        /// <summary>大厅名单：每人 ready / 在不在游戏里 / 用时 / 得分。</summary>
        internal static void DrawRosterTable(bool isHost)
        {
            GUILayout.Space(4);
            if (Roster.Count == 0)
            {
                GUILayout.Label("（名单为空）");
                return;
            }

            bool scoreMode = Score.Mode == RaceGameMode.Score;
            var ranked = Roster.Ranked(scoreMode);
            for (int i = 0; i < ranked.Count; i++)
            {
                var s = ranked[i];
                string state;
                if (s.Finished && !s.Dnf)
                {
                    state = (scoreMode ? (s.Score + "分 用时" + Race.FormatTime(s.FinishTime))
                                       : (Race.FormatTime(s.FinishTime) + " " + s.Score + "分"));
                }
                else if (s.Dnf) state = "未完成 " + s.Score + "分";
                else if (Race.Phase == RacePhase.Racing || Race.Phase == RacePhase.Countdown)
                    state = "跑图中 死" + s.Deaths + " " + s.Score + "分";
                else state = (s.Ready ? (s.InWorld ? "已准备" : "已准备(在菜单)") : "未准备");

                GUILayout.Label((i + 1) + ". " + s.Display + " (id=" + s.Id + ")   " + state);
            }
            GUILayout.Label("已就绪 " + Roster.ReadyCount() + "/" + Roster.Count
                + "   你: " + (Race.LocalReady ? "✔" : "…"));
        }

        internal static string GetStatusText()
        {
            if (NetworkManager.LastError != null)
                return "网络错误: " + NetworkManager.LastError;
            if (Plugin.DebugMode) return "DEBUG SIM";
            if (NetworkManager.Instance == null) return "未初始化";
            var nm = NetworkManager.Instance;
            if (nm.IsReconnecting)
            {
                return "重连中 第" + nm.ReconnectAttempt + " 次尝试（已 " + nm.ReconnectElapsed.ToString("F0")
                    + "s，会话已保留）...";
            }
            if (nm.Connected)
            {
                string ping = "";
                int p = nm.GetAveragePing();
                if (p >= 0) ping = "  " + p + "ms";
                string role = nm.IsHost
                    ? "HOST (ID=" + nm.LocalPlayerId + ")"
                    : "CLIENT (ID=" + nm.LocalPlayerId + ")";
                return role + ping;
            }
            return "未连接";
        }
    }
}
