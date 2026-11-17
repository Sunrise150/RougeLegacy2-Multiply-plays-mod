public class Achievement_EV
{
	public static void RunAchievementSafetyChecks()
	{
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CastleBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossCastleDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CastleBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossCastleAdvancedDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BridgeBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossBridgeDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BridgeBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossBridgeAdvancedDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ForestBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossForestDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ForestBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossForestAdvancedDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossStudyDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossStudyAdvancedDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TowerBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossTowerDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TowerBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossTowerAdvancedDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossCaveDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossCaveAdvancedDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.GardenBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossGardenDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.GardenBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossGardenAdvancedDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.FinalBoss_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossFinalDefeated, unlocked: true);
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.FinalBoss_Prime_Defeated_FirstTime))
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.BossFinalAdvancedDefeated, unlocked: true);
		}
		if (ChallengeManager.GetChallengeTrophyRank(ChallengeType.NightmareKhidr) >= ChallengeTrophyRank.Bronze)
		{
			SaveManager.ModeSaveData.SetAchievementUnlocked(AchievementType.NightmareKhidr, unlocked: true);
		}
	}
}
