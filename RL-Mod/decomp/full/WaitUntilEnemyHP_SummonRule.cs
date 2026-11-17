using System.Collections;
using UnityEngine;

public class WaitUntilEnemyHP_SummonRule : BaseSummonRule
{
	[SerializeField]
	private ParamEqualityType m_equalityType;

	[SerializeField]
	private float m_percentValue;

	[SerializeField]
	private bool m_includeRegularEnemies = true;

	[SerializeField]
	private bool m_includeSummonedEnemies = true;

	public override SummonRuleType RuleType => SummonRuleType.WaitUntilEnemyHP;

	public override Color BoxColor => Color.blue;

	public override string RuleLabel => "Wait Until Enemy HP";

	public override IEnumerator RunSummonRule()
	{
		bool equalitySatisfied = false;
		BaseRoom room = null;
		if (PlayerManager.IsInstantiated)
		{
			room = PlayerManager.GetCurrentPlayerRoom();
		}
		while (!equalitySatisfied)
		{
			float num = 0f;
			if (m_includeRegularEnemies && (bool)room)
			{
				EnemySpawnController[] enemySpawnControllers = room.SpawnControllerManager.EnemySpawnControllers;
				foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
				{
					EnemyController enemyInstance = enemySpawnController.EnemyInstance;
					if ((bool)enemySpawnController.EnemyInstance && !enemySpawnController.EnemyInstance.IsDead && enemyInstance.CurrentHealth > 0f)
					{
						num += enemyInstance.CurrentHealth;
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
			equalitySatisfied = EqualitySatisfied(num);
			yield return null;
		}
		base.IsRuleComplete = true;
	}

	private bool EqualitySatisfied(float currentEnemyHP)
	{
		float savedCurrentEnemyHP = base.SummonController.SavedCurrentEnemyHP;
		savedCurrentEnemyHP *= m_percentValue;
		return m_equalityType switch
		{
			ParamEqualityType.Equals => currentEnemyHP == savedCurrentEnemyHP, 
			ParamEqualityType.NotEqual => currentEnemyHP != savedCurrentEnemyHP, 
			ParamEqualityType.Greater => currentEnemyHP > savedCurrentEnemyHP, 
			ParamEqualityType.GreaterOrEqual => currentEnemyHP >= savedCurrentEnemyHP, 
			ParamEqualityType.Less => currentEnemyHP < savedCurrentEnemyHP, 
			ParamEqualityType.LessOrEqual => currentEnemyHP <= savedCurrentEnemyHP, 
			_ => false, 
		};
	}
}
