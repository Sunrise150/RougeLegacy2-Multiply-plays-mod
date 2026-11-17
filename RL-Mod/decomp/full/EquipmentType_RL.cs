using System;

public static class EquipmentType_RL
{
	private static EquipmentType[] m_typeArray;

	private static EquipmentCategoryType[] m_categoryTypeArray;

	public static EquipmentType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(EquipmentType)) as EquipmentType[];
			}
			return m_typeArray;
		}
	}

	public static EquipmentCategoryType[] CategoryTypeArray
	{
		get
		{
			if (m_categoryTypeArray == null)
			{
				m_categoryTypeArray = Enum.GetValues(typeof(EquipmentCategoryType)) as EquipmentCategoryType[];
			}
			return m_categoryTypeArray;
		}
	}

	public static EquipmentCategoryType GetFinalGearCategoryType(SpecialItemType specialItemType)
	{
		return specialItemType switch
		{
			SpecialItemType.FinalGear_Weapon => EquipmentCategoryType.Weapon, 
			SpecialItemType.FinalGear_Helm => EquipmentCategoryType.Head, 
			SpecialItemType.FinalGear_Chest => EquipmentCategoryType.Chest, 
			SpecialItemType.FinalGear_Cape => EquipmentCategoryType.Cape, 
			SpecialItemType.FinalGear_Trinket => EquipmentCategoryType.Trinket, 
			_ => EquipmentCategoryType.None, 
		};
	}
}
