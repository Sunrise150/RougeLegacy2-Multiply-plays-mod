using System;

public class ConditionFlag_RL
{
	private static ConditionFlag[] m_typeArray;

	public static ConditionFlag[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(ConditionFlag)) as ConditionFlag[];
			}
			return m_typeArray;
		}
	}

	public static bool IsConditionFulfilled(ConditionFlag id)
	{
		return id switch
		{
			ConditionFlag.Heirloom_Dash => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockAirDash) > 0, 
			ConditionFlag.Heirloom_Memory => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockMemory) > 0, 
			ConditionFlag.Heirloom_DoubleJump => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockDoubleJump) > 0, 
			ConditionFlag.Heirloom_Downstrike => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockBouncableDownstrike) > 0, 
			ConditionFlag.Heirloom_VoidDash => SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockVoidDash) > 0, 
			ConditionFlag.BossDefeated_Castle => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CastleBoss_Defeated), 
			ConditionFlag.BossDefeated_Bridge => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BridgeBoss_Defeated), 
			ConditionFlag.BossDefeated_Forest => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ForestBoss_Defeated), 
			ConditionFlag.BossDefeated_Study => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyBoss_Defeated), 
			ConditionFlag.BossDefeated_Tower => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TowerBoss_Defeated), 
			ConditionFlag.BossDefeated_Cave => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated), 
			ConditionFlag.BossDefeated_Garden => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.GardenBoss_Defeated), 
			ConditionFlag.BossDefeated_Final => SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.FinalBoss_Defeated), 
			ConditionFlag.Insight_HeirloomSpinKick_Hidden_Discovered => SaveManager.PlayerSaveData.GetInsightState(InsightType.HeirloomSpinKick_Hidden) > InsightState.Undiscovered, 
			ConditionFlag.Insight_HeirloomDoubleJump_Hidden_Discovered => SaveManager.PlayerSaveData.GetInsightState(InsightType.HeirloomDoubleJump_Hidden) > InsightState.Undiscovered, 
			ConditionFlag.Insight_CastleBoss_DoorOpened_Resolved => SaveManager.PlayerSaveData.GetInsightState(InsightType.CastleBoss_DoorOpened) > InsightState.DiscoveredAndViewed, 
			ConditionFlag.Insight_CastleBoss_DoorOpened_Discovered => SaveManager.PlayerSaveData.GetInsightState(InsightType.CastleBoss_DoorOpened) > InsightState.Undiscovered, 
			_ => true, 
		};
	}
}
