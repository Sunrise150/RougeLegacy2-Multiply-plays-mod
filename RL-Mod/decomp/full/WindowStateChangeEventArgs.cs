using System;

public class WindowStateChangeEventArgs : EventArgs
{
	public WindowID Window { get; private set; }

	public bool IsOpen { get; private set; }

	public WindowStateChangeEventArgs(WindowID id, bool isOpen)
	{
		Window = id;
		IsOpen = isOpen;
	}
}
