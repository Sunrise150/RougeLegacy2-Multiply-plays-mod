using System;
using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

public static class GridPointOperationsUtility
{
	public static IEnumerable<GridPoint> GetGridPointsWithAvailableDoorsOnSide(BiomeController biomeController, RoomSide side)
	{
		return GetGridPointsWithAvailableDoorsOnSide(biomeController, side, biomeController.GridPointManager.GridPoints);
	}

	public static IEnumerable<GridPoint> GetGridPointsWithAvailableDoorsOnSide(BiomeController biomeController, RoomSide side, IEnumerable<GridPoint> gridPoints)
	{
		GridPoint[] array = gridPoints.Where((GridPoint gridPoint) => gridPoint.GetDoorLocation(side) != DoorLocation.Empty).ToArray();
		List<GridPoint> list = new List<GridPoint>();
		GridPoint[] array2 = array;
		foreach (GridPoint gridPoint2 in array2)
		{
			DoorLocation doorLocation = gridPoint2.GetDoorLocation(side);
			if (GridController.GetIsGridSpaceAvailable(gridPoint2.Owner.GetDoorLeadsToGridCoordinates(doorLocation)))
			{
				list.Add(gridPoint2);
			}
		}
		return list;
	}

	public static IEnumerable<GridPoint> GetGridPointsOnBorder(BiomeController biomeController, RoomSide side)
	{
		List<GridPoint> list = new List<GridPoint>();
		int num = biomeController.GridPointManager.Extents[side];
		foreach (GridPoint gridPoint in biomeController.GridPointManager.GridPoints)
		{
			bool flag = false;
			switch (side)
			{
			case RoomSide.Top:
			case RoomSide.Bottom:
				if (side == RoomSide.Top)
				{
					if (gridPoint.GridCoordinates.y == num - 1)
					{
						flag = true;
					}
				}
				else if (gridPoint.GridCoordinates.y == num)
				{
					flag = true;
				}
				break;
			case RoomSide.Right:
				if (gridPoint.GridCoordinates.x == num - 1)
				{
					flag = true;
				}
				break;
			default:
				if (gridPoint.GridCoordinates.x == num)
				{
					flag = true;
				}
				break;
			}
			if (flag)
			{
				list.Add(gridPoint);
			}
		}
		return list;
	}

