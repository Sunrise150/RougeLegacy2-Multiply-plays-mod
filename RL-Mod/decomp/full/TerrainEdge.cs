using System;
using UnityEngine;

[Serializable]
public class TerrainEdge
{
	[SerializeField]
	private Vector2 m_start;

	[SerializeField]
	private Vector2 m_end;

	[SerializeField]
	private bool m_hasEdge;

	public Vector2 Start
	{
		get
		{
			return m_start;
		}
		private set
		{
			m_start = value;
		}
	}

	public Vector2 End
	{
		get
		{
			return m_end;
		}
		private set
		{
			m_end = value;
		}
	}

	public bool HasEdge
	{
		get
		{
			return m_hasEdge;
		}
		private set
		{
			m_hasEdge = value;
		}
	}

	public TerrainEdge(Vector2 start, Vector2 end, bool hasEdge)
	{
		Start = start;
		End = end;
		HasEdge = hasEdge;
	}
}
