using System.Collections;
using UnityEngine;

public class SpearKnight_Basic_AIScript : BaseAIScript
{
	private const string MINIBOSS_BOLT_PROJECTILE = "SpearKnightBoltMinibossProjectile";

	private const string HEAD_BOBBLE_PROJECTILE = "SpearKnightCurseProjectile";

	private const string CURSE_PROJECTILE = "SpearKnightCurseProjectile";

	private const string DAGGER_BOLT_PROJECTILE = "SpearKnightDaggerBoltRedProjectile";

	private const string EXPERT_DAGGER_BOLT_PROJECTILE = "SpearKnightDaggerBoltRedExpertProjectile";

	private const string MINIBOSS_DAGGER_BOLT_PROJECTILE = "SpearKnightDaggerBoltRedMinibossProjectile";

	protected float m_dash_TellIntro_AnimationSpeed = 1f;

	protected float m_dash_TellHold_AnimationSpeed = 1f;

	protected float m_dash_TellIntroAndHold_Delay = 0.75f;

	protected float m_dash_AttackIntro_AnimationSpeed = 1f;

	protected float m_dash_AttackIntro_Delay;

	protected float m_dash_AttackHold_AnimationSpeed = 1f;

	protected float m_dash_AttackHold_Delay;

	protected float m_dash_Exit_AnimationSpeed = 0.65f;

	protected float m_dash_Exit_Delay;

	protected float m_dash_Exit_ForceIdle = 0.15f;

	protected float m_dash_AttackCD = 2.5f;

	protected float m_dashUppercut_TellIntro_AnimationSpeed = 1f;

	protected float m_dashUppercut_TellHold_AnimationSpeed = 1f;

	protected float m_dashUppercut_TellIntroAndHold_Delay = 0.9f;

	protected float m_dashUppercut_DashOnly_AttackIntro_AnimationSpeed = 1f;

	protected float m_dashUppercut_DashOnly_AttackIntro_Delay;

	protected float m_dashUppercut_DashOnly_AttackHold_AnimationSpeed = 1f;

	protected float m_dashUppercut_DashOnly_AttackHold_Delay;

	protected float m_dashUppercut_UppercutOnly_AttackIntro_AnimationSpeed = 1f;

	protected float m_dashUppercut_UppercutOnly_AttackIntro_Delay;

	protected float m_dashUppercut_UppercutOnly_AttackHold_AnimationSpeed = 1.25f;

	protected float m_dashUppercut_UppercutOnly_AttackHold_Delay;

	protected float m_dashUppercut_Exit_AnimationSpeed = 0.65f;

	protected float m_dashUppercut_Exit_Delay;

	protected float m_dashUppercut_Exit_ForceIdle = 0.15f;

	protected float m_dashUppercut_AttackCD;

	protected float m_thrust_TellIntro_AnimationSpeed = 1f;

	protected float m_thrust_TellHold_AnimationSpeed = 1f;

	protected float m_thrust_TellIntroAndHold_Delay = 0.75f;

	protected float m_thrust_TellIntroAndHoldRepeat_Delay = 0.15f;

	protected float m_thrust_AttackIntro_AnimationSpeed = 1f;

	protected float m_thrust_AttackIntro_Delay;

	protected float m_thrust_AttackHold_AnimationSpeed = 1f;

	protected float m_thrust_AttackHold_Delay;

	protected float m_thrust_Exit_AnimationSpeed = 0.65f;

	protected float m_thrust_Exit_Delay;

	protected float m_thrust_Exit_ForceIdle = 0.15f;

	protected float m_thrust_AttackCD = 2.5f;

	protected float m_throw_TellIntro_AnimationSpeed = 0.75f;

	protected float m_throw_TellHold_AnimationSpeed = 0.65f;

	protected float m_throw_TellIntroAndHold_Delay = 0.9f;

	protected float m_throw_AttackIntro_AnimationSpeed = 1f;

	protected float m_throw_AttackIntro_Delay;

	protected float m_throw_AttackHold_AnimationSpeed = 1f;

	protected float m_throw_AttackHold_Delay = 0.25f;

	protected float m_throw_Exit_AnimationSpeed = 0.45f;

	protected float m_throw_Exit_Delay = 0.15f;

	protected const string HEAD_BOBBLE_TELL_INTRO = "SpearKnight_HeadShake_Tell_Intro";

	protected const string HEAD_BOBBLE_TELL_HOLD = "SpearKnight_HeadShake_Tell_Hold";

	protected const string HEAD_BOBBLE_ATTACK_INTRO = "SpearKnight_HeadShake_Attack_Intro";

	protected const string HEAD_BOBBLE_ATTACK_HOLD = "SpearKnight_HeadShake_Attack_Hold";

	protected const string HEAD_BOBBLE_EXIT = "SpearKnight_HeadShake_Exit";

