using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Foreground;
using Sigtrap.Relays;
using UnityEngine;

[ExecuteInEditMode]
public class Room : BaseRoom, ISummoner, ILevelConsumer
{
	[SerializeField]
	private bool m_canMerge = true;

	[SerializeField]
	private bool m_canFlip = true;

	[SerializeField]
	private bool m_disableRoomSaving;

	[SerializeField]
	private bool m_disableRoomLetteringBoxing;

	[SerializeField]
	private bool m_disableOneWaysAtBottomDoors;

	[SerializeField]
	private BiomeType m_appearanceOverride;

	[SerializeField]
	private BiomeArtData m_biomeArtDataOverride;

	[SerializeField]
	private int m_levelOverride = -1;

	[SerializeField]
	private bool m_allowItemDrops = true;

	[SerializeField]
	private bool m_isEasy;

	[SerializeField]
	[HideInInspector]
	private Vector2Int m_unitDimensions = new Vector2Int(32, 18);

	[SerializeField]
	[HideInInspector]
	private RoomID m_roomID;

	[SerializeField]
	[HideInInspector]
	private bool m_isDirty = true;

	private Bounds m_bounds;

	private bool m_hasSearchedForGlobalTeleporter;

	private GlobalTeleporterController m_globalTeleporter;

	private PlayerRoomSpawn m_spawn;

	private int m_roomNumber = -1;

	protected GameObject m_doorsLocation;

	protected GameObject m_walls;

	protected Transform m_enemies;

	private int m_level = -1;

	protected Transform m_oneWayLocation;

	private RoomConnectedEventArgs m_roomConnectedEventArgs;

	protected Relay<object, EventArgs> m_doorClosedRelay = new Relay<object, EventArgs>();

	protected Relay<object, RoomConnectedEventArgs> m_roomConnectedRelay = new Relay<object, RoomConnectedEventArgs>();

	protected Relay<object, EventArgs> m_roomMergedRelay = new Relay<object, EventArgs>();

	private static List<Door> m_doorListHelper_STATIC = new List<Door>();

	public IRelayLink<object, EventArgs> DoorClosedRelay => m_doorClosedRelay.link;

	public IRelayLink<object, RoomConnectedEventArgs> RoomConnectedRelay => m_roomConnectedRelay.link;

	public IRelayLink<object, EventArgs> RoomMergedRelay => m_roomMergedRelay.link;

	public override bool SpawnedAsEasyRoom
	{
		get
		{
			if (BiomeCreation_EV.FORCE_EASY_ROOMS_AT_BIOME_START.TryGetValue(base.BiomeType, out var value) && BurdenManager.GetTotalBurdenLevel() == 0)
			{
				if (RoomType == RoomType.Standard && SpecialRoomType == SpecialRoomType.None && !GridPointManager.IsTunnelDestination)
				{
					return RoomNumber <= value + 1;
				}
				return false;
			}
			return false;
		}
	}

	public int LevelOverride => m_levelOverride;

	public override BiomeArtData BiomeArtDataOverride => m_biomeArtDataOverride;

	public BiomeType AppearanceOverride => m_appearanceOverride;

	public override Bounds Bounds
	{
		get
		{
			if (Application.isPlaying)
			{
				if (IsInitialised && m_bounds == default(Bounds))
				{
					m_bounds = new Bounds(base.transform.position, new Vector3(UnitDimensions.x, UnitDimensions.y, 0f));
					float x = m_bounds.center.x - m_bounds.extents.x;
					float y = m_bounds.center.y - m_bounds.extents.y;
					base.BoundsRect = new Rect(new Vector2(x, y), m_bounds.size);
				}
				return m_bounds;
			}
			return new Bounds(base.transform.position, new Vector3(UnitDimensions.x, UnitDimensions.y, 0f));
		}
	}

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

	public bool CanFlip => m_canFlip;

	public bool CanMerge
	{
		get
		{
			return m_canMerge;
		}
		set
		{
			m_canMerge = value;
		}
	}

