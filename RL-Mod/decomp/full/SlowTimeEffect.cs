using System.Collections;
using UnityEngine;

public class SlowTimeEffect : BaseEffect
{
	private const float FIXED_DELTA_TIME = 1f / 60f;

	[SerializeField]
	private float m_timeScaleValue = 1f;

	private float m_slowDuration;

	private Coroutine m_slowCoroutine;

	private WaitRL_Yield m_waitYield;

	private TimeScaleType m_slowTimeScaleType;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
	}

	public override void Play(float duration, EffectStopType stopType)
	{
		base.Play(duration, stopType);
		if (SaveManager.ConfigData.DisableSlowdownOnHit)
		{
			PlayComplete();
			return;
		}
		m_slowDuration = duration;
		SlowTime();
	}

	public override void Stop(EffectStopType stopType)
	{
		if (base.IsPlaying)
		{
			RLTimeScale.SetTimeScale(m_slowTimeScaleType, 1f);
		}
		PlayComplete();
	}

	private void SlowTime()
	{
		if (m_slowCoroutine != null)
		{
			StopCoroutine(m_slowCoroutine);
			m_slowCoroutine = null;
		}
		if (m_slowDuration > 0f)
		{
			m_slowCoroutine = StartCoroutine(SlowTimeCoroutine());
		}
	}

	private IEnumerator SlowTimeCoroutine()
	{
		base.IsPlaying = true;
		_ = Time.time;
		bool useUnscaledTime = false;
		float num = m_slowDuration * m_timeScaleValue;
		if (num < 0.1f)
		{
			num = m_slowDuration;
			useUnscaledTime = true;
		}
		m_waitYield.CreateNew(num, useUnscaledTime);
		m_slowTimeScaleType = RLTimeScale.GetAvailableSlowTimeStack();
		RLTimeScale.SetTimeScale(m_slowTimeScaleType, m_timeScaleValue);
		yield return m_waitYield;
		RLTimeScale.SetTimeScale(m_slowTimeScaleType, 1f);
		PlayComplete();
	}

	protected override void OnDisable()
	{
		if (base.IsPlaying)
		{
			RLTimeScale.SetTimeScale(m_slowTimeScaleType, 1f);
		}
		base.OnDisable();
	}
}
