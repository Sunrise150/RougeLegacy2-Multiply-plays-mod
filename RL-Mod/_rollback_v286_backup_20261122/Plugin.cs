using System;
using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace RL2Multiplayer
{
    [BepInPlugin("com.rl2.mp", "Rogue Legacy 2 Multiplayer", "2.8.6")]
    public class Plugin : BaseUnityPlugin
    {
        internal static ManualLogSource Log;
        internal static Plugin Instance;

        public static ConfigEntry<string> HostAddressCfg;
        public static ConfigEntry<string> PlayerNameCfg;
        public static ConfigEntry<bool> DebugModeCfg;
        public static ConfigEntry<bool> KeepWorldRunningCfg;
        public static ConfigEntry<bool> CoOpRoomSyncCfg;
        public static ConfigEntry<bool> CoOpSceneSyncCfg;

        public static string HostAddress { get { return HostAddressCfg != null ? HostAddressCfg.Value : "127.0.0.1"; } }

        /// <summary>
        /// 本机在联机中显示的名字（聊天标签/大厅名单）。限制长度并剥掉控制字符，
        /// 避免把聊天面板或 GUI 撑坏；空值回退为"玩家"。
        /// </summary>
        public static string PlayerName
        {
            get
            {
                string s = PlayerNameCfg != null ? PlayerNameCfg.Value : "";
                if (s == null) s = "";
                s = NetValidation.Text(s.Trim());
                if (s.Length > 16) s = s.Substring(0, 16);
                return string.IsNullOrEmpty(s) ? "玩家" : s;
            }
        }

        /// <summary>统一版本号文本，供 UI 显示。</summary>
        public static string VersionText { get { return "2.8.6"; } }

        public static bool DebugMode { get; private set; }

        internal static void SetDebugMode(bool value) { DebugMode = value; }

        /// <summary>
        /// Harmony 补丁失败时的完整错误描述。
        /// HarmonyX 把所有补丁异常统一包成 "IL Compile Error (unknown location)"，
        /// 真正的原因在 InnerException 里 —— 只打 ex.Message 永远查不出问题。
        /// </summary>
        internal static string PatchError(Exception ex)
        {
            try
            {
                var sb = new System.Text.StringBuilder();
                var cur = ex;
                int depth = 0;
                while (cur != null && depth < 4)
                {
                    if (depth > 0) sb.Append("  <- 内层: ");
                    sb.Append('[').Append(cur.GetType().Name).Append("] ").Append(cur.Message);
                    cur = cur.InnerException;
                    depth++;
                }
                var st = ex.StackTrace;
                if (st != null)
                {
                    var first = st.Split('\n');
                    if (first.Length > 0) sb.Append("  | at ").Append(first[0].Trim());
                }
                if (ex.InnerException != null && ex.InnerException.StackTrace != null)
                {
                    var ist = ex.InnerException.StackTrace.Split('\n');
                    if (ist.Length > 0) sb.Append("  | inner at ").Append(ist[0].Trim());
                }
                return sb.ToString();
            }
            catch { return ex.Message; }
        }

        private Harmony _harmony;

        private void Awake()
        {
            Log = Logger;
            Instance = this;

            Log.LogInfo("[RL2MP] === Plugin Awake START (v2.8.6) ===");

            Log.LogInfo("[RL2MP] Binding BepInEx config entries...");
            HostAddressCfg = Config.Bind<string>("Network", "HostAddress", "127.0.0.1", "主机 IP 地址（客户端连接时使用）");
            PlayerNameCfg = Config.Bind<string>("Network", "PlayerName", "玩家",
                "联机时显示的名字（聊天和名单里用来自我介绍；两端各自设置）");
            DebugModeCfg = Config.Bind<bool>("Debug", "DebugMode", false, "单人调试模式：模拟一个远程玩家，无需真实网络");
            KeepWorldRunningCfg = Config.Bind<bool>("CoOp", "KeepWorldRunningWhenPaused", true,
                "联机时打开暂停菜单是否保持游戏世界继续运行（推荐开启：避免暂停导致怪物血量/AI 不同步）");
            CoOpRoomSyncCfg = Config.Bind<bool>("CoOp", "EnableRoomSync", true,
                "协作全员换房：任一人过门，所有人一起进新房（协作模式自动开启；竞速/单人无效）");
            CoOpSceneSyncCfg = Config.Bind<bool>("CoOp", "EnableSceneSync", true,
                "协作全员场景切换：主机回家/上船/开新局时客机跟随（协作模式自动开启；竞速/单人无效）");

            DebugMode = DebugModeCfg.Value;
            PauseCompat.KeepWorldRunning = KeepWorldRunningCfg.Value;
            Log.LogInfo("[RL2MP] Config ready. HostAddress='" + HostAddress + "', DebugMode=" + DebugMode
                + ", KeepWorldRunning=" + PauseCompat.KeepWorldRunning);

            Log.LogInfo("[RL2MP] Creating Harmony instance 'com.rl2.mp'...");
            _harmony = new Harmony("com.rl2.mp");
            _harmony.PatchAll();
            DownedPatches.Apply(_harmony);
            CoOpRescue.Apply(_harmony);
            EnemySync.Apply(_harmony);
            RoomTravelSync.Apply(_harmony);  // v2.0.0：协作全员换房（默认关闭）
            SceneChangeSync.Apply(_harmony); // v2.0.0：协作全员场景切换（默认关闭）
            SkillTreeSync.Apply(_harmony);    // v2.0.2：协作技能树共享（取 max 并集，写各自存档）
            PlayerHurtSync.Apply(_harmony);   // v2.1.0：分身受击转发（本地受伤时间戳用于去重）
            PauseDamageFix.Apply(_harmony);   // v2.2.1：暂停菜单期间不再把玩家判成无敌
            SeedSync.Apply(_harmony);
            RacePatches.Apply(_harmony);
            ScorePatches.Apply(_harmony);
            ProjectileSync.Apply(_harmony);
            DashFxSync.Apply(_harmony);
            PauseCompat.Apply(_harmony);   // v1.9.0：联机暂停不冻结世界
            MainMenuHubButton.Apply(_harmony);  // v1.9.0：主菜单原生「联机中心」按钮
            PauseMenuHubTab.Apply(_harmony);    // v1.9.3：ESC 暂停菜单「联机中心」Tab
            RoomFix.Apply(_harmony);
            MapSync.ApplyPatches(_harmony);
            RaceRulesSync.ApplySaveProtection(_harmony);
            Bootstrap.ApplyLateInitPatch(_harmony);
            Log.LogInfo("[RL2MP] Harmony patch applied.");

            // RL2 的启动流程（OnGameLoadManager.Run -> Application..cctor -> Chainloader）末尾会把
            // 这期间创建的所有对象销毁掉，连 DontDestroyOnLoad 都救不回来。
            // 所以这里只挂“静态回调”（不依赖任何 GameObject 存活），等启动结束后再由 Bootstrap 建对象。
            Log.LogInfo("[RL2MP] Registering static bootstrap hooks...");
            SceneManager.sceneLoaded += Bootstrap.OnSceneLoaded;
            Application.onBeforeRender += Bootstrap.Tick;
            Application.quitting += Bootstrap.OnQuitting;

            // 依赖解析保险：mod 的依赖（如 LiteNetLib 依赖 System.Runtime.Serialization）
            // 游戏环境可能没有，mono 探测不到插件目录时由这里兜底加载。
            // DLL 来源：BepInEx/plugins 目录（或 mod 同目录）。
            AppDomain.CurrentDomain.AssemblyResolve += Bootstrap.ResolveModDependency;

            Bootstrap.Ensure("Plugin.Awake");
            Bootstrap.DumpSceneState("after-awake");

            Log.LogInfo("[RL2MP] === Plugin Awake DONE ===");
        }

        // 注意：这个 OnDestroy 在 RL2 启动阶段就会触发（不是退出游戏），所以这里不做任何清理/反补丁，
        // 真正的清理放到 Application.quitting。
        private void OnDestroy()
        {
            Log.LogInfo("[RL2MP] === Plugin.OnDestroy (RL2 启动清理或退出；运行对象由 Bootstrap 托管) ===");
        }
    }

    /// <summary>
    /// 运行时对象（NetworkManager / PlayerSync / ControlPanel）的托管创建器。
    /// 全部是静态方法，即使 Plugin 组件被销毁，挂在上面的回调依然有效。
    /// </summary>
    internal static class Bootstrap
    {
        /// <summary>兜底依赖解析：从 RL-Mod.dll 所在目录（plugins）加载缺失程序集。</summary>
        internal static Assembly ResolveModDependency(object sender, ResolveEventArgs args)
        {
            try
            {
                // 只处理缺"裸名"的请求；跳过我们自己已加载的
                string name = new AssemblyName(args.Name).Name;

                // 搜索插件目录（RL-Mod.dll 同目录）
                string selfDir = Path.GetDirectoryName(typeof(Plugin).Assembly.Location);
                if (string.IsNullOrEmpty(selfDir)) return null;

                string candidate = Path.Combine(selfDir, name + ".dll");
                if (!File.Exists(candidate)) return null;

                Assembly asm = Assembly.LoadFrom(candidate);
                Plugin.Log.LogInfo("[RL2MP] [RESOLVE] 缺失依赖 '" + args.Name + "' -> 从 " + candidate + " 加载成功。");
                return asm;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RESOLVE] 解析 '" + args.Name + "' 失败: " + ex.Message);
                return null;
            }
        }

        private static GameObject _root;
        private static float _lastTryTime;
        private static int _attempts;

        internal static GameObject Root { get { return _root; } }
        internal static bool IsAlive { get { return _root != null; } }

        internal static void Ensure(string reason)
        {
            if (_root != null) return;

            _attempts++;
            try
            {
                var go = new GameObject("RL2MP_Runtime");
                UnityEngine.Object.DontDestroyOnLoad(go);

                go.AddComponent<NetworkManager>();
                go.AddComponent<PlayerSync>();
                go.AddComponent<ControlPanel>();
                go.AddComponent<DownedManager>();
                go.AddComponent<RaceHud>();

                _root = go;
                Plugin.Log.LogInfo("[RL2MP] [BOOT] Runtime root created (" + reason + "), attempt=" + _attempts
                    + ", id=" + go.GetInstanceID());
            }
            catch (Exception ex)
            {
                Plugin.Log.LogError("[RL2MP] [BOOT] Failed to create runtime root (" + reason + "): " + ex);
            }
        }

        internal static void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Plugin.Log.LogInfo("[RL2MP] [BOOT] sceneLoaded '" + scene.name + "' (mode=" + mode
                + "), root alive=" + IsAlive + " -> ensure");
            Ensure("sceneLoaded:" + scene.name);
        }

        /// <summary>看门狗：万一对象又被游戏清掉，1 秒内重建。
        /// v1.8.2：不再泵网络（渲染路径做网络收发+全场景扫描会卡顿；Update 用
        /// unscaledDeltaTime 暂停时照常跑，本就不需要第二驱动）。</summary>
        internal static void Tick()
        {
            // v1.8.2：不再在 onBeforeRender 里泵网络。onBeforeRender 处于渲染路径，
            // 在这里做 PollEvents + FindObjectsOfType(全场景扫描) 会阻塞渲染→严重卡顿。
            // NetworkManager.Update 用 unscaledDeltaTime，游戏暂停(timeScale=0)时照样每帧跑，
            // 收发本就不中断，无需第二驱动。这里只保留看门狗（对象被清掉时重建）。
            if (_root != null) return;

            float now = Time.realtimeSinceStartup;
            if (now - _lastTryTime < 1f) return;
            _lastTryTime = now;

            Ensure("watchdog");
        }

        internal static void OnQuitting()
        {
            Plugin.Log.LogInfo("[RL2MP] Application.quitting -> cleanup");
            try
            {
                if (NetworkManager.Instance != null)
                {
                    NetworkManager.Instance.StopDebugSim();
                    NetworkManager.Instance.Stop();
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] cleanup exception: " + ex.Message);
            }
        }

        /// <summary>
        /// RL2 的启动入口是 OnGameLoadManager.Run()。给它挂一个 postfix，
        /// 在启动流程真正结束之后再建运行时对象。
        /// </summary>
        internal static void ApplyLateInitPatch(Harmony harmony)
        {
            try
            {
                var type = AccessTools.TypeByName("OnGameLoadManager");
                if (type == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [BOOT] OnGameLoadManager type not found, fallback to sceneLoaded only.");
                    return;
                }

                var method = AccessTools.Method(type, "Run");
                if (method == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [BOOT] OnGameLoadManager.Run not found, fallback to sceneLoaded only.");
                    return;
                }

                var postfix = new HarmonyMethod(typeof(Bootstrap).GetMethod("OnGameLoadManagerRunPostfix",
                    BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public));

                harmony.Patch(method, postfix: postfix);
                Plugin.Log.LogInfo("[RL2MP] [BOOT] Patched OnGameLoadManager.Run postfix (late init).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [BOOT] ApplyLateInitPatch failed: " + ex.Message);
            }
        }

        private static void OnGameLoadManagerRunPostfix()
        {
            Plugin.Log.LogInfo("[RL2MP] [BOOT] OnGameLoadManager.Run finished -> ensure runtime root");
            Ensure("OnGameLoadManager.Run postfix");
        }

        internal static void DumpSceneState(string tag)
        {
            try
            {
                var scene = SceneManager.GetActiveScene();
                Plugin.Log.LogInfo("[RL2MP] [BOOT] (" + tag + ") activeScene='" + scene.name
                    + "' loaded=" + scene.isLoaded
                    + " frame=" + Time.frameCount
                    + " rootCount=" + scene.rootCount
                    + " screen=" + Screen.width + "x" + Screen.height);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [BOOT] DumpSceneState failed: " + ex.Message);
            }
        }
    }
}
