using System.Collections;
using UnityEngine;

public class TimeSlow_Ability : BaseAbility_RL, ITalent, IAbility
{
	private bool m_spellActive;

	private float m_ticRateStartTime;

	protected override float TellIntroAnimSpeed => 2f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 2f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.1f;

	protected override float AttackAnimExitDelay => 0.9f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	public override void PreCastAbility()
	{
		if (m_spellActive)
		{
			m_spellActive = false;
			StopAbility(abilityInterrupted: true);
			StopSlowdown();
		}
		else
		{
			m_spellActive = true;
			base.PreCastAbility();
		}
	}

	public override IEnumerator CastAbility()
	{
		if (m_spellActive)
		{
			yield return base.CastAbility();
		}
	}

	protected override void FireProjectile()
	{
		RLTimeScale.SetTimeScale(TimeScaleType.TimeSlowAbility, 0.2f);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CharacterMove.WalkSpeed *= 5f;
		playerController.CharacterMove.MovementSpeed = playerController.CharacterMove.WalkSpeed;
		playerController.AscentMultiplierOverride *= 5f;
		playerController.FallMultiplierOverride = 5f;
		playerController.CharacterJump.JumpHeightMultiplier = 5f;
		playerController.Animator.speed *= 5f;
		playerController.CharacterDash.DashForce *= 5f;
		ApplyAbilityCosts();
	}

	protected override void ApplyAbilityCosts()
	{
		if ((base.MaxAmmo > 0 && base.CurrentAmmo <= 0) || (base.ActualCost > 0 && m_abilityController.PlayerController.CurrentManaAsInt <= 0))
		{
			StopAbility(abilityInterrupted: true);
			StopSlowdown();
			m_spellActive = false;
		}
		else
		{
			m_ticRateStartTime = Time.time;
			base.ApplyAbilityCosts();
		}
	}

	private void StopSlowdown()
	{
		RLTimeScale.SetTimeScale(TimeScaleType.TimeSlowAbility, 1f);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CharacterMove.WalkSpeed /= 5f;
		playerController.CharacterMove.MovementSpeed = playerController.CharacterMove.WalkSpeed;
		playerController.AscentMultiplierOverride /= 5f;
		playerController.FallMultiplierOverride /= 5f;
		playerController.Animator.speed /= 5f;
		playerController.CharacterDash.DashForce /= 5f;
		playerController.CharacterJump.JumpHeightMultiplier = 1f;
	}

	protected void FixedUpdate()
	{
		if (m_spellActive && Time.time >= m_ticRateStartTime + 0.5f)
		{
			ApplyAbilityCosts();
		}
	}
}
