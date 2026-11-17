using System.Collections;
using UnityEngine;

public class BurnImmunityStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Burn_Immunity;

	public override float StartingDurationOverride => 0f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.BurnImmunity, base.Duration);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}
}
