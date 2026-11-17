using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

public class CheckConnectionRoomsBuildRule
{
	public virtual IEnumerator UpdateConnectionPoint(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		RoomSide connectDirection = biomeController.BiomeData.ConnectDirection;
		BiomeController biomeController2 = WorldBuilder.GetBiomeController(biomeController.BiomeData.ConnectsTo);
		RoomMetaData transitionRoom = RoomLibrary.GetSetCollection(biomeController.Biome).TransitionRoom;
		if (!(biomeController2 == null))
		{
			List<GridPoint> list = GetGridPointsWithAvailableDoorsOnSide(biomeController2, connectDirection).ToList();
			List<GridPoint> list2 = biomeCreator.GetGridPointsOnSideOfBiomeTransitionRoom(biomeController2, connectDirection).ToList();
			List<GridPoint> list3 = biomeCreator.GetGridPointsWithSpaceOnSide(biomeController2, connectDirection, biomeController2.GridPointManager.GridPoints).ToList();
			List<GridPoint> list4 = biomeCreator.GetGridPointsWhereTransitionRoomFits(biomeController, biomeController2.GridPointManager.GridPoints).ToList();
			List<GridPoint> list5 = list2.Intersect(list).ToList().Intersect(list3)
				.ToList()
				.Intersect(list4)
				.ToList();
			if (list5.Count() == 0)
			{
				list5 = list.Intersect(list3).Intersect(list4).ToList();
			}
			if (list5.Count() > 0)
			{
				GridPoint centermostConnectionPoint = GetCentermostConnectionPoint(biomeController2, connectDirection, list5);
				SetConnectionPoint(connectDirection, biomeController2, transitionRoom, centermostConnectionPoint);
			}
			else
			{
				ReplaceRoomAndSetConnectionPoint(biomeCreator, biomeController, connectDirection, biomeController2, transitionRoom, list2, list3, list4);
			}
		}
		yield break;
	}

	private IEnumerable<GridPoint> GetGridPointsWithAvailableDoorsOnSide(BiomeController biomeController, RoomSide side)
	{
		GridPoint[] array = biomeController.GridPointManager.GridPoints.Where((GridPoint gridPoint) => gridPoint.GetDoorLocation(side) != DoorLocation.Empty).ToArray();
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

	private void ReplaceRoomAndSetConnectionPoint(BiomeCreator biomeCreator, BiomeController biomeController, RoomSide side, BiomeController connectsToBiomeController, RoomMetaData transitionRoomMetaData, IEnumerable<GridPoint> gridPointsOnSideOfTransitionRoom, IEnumerable<GridPoint> gridPointsWithSpace, IEnumerable<GridPoint> gridPointsWhereTransitionRoomFits)
	{
		List<GridPoint> first = biomeCreator.GetGridPointsOfRoomType(connectsToBiomeController, RoomType.Standard).ToList();
		List<GridPoint> list = first.Intersect(gridPointsOnSideOfTransitionRoom).Intersect(gridPointsWithSpace).Intersect(gridPointsWhereTransitionRoomFits)
			.ToList();
		if (list.Count() == 0)
		{
			list = first.Intersect(gridPointsWithSpace).Intersect(gridPointsWhereTransitionRoomFits).ToList();
		}
		bool flag = false;
		while (!flag && list.Count() > 0)
		{
			GridPoint centermostConnectionPoint = GetCentermostConnectionPoint(connectsToBiomeController, side, list);
			if (centermostConnectionPoint != null)
			{
				RoomMetaData roomMetaData = centermostConnectionPoint.RoomMetaData;
				flag = ReplaceRoomUtility.ReplaceRoomMetaData(connectsToBiomeController, centermostConnectionPoint, side);
				if (flag)
				{
					SetConnectionPoint(side, connectsToBiomeController, transitionRoomMetaData, centermostConnectionPoint);
					if (centermostConnectionPoint.RoomMetaData == roomMetaData)
					{
						string text = $"<color=red>| {this} | Failed to find a Replacement Room for Room <b>({roomMetaData.ID})</b></color>";
						Debug.LogFormat(text);
						biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text);
					}
				}
				else
				{
					list.Remove(centermostConnectionPoint);
				}
			}
			else
			{
				string text2 = $"<color=red>| {this} | Failed to find a Connection Point on {connectsToBiomeController.Biome} Biome's {side} side.</color>";
				Debug.LogFormat(text2);
				biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text2);
			}
		}
		if (!flag)
		{
			string text3 = $"<color=red>| {this} | Failed to find a Connection Point on {connectsToBiomeController.Biome} Biome's {side} side.</color>";
			Debug.LogFormat(text3);
			biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text3);
		}
	}

	private static void SetConnectionPoint(RoomSide side, BiomeController connectsToBiomeController, RoomMetaData transitionRoomMetaData, GridPoint centerMost)
	{
		int num = transitionRoomMetaData.Size.x;
		if (side == RoomSide.Left || side == RoomSide.Right)
		{
			num = transitionRoomMetaData.Size.y;
		}
		List<int> list = new List<int>();
		for (int i = 0; i < num; i++)
		{
			if (GridController.GetIsSpaceForRoomAtDoor(GridController.GetGridCoordsOnGridPointSide(centerMost, side), transitionRoomMetaData.Size, side, i))
			{
				list.Add(i);
			}
		}
		connectsToBiomeController.GridPointManager.SetConnectionPoint(side, centerMost, list);
	}

	protected static GridPoint GetCentermostConnectionPoint(BiomeController biomeController, RoomSide side, IEnumerable<GridPoint> connectionPoints)
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
		return connectionPoints.OrderBy(keySelector).First();
	}
}
