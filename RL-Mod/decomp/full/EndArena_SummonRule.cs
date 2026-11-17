using System;
using System.Collections;

[Serializable]
public class EndArena_SummonRule : BaseSummonRule
{
	public override SummonRuleType RuleType => SummonRuleType.EndArena;

	public override string RuleLabel => "End Arena";

	public override IEnumerator RunSummonRule()
	{
		EnemyManager.DisableAllSummonedEnemies();
		base.IsRuleComplete = true;
		base.SummonController.StopArena(arenaComplete: true);
		yield break;
	}
}
