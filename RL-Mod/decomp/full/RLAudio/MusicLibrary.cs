using System.Collections.Generic;
using UnityEngine;

namespace RLAudio;

[CreateAssetMenu(menuName = "Custom/Libraries/Music Library")]
public class MusicLibrary : AudioLibrary<MusicLibraryEntry>
{
	[SerializeField]
	private MusicLookupEntry[] m_lookupEntries;

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/MusicLibrary";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/MusicLibrary.asset";

	private static Dictionary<string, MusicLibraryEntry> m_biomeMusicLookupTable;

	private static Dictionary<SongID, string> m_musicIDToFMODEventPathLookupTable;

	private static MusicLibrary Instance { get; set; }

	public static bool IsInitialized { get; private set; }

	public static SongID[] GetBiomeMusic(BiomeType biome)
	{
		if (m_biomeMusicLookupTable.ContainsKey(biome.ToString()))
		{
			return m_biomeMusicLookupTable[biome.ToString()].MusicTracks;
		}
		return Instance.m_default.MusicTracks;
	}

	public static string GetFMODEventPath(SongID musicID)
	{
		if (m_musicIDToFMODEventPathLookupTable.ContainsKey(musicID))
		{
			return m_musicIDToFMODEventPathLookupTable[musicID];
		}
		Debug.LogFormat("<color=red>| MusicLibrary | No entry found with key (<b>{0}</b>) in lookup table</color>", musicID);
		return string.Empty;
	}

	public static void Initialize()
	{
		Instance = CDGResources.Load<MusicLibrary>("Scriptable Objects/Libraries/MusicLibrary");
		InitializeBiomeMusicLookupTable();
		InitializeMusicToFMODEventPathLookupTable();
		IsInitialized = true;
	}

	private static void InitializeBiomeMusicLookupTable()
	{
		m_biomeMusicLookupTable = new Dictionary<string, MusicLibraryEntry>();
		foreach (MusicLibraryEntry entry in Instance.Entries)
		{
			m_biomeMusicLookupTable.Add(entry.Key, entry);
		}
	}

	private static void InitializeMusicToFMODEventPathLookupTable()
	{
		m_musicIDToFMODEventPathLookupTable = new Dictionary<SongID, string>();
		MusicLookupEntry[] lookupEntries = Instance.m_lookupEntries;
		foreach (MusicLookupEntry musicLookupEntry in lookupEntries)
		{
			m_musicIDToFMODEventPathLookupTable.Add(musicLookupEntry.ID, musicLookupEntry.EventRef);
		}
	}
}
