using System.Collections;
using UnityEngine;

public class StatusEffectTester : MonoBehaviour
{
	[SerializeField]
	private StatusEffectType m_statusEffectToAdd;

	[SerializeField]
	private bool m_applyOnlyToExpertEnemies;

	[SerializeField]
	private float m_duration = 5f;

	private IEnumerator Start()
	{
		yield return new WaitForSeconds(0.5f);
		StartStatusEffect();
	}

	public void StartStatusEffect()
	{
		if (m_statusEffectToAdd == StatusEffectType.None)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		EnemyController caster = null;
		EnemySpawnController[] enemySpawnControllers = PlayerManager.GetCurrentPlayerRoom().SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (enemySpawnController.ShouldSpawn && (bool)enemySpawnController.EnemyInstance)
			{
				EnemyController component = enemySpawnController.EnemyInstance.gameObject.GetComponent<EnemyController>();
				if ((bool)component && !component.IsDead && (component.EnemyRank == EnemyRank.Expert || !m_applyOnlyToExpertEnemies))
				{
					caster = component;
					component.StatusEffectController.StartStatusEffect(m_statusEffectToAdd, m_duration, playerController);
				}
			}
		}
		playerController.StatusEffectController.StartStatusEffect(m_statusEffectToAdd, m_duration, caster);
	}

	public void StopStatusEffect()
	{
		if (m_statusEffectToAdd == StatusEffectType.None)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		EnemySpawnController[] enemySpawnControllers = PlayerManager.GetCurrentPlayerRoom().SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (enemySpawnController.ShouldSpawn && enemySpawnController.EnemyInstance != null)
			{
				EnemyController component = enemySpawnController.EnemyInstance.gameObject.GetComponent<EnemyController>();
				if ((bool)component)
				{
					component.StatusEffectController.StopStatusEffect(m_statusEffectToAdd, interrupted: true);
				}
			}
		}
		playerController.StatusEffectController.StopStatusEffect(m_statusEffectToAdd, interrupted: true);
	}
}
