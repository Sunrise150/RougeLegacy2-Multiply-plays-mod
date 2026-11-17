using System;

public class RoomCountEventArgs : EventArgs
{
	public int Count { get; private set; }

	public RoomCountEventArgs(int count)
	{
		Count = count;
	}
}
