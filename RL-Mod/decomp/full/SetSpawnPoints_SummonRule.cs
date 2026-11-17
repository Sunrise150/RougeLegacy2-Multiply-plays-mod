using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SetSpawnPoints_SummonRule : BaseSummonRule
{
	[SerializeField]
	private int[] m_spawnPointArray = new int[0];

	public override SummonRuleType RuleType => SummonRuleType.SetSpawnPoints;

	public override Color BoxColor => Color.cyan;

	public override string RuleLabel => "Set Spawn Points";

	public override IEnumerator RunSummonRule()
	{
		base.SummonController.SpawnPoints.Clear();
		base.SummonController.SpawnPoints.AddRange(m_spawnPointArray);
		base.SummonController.AvailableSpawnPoints.Clear();
		base.SummonController.AvailableSpawnPoints.AddRange(base.SummonController.SpawnPoints);
		base.IsRuleComplete = true;
		yield break;
	}
}
