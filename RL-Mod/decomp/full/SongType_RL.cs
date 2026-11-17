using System;

public class SongType_RL
{
	private static SongID[] m_typeArray;

	public static SongID[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(SongID)) as SongID[];
			}
			return m_typeArray;
		}
	}
}
