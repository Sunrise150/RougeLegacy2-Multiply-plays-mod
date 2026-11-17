using System;

public class RoomMergeEventArgs : EventArgs
{
	public Room NewRoom { get; private set; }

	public Room RoomA { get; private set; }

	public Room RoomB { get; private set; }

	public RoomSide AtSideOfA { get; private set; }

	public RoomMergeEventArgs(Room newRoom, Room roomA, Room roomB, RoomSide atSideOfRoomA)
	{
		NewRoom = newRoom;
		RoomA = roomA;
		RoomB = roomB;
		AtSideOfA = atSideOfRoomA;
	}
}
