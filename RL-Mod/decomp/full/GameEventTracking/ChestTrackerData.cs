using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public struct ChestTrackerData : IGameEventData
{
	private float m_timeStamp;

	private ChestType m_chestType;

	private int m_timesLoaded;

	private bool m_containsGold;

	private BiomeType m_biome;

	private int m_biomeControllerIndex;

	private int m_chestIndex;

	public int ChestIndex => m_chestIndex;

	public BiomeType Biome => m_biome;

	public int BiomeControllerIndex => m_biomeControllerIndex;

	public ChestType ChestType => m_chestType;

	public bool ContainsGold => m_containsGold;

	public float TimeStamp => m_timeStamp;

	public int TimesLoaded => m_timesLoaded;

	public ChestTrackerData(ChestType chestType, bool containsGold, BiomeType biome, int biomeControllerIndex, int chestIndex)
	{
		m_chestType = chestType;
		m_containsGold = containsGold;
		m_timesLoaded = SaveManager.StageSaveData.TimesTrackerWasLoaded;
		m_timeStamp = Time.frameCount;
		m_biome = biome;
		m_biomeControllerIndex = biomeControllerIndex;
		m_chestIndex = chestIndex;
	}
}
