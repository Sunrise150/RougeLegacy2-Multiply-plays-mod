using System;

public class GamePauseStateChangeEventArgs : EventArgs
{
	public bool IsPaused { get; private set; }

	public GamePauseStateChangeEventArgs(bool isPaused)
	{
		Initialize(isPaused);
	}

	public void Initialize(bool isPaused)
	{
		IsPaused = isPaused;
	}
}
