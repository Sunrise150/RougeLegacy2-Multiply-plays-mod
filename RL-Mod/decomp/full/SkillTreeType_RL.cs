using System;

public class SkillTreeType_RL
{
	private static SkillTreeType[] m_typeArray;

	public static SkillTreeType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(SkillTreeType)) as SkillTreeType[];
			}
			return m_typeArray;
		}
	}
}
