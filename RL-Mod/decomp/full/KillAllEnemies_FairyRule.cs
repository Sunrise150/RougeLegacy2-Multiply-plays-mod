using System;
using System.Linq;
using UnityEngine;

public class KillAllEnemies_FairyRule : FairyRule
{
	private bool m_isListeningForEnemyDeathEvent;

	private Action<MonoBehaviour, EventArgs> m_checkEnemyDeathCount;

	public override string Description => "LOC_ID_FAIRY_RULE_KILL_ALL_ENEMIES_1";

	public override FairyRuleID ID => FairyRuleID.KillAllEnemies;

	public override bool LockChestAtStart => true;

	private void Awake()
	{
		m_checkEnemyDeathCount = CheckEnemyDeathCount;
	}

	private void OnDestroy()
	{
		UnsubscribeFromEnemyDeathEvent();
	}

	public override void RunRule(FairyRoomController fairyRoomController)
	{
		if (base.State != FairyRoomState.Failed)
		{
			if (!PerformInitialEnemyDeathCount())
			{
				base.State = FairyRoomState.Running;
				m_isListeningForEnemyDeathEvent = true;
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_checkEnemyDeathCount);
			}
			else
			{
				SetIsPassed();
			}
		}
	}

	public override void StopRule()
	{
		base.StopRule();
		UnsubscribeFromEnemyDeathEvent();
	}

	private void UnsubscribeFromEnemyDeathEvent()
	{
		if (m_isListeningForEnemyDeathEvent)
		{
			m_isListeningForEnemyDeathEvent = false;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_checkEnemyDeathCount);
		}
	}

	private bool PerformInitialEnemyDeathCount()
	{
		if (PlayerManager.IsInstantiated)
		{
			EnemySpawnController[] enemySpawnControllers = PlayerManager.GetCurrentPlayerRoom().SpawnControllerManager.EnemySpawnControllers;
			foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
			{
				if (!enemySpawnController.IsDead && !FairyRoomController.EnemyExceptionArray.Contains(enemySpawnController.Type))
				{
					return false;
				}
			}
			return true;
		}
		return false;
	}

	private void CheckEnemyDeathCount(object sender, EventArgs eventArgs)
	{
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		EnemyController victim = (eventArgs as EnemyDeathEventArgs).Victim;
		bool flag = true;
		EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (!(victim == null) && !(enemySpawnController.EnemyInstance == null) && !enemySpawnController.EnemyInstance.Equals(null) && !(victim.gameObject == enemySpawnController.EnemyInstance.gameObject) && !enemySpawnController.IsDead && !FairyRoomController.EnemyExceptionArray.Contains(enemySpawnController.Type))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			SetIsPassed();
			UnsubscribeFromEnemyDeathEvent();
		}
	}
}