	public static IEnumerable<GridPoint> GetGridPointsOnSideOfBiomeTransitionRoom(BiomeController biomeController, RoomSide side)
	{
		Vector2Int transitionRoomSize = RoomLibrary.GetSetCollection(biomeController.Biome).TransitionRoom.Size;
		Func<GridPoint, bool> func = null;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			func = (GridPoint entry) => entry.GridCoordinates.x <= biomeController.GridPointManager.Center.x;
			if (side == RoomSide.Right)
			{
				func = (GridPoint entry) => entry.GridCoordinates.x >= biomeController.GridPointManager.Center.x + transitionRoomSize.x - 1;
			}
		}
		else
		{
			func = (GridPoint entry) => entry.GridCoordinates.y <= biomeController.GridPointManager.Center.y;
			if (side == RoomSide.Top)
			{
				func = (GridPoint entry) => entry.GridCoordinates.y >= biomeController.GridPointManager.Center.y + transitionRoomSize.y - 1;
			}
		}
		return biomeController.GridPointManager.GridPoints.Where(func);
	}

	public static IEnumerable<GridPoint> GetGridPointsWithSpaceOnSide(BiomeController biomeController, RoomSide side, IEnumerable<GridPoint> gridPoints)
	{
		int maxExtents = GridController.Extents[side];
		foreach (GridPoint gridPoint in gridPoints)
		{
			Vector2Int gridCoordsOnGridPointSide = GridController.GetGridCoordsOnGridPointSide(gridPoint, side);
			Vector2Int roomSize = Vector2Int.zero;
			switch (side)
			{
			case RoomSide.Left:
				roomSize = new Vector2Int(gridCoordsOnGridPointSide.x + 1 - maxExtents, 1);
				break;
			case RoomSide.Right:
				roomSize = new Vector2Int(maxExtents - gridCoordsOnGridPointSide.x, 1);
				break;
			case RoomSide.Top:
				roomSize = new Vector2Int(1, maxExtents - gridCoordsOnGridPointSide.y);
				break;
			case RoomSide.Bottom:
				roomSize = new Vector2Int(1, gridCoordsOnGridPointSide.y + 1 - maxExtents);
				break;
			}
			if (GridController.GetIsSpaceForRoomAtDoor(gridCoordsOnGridPointSide, roomSize, side, 0))
			{
				yield return gridPoint;
			}
		}
	}

	public static IEnumerable<GridPoint> GetGridPointsWithSpaceOnSide(BiomeController biomeController, RoomSide side)
	{
		return GetGridPointsWithSpaceOnSide(biomeController, side, biomeController.GridPointManager.GridPoints);
	}

	public static IEnumerable<GridPoint> GetGridPointsGivenDistanceFromBorder(BiomeController biomeController, RoomSide side, int distance)
	{
		return GetGridPointsGivenDistanceFromBorder(biomeController, side, distance, biomeController.GridPointManager.GridPoints);
	}

	public static IEnumerable<GridPoint> GetGridPointsGivenDistanceFromBorder(BiomeController biomeController, RoomSide side, int distance, IEnumerable<GridPoint> gridPoints)
	{
		List<GridPoint> list = new List<GridPoint>();
		int num = biomeController.GridPointManager.Extents[side];
		foreach (GridPoint gridPoint in gridPoints)
		{
			bool flag = false;
			switch (side)
			{
			case RoomSide.Left:
			case RoomSide.Right:
				if (side == RoomSide.Left)
				{
					if (gridPoint.GridCoordinates.x - num == distance)
					{
						flag = true;
					}
				}
				else if (num - (gridPoint.GridCoordinates.x + 1) == distance)
				{
					flag = true;
				}
				break;
			case RoomSide.Bottom:
				if (num - gridPoint.GridCoordinates.y == distance)
				{
					flag = true;
				}
				break;
			default:
				if (num - (gridPoint.GridCoordinates.y + 1) == distance)
				{
					flag = true;
				}
				break;
			}
			if (flag)
			{
				list.Add(gridPoint);
			}
		}
		return list;
	}

	public static IEnumerable<GridPoint> GetGridPointsWhereRoomFits(BiomeController biomeController, DoorLocation doorLocationToCheck, RoomMetaData room, bool isMirrored, bool checkForWayOut, IEnumerable<GridPoint> gridPoints)
	{
		List<GridPoint> list = new List<GridPoint>();
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(doorLocationToCheck.RoomSide);
		IEnumerable<DoorLocation> doorLocationsInRoom = room.DoorLocations;
		if (isMirrored)
		{
			doorLocationsInRoom = RoomUtility.GetMirroredDoorLocations(room);
		}
		foreach (GridPoint gridPoint in gridPoints)
		{
			Vector2Int gridCoordsOnGridPointSide = GridController.GetGridCoordsOnGridPointSide(gridPoint, oppositeSide);
			Vector2Int size = room.Size;
			if (GridController.GetIsSpaceForRoomAtDoor(gridCoordsOnGridPointSide, size, oppositeSide, doorLocationToCheck.DoorNumber))
			{
				Vector2Int roomGridCoordinates = GridController.GetRoomGridCoordinates(gridCoordsOnGridPointSide, size, doorLocationToCheck);
				bool flag = true;
				if (checkForWayOut)
				{
					flag = GetIsWayOutOfRoom(roomGridCoordinates, size, doorLocationsInRoom);
				}
				if (flag)
				{
					list.Add(gridPoint);
				}
			}
		}
		return list;
	}

	public static IEnumerable<GridPoint> GetGridPointsWhereRoomFits(BiomeController biomeController, DoorLocation doorLocationToCheck, RoomMetaData room, bool isMirrored, bool checkForWayOut)
	{
		return GetGridPointsWhereRoomFits(biomeController, doorLocationToCheck, room, isMirrored, checkForWayOut, biomeController.GridPointManager.GridPoints);
	}

	public static bool GetIsWayOutOfRoom(Vector2Int potentialGridCoordinates, Vector2Int roomSize, IEnumerable<DoorLocation> doorLocationsInRoom)
	{
		bool result = false;
		foreach (DoorLocation item in doorLocationsInRoom)
		{
			if (GridController.GetIsGridSpaceAvailable(GridController.GetDoorLeadsToGridCoordinates(potentialGridCoordinates, roomSize, item)))
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public static GridPoint GetCentermostGridPoint(BiomeController biomeController, RoomSide side, IEnumerable<GridPoint> gridPoints)
	{
		RoomSide key = RoomSide.Top;
		RoomSide key2 = RoomSide.Bottom;
		if (side == RoomSide.Top || side == RoomSide.Bottom)
		{
			key = RoomSide.Left;
			key2 = RoomSide.Right;
		}
		int num = biomeController.GridPointManager.Extents[key];
		int num2 = biomeController.GridPointManager.Extents[key2];
		int midpoint = num2 - (num2 - num) / 2;
		Func<GridPoint, int> keySelector = (GridPoint entry) => Mathf.Abs(entry.GridCoordinates.y - midpoint);
		if (side == RoomSide.Top || side == RoomSide.Bottom)
		{
			keySelector = (GridPoint entry) => Mathf.Abs(entry.GridCoordinates.x - midpoint);
		}
		return gridPoints.OrderBy(keySelector).First();
	}
}
