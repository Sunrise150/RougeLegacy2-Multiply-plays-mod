using System.Collections.Generic;
using UnityEngine;

public class MapRoomEntry : MonoBehaviour
{
	private const float ROOM_ABS_SHRINK_AMOUNT = 0.06f;

	[SerializeField]
	private GameObject m_enemyIconsParentGO;

	[SerializeField]
	private GameObject m_teleporterIconParentGO;

	[SerializeField]
	private GameObject m_chestIconsParentGO;

	[SerializeField]
	private GameObject m_doorsParentGO;

	[SerializeField]
	private GameObject m_specialRoomIconsParentGO;

	[SerializeField]
	private GameObject m_specialIndicatorIconParentGO;

	[SerializeField]
	private GameObject m_terrainGO;

	private Ferr2DT_PathTerrain m_terrain;

	private Rect m_bounds;

	private Rect m_absBounds;

	private GameObject[] m_chestOpenIconList;

	private GameObject[] m_chestClosedIconList;

	private GameObject[] m_enemyIconList;

	private GameObject[] m_enemyKilledIconList;

	private GameObject[] m_specialRoomIconList;

	private GameObject[] m_specialRoomUsedIconList;

	private GameObject m_specialIndicatorIcon;

	private int m_chestSpawnedFlag;

	private static List<Vector2> m_meshPointsHelper = new List<Vector2>(12);

	private static List<Vector2> m_pointOffsetHelper = new List<Vector2>(12);

	private static List<GameObject> m_specialIconsListHelper_STATIC = new List<GameObject>();

	private static List<GameObject> m_specialUsedIconsListHelper_STATIC = new List<GameObject>();

	private static SortedSet<GridPointManager> m_gridPointsToCheck_STATIC;

	private static List<GridPointManagerContentEntry> m_gridPointRoomContentList_STATIC = new List<GridPointManagerContentEntry>();

	private static GridPointManager m_lastGridPointChecked;

	public int BiomeControllerIndex { get; set; }

	public int[] RoomCompleteBiomeControllerIndexOverrides { get; private set; }

	public RoomType RoomType { get; set; }

	public bool IsMergeRoomEntry { get; set; }

	public bool HasTeleporter { get; private set; }

	public bool HasSpecialRoomIcon
	{
		get
		{
			if ((bool)m_specialRoomIconsParentGO)
			{
				return m_specialRoomIconsParentGO.transform.childCount > 0;
			}
			return false;
		}
	}

	public bool HasTeleporterIcon
	{
		get
		{
			if ((bool)m_teleporterIconParentGO)
			{
				return m_teleporterIconParentGO.transform.childCount > 0;
			}
			return false;
		}
	}

	public Rect Bounds => m_bounds;

	public Rect AbsBounds => m_absBounds;

	public GlobalTeleporterController Teleporter { get; private set; }

	public GameObject TerrainGO => m_terrainGO;

	public Ferr2DT_PathTerrain Terrain => m_terrain;

	public bool IsTunnelExit { get; set; }

	public bool WasVisited { get; set; }

	public SpecialRoomType SpecialRoomType { get; set; }

