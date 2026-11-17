using System;

public class HitboxType_RL
{
	private static HitboxType[] m_hbTypeArray;

	public static HitboxType[] TypeArray
	{
		get
		{
			if (m_hbTypeArray == null)
			{
				m_hbTypeArray = Enum.GetValues(typeof(HitboxType)) as HitboxType[];
			}
			return m_hbTypeArray;
		}
	}
}
