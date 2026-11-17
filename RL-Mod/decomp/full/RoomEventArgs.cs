using System;
using UnityEngine;

public class RoomEventArgs : EventArgs
{
	public BaseRoom Room { get; protected set; }

	public float Time { get; protected set; }

	public RoomEventArgs(BaseRoom room)
	{
		Initialize(room);
	}

	public void Initialize(BaseRoom room)
	{
		Room = room;
		Time = UnityEngine.Time.time;
	}
}