	public void ToggleIconVisibility(MapIconType iconType, int index, bool visible)
	{
		switch (iconType)
		{
		case MapIconType.ChestOpened:
			if (index == -1)
			{
				if (m_chestOpenIconList == null)
				{
					break;
				}
				for (int k = 0; k < m_chestOpenIconList.Length; k++)
				{
					if ((m_chestSpawnedFlag & (1 << k)) != 0 || !visible)
					{
						m_chestOpenIconList[k].SetActive(visible);
					}
				}
			}
			else if (((m_chestSpawnedFlag & (1 << index)) != 0 || !visible) && m_chestOpenIconList != null && index >= 0 && index < m_chestOpenIconList.Length)
			{
				m_chestOpenIconList[index].SetActive(visible);
			}
			break;
		case MapIconType.ChestClosed:
			if (index == -1)
			{
				if (m_chestClosedIconList == null)
				{
					break;
				}
				for (int j = 0; j < m_chestClosedIconList.Length; j++)
				{
					if ((m_chestSpawnedFlag & (1 << j)) != 0 || !visible)
					{
						m_chestClosedIconList[j].SetActive(visible);
					}
				}
			}
			else if (((m_chestSpawnedFlag & (1 << index)) != 0 || !visible) && m_chestClosedIconList != null && index >= 0 && index < m_chestClosedIconList.Length)
			{
				m_chestClosedIconList[index].SetActive(visible);
			}
			break;
		case MapIconType.Door:
			m_doorsParentGO.gameObject.SetActive(visible);
			break;
		case MapIconType.Enemy:
			if (index == -1)
			{
				if (m_enemyIconList != null)
				{
					GameObject[] specialRoomUsedIconList = m_enemyIconList;
					for (int i = 0; i < specialRoomUsedIconList.Length; i++)
					{
						specialRoomUsedIconList[i].SetActive(visible);
					}
				}
			}
			else if (m_enemyIconList != null && index >= 0 && index < m_enemyIconList.Length)
			{
				m_enemyIconList[index].SetActive(visible);
			}
			break;
		case MapIconType.EnemyKilled:
			if (index == -1)
			{
				if (m_enemyKilledIconList != null)
				{
					GameObject[] specialRoomUsedIconList = m_enemyKilledIconList;
					for (int i = 0; i < specialRoomUsedIconList.Length; i++)
					{
						specialRoomUsedIconList[i].SetActive(visible);
					}
				}
			}
			else if (m_enemyKilledIconList != null && index >= 0 && index < m_enemyKilledIconList.Length)
			{
				m_enemyKilledIconList[index].SetActive(visible);
			}
			break;
		case MapIconType.Teleporter:
			if ((bool)m_teleporterIconParentGO)
			{
				m_teleporterIconParentGO.gameObject.SetActive(visible);
			}
			break;
		case MapIconType.SpecialRoom:
			if (!m_specialRoomIconsParentGO)
			{
				break;
			}
			if (index == -1)
			{
				if (m_specialRoomIconList != null)
				{
					GameObject[] specialRoomUsedIconList = m_specialRoomIconList;
					for (int i = 0; i < specialRoomUsedIconList.Length; i++)
					{
						specialRoomUsedIconList[i].SetActive(visible);
					}
				}
			}
			else if (m_specialRoomIconList != null && index >= 0 && index < m_specialRoomIconList.Length)
			{
				m_specialRoomIconList[index].SetActive(visible);
			}
			break;
		case MapIconType.SpecialRoomUsed:
			if (!m_specialRoomIconsParentGO)
			{
				break;
			}
			if (index == -1)
			{
				if (m_specialRoomUsedIconList != null)
				{
					GameObject[] specialRoomUsedIconList = m_specialRoomUsedIconList;
					for (int i = 0; i < specialRoomUsedIconList.Length; i++)
					{
						specialRoomUsedIconList[i].SetActive(visible);
					}
				}
			}
			else if (m_specialRoomUsedIconList != null && index >= 0 && index < m_specialRoomUsedIconList.Length)
			{
				m_specialRoomUsedIconList[index].SetActive(visible);
			}
			break;
		case MapIconType.SpecialIndicator:
			if ((bool)m_specialIndicatorIcon)
			{
				m_specialIndicatorIcon.SetActive(visible);
			}
			break;
		}
	}

	public void SetSpecialIconAlpha(float alpha)
	{
		if (m_specialRoomIconList == null)
		{
			return;
		}
		GameObject[] specialRoomIconList = m_specialRoomIconList;
		for (int i = 0; i < specialRoomIconList.Length; i++)
		{
			SpriteRenderer componentInChildren = specialRoomIconList[i].GetComponentInChildren<SpriteRenderer>(includeInactive: true);
			if ((bool)componentInChildren)
			{
				componentInChildren.SetAlpha(alpha);
			}
		}
	}

	public void ToggleAllIconVisibility(bool visible)
	{
		MapIconType[] typeArray = MapIconType_RL.TypeArray;
		foreach (MapIconType iconType in typeArray)
		{
			ToggleIconVisibility(iconType, -1, visible);
		}
	}

	public void ToggleTerrainVisibility(bool visible)
	{
		m_terrainGO.gameObject.SetActive(visible);
	}

	private Vector2 GetWorldAsLocalPoint(Vector2 center, Vector2 point)
	{
		return point - center;
	}

