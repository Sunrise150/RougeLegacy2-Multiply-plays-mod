using System.Collections;
using UnityEngine;

public class FlyingAxe_Basic_AIScript : BaseAIScript, IBodyOnEnterHitResponse, IHitResponse, IBodyOnStayHitResponse
{
	protected float m_sideSpin_TellIntro_AnimationSpeed = 1f;

	protected float m_sideSpin_TellHold_AnimationSpeed = 1f;

	protected float m_sideSpin_TellIntroAndHold_Duration = 1f;

	protected float m_sideSpin_AttackIntro_AnimationSpeed = 1f;

	protected float m_sideSpin_AttackIntro_Delay;

	protected const float m_sideSpin_AttackHold_AnimationSpeed = 1f;

	protected const float m_sideSpin_AttackHold_Delay = 0f;

	protected float m_sideSpin_ExitIntro_AnimationSpeed = 1f;

	protected const float m_sideSpin_ExitIntro_EndDelay = 0.15f;

	protected const float m_sideSpin_Exit_ForceIdle = 0.15f;

	protected float m_vertSpin_TellIntro_AnimationSpeed = 1f;

	protected float m_vertSpin_TellHold_AnimationSpeed = 1f;

	protected float m_vertSpin_TellIntroAndHold_Delay = 1.2f;

	protected float m_vertSpin_AttackIntro_AnimationSpeed = 1f;

	protected float m_vertSpin_AttackIntro_Delay;

	protected const float m_vertSpin_AttackHold_AnimationSpeed = 1f;

	protected const float m_vertSpin_AttackHold_Delay = 0f;

	protected float m_vertSpin_ExitIntro_AnimationSpeed = 1f;

	protected const float m_vertSpin_ExitIntro_EndDelay = 0.15f;

	protected const float m_vertSpin_Exit_ForceIdle = 0.25f;

	protected float m_vertSpin_Exit_AttackCD = 5f;

	private Projectile_RL m_flameThrowerProjectile1;

	private Projectile_RL m_flameThrowerProjectile2;

	private Projectile_RL m_flameThrowerProjectile3;

	private Projectile_RL m_flameThrowerProjectile4;

	private const string FLAME_TELL_PROJECTILE = "WallTurretFlameThrowerWarningProjectile_Template";

	private const string FIREBALL_PROJECTILE = "FlyingAxeFireballProjectile";

	protected Projectile_RL m_flameTellProjectile1;

	protected Projectile_RL m_flameTellProjectile2;

	protected Projectile_RL m_flameTellProjectile3;

	protected Projectile_RL m_flameTellProjectile4;

	protected override Vector2 IdleDuration => new Vector2(0.3f, 0.7f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.8f, 1.1f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.8f, 1.1f);

	protected virtual float m_sideSpin_Attack_Duration => 5.5f;

	protected virtual float m_sideSpin_Attack_TurnSpeed => 85f;

	protected virtual float m_sideSpin_Attack_MovementSpeed => 9f;

	protected virtual float m_sideSpin_Exit_AttackCD => 0f;

	protected virtual Vector2 m_sideSpin_InternalKnockBackMod => new Vector2(1.75f, 1.75f);

	protected virtual float m_sideSpin_FlightRecoveryMod => 2f;

	protected virtual float m_vertSpin_Attack_ChaseDuration => 2.5f;

	protected virtual float m_vertSpin_Attack_TurnSpeed => 0f;

	protected virtual float m_vertSpin_Attack_MovementSpeed => 0f;

	protected virtual bool m_vertSpin_Attack_FireBulletsWhileSpinning => false;

	protected virtual int m_vertSpin_Attack_BulletShotPerLoop => 5;

	protected virtual int m_vertSpin_Attack_TotalLoops => 10;

