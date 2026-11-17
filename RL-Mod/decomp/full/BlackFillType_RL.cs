using System;

public class BlackFillType_RL
{
	private static BlackFillType[] m_typeArray;

	public static BlackFillType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(BlackFillType)) as BlackFillType[];
			}
			return m_typeArray;
		}
	}
}
