using System;

public class EquipmentPurchasedLevelChangeEventArgs : EventArgs
{
	public EquipmentCategoryType EquipmentCategoryType { get; private set; }

	public EquipmentType EquipmentType { get; private set; }

	public int NewLevel { get; private set; }

	public EquipmentPurchasedLevelChangeEventArgs(EquipmentCategoryType equipCat, EquipmentType equipType, int newLevel)
	{
		Initialize(equipCat, equipType, newLevel);
	}

	public void Initialize(EquipmentCategoryType equipCat, EquipmentType equipType, int newLevel)
	{
		EquipmentCategoryType = equipCat;
		EquipmentType = equipType;
		NewLevel = NewLevel;
	}
}
