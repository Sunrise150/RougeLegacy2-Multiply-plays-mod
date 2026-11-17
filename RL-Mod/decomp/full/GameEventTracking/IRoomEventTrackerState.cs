using System.Collections.Generic;

namespace GameEventTracking;

public interface IRoomEventTrackerState : IGameEventTrackerState
{
	List<RoomTrackerData> RoomsEntered { get; }

	void Initialise(List<RoomTrackerData> roomsEntered);
}
