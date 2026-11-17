using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Hazard Library")]
public class HazardLibrary : ScriptableObject
{
	[SerializeField]
	private HazardLibraryEntry[] m_hazardPrefabs;

	[Header("Point Hazards")]
	[SerializeField]
	private Hazards m_pointHazards;

	[Header("Line Hazards")]
	[SerializeField]
	private Hazards m_lineHazards;

	[Header("Turret Hazards")]
	[SerializeField]
	private Hazards m_turretHazards;

	[Header("Turret Hazards")]
	[SerializeField]
	private Hazards m_spikeHazards;

	private static HazardLibrary m_instance = null;

	private static Dictionary<HazardType, Hazard> m_hazardTable = null;

	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/HazardLibrary";

	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Libraries/HazardLibrary.asset";

	private static HazardType[] LINE_HAZARDS = new HazardType[11]
	{
		HazardType.None,
		HazardType.BiomeSpecific,
		HazardType.SpikeTrap,
		HazardType.Buzzsaw,
		HazardType.BreakableSpike,
		HazardType.PressurePlate,
		HazardType.SpringTrap,
		HazardType.Conveyor,
		HazardType.SnowMound,
		HazardType.Bodies,
		HazardType.BreakableSpikeTall
	};

	private static HazardType[] TURRET_HAZARDS = new HazardType[6]
	{
		HazardType.None,
		HazardType.BiomeSpecific,
		HazardType.WallTurret_Standard,
		HazardType.WallTurret_FlameThrower,
		HazardType.RaycastTurret_Arrow,
		HazardType.RaycastTurret_Curse
	};

	private static HazardType[] POINT_HAZARDS = new HazardType[15]
	{
		HazardType.None,
		HazardType.BiomeSpecific,
		HazardType.Orbiter,
		HazardType.Triple_Orbiter,
		HazardType.Windmill,
		HazardType.ShrinkOnDash,
		HazardType.Sentry,
		HazardType.ProximityMine,
		HazardType.VoidTrap,
		HazardType.ExhaustPoint,
		HazardType.HomingVine,
		HazardType.ProximityProjectile,
		HazardType.IceCrystal,
		HazardType.SentryWithIce,
		HazardType.RisingWater
	};

	private static HazardType[] FERR2D_HAZARDS = new HazardType[3]
	{
		HazardType.None,
		HazardType.BiomeSpecific,
		HazardType.TallSpike
	};

	public static HazardLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<HazardLibrary>("Scriptable Objects/Libraries/HazardLibrary");
			}
			return m_instance;
		}
	}

	public static HazardLibraryEntry[] HazardPrefabs
	{
		get
		{
			return Instance.m_hazardPrefabs;
		}
		set
		{
			Instance.m_hazardPrefabs = value;
		}
	}

	public static Hazards GetHazards(HazardCategory hazardCategory)
	{
		switch (hazardCategory)
		{
		case HazardCategory.Point:
			return Instance.m_pointHazards;
		case HazardCategory.Line:
			return Instance.m_lineHazards;
		case HazardCategory.Turret:
			return Instance.m_turretHazards;
		case HazardCategory.Ferr2D:
			return Instance.m_spikeHazards;
		default:
			Debug.LogFormat("<color=red>| HazardLibrary | No entry found matching given Hazard category ({0})", hazardCategory);
			return null;
		}
	}

	public static Dictionary<BiomeType, HazardType[]> GetHazardsInBiomeTable(HazardCategory hazardCategory)
	{
		switch (hazardCategory)
		{
		case HazardCategory.Point:
			return Instance.m_pointHazards.HazardsInBiomeTable;
		case HazardCategory.Line:
			return Instance.m_lineHazards.HazardsInBiomeTable;
		case HazardCategory.Turret:
			return Instance.m_turretHazards.HazardsInBiomeTable;
		case HazardCategory.Ferr2D:
			return Instance.m_spikeHazards.HazardsInBiomeTable;
		default:
			Debug.LogFormat("<color=red>| HazardLibrary | No entry found matching given Hazard category ({0})", hazardCategory);
			return null;
		}
	}

	public static HazardType[] GetDefaultHazardTypes(HazardCategory hazardCategory)
	{
		switch (hazardCategory)
		{
		case HazardCategory.Point:
			return Instance.m_pointHazards.DefaultHazardsInBiomes;
		case HazardCategory.Line:
			return Instance.m_lineHazards.DefaultHazardsInBiomes;
		case HazardCategory.Turret:
			return Instance.m_turretHazards.DefaultHazardsInBiomes;
		case HazardCategory.Ferr2D:
			return Instance.m_spikeHazards.DefaultHazardsInBiomes;
		default:
			Debug.LogFormat("<color=red>| HazardLibrary | No entry found matching given Hazard category ({0})", hazardCategory);
			return null;
		}
	}

	public static int GetHazardTypeIndexInHazardCategory(HazardType hazardType, HazardCategory hazardCategory)
	{
		HazardType[] hazardTypesInHazardCategory = GetHazardTypesInHazardCategory(hazardCategory);
		for (int i = 0; i < hazardTypesInHazardCategory.Length; i++)
		{
			if (hazardTypesInHazardCategory[i] == hazardType)
			{
				return i;
			}
		}
		return -1;
	}

	public static HazardType[] GetHazardTypesInHazardCategory(HazardCategory hazardCategory)
	{
		switch (hazardCategory)
		{
		case HazardCategory.Point:
			return POINT_HAZARDS;
		case HazardCategory.Line:
			return LINE_HAZARDS;
		case HazardCategory.Turret:
			return TURRET_HAZARDS;
		case HazardCategory.Ferr2D:
			return FERR2D_HAZARDS;
		default:
			Debug.LogFormat("<color=red>| HazardLibrary | No entry for Hazard Category ({0})</color>", hazardCategory);
			return new HazardType[0];
		}
	}

	public static Hazard GetPrefab(HazardType hazardType)
	{
		if (m_hazardTable == null)
		{
			m_hazardTable = new Dictionary<HazardType, Hazard>();
			HazardLibraryEntry[] hazardPrefabs = HazardPrefabs;
			foreach (HazardLibraryEntry hazardLibraryEntry in hazardPrefabs)
			{
				m_hazardTable.Add(hazardLibraryEntry.Hazard, hazardLibraryEntry.Prefab);
			}
		}
		if (m_hazardTable.ContainsKey(hazardType))
		{
			return m_hazardTable[hazardType];
		}
		Debug.LogFormat("<color=red>| HazardLibrary | Hazard Table does not contain an entry for Hazard Type ({0})</color>", hazardType);
		return null;
	}
}
