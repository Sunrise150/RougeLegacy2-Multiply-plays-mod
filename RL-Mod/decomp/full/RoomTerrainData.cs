using System.Collections.Generic;
using UnityEngine;

public class RoomTerrainData : ScriptableObject
{
	[SerializeField]
	private List<TerrainData> m_terrain;

	public List<TerrainData> Terrain
	{
		get
		{
			return m_terrain;
		}
		private set
		{
			m_terrain = value;
		}
	}

	public void Initialise(BaseRoom room)
	{
		Terrain = new List<TerrainData>();
		foreach (Ferr2DT_PathTerrain platform in room.TerrainManager.Platforms)
		{
			Terrain.Add(new TerrainData(platform));
		}
	}
}