	public void CreateRoomTerrainForRoom(BaseRoom room)
	{
		if (room is Room)
		{
			GridPointManager gridPointManager = (room as Room).GridPointManager;
			if (GameUtility.IsInGame && gridPointManager != null)
			{
				Ferr2DT_PathTerrain ferr2DT_PathTerrain = MapTerrainLibrary.GetMapTerrain(gridPointManager.Size.x, gridPointManager.Size.y);
				bool flag = false;
				if (!ferr2DT_PathTerrain)
				{
					ferr2DT_PathTerrain = MapTerrainLibrary.GetDefaultMapTerrain();
					flag = true;
				}
				CreateMapRoomEntryInstance(ferr2DT_PathTerrain, gridPointManager.Bounds.center);
				if (flag)
				{
					m_terrain.ClearPoints();
					List<Vector2> meshPoints = GetMeshPoints(gridPointManager);
					List<Vector2> pointOffsets = GetPointOffsets(meshPoints);
					for (int i = 0; i < meshPoints.Count; i++)
					{
						m_terrain.AddPoint(meshPoints[i] + pointOffsets[i]);
					}
					m_terrain.BuildMeshOnly();
				}
				SetBounds();
				SetAppearance(room.BiomeType);
			}
			else
			{
				if (!GameUtility.IsInLevelEditor)
				{
					return;
				}
				Ferr2DT_PathTerrain ferr2DT_PathTerrain2 = null;
				if (gridPointManager != null)
				{
					ferr2DT_PathTerrain2 = MapTerrainLibrary.GetMapTerrain(gridPointManager.Size.x, gridPointManager.Size.y);
				}
				bool flag2 = false;
				if (!ferr2DT_PathTerrain2)
				{
					ferr2DT_PathTerrain2 = MapTerrainLibrary.GetDefaultMapTerrain();
					flag2 = true;
				}
				CreateMapRoomEntryInstance(ferr2DT_PathTerrain2, base.transform.position);
				if (flag2)
				{
					m_terrain.ClearPoints();
					List<Vector2> meshPoints2 = GetMeshPoints(room.Collider2D as PolygonCollider2D);
					List<Vector2> pointOffsets2 = GetPointOffsets(meshPoints2);
					for (int j = 0; j < meshPoints2.Count; j++)
					{
						m_terrain.AddPoint(meshPoints2[j] + pointOffsets2[j]);
					}
					m_terrain.BuildMeshOnly();
				}
				m_terrain.transform.localPosition = Vector3.zero;
				SetBounds();
				SetAppearance(room.BiomeType);
			}
		}
		else
		{
			Vector2 center = room.Bounds.center;
			CreateMapRoomEntryInstance(MapTerrainLibrary.GetDefaultMapTerrain(), center);
			m_terrain.ClearPoints();
			List<List<Vector2>> list = new List<List<Vector2>>();
			MergeRoom mergeRoom = room as MergeRoom;
			for (int k = 0; k < mergeRoom.StandaloneGridPointManagers.Length; k++)
			{
				List<Vector2> fourCorners = GetFourCorners(mergeRoom.StandaloneGridPointManagers[k]);
				list.Add(fourCorners);
			}
			List<Vector2> list2 = MergeRoomTools.GetUnion(list)[0];
			List<Vector2> list3 = new List<Vector2>();
			for (int l = 0; l < list2.Count; l++)
			{
				Vector2 worldAsLocalPoint = GetWorldAsLocalPoint(center, MapController.GetScaledPoint(center, list2[l]));
				list3.Add(worldAsLocalPoint);
			}
			List<Vector2> pointOffsets3 = GetPointOffsets(list3);
			for (int m = 0; m < list3.Count; m++)
			{
				m_terrain.AddPoint(list3[m] + pointOffsets3[m]);
			}
			m_terrain.BuildMeshOnly();
			SetBounds();
			SetAppearance(room.BiomeType);
		}
	}

	public void CreateRoomTerrainForGridPoint(GridPointManager room)
	{
		if (room.MergeWithGridPointManagers.Count == 0)
		{
			Ferr2DT_PathTerrain ferr2DT_PathTerrain = MapTerrainLibrary.GetMapTerrain(room.Size.x, room.Size.y);
			bool flag = false;
			if (!ferr2DT_PathTerrain)
			{
				ferr2DT_PathTerrain = MapTerrainLibrary.GetDefaultMapTerrain();
				flag = true;
			}
			CreateMapRoomEntryInstance(ferr2DT_PathTerrain, room.Bounds.center);
			if (flag)
			{
				m_terrain.ClearPoints();
				List<Vector2> meshPoints = GetMeshPoints(room);
				List<Vector2> pointOffsets = GetPointOffsets(meshPoints);
				for (int i = 0; i < meshPoints.Count; i++)
				{
					m_terrain.AddPoint(meshPoints[i] + pointOffsets[i]);
				}
				m_terrain.BuildMeshOnly();
			}
			SetBounds();
			SetAppearance(room.Biome);
			return;
		}
		Vector2 center = room.Bounds.center;
		CreateMapRoomEntryInstance(MapTerrainLibrary.GetDefaultMapTerrain(), center);
		m_terrain.ClearPoints();
		List<List<Vector2>> list = new List<List<Vector2>>();
		List<GridPointManager> list2 = new List<GridPointManager> { room };
		for (int j = 0; j < room.MergeWithGridPointManagers.Count; j++)
		{
			list2.Add(room.MergeWithGridPointManagers[j]);
		}
		for (int k = 0; k < list2.Count; k++)
		{
			List<Vector2> fourCorners = GetFourCorners(list2[k]);
			list.Add(fourCorners);
		}
		List<Vector2> list3 = MergeRoomTools.GetUnion(list)[0];
		List<Vector2> list4 = new List<Vector2>();
		for (int l = 0; l < list3.Count; l++)
		{
			Vector2 worldAsLocalPoint = GetWorldAsLocalPoint(center, MapController.GetScaledPoint(center, list3[l]));
			list4.Add(worldAsLocalPoint);
		}
		List<Vector2> pointOffsets2 = GetPointOffsets(list4);
		for (int m = 0; m < list4.Count; m++)
		{
			m_terrain.AddPoint(list4[m] + pointOffsets2[m]);
		}
		m_terrain.BuildMeshOnly();
		SetBounds();
		SetAppearance(room.Biome);
	}

