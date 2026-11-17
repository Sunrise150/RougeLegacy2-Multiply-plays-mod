using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

public class CreateRoomsBuildRule_PlaceRoomOnBiomeSide : CreateRoomsBuildRule
{
	protected void CreateRoomOnBiomeSide(BiomeCreator biomeCreator, BiomeController biomeController, RoomSide biomeSide, RoomMetaData roomMetaData, RoomType roomTypeOverride = RoomType.None)
	{
		Dictionary<GridPoint, List<DoorLocation>> dictionary = null;
		GridPoint gridPoint = null;
		for (int i = 0; i < 4; i++)
		{
			switch (i)
			{
			case 0:
				dictionary = GetGridPointsOnBorderWithAvailableDoorsWhereMandatoryRoomFits(biomeController, roomMetaData, biomeSide);
				if (dictionary.Count > 0)
				{
					gridPoint = GetConnectionPoint(dictionary);
				}
				break;
			case 1:
				dictionary = GetGridPointsOnBorderWithNoAvailableDoorsWhereMandatoryRoomFits(biomeController, roomMetaData, biomeSide);
				if (dictionary.Count > 0)
				{
					gridPoint = ReplaceRandomRoomWithNoDoorWithOneWithDoor(biomeController, dictionary, biomeSide);
				}
				break;
			case 2:
				dictionary = GetGridPointsWithAvailableDoorsWhereMandatoryRoomFits(biomeController, roomMetaData, biomeSide);
				if (dictionary.Count > 0)
				{
					gridPoint = GetConnectionPoint(dictionary);
				}
				break;
			case 3:
				dictionary = GetGridPointsWithNoAvailableDoorsWhereMandatoryRoomFits(biomeController, roomMetaData, biomeSide);
				if (dictionary.Count > 0)
				{
					gridPoint = ReplaceRandomRoomWithNoDoorWithOneWithDoor(biomeController, dictionary, biomeSide);
				}
				break;
			}
			if (gridPoint != null)
			{
				break;
			}
		}
		if (gridPoint != null)
		{
			Vector2Int gridCoordsOnGridPointSide = GridController.GetGridCoordsOnGridPointSide(gridPoint, biomeSide);
			List<DoorLocation> list = dictionary[gridPoint];
			DoorLocation doorLocation = list[0];
			if (list.Count > 1)
			{
				int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, "Choose Random Door Location", 0, list.Count);
				doorLocation = list[randomNumber];
			}
			CreateSpecificRoom(biomeCreator, biomeController, roomMetaData, gridCoordsOnGridPointSide, doorLocation, BiomeType.None, roomTypeOverride);
		}
		else
		{
			string text = $"<color=red>| {this} | There are no Rooms in this biome with available Doors where the Mandatory Room would fit and no Rooms that we could replace.</color>";
			Debug.LogFormat(text);
			biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text);
		}
	}

	private Dictionary<GridPoint, List<DoorLocation>> GetGridPointsOnBorderWithAvailableDoorsWhereMandatoryRoomFits(BiomeController biomeController, RoomMetaData MandatoryRoomMetaData, RoomSide biomeSide)
	{
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(biomeSide);
		List<DoorLocation> list = MandatoryRoomMetaData.GetDoorsOnSide(oppositeSide).ToList();
		Dictionary<GridPoint, List<DoorLocation>> dictionary = new Dictionary<GridPoint, List<DoorLocation>>();
		foreach (DoorLocation item in list)
		{
			List<GridPoint> gridPointsOnBorderWhereRoomFits = FindSpaceForRoomUtility.GetGridPointsOnBorderWhereRoomFits(biomeController, MandatoryRoomMetaData, isMirrored: false, item);
			AddPotentialConnectionPoints(dictionary, item, gridPointsOnBorderWhereRoomFits);
		}
		return dictionary;
	}

	private Dictionary<GridPoint, List<DoorLocation>> GetGridPointsWithAvailableDoorsWhereMandatoryRoomFits(BiomeController biomeController, RoomMetaData MandatoryRoomMetaData, RoomSide biomeSide)
	{
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(biomeSide);
		List<DoorLocation> list = MandatoryRoomMetaData.GetDoorsOnSide(oppositeSide).ToList();
		Dictionary<GridPoint, List<DoorLocation>> dictionary = new Dictionary<GridPoint, List<DoorLocation>>();
		foreach (DoorLocation item in list)
		{
			List<GridPoint> gridPointsWhereRoomFits = FindSpaceForRoomUtility.GetGridPointsWhereRoomFits(biomeController, MandatoryRoomMetaData, isMirrored: false, item);
			AddPotentialConnectionPoints(dictionary, item, gridPointsWhereRoomFits);
		}
		return dictionary;
	}

	private Dictionary<GridPoint, List<DoorLocation>> GetGridPointsWithNoAvailableDoorsWhereMandatoryRoomFits(BiomeController biomeController, RoomMetaData MandatoryRoomMetaData, RoomSide biomeSide)
	{
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(biomeSide);
		List<DoorLocation> list = MandatoryRoomMetaData.GetDoorsOnSide(oppositeSide).ToList();
		Dictionary<GridPoint, List<DoorLocation>> dictionary = new Dictionary<GridPoint, List<DoorLocation>>();
		foreach (DoorLocation item in list)
		{
			List<GridPoint> gridPointsWhereRoomFits = FindSpaceForRoomUtility.GetGridPointsWhereRoomFits(biomeController, MandatoryRoomMetaData, isMirrored: false, item, checkForAvailableDoors: false, RoomType.Standard);
			AddPotentialConnectionPoints(dictionary, item, gridPointsWhereRoomFits);
		}
		return dictionary;
	}

	private Dictionary<GridPoint, List<DoorLocation>> GetGridPointsOnBorderWithNoAvailableDoorsWhereMandatoryRoomFits(BiomeController biomeController, RoomMetaData MandatoryRoomMetaData, RoomSide biomeSide)
	{
		RoomSide oppositeSide = RoomUtility.GetOppositeSide(biomeSide);
		List<DoorLocation> list = MandatoryRoomMetaData.GetDoorsOnSide(oppositeSide).ToList();
		Dictionary<GridPoint, List<DoorLocation>> dictionary = new Dictionary<GridPoint, List<DoorLocation>>();
		foreach (DoorLocation item in list)
		{
			List<GridPoint> gridPointsOnBorderWhereRoomFits = FindSpaceForRoomUtility.GetGridPointsOnBorderWhereRoomFits(biomeController, MandatoryRoomMetaData, isMirrored: false, item, checkForAvailableDoors: false);
			AddPotentialConnectionPoints(dictionary, item, gridPointsOnBorderWhereRoomFits);
		}
		return dictionary;
	}

	private static void AddPotentialConnectionPoints(Dictionary<GridPoint, List<DoorLocation>> potentialConnectionPoints, DoorLocation doorLocation, List<GridPoint> connectionPoints)
	{
		for (int i = 0; i < connectionPoints.Count; i++)
		{
			if (!potentialConnectionPoints.ContainsKey(connectionPoints[i]))
			{
				potentialConnectionPoints.Add(connectionPoints[i], new List<DoorLocation>());
			}
			potentialConnectionPoints[connectionPoints[i]].Add(doorLocation);
		}
	}

	private GridPoint GetConnectionPoint(Dictionary<GridPoint, List<DoorLocation>> potentialConnectionPoints)
	{
		GridPoint key = potentialConnectionPoints.First().Key;
		if (potentialConnectionPoints.Count > 1)
		{
			int randomNumber = RNGManager.GetRandomNumber(RngID.BiomeCreation, "Get Random Connection Point", 0, potentialConnectionPoints.Count);
			key = potentialConnectionPoints.ElementAt(randomNumber).Key;
		}
		return key;
	}

	private static GridPoint ReplaceRandomRoomWithNoDoorWithOneWithDoor(BiomeController biomeController, Dictionary<GridPoint, List<DoorLocation>> potentialConnectionPoints, RoomSide biomeSide)
	{
		GridPoint result = null;
		foreach (KeyValuePair<GridPoint, List<DoorLocation>> potentialConnectionPoint in potentialConnectionPoints)
		{
			GridPoint key = potentialConnectionPoint.Key;
			if (ReplaceRoomUtility.ReplaceRoomMetaData(biomeController, key, biomeSide))
			{
				result = key;
				break;
			}
		}
		return result;
	}
}
