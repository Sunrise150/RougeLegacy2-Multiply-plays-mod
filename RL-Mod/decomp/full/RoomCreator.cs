using System;
using System.Collections.Generic;
using Cinemachine;
using ClipperLibFerr;
using UnityEngine;

public class RoomCreator : MonoBehaviour
{
	[SerializeField]
	private GameObject m_playerSpawnPrefab;

	[SerializeField]
	private GameObject m_roomPrefab;

	[SerializeField]
	private GameObject m_chestPrefab;

	[SerializeField]
	private GameObject m_specialPlatformSpawnerPrefab;

	[SerializeField]
	private GameObject m_spikePrefab;

	[SerializeField]
	private GameObject m_localTeleporterPrefab;

	[SerializeField]
	private GameObject m_teleporterLeyLinesPrefab;

	[SerializeField]
	private GameObject m_globalTeleporterPrefab;

	[SerializeField]
	private GameObject m_lineHazardSpawnControllerPrefab;

	[SerializeField]
	private GameObject m_pointHazardSpawnControllerPrefab;

	[SerializeField]
	private GameObject m_turretHazardSpawnControllerPrefab;

	[SerializeField]
	private GameObject m_turretDiagonalPrefab;

	[SerializeField]
	private Ferr2DT_PathTerrain m_defaultTerrainPrefab;

	[SerializeField]
	private Ferr2DT_PathTerrain m_defaultDoorsPrefab;

	[SerializeField]
	private Ferr2DT_PathTerrain m_defaultOneWayTerrainPrefab;

	[SerializeField]
	private CinemachineVirtualCamera m_cinemachineVirtualCameraPrefab;

	[SerializeField]
	private GameObject m_enemyPrefab;

	[SerializeField]
	private GameObject m_cutoutPrefab;

	[SerializeField]
	private GameObject m_tunnelSpawnControllerPrefab;

	[SerializeField]
	private GameObject m_cloudSpawnControllerPrefab;

	[SerializeField]
	private GameObject m_invisibleCeilingSpawnControllerPrefab;

	[SerializeField]
	private GameObject m_simpleRoomPrefab;

	[SerializeField]
	private GameObject m_bridge1x3RoomPrefab;

	[SerializeField]
	private GameObject m_roomForTunnelTestingPrefab;

	[SerializeField]
	private GameObject m_hiddenWallPrefab;

	[SerializeField]
	private GameObject m_buildingInteriorPrefab;

	public const int ROOM_UNIT_WIDTH = 32;

	public const int ROOM_UNIT_HEIGHT = 18;

	public const int SIDE_DOOR_UNIT_SIZE = 5;

	public const int UPPER_LOWER_DOOR_UNIT_SIZE = 6;

	public const float WALL_WIDTH = 1f;

	public GameObject RoomTemplatePrefab => m_roomPrefab;

	public GameObject SimpleRoomPrefab => m_simpleRoomPrefab;

	public GameObject Bridge1x3RoomPrefab => m_bridge1x3RoomPrefab;

	public GameObject CreateRoomInstance(int roomWidth, int roomHeight)
	{
		GameObject obj = UnityEngine.Object.Instantiate(m_roomPrefab);
		obj.tag = "Room";
		Room component = obj.GetComponent<Room>();
		component.SetUnitDimensions(roomWidth * 32, roomHeight * 18);
		GameObject wallsLocation = new GameObject("Walls");
		component.WallsLocation = wallsLocation;
		CreateRoomSides(component);
		GameObject doorsLocation = new GameObject("Doors");
		component.DoorsLocation = doorsLocation;
		CreateRoomDoors(component);
		CreateRoomBounds(roomWidth, roomHeight, component);
		return obj;
	}

	private GameObject CreateRoomBounds(int roomWidth, int roomHeight, Room room)
	{
		Ferr2DT_PathTerrain component = room.gameObject.GetComponent<Ferr2DT_PathTerrain>();
		int num = roomWidth * 16;
		int num2 = roomHeight * 9;
		component.ClearPoints();
		component.AddPoint(new Vector2(-num, num2));
		component.AddPoint(new Vector2(-num, -num2));
		component.AddPoint(new Vector2(num, -num2));
		component.AddPoint(new Vector2(num, num2));
		component.FillMode = Ferr2D_SectionMode.Normal;
		component.EdgeMode = Ferr2D_SectionMode.None;
		component.Build();
		return component.gameObject;
	}

