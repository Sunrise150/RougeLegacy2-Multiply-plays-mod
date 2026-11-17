using System;
using Rooms;
using UnityEngine;

[Serializable]
public class RoomReferenceEntry
{
	[SerializeField]
	private RoomReferenceType m_roomReferenceType;

	[SerializeField]
	private RoomMetaData m_roomMetaData;

	[SerializeField]
	private string m_roomMetaDataPath;

	public RoomReferenceType RoomReferenceType
	{
		get
		{
			return m_roomReferenceType;
		}
		set
		{
			m_roomReferenceType = value;
		}
	}

	public RoomMetaData RoomMetaData
	{
		get
		{
			return m_roomMetaData;
		}
		set
		{
			m_roomMetaData = value;
		}
	}

	public string RoomMetaDataPath => m_roomMetaDataPath;
}
