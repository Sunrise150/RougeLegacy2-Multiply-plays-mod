using System;
using HarmonyLib;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.0.1 协作模式：Boss 击败进度同步（主机权威）。
    ///
    /// 背景：原版"房间 boss 门是否解锁"完全取决于各自本地存档 SaveManager.PlayerSaveData
    /// 里的 PlayerSaveFlag.*Boss_Defeated。协作时两端各存各档，于是出现：
    ///   客机用自己的存档（例如 P2，已打完 6 个常规 Boss）进门 -> 门全开 -> 能直接打第 7 个 Boss；
    ///   主机存档没打完 -> 门锁着 -> 进不去。世界状态分叉。
    ///
    /// 用户拍板：房间 Boss 进度由主机决定；金币/装备/世界树收益仍各自进各自存档。
    ///
    /// 方案（最小侵入，只改内存不落盘）：
    ///   1) 主机每 0.5s 把 8 个 Boss 的 Defeated 状态打包成 ushort 位图广播（ReliableOrdered）。
    ///      新客机加入 / 中途重连也会在下一次广播拿到完整位图。
    ///   2) 客机收到后，把位图写进自己 SaveManager.PlayerSaveData 的对应 flag（仅内存）。
    ///      —— 不写盘、不动金币/装备/NG+，所以客机自己的存档收益完全不受影响。
    ///   3) 只在协作 + Connected 时生效；竞速 / 单人一律不碰。
    ///
    /// 效果：客机门状态跟主机一致（主机没打完的门，客机也进不去），两端始终能一起推进。
    /// 断线/停止网络后调 Restore() 把内存 flag 还原成客机自己存档的值（不污染源存档）。
    /// </summary>
    internal static class BossProgressSync
    {
        // 顺序即 bit 位：0=Castle 1=Bridge 2=Forest 3=Study 4=Tower 5=Cave 6=Garden 7=Final
        internal static readonly PlayerSaveFlag[] BossFlags =
        {
            PlayerSaveFlag.CastleBoss_Defeated,
            PlayerSaveFlag.BridgeBoss_Defeated,
            PlayerSaveFlag.ForestBoss_Defeated,
            PlayerSaveFlag.StudyBoss_Defeated,
            PlayerSaveFlag.TowerBoss_Defeated,
            PlayerSaveFlag.CaveBoss_Defeated,
            PlayerSaveFlag.GardenBoss_Defeated,
            PlayerSaveFlag.FinalBoss_Defeated,
        };

        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        private static float _sendTimer;
        private static ushort _lastSentMask = ushort.MaxValue;   // 强制首帧发一次
        private static ushort _appliedMask;
        private static bool _hasApplied;
        private static float _lastLog;

        /// <summary>从本地存档读 8 个 Boss flag 组成位图。</summary>
        private static ushort BuildMaskFromLocal()
        {
            ushort mask = 0;
            var psd = SaveManager.PlayerSaveData;
            if (psd == null) return mask;
            for (int i = 0; i < BossFlags.Length; i++)
            {
                bool beaten = false;
                try { beaten = psd.GetFlag(BossFlags[i]); } catch { }
                if (beaten) mask |= (ushort)(1 << i);
            }
            return mask;
        }

        /// <summary>主机：每 0.5s 广播权威 Boss 位图（变化即发，也周期重发兜底新加入者）。</summary>
        internal static void HostTick()
        {
            if (!Active || NetworkManager.Instance == null || !RoomAuthority.ShouldDriveWorld) return;

            // v3.0.0：ShouldDriveWorld 已保证"只有站着的权威方"才广播。
            // 主机倒地时令牌移交存活客机，BuildMaskFromLocal 读的就是客机内存里的位图——
            // 客机还在打 Boss、门状态是活的，不会再被主机的旧位图锁回去。
            // 主机复活后令牌归位，Tick 检测到易手会 ResetHost 强制重发一次。

            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer < 0.5f) return;
            _sendTimer = 0f;

            try
            {
                ushort mask = BuildMaskFromLocal();
                // 位图没变且已经发过一次，就不重复刷（3s 兜底重发一次，覆盖中途加入）
                if (mask == _lastSentMask && Time.unscaledTime - _lastLog < 3f) return;
                _lastSentMask = mask;
                _lastLog = Time.unscaledTime;

                var w = new LiteNetLib.Utils.NetDataWriter();
                w.Put((byte)MsgType.BossProgress);
                w.Put(mask);
                NetworkManager.Instance.SendBossProgress(w);

                Plugin.Log.LogInfo("[BOSSSYNC] 主机广播 Boss 位图 mask=" + Convert.ToString(mask, 2).PadLeft(8, '0'));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[BOSSSYNC] HostTick 异常: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>客机：收到主机权威位图，在内存里覆写本地 Boss flag（不落盘）。</summary>
        internal static void ApplySnapshot(ushort mask)
        {
            if (!Active || NetworkManager.Instance == null || RoomAuthority.IsLocalAuthority) return;
            if (mask == _appliedMask && _hasApplied) return;

            try
            {
                var psd = SaveManager.PlayerSaveData;
                if (psd == null)
                {
                    Plugin.Log.LogWarning("[BOSSSYNC] 客机收到位图但 PlayerSaveData 尚未就绪，跳过。");
                    return;
                }

                // 首次应用前记录客机自己的原始值，供断线还原
                if (!_hasApplied) CaptureOriginal();

                for (int i = 0; i < BossFlags.Length; i++)
                {
                    bool want = (mask & (ushort)(1 << i)) != 0;
                    bool cur = false;
                    try { cur = psd.GetFlag(BossFlags[i]); } catch { }
                    // v2.8.8：恢复双向覆写（主机权威）。v2.8.7 改成"只解锁不回锁"破坏了
                    // 用户拍板的"Boss 门主机说了算"：主机位图为 0 时客机本地 true 不再被压回，
                    // 两端门状态分叉（客机门开、主机门关）→ 进出门口出错。
                    // 原问题"主机死后旧 mask 回锁客机"的真正解法不在这里，而在复活时
                    // ResetHost() 强制重发主机当前权威 mask（见 Downed.Revive）。
                    if (cur != want)
                    {
                        try { psd.SetFlag(BossFlags[i], want); } catch { }
                    }
                }

                _appliedMask = mask;
                _hasApplied = true;
                Plugin.Log.LogInfo("[BOSSSYNC] 客机已对齐主机 Boss 进度 mask=" + Convert.ToString(mask, 2).PadLeft(8, '0')
                    + "（仅内存，未改自己存档）");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[BOSSSYNC] ApplySnapshot 异常: " + Plugin.PatchError(ex));
            }
        }

        private static bool[] _origFlags;

        private static void CaptureOriginal()
        {
            _origFlags = new bool[BossFlags.Length];
            var psd = SaveManager.PlayerSaveData;
            if (psd == null) return;
            for (int i = 0; i < BossFlags.Length; i++)
            {
                bool v = false;
                try { v = psd.GetFlag(BossFlags[i]); } catch { }
                _origFlags[i] = v;
            }
        }

        /// <summary>断线/停止网络：把内存 flag 还原成客机自己存档的进度。</summary>
        internal static void Restore(string reason)
        {
            if (!_hasApplied) return;
            _hasApplied = false;
            _appliedMask = 0;
            try
            {
                var psd = SaveManager.PlayerSaveData;
                if (psd == null || _origFlags == null) return;
                for (int i = 0; i < BossFlags.Length; i++)
                {
                    try { psd.SetFlag(BossFlags[i], _origFlags[i]); } catch { }
                }
                Plugin.Log.LogInfo("[BOSSSYNC] 已还原客机自身 Boss 进度（" + reason + "）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[BOSSSYNC] Restore 异常: " + ex.Message);
            }
        }

        internal static void ResetHost()
        {
            _lastSentMask = ushort.MaxValue;
            _sendTimer = 0f;
        }
    }
}
