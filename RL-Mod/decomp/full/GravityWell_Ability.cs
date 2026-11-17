using System.Collections;
using FMODUnity;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class GravityWell_Ability : BaseAbility_RL, ISpell, IAbility
{
	[SerializeField]
	private StudioEventEmitter m_chargeEmitter;

	private float m_castDuration = 0.75f;

	private float m_castStartTime;

	protected override float TellIntroAnimSpeed => 1f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0.35f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.1f;

	protected override float AttackAnimExitDelay => 0.2f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		m_chargeEmitter.Play();
	}

	public override IEnumerator CastAbility()
	{
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement)
		{
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
			m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: false);
		}
		m_castStartTime = 0f;
		yield return base.CastAbility();
	}

	protected override void FireProjectile()
	{
		m_chargeEmitter.Stop();
		base.FireProjectile();
		m_castStartTime = Time.time;
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack)
		{
			while (Time.time < m_castStartTime + m_castDuration)
			{
				yield return null;
			}
		}
		yield return base.ChangeAnim(duration);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		m_chargeEmitter.Stop();
		m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
		if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
		base.StopAbility(abilityInterrupted);
	}
}