	private List<Vector2> GetFourCorners(GridPointManager gridPointManager)
	{
		Vector2 item = gridPointManager.Bounds.min;
		Vector2 item2 = new Vector2(gridPointManager.Bounds.max.x, gridPointManager.Bounds.min.y);
		Vector2 item3 = gridPointManager.Bounds.max;
		Vector2 item4 = new Vector2(gridPointManager.Bounds.min.x, gridPointManager.Bounds.max.y);
		return new List<Vector2> { item, item2, item3, item4 };
	}

	private List<Vector2> GetMeshPoints(GridPointManager gridPointManager)
	{
		float x = gridPointManager.Bounds.min.x;
		float y = gridPointManager.Bounds.min.y;
		float num = gridPointManager.Bounds.max.x - gridPointManager.Bounds.min.x;
		float num2 = gridPointManager.Bounds.max.y - gridPointManager.Bounds.min.y;
		Vector2 worldAsLocalPoint = GetWorldAsLocalPoint(gridPointManager.Bounds.center, MapController.GetScaledPoint(gridPointManager.Bounds.center, x, y));
		Vector2 worldAsLocalPoint2 = GetWorldAsLocalPoint(gridPointManager.Bounds.center, MapController.GetScaledPoint(gridPointManager.Bounds.center, x + num, y));
		Vector2 worldAsLocalPoint3 = GetWorldAsLocalPoint(gridPointManager.Bounds.center, MapController.GetScaledPoint(gridPointManager.Bounds.center, x + num, y + num2));
		Vector2 worldAsLocalPoint4 = GetWorldAsLocalPoint(gridPointManager.Bounds.center, MapController.GetScaledPoint(gridPointManager.Bounds.center, x, y + num2));
		m_meshPointsHelper.Clear();
		m_meshPointsHelper.Add(worldAsLocalPoint);
		m_meshPointsHelper.Add(worldAsLocalPoint2);
		m_meshPointsHelper.Add(worldAsLocalPoint3);
		m_meshPointsHelper.Add(worldAsLocalPoint4);
		return m_meshPointsHelper;
	}

	private void CreateMapRoomEntryInstance(Ferr2DT_PathTerrain terrainPrefab, Vector2 center)
	{
		m_terrain = Object.Instantiate(terrainPrefab, m_terrainGO.transform);
		Vector3 mapPositionFromWorld = MapController.GetMapPositionFromWorld(center);
		m_terrain.transform.position = mapPositionFromWorld;
	}

	private void SetBounds()
	{
		float num = float.MaxValue;
		float num2 = float.MinValue;
		float num3 = float.MaxValue;
		float num4 = float.MinValue;
		if (m_terrain.MeshData == null)
		{
			foreach (Vector2 item in m_terrain.PathData.GetFinalPath())
			{
				if (item.x < num)
				{
					num = item.x;
				}
				else if (item.x > num2)
				{
					num2 = item.x;
				}
				if (item.y < num3)
				{
					num3 = item.y;
				}
				else if (item.y > num4)
				{
					num4 = item.y;
				}
			}
		}
		else
		{
			num = m_terrain.MeshData.bounds.center.x - m_terrain.MeshData.bounds.extents.x;
			num2 = m_terrain.MeshData.bounds.center.x + m_terrain.MeshData.bounds.extents.x;
			num3 = m_terrain.MeshData.bounds.center.y - m_terrain.MeshData.bounds.extents.y;
			num4 = m_terrain.MeshData.bounds.center.y + m_terrain.MeshData.bounds.extents.y;
		}
		m_bounds = new Rect(num, num3, num2 - num, num4 - num3);
		m_absBounds = m_bounds;
		m_absBounds.x += m_terrain.transform.position.x;
		m_absBounds.y += m_terrain.transform.position.y;
	}

	private void SetAppearance(BiomeType biome)
	{
		biome = BiomeType_RL.GetGroupedBiomeType(biome);
		BiomeArtData artData = BiomeArtDataLibrary.GetArtData(biome);
		if ((bool)artData)
		{
			m_terrain.vertexColor = artData.Ferr2DBiomeArtData.MapColor;
			m_terrain.CreateVertColors(useSharedMesh: false);
		}
	}

