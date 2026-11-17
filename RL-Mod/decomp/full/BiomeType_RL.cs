using System;

public class BiomeType_RL
{
	private static BiomeType[] m_typeArray;

	public static BiomeType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(BiomeType)) as BiomeType[];
			}
			return m_typeArray;
		}
	}

	public static BiomeType GetGroupedBiomeType(BiomeType biomeType)
	{
		switch (biomeType)
		{
		case BiomeType.ForestTop:
		case BiomeType.ForestBottom:
			return BiomeType.Forest;
		case BiomeType.TowerExterior:
			return BiomeType.Tower;
		case BiomeType.CaveMiddle:
		case BiomeType.CaveBottom:
			return BiomeType.Cave;
		default:
			return biomeType;
		}
	}

	public static bool IsValidBiome(BiomeType biomeType)
	{
		switch (biomeType)
		{
		case BiomeType.None:
		case BiomeType.Editor:
		case BiomeType.Lineage:
		case BiomeType.Spawn:
		case BiomeType.Special:
		case BiomeType.Any:
			return false;
		default:
			return true;
		}
	}
}
