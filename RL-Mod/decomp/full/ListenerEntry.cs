using System;
using UnityEngine;

[Serializable]
public class ListenerEntry
{
	public string Event;

	public MonoBehaviour MonoBehaviour;

	public string MonoBehaviourName;

	public string Method;

	public int FrameNumber;

	public ListenerEntry(string eventID, MonoBehaviour monoBehaviour, string method, int frameNumber)
	{
		Event = eventID;
		MonoBehaviour = monoBehaviour;
		MonoBehaviourName = monoBehaviour.ToString();
		Method = method;
		FrameNumber = Time.frameCount;
	}
}