	protected virtual Vector2 m_vertSpin_Attack_FireballSpeedMod => new Vector2(0.5f, 1f);

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "FlyingAxeBounceBoltProjectile", "FlyingAxeFlameThrowerProjectile", "WallTurretFlameThrowerWarningProjectile_Template", "FlyingAxeFireballProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Side_Spin_Attack()
	{
		base.EnemyController.FollowTarget = true;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		yield return Default_TellIntroAndLoop("SideSpin_Tell_Intro", m_sideSpin_TellIntro_AnimationSpeed, "SideSpin_Tell_Hold", m_sideSpin_TellHold_AnimationSpeed, m_sideSpin_TellIntroAndHold_Duration);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("SideSpin_Attack_Intro", m_sideSpin_AttackIntro_AnimationSpeed, m_sideSpin_AttackIntro_Delay);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_sideSpin_Attack_MovementSpeed;
		base.EnemyController.BaseTurnSpeed = m_sideSpin_Attack_TurnSpeed;
		base.EnemyController.InternalKnockbackMod = m_sideSpin_InternalKnockBackMod;
		base.EnemyController.FlightKnockbackDecelerationMod = m_sideSpin_FlightRecoveryMod;
		yield return Default_Animation("SideSpin_Attack_Hold", 1f, 0f);
		if (m_sideSpin_Attack_Duration > 0f)
		{
			yield return Wait(m_sideSpin_Attack_Duration);
		}
		base.EnemyController.ResetBaseValues();
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SideSpin_Exit", m_sideSpin_ExitIntro_AnimationSpeed, 0.15f);
		base.EnemyController.ResetTurnTrigger();
		yield return Default_Attack_Cooldown(0.15f, m_sideSpin_Exit_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator Vertical_Spin_Attack()
	{
		base.EnemyController.AlwaysFacing = false;
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		base.EnemyController.FollowOffset = Vector3.zero;
		m_flameTellProjectile1 = this.FireProjectile("WallTurretFlameThrowerWarningProjectile_Template", new Vector2(1f, 0f), matchFacing: false);
		m_flameTellProjectile2 = this.FireProjectile("WallTurretFlameThrowerWarningProjectile_Template", new Vector2(0f, 1f), matchFacing: false, 90f);
		m_flameTellProjectile3 = this.FireProjectile("WallTurretFlameThrowerWarningProjectile_Template", new Vector2(-1f, 0f), matchFacing: false, 180f);
		m_flameTellProjectile4 = this.FireProjectile("WallTurretFlameThrowerWarningProjectile_Template", new Vector2(0f, -1f), matchFacing: false, 270f);
		yield return Default_TellIntroAndLoop("VerticalSpin_Tell_Intro", m_vertSpin_TellIntro_AnimationSpeed, "VerticalSpin_Tell_Hold", m_vertSpin_TellHold_AnimationSpeed, m_vertSpin_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		StopProjectile(ref m_flameTellProjectile1);
		StopProjectile(ref m_flameTellProjectile2);
		StopProjectile(ref m_flameTellProjectile3);
		StopProjectile(ref m_flameTellProjectile4);
		yield return Default_Animation("VerticalSpin_Attack_Intro", m_vertSpin_AttackIntro_AnimationSpeed, m_vertSpin_AttackIntro_Delay);
		base.EnemyController.FlyingMovementType = FlyingMovementType.Towards;
		base.EnemyController.BaseSpeed = m_vertSpin_Attack_MovementSpeed;
		base.EnemyController.BaseTurnSpeed = m_vertSpin_Attack_TurnSpeed;
		yield return Default_Animation("VerticalSpin_Attack_Hold", 1f, 0f, waitUntilAnimationComplete: false);
		m_flameThrowerProjectile1 = this.FireProjectile("FlyingAxeFlameThrowerProjectile", new Vector2(1f, 0f), matchFacing: false);
		m_flameThrowerProjectile2 = this.FireProjectile("FlyingAxeFlameThrowerProjectile", new Vector2(0f, 1f), matchFacing: false, 90f);
		m_flameThrowerProjectile3 = this.FireProjectile("FlyingAxeFlameThrowerProjectile", new Vector2(-1f, 0f), matchFacing: false, 180f);
		m_flameThrowerProjectile4 = this.FireProjectile("FlyingAxeFlameThrowerProjectile", new Vector2(0f, -1f), matchFacing: false, 270f);
		if (m_vertSpin_Attack_ChaseDuration > 0f)
		{
			if (m_vertSpin_Attack_FireBulletsWhileSpinning)
			{
				int angleDivider = 360 / m_vertSpin_Attack_BulletShotPerLoop;
				for (int i = 0; i < m_vertSpin_Attack_TotalLoops; i++)
				{
					for (int j = 0; j < m_vertSpin_Attack_BulletShotPerLoop; j++)
					{
						int num = Random.Range(angleDivider * j, angleDivider * (j + 1));
						float speedMod = Random.Range(m_vertSpin_Attack_FireballSpeedMod.x, m_vertSpin_Attack_FireballSpeedMod.y);
						this.FireProjectile("FlyingAxeFireballProjectile", new Vector2(0f, 0f), matchFacing: false, num, speedMod);
					}
					yield return Wait(m_vertSpin_Attack_ChaseDuration / (float)m_vertSpin_Attack_TotalLoops);
				}
			}
			else
			{
				yield return Wait(m_vertSpin_Attack_ChaseDuration);
			}
		}
		base.EnemyController.ResetBaseValues();
		base.EnemyController.FlyingMovementType = FlyingMovementType.Stop;
		StopProjectile(ref m_flameThrowerProjectile1);
		StopProjectile(ref m_flameThrowerProjectile2);
		StopProjectile(ref m_flameThrowerProjectile3);
		StopProjectile(ref m_flameThrowerProjectile4);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("VerticalSpin_Exit", m_vertSpin_ExitIntro_AnimationSpeed, 0.15f);
		base.EnemyController.ResetTurnTrigger();
		yield return Default_Attack_Cooldown(0.25f, m_vertSpin_Exit_AttackCD);
		base.EnemyController.AlwaysFacing = true;
	}

	public void BodyOnEnterHitResponse(IHitboxController otherHBController)
	{
	}

	public void BodyOnStayHitResponse(IHitboxController otherHBController)
	{
		BodyOnEnterHitResponse(otherHBController);
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		base.EnemyController.DisableDistanceThresholdCheck = true;
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		StopProjectile(ref m_flameTellProjectile1);
		StopProjectile(ref m_flameTellProjectile2);
		StopProjectile(ref m_flameTellProjectile3);
		StopProjectile(ref m_flameTellProjectile4);
		StopProjectile(ref m_flameThrowerProjectile1);
		StopProjectile(ref m_flameThrowerProjectile2);
		StopProjectile(ref m_flameThrowerProjectile3);
		StopProjectile(ref m_flameThrowerProjectile4);
	}
}
