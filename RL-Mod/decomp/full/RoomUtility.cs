using System;
using System.Collections.Generic;
using Rooms;
using UnityEngine;

public static class RoomUtility
{
	private static List<Transform> m_mirrorRoomHelper = new List<Transform>();

	private static Vector2Int MINIMUM_LARGE_ROOM_SIZE = 2 * Vector2Int.one;

	public static Vector2 GetDoorCenterPoint(Vector2 roomCoordinates, Vector2Int roomDimensions, DoorLocation doorLocation)
	{
		Vector2 vector = roomDimensions * new Vector2(32f, 18f);
		Vector2 zero = Vector2.zero;
		if (doorLocation.RoomSide == RoomSide.Top || doorLocation.RoomSide == RoomSide.Bottom)
		{
			float x = roomCoordinates.x + ((float)doorLocation.DoorNumber + 0.5f) * 32f;
			float y = roomCoordinates.y;
			if (doorLocation.RoomSide == RoomSide.Top)
			{
				y = roomCoordinates.y + vector.y;
			}
			zero = new Vector2(x, y);
		}
		else
		{
			if (doorLocation.RoomSide != RoomSide.Left && doorLocation.RoomSide != RoomSide.Right)
			{
				throw new ArgumentException("Side", $"Side must be Left, Right, Top or Bottom");
			}
			float x2 = roomCoordinates.x;
			if (doorLocation.RoomSide == RoomSide.Right)
			{
				x2 = roomCoordinates.x + vector.x;
			}
			float y2 = roomCoordinates.y + vector.y - ((float)doorLocation.DoorNumber + 0.5f) * 18f;
			zero = new Vector2(x2, y2);
		}
		return zero;
	}

	public static Room CreateMirrorVersionOfRoom(Room room)
	{
		Room room2 = UnityEngine.Object.Instantiate(room);
		room2.SetIsMirrored(isMirrored: true);
		m_mirrorRoomHelper.Clear();
		room2.transform.FindAllTransforms(m_mirrorRoomHelper);
		foreach (Transform item in m_mirrorRoomHelper)
		{
			GameObject gameObject = item.gameObject;
			if (!gameObject.CompareTag("Door"))
			{
				Ferr2DT_PathTerrain component = gameObject.GetComponent<Ferr2DT_PathTerrain>();
				IMirror component2 = gameObject.GetComponent<IMirror>();
				if ((bool)component)
				{
					MirrorFerr2D(component);
				}
				else
				{
					gameObject.transform.localPosition = GetMirrorPosition(gameObject.transform);
				}
				component2?.Mirror();
			}
		}
		room2.SetSpawnControllerManager(new RoomSpawnControllerManager(room2));
		SpawnLogicController[] spawnLogicControllers = room2.SpawnControllerManager.SpawnLogicControllers;
		for (int i = 0; i < spawnLogicControllers.Length; i++)
		{
			SpawnConditionsEntry[] spawnConditions = spawnLogicControllers[i].SpawnConditions;
			for (int j = 0; j < spawnConditions.Length; j++)
			{
				SpawnScenarioEntry[] scenarios = spawnConditions[j].Scenarios;
				for (int k = 0; k < scenarios.Length; k++)
				{
					if (scenarios[k].Scenario is Door_SpawnScenario door_SpawnScenario)
					{
						if (door_SpawnScenario.Side == RoomSide.Left || door_SpawnScenario.Side == RoomSide.Right)
						{
							door_SpawnScenario.Side = GetOppositeSide(door_SpawnScenario.Side);
						}
						else if ((door_SpawnScenario.Side == RoomSide.Top || door_SpawnScenario.Side == RoomSide.Bottom) && door_SpawnScenario.Number != -1)
						{
							door_SpawnScenario.Number = GetMirrorDoorNumber(new DoorLocation(door_SpawnScenario.Side, door_SpawnScenario.Number), room2.Size);
						}
					}
				}
			}
		}
		foreach (Door door in room2.Doors)
		{
			MirrorFerr2D(door.Ferr2D);
			DoorLocation mirrorDoorLocation = GetMirrorDoorLocation(room2, new DoorLocation(door.Side, door.Number));
			door.Side = mirrorDoorLocation.RoomSide;
			door.Number = mirrorDoorLocation.DoorNumber;
			if (door.Side == RoomSide.Left || door.Side == RoomSide.Right)
			{
				door.GameObject.name = door.Side.ToString() + " Door";
			}
		}
		return room2;
	}

