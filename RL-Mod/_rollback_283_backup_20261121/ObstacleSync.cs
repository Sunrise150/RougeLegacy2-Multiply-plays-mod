using System;
using System.Collections.Generic;
using HarmonyLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.5.0: authoritative position sync for moving obstacles (elevators, buzzsaws,
    /// earth-shift platforms) that are driven by each machine's own Time.time and thus
    /// diverge between host and guest.
    ///
    /// Borrows rl2-coop's snapshot-on-host-timeline approach:
    ///  - Host samples each obstacle's world position at 15Hz, stamped with NetClock.HostNow,
    ///    keyed by room coords + type + sibling index (stable across machines; never InstanceID).
    ///  - Guest stores a 3-sample history per obstacle and renders at HostNow - delay via
    ///    late low-pass override in LateUpdate (after each obstacle's Update/FixedUpdate).
    ///  - Positions only; the client's own physics/collision stays authoritative for the player.
    ///  - All keys/positions validated (NaN/Inf/count caps) before reaching the transform.
    /// </summary>
    internal static class ObstacleSync
    {
        private struct Sample
        {
            public Vector3 Pos;
            public double At;
        }

        private struct Obs
        {
            public Sample S2, S1, S0;
            public bool HasRendered;
            public Vector3 Rendered;
            public float RecvAt;
        }

        private static readonly NetDataWriter _writer = new NetDataWriter();
        private static readonly Dictionary<int, Obs> _authority = new Dictionary<int, Obs>();
        private static readonly List<Component> _obsBuf = new List<Component>();

        // v2.5.3：主机掉帧热点——原来 15Hz × 3 次 Resources.FindObjectsOfTypeAll
        // （≈45 次/秒全 Unity 对象注册表扫描）。改为低频刷新缓存：正常 2Hz；
        // 换场景/换房间立即失效。障碍仍是 15Hz 采样，只是枚举不再每次全扫。
        private static readonly List<Component> _obsCache = new List<Component>();
        private static bool _cacheValid;
        private static float _collectTime = -999f;
        private static int _cacheSceneHandle = int.MinValue;
        private static float _cacheRoomX;
        private static float _cacheRoomY;
        private const float CollectInterval = 0.5f;

        private static float _sendTimer;
        private const float SendInterval = 0.0667f;   // 15Hz
        private const float StaleTimeout = 0.8f;
        private const float RenderDelay = 0.10f;      // host-timeline render delay
        private const float MoveEpsilon = 0.01f;
        private const int MaxObstacles = ValidationMax;
        private const int ValidationMax = 64;

        private static float _lastLog;

        internal static bool Active
        {
            get
            {
                return Score.Style == PlayStyle.CoOp && DownedState.InSession
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected;
            }
        }

        // ---------------- host: sample + broadcast ----------------

        internal static void HostTick()
        {
            if (!Active || NetworkManager.Instance == null || !NetworkManager.Instance.IsHost) return;
            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer < SendInterval) return;
            _sendTimer = 0f;

            try
            {
                Vector2 rc = GetCurrentRoomCoords();
                CollectObstacles(_obsBuf, rc);
                if (_obsBuf.Count == 0) return;
                int count = _obsBuf.Count > MaxObstacles ? MaxObstacles : _obsBuf.Count;

                _writer.Reset();
                _writer.Put((byte)MsgType.ObstaclePose);
                _writer.Put(NetClock.HostNow);
                _writer.Put(count);

                float cx = rc.x;
                float cy = rc.y;

                int wrote = 0;
                for (int i = 0; i < count; i++)
                {
                    var c = _obsBuf[i];
                    if (c == null) continue;
                    int key = BuildKey(c, i, cx, cy);
                    Vector3 p = c.transform.position;
                    if (!RLMod.Core.Protocol.Validation.ValidCoord(p.x, p.y, p.z)) continue;
                    _writer.Put(key);
                    _writer.Put(p.x);
                    _writer.Put(p.y);
                    _writer.Put(p.z);
                    wrote++;
                }
                if (wrote == 0) return;
                // count 已在头部：若写过少，重写包（极少发生，保守处理）。
                if (wrote != count)
                {
                    _writer.Reset();
                    _writer.Put((byte)MsgType.ObstaclePose);
                    _writer.Put(NetClock.HostNow);
                    _writer.Put(wrote);
                    // 重扫一次只写合法项（简单起见：重新逐项写）
                    int w2 = 0;
                    for (int i = 0; i < count && w2 < wrote; i++)
                    {
                        var c = _obsBuf[i];
                        if (c == null) continue;
                        Vector3 p = c.transform.position;
                        if (!RLMod.Core.Protocol.Validation.ValidCoord(p.x, p.y, p.z)) continue;
                        _writer.Put(BuildKey(c, i, cx, cy));
                        _writer.Put(p.x); _writer.Put(p.y); _writer.Put(p.z);
                        w2++;
                    }
                }
                NetworkManager.Instance.SendObstaclePose(_writer);

                if (Time.unscaledTime - _lastLog > 10f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [OBS] 主机广播移动障碍 pos=" + wrote + "（15Hz）");
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [OBS] host tick failed: " + Plugin.PatchError(ex));
            }
        }

        // ---------------- guest: receive + render ----------------

        internal static void ApplySnapshot(NetDataReader reader)
        {
            try
            {
                if (reader.AvailableBytes < 12) return;
                double packetTime = reader.GetDouble();
                if (!RLMod.Core.Protocol.Validation.ValidHostTime(packetTime, NetClock.HostNow))
                    packetTime = NetClock.HostNow;
                int n = reader.GetInt();
                if (!RLMod.Core.Protocol.Validation.ValidCount(n, MaxObstacles)) return;

                float now = Time.unscaledTime;
                for (int i = 0; i < n; i++)
                {
                    int key = reader.GetInt();
                    float x = reader.GetFloat(), y = reader.GetFloat(), z = reader.GetFloat();
                    if (!RLMod.Core.Protocol.Validation.ValidCoord(x, y, z)) continue;

                    Obs o;
                    if (!_authority.TryGetValue(key, out o)) o = new Obs();
                    else { o.S2 = o.S1; o.S1 = o.S0; }
                    o.S0 = new Sample { Pos = new Vector3(x, y, z), At = packetTime };
                    o.RecvAt = now;
                    _authority[key] = o;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [OBS] apply failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>客机 LateUpdate：把权威障碍物轨迹低通过写到本地实例（在原生 Update 之后）。</summary>
        internal static void ClientApplyLate()
        {
            if (!Active || NetworkManager.Instance == null || NetworkManager.Instance.IsHost) return;
            if (_authority.Count == 0) return;

            try
            {
                Vector2 rc = GetCurrentRoomCoords();
                float cx = rc.x;
                float cy = rc.y;
                CollectObstacles(_obsBuf, rc);
                if (_obsBuf.Count == 0) return;

                float now = Time.unscaledTime;
                float dt = Mathf.Clamp(Time.unscaledDeltaTime, 0.001f, 0.05f);
                double renderAt = NetClock.HostNow - RenderDelay;

                for (int i = 0; i < _obsBuf.Count; i++)
                {
                    var c = _obsBuf[i];
                    if (c == null) continue;
                    int key = BuildKey(c, i, cx, cy);
                    Obs o;
                    if (!_authority.TryGetValue(key, out o)) continue;
                    if (now - o.RecvAt > StaleTimeout) continue;

                    Vector3 want = Evaluate(o, renderAt);
                    if (!o.HasRendered)
                    {
                        o.Rendered = want;
                        o.HasRendered = true;
                        try { c.transform.position = want; } catch { }
                    }
                    else
                    {
                        o.Rendered = Vector3.Lerp(o.Rendered, want, Mathf.Clamp01(dt * 25f));
                        if ((c.transform.position - o.Rendered).sqrMagnitude > MoveEpsilon * MoveEpsilon)
                        {
                            try { c.transform.position = o.Rendered; } catch { }
                        }
                    }
                    _authority[key] = o;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [OBS] client apply failed: " + Plugin.PatchError(ex));
            }
        }

        private static Vector3 Evaluate(Obs o, double renderAt)
        {
            if (o.S0.At <= 0.0001) return o.S0.Pos;
            if (renderAt >= o.S0.At) return o.S0.Pos;
            if (o.S1.At > 0.0001)
            {
                if (renderAt >= o.S1.At)
                {
                    double span = o.S0.At - o.S1.At;
                    double f = span > 0.0001 ? (renderAt - o.S1.At) / span : 1.0;
                    return Vector3.Lerp(o.S1.Pos, o.S0.Pos, Mathf.Clamp01((float)f));
                }
                if (o.S2.At > 0.0001 && renderAt >= o.S2.At)
                {
                    double span = o.S1.At - o.S2.At;
                    double f = span > 0.0001 ? (renderAt - o.S2.At) / span : 1.0;
                    return Vector3.Lerp(o.S2.Pos, o.S1.Pos, Mathf.Clamp01((float)f));
                }
                return o.S2.At > 0.0001 ? o.S2.Pos : o.S1.Pos;
            }
            return o.S0.Pos;
        }

        // ---------------- enumeration + stable key ----------------

        /// <summary>Collect all continuously-moving obstacles in the current scene. Deterministic order.</summary>
        private static void CollectObstacles(List<Component> outList, Vector2 room)
        {
            outList.Clear();

            // 换场景/换房间立即重建缓存；否则 2Hz 刷新即可。
            int sceneHandle = UnityEngine.SceneManagement.SceneManager.GetActiveScene().handle;
            bool roomChanged = Mathf.Abs(room.x - _cacheRoomX) > 0.5f
                            || Mathf.Abs(room.y - _cacheRoomY) > 0.5f;
            float now = Time.unscaledTime;
            if (!_cacheValid || sceneHandle != _cacheSceneHandle || roomChanged
                || now - _collectTime >= CollectInterval)
            {
                RefreshObstacleCache(sceneHandle, room, now);
            }

            for (int i = 0; i < _obsCache.Count; i++)
            {
                var c = _obsCache[i];
                if (c == null || c.gameObject == null || !c.gameObject.scene.IsValid()) continue;
                outList.Add(c);
            }
        }

        /// <summary>低频全量枚举（唯一还走 Resources.FindObjectsOfTypeAll 的地方）。</summary>
        private static void RefreshObstacleCache(int sceneHandle, Vector2 room, float now)
        {
            _cacheSceneHandle = sceneHandle;
            _cacheRoomX = room.x;
            _cacheRoomY = room.y;
            _collectTime = now;
            _cacheValid = true;
            _obsCache.Clear();

            // Elevator/other time-driven SpecialPlatforms.
            var platforms = Resources.FindObjectsOfTypeAll<SpecialPlatform>();
            for (int i = 0; i < platforms.Length; i++)
            {
                var p = platforms[i];
                if (p == null || p.gameObject == null || !p.gameObject.scene.IsValid()) continue;
                if (p is ElevatorPlatform) _obsCache.Add(p); // only the continuously-moving one matters
            }
            // Buzzsaws that patrol on Time.
            var saws = Resources.FindObjectsOfTypeAll<Buzzsaw_Hazard>();
            for (int i = 0; i < saws.Length; i++)
            {
                var s = saws[i];
                if (s != null && s.gameObject != null && s.gameObject.scene.IsValid()) _obsCache.Add(s);
            }
            // Earth-shift platforms (Lerp by Time.deltaTime).
            var shifts = Resources.FindObjectsOfTypeAll<EarthShiftPlatformController>();
            for (int i = 0; i < shifts.Length; i++)
            {
                var e = shifts[i];
                if (e != null && e.gameObject != null && e.gameObject.scene.IsValid()) _obsCache.Add(e);
            }
        }

        /// <summary>
        /// Stable key: room coords + obstacle kind + index-within-kind. Never InstanceID.
        /// The caller passes the flat list index; we classify by type so the per-kind index
        /// matches between machines as long as collect order is deterministic.
        /// </summary>
        private static int BuildKey(Component c, int flatIndex, float cx, float cy)
        {
            unchecked
            {
                int kind = 1;
                if (c is ElevatorPlatform) kind = 1;
                else if (c is Buzzsaw_Hazard) kind = 2;
                else if (c is EarthShiftPlatformController) kind = 3;

                // Sibling index within parent is stable for identical scenes (same seed/map),
                // unlike GetInstanceID or a moving world position.
                int sibling = c.transform != null && c.transform.parent != null
                    ? c.transform.GetSiblingIndex()
                    : flatIndex;

                int h = 17;
                h = h * 31 + Mathf.RoundToInt(cx * 10f);
                h = h * 31 + Mathf.RoundToInt(cy * 10f);
                h = h * 31 + kind;
                h = h * 31 + sibling;
                return h;
            }
        }

        private static Vector2 GetCurrentRoomCoords()
        {
            try
            {
                var br = PlayerManager.GetCurrentPlayerRoom();
                if (br == null) return Vector2.zero;
                var r = br as Room;
                return r != null ? r.Coordinates : (Vector2)br.transform.position;
            }
            catch { return Vector2.zero; }
        }

        internal static void ResetAll()
        {
            _authority.Clear();
            _obsBuf.Clear();
            _obsCache.Clear();
            _cacheValid = false;
            _collectTime = -999f;
            _cacheSceneHandle = int.MinValue;
            _sendTimer = 0f;
        }
    }
}
