using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Room Usage Tracker")]
public class RoomUsageTracker : ScriptableObject
{
	[SerializeField]
	private List<RoomUsageEntry> m_roomUsageTable;

	private static RoomUsageTracker m_instance;

	private static Dictionary<Room, RoomUsageEntry> m_entryTable;

	private const string RESOURCES_PATH = "Scriptable Objects/RoomUsageTracker";

	private const string EDITOR_PATH = "Assets/Content/Scriptable Objects/RoomUsageTracker.asset";

	private static RoomUsageTracker Instance
	{
		get
		{
			if (m_instance == null)
			{
				if (Application.isPlaying)
				{
					m_instance = CDGResources.Load<RoomUsageTracker>("Scriptable Objects/RoomUsageTracker");
				}
				if (m_instance == null)
				{
					Debug.LogFormat("<color=red>| CameraLayerUtility | Failed to find Room Usage Tracker SO at Path ({0})</color>", "Scriptable Objects/RoomUsageTracker");
				}
			}
			return m_instance;
		}
	}

	public List<RoomUsageEntry> Entries
	{
		get
		{
			return m_roomUsageTable;
		}
		set
		{
			m_roomUsageTable = value;
		}
	}

	public void Reset()
	{
		if (Instance.Entries != null)
		{
			Instance.Entries.Clear();
		}
	}

	public static void TrackRoomPrefab(Room roomPrefab)
	{
		if (m_entryTable == null)
		{
			m_entryTable = new Dictionary<Room, RoomUsageEntry>();
			for (int i = 0; i < Instance.Entries.Count; i++)
			{
				m_entryTable.Add(Instance.Entries[i].RoomPrefab, Instance.Entries[i]);
			}
		}
		if (!m_entryTable.ContainsKey(roomPrefab))
		{
			RoomUsageEntry roomUsageEntry = new RoomUsageEntry(roomPrefab);
			Instance.Entries.Add(roomUsageEntry);
			m_entryTable.Add(roomPrefab, roomUsageEntry);
		}
		m_entryTable[roomPrefab].Count++;
	}
}
