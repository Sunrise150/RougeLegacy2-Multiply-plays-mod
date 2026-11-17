using System;

public class RuneFoundStateChangeEventArgs : EventArgs
{
	public RuneType RuneType { get; private set; }

	public FoundState NewFoundState { get; private set; }

	public RuneFoundStateChangeEventArgs(RuneType runeType, FoundState newFoundState)
	{
		Initialize(runeType, newFoundState);
	}

	public void Initialize(RuneType runeType, FoundState newFoundState)
	{
		RuneType = runeType;
		NewFoundState = newFoundState;
	}
}
