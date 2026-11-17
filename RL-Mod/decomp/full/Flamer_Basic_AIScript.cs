using System.Collections;
using UnityEngine;

public class Flamer_Basic_AIScript : BaseAIScript
{
	protected enum FlameFiringState
	{
		None,
		Regular,
		Mega1,
		Mega2
	}

	protected const string FLAME_TELL_PROJECTILE = "FlamerWarningProjectile";

	protected Projectile_RL m_flameTellProjectile;

	protected Projectile_RL m_flameWalkProjectile;

	protected Projectile_RL m_flameWalkProjectile2;

	protected Projectile_RL m_flameWalkProjectile3;

	protected FlameFiringState m_firingState;

	protected WaitUntil m_waitUntilFallingYield;

	protected WaitRL_Yield m_loseGravityDurationYield;

	protected const string FLAMEWALK_TELL_INTRO = "Flame_Tell_Intro";

	protected const string FLAMEWALK_TELL_HOLD = "Flame_Tell_Hold";

	protected const string FLAMEWALK_ATTACK_INTRO = "Flame_Attack_Intro";

	protected const string FLAMEWALK_ATTACK_HOLD = "Flame_Attack_Hold";

	protected const string FLAMEWALK_EXIT = "Flame_Exit";

	protected const string FLAMEWALK_LINEBOLT_PROJECTILE = "FlamerLineBoltProjectile";

	protected float m_flameWalk_TellIntro_AnimationSpeed = 0.85f;

	protected float m_flameWalk_TellHold_AnimationSpeed = 1.3f;

	protected float m_flameWalk_Tell_Delay = 1.15f;

	protected float m_flameWalk_AttackIntro_AnimationSpeed = 1f;

	protected float m_flameWalk_AttackIntro_Delay;

	protected float m_flameWalk_AttackHold_AnimationSpeed = 1f;

	protected float m_flameWalk_AttackHold_Delay;

	protected float m_flameWalk_Exit_AnimationSpeed = 1f;

	protected float m_flameWalk_Exit_Delay;

	protected const string FLAME_BOLT_TELL_INTRO = "BigFlame_Tell_Intro";

	protected const string FLAME_BOLT_TELL_HOLD = "BigFlame_Tell_Hold";

	protected const string FLAME_BOLT_ATTACK_INTRO = "BigFlame_Attack_Intro";

	protected const string FLAME_BOLT_ATTACK_HOLD = "BigFlame_Attack_Hold";

	protected const string FLAME_BOLT_EXIT = "BigFlame_Exit";

	protected const string FLAME_BOLT_PROJECTILE = "FlamerBombProjectile";

	protected const string FLAME_BOLT_EXPLOSION_PROJECTILE = "FlamerBombExplosionProjectile";

	protected float m_flameBolt_TellIntro_AnimationSpeed = 1.15f;

	protected float m_flameBolt_TellHold_AnimationSpeed = 1.3f;

	protected float m_flameBolt_Tell_Delay = 1.3f;

	protected float m_flameBolt_AttackIntro_AnimationSpeed = 1f;

	protected float m_flameBolt_AttackIntro_Delay;

	protected float m_flameBolt_AttackHold_AnimationSpeed = 1f;

	protected float m_flameBolt_AttackHold_Delay;

	protected float m_flameBolt_Exit_AnimationSpeed = 1f;

	protected float m_flameBolt_Exit_Delay;

	protected const string BIGFLAME_TELL_INTRO = "BigFlame_Tell_Intro";

	protected const string BIGFLAME_TELL_HOLD = "BigFlame_Tell_Hold";

	protected const string BIGFLAME_ATTACK_INTRO = "BigFlame_Attack_Intro";

	protected const string BIGFLAME_ATTACK_HOLD = "BigFlame_Attack_Hold";

	protected const string BIGFLAME_EXIT = "BigFlame_Exit";

	protected const string BIGFLAME_PROJECTILE = "FlamerLineBoltMinibossProjectile";

	protected float m_megaFlame_TellIntro_AnimationSpeed = 0.85f;

