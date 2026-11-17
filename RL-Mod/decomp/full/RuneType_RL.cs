using System;

public class RuneType_RL
{
	private static RuneType[] m_typeArray;

	public static RuneType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(RuneType)) as RuneType[];
			}
			return m_typeArray;
		}
	}
}
