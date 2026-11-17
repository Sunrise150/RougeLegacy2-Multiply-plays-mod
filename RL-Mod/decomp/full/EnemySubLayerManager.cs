using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Enemy Sub Layer Manager")]
public class EnemySubLayerManager : ScriptableObject
{
	[SerializeField]
	private List<EnemySubLayerEntry> m_enemySubLayers;

	[SerializeField]
	private int m_subLayerDelta = 10;

	public const string RESOURCES_PATH = "Scriptable Objects/EnemySubLayerManager";

	public const string ASSETS_PATH = "Assets/Content/Scriptable Objects/EnemySubLayerManager.asset";

	public const string ENEMY_PREFABS_RESOURCES_PATH = "Prefabs/Enemies";

	public const string ENEMY_PREFABS_ASSETS_PATH = "Assets/Content/Prefabs/Enemies";

	private static EnemySubLayerManager m_instance;

	private EnemySizeComparer m_enemySizeComparer;

	private Dictionary<EnemyTypeAndRank, int> m_subLayerTable;

	private static EnemySubLayerManager Instance
	{
		get
		{
			if (Application.isPlaying)
			{
				if (m_instance == null)
				{
					m_instance = CDGResources.Load<EnemySubLayerManager>("Scriptable Objects/EnemySubLayerManager");
				}
				return m_instance;
			}
			return m_instance;
		}
	}

	public List<EnemySubLayerEntry> EnemySubLayers
	{
		get
		{
			return m_enemySubLayers;
		}
		private set
		{
			m_enemySubLayers = value;
		}
	}

	public static int GetSubLayer(EnemyTypeAndRank enemy)
	{
		if (Instance.m_subLayerTable == null)
		{
			Instance.m_subLayerTable = new Dictionary<EnemyTypeAndRank, int>();
			for (int i = 0; i < Instance.EnemySubLayers.Count; i++)
			{
				if (!Instance.m_subLayerTable.ContainsKey(Instance.EnemySubLayers[i].Enemy))
				{
					Instance.m_subLayerTable.Add(Instance.EnemySubLayers[i].Enemy, Instance.EnemySubLayers[i].SubLayer);
					continue;
				}
				Debug.LogFormat("<color=red>| EnemySubLayerManager | {0} {1} duplicate in Enemy Sub Layer list.</color>", Instance.EnemySubLayers[i].Enemy.Rank, Instance.EnemySubLayers[i].Enemy.Type);
			}
		}
		if (Instance.m_subLayerTable.ContainsKey(enemy))
		{
			return CameraLayerUtility.DefaultEnemySubLayer + Instance.m_subLayerTable[enemy];
		}
		return 0;
	}

	public void PrintSizes()
	{
		for (int i = 0; i < Instance.m_enemySubLayers.Count; i++)
		{
			EnemyController enemyPrefab = EnemyLibrary.GetEnemyPrefab(Instance.m_enemySubLayers[i].Enemy.Type, Instance.m_enemySubLayers[i].Enemy.Rank);
			Bounds bounds = EnemyUtility.GetBounds(enemyPrefab.gameObject);
			float enemyScale = EnemyUtility.GetEnemyScale(enemyPrefab);
			Debug.LogFormat("{0}: {1} {2} size = [{3}, {4}]", i + 1, Instance.m_enemySubLayers[i].Enemy.Rank, Instance.m_enemySubLayers[i].Enemy.Type, bounds.size.x * enemyScale, bounds.size.y * enemyScale);
		}
	}

	public void Reset()
	{
		if (!Application.isPlaying)
		{
			if (EnemySubLayers == null)
			{
				EnemySubLayers = new List<EnemySubLayerEntry>();
			}
			else
			{
				EnemySubLayers.Clear();
			}
		}
	}

	public void UpdateSubLayers()
	{
		_ = Application.isPlaying;
	}
}
