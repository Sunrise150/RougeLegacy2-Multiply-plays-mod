using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class WaitUntilXRemaining_SummonRule : BaseSummonRule
{
	[SerializeField]
	private int m_numEnemiesRemaining;

	private WaitUntil m_waitYield;

	public override SummonRuleType RuleType => SummonRuleType.WaitUntilXRemaining;

	public override string RuleLabel => "Wait Until X Remaining";

	public override IEnumerator RunSummonRule()
	{
		if (m_waitYield == null)
		{
			m_waitYield = new WaitUntil(() => EnemyManager.NumActiveSummonedEnemies <= m_numEnemiesRemaining);
		}
		yield return m_waitYield;
		base.IsRuleComplete = true;
	}
}
