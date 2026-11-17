using System;

public class ItemDropType_RL
{
	public const int MAX_NUM_ITEM_DROPS = 10;

	private static ItemDropType[] m_types;

	private static SpecialItemType[] m_specialTypes;

	public static ItemDropType[] Types
	{
		get
		{
			if (m_types == null)
			{
				m_types = Enum.GetValues(typeof(ItemDropType)) as ItemDropType[];
			}
			return m_types;
		}
	}

	public static SpecialItemType[] SpecialTypes
	{
		get
		{
			if (m_specialTypes == null)
			{
				m_specialTypes = Enum.GetValues(typeof(SpecialItemType)) as SpecialItemType[];
			}
			return m_specialTypes;
		}
	}
}
