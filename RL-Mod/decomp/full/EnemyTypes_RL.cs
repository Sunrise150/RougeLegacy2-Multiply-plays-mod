using System;

public static class EnemyTypes_RL
{
	private static EnemyType[] m_typeArray;

	private static EnemyRank[] m_rankArray;

	public static EnemyType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(EnemyType)) as EnemyType[];
			}
			return m_typeArray;
		}
	}

	public static EnemyRank[] RankArray
	{
		get
		{
			if (m_rankArray == null)
			{
				m_rankArray = Enum.GetValues(typeof(EnemyRank)) as EnemyRank[];
			}
			return m_rankArray;
		}
	}
}
