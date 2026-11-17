using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MergeRoom : BaseRoom, ISummoner, ILevelConsumer
{
	private Bounds m_bounds;

	private Dictionary<GridPointManager, GlobalTeleporterController> m_globalTeleporters = new Dictionary<GridPointManager, GlobalTeleporterController>();

	private Dictionary<Transform, List<Ferr2DT_PathTerrain>> m_standaloneRoomToTerrainTable = new Dictionary<Transform, List<Ferr2DT_PathTerrain>>();

	private int m_levelOverride = -1;

	public override bool SpawnedAsEasyRoom
	{
		get
		{
			if (BiomeCreation_EV.FORCE_EASY_ROOMS_AT_BIOME_START.TryGetValue(base.BiomeType, out var value) && BurdenManager.GetTotalBurdenLevel() == 0 && StandaloneGridPointManagers.Length != 0)
			{
				int roomNumber = StandaloneGridPointManagers[0].RoomNumber;
				if (StandaloneGridPointManagers[0].RoomType == RoomType.Standard && !StandaloneGridPointManagers[0].IsTunnelDestination)
				{
					return roomNumber <= value + 1;
				}
				return false;
			}
			return false;
		}
	}

	public override BiomeArtData BiomeArtDataOverride => null;

	public override Bounds Bounds => m_bounds;

	public override List<Door> Doors
	{
		get
		{
			if (m_doors == null)
			{
				m_doors = new List<Door>();
			}
			return m_doors;
		}
	}

	public Dictionary<GridPointManager, GlobalTeleporterController> GlobalTeleporters
	{
		get
		{
			return m_globalTeleporters;
		}
		private set
		{
			m_globalTeleporters = value;
		}
	}

	public override int Level
	{
		get
		{
			if (m_levelOverride != -1)
			{
				return m_levelOverride;
			}
			return StandaloneRoomLevels[0];
		}
	}

	public override RoomType RoomType => RoomType.Standard;

	public override SpecialRoomType SpecialRoomType => SpecialRoomType.None;

	public RoomEnemyManager[] StandaloneRoomEnemyManagers { get; private set; }

	public Bounds[] StandaloneRoomBounds { get; private set; }

	public GridPointManager[] StandaloneGridPointManagers { get; private set; }

	public int[] StandaloneRoomLevels { get; private set; }

	public Dictionary<Transform, List<Ferr2DT_PathTerrain>> StandaloneRoomToTerrainTable
	{
		get
		{
			return m_standaloneRoomToTerrainTable;
		}
		private set
		{
			m_standaloneRoomToTerrainTable = value;
		}
	}

	public override bool AllowItemDrops => true;

	public void AddRooms(List<Room> rooms)
	{
		StartCoroutine(AddRoomsCoroutine(rooms));
	}

	private IEnumerator AddRoomsCoroutine(List<Room> rooms)
	{
		if (rooms == null)
		{
			throw new ArgumentNullException("rooms");
		}
		if (rooms.Count < 2)
		{
			throw new ArgumentException("rooms", "Count must be greater than or equal to 2");
		}
		IEnumerable<IGrouping<BiomeType, Room>> enumerable = from entry in rooms
			group entry by entry.AppearanceBiomeType;
		if (enumerable.Count() != 1)
		{
			string text = "";
			foreach (IGrouping<BiomeType, Room> item in enumerable)
			{
				text = text + item.Key.ToString() + " | ";
			}
			foreach (Room room in rooms)
			{
				Debug.Log("RoomID: " + room.GridPointManager.RoomMetaData.name + " BiomeType: " + room.AppearanceBiomeType);
			}
			throw new ArgumentException("Room.Biome", "Rooms must be from the same Biome. BiomeTypes found: " + text);
		}
		base.AppearanceBiomeType = rooms.First().AppearanceBiomeType;
		m_isInBiome = rooms.First().BiomeType;
		InitializeProperties(rooms);
		base.transform.position = Bounds.center;
		if (base.AppearanceBiomeType == BiomeType.Stone)
		{
			CreateBridgeBiomeChunkColliders(rooms);
		}
		PerformOperationsOnInternalDoors(rooms);
		BiomeController biomeController = WorldBuilder.GetBiomeController(base.BiomeType);
		foreach (Room room2 in rooms)
		{
			CopyRoom(room2);
			biomeController.Rooms.Remove(room2);
			if (base.Backgrounds == null && room2.Backgrounds != null && room2.Backgrounds.Length != 0)
			{
				SetBackgroundPoolEntries(room2.Backgrounds);
			}
		}
		SetSpawnControllerManager(new RoomSpawnControllerManager(this));
		SetTerrainManager(new RoomTerrainManager(this));
		base.SaveController = new RoomSaveController(this);
		yield return null;
		InjectRoom();
	}

	private void CopyRoom(Room room)
	{
		room.OnMerge();
		room.gameObject.transform.SetParent(base.transform, worldPositionStays: true);
		Component[] components = room.gameObject.GetComponents<Component>();
		for (int num = components.Length - 1; num >= 0; num--)
		{
			Component component = components[num];
			if (!(component is Transform) && !(component is Collider2D) && !(component is RoomEnemyManager))
			{
				UnityEngine.Object.Destroy(component);
			}
		}
	}

	private void CreateBridgeBiomeChunkColliders(List<Room> rooms)
	{
		GameObject obj = new GameObject("Merge Room Chunks");
		obj.transform.SetParent(base.transform);
		obj.transform.localPosition = Vector3.zero;
		obj.AddComponent<MergeRoomChunkManager>().Initialize(this, rooms);
	}

	protected override int GetDecoSeed()
	{
		int result = -1;
		if (StandaloneGridPointManagers != null && StandaloneGridPointManagers.Length != 0)
		{
			result = StandaloneGridPointManagers[0].DecoSeed;
		}
		return result;
	}

	private bool GetIsInternalDoor(List<Room> rooms, Door door)
	{
		bool flag = false;
		if (door.Side == RoomSide.Left)
		{
			flag = Mathf.Abs(Bounds.min.x - door.CenterPoint.x) < 0.1f;
		}
		else if (door.Side == RoomSide.Right)
		{
			flag = Mathf.Abs(Bounds.max.x - door.CenterPoint.x) < 0.1f;
		}
		else if (door.Side == RoomSide.Bottom)
		{
			flag = Mathf.Abs(Bounds.min.y - door.CenterPoint.y) < 0.1f;
		}
		else if (door.Side == RoomSide.Top)
		{
			flag = Mathf.Abs(Bounds.max.y - door.CenterPoint.y) < 0.1f;
		}
		return !flag;
	}

	protected override int GetSpecialPropSeed()
	{
		int result = -1;
		if (StandaloneGridPointManagers != null && StandaloneGridPointManagers.Length != 0)
		{
			result = StandaloneGridPointManagers[0].SpecialPropSeed;
		}
		return result;
	}

	private void GrowBounds(Room room)
	{
		Bounds bounds = default(Bounds);
		bounds = Bounds;
		bounds.Encapsulate(room.Bounds);
		m_bounds = bounds;
		float x = m_bounds.center.x - m_bounds.extents.x;
		float y = m_bounds.center.y - m_bounds.extents.y;
		base.BoundsRect = new Rect(new Vector2(x, y), m_bounds.size);
	}

	private void InitializeProperties(List<Room> rooms)
	{
		StandaloneGridPointManagers = new GridPointManager[rooms.Count];
		StandaloneRoomBounds = new Bounds[rooms.Count];
		StandaloneRoomLevels = new int[rooms.Count];
		StandaloneRoomEnemyManagers = new RoomEnemyManager[rooms.Count];
		m_bounds = new Bounds(rooms.First().gameObject.transform.position, Vector3.zero);
		float x = m_bounds.center.x - m_bounds.extents.x;
		float y = m_bounds.center.y - m_bounds.extents.y;
		base.BoundsRect = new Rect(new Vector2(x, y), m_bounds.size);
		for (int i = 0; i < rooms.Count; i++)
		{
			StandaloneRoomToTerrainTable.Add(rooms[i].gameObject.transform, rooms[i].TerrainManager.Platforms);
			StandaloneRoomBounds[i] = rooms[i].Bounds;
			StandaloneGridPointManagers[i] = rooms[i].GridPointManager;
			StandaloneRoomLevels[i] = rooms[i].Level;
			StandaloneRoomEnemyManagers[i] = rooms[i].RoomEnemyManager;
			if (rooms[i].GlobalTeleporter != null)
			{
				GlobalTeleporters.Add(rooms[i].GridPointManager, rooms[i].GlobalTeleporter);
			}
			GrowBounds(rooms[i]);
			base.ConnectedRooms.AddRange(rooms[i].ConnectedRooms);
		}
	}

	private void PerformOperationsOnInternalDoors(List<Room> rooms)
	{
		foreach (Room room in rooms)
		{
			for (int num = room.Doors.Count - 1; num >= 0; num--)
			{
				Door door = room.Doors[num];
				if (GetIsInternalDoor(rooms, door))
				{
					UnityEngine.Object.Destroy(door.GameObject);
				}
				else
				{
					if (m_doors == null)
					{
						m_doors = new List<Door>();
					}
					door.PlayerEnterRelay.AddListener(base.OnPlayerExitViaDoor);
					m_doors.Add(door);
				}
			}
		}
	}

	public override void SetLevel(int value)
	{
		m_levelOverride = value;
	}

	[SpecialName]
	GameObject ISummoner.get_gameObject()
	{
		return base.gameObject;
	}
}
