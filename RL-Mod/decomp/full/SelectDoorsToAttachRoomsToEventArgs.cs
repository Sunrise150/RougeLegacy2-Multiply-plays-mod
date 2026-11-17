using System;
using System.Collections.Generic;
using UnityEngine;

public class SelectDoorsToAttachRoomsToEventArgs : EventArgs
{
	public BaseRoom Room { get; protected set; }

	public Dictionary<RoomSide, List<int>> DoorTable { get; private set; }

	public float Time { get; protected set; }

	public SelectDoorsToAttachRoomsToEventArgs(BaseRoom room, Dictionary<RoomSide, List<int>> doorTable)
	{
		Room = room;
		DoorTable = doorTable;
		Time = UnityEngine.Time.time;
	}
}