	public static Vector2 GetDoorCoordinates(Room room, Door door)
	{
		if (door.Side == RoomSide.Left || door.Side == RoomSide.Right)
		{
			float x = room.Coordinates.x;
			if (door.Side == RoomSide.Right)
			{
				x = room.Coordinates.x + (float)room.UnitDimensions.x;
			}
			return new Vector2(x, room.Coordinates.y + (float)((room.Size.y - 1 - door.Number) * 18));
		}
		if (door.Side == RoomSide.Top || door.Side == RoomSide.Bottom)
		{
			float y = room.Coordinates.y;
			if (door.Side == RoomSide.Top)
			{
				y = room.Coordinates.y + (float)room.UnitDimensions.y;
			}
			return new Vector2(room.Coordinates.x + (float)(door.Number * 32), y);
		}
		throw new ArgumentException("Side", $"Side must be Left, Right, Top or Bottom");
	}

	public static IEnumerable<DoorLocation> GetDoorLocations(GridPoint gridPoint)
	{
		DoorLocation[] doors = gridPoint.RoomMetaData.DoorLocations;
		for (int i = 0; i < doors.Length; i++)
		{
			DoorLocation doorLocation = doors[i];
			if (gridPoint.IsMirrored)
			{
				doorLocation = GetMirrorDoorLocation(gridPoint.RoomMetaData.Size, doorLocation);
			}
			yield return doorLocation;
		}
	}

	public static Vector3 GetMirrorPosition(Transform transform)
	{
		return new Vector3(-1f * transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
	}

	public static DoorLocation GetMirrorDoorLocation(Room room, DoorLocation doorLocation)
	{
		return GetMirrorDoorLocation(room.Size, doorLocation);
	}

	public static DoorLocation GetMirrorDoorLocation(Vector2Int roomSize, DoorLocation doorLocation)
	{
		RoomSide roomSide = doorLocation.RoomSide;
		int mirrorDoorNumber = GetMirrorDoorNumber(doorLocation, roomSize);
		if (doorLocation.RoomSide == RoomSide.Left || doorLocation.RoomSide == RoomSide.Right)
		{
			roomSide = GetOppositeSide(doorLocation.RoomSide);
		}
		return new DoorLocation(roomSide, mirrorDoorNumber);
	}

	public static List<DoorLocation> GetMirroredDoorLocations(RoomMetaData roomMetaData)
	{
		List<DoorLocation> list = new List<DoorLocation>();
		for (int i = 0; i < roomMetaData.DoorLocations.Length; i++)
		{
			list.Add(GetMirrorDoorLocation(roomMetaData.Size, roomMetaData.DoorLocations[i]));
		}
		return list;
	}

	public static RoomSide GetMirrorSide(RoomSide side)
	{
		if (side == RoomSide.Top || side == RoomSide.Bottom)
		{
			return side;
		}
		return GetOppositeSide(side);
	}

	public static int GetMirrorDoorNumber(DoorLocation doorLocation, Vector2Int roomSize)
	{
		if (doorLocation.RoomSide == RoomSide.Top || doorLocation.RoomSide == RoomSide.Bottom)
		{
			return roomSize.x - 1 - doorLocation.DoorNumber;
		}
		return doorLocation.DoorNumber;
	}

	public static int GetMirrorDoorNumber(RoomSide side, int doorNumber, Vector2Int roomSize)
	{
		return GetMirrorDoorNumber(new DoorLocation(side, doorNumber), roomSize);
	}

	public static RoomSide GetOppositeSide(RoomSide roomSide)
	{
		return roomSide switch
		{
			RoomSide.Top => RoomSide.Bottom, 
			RoomSide.Bottom => RoomSide.Top, 
			RoomSide.Left => RoomSide.Right, 
			RoomSide.Right => RoomSide.Left, 
			_ => RoomSide.None, 
		};
	}

	public static int GetRoomLevel(BiomeType biome, int roomNumber)
	{
		biome = BiomeType_RL.GetGroupedBiomeType(biome);
		BiomeData data = BiomeDataLibrary.GetData(biome);
		if (data != null)
		{
			float num = ((data.RoomLevelScale > 0f) ? data.RoomLevelScale : 1f);
			int num2 = data.EnemyStartLevel * Mathf.RoundToInt(1f + BurdenManager.GetBurdenStatGain(BurdenType.RoomCount));
			return Mathf.FloorToInt((float)roomNumber / num) + num2;
		}
		if (biome != BiomeType.Lineage)
		{
			Debug.LogFormat("<color=red>[RoomUtility] Failed to retrieve Biome Data for Biome ({0})</color>", biome);
		}
		return 1;
	}

	public static RoomType GetRoomType(Room room)
	{
		if (room != null)
		{
			if ((bool)room.gameObject.GetComponent<FairyRoomController>())
			{
				return RoomType.Fairy;
			}
			if ((bool)room.gameObject.GetComponent<BossRoomController>())
			{
				return RoomType.Boss;
			}
			return RoomType.Standard;
		}
		return RoomType.None;
	}

	public static void InjectRoomReferenceIntoChildrenOfGameObject(BaseRoom room, GameObject rootGameObject)
	{
		IRoomConsumer[] componentsInChildren = rootGameObject.GetComponentsInChildren<IRoomConsumer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetRoom(room);
		}
	}

