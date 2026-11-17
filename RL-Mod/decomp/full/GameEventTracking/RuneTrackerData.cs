using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public struct RuneTrackerData : ISpecialItemData, IGameEventData
{
	private RuneType m_runeType;

	private float m_timeStamp;

	private int m_timesLoaded;

	public RuneType RuneType => m_runeType;

	public float TimeStamp => m_timeStamp;

	public int TimesLoaded => m_timesLoaded;

	public RuneTrackerData(RuneType runeType)
	{
		m_runeType = runeType;
		m_timesLoaded = SaveManager.StageSaveData.TimesTrackerWasLoaded;
		m_timeStamp = Time.frameCount;
	}
}
