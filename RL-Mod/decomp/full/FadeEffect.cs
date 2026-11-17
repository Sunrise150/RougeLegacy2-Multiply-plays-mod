using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class FadeEffect : BaseEffect
{
	private enum FadeObjectType
	{
		CanvasGroup,
		Image,
		SpriteRenderer
	}

	[SerializeField]
	private FadeObjectType m_fadeObjType;

	[SerializeField]
	private float m_fadeInDuration;

	[SerializeField]
	private float m_stayDuration;

	[SerializeField]
	private float m_fadeOutDuration;

	[SerializeField]
	private bool m_fadeOutThenFadeIn;

	[SerializeField]
	private bool m_useUnscaledTime;

	private Tween m_fadeTween;

	private object m_fadeObj;

	private WaitRL_Yield m_waitYield;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
	}

	public override void Play(float duration = 0f, EffectStopType stopType = EffectStopType.Gracefully)
	{
		base.Play(duration, stopType);
		StopAllCoroutines();
		switch (m_fadeObjType)
		{
		case FadeObjectType.CanvasGroup:
		{
			CanvasGroup componentInChildren2 = GetComponentInChildren<CanvasGroup>();
			if (componentInChildren2 != null)
			{
				StartCoroutine(FadeTween(componentInChildren2, "alpha"));
			}
			break;
		}
		case FadeObjectType.Image:
		{
			Image componentInChildren3 = GetComponentInChildren<Image>();
			if (componentInChildren3 != null)
			{
				StartCoroutine(FadeTween(componentInChildren3, "color.a"));
			}
			break;
		}
		case FadeObjectType.SpriteRenderer:
		{
			SpriteRenderer componentInChildren = GetComponentInChildren<SpriteRenderer>();
			if (componentInChildren != null)
			{
				StartCoroutine(FadeTween(componentInChildren, "color.a"));
			}
			break;
		}
		}
	}

	private IEnumerator FadeTween(object fadeObj, string property)
	{
		m_fadeObj = fadeObj;
		float num = 1f;
		float fadeOutEndAlpha = 0f;
		float duration = m_fadeInDuration;
		float fadeOutDuration = m_fadeOutDuration;
		if (m_fadeOutThenFadeIn)
		{
			num = 0f;
			fadeOutEndAlpha = 1f;
			duration = m_fadeOutDuration;
			fadeOutDuration = m_fadeInDuration;
		}
		if (!m_useUnscaledTime)
		{
			m_fadeTween = TweenManager.TweenTo(fadeObj, duration, Ease.None, property, num);
			m_fadeTween.ID = "Fading";
			yield return m_fadeTween.TweenCoroutine;
			if (m_stayDuration > 0f)
			{
				m_waitYield.CreateNew(m_stayDuration, m_useUnscaledTime);
				yield return m_waitYield;
			}
			m_fadeTween = TweenManager.TweenTo(fadeObj, fadeOutDuration, Ease.None, property, fadeOutEndAlpha);
			m_fadeTween.ID = "Fading";
			yield return m_fadeTween.TweenCoroutine;
		}
		else
		{
			m_fadeTween = TweenManager.TweenTo_UnscaledTime(fadeObj, duration, Ease.None, property, num);
			m_fadeTween.ID = "Fading";
			yield return m_fadeTween.TweenCoroutine;
			if (m_stayDuration > 0f)
			{
				m_waitYield.CreateNew(m_stayDuration, m_useUnscaledTime);
				yield return m_waitYield;
			}
			m_fadeTween = TweenManager.TweenTo_UnscaledTime(fadeObj, fadeOutDuration, Ease.None, property, fadeOutEndAlpha);
			m_fadeTween.ID = "Fading";
			yield return m_fadeTween.TweenCoroutine;
		}
		Stop(EffectStopType.Gracefully);
	}

	public override void Stop(EffectStopType stopType)
	{
		if (m_fadeTween != null)
		{
			m_fadeTween.StopTweenWithConditionChecks(runEndHandlers: false, m_fadeObj, "Fading");
		}
		PlayComplete();
	}
}
