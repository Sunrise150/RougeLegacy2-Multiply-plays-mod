using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SetSummonPool_SummonRule : BaseSummonRule
{
	public static EnemyType[] SummonExceptionArray = new EnemyType[2]
	{
		EnemyType.BouncySpike,
		EnemyType.Eggplant
	};

	[SerializeField]
	private bool m_poolIsBiomeSpecific;

	[SerializeField]
	private bool m_spawnFlyingOnly;

	[SerializeField]
	private EnemyTypeAndRank[] m_enemiesToSummonArray = new EnemyTypeAndRank[0];

	public override SummonRuleType RuleType => SummonRuleType.SetSummonPool;

	public EnemyTypeAndRank[] EnemiesToSummonArray => m_enemiesToSummonArray;

	public bool PoolIsBiomeSpecific => m_poolIsBiomeSpecific;

	public override Color BoxColor => Color.cyan;

	public override string RuleLabel => "Set Summon Pool";

	public override IEnumerator RunSummonRule()
	{
		base.SummonController.SummonPool.Clear();
		while (!PlayerManager.IsInstantiated || PlayerManager.GetCurrentPlayerRoom() == null)
		{
			yield return null;
		}
		base.SummonController.PoolIsBiomeSpecific = m_poolIsBiomeSpecific;
		if (m_poolIsBiomeSpecific)
		{
			List<EnemyTypeAndRank> allEnemiesInBiome = EnemyUtility.GetAllEnemiesInBiome(PlayerManager.GetCurrentPlayerRoom().BiomeType, m_spawnFlyingOnly);
			for (int i = 0; i < allEnemiesInBiome.Count; i++)
			{
				if (SummonExceptionArray.Contains(allEnemiesInBiome[i].Type))
				{
					allEnemiesInBiome.RemoveAt(i);
					i--;
				}
			}
			base.SummonController.SummonPool.AddRange(allEnemiesInBiome);
		}
		else
		{
			base.SummonController.SummonPool.AddRange(m_enemiesToSummonArray);
		}
		base.IsRuleComplete = true;
	}
}
