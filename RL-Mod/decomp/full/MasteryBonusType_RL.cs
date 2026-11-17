using System;

public class MasteryBonusType_RL
{
	private static MasteryBonusType[] m_typeArray;

	public static MasteryBonusType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(MasteryBonusType)) as MasteryBonusType[];
			}
			return m_typeArray;
		}
	}
}
