using System;
using System.Collections.Generic;
using GameEventTracking;

[Serializable]
public class StageSaveData : IVersionUpdateable
{
	public int REVISION_NUMBER = 14;

	public int FILE_NUMBER;

	public int BiomeCreationSeed = -1;

	public int MergeRoomSeed = -1;

	public int EnemySeed = -1;

	public int PropSeed = -1;

	private Dictionary<BiomeType, List<RoomSaveData>> m_roomSaveDataDict = new Dictionary<BiomeType, List<RoomSaveData>>();

	[NonSerialized]
	private Dictionary<BiomeType, Dictionary<int, RoomSaveData>> m_roomSaveDataLookupTable;

	public int TimesTrackerWasLoaded;

	public List<EnemyTrackerData> EnemyTrackerDataList;

	public List<ChestTrackerData> ChestTrackerDataList;

	public List<ItemTrackerData> ItemTrackerDataList;

	public List<RoomTrackerData> RoomTrackerDataList;

	public bool ForceResetWorld;

	public Dictionary<BiomeType, List<RoomSaveData>> RoomSaveDataDict
	{
		get
		{
			return m_roomSaveDataDict;
		}
		private set
		{
			m_roomSaveDataDict = value;
		}
	}

	public void LinkTrackerData()
	{
		RoomTrackerDataList = GameEventTrackerManager.RoomEventTracker.RoomsEntered;
		EnemyTrackerDataList = GameEventTrackerManager.EnemyEventTracker.EnemiesKilled;
		ChestTrackerDataList = GameEventTrackerManager.ItemEventTracker.ChestsOpened;
		ItemTrackerDataList = GameEventTrackerManager.ItemEventTracker.ItemsCollected;
	}

	public RoomSaveData GetRoomSaveData(BiomeType biomeType, int biomeControllerIndex)
	{
		Dictionary<int, RoomSaveData> roomDataLookupTable = GetRoomDataLookupTable(biomeType);
		if (roomDataLookupTable != null && roomDataLookupTable.ContainsKey(biomeControllerIndex))
		{
			return roomDataLookupTable[biomeControllerIndex];
		}
		return null;
	}

	public Dictionary<int, RoomSaveData> GetRoomDataLookupTable(BiomeType biomeType)
	{
		biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
		if (m_roomSaveDataLookupTable.TryGetValue(biomeType, out var value))
		{
			return value;
		}
		return null;
	}

	public List<RoomSaveData> GetRoomDataList(BiomeType biomeType)
	{
		biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
		if (RoomSaveDataDict.TryGetValue(biomeType, out var value))
		{
			return value;
		}
		return null;
	}

	public void CreateLevelEditorRoomSaveDataList(BaseRoom levelEditorRoom)
	{
		RoomSaveDataDict.Clear();
		BiomeType biomeType = levelEditorRoom.BiomeType;
		biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
		List<RoomSaveData> list = new List<RoomSaveData>();
		foreach (BaseRoom room in OnPlayManager.RoomList)
		{
			RoomSaveData roomSaveData = CreateRoomSaveData(biomeType, (room as Room).GridPointManager);
			roomSaveData.BiomeType = biomeType;
			list.Add(roomSaveData);
		}
		RoomSaveDataDict.Add(biomeType, list);
		CreateAllBiomeLookupTable();
	}

	public void CreateEmptyRoomSaveDataList()
	{
		RoomSaveDataDict.Clear();
		foreach (KeyValuePair<BiomeType, BiomeController> biomeController in WorldBuilder.BiomeControllers)
		{
			BiomeController value = biomeController.Value;
			BiomeType groupedBiomeType = BiomeType_RL.GetGroupedBiomeType(value.Biome);
			List<GridPointManager> gridPointManagers = value.GridPointManager.GridPointManagers;
			gridPointManagers.AddRange(value.GridPointManager.TunnelDestinationGridPointManagers);
			List<RoomSaveData> list = new List<RoomSaveData>();
			for (int i = 0; i < gridPointManagers.Count; i++)
			{
				GridPointManager gridPointRoom = gridPointManagers[i];
				RoomSaveData item = CreateRoomSaveData(groupedBiomeType, gridPointRoom);
				list.Add(item);
			}
			RoomSaveDataDict.Add(groupedBiomeType, list);
		}
		CreateAllBiomeLookupTable();
	}

