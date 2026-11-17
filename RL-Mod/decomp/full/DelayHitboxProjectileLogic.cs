using System.Collections;
using UnityEngine;

public class DelayHitboxProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private HitboxType m_hitboxToDelay;

	[SerializeField]
	private float m_duration;

	private bool m_hitboxDisabled;

	private void OnEnable()
	{
		StartCoroutine(DelayCoroutine());
	}

	private IEnumerator DelayCoroutine()
	{
		m_hitboxDisabled = true;
		base.SourceProjectile.HitboxController.SetHitboxActiveState(m_hitboxToDelay, active: false);
		float delay = Time.time + m_duration;
		while (Time.time < delay)
		{
			yield return null;
		}
		m_hitboxDisabled = false;
		base.SourceProjectile.HitboxController.SetHitboxActiveState(m_hitboxToDelay, active: true);
	}

	private void OnDisable()
	{
		if (m_hitboxDisabled && (bool)base.SourceProjectile)
		{
			base.SourceProjectile.HitboxController.SetHitboxActiveState(m_hitboxToDelay, active: true);
		}
		m_hitboxDisabled = false;
	}
}
