using System.Collections;

public class CreateRoomsBuildRule_Forest : CreateRoomsBuildRule_PlaceRoomOnBiomeSide
{
	public override IEnumerator CreateRooms(BiomeCreator biomeCreator, BiomeController biomeController)
	{
		yield return base.CreateRooms(biomeCreator, biomeController);
		CreateRoomOnBiomeSide(biomeCreator, biomeController, RoomSide.Right, RoomReferenceLibrary.GetRoomMetaData(RoomReferenceType.HeirloomDoubleJump_Forest));
	}
}
