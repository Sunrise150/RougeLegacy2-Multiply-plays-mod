using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class HazardRandomizer
{
	private static Dictionary<HazardCategory, List<HazardType>> m_hazardTypeTable = new Dictionary<HazardCategory, List<HazardType>>();

	private static HazardType GetRandomHazardType(HazardCategory category)
	{
		HazardType result = HazardType.None;
		if (!m_hazardTypeTable.ContainsKey(category))
		{
			m_hazardTypeTable.Add(category, (from entry in HazardLibrary.GetHazardTypesInHazardCategory(category)
				where entry != HazardType.BiomeSpecific
				select entry).ToList());
		}
		else if (m_hazardTypeTable[category].Count == 0)
		{
			m_hazardTypeTable[category] = (from entry in HazardLibrary.GetHazardTypesInHazardCategory(category)
				where entry != HazardType.BiomeSpecific
				select entry).ToList();
		}
		if (m_hazardTypeTable[category].Count > 0)
		{
			int index = 0;
			if (m_hazardTypeTable[category].Count > 1)
			{
				index = Random.Range(0, m_hazardTypeTable[category].Count);
			}
			result = m_hazardTypeTable[category][index];
			m_hazardTypeTable[category].RemoveAt(index);
		}
		return result;
	}

	public static void RandomizeHazards(IEnumerable<IHazardSpawnController> spawnControllers)
	{
		IEnumerable<IHazardSpawnController> spawnControllers2 = spawnControllers.Where((IHazardSpawnController entry) => entry is PointHazardSpawnController);
		IEnumerable<IHazardSpawnController> spawnControllers3 = spawnControllers.Where((IHazardSpawnController entry) => entry is LineHazardSpawnController);
		IEnumerable<IHazardSpawnController> spawnControllers4 = spawnControllers.Where((IHazardSpawnController entry) => entry is TurretHazardSpawnController);
		SetRandomHazardType(HazardCategory.Point, spawnControllers2);
		SetRandomHazardType(HazardCategory.Line, spawnControllers3);
		SetRandomHazardType(HazardCategory.Turret, spawnControllers4);
	}

	private static void SetRandomHazardType(HazardCategory category, IEnumerable<IHazardSpawnController> spawnControllers)
	{
		if (spawnControllers.Count() == 0)
		{
			return;
		}
		HazardType randomHazardType = GetRandomHazardType(category);
		if (randomHazardType == HazardType.None)
		{
			return;
		}
		foreach (IHazardSpawnController spawnController in spawnControllers)
		{
			spawnController.SetType(randomHazardType);
		}
	}
}
