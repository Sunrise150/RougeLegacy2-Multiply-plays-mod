using System;

public class RelicType_RL
{
	private static RelicType[] m_relicTypeArray;

	public static RelicType[] TypeArray
	{
		get
		{
			if (m_relicTypeArray == null)
			{
				m_relicTypeArray = Enum.GetValues(typeof(RelicType)) as RelicType[];
			}
			return m_relicTypeArray;
		}
	}
}
