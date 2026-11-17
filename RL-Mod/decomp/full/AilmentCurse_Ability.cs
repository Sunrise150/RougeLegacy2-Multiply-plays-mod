using System.Collections;
using FMODUnity;
using UnityEngine;

public class AilmentCurse_Ability : BaseAbility_RL, ISpell, IAbility
{
	[SerializeField]
	private StudioEventEmitter m_chargeEmitter;

	protected override float TellIntroAnimSpeed => 1.5f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0.375f;

	protected override float AttackIntroAnimSpeed => 1.1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1.1f;

	protected override float AttackAnimExitDelay => 0f;

	protected override float ExitAnimSpeed => 2f;

	protected override float ExitAnimExitDelay => 0f;

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		m_chargeEmitter.Play();
	}

	public override IEnumerator CastAbility()
	{
		m_abilityController.PlayerController.PauseGravity(stopVelocity: true, lockPlayerControls: true);
		yield return base.CastAbility();
	}

	protected override void FireProjectile()
	{
		m_chargeEmitter.Stop();
		base.FireProjectile();
		m_abilityController.PlayerController.StartCoroutine(TimeStop());
	}

	private IEnumerator TimeStop()
	{
		TimeScaleType timeScaleType = RLTimeScale.GetAvailableSlowTimeStack();
		RLTimeScale.SetTimeScale(timeScaleType, 0.1f);
		float timeDuration = Time.unscaledTime + 0.1f;
		while (Time.unscaledTime < timeDuration)
		{
			yield return null;
		}
		RLTimeScale.SetTimeScale(timeScaleType, 1f);
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_chargeEmitter.Stop();
		m_abilityController.PlayerController.ResumeGravity();
		base.StopAbility(abilityInterrupted);
	}
}
