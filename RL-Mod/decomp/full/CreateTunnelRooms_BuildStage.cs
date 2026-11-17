using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

public class CreateTunnelRooms_BuildStage : IBiomeBuildStage
{
	private static Vector2Int DESTINATION_GRID_COORDINATES = new Vector2Int(-50, 50);

	private const string SPECIAL_ROOM_LOBBY_PATH = "Scriptable Objects/Room Meta Data/Levels Linker/Levels Linker_28_RoomMetaData";

	public static EventHandler<RoomEventArgs> RoomCreatedEvent;

	private static RoomEventArgs m_roomCreatedEventArgs;

	private static List<RoomID> m_destinationRooms = new List<RoomID>();

	private static Dictionary<Tunnel, List<Room>> m_rootToRoomsTable = new Dictionary<Tunnel, List<Room>>();

	private static RoomMetaData m_lobbyMetaData = null;

	public IEnumerator Run(BiomeController biomeController)
	{
		TunnelSpawnController[] tunnelSpawnControllers = biomeController.TunnelSpawnControllers;
		if (tunnelSpawnControllers == null || tunnelSpawnControllers.Length == 0)
		{
			yield break;
		}
		RNGManager.SetSeed(RngID.Tunnel_RoomSeed, (tunnelSpawnControllers[0].Room as Room).GridPointManager.TunnelRoomSeed);
		for (int i = 0; i < tunnelSpawnControllers.Length; i++)
		{
			if (biomeController.Biome != BiomeType.Castle || !tunnelSpawnControllers[i].ShouldSpawn || !(tunnelSpawnControllers[i].Room != null) || tunnelSpawnControllers[i].Room.RoomType != RoomType.Transition || !(tunnelSpawnControllers[i].Tunnel != null))
			{
				if (tunnelSpawnControllers[i].SpawnLogicController != null)
				{
					tunnelSpawnControllers[i].SpawnLogicController.RunIsSpawnedCheck();
				}
				if (tunnelSpawnControllers[i].ShouldSpawn && tunnelSpawnControllers[i].Direction == TunnelDirection.Entrance)
				{
					tunnelSpawnControllers[i].Spawn();
					m_destinationRooms.Clear();
					m_rootToRoomsTable.Clear();
					yield return CreateDestination(biomeController, tunnelSpawnControllers[i], null);
				}
			}
		}
	}

	public static IEnumerator RunFromLevelEditor(BiomeController biomeController, Room room)
	{
		if (!GameUtility.IsInLevelEditor)
		{
			yield break;
		}
		m_destinationRooms.Clear();
		m_rootToRoomsTable.Clear();
		TunnelSpawnController[] tunnelSpawnControllers = room.GetComponentsInChildren<TunnelSpawnController>(includeInactive: true);
		if (tunnelSpawnControllers == null || tunnelSpawnControllers.Length == 0)
		{
			yield break;
		}
		for (int i = 0; i < tunnelSpawnControllers.Length; i++)
		{
			if (tunnelSpawnControllers[i].ShouldSpawn && tunnelSpawnControllers[i].Direction == TunnelDirection.Entrance)
			{
				yield return CreateDestination(biomeController, tunnelSpawnControllers[i], null);
			}
		}
	}

	private static Tunnel GetTunnelPrefab(TunnelCategory category)
	{
		return TunnelLibrary.GetPrefab(category);
	}

	private static IEnumerator CreateDestination(BiomeController biomeController, TunnelSpawnController tunnelSpawnController, Tunnel entrance)
	{
		Tunnel tunnel = CreateTunnelInstance(tunnelSpawnController, entrance);
		if (tunnelSpawnController.Direction == TunnelDirection.Entrance)
		{
			if (!tunnelSpawnController.IsDestinationRoot)
			{
				if (!GetIsTunnelToSpecialRoom(tunnelSpawnController, out var specialRoomType))
				{
					if (!GetHasRoomInChainBeenInstantiated(tunnelSpawnController, out var room))
					{
						yield return DestinationDoesNotExist(biomeController, tunnel, tunnelSpawnController);
					}
					else
					{
						DestinationAlreadyExists(tunnel, room);
					}
				}
				else
				{
					int roomSeed = GetRoomSeed(tunnelSpawnController.Room);
					GridPointManager gridPointManager = CreateLobbyGridPointManager(biomeController, specialRoomType);
					gridPointManager.SetRoomSeed(roomSeed);
					GridPointManager gridPointManager2 = CreateSpecialRoomGridPointManager(biomeController, gridPointManager, specialRoomType);
					gridPointManager2.SetRoomSeed(roomSeed);
					yield return CreateSpecialRoomDestination(biomeController, tunnel, gridPointManager, gridPointManager2, roomSeed);
				}
			}
		}
		else if (entrance.Index == tunnel.Index)
		{
			ConnectTunnels(entrance, tunnel);
		}
		if ((bool)entrance)
		{
			tunnelSpawnController.Room.gameObject.SetActive(value: false);
		}
	}

