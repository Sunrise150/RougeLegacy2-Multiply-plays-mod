using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(EnemyController))]
public class EnemyGroupHPController : MonoBehaviour
{
	[SerializeField]
	private bool m_disableAutoGrouping;

	[SerializeField]
	[ConditionalHide("m_disableAutoGrouping", true, true)]
	private bool m_autoGroupBySameEnemyType;

	[SerializeField]
	[ConditionalHide("m_disableAutoGrouping", true, true)]
	private bool m_autoGroupBySameEnemyRank;

	private List<EnemyController> m_enemyGroup = new List<EnemyController>();

	private EnemyController m_baseEnemy;

	private bool m_isInitialized;

	private Action<object, CharacterHitEventArgs> m_onEnemyHit;

	private Action<object, HealthChangeEventArgs> m_onEnemyHPGain;

	private void Awake()
	{
		m_baseEnemy = GetComponent<EnemyController>();
		m_onEnemyHit = OnEnemyHit;
		m_onEnemyHPGain = OnEnemyHPGain;
	}

	private IEnumerator Initialize()
	{
		while (!m_baseEnemy.IsInitialized)
		{
			yield return null;
		}
		m_isInitialized = true;
		GroupEnemyHits();
	}

	private void OnEnable()
	{
		if (!m_isInitialized)
		{
			StartCoroutine(Initialize());
		}
		else
		{
			GroupEnemyHits();
		}
	}

	private void OnDisable()
	{
		StartAddingEnemyGroup();
	}

	public void StartAddingEnemyGroup()
	{
		if ((bool)m_baseEnemy && m_baseEnemy.IsInitialized)
		{
			m_baseEnemy.CharacterHitResponse.OnCharacterHitRelay.RemoveListener(m_onEnemyHit);
			m_baseEnemy.HealthChangeRelay.RemoveListener(m_onEnemyHPGain);
		}
		m_enemyGroup.Clear();
	}

	public void StopAddingEnemyGroup()
	{
		m_baseEnemy.CharacterHitResponse.OnCharacterHitRelay.AddListener(m_onEnemyHit);
		m_baseEnemy.HealthChangeRelay.AddListener(m_onEnemyHPGain);
	}

	public void AddEnemyToGroup(EnemyController enemy)
	{
		if (!m_enemyGroup.Contains(enemy))
		{
			m_enemyGroup.Add(enemy);
		}
	}

	public void RemoveEnemyFromGroup(EnemyController enemy)
	{
		if (m_enemyGroup.Contains(enemy))
		{
			m_enemyGroup.Remove(enemy);
		}
	}

	private void GroupEnemyHits()
	{
		if (!m_disableAutoGrouping)
		{
			StartAddingEnemyGroup();
			if (m_autoGroupBySameEnemyType || m_autoGroupBySameEnemyRank)
			{
				EnemySpawnController[] enemySpawnControllers = m_baseEnemy.Room.SpawnControllerManager.EnemySpawnControllers;
				for (int i = 0; i < enemySpawnControllers.Length; i++)
				{
					EnemyController enemyInstance = enemySpawnControllers[i].EnemyInstance;
					if ((bool)enemyInstance && (!m_autoGroupBySameEnemyType || enemyInstance.EnemyType == m_baseEnemy.EnemyType) && (!m_autoGroupBySameEnemyRank || enemyInstance.EnemyRank == m_baseEnemy.EnemyRank))
					{
						m_enemyGroup.Add(enemyInstance);
					}
				}
			}
			if (!m_enemyGroup.Contains(m_baseEnemy))
			{
				m_enemyGroup.Add(m_baseEnemy);
			}
		}
		StopAddingEnemyGroup();
	}

	private void OnEnemyHPGain(object sender, EventArgs args)
	{
		if (args is HealthChangeEventArgs healthChangeEventArgs && healthChangeEventArgs.PrevHealthValue < healthChangeEventArgs.NewHealthValue)
		{
			SyncHP();
		}
	}

	private void OnEnemyHit(object sender, CharacterHitEventArgs args)
	{
		if (m_baseEnemy.IsDead || !(args.DamageTaken > 0f))
		{
			return;
		}
		foreach (EnemyController item in m_enemyGroup)
		{
			if (!(item == m_baseEnemy))
			{
				if (!item.TakesNoDamage)
				{
					bool immuneToAllStatusEffects = item.StatusEffectController.ImmuneToAllStatusEffects;
					item.StatusEffectController.ImmuneToAllStatusEffects = true;
					StatusEffectController.DISABLE_ADDING_STATUS_EFFECTS = true;
					item.CanIncrementRelicHitCounter = false;
					item.CharacterHitResponse.StartHitResponse(args.Attacker.gameObject, args.Attacker, args.DamageTaken, trueDamage: true, fireEvents: false);
					item.CanIncrementRelicHitCounter = true;
					item.StatusEffectController.ImmuneToAllStatusEffects = immuneToAllStatusEffects;
					StatusEffectController.DISABLE_ADDING_STATUS_EFFECTS = false;
				}
				else
				{
					item.SetHealth(0f - args.DamageTaken, additive: true, runEvents: true);
				}
			}
		}
		SyncHP();
	}

	private void SyncHP()
	{
		float num = 0f;
		foreach (EnemyController item in m_enemyGroup)
		{
			if (!item.IsDead && item.CurrentHealth > num)
			{
				num = item.CurrentHealth;
			}
		}
		foreach (EnemyController item2 in m_enemyGroup)
		{
			if (!item2.IsDead)
			{
				item2.SetHealth(num, additive: false, runEvents: false);
			}
		}
	}
}
