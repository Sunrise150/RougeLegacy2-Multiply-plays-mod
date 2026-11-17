using System;

public class GameModeType_RL
{
	private static GameModeType[] m_typeArray;

	public static GameModeType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(GameModeType)) as GameModeType[];
			}
			return m_typeArray;
		}
	}
}
