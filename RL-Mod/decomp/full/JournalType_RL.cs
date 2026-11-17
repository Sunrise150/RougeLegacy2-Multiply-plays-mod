using System;

public class JournalType_RL
{
	private static JournalCategoryType[] m_categoryTypeArray;

	private static JournalCategoryType[] m_sortedCategoryTypeArray;

	public static JournalCategoryType[] CategoryTypeArray
	{
		get
		{
			if (m_categoryTypeArray == null)
			{
				m_categoryTypeArray = Enum.GetValues(typeof(JournalCategoryType)) as JournalCategoryType[];
			}
			return m_categoryTypeArray;
		}
	}

	public static JournalCategoryType[] SortedCategoryTypeArray
	{
		get
		{
			if (m_sortedCategoryTypeArray == null)
			{
				m_sortedCategoryTypeArray = new JournalCategoryType[7]
				{
					JournalCategoryType.None,
					JournalCategoryType.Castle,
					JournalCategoryType.Bridge,
					JournalCategoryType.Forest,
					JournalCategoryType.Study,
					JournalCategoryType.Tower,
					JournalCategoryType.Cave
				};
			}
			return m_sortedCategoryTypeArray;
		}
	}

	public static JournalCategoryType ConvertBiomeToJournalCategoryType(BiomeType biome)
	{
		switch (biome)
		{
		case BiomeType.Castle:
			return JournalCategoryType.Castle;
		case BiomeType.Forest:
		case BiomeType.ForestTop:
		case BiomeType.ForestBottom:
			return JournalCategoryType.Forest;
		case BiomeType.Stone:
			return JournalCategoryType.Bridge;
		case BiomeType.Study:
			return JournalCategoryType.Study;
		case BiomeType.Tower:
		case BiomeType.TowerExterior:
			return JournalCategoryType.Tower;
		case BiomeType.Cave:
			return JournalCategoryType.Cave;
		default:
			return JournalCategoryType.None;
		}
	}

	public static BiomeType ConvertJournalCategoryTypeToBiome(JournalCategoryType journalCategoryType)
	{
		return journalCategoryType switch
		{
			JournalCategoryType.Castle => BiomeType.Castle, 
			JournalCategoryType.Bridge => BiomeType.Stone, 
			JournalCategoryType.Forest => BiomeType.Forest, 
			JournalCategoryType.Study => BiomeType.Study, 
			JournalCategoryType.Tower => BiomeType.Tower, 
			JournalCategoryType.Cave => BiomeType.Cave, 
			_ => BiomeType.None, 
		};
	}
}
