using System.Collections;
using UnityEngine;

public class CharacterHitCritical_TextPopup : TextPopupObj
{
	private static bool FLIP_XPOS_STATIC;

	[SerializeField]
	private SpriteRenderer m_twoNumberBacking;

	[SerializeField]
	private SpriteRenderer m_threeNumberBacking;

	[SerializeField]
	private SpriteRenderer m_fourNumberBacking;

	protected override IEnumerator SpawnEffectCoroutine()
	{
		int length = base.Text.Replace("-", "").Length;
		if (length <= 2)
		{
			SetBackingActive(m_twoNumberBacking);
		}
		else if (length <= 3)
		{
			SetBackingActive(m_threeNumberBacking);
		}
		else
		{
			SetBackingActive(m_fourNumberBacking);
		}
		float num = Random.Range(0.25f, 1f);
		if (FLIP_XPOS_STATIC)
		{
			num *= -1f;
		}
		FLIP_XPOS_STATIC = !FLIP_XPOS_STATIC;
		float y = Random.Range(-0.5f, 0.5f);
		base.transform.Translate(num, y, 0f);
		float delay = m_animationClipDuration + Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		base.gameObject.SetActive(value: false);
	}

	private void SetBackingActive(SpriteRenderer backing)
	{
		if (m_twoNumberBacking != backing && m_twoNumberBacking.gameObject.activeSelf)
		{
			m_twoNumberBacking.gameObject.SetActive(value: false);
		}
		if (m_threeNumberBacking != backing && m_threeNumberBacking.gameObject.activeSelf)
		{
			m_threeNumberBacking.gameObject.SetActive(value: false);
		}
		if (m_fourNumberBacking != backing && m_fourNumberBacking.gameObject.activeSelf)
		{
			m_fourNumberBacking.gameObject.SetActive(value: false);
		}
		if (!backing.gameObject.activeSelf)
		{
			backing.gameObject.SetActive(value: true);
		}
	}
}
