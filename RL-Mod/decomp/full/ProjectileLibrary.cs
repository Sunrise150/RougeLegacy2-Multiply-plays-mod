using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Projectile Library")]
public class ProjectileLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ProjectileLibrary";

	[SerializeField]
	private Sprite m_defaultOffscreenSprite;

	[SerializeField]
	private Sprite m_voidOffscreenSprite;

	[SerializeField]
	private Sprite m_curseOffscreenSprite;

	[SerializeField]
	private Sprite m_energyOffscreenSprite;

	[SerializeField]
	private ProjectileBuffColourEntry[] m_buffColorArray;

	[SerializeField]
	private ProjectileEntry[] m_projectileEntryArray;

	private static ProjectileLibrary m_instance;

	public static Sprite DefaultOffscreenSprite => Instance.m_defaultOffscreenSprite;

	public static Sprite VoidOffscreenSprite => Instance.m_voidOffscreenSprite;

	public static Sprite EnergyOffscreenSprite => Instance.m_energyOffscreenSprite;

	public static Sprite CurseOffscreenSprite => Instance.m_curseOffscreenSprite;

	public static ProjectileEntry[] ProjectileEntryArray
	{
		get
		{
			return Instance.m_projectileEntryArray;
		}
		set
		{
			Instance.m_projectileEntryArray = value;
		}
	}

	public static ProjectileBuffColourEntry[] ProjectileBuffColourArray
	{
		get
		{
			return Instance.m_buffColorArray;
		}
		set
		{
			Instance.m_buffColorArray = value;
		}
	}

	public static ProjectileLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ProjectileLibrary>("Scriptable Objects/Libraries/ProjectileLibrary");
				if (Application.isPlaying)
				{
					ProjectileEntry[] projectileEntryArray = m_instance.m_projectileEntryArray;
					for (int i = 0; i < projectileEntryArray.Length; i++)
					{
						if (projectileEntryArray[i].ProjectilePrefab == null)
						{
							throw new Exception("Null projectile found in projectile library.");
						}
					}
				}
			}
			return m_instance;
		}
	}

	public static Projectile_RL GetProjectile(string projectileName)
	{
		ProjectileEntry[] projectileEntryArray = ProjectileEntryArray;
		foreach (ProjectileEntry projectileEntry in projectileEntryArray)
		{
			if (projectileEntry.ProjectilePrefab.name.Equals(projectileName, StringComparison.OrdinalIgnoreCase))
			{
				return projectileEntry.ProjectilePrefab;
			}
		}
		return null;
	}

	public static ProjectileEntry GetProjectileEntry(string projectileName)
	{
		ProjectileEntry[] projectileEntryArray = ProjectileEntryArray;
		foreach (ProjectileEntry projectileEntry in projectileEntryArray)
		{
			if (projectileEntry.ProjectilePrefab.name.Equals(projectileName, StringComparison.OrdinalIgnoreCase))
			{
				return projectileEntry;
			}
		}
		return null;
	}

	public static Color GetBuffColor(ProjectileBuffType buffType)
	{
		for (int i = 0; i < m_instance.m_buffColorArray.Length; i++)
		{
			if (m_instance.m_buffColorArray[i].ProjectileBuffType == buffType)
			{
				return m_instance.m_buffColorArray[i].Color;
			}
		}
		return Color.white;
	}

	public static byte GetBuffPriority(ProjectileBuffType buffType)
	{
		ProjectileBuffColourEntry[] buffColorArray = m_instance.m_buffColorArray;
		foreach (ProjectileBuffColourEntry projectileBuffColourEntry in buffColorArray)
		{
			if (projectileBuffColourEntry.ProjectileBuffType == buffType)
			{
				return projectileBuffColourEntry.Priority;
			}
		}
		return 0;
	}
}