	public Vector2 Coordinates
	{
		get
		{
			Vector2 vector = base.transform.position;
			return new Vector2(vector.x - 0.5f * (float)UnitDimensions.x, vector.y - 0.5f * (float)UnitDimensions.y);
		}
	}

	public override bool DisableRoomLetterBoxing => m_disableRoomLetteringBoxing;

	public override bool DisableRoomSaving => m_disableRoomSaving;

	public GlobalTeleporterController GlobalTeleporter
	{
		get
		{
			if (!m_hasSearchedForGlobalTeleporter && m_globalTeleporter == null)
			{
				m_hasSearchedForGlobalTeleporter = true;
				m_globalTeleporter = GetComponentInChildren<GlobalTeleporterController>();
			}
			return m_globalTeleporter;
		}
	}

	public GridPointManager GridPointManager { get; private set; }

	public bool IsDirty
	{
		get
		{
			return m_isDirty;
		}
		set
		{
			m_isDirty = value;
		}
	}

	public bool IsInitialised { get; private set; }

	public bool IsMirrored { get; private set; }

	public bool IsStandardSize
	{
		get
		{
			if (UnitDimensions.x % 32 == 0)
			{
				return UnitDimensions.y % 18 == 0;
			}
			return false;
		}
	}

	public override int Level => m_level;

	public PlayerRoomSpawn PlayerSpawn
	{
		get
		{
			if (m_spawn == null)
			{
				m_spawn = GetComponentInChildren<PlayerRoomSpawn>();
			}
			return m_spawn;
		}
	}

	public RoomEnemyManager RoomEnemyManager { get; private set; }

	public RoomID RoomID
	{
		get
		{
			return m_roomID;
		}
		protected set
		{
			m_roomID = value;
		}
	}

	public Vector2Int Size
	{
		get
		{
			int num = UnitDimensions.x / 32;
			int num2 = UnitDimensions.y / 18;
			if (UnitDimensions.x % 32 != 0)
			{
				num++;
			}
			if (UnitDimensions.y % 18 != 0)
			{
				num2++;
			}
			return new Vector2Int(num, num2);
		}
	}

	public override SpecialRoomType SpecialRoomType => GridPointManager.RoomMetaData.SpecialRoomType;

	public Vector2Int UnitDimensions
	{
		get
		{
			return m_unitDimensions;
		}
		private set
		{
			m_unitDimensions = value;
		}
	}

	public GameObject DoorsLocation
	{
		get
		{
			if (!m_doorsLocation)
			{
				m_doorsLocation = base.transform.Find("Doors").gameObject;
			}
			return m_doorsLocation;
		}
		set
		{
			value.transform.SetParent(base.transform);
			m_doorsLocation = value;
		}
	}

	public GameObject WallsLocation
	{
		get
		{
			if (!m_walls)
			{
				Transform transform = base.transform.FindDeep("Walls");
				if (!transform)
				{
					throw new NullReferenceException($"No Child called Walls found on Room ({base.name})");
				}
				m_walls = transform.gameObject;
			}
			return m_walls;
		}
		set
		{
			value.transform.SetParent(base.transform);
			m_walls = value;
		}
	}

	public Transform EnemyLocation
	{
		get
		{
			if (m_enemies == null)
			{
				m_enemies = base.transform.Find("Enemies");
				if (m_enemies == null && Application.isEditor)
				{
					GameObject gameObject = new GameObject("Enemies");
					gameObject.transform.SetParent(base.transform);
					gameObject.transform.localPosition = Vector3.zero;
					m_enemies = gameObject.transform;
				}
			}
			return m_enemies;
		}
	}

	public Transform OneWayLocation
	{
		get
		{
			if (!Application.isPlaying)
			{
				if (m_oneWayLocation == null)
				{
					m_oneWayLocation = base.transform.Find("One Ways");
					if (m_oneWayLocation == null)
					{
						GameObject gameObject = new GameObject("One Ways");
						gameObject.transform.SetParent(base.transform);
						gameObject.transform.localPosition = Vector3.zero;
						m_oneWayLocation = gameObject.transform;
					}
				}
				return m_oneWayLocation;
			}
			return base.transform;
		}
	}

