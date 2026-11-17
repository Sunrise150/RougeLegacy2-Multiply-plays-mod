using System;

[Serializable]
public class HighestNGBossBeatenEntry
{
	public int HighestCastleBossBeaten = -1;

	public int HighestBridgeBossBeaten = -1;

	public int HighestForestBossBeaten = -1;

	public int HighestStudyBossBeaten = -1;

	public int HighestTowerBossBeaten = -1;

	public int HighestCaveBossBeaten = -1;

	public int HighestGardenBossBeaten = -1;

	public int HighestFinalBossBeaten = -1;

	public int GetHighestNGBossBeaten(BossID bossID)
	{
		return bossID switch
		{
			BossID.Castle_Boss => HighestCastleBossBeaten, 
			BossID.Bridge_Boss => HighestBridgeBossBeaten, 
			BossID.Forest_Boss => HighestForestBossBeaten, 
			BossID.Study_Boss => HighestStudyBossBeaten, 
			BossID.Tower_Boss => HighestTowerBossBeaten, 
			BossID.Cave_Boss => HighestCaveBossBeaten, 
			BossID.Garden_Boss => HighestGardenBossBeaten, 
			BossID.Final_Boss => HighestFinalBossBeaten, 
			_ => -1, 
		};
	}

	public void SetHighestNGBossBeaten(BossID bossID, int value, bool forceOverride)
	{
		int highestNGBossBeaten = GetHighestNGBossBeaten(bossID);
		if (value > highestNGBossBeaten || forceOverride)
		{
			switch (bossID)
			{
			case BossID.Castle_Boss:
				HighestCastleBossBeaten = value;
				break;
			case BossID.Bridge_Boss:
				HighestBridgeBossBeaten = value;
				break;
			case BossID.Forest_Boss:
				HighestForestBossBeaten = value;
				break;
			case BossID.Study_Boss:
				HighestStudyBossBeaten = value;
				break;
			case BossID.Tower_Boss:
				HighestTowerBossBeaten = value;
				break;
			case BossID.Cave_Boss:
				HighestCaveBossBeaten = value;
				break;
			case BossID.Garden_Boss:
				HighestGardenBossBeaten = value;
				break;
			case BossID.Final_Boss:
				HighestFinalBossBeaten = value;
				break;
			}
		}
	}
}
