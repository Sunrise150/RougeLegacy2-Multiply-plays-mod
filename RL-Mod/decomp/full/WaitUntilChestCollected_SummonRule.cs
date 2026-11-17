using System.Collections;
using UnityEngine;

public class WaitUntilChestCollected_SummonRule : BaseSummonRule
{
	public override SummonRuleType RuleType => SummonRuleType.WaitUntilChestCollected;

	public override string RuleLabel => "Wait Until Chest Collected";

	public override IEnumerator RunSummonRule()
	{
		ChestSpawnController chestSpawner = ((base.SerializedObject != null) ? (base.SerializedObject as ChestSpawnController) : null);
		if ((bool)chestSpawner)
		{
			while (!chestSpawner.ChestInstance || !chestSpawner.ChestInstance.IsOpen)
			{
				yield return null;
			}
		}
		float delayTime = Time.time + 0.5f;
		while (Time.time < delayTime)
		{
			yield return null;
		}
		while (ItemDropManager.HasActiveItemDrops)
		{
			yield return null;
		}
		base.IsRuleComplete = true;
	}
}
