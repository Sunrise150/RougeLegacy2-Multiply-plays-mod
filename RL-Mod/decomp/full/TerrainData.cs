using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class TerrainData
{
	[SerializeField]
	private List<TerrainEdge> m_edges;

	[SerializeField]
	private Vector2 m_localPosition;

	public List<TerrainEdge> Edges
	{
		get
		{
			return m_edges;
		}
		private set
		{
			m_edges = value;
		}
	}

	public Vector2 LocalPosition
	{
		get
		{
			return m_localPosition;
		}
		private set
		{
			m_localPosition = value;
		}
	}

	public TerrainData(Ferr2DT_PathTerrain ferr2DTerrain)
	{
		LocalPosition = ferr2DTerrain.transform.localPosition;
		Edges = new List<TerrainEdge>();
		for (int i = 0; i < ferr2DTerrain.PathData.Count; i++)
		{
			Vector2 start = ferr2DTerrain.PathData.Get(i);
			Vector2 end = ((i >= ferr2DTerrain.PathData.Count - 1) ? ferr2DTerrain.PathData.Get(0) : ferr2DTerrain.PathData.Get(i + 1));
			bool hasEdge = ferr2DTerrain.PathData.GetData(i).directionOverride != 4;
			Edges.Add(new TerrainEdge(start, end, hasEdge));
		}
	}
}
