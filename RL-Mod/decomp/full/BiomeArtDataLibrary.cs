using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Art Data Library")]
public class BiomeArtDataLibrary : ScriptableObject
{
	[SerializeField]
	private List<BiomeArtDataEntry> m_artTable;

	public static string RESOURCES_PATH = "Scriptable Objects/Libraries/BiomeArtDataLibrary";

	private Dictionary<BiomeType, BiomeArtData> m_artDataDictionary;

	private static BiomeArtDataLibrary m_instance;

	private static BiomeArtDataLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<BiomeArtDataLibrary>(RESOURCES_PATH);
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	public static List<BiomeArtDataEntry> ArtDataTable => Instance.m_artTable;

	private void Initialize()
	{
		Instance.m_artDataDictionary = new Dictionary<BiomeType, BiomeArtData>();
		foreach (BiomeArtDataEntry item in m_artTable)
		{
			Instance.m_artDataDictionary.Add(item.BiomeType, item.BiomeArtData);
		}
	}

	public static BiomeArtData GetArtData(BiomeType biome)
	{
		return Instance.m_artDataDictionary[biome];
	}
}
