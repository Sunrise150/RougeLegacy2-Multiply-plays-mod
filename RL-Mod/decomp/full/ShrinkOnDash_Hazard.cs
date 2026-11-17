using System;
using System.Collections;
using UnityEngine;

public class ShrinkOnDash_Hazard : Hazard
{
	private float m_initialScale;

	private WaitRL_Yield m_waitYield;

	private Tween m_shrinkTween;

	private Tween m_growTween;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDash;

	protected override void Awake()
	{
		base.Awake();
		m_onPlayerDash = OnPlayerDash;
		m_waitYield = new WaitRL_Yield(0f);
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDash, m_onPlayerDash);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDash, m_onPlayerDash);
	}

	public override void Initialize(HazardArgs hazardArgs)
	{
		base.InitialState = hazardArgs.InitialState;
		switch (base.InitialState)
		{
		case StateID.One:
			m_initialScale = 6f;
			break;
		case StateID.Two:
			m_initialScale = 6f;
			break;
		case StateID.Random:
			if (CDGHelper.RandomPlusMinus() > 0)
			{
				m_initialScale = 6f;
			}
			else
			{
				m_initialScale = 6f;
			}
			break;
		}
		base.transform.localScale = new Vector3(m_initialScale, m_initialScale, base.transform.localScale.z);
	}

	private void OnPlayerDash(MonoBehaviour sender, EventArgs args)
	{
		StopShrinkCoroutine();
		StartCoroutine(ShrinkCoroutine());
	}

	private void StopShrinkCoroutine()
	{
		StopAllCoroutines();
		if (m_shrinkTween != null)
		{
			m_shrinkTween.StopTweenWithConditionChecks(runEndHandlers: false, base.transform, "ShrinkOnDash_Shrink");
		}
		if (m_growTween != null)
		{
			m_growTween.StopTweenWithConditionChecks(runEndHandlers: false, base.transform, "ShrinkOnDash_Grow");
		}
	}

	private IEnumerator ShrinkCoroutine()
	{
		float num = 0.25f;
		m_shrinkTween = TweenManager.TweenTo(base.transform, 0.25f, Ease.Quad.EaseInOut, "localScale.x", num, "localScale.y", num);
		m_shrinkTween.ID = "ShrinkOnDash_Shrink";
		yield return m_shrinkTween.TweenCoroutine;
		m_waitYield.CreateNew(2f);
		yield return m_waitYield;
		float duration = m_initialScale / 1.5f;
		m_growTween = TweenManager.TweenTo(base.transform, duration, Ease.Quad.EaseInOut, "localScale.x", m_initialScale, "localScale.y", m_initialScale);
		m_growTween.ID = "ShrinkOnDash_Grow";
		yield return m_growTween.TweenCoroutine;
	}

	public override void ResetHazard()
	{
		StopShrinkCoroutine();
		base.transform.localScale = new Vector3(m_initialScale, m_initialScale, base.transform.localScale.z);
	}
}
