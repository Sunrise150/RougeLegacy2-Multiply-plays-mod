using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Special Platform Library")]
public class SpecialPlatformLibrary : ScriptableObject
{
	[Serializable]
	private class SpecialPlatformEntry
	{
		public SpecialPlatformType Type;

		public SpecialPlatform Prefab;
	}

	[SerializeField]
	private SpecialPlatformEntry[] m_entries;

	[SerializeField]
	private BiomeSpecialPlatformEntry[] m_specialPlatformsInBiomes;

	[SerializeField]
	private SpecialPlatformType[] m_defaultSpecialPlatformsInBiomes;

	private const string RESOURCES_PATH = "Scriptable Objects/Libraries/SpecialPlatformLibrary";

	private const string EDITOR_PATH = "Assets/Content/Scriptable Objects/Libraries/SpecialPlatformLibrary.asset";

	private static Dictionary<SpecialPlatformType, SpecialPlatformEntry> m_entryTable;

	private static SpecialPlatformLibrary m_instance;

	private static Dictionary<BiomeType, SpecialPlatformType[]> m_specialPlatformsInBiomeTable;

	private static SpecialPlatformLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				if (Application.isPlaying)
				{
					m_instance = CDGResources.Load<SpecialPlatformLibrary>("Scriptable Objects/Libraries/SpecialPlatformLibrary");
				}
				if (m_instance == null)
				{
					Debug.LogFormat("<color=red>| SpecialPlatformLibrary | Failed to find SpecialPlatformLibrary SO at Path ({0})</color>", "Scriptable Objects/Libraries/SpecialPlatformLibrary");
				}
			}
			return m_instance;
		}
	}

	public static BiomeSpecialPlatformEntry[] SpecialPlatformsInBiomes
	{
		get
		{
			return Instance.m_specialPlatformsInBiomes;
		}
		set
		{
			if (!Application.isPlaying)
			{
				Instance.m_specialPlatformsInBiomes = value;
			}
		}
	}

	public static SpecialPlatformType[] DefaultSpecialPlatformsInBiomes
	{
		get
		{
			return Instance.m_defaultSpecialPlatformsInBiomes;
		}
		set
		{
			if (!Application.isPlaying)
			{
				Instance.m_defaultSpecialPlatformsInBiomes = value;
			}
		}
	}

	private static void CreateEntryTable()
	{
		m_entryTable = new Dictionary<SpecialPlatformType, SpecialPlatformEntry>();
		SpecialPlatformEntry[] entries = Instance.m_entries;
		foreach (SpecialPlatformEntry specialPlatformEntry in entries)
		{
			m_entryTable.Add(specialPlatformEntry.Type, specialPlatformEntry);
		}
	}

	public static SpecialPlatform CreatePlatformInstance(SpecialPlatformType type, bool isActive = true)
	{
		if (m_entryTable == null)
		{
			CreateEntryTable();
		}
		if (m_entryTable.ContainsKey(type))
		{
			SpecialPlatform specialPlatform = UnityEngine.Object.Instantiate(m_entryTable[type].Prefab);
			specialPlatform.gameObject.SetActive(isActive);
			return specialPlatform;
		}
		Debug.LogFormat("<color=red>| {0} | Table doesn't contain an entry matching Type ({1})</color>", Instance, type);
		return null;
	}

	public static SpecialPlatformType[] GetPlatformTypesInBiome(BiomeType biome)
	{
		if (m_specialPlatformsInBiomeTable == null)
		{
			InitialiseSpecialPlatformTable();
		}
		SpecialPlatformType[] result = DefaultSpecialPlatformsInBiomes;
		if (m_specialPlatformsInBiomeTable.ContainsKey(biome))
		{
			result = m_specialPlatformsInBiomeTable[biome];
		}
		return result;
	}

	private static void InitialiseSpecialPlatformTable()
	{
		m_specialPlatformsInBiomeTable = new Dictionary<BiomeType, SpecialPlatformType[]>();
		BiomeSpecialPlatformEntry[] specialPlatformsInBiomes = SpecialPlatformsInBiomes;
		foreach (BiomeSpecialPlatformEntry biomeSpecialPlatformEntry in specialPlatformsInBiomes)
		{
			m_specialPlatformsInBiomeTable.Add(biomeSpecialPlatformEntry.Biome, biomeSpecialPlatformEntry.SpecialPlatforms);
		}
	}
}
