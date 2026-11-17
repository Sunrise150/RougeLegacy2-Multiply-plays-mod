using System;

public class AchievementType_RL
{
	private static AchievementType[] m_typeArray;

	public static AchievementType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(AchievementType)) as AchievementType[];
			}
			return m_typeArray;
		}
	}
}
