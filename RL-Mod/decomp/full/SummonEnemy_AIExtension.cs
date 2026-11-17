using System.Collections;
using UnityEngine;

public static class SummonEnemy_AIExtension
{
	public static EnemyController SummonEnemy_NoYield(this BaseAIScript aiScript, EnemyType enemyType, EnemyRank rank, Vector2 spawnOffset, bool usesAbsPos, bool runSummonAnim)
	{
		SummonEnemyController component = aiScript.EnemyController.gameObject.GetComponent<SummonEnemyController>();
		if ((bool)component)
		{
			if (component.Contains(enemyType, rank))
			{
				EnemyController enemyController = EnemyManager.SummonEnemy(aiScript.EnemyController, enemyType, rank, spawnOffset, usesAbsPos, runSummonAnim);
				if (!runSummonAnim)
				{
					enemyController.gameObject.SetActive(value: true);
				}
				return enemyController;
			}
			Debug.Log("Could not summon enemy: " + enemyType.ToString() + " - " + rank.ToString() + ". Enemy not found in SummonEnemyController.");
		}
		else
		{
			Debug.Log("Could not summon enemy: " + enemyType.ToString() + " - " + rank.ToString() + ". Enemy must have SummonEnemyController.");
		}
		return null;
	}

	public static IEnumerator SummonEnemy(this BaseAIScript aiScript, EnemyType enemyType, EnemyRank rank, Vector2 spawnOffset, bool usesAbsPos)
	{
		SummonEnemyController component = aiScript.EnemyController.gameObject.GetComponent<SummonEnemyController>();
		if ((bool)component)
		{
			if (component.Contains(enemyType, rank))
			{
				EnemyController enemy = EnemyManager.SummonEnemy(aiScript.EnemyController, enemyType, rank, spawnOffset, usesAbsPos, runSummonAnim: false);
				yield return EnemyManager.RunSummonAnimCoroutine(enemy);
				yield break;
			}
			Debug.Log("Could not summon enemy: " + enemyType.ToString() + " - " + rank.ToString() + ". Enemy not found in SummonEnemyController.");
		}
		else
		{
			Debug.Log("Could not summon enemy: " + enemyType.ToString() + " - " + rank.ToString() + ". Enemy must have SummonEnemyController.");
		}
	}

	public static EnemyController SummonEnemy_NoYield(this BaseAIScript aiScript, EnemyType enemyType, EnemyRank rank, int roomSpawnPosIndex, bool runSummonAnim)
	{
		SpawnPositionController componentInChildren = aiScript.EnemyController.Room.gameObject.GetComponentInChildren<SpawnPositionController>();
		if ((bool)componentInChildren)
		{
			if (roomSpawnPosIndex == -1)
			{
				roomSpawnPosIndex = Random.Range(0, componentInChildren.SpawnPositionArray.Length);
			}
			Vector3 spawnPosition = componentInChildren.GetSpawnPosition(roomSpawnPosIndex);
			return aiScript.SummonEnemy_NoYield(enemyType, rank, spawnPosition, usesAbsPos: true, runSummonAnim);
		}
		Debug.Log("Could not summon enemy via spawnPositionIndex. Room missing SpawnPositionController.");
		return null;
	}

	public static IEnumerator SummonEnemy(this BaseAIScript aiScript, EnemyType enemyType, EnemyRank rank, int roomSpawnPosIndex)
	{
		SpawnPositionController componentInChildren = aiScript.EnemyController.Room.gameObject.GetComponentInChildren<SpawnPositionController>();
		if ((bool)componentInChildren)
		{
			if (roomSpawnPosIndex == -1)
			{
				roomSpawnPosIndex = Random.Range(0, componentInChildren.SpawnPositionArray.Length);
			}
			Vector3 spawnPosition = componentInChildren.GetSpawnPosition(roomSpawnPosIndex);
			yield return aiScript.SummonEnemy(enemyType, rank, spawnPosition, usesAbsPos: true);
		}
		else
		{
			Debug.Log("Could not summon enemy via spawnPositionIndex. Room missing SpawnPositionController.");
		}
	}
}
