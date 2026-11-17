using System.Collections;
using UnityEngine;

public class ArmorBreakStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_ArmorBreak;

	public override float StartingDurationOverride => 2.5f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.ArmorBreak, base.Duration);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}
}
