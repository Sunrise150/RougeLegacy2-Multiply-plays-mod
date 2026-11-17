using System.Collections;
using UnityEngine;

public class TriggerDelaySkillCritProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private float m_skillCritTriggerDelay;

	[SerializeField]
	[Tooltip("Enabling this means that the projectile will crit BEFORE the time delay, and afterwards will only deal regular damage.")]
	private bool m_onlyCritBeforeDelay;

	[SerializeField]
	private GameObject m_critEffectGO;

	[SerializeField]
	private SpriteRenderer m_spriteRenderer;

	[SerializeField]
	private Color m_critTintColor;

	private Color m_initialTint;

	private void OnEnable()
	{
		if ((bool)m_spriteRenderer)
		{
			m_initialTint = m_spriteRenderer.color;
		}
		StartCoroutine(TriggerSkillCritCoroutine());
	}

	private IEnumerator TriggerSkillCritCoroutine()
	{
		float duration = Time.time + m_skillCritTriggerDelay;
		if (m_onlyCritBeforeDelay)
		{
			ApplyCrit();
			EnableCritEffects();
		}
		while (Time.time < duration)
		{
			yield return null;
		}
		if (m_onlyCritBeforeDelay)
		{
			UnapplyCrit();
			DisableCritEffects();
		}
		else
		{
			ApplyCrit();
			EnableCritEffects();
		}
	}

	private void ApplyCrit()
	{
		if (base.SourceProjectile.ActualCritChance < 100f)
		{
			base.SourceProjectile.ActualCritChance += 100f;
		}
	}

	private void UnapplyCrit()
	{
		if (base.SourceProjectile.ActualCritChance >= 100f)
		{
			base.SourceProjectile.ActualCritChance -= 100f;
		}
	}

	private void EnableCritEffects()
	{
		if ((bool)m_spriteRenderer)
		{
			m_spriteRenderer.color = m_critTintColor;
		}
		if ((bool)m_critEffectGO)
		{
			m_critEffectGO.SetActive(value: true);
		}
	}

	private void DisableCritEffects()
	{
		if ((bool)m_spriteRenderer)
		{
			m_spriteRenderer.color = m_initialTint;
		}
		if ((bool)m_critEffectGO)
		{
			m_critEffectGO.SetActive(value: false);
		}
	}

	private void OnDisable()
	{
		DisableCritEffects();
	}
}
