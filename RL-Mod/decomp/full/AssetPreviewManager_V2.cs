using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Asset Preview Manager V2")]
public class AssetPreviewManager_V2 : ScriptableObject
{
	[Serializable]
	public class EnemyRankEntry_V2
	{
		public EnemyRank Rank = EnemyRank.None;

		public GameObject EntryObj;

		public EnemyRankEntry_V2(EnemyRank rank)
		{
			Rank = rank;
		}
	}

	[Serializable]
	public class EnemyPreviewEntry_V2
	{
		public EnemyType EnemyType;

		public EnemyRankEntry_V2[] RankEntries;

		public EnemyPreviewEntry_V2(EnemyType enemyType)
		{
			EnemyType = enemyType;
			RankEntries = new EnemyRankEntry_V2[4];
			RankEntries[0] = new EnemyRankEntry_V2(EnemyRank.Basic);
			RankEntries[1] = new EnemyRankEntry_V2(EnemyRank.Advanced);
			RankEntries[2] = new EnemyRankEntry_V2(EnemyRank.Expert);
			RankEntries[3] = new EnemyRankEntry_V2(EnemyRank.Miniboss);
		}
	}

	[Serializable]
	public class ChestPreviewEntry_V2
	{
		public ChestType ChestType;

		public GameObject EntryObj;

		public ChestPreviewEntry_V2(ChestType chestType)
		{
			ChestType = chestType;
		}
	}

	public GameObject m_defaultPreviewEntry;

	[Space(10f)]
	public EnemyPreviewEntry_V2[] EnemyPreviewEntries;

	public ChestPreviewEntry_V2[] ChestPreviewEntries;

	[NonSerialized]
	public string EntryFolderPath = "Prefabs/Enemy Previews";

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/AssetPreviewManager_V2";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/AssetPreviewManager_V2.asset";

	private static AssetPreviewManager_V2 m_instance;

	private static Dictionary<ChestType, GameObject> m_chestPreviewTable;

	private static Dictionary<EnemyTypeAndRank, GameObject> m_enemyPreviewTable;

	private static AssetPreviewManager_V2 Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = CDGResources.Load<AssetPreviewManager_V2>("Scriptable Objects/Libraries/AssetPreviewManager_V2");
			}
			return m_instance;
		}
	}

	public static GameObject GetEnemyPreviewEntry(EnemyType enemyType, EnemyRank enemyRank)
	{
		if (m_enemyPreviewTable == null)
		{
			CreateEnemyPreviewTable();
		}
		EnemyTypeAndRank key = new EnemyTypeAndRank(enemyType, enemyRank);
		if (m_enemyPreviewTable.TryGetValue(key, out var value))
		{
			return value;
		}
		if (!m_enemyPreviewTable.ContainsKey(key))
		{
			Debug.LogFormat("<color=red>| {0} | Doesn't contain an entry for Enemy ({1},{2})</color>", Instance, enemyRank, enemyType);
		}
		else if (m_enemyPreviewTable[key] == null)
		{
			Debug.LogFormat("<color=red>| {0} | EntryObj field is null for Enemy ({1},{2})</color>", Instance, enemyRank, enemyType);
		}
		return Instance.m_defaultPreviewEntry;
	}

	private static void CreateEnemyPreviewTable()
	{
		m_enemyPreviewTable = new Dictionary<EnemyTypeAndRank, GameObject>();
		EnemyPreviewEntry_V2[] enemyPreviewEntries = Instance.EnemyPreviewEntries;
		foreach (EnemyPreviewEntry_V2 enemyPreviewEntry_V in enemyPreviewEntries)
		{
			EnemyRankEntry_V2[] rankEntries = enemyPreviewEntry_V.RankEntries;
			foreach (EnemyRankEntry_V2 enemyRankEntry_V in rankEntries)
			{
				m_enemyPreviewTable.Add(new EnemyTypeAndRank(enemyPreviewEntry_V.EnemyType, enemyRankEntry_V.Rank), enemyRankEntry_V.EntryObj);
			}
		}
	}

	public static GameObject GetChestPreviewEntry(ChestType chestType)
	{
		if (m_chestPreviewTable == null)
		{
			CreateChestPreviewTable();
		}
		if (m_chestPreviewTable.TryGetValue(chestType, out var value))
		{
			return value;
		}
		Debug.LogFormat("<color=red>| {0} | Doesn't contain an entry for Chest ({1})</color>", Instance, chestType);
		return Instance.m_defaultPreviewEntry;
	}

	private static void CreateChestPreviewTable()
	{
		m_chestPreviewTable = new Dictionary<ChestType, GameObject>();
		ChestPreviewEntry_V2[] chestPreviewEntries = Instance.ChestPreviewEntries;
		foreach (ChestPreviewEntry_V2 chestPreviewEntry_V in chestPreviewEntries)
		{
			m_chestPreviewTable.Add(chestPreviewEntry_V.ChestType, chestPreviewEntry_V.EntryObj);
		}
	}
}
