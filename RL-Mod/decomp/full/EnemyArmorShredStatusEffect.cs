using System.Collections;
using UnityEngine;

public class EnemyArmorShredStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_ArmorShred;

	public override float StartingDurationOverride => float.MaxValue;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.ArmorShred, base.Duration);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}
}
