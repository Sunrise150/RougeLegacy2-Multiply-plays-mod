using System;
using System.Collections.Generic;
using Sigtrap.Relays;
using UnityEngine;

public class Messenger<BaseClassName, EventIDs> : MonoBehaviour where BaseClassName : MonoBehaviour
{
	protected static Dictionary<EventIDs, Relay<MonoBehaviour, EventArgs>> m_eventTable = new Dictionary<EventIDs, Relay<MonoBehaviour, EventArgs>>();

	private static BaseClassName m_instance;

	protected static BaseClassName Instance { get; set; }

	public static Dictionary<EventIDs, Relay<MonoBehaviour, EventArgs>> EventTable
	{
		get
		{
			return m_eventTable;
		}
		protected set
		{
			m_eventTable = value;
		}
	}

	protected static BaseClassName CreateInstance()
	{
		BaseClassName val = CDGHelper.FindStaticInstance<BaseClassName>(createNewIfNull: true);
		UnityEngine.Object.DontDestroyOnLoad(val);
		return val;
	}

	public static void AddListener(EventIDs eventID, Action<MonoBehaviour, EventArgs> listener)
	{
		if (GameManager.IsApplicationClosing || listener == null)
		{
			return;
		}
		if (!Instance)
		{
			Instance = CreateInstance();
			if (Instance == null)
			{
				return;
			}
		}
		if (EventTable.ContainsKey(eventID))
		{
			EventTable[eventID].AddListener(listener);
			return;
		}
		EventTable.Add(eventID, new Relay<MonoBehaviour, EventArgs>());
		EventTable[eventID].AddListener(listener);
	}

	public static void RemoveListener(EventIDs eventID, Action<MonoBehaviour, EventArgs> listener)
	{
		if (!GameManager.IsApplicationClosing && listener != null && EventTable.ContainsKey(eventID))
		{
			EventTable[eventID].RemoveListener(listener);
		}
	}

	public static void Broadcast(EventIDs eventID, MonoBehaviour sender, EventArgs eventArgs)
	{
		if (GameManager.IsApplicationClosing)
		{
			return;
		}
		if (!Instance)
		{
			Instance = CreateInstance();
			if (!Instance)
			{
				Debug.LogFormat("{0}: Failed to create instance of Messenger", Time.frameCount);
				return;
			}
		}
		if (EventTable.ContainsKey(eventID) && EventTable[eventID] != null)
		{
			EventTable[eventID].Dispatch(sender, eventArgs);
		}
	}
}
