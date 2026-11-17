using System;

public class ChallengeScoringType_RL
{
	private static ChallengeScoringType[] m_typeArray;

	public static ChallengeScoringType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(ChallengeScoringType)) as ChallengeScoringType[];
			}
			return m_typeArray;
		}
	}
}
