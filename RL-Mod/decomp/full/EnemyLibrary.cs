using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Enemy Library")]
public class EnemyLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/EnemyLibrary";

	[SerializeField]
	private EnemyPrefabEntry[] m_enemyPrefabEntryList;

	private Dictionary<EnemyType, EnemyPrefabEntry> m_enemyPrefabEntryDict;

	private Dictionary<string, EnemyController> m_loadedEnemyPrefabDict = new Dictionary<string, EnemyController>();

	private static EnemyLibrary m_instance;

	private static EnemyLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<EnemyLibrary>("Scriptable Objects/Libraries/EnemyLibrary");
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	public static EnemyPrefabEntry[] EnemyPrefabEntryList => Instance.m_enemyPrefabEntryList;

	private void Initialize()
	{
		m_enemyPrefabEntryDict = new Dictionary<EnemyType, EnemyPrefabEntry>();
		EnemyPrefabEntry[] enemyPrefabEntryList = m_enemyPrefabEntryList;
		foreach (EnemyPrefabEntry enemyPrefabEntry in enemyPrefabEntryList)
		{
			m_enemyPrefabEntryDict.Add(enemyPrefabEntry.EnemyType, enemyPrefabEntry);
		}
	}

	public static string GetEnemyPrefabPath(EnemyType enemyType, EnemyRank enemyRank)
	{
		if (Instance.m_enemyPrefabEntryDict.TryGetValue(enemyType, out var value))
		{
			return value.GetPrefabPath(enemyRank);
		}
		return null;
	}

	public static EnemyController GetEnemyPrefab(EnemyType enemyType, EnemyRank enemyRank)
	{
		string enemyPrefabPath = GetEnemyPrefabPath(enemyType, enemyRank);
		if (!string.IsNullOrEmpty(enemyPrefabPath))
		{
			if (Instance.m_loadedEnemyPrefabDict.ContainsKey(enemyPrefabPath))
			{
				return Instance.m_loadedEnemyPrefabDict[enemyPrefabPath];
			}
			EnemyController component = CDGResources.Load<GameObject>(enemyPrefabPath).GetComponent<EnemyController>();
			Instance.m_loadedEnemyPrefabDict.Add(enemyPrefabPath, component);
			return component;
		}
		return null;
	}

	public static void ClearLoadedPrefabs()
	{
		string[] array = Instance.m_loadedEnemyPrefabDict.Keys.ToArray();
		foreach (string key in array)
		{
			Instance.m_loadedEnemyPrefabDict[key] = null;
		}
		Instance.m_loadedEnemyPrefabDict.Clear();
	}

	private static string GetSpecialEnemyName(EnemyType enemyType, EnemyRank enemyRank)
	{
		switch (enemyType)
		{
		case EnemyType.BouncySpike:
			switch (enemyRank)
			{
			case EnemyRank.Advanced:
				return "BouncySpike_Dodgeable_Advanced";
			case EnemyRank.Expert:
				return "BouncySpike_DownStrikeable_Expert";
			case EnemyRank.Miniboss:
				return "BouncySpike_Ricochet_Miniboss";
			}
			break;
		case EnemyType.Dummy:
			return "Dummy_Basic";
		}
		return null;
	}
}
