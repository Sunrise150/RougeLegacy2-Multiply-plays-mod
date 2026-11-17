using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

public class CreateRoomsBuildRule_Tower : CreateRoomsBuildRule
{
	protected Bounds m_defaultBorder;

	protected Bounds m_tier1Border;

	protected Bounds m_tier2Border;

	protected Bounds m_tier3Border;

	private static readonly List<DoorLocation> m_topDoorList = new List<DoorLocation>
	{
		new DoorLocation(RoomSide.Top, 0),
		new DoorLocation(RoomSide.Top, 1),
		new DoorLocation(RoomSide.Top, 2)
	};

	private BiomeType Tier01Biome => BiomeType.Tower;

	private BiomeType Tier02Biome => BiomeType.TowerExterior;

	private BiomeType Tier03Biome => BiomeType.TowerExterior;

	private Dictionary<RoomSide, int> Tier01Border => new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			1
		},
		{
			RoomSide.Right,
			-1
		},
		{
			RoomSide.Top,
			500
		},
		{
			RoomSide.Bottom,
			4
		}
	};

	private Dictionary<RoomSide, int> Tier02Border => new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			-1
		},
		{
			RoomSide.Right,
			-3
		},
		{
			RoomSide.Top,
			500
		},
		{
			RoomSide.Bottom,
			4
		}
	};

	private Dictionary<RoomSide, int> Tier03Border => new Dictionary<RoomSide, int>
	{
		{
			RoomSide.Left,
			3
		},
		{
			RoomSide.Right,
			1
		},
		{
			RoomSide.Top,
			500
		},
		{
			RoomSide.Bottom,
			4
		}
	};

	public override IEnumerator CreateRooms(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		CreateBorders(biomeController);
		yield return base.CreateRooms(biomeCreator, biomeController);
		CreateTopOfTower(biomeCreator, biomeController);
		CreateJournalRoom(biomeCreator, biomeController);
	}

	protected override void BuildRoomsAtRandomDoorLocations(BiomeCreator biomeCreator, BiomeController biomeController, int finalRoomCountInBiome, GridPointManager originRoom, List<RoomSetEntry> standardRoomPoolOverride)
	{
		foreach (DoorLocation buildRoomAtDoorLocation in biomeCreator.GetBuildRoomAtDoorLocations(biomeController, originRoom))
		{
			if (CreateRoomsBuildRule.GetNumberOfRoomsPlacedSoFar(biomeController) >= finalRoomCountInBiome || biomeCreator.State == BiomeBuildStateID.Failed)
			{
				break;
			}
			if (!GridController.GetIsGridSpaceAvailable(originRoom.GetDoorLeadsToGridCoordinates(buildRoomAtDoorLocation)))
			{
				continue;
			}
			Vector2Int doorLeadsToGridCoordinates = originRoom.GetDoorLeadsToGridCoordinates(buildRoomAtDoorLocation);
			TierID tier = GetTier(doorLeadsToGridCoordinates);
			if (tier != TierID.None)
			{
				BiomeType biome = GetBiome(tier);
				Bounds border = GetBorder(tier);
				RoomTypeEntry targetRoomRequirements = biomeCreator.GetTargetRoomRequirements();
				Dictionary<Vector2Int, List<DoorLocation>> roomSizesAndWhatDoorLocationsTheyFitAt = biomeCreator.GetRoomSizesAndWhatDoorLocationsTheyFitAt(border, RoomLibrary.GetSetCollection(biome).MaxRoomSize, originRoom, buildRoomAtDoorLocation);
				RoomType roomType;
				RoomSetEntry[] potentialRooms = GetPotentialRooms(biomeCreator, biomeController, roomSizesAndWhatDoorLocationsTheyFitAt, targetRoomRequirements, standardRoomPoolOverride, out roomType, biome, tier);
				if (potentialRooms.Length != 0)
				{
					CreateRoomsBuildRule.BuildRoom(biomeCreator, biomeController, originRoom, buildRoomAtDoorLocation, roomSizesAndWhatDoorLocationsTheyFitAt, roomType, potentialRooms, biome);
					continue;
				}
				biomeCreator.AddRoomTypeToBacklog(targetRoomRequirements);
				PrintNoPotentialRoomsErrorMessage(biome, roomSizesAndWhatDoorLocationsTheyFitAt);
			}
		}
	}

	private void CreateBorders(BiomeController biomeController)
	{
		GridPointManager transitionRoom = biomeController.GridPointManager.GridPointManagers.First();
		m_defaultBorder = biomeController.GridPointManager.Border;
		m_tier1Border = GetBounds(Tier01Border, transitionRoom);
		m_tier2Border = GetBounds(Tier02Border, transitionRoom);
		m_tier3Border = GetBounds(Tier03Border, transitionRoom);
	}

	protected Bounds GetBounds(Dictionary<RoomSide, int> border, GridPointManager transitionRoom)
	{
		int num = transitionRoom.GridCoordinates.x + border[RoomSide.Left];
		int num2 = transitionRoom.GridCoordinates.y + border[RoomSide.Bottom];
		int num3 = transitionRoom.GridCoordinates.x + transitionRoom.Size.x + border[RoomSide.Right];
		int num4 = transitionRoom.GridCoordinates.y + transitionRoom.Size.y + border[RoomSide.Top];
		Bounds result = default(Bounds);
		result.SetMinMax(new Vector2(num, num2), new Vector2(num3, num4));
		return result;
	}

	protected BiomeType GetBiome(TierID tier)
	{
		BiomeType result = Tier01Biome;
		switch (tier)
		{
		case TierID.Two:
			result = Tier02Biome;
			break;
		case TierID.Three:
			result = Tier03Biome;
			break;
		}
		return result;
	}

	protected Bounds GetBorder(TierID tier)
	{
		Bounds result = m_tier1Border;
		switch (tier)
		{
		case TierID.Two:
			result = m_tier2Border;
			break;
		case TierID.Three:
			result = m_tier3Border;
			break;
		}
		return result;
	}

	private bool GetAreCoordinatesWithinBorder(Vector2Int coordinates, Bounds border)
	{
		if ((float)coordinates.x >= border.max.x || (float)coordinates.y >= border.max.y)
		{
			return false;
		}
		return border.Contains((Vector2)coordinates);
	}

	public TierID GetTier(Vector2Int coords)
	{
		if (GetAreCoordinatesWithinBorder(coords, m_tier1Border))
		{
			return TierID.One;
		}
		if (GetAreCoordinatesWithinBorder(coords, m_tier2Border))
		{
			return TierID.Two;
		}
		if (GetAreCoordinatesWithinBorder(coords, m_tier3Border))
		{
			return TierID.Three;
		}
		return TierID.None;
	}

	private void CreateTopOfTower(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		ReplaceRoomAtTopIfNecessary(biomeCreator, biomeController);
		int num = (int)m_tier1Border.min.x;
		int y = biomeController.GridPointManager.Extents[RoomSide.Top];
		RoomMetaData roomMetaData = RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntranceCentre_Tower);
		CreateSpecificRoom(biomeCreator, biomeController, roomMetaData, new Vector2Int(num, y), new DoorLocation(RoomSide.Bottom, 0), BiomeType.TowerExterior, RoomType.BossEntrance);
		RoomMetaData roomMetaData2 = RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntranceLeft_Tower);
		CreateSpecificRoom(biomeCreator, biomeController, roomMetaData2, new Vector2Int(num - roomMetaData2.Size.x, y), new DoorLocation(RoomSide.Bottom, 0), BiomeType.TowerExterior, RoomType.BossEntrance);
		RoomMetaData roomMetaData3 = RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntranceRight_Tower);
		CreateSpecificRoom(biomeCreator, biomeController, roomMetaData3, new Vector2Int(num + roomMetaData3.Size.x, y), new DoorLocation(RoomSide.Bottom, 0), BiomeType.TowerExterior, RoomType.BossEntrance);
	}

	private void CreateJournalRoom(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		bool flag = false;
		DoorLocation doorLocation = default(DoorLocation);
		Vector2Int coords = default(Vector2Int);
		List<GridPointManager> list = new List<GridPointManager>(biomeController.GridPointManager.GridPointManagers);
		RoomUtility.ShuffleList(list);
		foreach (GridPointManager item in list)
		{
			foreach (DoorLocation doorLocation2 in item.DoorLocations)
			{
				Vector2Int doorLeadsToGridCoordinates = item.GetDoorLeadsToGridCoordinates(doorLocation2);
				if (GetTier(doorLeadsToGridCoordinates) == TierID.One && biomeCreator.GetIsDoorLocationValid(biomeController, item, doorLocation2))
				{
					flag = true;
					doorLocation = new DoorLocation(RoomUtility.GetOppositeSide(doorLocation2.RoomSide), 0);
					coords = doorLeadsToGridCoordinates;
					break;
				}
			}
			if (flag)
			{
				break;
			}
		}
		if (flag)
		{
			CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.Journal_Tower), coords, doorLocation);
			return;
		}
		string text = "<color=red>| CreateRoomsBuildRule_Tower | The Tower Interior had no available spaces in which to put the Journal room.</color>";
		Debug.Log(text);
		biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text);
	}

	private RoomSetEntry[] GetPotentialRooms(BiomeCreator biomeCreator, BiomeController biomeController, Dictionary<Vector2Int, List<DoorLocation>> roomSizesThatFit, RoomTypeEntry roomRequirements, List<RoomSetEntry> standardRoomPool, out RoomType roomType, BiomeType biome, TierID tier)
	{
		roomType = RoomType.None;
		List<RoomSetEntry> standardRoomPoolOverride = standardRoomPool;
		if (biome == BiomeType.TowerExterior)
		{
			standardRoomPoolOverride = null;
		}
		RoomSetEntry[] array = CreateRoomsBuildRule.GetPotentialRooms(biomeCreator, biomeController, roomSizesThatFit, roomRequirements, standardRoomPoolOverride, out roomType, biome);
		if (tier == TierID.Two && array.Length != 0)
		{
			List<RoomSetEntry> second = array.Where((RoomSetEntry room) => room.IsMirrored).ToList();
			array = array.Intersect(second).ToArray();
		}
		else if (tier == TierID.Three && array.Length != 0)
		{
			List<RoomSetEntry> second2 = array.Where((RoomSetEntry room) => !room.IsMirrored).ToList();
			array = array.Intersect(second2).ToArray();
		}
		return array;
	}

	private static void ReplaceRoomAtTopIfNecessary(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		List<GridPointManager> list = (from point in biomeCreator.GetGridPointsAtExtent(biomeController, RoomSide.Top, 0)
			select point.Owner).Distinct().ToList();
		if (list.Any((GridPointManager originRoom) => originRoom.DoorLocations.Any((DoorLocation location) => location.RoomSide == RoomSide.Top)))
		{
			return;
		}
		int num = -1;
		RoomSetEntry roomSetEntry = default(RoomSetEntry);
		for (int i = 0; i < list.Count; i++)
		{
			RoomSetEntry replacementRoom = ReplaceRoomUtility.GetReplacementRoom(biomeController, list[i], list[i].DoorLocations, m_topDoorList, matchMirrored: true);
			if (replacementRoom.RoomMetaData != null)
			{
				num = i;
				roomSetEntry = replacementRoom;
				if (!list[i].RoomMetaData.IsSpecialRoom)
				{
					break;
				}
			}
		}
		if (num != -1)
		{
			list[num].SetRoomMetaData(roomSetEntry.RoomMetaData, roomSetEntry.IsMirrored);
			return;
		}
		string text = "<color=red>| CreateRoomsBuildRule_Tower | The topmost rooms of the tower did not have any top doors, and no suitable replacements were found.</color>";
		Debug.Log(text);
		biomeCreator.SetState(biomeController, BiomeBuildStateID.Failed, text);
	}
}