	private static IEnumerator DestinationDoesNotExist(BiomeController biomeController, Tunnel tunnelInstance, TunnelSpawnController tunnelSpawnController)
	{
		Room room = CreateRoomInstance(biomeController, tunnelSpawnController.DestinationRoomPrefab, tunnelSpawnController.LeadsToRoomType);
		if (!m_destinationRooms.Contains(room.RoomID))
		{
			m_destinationRooms.Add(room.RoomID);
			m_rootToRoomsTable[tunnelInstance.Root].Add(room);
			InitializeDestinationRoom(biomeController, tunnelSpawnController, room, DESTINATION_GRID_COORDINATES);
			TunnelSpawnController[] destinationTunnelSpawnControllers = room.GetComponentsInChildren<TunnelSpawnController>(includeInactive: true);
			for (int i = 0; i < destinationTunnelSpawnControllers.Length; i++)
			{
				yield return CreateDestination(biomeController, destinationTunnelSpawnControllers[i], tunnelInstance);
			}
		}
		else
		{
			_ = Application.isEditor;
		}
	}

	private static void DestinationAlreadyExists(Tunnel tunnelInstance, Room roomInstance)
	{
		ISpawnController[] spawnControllers = roomInstance.SpawnControllerManager.SpawnControllers;
		for (int i = 0; i < spawnControllers.Length; i++)
		{
			TunnelSpawnController tunnelSpawnController = spawnControllers[i] as TunnelSpawnController;
			if (!(tunnelSpawnController != null) || tunnelSpawnController.Index != tunnelInstance.Index)
			{
				continue;
			}
			if (tunnelSpawnController.Direction != TunnelDirection.Exit || !tunnelSpawnController.ShouldSpawn)
			{
				throw new InvalidOperationException("An error occurred while attempting to connect two Tunnels");
			}
			Tunnel tunnel = tunnelSpawnController.Tunnel;
			if (tunnel == null)
			{
				tunnel = CreateTunnelInstance(tunnelSpawnController);
				tunnel.SetRoot(tunnelInstance.Root);
				tunnelSpawnController.SetTunnelInstance(tunnel);
			}
			ConnectTunnels(tunnelInstance, tunnelSpawnController.Tunnel);
			break;
		}
	}

	private static Tunnel CreateTunnelInstance(TunnelSpawnController tunnelSpawnController, Tunnel entrance)
	{
		if (tunnelSpawnController.IsDestinationRoot && entrance == null)
		{
			if (!GameUtility.IsInLevelEditor)
			{
				throw new ArgumentException($"| CreateTunnelRooms_BuildStage | Given Spawn Controller's destination is Root, but entrance argument is null.");
			}
			Debug.Log($"<color=red>| CreateTunnelRooms_BuildStage | Given Spawn Controller's destination is Root, but entrance argument is null.</color>");
			return null;
		}
		Tunnel tunnel = CreateTunnelInstance(tunnelSpawnController);
		tunnel.SetIsDestinationRoot(tunnelSpawnController.IsDestinationRoot);
		if (entrance == null)
		{
			m_rootToRoomsTable.Add(tunnel, new List<Room>());
			tunnel.SetRoot(tunnel);
		}
		else
		{
			tunnel.SetRoot(entrance.Root);
		}
		tunnelSpawnController.SetTunnelInstance(tunnel);
		return tunnel;
	}

	private static bool GetHasRoomInChainBeenInstantiated(TunnelSpawnController spawnController, out Room room)
	{
		room = null;
		bool result = false;
		Tunnel root = spawnController.Tunnel.Root;
		List<Room> list = m_rootToRoomsTable[root];
		for (int i = 0; i < list.Count; i++)
		{
			Room room2 = m_rootToRoomsTable[root][i];
			if (spawnController.DestinationRoomPrefab.RoomID == room2.RoomID)
			{
				result = true;
				room = room2;
				break;
			}
		}
		return result;
	}

