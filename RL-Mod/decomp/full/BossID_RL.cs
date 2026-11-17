using System;

public class BossID_RL
{
	private static BossID[] m_typeArray;

	public static BossID[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(BossID)) as BossID[];
			}
			return m_typeArray;
		}
	}

	public static bool IsBossBeaten(BossID bossID)
	{
		return bossID switch
		{
			BossID.Castle_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CastleBoss_Defeated), 
			BossID.Bridge_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BridgeBoss_Defeated), 
			BossID.Forest_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ForestBoss_Defeated), 
			BossID.Study_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyBoss_Defeated), 
			BossID.Tower_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TowerBoss_Defeated), 
			BossID.Cave_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated), 
			BossID.Garden_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.GardenBoss_Defeated), 
			BossID.Final_Boss => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.FinalBoss_Defeated), 
			_ => false, 
		};
	}

	public static BiomeType GetBossIDToBiome(BossID bossID)
	{
		return bossID switch
		{
			BossID.Castle_Boss => BiomeType.Castle, 
			BossID.Bridge_Boss => BiomeType.Stone, 
			BossID.Forest_Boss => BiomeType.Forest, 
			BossID.Study_Boss => BiomeType.Study, 
			BossID.Tower_Boss => BiomeType.Tower, 
			BossID.Cave_Boss => BiomeType.Cave, 
			_ => BiomeType.None, 
		};
	}

	public static BossID GetBiomeToBossID(BiomeType biomeType)
	{
		return biomeType switch
		{
			BiomeType.Castle => BossID.Castle_Boss, 
			BiomeType.Stone => BossID.Bridge_Boss, 
			BiomeType.Forest => BossID.Forest_Boss, 
			BiomeType.Study => BossID.Study_Boss, 
			BiomeType.Tower => BossID.Tower_Boss, 
			BiomeType.Cave => BossID.Cave_Boss, 
			_ => BossID.None, 
		};
	}
}
