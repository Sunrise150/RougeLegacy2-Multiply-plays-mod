using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Projectile Sub Layer Manager")]
public class ProjectileSubLayerManager : ScriptableObject
{
	[SerializeField]
	private List<ProjectileSubLayerEntry> m_projectileSubLayers;

	[SerializeField]
	private int m_subLayerDelta = 10;

	public const string RESOURCES_PATH = "Scriptable Objects/ProjectileSubLayerManager";

	public const string ASSETS_PATH = "Assets/Content/Scriptable Objects/ProjectileSubLayerManager.asset";

	public const string PREFABS_RESOURCES_PATH = "Prefabs/Projectiles";

	public const string PREFABS_ASSETS_PATH = "Assets/Content/Prefabs/Projectiles";

	private static ProjectileSubLayerManager m_instance;

	private ProjectileSizeComparer m_comparer;

	private Dictionary<string, int> m_subLayerTable;

	private static ProjectileSubLayerManager Instance
	{
		get
		{
			if (Application.isPlaying)
			{
				if (m_instance == null)
				{
					m_instance = CDGResources.Load<ProjectileSubLayerManager>("Scriptable Objects/ProjectileSubLayerManager");
				}
				return m_instance;
			}
			return m_instance;
		}
	}

	public List<ProjectileSubLayerEntry> SubLayers
	{
		get
		{
			return m_projectileSubLayers;
		}
		private set
		{
			m_projectileSubLayers = value;
		}
	}

	public static int GetSubLayer(string projectileName)
	{
		if (Instance.m_subLayerTable == null)
		{
			Instance.m_subLayerTable = new Dictionary<string, int>(100);
			for (int i = 0; i < Instance.SubLayers.Count; i++)
			{
				Instance.m_subLayerTable.Add(Instance.SubLayers[i].Name, Instance.SubLayers[i].SubLayer);
			}
		}
		if (Instance.m_subLayerTable.ContainsKey(projectileName))
		{
			return CameraLayerUtility.DefaultProjectileSubLayer + Instance.m_subLayerTable[projectileName];
		}
		return 0;
	}

	public void Reset()
	{
		if (!Application.isPlaying)
		{
			if (SubLayers == null)
			{
				SubLayers = new List<ProjectileSubLayerEntry>();
			}
			else
			{
				SubLayers.Clear();
			}
		}
	}

	public void UpdateSubLayers()
	{
		_ = Application.isPlaying;
	}
}
