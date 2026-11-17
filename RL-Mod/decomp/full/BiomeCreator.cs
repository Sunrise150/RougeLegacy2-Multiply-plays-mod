using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RLWorldCreation;
using Rooms;
using UnityEngine;

public class BiomeCreator : MonoBehaviour
{
	[SerializeField]
	private bool m_displayDeadEndWarnings;

	[SerializeField]
	private bool m_drawGridGizmos;

	private static int FORCE_BUILD_ROOM_IF_TRANSITION_ROOM_DOOR_COUNT_IS_LESS_THAN = 2;

	private Queue<GridPointManager> m_buildQueue = new Queue<GridPointManager>();

	private GridController m_gridController;

	private int m_roomNumber;

	private int m_specialRoomPlacementInterval;

	private List<RoomTypeEntry> m_specialRoomPool;

	private BiomeBuildStateID m_state;

	private static string m_deadEndSeeds = "";

	private RoomRequirementsController m_roomRequirementsController;

	private Dictionary<BiomeType, int> m_timesBiomeHasConnectedTooEarly = new Dictionary<BiomeType, int>();

	private CreateRoomsBuildRule m_createRoomsBuildRule;

	private Dictionary<GridPointManager, List<DoorLocation>> m_checkedDoors = new Dictionary<GridPointManager, List<DoorLocation>>();

	public Queue<GridPointManager> BuildQueue
	{
		get
		{
			return m_buildQueue;
		}
		private set
		{
			m_buildQueue = value;
		}
	}

	public GridController GridController
	{
		get
		{
			return m_gridController;
		}
		private set
		{
			m_gridController = value;
		}
	}

	public bool IsComplete { get; private set; }

	public bool IsPaused { get; set; }

	public int RoomNumber
	{
		get
		{
			return m_roomNumber;
		}
		private set
		{
			m_roomNumber = value;
		}
	}

	public int SpecialRoomPlacementInterval
	{
		get
		{
			return m_specialRoomPlacementInterval;
		}
		set
		{
			m_specialRoomPlacementInterval = value;
		}
	}

	public List<RoomTypeEntry> SpecialRoomPool
	{
		get
		{
			return m_specialRoomPool;
		}
		set
		{
			m_specialRoomPool = value;
		}
	}

	public BiomeBuildStateID State
	{
		get
		{
			return m_state;
		}
		private set
		{
			m_state = value;
		}
	}

	public string BiomeBuildFailureDescription { get; private set; }

	public void SetState(BiomeController biomeController, BiomeBuildStateID stateID, string failureDescription = "")
	{
		if (State != stateID)
		{
			if (stateID == BiomeBuildStateID.Failed)
			{
				BiomeBuildFailureDescription = failureDescription;
			}
			if (stateID != BiomeBuildStateID.Running)
			{
				StopAllCoroutines();
			}
			State = stateID;
		}
	}

	private void BuildComplete(BiomeController biomeController)
	{
		SetState(biomeController, BiomeBuildStateID.Complete);
	}

	public GridPointManager BuildRoom(BiomeController biomeController, RoomType roomType, RoomSetEntry roomSetEntry, Vector2Int buildCoordinates, DoorLocation doorLocation, BiomeType overrideBiome = BiomeType.None, bool incrementRoomCount = true)
	{
		BiomeType biome = biomeController.Biome;
		if (overrideBiome != 0)
		{
			biome = overrideBiome;
		}
		else if (roomSetEntry.RoomMetaData.BiomeOverride != 0)
		{
			biome = roomSetEntry.RoomMetaData.BiomeOverride;
		}
		m_checkedDoors.Clear();
		RoomNumber++;
		GridPointManager gridPointManager = biomeController.GridPointManager.AddRoomToGrid(RoomNumber, roomSetEntry.RoomMetaData, roomSetEntry.IsMirrored, roomType, GridController.GetRoomGridCoordinates(buildCoordinates, roomSetEntry.RoomMetaData.Size, doorLocation), biome, incrementRoomCount);
		BuildQueue.Enqueue(gridPointManager);
		return gridPointManager;
	}

	public IEnumerator CreateBiome(BiomeController biomeController)
	{
		if (RoomLibrary.IsLoaded)
		{
			Initialise(biomeController);
			yield return SetConnectionRoom(biomeController);
			if (State != BiomeBuildStateID.Failed)
			{
				CreateTransitionRoom(biomeController);
			}
			if (State != BiomeBuildStateID.Failed)
			{
				m_roomRequirementsController = new RoomRequirementsController(biomeController);
			}
			if (State != BiomeBuildStateID.Failed)
			{
				yield return CreateRooms(biomeController);
			}
			if (State != BiomeBuildStateID.Failed)
			{
				BuildComplete(biomeController);
			}
		}
		else
		{
			string text = $"<color=red>| {this} | Cannot create {biomeController.Biome} Biome because Room Library has not been loaded.</color>";
			Debug.LogFormat(text);
			SetState(biomeController, BiomeBuildStateID.Failed, text);
		}
	}

	private IEnumerator CreateRooms(BiomeController biomeController)
	{
		m_createRoomsBuildRule = null;
		if (biomeController.Biome == BiomeType.Tower)
		{
			m_createRoomsBuildRule = new CreateRoomsBuildRule_Tower();
		}
		else if (biomeController.Biome == BiomeType.Cave)
		{
			m_createRoomsBuildRule = new CreateRoomsBuildRule_Cave();
		}
		else if (biomeController.Biome == BiomeType.Stone)
		{
			m_createRoomsBuildRule = new CreateRoomsBuildRule_Stone();
		}
		else if (biomeController.Biome == BiomeType.Forest)
		{
			m_createRoomsBuildRule = new CreateRoomsBuildRule_Forest();
		}
		else if (biomeController.Biome == BiomeType.Study)
		{
			m_createRoomsBuildRule = new CreateRoomsBuildRule_Study();
		}
		else
		{
			m_createRoomsBuildRule = new CreateRoomsBuildRule();
		}
		yield return m_createRoomsBuildRule.CreateRooms(this, biomeController);
	}

