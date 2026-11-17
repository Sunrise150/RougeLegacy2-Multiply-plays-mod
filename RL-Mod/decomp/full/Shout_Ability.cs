using MoreMountains.CorgiEngine;
using UnityEngine;

public class Shout_Ability : BaseAbility_RL, ITalent, IAbility
{
	[SerializeField]
	private Vector2 m_pushbackAmount = Vector2.zero;

	[HelpBox("OnShoutEvent WARNING: For Player abilities, set FollowTarget to OTHER to properly follow player.", HelpBoxMessageType.Warning)]
	public UnityEvent_GameObject OnShoutEvent;

	public virtual Vector2 PushbackAmount => m_pushbackAmount;

	protected override void FireProjectile()
	{
		base.FireProjectile();
		if (!m_abilityController.PlayerController.IsGrounded && PushbackAmount != Vector2.zero)
		{
			m_abilityController.PlayerController.CharacterJump.ResetBrakeForce();
			Vector2 pushbackAmount = PushbackAmount;
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				pushbackAmount.x = 0f - pushbackAmount.x;
			}
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
			Vector2 pushbackAmount2 = PushbackAmount;
			if (m_abilityController.PlayerController.IsFacingRight)
			{
				pushbackAmount2.x = 0f - pushbackAmount2.x;
			}
			m_abilityController.PlayerController.SetVelocity(pushbackAmount2.x, pushbackAmount2.y, additive: false);
		}
		if (OnShoutEvent != null)
		{
			OnShoutEvent.Invoke(m_abilityController.PlayerController.gameObject);
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		base.StopAbility(abilityInterrupted);
		if (!m_abilityController.PlayerController.IsGrounded && PushbackAmount != Vector2.zero && m_abilityController.PlayerController.ConditionState != CharacterStates.CharacterConditions.Stunned)
		{
			m_abilityController.PlayerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		}
	}
}
