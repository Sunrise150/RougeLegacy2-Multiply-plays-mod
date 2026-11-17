using System;
using UnityEngine;

public class TimeEventArgs : EventArgs
{
	public float Time { get; protected set; }

	public TimeEventArgs()
	{
		Time = UnityEngine.Time.time;
	}
}
