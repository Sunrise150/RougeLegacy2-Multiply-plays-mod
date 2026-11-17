using System;

public class HeirloomType_RL
{
	private static HeirloomType[] m_typeArray;

	public static HeirloomType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(HeirloomType)) as HeirloomType[];
			}
			return m_typeArray;
		}
	}
}
