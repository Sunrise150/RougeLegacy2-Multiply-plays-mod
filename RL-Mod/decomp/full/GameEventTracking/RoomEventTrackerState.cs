using System;
using System.Collections.Generic;

namespace GameEventTracking;

[Serializable]
public class RoomEventTrackerState : IRoomEventTrackerState, IGameEventTrackerState
{
	private List<RoomTrackerData> m_roomsEntered;

	public List<RoomTrackerData> RoomsEntered
	{
		get
		{
			return m_roomsEntered;
		}
		private set
		{
			m_roomsEntered = value;
		}
	}

	public RoomEventTrackerState(List<RoomTrackerData> roomsEntered)
	{
		Initialise(roomsEntered);
	}

	public void Initialise(List<RoomTrackerData> roomsEntered)
	{
		RoomsEntered = roomsEntered;
	}
}
