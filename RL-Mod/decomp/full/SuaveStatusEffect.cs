using System.Collections;
using UnityEngine;

public class SuaveStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Player_Suave;

	public override float StartingDurationOverride => 9999999f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		(m_charController as PlayerController).InitializeStrengthMods();
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		if (PlayerManager.IsInstantiated && !GameManager.IsApplicationClosing)
		{
			(m_charController as PlayerController).InitializeStrengthMods();
		}
	}
}
