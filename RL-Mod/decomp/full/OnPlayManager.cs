using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using FMODUnity;
using GameEventTracking;
using RLWorldCreation;
using Rooms;
using UnityEngine;

public class OnPlayManager : MonoBehaviour
{
	private List<Room> m_additionalRooms;

	private Room m_baseRoom;

	private Coroutine m_buildRoomCoroutine;

	private bool m_closeAllDoorsButOneToggle;

	private int m_currentBiomeIndexInArtDataLibrary;

	private int m_currentDoorIndex = -1;

	private BaseRoom m_currentRoom;

	private int m_cutoutCount = -1;

	private int m_doorCount = -1;

	private static OnPlayManager m_instance = null;

	private GameObject m_player;

	private RoomCreator m_roomCreator;

	private Dictionary<SkyBiomeArtData, Sky> m_skyTable = new Dictionary<SkyBiomeArtData, Sky>();

	private Dictionary<WeatherBiomeArtData, Weather[]> m_weatherTable = new Dictionary<WeatherBiomeArtData, Weather[]>();

	private static Vector2 ROOM_FOR_TUNNEL_TESTING_POSITION = new Vector2(1000f, 1000f);

	private const string CREATE_TRANSITION_POINTS_EDITOR_PREFS_KEY = "CreateTransitionPoints";

	[SerializeField]
	private Room m_roomOverride;

	[SerializeField]
	private KeyCode m_cycleClosedDoorKey = KeyCode.KeypadPlus;

	[SerializeField]
	private KeyCode m_toggleCloseAllDoorsButOneKey = KeyCode.KeypadMinus;

	[SerializeField]
	private KeyCode m_cycleBiome = KeyCode.End;

	[SerializeField]
	private KeyCode m_cycleTransitionPointBiome = KeyCode.PageDown;

	[SerializeField]
	private KeyCode m_flipRoom = KeyCode.Insert;

	[SerializeField]
	private KeyCode m_cycleDifficulty = KeyCode.Period;

	[SerializeField]
	private KeyCode m_randomizeHazards = KeyCode.Slash;

	[SerializeField]
	[ReadOnly]
	private BiomeType m_currentBiome;

	[SerializeField]
	private bool m_attachRoomsToDoors = true;

	[SerializeField]
	private bool m_flipCurrentRoomHorizontally;

	[SerializeField]
	private BiomeType m_startingBiome = BiomeType.Castle;

	[SerializeField]
	private bool m_isCameraConstrainerEnabled = true;

	[Header("Overrides")]
	[SerializeField]
	private int m_roomLevel = 1;

	private BiomeType m_previousBiome;

	private BiomeController m_biomeController;

	public List<BaseRoom> ActiveRooms
	{
		get
		{
			List<BaseRoom> list = new List<BaseRoom>();
			if (m_currentRoom != null)
			{
				list.Add(m_currentRoom);
			}
			if (m_additionalRooms != null && m_additionalRooms.Count > 0)
			{
				list.AddRange(((IEnumerable<Room>)m_additionalRooms).Select((Func<Room, BaseRoom>)((Room room) => room)));
			}
			return list;
		}
	}

