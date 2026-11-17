using System.Collections.Generic;
using Rooms;
using UnityEngine;

public class GridPointManager
{
	private List<GridPointManager> m_mergeWithGridPointManagers = new List<GridPointManager>();

	private int m_biomeControllerIndex = -1;

	private BiomeType m_appearanceOverride;

	public int RoomSeed { get; private set; } = -1;


	public int TunnelRoomSeed { get; private set; } = -1;


	public int PropSeed { get; private set; } = -1;


	public int DecoSeed { get; private set; } = -1;


	public int EnemySeed { get; private set; } = -1;


	public int ChestSeed { get; private set; } = -1;


	public int HazardSeed { get; private set; } = -1;


	public int SpecialPropSeed { get; private set; } = -1;


	public BiomeType Biome { get; }

	public BiomeType AppearanceOverride
	{
		get
		{
			return m_appearanceOverride;
		}
		private set
		{
			m_appearanceOverride = value;
		}
	}

	public Bounds Bounds { get; }

	public GridPointManagerContentEntry[] Content { get; set; }

	public List<DoorLocation> DoorLocations
	{
		get
		{
			List<DoorLocation> list = new List<DoorLocation>();
			for (int i = 0; i < GridPoints.GetLength(0); i++)
			{
				for (int j = 0; j < GridPoints.GetLength(1); j++)
				{
					list.AddRange(GridPoints[i, j].Doors);
				}
			}
			return list;
		}
	}

	public Vector2Int GridCoordinates { get; }

	public GridPoint[,] GridPoints { get; private set; }

	public List<GridPoint> GridPointList { get; private set; }

	public bool IsRoomMirrored { get; private set; }

	public bool IsTunnelDestination { get; }

	public Vector2Int MinExtents => GridCoordinates;

	public Vector2Int MaxExtents => GridCoordinates + Size;

	public RoomMetaData RoomMetaData { get; private set; }

	public int RoomNumber { get; }

	public RoomType RoomType { get; set; }

	public Vector2Int Size { get; }

	public List<GridPointManager> MergeWithGridPointManagers
	{
		get
		{
			return m_mergeWithGridPointManagers;
		}
		private set
		{
			m_mergeWithGridPointManagers = value;
		}
	}

	public int BiomeControllerIndex
	{
		get
		{
			return m_biomeControllerIndex;
		}
		private set
		{
			m_biomeControllerIndex = value;
		}
	}

	public GridPointManager(int roomNumber, Vector2Int coordinates, BiomeType biome, RoomType roomType, RoomMetaData roomMetaData, bool isRoomMirrored, bool isTunnelDestination = false)
	{
		RoomNumber = roomNumber;
		GridCoordinates = coordinates;
		Biome = biome;
		RoomType = roomType;
		RoomMetaData = roomMetaData;
		IsRoomMirrored = isRoomMirrored;
		IsTunnelDestination = isTunnelDestination;
		if (roomMetaData != null)
		{
			if (roomMetaData.BiomeOverride == BiomeType.Lineage)
			{
				int num = Mathf.Min(SaveManager.LineageSaveData.LineageHeirList.Count, 20) - 4;
				num = Mathf.Max(0, Mathf.CeilToInt((float)num / 3f));
				int x = 2 + num;
				Size = new Vector2Int(x, 1);
			}
			else
			{
				Size = roomMetaData.Size;
			}
		}
		CreateGridPoints();
		if (GridPointList != null && GridPointList.Count > 0)
		{
			Bounds bounds = new Bounds
			{
				center = GridPointList[0].Bounds.center
			};
			for (int i = 0; i < GridPointList.Count; i++)
			{
				bounds.Encapsulate(GridController.GetGridPointBounds(GridPointList[i].GridCoordinates));
			}
			Bounds = bounds;
		}
		AppearanceOverride = roomMetaData.BiomeOverride;
	}

	private void CreateGridPoints()
	{
		if (GridPointList == null)
		{
			GridPointList = new List<GridPoint>();
		}
		if (GridPoints == null)
		{
			GridPoints = new GridPoint[Size.x, Size.y];
		}
		for (int i = 0; i < Size.x; i++)
		{
			bool flag = i == 0;
			bool flag2 = i == Size.x - 1;
			for (int j = 0; j < Size.y; j++)
			{
				List<DoorLocation> list = new List<DoorLocation>();
				bool num = j == Size.y - 1;
				bool flag3 = j == 0;
				int doorNumber = Size.y - 1 - j;
				if (flag && GetDoorLocation(RoomSide.Left, doorNumber) != DoorLocation.Empty)
				{
					list.Add(new DoorLocation(RoomSide.Left, doorNumber));
				}
				if (flag2 && GetDoorLocation(RoomSide.Right, doorNumber) != DoorLocation.Empty)
				{
					list.Add(new DoorLocation(RoomSide.Right, doorNumber));
				}
				doorNumber = i;
				if (num && GetDoorLocation(RoomSide.Top, doorNumber) != DoorLocation.Empty)
				{
					list.Add(new DoorLocation(RoomSide.Top, doorNumber));
				}
				if (flag3 && GetDoorLocation(RoomSide.Bottom, doorNumber) != DoorLocation.Empty)
				{
					list.Add(new DoorLocation(RoomSide.Bottom, doorNumber));
				}
				CreateGridPoint(i, j, list);
			}
		}
	}

