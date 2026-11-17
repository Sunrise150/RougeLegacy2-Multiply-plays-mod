using System;

public class LevelManagerStateChangeEventArgs : EventArgs
{
	public LevelManagerState State { get; private set; }

	public LevelManagerStateChangeEventArgs(LevelManagerState state)
	{
		State = state;
	}
}
