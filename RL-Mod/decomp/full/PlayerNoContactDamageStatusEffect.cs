using System.Collections;
using UnityEngine;

public class PlayerNoContactDamageStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Player_NoContactDamage;

	public override float StartingDurationOverride => 9999999f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}
}
