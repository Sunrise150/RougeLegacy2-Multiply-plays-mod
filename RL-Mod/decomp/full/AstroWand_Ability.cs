using System.Collections;
using MoreMountains.CorgiEngine;

public class AstroWand_Ability : BaseAbility_RL
{
	private bool m_stopVelocityWhenAiming = true;

	private float m_gravityReductionModWhenAiming = 0.25f;

	private float m_storedFallMultiplier;

	protected override float TellIntroAnimSpeed => 5f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0.155f;

	protected override float AttackIntroAnimSpeed => 5f;

	protected override float AttackIntroAnimExitDelay => 0.1f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.175f;

	protected override float ExitAnimSpeed => 1f;

	protected override float ExitAnimExitDelay => 0f;

	private float PushbackAmountY => 12f;

	public override IEnumerator CastAbility()
	{
		if (m_stopVelocityWhenAiming)
		{
			if (m_abilityController.PlayerController.CharacterJump.JumpHappenedThisFrame)
			{
				m_abilityController.PlayerController.MovementState = CharacterStates.MovementStates.Idle;
			}
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
		}
		yield return base.CastAbility();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		yield return base.ChangeAnim(duration);
		if (base.CurrentAbilityAnimState == AbilityAnimState.TellIntro)
		{
			m_storedFallMultiplier = m_abilityController.PlayerController.FallMultiplierOverride;
			if (m_gravityReductionModWhenAiming < 1f)
			{
				m_abilityController.PlayerController.FallMultiplierOverride = m_gravityReductionModWhenAiming;
			}
			if (!m_abilityController.PlayerController.IsGrounded)
			{
				m_animator.SetBool("Hover", value: true);
			}
		}
		else if (base.CurrentAbilityAnimState == AbilityAnimState.Tell)
		{
			m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
			if (!m_abilityController.PlayerController.IsGrounded)
			{
				m_animator.SetBool("Hover", value: false);
			}
		}
	}

	protected override void FireProjectile()
	{
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerCastingAstroWand, this, null);
		base.FireProjectile();
		if (!m_abilityController.PlayerController.IsGrounded)
		{
			float num = PushbackAmountY;
			if (num < m_abilityController.PlayerController.Velocity.y)
			{
				num = m_abilityController.PlayerController.Velocity.y;
			}
			m_abilityController.PlayerController.SetVelocityY(num, additive: false);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if (m_gravityReductionModWhenAiming < 1f)
		{
			m_abilityController.PlayerController.FallMultiplierOverride = m_storedFallMultiplier;
		}
		m_animator.SetBool("Hover", value: false);
		base.StopAbility(abilityInterrupted);
	}
}
