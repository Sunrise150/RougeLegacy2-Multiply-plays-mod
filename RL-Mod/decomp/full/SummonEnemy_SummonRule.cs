using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class SummonEnemy_SummonRule : BaseSummonRule
{
	[SerializeField]
	private float m_summonValue;

	[SerializeField]
	private float m_summonDelay;

	[SerializeField]
	private bool m_randomizeEnemiesOnce;

	[SerializeField]
	private bool m_spawnFast;

	[SerializeField]
	private bool m_summonAsCommander;

	private WaitRL_Yield m_waitYield;

	private List<EnemyTypeAndRank> m_possibleEnemiesList;

	public override SummonRuleType RuleType => SummonRuleType.SummonEnemies;

	public override Color BoxColor => Color.green;

	public override string RuleLabel => "Summon Enemies";

	public override void Initialize(SummonRuleController summonController)
	{
		base.Initialize(summonController);
		m_waitYield = new WaitRL_Yield(0f);
		m_possibleEnemiesList = new List<EnemyTypeAndRank>();
	}

	public override IEnumerator RunSummonRule()
	{
		if (!base.SummonController.HasArenaStarted)
		{
			throw new Exception("Cannot start arena.  StartArena summon rule must be added before SummonEnemy can be called.");
		}
		SpawnPositionController spawnPosController = base.SummonController.gameObject.GetComponent<SpawnPositionController>();
		m_possibleEnemiesList.Clear();
		float remainingSummonValue = m_summonValue;
		int randomizeOnceEnemyIndex = -1;
		bool canEvolve = false;
		while (remainingSummonValue > 0f)
		{
			float num = float.MaxValue;
			foreach (EnemyTypeAndRank item in base.SummonController.SummonPool)
			{
				float summonValue = EnemyClassLibrary.GetEnemyData(item.Type, item.Rank).SummonValue;
				if (summonValue < num)
				{
					num = summonValue;
				}
				if (summonValue <= remainingSummonValue)
				{
					m_possibleEnemiesList.Add(item);
				}
			}
			if (m_possibleEnemiesList.Count == 0)
			{
				foreach (EnemyTypeAndRank item2 in base.SummonController.SummonPool)
				{
					if (EnemyClassLibrary.GetEnemyData(item2.Type, item2.Rank).SummonValue == num)
					{
						m_possibleEnemiesList.Add(item2);
					}
				}
			}
			int num2 = UnityEngine.Random.Range(0, m_possibleEnemiesList.Count);
			if (m_randomizeEnemiesOnce && randomizeOnceEnemyIndex != -1)
			{
				num2 = randomizeOnceEnemyIndex;
			}
			randomizeOnceEnemyIndex = num2;
			EnemyTypeAndRank randEnemy = m_possibleEnemiesList[num2];
			if (base.SummonController.SummonDifficultyOverride != EnemyRank.None)
			{
				randEnemy = new EnemyTypeAndRank(randEnemy.Type, base.SummonController.SummonDifficultyOverride);
			}
			num2 = UnityEngine.Random.Range(0, base.SummonController.AvailableSpawnPoints.Count);
			int randSpawnPointIndex = base.SummonController.AvailableSpawnPoints[num2];
			base.SummonController.AvailableSpawnPoints.RemoveAt(num2);
			if (base.SummonController.AvailableSpawnPoints.Count <= 0)
			{
				base.SummonController.AvailableSpawnPoints.AddRange(base.SummonController.SpawnPoints);
			}
			remainingSummonValue -= EnemyClassLibrary.GetEnemyData(randEnemy.Type, randEnemy.Rank).SummonValue;
			if (base.SummonController.PoolIsBiomeSpecific && canEvolve && base.SummonController.SummonDifficultyOverride == EnemyRank.None && canEvolve && randEnemy.Rank < EnemyRank.Expert)
			{
				float burdenStatGain = BurdenManager.GetBurdenStatGain(BurdenType.EnemyEvolve);
				if (UnityEngine.Random.Range(0f, 1f) < burdenStatGain)
				{
					randEnemy = new EnemyTypeAndRank(randEnemy.Type, randEnemy.Rank + 1);
				}
			}
			while (!PlayerManager.IsInstantiated || PlayerManager.GetCurrentPlayerRoom() == null)
			{
				yield return null;
			}
			Vector2 spawnPosOffset = spawnPosController.GetSpawnPosition(randSpawnPointIndex);
			float speedMod = (m_spawnFast ? 3f : 1f);
			EnemyController enemyController = EnemyManager.SummonEnemy(PlayerManager.GetCurrentPlayerRoom(), randEnemy.Type, randEnemy.Rank, spawnPosOffset, useAbsPos: true, runSummonAnim: true, speedMod);
			enemyController.IsCommander = m_summonAsCommander;
			enemyController.SetLevel(base.SummonController.SummonLevelOverride);
			if (remainingSummonValue > 0f && m_summonDelay > 0f)
			{
				m_waitYield.CreateNew(m_summonDelay);
				yield return m_waitYield;
			}
		}
		base.IsRuleComplete = true;
	}
}
