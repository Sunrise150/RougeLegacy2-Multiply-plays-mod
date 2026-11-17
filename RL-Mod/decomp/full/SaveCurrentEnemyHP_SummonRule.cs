using System.Collections;
using UnityEngine;

public class SaveCurrentEnemyHP_SummonRule : BaseSummonRule
{
	[SerializeField]
	private bool m_includeRegularEnemies = true;

	[SerializeField]
	private bool m_includeSummonedEnemies = true;

	public override SummonRuleType RuleType => SummonRuleType.SaveCurrentEnemyHP;

	public override string RuleLabel => "Save Current Enemy HP";

	public override IEnumerator RunSummonRule()
	{
		float num = 0f;
		if (m_includeRegularEnemies && PlayerManager.IsInstantiated)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			if ((bool)currentPlayerRoom)
			{
				EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
				for (int i = 0; i < enemySpawnControllers.Length; i++)
				{
					EnemyController enemyInstance = enemySpawnControllers[i].EnemyInstance;
					if ((bool)enemyInstance && !enemyInstance.IsDead && enemyInstance.CurrentHealth > 0f)
					{
						num += enemyInstance.CurrentHealth;
					}
				}
			}
		}
		if (m_includeSummonedEnemies)
		{
			foreach (EnemyController summonedEnemy in EnemyManager.SummonedEnemyList)
			{
				if (!summonedEnemy.IsDead && summonedEnemy.CurrentHealth > 0f)
				{
					num += summonedEnemy.CurrentHealth;
				}
			}
		}
		base.SummonController.SavedCurrentEnemyHP = num;
		base.IsRuleComplete = true;
		yield break;
	}
}
