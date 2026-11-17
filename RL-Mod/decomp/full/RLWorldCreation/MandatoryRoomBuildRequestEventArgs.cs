namespace RLWorldCreation;

public class MandatoryRoomBuildRequestEventArgs : BiomeCreationEventArgs
{
	public string RoomID { get; }

	public MandatoryRoomBuildRequestEventArgs(string roomID)
	{
		RoomID = roomID;
	}
}
