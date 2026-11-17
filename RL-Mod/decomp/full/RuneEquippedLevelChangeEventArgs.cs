using System;

public class RuneEquippedLevelChangeEventArgs : EventArgs
{
	public int NewLevel { get; private set; }

	public RuneType RuneType { get; private set; }

	public RuneEquippedLevelChangeEventArgs(RuneType runeType, int newLevel)
	{
		Initialize(runeType, newLevel);
	}

	public void Initialize(RuneType runeType, int newLevel)
	{
		RuneType = runeType;
		NewLevel = newLevel;
	}
}
