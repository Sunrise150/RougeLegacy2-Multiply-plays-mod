using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Rooms;
using UnityEngine;

public class CreateRoomsBuildRule_Stone : CreateRoomsBuildRule
{
	private bool m_isToTowerBuilt;

	private bool m_isFirstSplitBuilt;

	private bool m_isSecondSplitBuilt;

	private const bool BUILD_TO_TOWER = true;

	public static int BRIDGE_HEIGHT = 3;

	public override IEnumerator CreateRooms(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		if (biomeCreator.BuildQueue.Peek().RoomType == RoomType.Transition)
		{
			biomeCreator.BuildQueue.Dequeue();
		}
		CreateStartOfBridge(biomeCreator, biomeController);
		yield return base.CreateRooms(biomeCreator, biomeController);
		CreateEndOfBridge(biomeCreator, biomeController);
	}

	private void CreateStartOfBridge(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		GridPointManager gridPointManager = biomeController.GridPointManager.GridPointManagers.First();
		int x = gridPointManager.GridCoordinates.x + gridPointManager.Size.x;
		int y = gridPointManager.GridCoordinates.y;
		CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.FirstNonTransitionRoom_Bridge), new Vector2Int(x, y), new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.Standard, incrementRoomCount: false);
	}

	private void CreateEndOfBridge(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		GridPointManager gridPointManager = biomeController.GridPointManager.GridPointManagers.First();
		int x = biomeController.GridPointManager.Extents[RoomSide.Right];
		int y = gridPointManager.GridCoordinates.y;
		CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.SecondLastRoom_Bridge), new Vector2Int(x, y), new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.Standard);
		x = biomeController.GridPointManager.Extents[RoomSide.Right];
		CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.LastRoom_Bridge), new Vector2Int(x, y), new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.BossEntrance);
	}

	protected override void BuildRoomsAtRandomDoorLocations(BiomeCreator biomeCreator, BiomeController biomeController, int finalRoomCountInBiome, GridPointManager originRoom, List<RoomSetEntry> standardRoomPoolOverride)
	{
		int num = biomeController.TargetRoomCountsByRoomType.Sum((KeyValuePair<RoomType, int> entry) => entry.Value) + 2 + 3 + 2;
		int num2 = (int)((float)num * 0.25f);
		int num3 = (int)((float)num * 0.5f);
		int num4 = (int)((float)num * 0.75f);
		if (!m_isToTowerBuilt && biomeCreator.RoomNumber > num3)
		{
			int x = biomeController.GridPointManager.Extents[RoomSide.Right];
			int y = originRoom.GridCoordinates.y;
			Vector2Int coords = new Vector2Int(x, y);
			CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.PreTowerRoom_Bridge), coords, new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.Standard, incrementRoomCount: false);
			coords.x++;
			CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.ToTowerBiome_Bridge), coords, new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.None, incrementRoomCount: false);
			coords.x += 2;
			CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.PostTowerRoom_Bridge), coords, new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.Standard, incrementRoomCount: false);
			m_isToTowerBuilt = true;
		}
		else if ((!m_isFirstSplitBuilt && biomeCreator.RoomNumber > num2) || (!m_isSecondSplitBuilt && biomeCreator.RoomNumber > num4))
		{
			int x2 = biomeController.GridPointManager.Extents[RoomSide.Right];
			int y2 = originRoom.GridCoordinates.y;
			Vector2Int coords2 = new Vector2Int(x2, y2);
			CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.SplitRoomLeft_Bridge), coords2, new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.Standard, incrementRoomCount: false);
			coords2.x++;
			CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.SplitRoomRight_Bridge), coords2, new DoorLocation(RoomSide.Left, 2), BiomeType.None, RoomType.Standard, incrementRoomCount: false);
			if (!m_isFirstSplitBuilt)
			{
				m_isFirstSplitBuilt = true;
			}
			else if (!m_isSecondSplitBuilt)
			{
				m_isSecondSplitBuilt = true;
			}
		}
		else
		{
			base.BuildRoomsAtRandomDoorLocations(biomeCreator, biomeController, finalRoomCountInBiome, originRoom, standardRoomPoolOverride);
		}
	}
}
