using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

public class CreateRoomsBuildRule
{
	protected static RoomCriteriaLookupCacheKey m_roomCriteriaKey;

	protected static void BuildRoom(BiomeCreator biomeCreator, BiomeController biomeController, GridPointManager originRoom, DoorLocation doorLocation, Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit, RoomType roomType, RoomSetEntry[] potentialRooms, BiomeType biomeOverride = BiomeType.None)
	{
		RoomSetEntry randomRoom = default(RoomSetEntry);
		if (potentialRooms.Length > 1)
		{
			randomRoom = biomeCreator.GetRandomRoomPrefab(biomeController, potentialRooms, RoomLibrary.GetSetCollection(biomeController.Biome).GetRNGWeightSum(m_roomCriteriaKey));
			if (!BiomeCreation_EV.GetDuplicateRoomsAllowed(biomeController.Biome))
			{
				for (int i = 0; i < 5; i++)
				{
					bool flag = false;
					for (int j = 0; j < biomeController.GridPointManager.GridPointManagers.Count; j++)
					{
						if (biomeController.GridPointManager.GridPointManagers[j].RoomMetaData == randomRoom.RoomMetaData)
						{
							flag = true;
							break;
						}
					}
					if (!flag)
					{
						break;
					}
					randomRoom = biomeCreator.GetRandomRoomPrefab(biomeController, potentialRooms, RoomLibrary.GetSetCollection(biomeController.Biome).GetRNGWeightSum(m_roomCriteriaKey));
				}
			}
		}
		else
		{
			randomRoom = potentialRooms[0];
		}
		if (potentialRooms.Length > 1 && randomRoom.RoomMetaData == originRoom.RoomMetaData)
		{
			RoomSetEntry[] array = potentialRooms.Where((RoomSetEntry entry) => entry.RoomMetaData != randomRoom.RoomMetaData).ToArray();
			if (array.Length != 0)
			{
				randomRoom = biomeCreator.GetRandomRoomPrefab(biomeController, array, RoomLibrary.GetSetCollection(biomeController.Biome).GetRNGWeightSum(m_roomCriteriaKey));
			}
		}
		if (!randomRoom.Equals(default(RoomSetEntry)))
		{
			DoorLocation randomDoorLocation = biomeCreator.GetRandomDoorLocation(roomSizesThatFit[randomRoom.RoomMetaData.Size], randomRoom, biomeController.Biome, biomeCreator.RoomNumber + 1);
			Vector2Int doorLeadsToGridCoordinates = originRoom.GetDoorLeadsToGridCoordinates(doorLocation);
			BiomeType biomeType = ((biomeOverride == BiomeType.None) ? biomeController.Biome : biomeOverride);
			biomeCreator.BuildRoom(biomeController, roomType, randomRoom, doorLeadsToGridCoordinates, randomDoorLocation, biomeType);
			DebugCreateRooms.BuildRoom(biomeType, doorLeadsToGridCoordinates, randomDoorLocation, randomRoom);
		}
		else
		{
			string text = $"<color=red>| CreateRoomsBuildRule | Failed to get build Random Room.</color>";
			Debug.LogFormat(text);
			biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text);
		}
	}

	protected virtual void BuildRoomsAtRandomDoorLocations(BiomeCreator biomeCreator, BiomeController biomeController, int finalRoomCountInBiome, GridPointManager originRoom, List<RoomSetEntry> standardRoomPoolOverride)
	{
		List<DoorLocation> buildRoomAtDoorLocations = biomeCreator.GetBuildRoomAtDoorLocations(biomeController, originRoom);
		for (int i = 0; i < buildRoomAtDoorLocations.Count; i++)
		{
			GridPointManager originRoom2 = originRoom;
			DoorLocation doorLocation = buildRoomAtDoorLocations[i];
			if (GetNumberOfRoomsPlacedSoFar(biomeController) >= finalRoomCountInBiome || biomeCreator.State == BiomeBuildStateID.Failed)
			{
				break;
			}
			if (!GridController.GetIsGridSpaceAvailable(originRoom.GetDoorLeadsToGridCoordinates(doorLocation)))
			{
				continue;
			}
			Dictionary<Vector2Int, List<DoorLocation>> dictionary = biomeCreator.GetRoomSizesAndWhatDoorLocationsTheyFitAt(biomeController.Border, RoomLibrary.GetSetCollection(biomeController.Biome).MaxRoomSize, originRoom, doorLocation);
			RoomTypeEntry targetRoomRequirements = biomeCreator.GetTargetRoomRequirements();
			RoomType roomType = RoomType.None;
			RoomSetEntry[] array = GetPotentialRooms(biomeCreator, biomeController, dictionary, targetRoomRequirements, standardRoomPoolOverride, out roomType);
			if (targetRoomRequirements.RoomMetaData != null && array == null)
			{
				List<GridPointManager> gridPointManagersWithAvailableDoors = GetGridPointManagersWithAvailableDoors(biomeController);
				RoomUtility.ShuffleList(gridPointManagersWithAvailableDoors);
				bool flag = true;
				Dictionary<Vector2Int, List<DoorLocation>> dictionary2 = new Dictionary<Vector2Int, List<DoorLocation>>();
				for (int j = 0; j < gridPointManagersWithAvailableDoors.Count; j++)
				{
					if (!flag)
					{
						break;
					}
					GridPointManager gridPointManager = gridPointManagersWithAvailableDoors[j];
					List<DoorLocation> doorLocations = gridPointManager.DoorLocations;
					RoomUtility.ShuffleList(doorLocations);
					for (int k = 0; k < doorLocations.Count; k++)
					{
						if (!flag)
						{
							break;
						}
						if (gridPointManager.GetConnectedRoom(doorLocations[k]) != null)
						{
							continue;
						}
						RoomSide oppositeSide = RoomUtility.GetOppositeSide(doorLocations[k].RoomSide);
						List<DoorLocation> list = targetRoomRequirements.RoomMetaData.GetDoorsOnSide(oppositeSide).ToList();
						RoomUtility.ShuffleList(list);
						for (int l = 0; l < list.Count; l++)
						{
							if (GridController.GetIsSpaceForRoomAtDoor(gridPointManager.GetDoorLeadsToGridCoordinates(doorLocations[k]), targetRoomRequirements.RoomMetaData.Size, doorLocations[k].RoomSide, list[l].DoorNumber))
							{
								originRoom2 = gridPointManager;
								doorLocation = doorLocations[k];
								Dictionary<Vector2Int, List<DoorLocation>> obj = new Dictionary<Vector2Int, List<DoorLocation>> { 
								{
									targetRoomRequirements.RoomMetaData.Size,
									new List<DoorLocation> { list[l] }
								} };
								roomType = targetRoomRequirements.RoomType;
								dictionary2 = obj;
								array = new RoomSetEntry[1]
								{
									new RoomSetEntry(targetRoomRequirements.RoomMetaData)
								};
								flag = false;
							}
						}
					}
				}
				if (array != null)
				{
					dictionary = dictionary2;
				}
				else if (biomeController.GridPointManager.StandardRoomCount < biomeController.TargetRoomCountsByRoomType[RoomType.Standard])
				{
					biomeCreator.AddRoomTypeToBacklog(targetRoomRequirements);
					roomType = RoomType.Standard;
					array = GetSetOfPotentialRooms(biomeController.Biome, roomType, dictionary, SpecialRoomType.None, mustBeEasy: false, standardRoomPoolOverride);
				}
				else
				{
					string text = $"<color=red> | {biomeController.Biome} | One or more Mandatory Rooms could not be placed during biome creation (including {targetRoomRequirements.RoomMetaData.ID})</color>";
					Debug.Log(text);
					biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text);
				}
			}
			else if (targetRoomRequirements.RoomType != 0 && array.Length == 0)
			{
				List<GridPointManager> gridPointManagersWithAvailableDoors2 = GetGridPointManagersWithAvailableDoors(biomeController);
				RoomUtility.ShuffleList(gridPointManagersWithAvailableDoors2);
				bool flag2 = true;
				Dictionary<Vector2Int, List<DoorLocation>> dictionary3 = new Dictionary<Vector2Int, List<DoorLocation>>();
				for (int m = 0; m < gridPointManagersWithAvailableDoors2.Count; m++)
				{
					if (!flag2)
					{
						break;
					}
					GridPointManager gridPointManager2 = gridPointManagersWithAvailableDoors2[m];
					List<DoorLocation> doorLocations2 = gridPointManager2.DoorLocations;
					RoomUtility.ShuffleList(doorLocations2);
					for (int n = 0; n < doorLocations2.Count; n++)
					{
						if (!flag2)
						{
							break;
						}
						if (gridPointManager2.GetConnectedRoom(doorLocations2[n]) == null)
						{
							roomType = targetRoomRequirements.RoomType;
							dictionary3 = biomeCreator.GetRoomSizesAndWhatDoorLocationsTheyFitAt(biomeController.Border, RoomLibrary.MaxRoomSize, gridPointManager2, doorLocations2[n]);
							array = GetPotentialRoomsWhenRoomIsNotStandard(biomeCreator, biomeController, dictionary, targetRoomRequirements, out roomType, biomeController.Biome);
							if (array.Length != 0)
							{
								originRoom2 = gridPointManager2;
								doorLocation = doorLocations2[n];
								flag2 = false;
							}
						}
					}
				}
				if (array.Length != 0)
				{
					dictionary = dictionary3;
				}
				else if (biomeController.GridPointManager.StandardRoomCount < biomeController.TargetRoomCountsByRoomType[RoomType.Standard])
				{
					biomeCreator.AddRoomTypeToBacklog(targetRoomRequirements);
					roomType = RoomType.Standard;
					array = GetSetOfPotentialRooms(biomeController.Biome, roomType, dictionary, SpecialRoomType.None, mustBeEasy: false, standardRoomPoolOverride);
				}
				else if (targetRoomRequirements.RoomType != RoomType.Trap)
				{
					string text2 = $"<color=red> | {biomeController.Biome} | One or more Bonus Rooms weren't placed during biome creation</color>";
					Debug.Log(text2);
					biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text2);
				}
			}
			if (array != null && array.Length != 0)
			{
				BuildRoom(biomeCreator, biomeController, originRoom2, doorLocation, dictionary, roomType, array);
			}
			else
			{
				PrintNoPotentialRoomsErrorMessage(biomeController.Biome, dictionary);
			}
		}
	}

	protected void CreateSpecificRoom(BiomeCreator biomeCreator, BiomeController biomeController, RoomMetaData room, Vector2Int coords, DoorLocation doorLocation, BiomeType biomeOverride = BiomeType.None, RoomType roomTypeOverride = RoomType.None, bool incrementRoomCount = true)
	{
		BiomeType overrideBiome = biomeController.Biome;
		if (biomeOverride != 0)
		{
			overrideBiome = biomeOverride;
		}
		RoomType roomType = RoomType.Mandatory;
		if (roomTypeOverride != RoomType.None)
		{
			roomType = roomTypeOverride;
		}
		biomeCreator.BuildRoom(biomeController, roomType, new RoomSetEntry(room), coords, doorLocation, overrideBiome, incrementRoomCount);
	}

	public static List<GridPointManager> GetGridPointManagersWithAvailableDoors(BiomeController biomeController)
	{
		List<GridPointManager> list = new List<GridPointManager>();
		for (int i = 0; i < biomeController.GridPointManager.GridPointManagers.Count; i++)
		{
			GridPointManager gridPointManager = biomeController.GridPointManager.GridPointManagers[i];
			List<DoorLocation> doorLocations = gridPointManager.DoorLocations;
			for (int j = 0; j < doorLocations.Count; j++)
			{
				DoorLocation doorLocation = doorLocations[j];
				if (gridPointManager.GetConnectedRoom(doorLocation) == null)
				{
					list.Add(gridPointManager);
					break;
				}
			}
		}
		return list;
	}

	protected static int GetNumberOfRoomsPlacedSoFar(BiomeController biomeController)
	{
		return biomeController.GridPointManager.StandardRoomCount + biomeController.GridPointManager.SpecialRoomCount;
	}

	public virtual IEnumerator CreateRooms(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		int num = biomeController.TargetRoomCountsByRoomType.Sum((KeyValuePair<RoomType, int> entry) => entry.Value);
		int num2 = num * 5;
		int num3 = 0;
		List<RoomSetEntry> standardRoomPoolOverride = null;
		while (GetNumberOfRoomsPlacedSoFar(biomeController) < num && biomeCreator.State != BiomeBuildStateID.Failed)
		{
			GridPointManager nextOriginRoom = biomeCreator.GetNextOriginRoom(biomeCreator, biomeController);
			if (num3 <= num2 && nextOriginRoom != null)
			{
				BuildRoomsAtRandomDoorLocations(biomeCreator, biomeController, num, nextOriginRoom, standardRoomPoolOverride);
				num3++;
				continue;
			}
			if (num3 > num2)
			{
				string text = $"<color=red>|{this}| Iteration count exceeded specified maximum ({num2}) while building {biomeController.Biome} Biome. Take note of Seed ({RNGSeedManager.GetCurrentSeed(biomeCreator.gameObject.scene.name)}) and let Caleb or Kenny know.</color>";
				Debug.LogFormat(text);
				biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text);
				DebugCreateRooms.MaxIterationCountReached();
			}
			break;
		}
		yield break;
	}

	protected static RoomSetEntry[] GetDefaultPotentialRooms(Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit, RoomTypeEntry roomRequirements, List<RoomSetEntry> standardRoomPoolOverride, out RoomType roomType, BiomeType biome)
	{
		roomType = roomRequirements.RoomType;
		return GetSetOfPotentialRooms(biome, roomType, roomSizesThatFit, SpecialRoomType.None, roomRequirements.MustBeEasy, standardRoomPoolOverride);
	}

	protected static RoomSetEntry[] GetPotentialRooms(BiomeCreator biomeCreator, BiomeController biomeController, Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit, RoomTypeEntry roomRequirements, List<RoomSetEntry> standardRoomPoolOverride, out RoomType roomType, BiomeType biomeOverride = BiomeType.None)
	{
		BiomeType biome = ((biomeOverride == BiomeType.None) ? biomeController.Biome : biomeOverride);
		if (roomRequirements.RoomMetaData != null)
		{
			return GetPotentialRoomsWhenPrefabIsSet(roomSizesThatFit, roomRequirements, out roomType);
		}
		if (roomRequirements.RoomType != 0)
		{
			return GetPotentialRoomsWhenRoomIsNotStandard(biomeCreator, biomeController, roomSizesThatFit, roomRequirements, out roomType, biome);
		}
		return GetDefaultPotentialRooms(roomSizesThatFit, roomRequirements, standardRoomPoolOverride, out roomType, biome);
	}

	protected static RoomSetEntry[] GetPotentialRoomsWhenPrefabIsSet(Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit, RoomTypeEntry roomRequirements, out RoomType roomType)
	{
		roomType = RoomType.Mandatory;
		bool num = roomSizesThatFit.ContainsKey(roomRequirements.RoomMetaData.Size) && roomSizesThatFit[roomRequirements.RoomMetaData.Size].Any((DoorLocation entry) => roomRequirements.RoomMetaData.GetHasDoor(new DoorLocation(entry.RoomSide, entry.DoorNumber)));
		RoomSetEntry[] result = null;
		if (num)
		{
			roomType = roomRequirements.RoomType;
			result = new RoomSetEntry[1]
			{
				new RoomSetEntry(roomRequirements.RoomMetaData)
			};
		}
		return result;
	}

	protected static RoomSetEntry[] GetPotentialRoomsWhenRoomIsNotStandard(BiomeCreator biomeCreator, BiomeController biomeController, Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit, RoomTypeEntry roomRequirements, out RoomType roomType, BiomeType biome)
	{
		SpecialRoomType specialRoomType = SpecialRoomType.None;
		roomType = roomRequirements.RoomType;
		RoomSetEntry[] array = new RoomSetEntry[0];
		if (roomType == RoomType.Bonus)
		{
			List<SpecialRoomType> list = new List<SpecialRoomType>(BiomeUtility.GetPotentialSpecialRoomTypesInBiome(biomeController.Biome));
			while (list.Count > 0)
			{
				specialRoomType = GetRandomBonusRoomType(biomeController, RngID.BiomeCreation, list);
				array = GetSetOfPotentialRooms(biome, roomType, roomSizesThatFit, specialRoomType, mustBeEasy: false, null);
				if (array.Length != 0)
				{
					break;
				}
				list.Remove(specialRoomType);
			}
		}
		else
		{
			array = GetSetOfPotentialRooms(biome, roomType, roomSizesThatFit, specialRoomType, roomRequirements.MustBeEasy, null);
		}
		return array;
	}

	public static SpecialRoomType GetRandomBonusRoomType(BiomeController biomeController, RngID rngID = RngID.BiomeCreation, List<SpecialRoomType> onlyCheckSpecialRoomTypes = null)
	{
		SpecialRoomType result = SpecialRoomType.None;
		BiomeData data = BiomeDataLibrary.GetData(biomeController.Biome);
		Dictionary<SpecialRoomType, int> dictionary = new Dictionary<SpecialRoomType, int>();
		int num = 0;
		for (int i = 0; i < data.BonusRoomWeights.Length; i++)
		{
			num += data.BonusRoomWeights[i].Weight;
			if (onlyCheckSpecialRoomTypes == null || (onlyCheckSpecialRoomTypes != null && onlyCheckSpecialRoomTypes.Contains(data.BonusRoomWeights[i].Type)))
			{
				dictionary.Add(data.BonusRoomWeights[i].Type, num);
			}
		}
		SpecialRoomType[] specialRoomTypeArray = RoomType_RL.SpecialRoomTypeArray;
		foreach (SpecialRoomType specialRoomType in specialRoomTypeArray)
		{
			if (!dictionary.ContainsKey(specialRoomType))
			{
				int defaultSpecialRoomWeight = BiomeCreation_EV.GetDefaultSpecialRoomWeight(specialRoomType);
				num += defaultSpecialRoomWeight;
				if (onlyCheckSpecialRoomTypes == null || (onlyCheckSpecialRoomTypes != null && onlyCheckSpecialRoomTypes.Contains(specialRoomType)))
				{
					dictionary.Add(specialRoomType, num);
				}
				else if (onlyCheckSpecialRoomTypes != null && !onlyCheckSpecialRoomTypes.Contains(specialRoomType))
				{
					num -= defaultSpecialRoomWeight;
				}
			}
		}
		int randomNumber = RNGManager.GetRandomNumber(rngID, "Get Random Bonus Room Type", 0, num);
		foreach (KeyValuePair<SpecialRoomType, int> item in dictionary)
		{
			if (randomNumber < item.Value)
			{
				result = item.Key;
				break;
			}
		}
		return result;
	}

	protected static RoomSetEntry[] GetSetOfPotentialRooms(BiomeType biome, RoomType roomType, Dictionary<Vector2Int, List<DoorLocation>> roomSizesAndWhatDoorNumbersTheyFitAt, SpecialRoomType specialRoomType, bool mustBeEasy, List<RoomSetEntry> standardRoomPoolOverride)
	{
		RoomSide roomSide = roomSizesAndWhatDoorNumbersTheyFitAt.First().Value[0].RoomSide;
		if (m_roomCriteriaKey == null)
		{
			m_roomCriteriaKey = new RoomCriteriaLookupCacheKey(biome, roomSide, roomType, roomSizesAndWhatDoorNumbersTheyFitAt, specialRoomType, mustBeEasy);
		}
		else
		{
			m_roomCriteriaKey.Initialise(biome, roomSide, roomType, roomSizesAndWhatDoorNumbersTheyFitAt, specialRoomType, mustBeEasy);
		}
		return RoomLibrary.GetSetCollection(biome).GetRoomsMatchingCriteria(m_roomCriteriaKey, standardRoomPoolOverride);
	}

	protected void PrintNoPotentialRoomsErrorMessage(BiomeType biome, Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit)
	{
		if (roomSizesThatFit.Count > 0)
		{
			Debug.LogFormat("<color=purple>| {0} | No Potential Rooms found in {1} Biome's Room Library. See following console log for more details.</color>", this, biome);
			string text = "";
			for (int i = 0; i < roomSizesThatFit.Count; i++)
			{
				KeyValuePair<Vector2Int, List<DoorLocation>> keyValuePair = roomSizesThatFit.ElementAt(i);
				if (keyValuePair.Value.Count == 0)
				{
					continue;
				}
				text += $"<b>{keyValuePair.Key.x}x{keyValuePair.Key.y}:</b>";
				text += "<";
				for (int j = 0; j < keyValuePair.Value.Count; j++)
				{
					DoorLocation doorLocation = keyValuePair.Value[j];
					text += $"{doorLocation.RoomSide}{doorLocation.DoorNumber}";
					if (j < keyValuePair.Value.Count - 1)
					{
						text += ", ";
					}
				}
				text += ">";
				if (i < roomSizesThatFit.Count - 1)
				{
					text += " ";
				}
			}
			if (!string.IsNullOrEmpty(text))
			{
				Debug.LogFormat("<color=purple>Cont'd from above... Does Room Library contain any Rooms matching the following criteria?: {1}</color>", this, text);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>|{0}| No Potential Rooms found in {1} Biome's Room Library because no Rooms fit</color>", this, biome);
		}
	}
}
