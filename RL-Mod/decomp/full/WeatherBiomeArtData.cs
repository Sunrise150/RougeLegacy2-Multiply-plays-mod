using System;
using UnityEngine;

[Serializable]
public class WeatherBiomeArtData
{
	[SerializeField]
	private Weather[] m_weatherPrefabArray;

	public Weather[] WeatherPrefabArray
	{
		get
		{
			return m_weatherPrefabArray;
		}
		set
		{
			m_weatherPrefabArray = value;
		}
	}
}
