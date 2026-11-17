using System;

public class RelicModType_RL
{
	private static RelicModType[] m_typeArray;

	public static RelicModType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(RelicModType)) as RelicModType[];
			}
			return m_typeArray;
		}
	}
}
