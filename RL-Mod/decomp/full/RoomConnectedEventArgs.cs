using System;

public class RoomConnectedEventArgs : EventArgs
{
	public Room CallingRoom { get; private set; }

	public BaseRoom ConnectedRoom { get; private set; }

	public RoomConnectedEventArgs(Room callingRoom, BaseRoom connectedRoom)
	{
		Initialize(callingRoom, connectedRoom);
	}

	public void Initialize(Room callingRoom, BaseRoom connectedRoom)
	{
		CallingRoom = callingRoom;
		ConnectedRoom = connectedRoom;
	}
}