	protected float m_headBobble_TellIntro_AnimSpeed = 1f;

	protected float m_headBobble_TellHold_AnimSpeed = 1f;

	protected float m_headBobble_TellIntroAndHold_Delay = 0.9f;

	protected float m_headBobble_AttackIntro_AnimSpeed = 1f;

	protected float m_headBobble_AttackIntro_Delay;

	protected float m_headBobble_AttackHold_AnimSpeed = 1f;

	protected float m_headBobble_AttackHold_Delay = 1f;

	protected float m_headBobble_AttackHold_Exit_Delay = 0.5f;

	protected float m_headBobble_Exit_AnimSpeed = 1f;

	protected float m_headBobble_Exit_Delay = 0.45f;

	protected float m_headBobble_Exit_IdleDuration = 0.15f;

	protected float m_headBobble_AttackCD = 15f;

	protected int m_numHeadBobbleProjectiles = 3;

	protected override Vector2 IdleDuration => new Vector2(0.35f, 0.75f);

	protected override Vector2 WalkTowardsDuration => new Vector2(0.5f, 0.85f);

	protected override Vector2 WalkAwayDuration => new Vector2(0.5f, 0.5f);

	protected virtual bool m_raiseKnockbackDefenseWhileAttacking => true;

	protected virtual int m_knockbackDefenseBoostOverride => 7;

	protected virtual float Dash_AttackSpeed => 28f;

	protected virtual float Dash_AttackDuration => 0.5f;

	protected virtual bool m_dashAttack_DashOffLedges => false;

	protected virtual float DashUppercut_AttackSpeed => 19f;

	protected virtual float DashUppercut_AttackDuration => 0.1f;

	protected virtual float DashUppercut_JumpSpeed => 0f;

	protected virtual float Uppercut_JumpPower => 27f;

	protected virtual float m_thrust_AttackSpeed => 28f;

	protected virtual float m_thrust_AttackDuration => 0.325f;

	protected virtual float m_thrust_AttackAmount => 2f;

	protected virtual float m_thrust_AttackLoopDelay => 0.15f;

	protected virtual float m_throw_Exit_ForceIdle => 0.4f;

	protected virtual float m_throw_AttackCD => 3.5f;

	protected virtual bool m_throw_Attack_TargetPlayer => false;

	protected virtual int m_throw_Attack_ProjectileAmount => 1;

