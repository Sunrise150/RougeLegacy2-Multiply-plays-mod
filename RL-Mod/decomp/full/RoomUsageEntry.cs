using System;

[Serializable]
public class RoomUsageEntry
{
	public Room RoomPrefab;

	public int Count;

	public RoomUsageEntry(Room roomPrefab)
	{
		RoomPrefab = roomPrefab;
		Count = 0;
	}
}