	private static GridPointManager CreateSpecialRoomGridPointManager(BiomeController biomeController, GridPointManager lobby, RoomType roomType)
	{
		if (RoomLibrary.IsInstantiated && !RoomLibrary.IsLoaded)
		{
			RoomLibrary.LoadCompiledRoomsFromLevelEditor();
		}
		List<RoomSetEntry> list = RoomLibrary.GetSetCollection(BiomeType.Castle).RoomTypeSets[roomType].ToList();
		if (roomType == RoomType.Bonus)
		{
			SpecialRoomType randomBonusRoomType = CreateRoomsBuildRule.GetRandomBonusRoomType(biomeController, RngID.Tunnel_RoomSeed);
			for (int num = list.Count - 1; num >= 0; num--)
			{
				if (list[num].RoomMetaData.SpecialRoomType != randomBonusRoomType)
				{
					list.RemoveAt(num);
				}
			}
		}
		int index = 0;
		if (list.Count > 1)
		{
			index = RNGManager.GetRandomNumber(RngID.Tunnel_RoomSeed, "Get Random Special Room for Tunnel Destination", 0, list.Count);
		}
		RoomSetEntry roomSetEntry = list[index];
		int num2 = 0;
		if (roomSetEntry.RoomMetaData.DoorLocations.Length > 1)
		{
			num2 = RNGManager.GetRandomNumber(RngID.Tunnel_RoomSeed, "Choose which Door will connect to Lobby", 0, roomSetEntry.RoomMetaData.DoorLocations.Length);
		}
		DoorLocation doorLocation = roomSetEntry.RoomMetaData.DoorLocations[num2];
		if (roomSetEntry.IsMirrored)
		{
			doorLocation = RoomUtility.GetMirrorDoorLocation(roomSetEntry.RoomMetaData.Size, doorLocation);
		}
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(doorLocation.RoomSide);
		DoorLocation location = new DoorLocation(oppositeSide, 0);
		Vector2Int roomGridCoordinates = GridController.GetRoomGridCoordinates(lobby.GetDoorLeadsToGridCoordinates(location), roomSetEntry.RoomMetaData.Size, doorLocation);
		BiomeType tunnelDestinationBiome = GetTunnelDestinationBiome(biomeController);
		return CreateGridPointManager(biomeController, roomSetEntry.RoomMetaData, roomType, roomGridCoordinates, roomSetEntry.IsMirrored, tunnelDestinationBiome);
	}

	private static BiomeType GetTunnelDestinationBiome(BiomeController biomeController)
	{
		BiomeType result = biomeController.Biome;
		if (BiomeCreation_EV.TUNNEL_DESTINATION_BIOME_TYPE_TABLE.ContainsKey(biomeController.Biome))
		{
			result = BiomeCreation_EV.TUNNEL_DESTINATION_BIOME_TYPE_TABLE[biomeController.Biome];
		}
		return result;
	}

	private static GridPointManager CreateLobbyGridPointManager(BiomeController biomeController, RoomType roomType)
	{
		if (m_lobbyMetaData == null)
		{
			m_lobbyMetaData = CDGResources.Load<RoomMetaData>("Scriptable Objects/Room Meta Data/Levels Linker/Levels Linker_28_RoomMetaData");
		}
		BiomeType tunnelDestinationBiome = GetTunnelDestinationBiome(biomeController);
		return CreateGridPointManager(biomeController, m_lobbyMetaData, RoomType.Standard, DESTINATION_GRID_COORDINATES, isRoomMirrored: false, tunnelDestinationBiome);
	}

	private static bool GetIsTunnelToSpecialRoom(TunnelSpawnController tunnelSpawnController, out RoomType specialRoomType)
	{
		bool num = tunnelSpawnController.LeadsToRoomType == RoomType.Fairy || tunnelSpawnController.LeadsToRoomType == RoomType.Bonus;
		bool flag = tunnelSpawnController.DestinationRoomPrefabsOverride != null && tunnelSpawnController.DestinationRoomPrefabsOverride.Length == 0;
		if (num && flag)
		{
			specialRoomType = tunnelSpawnController.LeadsToRoomType;
			return true;
		}
		specialRoomType = RoomType.None;
		return false;
	}

	private static void ConnectTunnels(Tunnel entrance, Tunnel exit)
	{
		entrance.SetDestination(exit);
		exit.SetDestination(entrance);
		exit.SetRoot(entrance.Root);
	}

	private static Tunnel CreateTunnelInstance(TunnelSpawnController tunnelSpawnController)
	{
		Tunnel tunnel = null;
		if (tunnelSpawnController.TunnelPrefabOverride != null)
		{
			tunnel = tunnelSpawnController.TunnelPrefabOverride.GetComponent<Tunnel>();
		}
		if (tunnel == null)
		{
			tunnel = GetTunnelPrefab(tunnelSpawnController.Category);
		}
		Tunnel tunnel2 = UnityEngine.Object.Instantiate(tunnel, tunnelSpawnController.Room.gameObject.transform);
		tunnel2.SetIsLocked(tunnelSpawnController.IsLocked);
		tunnel2.SetRoom(tunnelSpawnController.Room);
		tunnel2.SetIndex(tunnelSpawnController.Index);
		tunnel2.SetIsVisible(tunnelSpawnController.ShowTunnel);
		tunnel2.SetTransitionType(tunnelSpawnController.TransitionType);
		tunnel2.transform.position = tunnelSpawnController.GameObject.transform.position;
		return tunnel2;
	}

