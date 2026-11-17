using System.Collections.Generic;
using UnityEngine;

public class RoomTerrainManager
{
	private static List<Ferr2DT_PathTerrain> m_pathTerrainChildrenHelper_STATIC = new List<Ferr2DT_PathTerrain>();

	private static List<Ferr2DT_PathTerrain> m_hazardsHelper_STATIC = new List<Ferr2DT_PathTerrain>();

	public Ferr2DT_PathTerrain[] Hazards { get; private set; }

	public List<Ferr2DT_PathTerrain> OneWays { get; private set; }

	public List<Ferr2DT_PathTerrain> Platforms { get; private set; }

	public BaseRoom Room { get; }

	public RoomTerrainManager(BaseRoom room)
	{
		Room = room;
		Initialize();
	}

	public void Initialize()
	{
		Platforms = new List<Ferr2DT_PathTerrain>();
		OneWays = new List<Ferr2DT_PathTerrain>();
		m_hazardsHelper_STATIC.Clear();
		m_pathTerrainChildrenHelper_STATIC.Clear();
		Room.gameObject.GetComponentsInChildren(m_pathTerrainChildrenHelper_STATIC);
		foreach (Ferr2DT_PathTerrain item in m_pathTerrainChildrenHelper_STATIC)
		{
			if (item.CompareTag("Platform"))
			{
				Platforms.Add(item);
			}
			else if (item.CompareTag("OneWay"))
			{
				OneWays.Add(item);
			}
			else if (item.CompareTag("Hazard") || item.CompareTag("TriggerHazard"))
			{
				m_hazardsHelper_STATIC.Add(item);
			}
		}
		Hazards = m_hazardsHelper_STATIC.ToArray();
		foreach (Ferr2DT_PathTerrain item2 in m_pathTerrainChildrenHelper_STATIC)
		{
			item2.DisableOnStartMeshBuild = true;
		}
	}

	public void AddOneWay(Ferr2DT_PathTerrain oneWay)
	{
		OneWays.Add(oneWay);
	}

	public void RemoveOneWay(GameObject oneWayGameObject)
	{
		for (int num = OneWays.Count - 1; num >= 0; num--)
		{
			if (OneWays[num].gameObject == oneWayGameObject)
			{
				OneWays.RemoveAt(num);
				break;
			}
		}
	}

	public void AddPlatform(Ferr2DT_PathTerrain doorTerrain)
	{
		Platforms.Add(doorTerrain);
	}
}
