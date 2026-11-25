using System;
using System.Collections.Generic;
using HarmonyLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.5.0: authoritative *alive set* sync, fixing "[EPOS] 活怪=1 但权威条目=3".
    ///
    /// Root cause: the guest regenerates its own room from its own save, so an enemy
    /// the host already killed is still alive on the guest. The old tombstone mechanism
    /// keyed by EnemyIndex drifts when a room half-initializes, so kills missed entirely.
    ///
    /// Fix (borrowed from rl2-coop's authoritative death notices + room filtering):
    ///  - Host periodically broadcasts the POSITIVE set of currently-alive enemies for
    ///    the current room, with (key, type, x, y). Reliable so late joiners converge.
    ///  - Guest, after a room settles, kills any local live enemy that is NOT present in
    ///    the host's alive set. Matching is by key first, then by same-type + nearest
    ///    position (immune to EnemyIndex drift).
    ///  - Guarded by room-coordinate match + a settle delay, so mid-transition snapshots
    ///    from the previous room cannot wipe freshly spawned enemies.
    /// </summary>
    internal static class EnemyAliveSync
    {
        private struct AliveRec
        {
            public int Key;
            public int Type;
            public float X, Y;
        }

        private static readonly NetDataWriter _writer = new NetDataWriter();
        private static float _sendTimer;
        private const float SendInterval = 1.0f;        // low frequency: alive set changes rarely
        private const int MaxAlive = 64;
        private const float MatchDist = 8f;            // generous: tolerate room init drift
        private const float RoomSettleDelay = 1.6f;    // don't reconcile until room fully initialized

        // guest state: host's authoritative alive set for the room we believe we're in
        private static readonly List<AliveRec> _hostAlive = new List<AliveRec>();
        private static float _hostAliveAt;             // local time of last received set
        private static float _roomEnteredAt;
        private static Vector2 _hostRoomCoords;
        // v2.8.3：主机权威房间坐标（ALIVE 包里的 cx/cy，来自 GetCurrentRoomCoords）。
        // 观战定位房间必须用它，不能用"存活怪平均坐标"——怪分散在相邻房间时
        // 平均点会落到错误房间，表现为观战房间在 7*Bonus*/16*Trap* 之间反复横跳。
        internal static Vector2 HostRoomCoords { get { return _hostRoomCoords; } }
        internal static bool HasHostRoomCoords
        {
            get { return _hasHostSet && Time.unscaledTime - _hostAliveAt < 4f; }
        }
        // v2.8.0：主机存活怪的平均世界坐标。观战时用它激活主机所在房间，
        // 而不是用已冻结的观战目标坐标（主机死后目标坐标过期 -> 两端敌人集合不同）。
        private static Vector3 _hostAliveCenter;
        private static bool _hasHostAliveCenter;
        internal static Vector3 HostAliveCenter { get { return _hostAliveCenter; } }
        internal static bool HasHostAliveCenter
        {
            get { return _hasHostAliveCenter && Time.unscaledTime - _hostAliveAt < 4f; }
        }
        private static Vector2 _lastLocalRoomCoords = new Vector2(float.NaN, 0f);
        private static bool _hasHostSet;
        private static float _lastLog;
        private static int _lastKillCount;

        internal static bool Active
        {
            get
            {
                return Score.Style == PlayStyle.CoOp && DownedState.InSession
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected;
            }
        }

        // ---------------- host: broadcast alive set ----------------

        internal static void HostTick()
        {
            if (!Active || NetworkManager.Instance == null || !NetworkManager.Instance.IsHost) return;
            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer < SendInterval) return;
            _sendTimer = 0f;

            try
            {
                Vector2 rc = GetCurrentRoomCoords();
                float cx = rc.x;
                float cy = rc.y;
                var all = EnemySync.GetEnemiesForPosition();
                int count = 0;
                if (all != null)
                    for (int i = 0; i < all.Length; i++)
                        if (all[i] != null && !all[i].IsDead) count++;
                if (count > MaxAlive) count = MaxAlive;

                _writer.Reset();
                _writer.Put((byte)MsgType.EnemyAlive);
                _writer.Put(cx);
                _writer.Put(cy);
                _writer.Put(count);

                int wrote = 0;
                if (all != null)
                {
                    for (int i = 0; i < all.Length && wrote < count; i++)
                    {
                        var e = all[i];
                        if (e == null || e.IsDead) continue;
                        Vector3 p = e.transform.position;
                        _writer.Put(EnemySync.BuildKey(e));
                        _writer.Put((int)e.EnemyType);
                        _writer.Put(p.x);
                        _writer.Put(p.y);
                        wrote++;
                    }
                }
                NetworkManager.Instance.SendEnemyAlive(_writer);

                if (Time.unscaledTime - _lastLog > 8f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [ALIVE] 主机广播存活集 room=(" + cx + "," + cy + ") alive=" + wrote);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ALIVE] host tick failed: " + Plugin.PatchError(ex));
            }
        }

        // ---------------- guest: receive + reconcile ----------------

        internal static void ApplyAlive(NetDataReader reader)
        {
            try
            {
                float cx = reader.GetFloat();
                float cy = reader.GetFloat();
                int n = reader.GetInt();
                if (!RLMod.Core.Protocol.Validation.ValidCoord(cx, cy)
                    || !RLMod.Core.Protocol.Validation.ValidCount(n, MaxAlive)) return;

                _hostAlive.Clear();
                for (int i = 0; i < n; i++)
                {
                    AliveRec rec;
                    rec.Key = reader.GetInt();
                    rec.Type = reader.GetInt();
                    rec.X = reader.GetFloat();
                    rec.Y = reader.GetFloat();
                    if (!RLMod.Core.Protocol.Validation.ValidCoord(rec.X, rec.Y)) continue;
                    _hostAlive.Add(rec);
                }
                _hostRoomCoords = new Vector2(cx, cy);
                _hostAliveAt = Time.unscaledTime;
                _hasHostSet = true;

                if (_hostAlive.Count > 0)
                {
                    float sx = 0f, sy = 0f;
                    for (int i = 0; i < _hostAlive.Count; i++) { sx += _hostAlive[i].X; sy += _hostAlive[i].Y; }
                    _hostAliveCenter = new Vector3(sx / _hostAlive.Count, sy / _hostAlive.Count, 0f);
                    _hasHostAliveCenter = true;
                }
                else _hasHostAliveCenter = false;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ALIVE] apply failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>房间切换时由 RoomTravelSync/SceneChangeSync 调用，重置对账基准。</summary>
        internal static void OnRoomChanged()
        {
            _roomEnteredAt = Time.unscaledTime;
            _hasHostSet = false;
            _hostAlive.Clear();
            _lastKillCount = 0;
        }

        /// <summary>客机每帧对账（由 LateUpdate 驱动）：杀掉权威存活集里没有的本地活怪。</summary>
        internal static void ClientTick()
        {
            if (!Active || NetworkManager.Instance == null || NetworkManager.Instance.IsHost) return;
            if (!_hasHostSet) return;
            if (Time.unscaledTime - _hostAliveAt > 4f) return;      // 主机集合太旧，别据此杀怪
            if (Time.unscaledTime - _roomEnteredAt < RoomSettleDelay) return; // 房间还没初始化稳

            // 只在两端房间坐标一致时对账，避免拿上一房间集合误杀。
            Vector2 localCoords = GetCurrentRoomCoords();
            // 自检测换房：本地房间坐标变了 -> 重置 settle 窗与权威集合（自包含，不改 RoomTravelSync）。
            if (float.IsNaN(_lastLocalRoomCoords.x) || (localCoords - _lastLocalRoomCoords).sqrMagnitude > 1f)
            {
                _lastLocalRoomCoords = localCoords;
                _roomEnteredAt = Time.unscaledTime;
                _hasHostSet = false;
                _hostAlive.Clear();
                return;
            }
            if ((localCoords - _hostRoomCoords).sqrMagnitude > 1f) return;

            try
            {
                var all = EnemySync.GetEnemiesForPosition();
                if (all == null) return;
                int killed = 0;
                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];
                    if (e == null || e.IsDead) continue;
                    if (IsAliveOnHost(e)) continue;

                    // 权威集合里没有这只怪 -> 主机侧已死，本地补杀。
                    EnemySync.ApplyRemoteDeathHostAuthoritative(e);
                    killed++;
                }
                if (killed > 0 && Time.unscaledTime - _lastLog > 3f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [ALIVE] 客机按权威存活集补杀 " + killed
                        + " 只（解决活怪数不一致）");
                }
                _lastKillCount = killed;
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ALIVE] client tick failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>本地这只怪是否出现在主机权威存活集（先 key，后同类型+位置兜底）。</summary>
        private static bool IsAliveOnHost(EnemyController e)
        {
            int key = EnemySync.BuildKey(e);
            Vector3 p = e.transform.position;
            int type = (int)e.EnemyType;
            float best = MatchDist * MatchDist;
            for (int i = 0; i < _hostAlive.Count; i++)
            {
                var r = _hostAlive[i];
                if (r.Key == key) return true;
                if (r.Type != type) continue;
                float dx = r.X - p.x, dy = r.Y - p.y;
                float d = dx * dx + dy * dy;
                if (d < best) best = d;
            }
            return best < MatchDist * MatchDist;
        }

        private static Vector2 GetCurrentRoomCoords()
        {
            // v2.8.9：统一走权威坐标源。倒地观战时尸体停在死亡房间，
            // GetCurrentPlayerRoom() 会锁死在旧房间，导致 ALIVE 广播/对账坐标滞后。
            return AuthorityRoom.GetCoords();
        }

        internal static void ResetAll()
        {
            _hostAlive.Clear();
            _hasHostSet = false;
            _sendTimer = 0f;
            _roomEnteredAt = Time.unscaledTime;
        }
    }
}
