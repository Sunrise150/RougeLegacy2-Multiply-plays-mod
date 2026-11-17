using System;
using Rooms;

[Serializable]
public class MandatoryRoomEntry
{
	public string Description = string.Empty;

	public RoomType RoomType = RoomType.None;

	public RoomMetaData RoomMetaData;

	public ConditionFlag ReplacementCriteria;

	public RoomMetaData ReplacementRoom;
}
