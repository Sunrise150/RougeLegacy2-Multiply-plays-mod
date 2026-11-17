using System;

public class RelicChangedEventArgs : EventArgs
{
	public RelicType RelicType { get; private set; }

	public RelicChangedEventArgs(RelicType relicType)
	{
		Initialize(relicType);
	}

	public void Initialize(RelicType relicType)
	{
		RelicType = relicType;
	}
}
