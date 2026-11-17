using System;

public class HeirloomLevelChangedEventArgs : EventArgs
{
	public HeirloomType HeirloomType { get; private set; }

	public int PrevLevel { get; private set; }

	public int NewLevel { get; private set; }

	public HeirloomLevelChangedEventArgs(HeirloomType heirloomType, int prevLevel, int newLevel)
	{
		Initialize(heirloomType, prevLevel, newLevel);
	}

	public void Initialize(HeirloomType heirloomType, int oldLevel, int newLevel)
	{
		HeirloomType = heirloomType;
		PrevLevel = oldLevel;
		NewLevel = newLevel;
	}
}
