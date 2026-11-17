using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public struct BlueprintTrackerData : ISpecialItemData, IGameEventData
{
	private EquipmentCategoryType m_equipmentCategory;

	private EquipmentType m_equipmentType;

	private float m_timeStamp;

	private int m_timesLoaded;

	public EquipmentCategoryType EquipmentCategory => m_equipmentCategory;

	public EquipmentType EquipmentType => m_equipmentType;

	public float TimeStamp => m_timeStamp;

	public int TimesLoaded => m_timesLoaded;

	public BlueprintTrackerData(EquipmentCategoryType equipmentCategory, EquipmentType equipmentType)
	{
		m_equipmentCategory = equipmentCategory;
		m_equipmentType = equipmentType;
		m_timeStamp = Time.frameCount;
		m_timesLoaded = SaveManager.StageSaveData.TimesTrackerWasLoaded;
	}
}
