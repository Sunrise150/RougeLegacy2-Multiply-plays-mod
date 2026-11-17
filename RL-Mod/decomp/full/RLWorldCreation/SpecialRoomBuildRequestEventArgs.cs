namespace RLWorldCreation;

public class SpecialRoomBuildRequestEventArgs : BiomeCreationEventArgs
{
	public SpecialRoomType SpecialRoomType { get; }

	public bool IsSuccess { get; }

	public SpecialRoomBuildRequestEventArgs(SpecialRoomType specialRoomType, bool isSuccess)
	{
		SpecialRoomType = specialRoomType;
		IsSuccess = isSuccess;
	}
}
