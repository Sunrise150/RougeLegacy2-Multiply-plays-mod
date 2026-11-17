using System.Collections;

public class SpearKnight_Miniboss_AIScript : SpearKnight_Basic_AIScript
{
	protected float m_uppercutBullet_AttackIntro_AnimationSpeed = 1f;

	protected float m_uppercutBullet_AttackIntro_Delay = 0.5f;

	protected float m_uppercutBullet_AttackHold_AnimationSpeed = 1f;

	protected float m_uppercutBullet_AttackHold_Delay;

	protected float m_uppercutBullet_Exit_AnimationSpeed = 0.65f;

	protected float m_uppercutBullet_Exit_Delay;

	protected float m_uppercutBullet_Exit_ForceIdle = 0.15f;

	protected float m_uppercutBullet_AttackCD;

	protected float UppercutBullet_JumpPower = 31f;

	protected float UppercutBullet_JumpSpeed = 17f;

	protected float UppercutBullet_ProjectileAmount = 4f;

	protected float UppercutBullet_ProjectileAngle = 90f;

	protected float UppercutBullet_JumpToMaxHeightDuration = 1f;

	protected override float Dash_AttackSpeed => 32f;

	protected override float Dash_AttackDuration => 0.65f;

	protected override int m_knockbackDefenseBoostOverride => 7;

	protected override float DashUppercut_JumpSpeed => 10f;

	protected override bool m_throw_Attack_TargetPlayer => true;

	protected override int m_throw_Attack_ProjectileAmount => 2;

	protected override float m_throw_Attack_ProjectileDelay => 0.125f;

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public override IEnumerator UpperCutBullets()
	{
		StopAndFaceTarget();
		float jumpSpeed = UppercutBullet_JumpSpeed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			jumpSpeed = 0f - UppercutBullet_JumpSpeed;
		}
		base.EnemyController.LockFlip = true;
		yield return Default_Animation("Combo_Uppercut_Tell", m_uppercutBullet_AttackIntro_AnimationSpeed, m_uppercutBullet_AttackIntro_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Combo_Uppercut_Hold", m_uppercutBullet_AttackHold_AnimationSpeed, m_uppercutBullet_AttackHold_Delay, waitUntilAnimationComplete: false);
		SetVelocity(jumpSpeed, UppercutBullet_JumpPower, additive: false);
		for (int i = 0; (float)i < UppercutBullet_ProjectileAmount; i++)
		{
			this.FireProjectile("SpearKnightBoltMinibossProjectile", 0, matchFacing: true, UppercutBullet_ProjectileAngle);
			yield return Wait(UppercutBullet_JumpToMaxHeightDuration / UppercutBullet_ProjectileAmount);
		}
		yield return WaitUntilIsGrounded();
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Combo_Exit", m_uppercutBullet_Exit_AnimationSpeed, m_uppercutBullet_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_uppercutBullet_Exit_ForceIdle, m_uppercutBullet_AttackCD);
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
	}
}