	protected float m_megaFlame_TellHold_AnimationSpeed = 1.3f;

	protected float m_megaFlame_Tell_Delay = 1.75f;

	protected float m_megaFlame_AttackIntro_AnimationSpeed = 1f;

	protected float m_megaFlame_AttackIntro_Delay;

	protected float m_megaFlame_AttackHold_AnimationSpeed = 1f;

	protected float m_megaFlame_AttackHold_Delay;

	protected float m_megaFlame_Exit_AnimationSpeed = 1f;

	protected float m_megaFlame_Exit_Delay;

	private bool m_wasFrozenWhileFiring;

	protected virtual float m_jumpAttack_loseGravityDuration => 0.15f;

	protected virtual float m_flameWalk_Exit_ForceIdle => 1.25f;

	protected virtual float m_flameWalk_AttackCD => 0f;

	protected virtual float m_flameWalk_AttackDuration => 1.75f;

	protected virtual float m_flameWalk_AttackMoveSpeed => 6.5f;

	protected virtual float m_flameBolt_Exit_ForceIdle => 0.25f;

	protected virtual float m_flameBolt_AttackCD => 11f;

	protected virtual Vector2 m_flameBolt_Jump => new Vector2(-12f, 13f);

	protected virtual float m_flameBolt_SecondShotDelay => 0.35f;

	protected virtual float m_megaFlame_Exit_ForceIdle => 0.25f;

	protected virtual float m_megaFlame_AttackCD => 11f;

	protected virtual bool m_megaFlame_hasVerticalDashFlame => false;

	protected virtual float m_megaFlame_AttackDuration => 3f;

	protected virtual float m_megaFlame_AttackMidPauseDuration => 0.75f;

	protected virtual float m_megaFlame_AttackVertDuration => 2f;

