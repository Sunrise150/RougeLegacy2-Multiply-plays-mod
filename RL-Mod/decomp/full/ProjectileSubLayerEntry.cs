using System;
using UnityEngine;

[Serializable]
public class ProjectileSubLayerEntry
{
	public string Name;

	public int SubLayer;

	public ProjectileSubLayerEntry(Projectile_RL prefab, int subLayer)
	{
		string name = "";
		if (prefab != null)
		{
			if (prefab.ProjectileData != null)
			{
				name = prefab.name;
			}
			else
			{
				Debug.LogFormat("<color=purple>Projectile Prefab ({0}) has no Projectile Data set.</color>", prefab);
			}
		}
		else
		{
			Debug.LogFormat("<color=purple>ProjectileSubLayerEntry Constructor was passed a null prefab argument</color>");
		}
		Name = name;
		SubLayer = subLayer;
	}
}
