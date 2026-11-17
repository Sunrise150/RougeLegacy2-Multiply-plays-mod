using System;

[Serializable]
public class RoomSaveData
{
	public string RoomName;

	public BiomeType BiomeType;

	public bool RoomVisited;

	public bool IsRoomComplete;

	public int RoomSeed;

	public RoomID RoomID;

	public int GridCoordinatesX;

	public int GridCoordinatesY;

	public bool IsMerged;

	public int RoomNumber;

	public int[] MergedWithRoomNumbers;

	public int[] RoomCompleteBiomeControllerIndexOverrides;

	public string RoomMiscData;

	public RLBreakableSaveState[] BreakableStates;

	public RLBreakableSaveState[] DecoBreakableStates;

	public RLSaveState[] ChestStates;

	public RLSaveState[] EnemyStates;

	public RoomType RoomType;

	public bool IsMirrored;

	public bool IsTunnelDestination;

	public int BiomeControllerIndex;

	public bool IsEmpty
	{
		get
		{
			if (BreakableStates != null && ChestStates != null && EnemyStates != null)
			{
				return DecoBreakableStates == null;
			}
			return true;
		}
	}

	public void Clear()
	{
		RoomName = null;
		BiomeType = BiomeType.None;
		RoomVisited = false;
		IsRoomComplete = false;
		RoomSeed = 0;
		RoomID = default(RoomID);
		GridCoordinatesX = 0;
		GridCoordinatesY = 0;
		IsMerged = false;
		RoomNumber = 0;
		MergedWithRoomNumbers = null;
		RoomMiscData = null;
		RoomCompleteBiomeControllerIndexOverrides = null;
		BreakableStates = null;
		ChestStates = null;
		EnemyStates = null;
		DecoBreakableStates = null;
		RoomType = RoomType.None;
		IsMirrored = false;
		IsTunnelDestination = false;
		BiomeControllerIndex = 0;
	}
}
