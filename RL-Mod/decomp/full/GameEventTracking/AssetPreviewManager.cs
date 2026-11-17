using System.Collections.Generic;
using UnityEngine;

namespace GameEventTracking;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Asset Preview Manager")]
public class AssetPreviewManager : ScriptableObject
{
	public List<EnemyPreviewEntry> EnemyPreviewImages;

	public List<ChestPreviewEntry> ChestPreviewImages;

	[SerializeField]
	private Sprite m_defaultSprite;

	public string ImageFolderPath = "Textures/UI/DeathScreen";

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/AssetPreviewManager";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/AssetPreviewManager.asset";

	private static AssetPreviewManager m_instance;

	private static Dictionary<ChestType, Sprite> m_chestPreviewImageTable;

	private static Dictionary<EnemyTypeAndRank, Sprite> m_enemyPreviewTable;

	private static AssetPreviewManager Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<AssetPreviewManager>("Scriptable Objects/Libraries/AssetPreviewManager");
			}
			return m_instance;
		}
	}

	public static Sprite GetPreviewImage(ChestType chestType)
	{
		if (m_chestPreviewImageTable == null)
		{
			m_chestPreviewImageTable = new Dictionary<ChestType, Sprite>();
			foreach (ChestPreviewEntry chestPreviewImage in Instance.ChestPreviewImages)
			{
				m_chestPreviewImageTable.Add(chestPreviewImage.ChestType, chestPreviewImage.PreviewImage);
			}
		}
		if (m_chestPreviewImageTable.ContainsKey(chestType))
		{
			return m_chestPreviewImageTable[chestType];
		}
		return null;
	}

	public static Sprite GetPreviewImage(EnemyType enemyType, EnemyRank enemyRank)
	{
		if (m_enemyPreviewTable == null)
		{
			m_enemyPreviewTable = new Dictionary<EnemyTypeAndRank, Sprite>();
			foreach (EnemyPreviewEntry enemyPreviewImage in Instance.EnemyPreviewImages)
			{
				foreach (EnemyRankEntry rankEntry in enemyPreviewImage.RankEntries)
				{
					m_enemyPreviewTable.Add(new EnemyTypeAndRank(enemyPreviewImage.Type, rankEntry.Rank), rankEntry.PreviewImage);
				}
			}
		}
		EnemyTypeAndRank key = new EnemyTypeAndRank(enemyType, enemyRank);
		if (m_enemyPreviewTable.ContainsKey(key) && m_enemyPreviewTable[key] != null)
		{
			return m_enemyPreviewTable[key];
		}
		if (!m_enemyPreviewTable.ContainsKey(key))
		{
			Debug.LogFormat("<color=red>| {0} | Doesn't contain an entry for Enemy ({1},{2})</color>", Instance, enemyRank, enemyType);
		}
		else if (m_enemyPreviewTable[key] == null)
		{
			Debug.LogFormat("<color=red>| {0} | Sprite field is null for Enemy ({1},{2})</color>", Instance, enemyRank, enemyType);
		}
		return Instance.m_defaultSprite;
	}
}