	private static OnPlayManager Instance
	{
		get
		{
			if (!Application.isPlaying)
			{
				m_instance = UnityEngine.Object.FindObjectOfType<OnPlayManager>();
			}
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool IsCameraConstrainerEnabled
	{
		get
		{
			return Instance.m_isCameraConstrainerEnabled;
		}
		private set
		{
			Instance.m_isCameraConstrainerEnabled = value;
		}
	}

	public static bool IsInstantiated => Instance != null;

	public static Room CurrentRoom
	{
		get
		{
			if (Instance != null)
			{
				return Instance.m_currentRoom as Room;
			}
			return null;
		}
	}

	public static List<BaseRoom> RoomList => m_instance.ActiveRooms;

	public static BiomeController BiomeController => Instance.m_biomeController;

	public static BiomeType StartingBiome
	{
		get
		{
			return Instance.m_startingBiome;
		}
		private set
		{
			Instance.m_startingBiome = value;
		}
	}

	public static KeyCode CycleBiome => Instance.m_cycleBiome;

	public static KeyCode FlipRoom => Instance.m_flipRoom;

	public static KeyCode ToggleCloseAllDoorsButOneKey => Instance.m_toggleCloseAllDoorsButOneKey;

	public static KeyCode CycleClosedDoorKey => Instance.m_cycleClosedDoorKey;

	public static KeyCode CycleDifficulty => Instance.m_cycleDifficulty;

	public static KeyCode RandomizeHazards => Instance.m_randomizeHazards;

	public Room BaseRoom
	{
		get
		{
			return m_baseRoom;
		}
		private set
		{
			m_baseRoom = value;
		}
	}

	public static BiomeType CurrentBiome
	{
		get
		{
			if (Application.isPlaying)
			{
				return Instance.m_currentBiome;
			}
			return BiomeType.None;
		}
		private set
		{
			if (Application.isPlaying)
			{
				Instance.m_currentBiome = value;
			}
		}
	}

	public static BiomeType TransitionPointBiome
	{
		get
		{
			return BiomeType.None;
		}
		private set
		{
		}
	}

	public KeyCode CycleTransitionPointBiome
	{
		get
		{
			return m_cycleTransitionPointBiome;
		}
		private set
		{
			m_cycleTransitionPointBiome = value;
		}
	}

	public static event System.EventHandler WorldSetupEvent;

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			RuntimeManager.LoadBank("Master.strings");
			RuntimeManager.LoadBank("Master");
			RuntimeManager.LoadBank("Music");
			RuntimeManager.WaitForAllLoads();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator Start()
	{
		if (m_roomOverride == null)
		{
			Debug.LogFormat("<color=red>| {0} |  Room Override is null</color>", this);
			yield break;
		}
		DeactivateOtherRooms();
		StoreReferenceToBaseRoom();
		CurrentBiome = StartingBiome;
		CreateTunnelRooms_BuildStage.RoomCreatedEvent = (EventHandler<RoomEventArgs>)Delegate.Combine(CreateTunnelRooms_BuildStage.RoomCreatedEvent, new EventHandler<RoomEventArgs>(OnTunnelDestinationCreated));
		yield return new WaitUntil(() => CameraController.IsInstantiated);
		CameraController.SetCameraSettingsForBiome(CurrentBiome, BaseRoom);
		m_currentBiomeIndexInArtDataLibrary = BiomeArtDataLibrary.ArtDataTable.FindIndex((BiomeArtDataEntry entry) => entry.BiomeType == CurrentBiome);
		m_buildRoomCoroutine = StartCoroutine(RecreateRoom());
		yield return new WaitUntil(() => m_buildRoomCoroutine == null);
	}

	private void DeactivateOtherRooms()
	{
	}

	private void Update()
	{
		if (!(m_currentRoom == null) && (Input.GetKeyDown(CycleClosedDoorKey) || Input.GetKeyDown(ToggleCloseAllDoorsButOneKey) || Input.GetKeyDown(CycleBiome) || Input.GetKeyDown(FlipRoom) || Input.GetKeyDown(CycleDifficulty) || Input.GetKeyDown(RandomizeHazards) || Input.GetKeyDown(CycleTransitionPointBiome)) && m_buildRoomCoroutine == null)
		{
			if (Input.GetKeyDown(CycleClosedDoorKey))
			{
				CycleThroughDoors(closeAllButOneDoor: false);
			}
			else if (Input.GetKeyDown(ToggleCloseAllDoorsButOneKey))
			{
				CycleThroughDoors(closeAllButOneDoor: true);
			}
			else if (Input.GetKeyDown(CycleBiome))
			{
				CycleThroughBiomes();
			}
			else if (Input.GetKeyDown(FlipRoom))
			{
				m_flipCurrentRoomHorizontally = !m_flipCurrentRoomHorizontally;
			}
			else if (Input.GetKeyDown(CycleDifficulty))
			{
				CycleThroughDifficultyLevels();
			}
			else if (Input.GetKeyDown(RandomizeHazards))
			{
				RandomizeHazardsInRoom();
			}
			else if (Input.GetKeyDown(CycleTransitionPointBiome))
			{
				CycleThroughTransitionPointBiome();
			}
			SetupWorld();
		}
	}

	private void CycleThroughTransitionPointBiome()
	{
		BiomeType[] array = Enum.GetValues(typeof(BiomeType)) as BiomeType[];
		List<BiomeType> list = new List<BiomeType> { BiomeType.None };
		for (int i = 0; i < array.Length; i++)
		{
			if (BiomeType_RL.IsValidBiome(array[i]))
			{
				list.Add((BiomeType)array.GetValue(i));
			}
		}
		int num = 0;
		for (int j = 0; j < list.Count; j++)
		{
			if (TransitionPointBiome == list[j])
			{
				num = j;
				break;
			}
		}
		num = ((num != list.Count - 1) ? (num + 1) : 0);
		SetTransitionPointBiome(list[num]);
	}

	private IEnumerator CloseDoors(bool closeAllButOne)
	{
		if (m_roomOverride == null)
		{
			Debug.LogFormat("<color=red>| {0} | Room Override is null</color>", this);
			yield break;
		}
		if (BaseRoom == null)
		{
			Debug.LogFormat("<color=red>| {0} | Base Room is null</color>", this);
			yield break;
		}
		if (m_currentRoom == null)
		{
			Debug.LogFormat("<color=red>| {0} | Current Room is null</color>", this);
			yield break;
		}
		List<Door> doors = m_currentRoom.Doors;
		for (int i = 0; i < doors.Count; i++)
		{
			if (doors[i].DisabledFromLevelEditor)
			{
				doors[i].Close();
				i--;
			}
		}
		if (m_currentDoorIndex >= 0 && m_currentDoorIndex < doors.Count)
		{
			if (closeAllButOne)
			{
				for (int num = doors.Count - 1; num >= 0; num--)
				{
					if (num != m_currentDoorIndex)
					{
						doors[num].Close();
					}
				}
			}
			else
			{
				doors[m_currentDoorIndex].Close();
			}
		}
		yield return null;
	}

	private IEnumerator CompleteWorldCreation()
	{
		yield return null;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.LevelEditorWorldCreationComplete, this, new LevelEditorWorldCreationCompleteEventArgs(m_currentRoom));
		yield return null;
		yield return null;
	}

	private void CreateRoomBackgrounds()
	{
		if (CurrentBiome != BiomeType.Forest)
		{
			_ = CurrentBiome != BiomeType.Tutorial;
		}
		else
			_ = 0;
		BiomeCreatorTools.CreateBackground(m_currentRoom);
		if (m_additionalRooms == null)
		{
			return;
		}
		foreach (Room additionalRoom in m_additionalRooms)
		{
			BiomeCreatorTools.CreateBackground(additionalRoom);
		}
	}

	private void BuildTerrain()
	{
		if (m_additionalRooms != null)
		{
			foreach (Room additionalRoom in m_additionalRooms)
			{
				RoomUtility.BuildAllFerr2DTerrains(additionalRoom);
			}
		}
		RoomUtility.BuildAllFerr2DTerrains(m_currentRoom);
	}

	private IEnumerator CreateInstanceOfBaseRoom()
	{
		if (m_roomOverride == null)
		{
			Debug.LogFormat("<color=red>| {0} | Room Override is null</color>", this);
			yield break;
		}
		BaseRoom currentRoom = m_currentRoom;
		if (currentRoom != null)
		{
			currentRoom.PlayerExit(null);
			UnityEngine.Object.Destroy(currentRoom.gameObject);
		}
		Vector3 position = GridController.GetWorldPositionFromRoomCoordinates(Vector2.zero, BaseRoom.Size);
		if (m_flipCurrentRoomHorizontally)
		{
			m_currentRoom = RoomUtility.CreateMirrorVersionOfRoom(BaseRoom);
			m_currentRoom.gameObject.transform.position = position;
		}
		else
		{
			m_currentRoom = UnityEngine.Object.Instantiate(BaseRoom, position, Quaternion.identity);
		}
		m_currentRoom.BiomeType = m_currentBiome;
		m_currentRoom.gameObject.transform.SetParent(m_biomeController.gameObject.transform);
		m_currentRoom.gameObject.SetActive(value: true);
		RoomMetaData roomMetaData = new RoomMetaData(m_currentRoom as Room);
		RoomType roomType = GetRoomType(m_currentRoom as Room);
		GridPointManager gridPointManager = new GridPointManager(0, Vector2Int.zero, m_currentBiome, roomType, roomMetaData, m_flipCurrentRoomHorizontally);
		int randomNumber = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "Get Random Room Seed", 0, int.MaxValue);
		gridPointManager.SetRoomSeed(randomNumber);
		gridPointManager.SetBiomeControllerIndex(0);
		m_currentRoom.SetBiomeControllerIndex(0);
		(m_currentRoom as Room).Initialise(gridPointManager);
		m_biomeController.StandardRoomCreatedByWorldBuilder(m_currentRoom as Room);
		yield return null;
		if (m_attachRoomsToDoors)
		{
			yield return CreateRoomsConnectedToCurrentRoom();
		}
		if ((m_currentRoom as Room).LevelOverride == -1)
		{
			(m_currentRoom as Room).SetLevel(m_roomLevel);
		}
	}

