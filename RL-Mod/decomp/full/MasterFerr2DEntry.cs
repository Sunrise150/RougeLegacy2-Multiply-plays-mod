using System;
using UnityEngine;

[Serializable]
public class MasterFerr2DEntry
{
	[SerializeField]
	private Ferr2DT_PathTerrain m_master;

	[SerializeField]
	private Ferr2DSettings m_ferr2DSettings;

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

	public Ferr2DSettings Ferr2DSettings
	{
		get
		{
			return m_ferr2DSettings;
		}
		private set
		{
			m_ferr2DSettings = value;
		}
	}

	public void SetMaster(Ferr2DT_PathTerrain masterPrefab)
	{
		Master = masterPrefab;
		if (Master != null)
		{
			if (Ferr2DSettings == null)
			{
				Ferr2DSettings = new Ferr2DSettings();
			}
			Ferr2DSettings.Update(masterPrefab);
		}
		else
		{
			Ferr2DSettings = null;
		}
	}
}
