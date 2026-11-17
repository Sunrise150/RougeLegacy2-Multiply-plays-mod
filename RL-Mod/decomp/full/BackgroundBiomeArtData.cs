using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class BackgroundBiomeArtData
{
	[SerializeField]
	private List<Background> m_backgrounds;

	[SerializeField]
	private bool m_tileNormally = true;

	public List<Background> Backgrounds
	{
		get
		{
			return m_backgrounds;
		}
		set
		{
			m_backgrounds = value;
		}
	}

	public bool TileNormally => m_tileNormally;

	public void AddBackground()
	{
		if (Backgrounds == null)
		{
			Backgrounds = new List<Background>();
		}
		Backgrounds.Add(null);
	}
}
