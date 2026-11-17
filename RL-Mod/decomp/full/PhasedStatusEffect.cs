using System.Collections;
using UnityEngine;

public class PhasedStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Phased;

	public override float StartingDurationOverride => 3f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: false);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		m_charController.HitboxController.SetHitboxActiveState(HitboxType.Weapon, active: true);
	}
}
