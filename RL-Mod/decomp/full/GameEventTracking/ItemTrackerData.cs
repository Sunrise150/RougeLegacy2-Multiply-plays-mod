using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public struct ItemTrackerData : IGameEventData
{
	private float m_timeStamp;

	private float m_value;

	private int m_timesLoaded;

	private ItemDropType m_itemDropType;

	public float TimeStamp => m_timeStamp;

	public float Value => m_value;

	public int TimesLoaded => m_timesLoaded;

	public ItemDropType ItemDropType => m_itemDropType;

	public ItemTrackerData(float value, ItemDropType itemDropType)
	{
		m_value = value;
		m_itemDropType = itemDropType;
		m_timeStamp = Time.frameCount;
		m_timesLoaded = SaveManager.StageSaveData.TimesTrackerWasLoaded;
	}
}
