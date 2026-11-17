using System.Collections;
using UnityEngine;

public class DanceStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Player_Dance;

	public override float StartingDurationOverride => 99999f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		base.TimesStacked = Mathf.Clamp(base.TimesStacked, 0, 5);
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Dance, 5, base.TimesStacked);
		while (!m_charController.IsGrounded)
		{
			yield return null;
		}
		base.TimesStacked = 0;
		StopEffect();
	}
}
