using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Rogue Legacy 2/Room Weight Manager")]
public class RoomWeightManager : ScriptableObject
{
	[SerializeField]
	private List<RoomWeightEntry> m_entries;

	[SerializeField]
	private bool m_isEnabled = true;

	[SerializeField]
	private bool m_treatMirroredAsDistinct = true;

	private static Dictionary<int, int> m_roomRNGWeightTable;

	private static RoomWeightManager m_instance;

	private const string RESOURCES_PATH = "Scriptable Objects/RoomWeightManager";

	public List<RoomWeightEntry> Entries
	{
		get
		{
			return m_entries;
		}
		private set
		{
			m_entries = value;
		}
	}

	private static RoomWeightManager Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<RoomWeightManager>("Scriptable Objects/RoomWeightManager");
			}
			return m_instance;
		}
	}

	public static int GetWeight(RoomID roomID, bool isMirrored)
	{
		if (!Instance.m_treatMirroredAsDistinct)
		{
			isMirrored = false;
		}
		if (m_roomRNGWeightTable == null)
		{
			m_roomRNGWeightTable = new Dictionary<int, int>();
			foreach (RoomWeightEntry entry in Instance.Entries)
			{
				int hashedRoomID = RoomUtility.GetHashedRoomID(entry.RoomID, entry.IsMirrored);
				if (!m_roomRNGWeightTable.ContainsKey(hashedRoomID))
				{
					int rNGWeight = entry.RNGWeight;
					m_roomRNGWeightTable.Add(hashedRoomID, rNGWeight);
				}
			}
		}
		int hashedRoomID2 = RoomUtility.GetHashedRoomID(roomID, isMirrored);
		if (m_roomRNGWeightTable.TryGetValue(hashedRoomID2, out var value))
		{
			return value;
		}
		return BiomeCreation_EV.DEFAULT_ROOM_RNG_WEIGHT;
	}
}
