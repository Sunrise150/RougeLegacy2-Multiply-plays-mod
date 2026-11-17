using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public struct EnemyTrackerData : IGameEventData
{
	private BiomeType m_biome;

	private int m_enemyIndex;

	private EnemyType m_enemyType;

	private EnemyRank m_enemyRank;

	private int m_biomeControllerIndex;

	private float m_timeStamp;

	private int m_timesLoaded;

	public BiomeType Biome => m_biome;

	public int BiomeControllerIndex => m_biomeControllerIndex;

	public int EnemyIndex => m_enemyIndex;

	public EnemyType EnemyType => m_enemyType;

	public EnemyRank EnemyRank => m_enemyRank;

	public float TimeStamp => m_timeStamp;

	public int TimesLoaded => m_timesLoaded;

	public EnemyTrackerData(BiomeType biome, int biomeControllerIndex, EnemyType enemyType, EnemyRank enemyRank, int enemyIndex)
	{
		m_biome = biome;
		m_biomeControllerIndex = biomeControllerIndex;
		m_enemyType = enemyType;
		m_enemyRank = enemyRank;
		m_enemyIndex = enemyIndex;
		m_timeStamp = Time.frameCount;
		m_timesLoaded = SaveManager.StageSaveData.TimesTrackerWasLoaded;
	}
}
