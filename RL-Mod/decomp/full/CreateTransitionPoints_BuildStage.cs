using System.Collections;
using System.Collections.Generic;
using Spawn;
using UnityEngine;

public class CreateTransitionPoints_BuildStage : IBiomeBuildStage
{
	private static string TOP_DOOR_DATA = "Scriptable Objects/Spawn Controller Data/Props/TransitionPointTop_Editor_Data";

	private static string BOTTOM_DOOR_DATA = "Scriptable Objects/Spawn Controller Data/Props/TransitionPointBottom_Editor_Data";

	private static string SIDE_DOOR_DATA = "Scriptable Objects/Spawn Controller Data/Props/TransitionPointSide_Editor_Data";

	private static Dictionary<RoomSide, PropSpawnControllerData> m_dataTable = new Dictionary<RoomSide, PropSpawnControllerData>();

	public IEnumerator Run(BiomeController biomeController)
	{
		List<Door> transitionPoints = GetTransitionPoints(biomeController);
		PopulateDataTable();
		foreach (Door item in transitionPoints)
		{
			CreateTransitionPoint(item, item.TransitionsToBiome);
		}
		yield break;
	}

	private static void CreateTransitionPoint(Door door, BiomeType doorLeadsToBiome)
	{
		Prop[] array = m_dataTable[door.Side].DefaultProps;
		for (int i = 0; i < m_dataTable[door.Side].PropsPerBiome.Length; i++)
		{
			if (m_dataTable[door.Side].PropsPerBiome[i].Biome == doorLeadsToBiome)
			{
				array = m_dataTable[door.Side].PropsPerBiome[i].Props;
				break;
			}
		}
		if (array != null && array.Length != 0 && !(array[0] == null))
		{
			Prop prop = Object.Instantiate(array[0]);
			if (door.Side == RoomSide.Left)
			{
				prop.transform.localScale = new Vector3(-1f * prop.transform.localScale.x, prop.transform.localScale.y, prop.transform.localScale.z);
			}
			prop.transform.SetParent(door.Room.gameObject.transform);
			prop.transform.position = door.CenterPoint;
			prop.SetRoom(door.Room);
		}
	}

	public static IEnumerator RunFromLevelEditor(BiomeType leadsToBiome, Door door)
	{
		if (GameUtility.IsInLevelEditor)
		{
			PopulateDataTable();
			if (leadsToBiome != 0)
			{
				CreateTransitionPoint(door, leadsToBiome);
			}
		}
		yield break;
	}

	private static void PopulateDataTable()
	{
		RoomSide[] array = new RoomSide[4]
		{
			RoomSide.Left,
			RoomSide.Right,
			RoomSide.Top,
			RoomSide.Bottom
		};
		foreach (RoomSide roomSide in array)
		{
			if (!m_dataTable.ContainsKey(roomSide))
			{
				string path = "";
				switch (roomSide)
				{
				case RoomSide.Top:
					path = TOP_DOOR_DATA;
					break;
				case RoomSide.Bottom:
					path = BOTTOM_DOOR_DATA;
					break;
				case RoomSide.Left:
				case RoomSide.Right:
					path = SIDE_DOOR_DATA;
					break;
				}
				PropSpawnControllerData value = CDGResources.Load<PropSpawnControllerData>(path);
				m_dataTable.Add(roomSide, value);
			}
		}
	}

	private List<Door> GetTransitionPoints(BiomeController biomeController)
	{
		List<Door> list = new List<Door>();
		for (int i = 0; i < biomeController.Rooms.Count; i++)
		{
			if (biomeController.Rooms[i].Doors.Count == 0)
			{
				continue;
			}
			for (int j = 0; j < biomeController.Rooms[i].Doors.Count; j++)
			{
				Door door = biomeController.Rooms[i].Doors[j];
				Door connectedDoor = door.ConnectedDoor;
				if (biomeController.Rooms[i].Doors[j].IsBiomeTransitionPoint)
				{
					list.Add(biomeController.Rooms[i].Doors[j]);
				}
				else if (!connectedDoor.IsNativeNull() && ((door.Room.AppearanceBiomeType == BiomeType.Tower && connectedDoor.Room.AppearanceBiomeType == BiomeType.TowerExterior) || (door.Room.AppearanceBiomeType == BiomeType.TowerExterior && connectedDoor.Room.AppearanceBiomeType == BiomeType.Tower)))
				{
					list.Add(biomeController.Rooms[i].Doors[j]);
				}
			}
		}
		return list;
	}
}
