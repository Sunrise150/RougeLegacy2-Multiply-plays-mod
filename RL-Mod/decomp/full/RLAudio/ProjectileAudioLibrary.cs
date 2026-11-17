using UnityEngine;

namespace RLAudio;

[CreateAssetMenu(menuName = "Custom/Libraries/Projectile Audio Library")]
public class ProjectileAudioLibrary : AudioLibrary<ProjectileAudioLibraryEntry>
{
	private static ProjectileAudioLibrary m_instance;

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ProjectileAudioLibrary";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/ProjectileAudioLibrary.asset";

	private static ProjectileAudioLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ProjectileAudioLibrary>("Scriptable Objects/Libraries/ProjectileAudioLibrary");
			}
			return m_instance;
		}
	}

	public static ProjectileAudioLibraryEntry GetEntry(string projectileName)
	{
		return Instance.GetAudioLibraryEntry(projectileName);
	}

	public static ProjectileAudioLibraryEntry GetDefaultEntry()
	{
		return Instance.Default;
	}

	public static bool GetContainsProjectileEntry(string projectileName)
	{
		return Instance.GetContainsEntry(projectileName);
	}

	public static string GetSingleShotAudioPath(string projectileName)
	{
		ProjectileAudioLibraryEntry entry = GetEntry(projectileName);
		ProjectileAudioLibraryEntry defaultEntry = GetDefaultEntry();
		string spawnSingleEventPath = entry.SpawnSingleEventPath;
		if (string.IsNullOrEmpty(spawnSingleEventPath))
		{
			spawnSingleEventPath = defaultEntry.SpawnSingleEventPath;
		}
		return spawnSingleEventPath;
	}

	public static string GetMultiShotAudioPath(string projectileName)
	{
		ProjectileAudioLibraryEntry entry = GetEntry(projectileName);
		ProjectileAudioLibraryEntry defaultEntry = GetDefaultEntry();
		string spawnManyEventPath = entry.SpawnManyEventPath;
		if (string.IsNullOrEmpty(spawnManyEventPath))
		{
			spawnManyEventPath = defaultEntry.SpawnManyEventPath;
		}
		return spawnManyEventPath;
	}
}