	protected virtual float m_throw_Attack_ProjectileDelay => 0.5f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[6] { "SpearKnightBoltMinibossProjectile", "SpearKnightCurseProjectile", "SpearKnightCurseProjectile", "SpearKnightDaggerBoltRedProjectile", "SpearKnightDaggerBoltRedExpertProjectile", "SpearKnightDaggerBoltRedMinibossProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator DashAttack()
	{
		StopAndFaceTarget();
		float dashSpeed = Dash_AttackSpeed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			dashSpeed = 0f - dashSpeed;
		}
		base.EnemyController.LockFlip = true;
		if (m_dashAttack_DashOffLedges)
		{
			base.EnemyController.FallLedge = true;
		}
		yield return Default_TellIntroAndLoop("Dash_Tell_Intro", m_dash_TellIntro_AnimationSpeed, "Dash_Tell_Hold", m_dash_TellHold_AnimationSpeed, m_dash_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Dash_Attack_Intro", m_dash_AttackIntro_AnimationSpeed, m_dash_AttackIntro_Delay);
		yield return Default_Animation("Dash_Attack_Hold", m_dash_AttackHold_AnimationSpeed, m_dash_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (m_raiseKnockbackDefenseWhileAttacking && base.EnemyController.BaseKnockbackDefense < (float)m_knockbackDefenseBoostOverride)
		{
			base.EnemyController.BaseKnockbackDefense = m_knockbackDefenseBoostOverride;
		}
		SetVelocityX(dashSpeed, additive: false);
		base.EnemyController.GroundHorizontalVelocity = dashSpeed;
		base.EnemyController.DisableFriction = true;
		if (Dash_AttackDuration > 0f)
		{
			yield return Wait(Dash_AttackDuration);
		}
		yield return WaitUntilIsGrounded();
		SetVelocityX(0f, additive: false);
		base.EnemyController.GroundHorizontalVelocity = 0f;
		base.EnemyController.DisableFriction = false;
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("Dash_Exit", m_dash_Exit_AnimationSpeed, m_dash_Exit_Delay);
		base.EnemyController.FallLedge = false;
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_dash_Exit_ForceIdle, m_dash_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator DashAndUppercutAttack()
	{
		StopAndFaceTarget();
		float dashSpeed = DashUppercut_AttackSpeed;
		float jumpSpeed = DashUppercut_JumpSpeed;
		if (!base.EnemyController.IsTargetToMyRight)
		{
			dashSpeed = 0f - dashSpeed;
			jumpSpeed = 0f - jumpSpeed;
		}
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("Combo_Tell_Intro", m_dashUppercut_TellIntro_AnimationSpeed, "Combo_Tell_Hold", m_dashUppercut_TellHold_AnimationSpeed, m_dashUppercut_TellIntroAndHold_Delay);
		SetAttackingWithContactDamage(enable: true);
		yield return Default_Animation("Combo_Dash_Attack_Intro", m_dashUppercut_DashOnly_AttackIntro_AnimationSpeed, m_dashUppercut_DashOnly_AttackIntro_Delay);
		yield return Default_Animation("Combo_Dash_Attack_Hold", m_dashUppercut_DashOnly_AttackHold_AnimationSpeed, m_dashUppercut_DashOnly_AttackHold_Delay, waitUntilAnimationComplete: false);
		SetVelocityX(dashSpeed, additive: false);
		base.EnemyController.GroundHorizontalVelocity = dashSpeed;
		if (DashUppercut_AttackDuration > 0f)
		{
			yield return Wait(DashUppercut_AttackDuration);
		}
		yield return Default_Animation("Combo_Uppercut_Tell", m_dashUppercut_UppercutOnly_AttackIntro_AnimationSpeed, m_dashUppercut_UppercutOnly_AttackIntro_Delay);
		yield return Default_Animation("Combo_Uppercut_Hold", m_dashUppercut_UppercutOnly_AttackHold_AnimationSpeed, m_dashUppercut_UppercutOnly_AttackHold_Delay, waitUntilAnimationComplete: false);
		SetVelocity(jumpSpeed, Uppercut_JumpPower, additive: false);
		if (base.LogicController.EnemyLogicType == EnemyLogicType.Miniboss)
		{
			yield return Wait(0.45f);
			this.FireProjectile("SpearKnightBoltMinibossProjectile", 0, matchFacing: true, 55f);
			this.FireProjectile("SpearKnightBoltMinibossProjectile", 0, matchFacing: true, 125f);
			this.FireProjectile("SpearKnightBoltMinibossProjectile", 0, matchFacing: true, 75f);
			this.FireProjectile("SpearKnightBoltMinibossProjectile", 0, matchFacing: true, 105f);
			this.FireProjectile("SpearKnightBoltMinibossProjectile", 0, matchFacing: true, 95f);
			this.FireProjectile("SpearKnightBoltMinibossProjectile", 0, matchFacing: true, 85f);
		}
		yield return WaitUntilIsGrounded();
		yield return Default_Animation("Combo_Exit", m_dashUppercut_Exit_AnimationSpeed, m_dashUppercut_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Attack_Cooldown(m_dashUppercut_Exit_ForceIdle, m_dashUppercut_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ThrustAttack()
	{
		for (int i = 0; (float)i < m_thrust_AttackAmount; i++)
		{
			float thrustSpeed = m_thrust_AttackSpeed;
			if (i == 0)
			{
				yield return Default_TellIntroAndLoop("Thrust_Tell_Intro", m_thrust_TellIntro_AnimationSpeed, "Thrust_Tell_Hold", m_thrust_TellHold_AnimationSpeed, m_thrust_TellIntroAndHold_Delay);
			}
			else
			{
				yield return Default_TellIntroAndLoop("Thrust_Tell_Intro", m_thrust_TellIntro_AnimationSpeed, "Thrust_Tell_Hold", m_thrust_TellHold_AnimationSpeed, m_thrust_TellIntroAndHoldRepeat_Delay);
			}
			StopAndFaceTarget();
			if (!base.EnemyController.IsTargetToMyRight)
			{
				thrustSpeed = 0f - thrustSpeed;
			}
			base.EnemyController.LockFlip = true;
			yield return Default_Animation("Thrust_Attack_Intro", m_thrust_AttackIntro_AnimationSpeed, m_thrust_AttackIntro_Delay);
			yield return Default_Animation("Thrust_Attack_Hold", m_thrust_AttackHold_AnimationSpeed, m_thrust_AttackHold_Delay, waitUntilAnimationComplete: false);
			if (m_raiseKnockbackDefenseWhileAttacking && base.EnemyController.BaseKnockbackDefense < (float)m_knockbackDefenseBoostOverride)
			{
				base.EnemyController.BaseKnockbackDefense = m_knockbackDefenseBoostOverride;
			}
			SetVelocityX(thrustSpeed, additive: false);
			base.EnemyController.GroundHorizontalVelocity = thrustSpeed;
			base.EnemyController.DisableFriction = true;
			if (m_thrust_AttackDuration > 0f)
			{
				yield return Wait(m_thrust_AttackDuration);
			}
			SetVelocityX(0f, additive: false);
			base.EnemyController.GroundHorizontalVelocity = 0f;
			base.EnemyController.DisableFriction = false;
			if (m_thrust_AttackLoopDelay > 0f)
			{
				yield return Wait(m_thrust_AttackLoopDelay);
			}
			yield return Default_Animation("Thrust_Exit", m_thrust_Exit_AnimationSpeed, m_thrust_Exit_Delay);
			base.EnemyController.LockFlip = false;
		}
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_thrust_Exit_ForceIdle, m_thrust_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[RestLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator ThrowAttack()
	{
		StopAndFaceTarget();
		if (!m_throw_Attack_TargetPlayer)
		{
			base.EnemyController.LockFlip = true;
		}
		yield return Default_TellIntroAndLoop("SpearKnight_Throw_Tell_Intro", m_throw_TellIntro_AnimationSpeed, "SpearKnight_Throw_Tell_Hold", m_throw_TellHold_AnimationSpeed, m_throw_TellIntroAndHold_Delay);
		for (int i = 0; i < m_throw_Attack_ProjectileAmount; i++)
		{
			yield return Default_Animation("SpearKnight_Throw_Attack_Intro", m_throw_AttackIntro_AnimationSpeed, m_throw_AttackIntro_Delay);
			yield return Default_Animation("SpearKnight_Throw_Attack_Hold", m_throw_AttackHold_AnimationSpeed, 0f, waitUntilAnimationComplete: false);
			string projectileName = "SpearKnightDaggerBoltRedProjectile";
			if (base.EnemyController.EnemyRank == EnemyRank.Expert)
			{
				projectileName = "SpearKnightDaggerBoltRedExpertProjectile";
			}
			else if (base.EnemyController.EnemyRank == EnemyRank.Miniboss)
			{
				projectileName = "SpearKnightDaggerBoltRedMinibossProjectile";
			}
			if (m_throw_Attack_TargetPlayer)
			{
				float rads = Mathf.Atan2(base.EnemyController.TargetController.Midpoint.y - base.EnemyController.Midpoint.y, base.EnemyController.TargetController.Midpoint.x - base.EnemyController.Midpoint.x);
				rads = CDGHelper.WrapAngleDegrees(CDGHelper.ToDegrees(rads), convertToHalfRadius: false);
				this.FireProjectile(projectileName, 1, matchFacing: false, rads);
			}
			else
			{
				this.FireProjectile(projectileName, 1);
			}
			if (m_throw_Attack_ProjectileAmount > 1 && m_throw_Attack_ProjectileDelay > 0f)
			{
				yield return Wait(m_throw_Attack_ProjectileDelay);
			}
		}
		yield return Wait(m_throw_AttackHold_Delay);
		yield return Default_Animation("SpearKnight_Throw_Exit", m_throw_Exit_AnimationSpeed, m_throw_Exit_Delay);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_throw_Exit_ForceIdle, m_throw_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public virtual IEnumerator UpperCutBullets()
	{
		yield break;
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator HeadBobbleAttack()
	{
		StopAndFaceTarget();
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("SpearKnight_HeadShake_Tell_Intro", m_headBobble_TellIntro_AnimSpeed, "SpearKnight_HeadShake_Tell_Hold", m_headBobble_TellHold_AnimSpeed, m_headBobble_TellIntroAndHold_Delay);
		yield return Default_Animation("SpearKnight_HeadShake_Attack_Intro", m_headBobble_AttackIntro_AnimSpeed, m_headBobble_AttackIntro_Delay, waitUntilAnimationComplete: false);
		yield return this.ChangeAnimationState("SpearKnight_HeadShake_Attack_Hold");
		float projFireInterval = m_headBobble_AttackHold_Delay / (float)m_numHeadBobbleProjectiles;
		for (int i = 0; i < m_numHeadBobbleProjectiles; i++)
		{
			this.FireProjectile("SpearKnightCurseProjectile", 2, matchFacing: false, 90f);
			yield return Wait(projFireInterval);
		}
		yield return Wait(m_headBobble_AttackHold_Exit_Delay);
		SetAttackingWithContactDamage(enable: false);
		yield return Default_Animation("SpearKnight_HeadShake_Exit", m_headBobble_Exit_AnimSpeed, m_headBobble_Exit_Delay);
		base.EnemyController.LockFlip = false;
		base.EnemyController.ResetBaseValues();
		yield return Default_Attack_Cooldown(m_headBobble_Exit_IdleDuration, m_headBobble_AttackCD);
	}

	public override void OnLBCompleteOrCancelled()
	{
		base.OnLBCompleteOrCancelled();
		base.EnemyController.FallLedge = false;
		base.EnemyController.DisableFriction = false;
		base.EnemyController.GroundHorizontalVelocity = 0f;
		base.EnemyController.LockFlip = false;
	}
}
