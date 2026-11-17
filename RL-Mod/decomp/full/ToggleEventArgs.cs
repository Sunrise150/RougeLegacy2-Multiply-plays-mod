using System;

public class ToggleEventArgs : EventArgs
{
	public bool Value { get; private set; }

	public ToggleEventArgs(bool value)
	{
		Value = value;
	}
}
