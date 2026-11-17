using System.Collections.Generic;
using Rooms;
using UnityEngine;

public static class ReplaceRoomUtility
{
	public static bool ReplaceRoomMetaData(BiomeController biomeController, GridPoint gridPointToReplaceRoom, RoomSide side)
	{
		IEnumerable<DoorLocation> replacementDoorLocations = GetReplacementDoorLocations(biomeController, side, gridPointToReplaceRoom.Owner);
		List<DoorLocation> doorLocations = gridPointToReplaceRoom.Owner.DoorLocations;
		Vector2Int gridCoordinates = gridPointToReplaceRoom.Owner.GridCoordinates;
		Vector2Int size = gridPointToReplaceRoom.Owner.Size;
		for (int num = doorLocations.Count - 1; num >= 0; num--)
		{
			if (GridController.GetIsGridSpaceAvailable(GridController.GetDoorLeadsToGridCoordinates(gridCoordinates, size, doorLocations[num])))
			{
				doorLocations.RemoveAt(num);
			}
		}
		int doorNumber = GridController.GetDoorNumber(gridPointToReplaceRoom, side);
		DoorLocation item = new DoorLocation(side, doorNumber);
		if (!doorLocations.Contains(item))
		{
			doorLocations.Add(item);
		}
		bool matchMirrored = gridPointToReplaceRoom.Biome == BiomeType.TowerExterior;
		bool result = false;
		RoomSetEntry replacementRoom = GetReplacementRoom(biomeController, gridPointToReplaceRoom.Owner, doorLocations, replacementDoorLocations, matchMirrored);
		if (!replacementRoom.Equals(default(RoomSetEntry)) && replacementRoom.RoomMetaData != null)
		{
			gridPointToReplaceRoom.Owner.SetRoomMetaData(replacementRoom.RoomMetaData, replacementRoom.IsMirrored);
			result = true;
		}
		else
		{
			Debug.LogFormat("<color=purple>| ReplaceRoomUtility | Failed to find a Room with which to replace Room <b>{0}</b> at Grid Coords <b>{1}</b>. </color>", gridPointToReplaceRoom.Owner.RoomMetaData.ID, gridPointToReplaceRoom.Owner.GridCoordinates);
		}
		return result;
	}

	private static IEnumerable<DoorLocation> GetReplacementDoorLocations(BiomeController biomeController, RoomSide side, GridPointManager gridPointManager)
	{
		List<DoorLocation> list = new List<DoorLocation>();
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			for (int i = 0; i < gridPointManager.Size.y; i++)
			{
				list.Add(new DoorLocation(side, i));
			}
		}
		else
		{
			for (int j = 0; j < gridPointManager.Size.x; j++)
			{
				list.Add(new DoorLocation(side, j));
			}
		}
		return list;
	}

	public static RoomSetEntry GetReplacementRoom(BiomeController biomeController, GridPointManager gridPointManager, IEnumerable<DoorLocation> mustHaveAll, IEnumerable<DoorLocation> mustHaveAtLeastOne, bool matchMirrored)
	{
		if (gridPointManager.RoomType == RoomType.Mandatory || gridPointManager.RoomType == RoomType.BossEntrance || gridPointManager.RoomType == RoomType.Transition)
		{
			return default(RoomSetEntry);
		}
		HashSet<RoomSetEntry> setOfRoomsWithDoorLocations = GetSetOfRoomsWithDoorLocations(biomeController, mustHaveAll, mustHaveAtLeastOne, gridPointManager.Biome);
		HashSet<RoomSetEntry> hashSet = RoomLibrary.GetSetCollection(gridPointManager.Biome).RoomTypeSets[gridPointManager.RoomType];
		HashSet<RoomSetEntry> hashSet2 = RoomLibrary.GetSetCollection(gridPointManager.Biome).RoomTypeSets[RoomType.Standard];
		List<RoomSetEntry> list = new List<RoomSetEntry>();
		List<RoomSetEntry> list2 = new List<RoomSetEntry>();
		foreach (RoomSetEntry item in setOfRoomsWithDoorLocations)
		{
			if (!(gridPointManager.Size != item.RoomMetaData.Size) && (!matchMirrored || gridPointManager.IsRoomMirrored == item.IsMirrored))
			{
				if (hashSet.Contains(item))
				{
					list.Add(item);
				}
				else if (hashSet2.Contains(item))
				{
					list2.Add(item);
				}
			}
		}
		if (list.Count > 0)
		{
			return list[RNGManager.GetRandomNumber(RngID.BiomeCreation, "Get Random Replacement Room", 0, list.Count)];
		}
		if (list2.Count > 0)
		{
			return list2[RNGManager.GetRandomNumber(RngID.BiomeCreation, "Get Random Replacement Room", 0, list2.Count)];
		}
		return default(RoomSetEntry);
	}

	private static bool EntryContainsDoorLocation(RoomSetEntry entry, DoorLocation locationToFind)
	{
		for (int i = 0; i < entry.RoomMetaData.DoorLocations.Length; i++)
		{
			DoorLocation doorLocation = entry.RoomMetaData.DoorLocations[i];
			if (((!entry.IsMirrored) ? doorLocation : RoomUtility.GetMirrorDoorLocation(entry.RoomMetaData.Size, doorLocation)) == locationToFind)
			{
				return true;
			}
		}
		return false;
	}

	private static HashSet<RoomSetEntry> GetSetOfRoomsWithDoorLocations(BiomeController biomeController, IEnumerable<DoorLocation> mustHaveAll, IEnumerable<DoorLocation> mustHaveAtLeastOne, BiomeType biome)
	{
		HashSet<RoomSetEntry> roomSet = RoomLibrary.GetSetCollection(biome).RoomSet;
		HashSet<RoomSetEntry> hashSet = new HashSet<RoomSetEntry>();
		foreach (RoomSetEntry item in roomSet)
		{
			bool flag = true;
			foreach (DoorLocation item2 in mustHaveAll)
			{
				if (!EntryContainsDoorLocation(item, item2))
				{
					flag = false;
					break;
				}
			}
			if (!flag)
			{
				continue;
			}
			bool flag2 = false;
			foreach (DoorLocation item3 in mustHaveAtLeastOne)
			{
				if (EntryContainsDoorLocation(item, item3))
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				hashSet.Add(item);
			}
		}
		return hashSet;
	}
}