	public Ferr2DT_PathTerrain CreateTerrain(Room room)
	{
		Ferr2DT_PathTerrain ferr2DT_PathTerrain = UnityEngine.Object.Instantiate(m_defaultTerrainPrefab);
		ferr2DT_PathTerrain.transform.SetParent(room.WallsLocation.transform);
		ferr2DT_PathTerrain.transform.localPosition = Vector3.zero;
		ferr2DT_PathTerrain.transform.localRotation = Quaternion.identity;
		ferr2DT_PathTerrain.transform.localScale = Vector3.one;
		ferr2DT_PathTerrain.name = "Terrain";
		ferr2DT_PathTerrain.tag = "Platform";
		ferr2DT_PathTerrain.ClearPoints();
		ferr2DT_PathTerrain.AddPoint(new Vector2(-1f, 1f));
		ferr2DT_PathTerrain.AddPoint(new Vector2(-1f, -1f));
		ferr2DT_PathTerrain.AddPoint(new Vector2(1f, -1f));
		ferr2DT_PathTerrain.AddPoint(new Vector2(1f, 1f));
		ferr2DT_PathTerrain.Build();
		return ferr2DT_PathTerrain;
	}

	public Ferr2DT_PathTerrain CreateOneWay(Room room)
	{
		if (room == null)
		{
			throw new ArgumentNullException("room");
		}
		Ferr2DT_PathTerrain ferr2DT_PathTerrain = UnityEngine.Object.Instantiate(m_defaultOneWayTerrainPrefab);
		if (ferr2DT_PathTerrain != null)
		{
			ferr2DT_PathTerrain.transform.SetParent(room.OneWayLocation);
			ferr2DT_PathTerrain.transform.localPosition = Vector3.zero;
			ferr2DT_PathTerrain.name = "One Way";
			ferr2DT_PathTerrain.tag = "OneWay";
		}
		else
		{
			Debug.LogFormat("Failed to instantiate One Way Platform");
		}
		ferr2DT_PathTerrain.Build();
		return ferr2DT_PathTerrain;
	}