	private void CreateGridPoint(int xCoord, int yCoord, List<DoorLocation> doorLocations)
	{
		GridPoint gridPoint = GridPoints[xCoord, yCoord];
		if (gridPoint == null)
		{
			gridPoint = new GridPoint(this, new Vector2Int(xCoord, yCoord), doorLocations);
			GridPoints[xCoord, yCoord] = gridPoint;
			GridPointList.Add(gridPoint);
			if (GridController.IsInstantiated && !IsTunnelDestination)
			{
				GridController.AddRoomToGrid(gridPoint);
			}
		}
		else
		{
			gridPoint.SetDoorLocations(doorLocations);
		}
	}

	public void SetBiomeControllerIndex(int index)
	{
		BiomeControllerIndex = index;
	}

	public void SetAppearanceOverride(BiomeType biome)
	{
		AppearanceOverride = biome;
	}

	public List<DoorLocation> GetDoorLocationsOnSide(RoomSide side)
	{
		if (side == RoomSide.Any)
		{
			return DoorLocations;
		}
		List<DoorLocation> list = new List<DoorLocation>();
		foreach (DoorLocation doorLocation in DoorLocations)
		{
			if (doorLocation.RoomSide == side)
			{
				list.Add(doorLocation);
			}
		}
		return list;
	}

	public void SetMergeWithRooms(List<GridPointManager> roomsToMerge)
	{
		MergeWithGridPointManagers = new List<GridPointManager>(roomsToMerge);
		if (MergeWithGridPointManagers.Contains(this))
		{
			MergeWithGridPointManagers.Remove(this);
		}
	}

	public GridPointManager GetConnectedRoom(DoorLocation doorLocation)
	{
		GridPoint gridPoint = GridController.GetGridPoint(GetDoorLeadsToGridCoordinates(doorLocation));
		GridPointManager result = null;
		if (gridPoint != null)
		{
			RoomSide oppositeSide = RoomUtility.GetOppositeSide(doorLocation.RoomSide);
			if (gridPoint.GetDoorLocation(oppositeSide) != DoorLocation.Empty)
			{
				result = gridPoint.Owner;
			}
		}
		return result;
	}

	public DoorLocation GetDoorLocation(RoomSide side, int doorNumber)
	{
		DoorLocation result = DoorLocation.Empty;
		bool flag = false;
		if ((!IsRoomMirrored) ? RoomMetaData.GetHasDoor(new DoorLocation(side, doorNumber)) : RoomMetaData.GetHasDoor(new DoorLocation(side, doorNumber), isMirrored: true))
		{
			result = new DoorLocation(side, doorNumber);
		}
		return result;
	}

	public Vector2Int GetDoorLeadsToGridCoordinates(DoorLocation location)
	{
		return GridController.GetDoorLeadsToGridCoordinates(GridCoordinates, Size, location);
	}

	public void SetRoomMetaData(RoomMetaData roomMetaData, bool isRoomMirrored)
	{
		if (!roomMetaData.IsSpecialRoom && RoomType != 0 && RoomType != RoomType.Trap)
		{
			RoomType = RoomType.Standard;
		}
		RoomMetaData = roomMetaData;
		IsRoomMirrored = isRoomMirrored;
		CreateGridPoints();
	}

	public void SetRoomSeed(int seed)
	{
		RoomSeed = seed;
		RNGManager.SetSeed(RngID.Room_RNGSeed_Generator, RoomSeed);
		PropSeed = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "GridPointManager.SetRoomSeed()", 0, int.MaxValue);
		DecoSeed = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "GridPointManager.SetRoomSeed()", 0, int.MaxValue);
		EnemySeed = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "GridPointManager.SetRoomSeed()", 0, int.MaxValue);
		ChestSeed = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "GridPointManager.SetRoomSeed()", 0, int.MaxValue);
		HazardSeed = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "GridPointManager.SetRoomSeed()", 0, int.MaxValue);
		SpecialPropSeed = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "GridPointManager.SetRoomSeed()", 0, int.MaxValue);
		TunnelRoomSeed = RNGManager.GetRandomNumber(RngID.Room_RNGSeed_Generator, "GridPointManager.SetRoomSeed()", 0, int.MaxValue);
	}

	public GridPoint GetGridPoint(DoorLocation doorLocation)
	{
		for (int i = 0; i < GridPointList.Count; i++)
		{
			if (GridPointList[i].GetContainsDoorLocation(doorLocation))
			{
				return GridPointList[i];
			}
		}
		return null;
	}
}