	private static RoomSaveData CreateRoomSaveData(BiomeType biomeType, GridPointManager gridPointRoom)
	{
		RoomSaveData roomSaveData = new RoomSaveData();
		roomSaveData.RoomName = gridPointRoom.RoomMetaData.name;
		roomSaveData.RoomNumber = gridPointRoom.RoomNumber;
		roomSaveData.BiomeType = biomeType;
		roomSaveData.RoomType = gridPointRoom.RoomType;
		roomSaveData.IsMirrored = gridPointRoom.IsRoomMirrored;
		roomSaveData.IsTunnelDestination = gridPointRoom.IsTunnelDestination;
		roomSaveData.BiomeControllerIndex = gridPointRoom.BiomeControllerIndex;
		roomSaveData.RoomSeed = gridPointRoom.RoomSeed;
		roomSaveData.RoomID = gridPointRoom.RoomMetaData.ID;
		roomSaveData.GridCoordinatesX = gridPointRoom.GridCoordinates.x;
		roomSaveData.GridCoordinatesY = gridPointRoom.GridCoordinates.y;
		roomSaveData.IsMerged = gridPointRoom.MergeWithGridPointManagers.Count > 0;
		roomSaveData.MergedWithRoomNumbers = new int[gridPointRoom.MergeWithGridPointManagers.Count];
		for (int i = 0; i < gridPointRoom.MergeWithGridPointManagers.Count; i++)
		{
			roomSaveData.MergedWithRoomNumbers[i] = gridPointRoom.MergeWithGridPointManagers[i].RoomNumber;
		}
		return roomSaveData;
	}

	public void UpdateVersion()
	{
		if (REVISION_NUMBER != 14)
		{
			ForceResetWorld = true;
		}
		REVISION_NUMBER = 14;
	}

	public void VerifyBiomeSaveData(BiomeType biome)
	{
		BiomeController biomeController = WorldBuilder.GetBiomeController(biome);
		if (!(biomeController != null))
		{
			return;
		}
		int count = biomeController.GridPointManager.GridPointManagers.Count;
		int count2 = biomeController.GridPointManager.TunnelDestinationGridPointManagers.Count;
		int num = count + count2;
		if (!m_roomSaveDataDict.ContainsKey(biome))
		{
			return;
		}
		int count3 = m_roomSaveDataDict[biome].Count;
		if (num != count3)
		{
			for (int i = 0; i < biomeController.GridPointManager.TunnelDestinationGridPointManagers.Count; i++)
			{
				GridPointManager gridPointRoom = biomeController.GridPointManager.TunnelDestinationGridPointManagers[i];
				RoomSaveData item = CreateRoomSaveData(biome, gridPointRoom);
				m_roomSaveDataDict[biome].Add(item);
			}
			CreateRoomSaveDataLookupTable(biome);
		}
	}

	public void CreateAllBiomeLookupTable()
	{
		foreach (KeyValuePair<BiomeType, List<RoomSaveData>> item in RoomSaveDataDict)
		{
			CreateRoomSaveDataLookupTable(item.Key);
		}
	}

	public void CreateRoomSaveDataLookupTable(BiomeType biome)
	{
		if (m_roomSaveDataLookupTable == null)
		{
			m_roomSaveDataLookupTable = new Dictionary<BiomeType, Dictionary<int, RoomSaveData>>();
		}
		if (!RoomSaveDataDict.TryGetValue(biome, out var value))
		{
			return;
		}
		Dictionary<int, RoomSaveData> value2 = null;
		if (m_roomSaveDataLookupTable.TryGetValue(biome, out value2))
		{
			value2.Clear();
		}
		else
		{
			value2 = new Dictionary<int, RoomSaveData>();
			m_roomSaveDataLookupTable.Add(biome, value2);
		}
		foreach (RoomSaveData item in value)
		{
			if (!value2.ContainsKey(item.BiomeControllerIndex))
			{
				value2.Add(item.BiomeControllerIndex, item);
			}
		}
	}
}
