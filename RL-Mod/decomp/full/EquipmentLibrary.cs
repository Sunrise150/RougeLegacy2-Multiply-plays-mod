using System;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Equipment Library")]
public class EquipmentLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/EquipmentLibrary";

	[Space(10f)]
	[SerializeField]
	private EquipmentTypeEquipmentDataDictionary m_weaponEquipmentLibrary;

	[Space(10f)]
	[SerializeField]
	private EquipmentTypeEquipmentDataDictionary m_headEquipmentLibrary;

	[Space(10f)]
	[SerializeField]
	private EquipmentTypeEquipmentDataDictionary m_chestEquipmentLibrary;

	[Space(10f)]
	[SerializeField]
	private EquipmentTypeEquipmentDataDictionary m_capeEquipmentLibrary;

	[Space(10f)]
	[SerializeField]
	private EquipmentTypeEquipmentDataDictionary m_trinketEquipmentLibrary;

	private static EquipmentLibrary m_instance;

	private static EquipmentLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<EquipmentLibrary>("Scriptable Objects/Libraries/EquipmentLibrary");
			}
			return m_instance;
		}
	}

	public static EquipmentData GetEquipmentData(EquipmentCategoryType categoryType, EquipmentType equipType)
	{
		if (equipType == EquipmentType.None || categoryType == EquipmentCategoryType.None)
		{
			return null;
		}
		EquipmentTypeEquipmentDataDictionary equipmentTypeEquipmentDataDictionary = null;
		switch (categoryType)
		{
		case EquipmentCategoryType.Weapon:
			equipmentTypeEquipmentDataDictionary = Instance.m_weaponEquipmentLibrary;
			break;
		case EquipmentCategoryType.Head:
			equipmentTypeEquipmentDataDictionary = Instance.m_headEquipmentLibrary;
			break;
		case EquipmentCategoryType.Chest:
			equipmentTypeEquipmentDataDictionary = Instance.m_chestEquipmentLibrary;
			break;
		case EquipmentCategoryType.Cape:
			equipmentTypeEquipmentDataDictionary = Instance.m_capeEquipmentLibrary;
			break;
		case EquipmentCategoryType.Trinket:
			equipmentTypeEquipmentDataDictionary = Instance.m_trinketEquipmentLibrary;
			break;
		}
		EquipmentData value = null;
		if (equipmentTypeEquipmentDataDictionary != null)
		{
			equipmentTypeEquipmentDataDictionary.TryGetValue(equipType, out value);
			return value;
		}
		throw new Exception("Equipment Library is null.");
	}
}
