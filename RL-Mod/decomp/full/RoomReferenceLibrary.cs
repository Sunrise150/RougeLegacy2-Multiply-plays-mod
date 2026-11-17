using Rooms;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Room Reference Library")]
public class RoomReferenceLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/RoomReferenceLibrary";

	[SerializeField]
	private RoomReferenceEntry[] m_roomReferenceEntryArray;

	private static RoomReferenceLibrary m_instance;

	public static RoomReferenceLibrary Instance
	{
		get
		{
			if (!m_instance)
			{
				m_instance = CDGResources.Load<RoomReferenceLibrary>("Scriptable Objects/Libraries/RoomReferenceLibrary");
			}
			return m_instance;
		}
	}

	public static RoomReferenceEntry[] RoomReferenceArray
	{
		get
		{
			return Instance.m_roomReferenceEntryArray;
		}
		set
		{
			Instance.m_roomReferenceEntryArray = value;
		}
	}

	public static RoomReferenceEntry GetRoomReferenceEntry(RoomReferenceType roomReferenceType)
	{
		RoomReferenceEntry[] roomReferenceEntryArray = Instance.m_roomReferenceEntryArray;
		foreach (RoomReferenceEntry roomReferenceEntry in roomReferenceEntryArray)
		{
			if (roomReferenceEntry.RoomReferenceType == roomReferenceType)
			{
				return roomReferenceEntry;
			}
		}
		return null;
	}

	public static RoomMetaData GetRoomMetaData(RoomReferenceType roomReferenceType)
	{
		RoomReferenceEntry roomReferenceEntry = GetRoomReferenceEntry(roomReferenceType);
		if (!roomReferenceEntry.IsNativeNull())
		{
			return roomReferenceEntry.RoomMetaData;
		}
		return null;
	}

	public static string GetRoomMetaDataPath(RoomReferenceType roomReferenceType)
	{
		RoomReferenceEntry roomReferenceEntry = GetRoomReferenceEntry(roomReferenceType);
		if (!roomReferenceEntry.IsNativeNull())
		{
			return roomReferenceEntry.RoomMetaDataPath;
		}
		return null;
	}
}
