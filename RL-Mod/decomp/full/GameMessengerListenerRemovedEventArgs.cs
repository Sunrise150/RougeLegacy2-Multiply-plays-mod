using System;
using UnityEngine;

public class GameMessengerListenerRemovedEventArgs : EventArgs
{
	public string Event { get; private set; }

	public MonoBehaviour MonoBehaviour { get; private set; }

	public string Method { get; private set; }

	public int FrameNumber { get; private set; }

	public GameMessengerListenerRemovedEventArgs(string eventID, MonoBehaviour monoBehaviour, string method, int frameNumber)
	{
		Event = eventID;
		MonoBehaviour = monoBehaviour;
		Method = method;
		FrameNumber = Time.frameCount;
	}
}