	private IEnumerator SetConnectionRoom(BiomeController biomeController)
	{
		CheckConnectionRoomsBuildRule checkConnectionRoomsBuildRule = new CheckConnectionRoomsBuildRule_Default();
		yield return checkConnectionRoomsBuildRule.UpdateConnectionPoint(this, biomeController);
	}

	private void CreateTransitionRoom(BiomeController biomeController)
	{
		BiomeType connectsToBiome = BiomeUtility.GetConnectsToBiome(biomeController.Biome);
		if (connectsToBiome != 0 && WorldBuilder.GetBiomeController(connectsToBiome) != null)
		{
			RoomSide connectsToSide = BiomeUtility.GetConnectDirection(biomeController.Biome);
			GridPoint connectionPoint = WorldBuilder.GetBiomeController(connectsToBiome).GridPointManager.GetConnectionPoint(connectsToSide);
			DoorLocation empty = DoorLocation.Empty;
			try
			{
				empty = connectionPoint.Doors.Single((DoorLocation location) => location.RoomSide == connectsToSide);
				Vector2Int doorLeadsToGridCoordinates = connectionPoint.Owner.GetDoorLeadsToGridCoordinates(empty);
				RoomMetaData transitionRoom = RoomLibrary.GetSetCollection(biomeController.Biome).TransitionRoom;
				if (transitionRoom == null)
				{
					throw new InvalidProgramException($"| {this} | No Transition Room has been set for the {biomeController.Biome} Biome in the Room Pool.");
				}
				List<DoorLocation> validDoorLocations = WorldBuilder.GetBiomeController(connectsToBiome).GridPointManager.GetValidDoorLocations(connectsToSide);
				for (int num = validDoorLocations.Count - 1; num >= 0; num--)
				{
					if (!transitionRoom.GetHasDoor(validDoorLocations[num]))
					{
						validDoorLocations.RemoveAt(num);
					}
				}
				int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, "Get Random Transition Room Door Index", 0, validDoorLocations.Count);
				BuildRoom(biomeController, RoomType.Transition, new RoomSetEntry(transitionRoom), doorLeadsToGridCoordinates, validDoorLocations[randomNumber]);
				return;
			}
			catch (Exception ex)
			{
				string text = ex.ToString();
				if (ex is InvalidOperationException)
				{
					text = string.Format("<color=red>| {0} | No available Door Location on {1} side of {2} Biome's {1} Connection Point, but there should be.</color>", this, connectsToSide, connectsToBiome);
					Debug.LogFormat(text);
				}
				else
				{
					text = $"<color=red>| {this} | An Exception occurred while attempting to place <b>{biomeController.Biome} Biome's</b> Transition Room.</color>";
					Debug.LogFormat(text);
				}
				SetState(biomeController, BiomeBuildStateID.Failed, text);
				return;
			}
		}
		RoomMetaData transitionRoom2 = RoomLibrary.GetSetCollection(biomeController.Biome).TransitionRoom;
		Vector2Int buildCoordinates = Vector2Int.zero;
		if (biomeController.Biome == BiomeType.Garden)
		{
			buildCoordinates = new Vector2Int(-30, 0);
		}
		BuildRoom(biomeController, RoomType.Transition, new RoomSetEntry(transitionRoom2), buildCoordinates, DoorLocation.Empty);
	}

	public void CreateBiomeFromSaveData(BiomeController biomeController, List<RoomSaveData> roomSaveDataCollection)
	{
		Initialise(biomeController);
		Dictionary<int, GridPointManager> dictionary = new Dictionary<int, GridPointManager>();
		for (int i = 0; i < roomSaveDataCollection.Count; i++)
		{
			RoomSaveData roomSaveData = roomSaveDataCollection[i];
			if (roomSaveData.IsTunnelDestination)
			{
				continue;
			}
			Vector2Int roomCoordinates = new Vector2Int(roomSaveData.GridCoordinatesX, roomSaveData.GridCoordinatesY);
			RoomMetaData roomMetaDataFromID = RoomLibrary.GetRoomMetaDataFromID(roomSaveData.RoomID);
			BiomeType biome = roomSaveData.BiomeType;
			if (roomSaveData.BiomeType == BiomeType.Tower)
			{
				if (roomMetaDataFromID == RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntranceLeft_Tower) || roomMetaDataFromID == RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntranceCentre_Tower) || roomMetaDataFromID == RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntranceRight_Tower))
				{
					biome = BiomeType.TowerExterior;
				}
				else
				{
					foreach (RoomSetEntry item in RoomLibrary.GetSetCollection(BiomeType.TowerExterior).RoomSet)
					{
						if (item.RoomMetaData == roomMetaDataFromID)
						{
							biome = BiomeType.TowerExterior;
							break;
						}
					}
				}
			}
			GridPointManager gridPointManager = biomeController.GridPointManager.AddRoomToGrid(roomSaveData.RoomNumber, roomMetaDataFromID, roomSaveData.IsMirrored, roomSaveData.RoomType, roomCoordinates, biome);
			gridPointManager.SetBiomeControllerIndex(roomSaveData.BiomeControllerIndex);
			gridPointManager.SetRoomSeed(roomSaveData.RoomSeed);
			if (!roomSaveData.IsTunnelDestination)
			{
				if (!dictionary.ContainsKey(roomSaveData.RoomNumber))
				{
					dictionary.Add(roomSaveData.RoomNumber, gridPointManager);
					continue;
				}
				Debug.LogFormat("Key {0} already exists", roomSaveData.RoomNumber);
			}
		}
		int num = 0;
		int num2 = 0;
		List<int> list = new List<int>();
		for (int j = 0; j < roomSaveDataCollection.Count; j++)
		{
			if (!list.Contains(roomSaveDataCollection[j].BiomeControllerIndex))
			{
				if (roomSaveDataCollection[j].IsMerged)
				{
					num2++;
				}
				else if (!roomSaveDataCollection[j].IsTunnelDestination)
				{
					num++;
				}
				list.Add(roomSaveDataCollection[j].BiomeControllerIndex);
			}
		}
		biomeController.GridPointManager.SetStandaloneRoomCount(num);
		biomeController.GridPointManager.SetMergeRoomCount(num2);
		for (int k = 0; k < roomSaveDataCollection.Count; k++)
		{
			RoomSaveData roomSaveData2 = roomSaveDataCollection[k];
			if (roomSaveData2.IsMerged)
			{
				GridPointManager gridPointManager2 = dictionary[roomSaveData2.RoomNumber];
				int biomeControllerIndex = roomSaveData2.BiomeControllerIndex;
				List<GridPointManager> list2 = new List<GridPointManager>();
				for (int l = 0; l < roomSaveData2.MergedWithRoomNumbers.Length; l++)
				{
					int key = roomSaveData2.MergedWithRoomNumbers[l];
					GridPointManager gridPointManager3 = dictionary[key];
					list2.Add(gridPointManager3);
					gridPointManager3.SetBiomeControllerIndex(biomeControllerIndex);
				}
				gridPointManager2.SetMergeWithRooms(list2);
			}
		}
		SetState(biomeController, BiomeBuildStateID.Complete);
	}

	private bool GetBuildRoomAtDoorLocation(BiomeController biomeController, GridPointManager gridPointManager, DoorLocation doorLocation)
	{
		int num = -1;
		switch (doorLocation.RoomSide)
		{
		case RoomSide.Top:
			num = biomeController.BiomeData.AttachTopOdds;
			break;
		case RoomSide.Bottom:
			num = biomeController.BiomeData.AttachBottomOdds;
			break;
		case RoomSide.Left:
			num = biomeController.BiomeData.AttachLeftOdds;
			break;
		case RoomSide.Right:
			num = biomeController.BiomeData.AttachRightOdds;
			break;
		}
		return num switch
		{
			100 => true, 
			0 => false, 
			_ => RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Should Room be built at Door Location ({doorLocation.RoomSide},{doorLocation.DoorNumber}) in Room #{gridPointManager.RoomNumber} in {gridPointManager.Biome} Biome?", 0, 100) < num, 
		};
	}

	public List<DoorLocation> GetBuildRoomAtDoorLocations(BiomeController biomeController, GridPointManager gridPointManager)
	{
		List<DoorLocation> list = new List<DoorLocation>();
		if (gridPointManager.RoomType == RoomType.Transition)
		{
			List<RoomSide> sides = new List<RoomSide>
			{
				RoomSide.Left,
				RoomSide.Right,
				RoomSide.Bottom,
				RoomSide.Top
			};
			if (BiomeUtility.GetConnectsToBiome(biomeController.Biome) != 0)
			{
				sides.Remove(RoomUtility.GetOppositeSide(BiomeUtility.GetConnectDirection(gridPointManager.Biome)));
			}
			IEnumerable<DoorLocation> enumerable = (from location in GetDoorsInRoom(gridPointManager)
				where sides.Contains(location.RoomSide) && GetIsDoorLocationValid(biomeController, gridPointManager, location)
				select location).ToList();
			list = new List<DoorLocation>(enumerable);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (!GetBuildRoomAtDoorLocation(biomeController, gridPointManager, list[num]))
				{
					list.Remove(list[num]);
				}
			}
			if (list.Count < FORCE_BUILD_ROOM_IF_TRANSITION_ROOM_DOOR_COUNT_IS_LESS_THAN)
			{
				list = enumerable.ToList();
				RoomUtility.ShuffleList(list);
				list = list.GetRange(0, Mathf.Min(list.Count, FORCE_BUILD_ROOM_IF_TRANSITION_ROOM_DOOR_COUNT_IS_LESS_THAN));
			}
			else
			{
				RoomUtility.ShuffleList(list);
			}
		}
		else
		{
			list = (from location in GetDoorsInRoom(gridPointManager)
				where GetIsDoorLocationValid(biomeController, gridPointManager, location)
				select location).ToList();
			for (int num2 = list.Count - 1; num2 >= 0; num2--)
			{
				if (!GetBuildRoomAtDoorLocation(biomeController, gridPointManager, list[num2]))
				{
					list.Remove(list[num2]);
				}
			}
			RoomUtility.ShuffleList(list);
		}
		return list;
	}

	public bool GetIsDoorLocationValid(BiomeController biomeController, GridPointManager gridPointManager, DoorLocation doorLocation)
	{
		Vector2Int doorLeadsToGridCoordinates = gridPointManager.GetDoorLeadsToGridCoordinates(doorLocation);
		if (biomeController.GridPointManager.GetAreCoordinatesWithinBorder(doorLeadsToGridCoordinates, gridPointManager.RoomType == RoomType.Standard))
		{
			return GridController.GetIsGridSpaceAvailable(doorLeadsToGridCoordinates);
		}
		return false;
	}

	public GridPointManager GetNextOriginRoom(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		if (biomeCreator.BuildQueue.Count > 0)
		{
			return biomeCreator.BuildQueue.Dequeue();
		}
		if (m_displayDeadEndWarnings)
		{
			Debug.LogFormat("<color=purple>|{0}| Dead End Hit at Room Number ({1}) in Biome ({2})</color>", this, RoomNumber, biomeController.Biome);
		}
		int num = biomeController.GridPointManager.GridPointManagers.Count - 1;
		int num2 = num - (BiomeCreation_EV.GetNumberOfRoomsToLookBack(biomeController.Biome, biomeController.GridPointManager.GridPointManagers.Count, biomeController.TargetRoomCountsByRoomType.Sum((KeyValuePair<RoomType, int> entry) => entry.Value)) - 1);
		if (num2 == 0)
		{
			num2 = 1;
		}
		List<GridPointManager> list = new List<GridPointManager>();
		for (int num3 = num; num3 >= num2; num3--)
		{
			GridPointManager gridPointManager = biomeController.GridPointManager.GridPointManagers[num3];
			if (!m_checkedDoors.ContainsKey(gridPointManager))
			{
				m_checkedDoors.Add(gridPointManager, new List<DoorLocation>());
			}
			list.Add(gridPointManager);
			List<DoorLocation> doorsInRoom = GetDoorsInRoom(gridPointManager);
			RoomUtility.ShuffleList(doorsInRoom);
			foreach (DoorLocation item in doorsInRoom)
			{
				if (!CheckDoorLocation_GetNextOriginRoom(biomeController, gridPointManager, item))
				{
					continue;
				}
				if (!m_checkedDoors[gridPointManager].Contains(item))
				{
					m_checkedDoors[gridPointManager].Add(item);
				}
				Vector2Int doorLeadsToGridCoordinates = gridPointManager.GetDoorLeadsToGridCoordinates(item);
				if (!GridController.GetIsGridSpaceAvailable(doorLeadsToGridCoordinates))
				{
					continue;
				}
				bool flag = false;
				if (m_createRoomsBuildRule is CreateRoomsBuildRule_Tower)
				{
					if ((m_createRoomsBuildRule as CreateRoomsBuildRule_Tower).GetTier(doorLeadsToGridCoordinates) != TierID.None)
					{
						flag = true;
					}
				}
				else
				{
					flag = biomeController.GridPointManager.GetAreCoordinatesWithinBorder(doorLeadsToGridCoordinates, gridPointManager.RoomType == RoomType.Standard);
				}
				if (flag)
				{
					return gridPointManager;
				}
			}
		}
		return ReplaceOriginRoom(biomeCreator, biomeController);
	}

	private bool CheckDoorLocation_GetNextOriginRoom(BiomeController biomeController, GridPointManager originRoom, DoorLocation doorLocation)
	{
		bool result = true;
		if ((doorLocation.RoomSide == RoomSide.Top && biomeController.BiomeData.AttachTopOdds == 0) || (doorLocation.RoomSide == RoomSide.Bottom && biomeController.BiomeData.AttachBottomOdds == 0) || (doorLocation.RoomSide == RoomSide.Left && biomeController.BiomeData.AttachLeftOdds == 0) || (doorLocation.RoomSide == RoomSide.Right && biomeController.BiomeData.AttachRightOdds == 0))
		{
			result = false;
		}
		else
		{
			bool flag = true;
			for (int i = 0; i < biomeController.GridPointManager.GridPointManagers.Count; i++)
			{
				if (biomeController.GridPointManager.GridPointManagers[i].RoomType == RoomType.Standard)
				{
					flag = false;
					break;
				}
			}
			if (!flag && m_checkedDoors[originRoom].Contains(doorLocation))
			{
				result = false;
			}
		}
		return result;
	}

	private GridPointManager ReplaceOriginRoom(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		List<RoomSide> sidesToReplace = BiomeCreation_EV.GetSidesToReplace(biomeController.Biome);
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(BiomeUtility.GetConnectDirection(biomeController.Biome));
		if (oppositeSide != RoomSide.None)
		{
			sidesToReplace.Remove(oppositeSide);
		}
		RoomUtility.ShuffleList(sidesToReplace);
		foreach (RoomSide item in sidesToReplace)
		{
			IEnumerable<GridPoint> gridPointsWithSpaceOnSide = GetGridPointsWithSpaceOnSide(biomeController, item, biomeController.GridPointManager.GridPoints);
			IEnumerable<GridPoint> gridPointsOfRoomType = GetGridPointsOfRoomType(biomeController, RoomType.Transition);
			List<GridPoint> list = gridPointsWithSpaceOnSide.Except(gridPointsOfRoomType).ToList();
			IEnumerable<GridPoint> gridPointsOfRoomType2 = GetGridPointsOfRoomType(biomeController, RoomType.Standard);
			list = gridPointsWithSpaceOnSide.Intersect(gridPointsOfRoomType2).ToList();
			IEnumerable<GridPoint> gridPointsOfRoomType3 = GetGridPointsOfRoomType(biomeController, RoomType.Mandatory);
			list = list.Except(gridPointsOfRoomType3).ToList();
			bool flag = false;
			while (!flag && list.Count() > 0)
			{
				m_deadEndSeeds += $"{biomeController.Biome}:{RNGSeedManager.GetCurrentSeed(base.gameObject.scene.name)}, ";
				int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, "Get Random Room To Replace", 0, list.Count());
				GridPoint gridPoint = list.ElementAt(randomNumber);
				if (ReplaceRoomUtility.ReplaceRoomMetaData(biomeController, gridPoint, item))
				{
					return gridPoint.Owner;
				}
				list.RemoveAt(randomNumber);
			}
		}
		string text = $"<color=red>|{this}| {biomeController.Biome} Biome creation failed. There are Rooms remaining, but there is nowhere to put them. Room Number ({RoomNumber} of {biomeController.TargetRoomCountsByRoomType.Sum((KeyValuePair<RoomType, int> entry) => entry.Value)})</color>";
		Debug.LogFormat(text);
		SetState(biomeController, BiomeBuildStateID.Failed, text);
		return null;
	}

	private static List<DoorLocation> GetDoorsInRoom(GridPointManager gridPointManager)
	{
		return gridPointManager.DoorLocations;
	}

	public IEnumerable<GridPoint> GetGridPointsWithAvailableDoorsOnSide(BiomeController biomeController, RoomSide side)
	{
		Func<GridPoint, bool> func = null;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			func = (GridPoint entry) => entry.Doors.Any((DoorLocation door) => door.RoomSide == RoomSide.Left && GetIsDoorLocationValid(biomeController, entry.Owner, door));
			if (side == RoomSide.Right)
			{
				func = (GridPoint entry) => entry.Doors.Any((DoorLocation door) => door.RoomSide == RoomSide.Right && GetIsDoorLocationValid(biomeController, entry.Owner, door));
			}
		}
		else
		{
			func = (GridPoint entry) => entry.Doors.Any((DoorLocation door) => door.RoomSide == RoomSide.Bottom && GetIsDoorLocationValid(biomeController, entry.Owner, door));
			if (side == RoomSide.Top)
			{
				func = (GridPoint entry) => entry.Doors.Any((DoorLocation door) => door.RoomSide == RoomSide.Top && GetIsDoorLocationValid(biomeController, entry.Owner, door));
			}
		}
		return biomeController.GridPointManager.GridPoints.Where(func);
	}

	public IEnumerable<GridPoint> GetGridPointsAtExtent(BiomeController biomeController, RoomSide side, int extentOffset)
	{
		if (extentOffset < 0)
		{
			extentOffset = 0;
		}
		Func<GridPoint, bool> func = null;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			func = (GridPoint entry) => entry.GridCoordinates.x <= biomeController.GridPointManager.Extents[RoomSide.Left] + extentOffset;
			if (side == RoomSide.Right)
			{
				func = (GridPoint entry) => entry.GridCoordinates.x + 1 >= biomeController.GridPointManager.Extents[RoomSide.Right] - extentOffset;
			}
		}
		else
		{
			func = (GridPoint entry) => entry.GridCoordinates.y <= biomeController.GridPointManager.Extents[RoomSide.Bottom] + extentOffset;
			if (side == RoomSide.Top)
			{
				func = (GridPoint entry) => entry.GridCoordinates.y + 1 >= biomeController.GridPointManager.Extents[RoomSide.Top] - extentOffset;
			}
		}
		return biomeController.GridPointManager.GridPoints.Where(func);
	}

	public IEnumerable<GridPoint> GetGridPointsOfRoomType(BiomeController biomeController, RoomType roomType)
	{
		return biomeController.GridPointManager.GridPoints.Where((GridPoint gridPoint) => gridPoint.Owner.RoomType == roomType);
	}

	public IEnumerable<GridPoint> GetGridPointsWithSpaceOnSide(BiomeController biomeController, RoomSide side, IEnumerable<GridPoint> filteredGridPoints)
	{
		int maxExtents = GridController.Extents[side];
		foreach (GridPoint filteredGridPoint in filteredGridPoints)
		{
			Vector2Int gridCoordsOnGridPointSide = GridController.GetGridCoordsOnGridPointSide(filteredGridPoint, side);
			Vector2Int roomSize = Vector2Int.zero;
			switch (side)
			{
			case RoomSide.Left:
				roomSize = new Vector2Int(gridCoordsOnGridPointSide.x + 1 - maxExtents, 1);
				break;
			case RoomSide.Right:
				roomSize = new Vector2Int(maxExtents - gridCoordsOnGridPointSide.x, 1);
				break;
			case RoomSide.Top:
				roomSize = new Vector2Int(1, maxExtents - gridCoordsOnGridPointSide.y);
				break;
			case RoomSide.Bottom:
				roomSize = new Vector2Int(1, gridCoordsOnGridPointSide.y + 1 - maxExtents);
				break;
			}
			if (GridController.GetIsSpaceForRoomAtDoor(gridCoordsOnGridPointSide, roomSize, side, 0))
			{
				yield return filteredGridPoint;
			}
		}
	}

	public List<GridPoint> GetGridPointsWhereTransitionRoomFits(BiomeController biomeController, IEnumerable<GridPoint> filteredGridPoints)
	{
		List<GridPoint> list = new List<GridPoint>();
		BiomeData data = BiomeDataLibrary.GetData(biomeController.Biome);
		RoomMetaData transitionRoom = RoomLibrary.GetSetCollection(biomeController.Biome).TransitionRoom;
		RoomSide connectToSide = data.ConnectDirection;
		IEnumerable<DoorLocation> enumerable = transitionRoom.DoorLocations.Where((DoorLocation door) => door.RoomSide == connectToSide);
		if (biomeController.Biome != BiomeType.Stone)
		{
			int num = transitionRoom.Size.x;
			if (connectToSide == RoomSide.Left || connectToSide == RoomSide.Right)
			{
				num = transitionRoom.Size.y;
			}
			for (int i = 0; i < num; i++)
			{
				RoomSide oppositeSide = RoomUtility.GetOppositeSide(connectToSide);
				DoorLocation doorLocation = new DoorLocation(oppositeSide, i);
				if (!transitionRoom.GetHasDoor(doorLocation))
				{
					continue;
				}
				foreach (GridPoint filteredGridPoint in filteredGridPoints)
				{
					Vector2Int gridCoordsOnGridPointSide = GridController.GetGridCoordsOnGridPointSide(filteredGridPoint, connectToSide);
					Vector2Int size = transitionRoom.Size;
					if (GridController.GetIsSpaceForRoomAtDoor(gridCoordsOnGridPointSide, size, connectToSide, i))
					{
						Vector2Int roomGridCoordinates = GridController.GetRoomGridCoordinates(gridCoordsOnGridPointSide, transitionRoom.Size, doorLocation);
						if (GetIsWayOutOfTransitionRoom(roomGridCoordinates, size, enumerable))
						{
							list.Add(filteredGridPoint);
						}
					}
				}
			}
		}
		else
		{
			int num2 = WorldBuilder.GetBiomeController(biomeController.BiomeData.ConnectsTo).GridPointManager.Extents[RoomSide.Right];
			int num3 = CreateRoomsBuildRule_Stone.BRIDGE_HEIGHT - transitionRoom.Size.y;
			RoomSide oppositeSide2 = RoomUtility.GetOppositeSide(connectToSide);
			foreach (DoorLocation item in transitionRoom.GetDoorsOnSide(oppositeSide2).ToList())
			{
				foreach (GridPoint filteredGridPoint2 in filteredGridPoints)
				{
					Vector2Int gridCoordsOnGridPointSide2 = GridController.GetGridCoordsOnGridPointSide(filteredGridPoint2, connectToSide);
					int x = num2 - 1 - filteredGridPoint2.GridCoordinates.x;
					Vector2Int roomSize = new Vector2Int(x, CreateRoomsBuildRule_Stone.BRIDGE_HEIGHT);
					int atDoorNumber = item.DoorNumber + num3;
					if (!GridController.GetIsSpaceForRoomAtDoor(gridCoordsOnGridPointSide2, roomSize, connectToSide, atDoorNumber))
					{
						continue;
					}
					Vector2Int roomGridCoordinates2 = GridController.GetRoomGridCoordinates(gridCoordsOnGridPointSide2, transitionRoom.Size, new DoorLocation(RoomUtility.GetOppositeSide(connectToSide), 0));
					bool flag = false;
					foreach (DoorLocation item2 in enumerable)
					{
						int doorNumber = item2.DoorNumber + num3;
						DoorLocation location = new DoorLocation(connectToSide, doorNumber);
						if (GridController.GetIsGridSpaceAvailable(GridController.GetDoorLeadsToGridCoordinates(roomGridCoordinates2, roomSize, location)))
						{
							flag = true;
							break;
						}
					}
					if (flag)
					{
						list.Add(filteredGridPoint2);
					}
				}
			}
		}
		return list;
	}

	private bool GetIsWayOutOfTransitionRoom(Vector2Int potentialGridCoordinates, Vector2Int transitionRoomSize, IEnumerable<DoorLocation> doorLocationsInTransitionRoom)
	{
		bool result = false;
		foreach (DoorLocation item in doorLocationsInTransitionRoom)
		{
			if (GridController.GetIsGridSpaceAvailable(GridController.GetDoorLeadsToGridCoordinates(potentialGridCoordinates, transitionRoomSize, item)))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public IEnumerable<GridPoint> GetGridPointsOnSideOfBiomeTransitionRoom(BiomeController biomeController, RoomSide side)
	{
		Vector2Int transitionRoomSize = RoomLibrary.GetSetCollection(biomeController.Biome).TransitionRoom.Size;
		Func<GridPoint, bool> func = null;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			func = (GridPoint entry) => entry.GridCoordinates.x <= biomeController.GridPointManager.Center.x;
			if (side == RoomSide.Right)
			{
				func = (GridPoint entry) => entry.GridCoordinates.x >= biomeController.GridPointManager.Center.x + transitionRoomSize.x - 1;
			}
		}
		else
		{
			func = (GridPoint entry) => entry.GridCoordinates.y <= biomeController.GridPointManager.Center.y;
			if (side == RoomSide.Top)
			{
				func = (GridPoint entry) => entry.GridCoordinates.y >= biomeController.GridPointManager.Center.y + transitionRoomSize.y - 1;
			}
		}
		return biomeController.GridPointManager.GridPoints.Where(func);
	}

	public IEnumerable<GridPoint> GetGridPointsWhereTransitionRoomFitsAtDoorNumber(Room transitionRoomPrefab, RoomSide attachmentSide, DoorLocation transitionRoomDoorLocation, IEnumerable<GridPoint> potentialGridPoints)
	{
		IEnumerable<DoorLocation> doorLocationsInTransitionRoom = from door in transitionRoomPrefab.Doors
			where door.Side != attachmentSide
			select new DoorLocation(door.Side, door.Number);
		foreach (GridPoint potentialGridPoint in potentialGridPoints)
		{
			if (!(transitionRoomPrefab.GetDoor(transitionRoomDoorLocation.RoomSide, transitionRoomDoorLocation.DoorNumber) == null) && !(potentialGridPoint.GetDoorLocation(attachmentSide) == DoorLocation.Empty))
			{
				Vector2Int leadsToCoords = potentialGridPoint.Owner.GetDoorLeadsToGridCoordinates(potentialGridPoint.GetDoorLocation(attachmentSide));
				if (GridController.GetIsSpaceForRoomAtDoor(leadsToCoords, transitionRoomPrefab.Size, attachmentSide, transitionRoomDoorLocation.DoorNumber) && doorLocationsInTransitionRoom.Any((DoorLocation door) => GridController.GetIsGridSpaceAvailable(GridController.GetDoorLeadsToGridCoordinates(leadsToCoords, transitionRoomPrefab.Size, door))))
				{
					yield return potentialGridPoint;
				}
			}
		}
	}

	public IEnumerable<GridPoint> GetGridPointsWhereTransitionRoomCouldAttachOnSideAtDoorNumber(BiomeController biomeController, Room transitionRoomPrefab, RoomSide side, int doorNumber)
	{
		Func<GridPoint, bool> func = null;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			func = (GridPoint entry) => GridController.GetIsSpaceForRoomAtDoor(entry.GridCoordinates + Vector2Int.left, new Vector2Int(entry.GridCoordinates.x - biomeController.GridPointManager.Extents[side], transitionRoomPrefab.Size.y), side, doorNumber);
			if (side == RoomSide.Right)
			{
				func = (GridPoint entry) => GridController.GetIsSpaceForRoomAtDoor(entry.GridCoordinates + Vector2Int.right, new Vector2Int(biomeController.GridPointManager.Extents[side] - entry.GridCoordinates.x, transitionRoomPrefab.Size.y), side, doorNumber);
			}
		}
		else
		{
			func = (GridPoint entry) => GridController.GetIsSpaceForRoomAtDoor(entry.GridCoordinates + Vector2Int.down, new Vector2Int(transitionRoomPrefab.Size.x, entry.GridCoordinates.y - biomeController.GridPointManager.Extents[side]), side, doorNumber);
			if (side == RoomSide.Top)
			{
				func = (GridPoint entry) => GridController.GetIsSpaceForRoomAtDoor(entry.GridCoordinates + Vector2Int.up, new Vector2Int(transitionRoomPrefab.Size.x, biomeController.GridPointManager.Extents[side] - entry.GridCoordinates.y), side, doorNumber);
			}
		}
		List<GridPoint> list = biomeController.GridPointManager.GridPoints.Where(func).ToList();
		IEnumerable<DoorLocation> source = from door in transitionRoomPrefab.Doors
			where door.Side != side
			select new DoorLocation(door.Side, door.Number);
		List<GridPoint> list2 = new List<GridPoint>();
		foreach (GridPoint item in list)
		{
			DoorLocation location = new DoorLocation(side, 0);
			Vector2Int leadsToCoords = item.Owner.GetDoorLeadsToGridCoordinates(location);
			if (source.Any((DoorLocation door) => GridController.GetIsGridSpaceAvailable(GridController.GetDoorLeadsToGridCoordinates(leadsToCoords, transitionRoomPrefab.Size, door))))
			{
				list2.Add(item);
			}
		}
		return list2;
	}

	public IEnumerable<GridPoint> GetPotentialConnectionPoints(BiomeController biomeController, RoomSide side)
	{
		Func<GridPoint, bool> func = null;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			func = (GridPoint entry) => entry.GridCoordinates.x == biomeController.GridPointManager.Extents[RoomSide.Left];
			if (side == RoomSide.Right)
			{
				func = (GridPoint entry) => entry.GridCoordinates.x + entry.RoomMetaData.Size.x == biomeController.GridPointManager.Extents[RoomSide.Right];
			}
		}
		else
		{
			func = (GridPoint entry) => entry.GridCoordinates.y == biomeController.GridPointManager.Extents[RoomSide.Bottom];
			if (side == RoomSide.Top)
			{
				func = (GridPoint entry) => entry.GridCoordinates.y + entry.RoomMetaData.Size.y == biomeController.GridPointManager.Extents[RoomSide.Top];
			}
		}
		return from entry in biomeController.GridPointManager.GridPoints.Where(func)
			where entry.RoomType != RoomType.Transition
			select entry;
	}

	public DoorLocation GetRandomDoorLocation(List<DoorLocation> doorLocations, RoomSetEntry roomEntry, BiomeType biome, int roomNumber)
	{
		RoomSide roomSide = doorLocations.First().RoomSide;
		List<DoorLocation> list = new List<DoorLocation>();
		if (roomEntry.IsMirrored)
		{
			foreach (DoorLocation item in roomEntry.RoomMetaData.GetDoorsOnSide(roomSide, isMirrored: true))
			{
				DoorLocation mirrorDoorLocation = RoomUtility.GetMirrorDoorLocation(roomEntry.RoomMetaData.Size, item);
				list.Add(mirrorDoorLocation);
			}
		}
		else
		{
			foreach (DoorLocation item2 in roomEntry.RoomMetaData.GetDoorsOnSide(roomSide))
			{
				list.Add(item2);
			}
		}
		doorLocations = doorLocations.Intersect(list).ToList();
		if (doorLocations.Count > 0)
		{
			int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Get Random Door Location to attach Room #{roomNumber} in {biome} Biome", 0, doorLocations.Count);
			return doorLocations[randomNumber];
		}
		string text = "";
		for (int i = 0; i < doorLocations.Count; i++)
		{
			text += $"({doorLocations[i].RoomSide},{doorLocations[i].DoorNumber})";
			if (i != doorLocations.Count - 1)
			{
				text += ", ";
			}
		}
		throw new Exception($"|{this}| Given Room Entry ({roomEntry.RoomMetaData.ID.SceneName},{roomEntry.RoomMetaData.ID.Number}) does not contain any of the given Door Locations ({text}). Take note of Seed and let Paul and/or Kenny know.");
	}

	public RoomSetEntry GetRandomRoomPrefab(BiomeController biomeController, RoomSetEntry[] potentialRooms, int rngWeightSum)
	{
		bool flag = false;
		if (potentialRooms.Length > 1)
		{
			if (flag)
			{
				int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Get Random Room Prefab to use as Room #{RoomNumber + 1} from set of {potentialRooms.Length} Potential Rooms in {biomeController.Biome} Biome.", 0, potentialRooms.Length);
				return potentialRooms.ElementAt(randomNumber);
			}
			int randomNumber2 = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Get Random Room Prefab to use as Room #{RoomNumber + 1} from set of {potentialRooms.Length} Potential Rooms in {biomeController.Biome} Biome.", 0, rngWeightSum);
			if (true)
			{
				int indexOfWeightedRoomEntry = GetIndexOfWeightedRoomEntry(randomNumber2, potentialRooms);
				if (indexOfWeightedRoomEntry >= 0 && indexOfWeightedRoomEntry < potentialRooms.Length)
				{
					return potentialRooms[indexOfWeightedRoomEntry];
				}
			}
			else
			{
				for (int i = 0; i < potentialRooms.Length; i++)
				{
					if (potentialRooms[i].Weight >= randomNumber2)
					{
						return potentialRooms[i];
					}
				}
			}
			return default(RoomSetEntry);
		}
		return potentialRooms[0];
	}

	private int GetIndexOfWeightedRoomEntry(int randomNumber, RoomSetEntry[] rooms)
	{
		int num = 0;
		int num2 = rooms.Length - 1;
		while (num != num2)
		{
			int num3 = (num2 + num) / 2;
			if (rooms[num3].Weight < randomNumber)
			{
				num = num3 + 1;
			}
			else
			{
				num2 = num3;
			}
		}
		return num;
	}

	public Dictionary<Vector2Int, List<DoorLocation>> GetRoomSizesAndWhatDoorLocationsTheyFitAt(Bounds border, Vector2Int maxRoomSize, GridPointManager originRoom, DoorLocation doorLocation)
	{
		Dictionary<Vector2Int, List<DoorLocation>> dictionary = new Dictionary<Vector2Int, List<DoorLocation>>();
		Vector2Int doorLeadsToGridCoordinates = originRoom.GetDoorLeadsToGridCoordinates(doorLocation);
		for (int num = maxRoomSize.x; num > 0; num--)
		{
			for (int num2 = maxRoomSize.y; num2 > 0; num2--)
			{
				Vector2Int vector2Int = new Vector2Int(num, num2);
				List<DoorLocation> doorLocationsThatFitAtCoordinates = GridController.GetDoorLocationsThatFitAtCoordinates(doorLeadsToGridCoordinates, border, doorLocation.RoomSide, vector2Int);
				if (doorLocationsThatFitAtCoordinates.Count > 0)
				{
					dictionary.Add(vector2Int, doorLocationsThatFitAtCoordinates);
				}
			}
		}
		return dictionary;
	}

	public void AddRoomTypeToBacklog(RoomTypeEntry entry)
	{
		m_roomRequirementsController.AddToBacklog(entry);
	}

	public RoomTypeEntry GetTargetRoomRequirements()
	{
		return m_roomRequirementsController.GetRequirements(RoomNumber);
	}

	private void Initialise(BiomeController biomeController)
	{
		SetState(biomeController, BiomeBuildStateID.Running);
		if (BuildQueue == null)
		{
			BuildQueue = new Queue<GridPointManager>();
		}
		else
		{
			BuildQueue.Clear();
		}
		if (GridController == null)
		{
			GridController = new GridController();
		}
		RoomNumber = 0;
		SetBiomeRoomCounts(biomeController);
	}

	public void Reset()
	{
		m_buildQueue = new Queue<GridPointManager>();
		m_roomNumber = 0;
		m_specialRoomPlacementInterval = 0;
		m_specialRoomPool = null;
		m_state = BiomeBuildStateID.None;
	}

	private void SetBiomeRoomCounts(BiomeController biomeController)
	{
		biomeController.TargetRoomCountsByRoomType = new Dictionary<RoomType, int>();
		float num = 1f + BurdenManager.GetBurdenStatGain(BurdenType.RoomCount);
		int minInclusive = Mathf.RoundToInt((float)biomeController.BiomeData.MinFairyRoomCount * num);
		int num2 = Mathf.RoundToInt((float)biomeController.BiomeData.MaxFairyRoomCount * num);
		int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Get Fairy Room Count in {biomeController.Biome} Biome", minInclusive, num2 + 1);
		biomeController.TargetRoomCountsByRoomType[RoomType.Fairy] = randomNumber;
		int minInclusive2 = Mathf.RoundToInt((float)biomeController.BiomeData.MinRoomCount * num);
		int num3 = Mathf.RoundToInt((float)biomeController.BiomeData.MaxRoomCount * num);
		int randomNumber2 = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Get Standard Room Count in {biomeController.Biome} Biome", minInclusive2, num3 + 1);
		biomeController.TargetRoomCountsByRoomType[RoomType.Standard] = randomNumber2;
		int minInclusive3 = Mathf.RoundToInt((float)biomeController.BiomeData.MinTrapRoomCount * num);
		int num4 = Mathf.RoundToInt((float)biomeController.BiomeData.MaxTrapRoomCount * num);
		int randomNumber3 = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Get Trap Room Count in {biomeController.Biome} Biome", minInclusive3, num4 + 1);
		biomeController.TargetRoomCountsByRoomType[RoomType.Trap] = randomNumber3;
		int minInclusive4 = Mathf.RoundToInt((float)biomeController.BiomeData.MinBonusRoomCount * num);
		int num5 = Mathf.RoundToInt((float)biomeController.BiomeData.MaxBonusRoomCount * num);
		int randomNumber4 = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Get Bonus Room Count in {biomeController.Biome} Biome", minInclusive4, num5 + 1);
		biomeController.TargetRoomCountsByRoomType[RoomType.Bonus] = randomNumber4;
		int value = RoomLibrary.GetSetCollection(biomeController.Biome).MandatoryRooms.Length;
		biomeController.TargetRoomCountsByRoomType[RoomType.Mandatory] = value;
	}

	private void SetPremadeBiomeRoomCounts(BiomeController biomeController, List<Room> roomList)
	{
		biomeController.TargetRoomCountsByRoomType = new Dictionary<RoomType, int>();
		biomeController.TargetRoomCountsByRoomType[RoomType.Fairy] = 0;
		biomeController.TargetRoomCountsByRoomType[RoomType.Standard] = roomList.Count;
		biomeController.TargetRoomCountsByRoomType[RoomType.Trap] = 0;
		biomeController.TargetRoomCountsByRoomType[RoomType.Bonus] = 0;
		biomeController.TargetRoomCountsByRoomType[RoomType.Mandatory] = 0;
	}

	private IEnumerator BuildForcedRooms(List<Room> roomList)
	{
		yield break;
	}
}
