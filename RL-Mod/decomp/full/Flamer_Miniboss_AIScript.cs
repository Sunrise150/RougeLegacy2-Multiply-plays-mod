using System.Collections;
using UnityEngine;

public class Flamer_Miniboss_AIScript : Flamer_Basic_AIScript
{
	protected const string FLAMEWALK_LINEBOLT_MINIBOSS_PROJECTILE = "FlamerLineBoltMinibossProjectile";

	protected float m_jump_Tell_Delay = 0.75f;

	protected float m_jump_Tell_AnimationSpeed = 0.75f;

	protected float m_jumpAttack_TellIntro_AnimationSpeed = 0.85f;

	protected float m_jumpAttack_TellHold_AnimationSpeed = 1.3f;

	protected float m_jumpAttack_Tell_Delay = 0.5f;

	protected float m_jumpAttack_AttackIntro_AnimationSpeed = 1f;

	protected float m_jumpAttack_AttackIntro_Delay;

	protected float m_jumpAttack_AttackHold_AnimationSpeed = 1f;

	protected float m_jumpAttack_AttackHold_Delay;

	protected float m_jumpAttack_Exit_AnimationSpeed = 1f;

	protected float m_jumpAttack_Exit_Delay;

	protected override float m_flameWalk_Exit_ForceIdle => 0.25f;

	protected override float m_flameWalk_AttackCD => 11f;

	protected override float m_flameWalk_AttackMoveSpeed => 5f;

	protected virtual float m_jumpAttack_Exit_ForceIdle => 0.25f;

	protected virtual float m_jumpAttack_AttackCD => 11f;

	protected virtual Vector2 m_jumpAttack_Power => new Vector2(-14f, 34f);

	protected virtual bool m_jumpAttack_hasMultipleFlames => true;

	protected virtual float m_jumpAttack_AttackDuration => 1.5f;

	protected virtual float m_jumpAttack_AttackMoveSpeed => 0f;

	protected override void InitializeProjectileNameArray()
	{
		m_projectileNameArray = new string[4] { "FlamerLineBoltProjectile", "FlamerLineBoltMinibossProjectile", "FlamerLineBoltMinibossProjectile", "FlamerWarningProjectile" };
	}

	[CloseLogic]
	[MediumLogic]
	[FarLogic]
	[BasicEnemy]
	[AdvancedEnemy]
	[ExpertEnemy]
	[MinibossEnemy]
	public IEnumerator JumpAttack()
	{
		StopAndFaceTarget();
		yield return Wait(m_jumpAttack_Tell_Delay);
		if (!base.EnemyController.IsFacingRight)
		{
			SetVelocityX(m_jumpAttack_Power.x, additive: true);
		}
		else
		{
			SetVelocityX(0f - m_jumpAttack_Power.x, additive: true);
		}
		SetVelocityY(m_jumpAttack_Power.y, additive: false);
		yield return Wait(0.05f);
		yield return m_waitUntilFallingYield;
		SetVelocity(0f, 0f, additive: false);
		float storedKnockbackDefense = base.EnemyController.BaseKnockbackDefense;
		base.EnemyController.BaseKnockbackDefense = 99f;
		base.EnemyController.ControllerCorgi.GravityActive(state: false);
		StopAndFaceTarget();
		bool isFacingRight = base.EnemyController.IsFacingRight;
		base.EnemyController.LockFlip = true;
		yield return Default_TellIntroAndLoop("BigFlame_Tell_Intro", m_jumpAttack_TellIntro_AnimationSpeed, "BigFlame_Tell_Intro", m_jumpAttack_TellHold_AnimationSpeed, m_jumpAttack_Tell_Delay);
		yield return Default_Animation("BigFlame_Attack_Intro", m_jumpAttack_AttackIntro_AnimationSpeed, m_jumpAttack_AttackIntro_Delay);
		yield return Default_Animation("BigFlame_Attack_Hold", m_jumpAttack_AttackHold_AnimationSpeed, m_jumpAttack_AttackHold_Delay, waitUntilAnimationComplete: false);
		m_flameWalkProjectile = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: true, -90f);
		if (m_jumpAttack_hasMultipleFlames)
		{
			m_flameWalkProjectile2 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: true, -50f);
			m_flameWalkProjectile3 = this.FireProjectile("FlamerLineBoltMinibossProjectile", 1, matchFacing: true, -130f);
		}
		if (isFacingRight)
		{
			SetVelocityX(m_jumpAttack_AttackMoveSpeed, additive: false);
		}
		else
		{
			SetVelocityX(0f - m_jumpAttack_AttackMoveSpeed, additive: false);
		}
		if (m_jumpAttack_AttackDuration > 0f)
		{
			yield return Wait(m_jumpAttack_AttackDuration);
		}
		SetVelocityX(0f, additive: false);
		if ((bool)m_flameWalkProjectile && !m_flameWalkProjectile.IsFreePoolObj && m_flameWalkProjectile.Owner == base.EnemyController.gameObject)
		{
			m_flameWalkProjectile.FlagForDestruction();
		}
		if ((bool)m_flameWalkProjectile2 && !m_flameWalkProjectile2.IsFreePoolObj && m_flameWalkProjectile2.Owner == base.EnemyController.gameObject)
		{
			m_flameWalkProjectile2.FlagForDestruction();
		}
		if ((bool)m_flameWalkProjectile3 && !m_flameWalkProjectile3.IsFreePoolObj && m_flameWalkProjectile3.Owner == base.EnemyController.gameObject)
		{
			m_flameWalkProjectile3.FlagForDestruction();
		}
		yield return m_loseGravityDurationYield;
		base.EnemyController.BaseKnockbackDefense = storedKnockbackDefense;
		base.EnemyController.ControllerCorgi.GravityActive(state: true);
		yield return WaitUntilIsGrounded();
		yield return Wait(m_jumpAttack_Exit_Delay);
		yield return this.ChangeAnimationState(BaseAIScript.NEUTRAL_STATE);
		base.EnemyController.LockFlip = false;
		yield return Default_Attack_Cooldown(m_jumpAttack_Exit_ForceIdle, m_jumpAttack_AttackCD);
	}
}
