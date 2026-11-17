using System.Collections.Generic;
using System.Linq;
using Rooms;

public static class FindSpaceForRoomUtility
{
	public static List<GridPoint> GetGridPointsOnBorderWhereRoomFits(BiomeController biomeController, RoomMetaData room, bool isMirrored, DoorLocation attachAtDoorLocation, bool checkForAvailableDoors = true)
	{
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(attachAtDoorLocation.RoomSide);
		List<GridPoint> gridPoints = GridPointOperationsUtility.GetGridPointsOnBorder(biomeController, oppositeSide).ToList();
		List<GridPoint> list = null;
		return GridPointOperationsUtility.GetGridPointsWhereRoomFits(gridPoints: (!checkForAvailableDoors) ? GridPointOperationsUtility.GetGridPointsWithSpaceOnSide(biomeController, oppositeSide, gridPoints).ToList() : GridPointOperationsUtility.GetGridPointsWithAvailableDoorsOnSide(biomeController, oppositeSide, gridPoints).ToList(), biomeController: biomeController, doorLocationToCheck: attachAtDoorLocation, room: room, isMirrored: isMirrored, checkForWayOut: false).ToList();
	}

	public static List<GridPoint> GetGridPointsWhereRoomFits(BiomeController biomeController, RoomMetaData room, bool isMirrored, DoorLocation attachAtDoorLocation, bool checkForAvailableDoors = true, RoomType roomType = RoomType.None)
	{
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(attachAtDoorLocation.RoomSide);
		List<GridPoint> list = null;
		list = ((!checkForAvailableDoors) ? GridPointOperationsUtility.GetGridPointsWithSpaceOnSide(biomeController, oppositeSide).ToList() : GridPointOperationsUtility.GetGridPointsWithAvailableDoorsOnSide(biomeController, oppositeSide).ToList());
		if (roomType != RoomType.None)
		{
			list = list.Where((GridPoint entry) => entry.RoomType == roomType).ToList();
		}
		return GridPointOperationsUtility.GetGridPointsWhereRoomFits(biomeController, attachAtDoorLocation, room, isMirrored, checkForWayOut: false, list).ToList();
	}

	public static List<GridPoint> GetGridPointsGivenDistanceFromBorderWhereRoomFits(BiomeController biomeController, RoomMetaData room, bool isMirrored, DoorLocation attachAtDoorLocation, int distance)
	{
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(attachAtDoorLocation.RoomSide);
		List<GridPoint> gridPoints = GridPointOperationsUtility.GetGridPointsGivenDistanceFromBorder(biomeController, oppositeSide, distance).ToList();
		List<GridPoint> gridPoints2 = GridPointOperationsUtility.GetGridPointsWithAvailableDoorsOnSide(biomeController, oppositeSide, gridPoints).ToList();
		return GridPointOperationsUtility.GetGridPointsWhereRoomFits(biomeController, attachAtDoorLocation, room, isMirrored, checkForWayOut: false, gridPoints2).ToList();
	}
}
