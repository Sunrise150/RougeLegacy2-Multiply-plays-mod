using System;

public static class LanguageType_RL
{
	private static LanguageType[] m_typeArray;

	public static LanguageType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(LanguageType)) as LanguageType[];
			}
			return m_typeArray;
		}
	}
}
