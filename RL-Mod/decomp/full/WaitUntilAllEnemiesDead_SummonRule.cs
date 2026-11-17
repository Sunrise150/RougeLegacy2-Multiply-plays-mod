using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class WaitUntilAllEnemiesDead_SummonRule : BaseSummonRule
{
	[SerializeField]
	private bool m_checkForNonSummonedEnemiesToo;

	public override SummonRuleType RuleType => SummonRuleType.WaitUntilAllEnemiesDead;

	public override Color BoxColor => Color.blue;

	public override string RuleLabel => "Wait Until All Enemies Dead";

	public override IEnumerator RunSummonRule()
	{
		while (EnemyManager.NumActiveSummonedEnemies > 0)
		{
			yield return null;
		}
		if (m_checkForNonSummonedEnemiesToo)
		{
			while (EnemyManager.NumActiveEnemies > 0)
			{
				yield return null;
			}
		}
		base.IsRuleComplete = true;
	}
}
