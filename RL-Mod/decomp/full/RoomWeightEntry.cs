using System;
using UnityEngine;

[Serializable]
public struct RoomWeightEntry
{
	[SerializeField]
	private RoomID m_roomId;

	[SerializeField]
	private int m_weight;

	[SerializeField]
	private bool m_isMirrored;

	public const int DEFAULT_ROOM_WEIGHT = 100;

	public RoomID RoomID => m_roomId;

	public int RNGWeight => m_weight;

	public bool IsMirrored => m_isMirrored;

	public RoomWeightEntry(RoomID roomID, bool isMirrored)
		: this(roomID, isMirrored, 100)
	{
	}

	public RoomWeightEntry(RoomID roomID, bool isMirrored, int weight)
	{
		m_roomId = roomID;
		m_weight = weight;
		m_isMirrored = isMirrored;
	}

	public void SetWeight(int weight)
	{
		m_weight = weight;
	}
}
