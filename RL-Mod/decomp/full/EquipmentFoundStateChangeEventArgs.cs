using System;

public class EquipmentFoundStateChangeEventArgs : EventArgs
{
	public EquipmentCategoryType EquipmentCategoryType { get; private set; }

	public EquipmentType EquipmentType { get; private set; }

	public FoundState NewEquipmentFoundState { get; private set; }

	public EquipmentFoundStateChangeEventArgs(EquipmentCategoryType equipCat, EquipmentType equipType, FoundState newFoundState)
	{
		Initialize(equipCat, equipType, newFoundState);
	}

	public void Initialize(EquipmentCategoryType equipCat, EquipmentType equipType, FoundState newFoundState)
	{
		EquipmentCategoryType = equipCat;
		EquipmentType = equipType;
		NewEquipmentFoundState = newFoundState;
	}
}