	private static List<Vector2> GetPointOffsets(List<Vector2> meshPoints)
	{
		float num = 0.06f;
		int count = meshPoints.Count;
		m_pointOffsetHelper.Clear();
		for (int i = 0; i < count; i++)
		{
			m_pointOffsetHelper.Add(default(Vector2));
		}
		for (int j = 0; j < count; j++)
		{
			Vector2 vector = meshPoints[j];
			Vector2 vector2 = ((j < count - 1) ? meshPoints[j + 1] : meshPoints[0]) - vector;
			_ = Vector2.zero;
			if (Mathf.RoundToInt(vector2.x) == 0)
			{
				int num2 = ((vector2.y < 0f) ? 1 : (-1));
				m_pointOffsetHelper[j] += new Vector2((float)num2 * num, 0f);
				if (j < count - 1)
				{
					m_pointOffsetHelper[j + 1] += new Vector2((float)num2 * num, 0f);
				}
				else
				{
					m_pointOffsetHelper[0] += new Vector2((float)num2 * num, 0f);
				}
			}
			else
			{
				int num3 = ((vector2.x > 0f) ? 1 : (-1));
				m_pointOffsetHelper[j] += new Vector2(0f, (float)num3 * num);
				if (j < count - 1)
				{
					m_pointOffsetHelper[j + 1] += new Vector2(0f, (float)num3 * num);
				}
				else
				{
					m_pointOffsetHelper[0] += new Vector2(0f, (float)num3 * num);
				}
			}
		}
		return m_pointOffsetHelper;
	}

	private static List<Vector2> GetMeshPoints(PolygonCollider2D collider)
	{
		Vector2[] path = collider.GetPath(0);
		int num = path.Length;
		Vector3 position = collider.transform.position;
		m_meshPointsHelper.Clear();
		for (int i = 0; i < num; i++)
		{
			m_meshPointsHelper.Add(default(Vector2));
		}
		for (int j = 0; j < num; j++)
		{
			Vector2 vector = path[j];
			Vector3 vector2 = collider.transform.TransformPoint(vector);
			float x = 1f / 15f * (vector2.x - position.x) + position.x;
			float y = 1f / 15f * (vector2.y - position.y) + position.y;
			m_meshPointsHelper[j] = collider.transform.InverseTransformPoint(new Vector2(x, y));
		}
		return m_meshPointsHelper;
	}

	public void CreateDoorIconsForRoom(GameObject horizontalDoorPrefab, GameObject verticalDoorPrefab, BaseRoom room)
	{
		foreach (Door door in room.Doors)
		{
			GameObject gameObject = null;
			gameObject = ((door.Side != RoomSide.Left && door.Side != RoomSide.Right) ? Object.Instantiate(horizontalDoorPrefab, m_doorsParentGO.transform) : Object.Instantiate(verticalDoorPrefab, m_doorsParentGO.transform));
			Vector3 mapPositionFromWorld = MapController.GetMapPositionFromWorld(door.CenterPoint);
			gameObject.transform.position = mapPositionFromWorld;
		}
	}

	public void CreateDoorIconsForGridPoint(GameObject horizontalDoorPrefab, GameObject verticalDoorPrefab, GridPointManager room)
	{
		if (room.MergeWithGridPointManagers.Count > 0)
		{
			CreateDoorIconsForGridPoint_Internal(horizontalDoorPrefab, verticalDoorPrefab, room, isPartOfMergeRoom: true);
			for (int i = 0; i < room.MergeWithGridPointManagers.Count; i++)
			{
				CreateDoorIconsForGridPoint_Internal(horizontalDoorPrefab, verticalDoorPrefab, room.MergeWithGridPointManagers[i], isPartOfMergeRoom: true);
			}
		}
		else
		{
			CreateDoorIconsForGridPoint_Internal(horizontalDoorPrefab, verticalDoorPrefab, room);
		}
	}