	public override bool AllowItemDrops => m_allowItemDrops;

	public bool IsEasy => m_isEasy;

	public bool DisableOneWaysAtBottomDoors => m_disableOneWaysAtBottomDoors;

	private void Start()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		foreach (Door door in Doors)
		{
			AddDoorEventHandlers(door);
		}
	}

	protected override void OnDestroy()
	{
		if (Application.isPlaying && Doors != null)
		{
			for (int num = Doors.Count - 1; num >= 0; num--)
			{
				RemoveDoor(Doors[num]);
			}
		}
		base.OnDestroy();
	}

	private void OnDrawGizmos()
	{
	}

	public void Initialise(BiomeType biome, RoomType roomType, int roomNumber = -1)
	{
		if (GridPointManager != null && GridPointManager.AppearanceOverride != 0)
		{
			biome = GridPointManager.AppearanceOverride;
		}
		base.AppearanceBiomeType = biome;
		RoomType = roomType;
		RoomNumber = roomNumber;
		CalculateRoomLevel();
		UpdateVisualAppearance();
		ForegroundController component = GetComponent<ForegroundController>();
		if ((bool)component && !component.IsInitialized)
		{
			component.Initialize();
		}
		if (base.SpawnControllerManager == null)
		{
			SetSpawnControllerManager(new RoomSpawnControllerManager(this));
		}
		if (base.TerrainManager == null)
		{
			SetTerrainManager(new RoomTerrainManager(this));
		}
		if (base.SaveController == null)
		{
			base.SaveController = new RoomSaveController(this);
		}
		m_roomInitializedRelay.Dispatch(this, EventArgs.Empty);
		RoomEnemyManager = GetComponent<RoomEnemyManager>();
		if (RoomEnemyManager == null)
		{
			Debug.LogFormat("<color=red>| {0} | Room ({1}) is missing a RoomEnemyManager component. In the meantime one will be added, but if you see this message please add a task to pivotal to add one.</color>", this, base.name);
			RoomEnemyManager = base.gameObject.AddComponent<RoomEnemyManager>();
		}
		InjectRoom();
		IsInitialised = true;
	}

	public void Initialise(GridPointManager gridPointManager)
	{
		GridPointManager = gridPointManager;
		Initialise(gridPointManager.Biome, gridPointManager.RoomType, gridPointManager.RoomNumber);
	}

	private void AddConnectedRoom(BaseRoom room)
	{
		if (room == null)
		{
			throw new ArgumentNullException("room", $"[{this}] Argument can't be null");
		}
		if (!base.ConnectedRooms.Contains(room))
		{
			base.ConnectedRooms.Add(room);
			DispatchRoomConnectedRelay(room);
		}
	}

	private void ApplyBiomeArtData(BiomeArtData biomeArtData)
	{
		Renderer component = GetComponent<Renderer>();
		if ((bool)component)
		{
			component.enabled = false;
		}
		Ferr2DBiomeArtData ferr2DBiomeArtData = biomeArtData.Ferr2DBiomeArtData;
		Ferr2DSettings settings = null;
		if (ferr2DBiomeArtData != null && ferr2DBiomeArtData.TerrainMaster != null)
		{
			settings = ferr2DBiomeArtData.TerrainMaster.Ferr2DSettings;
		}
		if (base.TerrainManager == null)
		{
			base.TerrainManager = new RoomTerrainManager(this);
		}
		UpdateFerr2DSetting(base.TerrainManager.Platforms, settings);
		Ferr2DSettings settings2 = null;
		if (ferr2DBiomeArtData != null && ferr2DBiomeArtData.OneWayMaster != null)
		{
			settings2 = ferr2DBiomeArtData.OneWayMaster.Ferr2DSettings;
		}
		UpdateFerr2DSetting(base.TerrainManager.OneWays, settings2);
	}

	public void CalculateRoomLevel()
	{
		int roomNumber = RoomNumber;
		if (base.SpawnControllerManager != null)
		{
			TunnelSpawnController[] tunnelSpawnControllers = base.SpawnControllerManager.TunnelSpawnControllers;
			foreach (TunnelSpawnController tunnelSpawnController in tunnelSpawnControllers)
			{
				if ((bool)tunnelSpawnController && tunnelSpawnController.Tunnel != null && tunnelSpawnController.Tunnel.Root != null)
				{
					roomNumber = tunnelSpawnController.Tunnel.Root.Room.BiomeControllerIndex;
					break;
				}
			}
		}
		int num = ((RoomType == RoomType.Boss) ? (BiomeDataLibrary.GetData(base.BiomeType).BossStartLevel * Mathf.RoundToInt(1f + BurdenManager.GetBurdenStatGain(BurdenType.RoomCount))) : RoomUtility.GetRoomLevel(base.BiomeType, roomNumber));
		int num2 = SaveManager.PlayerSaveData.NewGamePlusLevel * 44;
		num += num2;
		if (m_levelOverride > -1)
		{
			num = m_levelOverride;
		}
		SetLevel(num);
	}

	public void InjectLevel()
	{
		ILevelConsumer[] componentsInChildren = GetComponentsInChildren<ILevelConsumer>();
		foreach (ILevelConsumer levelConsumer in componentsInChildren)
		{
			if (levelConsumer != this)
			{
				levelConsumer.SetLevel(Level);
			}
		}
	}

	public void MoveToCoordinates(Vector2 coords)
	{
		base.transform.position = GridController.GetWorldPositionFromRoomCoordinates(coords, Size);
	}

	public void OnMerge()
	{
		m_roomMergedRelay.Dispatch(this, EventArgs.Empty);
	}

	private void UpdateVisualAppearance()
	{
		BiomeType terrainBiomeType = ArtUtility.GetTerrainBiomeType(this);
		BiomeArtData biomeArtData = BiomeArtDataOverride;
		if (!biomeArtData)
		{
			biomeArtData = BiomeArtDataLibrary.GetArtData(terrainBiomeType);
		}
		ApplyBiomeArtData(biomeArtData);
	}

	private void UpdateFerr2DSetting(IEnumerable<Ferr2DT_PathTerrain> ferr2DObjects, Ferr2DSettings settings)
	{
		if (!settings.Master)
		{
			return;
		}
		QualitySettings.GetQualityLevel();
		foreach (Ferr2DT_PathTerrain ferr2DObject in ferr2DObjects)
		{
			if (!ferr2DObject.OverrideDisabledMergeMaterial)
			{
				ferr2DObject.SetMaterial(settings.Material);
			}
			ferr2DObject.fillSplit = false;
			ferr2DObject.fillSplitDistance = settings.GridSpacing;
			ferr2DObject.vertexColorType = settings.ColorType;
			ferr2DObject.vertexGradientDistance = settings.VertexGradientDistance;
			ferr2DObject.vertexGradient = settings.VertexGradient;
			ferr2DObject.pixelsPerUnit = settings.PixelsPerUnit;
			ferr2DObject.createTangents = settings.CreateTangents;
			ferr2DObject.fillZ = settings.FillZ;
			Renderer component = ferr2DObject.GetComponent<Renderer>();
			component.sortingLayerID = settings.SortingLayer;
			component.sortingOrder = settings.OrderInLayer;
		}
	}

	private void AddDoorEventHandlers(Door door)
	{
		door.DoorDestroyedRelay.AddListener(OnDoorDestroyed);
		door.PlayerEnterRelay.AddListener(base.OnPlayerExitViaDoor);
		door.DoorConnectRelay.AddListener(OnDoorConnect);
		door.CloseRelay.AddListener(OnDoorClose);
	}

	private void RemoveDoor(Door door)
	{
		Doors.Remove(door);
		door.DoorDestroyedRelay.RemoveListener(OnDoorDestroyed);
		door.PlayerEnterRelay.RemoveListener(base.OnPlayerExitViaDoor);
		door.DoorConnectRelay.RemoveListener(OnDoorConnect);
		door.CloseRelay.RemoveListener(OnDoorClose);
	}

	protected virtual void DispatchRoomConnectedRelay(BaseRoom room)
	{
		if (m_roomConnectedEventArgs == null)
		{
			m_roomConnectedEventArgs = new RoomConnectedEventArgs(this, room);
		}
		else
		{
			m_roomConnectedEventArgs.Initialize(this, room);
		}
		m_roomConnectedRelay.Dispatch(this, m_roomConnectedEventArgs);
	}

	private void OnDoorClose(object sender, DoorEventArgs eventArgs)
	{
		Door door = eventArgs.Door;
		RemoveDoor(door);
		m_doorClosedRelay.Dispatch(this, EventArgs.Empty);
	}

	private void OnDoorConnect(object sender, DoorConnectEventArgs eventArgs)
	{
		if (eventArgs.DoorB != null)
		{
			if (eventArgs.DoorB.Room != null)
			{
				AddConnectedRoom(eventArgs.DoorB.Room);
				return;
			}
			Debug.LogFormat("<color=red>[{0}]: Door ({1}) is connected to a Door whose Room Property is null. Did you forget to Initialise that Door's Room?</color>", this, eventArgs.DoorA.Side.ToString() + "-" + eventArgs.DoorA.Number);
		}
		else
		{
			Debug.LogFormat("[{0}]: DoorB argument is null", this);
		}
	}

	private void OnDoorDestroyed(object sender, EventArgs eventArgs)
	{
		Door door = sender as Door;
		RemoveDoor(door);
	}

	protected override int GetDecoSeed()
	{
		int result = -1;
		if (GridPointManager != null)
		{
			result = GridPointManager.DecoSeed;
		}
		return result;
	}

	protected override int GetSpecialPropSeed()
	{
		int result = -1;
		if (GridPointManager != null)
		{
			result = GridPointManager.SpecialPropSeed;
		}
		return result;
	}

	public Door GetDoor(RoomSide side, int doorNumber)
	{
		Door result = null;
		List<Door> doorsOnSide = GetDoorsOnSide(side);
		if (doorsOnSide.Count > 0)
		{
			for (int i = 0; i < doorsOnSide.Count; i++)
			{
				if (doorsOnSide[i].Number == doorNumber)
				{
					result = doorsOnSide[i];
					break;
				}
			}
		}
		return result;
	}

	public List<Door> GetDoorsOnSide(RoomSide side)
	{
		if (side != RoomSide.Any)
		{
			m_doorListHelper_STATIC.Clear();
			List<Door> doors = Doors;
			int count = doors.Count;
			for (int i = 0; i < count; i++)
			{
				Door door = doors[i];
				if (door.Side == side)
				{
					m_doorListHelper_STATIC.Add(door);
				}
			}
			return m_doorListHelper_STATIC;
		}
		return Doors;
	}

	public List<Door> GetDoorsOnSideCopy(RoomSide side)
	{
		return GetDoorsOnSide(side).ToList();
	}

	public void SetCanMerge(bool canMerge)
	{
		if (CanMerge != canMerge)
		{
			CanMerge = canMerge;
		}
	}

	public void SetIsMirrored(bool isMirrored)
	{
		IsMirrored = isMirrored;
	}

	public override void SetLevel(int value)
	{
		m_level = value;
		InjectLevel();
	}

	public void SetUnitDimensions(int unitWidth, int unitHeight)
	{
		UnitDimensions = new Vector2Int(unitWidth, unitHeight);
	}

	public void ForceBiomeArtDataOverride(BiomeArtData biomeArtDataOverride)
	{
		m_biomeArtDataOverride = biomeArtDataOverride;
	}

	public void ResetBounds()
	{
		m_bounds = default(Bounds);
	}

	[SpecialName]
	GameObject ISummoner.get_gameObject()
	{
		return base.gameObject;
	}
}
