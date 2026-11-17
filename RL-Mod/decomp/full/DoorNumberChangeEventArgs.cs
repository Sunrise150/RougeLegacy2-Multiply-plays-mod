using System;

public class DoorNumberChangeEventArgs : EventArgs
{
	public Door Door { get; private set; }

	public int OriginalNumber { get; private set; }

	public int NewNumber { get; private set; }

	public DoorNumberChangeEventArgs(Door door, int originalNumber, int newNumber)
	{
		Door = door;
		OriginalNumber = originalNumber;
		NewNumber = newNumber;
	}
}
