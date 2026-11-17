using System;

public class BreakableEventArgs : EventArgs
{
	public Breakable BreakableObj { get; private set; }

	public BreakableEventArgs(Breakable breakableObj)
	{
		Initialize(breakableObj);
	}

	public void Initialize(Breakable breakableObj)
	{
		BreakableObj = breakableObj;
	}
}
