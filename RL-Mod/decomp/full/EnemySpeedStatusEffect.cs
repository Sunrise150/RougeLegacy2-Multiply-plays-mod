using System.Collections;
using UnityEngine;

public class EnemySpeedStatusEffect : BaseStatusEffect
{
	private Vector3 m_storedScale;

	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Speed;

	public override float StartingDurationOverride => float.MaxValue;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Speed);
		m_charController.ResetBaseValues();
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		m_charController.ResetBaseValues();
	}
}
