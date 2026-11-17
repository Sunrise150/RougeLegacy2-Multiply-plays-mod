using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RLWorldCreation;

public class MergeRoomStrategy
{
	protected CreateMergeRooms_BuildStage m_buildStage;

	public virtual IEnumerator Run(CreateMergeRooms_BuildStage buildStage, BiomeController biomeController)
	{
		m_buildStage = buildStage;
		List<GridPointManager> potentialMergeTargets_V = GetPotentialMergeTargets_V2(biomeController, biomeController.Biome, RoomType.BossEntrance, RoomType.Fairy, RoomType.Transition, RoomType.Bonus);
		yield return MergeRooms_V2(biomeController, biomeController.Biome, potentialMergeTargets_V);
	}

	protected virtual List<GridPointManager> GetPotentialMergeTargets_V2(BiomeController biomeController, BiomeType biome, params RoomType[] exludeRoomTypes)
	{
		return biomeController.GridPointManager.GridPointManagers.Where((GridPointManager room) => room.RoomMetaData.CanMerge && room.Biome == biome && !exludeRoomTypes.Contains(room.RoomType)).ToList();
	}

	protected IEnumerator MergeRooms_V2(BiomeController biomeController, BiomeType biome, List<GridPointManager> rooms, bool shuffle = true)
	{
		if (shuffle)
		{
			RoomUtility.ShuffleList(rooms);
		}
		while (rooms.Count > 1)
		{
			GridPointManager baseRoom = rooms.First();
			rooms.RemoveAt(0);
			RoomSide[] sidesToCheckForMerge_V = GetSidesToCheckForMerge_V2(baseRoom);
			HashSet<GridPointManager> hashSet = new HashSet<GridPointManager>();
			foreach (RoomSide side in sidesToCheckForMerge_V)
			{
				List<GridPointManager> other = RecursiveMerge_V2(biomeController, rooms, baseRoom, null, side);
				hashSet.UnionWith(other);
			}
			if (hashSet.Count <= 1)
			{
				continue;
			}
			foreach (GridPointManager item in hashSet)
			{
				rooms.Remove(item);
				item.SetMergeWithRooms(hashSet.ToList());
			}
		}
		UpdateBiomeControllerIndices(biomeController);
		yield break;
	}

	protected static void UpdateBiomeControllerIndices(BiomeController biomeController)
	{
		List<GridPointManager> list = new List<GridPointManager>();
		List<GridPointManager> list2 = new List<GridPointManager>();
		for (int i = 0; i < biomeController.GridPointManager.GridPointManagers.Count; i++)
		{
			GridPointManager gridPointManager = biomeController.GridPointManager.GridPointManagers[i];
			if (gridPointManager.MergeWithGridPointManagers.Count == 0)
			{
				list.Add(gridPointManager);
			}
			else
			{
				list2.Add(gridPointManager);
			}
		}
		for (int j = 0; j < list.Count; j++)
		{
			list[j].SetBiomeControllerIndex(j);
		}
		List<GridPointManager> list3 = new List<GridPointManager>();
		while (list2.Count > 0)
		{
			GridPointManager gridPointManager2 = list2[0];
			list2.RemoveAt(0);
			list3.Add(gridPointManager2);
			for (int k = 0; k < gridPointManager2.MergeWithGridPointManagers.Count; k++)
			{
				GridPointManager item = gridPointManager2.MergeWithGridPointManagers[k];
				list2.Remove(item);
			}
		}
		int num = list.Count;
		for (int l = 0; l < list3.Count; l++)
		{
			list3[l].SetBiomeControllerIndex(num);
			for (int m = 0; m < list3[l].MergeWithGridPointManagers.Count; m++)
			{
				list3[l].MergeWithGridPointManagers[m].SetBiomeControllerIndex(num);
			}
			num++;
		}
		biomeController.GridPointManager.SetStandaloneRoomCount(list.Count);
		biomeController.GridPointManager.SetMergeRoomCount(list3.Count);
	}

	private List<GridPointManager> RecursiveMerge_V2(BiomeController biomeController, List<GridPointManager> rooms, GridPointManager baseRoom, GridPointManager roomToCheck, RoomSide side)
	{
		List<GridPointManager> list = new List<GridPointManager>();
		GridPointManager gridPointManager = baseRoom;
		if (roomToCheck != null)
		{
			gridPointManager = roomToCheck;
			list.Add(gridPointManager);
		}
		else
		{
			list.Add(baseRoom);
		}
		List<DoorLocation> doorLocationsOnSide = gridPointManager.GetDoorLocationsOnSide(side);
		if (doorLocationsOnSide.Count > 0)
		{
			List<GridPointManager> list2 = new List<GridPointManager>();
			foreach (DoorLocation item in doorLocationsOnSide)
			{
				GridPointManager connectedRoom = gridPointManager.GetConnectedRoom(item);
				if (connectedRoom != null && !list2.Contains(connectedRoom))
				{
					list2.Add(connectedRoom);
				}
			}
			if (list2.Count == 1)
			{
				GridPointManager gridPointManager2 = list2[0];
				if (IsRoomInRoomPool_V2(rooms, gridPointManager2) && IsRoomInBiome_V2(baseRoom.Biome, gridPointManager2) && AttemptMerge(biomeController, side, gridPointManager2) && AreTargetOfMergeBoundsValid_V2(baseRoom, gridPointManager2, side) && IsDoorLayoutOfConnectedRoomValid_V2(baseRoom, gridPointManager, gridPointManager2, side))
				{
					list.AddRange(RecursiveMerge_V2(biomeController, rooms, baseRoom, gridPointManager2, side));
				}
			}
		}
		return list;
	}

