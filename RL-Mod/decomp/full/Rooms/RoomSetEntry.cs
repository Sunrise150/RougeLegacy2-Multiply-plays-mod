namespace Rooms;

public struct RoomSetEntry
{
	public bool IsMirrored { get; private set; }

	public RoomMetaData RoomMetaData { get; }

	public int Weight { get; private set; }

	public int DoorMask { get; private set; }

	public RoomSetEntry(RoomMetaData roomMetaData, bool isMirrored = false)
	{
		RoomMetaData = roomMetaData;
		IsMirrored = isMirrored;
		Weight = -1;
		DoorMask = 0;
		DoorLocation[] doorLocations = roomMetaData.DoorLocations;
		for (int i = 0; i < doorLocations.Length; i++)
		{
			DoorLocation doorLocation = doorLocations[i];
			RoomSide roomSide = ((!IsMirrored) ? doorLocation.RoomSide : RoomUtility.GetMirrorSide(doorLocation.RoomSide));
			int num = ((!IsMirrored) ? doorLocation.DoorNumber : RoomUtility.GetMirrorDoorNumber(doorLocation, roomMetaData.Size));
			DoorMask |= 1 << (int)roomSide * 3 + num;
		}
	}

	public RoomSetEntry(RoomSetEntry original, int weight)
	{
		IsMirrored = original.IsMirrored;
		RoomMetaData = original.RoomMetaData;
		Weight = weight;
		DoorMask = original.DoorMask;
	}

	public override string ToString()
	{
		return RoomMetaData.ID.ToString() + (IsMirrored ? "M" : "");
	}
}
