using System;
using System.Collections;
using System.Reflection;
using HarmonyLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v3.0.25：开局（进城堡）流程同步 —— 交钱给 Charon + 锁城堡（世界锁）+ 同一套动画。
    ///
    /// 原生流程（HubTown 中心镇 -> 城堡）：
    ///   1) 和 Charon 说话 -> PlayerSaveData.GiveMoneyToCharon(caller)
    ///      把【全部金币】交出去（GoldCollected 归零），并派发 GoldChanged / GoldSavedChanged。
    ///   2) 和 Architect（建筑师）说话 -> ArchitectShop.SetLockState(CastleLockState)
    ///      播放"上锁/解锁"动画（Animator Fire / Retract）+ 音效（mechanismLockAudioEvent），
    ///      并写 SaveManager.PlayerSaveData.CastleLockState（这就是"世界锁"）。
    ///   3) LevelManager_RL 看 CastleLockState 决定加载 World 场景开新局。
    ///
    /// 问题：1) 和 2) 都是【本地 UI 交互】触发的。联机时主机点了，客机只是被
    /// SceneChangeSync 拉着一起换了场景 —— 客机没交钱（金币还是满的）、
    /// 锁状态也没同步，两端的"世界锁"动画/音效自然对不上。
    /// 用户反馈原话："进入城堡是要将钱全部给交出才能进入的，你这样子的传送，
    /// 扣除的只是触发交钱这个逻辑的那个人，而且还有世界锁这个操作，
    /// 客机和主机关于这方面的动画没有统一"。
    ///
    /// 做法：新增 RunStart 消息，把"锁状态 + 是否交了钱"广播出去；
    /// 收到端调用【游戏自己的】同一套逻辑（GiveMoneyToCharon / ArchitectShop.SetLockState），
    /// 于是金币归零一致、锁状态一致、动画与音效也一致。
    /// 锁状态用轮询检测（SetLockState 是迭代器方法，Harmony 打迭代器的 postfix
    /// 时序不可靠，轮询更稳）。
    /// 协议 3013 -> 3014。
    /// </summary>
    internal static class RunStartSync
    {
        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp && DownedState.InSession
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        /// <summary>正在套用对端状态：防止回声（对端广播 -> 我方再广播回去）。</summary>
        private static bool _applyingRemote;
        /// <summary>上次已知的本地锁状态（用于轮询检测变化）。</summary>
        private static CastleLockState _lastLockState = CastleLockState.NotLocked;
        /// <summary>已经初始化过 _lastLockState（避免开局第一帧就误报"变了"）。</summary>
        private static bool _lockStateInit;
        private static float _lastLog;
        private static float _lastWarn;

        private static MethodInfo _giveMoneyMethod;
        private static MethodInfo _setLockStateMethod;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                _giveMoneyMethod = AccessTools.Method(typeof(PlayerSaveData), "GiveMoneyToCharon",
                    new[] { typeof(MonoBehaviour) });
                if (_giveMoneyMethod != null)
                {
                    var post = new HarmonyMethod(typeof(RunStartSync).GetMethod("GiveMoneyPostfix",
                        BindingFlags.Static | BindingFlags.NonPublic));
                    harmony.Patch(_giveMoneyMethod, postfix: post);
                    Plugin.Log.LogInfo("[RL2MP] [RUNSTART] 补丁 OK: GiveMoneyToCharon postfix（开局交钱同步）。");
                }
                else
                {
                    Plugin.Log.LogWarning("[RL2MP] [RUNSTART] GiveMoneyToCharon 未找到，交钱同步不可用。");
                }

                _setLockStateMethod = AccessTools.Method(typeof(ArchitectShop), "SetLockState",
                    new[] { typeof(CastleLockState) });
                if (_setLockStateMethod != null)
                    Plugin.Log.LogInfo("[RL2MP] [RUNSTART] ArchitectShop.SetLockState 已定位（对端将播放同一套锁城堡动画）。");
                else
                    Plugin.Log.LogWarning("[RL2MP] [RUNSTART] ArchitectShop.SetLockState 未找到，"
                        + "对端只能同步锁状态、动画不同步。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [RUNSTART] 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>由 NetworkManager.LateUpdate 每帧驱动：轮询本地锁状态，变了就广播。</summary>
        internal static void Tick()
        {
            if (!Active) { _lockStateInit = false; return; }
            try
            {
                var psd = SaveManager.PlayerSaveData;
                if (psd == null) return;
                CastleLockState now = psd.CastleLockState;
                if (!_lockStateInit)
                {
                    _lockStateInit = true;
                    _lastLockState = now;
                    return;
                }
                if (now == _lastLockState) return;

                _lastLockState = now;
                if (_applyingRemote) return;   // 刚才是我们自己在套用对端状态，不再回发
                Broadcast(now, false);
                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [RUNSTART] 广播锁状态变化 -> " + now);
                }
            }
            catch (Exception ex)
            {
                Warn("锁状态轮询失败: " + ex.Message);
            }
        }

        // ---------------- 发送端 ----------------

        /// <summary>本地把全部金币交给 Charon 之后：广播，让对端也交掉自己的金币。</summary>
        private static void GiveMoneyPostfix(PlayerSaveData __instance)
        {
            if (_applyingRemote || !Active) return;
            try
            {
                CastleLockState st = CastleLockState.NotLocked;
                try { st = SaveManager.PlayerSaveData.CastleLockState; } catch { }
                Broadcast(st, true);
                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [RUNSTART] 广播开局交钱（对端金币也会归零）。");
                }
            }
            catch (Exception ex) { Warn("交钱广播失败: " + ex.Message); }
        }

        private static void Broadcast(CastleLockState lockState, bool gaveMoney)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.RunStart);
            w.Put(nm.LocalPlayerId);
            w.Put((byte)lockState);
            w.Put((byte)(gaveMoney ? 1 : 0));
            nm.SendRunStart(w);
        }

        // ---------------- 接收端 ----------------

        /// <summary>由 NetworkManager 收到 RunStart 包后调用（已做 sender 校验/中转）。</summary>
        internal static void ApplyRemote(int senderId, byte lockState, byte flags)
        {
            if (!Active) return;
            bool gaveMoney = (flags & 1) != 0;
            _applyingRemote = true;
            try
            {
                // 1) 金币：各自的金币数不同，但语义一致 —— 全部交给 Charon，本地金币归零。
                if (gaveMoney) GiveMoneyLocally();

                // 2) 锁状态 + 同一套动画：直接调用原生 ArchitectShop.SetLockState，
                //    动画(Fire/Retract)、音效(GoldChanged 广播) 与主机完全一致。
                _lastLockState = (CastleLockState)lockState;
                _lockStateInit = true;
                ApplyLockState((CastleLockState)lockState);

                if (Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [RUNSTART] 套用对端开局状态：锁=" + (CastleLockState)lockState
                        + " 交钱=" + gaveMoney);
                }
            }
            catch (Exception ex)
            {
                Warn("套用对端开局状态失败: " + ex.Message);
            }
            finally
            {
                _applyingRemote = false;
            }
        }

        /// <summary>本地也把全部金币交给 Charon（原生同一个方法）。</summary>
        private static void GiveMoneyLocally()
        {
            try
            {
                var psd = SaveManager.PlayerSaveData;
                if (psd == null) return;
                if (psd.GoldCollected <= 0) return;   // 原生方法本身也会提前返回
                psd.GiveMoneyToCharon(null);
            }
            catch (Exception ex)
            {
                Warn("本地交钱失败: " + ex.Message);
            }
        }

        /// <summary>套用锁状态，并尽量播放原生那套"世界锁"动画。</summary>
        private static void ApplyLockState(CastleLockState state)
        {
            try
            {
                var psd = SaveManager.PlayerSaveData;
                if (psd == null) return;

                if (_setLockStateMethod == null) return;   // 找不到原生实现：状态由上面的赋值处理

                var all = Resources.FindObjectsOfTypeAll<ArchitectShop>();
                ArchitectShop shop = null;
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] == null) continue;
                    if (!all[i].gameObject.scene.IsValid()) continue;
                    shop = all[i];
                    break;
                }
                if (shop == null)
                {
                    // 本地没有建筑师（不在中心镇）—— 只同步状态，动画无从谈起。
                    psd.CastleLockState = state;
                    return;
                }
                var co = _setLockStateMethod.Invoke(shop, new object[] { state }) as IEnumerator;
                if (co != null) shop.StartCoroutine(co);
            }
            catch (Exception ex)
            {
                Warn("锁城堡动画失败: " + ex.Message);
                try { SaveManager.PlayerSaveData.CastleLockState = state; } catch { }
            }
        }

        internal static void Reset()
        {
            _applyingRemote = false;
            _lockStateInit = false;
            _lastLockState = CastleLockState.NotLocked;
        }

        private static void Warn(string msg)
        {
            if (Time.unscaledTime - _lastWarn < 3f) return;
            _lastWarn = Time.unscaledTime;
            Plugin.Log.LogWarning("[RL2MP] [RUNSTART] " + msg);
        }
    }
}
