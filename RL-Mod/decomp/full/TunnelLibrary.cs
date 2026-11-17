using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Tunnel Library")]
public class TunnelLibrary : ScriptableObject
{
	[SerializeField]
	private TunnelLibraryEntry[] m_entries;

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/TunnelLibrary";

	public static string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/TunnelLibrary.asset";

	private static TunnelLibrary m_instance = null;

	private static Dictionary<TunnelCategory, Tunnel> m_prefabTable = null;

	public TunnelLibraryEntry[] Entries
	{
		get
		{
			return m_entries;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_entries = value;
			}
		}
	}

	public static TunnelLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<TunnelLibrary>("Scriptable Objects/Libraries/TunnelLibrary");
			}
			return m_instance;
		}
	}

	public static Tunnel GetPrefab(TunnelCategory category)
	{
		if (m_prefabTable == null)
		{
			m_prefabTable = new Dictionary<TunnelCategory, Tunnel>();
			TunnelLibraryEntry[] entries = Instance.Entries;
			foreach (TunnelLibraryEntry tunnelLibraryEntry in entries)
			{
				m_prefabTable.Add(tunnelLibraryEntry.Category, tunnelLibraryEntry.Prefab);
			}
		}
		if (m_prefabTable.ContainsKey(category))
		{
			return m_prefabTable[category];
		}
		Debug.LogFormat("<color=red>| {0} | Table doesn't contain an entry for Tunnel Category ({1}). Using default.</color>", Instance, category);
		return m_prefabTable[TunnelCategory.Default];
	}
}
