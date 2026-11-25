using System;
using System.Reflection;
using HarmonyLib;
using SceneManagement_RL;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.0.0 协作模式：全员场景切换（一起回家 / 一起上船 / 一起开新局）。
    ///
    /// 背景：RL2 一把结束后走 死亡窗口 -> Lineage(选继承人) -> Town(技能树) -> 上船 -> World。
    /// 两端各自存档、各自跑这套流程，容易分叉（主机回家了客机还在死亡窗口）。
    /// 本模块让任一端调用 SceneLoader_RL.LoadScene 时广播 sceneId+transitionId，
    /// 其他端跟随切换，保证两端始终在同一场景，能一起升级、一起上船开下一把。
    ///
    /// 防回声：patch LoadScene prefix，广播前先看"最近跟随签名"是否在去重时间窗内，
    /// 是则跳过广播（说明这次 LoadScene 是跟随别人触发的，不该再传回去）。
    /// 只在协作 + Connected + 开关(EnableSceneSync)开启时生效。
    /// </summary>
    internal static class SceneChangeSync
    {
        internal static bool Enabled
        {
            get { return Plugin.CoOpSceneSyncCfg != null && Plugin.CoOpSceneSyncCfg.Value; }
            set { if (Plugin.CoOpSceneSyncCfg != null) Plugin.CoOpSceneSyncCfg.Value = value; }
        }

        internal static bool Active
        {
            // v2.0.1：协作模式自动开启，无需开关（竞速/单人不碰）。
            // 不再依赖配置文件里的 Enabled —— 老实例配置写死 false 也会失效。
            get { return Score.Style == PlayStyle.CoOp
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        private static int _lastFollowSig = -1;
        private static float _lastFollowAt;
        private const float DedupWindow = 6f;   // v2.0.2：2s 太短——客机套用主机地图后的内部重进在转场结束(>2s)才发生，去重失效导致回声循环
        private static float _lastLog;

        // ---------------- 加载看门狗（v2.3.2） ----------------
        // 真机事故：主机在 Lineage 选完继承人后 LoadScene(Town) 已经发起（prefix 里广播也发出去了），
        // 但场景永远没加载完成，主循环还在跑（心跳/网络都正常），画面一直停在加载中。
        // 这里记录"最后一次加载请求"，超过 10 秒目标场景还没变成当前场景就主动重试一次。
        private static int _loadTargetScene = -1;
        private static int _loadTargetTransition;
        private static float _loadReqAt;
        private static int _loadRetries;
        // v2.5.2：看门狗自己发起的重试期间置 true，让 LoadScenePrefix 不要重置状态
        //（否则 _loadRetries 被清零 -> 无限重试，日志里刷了 5 次）。
        private static bool _watchdogRetrying;

        /// <summary>正在转场/加载：这期间不要拦游戏的 timeScale（让游戏自己控制过渡节奏）。</summary>
        private static float _transitioningUntil;
        internal static bool IsTransitioning
        {
            get { return _transitioningUntil > 0f && Time.unscaledTime < _transitioningUntil; }
        }

        private static string SceneNameOf(int sceneId)
        {
            try { return SceneLoadingUtility.GetSceneName((SceneID)sceneId); }
            catch { return null; }
        }

        private static string CurrentSceneName()
        {
            try { return UnityEngine.SceneManagement.SceneManager.GetActiveScene().name; }
            catch { return null; }
        }

        /// <summary>
        /// v2.5.2：清掉 SceneLoader_RL 卡住的加载协程，让看门狗重载真正生效。
        /// 游戏 LoadScene 在 m_loadSceneCoroutine != null 时直接丢弃新请求；
        /// 场景过渡在 Lineage（等玩家选继承人）等场景会停摆，导致该协程永不结束。
        /// 通过反射停掉它并置 null，属兜底手段，只在看门狗判定真卡住时调用一次。
        /// </summary>
        private static void ClearStuckLoadCoroutine()
        {
            try
            {
                var instProp = typeof(SceneLoader_RL).GetProperty("Instance",
                    BindingFlags.Static | BindingFlags.NonPublic);
                var instance = instProp != null ? instProp.GetValue(null, null) as MonoBehaviour : null;

                // 游戏有两个可能卡住的转场协程，都清掉：
                //  m_loadSceneCoroutine           —— LoadScene 的加载协程（会被"已在运行"保护丢弃新请求）
                //  m_runTransitionWithLogicCoroutine —— Lineage 选人后走 RunTransitionWithLogic 的协程
                string[] fields = { "m_loadSceneCoroutine", "m_runTransitionWithLogicCoroutine" };
                bool cleared = false;
                foreach (var name in fields)
                {
                    var f = typeof(SceneLoader_RL).GetField(name,
                        BindingFlags.Static | BindingFlags.NonPublic);
                    if (f == null) continue;
                    var co = f.GetValue(null) as Coroutine;
                    if (co == null) continue;
                    if (instance != null)
                    {
                        try { instance.StopCoroutine(co); } catch { }
                    }
                    f.SetValue(null, null);
                    cleared = true;
                }
                if (cleared)
                    Plugin.Log.LogWarning("[RL2MP] [SCENE] 已清除卡住的转场协程，重载可真正生效。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SCENE] 清除卡住协程失败: " + ex.Message);
            }
        }

        /// <summary>由 NetworkManager.LateUpdate 每帧驱动：发现"请求了但没加载完"就重试一次。</summary>
        internal static void Tick()
        {
            if (_loadTargetScene >= 0 && !Active) _loadTargetScene = -1;
            if (_loadTargetScene < 0) return;

            string want = SceneNameOf(_loadTargetScene);
            string cur = CurrentSceneName();
            if (!string.IsNullOrEmpty(want) && cur == want)
            {
                // 加载成功
                _loadTargetScene = -1;
                _transitioningUntil = 0f;
                return;
            }

            float now = Time.unscaledTime;
            if (now - _loadReqAt < 10f) return;

            if (_loadRetries >= 2)
            {
                Plugin.Log.LogWarning("[RL2MP] [SCENE] 场景 '" + want + "' 两次重试后仍未加载完成（当前 '"
                    + cur + "'），停止重试。如果画面还停在加载中，请退出重进。");
                _loadTargetScene = -1;
                _transitioningUntil = 0f;
                return;
            }

            _loadRetries++;
            _loadReqAt = now;
            Plugin.Log.LogWarning("[RL2MP] [SCENE] 场景 '" + want + "' 10 秒仍未加载完成（当前 '"
                + cur + "'），清除卡住的转场协程并重试（第 " + _loadRetries + " 次）。");
            try
            {
                // v2.5.2 关键修复：SceneLoader_RL.LoadScene 内部有
                //   if (m_loadSceneCoroutine == null) 才启动协程，否则直接丢弃请求。
                // 原加载协程卡住（如 Lineage 等玩家选人时转场协程停摆）时，
                // 光调 LoadScene 会被静默丢弃 —— 这正是"重试 5 次全无效"的原因。
                // 先把卡住协程清掉并放行时间轴，重载才真正生效。
                ClearStuckLoadCoroutine();
                // v2.8.5：裸设 Time.timeScale=1f 没用 —— RLTimeScale.SetTimeScale 会把
                // 9 个类型的值取最小值再写回 Time.timeScale，字典里只要残留一项 0
                // （Lineage 死亡窗口的 Game=0、Boss 过场的 Cutscene=0），下一帧就打回 0，
                // LoadSceneCoroutine 里的 WaitForSeconds(0.1f) 永远走不完，场景永远加载不完。
                // 用游戏自己的 Reset()（= SetAllTimeScale(1f)）一次清空全部项。
                // 同时清掉转场窗口标志，让 PauseCompat 在重试期恢复拦截暂停请求，
                // 防止刚 Reset 完又被新的 SetTimeScale(Game,0) 拉回 0。
                RLTimeScale.Reset();
                _transitioningUntil = 0f;
                SuppressBroadcast = true;
                _watchdogRetrying = true;
                try { SceneLoader_RL.LoadScene((SceneID)_loadTargetScene, (TransitionID)_loadTargetTransition); }
                finally { _watchdogRetrying = false; SuppressBroadcast = false; }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SCENE] 重试加载异常: " + ex.Message);
            }
        }

        /// <summary>
        /// v2.0.2：内部强制重进 World（套用主机地图 / 种子对齐）时置 true，
        /// 让 LoadScenePrefix 不把这些"程序自己发起的"场景切换广播出去——
        /// 否则客机重进 -> 广播给主机 -> 主机跟随 -> 主机再广播 -> 客机再跟随……无限加载。
        /// </summary>
        internal static bool SuppressBroadcast;

        private static int MakeSig(int sceneId, int transitionId) { return sceneId * 100 + transitionId; }

        /// <summary>patch LoadScene prefix：本地主动切场景时广播，跟随触发的不回传。</summary>
        private static void LoadScenePrefix(SceneID sceneID, TransitionID transitionID)
        {
            // v2.3.2：先记录加载请求（看门狗用），再做广播判断。
            // v2.5.2：看门狗自己发起的重试不重置状态 —— 原来重试调 LoadScene 会
            // 走到这里把 _loadRetries 清 0，导致"只重试一次"永远不成立，每 10s 刷一次。
            if (!_watchdogRetrying)
            {
                try
                {
                    _loadTargetScene = (int)sceneID;
                    _loadTargetTransition = (int)transitionID;
                    _loadReqAt = Time.unscaledTime;
                    _loadRetries = 0;
                    _transitioningUntil = Time.unscaledTime + 15f;
                }
                catch { }
            }

            if (!Active || NetworkManager.Instance == null) return;
            if (SuppressBroadcast) return;   // v2.0.2：内部重进不广播，阻断回声循环

            // v2.0.4：客机退回主菜单/看结局时绝不广播——否则会把正在对局里的主机一起踹回主菜单
            //（真机日志：客机 [SCENE] 广播场景切换 -> MainMenu/FadeToBlackNoLoading (isHost=False)
            //  主机随即 [SCENE] 跟随场景切换 -> MainMenu，两边都掉出对局，表现为"两端卡死"）
            if (!NetworkManager.Instance.IsHost
                && (sceneID == SceneID.MainMenu || sceneID == SceneID.Credits))
            {
                Plugin.Log.LogInfo("[RL2MP] [SCENE] 客机切到 " + sceneID + "，不广播（避免拖主机离开对局）。");
                return;
            }

            try
            {
                int sid = (int)sceneID, tid = (int)transitionID;
                int sig = MakeSig(sid, tid);
                // 若这次是刚跟随别人触发的同一场景，别再广播（防回声）
                if (sig == _lastFollowSig && Time.unscaledTime - _lastFollowAt < DedupWindow) return;

                var w = new LiteNetLib.Utils.NetDataWriter();
                w.Put((byte)MsgType.SceneChange);
                w.Put(NetworkManager.Instance.LocalPlayerId);
                w.Put(sid); w.Put(tid);
                NetworkManager.Instance.SendSceneChange(w);

                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [SCENE] 广播场景切换 -> " + sceneID + "/" + transitionID
                        + " (isHost=" + NetworkManager.Instance.IsHost + ")");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SCENE] LoadScenePrefix 异常: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>收到对端的场景切换：跟随切换本地场景。</summary>
        internal static void FollowHost(int senderId, int sceneId, int transitionId)
        {
            if (!Active) return;

            var nm = NetworkManager.Instance;
            var target = (SceneID)sceneId;

            // v2.3.1：Lineage 是"个人死亡流程"（选继承人），绝不能跨端跟随。
            // 真机事故：双方全灭 → 客机先走真实死亡进 Lineage → 主机跟了过去，
            // 但主机自己的角色还停在"倒地待救"，Lineage 里根本没有可生成的玩家 →
            // 主机无法选继承人 → 又被跟随拖进 Town → 变成一个白块、不能动、不能交互。
            // 正确语义：队友进 Lineage 只代表"这一局结束了"，本地应该自己走真实死亡，
            // 由游戏原生流程把自己送进 Lineage 选人。
            if (target == SceneID.Lineage)
            {
                if (DownedState.LocalDown)
                {
                    Plugin.Log.LogInfo("[RL2MP] [SCENE] 队友已进入 Lineage（真实死亡），本地还在倒地"
                        + " -> 一并结束本局，自己走真实死亡流程。");
                    CoOpRescue.LocalDown = false;
                    DownedState.TriggerRealDeath();
                }
                else
                {
                    Plugin.Log.LogInfo("[RL2MP] [SCENE] 目标 Lineage 是个人死亡流程，不跟随。");
                }
                return;
            }

            // v2.3.1：本地还在 Lineage 选继承人时，拒绝任何跟随。
            // 否则会被队友的 Town/World 切换直接拖走，导致"没选人就被扔进城堡"。
            try
            {
                var curScene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                if (curScene.name == "Lineage")
                {
                    Plugin.Log.LogInfo("[RL2MP] [SCENE] 本地还在 Lineage 选继承人，不跟随 "
                        + target + "。");
                    return;
                }
            }
            catch { }

            // v2.3.1：Town 是各自回城（点技能树），各自会走原生流程过去，不需要跟随。
            if (target == SceneID.Town)
            {
                Plugin.Log.LogInfo("[RL2MP] [SCENE] 目标是 Town（各自回城），不跟随。");
                return;
            }

            // v2.0.4：主机不跟客机回主菜单/结局——客机自己退出对局不该清零主机的进度
            if (nm != null && nm.IsHost
                && (target == SceneID.MainMenu || target == SceneID.Credits))
            {
                Plugin.Log.LogInfo("[RL2MP] [SCENE] 客机切到 " + target + "（离开对局），主机不跟随。");
                return;
            }
            try
            {
                // 已经在这个场景就不重复切
                var cur = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
                var targetName = SceneLoadingUtility.GetSceneName((SceneID)sceneId);
                if (!string.IsNullOrEmpty(targetName) && cur.name == targetName)
                {
                    Plugin.Log.LogInfo("[RL2MP] [SCENE] 本地已在场景 " + targetName + "，跳过跟随。");
                    return;
                }

                int sig = MakeSig(sceneId, transitionId);
                _lastFollowSig = sig;
                _lastFollowAt = Time.unscaledTime;

                Plugin.Log.LogInfo("[RL2MP] [SCENE] 跟随场景切换 -> " + (SceneID)sceneId + "/" + (TransitionID)transitionId);
                // v2.0.2：跟随触发的 LoadScene 绝不能再广播回对端，否则无限回声
                SuppressBroadcast = true;
                try { SceneLoader_RL.LoadScene((SceneID)sceneId, (TransitionID)transitionId); }
                finally { SuppressBroadcast = false; }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SCENE] FollowHost 异常: " + Plugin.PatchError(ex));
            }
        }

        private static MethodInfo _loadSceneMethod;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                _loadSceneMethod = AccessTools.Method(typeof(SceneLoader_RL), "LoadScene",
                    new Type[] { typeof(SceneID), typeof(TransitionID) });
                if (_loadSceneMethod == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [SCENE] SceneLoader_RL.LoadScene 未找到，场景同步不可用。");
                    return;
                }
                var pre = new HarmonyMethod(typeof(SceneChangeSync).GetMethod("LoadScenePrefix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(_loadSceneMethod, prefix: pre);
                Plugin.Log.LogInfo("[RL2MP] [SCENE] 补丁 OK: SceneLoader_RL.LoadScene prefix（全员场景切换，默认关闭）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [SCENE] 补丁失败: " + Plugin.PatchError(ex));
            }
        }
    }
}
