using System;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

[Serializable]
public class ProjectileAudioLibraryEntry : AudioLibraryEntry
{
	[SerializeField]
	[EventRef]
	private string m_spawnSingleEventPath;

	[SerializeField]
	private bool m_useSpawnManyAudioWhenAppropriate;

	[SerializeField]
	[EventRef]
	private string m_spawnManyEventPath;

	[SerializeField]
	[EventRef]
	private string m_lifetimeEventPath;

	[SerializeField]
	[EventRef]
	private string m_hitSurfaceEventPath;

	[SerializeField]
	[EventRef]
	private string m_hitItemEventPath;

	[SerializeField]
	[EventRef]
	private string m_hitCharacterEventPath;

	[SerializeField]
	[EventRef]
	private string m_deathEventPath;

	public string SpawnSingleEventPath => m_spawnSingleEventPath;

	public bool UseSpawnManyAudioWhenAppropriate => m_useSpawnManyAudioWhenAppropriate;

	public string SpawnManyEventPath => m_spawnManyEventPath;

	public string HitSurfaceEventPath => m_hitSurfaceEventPath;

	public string HitItemEventPath => m_hitItemEventPath;

	public string HitCharacterEventPath => m_hitCharacterEventPath;

	public string DeathEventPath => m_deathEventPath;

	public string LifetimeEventPath => m_lifetimeEventPath;

	public ProjectileAudioLibraryEntry(string key, string spawnSinglePath, string spawnManyPath, string lifeTimeEventPath, string hitSurfaceEventPath, string hitItemEventPath, string hitCharacterEventPath, string deathEventPath)
	{
		if (!Application.isPlaying)
		{
			Key = key;
			m_spawnSingleEventPath = spawnSinglePath;
			m_spawnManyEventPath = spawnManyPath;
			m_lifetimeEventPath = lifeTimeEventPath;
			m_hitSurfaceEventPath = hitSurfaceEventPath;
			m_hitItemEventPath = hitItemEventPath;
			m_hitCharacterEventPath = hitCharacterEventPath;
			m_deathEventPath = deathEventPath;
			return;
		}
		throw new Exception("This should never be called during runtime");
	}

	public ProjectileAudioLibraryEntry()
	{
	}
}
