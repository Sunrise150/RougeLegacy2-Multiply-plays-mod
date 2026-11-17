using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class AmbientLightAndFogBiomeArtData
{
	[SerializeField]
	private bool m_fog;

	[SerializeField]
	private int m_fogModeIndex;

	[SerializeField]
	private FogMode m_fogMode = FogMode.Linear;

	[SerializeField]
	private Color m_fogColor;

	[SerializeField]
	private float m_fogDensity;

	[SerializeField]
	private float m_fogStartDistance;

	[SerializeField]
	private float m_fogEndDistance;

	[SerializeField]
	private Light m_lightMasterPrefab;

	public bool Fog
	{
		get
		{
			return m_fog;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_fog = value;
			}
		}
	}

	public FogMode FogMode
	{
		get
		{
			if (FogModeIndex == 0)
			{
				m_fogMode = FogMode.Linear;
			}
			else if (FogModeIndex == 1)
			{
				m_fogMode = FogMode.Exponential;
			}
			else
			{
				if (FogModeIndex != 2)
				{
					throw new ArgumentOutOfRangeException("FogModeIndex");
				}
				m_fogMode = FogMode.ExponentialSquared;
			}
			return m_fogMode;
		}
	}

	public int FogModeIndex
	{
		get
		{
			return m_fogModeIndex;
		}
		set
		{
			m_fogModeIndex = value;
		}
	}

	public Color FogColor
	{
		get
		{
			return m_fogColor;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_fogColor = value;
			}
		}
	}

	public float FogDensity
	{
		get
		{
			return m_fogDensity;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_fogDensity = value;
			}
		}
	}

	public float FogStartDistance
	{
		get
		{
			return m_fogStartDistance;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_fogStartDistance = value;
			}
		}
	}

	public float FogEndDistance
	{
		get
		{
			return m_fogEndDistance;
		}
		set
		{
			if (!Application.isPlaying)
			{
				m_fogEndDistance = value;
			}
		}
	}

	public Color AmbientLightColor => Color.black;

	public AmbientMode AmbientMode => AmbientMode.Flat;

	public Light LightMasterPrefab
	{
		get
		{
			return m_lightMasterPrefab;
		}
		set
		{
			m_lightMasterPrefab = value;
		}
	}
}