	private void CreateDoorIconsForGridPoint_Internal(GameObject horizontalDoorPrefab, GameObject verticalDoorPrefab, GridPointManager gpRoom, bool isPartOfMergeRoom = false)
	{
		foreach (DoorLocation doorLocation in gpRoom.DoorLocations)
		{
			GridPointManager gridPointManager = null;
			if (gpRoom.IsTunnelDestination)
			{
				BiomeType groupedBiomeType = BiomeType_RL.GetGroupedBiomeType(gpRoom.Biome);
				BiomeController obj = (GameUtility.IsInLevelEditor ? OnPlayManager.BiomeController : WorldBuilder.GetBiomeController(groupedBiomeType));
				int biomeControllerIndex = gpRoom.BiomeControllerIndex;
				BaseRoom room = obj.GetRoom(biomeControllerIndex);
				if (room is Room)
				{
					Door door = (room as Room).GetDoor(doorLocation.RoomSide, doorLocation.DoorNumber);
					if (door != null)
					{
						BaseRoom connectedRoom = door.ConnectedRoom;
						if (connectedRoom != null && connectedRoom is Room)
						{
							gridPointManager = (connectedRoom as Room).GridPointManager;
						}
					}
				}
			}
			else
			{
				gridPointManager = gpRoom.GetConnectedRoom(doorLocation);
			}
			if (gridPointManager != null && (!isPartOfMergeRoom || !gpRoom.MergeWithGridPointManagers.Contains(gridPointManager)))
			{
				GameObject gameObject = ((doorLocation.RoomSide != RoomSide.Left && doorLocation.RoomSide != RoomSide.Right) ? Object.Instantiate(horizontalDoorPrefab, m_doorsParentGO.transform) : Object.Instantiate(verticalDoorPrefab, m_doorsParentGO.transform));
				Vector3 mapPositionFromWorld = MapController.GetMapPositionFromWorld(RoomUtility.GetDoorCenterPoint(GridController.GetRoomCoordinatesFromGridCoordinates(gpRoom.GridCoordinates), gpRoom.Size, doorLocation));
				gameObject.transform.position = mapPositionFromWorld;
				bool flag = (gpRoom.Biome == BiomeType.Tower && gridPointManager.Biome == BiomeType.TowerExterior) || (gpRoom.Biome == BiomeType.TowerExterior && gridPointManager.Biome == BiomeType.Tower);
				if (gpRoom.Biome != gridPointManager.Biome && !flag)
				{
					gameObject.GetComponent<SpriteRenderer>().color = Color.red;
				}
			}
		}
	}

