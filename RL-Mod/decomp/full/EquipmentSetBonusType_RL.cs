using System;

public class EquipmentSetBonusType_RL
{
	private static EquipmentSetBonusType[] m_typeArray;

	public static EquipmentSetBonusType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(EquipmentSetBonusType)) as EquipmentSetBonusType[];
			}
			return m_typeArray;
		}
	}
}
