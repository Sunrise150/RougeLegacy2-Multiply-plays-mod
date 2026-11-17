using System.Collections;
using MoreMountains.CorgiEngine;
using UnityEngine;

public class Roll_Ability : BaseAbility_RL, ITalent, IAbility
{
	private float m_rollStartTime;

	private float m_prevInvincTime;

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

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		ApplyAbilityCosts();
		StartCooldownTimer();
	}

	public override IEnumerator CastAbility()
	{
		PlayerController playerController = m_abilityController.PlayerController;
		if (playerController.CharacterDash.IsDashing)
		{
			playerController.CharacterDash.StopDash();
		}
		m_prevInvincTime = playerController.InvincibilityTimer;
		if (m_prevInvincTime > 5f)
		{
			m_prevInvincTime = 0f;
		}
		playerController.CharacterHitResponse.SetInvincibleTime(9999f, additive: false, playInvincibility: false);
		playerController.ConditionState = CharacterStates.CharacterConditions.DisableHorizontalMovement;
		playerController.CharacterJump.ResetBrakeForce();
		playerController.DisableFriction = true;
		playerController.DisableDoorBlock = true;
		float num = Rewired_RL.Player.GetAxis("MoveHorizontal");
		if (num == 0f)
		{
			num = ((!playerController.IsFacingRight) ? (-1f) : 1f);
		}
		if ((num > 0f && !playerController.IsFacingRight) || (num < 0f && playerController.IsFacingRight))
		{
			playerController.CharacterCorgi.Flip();
		}
		if (num > 0f)
		{
			playerController.SetVelocityX(22f, additive: false);
		}
		else
		{
			playerController.SetVelocityX(-22f, additive: false);
		}
		playerController.SetVelocityY(0f, additive: false);
		m_rollStartTime = Time.time;
		yield return base.CastAbility();
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack)
		{
			while (Time.time - m_rollStartTime < 0.3f)
			{
				yield return null;
			}
			m_animator.SetTrigger("Change_Ability_Anim");
		}
		else
		{
			yield return base.ChangeAnim(duration);
		}
	}

	protected override void OnExitExitLogic()
	{
		base.OnExitExitLogic();
		m_abilityController.PlayerController.JustRolled = true;
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		PlayerController playerController = m_abilityController.PlayerController;
		playerController.DisableFriction = false;
		playerController.DisableDoorBlock = false;
		float b = m_prevInvincTime - (Time.time - m_rollStartTime);
		b = Mathf.Min(1f, b);
		if (b > 0.1f)
		{
			playerController.CharacterHitResponse.SetInvincibleTime(b, additive: false, playerController.CharacterHitResponse.InvincibilityEffectPlaying);
		}
		else
		{
			playerController.CharacterHitResponse.SetInvincibleTime(0.1f, additive: false, playInvincibility: false);
		}
		playerController.ConditionState = CharacterStates.CharacterConditions.Normal;
		if ((bool)m_firedProjectile)
		{
			m_firedProjectile.FlagForDestruction();
		}
		base.StopAbility(abilityInterrupted);
	}
}
