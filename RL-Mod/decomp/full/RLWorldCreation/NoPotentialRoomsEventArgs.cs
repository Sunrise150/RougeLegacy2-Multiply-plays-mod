using System.Collections.Generic;
using UnityEngine;

namespace RLWorldCreation;

public class NoPotentialRoomsEventArgs : BiomeCreationEventArgs
{
	public GridPointManager OriginRoom { get; }

	public DoorLocation DoorLocation { get; }

	public Dictionary<Vector2Int, List<DoorLocation>> RoomSizesThatFit { get; }

	public NoPotentialRoomsEventArgs(GridPointManager originRoom, DoorLocation doorLocation, Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit)
	{
		OriginRoom = originRoom;
		DoorLocation = doorLocation;
		RoomSizesThatFit = roomSizesThatFit;
	}
}
