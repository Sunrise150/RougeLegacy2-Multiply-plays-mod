using System.Collections;
using UnityEngine;

public class MagicBreakStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_MagicBreak;

	public override float StartingDurationOverride => 2.5f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.MagicBreak, base.Duration);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}
}
