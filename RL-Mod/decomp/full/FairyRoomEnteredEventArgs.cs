using System;

public class FairyRoomEnteredEventArgs : EventArgs
{
	public FairyRoomController FairyRoomController { get; private set; }

	public FairyRoomEnteredEventArgs(FairyRoomController fairyRoom)
	{
		Initialise(fairyRoom);
	}

	public void Initialise(FairyRoomController fairyRoom)
	{
		FairyRoomController = fairyRoom;
	}
}