	private static bool AreTargetOfMergeBoundsEqual_V2(GridPointManager baseRoom, GridPointManager target, RoomSide side)
	{
		bool result = false;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			if (baseRoom.Bounds.min.y == target.Bounds.min.y && baseRoom.Bounds.max.y == target.Bounds.max.y)
			{
				result = true;
			}
		}
		else if (baseRoom.Bounds.min.x == target.Bounds.min.x && baseRoom.Bounds.max.x == target.Bounds.max.x)
		{
			result = true;
		}
		return result;
	}

	private static bool AreTargetOfMergeBoundsValid_V2(GridPointManager baseRoom, GridPointManager target, RoomSide side)
	{
		bool flag = false;
		if (AreTargetOfMergeBoundsEqual_V2(baseRoom, target, side))
		{
			return true;
		}
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			return target.Bounds.min.y >= baseRoom.Bounds.min.y && target.Bounds.max.y <= baseRoom.Bounds.max.y;
		}
		return target.Bounds.min.x >= baseRoom.Bounds.min.x && target.Bounds.max.x <= baseRoom.Bounds.max.x;
	}

	protected virtual bool AttemptMerge(BiomeController biomeController, RoomSide side, GridPointManager connectedRoom)
	{
		int mergeOdds = GetMergeOdds(biomeController, side);
		if (mergeOdds > 0)
		{
			return RNGManager.GetRandomNumber(RngID.MergeRooms, "Attempt Merge?", 0, 99) <= mergeOdds;
		}
		return false;
	}

	private int GetMergeOdds(BiomeController biomeController, RoomSide side)
	{
		return side switch
		{
			RoomSide.Top => biomeController.BiomeData.MergeTopOdds, 
			RoomSide.Bottom => biomeController.BiomeData.MergeBottomOdds, 
			RoomSide.Left => biomeController.BiomeData.MergeLeftOdds, 
			RoomSide.Right => biomeController.BiomeData.MergeRightOdds, 
			_ => throw new ArgumentOutOfRangeException(), 
		};
	}

	private static RoomSide[] GetSidesToCheckForMerge_V2(GridPointManager baseRoom)
	{
		RoomSide[] array = baseRoom.DoorLocations.Select((DoorLocation door) => door.RoomSide).ToArray();
		int num = 0;
		if (array.Length > 1)
		{
			num = RNGManager.GetRandomNumber(RngID.MergeRooms, $"Get Random Side of Room {baseRoom.RoomNumber} to attempt merge", 0, array.Length);
		}
		RoomSide[] result = new RoomSide[2]
		{
			RoomSide.Left,
			RoomSide.Right
		};
		if (array[num] == RoomSide.Top || array[num] == RoomSide.Bottom)
		{
			result = new RoomSide[2]
			{
				RoomSide.Top,
				RoomSide.Bottom
			};
		}
		return result;
	}

	private static bool IsDoorLayoutOfConnectedRoomValid_V2(GridPointManager baseRoom, GridPointManager currentRoom, GridPointManager target, RoomSide side)
	{
		List<RoomSide> invalidSides = new List<RoomSide>();
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			if (target.Bounds.min.y > baseRoom.Bounds.min.y)
			{
				invalidSides.Add(RoomSide.Bottom);
			}
			if (target.Bounds.max.y < baseRoom.Bounds.max.y)
			{
				invalidSides.Add(RoomSide.Top);
			}
		}
		else
		{
			if (target.Bounds.min.x > baseRoom.Bounds.min.x)
			{
				invalidSides.Add(RoomSide.Left);
			}
			if (target.Bounds.max.x < baseRoom.Bounds.max.x)
			{
				invalidSides.Add(RoomSide.Right);
			}
		}
		bool flag = true;
		if (invalidSides.Count > 0)
		{
			flag = !target.DoorLocations.Any((DoorLocation d) => invalidSides.Contains(d.RoomSide));
		}
		if (flag)
		{
			RoomSide oppositeSide = RoomUtility.GetOppositeSide(side);
			List<DoorLocation> doorLocationsOnSide = target.GetDoorLocationsOnSide(oppositeSide);
			for (int i = 0; i < doorLocationsOnSide.Count; i++)
			{
				GridPointManager connectedRoom = target.GetConnectedRoom(doorLocationsOnSide[i]);
				if (connectedRoom != null && connectedRoom != currentRoom)
				{
					flag = false;
					break;
				}
			}
		}
		return flag;
	}

	private static bool IsRoomInBiome_V2(BiomeType biome, GridPointManager room)
	{
		return room.Biome == biome;
	}

	private static bool IsRoomInRoomPool_V2(List<GridPointManager> roomPool, GridPointManager room)
	{
		return roomPool.Contains(room);
	}
}
