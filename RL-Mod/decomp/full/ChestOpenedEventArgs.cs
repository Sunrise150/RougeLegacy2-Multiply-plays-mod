using System;
using System.Collections.Generic;

public class ChestOpenedEventArgs : EventArgs
{
	public ChestObj Chest { get; private set; }

	public SpecialItemType SpecialItemType { get; private set; }

	public int GoldValue { get; private set; }

	public List<ISpecialItemDrop> SpecialItems { get; private set; }

	public ChestOpenedEventArgs(ChestObj chest, SpecialItemType specialItemType, int dropAmount, List<ISpecialItemDrop> specialItems)
	{
		Initialize(chest, specialItemType, dropAmount, specialItems);
	}

	public void Initialize(ChestObj chest, SpecialItemType specialItemType, int dropAmount, List<ISpecialItemDrop> specialItems)
	{
		Chest = chest;
		GoldValue = dropAmount;
		SpecialItemType = specialItemType;
		SpecialItems = specialItems;
	}
}