	public static void MirrorFerr2D(Ferr2DT_PathTerrain ferr2D)
	{
		Ferr2DUtilities.Flip(ferr2D, flipHorizontally: true, flipVertically: false, recentre: false);
		ferr2D.transform.localPosition = new Vector2(-1f * ferr2D.transform.localPosition.x, ferr2D.transform.localPosition.y);
		ferr2D.GetComponentInChildren<IHitboxController>()?.RealignHitboxColliders();
	}

	public static void BuildAllFerr2DTerrains(BaseRoom room)
	{
		MergeRoomTools.MergeTerrain(room);
		List<Ferr2DT_PathTerrain> oneWays = room.TerrainManager.OneWays;
		for (int i = 0; i < oneWays.Count; i++)
		{
			_ = oneWays[i];
			oneWays[i].Build();
		}
		Ferr2DT_PathTerrain[] hazards = room.TerrainManager.Hazards;
		for (int j = 0; j < hazards.Length; j++)
		{
			hazards[j].BuildMeshOnly();
		}
	}

	public static void ShuffleList<T>(List<T> list)
	{
		if (list.Count <= 1)
		{
			return;
		}
		string text = "";
		for (int i = 0; i < list.Count; i++)
		{
			text += list[i].ToString();
			if (i < list.Count - 1)
			{
				text += ", ";
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			int index = j + RNGManager.GetRandomNumber(RngID.BiomeCreation, $"Shuffle List. {text}", 0, list.Count - j);
			T value = list[index];
			list[index] = list[j];
			list[j] = value;
		}
	}

	public static bool GetIsRoomLarge(Vector2Int roomSize)
	{
		bool result = false;
		if (roomSize.x > MINIMUM_LARGE_ROOM_SIZE.x || roomSize.y > MINIMUM_LARGE_ROOM_SIZE.y)
		{
			result = true;
		}
		return result;
	}

	public static bool GetIsRoomLarge(BaseRoom room)
	{
		if (room == null)
		{
			throw new ArgumentNullException("room");
		}
		if (room is Room)
		{
			return GetIsRoomLarge((room as Room).Size);
		}
		return true;
	}

	public static int GetHashedRoomID(RoomID id, bool isMirrored)
	{
		return ((((0x1429 ^ id.SceneName.GetHashCode()) * 397) ^ id.Number) * 397) ^ (isMirrored ? 1 : 0);
	}

	public static int GetDoorLocationMask(RoomSide side, List<DoorLocation> locations)
	{
		int num = 0;
		for (int i = 0; i < locations.Count; i++)
		{
			num |= 1 << (int)side * 3 + locations[i].DoorNumber;
		}
		return num;
	}
}
