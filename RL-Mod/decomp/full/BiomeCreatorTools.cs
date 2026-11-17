using System;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using ClipperLibFerr;
using UnityEngine;

public static class BiomeCreatorTools
{
	private static float ROOM_COLLIDER_SCALE = 0.9f;

	private static LayerMask ROOM_COLLISION_CHECK_LAYER_MASK = LayerMask.NameToLayer("LevelBounds");

	private const float OUTER_EDGE_MULTIPLIER = 0.5f;

	private const string LEVEL_MANAGER_NAME = "LevelManager";

	private const string LEVEL_EDITOR_NAME = "LevelEditor";

	private const string DOORS_CHILD_NAME = "Doors";

	private const string WALLS_CHILD_NAME = "Walls";

	private const string ONE_WAYS_CHILD_NAME = "One Ways";

	private const string ENEMIES_CHILD_NAME = "Enemies";

	private const string ITEMS_CHILD_NAME = "Items";

	private const string CHESTS_CHILD_NAME = "Chests";

	private const string CINEMACHINE_VIRTUAL_CAMERA_PREFAB_PATH = "Prefabs/Camera/Cinemachine Virtual Camera";

	private static BiomeCreator m_biomeCreator = null;

	private static List<Weather> m_weatherList_STATIC = new List<Weather>();

	private static List<BackgroundPoolEntry> m_backgroundPoolEntryHelper = new List<BackgroundPoolEntry>();

	public static BiomeCreator BiomeCreator
	{
		get
		{
			if (Application.isPlaying)
			{
				if (m_biomeCreator == null)
				{
					GameObject gameObject = GameObject.Find("LevelManager");
					if (gameObject != null)
					{
						m_biomeCreator = gameObject.GetComponent<BiomeCreator>();
						if (m_biomeCreator == null)
						{
							throw new MissingComponentException("BiomeCreator");
						}
					}
				}
				return m_biomeCreator;
			}
			return null;
		}
	}

	public static void PositionRoom(Room destinationRoom, RoomSide atSide, int atDoorIndex, Room room, int doorIndex)
	{
		if (destinationRoom == null)
		{
			throw new ArgumentNullException("destinationRoom");
		}
		if (room == null)
		{
			throw new ArgumentNullException("room");
		}
		if (atDoorIndex < 0)
		{
			throw new ArgumentException("atDoorIndex", "Value must be greater than or equal to 0");
		}
		if (atDoorIndex < 0)
		{
			throw new ArgumentException("doorIndex", "Value must be greater than or equal to 0");
		}
		switch (atSide)
		{
		case RoomSide.None:
		case RoomSide.Any:
			throw new ArgumentException("atSide", "Value must be Left, Right, Top or Bottom");
		case RoomSide.Left:
		case RoomSide.Right:
		{
			float x = 0f;
			if (atSide == RoomSide.Left)
			{
				x = room.UnitDimensions.x;
			}
			Vector2 coords2 = destinationRoom.GetDoor(atSide, atDoorIndex).Coordinates - new Vector2(x, (room.Size.y - 1 - doorIndex) * 18);
			room.MoveToCoordinates(coords2);
			break;
		}
		case RoomSide.Top:
		case RoomSide.Bottom:
		{
			float y = destinationRoom.Coordinates.y + (float)destinationRoom.UnitDimensions.y;
			if (atSide == RoomSide.Bottom)
			{
				y = destinationRoom.Coordinates.y - (float)room.UnitDimensions.y;
			}
			Vector2 coords = new Vector2(destinationRoom.Coordinates.x + (float)(atDoorIndex * 32) - (float)(doorIndex * 32), y);
			room.MoveToCoordinates(coords);
			break;
		}
		}
	}

	public static void MergeTerrain(BaseRoom room)
	{
		if (room == null)
		{
			throw new ArgumentNullException("room");
		}
		List<Ferr2DT_PathTerrain> list = room.TerrainManager.Platforms.ToList();
		if (list != null && list.Count > 0 && Application.isPlaying)
		{
			Ferr2DUtilities.FastEasyBoolean(ClipType.ctUnion, list, Ferr2DUtilities.Ferr2DUndoType.Destroy);
		}
	}

