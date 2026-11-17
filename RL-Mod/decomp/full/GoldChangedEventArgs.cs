using System;

public class GoldChangedEventArgs : EventArgs
{
	public int PreviousGoldAmount { get; private set; }

	public int NewGoldAmount { get; private set; }

	public GoldChangedEventArgs(int prevGold, int newGold)
	{
		Initialize(prevGold, newGold);
	}

	public void Initialize(int prevGold, int newGold)
	{
		PreviousGoldAmount = prevGold;
		NewGoldAmount = newGold;
	}
}