	public void CreateEnemyIconsForRoom(GameObject enemyIconPrefab, GameObject enemyKilledIconPrefab, BaseRoom room)
	{
		EnemySpawnController[] enemySpawnControllers = room.SpawnControllerManager.EnemySpawnControllers;
		m_enemyKilledIconList = new GameObject[enemySpawnControllers.Length];
		EnemySpawnController[] array = enemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in array)
		{
			GameObject gameObject = Object.Instantiate(enemyKilledIconPrefab, m_enemyIconsParentGO.transform);
			gameObject.transform.position = MapController.GetMapPositionFromWorld(enemySpawnController.transform.position, applyIconYOffset: true);
			m_enemyKilledIconList[enemySpawnController.EnemyIndex] = gameObject;
			if (!enemySpawnController.ShouldSpawn || enemySpawnController.Rank == EnemyRank.Miniboss)
			{
				gameObject.SetActive(value: false);
			}
		}
	}

	public void CreateChestIconsForRoom(GameObject chestOpenIconPrefab, GameObject chestClosedIconPrefab, BaseRoom room)
	{
		ChestSpawnController[] chestSpawnControllers = room.SpawnControllerManager.ChestSpawnControllers;
		m_chestClosedIconList = new GameObject[chestSpawnControllers.Length];
		m_chestOpenIconList = new GameObject[chestSpawnControllers.Length];
		int num = 0;
		ChestSpawnController[] array = chestSpawnControllers;
		foreach (ChestSpawnController chestSpawnController in array)
		{
			GameObject gameObject = Object.Instantiate(chestOpenIconPrefab, m_chestIconsParentGO.transform);
			gameObject.transform.position = MapController.GetMapPositionFromWorld(chestSpawnController.transform.position, applyIconYOffset: true);
			m_chestOpenIconList[num] = gameObject;
			GameObject gameObject2 = Object.Instantiate(chestClosedIconPrefab, m_chestIconsParentGO.transform);
			gameObject2.transform.position = gameObject.transform.position;
			m_chestClosedIconList[num] = gameObject2;
			m_chestSpawnedFlag |= 1 << num;
			num++;
			if (chestSpawnController.ChestType == ChestType.Fairy)
			{
				gameObject.SetActive(value: false);
				gameObject2.SetActive(value: false);
			}
		}
	}

	public void CreateChestIconsForGridPoint(GameObject chestOpenIconPrefab, GameObject chestClosedIconPrefab, GridPointManager room)
	{
		List<GridPointManagerContentEntry> roomContent = GetRoomContent(room, RoomContentType.Chest, excludeNotSpawned: false);
		int count = roomContent.Count;
		m_chestClosedIconList = new GameObject[count];
		m_chestOpenIconList = new GameObject[count];
		int num = 0;
		m_chestSpawnedFlag = 0;
		foreach (GridPointManagerContentEntry item in roomContent)
		{
			GameObject gameObject = Object.Instantiate(chestOpenIconPrefab, m_chestIconsParentGO.transform);
			Vector2 worldPosition = item.WorldPosition;
			gameObject.transform.position = MapController.GetMapPositionFromWorld(worldPosition, applyIconYOffset: true);
			m_chestOpenIconList[num] = gameObject;
			GameObject gameObject2 = (gameObject2 = Object.Instantiate(chestClosedIconPrefab, m_chestIconsParentGO.transform));
			gameObject2.transform.position = gameObject.transform.position;
			m_chestClosedIconList[num] = gameObject2;
			if (!item.IsSpawned)
			{
				gameObject2.SetActive(value: false);
			}
			else
			{
				m_chestSpawnedFlag |= 1 << num;
			}
			num++;
			gameObject.SetActive(value: false);
		}
	}

	public void CreateSpecialRoomIconsForRoom(GameObject specialRoomIconPrefab, GameObject specialRoomUsedIconPrefab, BaseRoom room)
	{
		Object.Instantiate(specialRoomIconPrefab, m_specialRoomIconsParentGO.transform).transform.position = MapController.GetMapPositionFromWorld(room.gameObject.transform.position);
		Object.Instantiate(specialRoomUsedIconPrefab, m_specialRoomIconsParentGO.transform).transform.position = MapController.GetMapPositionFromWorld(room.gameObject.transform.position);
	}

	public void CreateSpecialIndicatorIconForRoom(GameObject specialIndicatorIcon, BaseRoom room)
	{
		m_specialIndicatorIcon = Object.Instantiate(specialIndicatorIcon, m_specialIndicatorIconParentGO.transform);
		m_specialIndicatorIcon.transform.position = MapController.GetMapPositionFromWorld(room.gameObject.transform.position);
	}

	public void CreateEnemyIconsForGridPoint(GameObject enemyIconPrefab, GameObject enemyKilledIconPrefab, GridPointManager room)
	{
		List<GridPointManagerContentEntry> roomContent = GetRoomContent(room, RoomContentType.Enemy, excludeNotSpawned: false);
		int count = roomContent.Count;
		m_enemyKilledIconList = new GameObject[count];
		int num = 0;
		foreach (GridPointManagerContentEntry item in roomContent)
		{
			GameObject gameObject = Object.Instantiate(enemyKilledIconPrefab, m_enemyIconsParentGO.transform);
			gameObject.transform.position = MapController.GetMapPositionFromWorld(item.WorldPosition, applyIconYOffset: true);
			m_enemyKilledIconList[num] = gameObject;
			gameObject.SetActive(value: false);
			num++;
		}
	}

	public void CreateSpecialRoomIconsForGridPoint(GridPointManager room, GameObject specialIndicatorIconPrefab)
	{
		RoomType = room.RoomType;
		bool flag = room.MergeWithGridPointManagers.Count > 0;
		m_gridPointsToCheck_STATIC.Clear();
		m_lastGridPointChecked = null;
		m_specialIconsListHelper_STATIC.Clear();
		m_specialUsedIconsListHelper_STATIC.Clear();
		m_gridPointsToCheck_STATIC.Add(room);
		foreach (GridPointManager mergeWithGridPointManager in room.MergeWithGridPointManagers)
		{
			if (mergeWithGridPointManager.RoomType != RoomType.BossEntrance)
			{
				m_gridPointsToCheck_STATIC.Add(mergeWithGridPointManager);
			}
		}
		foreach (GridPointManager item in m_gridPointsToCheck_STATIC)
		{
			GameObject specialIconPrefab = MapController.GetSpecialIconPrefab(item, getUsed: false, flag);
			GameObject specialIconPrefab2 = MapController.GetSpecialIconPrefab(item, getUsed: true, flag);
			if ((bool)specialIconPrefab && (bool)specialIconPrefab2)
			{
				Vector3 mapPositionFromWorld = MapController.GetMapPositionFromWorld(item.Bounds.center);
				GameObject gameObject = Object.Instantiate(specialIconPrefab, m_specialRoomIconsParentGO.transform);
				gameObject.transform.position = mapPositionFromWorld;
				m_specialIconsListHelper_STATIC.Add(gameObject);
				GameObject gameObject2 = Object.Instantiate(specialIconPrefab2, m_specialRoomIconsParentGO.transform);
				gameObject2.transform.position = mapPositionFromWorld;
				m_specialUsedIconsListHelper_STATIC.Add(gameObject2);
			}
			switch (item.RoomType)
			{
			case RoomType.BossEntrance:
				SpecialRoomType = SpecialRoomType.BossEntrance;
				break;
			case RoomType.Fairy:
				SpecialRoomType = SpecialRoomType.Fairy;
				break;
			case RoomType.Bonus:
			case RoomType.Mandatory:
			{
				SpecialRoomType specialRoomType = item.RoomMetaData.SpecialRoomType;
				SpecialRoomType = specialRoomType;
				break;
			}
			}
			if (room.Biome == BiomeType.Cave && (item.RoomType == RoomType.BossEntrance || SpecialRoomType == SpecialRoomType.SubbossEntrance || SpecialRoomType == SpecialRoomType.WhitePip || SpecialRoomType == SpecialRoomType.WhitePipUnique))
			{
				CreateSpecialIndicatorIconForGridPoint(specialIndicatorIconPrefab, item);
			}
		}
		int count = m_specialIconsListHelper_STATIC.Count;
		if (count > 0)
		{
			m_specialRoomIconList = new GameObject[count];
			m_specialRoomUsedIconList = new GameObject[count];
			for (int i = 0; i < count; i++)
			{
				m_specialRoomIconList[i] = m_specialIconsListHelper_STATIC[i];
				m_specialRoomUsedIconList[i] = m_specialUsedIconsListHelper_STATIC[i];
			}
		}
		bool flag2 = room.Biome == BiomeType.Tower && room.RoomMetaData.SpecialRoomType == SpecialRoomType.Journal && !room.IsTunnelDestination;
		if ((flag || flag2) && count > 0 && room.RoomType != RoomType.BossEntrance)
		{
			RoomCompleteBiomeControllerIndexOverrides = new int[count];
			for (int j = 0; j < count; j++)
			{
				RoomCompleteBiomeControllerIndexOverrides[j] = -1;
			}
			MapController.AddMapEntryNeedsLinking(BiomeType_RL.GetGroupedBiomeType(room.Biome), this);
		}
		ToggleIconVisibility(MapIconType.SpecialIndicator, -1, visible: false);
		ToggleIconVisibility(MapIconType.SpecialRoomUsed, -1, visible: false);
	}

	private List<GridPointManagerContentEntry> GetRoomContent(GridPointManager room, RoomContentType contentType, bool excludeNotSpawned)
	{
		if (room != m_lastGridPointChecked)
		{
			if (m_gridPointsToCheck_STATIC == null)
			{
				m_gridPointsToCheck_STATIC = new SortedSet<GridPointManager>(Comparer<GridPointManager>.Create((GridPointManager a, GridPointManager b) => a.RoomNumber.CompareTo(b.RoomNumber)));
			}
			else
			{
				m_gridPointsToCheck_STATIC.Clear();
			}
		}
		m_lastGridPointChecked = room;
		m_gridPointsToCheck_STATIC.Add(room);
		foreach (GridPointManager mergeWithGridPointManager in room.MergeWithGridPointManagers)
		{
			m_gridPointsToCheck_STATIC.Add(mergeWithGridPointManager);
		}
		m_gridPointRoomContentList_STATIC.Clear();
		foreach (GridPointManager item2 in m_gridPointsToCheck_STATIC)
		{
			if (item2.Content == null)
			{
				continue;
			}
			GridPointManagerContentEntry[] content = item2.Content;
			for (int i = 0; i < content.Length; i++)
			{
				GridPointManagerContentEntry item = content[i];
				if (item.ContentType == contentType && (!excludeNotSpawned || (excludeNotSpawned && item.IsSpawned)))
				{
					m_gridPointRoomContentList_STATIC.Add(item);
				}
			}
		}
		return m_gridPointRoomContentList_STATIC;
	}

	public void CreateSpecialIndicatorIconForGridPoint(GameObject specialIndicatorIcon, GridPointManager room)
	{
		m_specialIndicatorIcon = Object.Instantiate(specialIndicatorIcon, m_specialIndicatorIconParentGO.transform);
		Vector3 mapPositionFromWorld = MapController.GetMapPositionFromWorld(room.Bounds.center);
		m_specialIndicatorIcon.transform.position = mapPositionFromWorld;
	}

	public bool CreateTeleporterIconForGridPoint(GameObject teleporterIconPrefab, GridPointManager room)
	{
		List<GridPointManagerContentEntry> roomContent = GetRoomContent(room, RoomContentType.Teleporter, excludeNotSpawned: false);
		foreach (GridPointManagerContentEntry item in roomContent)
		{
			_ = item;
			if (room.RoomType != RoomType.BossEntrance && room.RoomMetaData.SpecialRoomType != SpecialRoomType.SubbossEntrance && room.RoomMetaData.SpecialRoomType != SpecialRoomType.HeirloomEntrance && room.RoomMetaData.SpecialRoomType != SpecialRoomType.NPC && room.RoomMetaData.SpecialRoomType != SpecialRoomType.WhitePipUnique)
			{
				Object.Instantiate(teleporterIconPrefab, m_teleporterIconParentGO.transform).transform.position = base.transform.position;
			}
		}
		ToggleIconVisibility(MapIconType.Teleporter, -1, visible: false);
		return HasTeleporter = roomContent.Count > 0;
	}
}
