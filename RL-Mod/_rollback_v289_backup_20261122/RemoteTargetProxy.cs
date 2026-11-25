using UnityEngine;

namespace RL2Multiplayer
{
    /// <summary>
    /// v2.8.4：挂在远端玩家分身（RemotePlayer_x）root 上的最小控制器代理。
    ///
    /// 背景（真机 bug）：一方倒地后，EnemyAiFix 会把本地怪的 e.Target 指向存活方分身，
    /// 让权威端怪照着存活方位置出招（命中靠分身上的 RemoteHurtbox 转发给真身）。
    /// 但 EnemyController.Target 的 setter 会做 m_target.GetComponent&lt;BaseCharacterController&gt;()，
    /// 分身是被 StripClone 剥掉所有脚本的视觉克隆 —— 结果是 TargetController == null。
    /// 游戏里 40+ 个怪物 AI 脚本攻击前都无条件读 TargetController.Midpoint，空引用直接让
    /// 攻击协程在生成 hitbox 之前崩掉 —— 表现就是"怪不打存活方"。
    ///
    /// 本代理只提供一个"站在分身位置的有效 BaseCharacterController"：
    ///  - override Awake 为空，避免基类初始化去依赖没有的 Corgi 组件；
    ///  - IsInitialized 保持 false，Midpoint 基类实现会返回 transform.position（正是分身位置）；
    ///  - 三个 abstract 成员给最小实现，不参与任何真实伤害结算（分身受击走 RemoteHurtbox）。
    ///
    /// 注意：不要给它加 Collider/Rigidbody，也不要让它进入任何游戏系统；它纯粹是 TargetController 锚点。
    /// </summary>
    public sealed class RemoteTargetProxy : BaseCharacterController
    {
        public override float BaseScaleToOffsetWith { get { return 1f; } }

        public override int ActualMaxHealth { get { return 1; } }

        public override float CalculateDamageTaken(IDamageObj damageObj,
            out CriticalStrikeType critType, out float damageBlocked,
            float damageOverride = -1f, bool trueDamage = false, bool pureCalculation = true)
        {
            critType = CriticalStrikeType.None;
            damageBlocked = 0f;
            // 分身不吃原生伤害（命中走 RemoteHurtbox → 真身），这里只回一个正数让调用方不崩。
            return damageObj != null ? damageObj.ActualDamage : 1f;
        }

        protected override void Awake()
        {
            // 故意不调 base.Awake()：基类会去初始化 Corgi/状态机等，代理不需要。
            // IsInitialized 保持默认 false → Midpoint 返回 transform.position。
        }
    }
}
