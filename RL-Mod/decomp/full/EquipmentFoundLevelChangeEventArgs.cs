using System;

public class EquipmentFoundLevelChangeEventArgs : EventArgs
{
	public EquipmentCategoryType EquipmentCategoryType { get; private set; }

	public EquipmentType EquipmentType { get; private set; }

	public int NewLevel { get; private set; }

	public EquipmentFoundLevelChangeEventArgs(EquipmentCategoryType equipCat, EquipmentType equipType, int newLevel)
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
