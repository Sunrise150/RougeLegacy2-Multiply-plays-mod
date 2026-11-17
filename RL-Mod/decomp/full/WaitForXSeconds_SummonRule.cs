using System.Collections;
using UnityEngine;

public class WaitForXSeconds_SummonRule : BaseSummonRule
{
	[SerializeField]
	private float m_waitDuration;

	private WaitRL_Yield m_waitYield;

	public override SummonRuleType RuleType => SummonRuleType.WaitForXSeconds;

	public override string RuleLabel => "Wait for X Seconds";

	public override IEnumerator RunSummonRule()
	{
		if (m_waitYield == null)
		{
			m_waitYield = new WaitRL_Yield(m_waitDuration);
		}
		else
		{
			m_waitYield.CreateNew(m_waitDuration);
		}
		yield return m_waitYield;
		base.IsRuleComplete = true;
	}
}
