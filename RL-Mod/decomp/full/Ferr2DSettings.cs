using System;
using UnityEngine;

[Serializable]
public class Ferr2DSettings
{
	[SerializeField]
	private Ferr2DT_PathTerrain m_master;

	public Ferr2DT_PathTerrain Master
	{
		get
		{
			return m_master;
		}
		private set
		{
			m_master = value;
		}
	}

	public Ferr2DT_Material Material
	{
		get
		{
			if (Master != null)
			{
				return Master.TerrainMaterial as Ferr2DT_Material;
			}
			return null;
		}
	}

	public bool InteriorGridVerts => false;

	public float GridSpacing => -1f;

	public Ferr2DT_ColorType ColorType => Ferr2DT_ColorType.SolidColor;

	public float VertexGradientDistance => -1f;

	public Gradient VertexGradient
	{
		get
		{
			if (Master != null)
			{
				return Master.vertexGradient;
			}
			return null;
		}
	}

	public float PixelsPerUnit
	{
		get
		{
			if (Master != null)
			{
				return Master.pixelsPerUnit;
			}
			return -1f;
		}
	}

	public bool CreateTangents
	{
		get
		{
			if (Master != null)
			{
				return Master.createTangents;
			}
			return false;
		}
	}

	public int SortingLayer
	{
		get
		{
			if (Master != null)
			{
				return Master.GetComponent<Renderer>().sortingLayerID;
			}
			return 0;
		}
	}

	public float FillZ
	{
		get
		{
			if (Master != null)
			{
				return Master.fillZ;
			}
			return 0f;
		}
	}

	public int OrderInLayer
	{
		get
		{
			if (Master != null)
			{
				return Master.GetComponent<Renderer>().sortingOrder;
			}
			return -1;
		}
	}

	public void Update(Ferr2DT_PathTerrain masterPrefab)
	{
		Master = masterPrefab;
	}
}
