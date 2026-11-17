using System.Collections;
using UnityEngine;

public class MapIconAnimationCurve : MonoBehaviour
{
	[SerializeField]
	private bool m_overrideMapControllerValues;

	[SerializeField]
	private bool m_animateOnEnable;

	[SerializeField]
	[ConditionalHide("m_overrideMapControllerValues", true)]
	private AnimationCurve m_animCurve;

	[SerializeField]
	[ConditionalHide("m_overrideMapControllerValues", true)]
	private float m_animLoopdelay = 1f;

	private void OnEnable()
	{
		if (m_animateOnEnable)
		{
			if (m_overrideMapControllerValues)
			{
				StartCoroutine(AnimationCoroutine());
			}
			else
			{
				StartCoroutine(GlobalAnimationCoroutine());
			}
		}
	}

	private void OnBecameVisible()
	{
		if (base.enabled)
		{
			if (m_overrideMapControllerValues)
			{
				StartCoroutine(AnimationCoroutine());
			}
			else
			{
				StartCoroutine(GlobalAnimationCoroutine());
			}
		}
	}

	private void OnBecameInvisible()
	{
		if (base.enabled)
		{
			StopAllCoroutines();
		}
	}

	private IEnumerator GlobalAnimationCoroutine()
	{
		while (true)
		{
			float num = 1f + MapController.MapIconAnimationCurveValue;
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
	}

	private IEnumerator AnimationCoroutine()
	{
		base.transform.localScale = Vector3.one;
		float startTime = Time.unscaledTime;
		float elapsedPercent = 0f;
		float duration = m_animCurve.keys[m_animCurve.keys.Length - 1].time;
		while (elapsedPercent < 1f)
		{
			elapsedPercent = (Time.unscaledTime - startTime) / duration;
			float num = 1f + m_animCurve.Evaluate(elapsedPercent);
			base.transform.localScale = new Vector3(num, num, num);
			yield return null;
		}
		float delay = Time.unscaledTime + m_animLoopdelay;
		while (Time.unscaledTime < delay)
		{
			yield return null;
		}
		StartCoroutine(AnimationCoroutine());
	}
}
