using Rooms;

public class RoomTypeEntry
{
	public RoomType RoomType = RoomType.None;

	public RoomMetaData RoomMetaData;

	public RoomSide Side = RoomSide.Any;

	public bool MustBeEasy;

	public RoomTypeEntry(RoomType roomType)
		: this(roomType, RoomSide.Any, null, mustBeEasy: false)
	{
	}

	public RoomTypeEntry(RoomType roomType, RoomSide side, RoomMetaData roomMetaData, bool mustBeEasy)
	{
		RoomType = roomType;
		Side = side;
		RoomMetaData = roomMetaData;
		MustBeEasy = mustBeEasy;
	}

	public override string ToString()
	{
		string text = "None";
		if (RoomMetaData != null)
		{
			text = RoomMetaData.ID.ToString();
		}
		return $"(Type = {RoomType}. ID = {text}, Side = {Side}, IsEasy = {MustBeEasy})";
	}
}
