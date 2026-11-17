using System;
using System.Collections;
using UnityEngine;

[Serializable]
public class SetEnemiesDefeated_SummonRule : BaseSummonRule
{
	[SerializeField]
	private EnemyType m_enemyTypeDefeated;

	[SerializeField]
	private EnemyRank m_enemyRankDefeated;

	[SerializeField]
	private int m_timesDefeated;

	[SerializeField]
	private bool m_additive = true;

	public override SummonRuleType RuleType => SummonRuleType.SetEnemiesDefeated;

	public override string RuleLabel => "Set Enemies Defeated";

	public override IEnumerator RunSummonRule()
	{
		if (m_enemyTypeDefeated != 0 && m_enemyRankDefeated != EnemyRank.None && m_enemyTypeDefeated != EnemyType.Any && m_enemyRankDefeated != EnemyRank.Any && EnemyClassLibrary.GetEnemyData(m_enemyTypeDefeated, m_enemyRankDefeated) != null)
		{
			SaveManager.ModeSaveData.SetEnemiesDefeated(GameModeType.Regular, m_enemyTypeDefeated, m_enemyRankDefeated, m_timesDefeated, m_additive);
		}
		base.IsRuleComplete = true;
		yield break;
	}
}
