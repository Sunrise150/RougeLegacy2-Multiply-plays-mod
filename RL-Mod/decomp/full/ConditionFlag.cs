using System;

[Flags]
public enum ConditionFlag
{
	None = 0,
	Heirloom_Dash = 1,
	Heirloom_Memory = 2,
	Heirloom_DoubleJump = 4,
	Heirloom_Downstrike = 8,
	Heirloom_VoidDash = 0x10,
	Insight_HeirloomSpinKick_Hidden_Discovered = 0x400,
	Insight_HeirloomDoubleJump_Hidden_Discovered = 0x800,
	Insight_CastleBoss_DoorOpened_Resolved = 0x8000,
	Insight_CastleBoss_DoorOpened_Discovered = 0x10000,
	BossDefeated_Castle = 0x100000,
	BossDefeated_Bridge = 0x200000,
	BossDefeated_Forest = 0x400000,
	BossDefeated_Study = 0x800000,
	BossDefeated_Tower = 0x1000000,
	BossDefeated_Cave = 0x2000000,
	BossDefeated_Final = 0x4000000,
	BossDefeated_Garden = 0x8000000
}