	private static Room CreateRoomInstance(BiomeController biomeController, Room roomPrefab, RoomType roomType)
	{
		Transform parent = null;
		if ((bool)biomeController)
		{
			parent = biomeController.StandardRoomStorageLocation;
			if (roomType == RoomType.Boss)
			{
				parent = biomeController.RoomsConnectedByTunnelStorageLocation;
			}
		}
		Room room = UnityEngine.Object.Instantiate(roomPrefab, parent);
		room.BiomeType = biomeController.Biome;
		InvokeRoomCreatedEvent(room);
		return room;
	}

	private static void InvokeRoomCreatedEvent(Room room)
	{
		if (m_roomCreatedEventArgs == null)
		{
			m_roomCreatedEventArgs = new RoomEventArgs(room);
		}
		else
		{
			m_roomCreatedEventArgs.Initialize(room);
		}
		if (RoomCreatedEvent != null)
		{
			RoomCreatedEvent(null, m_roomCreatedEventArgs);
		}
	}

	private static int GetNextRoomNumber(BiomeController biomeController)
	{
		return biomeController.GridPointManager.TunnelDestinationGridPointManagers.Count;
	}

	private static int GetNextBiomeControllerIndex(BiomeController biomeController)
	{
		return biomeController.GridPointManager.StandaloneRoomCount + biomeController.GridPointManager.MergeRoomCount + biomeController.GridPointManager.TunnelDestinationCount - 1;
	}

	private static GridPointManager CreateGridPointManager(BiomeController biomeController, RoomMetaData roomMetaData, RoomType roomType, Vector2Int gridCoords, bool isRoomMirrored = false, BiomeType biomeOverride = BiomeType.None)
	{
		GridPointManager gridPointManager = new GridPointManager(GetNextRoomNumber(biomeController), gridCoords, biomeController.Biome, roomType, roomMetaData, isRoomMirrored, isTunnelDestination: true);
		if (biomeOverride != 0 && gridPointManager.AppearanceOverride == BiomeType.None)
		{
			gridPointManager.SetAppearanceOverride(biomeOverride);
		}
		biomeController.GridPointManager.AddTunnelDestinationGridPointManager(gridPointManager);
		int nextBiomeControllerIndex = GetNextBiomeControllerIndex(biomeController);
		gridPointManager.SetBiomeControllerIndex(nextBiomeControllerIndex);
		return gridPointManager;
	}

