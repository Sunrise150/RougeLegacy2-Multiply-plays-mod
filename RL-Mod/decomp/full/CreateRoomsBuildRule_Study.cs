using System.Collections;

public class CreateRoomsBuildRule_Study : CreateRoomsBuildRule_PlaceRoomOnBiomeSide
{
	public override IEnumerator CreateRooms(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		yield return base.CreateRooms(biomeCreator, biomeController);
		CreateRoomOnBiomeSide(biomeCreator, biomeController, RoomSide.Left, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.SpearKnightMinibossEntrance_Study), RoomType.Bonus);
		CreateRoomOnBiomeSide(biomeCreator, biomeController, RoomSide.Right, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.SecretMemory_Study), RoomType.Mandatory);
		CreateRoomOnBiomeSide(biomeCreator, biomeController, RoomSide.Top, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.BossEntrance_Study), RoomType.BossEntrance);
	}
}
