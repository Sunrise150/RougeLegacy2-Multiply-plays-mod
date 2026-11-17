using System;

public static class ClassType_RL
{
	private static ClassType[] m_typeArray;

	public static ClassType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(ClassType)) as ClassType[];
			}
			return m_typeArray;
		}
	}
}
