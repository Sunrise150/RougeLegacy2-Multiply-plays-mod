using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GridController
{
	public const int GRID_SIZE = 1000;

	private static Dictionary<RoomSide, int> m_extents = new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			0
		},
		{
			RoomSide.Right,
			0
		},
		{
			RoomSide.Top,
			0
		},
		{
			RoomSide.Bottom,
			0
		}
	};

	private static GridPoint[,] m_gridPoints = null;

	private static int m_gridOffset = -1;

	private static GridController m_instance = null;

	private static HashSet<Vector2Int> m_occupiedGridPointCoords = new HashSet<Vector2Int>();

	public static Dictionary<RoomSide, int> Extents
	{
		get
		{
			return m_extents;
		}
		private set
		{
			m_extents = value;
		}
	}

	public static GridPoint[,] GridPoints
	{
		get
		{
			return m_gridPoints;
		}
		private set
		{
			m_gridPoints = value;
		}
	}

	public static bool IsInstantiated => m_instance != null;

	public GridController()
	{
		m_instance = this;
		if (GridPoints == null)
		{
			GridPoints = new GridPoint[1000, 1000];
		}
	}

	public static void AddRoomToGrid(GridPoint gridPoint)
	{
		if (m_gridOffset == -1)
		{
			m_gridOffset = 500;
		}
		int num = gridPoint.GridCoordinates.x + m_gridOffset;
		int num2 = gridPoint.GridCoordinates.y + m_gridOffset;
		try
		{
			GridPoints[num, num2] = gridPoint;
		}
		catch (Exception ex)
		{
			if (ex is IndexOutOfRangeException)
			{
				Debug.LogFormat("<color=red>| GridController | The GRID_SIZE value ({0}) is too small. Set it to a larger value.</color>", 1000);
			}
			throw;
		}
		m_occupiedGridPointCoords.Add(gridPoint.GridCoordinates);
		UpdateExtentAfterAdd(gridPoint);
	}

	public void ClearGridPoint(GridPoint gridPoint)
	{
		if (m_gridOffset == -1)
		{
			m_gridOffset = 500;
		}
		for (int i = 0; i < gridPoint.RoomMetaData.Size.x; i++)
		{
			for (int j = 0; j < gridPoint.RoomMetaData.Size.y; j++)
			{
				GridPoints[gridPoint.GridCoordinates.x + i + m_gridOffset, gridPoint.GridCoordinates.y + j + m_gridOffset] = null;
			}
		}
		m_occupiedGridPointCoords.Remove(gridPoint.GridCoordinates);
	}

	public static Vector2Int GetDoorLeadsToGridCoordinates(Vector2Int roomCoordinates, Vector2Int roomSize, DoorLocation location)
	{
		int x = -1;
		int y = -1;
		if (location.RoomSide == RoomSide.Left || location.RoomSide == RoomSide.Right)
		{
			x = ((location.RoomSide != RoomSide.Left) ? (roomCoordinates.x + roomSize.x) : (roomCoordinates.x - 1));
			y = roomCoordinates.y + roomSize.y - 1 - location.DoorNumber;
		}
		else if (location.RoomSide == RoomSide.Top || location.RoomSide == RoomSide.Bottom)
		{
			x = roomCoordinates.x + location.DoorNumber;
			y = roomCoordinates.y + roomSize.y;
			if (location.RoomSide == RoomSide.Bottom)
			{
				y = roomCoordinates.y - 1;
			}
		}
		return new Vector2Int(x, y);
	}

	public List<DoorLocation> GetDoorLocationsThatFitAtCoordinates(Vector2Int coordinates, Bounds border, RoomSide side, Vector2Int roomSize)
	{
		List<DoorLocation> list = new List<DoorLocation>();
		bool flag = false;
		int num = roomSize.x;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			num = roomSize.y;
		}
		for (int i = 0; i < num; i++)
		{
			if (GetIsSpaceForRoomAtDoor(coordinates, side, roomSize, i, border))
			{
				flag = true;
				list.Add(new DoorLocation(RoomUtility.GetOppositeSide(side), i));
			}
			else if (flag)
			{
				break;
			}
		}
		return list;
	}

	public static Vector2 GetRoomCoordinatesFromGridCoordinates(Vector2Int gridCoordinates)
	{
		return new Vector2(gridCoordinates.x * 32, gridCoordinates.y * 18);
	}

	public static bool GetIsGridSpaceAvailable(Vector2Int coordinates)
	{
		int num = coordinates.x + m_gridOffset;
		int num2 = coordinates.y + m_gridOffset;
		return GridPoints[num, num2] == null;
	}

	public static bool GetIsSpaceForRoomAtDoor(Vector2Int coordinates, Vector2Int roomSize, RoomSide side, int atDoorNumber)
	{
		int num = 1;
		if (side == RoomSide.Left || side == RoomSide.Bottom)
		{
			num = -1;
		}
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			for (int num2 = roomSize.x - 1; num2 >= 0; num2--)
			{
				int num3 = coordinates.x + m_gridOffset + num * num2;
				for (int num4 = atDoorNumber; num4 > -roomSize.y + atDoorNumber; num4--)
				{
					int num5 = coordinates.y + m_gridOffset + num4;
					if (GridPoints[num3, num5] != null)
					{
						return false;
					}
				}
			}
		}
		else
		{
			for (int num6 = roomSize.y - 1; num6 >= 0; num6--)
			{
				int num7 = coordinates.y + m_gridOffset + num * num6;
				for (int num8 = atDoorNumber; num8 > -roomSize.x + atDoorNumber; num8--)
				{
					int num9 = coordinates.x + m_gridOffset - num8;
					if (GridPoints[num9, num7] != null)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static bool GetIsSpaceForRoomAtDoor(Vector2Int coordinates, RoomSide side, Vector2Int roomSize, int atDoorNumber, Bounds border)
	{
		int num = 1;
		if (side == RoomSide.Left || side == RoomSide.Bottom)
		{
			num = -1;
		}
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			for (int num2 = roomSize.x - 1; num2 >= 0; num2--)
			{
				int num3 = coordinates.x + num * num2;
				int num4 = num3 + m_gridOffset;
				for (int num5 = atDoorNumber; num5 > -roomSize.y + atDoorNumber; num5--)
				{
					int num6 = coordinates.y + num5;
					int num7 = num6 + m_gridOffset;
					bool num8 = GridPoints[num4, num7] != null;
					bool flag = border.max.x == (float)num3 || border.max.y == (float)num6 || !border.Contains(new Vector2(num3, num6));
					if (num8 || flag)
					{
						return false;
					}
				}
			}
		}
		else
		{
			for (int num9 = roomSize.y - 1; num9 >= 0; num9--)
			{
				int num10 = coordinates.y + num * num9;
				int num11 = num10 + m_gridOffset;
				for (int num12 = atDoorNumber; num12 > -roomSize.x + atDoorNumber; num12--)
				{
					int num13 = coordinates.x - num12;
					int num14 = num13 + m_gridOffset;
					bool num15 = GridPoints[num14, num11] != null;
					bool flag2 = border.max.x == (float)num13 || border.max.y == (float)num10 || !border.Contains(new Vector2(num13, num10));
					if (num15 || flag2)
					{
						return false;
					}
				}
			}
		}
		return true;
	}

	public static int GetDoorNumber(GridPoint gridPoint, RoomSide side)
	{
		int result = gridPoint.LocalCoords.x;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			result = gridPoint.Owner.Size.y - 1 - gridPoint.LocalCoords.y;
		}
		return result;
	}

	public static Vector2Int GetGridCoordsOnGridPointSide(GridPoint gridPoint, RoomSide side)
	{
		return GetDoorLeadsToGridCoordinates(gridPoint.GridCoordinates, Vector2Int.one, new DoorLocation(side, 0));
	}

	public static Bounds GetGridPointBounds(Vector2Int gridCoordinates)
	{
		Bounds result = default(Bounds);
		Vector2 worldPositionFromRoomCoordinates = GetWorldPositionFromRoomCoordinates(new Vector2(gridCoordinates.x * 32, gridCoordinates.y * 18), new Vector2Int(1, 1));
		result.center = worldPositionFromRoomCoordinates;
		float x = worldPositionFromRoomCoordinates.x - 16f;
		float y = worldPositionFromRoomCoordinates.y - 9f;
		float x2 = worldPositionFromRoomCoordinates.x + 16f;
		float y2 = worldPositionFromRoomCoordinates.y + 9f;
		result.SetMinMax(new Vector2(x, y), new Vector2(x2, y2));
		return result;
	}

	public static Vector2 GetWorldPositionFromRoomCoordinates(Vector2 roomCoordinates, Vector2Int roomSize)
	{
		float x = 0.5f * (float)roomSize.x * 32f;
		float y = 0.5f * (float)roomSize.y * 18f;
		Vector2 vector = new Vector2(x, y);
		return roomCoordinates + vector;
	}

	public static Vector2Int GetRoomGridCoordinates(Vector2Int buildCoordinates, Vector2Int roomSize, DoorLocation location)
	{
		int x = buildCoordinates.x;
		int y = buildCoordinates.y;
		if (location != DoorLocation.Empty)
		{
			if (location.RoomSide == RoomSide.Left || location.RoomSide == RoomSide.Right)
			{
				if (location.RoomSide == RoomSide.Left)
				{
					x = buildCoordinates.x;
				}
				else if (location.RoomSide == RoomSide.Right)
				{
					x = buildCoordinates.x - roomSize.x + 1;
				}
				y = buildCoordinates.y - (roomSize.y - 1 - location.DoorNumber);
			}
			else if (location.RoomSide == RoomSide.Top || location.RoomSide == RoomSide.Bottom)
			{
				x = buildCoordinates.x - location.DoorNumber;
				if (location.RoomSide == RoomSide.Top)
				{
					y = buildCoordinates.y - roomSize.y + 1;
				}
			}
		}
		return new Vector2Int(x, y);
	}

	public void OnBiomeControllerDestroyed(BiomeController biomeController)
	{
		foreach (GridPoint gridPoint in biomeController.GridPointManager.GridPoints)
		{
			ClearGridPoint(gridPoint);
		}
		Extents[RoomSide.Left] = m_occupiedGridPointCoords.Min((Vector2Int coords) => coords.x);
		Extents[RoomSide.Right] = m_occupiedGridPointCoords.Max((Vector2Int coords) => coords.x);
		Extents[RoomSide.Top] = m_occupiedGridPointCoords.Max((Vector2Int coords) => coords.y);
		Extents[RoomSide.Bottom] = m_occupiedGridPointCoords.Min((Vector2Int coords) => coords.y);
	}

	private static void UpdateExtentAfterAdd(GridPoint gridPoint)
	{
		if (gridPoint.GridCoordinates.x >= Extents[RoomSide.Right])
		{
			Extents[RoomSide.Right] = gridPoint.GridCoordinates.x + 1;
		}
		else if (gridPoint.GridCoordinates.x < Extents[RoomSide.Left])
		{
			Extents[RoomSide.Left] = gridPoint.GridCoordinates.x;
		}
		if (gridPoint.GridCoordinates.y >= Extents[RoomSide.Top])
		{
			Extents[RoomSide.Top] = gridPoint.GridCoordinates.y + 1;
		}
		else if (gridPoint.GridCoordinates.y < Extents[RoomSide.Bottom])
		{
			Extents[RoomSide.Bottom] = gridPoint.GridCoordinates.y;
		}
	}

	public static void Reset()
	{
		if (GridPoints != null)
		{
			for (int i = 0; i < GridPoints.GetLength(0); i++)
			{
				for (int j = 0; j < GridPoints.GetLength(1); j++)
				{
					GridPoints[i, j] = null;
				}
			}
		}
		if (Extents != null)
		{
			Extents[RoomSide.Left] = 0;
			Extents[RoomSide.Right] = 0;
			Extents[RoomSide.Top] = 0;
			Extents[RoomSide.Bottom] = 0;
		}
	}

	public static bool GetDoesDoorLeadToDifferentBiome(Door door)
	{
		BiomeType appearanceBiomeType = door.Room.AppearanceBiomeType;
		BiomeType doorLeadsToBiome = GetDoorLeadsToBiome(door);
		if ((appearanceBiomeType == BiomeType.Tower && doorLeadsToBiome == BiomeType.TowerExterior) || (appearanceBiomeType == BiomeType.TowerExterior && doorLeadsToBiome == BiomeType.Tower) || (appearanceBiomeType == BiomeType.Stone && doorLeadsToBiome == BiomeType.Town) || (appearanceBiomeType == BiomeType.Town && doorLeadsToBiome == BiomeType.Stone) || (appearanceBiomeType == BiomeType.TowerExterior && doorLeadsToBiome == BiomeType.Study) || (appearanceBiomeType == BiomeType.Study && doorLeadsToBiome == BiomeType.TowerExterior))
		{
			return false;
		}
		return appearanceBiomeType != doorLeadsToBiome;
	}

	public static GridPoint GetGridPoint(Vector2Int coords)
	{
		GridPoint result = null;
		if (GridPoints != null)
		{
			int num = coords.x + m_gridOffset;
			int num2 = coords.y + m_gridOffset;
			try
			{
				result = GridPoints[num, num2];
			}
			catch (Exception)
			{
			}
		}
		return result;
	}

	public static GridPoint GetGridPoint(Door door)
	{
		return (door.Room as Room).GridPointManager.GetGridPoint(new DoorLocation(door.Side, door.Number));
	}

	public static BiomeType GetDoorLeadsToBiome(Door door)
	{
		BiomeType result = door.Room.AppearanceBiomeType;
		GridPoint gridPoint = GetGridPoint(door.GridPointCoordinates);
		if (gridPoint != null)
		{
			GridPoint connectedGridPoint = gridPoint.GetConnectedGridPoint(door.Side);
			if (connectedGridPoint != null)
			{
				result = connectedGridPoint.Biome;
			}
		}
		return result;
	}
}
