using System;

public class RuneFoundLevelChangeEventArgs : EventArgs
{
	public RuneType RuneType { get; private set; }

	public int NewLevel { get; private set; }

	public RuneFoundLevelChangeEventArgs(RuneType runeType, int newLevel)
	{
		Initialize(runeType, newLevel);
	}

	public void Initialize(RuneType runeType, int newLevel)
	{
		RuneType = runeType;
		NewLevel = newLevel;
	}
}
