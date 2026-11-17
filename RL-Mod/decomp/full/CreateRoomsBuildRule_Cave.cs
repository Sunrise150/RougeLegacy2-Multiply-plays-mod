using System.Collections;
using UnityEngine;

public class CreateRoomsBuildRule_Cave : CreateRoomsBuildRule_PlaceRoomOnBiomeSide
{
	public override IEnumerator CreateRooms(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		Vector2Int doorLeadsToGridCoordinates = biomeController.GridPointManager.GridPointManagers[0].GetDoorLeadsToGridCoordinates(new DoorLocation(RoomSide.Bottom, 1));
		CreateSpecificRoom(biomeCreator, biomeController, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.FirstNonTransitionRoom_Cave), doorLeadsToGridCoordinates, new DoorLocation(RoomSide.Top, 0), BiomeType.None, RoomType.None, incrementRoomCount: false);
		yield return base.CreateRooms(biomeCreator, biomeController);
		CreateRoomOnBiomeSide(biomeCreator, biomeController, RoomSide.Right, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntrance_Cave), RoomType.BossEntrance);
		CreateRoomOnBiomeSide(biomeCreator, biomeController, RoomSide.Bottom, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.MinesEntrance_Cave), RoomType.Mandatory);
	}
}
