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

            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            var w = new NetDataWriter();
            w.Put((byte)MsgType.ProjectileDespawn);
            w.Put(nm.LocalPlayerId);
            w.Put(spawnId);
            nm.SendProjectileDespawn(w);
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
                try
                {
                    // useAbsPos=true 且 component=null：localPosition 直接落地（池对象无父级），
                    // 不触发 SnapToOwner 的挂接与翻转逻辑，终态由我们覆盖
                    p = ProjectileManager.FireProjectile(sourceGo, name, Vector2.zero, false, 0f, 1f,
                        true, true, true, true);
                }
                finally
                {
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