	private RoomType GetRoomType(Room room)
	{
		RoomType result = RoomType.Standard;
		if (room.GetComponent<BossRoomController>() != null)
		{
			result = RoomType.Boss;
		}
		else if ((bool)room.GetComponent<FairyRoomController>())
		{
			result = RoomType.Fairy;
		}
		return result;
	}

	private IEnumerator CreateRoomsConnectedToCurrentRoom()
	{
		if (m_roomCreator == null)
		{
			m_roomCreator = GetComponent<RoomCreator>();
		}
		if (m_additionalRooms == null)
		{
			m_additionalRooms = new List<Room>();
		}
		else
		{
			for (int num = m_additionalRooms.Count - 1; num >= 0; num--)
			{
				Room room = m_additionalRooms[num];
				if (room != null && room.gameObject != null)
				{
					UnityEngine.Object.Destroy(room.gameObject);
				}
			}
			m_additionalRooms.Clear();
		}
		yield return null;
		int biomeControllerIndex = 1;
		foreach (Door door in m_currentRoom.Doors)
		{
			int doorIndex = 0;
			GameObject original = m_roomCreator.SimpleRoomPrefab;
			if (CurrentBiome == BiomeType.Stone && (door.Side == RoomSide.Left || door.Side == RoomSide.Right))
			{
				original = m_roomCreator.Bridge1x3RoomPrefab;
				doorIndex = door.Number;
			}
			GameObject obj = UnityEngine.Object.Instantiate(original, new Vector3(-1000f, 0f, 0f), Quaternion.identity);
			obj.name = "DummyRoom:" + door.Side.ToString() + "-" + door.Number;
			Room component = obj.GetComponent<Room>();
			component.BiomeType = m_currentBiome;
			obj.transform.SetParent(m_biomeController.gameObject.transform);
			ISpawnController[] componentsInChildren = obj.GetComponentsInChildren<ISpawnController>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].SpawnLogicController.RunIsSpawnedCheck();
			}
			m_additionalRooms.Add(component);
			BiomeType biomeType = CurrentBiome;
			if (biomeType == BiomeType.TowerExterior)
			{
				biomeType = BiomeType.Tower;
			}
			BiomeCreatorTools.PositionRoom(m_currentRoom as Room, door.Side, door.Number, component, doorIndex);
			RoomMetaData roomMetaData = new RoomMetaData(component);
			RoomContentMetaDataFactory.CreateContentMetaData(roomMetaData);
			int roomNumber = biomeControllerIndex;
			DoorLocation location = new DoorLocation(door.Side, door.Number);
			Vector2Int coordinates = GridController.GetRoomGridCoordinates(location: new DoorLocation(RoomUtility.GetOppositeSide(door.Side), 0), buildCoordinates: GridController.GetDoorLeadsToGridCoordinates((m_currentRoom as Room).GridPointManager.GridCoordinates, (m_currentRoom as Room).GridPointManager.Size, location), roomSize: roomMetaData.Size);
			GridPointManager gridPointManager = new GridPointManager(roomNumber, coordinates, biomeType, RoomType.Standard, roomMetaData, isRoomMirrored: false);
			gridPointManager.SetBiomeControllerIndex(biomeControllerIndex);
			component.Initialise(gridPointManager);
			component.SetBiomeControllerIndex(biomeControllerIndex);
			CreateSky(component);
			CreateWeather(component);
			biomeControllerIndex++;
			m_biomeController.StandardRoomCreatedByWorldBuilder(component);
			yield return null;
		}
		m_biomeController.GridPointManager.SetStandaloneRoomCount(m_biomeController.StandaloneRoomCount);
		yield return new WaitForSeconds(0.1f);
		foreach (Room additionalRoom in m_additionalRooms)
		{
			BiomeCreatorTools.CloseUnconnectedDoorsInRoom(additionalRoom);
			BiomeCreatorTools.CreateCinemachineVirtualCamera(additionalRoom);
			additionalRoom.gameObject.SetActive(value: false);
		}
	}

	private void CreateSky(BaseRoom room)
	{
		if ((bool)BiomeDataLibrary.GetData(CurrentBiome))
		{
			BiomeType biome = CurrentBiome;
			Room room2 = room as Room;
			if (!room2.IsNativeNull() && room2.AppearanceOverride != 0)
			{
				biome = room2.AppearanceOverride;
			}
			BiomeArtData biomeArtData = room.BiomeArtDataOverride;
			if (!biomeArtData)
			{
				biomeArtData = BiomeArtDataLibrary.GetArtData(biome);
			}
			if (!m_skyTable.TryGetValue(biomeArtData.SkyData, out var value))
			{
				value = BiomeCreatorTools.CreateSkyInstance(biomeArtData);
				m_skyTable.Add(biomeArtData.SkyData, value);
			}
			value.RoomList.Add(room);
		}
	}

	private void CreateWeather(BaseRoom room)
	{
		BiomeType biome = CurrentBiome;
		Room room2 = room as Room;
		if (!room2.IsNativeNull() && room2.AppearanceOverride != 0)
		{
			biome = room2.AppearanceOverride;
		}
		BiomeArtData biomeArtData = room.BiomeArtDataOverride;
		if (!biomeArtData)
		{
			biomeArtData = BiomeArtDataLibrary.GetArtData(biome);
		}
		if (!biomeArtData)
		{
			return;
		}
		if (!m_weatherTable.TryGetValue(biomeArtData.WeatherData, out var value))
		{
			value = BiomeCreatorTools.CreateWeatherInstances(biomeArtData);
			m_weatherTable.Add(biomeArtData.WeatherData, value);
		}
		if (value != null)
		{
			Weather[] array = value;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].RoomList.Add(room);
			}
		}
	}

	private void CycleThroughBiomes()
	{
		m_currentBiomeIndexInArtDataLibrary++;
		if (m_currentBiomeIndexInArtDataLibrary == BiomeArtDataLibrary.ArtDataTable.Count)
		{
			m_currentBiomeIndexInArtDataLibrary = 0;
		}
		SetCurrentBiome(BiomeArtDataLibrary.ArtDataTable[m_currentBiomeIndexInArtDataLibrary].BiomeType);
	}

	private void CycleThroughDifficultyLevels()
	{
		GameUtility.Difficulty++;
		Debug.LogFormat("<color=purple>Difficulty Level = {0}</purple>", GameUtility.Difficulty);
	}

	private void CycleThroughDoors(bool closeAllButOneDoor)
	{
		m_closeAllDoorsButOneToggle = closeAllButOneDoor;
		if (m_doorCount == -1)
		{
			m_doorCount = BaseRoom.Doors.Count;
		}
		m_currentDoorIndex++;
		if (m_currentDoorIndex > m_doorCount - 1)
		{
			m_currentDoorIndex = -1;
		}
	}

	private void OnTunnelDestinationCreated(object sender, RoomEventArgs eventArgs)
	{
		Room room = eventArgs.Room as Room;
		m_additionalRooms.Add(room);
		CreateSky(room);
		CreateWeather(room);
		BiomeCreatorTools.CreateCinemachineVirtualCamera(eventArgs.Room);
	}

	private IEnumerator PlaceOneWaysAlongBottomDoors()
	{
		if (CurrentBiome == BiomeType.TowerExterior)
		{
			yield break;
		}
		List<Door> doorsOnSide = (m_currentRoom as Room).GetDoorsOnSide(RoomSide.Bottom);
		if (doorsOnSide == null || doorsOnSide.Count <= 0)
		{
			yield break;
		}
		foreach (Door door in doorsOnSide)
		{
			yield return new WaitUntil(() => door.Room != null);
			BiomeCreatorTools.PlaceOneWayAtDoor(door);
		}
	}

	private void RandomizeHazardsInRoom()
	{
		HazardRandomizer.RandomizeHazards(BaseRoom.SpawnControllerManager.SpawnControllers.Select((ISpawnController entry) => entry as IHazardSpawnController));
	}

	private IEnumerator RecreateRoom()
	{
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.UpdatePools, null, null);
		if (m_biomeController != null)
		{
			UnityEngine.Object.Destroy(m_biomeController.gameObject);
		}
		m_biomeController = WorldBuilder.CreateBiomeController(CurrentBiome, null);
		yield return CreateInstanceOfBaseRoom();
		yield return PlaceOneWaysAlongBottomDoors();
		if (m_previousBiome != CurrentBiome)
		{
			m_previousBiome = CurrentBiome;
			CreateSky(m_currentRoom);
			CreateWeather(m_currentRoom);
		}
		m_currentRoom.CinemachineCamera.GetComponent<CinemachineConfiner_RL>().enabled = IsCameraConstrainerEnabled;
		yield return CloseDoors(m_closeAllDoorsButOneToggle);
		BiomeType transitionPointBiome = TransitionPointBiome;
		if (transitionPointBiome != 0)
		{
			foreach (Door door in m_currentRoom.Doors)
			{
				yield return CreateTransitionPoints_BuildStage.RunFromLevelEditor(transitionPointBiome, door);
			}
		}
		UpdateSpawnLogicControllers();
		SpawnSimpleSpawnControllers();
		yield return CreateTunnelRooms_BuildStage.RunFromLevelEditor(m_biomeController, m_currentRoom as Room);
		yield return CompleteWorldCreation();
		BuildTerrain();
		CreateRoomBackgrounds();
		BiomeTransitionController.UpdateObjectPools(BiomeType.None, m_biomeController.Biome);
		MapController.Reset();
		MapController.Initialize();
		if (ItemDropManager.IsInitialized)
		{
			ItemDropManager.DisableAllItemDrops();
		}
		if (EnemyManager.IsInitialized)
		{
			EnemyManager.DisableAllEnemies();
			EnemyManager.DisableAllSummonedEnemies();
		}
		SpawnPlayer();
		if (OnPlayManager.WorldSetupEvent != null)
		{
			OnPlayManager.WorldSetupEvent(this, EventArgs.Empty);
		}
		m_buildRoomCoroutine = null;
	}

	public static void Restart()
	{
		GameEventTrackerManager.Reset();
		MapController.Reset();
		Instance.SetupWorld();
	}

	public static void SetBiome(BiomeType biome)
	{
		if (biome == BiomeType.Any || biome == BiomeType.None || biome == BiomeType.Editor || biome == BiomeType.Spawn || biome == BiomeType.Special || Instance.m_buildRoomCoroutine != null)
		{
			return;
		}
		for (int i = 0; i < BiomeArtDataLibrary.ArtDataTable.Count; i++)
		{
			if (biome == BiomeArtDataLibrary.ArtDataTable[i].BiomeType)
			{
				Instance.m_currentBiomeIndexInArtDataLibrary = i;
				break;
			}
		}
		SetCurrentBiome(biome);
		Instance.m_buildRoomCoroutine = Instance.StartCoroutine(Instance.RecreateRoom());
	}

	public static void SetTransitionPointBiome(BiomeType transitionPointBiome, bool rebuild = false)
	{
		if (transitionPointBiome == BiomeType.None || BiomeType_RL.IsValidBiome(transitionPointBiome))
		{
			TransitionPointBiome = transitionPointBiome;
			if (rebuild)
			{
				Instance.m_buildRoomCoroutine = Instance.StartCoroutine(Instance.RecreateRoom());
			}
		}
	}

	public static void SetDifficulty(int difficulty)
	{
		GameUtility.Difficulty = difficulty;
		Instance.m_buildRoomCoroutine = Instance.StartCoroutine(Instance.RecreateRoom());
	}

	public static void SetCameraConstrainerIsEnabled(bool isEnabled)
	{
		IsCameraConstrainerEnabled = isEnabled;
		_ = Application.isPlaying;
		if (Application.isPlaying)
		{
			Instance.m_currentRoom.CinemachineCamera.GetComponent<CinemachineConfiner_RL>().enabled = IsCameraConstrainerEnabled;
		}
	}

	private static void SetCurrentBiome(BiomeType biome)
	{
		Instance.m_previousBiome = CurrentBiome;
		CurrentBiome = biome;
		CameraController.SetCameraSettingsForBiome(CurrentBiome, null);
	}

	public static void SetStartingBiome(BiomeType biome)
	{
		if (BiomeType_RL.IsValidBiome(biome))
		{
			StartingBiome = biome;
		}
	}

	public void SetupWorld()
	{
		PlayerManager.GetPlayerController().ResetCharacter();
		m_buildRoomCoroutine = StartCoroutine(RecreateRoom());
	}

	private void SpawnSimpleSpawnControllers()
	{
		foreach (BaseRoom activeRoom in ActiveRooms)
		{
			ISpawnController[] spawnControllers = activeRoom.SpawnControllerManager.SpawnControllers;
			foreach (ISpawnController spawnController in spawnControllers)
			{
				if (spawnController is ISimpleSpawnController)
				{
					(spawnController as ISimpleSpawnController).Spawn();
				}
			}
		}
	}

	private void SpawnPlayer()
	{
		if (m_currentRoom != null)
		{
			if (m_player == null)
			{
				m_player = PlayerManager.GetPlayer();
			}
			else
			{
				m_player.gameObject.SetActive(value: true);
			}
			PlayerManager.GetPlayerController().CurrentlyInRoom = null;
			PlayerManager.GetPlayerController().ResetStates();
			if (PlayerManager.GetPlayerController().IsInvincible)
			{
				PlayerManager.GetPlayerController().CharacterHitResponse.SetInvincibleTime(PlayerManager.GetPlayerController().InvincibilityTimer, additive: false, playInvincibility: true);
			}
			m_currentRoom.PlacePlayerInRoom(null);
		}
		else
		{
			Debug.LogFormat("<color=red>{0}: Can't spawn Player because m_currentRoom is null</color>", Time.frameCount);
		}
	}

	public void ClearActiveRooms()
	{
		m_additionalRooms.Clear();
		m_currentRoom = null;
	}

	private void StoreReferenceToBaseRoom()
	{
		m_baseRoom = m_roomOverride;
		BiomeCreatorTools.CreateCinemachineVirtualCamera(BaseRoom);
		BaseRoom.gameObject.SetActive(value: false);
	}

	private void UpdateSpawnLogicControllers(SpawnScenarioCheckStage checkStage = SpawnScenarioCheckStage.PreMerge)
	{
		foreach (BaseRoom activeRoom in ActiveRooms)
		{
			ISpawnController[] spawnControllers = activeRoom.SpawnControllerManager.SpawnControllers;
			foreach (ISpawnController spawnController in spawnControllers)
			{
				if (spawnController != null && spawnController.SpawnLogicController != null)
				{
					spawnController.SpawnLogicController.RunIsSpawnedCheck(checkStage);
				}
			}
		}
	}

	private void AddCurrentRoomToGrid(Room room)
	{
		new RoomMetaData(room);
	}
}
