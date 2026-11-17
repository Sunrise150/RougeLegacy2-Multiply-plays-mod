using System;

public class RoomViaDoorEventArgs : EventArgs
{
	public BaseRoom Room { get; private set; }

	public Door ViaDoor { get; private set; }

	public RoomViaDoorEventArgs(BaseRoom room, Door viaDoor = null)
	{
		Initialise(room, viaDoor);
	}

	public void Initialise(BaseRoom room, Door viaDoor = null)
	{
		Room = room;
		ViaDoor = viaDoor;
	}
}
