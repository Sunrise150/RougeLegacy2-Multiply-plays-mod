using System.Collections;
using UnityEngine;

public class SelfAnimateEffect : BaseEffect
{
	[SerializeField]
	private string m_movingLeftParamStart;

	[SerializeField]
	private string m_movingLeftParamEnd;

	[SerializeField]
	private string m_movingRightParamStart;

	[SerializeField]
	private string m_movingRightParamEnd;

	[SerializeField]
	private string m_movingUpParamStart;

	[SerializeField]
	private string m_movingUpParamEnd;

	[SerializeField]
	private string m_movingDownParamStart;

	[SerializeField]
	private string m_movingDownParamEnd;

	[Space(10f)]
	[SerializeField]
	private bool m_runOnlyOnce;

	private WaitRL_Yield m_waitYield;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f, useUnscaledTime: true);
	}

	public override void Play(float duration = 0f, EffectStopType stopType = EffectStopType.Gracefully)
	{
		base.Play(duration, stopType);
		if ((bool)base.Source)
		{
			if ((bool)base.SourceAnimator)
			{
				StartCoroutine(PlayTimedAnimator(duration, stopType));
			}
			else
			{
				Debug.Log("<color=yellow>Cannot Trigger SelfAnimate Effect.  Source does not have animator.</color>");
			}
		}
	}

	private IEnumerator PlayTimedAnimator(float duration, EffectStopType stopType)
	{
		string text = "";
		string endParamName = "";
		if ((base.EffectDirection & EffectTriggerDirection.MovingDown) != 0)
		{
			text = m_movingDownParamStart;
			endParamName = m_movingDownParamEnd;
		}
		else if ((base.EffectDirection & EffectTriggerDirection.MovingUp) != 0)
		{
			text = m_movingUpParamStart;
			endParamName = m_movingUpParamEnd;
		}
		if ((base.EffectDirection & EffectTriggerDirection.MovingLeft) != 0)
		{
			text = m_movingLeftParamStart;
			endParamName = m_movingLeftParamEnd;
		}
		else if ((base.EffectDirection & EffectTriggerDirection.MovingRight) != 0)
		{
			text = m_movingRightParamStart;
			endParamName = m_movingRightParamEnd;
		}
		if (string.IsNullOrEmpty(text))
		{
			text = m_movingLeftParamStart;
		}
		if (string.IsNullOrEmpty(endParamName))
		{
			endParamName = m_movingLeftParamEnd;
		}
		if (!string.IsNullOrEmpty(text))
		{
			base.SourceAnimator.SetTrigger(text);
		}
		if (stopType == EffectStopType.Gracefully)
		{
			yield return null;
			AnimatorStateInfo currentAnimatorStateInfo = base.SourceAnimator.GetCurrentAnimatorStateInfo(base.AnimatorLayer);
			float num = currentAnimatorStateInfo.length * (1f - currentAnimatorStateInfo.normalizedTime);
			if (duration > num)
			{
				num = duration;
			}
			m_waitYield.CreateNew(num);
		}
		else
		{
			m_waitYield.CreateNew(duration);
		}
		yield return m_waitYield;
		if (!string.IsNullOrEmpty(endParamName))
		{
			base.SourceAnimator.SetTrigger(endParamName);
		}
		PlayComplete();
	}

	public override void Stop(EffectStopType stopType)
	{
		string text = "";
		switch (base.EffectDirection)
		{
		case EffectTriggerDirection.Anywhere:
		case EffectTriggerDirection.MovingLeft:
		case EffectTriggerDirection.StandingOn:
			_ = m_movingLeftParamStart;
			text = m_movingLeftParamEnd;
			break;
		case EffectTriggerDirection.MovingRight:
			_ = m_movingRightParamStart;
			text = m_movingRightParamEnd;
			break;
		case EffectTriggerDirection.MovingUp:
			_ = m_movingUpParamStart;
			text = m_movingUpParamEnd;
			break;
		case EffectTriggerDirection.MovingDown:
			_ = m_movingDownParamStart;
			text = m_movingDownParamEnd;
			break;
		}
		if ((bool)base.SourceAnimator && !string.IsNullOrEmpty(text))
		{
			base.SourceAnimator.SetTrigger(text);
		}
		PlayComplete();
	}
}
