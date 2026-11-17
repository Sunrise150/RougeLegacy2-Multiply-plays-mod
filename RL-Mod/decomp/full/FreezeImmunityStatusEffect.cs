using System.Collections;
using UnityEngine;

public class FreezeImmunityStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_FreezeImmunity;

	public override float StartingDurationOverride => 0f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.FreezeImmunity, base.Duration);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}
}
