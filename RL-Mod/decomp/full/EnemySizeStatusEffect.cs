using System.Collections;
using UnityEngine;

public class EnemySizeStatusEffect : BaseStatusEffect
{
	public override StatusEffectType StatusEffectType => StatusEffectType.Enemy_Size;

	public override float StartingDurationOverride => float.MaxValue;

	protected override IEnumerator StartEffectCoroutine(IDamageObj caster, bool justCasted)
	{
		m_charController.StatusBarController.ApplyUIEffect(StatusBarEntryType.Size);
		EnemyController enemyController = m_charController as EnemyController;
		if ((bool)enemyController)
		{
			enemyController.InitializeLevelMods();
			enemyController.UpdateEnemyDataScale();
			int num = Mathf.CeilToInt((float)(enemyController.BaseMaxHealth + enemyController.MaxHealthAdd + enemyController.MaxHealthTemporaryAdd) * 2f);
			enemyController.SetHealth(num, additive: true, runEvents: false);
		}
		while (Time.time < base.EndTime)
		{
			yield return null;
		}
		StopEffect();
	}

	public override void StopEffect(bool interrupted = false)
	{
		base.StopEffect(interrupted);
		EnemyController enemyController = m_charController as EnemyController;
		if ((bool)enemyController)
		{
			enemyController.UpdateEnemyDataScale();
		}
	}
}
