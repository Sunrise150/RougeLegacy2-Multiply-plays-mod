using System;

public class RoomCreatedEventArgs : EventArgs
{
	public Room Room { get; private set; }

	public RoomCreatedEventArgs(Room room)
	{
		Room = room;
	}
}
