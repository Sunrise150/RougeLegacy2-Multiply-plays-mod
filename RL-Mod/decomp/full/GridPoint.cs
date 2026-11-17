using System.Collections.Generic;
using Rooms;
using UnityEngine;

public class GridPoint
{
	private Bounds m_bounds;

	public BiomeType Biome => Owner.Biome;

	public Bounds Bounds
	{
		get
		{
			if (Owner != null && m_bounds == default(Bounds))
			{
				m_bounds = GridController.GetGridPointBounds(GridCoordinates);
			}
			return m_bounds;
		}
	}

	public List<DoorLocation> Doors { get; private set; }

	public Vector2Int GridCoordinates => Owner.GridCoordinates + LocalCoords;

	public bool IsMirrored => Owner.IsRoomMirrored;

	public Vector2Int LocalCoords { get; }

	public GridPointManager Owner { get; }

	public RoomMetaData RoomMetaData => Owner.RoomMetaData;

	public int RoomNumber => Owner.RoomNumber;

	public RoomType RoomType => Owner.RoomType;

	public GridPoint(GridPointManager owner, Vector2Int localCoords, List<DoorLocation> doors)
	{
		Owner = owner;
		LocalCoords = localCoords;
		Doors = doors;
	}

	public DoorLocation GetDoorLocation(RoomSide side)
	{
		for (int i = 0; i < Doors.Count; i++)
		{
			if (Doors[i].RoomSide == side)
			{
				return Doors[i];
			}
		}
		return DoorLocation.Empty;
	}

	public bool GetContainsDoorLocation(DoorLocation doorLocation)
	{
		for (int i = 0; i < Doors.Count; i++)
		{
			if (Doors[i] == doorLocation)
			{
				return true;
			}
		}
		return false;
	}

	public void SetDoorLocations(List<DoorLocation> doorLocations)
	{
		Doors = doorLocations;
	}

	public GridPoint GetConnectedGridPoint(RoomSide side)
	{
		if (GetDoorLocation(side) != DoorLocation.Empty)
		{
			GridPoint gridPointOnSide = GetGridPointOnSide(side);
			if (gridPointOnSide != null)
			{
				RoomSide oppositeSide = RoomUtility.GetOppositeSide(side);
				if (gridPointOnSide.GetDoorLocation(oppositeSide) != DoorLocation.Empty)
				{
					return gridPointOnSide;
				}
			}
		}
		return null;
	}

	private GridPoint GetGridPointOnSide(RoomSide side)
	{
		Vector2Int zero = Vector2Int.zero;
		switch (side)
		{
		case RoomSide.Top:
			zero.y = 1;
			break;
		case RoomSide.Bottom:
			zero.y = -1;
			break;
		case RoomSide.Left:
			zero.x = -1;
			break;
		case RoomSide.Right:
			zero.x = 1;
			break;
		}
		return GridController.GetGridPoint(GridCoordinates + zero);
	}
}
