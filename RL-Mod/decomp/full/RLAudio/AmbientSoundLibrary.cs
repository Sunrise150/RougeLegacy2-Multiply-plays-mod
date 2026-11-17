using FMODUnity;
using UnityEngine;

namespace RLAudio;

[CreateAssetMenu(menuName = "Custom/Libraries/Ambient Sound Library")]
public class AmbientSoundLibrary : AudioLibrary<AmbientSoundLibraryEntry>
{
	[SerializeField]
	[EventRef]
	private string[] m_fairyRoomSmall;

	[SerializeField]
	[EventRef]
	private string m_fairyRoomSmallSnapshot;

	[SerializeField]
	[EventRef]
	private string[] m_fairyRoomLarge;

	[SerializeField]
	[EventRef]
	private string m_fairyRoomLargeSnapshot;

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/AmbientSoundLibrary";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/AmbientSoundLibrary.asset";

	private static AmbientSoundLibrary m_instance;

	private static AmbientSoundLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<AmbientSoundLibrary>("Scriptable Objects/Libraries/AmbientSoundLibrary");
			}
			return m_instance;
		}
	}

	public static string[] GetAmbientAudioEventPaths(BiomeType biome, RoomType roomType, bool isRoomLarge)
	{
		if (roomType == RoomType.Fairy)
		{
			if (isRoomLarge)
			{
				return Instance.m_fairyRoomLarge;
			}
			return Instance.m_fairyRoomSmall;
		}
		return GetEventPaths(Instance.GetAudioLibraryEntry(biome.ToString()), roomType, isRoomLarge);
	}

	public static string GetAmbientAudioSnapshotPath(BiomeType biome, RoomType roomType, bool isRoomLarge)
	{
		if (roomType == RoomType.Fairy)
		{
			if (isRoomLarge)
			{
				return Instance.m_fairyRoomLargeSnapshot;
			}
			return Instance.m_fairyRoomSmallSnapshot;
		}
		return GetSnapshotPath(Instance.GetAudioLibraryEntry(biome.ToString()), roomType, isRoomLarge);
	}

	private static string[] GetEventPaths(AmbientSoundLibraryEntry entry, RoomType roomType, bool isRoomLarge)
	{
		string[] result = new string[0];
		if (roomType == RoomType.Transition)
		{
			if (entry.TransitionRoomEntry.Source == AudioSource.UserSpecified)
			{
				result = entry.TransitionRoomEntry.EventPaths;
			}
			else if (entry.TransitionRoomEntry.Source == AudioSource.Default)
			{
				result = Instance.Default.TransitionRoomEntry.EventPaths;
			}
		}
		else if (isRoomLarge)
		{
			if (entry.LargeRoomEntry.Source == AudioSource.UserSpecified)
			{
				result = entry.LargeRoomEntry.EventPaths;
			}
			else if (entry.LargeRoomEntry.Source == AudioSource.Default)
			{
				result = Instance.Default.LargeRoomEntry.EventPaths;
			}
		}
		else if (entry.SmallRoomEntry.Source == AudioSource.UserSpecified)
		{
			result = entry.SmallRoomEntry.EventPaths;
		}
		else if (entry.SmallRoomEntry.Source == AudioSource.Default)
		{
			result = Instance.Default.SmallRoomEntry.EventPaths;
		}
		return result;
	}

	private static string GetSnapshotPath(AmbientSoundLibraryEntry entry, RoomType roomType, bool isRoomLarge)
	{
		string result = string.Empty;
		if (roomType == RoomType.Transition)
		{
			if (entry.TransitionRoomSnapshotEntry.Source == AudioSource.UserSpecified)
			{
				result = entry.TransitionRoomSnapshotEntry.SnapshotPath;
			}
			else if (entry.TransitionRoomEntry.Source == AudioSource.Default)
			{
				result = Instance.Default.TransitionRoomSnapshot;
			}
		}
		else if (isRoomLarge)
		{
			if (entry.LargeRoomSnapshotEntry.Source == AudioSource.UserSpecified)
			{
				result = entry.LargeRoomSnapshotEntry.SnapshotPath;
			}
			else if (entry.LargeRoomSnapshotEntry.Source == AudioSource.Default)
			{
				result = Instance.Default.LargeRoomSnapshot;
			}
		}
		else if (entry.SmallRoomSnapshotEntry.Source == AudioSource.UserSpecified)
		{
			result = entry.SmallRoomSnapshotEntry.SnapshotPath;
		}
		else if (entry.SmallRoomSnapshotEntry.Source == AudioSource.Default)
		{
			result = Instance.Default.SmallRoomSnapshot;
		}
		return result;
	}
}
