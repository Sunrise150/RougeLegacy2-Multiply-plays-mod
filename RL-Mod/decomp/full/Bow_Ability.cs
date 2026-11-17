using System.Collections;
using RLAudio;
using UnityEngine;

public class Bow_Ability : AimedAbility_RL, IAttack, IAbility, IAudioEventEmitter
{
	protected override float TellIntroAnimSpeed => 2f;

	protected override float TellIntroAnimExitDelay => 0f;

	protected override float TellAnimSpeed => 1f;

	protected override float TellAnimExitDelay => 0f;

	protected override float AttackIntroAnimSpeed => 1f;

	protected override float AttackIntroAnimExitDelay => 0f;

	protected override float AttackAnimSpeed => 1f;

	protected override float AttackAnimExitDelay => 0.08f;

	protected override float ExitAnimSpeed => 1.25f;

	protected override float ExitAnimExitDelay => 0f;

	protected virtual float GravityReduction => 0.425f;

	protected virtual Vector2 BowPushbackAmount => new Vector2(4.5f, 13.5f);

	public override Vector2 PushbackAmount => BowPushbackAmount;

	public override void Initialize(CastAbility_RL abilityController, CastAbilityType castAbilityType)
	{
		base.Initialize(abilityController, castAbilityType);
		m_gravityReductionModWhenAiming = GravityReduction;
	}

	public override void PreCastAbility()
	{
		base.PreCastAbility();
		m_abilityController.PlayerController.LookController.ArrowGeo.gameObject.SetActive(value: true);
	}

	protected override IEnumerator ChangeAnim(float duration)
	{
		if (base.CurrentAbilityAnimState == AbilityAnimState.Attack_Intro)
		{
			Color startColor = new Color(1f, 1f, 1f, 1f);
			m_aimLine.startColor = startColor;
		}
		yield return base.ChangeAnim(duration);
	}

	protected override void Update()
	{
		base.Update();
		if ((bool)m_critEffectGO)
		{
			Vector3 localEulerAngles = m_critEffectGO.transform.localEulerAngles;
			localEulerAngles.z = m_aimAngle;
			m_critEffectGO.transform.localEulerAngles = localEulerAngles;
		}
	}

	public override void StopAbility(bool abilityInterrupted)
	{
		m_abilityController.PlayerController.LookController.ArrowGeo.gameObject.SetActive(value: false);
		base.StopAbility(abilityInterrupted);
	}

	protected override void PlayCritWindowAudio()
	{
	}
}
