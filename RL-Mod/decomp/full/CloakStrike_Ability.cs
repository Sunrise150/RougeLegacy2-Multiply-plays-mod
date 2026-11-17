using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

[Obsolete("Currently Using CloakStrikeNew instead.")]
public class CloakStrike_Ability : BaseAbility_RL, ITalent, IAbility
{
	protected override float TellIntroAnimSpeed => 1f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0.2f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0f;

	public override IEnumerator CastAbility()
	{
		PlayerController playerController = m_abilityController.PlayerController;
		playerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		playerController.ControllerCorgi.State.IsCollidingBelow = false;
		playerController.ControllerCorgi.State.IsFalling = true;
		playerController.CharacterJump.ResetBrakeForce();
		while (!base.IsAnimationComplete && !playerController.IsGrounded)
		{
			yield return null;
		}
	}

	protected override void OnEnterTellIntroLogic()
	{
		PlayerController playerController = m_abilityController.PlayerController;
		if (playerController.IsFacingRight)
		{
			playerController.SetVelocity(Ability_EV.CLOAKSTRIKE_INITIAL_VELOCITY.x, Ability_EV.CLOAKSTRIKE_INITIAL_VELOCITY.y, additive: false);
		}
		else
		{
			playerController.SetVelocity(0f - Ability_EV.CLOAKSTRIKE_INITIAL_VELOCITY.x, Ability_EV.CLOAKSTRIKE_INITIAL_VELOCITY.y, additive: false);
		}
		base.OnEnterTellIntroLogic();
	}

	protected override void OnEnterAttackLogic()
	{
		PlayerController playerController = m_abilityController.PlayerController;
		playerController.ControllerCorgi.GravityActive(state: false);
		if (playerController.IsFacingRight)
		{
			playerController.SetVelocity(Ability_EV.CLOAKSTRIKE_ATTACK_VELOCITY.x, Ability_EV.CLOAKSTRIKE_ATTACK_VELOCITY.y, additive: false);
		}
		else
		{
			playerController.SetVelocity(0f - Ability_EV.CLOAKSTRIKE_ATTACK_VELOCITY.x, Ability_EV.CLOAKSTRIKE_ATTACK_VELOCITY.y, additive: false);
		}
		base.OnEnterAttackLogic();
	}

	protected override void FireProjectile()
	{
		base.FireProjectile();
		m_firedProjectile.OnCollisionRelay.AddListener(Ricochet);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		m_abilityController.Animator.SetTrigger("Cancel_Ability_Anim");
		if (m_firedProjectile != null)
		{
			m_firedProjectile.OnCollisionRelay.RemoveListener(Ricochet);
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		base.StopAbility(abilityInterrupted);
	}

	public void Ricochet(Projectile_RL projectile, GameObject colliderObj)
	{
		StopAbility(abilityInterrupted: true);
	}
}
