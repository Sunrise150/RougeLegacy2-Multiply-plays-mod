using System;
using UnityEngine;

[Serializable]
public class EffectEntry
{
	[SerializeField]
	private BaseEffect m_effectPrefab;

	[SerializeField]
	private int m_effectPoolSize = 1;

	[SerializeField]
	private EffectType m_effectType;

	public string Name
	{
		get
		{
			if (m_effectPrefab != null)
			{
				return m_effectPrefab.name;
			}
			return "NULL";
		}
	}

	public BaseEffect EffectPrefab
	{
		get
		{
			return m_effectPrefab;
		}
		set
		{
			m_effectPrefab = value;
		}
	}

	public int EffectPoolSize
	{
		get
		{
			return m_effectPoolSize;
		}
		set
		{
			m_effectPoolSize = value;
		}
	}

	public EffectType EffectType
	{
		get
		{
			return m_effectType;
		}
		set
		{
			m_effectType = value;
		}
	}
}
