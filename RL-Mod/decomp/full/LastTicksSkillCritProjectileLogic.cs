using System.Collections;
using UnityEngine;

public class LastTicksSkillCritProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	[Tooltip("The lifespan of a projectile is not reliable, because it is often tied to how long the ability is cast. Therefore you need to manually count how many ticks and input it here.")]
	private int m_projectileNumTicks;

	[SerializeField]
	private int m_lastTicksTrigger;

	private void OnEnable()
	{
		StartCoroutine(TriggerSkillCritCoroutine());
	}

	private IEnumerator TriggerSkillCritCoroutine()
	{
		float repeatHitDuration = base.SourceProjectile.HitboxController.RepeatHitDuration;
		float num = 0f;
		int num2;
		if (base.SourceProjectile.HasIntro)
		{
			num2 = m_projectileNumTicks - m_lastTicksTrigger;
		}
		else
		{
			num2 = m_projectileNumTicks - 1 - m_lastTicksTrigger;
			num = repeatHitDuration / 2f;
		}
		float triggerStartTime2 = repeatHitDuration * (float)num2;
		triggerStartTime2 += Time.time + num;
		while (Time.time < triggerStartTime2)
		{
			yield return null;
		}
		if (base.SourceProjectile.ActualCritChance < 100f)
		{
			base.SourceProjectile.ActualCritChance += 100f;
		}
	}
}