	private void CreateRoomSides(Room room)
	{
		int x = room.UnitDimensions.x;
		int y = room.UnitDimensions.y;
		int num = 13;
		int num2 = (int)Math.Round(13.0, MidpointRounding.AwayFromZero);
		num2 -= num;
		int num3 = 6;
		int num4 = (int)Math.Round(6.5, MidpointRounding.AwayFromZero);
		num4 -= num3;
		for (int i = 0; i < x / 32; i++)
		{
			int num5 = -(int)((float)x / 2f) + i * 32;
			int num6 = (int)((float)y / 2f);
			Ferr2DT_PathTerrain ferr2DT_PathTerrain = CreateTerrain(room);
			ferr2DT_PathTerrain.PathData.Set(0, new Vector2(num5, num6));
			ferr2DT_PathTerrain.PathData.Set(1, new Vector2(num5, num6 - 1));
			ferr2DT_PathTerrain.PathData.Set(2, new Vector2(num5 + num, num6 - 1));
			ferr2DT_PathTerrain.PathData.Set(3, new Vector2(num5 + num, num6));
			ferr2DT_PathTerrain.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain, rebuild: true);
			num5 += num + 6;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain2 = CreateTerrain(room);
			ferr2DT_PathTerrain2.PathData.Set(0, new Vector2(num5, num6));
			ferr2DT_PathTerrain2.PathData.Set(1, new Vector2(num5, num6 - 1));
			ferr2DT_PathTerrain2.PathData.Set(2, new Vector2(num5 + num + num2, num6 - 1));
			ferr2DT_PathTerrain2.PathData.Set(3, new Vector2(num5 + num + num2, num6));
			ferr2DT_PathTerrain2.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain2, rebuild: true);
			num5 = -(int)((float)x / 2f) + i * 32;
			num6 = -num6 + 1;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain3 = CreateTerrain(room);
			ferr2DT_PathTerrain3.PathData.Set(0, new Vector2(num5, num6));
			ferr2DT_PathTerrain3.PathData.Set(1, new Vector2(num5, num6 - 1));
			ferr2DT_PathTerrain3.PathData.Set(2, new Vector2(num5 + num, num6 - 1));
			ferr2DT_PathTerrain3.PathData.Set(3, new Vector2(num5 + num, num6));
			ferr2DT_PathTerrain3.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain3, rebuild: true);
			num5 += num + 6;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain4 = CreateTerrain(room);
			ferr2DT_PathTerrain4.PathData.Set(0, new Vector2(num5, num6));
			ferr2DT_PathTerrain4.PathData.Set(1, new Vector2(num5, num6 - 1));
			ferr2DT_PathTerrain4.PathData.Set(2, new Vector2(num5 + num + num2, num6 - 1));
			ferr2DT_PathTerrain4.PathData.Set(3, new Vector2(num5 + num + num2, num6));
			ferr2DT_PathTerrain4.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain4, rebuild: true);
		}
		for (int j = 0; j < y / 18; j++)
		{
			int num7 = -(int)((float)x / 2f);
			int num8 = (int)((float)y / 2f) - j * 18;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain5 = CreateTerrain(room);
			ferr2DT_PathTerrain5.PathData.Set(0, new Vector2(num7, num8));
			ferr2DT_PathTerrain5.PathData.Set(1, new Vector2(num7, num8 - num3));
			ferr2DT_PathTerrain5.PathData.Set(2, new Vector2(num7 + 1, num8 - num3));
			ferr2DT_PathTerrain5.PathData.Set(3, new Vector2(num7 + 1, num8));
			ferr2DT_PathTerrain5.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain5, rebuild: true);
			num8 -= num3 + 5;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain6 = CreateTerrain(room);
			ferr2DT_PathTerrain6.PathData.Set(0, new Vector2(num7, num8));
			ferr2DT_PathTerrain6.PathData.Set(1, new Vector2(num7, num8 - num3 - num4));
			ferr2DT_PathTerrain6.PathData.Set(2, new Vector2(num7 + 1, num8 - num3 - num4));
			ferr2DT_PathTerrain6.PathData.Set(3, new Vector2(num7 + 1, num8));
			ferr2DT_PathTerrain6.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain6, rebuild: true);
			num8 = (int)((float)y / 2f) - j * 18;
			num7 = -num7 - 1;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain7 = CreateTerrain(room);
			ferr2DT_PathTerrain7.PathData.Set(0, new Vector2(num7, num8));
			ferr2DT_PathTerrain7.PathData.Set(1, new Vector2(num7, num8 - num3));
			ferr2DT_PathTerrain7.PathData.Set(2, new Vector2(num7 + 1, num8 - num3));
			ferr2DT_PathTerrain7.PathData.Set(3, new Vector2(num7 + 1, num8));
			ferr2DT_PathTerrain7.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain7, rebuild: true);
			num8 -= num3 + 5;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain8 = CreateTerrain(room);
			ferr2DT_PathTerrain8.PathData.Set(0, new Vector2(num7, num8));
			ferr2DT_PathTerrain8.PathData.Set(1, new Vector2(num7, num8 - num3 - num4));
			ferr2DT_PathTerrain8.PathData.Set(2, new Vector2(num7 + 1, num8 - num3 - num4));
			ferr2DT_PathTerrain8.PathData.Set(3, new Vector2(num7 + 1, num8));
			ferr2DT_PathTerrain8.name = "Sides";
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain8, rebuild: true);
		}
		List<Ferr2DT_PathTerrain> list = new List<Ferr2DT_PathTerrain>();
		foreach (Transform item in room.WallsLocation.transform)
		{
			Ferr2DT_PathTerrain component = item.gameObject.GetComponent<Ferr2DT_PathTerrain>();
			if (component != null)
			{
				list.Add(component);
			}
		}
		Ferr2DUtilities.EasyBoolean(ClipType.ctUnion, list, Ferr2DUtilities.Ferr2DUndoType.DestroyImmediate);
	}

	public GameObject CreateBuildingInterior(Room currentlySelectedRoom)
	{
		return null;
	}

	public GameObject CreateHiddenRoomWall(Room currentlySelectedRoom)
	{
		return null;
	}

	public GameObject CreateCloud(Room room)
	{
		return InstantiatePrefabInRoomAtLocalPosition(m_cloudSpawnControllerPrefab, room);
	}

	public GameObject CreateInvisibleCeiling(Room room)
	{
		return InstantiatePrefabInRoomAtLocalPosition(m_invisibleCeilingSpawnControllerPrefab, room);
	}

	public GameObject CreateTunnel(Room room)
	{
		return InstantiatePrefabInRoomAtLocalPosition(m_tunnelSpawnControllerPrefab, room);
	}

	public void CreateSpikes(Room room)
	{
	}

	public void CreateLocalTeleporter(Room room)
	{
	}

	public void CreateTeleporterLeyLines(Room room)
	{
	}

	public void CreateGlobalTeleporter(Room room)
	{
	}

	public void CreateOrbiter(Room room)
	{
	}

	public void CreateTurret(Room room, bool diagonal)
	{
	}

	public void CreateHazardSpawnController(HazardCategory hazardCategory, Room room)
	{
	}

	public void CreateChest(Room room)
	{
	}

	public void CreateBouncePlatform(Room room)
	{
	}

	public GameObject CreateEnemy(Room room)
	{
		if (room == null)
		{
			throw new ArgumentNullException("room");
		}
		if (m_enemyPrefab != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(m_enemyPrefab);
			obj.transform.SetParent(room.EnemyLocation);
			return obj;
		}
		Debug.LogFormat("<color=yellow>Enemy Prefab Field in RoomCreator is null</color>");
		return null;
	}

	private void CreateRoomDoors(Room room)
	{
		int x = room.UnitDimensions.x;
		int y = room.UnitDimensions.y;
		int num = 13;
		int num2 = 6;
		int num3 = x / 32;
		for (int i = 0; i < num3; i++)
		{
			int num4 = -(int)((float)x / 2f) + 32 * i + num;
			int num5 = (int)((float)y / 2f);
			Ferr2DT_PathTerrain ferr2DT_PathTerrain = UnityEngine.Object.Instantiate(m_defaultDoorsPrefab);
			ferr2DT_PathTerrain.GetComponent<Door>().InitialiseInEditor(RoomSide.Top, i);
			ferr2DT_PathTerrain.transform.SetParent(room.DoorsLocation.transform);
			ferr2DT_PathTerrain.transform.localPosition = Vector3.zero;
			ferr2DT_PathTerrain.transform.localRotation = Quaternion.identity;
			ferr2DT_PathTerrain.transform.localScale = Vector3.one;
			ferr2DT_PathTerrain.name = "Top Door";
			ferr2DT_PathTerrain.ClearPoints();
			ferr2DT_PathTerrain.AddPoint(new Vector2(num4, num5));
			ferr2DT_PathTerrain.AddPoint(new Vector2(num4, num5 - 1));
			ferr2DT_PathTerrain.AddPoint(new Vector2(num4 + 6, num5 - 1));
			ferr2DT_PathTerrain.AddPoint(new Vector2(num4 + 6, num5));
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain, rebuild: true);
			Ferr2DT_PathTerrain ferr2DT_PathTerrain2 = UnityEngine.Object.Instantiate(m_defaultDoorsPrefab);
			ferr2DT_PathTerrain2.GetComponent<Door>().InitialiseInEditor(RoomSide.Bottom, i);
			ferr2DT_PathTerrain2.transform.SetParent(room.DoorsLocation.transform);
			ferr2DT_PathTerrain2.transform.localPosition = Vector3.zero;
			ferr2DT_PathTerrain2.transform.localRotation = Quaternion.identity;
			ferr2DT_PathTerrain2.transform.localScale = Vector3.one;
			ferr2DT_PathTerrain2.name = "Bottom Door";
			num5 = -num5 + 1;
			ferr2DT_PathTerrain2.ClearPoints();
			ferr2DT_PathTerrain2.AddPoint(new Vector2(num4, num5));
			ferr2DT_PathTerrain2.AddPoint(new Vector2(num4, num5 - 1));
			ferr2DT_PathTerrain2.AddPoint(new Vector2(num4 + 6, num5 - 1));
			ferr2DT_PathTerrain2.AddPoint(new Vector2(num4 + 6, num5));
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain2, rebuild: true);
		}
		num3 = y / 18;
		for (int j = 0; j < num3; j++)
		{
			int num6 = -(int)((float)x / 2f);
			int num7 = (int)((float)y / 2f) - j * 18 - num2;
			Ferr2DT_PathTerrain ferr2DT_PathTerrain3 = UnityEngine.Object.Instantiate(m_defaultDoorsPrefab);
			ferr2DT_PathTerrain3.GetComponent<Door>().InitialiseInEditor(RoomSide.Left, j);
			ferr2DT_PathTerrain3.transform.SetParent(room.DoorsLocation.transform);
			ferr2DT_PathTerrain3.transform.localPosition = Vector3.zero;
			ferr2DT_PathTerrain3.transform.localRotation = Quaternion.identity;
			ferr2DT_PathTerrain3.transform.localScale = Vector3.one;
			ferr2DT_PathTerrain3.name = "Left Door";
			ferr2DT_PathTerrain3.ClearPoints();
			ferr2DT_PathTerrain3.AddPoint(new Vector2(num6, num7));
			ferr2DT_PathTerrain3.AddPoint(new Vector2(num6, num7 - 5));
			ferr2DT_PathTerrain3.AddPoint(new Vector2(num6 + 1, num7 - 5));
			ferr2DT_PathTerrain3.AddPoint(new Vector2(num6 + 1, num7));
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain3, rebuild: true);
			Ferr2DT_PathTerrain ferr2DT_PathTerrain4 = UnityEngine.Object.Instantiate(m_defaultDoorsPrefab);
			ferr2DT_PathTerrain4.GetComponent<Door>().InitialiseInEditor(RoomSide.Right, j);
			ferr2DT_PathTerrain4.transform.SetParent(room.DoorsLocation.transform);
			ferr2DT_PathTerrain4.transform.localPosition = Vector3.zero;
			ferr2DT_PathTerrain4.transform.localRotation = Quaternion.identity;
			ferr2DT_PathTerrain4.transform.localScale = Vector3.one;
			ferr2DT_PathTerrain4.name = "Right Door";
			num6 = -num6 - 1;
			ferr2DT_PathTerrain4.ClearPoints();
			ferr2DT_PathTerrain4.AddPoint(new Vector2(num6, num7));
			ferr2DT_PathTerrain4.AddPoint(new Vector2(num6, num7 - 5));
			ferr2DT_PathTerrain4.AddPoint(new Vector2(num6 + 1, num7 - 5));
			ferr2DT_PathTerrain4.AddPoint(new Vector2(num6 + 1, num7));
			Ferr2DUtilities.RecentreFerr2DTerrain(ferr2DT_PathTerrain4, rebuild: true);
		}
	}

	public void CreatePlayerSpawn(Room room)
	{
		InstantiatePrefabInRoomAtLocalPosition(m_playerSpawnPrefab, room);
	}

	public GameObject CreateRoomForTunnelTesting(Vector3 position)
	{
		if (m_roomForTunnelTestingPrefab != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(m_roomForTunnelTestingPrefab);
			obj.transform.position = position;
			return obj;
		}
		return null;
	}

	private GameObject InstantiatePrefabInRoomAtLocalPosition(GameObject prefab, Room room, Vector3 localPosition = default(Vector3))
	{
		return null;
	}
}