	public static void CopyToRoom<T>(List<T> data, Room toRoom, string intoChildNamed = "")
	{
		if (toRoom == null)
		{
			throw new ArgumentNullException("toRoom");
		}
		if (data == null || data.Count == 0)
		{
			Debug.LogFormat("<color=yellow>Data being copied into Room is empty</color>");
			return;
		}
		Transform transform = toRoom.transform;
		if (!string.IsNullOrEmpty(intoChildNamed))
		{
			transform = toRoom.transform.Find(intoChildNamed);
			if (transform == null)
			{
				transform = new GameObject(intoChildNamed).transform;
			}
			transform.SetParent(toRoom.transform, worldPositionStays: false);
			transform.localPosition = Vector3.zero;
		}
		foreach (T datum in data)
		{
			MonoBehaviour monoBehaviour = datum as MonoBehaviour;
			if (monoBehaviour != null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(monoBehaviour.gameObject, transform);
				gameObject.transform.position = monoBehaviour.transform.position;
				gameObject.name = monoBehaviour.name;
			}
		}
	}

	public static OneWay PlaceOneWayAtDoor(Door door)
	{
		if (door == null)
		{
			throw new ArgumentNullException("door");
		}
		if (door.Side != RoomSide.Bottom)
		{
			throw new ArgumentException("door.Side", "Side must be Bottom");
		}
		Transform transform = (door.Room as Room).OneWayLocation;
		if (transform == null)
		{
			transform = new GameObject("One Ways").transform;
			transform.SetParent(door.Room.gameObject.transform, worldPositionStays: false);
		}
		Ferr2DT_PathTerrain ferr2DT_PathTerrain = UnityEngine.Object.Instantiate(RoomPrefabLibrary.BottomDoorOneWayPrefab, transform, worldPositionStays: false);
		door.Room.TerrainManager.AddOneWay(ferr2DT_PathTerrain);
		float x = door.Coordinates.x + 16f;
		float y = door.Coordinates.y + 1f;
		ferr2DT_PathTerrain.transform.position = new Vector3(x, y, 0f);
		ferr2DT_PathTerrain.ClearPoints();
		Vector2 aPt = new Vector2(3f, 0f);
		Vector2 aPt2 = new Vector2(-3f, 0f);
		ferr2DT_PathTerrain.AddPoint(aPt);
		ferr2DT_PathTerrain.AddPoint(aPt2);
		OneWay component = ferr2DT_PathTerrain.GetComponent<OneWay>();
		BiomeType biomeType = ArtUtility.GetTerrainBiomeType(door.Room);
		if (biomeType == BiomeType.None)
		{
			biomeType = BiomeType.Editor;
		}
		component.SetVisuals(biomeType, door.Room);
		door.SetOneWay(component.gameObject);
		return component;
	}

