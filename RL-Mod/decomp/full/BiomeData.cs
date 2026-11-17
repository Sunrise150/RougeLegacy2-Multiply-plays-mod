using UnityEngine;

public class BiomeData : ScriptableObject
{
	[Header("Room Counts")]
	public int MinRoomCount;

	public int MaxRoomCount;

	[Space]
	public int MinFairyRoomCount;

	public int MaxFairyRoomCount;

	[Space]
	public int MinBonusRoomCount;

	public int MaxBonusRoomCount;

	[Space]
	public BonusRoomWeightEntry[] BonusRoomWeights;

	[Space]
	public int MinTrapRoomCount;

	public int MaxTrapRoomCount;

	[Space]
	[Header("Enemies")]
	public float RoomLevelScale;

	[Space]
	public int EnemyStartLevel;

	public int BossStartLevel;

	[Space]
	[Header("Odds")]
	public int AttachTopOdds;

	public int AttachBottomOdds;

	public int AttachLeftOdds;

	public int AttachRightOdds;

	[Space]
	public int MergeTopOdds;

	public int MergeBottomOdds;

	public int MergeLeftOdds;

	public int MergeRightOdds;

	[Space]
	public int MaxMergeVerticalCount;

	public int MaxMergeHorizontalCount;

	[Space]
	[Header("Connection")]
	public BiomeType ConnectsTo;

	public RoomSide ConnectDirection;

	[Space]
	[Header("Localization")]
	public string BiomeNameLocID;
}
