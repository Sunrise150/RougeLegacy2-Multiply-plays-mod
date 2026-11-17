using System;

public class SpecialRoomCompletedEventArgs : EventArgs
{
	public BaseSpecialRoomController SpecialRoomController { get; private set; }

	public SpecialRoomCompletedEventArgs(BaseSpecialRoomController specialRoomController)
	{
		Initialize(specialRoomController);
	}

	public void Initialize(BaseSpecialRoomController specialRoomController)
	{
		SpecialRoomController = specialRoomController;
	}
}
