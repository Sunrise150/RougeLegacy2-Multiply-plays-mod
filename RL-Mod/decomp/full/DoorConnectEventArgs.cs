using System;

public class DoorConnectEventArgs : EventArgs
{
	public Door DoorA { get; private set; }

	public Door DoorB { get; private set; }

	public DoorConnectEventArgs(Door doorA, Door doorB)
	{
		Initialize(doorA, doorB);
	}

	public void Initialize(Door doorA, Door doorB)
	{
		DoorA = doorA;
		DoorB = doorB;
	}
}
