using System;

public class EquippedChangeEventArgs : EventArgs
{
	public EquipmentCategoryType EquipmentCategoryType { get; private set; }

	public EquipmentType EquippedType { get; private set; }

	public EquippedChangeEventArgs(EquipmentCategoryType equipCat, EquipmentType newEquipType)
	{
		Initialize(equipCat, newEquipType);
	}

	public void Initialize(EquipmentCategoryType equipCat, EquipmentType newEquipType)
	{
		EquipmentCategoryType = equipCat;
		EquippedType = newEquipType;
	}
}
