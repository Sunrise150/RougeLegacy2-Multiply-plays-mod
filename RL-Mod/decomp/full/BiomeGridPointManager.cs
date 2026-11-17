using System;
using System.Collections.Generic;
using Rooms;
using UnityEngine;

public class BiomeGridPointManager
{
	private BiomeType m_biome;

	private List<GridPointManager> m_tunnelDestinationGridPointManagers = new List<GridPointManager>();

	private Dictionary<RoomSide, GridPoint> m_connectionPoints = new Dictionary<RoomSide, GridPoint>
	{
		{
			RoomSide.Left,
			null
		},
		{
			RoomSide.Right,
			null
		},
		{
			RoomSide.Top,
			null
		},
		{
			RoomSide.Bottom,
			null
		}
	};

	public Bounds Border { get; set; }

	public Vector2Int Center { get; private set; }

	private Dictionary<RoomSide, GridPoint> ConnectionPoints
	{
		get
		{
			return m_connectionPoints;
		}
		set
		{
			m_connectionPoints = value;
		}
	}

	public Dictionary<RoomSide, int> Extents { get; private set; }

	public List<GridPoint> GridPoints
	{
		get
		{
			List<GridPoint> list = new List<GridPoint>();
			if (GridPointManagers != null)
			{
				for (int i = 0; i < GridPointManagers.Count; i++)
				{
					list.AddRange(GridPointManagers[i].GridPointList);
				}
			}
			return list;
		}
	}

	public List<GridPointManager> GridPointManagers { get; private set; }

	public int StandardRoomCount { get; private set; }

	public int SpecialRoomCount { get; private set; }

	public List<GridPointManager> TunnelDestinationGridPointManagers
	{
		get
		{
			return m_tunnelDestinationGridPointManagers;
		}
		private set
		{
			m_tunnelDestinationGridPointManagers = value;
		}
	}

	public int StandaloneRoomCount { get; private set; }

	public int MergeRoomCount { get; private set; }

	private Dictionary<RoomSide, List<DoorLocation>> ValidDoorLocations { get; set; }

	public int TunnelDestinationCount { get; private set; }

	public event EventHandler<GridPointManager> RoomBuiltEvent;

	public BiomeGridPointManager(BiomeType biome)
	{
		m_biome = biome;
	}

	public GridPointManager AddRoomToGrid(int roomNumber, RoomMetaData roomMetaData, bool isRoomMirrored, RoomType roomType, Vector2Int roomCoordinates, BiomeType biome, bool incrementRoomCount = true)
	{
		if (GridPointManagers == null)
		{
			GridPointManagers = new List<GridPointManager>();
		}
		if (roomType == RoomType.Transition)
		{
			Center = roomCoordinates;
			SetBorder(roomCoordinates);
		}
		else if (incrementRoomCount)
		{
			if (roomType == RoomType.Standard)
			{
				StandardRoomCount++;
			}
			else
			{
				SpecialRoomCount++;
			}
		}
		GridPointManager gridPointManager = new GridPointManager(roomNumber, roomCoordinates, biome, roomType, roomMetaData, isRoomMirrored);
		gridPointManager.SetBiomeControllerIndex(GridPointManagers.Count);
		GridPointManagers.Add(gridPointManager);
		UpdateExtents(gridPointManager);
		if (this.RoomBuiltEvent != null)
		{
			this.RoomBuiltEvent(this, gridPointManager);
		}
		return gridPointManager;
	}

	public void AddTunnelDestinationGridPointManager(GridPointManager gridPointManager)
	{
		if (TunnelDestinationGridPointManagers == null)
		{
			TunnelDestinationGridPointManagers = new List<GridPointManager>();
		}
		TunnelDestinationGridPointManagers.Add(gridPointManager);
		TunnelDestinationCount++;
	}

	public bool GetAreCoordinatesWithinBorder(Vector2Int coordinates, bool isStandardRoom)
	{
		if (isStandardRoom && m_biome != BiomeType.Stone && m_biome != BiomeType.Tower && m_biome != BiomeType.TowerExterior)
		{
			if ((float)coordinates.x >= Border.max.x || (float)coordinates.x <= Border.min.x || (float)coordinates.y >= Border.max.y || (float)coordinates.y <= Border.min.y)
			{
				return false;
			}
			return true;
		}
		if ((float)coordinates.x >= Border.max.x || (float)coordinates.y >= Border.max.y)
		{
			return false;
		}
		return Border.Contains((Vector2)coordinates);
	}

	public GridPoint GetConnectionPoint(RoomSide side)
	{
		return ConnectionPoints[side];
	}

