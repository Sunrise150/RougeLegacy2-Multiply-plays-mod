using System;
using UnityEngine;

[Serializable]
public class Ferr2DBiomeArtData
{
	[SerializeField]
	private MasterFerr2DEntry m_terrainMaster;

	[SerializeField]
	private MasterFerr2DEntry m_oneWayMaster;

	[SerializeField]
	private Color m_tint = Color.white;

	[SerializeField]
	private Color m_mapColor = Color.white;

	public Color Tint
	{
		get
		{
			return m_tint;
		}
		private set
		{
			m_tint = value;
		}
	}

	public Color MapColor
	{
		get
		{
			return m_mapColor;
		}
		private set
		{
			m_mapColor = value;
		}
	}

	public MasterFerr2DEntry TerrainMaster => m_terrainMaster;

	public MasterFerr2DEntry OneWayMaster => m_oneWayMaster;

	public void SetTint(Color tint)
	{
		Tint = tint;
	}

	public void SetMapColor(Color color)
	{
		MapColor = color;
	}
}
