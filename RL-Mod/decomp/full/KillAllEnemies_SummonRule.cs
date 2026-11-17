using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class KillAllEnemies_SummonRule : BaseSummonRule
{
	[SerializeField]
	private bool m_killAllNonSummonedEnemies = true;

	[SerializeField]
	private bool m_killAllSummonedEnemies = true;

	public override SummonRuleType RuleType => SummonRuleType.KillAllEnemies;

	public override Color BoxColor => Color.blue;

	public override string RuleLabel => "Kill All Enemies";

	public override IEnumerator RunSummonRule()
	{
		if (m_killAllNonSummonedEnemies)
		{
			EnemyManager.KillAllNonSummonedEnemies();
		}
		if (m_killAllSummonedEnemies)
		{
			EnemyManager.KillAllSummonedEnemies();
		}
		base.IsRuleComplete = true;
		yield break;
	}
}
