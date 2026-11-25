using System;
using System.Reflection;
using HarmonyLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v3.0.26：漂流屋的【挑战】必须大家一起进。
    ///
    /// 原生流程：玩家在漂流屋和挑战 NPC 交互 -> ChallengeTunnelController.EnterChallenge(type)
    /// -> EnterTunnel(...) -> ChallengeManager.SetActiveChallenge(type)
    /// -> Tunnel.ForceEnterTunnel(animate:false) -> Room.PlayerExit(null) -> Destination.PlacePlayerHere()
    ///
    /// 问题：整个流程是【纯本地】的。谁先点了，谁就立刻被送进挑战屋；
    /// 队友还站在漂流屋里 —— 于是出现"一方在房间里、一方在挑战里"。
    /// （旧版 RoomTravelSync 会把"无门进入"也当过门广播，队友更是被直接拖进挑战房，
    ///   v3.0.25 已经把那条堵掉了；现在补上"一起进"。）
    ///
    /// 做法：加一条 Challenge 消息做【会合门闩】：
    ///   1) 本地玩家选了挑战 -> 拦住原生进入（Harmony prefix 返回 false），
    ///      广播 state=Ready(0)+挑战类型，屏幕上等队友；
    ///   2) 收到队友的 Ready（且挑战类型相同）-> 两边同时放行原生进入 -> 一起进挑战；
    ///   3) 收到队友的 Entered(1) 而本地还没选 -> 本地也进同一个挑战，避免落单；
    ///   4) 超时（默认 20 秒）没人来 -> 放行原生进入（宁可单进也不能把人永久卡在漂流屋）。
    ///
    /// 协议 3014 -> 3015。
    /// </summary>
    internal static class ChallengeSync
    {
        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp && DownedState.InSession
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        private const byte StateReady = 0;
        private const byte StateEntered = 1;

        /// <summary>等待队友的宽限（秒）。超时后单进，绝不把人永久卡住。</summary>
        private const float WaitTimeout = 20f;

        private static MethodInfo _enterChallengeMethod;
        /// <summary>本地已选、正在等队友的挑战。</summary>
        private static ChallengeType _pendingType = ChallengeType.None;
        private static float _pendingAt;
        /// <summary>队友已经就位。</summary>
        private static bool _peerReady;
        private static int _peerType = -1;
        /// <summary>放行原生进入时置位，避免自己把闸门又关上一次。</summary>
        private static bool _lettingThrough;
        /// <summary>已经广播过"我进挑战了"。</summary>
        private static bool _sentEntered;
        private static float _lastLog;

        internal static void Apply(Harmony harmony)
        {
            try
            {
                _enterChallengeMethod = AccessTools.Method(typeof(ChallengeTunnelController), "EnterChallenge",
                    new[] { typeof(ChallengeType), typeof(bool) });
                if (_enterChallengeMethod == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [CHAL] ChallengeTunnelController.EnterChallenge 未找到，"
                        + "挑战同步不可用。");
                    return;
                }
                var pre = new HarmonyMethod(typeof(ChallengeSync).GetMethod("EnterChallengePrefix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(_enterChallengeMethod, prefix: pre);
                Plugin.Log.LogInfo("[RL2MP] [CHAL] 补丁 OK: ChallengeTunnelController.EnterChallenge prefix"
                    + "（挑战一起进，超时兜底）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAL] 补丁失败: " + Plugin.PatchError(ex));
            }
        }

        // ---------------- 本地拦门 ----------------

        /// <summary>
        /// 拦住本地玩家单方面进入挑战，等队友到齐再一起进。
        /// 返回 false = 跳过原生进入（我们自己放行时用 _lettingThrough 绕开）。
        /// </summary>
        private static bool EnterChallengePrefix(ChallengeTunnelController __instance,
            ChallengeType challengeType, bool animateTransition)
        {
            try
            {
                if (!Active) return true;
                if (_lettingThrough) return true;
                if (_pendingType != ChallengeType.None) return false;   // 已经在等，别重复触发

                _pendingType = challengeType;
                _pendingAt = Time.unscaledTime;
                _peerReady = false;
                _peerType = -1;
                _sentEntered = false;

                Broadcast(StateReady, (int)challengeType);
                if (Time.unscaledTime - _lastLog > 1f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [CHAL] 已选择挑战 " + challengeType
                        + "，等队友一起进入（最多等 " + WaitTimeout + " 秒）。");
                }
                return false;   // 拦住原生进入
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAL] EnterChallengePrefix 异常: " + ex.Message);
                return true;
            }
        }

        /// <summary>由 NetworkManager.LateUpdate 驱动：看门闩是否该开了。</summary>
        internal static void Tick()
        {
            if (!Active) { Reset(); return; }
            if (_pendingType == ChallengeType.None) return;

            try
            {
                if (_peerReady && _peerType == (int)_pendingType)
                {
                    Proceed(_pendingType, "队友已就位");
                    return;
                }
                if (Time.unscaledTime - _pendingAt >= WaitTimeout)
                {
                    Proceed(_pendingType, "等待超时，单进");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAL] Tick 异常: " + ex.Message);
            }
        }

        /// <summary>放行原生进入（两端同时做，于是两人一起进挑战）。</summary>
        private static void Proceed(ChallengeType type, string why)
        {
            ChallengeType pending = _pendingType;
            _pendingType = ChallengeType.None;
            _peerReady = false;
            _peerType = -1;

            var ctrl = FindController();
            if (ctrl == null || _enterChallengeMethod == null)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAL] 找不到 ChallengeTunnelController，无法进入挑战。");
                return;
            }
            Plugin.Log.LogInfo("[RL2MP] [CHAL] 放行进入挑战 " + pending + "（" + why + "）。");
            _lettingThrough = true;
            try
            {
                // animateTransition 传 false：两端同时进，用同一套无过场路径，
                // 避免一端走 SceneLoader_RL 转场被场景同步又拉一次。
                _enterChallengeMethod.Invoke(ctrl, new object[] { pending, false });
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAL] 进入挑战失败: " + Plugin.PatchError(ex));
            }
            finally
            {
                _lettingThrough = false;
            }

            if (!_sentEntered)
            {
                _sentEntered = true;
                Broadcast(StateEntered, (int)pending);
            }
        }

        // ---------------- 收发 ----------------

        private static void Broadcast(byte state, int challengeType)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.Challenge);
            w.Put(nm.LocalPlayerId);
            w.Put(state);
            w.Put(challengeType);
            nm.SendChallenge(w);
        }

        /// <summary>由 NetworkManager 收到 Challenge 包后调用（已做 sender 校验/中转）。</summary>
        internal static void ApplyRemote(int senderId, byte state, int challengeType)
        {
            if (!Active) return;
            if (challengeType < 0) return;
            try
            {
                if (state == StateReady)
                {
                    _peerReady = true;
                    _peerType = challengeType;
                    if (Time.unscaledTime - _lastLog > 1f)
                    {
                        _lastLog = Time.unscaledTime;
                        Plugin.Log.LogInfo("[RL2MP] [CHAL] 队友已就位（挑战 " + challengeType + "）。");
                    }
                    return;
                }

                if (state == StateEntered)
                {
                    if (_pendingType != ChallengeType.None)
                    {
                        _peerReady = true;
                        _peerType = challengeType;
                        return;
                    }
                    // 队友已经进了，本地还没选 —— 跟着进同一个挑战，避免落单。
                    if (challengeType > 0 && CanFollowIntoChallenge())
                    {
                        Plugin.Log.LogInfo("[RL2MP] [CHAL] 队友已进入挑战 " + challengeType
                            + "，本地一起进入。");
                        Proceed((ChallengeType)challengeType, "队友已进挑战");
                    }
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [CHAL] 套用对端状态失败: " + ex.Message);
            }
        }

        /// <summary>本地是否适合跟着队友进挑战：得在漂流屋、且当前不在挑战里。</summary>
        private static bool CanFollowIntoChallenge()
        {
            try
            {
                if (ChallengeManager.IsInChallenge) return false;
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc == null) return false;
                var room = pc.CurrentlyInRoom as BaseRoom;
                if (room == null) return false;
                // 只在漂流屋里才跟着进 —— 在城堡里被拉进挑战显然不对。
                if (room.BiomeType != BiomeType.DriftHouse) return false;
                return true;
            }
            catch { return false; }
        }

        private static ChallengeTunnelController FindController()
        {
            try
            {
                var all = Resources.FindObjectsOfTypeAll<ChallengeTunnelController>();
                for (int i = 0; i < all.Length; i++)
                {
                    if (all[i] == null) continue;
                    if (!all[i].gameObject.scene.IsValid()) continue;
                    return all[i];
                }
            }
            catch { }
            return null;
        }

        internal static void Reset()
        {
            _pendingType = ChallengeType.None;
            _peerReady = false;
            _peerType = -1;
            _lettingThrough = false;
            _sentEntered = false;
        }
    }
}
