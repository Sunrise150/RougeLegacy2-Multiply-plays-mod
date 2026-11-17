using System;

public class ProjectileBuffType_RL
{
	private static ProjectileBuffType[] m_typeArray;

	public static ProjectileBuffType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(ProjectileBuffType)) as ProjectileBuffType[];
			}
			return m_typeArray;
		}
	}
}
