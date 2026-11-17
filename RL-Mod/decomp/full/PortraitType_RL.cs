using System;

public class PortraitType_RL
{
	private static PortraitType[] m_typeArray;

	public static PortraitType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(PortraitType)) as PortraitType[];
			}
			return m_typeArray;
		}
	}
}
