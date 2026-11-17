using System.Collections;
using UnityEngine;

public class SlowTime_SummonRule : BaseSummonRule
{
	[SerializeField]
	private float m_slowAmount;

	[SerializeField]
	private float m_slowDuration;

	public override Color BoxColor => Color.yellow;

	public override SummonRuleType RuleType => SummonRuleType.SlowTime;

	public override string RuleLabel => "Slow Time";

	public override IEnumerator RunSummonRule()
	{
		TimeScaleType timeScaleType = RLTimeScale.GetAvailableSlowTimeStack();
		RLTimeScale.SetTimeScale(timeScaleType, m_slowAmount);
		float slowDuration = Time.time + m_slowDuration * m_slowAmount;
		while (Time.time < slowDuration)
		{
			yield return null;
		}
		RLTimeScale.SetTimeScale(timeScaleType, 1f);
		base.IsRuleComplete = true;
	}
}
