using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LiteNetLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 子弹视觉同步：本地玩家发射的投射物广播给远端，远端用游戏原生 FireProjectile
    /// 复刻一颗"只出画面不出伤害"的弹体。
    ///
    /// 设计要点：
    /// 1. 发送端 postfix ProjectileManager.FireProjectile —— 这是全游戏唯一的弹体发射入口，
    ///    捕获发射后的终态快照（世界坐标/角度/缩放符号/朝向/速度/加速度/寿命），远端按终态复刻，
    ///    不依赖两边的朝向/位置对齐。
    /// 2. 接收端以远端分身为 source 调同一个 FireProjectile：池对象无父级、component 为 null
    ///    时 ApplyProjectileDamage 提前返回，不会读错数值；随后覆盖终态字段。
    /// 3. 免伤：InstanceID 进 _visualOnly 集合，
    ///    patch BaseCharacterHitResponse.StartHitResponse（伤害/击退/眩晕/击杀记账全走这里）
    ///    与 Breakable.TriggerCollision（罐子/箱子被 0 伤也会 -1HP，必须拦），
    ///    弹体自身的撞墙火花、命中特效不受影响。
    /// 4. 回收：Projectile_RL.OnDisable 时移出集合，防止池复用同实例时误判真弹。
    /// </summary>
    internal static class ProjectileSync
    {
        /// <summary>视觉弹的 InstanceID。仅这些弹体免除伤害结算。</summary>
        private static readonly HashSet<int> _visualOnly = new HashSet<int>();

        /// <summary>接收端正在生成视觉弹：防止我们的 FireProjectile 调用再次触发发送 postfix 死循环。</summary>
        private static bool _spawningRemote;

        private static FieldInfo _poolDictField;

        private static float _lastErrLog;

        // v2.7.0：发射/消失配对。spawnId 由发射端分配，跨端用 (ownerId, spawnId) 定位视觉弹。
        private static uint _nextSpawnId = 1;
        private static readonly Dictionary<int, uint> _spawnIdByInstance = new Dictionary<int, uint>();
        private static readonly Dictionary<long, Projectile_RL> _visualBySpawnId = new Dictionary<long, Projectile_RL>();
        private static long SpawnKey(int ownerId, uint spawnId)
        {
            return (((long)ownerId) << 32) | spawnId;
        }

        // ---- v3.0.6：投射物姿态同步（火环 carousel 靠 C# 每帧旋转，发射快照会定格）----
        /// <summary>本地玩家发射的活跃弹，spawnId -> 弹体，供 30Hz 采样世界位姿。</summary>
        private static readonly Dictionary<uint, Projectile_RL> _poseTracked = new Dictionary<uint, Projectile_RL>();
        /// <summary>接收端每个视觉弹上一次应用的姿态（用于算速度，避免包间冻结/抖动）。</summary>
        private struct PoseMem
        {
            public Vector3 Pos;
            public float Time;
        }
        private static readonly Dictionary<long, PoseMem> _poseMem = new Dictionary<long, PoseMem>();
        private static float _lastPoseSend;
        private const float PoseSendInterval = 0.033f; // 30Hz：旋转火环 360°/s 时每步约 12°

        internal static void Apply(Harmony harmony)
        {
            TryPatch(harmony,
                AccessTools.Method(typeof(ProjectileManager), "FireProjectile"),
                "FireProjectilePostfix", "弹体发射同步", postfixName: "FireProjectilePostfix");

            TryPatch(harmony,
                AccessTools.Method(typeof(Projectile_RL), "OnDisable"),
                null, "弹体回收清理", postfixName: "ProjectileDisablePostfix");

            TryPatch(harmony,
                AccessTools.Method(typeof(BaseCharacterHitResponse), "StartHitResponse"),
                "StartHitResponsePrefix", "视觉弹免伤");

            TryPatch(harmony,
                AccessTools.Method(typeof(Breakable), "TriggerCollision"),
                "BreakableTriggerPrefix", "视觉弹不破坏可破坏物");
        }

        // ---------------- 发送端 ----------------

        /// <summary>FireProjectile postfix：本地玩家发射的弹 -> 广播终态快照。</summary>
        internal static void FireProjectilePostfix(GameObject source, string projectileName, Projectile_RL __result)
        {
            try
            {
                if (__result == null || _spawningRemote) return;

                var nm = NetworkManager.Instance;
                if (nm == null || !nm.Connected) return;

                var ps = PlayerSync.Instance;
                if (ps == null || ps.CachedLocalPlayerObject == null) return;
                if (source == null) return;
                var localGo = ps.CachedLocalPlayerObject;

                int ownerId;
                if (source == localGo || source.transform.root == localGo.transform.root)
                {
                    ownerId = nm.LocalPlayerId;
                }
                else
                {
                    // v2.8.0：主机权威敌人发射的弹也让客机看到。ownerId 用负数编码敌人 key，
                    // 与玩家 id（0..3）区分。客机怪已 Puppetize，不会重复生成本地弹。
                    if (!nm.IsHost) return;
                    var enemy = FindEnemyOwner(source);
                    if (enemy == null) return;
                    int ekey = EnemySync.BuildKey(enemy) & 0x7fffffff;
                    if (ekey == 0) ekey = 1;
                    ownerId = -ekey;   // 保证负数，不会和玩家 id 冲突
                }

                var t = __result.transform;
                bool snap = __result.SnapToOwner;

                // 给这颗弹一个会话内唯一 id，供"提前消失"消息精确配对
                uint spawnId = _nextSpawnId++;
                int instId = __result.gameObject.GetInstanceID();
                _spawnIdByInstance[instId] = spawnId;
                // v3.0.6：只跟踪本地玩家自己的弹做姿态广播（敌人弹发射端不驱动 carousel）。
                if (ownerId >= 0 && ownerId == nm.LocalPlayerId)
                    _poseTracked[spawnId] = __result;

                var w = new NetDataWriter();
                w.Put((byte)MsgType.Projectile);
                w.Put(ownerId);
                w.Put(spawnId);
                w.Put(projectileName ?? "");
                w.Put(t.position.x); w.Put(t.position.y); w.Put(t.position.z);
                w.Put(t.eulerAngles.z);
                w.Put(t.localScale.x);
                Vector2 heading = __result.Heading;
                w.Put(heading.x); w.Put(heading.y);
                w.Put(__result.Speed);
                Vector2 acc = __result.Acceleration;
                w.Put(acc.x); w.Put(acc.y);
                w.Put(snap);
                if (snap)
                {
                    // Snap 弹挂在 owner 身上：发相对偏移，远端同样挂到分身下
                    w.Put(t.localPosition.x); w.Put(t.localPosition.y); w.Put(t.localPosition.z);
                    w.Put(t.localEulerAngles.z);
                }
                w.Put(__result.LifespanTimer);

                nm.SendProjectile(w);
            }
            catch (Exception ex)
            {
                float now = Time.unscaledTime;
                if (now - _lastErrLog > 5f)
                {
                    _lastErrLog = now;
                    Plugin.Log.LogWarning("[RL2MP] [PROJ] 发送端捕获失败: " + Plugin.PatchError(ex));
                }
            }
        }

        /// <summary>
        /// Projectile_RL.OnDisable postfix：池回收时移出免伤集合，并通知对端同步消失。
        /// 火墙/持续法术结束走 SetActive(false) 或 FlagForDestruction，最终都会到这里；
        /// 此前没有 despawn，对端视觉弹就停在最后一帧永不消失。
        /// </summary>
        internal static void ProjectileDisablePostfix(Projectile_RL __instance)
        {
            if (__instance == null) return;
            int instId = __instance.gameObject.GetInstanceID();
            _visualOnly.Remove(instId);

            uint spawnId;
            if (!_spawnIdByInstance.TryGetValue(instId, out spawnId)) return;
            _spawnIdByInstance.Remove(instId);
            _poseTracked.Remove(spawnId);

            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.ProjectileDespawn);
            w.Put(nm.LocalPlayerId);
            w.Put(spawnId);
            nm.SendProjectileDespawn(w);
        }

        // ---------------- v3.0.6：投射物姿态同步 ----------------

        /// <summary>
        /// 一批活跃弹的世界位姿。字段顺序即线协议顺序（协议 3007）。
        /// </summary>
        internal sealed class PoseBatch
        {
            public int OwnerId;
            public uint[] Ids;
            public float[] X;
            public float[] Y;
            public float[] Z;
            public float[] RotZ;
            public float[] ScaleX;

            internal void Write(NetDataWriter w)
            {
                w.Put(OwnerId);
                w.Put((byte)Ids.Length);
                for (int i = 0; i < Ids.Length; i++)
                {
                    w.Put(Ids[i]);
                    w.Put(X[i]); w.Put(Y[i]); w.Put(Z[i]);
                    w.Put(RotZ[i]);
                    w.Put(ScaleX[i]);
                }
            }

            internal static PoseBatch Read(NetDataReader r)
            {
                try
                {
                    var b = new PoseBatch();
                    b.OwnerId = r.GetInt();
                    byte n = r.GetByte();
                    if (n == 0 || n > 64) return null;
                    b.Ids = new uint[n];
                    b.X = new float[n]; b.Y = new float[n]; b.Z = new float[n];
                    b.RotZ = new float[n]; b.ScaleX = new float[n];
                    for (int i = 0; i < n; i++)
                    {
                        b.Ids[i] = r.GetUInt();
                        b.X[i] = r.GetFloat(); b.Y[i] = r.GetFloat(); b.Z[i] = r.GetFloat();
                        b.RotZ[i] = r.GetFloat();
                        b.ScaleX[i] = r.GetFloat();
                    }
                    return b;
                }
                catch
                {
                    return null;
                }
            }
        }

        /// <summary>
        /// 由 NetworkManager.Update 每帧调用，内部按 30Hz 节流。
        /// 采样本地玩家所有活跃弹的世界位姿并广播——火环(FlameBarrier)的弹挂在
        /// CarouselGO 下靠 C# 每帧旋转，发射快照只发一次会让远端定格第一帧；
        /// 这里持续送世界坐标，远端直接覆盖，环就转起来了（HKMP 位置/旋转差值思路）。
        /// </summary>
        internal static void TickPose()
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected || nm.LocalPlayerId < 0) return;
            if (_poseTracked.Count == 0) return;
            float now = Time.unscaledTime;
            if (now - _lastPoseSend < PoseSendInterval) return;
            _lastPoseSend = now;

            var ids = new List<uint>();
            var xs = new List<float>();
            var ys = new List<float>();
            var zs = new List<float>();
            var rots = new List<float>();
            var scales = new List<float>();
            foreach (var kv in _poseTracked)
            {
                var p = kv.Value;
                if (p == null || !p.gameObject.activeInHierarchy) continue;
                var t = p.transform;
                ids.Add(kv.Key);
                xs.Add(t.position.x); ys.Add(t.position.y); zs.Add(t.position.z);
                rots.Add(t.eulerAngles.z);
                scales.Add(t.localScale.x);
            }
            if (ids.Count == 0 || ids.Count > 64) return;

            var batch = new PoseBatch
            {
                OwnerId = nm.LocalPlayerId,
                Ids = ids.ToArray(), X = xs.ToArray(), Y = ys.ToArray(), Z = zs.ToArray(),
                RotZ = rots.ToArray(), ScaleX = scales.ToArray()
            };
            var w = new NetDataWriter();
            w.Put((byte)MsgType.ProjectilePose);
            batch.Write(w);
            nm.SendProjectilePose(w);
        }

        /// <summary>收到权威端广播的弹姿态：按 (ownerId, spawnId) 找到视觉弹，覆盖世界位姿。</summary>
        internal static void ApplyPose(PoseBatch b)
        {
            if (b == null) return;
            float now = Time.unscaledTime;
            for (int i = 0; i < b.Ids.Length; i++)
            {
                long key = SpawnKey(b.OwnerId, b.Ids[i]);
                Projectile_RL p;
                if (!_visualBySpawnId.TryGetValue(key, out p) || p == null) continue;
                var t = p.transform;
                var target = new Vector3(b.X[i], b.Y[i], b.Z[i]);

                // 首次接管这颗弹：清掉它自身的飞行速度/加速度，改为完全由姿态驱动。
                // 否则 Corgi 的本地飞行会和姿态覆盖打架，火环弹会边转边飘。
                long mk = key;
                if (!_poseMem.ContainsKey(mk))
                {
                    try
                    {
                        p.Speed = 0f;
                        p.Acceleration = Vector2.zero;
                        p.SetCorgiVelocity(Vector2.zero);
                    }
                    catch { }
                }
                _poseMem[mk] = new PoseMem { Pos = target, Time = now };

                t.position = target;
                t.eulerAngles = new Vector3(0f, 0f, b.RotZ[i]);
                var s = t.localScale;
                if (b.ScaleX[i] != 0f && Mathf.Abs(s.x) > 0.0001f)
                {
                    bool wantNeg = b.ScaleX[i] < 0f;
                    bool isNeg = s.x < 0f;
                    if (wantNeg != isNeg) t.localScale = new Vector3(-s.x, s.y, s.z);
                }
            }
        }

        // ---------------- 免伤补丁 ----------------

        /// <summary>StartHitResponse prefix：视觉弹命中角色时不造成伤害/击退/眩晕/击杀。</summary>
        internal static bool StartHitResponsePrefix(IDamageObj damageObj)
        {
            if (damageObj is Projectile_RL p && IsVisualOnly(p)) return false;
            return true;
        }

        /// <summary>Breakable.TriggerCollision prefix：视觉弹不破坏罐子/箱子（0 伤也会 -1HP）。</summary>
        internal static bool BreakableTriggerPrefix(IDamageObj damageObj)
        {
            if (damageObj is Projectile_RL p && IsVisualOnly(p)) return false;
            return true;
        }

        internal static bool IsVisualOnly(Projectile_RL p)
        {
            return p != null && _visualOnly.Contains(p.gameObject.GetInstanceID());
        }

        // ---------------- 接收端 ----------------

        /// <summary>用远端分身作 source 复刻一颗视觉弹，然后覆盖终态字段。</summary>
        internal static void SpawnRemoteVisual(int ownerId, uint spawnId, string name,
            float px, float py, float pz, float eulerZ, float scaleX,
            float headingX, float headingY, float speed,
            float accX, float accY, bool snap,
            float snapLx, float snapLy, float snapLz, float snapRotZ, float lifespan)
        {
            if (_spawningRemote) return;

            var ps = PlayerSync.Instance;
            if (ps == null) return;
            GameObject sourceGo;
            if (ownerId >= 0)
            {
                // 玩家弹：以对端分身为 source
                if (!ps.RemotePlayers.TryGetValue(ownerId, out sourceGo) || sourceGo == null) return;
            }
            else
            {
                // 敌人弹（ownerId 是负数 key）：对端没有"敌人分身"对象，
                // 借本地玩家对象作 source 仅为满足 FireProjectile 的非空检查；
                // 弹体已标记免伤，不会误伤自己。
                sourceGo = ps.CachedLocalPlayerObject;
                if (sourceGo == null) return;
            }

            var inst = ProjectileManager.Instance;
            if (inst == null) return;

            try
            {
                // 池未预热（本机玩家没发射过这种弹）时先建池
                if (!PoolContains(inst, name))
                {
                    inst.AddProjectileToPool(name);
                    if (!PoolContains(inst, name))
                    {
                        Plugin.Log.LogWarning("[RL2MP] [PROJ] 弹池缺少 '" + name + "'，跳过视觉复刻。");
                        return;
                    }
                }

                _spawningRemote = true;
                Projectile_RL p;
                // v3.0.6：抑制视觉弹自带的 spawn 特效。分身没有真身的 BaseCharacterController，
                // 弹体 OnSpawn 的 EffectTrigger 调 get_EffectVelocity 时 OwnerController 为 null
                // → NRE（真机日志：ResonantSpinKickProjectile 复刻失败）。这些特效由 SpellFx 桥
                // 和本模块负责，视觉弹自己不播，既修 NRE 又防双发。同步调用，try/finally 恢复。
                bool oldDisable = EffectTriggerAnimBehaviour.DISABLE_GLOBALLY;
                EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = true;
                try
                {
                    // useAbsPos=true 且 component=null：localPosition 直接落地（池对象无父级），
                    // 不触发 SnapToOwner 的挂接与翻转逻辑，终态由我们覆盖
                    p = ProjectileManager.FireProjectile(sourceGo, name, Vector2.zero, false, 0f, 1f,
                        true, true, true, true);
                }
                finally
                {
                    EffectTriggerAnimBehaviour.DISABLE_GLOBALLY = oldDisable;
                    _spawningRemote = false;
                }
                if (p == null) return;

                var t = p.transform;
                if (snap)
                {
                    t.SetParent(sourceGo.transform, false);
                    t.localPosition = new Vector3(snapLx, snapLy, snapLz);
                    t.localEulerAngles = new Vector3(0f, 0f, snapRotZ);
                }
                else
                {
                    t.position = new Vector3(px, py, pz);
                    t.localEulerAngles = new Vector3(0f, 0f, eulerZ);
                    var s = t.localScale;
                    t.localScale = new Vector3(scaleX, s.y, s.z);
                }

                p.Heading = new Vector2(headingX, headingY);
                p.Speed = speed;
                p.Acceleration = new Vector2(accX, accY);
                p.LifespanTimer = lifespan;
                p.SetCorgiVelocity(p.Heading * p.Speed);

                // 在物理结算前登记免伤（Spawn 已在 FireProjectile 内部完成，碰撞在下一物理步才判定）
                _visualOnly.Add(p.gameObject.GetInstanceID());
                // v2.7.0：记下 (ownerId, spawnId) -> 视觉弹，供 despawn 精确回收
                _visualBySpawnId[SpawnKey(ownerId, spawnId)] = p;
            }
            catch (Exception ex)
            {
                float now = Time.unscaledTime;
                if (now - _lastErrLog > 5f)
                {
                    _lastErrLog = now;
                    Plugin.Log.LogWarning("[RL2MP] [PROJ] 视觉弹复刻失败 '" + name + "': " + Plugin.PatchError(ex));
                }
            }
        }

        /// <summary>
        /// 收到发射端的"提前消失"通知：把那颗视觉弹也回收掉。
        /// 火墙/持续法术结束就是走这条路；只靠 LifespanTimer 会停在最后一帧。
        /// </summary>
        internal static void HandleRemoteDespawn(int ownerId, uint spawnId)
        {
            long key = SpawnKey(ownerId, spawnId);
            Projectile_RL p;
            if (!_visualBySpawnId.TryGetValue(key, out p)) return;
            _visualBySpawnId.Remove(key);
            _poseMem.Remove(key);
            if (p == null) return;
            try
            {
                _visualOnly.Remove(p.gameObject.GetInstanceID());
                p.FlagForDestruction();
            }
            catch
            {
                try { if (p.gameObject != null) p.gameObject.SetActive(false); } catch { }
            }
        }

        /// <summary>source 是否属于某只敌人（玩家攻击的弹走玩家分支）。</summary>
        private static EnemyController FindEnemyOwner(GameObject source)
        {
            if (source == null) return null;
            var root = source.transform.root;
            var all = EnemySync.GetEnemiesForPosition();
            if (all == null) return null;
            for (int i = 0; i < all.Length; i++)
            {
                var e = all[i];
                if (e == null || e.transform == null) continue;
                if (e.transform.root == root || e.gameObject == root) return e;
            }
            return null;
        }

        private static bool PoolContains(ProjectileManager inst, string name)
        {
            if (_poolDictField == null)
                _poolDictField = AccessTools.Field(typeof(ProjectileManager), "m_projectileDict");
            var dict = _poolDictField?.GetValue(inst) as Dictionary<string, GenericPool_RL<Projectile_RL>>;
            return dict != null && dict.ContainsKey(name);
        }

        private static void TryPatch(Harmony harmony, MethodInfo target, string prefixName,
            string label, string postfixName = null)
        {
            try
            {
                if (target == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [PROJ] 补丁目标为空: " + label);
                    return;
                }
                HarmonyMethod pre = null, post = null;
                if (!string.IsNullOrEmpty(prefixName))
                    pre = new HarmonyMethod(typeof(ProjectileSync).GetMethod(prefixName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                if (!string.IsNullOrEmpty(postfixName))
                    post = new HarmonyMethod(typeof(ProjectileSync).GetMethod(postfixName,
                        BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static));
                harmony.Patch(target, prefix: pre, postfix: post);
                Plugin.Log.LogInfo("[RL2MP] [PROJ] 补丁 OK: " + label);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [PROJ] " + label + " 补丁失败: " + Plugin.PatchError(ex));
            }
        }
    }
}
