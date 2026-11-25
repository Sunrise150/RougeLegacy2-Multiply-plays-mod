using System;
using MoreMountains.CorgiEngine;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 世界驱动权令牌（v3.0.0，借鉴 HKMP 的 SceneHost 选举与 TUNIC 的单写者所有权）。
    ///
    /// 旧架构把"世界驱动权"写死成 NetworkManager.IsHost（HostId=0 固定）。
    /// 主机一倒地，它的 PlayerController 被 SetActive(false)，HostTick 依赖的本地世界
    /// 采样全部失效，于是怪物/障碍/Boss 位图/房间坐标集体停摆——这就是
    /// "主机死了门就打不开、客机看不到障碍物、怪物不匹配"的共同根因。
    ///
    /// 新架构：驱动权跟着"还站着的人"走。
    ///   - 主机没倒 -> 主机驱动（行为与旧版完全一致）
    ///   - 主机倒了、客机还站着 -> 客机驱动，主机退为应用端
    ///   - 两端都倒 -> 无人驱动（本局已结束，RunOver 会处理）
    ///
    /// 关键：两端必须算出同一个 AuthorityPlayerId，且不需要新增网络消息。
    /// 两端本来就在收 PlayerDown 心跳：
    ///   - 主机看自己的 DownedState.LocalDown，看客机的 PlayerSync.RemoteDowned
    ///   - 客机看自己的 DownedState.LocalDown，看主机的 RemoteDowned.Contains(0)
    /// 同样的输入 -> 同样的权威，天然一致，没有"谁来宣布移交"的竞态。
    ///
    /// 时钟：快照仍打 NetClock.HostNow。该时钟是本局共享时间轴（客机值是主机时钟的
    /// 估计），所以客机当权威时打的时间戳，主机依然能正确解释——无需切换时钟源。
    /// </summary>
    internal static class RoomAuthority
    {
        private static int _lastAuthorityId = int.MinValue;

        /// <summary>只有协作对局内才做移交；单机/竞速保持旧的主机权威行为。</summary>
        internal static bool Enabled
        {
            get
            {
                var nm = NetworkManager.Instance;
                return nm != null && nm.SessionActive && Score.Style == PlayStyle.CoOp;
            }
        }

        /// <summary>当前世界驱动方的玩家 ID。主机=0；无法确定时回退为主机。</summary>
        internal static int AuthorityPlayerId
        {
            get
            {
                var nm = NetworkManager.Instance;
                if (nm == null) return 0;
                if (!Enabled) return nm.IsHost ? 0 : -1;

                var ps = PlayerSync.Instance;

                // 主机是否倒地：主机看自己，客机看心跳。
                bool hostDown = nm.IsHost
                    ? DownedState.LocalDown
                    : (ps != null && ps.RemoteDowned.Contains(0));

                if (!hostDown) return 0; // 主机站着：维持原状，主机就是权威

                // 主机倒了：找一个还站着的客机接管。
                if (!nm.IsHost)
                {
                    // 客机视角：只要自己没倒，就是自己接管（双人局即唯一客机）。
                    return DownedState.LocalDown ? 0 : nm.LocalPlayerId;
                }

                // 主机视角：挑 ID 最小的存活客机，与客机自己的判断保持一致。
                int best = -1;
                foreach (var id in nm.ClientIds)
                {
                    if (id == 0) continue;
                    if (ps != null && ps.RemoteDowned.Contains(id)) continue;
                    if (best < 0 || id < best) best = id;
                }
                return best < 0 ? 0 : best; // 全员倒地：交回主机（本局即将结束）
            }
        }

        /// <summary>本机当前是否持有世界驱动权。</summary>
        internal static bool IsLocalAuthority
        {
            get
            {
                var nm = NetworkManager.Instance;
                if (nm == null) return false;
                int auth = AuthorityPlayerId;
                return auth == nm.LocalPlayerId || (auth == 0 && nm.IsHost);
            }
        }

        /// <summary>
        /// 本机是否应当真正采样并广播世界状态。
        /// 比 IsLocalAuthority 多一条：权威方自己必须站着——尸体不能驱动世界。
        /// 所有 HostTick 都用这个判定。
        /// </summary>
        internal static bool ShouldDriveWorld
        {
            get { return IsLocalAuthority && !DownedState.LocalDown; }
        }

        /// <summary>
        /// 每帧检测权威是否易手。新权威接管时重置节流状态并强制全量广播一次，
        /// 避免接管的头一拍还被"距上次发送不足 N 秒"挡掉，让对端多等一个周期。
        /// </summary>
        internal static void Tick()
        {
            var nm = NetworkManager.Instance;
            if (nm == null) return;

            int auth = AuthorityPlayerId;
            if (auth == _lastAuthorityId) return;

            int prev = _lastAuthorityId;
            _lastAuthorityId = auth;
            if (prev == int.MinValue || !Enabled) return;

            string who = auth == 0 ? "主机" : "客机 P" + auth;
            Plugin.Log.LogInfo("[AUTH] 世界驱动权移交 -> " + who
                + " (isHost=" + nm.IsHost + ", localId=" + nm.LocalPlayerId
                + ", localDown=" + DownedState.LocalDown + ")");

            bool nowLocal = IsLocalAuthority;

            // v3.0.4：权威易手 -> 让观战方下一拍强制重解析房间（旧权威坐标可能已失效）。
            DownedState.ForceSpectateRoomRefresh = true;

            // v3.0.17：借鉴 NetworkingReworked 的 ClearAll —— 权威/拥有权重算时，
            // 所有“忽略下一次 / 静默窗 / 门闩”闩锁都必须清干净。旧实现只在断网
            // Reset 时清理，主机倒地移交后残留的 _suppressUntil / _manualExitUntil
            // 会把合法过门当回声吞掉，表现为“门触发器又坏了”。
            try { RoomTravelSync.Reset("authority changed"); } catch { }
            try { RoomFix.Reset("authority changed"); } catch { }

            // v3.0.4：权威易手时同步翻转本地怪的 AI 控制权（Puppetize <-> 恢复）。
            // 这是"主机倒地后怪不攻击客机 / 观战画面怪静止"的架构根因修复：
            // 接管方必须把之前被 Puppetize 关停的怪重新唤醒，否则新权威驱动的是死怪。
            try { EnemyPositionSync.OnAuthorityChanged(nowLocal); } catch (Exception ex)
            {
                Plugin.Log.LogWarning("[AUTH] 切换本地怪 AI 控制权失败: " + ex.Message);
            }

            if (!nowLocal) return;

            // v3.0.18：旧权威倒地期间，新权威当前房的门出口触发器可能被半初始化流程
            // 关掉，或者 room.IsPlayerInRoom 还是 false。只清闩锁不够——补一次
            // MarkEntered 把触发器重新打开，避免“刚接管就过不了第一扇门”。
            // MarkEntered 不会重刷怪，也不会强开已经 active 的触发器（v3.0.17 语义）。
            try
            {
                var ps = PlayerSync.Instance;
                var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
                if (pc != null && !DownedState.LocalDown && pc.CurrentlyInRoom != null)
                    RoomFix.MarkEntered(pc.CurrentlyInRoom, null);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[AUTH] 新权威重开门触发器失败: " + ex.Message);
            }

            // 新权威：清掉旧的发送节流，立刻补一拍全量权威快照。
            try { BossProgressSync.ResetHost(); } catch { }
        }

        /// <summary>断线/新局时复位，让下一拍重新判定并记录移交。</summary>
        internal static void Reset()
        {
            _lastAuthorityId = int.MinValue;
        }
    }
}
