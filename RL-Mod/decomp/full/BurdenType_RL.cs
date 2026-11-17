using System;

public class BurdenType_RL
{
	private static BurdenType[] m_typeArray;

	public static BurdenType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(BurdenType)) as BurdenType[];
			}
			return m_typeArray;
		}
	}
}
