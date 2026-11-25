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
        private static readonly NetDataWriter _rotWriter = new NetDataWriter();
        private static readonly Dictionary<int, Obs> _authority = new Dictionary<int, Obs>();
        private static readonly List<Component> _obsBuf = new List<Component>();
        // v2.9.0：旋转型机关的权威角度，key -> (角度, 接收时刻)
        private static readonly Dictionary<int, RotObs> _rotAuthority = new Dictionary<int, RotObs>();
        private static readonly List<Component> _rotBuf = new List<Component>();

        private struct RotObs
        {
            public float Angle;
            public float RecvAt;
        }

        // v2.8.8：Buzzsaw_Hazard 的位移发生在 m_pivot 子对象上（FixedUpdate 改
        // m_pivot.transform.localPosition.x），root 恒静止。旧版采 root.position 恒等于
        // 起点 → 主机有移动锯齿、客机不动。反射取一次 pivot 并缓存，采样/写回统一用它。
        private static readonly System.Reflection.FieldInfo _buzzsawPivotField =
            AccessTools.Field(typeof(Buzzsaw_Hazard), "m_pivot")
            ?? AccessTools.Field(typeof(Hazard), "m_pivot");  // 字段定义在基类 Hazard 上

        static ObstacleSync()
        {
            Plugin.Log.LogInfo("[RL2MP] [OBS] Buzzsaw pivot 反射="
                + (_buzzsawPivotField != null ? "OK" : "失败(退化为root)"));
        }

        /// <summary>取真正被驱动位移的 transform（Buzzsaw 是 pivot 子对象，其余是自身）。</summary>
        private static Transform GetSyncTransform(Component c)
        {
            // v2.9.0：所有 pivot 型 Hazard（Buzzsaw 位移、Windmill/Orbiter 旋转）都取
            // 基类 Hazard.m_pivot，root 恒静止。m_pivot 字段定义在基类，对子类实例同样有效。
            if (c is Hazard && _buzzsawPivotField != null)
            {
                try
                {
                    var pivotObj = _buzzsawPivotField.GetValue(c) as GameObject;
                    if (pivotObj != null && pivotObj.transform != null) return pivotObj.transform;
                }
                catch { }
            }
            return c != null ? c.transform : null;
        }

        // v2.5.3：主机掉帧热点——原来 15Hz × 3 次 Resources.FindObjectsOfTypeAll
        // （≈45 次/秒全 Unity 对象注册表扫描）。改为低频刷新缓存：正常 2Hz；
        // 换场景/换房间立即失效。障碍仍是 15Hz 采样，只是枚举不再每次全扫。
        private static readonly List<Component> _obsCache = new List<Component>();
        private static readonly List<Component> _rotCache = new List<Component>();
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

        // v3.0.1（借鉴 HKMP RotationComponent 差值检测）：签名相同且未到强制重发时刻就跳过该包。
        // 周期强制重发保证刚换房/刚加入的客机也能在 ForcePoseResend 内拿到权威快照。
        private static int _lastPoseSig;
        private static int _lastRotSig;
        private static float _lastPoseSendAt;
        private static float _lastRotSendAt;
        private const float ForcePoseResend = 0.5f;
        private const float ForceRotResend = 0.5f;

        private static float _lastLog;
        private static float _lastRecvLog;

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
            if (!Active || NetworkManager.Instance == null || !RoomAuthority.ShouldDriveWorld) return;
            _sendTimer += Time.unscaledDeltaTime;
            if (_sendTimer < SendInterval) return;
            _sendTimer = 0f;

            try
            {
                Vector2 rc = GetCurrentRoomCoords();
                CollectObstacles(_obsBuf, rc);
                float cx = rc.x;
                float cy = rc.y;

                // ---- v2.9.0：旋转型机关角度（独立包，不受位置障碍是否为空影响）----
                // v3.0.1：HKMP 式差值检测——先算签名，全静止且未到强制重发时刻就跳过。
                if (_rotCache.Count > 0)
                {
                    int rotSig = ComputeRotSignature(cx, cy);
                    bool rotChanged = rotSig != _lastRotSig;
                    bool rotForce = Time.unscaledTime - _lastRotSendAt >= ForceRotResend;
                    if (!rotChanged && !rotForce)
                    {
                        // 全静止：跳过，等下次强制重发（Rotate 角度没变，客机本地保持即可）
                    }
                    else
                    {
                    _lastRotSig = rotSig;
                    _lastRotSendAt = Time.unscaledTime;
                    _rotWriter.Reset();
                    _rotWriter.Put((byte)MsgType.ObstacleRotation);
                    _rotWriter.Put(NetClock.HostNow);
                    int rc2 = _rotCache.Count > MaxObstacles ? MaxObstacles : _rotCache.Count;
                    _rotWriter.Put(rc2);
                    int rw = 0;
                    for (int i = 0; i < rc2; i++)
                    {
                        var c = _rotCache[i];
                        if (c == null) continue;
                        float angle = GetRotation(c);
                        if (float.IsNaN(angle) || float.IsInfinity(angle)) continue;
                        _rotWriter.Put(BuildRotKey(c, cx, cy));
                        _rotWriter.Put(angle);
                        rw++;
                    }
                    if (rw > 0)
                    {
                        // count 头部写的是 rc2，若合法项更少需重写包
                        if (rw != rc2)
                        {
                            _rotWriter.Reset();
                            _rotWriter.Put((byte)MsgType.ObstacleRotation);
                            _rotWriter.Put(NetClock.HostNow);
                            _rotWriter.Put(rw);
                            for (int i = 0; i < rc2; i++)
                            {
                                var c = _rotCache[i];
                                if (c == null) continue;
                                float angle = GetRotation(c);
                                if (float.IsNaN(angle) || float.IsInfinity(angle)) continue;
                                _rotWriter.Put(BuildRotKey(c, cx, cy));
                                _rotWriter.Put(angle);
                            }
                        }
                        NetworkManager.Instance.SendObstacleRotation(_rotWriter);
                    }
                    }
                }

                // ---- 位置型障碍（原逻辑）----
                if (_obsBuf.Count == 0) return;
                int count = _obsBuf.Count > MaxObstacles ? MaxObstacles : _obsBuf.Count;

                _writer.Reset();
                _writer.Put((byte)MsgType.ObstaclePose);
                _writer.Put(NetClock.HostNow);
                _writer.Put(count);

                int wrote = 0;
                for (int i = 0; i < count; i++)
                {
                    var c = _obsBuf[i];
                    if (c == null) continue;
                    int key = BuildKey(c, i, cx, cy);
                    Transform st = GetSyncTransform(c);
                    if (st == null) continue;
                    Vector3 p = st.position;
                    if (!RLMod.Core.Protocol.Validation.ValidCoord(p.x, p.y, p.z)) continue;
                    _writer.Put(key);
                    _writer.Put(p.x);
                    _writer.Put(p.y);
                    _writer.Put(p.z);
                    wrote++;
                }
                if (Time.unscaledTime - _lastLog > 5f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[OBS] 主机采样障碍 room=(" + cx + "," + cy + ") 数量="
                        + wrote + "（权威坐标来自 AuthorityRoom）");
                }
                if (wrote == 0) return;
                // v3.0.1：HKMP 式差值检测——上面包已经构建好（保证 wrote/count 正确），
                // 这里按签名决定要不要真的发。全静止且未到强制重发时刻就跳过，省带宽。
                int poseSig = ComputePoseSignature(_obsBuf, count);
                bool poseChanged = poseSig != _lastPoseSig;
                bool poseForce = Time.unscaledTime - _lastPoseSendAt >= ForcePoseResend;
                if (!poseChanged && !poseForce) return;
                _lastPoseSig = poseSig;
                _lastPoseSendAt = Time.unscaledTime;
                // count 已在头部：若写过少，重写包（极少发生，保守处理）。
                if (wrote != count)
                {
                    _writer.Reset();
                    _writer.Put((byte)MsgType.ObstaclePose);
                    _writer.Put(NetClock.HostNow);
                    _writer.Put(wrote);
                    int w2 = 0;
                    for (int i = 0; i < count && w2 < wrote; i++)
                    {
                        var c = _obsBuf[i];
                        if (c == null) continue;
                        Transform st2 = GetSyncTransform(c);
                        if (st2 == null) continue;
                        Vector3 p = st2.position;
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
                if (Time.unscaledTime - _lastRecvLog > 5f)
                {
                    _lastRecvLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[OBS] 客机收到障碍包 n=" + n + " 权威条目=" + _authority.Count);
                }
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

        /// <summary>v2.9.0：接收主机旋转机关角度。</summary>
        internal static void ApplyRotation(NetDataReader reader)
        {
            try
            {
                if (reader.AvailableBytes < 8) return;
                double packetTime = reader.GetDouble();
                if (!RLMod.Core.Protocol.Validation.ValidHostTime(packetTime, NetClock.HostNow))
                    packetTime = NetClock.HostNow;
                int n = reader.GetInt();
                if (!RLMod.Core.Protocol.Validation.ValidCount(n, MaxObstacles)) return;

                float now = Time.unscaledTime;
                for (int i = 0; i < n; i++)
                {
                    int key = reader.GetInt();
                    float angle = reader.GetFloat();
                    if (float.IsNaN(angle) || float.IsInfinity(angle)) continue;
                    RotObs o;
                    o.Angle = angle;
                    o.RecvAt = now;
                    _rotAuthority[key] = o;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [OBS] apply rotation failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>客机 LateUpdate：把权威障碍物轨迹低通过写到本地实例（在原生 Update 之后）。</summary>
        internal static void ClientApplyLate()
        {
            if (!Active || NetworkManager.Instance == null || RoomAuthority.IsLocalAuthority) return;
            if (_authority.Count == 0) return;

            try
            {
                Vector2 rc = GetCurrentRoomCoords();
                float cx = rc.x;
                float cy = rc.y;
                CollectObstacles(_obsBuf, rc);
                // v2.9.0：即使没有位置障碍，也要继续跑下面的旋转角度覆盖（_rotCache 独立）。
                if (_obsBuf.Count == 0 && _rotCache.Count == 0 && _rotAuthority.Count == 0) return;

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

                    Transform st = GetSyncTransform(c);
                    if (st == null) continue;
                    Vector3 want = Evaluate(o, renderAt);
                    if (!o.HasRendered)
                    {
                        o.Rendered = want;
                        o.HasRendered = true;
                        try { st.position = want; } catch { }
                    }
                    else
                    {
                        o.Rendered = Vector3.Lerp(o.Rendered, want, Mathf.Clamp01(dt * 25f));
                        if ((st.position - o.Rendered).sqrMagnitude > MoveEpsilon * MoveEpsilon)
                        {
                            try { st.position = o.Rendered; } catch { }
                        }
                    }
                    _authority[key] = o;
                }

                // v2.9.0：旋转机关角度覆盖。必须在原生 Update 之后写（LateUpdate），
                // 否则客机本地 Update 会把角度再转回去。直接覆盖为权威角度，
                // 主机 culling 远离时角度冻结 -> 客机也停在同一角度，不再"原地打转"发散。
                if (_rotAuthority.Count > 0 && _rotCache.Count > 0)
                {
                    for (int i = 0; i < _rotCache.Count; i++)
                    {
                        var c = _rotCache[i];
                        if (c == null) continue;
                        RotObs ro;
                        if (!_rotAuthority.TryGetValue(BuildRotKey(c, cx, cy), out ro)) continue;
                        if (now - ro.RecvAt > StaleTimeout) continue;
                        SetRotation(c, ro.Angle);
                    }
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

            // v2.9.0：旋转型机关。它们转的是 m_pivot 的 localEulerAngles.z（root 静止），
            // 且主机 culling 远离时 pivot 被禁用、角度冻结，客机本地一直转 -> 发散。
            // 用独立缓存 + 角度通道同步，不混进位置通道。
            _rotCache.Clear();
            var windmills = Resources.FindObjectsOfTypeAll<Windmill_Hazard>();
            for (int i = 0; i < windmills.Length; i++)
            {
                var w = windmills[i];
                if (w != null && w.gameObject != null && w.gameObject.scene.IsValid()) _rotCache.Add(w);
            }
            var orbiters = Resources.FindObjectsOfTypeAll<Orbiter_Hazard>();
            for (int i = 0; i < orbiters.Length; i++)
            {
                var o = orbiters[i];
                if (o != null && o.gameObject != null && o.gameObject.scene.IsValid()) _rotCache.Add(o);
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

                // v3.0.1（借鉴 HKMP entityId）：key 只由障碍自身的稳定身份决定，
                // 不再掺入"当前房间坐标 cx/cy"。旧版两端不在同一房间时 cx/cy 不同 →
                // key 对不上 → 客机看不到障碍（真机："主机在后面，客机先到就原地打转"）。
                // 障碍 root 恒静止（动的是 pivot），其世界坐标是两端共享的同一物理位置，
                // 是比 sibling index 更稳的身份（池化复用时 sibling 会变）。
                Vector3 anchor = c != null && c.transform != null ? c.transform.position : Vector3.zero;
                int h = 17;
                h = h * 31 + kind;
                h = h * 31 + Mathf.RoundToInt(anchor.x * 2f);
                h = h * 31 + Mathf.RoundToInt(anchor.y * 2f);
                // 极端情况下两个同类障碍 root 坐标重合（0.5m 精度内），用 flatIndex 兜底区分；
                // 正常场景两端枚举顺序一致（RefreshObstacleCache 顺序确定），此项恒为同值。
                h = h * 31 + flatIndex;
                _ = cx; _ = cy;
                return h;
            }
        }

        // ---------- v2.9.0：旋转型机关角度同步 ----------

        /// <summary>
        /// 旋转机关的 key。root 静止，用房间坐标+类型+root 世界坐标(x,y,0.5m 精度)做键，
        /// 比池化对象的 sibling index 稳定（后者在 GetInstanceID 复用时会两端对不上）。
        /// </summary>
        private static int BuildRotKey(Component c, float cx, float cy)
        {
            unchecked
            {
                int kind = c is Windmill_Hazard ? 1 : (c is Orbiter_Hazard ? 2 : 9);
                Vector3 p = c != null && c.transform != null ? c.transform.position : Vector3.zero;
                // v3.0.1（借鉴 HKMP entityId）：去掉当前房间坐标段。root 静止，其世界坐标
                // 是稳定身份；两端即使处在不同房间也能对同一个机关算出同一个 key。
                int h = 17;
                h = h * 31 + kind;
                h = h * 31 + Mathf.RoundToInt(p.x * 2f);
                h = h * 31 + Mathf.RoundToInt(p.y * 2f);
                _ = cx; _ = cy;
                return h;
            }
        }

        /// <summary>v3.0.1：旋转机关包内容签名（与 BuildRotKey/GetRotation 用同一量化精度）。</summary>
        private static int ComputeRotSignature(float cx, float cy)
        {
            unchecked
            {
                int h = 17;
                int rc = _rotCache.Count > MaxObstacles ? MaxObstacles : _rotCache.Count;
                for (int i = 0; i < rc; i++)
                {
                    var c = _rotCache[i];
                    if (c == null) continue;
                    float angle = GetRotation(c);
                    if (float.IsNaN(angle) || float.IsInfinity(angle)) continue;
                    h = h * 31 + BuildRotKey(c, cx, cy);
                    h = h * 31 + Mathf.RoundToInt(angle * 10f);   // 0.1° 精度
                }
                return h;
            }
        }

        /// <summary>v3.0.1：位置障碍包内容签名（与 BuildKey/采样位置用同一量化精度）。</summary>
        private static int ComputePoseSignature(List<Component> list, int count)
        {
            unchecked
            {
                int h = 17;
                for (int i = 0; i < count; i++)
                {
                    var c = list[i];
                    if (c == null) continue;
                    Transform st = GetSyncTransform(c);
                    if (st == null) continue;
                    Vector3 p = st.position;
                    if (!RLMod.Core.Protocol.Validation.ValidCoord(p.x, p.y, p.z)) continue;
                    h = h * 31 + BuildKey(c, i, 0f, 0f);
                    h = h * 31 + Mathf.RoundToInt(p.x * 100f);
                    h = h * 31 + Mathf.RoundToInt(p.y * 100f);
                    h = h * 31 + Mathf.RoundToInt(p.z * 100f);
                }
                return h;
            }
        }

        /// <summary>读权威角度：优先 CurrentRotation 属性（语义明确），回退 pivot 角度。</summary>
        private static float GetRotation(Component c)
        {
            try
            {
                var t = c.GetType();
                var prop = AccessTools.Property(t, "CurrentRotation");
                if (prop != null) return Convert.ToSingle(prop.GetValue(c, null));
            }
            catch { }
            try
            {
                var pivot = GetSyncTransform(c);
                if (pivot != null) return pivot.localEulerAngles.z;
            }
            catch { }
            return 0f;
        }

        /// <summary>客机覆盖角度：写 pivot 的 localEulerAngles.z（root 静止）。</summary>
        private static void SetRotation(Component c, float angle)
        {
            try
            {
                var pivot = GetSyncTransform(c);
                if (pivot == null) return;
                var e = pivot.localEulerAngles;
                e.z = angle;
                pivot.localEulerAngles = e;
            }
            catch { }
        }

        private static Vector2 GetCurrentRoomCoords()
        {
            // v2.8.9：统一走权威坐标源。倒地观战时尸体停在死亡房间，
            // 旧实现锁死在旧房间 -> BuildKey 房间段与存活方不一致 -> 客机收不到障碍物。
            return AuthorityRoom.GetCoords();
        }

        // ---------------- v3.0.4：机关离屏 culling 拦截 ----------------

        /// <summary>
        /// 反编译铁证（Hazard.SetIsCulled + HazardManager.CullingSphereStateChanged）：
        /// 机关离屏时游戏调用 SetIsCulled(true) -> m_pivot.SetActive(false) + Animator.enabled=false，
        /// 于是 Windmill_Hazard.Update()/Orbiter_Hazard.Update() 对 pivot 角度的写入失效，
        /// 机关"原地打转/冻结"。主机离屏而客机在场时，主机采到冻结值并广播 -> 客机看到静止机关。
        ///
        /// 借鉴 TUNIC 的"永远 AlwaysAnimate"精神，但针对 RL2 的实际机制（是 SetActive 而非
        /// Animator.cullingMode）：协作对局内，只要本机持有世界驱动权，就禁止把"我们同步的
        /// 旋转机关"cull 掉，让它持续更新、持续采样广播。单机/竞速/非权威端完全不改原生行为。
        /// </summary>
        internal static void ApplyCullingPatch(Harmony harmony)
        {
            try
            {
                var target = AccessTools.Method(typeof(Hazard), "SetIsCulled");
                if (target == null)
                {
                    Plugin.Log.LogWarning("[OBS] Hazard.SetIsCulled 未找到，机关离屏保活未启用。");
                    return;
                }
                harmony.Patch(target,
                    prefix: new HarmonyMethod(typeof(ObstacleSync), "SetIsCulledPrefix"));
                Plugin.Log.LogInfo("[RL2MP] [OBS] 机关离屏保活补丁 OK（权威端不 cull 旋转机关）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[OBS] 机关离屏保活补丁失败: " + Plugin.PatchError(ex));
            }
        }

        private static void SetIsCulledPrefix(Hazard __instance, ref bool culled)
        {
            // 只在"需要本机权威采样"时放行；非权威端让游戏正常 cull，省性能。
            if (!culled) return;
            if (!Active) return;
            if (NetworkManager.Instance == null || !RoomAuthority.ShouldDriveWorld) return;
            // 只保活我们同步的旋转机关类型，其余机关（含 RisingWater/Sentry 等）保持原生剔除。
            if (__instance is Windmill_Hazard || __instance is Orbiter_Hazard)
                culled = false;
        }

        internal static void ResetAll()
        {
            _authority.Clear();
            _obsBuf.Clear();
            _obsCache.Clear();
            _rotAuthority.Clear();
            _rotBuf.Clear();
            _rotCache.Clear();
            _cacheValid = false;
            _collectTime = -999f;
            _cacheSceneHandle = int.MinValue;
            _sendTimer = 0f;
            // v3.0.1：签名/发送时刻清零，保证新会话首包一定发出（不因旧签名被差值跳过）
            _lastPoseSig = 0;
            _lastRotSig = 0;
            _lastPoseSendAt = 0f;
            _lastRotSendAt = 0f;
        }
    }
}
