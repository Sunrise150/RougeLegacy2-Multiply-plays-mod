using System;
using UnityEngine;

public class MessengerListenerChangeEventArgs : EventArgs
{
	public string Event { get; private set; }

	public MonoBehaviour MonoBehaviour { get; private set; }

	public string Method { get; private set; }

	public int FrameNumber { get; private set; }

	public MessengerListenerChangeEventArgs(string eventID, MonoBehaviour monoBehaviour, string method, int frameNumber)
	{
		Initialize(eventID, monoBehaviour, method, frameNumber);
	}

	public void Initialize(string eventID, MonoBehaviour monoBehaviour, string method, int frameNumber)
	{
		Event = eventID;
		MonoBehaviour = monoBehaviour;
		Method = method;
		FrameNumber = frameNumber;
	}
}
