using System;

public class ClownRoomEnteredEventArgs : EventArgs
{
	public ClownRoomController ClownRoomController { get; private set; }

	public ClownRoomEnteredEventArgs(ClownRoomController clownRoom)
	{
		Initialise(clownRoom);
	}

	public void Initialise(ClownRoomController clownRoom)
	{
		ClownRoomController = clownRoom;
	}
}
