using System;

public class RunePurchaseLevelChangeEventArgs : EventArgs
{
	public RuneType RuneType { get; private set; }

	public int NewLevel { get; private set; }

	public RunePurchaseLevelChangeEventArgs(RuneType runeType, int newLevel)
	{
		Initialize(runeType, newLevel);
	}

	public void Initialize(RuneType runeType, int newLevel)
	{
		RuneType = runeType;
		NewLevel = newLevel;
	}
}
