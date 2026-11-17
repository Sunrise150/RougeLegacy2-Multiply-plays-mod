using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Biome Library")]
public class BiomeDataLibrary : ScriptableObject
{
	private const string RESOURCES_PATH = "Scriptable Objects/Libraries/BiomeDataLibrary";

	[SerializeField]
	private List<BiomeDataEntry> m_dataTable;

	private Dictionary<BiomeType, BiomeData> m_dataDictionary;

	private static BiomeDataLibrary m_instance;

	private static BiomeDataLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<BiomeDataLibrary>("Scriptable Objects/Libraries/BiomeDataLibrary");
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	private void Initialize()
	{
		Instance.m_dataDictionary = new Dictionary<BiomeType, BiomeData>();
		foreach (BiomeDataEntry item in m_dataTable)
		{
			Instance.m_dataDictionary.Add(item.BiomeType, item.BiomeData);
		}
	}

	public static BiomeData GetData(BiomeType biomeType)
	{
		return Instance.m_dataDictionary[biomeType];
	}
}