	public static Sky CreateSkyInstance(BiomeArtData biomeArtData)
	{
		if ((bool)biomeArtData.SkyData.SkyPrefab)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(biomeArtData.SkyData.SkyPrefab);
			gameObject.GetComponent<Sky>();
			return gameObject.GetComponent<Sky>();
		}
		Debug.LogFormat("<color=yellow>{0}: Sky Prefab is null</color>", Time.frameCount);
		return null;
	}

	public static Weather[] CreateWeatherInstances(BiomeArtData biomeArtData)
	{
		m_weatherList_STATIC.Clear();
		if (biomeArtData.WeatherData.WeatherPrefabArray != null)
		{
			Weather[] weatherPrefabArray = biomeArtData.WeatherData.WeatherPrefabArray;
			for (int i = 0; i < weatherPrefabArray.Length; i++)
			{
				Weather item = UnityEngine.Object.Instantiate(weatherPrefabArray[i]);
				m_weatherList_STATIC.Add(item);
			}
			return m_weatherList_STATIC.ToArray();
		}
		Debug.LogFormat("<color=yellow>{0}: No weather prefabs found in biome</color>", Time.frameCount);
		return null;
	}

	public static void CreateBackground(BaseRoom targetRoom)
	{
		if (targetRoom is Room)
		{
			m_backgroundPoolEntryHelper.Clear();
			Room room = targetRoom as Room;
			BiomeType appearanceBiomeType = room.AppearanceBiomeType;
			if (appearanceBiomeType != 0)
			{
				float num = room.Coordinates.x;
				float num2 = room.Coordinates.y;
				float num3 = room.Coordinates.x + (float)room.UnitDimensions.x;
				float num4 = room.Coordinates.y + (float)room.UnitDimensions.y;
				foreach (GridPointManager mergeWithGridPointManager in room.GridPointManager.MergeWithGridPointManagers)
				{
					num = Mathf.Min(num, mergeWithGridPointManager.GridCoordinates.x * 32);
					num2 = Mathf.Min(num2, mergeWithGridPointManager.GridCoordinates.y * 18);
					num3 = Mathf.Max(num3, (mergeWithGridPointManager.GridCoordinates.x + mergeWithGridPointManager.Size.x) * 32);
					num4 = Mathf.Max(num4, (mergeWithGridPointManager.GridCoordinates.y + mergeWithGridPointManager.Size.y) * 18);
				}
				Vector2Int vector2Int = new Vector2Int((int)Mathf.Abs(num3 - num), (int)Mathf.Abs(num4 - num2));
				Background background = null;
				RoomBackgroundOverride component = targetRoom.gameObject.GetComponent<RoomBackgroundOverride>();
				bool flag = true;
				bool flag2 = false;
				if ((bool)component)
				{
					background = component.BackgroundOverride;
					flag = component.IsTiled;
				}
				else
				{
					BiomeArtData biomeArtData = targetRoom.BiomeArtDataOverride;
					if (!biomeArtData)
					{
						biomeArtData = BiomeArtDataLibrary.GetArtData(appearanceBiomeType);
					}
					if ((bool)biomeArtData)
					{
						background = GetBackgroundPrefab(biomeArtData.BackgroundData);
						flag2 = biomeArtData.BackgroundData.TileNormally;
					}
					else
					{
						Debug.LogFormat("<color=red>{0}: [BiomeCreatorTools] Failed to find Biome Art Data for Biome ({1})</color>", Time.frameCount, appearanceBiomeType);
					}
				}
				if ((bool)background)
				{
					if (flag)
					{
						Vector2Int vector2Int2 = new Vector2Int(vector2Int.x / 32, vector2Int.y / 18);
						Vector2 vector = new Vector2(num - 16f, num2 - 9f);
						for (int i = 0; i < vector2Int2.x + 2; i++)
						{
							if (flag2)
							{
								for (int j = 0; j < vector2Int2.y + 2; j++)
								{
									BackgroundPoolEntry backgroundPoolEntry = new BackgroundPoolEntry();
									backgroundPoolEntry.BackgroundPrefab = background;
									backgroundPoolEntry.Position = vector + new Vector2(i * 32, j * 18);
									m_backgroundPoolEntryHelper.Add(backgroundPoolEntry);
								}
							}
							else
							{
								BackgroundPoolEntry backgroundPoolEntry2 = new BackgroundPoolEntry();
								backgroundPoolEntry2.BackgroundPrefab = background;
								backgroundPoolEntry2.Position = vector + new Vector2(i * 32, 0f);
								m_backgroundPoolEntryHelper.Add(backgroundPoolEntry2);
							}
						}
					}
					else if ((bool)background)
					{
						BackgroundPoolEntry backgroundPoolEntry3 = new BackgroundPoolEntry();
						backgroundPoolEntry3.BackgroundPrefab = background;
						backgroundPoolEntry3.Position = room.gameObject.transform.position;
						m_backgroundPoolEntryHelper.Add(backgroundPoolEntry3);
					}
				}
			}
			room.SetBackgroundPoolEntries(m_backgroundPoolEntryHelper.ToArray());
		}
		else if (targetRoom is MergeRoom)
		{
			Debug.LogFormat("<color=red>{0}: BiomeCreatorTools.CreateBackground() currently only works with Room's</color>", Time.frameCount);
		}
	}

	private static Background GetBackgroundPrefab(BackgroundBiomeArtData backgroundData)
	{
		if (backgroundData != null && backgroundData.Backgrounds != null && backgroundData.Backgrounds.Count > 0)
		{
			List<Background> list = backgroundData.Backgrounds.Where((Background entry) => entry.Size == new Vector2Int(1, 1)).ToList();
			int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, $"(BiomeCreatorTools) GetBackgroundPrefab", 0, list.Count);
			return list[randomNumber];
		}
		return null;
	}

	public static CinemachineVirtualCamera CreateCinemachineVirtualCamera(BaseRoom room)
	{
		if (room == null)
		{
			throw new ArgumentNullException("room");
		}
		return UnityEngine.Object.Instantiate(RoomPrefabLibrary.CinemachineVirtualCameraPrefab, room.gameObject.transform, worldPositionStays: false);
	}

	public static void SetOuterEdges(IEnumerable<Ferr2DT_PathTerrain> terrainToSet, Bounds bounds)
	{
		foreach (Ferr2DT_PathTerrain item in terrainToSet)
		{
			SetOuterEdges(item, bounds);
		}
	}

	public static void SetOuterEdges(Ferr2DT_PathTerrain terrain, Bounds bounds)
	{
		int count = terrain.PathData.Count;
		for (int i = 0; i < count; i++)
		{
			Vector2 vector = terrain.PathData.Get(i);
			Vector2 vector2 = ((i >= count - 1) ? terrain.PathData.Get(0) : terrain.PathData.Get(i + 1));
			vector = terrain.transform.TransformPoint(vector);
			vector2 = terrain.transform.TransformPoint(vector2);
			if (AreBothPointsOnSameEdge(bounds, vector, vector2))
			{
				Ferr2D_PointData data = terrain.PathData.GetData(i);
				data.directionOverride = 4;
				terrain.PathData.SetData(i, data);
			}
		}
	}

	public static bool AreBothPointsOnSameEdge(Bounds bounds, Vector2 pointA, Vector2 pointB)
	{
		bool result = false;
		if ((pointA.x == bounds.min.x && pointB.x == bounds.min.x) || (pointA.x == bounds.max.x && pointB.x == bounds.max.x) || (pointA.y == bounds.min.y && pointB.y == bounds.min.y) || (pointA.y == bounds.max.y && pointB.y == bounds.max.y))
		{
			result = true;
		}
		return result;
	}

	private static bool GetIsPointInRoom(BaseRoom room, Vector2 point)
	{
		bool result = false;
		if (room is Room)
		{
			result = room.Collider2D.OverlapPoint(point);
		}
		else if (room is MergeRoom)
		{
			Bounds[] standaloneRoomBounds = (room as MergeRoom).StandaloneRoomBounds;
			foreach (Bounds bounds in standaloneRoomBounds)
			{
				if (bounds.Contains(point))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public static List<Door> GetDoorsInRoomThatFit(Door door, Room room)
	{
		List<Door> doorsOnSide = room.GetDoorsOnSide(RoomUtility.GetOppositeSide(door.Side));
		Vector2 coordinates = door.Coordinates;
		List<Door> list = new List<Door>();
		foreach (Door item in doorsOnSide)
		{
			Vector2 zero = Vector2.zero;
			if (door.Side == RoomSide.Left || door.Side == RoomSide.Right)
			{
				float x = 0f;
				if (door.Side == RoomSide.Left)
				{
					x = room.Size.x * 32;
				}
				zero = coordinates - new Vector2(x, (room.Size.y - 1 - item.Number) * 18);
			}
			else
			{
				float y = 0f;
				if (door.Side == RoomSide.Bottom)
				{
					y = -1 * room.Size.y * 18;
				}
				zero = coordinates + new Vector2(-1 * item.Number * 32, y);
			}
			if (!CanRoomFit(zero, room))
			{
				list.Add(item);
			}
		}
		foreach (Door item2 in list)
		{
			doorsOnSide.Remove(item2);
		}
		return doorsOnSide;
	}

	public static Vector2 GetCoordinatesOfRoomConnectedToDoor(Door door, Vector2Int roomSize, int doorNumber)
	{
		float x = 0f;
		float y = 0f;
		RoomSide side = door.Side;
		switch (side)
		{
		case RoomSide.Left:
		case RoomSide.Right:
			if (side == RoomSide.Left)
			{
				x = roomSize.x * 32;
			}
			y = (roomSize.y - 1 - doorNumber) * 18;
			break;
		case RoomSide.Top:
		case RoomSide.Bottom:
			x = doorNumber * 32;
			if (side == RoomSide.Bottom)
			{
				y = roomSize.y * 18;
			}
			break;
		}
		return door.Coordinates - new Vector2(x, y);
	}

	public static bool CanRoomFit(Vector2 coordinates, Vector2 roomSize)
	{
		Vector2 vector = new Vector2(roomSize.x * 32f, roomSize.y * 18f);
		return !Physics2D.OverlapBox(coordinates + 0.5f * vector, ROOM_COLLIDER_SCALE * vector, 0f, ROOM_COLLISION_CHECK_LAYER_MASK);
	}

	public static bool CanRoomFit(Vector2 coordinates, Room room)
	{
		return CanRoomFit(coordinates, room.Size);
	}

	public static void GrowTerrainAlongEdge(BaseRoom room)
	{
		float num = 2f;
		if (!(room is Room))
		{
			return;
		}
		Room room2 = room as Room;
		float x = room2.Coordinates.x;
		float num2 = room2.Coordinates.x + (float)room2.UnitDimensions.x;
		float y = room2.Coordinates.y;
		float num3 = room2.Coordinates.y + (float)room2.UnitDimensions.y;
		Vector2 vector = new Vector2(x, num3);
		Vector2 key = new Vector2(num2, num3);
		Vector2 key2 = new Vector2(num2, y);
		Vector2 key3 = new Vector2(x, y);
		Vector2 value = num * (vector - new Vector2(room2.transform.position.x, room2.transform.position.y)).normalized;
		Vector2 value2 = new Vector2(-1f * value.x, value.y);
		Vector2 value3 = new Vector2(value.x, -1f * value.y);
		Vector2 value4 = new Vector2(-1f * value.x, -1f * value.y);
		Dictionary<Vector2, Vector2> dictionary = new Dictionary<Vector2, Vector2>
		{
			{ vector, value },
			{ key, value2 },
			{ key2, value4 },
			{ key3, value3 }
		};
		foreach (Ferr2DT_PathTerrain platform in room.TerrainManager.Platforms)
		{
			foreach (Vector2 point in platform.PathData.GetPoints(0))
			{
				Vector2 key4 = room.gameObject.transform.TransformPoint(point);
				Vector2 vector2 = Vector2.zero;
				if (dictionary.ContainsKey(key4))
				{
					vector2 = dictionary[key4];
				}
				else if (key4.x == x)
				{
					vector2 = new Vector2(0f - num, 0f);
				}
				else if (key4.x == num2)
				{
					vector2 = new Vector2(num, 0f);
				}
				else if (key4.y == num3)
				{
					vector2 = new Vector2(0f, num);
				}
				else if (key4.y == y)
				{
					vector2 = new Vector2(0f, 0f - num);
				}
				_ = vector2 != Vector2.zero;
			}
		}
	}

	public static void SimplifyAllFerr2DColliders(BaseRoom room, bool rebuild, bool applyBoxConversion)
	{
		if (room.TerrainManager == null)
		{
			room.SetTerrainManager(new RoomTerrainManager(room));
		}
		else
		{
			room.TerrainManager.Initialize();
		}
		foreach (Ferr2DT_PathTerrain platform in room.TerrainManager.Platforms)
		{
			if (!platform.SimpleCollider)
			{
				platform.SimpleCollider = true;
				if ((bool)platform.GetComponent<Collider2D>() && (rebuild || applyBoxConversion))
				{
					platform.PathData.SetDirty();
					platform.RecreateCollider();
				}
			}
			if (applyBoxConversion && platform.FillMode == Ferr2D_SectionMode.Normal)
			{
				PolygonCollider2D component = platform.GetComponent<PolygonCollider2D>();
				if ((bool)component && (bool)CDGHelper.ConvertPolyToBoxCollider(component, destroyPolyCollider: true))
				{
					platform.RecreateCollider();
				}
			}
		}
		foreach (Ferr2DT_PathTerrain oneWay in room.TerrainManager.OneWays)
		{
			if (!oneWay.SimpleCollider)
			{
				oneWay.SimpleCollider = true;
				if (oneWay.GetComponent<Collider2D>() != null && (rebuild || applyBoxConversion))
				{
					oneWay.PathData.SetDirty();
					oneWay.RecreateCollider();
				}
			}
			if (applyBoxConversion && oneWay.FillMode == Ferr2D_SectionMode.Normal)
			{
				PolygonCollider2D component2 = oneWay.GetComponent<PolygonCollider2D>();
				if (component2 != null && CDGHelper.ConvertPolyToBoxCollider(component2, destroyPolyCollider: true) != null)
				{
					oneWay.RecreateCollider();
				}
			}
		}
	}

	public static void CloseUnconnectedDoorsInRoom(Room room)
	{
		if (room == null)
		{
			throw new ArgumentNullException("room");
		}
		List<Door> doors = room.Doors;
		for (int num = doors.Count - 1; num >= 0; num--)
		{
			Door door = doors[num];
			try
			{
				if (!GameUtility.IsInLevelEditor && GridController.GetDoesDoorLeadToDifferentBiome(door))
				{
					door.SetIsBiomeTransitionPoint(GridController.GetDoorLeadsToBiome(door));
				}
				else if (door.ConnectedDoor == null || door.ConnectedDoor.DisabledFromLevelEditor || door.DisabledFromLevelEditor)
				{
					bool replaceWithWall = true;
					if (BiomeCreation_EV.DISABLE_HORIZONTAL_FILL_IN_BIOMES.Contains(room.AppearanceBiomeType) && (door.Side == RoomSide.Left || door.Side == RoomSide.Right))
					{
						replaceWithWall = false;
					}
					else if (BiomeCreation_EV.DISABLE_VERTICAL_FILL_IN_BIOMES.Contains(room.AppearanceBiomeType) && (door.Side == RoomSide.Top || door.Side == RoomSide.Bottom))
					{
						replaceWithWall = false;
					}
					door.Close(replaceWithWall);
				}
			}
			catch (Exception)
			{
				if (door.Room == null)
				{
					Debug.LogFormat("Room is null on Door");
				}
				throw;
			}
		}
	}
}
