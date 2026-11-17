using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SetSummonPoolDifficulty_SummonRule : BaseSummonRule
{
	[SerializeField]
	private EnemyRank m_summonPoolDifficulty;

	public EnemyRank SummonPoolDifficulty => m_summonPoolDifficulty;

	public override SummonRuleType RuleType => SummonRuleType.SetSummonPoolDifficulty;

	public override string RuleLabel => "Set Summon Pool Difficulty";

	public override IEnumerator RunSummonRule()
	{
		base.SummonController.SummonDifficultyOverride = m_summonPoolDifficulty;
		base.IsRuleComplete = true;
		yield break;
	}
}
