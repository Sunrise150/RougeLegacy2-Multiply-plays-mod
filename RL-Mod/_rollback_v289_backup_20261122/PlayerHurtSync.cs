using System;
using System.Reflection;
using HarmonyLib;
using LiteNetLib.Utils;
using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// 远端分身的"伤害对象"占位实现。
    ///
    /// 为什么需要它：真身端的扣血入口是 PlayerHitResponse.StartHitResponse(otherRoot, damageObj, ...)，
    /// 里面必须有一个非空的 IDamageObj 才会走到 CharacterDamaged（扣血/暴击/击退/死亡全在里面）。
    /// 网络包不可能把对方的 Projectile_RL / EnemyController 传过来，所以这里造一个"数值壳"：
    /// BaseDamage/ActualDamage 就是发送端算出来的伤害，其余按普通物理攻击填。
    /// 这样受击方的原生流程（闪白、伤害数字、屏幕震动、无敌帧、死亡判定）全部照常触发。
    /// </summary>
    public class RemoteDamageObj : MonoBehaviour, IDamageObj
    {
        private float _damage;
        private float _knockback = 1f;
        private float _stun;
        private StrikeType _strike = StrikeType.Sharp;

        public void Setup(float damage, float knockback, float stun, StrikeType strike)
        {
            _damage = damage;
            _knockback = knockback;
            _stun = stun;
            _strike = strike;
        }

        public float BaseDamage { get { return _damage; } }
        public float ActualDamage { get { return _damage; } }
        public float ActualCritChance { get { return 0f; } }
        public float ActualCritDamage { get { return 0f; } }
        public Vector2 ExternalKnockbackMod { get { return Vector2.one; } }
        public float BaseKnockbackStrength { get; set; }
        public float ActualKnockbackStrength { get { return _knockback; } }
        public float BaseStunStrength { get; set; }
        public float ActualStunStrength { get { return _stun; } }
        public StrikeType StrikeType { get { return _strike; } }
        public bool IsDotDamage { get { return false; } }
        public StatusEffectType[] StatusEffectTypes { get { return null; } }
        public float[] StatusEffectDurations { get { return null; } }
    }

    /// <summary>
    /// 协作模式：分身受到的伤害转发给"分身背后那个真身"。
    ///
    /// 背景（用户反馈）：两端各跑一份游戏，对面的玩家在本端只是一个外观克隆（RemotePlayer_x），
    /// 没有 PlayerController、没有受击判定。于是本端画面里怪物子弹明明穿过对方身体，
    /// 对方却毫无反应——看起来就是"打不到人"。
    ///
    /// 现在的链路：
    ///   1) RemoteHurtbox（挂在分身上的受击盒）被敌方 hitbox/弹丸碰到 →
    ///      拿到对方 IDamageObj 的实际伤害 → 本地先播受击特效（立刻有反馈）。
    ///   2) 把 (目标玩家 id, 伤害, 眩晕, 伤害源坐标) 发给对端。
    ///   3) 对端确认"目标就是我" → 用 RemoteDamageObj 走原生 PlayerHitResponse.StartHitResponse
    ///      扣血、击退、闪白、播伤害数字、死亡判定 —— 死亡后 Downed/复活流程照常工作。
    ///
    /// 防双倍：两端都有自己的怪物副本，同一次攻击可能在两端都命中（本地真身被本地怪打 +
    /// 对端转发过来）。这里用"本地优先"去重——真身端最近 LocalPriorityWindow 内已经被本地
    /// 伤害结算过，就直接忽略网络来的那一条，避免协作模式下血量掉得莫名其妙快。
    /// </summary>
    internal static class PlayerHurtSync
    {
        internal static bool Active
        {
            get
            {
                return Score.Style == PlayStyle.CoOp && DownedState.InSession
                       && NetworkManager.Instance != null && NetworkManager.Instance.Connected;
            }
        }

        /// <summary>本地刚受过伤的时间窗：窗内的网络伤害被丢弃（本地结算优先）。</summary>
        private const float LocalPriorityWindow = 0.18f;
        /// <summary>两条网络伤害之间的最小间隔（3 人局两端都转发同一次命中时去重）。</summary>
        private const float NetDedupWindow = 0.18f;

        private static float _lastLocalHurtAt = -10f;
        private static float _lastNetHurtAt = -10f;
        private static RemoteDamageObj _dummy;
        private static float _lastLog;

        private static void Log(string msg, bool warn = false)
        {
            if (Time.unscaledTime - _lastLog < 1f) return;
            _lastLog = Time.unscaledTime;
            if (warn) Plugin.Log.LogWarning("[RL2MP] [HURT] " + msg);
            else Plugin.Log.LogInfo("[RL2MP] [HURT] " + msg);
        }

        // ---------------- 发送端 ----------------

        /// <summary>分身被击中：把伤害转发给分身背后的真身。</summary>
        internal static void SendHurt(int targetId, float damage, float stun, Vector2 sourcePos)
        {
            var nm = NetworkManager.Instance;
            if (nm == null || !nm.Connected) return;
            if (targetId == nm.LocalPlayerId) return;    // 自己打自己（不该发生）
            if (damage <= 0f) return;

            try
            {
                var w = new NetDataWriter();
                w.Put((byte)MsgType.PlayerHurt);
                w.Put(nm.LocalPlayerId);
                w.Put(targetId);
                w.Put(damage);
                w.Put(stun);
                w.Put(sourcePos.x);
                w.Put(sourcePos.y);
                nm.SendPlayerHurt(w);

                if (Time.unscaledTime - _lastLog > 1f)
                {
                    _lastLog = Time.unscaledTime;
                    Plugin.Log.LogInfo("[RL2MP] [HURT] 分身被击中 -> 转发给 P" + targetId
                        + " dmg=" + damage.ToString("F1"));
                }
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [HURT] SendHurt 异常: " + Plugin.PatchError(ex));
            }
        }

        // ---------------- 接收端 ----------------

        /// <summary>收到"你的角色被打了"：在本端真身上复现这次伤害。</summary>
        internal static void OnReceive(int senderId, int targetId, float damage, float stun, float srcX, float srcY)
        {
            var nm = NetworkManager.Instance;
            if (nm == null) return;
            if (targetId != nm.LocalPlayerId) return;   // 不是打我
            if (!Active) return;
            if (damage <= 0f) return;

            float now = Time.unscaledTime;
            if (now - _lastNetHurtAt < NetDedupWindow) return;

            var ps = PlayerSync.Instance;
            var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
            if (pc == null || pc.IsDead || DownedState.LocalDown) return;

            // 本地优先：真身刚被本端怪物打过 / 处于无敌帧，就不要再叠一次
            if (now - _lastLocalHurtAt < LocalPriorityWindow)
            {
                Log("忽略网络伤害（本地刚结算过）dmg=" + damage.ToString("F1"));
                return;
            }

            var hitResponse = pc.GetComponent<PlayerHitResponse>();
            if (hitResponse == null)
            {
                Log("真身没有 PlayerHitResponse，无法应用伤害。", true);
                return;
            }
            // 只跳过真正的无敌帧。不能用 IsInvincible：它在"Game 输入映射被禁用"（窗口失焦/
            // 暂停菜单）时也会返回 true，而单机双开测试时非焦点窗口恰好就是这种状态，
            // 会把所有转发伤害全部拦掉——这正是"子弹打上去没伤害"的另一半原因。
            if (hitResponse.InvincibleTimer > 0.001f) return;

            _lastNetHurtAt = now;
            try
            {
                var dummy = EnsureDummy();
                dummy.Setup(damage, 1f, stun, StrikeType.Sharp);
                // 伤害源坐标：让原生击退方向（比 x 大小）和受击特效位置都朝正确方向
                dummy.transform.position = new Vector3(srcX, srcY, pc.transform.position.z);

                hitResponse.StartHitResponse(dummy.gameObject, dummy, -1f, false, true);
                _ignoreWatchUntil = now + 0.35f;   // 这次掉血是我们自己造成的，别计入"本地优先"
                Plugin.Log.LogInfo("[RL2MP] [HURT] 应用网络伤害 dmg=" + damage.ToString("F1")
                    + " 来自 P" + senderId + " 剩余血量=" + pc.CurrentHealth.ToString("F0"));
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [HURT] 应用伤害异常: " + Plugin.PatchError(ex));
            }
        }

        private static RemoteDamageObj EnsureDummy()
        {
            if (_dummy != null) return _dummy;
            var go = new GameObject("RL2MP_RemoteDamage");
            UnityEngine.Object.DontDestroyOnLoad(go);
            _dummy = go.AddComponent<RemoteDamageObj>();
            return _dummy;
        }

        // ---------------- 本地受伤兜底监控 ----------------
        // Harmony 补丁没打上（版本差异/反射失败）时，靠"血量掉了"也能识别出本地刚挨过打，
        // 保证"本地优先"去重始终有效，不会在协作模式下出现双倍伤害。
        private static float _lastHealth = -1f;
        private static float _ignoreWatchUntil;

        internal static void Tick()
        {
            var ps = PlayerSync.Instance;
            var pc = ps != null ? ps.LocalPlayer as PlayerController : null;
            if (pc == null) { _lastHealth = -1f; return; }

            float hp = pc.CurrentHealth;
            if (_lastHealth >= 0f && hp < _lastHealth - 0.01f)
            {
                // 我们自己应用网络伤害导致的掉血不算（否则会把后续真正该吃的伤害也一并抑制）
                if (Time.unscaledTime > _ignoreWatchUntil) _lastLocalHurtAt = Time.unscaledTime;
            }
            _lastHealth = hp;
        }

        // ---------------- 本地受伤记录（去重用） ----------------

        /// <summary>
        /// PlayerHitResponse.StartHitResponse 的 postfix：记下"本地确实挨了一下"的时刻。
        /// 只记录本地来源（网络转发用的 RemoteDamageObj 不算），供 OnReceive 做本地优先去重。
        /// </summary>
        private static void StartHitResponsePostfix(IDamageObj damageObj)
        {
            if (damageObj is RemoteDamageObj) return;
            _lastLocalHurtAt = Time.unscaledTime;
        }

        internal static void Reset(string reason)
        {
            _lastLocalHurtAt = -10f;
            _lastNetHurtAt = -10f;
        }

        internal static void Apply(Harmony harmony)
        {
            try
            {
                var m = AccessTools.Method(typeof(PlayerHitResponse), "StartHitResponse",
                    new Type[] { typeof(GameObject), typeof(IDamageObj), typeof(float), typeof(bool), typeof(bool) });
                if (m == null)
                {
                    Plugin.Log.LogWarning("[RL2MP] [HURT] PlayerHitResponse.StartHitResponse 未找到，本地优先去重不可用。");
                    return;
                }
                var post = new HarmonyMethod(typeof(PlayerHurtSync).GetMethod("StartHitResponsePostfix",
                    BindingFlags.Static | BindingFlags.NonPublic));
                harmony.Patch(m, postfix: post);
                Plugin.Log.LogInfo("[RL2MP] [HURT] 补丁 OK: PlayerHitResponse.StartHitResponse postfix（本地受伤时间戳）。");
            }
            catch (Exception ex)
            {
                Plugin.Log.LogWarning("[RL2MP] [HURT] 补丁失败: " + Plugin.PatchError(ex));
            }
        }
    }
}
