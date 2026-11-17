using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Sky Palette Library")]
public class SkyPaletteLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/SkyPaletteLibrary";

	[SerializeField]
	private SkyPaletteEntry[] m_skyPaletteArray;

	private Dictionary<SkyPaletteType, SkyPaletteEntry> m_skyPaletteDict;

	private static SkyPaletteLibrary m_instance;

	private static SkyPaletteLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<SkyPaletteLibrary>("Scriptable Objects/Libraries/SkyPaletteLibrary");
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	public static SkyPaletteEntry GetPalette(SkyPaletteType type)
	{
		SkyPaletteEntry value = null;
		if (Instance.m_skyPaletteDict.TryGetValue(type, out value))
		{
			return value;
		}
		throw new Exception("SkyPaletteType: " + type.ToString() + " not found in SkyPalette Library.");
	}

	private void Initialize()
	{
		m_skyPaletteDict = new Dictionary<SkyPaletteType, SkyPaletteEntry>();
		SkyPaletteEntry[] skyPaletteArray = m_skyPaletteArray;
		foreach (SkyPaletteEntry skyPaletteEntry in skyPaletteArray)
		{
			if (m_skyPaletteDict.ContainsKey(skyPaletteEntry.SkyPaletteType))
			{
				throw new Exception("SkyPaletteType: " + skyPaletteEntry.SkyPaletteType.ToString() + " already found in SkyPaletteLibrary.  Duplicates not allowed.");
			}
			m_skyPaletteDict.Add(skyPaletteEntry.SkyPaletteType, skyPaletteEntry);
		}
	}
}