	private static IEnumerator CreateSpecialRoomDestination(BiomeController biomeController, Tunnel tunnelToLobby, GridPointManager lobby, GridPointManager room, int roomSeed)
	{
		Room lobbyInstance = InstantiateRooms_BuildStage.CreateRoomInstance(biomeController, lobby);
		lobbyInstance.gameObject.transform.parent = biomeController.RoomsConnectedByTunnelStorageLocation;
		biomeController.RoomConnectedByTunnelCreatedByWorldBuilder(lobbyInstance);
		InvokeRoomCreatedEvent(lobbyInstance);
		Tunnel exit = CreateTunnelInstance(lobbyInstance.gameObject.GetComponentInChildren<TunnelSpawnController>());
		ConnectTunnels(tunnelToLobby, exit);
		Room roomInstance = InstantiateRooms_BuildStage.CreateRoomInstance(biomeController, room);
		roomInstance.gameObject.transform.parent = biomeController.RoomsConnectedByTunnelStorageLocation;
		yield return new WaitUntil(() => lobbyInstance.Doors.Any((Door door) => door.ConnectedDoor != null));
		List<Door> doorsOnSide = lobbyInstance.GetDoorsOnSide(RoomSide.Bottom);
		for (int j = 0; j < doorsOnSide.Count; j++)
		{
			BiomeCreatorTools.PlaceOneWayAtDoor(doorsOnSide[j]);
		}
		List<Door> doorsOnSide2 = roomInstance.GetDoorsOnSide(RoomSide.Bottom);
		for (int k = 0; k < doorsOnSide2.Count; k++)
		{
			BiomeCreatorTools.PlaceOneWayAtDoor(doorsOnSide2[k]);
		}
		biomeController.RoomConnectedByTunnelCreatedByWorldBuilder(roomInstance);
		InvokeRoomCreatedEvent(roomInstance);
		int biomeControllerIndex = lobby.BiomeControllerIndex;
		lobbyInstance.SetBiomeControllerIndex(biomeControllerIndex);
		int biomeControllerIndex2 = room.BiomeControllerIndex;
		roomInstance.SetBiomeControllerIndex(biomeControllerIndex2);
		RunSpawnLogicOnRoom(lobbyInstance);
		RunSpawnLogicOnRoom(roomInstance);
		if (!GameUtility.IsInLevelEditor)
		{
			BiomeCreatorTools.CreateBackground(lobbyInstance);
			BiomeCreatorTools.CreateBackground(roomInstance);
		}
		lobbyInstance.gameObject.SetActive(value: false);
		roomInstance.gameObject.SetActive(value: false);
		TunnelSpawnController[] tunnelSpawnControllers = roomInstance.gameObject.GetComponentsInChildren<TunnelSpawnController>(includeInactive: true);
		if (tunnelSpawnControllers == null || tunnelSpawnControllers.Length == 0)
		{
			yield break;
		}
		for (int i = 0; i < tunnelSpawnControllers.Length; i++)
		{
			if (tunnelSpawnControllers[i].SpawnLogicController != null)
			{
				tunnelSpawnControllers[i].SpawnLogicController.RunIsSpawnedCheck();
			}
			if (tunnelSpawnControllers[i].ShouldSpawn && tunnelSpawnControllers[i].Direction == TunnelDirection.Entrance)
			{
				tunnelSpawnControllers[i].Spawn();
				m_destinationRooms.Clear();
				m_rootToRoomsTable.Clear();
				yield return CreateDestination(biomeController, tunnelSpawnControllers[i], null);
			}
		}
	}

	private static void InitializeDestinationRoom(BiomeController biomeController, TunnelSpawnController tunnelSpawnController, Room destination, Vector2Int gridCoords)
	{
		GridPointManager gridPointManager = CreateGridPointManager(biomeController, tunnelSpawnController.RoomMetaData, tunnelSpawnController.LeadsToRoomType, gridCoords);
		if (gridPointManager.AppearanceOverride == BiomeType.None)
		{
			BiomeType tunnelDestinationBiome = GetTunnelDestinationBiome(biomeController);
			gridPointManager.SetAppearanceOverride(tunnelDestinationBiome);
		}
		destination.Initialise(gridPointManager);
		destination.SetBiomeControllerIndex(gridPointManager.BiomeControllerIndex);
		int roomSeed = GetRoomSeed(tunnelSpawnController.Room);
		gridPointManager.SetRoomSeed(roomSeed);
		RunSpawnLogicOnRoom(destination);
		destination.transform.parent = biomeController.RoomsConnectedByTunnelStorageLocation;
		Vector2 roomCoordinatesFromGridCoordinates = GridController.GetRoomCoordinatesFromGridCoordinates(DESTINATION_GRID_COORDINATES);
		destination.transform.position = GridController.GetWorldPositionFromRoomCoordinates(roomCoordinatesFromGridCoordinates, tunnelSpawnController.RoomMetaData.Size);
		if ((bool)biomeController)
		{
			biomeController.RoomConnectedByTunnelCreatedByWorldBuilder(destination);
		}
		if (!GameUtility.IsInLevelEditor)
		{
			BiomeCreatorTools.CreateBackground(destination);
		}
	}

	private static int GetRoomSeed(BaseRoom room)
	{
		int num = 0;
		if (room is Room)
		{
			return (room as Room).GridPointManager.RoomSeed;
		}
		return (room as MergeRoom).StandaloneGridPointManagers[0].RoomSeed;
	}

	private static void RunSpawnLogicOnRoom(Room room)
	{
		for (int num = room.Doors.Count - 1; num >= 0; num--)
		{
			Door door = room.Doors[num];
			if (door.ConnectedDoor == null)
			{
				door.Close();
			}
		}
		SpawnLogicController[] spawnLogicControllers = room.SpawnControllerManager.SpawnLogicControllers;
		for (int i = 0; i < spawnLogicControllers.Length; i++)
		{
			spawnLogicControllers[i].RunIsSpawnedCheck();
		}
		ISimpleSpawnController[] simpleSpawnControllers_NoProps = room.SpawnControllerManager.SimpleSpawnControllers_NoProps;
		for (int i = 0; i < simpleSpawnControllers_NoProps.Length; i++)
		{
			simpleSpawnControllers_NoProps[i].Spawn();
		}
	}
}
