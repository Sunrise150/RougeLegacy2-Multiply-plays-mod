using System;
using UnityEngine;

namespace GameEventTracking;

[Serializable]
public struct RoomTrackerData : IGameEventData
{
	private BiomeType m_biome;

	private int m_biomeControllerIndex;

	private float m_timeStamp;

	private int m_timesLoaded;

	private float m_worldPositionX;

	private float m_worldPositionY;

	private bool m_viaBiomeTransitionDoor;

	public BiomeType Biome => m_biome;

	public int BiomeControllerIndex => m_biomeControllerIndex;

	public float TimeStamp => m_timeStamp;

	public int TimesLoaded => m_timesLoaded;

	public Vector2 WorldPosition => new Vector2(m_worldPositionX, m_worldPositionY);

	public bool ViaBiomeTransitionDoor => m_viaBiomeTransitionDoor;

	public RoomTrackerData(BiomeType biome, int biomeControllerIndex, Vector2 worldPosition = default(Vector2), bool viaBiomeTransitionDoor = false)
	{
		m_timeStamp = Time.frameCount;
		m_biome = biome;
		m_biomeControllerIndex = biomeControllerIndex;
		m_timesLoaded = SaveManager.StageSaveData.TimesTrackerWasLoaded;
		m_worldPositionX = worldPosition.x;
		m_worldPositionY = worldPosition.y;
		m_viaBiomeTransitionDoor = viaBiomeTransitionDoor;
	}
}
