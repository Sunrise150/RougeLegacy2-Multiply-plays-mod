using System;
using UnityEngine;

[Serializable]
public class SkyBiomeArtData
{
	[SerializeField]
	private GameObject m_skyPrefab;

	[SerializeField]
	private Vector2Int m_dimensions;

	public GameObject SkyPrefab
	{
		get
		{
			return m_skyPrefab;
		}
		set
		{
			m_skyPrefab = value;
		}
	}

	public Vector2Int Dimensions
	{
		get
		{
			return m_dimensions;
		}
		set
		{
			m_dimensions = value;
		}
	}
}
