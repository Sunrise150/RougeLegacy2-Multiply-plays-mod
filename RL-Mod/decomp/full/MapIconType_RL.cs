using System;

public class MapIconType_RL
{
	private static MapIconType[] m_typeArray;

	public static MapIconType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(MapIconType)) as MapIconType[];
			}
			return m_typeArray;
		}
	}
}
