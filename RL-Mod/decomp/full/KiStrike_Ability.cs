using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class KiStrike_Ability : BaseAbility_RL, ITalent, IAbility
{
	[SerializeField]
	private bool m_disableGravity;

	protected override float TellIntroAnimSpeed => 2f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 2f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.1f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	public override IEnumerator CastAbility()
	{
		if (m_disableGravity && m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement)
		{
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
			m_abilityController.PlayerController.SetVelocity(0f, 0f, additive: false);
			m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: false);
		}
		yield return base.CastAbility();
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
			m_firedProjectile = null;
		}
		if (m_disableGravity)
		{
			m_abilityController.PlayerController.ControllerCorgi.GravityActive(state: true);
			if (m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
			{
				m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
			}
		}
		base.StopAbility(abilityInterrupted);
	}
}
