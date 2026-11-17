using System.Collections;

public class WaitUntilModeShift_SummonRule : BaseSummonRule
{
	public override SummonRuleType RuleType => SummonRuleType.WaitUntilModeShift;

	public override string RuleLabel => "Wait Until ModeShift";

	public override IEnumerator RunSummonRule()
	{
		bool inModeShift = false;
		BaseRoom room = PlayerManager.GetCurrentPlayerRoom();
		while (!inModeShift)
		{
			EnemySpawnController[] enemySpawnControllers = room.SpawnControllerManager.EnemySpawnControllers;
			foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
			{
				if ((bool)enemySpawnController && (bool)enemySpawnController.EnemyInstance && enemySpawnController.EnemyInstance.ModeshiftDamageMod != 1f)
				{
					inModeShift = true;
					break;
				}
			}
			yield return null;
		}
		base.IsRuleComplete = true;
	}
}
