using System;

public class ChestType_RL
{
	private static ChestType[] m_typeArray;

	public static ChestType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(ChestType)) as ChestType[];
			}
			return m_typeArray;
		}
	}

	public static int GetChestRarity(ChestType chestType)
	{
		return chestType switch
		{
			ChestType.Bronze => 0, 
			ChestType.Silver => 1, 
			ChestType.Gold => 2, 
			ChestType.Boss => 3, 
			_ => 4, 
		};
	}
}
