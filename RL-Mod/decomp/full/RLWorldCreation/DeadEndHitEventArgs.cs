using System.Collections.Generic;

namespace RLWorldCreation;

public class DeadEndHitEventArgs : BiomeCreationEventArgs
{
	public int RoomNumber { get; }

	public int StartLookingIndex { get; }

	public int StopLookingIndex { get; }

	public IEnumerable<GridPointManager> CheckedRooms { get; }

	public GridPointManager ChosenRoom { get; }

	public DeadEndHitEventArgs(int roomNumber, int startLookingIndex, int stopLookingIndex, IEnumerable<GridPointManager> checkedRooms, GridPointManager chosenRoom)
	{
		RoomNumber = roomNumber;
		StartLookingIndex = startLookingIndex;
		StopLookingIndex = stopLookingIndex;
		CheckedRooms = checkedRooms;
		ChosenRoom = chosenRoom;
	}
}
