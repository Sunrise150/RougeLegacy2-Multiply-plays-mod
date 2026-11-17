using System.Collections;
using UnityEngine;

public class DizzyStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Dizzy;

	public override float StartingDurationOverride => 3f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Dizzy, base.Duration);
		(m_charController as EnemyController).LogicController.StopAllLogic(resetCurrentState: true);
		m_charController.Animator.Play("Neutral");
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}
}
