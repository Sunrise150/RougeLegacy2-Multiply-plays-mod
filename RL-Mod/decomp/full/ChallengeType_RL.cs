using System;

public class ChallengeType_RL
{
	private static ChallengeType[] m_typeArray;

	public static ChallengeType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(ChallengeType)) as ChallengeType[];
			}
			return m_typeArray;
		}
	}
}
