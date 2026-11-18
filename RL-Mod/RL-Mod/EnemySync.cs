using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v1.7.9 协作模式：共享 BOSS / 怪物血量。
    ///
    /// 目标：两端各跑一份游戏、各有房间/怪物实例。A 打怪扣的血要让 B 端同一只怪也扣，
    ///       "一起打同一条血"。
    ///
    /// 方案（伤害转发，最小侵入，不碰游戏原生伤害结算）：
    ///   1) postfix 拦 EnemyHitResponse.CharacterDamaged（这是敌人被攻击扣血的唯一入口），
    ///      拿到真实伤害值 + 攻击者；只转发"本地玩家造成"的伤害。
    ///   2) 接收端按 key 找到本地同一只怪，用抑制标志调 SetHealth 扣同样的血；扣到 0 且
    ///      未死则补一次 KillCharacter，让本地掉落/主线进度照常触发。
    ///   3) 任一端怪死亡时广播 EnemyDied，对端同步击杀，防止血条停在 0 但怪不死。
    ///   4) 敌人 key = 场景名哈希 + EnemyIndex + EnemyType + EnemyRank。两端地图种子/房间
    ///      布局同步，同一只怪的 EnemyIndex 稳定一致。
    ///
    /// 只转发玩家攻击伤害：环境/状态伤害、各自 AI 误伤不共享，避免一端踩刺把对端怪磨死。
    /// 防循环：应用转发伤害/死亡时置 _applyingRemote，postfix 见到直接跳过，绝不回传。
    /// </summary>
    internal static class EnemySync
    {
        private static bool _applyingRemote;
        private static float _lastLogUnmatched;

        // ---------------- v1.8.0：权威血量快照对账 ----------------
        // 事件型伤害转发只覆盖"发出时在线且已匹配"的端。玩家暂停 / 晚进房 / 丢包时
        // 会错过事件，导致血量回退成满血、怪死后对端留存。这里让主机周期性广播
        // 当前所有敌人的权威血量(百分比)与已死 key，客机按 key 幂等校正。
        private static float _snapshotTimer;
        private const float SnapshotInterval = 0.4f;         // 快照周期（unscaled，暂停时也发）
        private const float HealthEpsilon = 1.5f;            // 小于此血差不校正（避免动画/取整抖动）

        // v1.8.2：FindObjectsOfType 是全场景扫描，很慢。原先每次伤害/每次收快照都扫一遍，
        // 高攻速时严重卡顿。缓存 0.25s：把扫描频率从"每次事件"降到约 4 次/秒。
        // 缓存项即使已死/被销毁，使用处都有 e==null || e.IsDead 兜底，安全。
        private static EnemyController[] _enemyCache;
        private static float _enemyCacheTime;
        private const float EnemyCacheLifetime = 0.25f;
        private static EnemyController[] GetEnemies()
        {
            float now = Time.unscaledTime;
            if (_enemyCache == null || now - _enemyCacheTime >= EnemyCacheLifetime)
            {
                _enemyCache = UnityEngine.Object.FindObjectsOfType<EnemyController>();
                _enemyCacheTime = now;
            }
            return _enemyCache;
        }
        private static float _lastSnapLog;
        // v1.8.1：墓碑永久保留（每层楼内）。强者杀光的怪，弱者无论多晚进房都保持死亡。
        // 只在换场景（换楼层/回城）时由 ResetAll 清空——那时是全新一层，旧墓碑不再适用。
        // key 已含房间坐标，不会误杀其他房间的同序号怪。
        private static readonly HashSet<int> _tombstones = new HashSet<int>();
        // 客机侧：已经确认同步过血量的 key，用于日志降噪。
        private static float _lastApplyLog;

        internal static bool Active
        {
            get { return Score.Style == PlayStyle.CoOp && DownedState.InSession
                          && NetworkManager.Instance != null && NetworkManager.Instance.Connected; }
        }

        /// <summary>
        /// 主机每 0.4s 广播一次当前所有敌人的权威血量快照。
        /// 用 unscaledDeltaTime 驱动：游戏暂停(Time.timeScale=0)时依然对账，
        /// 这正是修复"主机一暂停数据就不收发"的关键——暂停期间照常发包。
        /// </summary>
        internal static void HostTick()
        {
            if (!Active || NetworkManager.Instance == null || !NetworkManager.Instance.IsHost) return;

            _snapshotTimer += Time.unscaledDeltaTime;
            if (_snapshotTimer < SnapshotInterval) return;
            _snapshotTimer = 0f;

            try
            {
                var w = new NetDataWriter();
                w.Put((byte)MsgType.EnemyStateSnapshot);

                var all = GetEnemies();
                int count = 0;
                // 第一遍：数活着的（用于写 count）
                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];
                    if (e != null && !e.IsDead) count++;
                }
                w.Put(count);
                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];
                    if (e == null || e.IsDead) continue;
                    int key = BuildKey(e);
                    int maxHp = e.ActualMaxHealth;
                    float pct = maxHp > 0 ? Mathf.Clamp01(e.CurrentHealth / maxHp) : 1f;
                    w.Put(key);
                    w.Put(pct);
                }

                // 第二遍：永久墓碑 key（v1.8.1：不再过期），让无论多晚进房的端都补杀。
                // 先写数量再写 key（接收端按此顺序解析）；2048 仅防御畸形增长。
                int tombCount = _tombstones.Count;
                if (tombCount > 2048) tombCount = 2048;
                w.Put(tombCount);
                int written = 0;
                foreach (var key in _tombstones)
                {
                    if (written >= tombCount) break;
                    w.Put(key);
                    written++;
                }

                NetworkManager.Instance.SendEnemySnapshot(w);

                if (Time.unscaledTime - _lastSnapLog > 5f)
                {
                    _lastSnapLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [ENEMY] 广播血量快照 enemies=" + count + " tombstones=" + tombCount);
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ENEMY] snapshot broadcast failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>
        /// 客机收到主机权威快照：按 key 幂等校正本地同一只怪的血量，并按墓碑补杀。
        /// 只把本地血量"拉低/对齐"到权威值——不反向抬高（抬高会覆盖本地刚打出的伤害，
        /// 因为伤害事件仍在并行转发，两边最终会收敛）。
        /// </summary>
        internal static void ApplySnapshot(NetDataReader reader)
        {
            try
            {
                int count = reader.GetInt();
                if (count < 0 || count > 512) return; // 防御畸形包
                var authoritative = new Dictionary<int, float>(count);
                for (int i = 0; i < count; i++)
                {
                    int key = reader.GetInt();
                    float pct = reader.GetFloat();
                    authoritative[key] = pct;
                }
                int tombCount = reader.GetInt();
                if (tombCount < 0 || tombCount > 2048) tombCount = 0;
                var tombs = new HashSet<int>();
                for (int i = 0; i < tombCount; i++) tombs.Add(reader.GetInt());

                var all = GetEnemies();
                _applyingRemote = true;
                try
                {
                    for (int i = 0; i < all.Length; i++)
                    {
                        var e = all[i];
                        if (e == null || e.IsDead) continue;
                        int key = BuildKey(e);

                        // 墓碑命中：主机端这只已经死了，本地补杀
                        if (tombs.Contains(key))
                        {
                            KillLocal(e, key, "快照墓碑补杀");
                            continue;
                        }

                        float pct;
                        if (!authoritative.TryGetValue(key, out pct)) continue;
                        int maxHp = e.ActualMaxHealth;
                        float target = pct * maxHp;
                        float delta = e.CurrentHealth - target;
                        // 只向下对齐：本地血量明显高于权威值才校正（漏了事件的满血怪）。
                        // 本地低于权威值说明本地刚打出伤害，交给伤害事件转发去收敛，别抬高。
                        if (delta > HealthEpsilon)
                        {
                            e.SetHealth(target, false, true);
                            if (Time.unscaledTime - _lastApplyLog > 3f)
                            {
                                _lastApplyLog = Time.unscaledTime;
                                Plugin.Log.LogInfo("[RL2MP] [ENEMY] 快照校正血量 key=" + key
                                    + " " + e.CurrentHealth.ToString("F0") + " -> " + target.ToString("F0"));
                            }
                        }
                    }
                }
                finally
                {
                    _applyingRemote = false;
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ENEMY] apply snapshot failed: " + Plugin.PatchError(ex));
            }
        }

        internal static void Apply(Harmony harmony)
        {
            try
            {
                // 敌人被攻击扣血的唯一入口。postfix 能拿到真实伤害(__result)和攻击者。
                var m = AccessTools.Method(typeof(EnemyHitResponse), "CharacterDamaged");
                if (m == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [ENEMY] EnemyHitResponse.CharacterDamaged not found; shared HP disabled.");
                    return;
                }
                var post = new HarmonyMethod(typeof(EnemySync).GetMethod("CharacterDamagedPostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(m, postfix: post);

                var km = AccessTools.Method(typeof(EnemyController), "KillCharacter",
                    new Type[] { typeof(GameObject), typeof(bool) });
                if (km != null)
                {
                    var killPre = new HarmonyMethod(typeof(EnemySync).GetMethod("KillCharacterPrefix",
                        BindingFlags.Static | BindingFlags.NonPublic));
                    harmony.Patch(km, prefix: killPre);
                }

                Plugin.Log.LogInfo("[RL2MP] [ENEMY] Patched EnemyHitResponse.CharacterDamaged + EnemyController.KillCharacter (shared HP).");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ENEMY] patch failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>敌人被攻击扣血后：只把"本地玩家造成"的伤害转发给对端。</summary>
        private static void CharacterDamagedPostfix(EnemyHitResponse __instance, IDamageObj damageObj,
            GameObject otherRootObj, ref float __result)
        {
            if (_applyingRemote || !Active) return;
            float dmg = __result;
            if (dmg <= 0f) return;

            try
            {
                var enemy = Traverse.Create(__instance).Field("m_enemyController").GetValue<EnemyController>();
                if (enemy == null || enemy.IsDead) return;

                // 只转发本地玩家发起的攻击（近战/投射物/召唤物）；敌人互相攻击、环境伤害不管
                if (!IsFromLocalPlayer(otherRootObj, damageObj)) return;

                int key = BuildKey(enemy);
                var nm = NetworkManager.Instance;
                if (nm == null) return;

                var w = new NetDataWriter();
                w.Put((byte)MsgType.EnemyDamage);
                w.Put(nm.LocalPlayerId);   // 发送者：接收端据此跳过自己，主机据此中转
                w.Put(key);
                w.Put(dmg);
                nm.SendEnemyDamage(w);
                Plugin.Log.LogInfo("[RL2MP] [ENEMY] 转发伤害 key=" + key + " dmg=" + dmg.ToString("F1")
                    + " (" + enemy.EnemyType + "/" + enemy.EnemyRank + " idx=" + enemy.EnemyIndex + ")");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ENEMY] forward failed: " + Plugin.PatchError(ex));
            }
        }

        /// <summary>攻击者是否是本地玩家本体/子物体，或其发射的投射物。</summary>
        private static bool IsFromLocalPlayer(GameObject attacker, IDamageObj damageObj)
        {
            var ps = PlayerSync.Instance;
            GameObject local = ps != null ? ps.CachedLocalPlayerObject : null;
            if (local == null) return false;

            if (attacker != null)
            {
                if (attacker == local || attacker.transform.root == local.transform.root) return true;
                var proj = attacker.GetComponent<Projectile_RL>();
                if (proj != null && proj.Owner != null && proj.Owner == local) return true;
            }
            if (damageObj != null)
            {
                var dgo = damageObj.gameObject;
                if (dgo != null)
                {
                    if (dgo == local || dgo.transform.root == local.transform.root) return true;
                    var proj2 = dgo.GetComponent<Projectile_RL>();
                    if (proj2 != null && proj2.Owner != null && proj2.Owner == local) return true;
                }
            }
            return false;
        }

        /// <summary>收到对端转发的伤害：本地同一只怪扣血，必要时补死亡。</summary>
        internal static void ApplyRemoteDamage(int key, float dmg)
        {
            if (!Active || dmg <= 0f) return;
            EnemyController target = FindByKey(key);
            if (target == null)
            {
                if (Time.unscaledTime - _lastLogUnmatched > 3f)
                {
                    _lastLogUnmatched = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [ENEMY] 收到伤害 key=" + key + " 但本地未找到对应怪（不同步/已死）。");
                }
                return;
            }

            bool wasDead = target.IsDead;
            _applyingRemote = true;
            try
            {
                target.SetHealth(0f - dmg, true, true); // additive 扣血；runEvents 让血条更新
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ENEMY] apply damage failed: " + ex.Message);
            }
            finally
            {
                _applyingRemote = false;
            }

            // SetHealth 不触发 KillCharacter（那是 HitResponse 在血量<=0 时做的），本地补一次。
            if (!wasDead && !target.IsDead && target.CurrentHealth <= 0f)
                KillLocal(target, key, "共享伤害击杀");
        }

        /// <summary>本地怪死亡 prefix：广播 EnemyDied，防止对端血条停在 0 但怪不死。</summary>
        private static void KillCharacterPrefix(EnemyController __instance)
        {
            if (_applyingRemote || !Active || __instance == null) return;
            try
            {
                int key = BuildKey(__instance);
                // v1.8.0：主机记下墓碑，随快照重发 8 秒，覆盖晚进房端的补杀窗口。
                var nm = NetworkManager.Instance;
                if (nm != null && nm.IsHost) _tombstones.Add(key);
                if (nm == null) return;
                var w = new NetDataWriter();
                w.Put((byte)MsgType.EnemyDied);
                w.Put(nm.LocalPlayerId);
                w.Put(key);
                nm.SendEnemyDamage(w);
            }
            catch { }
        }

        /// <summary>对端怪已死：本地对应怪若还活着，补一次死亡。</summary>
        internal static void ApplyRemoteDeath(int key)
        {
            if (!Active) return;
            EnemyController target = FindByKey(key);
            if (target == null || target.IsDead) return;
            KillLocal(target, key, "同步对端击杀");
        }

        private static void KillLocal(EnemyController target, int key, string reason)
        {
            var nm0 = NetworkManager.Instance;
            if (nm0 != null && nm0.IsHost) _tombstones.Add(key);
            _applyingRemote = true;
            try
            {
                var local = PlayerSync.Instance != null ? PlayerSync.Instance.CachedLocalPlayerObject : null;
                target.KillCharacter(local, true);
                Plugin.Log.LogInfo("[RL2MP] [ENEMY] " + reason + " key=" + key);
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [ENEMY] kill failed: " + ex.Message);
            }
            finally
            {
                _applyingRemote = false;
            }
        }

        private static int BuildKey(EnemyController e)
        {
            unchecked
            {
                int h = 17;
                string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
                h = h * 31 + (scene != null ? scene.GetHashCode() : 0);

                // v1.8.2：回退 v1.8.1 的房间坐标（Room.Coordinates）。实测两端用运行时
                // 世界坐标 *10 取整算出的 key 不一致（日志：收到伤害 key=... 未找到对应怪），
                // 导致墓碑命不中、伤害转发也失效。v1.8.0 仅用下面四项时用户确认血量同步正常，
                // 所以恢复它。代价：同一场景不同房间的同序号同类怪可能撞 key（永久墓碑有误杀
                // 风险），但实战两端同步推进、房间序列一致，概率很低——稳定优先。
                h = h * 31 + e.EnemyIndex;
                h = h * 31 + (int)e.EnemyType;
                h = h * 31 + (int)e.EnemyRank;
                return h;
            }
        }

        private static EnemyController FindByKey(int key)
        {
            try
            {
                var all = GetEnemies();
                for (int i = 0; i < all.Length; i++)
                {
                    var e = all[i];
                    if (e == null || e.IsDead) continue;
                    if (BuildKey(e) == key) return e;
                }
            }
            catch { }
            return null;
        }

        internal static void ResetAll()
        {
            _applyingRemote = false;
            _snapshotTimer = 0f;
            _tombstones.Clear();
            _enemyCache = null;   // v1.8.2：换场景时丢弃旧敌人引用
            _enemyCacheTime = 0f;
        }
    }
}
