using System;
using System.Collections.Generic;
using UnityEngine;

public class IterateThroughEnemyTypesInLevelEditor : MonoBehaviour
{
	[SerializeField]
	private OnPlayManager m_onPlayManager;

	[SerializeField]
	private int m_overrideLevel = 1;

	[SerializeField]
	private bool m_isOn;

	[SerializeField]
	private KeyCode m_cycleType = KeyCode.LeftArrow;

	[SerializeField]
	private KeyCode m_cycleRank = KeyCode.RightArrow;

	private int m_nextEnemyTypeIndex;

	private int m_nextEnemyRankIndex;

	private List<EnemyType> m_enemyTypes = new List<EnemyType>();

	private List<EnemyRank> m_enemyRanks = new List<EnemyRank>();

	private EnemySpawnController[] m_enemySpawnControllers;

	private EnemyType m_currentEnemyType;

	private EnemyRank m_currentEnemyRank = EnemyRank.None;

	private void Awake()
	{
		if (!m_isOn)
		{
			base.enabled = false;
		}
	}

	private void Start()
	{
		foreach (EnemyType value in Enum.GetValues(typeof(EnemyType)))
		{
			if (value != 0 && value != EnemyType.Any)
			{
				m_enemyTypes.Add(value);
			}
		}
		m_currentEnemyType = m_enemyTypes[0];
		foreach (EnemyRank value2 in Enum.GetValues(typeof(EnemyRank)))
		{
			if (value2 != EnemyRank.Any && value2 != EnemyRank.None)
			{
				m_enemyRanks.Add(value2);
			}
		}
		m_currentEnemyRank = m_enemyRanks[0];
	}

	private void Update()
	{
		if (Input.GetKeyDown(m_cycleType))
		{
			CycleEnemyType();
			ResetRoom();
		}
		if (Input.GetKeyDown(m_cycleRank))
		{
			CycleEnemyRank();
			ResetRoom();
		}
	}

	private void CycleEnemyRank()
	{
		if (m_enemySpawnControllers == null)
		{
			m_enemySpawnControllers = m_onPlayManager.BaseRoom.GetComponentsInChildren<EnemySpawnController>();
		}
		if (m_enemySpawnControllers.Length != 0)
		{
			m_currentEnemyRank = m_enemyRanks[m_nextEnemyRankIndex];
			EnemySpawnController[] enemySpawnControllers = m_enemySpawnControllers;
			foreach (EnemySpawnController obj in enemySpawnControllers)
			{
				obj.Override = true;
				obj.Type = m_currentEnemyType;
				obj.Rank = m_currentEnemyRank;
			}
			m_nextEnemyRankIndex++;
			if (m_nextEnemyRankIndex > m_enemyRanks.Count - 1)
			{
				m_nextEnemyRankIndex = 0;
			}
		}
	}

	private void CycleEnemyType()
	{
		if (m_enemySpawnControllers == null)
		{
			m_enemySpawnControllers = m_onPlayManager.BaseRoom.GetComponentsInChildren<EnemySpawnController>();
		}
		if (m_enemySpawnControllers.Length != 0)
		{
			m_currentEnemyType = m_enemyTypes[m_nextEnemyTypeIndex];
			EnemySpawnController[] enemySpawnControllers = m_enemySpawnControllers;
			foreach (EnemySpawnController obj in enemySpawnControllers)
			{
				obj.Override = true;
				obj.Type = m_currentEnemyType;
				obj.Rank = m_currentEnemyRank;
			}
			m_nextEnemyTypeIndex++;
			if (m_nextEnemyTypeIndex > m_enemyTypes.Count - 1)
			{
				m_nextEnemyTypeIndex = 0;
			}
		}
	}

	private void ResetRoom()
	{
		m_onPlayManager.SetupWorld();
	}
}
