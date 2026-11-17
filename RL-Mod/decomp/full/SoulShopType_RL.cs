using System;

public class SoulShopType_RL
{
	private static SoulShopType[] m_typeArray;

	public static SoulShopType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(SoulShopType)) as SoulShopType[];
			}
			return m_typeArray;
		}
	}
}
