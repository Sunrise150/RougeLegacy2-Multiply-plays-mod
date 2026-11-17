using System;

public static class TraitType_RL
{
	private static TraitType[] m_typeArray;

	public static TraitType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(TraitType)) as TraitType[];
			}
			return m_typeArray;
		}
	}
}
