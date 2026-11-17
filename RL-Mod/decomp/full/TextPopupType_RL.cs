using System;

public class TextPopupType_RL
{
	private static TextPopupType[] m_typeArray;

	public static TextPopupType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(TextPopupType)) as TextPopupType[];
			}
			return m_typeArray;
		}
	}
}
