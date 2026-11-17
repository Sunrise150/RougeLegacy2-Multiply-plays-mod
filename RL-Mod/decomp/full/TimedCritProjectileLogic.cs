using System.Collections;
using UnityEngine;

public class TimedCritProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private Vector2 m_critTimingWindow;

	private bool m_critApplied;

	private void OnEnable()
	{
		m_critApplied = false;
		StartCoroutine(TimingWindowCoroutine());
	}

	private IEnumerator TimingWindowCoroutine()
	{
		float critWindow = Time.time + m_critTimingWindow.x;
		float endTime = Time.time + m_critTimingWindow.y;
		while (Time.time < endTime)
		{
			if (Time.time >= critWindow && base.SourceProjectile.ActualCritChance < 100f && !m_critApplied)
			{
				base.SourceProjectile.ActualCritChance += 100f;
				base.SourceProjectile.ChangeSpriteRendererColor(ProjectileLibrary.GetBuffColor(ProjectileBuffType.PlayerDashAttack));
				m_critApplied = true;
			}
			yield return null;
		}
		if (m_critApplied)
		{
			if (base.SourceProjectile.ActualCritChance >= 100f)
			{
				base.SourceProjectile.ActualCritChance -= 100f;
				base.SourceProjectile.ResetSpriteRendererColor();
			}
			m_critApplied = false;
		}
	}
}
