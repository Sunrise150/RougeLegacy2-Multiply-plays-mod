using System;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

[Serializable]
public class ItemDropAudioLibraryEntry : AudioLibraryEntry
{
	[SerializeField]
	[EventRef]
	private string m_spawnSingleEventPath;

	[SerializeField]
	private bool m_useManyAudio;

	[SerializeField]
	[EventRef]
	private string m_spawnManyEventPath;

	[SerializeField]
	[EventRef]
	private string m_landEventPath;

	[SerializeField]
	[EventRef]
	private string m_collectEventPath;

	public string SpawnSingleEventPath => m_spawnSingleEventPath;

	public string SpawnManyEventPath => m_spawnManyEventPath;

	public string LandEventPath => m_landEventPath;

	public string CollectEventPath => m_collectEventPath;

	public bool UseManyAudio => m_useManyAudio;
}
