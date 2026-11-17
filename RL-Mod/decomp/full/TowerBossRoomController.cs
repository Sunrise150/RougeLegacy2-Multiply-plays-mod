using System;
using System.Collections;
using UnityEngine;

public class TowerBossRoomController : BossRoomController
{
	[SerializeField]
	private EnemySpawnController[] m_bossSpawnControllers;

	[SerializeField]
	private EnemySpawnController[] m_bossSpawnersToStartDisabled;

	[SerializeField]
	private bool m_disableBossIntro;

	[SerializeField]
	private bool m_disableActiveEyeballModeshift;

	[SerializeField]
	private bool m_disableInactiveEyeballModeshift;

	[SerializeField]
	private bool m_disableDeath;

	private Action m_runBossSpawnAnim;

	private Action m_runIntroComplete;

	protected override void Awake()
	{
		base.Awake();
		m_runBossSpawnAnim = RunBossSpawnAnim;
		m_runIntroComplete = RunIntroComplete;
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		EnemySpawnController[] bossSpawnControllers = m_bossSpawnControllers;
		for (int i = 0; i < bossSpawnControllers.Length; i++)
		{
			EnemyController enemyInstance = bossSpawnControllers[i].EnemyInstance;
			if (!enemyInstance)
			{
				continue;
			}
			EnemyGroupHPController component = enemyInstance.GetComponent<EnemyGroupHPController>();
			component.StartAddingEnemyGroup();
			EnemySpawnController[] bossSpawnControllers2 = m_bossSpawnControllers;
			foreach (EnemySpawnController obj in bossSpawnControllers2)
			{
				EnemyController enemyInstance2 = obj.EnemyInstance;
				if ((bool)obj)
				{
					component.AddEnemyToGroup(enemyInstance2);
				}
			}
			component.StopAddingEnemyGroup();
			if ((enemyInstance.EnemyType == EnemyType.EyeballBoss_Left || enemyInstance.EnemyType == EnemyType.EyeballBoss_Right) && enemyInstance.EnemyRank == EnemyRank.Miniboss)
			{
				enemyInstance.TakesNoDamage = true;
			}
			enemyInstance.DisableDeath = m_disableDeath;
		}
	}

	protected override IEnumerator StartIntro()
	{
		base.Boss.LockFlip = true;
		BossSpawnAnimRelay.AddOnce(m_runBossSpawnAnim);
		IntroCompleteRelay.AddOnce(m_runIntroComplete);
		EnemySpawnController[] bossSpawnControllers = m_bossSpawnControllers;
		foreach (EnemySpawnController enemySpawnController in bossSpawnControllers)
		{
			EnemyController enemyInstance = enemySpawnController.EnemyInstance;
			if ((bool)enemyInstance)
			{
				if (!DisableSpawner(enemySpawnController))
				{
					enemyInstance.Animator.SetBool("Centered", value: false);
					enemyInstance.Animator.Play("Intro_Idle");
					(enemyInstance.LogicController.LogicScript as EyeballBoss_Basic_AIScript).StartsDisabled = false;
					(enemyInstance.LogicController.LogicScript as EyeballBoss_Basic_AIScript).DisableModeshift = m_disableActiveEyeballModeshift;
					enemyInstance.TakesNoDamage = false;
				}
				else
				{
					(enemyInstance.LogicController.LogicScript as EyeballBoss_Basic_AIScript).StartsDisabled = true;
					(enemyInstance.LogicController.LogicScript as EyeballBoss_Basic_AIScript).DisableModeshift = m_disableInactiveEyeballModeshift;
				}
				enemyInstance.LogicController.DisableLogicActivationByDistance = true;
			}
		}
		if (!m_disableBossIntro)
		{
			yield return base.StartIntro();
		}
		RunIntroComplete();
	}

	private bool DisableSpawner(EnemySpawnController bossSpawner)
	{
		EnemySpawnController[] bossSpawnersToStartDisabled = m_bossSpawnersToStartDisabled;
		foreach (EnemySpawnController enemySpawnController in bossSpawnersToStartDisabled)
		{
			if (bossSpawner == enemySpawnController)
			{
				return true;
			}
		}
		return false;
	}

	private void RunBossSpawnAnim()
	{
		EnemySpawnController[] bossSpawnControllers = m_bossSpawnControllers;
		foreach (EnemySpawnController enemySpawnController in bossSpawnControllers)
		{
			if (!(enemySpawnController == BossSpawnController) && !DisableSpawner(enemySpawnController))
			{
				EnemyController enemyInstance = enemySpawnController.EnemyInstance;
				if ((bool)enemyInstance)
				{
					StartCoroutine(enemyInstance.LogicController.LogicScript.SpawnAnim());
				}
			}
		}
	}

	private void RunIntroComplete()
	{
		EnemySpawnController[] bossSpawnControllers = m_bossSpawnControllers;
		foreach (EnemySpawnController enemySpawnController in bossSpawnControllers)
		{
			if (!(enemySpawnController == BossSpawnController) && !DisableSpawner(enemySpawnController))
			{
				EnemyController enemyInstance = enemySpawnController.EnemyInstance;
				if ((bool)enemyInstance)
				{
					enemyInstance.LogicController.DisableLogicActivationByDistance = false;
				}
			}
		}
	}
}
