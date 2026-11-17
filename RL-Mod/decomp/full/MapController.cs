using System;
using System.Collections;
using System.Collections.Generic;
using RLWorldCreation;
using RL_Windows;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapController : MonoBehaviour
{
	private const float ICON_Y_POSITION_OFFSET = 2f;

	[SerializeField]
	private CameraFollowMode m_followMode = CameraFollowMode.Snap;

	[SerializeField]
	private Camera m_miniMapCamera;

	[SerializeField]
	private Camera m_mapWindowCamera;

	[SerializeField]
	private Camera m_deathMapCamera;

	[Header("Map Entry Prefab")]
	[SerializeField]
	private MapRoomEntry m_mapEntryPrefab;

	[Header("Map Icon Animations")]
	[SerializeField]
	private AnimationCurve m_animCurve;

	[SerializeField]
	private float m_animLoopdelay = 1f;

	[Header("Player Icon")]
	[SerializeField]
	private GameObject m_mapPlayerIconPrefab;

	[Header("Door Icons")]
	[SerializeField]
	private GameObject m_mapHorizontalDoor;

	[SerializeField]
	private GameObject m_mapVerticalDoor;

	[Header("Enemy Icons")]
	[SerializeField]
	private GameObject m_mapEnemyIcon;

	[SerializeField]
	private GameObject m_mapEnemyKilledIcon;

	[Header("Teleporter Icons")]
	[SerializeField]
	private GameObject m_mapTeleporterIcon;

	[SerializeField]
	private SpriteRenderer m_mapTeleporterLine;

	[Header("Chest Icons")]
	[SerializeField]
	private GameObject m_mapChestClosedIconPrefab;

	[SerializeField]
	private GameObject m_mapChestOpenIconPrefab;

	[SerializeField]
	private GameObject m_mapFairyChestClosedIconPrefab;

	[SerializeField]
	private GameObject m_mapFairyChestOpenedIconPrefab;

	[Header("Special Room Icons")]
	[SerializeField]
	private GameObject m_specialRoomIconPrefab;

	[SerializeField]
	private GameObject m_specialRoomUsedIconPrefab;

	[SerializeField]
	private GameObject m_heirloomRoomIconPrefab;

	[SerializeField]
	private GameObject m_heirloomRoomCompleteIconPrefab;

	[Header("Boss Icons")]
	[SerializeField]
	private GameObject m_bossIconPrefab;

	[SerializeField]
	private GameObject m_bossBeatenIconPrefab;

	[SerializeField]
	private GameObject m_subbossIconPrefab;

	[SerializeField]
	private GameObject m_subbossBeatenIconPrefab;

	[Header("NPC Icons")]
	[SerializeField]
	private GameObject m_npcRoomIconPrefab;

	[SerializeField]
	private GameObject m_npcRoomUsedIconPrefab;

	[Header("Special Indicator Icon")]
	[SerializeField]
	private GameObject m_specialIndicatorIconPrefab;

	public const float ROOM_ABS_SHRINK_AMOUNT = 0.06f;

	private const string ENEMY_LOCATION_NAME = "Enemy Objects";

	private const string ENEMIES_KILLED_LOCATION_NAME = "Enemies Killed";

	private const float FOLLOW_IF_X_OR_Y_ARE_LESS_THAN = 0.25f;

	private const float FOLLOW_IF_X_OR_Y_ARE_GREATER_THAN = 0.75f;

	public static Vector2 MAP_STARTING_POSITION = new Vector2(-100000f, -100000f);

	public const float FULL_MAP_CAMERA_SIZE = 9f;

	public const float HUD_MAP_CAMERA_SIZE = 2f;

	private Vector3 m_cameraDestination = new Vector3(-1f, -1f, -1f);

	private Coroutine m_tweenCameraCoroutine;

	private static bool m_isCameraFollowOn = false;

	private static Vector3 m_previousPlayerIconPosition;

	private static MapController m_instance = null;

	private static GameObject m_playerIcon;

	private static SpriteRenderer m_teleporterLine;

	private static GameObject m_mapObj;

	private Dictionary<BiomeType, Dictionary<int, MapRoomEntry>> m_mapRoomEntryDict = new Dictionary<BiomeType, Dictionary<int, MapRoomEntry>>();

	private static bool m_isInitialized = false;

	private List<GlobalTeleporterController> m_teleporterList = new List<GlobalTeleporterController>();

	private List<GridPointManager> m_gridPointManagersContainingTeleporters = new List<GridPointManager>();

	private static Transform m_enemyKilledLocation = null;

	private Rect m_globalMapBounds;

	private Dictionary<BiomeType, Rect> m_biomeMapBoundsTable = new Dictionary<BiomeType, Rect>();

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onBiomeEnter;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitRoom;

	private Action<MonoBehaviour, EventArgs> m_onUpdateCurrentRoomIcons;

	private Action<MonoBehaviour, EventArgs> m_linkTunnelBonusRooms;

	private Dictionary<BiomeType, List<MapRoomEntry>> m_mapEntriesThatNeedLinking = new Dictionary<BiomeType, List<MapRoomEntry>>();

	private List<TunnelSpawnController> m_mapEntryTunnelSpawners = new List<TunnelSpawnController>();

	private const string TOWER_EXTERIOR_NAME_CHECK = "Exterior";

	public static List<GlobalTeleporterController> TeleporterList => m_instance.m_teleporterList;

	public static bool IsCameraMoving { get; private set; }

	public static bool IsInitialized
	{
		get
		{
			return m_isInitialized;
		}
		private set
		{
			m_isInitialized = value;
		}
	}

	public static Camera Camera => m_instance.m_miniMapCamera;

	public static Camera MapWindowCamera => m_instance.m_mapWindowCamera;

	public static Camera DeathMapCamera => m_instance.m_deathMapCamera;

	public static Vector3 PlayerIconPosition
	{
		get
		{
			if ((bool)m_playerIcon)
			{
				return m_playerIcon.transform.position;
			}
			return MAP_STARTING_POSITION;
		}
	}

	public static CameraFollowMode FollowMode
	{
		get
		{
			return m_instance.m_followMode;
		}
		set
		{
			m_instance.m_followMode = value;
		}
	}

	public static List<GridPointManager> GridPointManagersContainingTeleporters
	{
		get
		{
			return m_instance.m_gridPointManagersContainingTeleporters;
		}
		private set
		{
			m_instance.m_gridPointManagersContainingTeleporters = value;
		}
	}

	public static float MapIconAnimationCurveValue { get; private set; }

	public static GameObject GetSpecialIconPrefab(GridPointManager roomToCheck, bool getUsed, bool isMergeRoom)
	{
		switch (roomToCheck.RoomType)
		{
		case RoomType.BossEntrance:
			if (!getUsed)
			{
				return m_instance.m_bossIconPrefab;
			}
			return m_instance.m_bossBeatenIconPrefab;
		case RoomType.Fairy:
			if (!getUsed)
			{
				return m_instance.m_mapFairyChestClosedIconPrefab;
			}
			return m_instance.m_mapFairyChestOpenedIconPrefab;
		case RoomType.Bonus:
		case RoomType.Mandatory:
		{
			SpecialRoomType specialRoomType = roomToCheck.RoomMetaData.SpecialRoomType;
			switch (specialRoomType)
			{
			case SpecialRoomType.Subboss:
			case SpecialRoomType.SubbossEntrance:
				if (!getUsed)
				{
					return m_instance.m_subbossIconPrefab;
				}
				return m_instance.m_subbossBeatenIconPrefab;
			case SpecialRoomType.Heirloom:
			case SpecialRoomType.HeirloomEntrance:
				if (!getUsed)
				{
					return m_instance.m_heirloomRoomIconPrefab;
				}
				return m_instance.m_heirloomRoomCompleteIconPrefab;
			default:
				if (roomToCheck.RoomType == RoomType.Mandatory && (specialRoomType == SpecialRoomType.None || specialRoomType == SpecialRoomType.WhitePipUnique) && !isMergeRoom)
				{
					if (!getUsed)
					{
						return m_instance.m_npcRoomIconPrefab;
					}
					return m_instance.m_npcRoomUsedIconPrefab;
				}
				if (!getUsed)
				{
					return m_instance.m_specialRoomIconPrefab;
				}
				return m_instance.m_specialRoomUsedIconPrefab;
			}
		}
		default:
			return null;
		}
	}

	public static Rect GetBiomeRect(BiomeType biomeType)
	{
		if (m_instance.m_biomeMapBoundsTable.TryGetValue(biomeType, out var value))
		{
			return value;
		}
		return Rect.zero;
	}

	public static MapRoomEntry GetMapRoomEntry(BiomeType biomeType, int biomeControllerIndex)
	{
		biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
		MapRoomEntry result = null;
		if (m_instance.m_mapRoomEntryDict.ContainsKey(biomeType) && m_instance.m_mapRoomEntryDict[biomeType].ContainsKey(biomeControllerIndex))
		{
			result = m_instance.m_mapRoomEntryDict[biomeType][biomeControllerIndex];
		}
		return result;
	}

	public static bool WasRoomVisited(BiomeType biomeType, int biomeControllerIndex)
	{
		bool result = false;
		MapRoomEntry mapRoomEntry = GetMapRoomEntry(biomeType, biomeControllerIndex);
		if (mapRoomEntry != null)
		{
			result = mapRoomEntry.WasVisited;
		}
		return result;
	}

	private void Awake()
	{
		if (!m_instance)
		{
			m_instance = this;
			m_onPlayerEnterRoom = OnPlayerEnterRoom;
			m_onBiomeEnter = OnBiomeEnter;
			m_onPlayerExitRoom = OnPlayerExitRoom;
			m_onUpdateCurrentRoomIcons = OnUpdateCurrentRoomIcons;
			m_linkTunnelBonusRooms = LinkTunnelBonusRooms;
			SceneManager.activeSceneChanged += OnActiveSceneChanged;
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeEnter, m_onBiomeEnter);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ChestOpened, m_onUpdateCurrentRoomIcons);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_onUpdateCurrentRoomIcons);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.SpecialRoomCompleted, m_onUpdateCurrentRoomIcons);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeCreationComplete, m_linkTunnelBonusRooms);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		m_miniMapCamera.targetTexture = null;
		m_isCameraFollowOn = false;
		m_previousPlayerIconPosition = Vector3.zero;
		if ((bool)m_playerIcon)
		{
			UnityEngine.Object.Destroy(m_playerIcon);
		}
		m_playerIcon = null;
		if ((bool)m_teleporterLine)
		{
			UnityEngine.Object.Destroy(m_teleporterLine);
		}
		m_teleporterLine = null;
		if ((bool)m_mapObj)
		{
			UnityEngine.Object.Destroy(m_mapObj);
		}
		m_mapObj = null;
		if ((bool)m_enemyKilledLocation)
		{
			UnityEngine.Object.Destroy(m_enemyKilledLocation);
		}
		m_enemyKilledLocation = null;
		m_isInitialized = false;
		m_instance = null;
		SceneManager.activeSceneChanged -= OnActiveSceneChanged;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeEnter, m_onBiomeEnter);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ChestOpened, m_onUpdateCurrentRoomIcons);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_onUpdateCurrentRoomIcons);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.SpecialRoomCompleted, m_onUpdateCurrentRoomIcons);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeCreationComplete, m_linkTunnelBonusRooms);
	}

	private void OnEnable()
	{
		StartCoroutine(MapIconAnimationCoroutine());
	}

	private IEnumerator MapIconAnimationCoroutine()
	{
		float startTime = Time.unscaledTime;
		bool isGamePaused = GameManager.IsGamePaused && !WindowManager.GetIsWindowOpen(WindowID.PlayerDeath);
		float duration = m_animCurve.keys[m_animCurve.keys.Length - 1].time;
		float elapsedPercent = 0f;
		while (elapsedPercent < 1f)
		{
			elapsedPercent = (Time.unscaledTime - startTime) / duration;
			if (isGamePaused)
			{
				MapIconAnimationCurveValue = m_animCurve.Evaluate(elapsedPercent);
			}
			else
			{
				MapIconAnimationCurveValue = 0f;
			}
			yield return null;
		}
		float delay = Time.unscaledTime + m_animLoopdelay;
		while (Time.unscaledTime < delay)
		{
			yield return null;
		}
		StartCoroutine(MapIconAnimationCoroutine());
	}

	public static Vector3 GetWorldPositionFromMap(Vector3 position)
	{
		Vector3 result = position;
		result /= 1f / 15f;
		result.z = 1f;
		result.x -= MAP_STARTING_POSITION.x;
		result.y -= MAP_STARTING_POSITION.y;
		return result;
	}

	public static void Initialize()
	{
		m_mapObj = new GameObject("Map Objects");
		m_mapObj.transform.SetParent(m_instance.transform);
		m_playerIcon = UnityEngine.Object.Instantiate(m_instance.m_mapPlayerIconPrefab, m_mapObj.transform);
		SetMapCameraPosition(MAP_STARTING_POSITION);
		UpdatePlayerIconPosition();
		m_teleporterLine = UnityEngine.Object.Instantiate(m_instance.m_mapTeleporterLine, m_mapObj.transform);
		m_teleporterLine.gameObject.SetActive(value: false);
		CreateWorldMap();
		SetCameraFollowIsOn(isOn: true);
		FollowMode = CameraFollowMode.Constant;
		foreach (KeyValuePair<BiomeType, Dictionary<int, MapRoomEntry>> item in m_instance.m_mapRoomEntryDict)
		{
			foreach (KeyValuePair<int, MapRoomEntry> item2 in item.Value)
			{
				if (item2.Value.RoomType == RoomType.Transition && SaveManager.PlayerSaveData.GetTeleporterIsUnlocked(item.Key))
				{
					MapRoomEntry value = item2.Value;
					value.gameObject.SetActive(value: true);
					value.WasVisited = true;
					value.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: true);
				}
				else
				{
					SetRoomVisible(item.Key, item2.Key, isVisible: false);
				}
			}
		}
		IsInitialized = true;
	}

	public static void LoadStageSaveData()
	{
		if (GameUtility.IsInLevelEditor)
		{
			return;
		}
		List<int> list = new List<int>();
		foreach (KeyValuePair<BiomeType, BiomeController> biomeController in WorldBuilder.BiomeControllers)
		{
			BiomeType key = biomeController.Key;
			BiomeController value = biomeController.Value;
			if (!(value != null) || value.GridPointManager.GridPointManagers == null)
			{
				continue;
			}
			list.Clear();
			foreach (GridPointManager gridPointManager in value.GridPointManager.GridPointManagers)
			{
				if (!list.Contains(gridPointManager.BiomeControllerIndex))
				{
					list.Add(gridPointManager.BiomeControllerIndex);
					SetRoomVisibleUsingSaveData(key, gridPointManager.BiomeControllerIndex, gridPointManager);
				}
			}
		}
	}

	public static void CentreCameraAroundPlayerIcon()
	{
		SetMapCameraPosition((Vector2)PlayerIconPosition);
	}

	public static void CreateWorldMap()
	{
		m_instance.m_biomeMapBoundsTable.Clear();
		m_instance.m_mapEntriesThatNeedLinking.Clear();
		foreach (KeyValuePair<BiomeType, BiomeController> biomeController in WorldBuilder.BiomeControllers)
		{
			BiomeType key = biomeController.Key;
			BiomeController value = biomeController.Value;
			m_instance.CreateGridPointRoomsOnMap(key, value.GridPointManager.GridPointManagers);
			if (TraitManager.IsTraitActive(TraitType.RevealAllChests))
			{
				ShowAllChestsInBiome(key);
			}
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveTuningForkTriggered))
		{
			SetCaveWhitePipVisibility(visible: true);
		}
	}

	public static Vector3 GetScaledPoint(Vector2 point)
	{
		return GetScaledPoint(Vector2.zero, point);
	}

	public static Vector3 GetScaledPoint(Vector2 center, float xPosition, float yPosition)
	{
		return GetScaledPoint(center, new Vector2(xPosition, yPosition));
	}

	public static Vector3 GetScaledPoint(Vector2 center, Vector2 point)
	{
		float x = 1f / 15f * (point.x - center.x) + center.x;
		float y = 1f / 15f * (point.y - center.y) + center.y;
		return new Vector3(x, y, 1f);
	}

	private void CreateRoomsOnMap(BiomeType biomeType, List<BaseRoom> rooms)
	{
		if (rooms == null || rooms.Count == 0)
		{
			Debug.LogFormat("<color=red>[{0}] rooms argument is null or empty</color>", m_instance);
			return;
		}
		biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
		Dictionary<int, MapRoomEntry> dictionary = new Dictionary<int, MapRoomEntry>();
		foreach (BaseRoom room in rooms)
		{
			MapRoomEntry value = CreateMapRoomEntryForRoom_LevelEditorOnly(room);
			if (!dictionary.ContainsKey(room.BiomeControllerIndex))
			{
				dictionary.Add(room.BiomeControllerIndex, value);
				continue;
			}
			Debug.Log("<color=red>WARNING: BiomeControllerIndex - " + room.BiomeControllerIndex + " for Biome - " + biomeType.ToString() + " already found in MapController.MapRoomEntryList</color>");
		}
		if (m_mapRoomEntryDict.ContainsKey(biomeType))
		{
			m_mapRoomEntryDict[biomeType] = dictionary;
		}
		else
		{
			m_mapRoomEntryDict.Add(biomeType, dictionary);
		}
	}

	private MapRoomEntry CreateMapRoomEntryForRoom_LevelEditorOnly(BaseRoom room)
	{
		MapRoomEntry mapRoomEntry = UnityEngine.Object.Instantiate(m_instance.m_mapEntryPrefab, m_mapObj.transform, worldPositionStays: true);
		mapRoomEntry.gameObject.name = room.gameObject.name + " - Map Room Entry";
		mapRoomEntry.gameObject.transform.position = GetMapPositionFromWorld(room.gameObject.transform.position);
		mapRoomEntry.CreateRoomTerrainForRoom(room);
		mapRoomEntry.CreateDoorIconsForRoom(m_instance.m_mapHorizontalDoor, m_instance.m_mapVerticalDoor, room);
		mapRoomEntry.CreateEnemyIconsForRoom(m_instance.m_mapEnemyIcon, m_instance.m_mapEnemyKilledIcon, room);
		mapRoomEntry.ToggleIconVisibility(MapIconType.EnemyKilled, -1, visible: false);
		mapRoomEntry.CreateChestIconsForRoom(m_instance.m_mapChestOpenIconPrefab, m_instance.m_mapChestClosedIconPrefab, room);
		mapRoomEntry.ToggleIconVisibility(MapIconType.ChestOpened, -1, visible: false);
		mapRoomEntry.RoomType = room.RoomType;
		switch (room.RoomType)
		{
		case RoomType.BossEntrance:
			mapRoomEntry.CreateSpecialRoomIconsForRoom(m_instance.m_bossIconPrefab, m_instance.m_bossBeatenIconPrefab, room);
			mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
			break;
		case RoomType.Fairy:
			mapRoomEntry.CreateSpecialRoomIconsForRoom(m_instance.m_mapFairyChestClosedIconPrefab, m_instance.m_mapFairyChestOpenedIconPrefab, room);
			mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
			break;
		case RoomType.Bonus:
		{
			SpecialRoomType specialRoomType = SpecialRoomType.None;
			BaseSpecialRoomController component = room.gameObject.GetComponent<BaseSpecialRoomController>();
			if ((bool)component)
			{
				specialRoomType = component.SpecialRoomType;
			}
			mapRoomEntry.SpecialRoomType = specialRoomType;
			switch (specialRoomType)
			{
			case SpecialRoomType.Subboss:
			case SpecialRoomType.SubbossEntrance:
				mapRoomEntry.CreateSpecialRoomIconsForRoom(m_instance.m_subbossIconPrefab, m_instance.m_subbossBeatenIconPrefab, room);
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
				break;
			case SpecialRoomType.Heirloom:
			case SpecialRoomType.HeirloomEntrance:
				mapRoomEntry.CreateSpecialRoomIconsForRoom(m_instance.m_heirloomRoomIconPrefab, m_instance.m_heirloomRoomCompleteIconPrefab, room);
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
				break;
			default:
				if (room.RoomType == RoomType.Mandatory && (specialRoomType == SpecialRoomType.None || specialRoomType == SpecialRoomType.WhitePipUnique))
				{
					mapRoomEntry.CreateSpecialRoomIconsForRoom(m_instance.m_npcRoomIconPrefab, m_instance.m_npcRoomUsedIconPrefab, room);
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
				}
				else
				{
					mapRoomEntry.CreateSpecialRoomIconsForRoom(m_instance.m_specialRoomIconPrefab, m_instance.m_specialRoomUsedIconPrefab, room);
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
				}
				break;
			}
			break;
		}
		}
		if (room.BiomeType == BiomeType.Cave && (room.RoomType == RoomType.BossEntrance || mapRoomEntry.SpecialRoomType == SpecialRoomType.SubbossEntrance || mapRoomEntry.SpecialRoomType == SpecialRoomType.WhitePip || mapRoomEntry.SpecialRoomType == SpecialRoomType.WhitePipUnique))
		{
			mapRoomEntry.CreateSpecialIndicatorIconForRoom(m_instance.m_specialIndicatorIconPrefab, room);
			mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
		}
		CreateTeleporterIconsForGridPoint((room as Room).GridPointManager, mapRoomEntry);
		mapRoomEntry.gameObject.SetActive(value: false);
		return mapRoomEntry;
	}

	private void CreateGridPointRoomsOnMap(BiomeType biomeType, List<GridPointManager> rooms)
	{
		if (rooms == null || rooms.Count == 0)
		{
			Debug.LogFormat("<color=red>[{0}] rooms argument is null or empty</color>", m_instance);
			return;
		}
		biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
		Dictionary<int, MapRoomEntry> dictionary = new Dictionary<int, MapRoomEntry>();
		List<GridPointManager> list = new List<GridPointManager>();
		List<GridPointManager> list2 = new List<GridPointManager>();
		for (int i = 0; i < rooms.Count; i++)
		{
			if (rooms[i].MergeWithGridPointManagers.Count == 0)
			{
				list.Add(rooms[i]);
			}
			else
			{
				if (list2.Contains(rooms[i]))
				{
					continue;
				}
				bool flag = true;
				for (int j = 0; j < rooms[i].MergeWithGridPointManagers.Count; j++)
				{
					if (list2.Contains(rooms[i].MergeWithGridPointManagers[j]))
					{
						flag = false;
						break;
					}
				}
				if (flag)
				{
					list2.Add(rooms[i]);
				}
			}
		}
		list.AddRange(list2);
		foreach (GridPointManager item in list)
		{
			MapRoomEntry value = CreateMapRoomEntryForGridPoint(item);
			dictionary.Add(item.BiomeControllerIndex, value);
		}
		if (m_mapRoomEntryDict.ContainsKey(biomeType))
		{
			m_mapRoomEntryDict[biomeType] = dictionary;
		}
		else
		{
			m_mapRoomEntryDict.Add(biomeType, dictionary);
		}
		if (m_biomeMapBoundsTable.ContainsKey(biomeType))
		{
			return;
		}
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MinValue;
		float num4 = float.MaxValue;
		foreach (KeyValuePair<int, MapRoomEntry> item2 in m_mapRoomEntryDict[biomeType])
		{
			MapRoomEntry value2 = item2.Value;
			if (!value2.IsTunnelExit)
			{
				if (value2.AbsBounds.xMin < num)
				{
					num = value2.AbsBounds.xMin;
				}
				if (value2.AbsBounds.xMax > num2)
				{
					num2 = value2.AbsBounds.xMax;
				}
				if (value2.AbsBounds.yMin < num4)
				{
					num4 = value2.AbsBounds.yMin;
				}
				if (value2.AbsBounds.yMax > num3)
				{
					num3 = value2.AbsBounds.yMax;
				}
			}
		}
		Rect value3 = new Rect(num, num4, num2 - num, num3 - num4);
		m_biomeMapBoundsTable.Add(biomeType, value3);
	}

	public static MapRoomEntry CreateMapRoomEntryForGridPoint(GridPointManager roomGridPoint)
	{
		MapRoomEntry mapRoomEntry = UnityEngine.Object.Instantiate(m_instance.m_mapEntryPrefab, m_mapObj.transform, worldPositionStays: true);
		mapRoomEntry.gameObject.name = roomGridPoint.RoomMetaData.ID.ToString() + " - Map Room Entry";
		Vector2 worldPositionFromRoomCoordinates = GridController.GetWorldPositionFromRoomCoordinates(GridController.GetRoomCoordinatesFromGridCoordinates(roomGridPoint.GridCoordinates), roomGridPoint.Size);
		mapRoomEntry.gameObject.transform.position = GetMapPositionFromWorld(worldPositionFromRoomCoordinates);
		mapRoomEntry.CreateRoomTerrainForGridPoint(roomGridPoint);
		mapRoomEntry.CreateDoorIconsForGridPoint(m_instance.m_mapHorizontalDoor, m_instance.m_mapVerticalDoor, roomGridPoint);
		mapRoomEntry.CreateEnemyIconsForGridPoint(m_instance.m_mapEnemyIcon, m_instance.m_mapEnemyKilledIcon, roomGridPoint);
		mapRoomEntry.CreateChestIconsForGridPoint(m_instance.m_mapChestOpenIconPrefab, m_instance.m_mapChestClosedIconPrefab, roomGridPoint);
		CreateTeleporterIconsForGridPoint(roomGridPoint, mapRoomEntry);
		mapRoomEntry.CreateSpecialRoomIconsForGridPoint(roomGridPoint, m_instance.m_specialIndicatorIconPrefab);
		mapRoomEntry.IsTunnelExit = roomGridPoint.IsTunnelDestination;
		mapRoomEntry.IsMergeRoomEntry = roomGridPoint.MergeWithGridPointManagers.Count > 0;
		mapRoomEntry.BiomeControllerIndex = roomGridPoint.BiomeControllerIndex;
		mapRoomEntry.gameObject.SetActive(value: false);
		return mapRoomEntry;
	}

	private void LinkTunnelBonusRooms(object sender, EventArgs args)
	{
		if (!(args is BiomeEventArgs biomeEventArgs) || !m_mapEntriesThatNeedLinking.TryGetValue(biomeEventArgs.Biome, out var value))
		{
			return;
		}
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeEventArgs.Biome) : OnPlayManager.BiomeController);
		foreach (MapRoomEntry item in value)
		{
			if (item.BiomeControllerIndex < 0 || item.BiomeControllerIndex > biomeController.Rooms.Count)
			{
				Debug.Log("<color=red>Something went wrong with populating the biomeControllerIndices for the MapRoomEntry.</color>");
				continue;
			}
			BaseRoom room = biomeController.GetRoom(item.BiomeControllerIndex);
			m_mapEntryTunnelSpawners.Clear();
			room.GetComponentsInChildren(includeInactive: true, m_mapEntryTunnelSpawners);
			for (int i = 0; i < m_mapEntryTunnelSpawners.Count; i++)
			{
				TunnelSpawnController tunnelSpawnController = m_mapEntryTunnelSpawners[i];
				bool flag = tunnelSpawnController.RoomMetaData == null;
				bool flag2 = !flag && tunnelSpawnController.RoomMetaData.SpecialRoomType != 0 && tunnelSpawnController.RoomMetaData.SpecialRoomType != SpecialRoomType.BossEntrance && tunnelSpawnController.LeadsToRoomType != RoomType.Boss;
				if (!tunnelSpawnController.ShouldSpawn || (!flag && !flag2))
				{
					m_mapEntryTunnelSpawners.RemoveAt(i);
					i--;
				}
			}
			int count = m_mapEntryTunnelSpawners.Count;
			int num = item.RoomCompleteBiomeControllerIndexOverrides.Length;
			if (count == num)
			{
				for (int j = 0; j < count; j++)
				{
					Tunnel tunnel = m_mapEntryTunnelSpawners[j].Tunnel;
					if (!tunnel)
					{
						continue;
					}
					BaseRoom room2 = tunnel.Destination.Room;
					if ((bool)room2.GetComponent<BaseSpecialRoomController>())
					{
						item.RoomCompleteBiomeControllerIndexOverrides[j] = room2.BiomeControllerIndex;
						continue;
					}
					foreach (BaseRoom connectedRoom in room2.ConnectedRooms)
					{
						if ((bool)connectedRoom.GetComponent<BaseSpecialRoomController>())
						{
							item.RoomCompleteBiomeControllerIndexOverrides[j] = connectedRoom.BiomeControllerIndex;
						}
					}
				}
			}
			else
			{
				Debug.Log("<color=red>WARNING: Cannot link tunnel room map entries in Biome: " + biomeEventArgs.Biome.ToString() + ". TunnelSpawner count (" + count + ") and MapRoomEntry SpecialIcon count (" + num + ") mismatch. RoomName: " + room.name + "</color>");
			}
		}
		value.Clear();
		m_mapEntriesThatNeedLinking.Remove(biomeEventArgs.Biome);
	}

	public static void AddMapEntryNeedsLinking(BiomeType biome, MapRoomEntry entry)
	{
		if (m_instance.m_mapEntriesThatNeedLinking.TryGetValue(biome, out var value))
		{
			if (!value.Contains(entry))
			{
				value.Add(entry);
			}
		}
		else
		{
			m_instance.m_mapEntriesThatNeedLinking.Add(biome, new List<MapRoomEntry>());
			m_instance.m_mapEntriesThatNeedLinking[biome].Add(entry);
		}
	}

	private static void CreateTeleporterIconsForGridPoint(GridPointManager room, MapRoomEntry mapRoomEntry)
	{
		if (mapRoomEntry.CreateTeleporterIconForGridPoint(m_instance.m_mapTeleporterIcon, room))
		{
			GridPointManagersContainingTeleporters.Add(room);
		}
	}

	private static MapRoomEntry CreateMapRoomEntryPostRoomInstantiation(BiomeType biome, int biomeControllerIndex)
	{
		MapRoomEntry mapRoomEntry = null;
		BiomeController biomeController = WorldBuilder.GetBiomeController(biome);
		if (!biomeController)
		{
			biomeController = OnPlayManager.BiomeController;
		}
		if ((bool)biomeController)
		{
			mapRoomEntry = CreateMapRoomEntryForGridPoint((biomeController.GetRoom(biomeControllerIndex) as Room).GridPointManager);
			Dictionary<int, MapRoomEntry> dictionary = m_instance.m_mapRoomEntryDict[biome];
			if (dictionary != null)
			{
				dictionary.Add(biomeControllerIndex, mapRoomEntry);
			}
			else
			{
				Debug.LogFormat("<color=red>| MapController | Failed to find entry for <b>{0}</b> Biome in Map Room Entry Dictionary, but should have.</color>", biome);
			}
		}
		return mapRoomEntry;
	}

	public static void DisplayAllRooms()
	{
		SetAllBiomeVisibility(isAllVisible: true, updateWasVisitedState: true, retainVisitedRoomData: false);
	}

	public static Vector3 GetMapPositionFromWorld(Vector3 position, bool applyIconYOffset = false)
	{
		if (applyIconYOffset)
		{
			position.y += 2f;
		}
		Vector3 scaledPoint = GetScaledPoint(position);
		scaledPoint.x += MAP_STARTING_POSITION.x;
		scaledPoint.y += MAP_STARTING_POSITION.y;
		return scaledPoint;
	}

	private void FixedUpdate()
	{
		if (m_isCameraFollowOn && PlayerManager.IsInstantiated)
		{
			Vector3 mapPositionFromWorld = GetMapPositionFromWorld(PlayerManager.GetPlayerController().Midpoint, applyIconYOffset: true);
			mapPositionFromWorld.z = 0.9f;
			m_playerIcon.gameObject.transform.position = mapPositionFromWorld;
			CentreCameraAroundPlayerIcon();
		}
	}

	private void LateUpdate()
	{
		if ((bool)m_teleporterLine && m_teleporterLine.gameObject.activeInHierarchy)
		{
			float x = CDGHelper.DistanceBetweenPts(PlayerIconPosition, m_miniMapCamera.transform.position);
			Vector2 size = m_teleporterLine.size;
			size.x = x;
			m_teleporterLine.size = size;
			Vector3 localEulerAngles = m_teleporterLine.gameObject.transform.localEulerAngles;
			float z = CDGHelper.AngleBetweenPts(PlayerIconPosition, m_miniMapCamera.transform.position);
			localEulerAngles.z = z;
			m_teleporterLine.gameObject.transform.localEulerAngles = localEulerAngles;
			m_teleporterLine.gameObject.transform.position = PlayerIconPosition;
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		BaseRoom room = (args as RoomViaDoorEventArgs).Room;
		BiomeType biomeType = room.BiomeType;
		MapRoomEntry mapRoomEntry = GetMapRoomEntry(biomeType, room.BiomeControllerIndex);
		if (mapRoomEntry == null)
		{
			mapRoomEntry = CreateMapRoomEntryPostRoomInstantiation(biomeType, room.BiomeControllerIndex);
		}
		if (!CutsceneManager.IsCutsceneActive)
		{
			SetRoomVisible(biomeType, room.BiomeControllerIndex, isVisible: true);
			if (mapRoomEntry.IsTunnelExit)
			{
				for (int i = 0; i < room.ConnectedRooms.Count; i++)
				{
					int biomeControllerIndex = room.ConnectedRooms[i].BiomeControllerIndex;
					MapRoomEntry mapRoomEntry2 = GetMapRoomEntry(biomeType, biomeControllerIndex);
					if ((bool)mapRoomEntry2 && mapRoomEntry2.WasVisited)
					{
						SetRoomVisible(biomeType, biomeControllerIndex, isVisible: true);
					}
				}
			}
		}
		if (TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			if (m_playerIcon.activeSelf)
			{
				m_playerIcon.SetActive(value: false);
			}
		}
		else if (!m_playerIcon.activeSelf)
		{
			m_playerIcon.SetActive(value: true);
		}
	}

	private void OnBiomeEnter(object sender, EventArgs args)
	{
		if ((bool)CutsceneManager.ExitRoomTunnel)
		{
			return;
		}
		BiomeType groupedBiomeType = BiomeType_RL.GetGroupedBiomeType((args as BiomeEventArgs).Biome);
		if (!BiomeType_RL.IsValidBiome(groupedBiomeType))
		{
			return;
		}
		if (TraitManager.IsTraitActive(TraitType.MapReveal))
		{
			ShowAllRoomsInBiome(groupedBiomeType);
		}
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(groupedBiomeType) : OnPlayManager.BiomeController);
		if (!biomeController)
		{
			return;
		}
		foreach (KeyValuePair<int, MapRoomEntry> item in m_mapRoomEntryDict[groupedBiomeType])
		{
			BaseRoom room = biomeController.GetRoom(item.Key);
			if (room.RoomType != RoomType.Fairy)
			{
				continue;
			}
			FairyRoomController component = room.gameObject.GetComponent<FairyRoomController>();
			if (!component || component.FairyRoomRuleEntries == null)
			{
				continue;
			}
			foreach (FairyRoomRuleEntry fairyRoomRuleEntry in component.FairyRoomRuleEntries)
			{
				if (fairyRoomRuleEntry.FairyRuleID == FairyRuleID.HiddenChest)
				{
					item.Value.SetSpecialIconAlpha(0f);
					break;
				}
			}
		}
	}

	public static void ShowAllRoomsInBiome(BiomeType biome)
	{
		SetAllBiomeVisibility(isAllVisible: false, updateWasVisitedState: false, retainVisitedRoomData: true);
		if (!m_instance.m_mapRoomEntryDict.ContainsKey(biome))
		{
			return;
		}
		foreach (KeyValuePair<int, MapRoomEntry> item in m_instance.m_mapRoomEntryDict[biome])
		{
			MapRoomEntry value = item.Value;
			value.gameObject.SetActive(value: true);
			RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(biome, item.Key);
			if (roomSaveData.IsNativeNull() || !roomSaveData.RoomVisited)
			{
				value.ToggleTerrainVisibility(visible: true);
				value.ToggleAllIconVisibility(visible: false);
				value.ToggleIconVisibility(MapIconType.Door, -1, visible: true);
				value.ToggleIconVisibility(MapIconType.SpecialRoom, -1, visible: true);
				value.ToggleIconVisibility(MapIconType.ChestClosed, -1, visible: true);
				value.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
				value.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: true);
				if (value.RoomType == RoomType.Fairy)
				{
					value.ToggleIconVisibility(MapIconType.ChestClosed, -1, visible: false);
					value.ToggleIconVisibility(MapIconType.ChestOpened, -1, visible: false);
				}
			}
		}
	}

	public static void ShowAllChestsInBiome(BiomeType biome)
	{
		if (!m_instance.m_mapRoomEntryDict.ContainsKey(biome))
		{
			return;
		}
		foreach (KeyValuePair<int, MapRoomEntry> item in m_instance.m_mapRoomEntryDict[biome])
		{
			MapRoomEntry value = item.Value;
			if (!value.WasVisited && value.RoomType != RoomType.Fairy)
			{
				value.gameObject.SetActive(value: true);
				value.ToggleTerrainVisibility(visible: false);
				value.ToggleAllIconVisibility(visible: false);
				value.ToggleIconVisibility(MapIconType.ChestClosed, -1, visible: true);
				value.ToggleIconVisibility(MapIconType.ChestOpened, -1, visible: false);
			}
		}
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		if (!IsInitialized)
		{
			return;
		}
		RoomViaDoorEventArgs roomViaDoorEventArgs = args as RoomViaDoorEventArgs;
		BaseRoom room = roomViaDoorEventArgs.Room;
		BiomeType biomeType = room.BiomeType;
		MapRoomEntry mapRoomEntry = GetMapRoomEntry(biomeType, room.BiomeControllerIndex);
		if (!(mapRoomEntry != null) || !mapRoomEntry.IsTunnelExit || !(roomViaDoorEventArgs.ViaDoor == null))
		{
			return;
		}
		foreach (KeyValuePair<int, MapRoomEntry> item in m_mapRoomEntryDict[biomeType])
		{
			int key = item.Key;
			mapRoomEntry = item.Value;
			if (mapRoomEntry.IsTunnelExit)
			{
				SetRoomVisible(biomeType, key, isVisible: false);
			}
		}
	}

	private void OnUpdateCurrentRoomIcons(object sender, EventArgs args)
	{
		if (PlayerManager.IsInstantiated)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			SetRoomVisible(currentPlayerRoom.BiomeType, currentPlayerRoom.BiomeControllerIndex, isVisible: true);
		}
	}

	private void OnActiveSceneChanged(Scene current, Scene next)
	{
		GridPointManagersContainingTeleporters.Clear();
	}

	public static void Reset()
	{
		Camera.orthographicSize = 2f;
		MapWindowCamera.gameObject.SetActive(value: false);
		DeathMapCamera.gameObject.SetActive(value: false);
		m_isCameraFollowOn = false;
		m_previousPlayerIconPosition = Vector3.zero;
		UnityEngine.Object.Destroy(m_mapObj);
		foreach (KeyValuePair<BiomeType, Dictionary<int, MapRoomEntry>> item in m_instance.m_mapRoomEntryDict)
		{
			foreach (KeyValuePair<int, MapRoomEntry> item2 in item.Value)
			{
				UnityEngine.Object.Destroy(item2.Value);
			}
			item.Value.Clear();
		}
		m_instance.m_mapRoomEntryDict.Clear();
		m_instance.m_biomeMapBoundsTable.Clear();
		m_isInitialized = false;
		m_enemyKilledLocation = null;
	}

	private void ResetCameraPosition()
	{
		SetMapCameraPosition(MAP_STARTING_POSITION);
	}

	public static void SetCameraFollowIsOn(bool isOn)
	{
		m_isCameraFollowOn = isOn;
		if (isOn)
		{
			m_instance.m_cameraDestination = new Vector3(PlayerIconPosition.x, PlayerIconPosition.y, Camera.transform.position.z);
		}
	}

	public static void SetAllBiomeVisibility(bool isAllVisible, bool updateWasVisitedState, bool retainVisitedRoomData)
	{
		if (m_instance.m_mapRoomEntryDict == null)
		{
			return;
		}
		if (!isAllVisible && (bool)m_playerIcon)
		{
			m_playerIcon.SetActive(value: false);
		}
		foreach (KeyValuePair<BiomeType, Dictionary<int, MapRoomEntry>> item in m_instance.m_mapRoomEntryDict)
		{
			foreach (KeyValuePair<int, MapRoomEntry> item2 in item.Value)
			{
				MapRoomEntry value = item2.Value;
				if (value.IsTunnelExit)
				{
					continue;
				}
				RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(item.Key, item2.Key);
				if (retainVisitedRoomData && !roomSaveData.IsNativeNull() && roomSaveData.RoomVisited)
				{
					value.gameObject.SetActive(isAllVisible);
					continue;
				}
				value.gameObject.SetActive(isAllVisible);
				value.ToggleTerrainVisibility(isAllVisible);
				value.ToggleAllIconVisibility(visible: false);
				if (isAllVisible)
				{
					value.ToggleIconVisibility(MapIconType.Door, -1, visible: true);
					value.ToggleIconVisibility(MapIconType.SpecialRoom, -1, visible: true);
					bool flag = roomSaveData.BiomeType == BiomeType.Tower && roomSaveData.RoomName.Contains("Exterior");
					if (value.RoomType != RoomType.Fairy || flag)
					{
						value.ToggleIconVisibility(MapIconType.ChestClosed, -1, visible: true);
					}
					value.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: true);
					value.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: true);
				}
				if (updateWasVisitedState)
				{
					value.WasVisited = isAllVisible;
				}
			}
		}
	}

	public static void SetVisitedRoomsVisibility(bool isVisible)
	{
		if (m_instance.m_mapRoomEntryDict == null)
		{
			return;
		}
		m_playerIcon.SetActive(value: true);
		foreach (KeyValuePair<BiomeType, Dictionary<int, MapRoomEntry>> item in m_instance.m_mapRoomEntryDict)
		{
			foreach (KeyValuePair<int, MapRoomEntry> item2 in item.Value)
			{
				if (!item2.Value.IsTunnelExit && item2.Value.WasVisited)
				{
					item2.Value.ToggleTerrainVisibility(visible: true);
					item2.Value.ToggleIconVisibility(MapIconType.Door, -1, visible: true);
					item2.Value.gameObject.SetActive(isVisible);
				}
			}
		}
	}

	public static void SetCaveWhitePipVisibility(bool visible)
	{
		if (TraitManager.IsTraitActive(TraitType.MapReveal) || !m_instance.m_mapRoomEntryDict.TryGetValue(BiomeType.Cave, out var value))
		{
			return;
		}
		foreach (KeyValuePair<int, MapRoomEntry> item in value)
		{
			bool flag = false;
			if (item.Value.SpecialRoomType == SpecialRoomType.WhitePip || item.Value.SpecialRoomType == SpecialRoomType.WhitePipUnique || item.Value.SpecialRoomType == SpecialRoomType.BossEntrance || item.Value.SpecialRoomType == SpecialRoomType.SubbossEntrance)
			{
				flag = true;
			}
			if (!flag)
			{
				continue;
			}
			if (visible)
			{
				bool flag2 = true;
				if (!item.Value.WasVisited)
				{
					item.Value.ToggleTerrainVisibility(visible: false);
					item.Value.ToggleIconVisibility(MapIconType.Door, -1, visible: false);
					item.Value.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: false);
					item.Value.ToggleIconVisibility(MapIconType.SpecialRoom, -1, visible: false);
				}
				else
				{
					flag2 = false;
				}
				if (flag2)
				{
					item.Value.gameObject.SetActive(value: true);
					item.Value.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: true);
				}
			}
			else
			{
				if (!item.Value.WasVisited)
				{
					item.Value.gameObject.SetActive(value: false);
				}
				item.Value.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
			}
		}
	}

	public static void SetIconVisibility(MapIconType iconType, BiomeType biome, int biomeControllerIndex, bool visible, int index = 0)
	{
		biome = BiomeType_RL.GetGroupedBiomeType(biome);
		if (m_instance.m_mapRoomEntryDict != null && m_instance.m_mapRoomEntryDict.ContainsKey(biome))
		{
			MapRoomEntry mapRoomEntry = GetMapRoomEntry(biome, biomeControllerIndex);
			if (mapRoomEntry != null)
			{
				mapRoomEntry.ToggleIconVisibility(iconType, index, visible);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Map Room Dictionary is null or Key ({1}) wasn't present in Dictionary</color>", m_instance, biome);
		}
	}

	public static void SetRoomVisibleUsingSaveData(BiomeType biome, int biomeControllerIndex, GridPointManager room)
	{
		RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(biome, biomeControllerIndex);
		if (roomSaveData == null || roomSaveData.IsEmpty || !roomSaveData.RoomVisited)
		{
			return;
		}
		biome = BiomeType_RL.GetGroupedBiomeType(biome);
		if (m_instance.m_mapRoomEntryDict == null || !m_instance.m_mapRoomEntryDict.ContainsKey(biome) || m_instance.m_mapRoomEntryDict[biome].Count <= 0 || biomeControllerIndex < 0 || biomeControllerIndex >= m_instance.m_mapRoomEntryDict[biome].Count)
		{
			return;
		}
		MapRoomEntry mapRoomEntry = GetMapRoomEntry(biome, biomeControllerIndex);
		if (!mapRoomEntry)
		{
			return;
		}
		mapRoomEntry.gameObject.SetActive(roomSaveData.RoomVisited);
		mapRoomEntry.ToggleTerrainVisibility(roomSaveData.RoomVisited);
		mapRoomEntry.ToggleIconVisibility(MapIconType.Door, -1, roomSaveData.RoomVisited);
		mapRoomEntry.WasVisited = roomSaveData.RoomVisited;
		if (mapRoomEntry.WasVisited && mapRoomEntry.RoomType != RoomType.Fairy && mapRoomEntry.RoomType != RoomType.Bonus)
		{
			for (int i = 0; i < roomSaveData.ChestStates.Length; i++)
			{
				if (!roomSaveData.ChestStates[i].IsSpawned)
				{
					mapRoomEntry.ToggleIconVisibility(MapIconType.ChestClosed, i, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.ChestOpened, i, visible: false);
				}
				else
				{
					bool isStateActive = roomSaveData.ChestStates[i].IsStateActive;
					mapRoomEntry.ToggleIconVisibility(MapIconType.ChestClosed, i, isStateActive);
					mapRoomEntry.ToggleIconVisibility(MapIconType.ChestOpened, i, !isStateActive);
				}
			}
		}
		else
		{
			mapRoomEntry.ToggleIconVisibility(MapIconType.ChestClosed, -1, visible: false);
			mapRoomEntry.ToggleIconVisibility(MapIconType.ChestOpened, -1, visible: false);
		}
		mapRoomEntry.ToggleIconVisibility(MapIconType.Teleporter, -1, !mapRoomEntry.HasSpecialRoomIcon);
		if (mapRoomEntry.HasSpecialRoomIcon)
		{
			if (roomSaveData.RoomCompleteBiomeControllerIndexOverrides == null || roomSaveData.RoomCompleteBiomeControllerIndexOverrides.Length == 0)
			{
				bool isRoomComplete = roomSaveData.IsRoomComplete;
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoom, 0, !isRoomComplete);
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, 0, isRoomComplete);
			}
			else
			{
				for (int j = 0; j < roomSaveData.RoomCompleteBiomeControllerIndexOverrides.Length; j++)
				{
					RoomSaveData roomSaveData2 = SaveManager.StageSaveData.GetRoomSaveData(biome, roomSaveData.RoomCompleteBiomeControllerIndexOverrides[j]);
					if (!roomSaveData2.IsNativeNull())
					{
						bool isRoomComplete2 = roomSaveData2.IsRoomComplete;
						mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoom, j, !isRoomComplete2);
						mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, j, isRoomComplete2);
					}
				}
			}
		}
		if (mapRoomEntry.HasSpecialRoomIcon || mapRoomEntry.HasTeleporterIcon)
		{
			mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
		}
	}

	public static void SetRoomVisible(BiomeType biome, int roomIndex, bool isVisible, bool showReenactment = false)
	{
		biome = BiomeType_RL.GetGroupedBiomeType(biome);
		MapRoomEntry mapRoomEntry;
		BaseRoom baseRoom;
		if (m_instance.m_mapRoomEntryDict != null && m_instance.m_mapRoomEntryDict.ContainsKey(biome))
		{
			mapRoomEntry = GetMapRoomEntry(biome, roomIndex);
			if (!mapRoomEntry)
			{
				BaseRoom room = WorldBuilder.GetBiomeController(biome).GetRoom(roomIndex);
				GridPointManager gridPointManager = null;
				gridPointManager = ((!(room is Room)) ? (room as MergeRoom).StandaloneGridPointManagers[0] : (room as Room).GridPointManager);
				mapRoomEntry = CreateMapRoomEntryForGridPoint(gridPointManager);
				m_instance.m_mapRoomEntryDict[biome].Add(gridPointManager.BiomeControllerIndex, mapRoomEntry);
				Debug.Log("<color=red>WARNING: Map room entry: " + roomIndex + " in biome: " + biome.ToString() + " could not be found in map controller. Creating a new one while setting room visible, but this really shouldn't happen.</color>");
			}
			if ((bool)mapRoomEntry)
			{
				if (!showReenactment)
				{
					baseRoom = null;
					baseRoom = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biome).GetRoom(roomIndex) : OnPlayManager.BiomeController.GetRoom(roomIndex));
					if (!mapRoomEntry || !baseRoom)
					{
						return;
					}
					mapRoomEntry.gameObject.SetActive(isVisible);
					mapRoomEntry.ToggleTerrainVisibility(isVisible);
					mapRoomEntry.ToggleIconVisibility(MapIconType.Door, -1, isVisible);
					mapRoomEntry.WasVisited = isVisible;
					if (isVisible)
					{
						if (TraitManager.IsTraitActive(TraitType.ShowEnemiesOnMap))
						{
							EnemySpawnController[] enemySpawnControllers = baseRoom.SpawnControllerManager.EnemySpawnControllers;
							for (int i = 0; i < enemySpawnControllers.Length; i++)
							{
								EnemySpawnController enemySpawnController = enemySpawnControllers[i];
								bool flag = enemySpawnController.EnemyInstance == null || (enemySpawnController.EnemyInstance != null && enemySpawnController.EnemyInstance.IsDead);
								if (enemySpawnController.ShouldSpawn && !flag)
								{
									mapRoomEntry.ToggleIconVisibility(MapIconType.Enemy, i, visible: true);
								}
								else
								{
									mapRoomEntry.ToggleIconVisibility(MapIconType.Enemy, i, visible: false);
								}
							}
						}
						else
						{
							mapRoomEntry.ToggleIconVisibility(MapIconType.Enemy, -1, visible: false);
							mapRoomEntry.ToggleIconVisibility(MapIconType.EnemyKilled, -1, visible: false);
						}
						if (baseRoom.RoomType != RoomType.Fairy && baseRoom.RoomType != RoomType.Bonus)
						{
							ChestSpawnController[] chestSpawnControllers = baseRoom.SpawnControllerManager.ChestSpawnControllers;
							for (int j = 0; j < chestSpawnControllers.Length; j++)
							{
								ChestSpawnController chestSpawnController = chestSpawnControllers[j];
								bool visible = false;
								bool visible2 = false;
								if (chestSpawnController.ShouldSpawn)
								{
									if (chestSpawnController.ChestInstance.IsOpen)
									{
										visible = true;
									}
									else
									{
										visible2 = true;
									}
								}
								mapRoomEntry.ToggleIconVisibility(MapIconType.ChestOpened, j, visible);
								mapRoomEntry.ToggleIconVisibility(MapIconType.ChestClosed, j, visible2);
							}
						}
						else
						{
							mapRoomEntry.ToggleIconVisibility(MapIconType.ChestOpened, -1, visible: false);
							mapRoomEntry.ToggleIconVisibility(MapIconType.ChestClosed, -1, visible: false);
						}
						RoomType roomType = mapRoomEntry.RoomType;
						if (roomType <= RoomType.BossEntrance)
						{
							if (roomType != 0)
							{
								if (roomType == RoomType.Fairy || roomType == RoomType.BossEntrance)
								{
									goto IL_02cb;
								}
							}
							else if (mapRoomEntry.IsMergeRoomEntry)
							{
								goto IL_02cb;
							}
						}
						else if (roomType == RoomType.Boss || roomType == RoomType.Bonus || roomType == RoomType.Mandatory)
						{
							goto IL_02cb;
						}
						goto IL_0432;
					}
					mapRoomEntry.ToggleIconVisibility(MapIconType.Enemy, -1, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.EnemyKilled, -1, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.ChestClosed, -1, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.ChestOpened, -1, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoom, -1, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.Teleporter, -1, visible: false);
					return;
				}
				if ((bool)mapRoomEntry)
				{
					mapRoomEntry.gameObject.SetActive(isVisible);
					mapRoomEntry.ToggleTerrainVisibility(isVisible);
					mapRoomEntry.ToggleIconVisibility(MapIconType.Door, -1, isVisible);
				}
				return;
			}
			if (!GameUtility.IsInLevelEditor)
			{
				Debug.LogFormat("<color=red>| {0} | {1} Biome's Map Room Dictionary doesn't contain an entry for Biome Controller Index ({2})</color>", m_instance, biome, roomIndex);
			}
			return;
		}
		if (!GameUtility.IsInLevelEditor)
		{
			Debug.LogFormat("<color=red>| {0} | Map Room Dictionary is null or Key ({1}) wasn't present in Dictionary</color>", m_instance, biome);
		}
		return;
		IL_0432:
		mapRoomEntry.ToggleIconVisibility(MapIconType.Teleporter, -1, !mapRoomEntry.HasSpecialRoomIcon);
		if (mapRoomEntry.HasSpecialRoomIcon || mapRoomEntry.HasTeleporterIcon)
		{
			mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
		}
		return;
		IL_02cb:
		if (mapRoomEntry.RoomCompleteBiomeControllerIndexOverrides == null || mapRoomEntry.RoomCompleteBiomeControllerIndexOverrides.Length == 0)
		{
			BaseSpecialRoomController componentInChildren = baseRoom.gameObject.GetComponentInChildren<BaseSpecialRoomController>();
			if (!componentInChildren || !componentInChildren.IsRoomComplete)
			{
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoom, -1, visible: true);
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
			}
			else
			{
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoom, -1, visible: false);
				mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: true);
			}
		}
		else
		{
			RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(biome, roomIndex);
			if (!roomSaveData.IsNativeNull() && roomSaveData.RoomCompleteBiomeControllerIndexOverrides == null)
			{
				roomSaveData.RoomCompleteBiomeControllerIndexOverrides = mapRoomEntry.RoomCompleteBiomeControllerIndexOverrides;
			}
			for (int k = 0; k < mapRoomEntry.RoomCompleteBiomeControllerIndexOverrides.Length; k++)
			{
				bool flag2 = false;
				int biomeControllerIndex = mapRoomEntry.RoomCompleteBiomeControllerIndexOverrides[k];
				RoomSaveData roomSaveData2 = SaveManager.StageSaveData.GetRoomSaveData(biome, biomeControllerIndex);
				if (!roomSaveData2.IsNativeNull())
				{
					flag2 = roomSaveData2.IsRoomComplete;
				}
				else
				{
					BaseRoom baseRoom2 = null;
					baseRoom2 = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biome).GetRoom(biomeControllerIndex) : OnPlayManager.BiomeController.GetRoom(biomeControllerIndex));
					BaseSpecialRoomController componentInChildren2 = baseRoom2.gameObject.GetComponentInChildren<BaseSpecialRoomController>();
					flag2 = (bool)componentInChildren2 && componentInChildren2.IsRoomComplete;
				}
				if (!flag2)
				{
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoom, k, visible: true);
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, k, visible: false);
				}
				else
				{
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoom, k, visible: false);
					mapRoomEntry.ToggleIconVisibility(MapIconType.SpecialRoomUsed, k, visible: true);
				}
			}
		}
		goto IL_0432;
	}

	public static void SetPlayerIconInRoom(BiomeType biome, int biomeControllerIndex, Vector2 worldPosition = default(Vector2))
	{
		if (!m_playerIcon.activeInHierarchy)
		{
			m_playerIcon.SetActive(value: true);
		}
		MapRoomEntry mapRoomEntry = GetMapRoomEntry(biome, biomeControllerIndex);
		if (mapRoomEntry != null)
		{
			Vector3 position = ((worldPosition == default(Vector2)) ? mapRoomEntry.AbsBounds.center : new Vector2(GetMapPositionFromWorld(worldPosition, applyIconYOffset: true).x, mapRoomEntry.AbsBounds.center.y));
			position.z = 0.9f;
			m_playerIcon.transform.position = position;
		}
	}

	public static void SetMapCameraPosition(Vector3 position)
	{
		m_instance.m_miniMapCamera.transform.position = new Vector3(position.x, position.y, m_instance.m_miniMapCamera.transform.position.z);
	}

	public static void TweenCameraToPlayer(float duration)
	{
		TweenCameraToPosition((Vector2)m_instance.m_mapPlayerIconPrefab.transform.position, duration);
	}

	public static void TweenCameraToPosition(Vector3 position, float duration)
	{
		if (m_instance.m_tweenCameraCoroutine != null)
		{
			m_instance.StopCoroutine(m_instance.m_tweenCameraCoroutine);
		}
		if (duration > 0f)
		{
			m_instance.m_tweenCameraCoroutine = m_instance.StartCoroutine(m_instance.TweenCameraCoroutine(position, duration));
		}
		else
		{
			SetMapCameraPosition((Vector2)position);
		}
	}

	private IEnumerator TweenCameraCoroutine(Vector3 position, float duration)
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return TweenManager.TweenTo_UnscaledTime(m_instance.m_miniMapCamera.transform, duration, Ease.Quad.EaseInOut, "position.x", position.x, "position.y", position.y, "position.z", 0).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	public static void UpdatePlayerIconPosition()
	{
		if (PlayerManager.IsInstantiated)
		{
			Vector3 mapPositionFromWorld = GetMapPositionFromWorld(PlayerManager.GetPlayerController().Midpoint, applyIconYOffset: true);
			mapPositionFromWorld.z = 0.9f;
			m_playerIcon.transform.position = mapPositionFromWorld;
		}
	}

	public static IEnumerator TeleportPlayerToRoom_Coroutine(BiomeType biomeType, GridPointManager gridPointManager, bool runExitAnim = true)
	{
		PlayerManager.GetCurrentPlayerRoom().PlayerExit(null);
		BiomeType originBiome = PlayerManager.GetCurrentPlayerRoom().BiomeType;
		if (originBiome != biomeType)
		{
			yield return BiomeTransitionController.BiomeTransitionCoroutine(originBiome, biomeType);
			BiomeTransitionController.DestroyOldCastleTransitionRoom(biomeType);
			BiomeTransitionController.DestroyBiomeRoomInstances(originBiome);
		}
		BiomeController biomeController = null;
		if (WorldBuilder.IsInstantiated)
		{
			biomeController = WorldBuilder.GetBiomeController(biomeType);
		}
		else if (GameUtility.IsInLevelEditor)
		{
			biomeController = OnPlayManager.BiomeController;
		}
		if (!(biomeController != null))
		{
			yield break;
		}
		BaseRoom room = biomeController.GetRoom(gridPointManager.BiomeControllerIndex);
		GlobalTeleporterController globalTeleporterController = ((!(room is Room)) ? (room as MergeRoom).GlobalTeleporters[gridPointManager] : (room as Room).GlobalTeleporter);
		if ((bool)globalTeleporterController)
		{
			Vector3 position = globalTeleporterController.transform.position;
			Vector3 localPosition = room.gameObject.transform.InverseTransformPoint(position);
			room.PlacePlayerInRoom(localPosition);
			if (runExitAnim)
			{
				globalTeleporterController.OnExitTeleportPlayer();
			}
		}
	}

	public static void SetTeleporterLineVisible(bool visible)
	{
		if (m_teleporterLine.gameObject.activeSelf != visible)
		{
			m_teleporterLine.gameObject.SetActive(visible);
		}
	}

	public static void SetPlayerIconVisible(bool visible)
	{
		m_playerIcon.gameObject.SetActive(visible);
	}
}