	public List<DoorLocation> GetValidDoorLocations(RoomSide side)
	{
		return ValidDoorLocations[side];
	}

	private void SetBorder(Vector2Int transitionRoomGridCoordinates)
	{
		Dictionary<RoomSide, int> biomeBorderOffsets = BiomeCreation_EV.GetBiomeBorderOffsets(m_biome);
		Bounds border = default(Bounds);
		RoomMetaData transitionRoom = RoomLibrary.GetSetCollection(m_biome).TransitionRoom;
		int num = transitionRoomGridCoordinates.x + biomeBorderOffsets[RoomSide.Left];
		int num2 = transitionRoomGridCoordinates.y + biomeBorderOffsets[RoomSide.Bottom];
		int num3 = transitionRoomGridCoordinates.x + transitionRoom.Size.x + biomeBorderOffsets[RoomSide.Right];
		int num4 = transitionRoomGridCoordinates.y + transitionRoom.Size.y + biomeBorderOffsets[RoomSide.Top];
		border.SetMinMax(new Vector2(num, num2), new Vector2(num3, num4));
		Border = border;
	}

	public void SetStandaloneRoomCount(int count)
	{
		StandaloneRoomCount = count;
	}

	public void SetMergeRoomCount(int count)
	{
		MergeRoomCount = count;
	}

	public void SetConnectionPoint(RoomSide side, GridPoint gridPoint, List<int> validTransitionRoomDoorNumbers)
	{
		ConnectionPoints[side] = gridPoint;
		if (ValidDoorLocations == null)
		{
			ValidDoorLocations = new Dictionary<RoomSide, List<DoorLocation>>();
		}
		ValidDoorLocations.Add(side, new List<DoorLocation>());
		for (int i = 0; i < validTransitionRoomDoorNumbers.Count; i++)
		{
			ValidDoorLocations[side].Add(new DoorLocation(RoomUtility.GetOppositeSide(side), validTransitionRoomDoorNumbers[i]));
		}
	}

	private void UpdateExtents(GridPointManager gridPointManager)
	{
		if (Extents == null)
		{
			Extents = new Dictionary<RoomSide, int>();
		}
		if (Extents.ContainsKey(RoomSide.Left))
		{
			if (gridPointManager.GridCoordinates.x < Extents[RoomSide.Left])
			{
				Extents[RoomSide.Left] = gridPointManager.GridCoordinates.x;
			}
		}
		else
		{
			Extents.Add(RoomSide.Left, gridPointManager.GridCoordinates.x);
		}
		if (Extents.ContainsKey(RoomSide.Right))
		{
			if (gridPointManager.GridCoordinates.x + gridPointManager.Size.x > Extents[RoomSide.Right])
			{
				Extents[RoomSide.Right] = gridPointManager.GridCoordinates.x + gridPointManager.Size.x;
			}
		}
		else
		{
			Extents.Add(RoomSide.Right, gridPointManager.GridCoordinates.x + gridPointManager.Size.x);
		}
		if (Extents.ContainsKey(RoomSide.Bottom))
		{
			if (gridPointManager.GridCoordinates.y < Extents[RoomSide.Bottom])
			{
				Extents[RoomSide.Bottom] = gridPointManager.GridCoordinates.y;
			}
		}
		else
		{
			Extents.Add(RoomSide.Bottom, gridPointManager.GridCoordinates.y);
		}
		if (Extents.ContainsKey(RoomSide.Top))
		{
			if (gridPointManager.GridCoordinates.y + gridPointManager.Size.y > Extents[RoomSide.Top])
			{
				Extents[RoomSide.Top] = gridPointManager.GridCoordinates.y + gridPointManager.Size.y;
			}
		}
		else
		{
			Extents.Add(RoomSide.Top, gridPointManager.GridCoordinates.y + gridPointManager.Size.y);
		}
	}

	public void RemoveTunnelDestinations()
	{
		for (int num = GridPointManagers.Count - 1; num >= 0; num--)
		{
			if (GridPointManagers[num].IsTunnelDestination)
			{
				GridPointManagers.RemoveAt(num);
			}
		}
		if (TunnelDestinationGridPointManagers != null)
		{
			TunnelDestinationGridPointManagers.Clear();
		}
		TunnelDestinationCount = 0;
	}

	public GridPointManager GetGridPointManager(int biomeControllerIndex)
	{
		GridPointManager result = null;
		if (GridPointManagers != null)
		{
			for (int i = 0; i < GridPointManagers.Count; i++)
			{
				if (GridPointManagers[i].BiomeControllerIndex == biomeControllerIndex)
				{
					result = GridPointManagers[i];
					break;
				}
			}
		}
		return result;
	}
}