	protected virtual float m_megaFlame_AttackMoveSpeed => 4f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[5] { "FlamerLineBoltProjectile", "FlamerLineBoltMinibossProjectile", "FlamerWarningProjectile", "FlamerBombProjectile", "FlamerBombExplosionProjectile" };
	}

	public override void Initialize(EnemyController enemyController)
	{
		base.Initialize(enemyController);
		m_waitUntilFallingYield = new WaitUntil(() => base.EnemyController.Velocity.y < 0f);
		m_loseGravityDurationYield = new WaitRL_Yield(m_jumpAttack_loseGravityDuration);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator CastFlameWalk()
	{
		StopAndFaceTarget();
		bool isFacingRight = base.EnemyController.IsFacingRight;
		base.EnemyController.LockFlip = true;
		m_flameTellProjectile = this.FireProjectile("FlamerWarningProjectile", 2);
		yield return Default_TellIntroAndLoop("Flame_Tell_Intro", m_flameWalk_TellIntro_AnimationSpeed, "Flame_Tell_Hold", m_flameWalk_TellHold_AnimationSpeed, m_flameWalk_Tell_Delay);
		StopProjectile(ref m_flameTellProjectile);
		yield return Default_Animation("Flame_Attack_Intro", m_flameWalk_AttackIntro_AnimationSpeed, m_flameWalk_AttackIntro_Delay);
		yield return Default_Animation("Flame_Attack_Hold", m_flameWalk_AttackHold_AnimationSpeed, m_flameWalk_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_firingState = FlameFiringState.Regular;
		m_flameWalkProjectile = this.FireProjectile("FlamerLineBoltProjectile", 2);
		if (isFacingRight)
		{
			SetVelocityX(m_flameWalk_AttackMoveSpeed, additive: false);
		}
		else
		{
			SetVelocityX(0f - m_flameWalk_AttackMoveSpeed, additive: false);
		}
		base.EnemyController.GroundHorizontalVelocity = base.EnemyController.Velocity.x;
		if (m_flameWalk_AttackDuration > 0f)
		{
			yield return Wait(m_flameWalk_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		base.EnemyController.GroundHorizontalVelocity = 0f;
		StopProjectile(ref m_flameWalkProjectile);
		yield return Default_Animation("Flame_Exit", m_flameWalk_Exit_AnimationSpeed, m_flameWalk_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_flameWalk_Exit_ForceIdle, m_flameWalk_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator FlameBolt()
	{
		StopAndFaceTarget();
		_ = base.EnemyController.IsFacingRight;
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("BigFlame_Tell_Intro", m_flameBolt_TellIntro_AnimationSpeed, "BigFlame_Tell_Hold", m_flameBolt_TellHold_AnimationSpeed, m_flameBolt_Tell_Delay);
		yield return Default_Animation("BigFlame_Attack_Intro", m_flameBolt_AttackIntro_AnimationSpeed, m_flameBolt_AttackIntro_Delay);
		yield return Default_Animation("BigFlame_Attack_Hold", m_flameBolt_AttackHold_AnimationSpeed, m_flameBolt_AttackHold_Delay, waitUntilAnimationComplete: false);
		if (base.EnemyController.IsFacingRight)
		{
			this.FireProjectile("FlamerBombProjectile", 3, matchFacing: false);
			SetVelocityX(m_flameBolt_Jump.x, additive: false);
		}
		else
		{
			this.FireProjectile("FlamerBombProjectile", 3, matchFacing: false, 180f);
			SetVelocityX(0f - m_flameBolt_Jump.x, additive: false);
		}
		SetVelocityY(m_flameBolt_Jump.y, additive: false);
		yield return Wait(0.05f);
		if (base.EnemyController.IsFacingRight)
		{
			base.EnemyController.JumpHorizontalVelocity = m_flameBolt_Jump.x;
		}
		else
		{
			base.EnemyController.JumpHorizontalVelocity = 0f - m_flameBolt_Jump.x;
		}
		if (base.EnemyController.EnemyRank == EnemyRank.Expert)
		{
			yield return Wait(m_flameBolt_SecondShotDelay);
			if (base.EnemyController.IsFacingRight)
			{
				this.FireProjectile("FlamerBombProjectile", 3, matchFacing: false);
				SetVelocityX(m_flameBolt_Jump.x, additive: false);
			}
			else
			{
				this.FireProjectile("FlamerBombProjectile", 3, matchFacing: false, 180f);
				SetVelocityX(0f - m_flameBolt_Jump.x, additive: false);
			}
			SetVelocityY(m_flameBolt_Jump.y, additive: false);
			yield return Wait(0.05f);
			if (base.EnemyController.IsFacingRight)
			{
				base.EnemyController.JumpHorizontalVelocity = m_flameBolt_Jump.x;
			}
			else
			{
				base.EnemyController.JumpHorizontalVelocity = 0f - m_flameBolt_Jump.x;
			}
		}
		yield return WaitUntilIsGrounded();
		SetVelocityX(0f, additive: false);
		yield return Default_Animation("BigFlame_Exit", m_flameBolt_Exit_AnimationSpeed, m_flameBolt_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_flameBolt_Exit_ForceIdle, m_flameBolt_AttackCD);
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[MinibossEnemy]
	public IEnumerator CastMegaFlame()
	{
		StopAndFaceTarget();
		bool isFacingRight = base.EnemyController.IsFacingRight;
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("BigFlame_Tell_Intro", m_megaFlame_TellIntro_AnimationSpeed, "BigFlame_Tell_Hold", m_megaFlame_TellHold_AnimationSpeed, m_megaFlame_Tell_Delay);
		yield return Default_Animation("BigFlame_Attack_Intro", m_megaFlame_AttackIntro_AnimationSpeed, m_megaFlame_AttackIntro_Delay);
		yield return Default_Animation("BigFlame_Attack_Hold", m_megaFlame_AttackHold_AnimationSpeed, m_megaFlame_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_firingState = FlameFiringState.Mega1;
		m_flameWalkProjectile = this.FireProjectile("FlamerLineBoltMinibossProjectile", 2, matchFacing: false);
		m_flameWalkProjectile2 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 0, matchFacing: false, 180f);
		if (isFacingRight)
		{
			SetVelocityX(m_megaFlame_AttackMoveSpeed, additive: false);
		}
		else
		{
			SetVelocityX(0f - m_megaFlame_AttackMoveSpeed, additive: false);
		}
		if (m_megaFlame_AttackDuration > 0f)
		{
			yield return Wait(m_megaFlame_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		StopProjectile(ref m_flameWalkProjectile);
		StopProjectile(ref m_flameWalkProjectile2);
		if (m_megaFlame_AttackMidPauseDuration > 0f)
		{
			yield return Wait(m_megaFlame_AttackMidPauseDuration);
		}
		m_firingState = FlameFiringState.Mega2;
		m_flameWalkProjectile = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: false, 70f);
		m_flameWalkProjectile2 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: false, 110f);
		m_flameWalkProjectile3 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: false, 90f);
		if (m_megaFlame_AttackVertDuration > 0f)
		{
			yield return Wait(m_megaFlame_AttackVertDuration);
		}
		yield return Default_Animation("BigFlame_Exit", m_megaFlame_Exit_AnimationSpeed, m_megaFlame_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.LockFlip = false;
		StopProjectile(ref m_flameWalkProjectile);
		StopProjectile(ref m_flameWalkProjectile2);
		StopProjectile(ref m_flameWalkProjectile3);
		yield return Default_Attack_Cooldown(m_megaFlame_Exit_ForceIdle, m_megaFlame_AttackCD);
	}

	public override void Pause()
	{
		base.Pause();
		if ((bool)m_flameWalkProjectile && !m_flameWalkProjectile.IsFreePoolObj && m_flameWalkProjectile.Owner == base.EnemyController.gameObject)
		{
			m_wasFrozenWhileFiring = true;
			StopProjectile(ref m_flameWalkProjectile);
		}
		if ((bool)m_flameWalkProjectile2 && !m_flameWalkProjectile2.IsFreePoolObj && m_flameWalkProjectile2.Owner == base.EnemyController.gameObject)
		{
			m_wasFrozenWhileFiring = true;
			StopProjectile(ref m_flameWalkProjectile2);
		}
		if ((bool)m_flameWalkProjectile3 && !m_flameWalkProjectile3.IsFreePoolObj && m_flameWalkProjectile3.Owner == base.EnemyController.gameObject)
		{
			m_wasFrozenWhileFiring = true;
			StopProjectile(ref m_flameWalkProjectile3);
		}
	}

	public override void Unpause()
	{
		base.Unpause();
		if (m_wasFrozenWhileFiring)
		{
			switch (m_firingState)
			{
			case FlameFiringState.Regular:
				m_flameWalkProjectile = this.FireProjectile("FlamerLineBoltProjectile", 2);
				break;
			case FlameFiringState.Mega1:
				m_flameWalkProjectile = this.FireProjectile("FlamerLineBoltMinibossProjectile", 2, matchFacing: false);
				m_flameWalkProjectile2 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 0, matchFacing: false, 180f);
				break;
			case FlameFiringState.Mega2:
				m_flameWalkProjectile = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: false, 70f);
				m_flameWalkProjectile2 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: false, 110f);
				m_flameWalkProjectile3 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: false, 90f);
				break;
			}
			m_wasFrozenWhileFiring = false;
		}
	}

	public override void OnLBCompleteOrCancelled()
	{
		StopProjectile(ref m_flameTellProjectile);
		StopProjectile(ref m_flameWalkProjectile);
		StopProjectile(ref m_flameWalkProjectile2);
		StopProjectile(ref m_flameWalkProjectile3);
		m_wasFrozenWhileFiring = false;
		base.EnemyController.GroundHorizontalVelocity = 0f;
		base.OnLBCompleteOrCancelled();
		m_firingState = FlameFiringState.None;
	}

	public override void ResetScript()
	{
		base.EnemyController.ControllerCorgi.GravityActive(state: true);
		base.EnemyController.DisableFriction = false;
		m_firingState = FlameFiringState.None;
		base.ResetScript();
	}
}
