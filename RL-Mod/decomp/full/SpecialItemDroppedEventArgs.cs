using System;

public class SpecialItemDroppedEventArgs : EventArgs
{
	public ISpecialItemDrop SpecialItemDrop { get; private set; }

	public SpecialItemDroppedEventArgs(ISpecialItemDrop specialItemDrop)
	{
		SpecialItemDrop = specialItemDrop;
	}

	public void Initialize(ISpecialItemDrop specialItemDrop)
	{
		SpecialItemDrop = specialItemDrop;
	}
}
