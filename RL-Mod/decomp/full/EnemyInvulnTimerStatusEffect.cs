using System.Collections;
using UnityEngine;

public class EnemyInvulnTimerStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_InvulnTimer;

	public override float StartingDurationOverride => 5f;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.InvulnTimer, base.Duration);
		m_statusEffectController.AddStatusEffectInvulnStack(StatusEffectType);
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		m_statusEffectController.RemoveStatusEffectInvulnStack(StatusEffectType);
	}
}
