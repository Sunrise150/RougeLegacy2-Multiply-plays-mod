using System.Collections;
using UnityEngine;

public class ObjectShakeEffect : BaseEffect
{
	[SerializeField]
	private float m_shakeAmplitude = 5f;

	private Vector3 m_startingPos;

	private Transform m_sourceTransform;

	private RectTransform m_sourceRectTransform;

	private bool UseRectTransform => m_sourceRectTransform != null;

	public override void Play(float duration = 0f, EffectStopType stopType = EffectStopType.Gracefully)
	{
		base.Play(duration, stopType);
		StartCoroutine(PlayTimedObjectShake(duration));
	}

	private IEnumerator PlayTimedObjectShake(float duration)
	{
		m_sourceRectTransform = base.Source.GetComponent<RectTransform>();
		m_sourceTransform = base.Source.transform;
		if (UseRectTransform)
		{
			m_startingPos = m_sourceRectTransform.anchoredPosition;
			float startingTime = Time.unscaledTime;
			while (Time.unscaledTime < startingTime + duration)
			{
				m_sourceRectTransform.anchoredPosition = m_startingPos + Random.insideUnitSphere * m_shakeAmplitude;
				yield return null;
			}
		}
		else
		{
			m_startingPos = m_sourceTransform.localPosition;
			float startingTime = Time.unscaledTime;
			while (Time.unscaledTime < startingTime + duration)
			{
				m_sourceTransform.localPosition = m_startingPos + Random.insideUnitSphere * m_shakeAmplitude;
				yield return null;
			}
		}
		Stop(EffectStopType.Gracefully);
	}

	public override void Stop(EffectStopType stopType)
	{
		StopAllCoroutines();
		if (UseRectTransform)
		{
			m_sourceRectTransform.anchoredPosition = m_startingPos;
		}
		else
		{
			m_sourceTransform.localPosition = m_startingPos;
		}
		PlayComplete();
	}
}
