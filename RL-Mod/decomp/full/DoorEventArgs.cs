using System;

public class DoorEventArgs : EventArgs
{
	public Door Door { get; private set; }

	public DoorEventArgs(Door door)
	{
		Initialize(door);
	}

	public void Initialize(Door door)
	{
		Door = door;
	}
}
