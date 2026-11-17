using System;
using System.Collections.Generic;
using Sigtrap.Relays;
using UnityEngine;

public class BaseSpecialRoomController : MonoBehaviour, IRoomConsumer
{
	private SpecialRoomCompletedEventArgs m_roomCompletedEventArgs;

	private Relay m_roomCompletedRelay = new Relay();

	private Relay m_roomInitializedRelay = new Relay();

	private Action m_onRoomDataLoaded;

	private Action<bool> m_onRoomDataSaved;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerExitRoom;

	private Action<object, RoomViaDoorEventArgs> m_playerEnterRoomInitialization;

	protected Dictionary<string, string> m_roomMiscDataTable;

	[SerializeField]
	private SpecialRoomType m_specialRoomType;

	public IRelayLink RoomCompletedRelay => m_roomCompletedRelay.link;

	public IRelayLink SpecialRoomInitializedRelay => m_roomInitializedRelay.link;

	public virtual SpecialRoomType SpecialRoomType => m_specialRoomType;

	public BaseRoom Room { get; private set; }

	public bool IsRoomComplete { get; protected set; }

	public RoomSaveData RoomSaveData { get; private set; }

	protected virtual void Awake()
	{
		m_roomCompletedEventArgs = new SpecialRoomCompletedEventArgs(this);
		m_onRoomDataLoaded = OnRoomDataLoaded;
		m_onRoomDataSaved = OnRoomDataSaved;
		m_onPlayerExitRoom = OnPlayerExitRoom;
		m_playerEnterRoomInitialization = PlayerEnterRoomInitialization;
	}

	private void SaveRoomMiscData()
	{
		if (m_roomMiscDataTable == null)
		{
			return;
		}
		string text = "";
		foreach (KeyValuePair<string, string> item in m_roomMiscDataTable)
		{
			text += $"<{item.Key}:{item.Value}>";
		}
		RoomSaveData.RoomMiscData = text;
	}

	private void LoadRoomMiscData()
	{
		if (!string.IsNullOrEmpty(RoomSaveData.RoomMiscData) && m_roomMiscDataTable == null)
		{
			m_roomMiscDataTable = new Dictionary<string, string>();
		}
		if (m_roomMiscDataTable == null)
		{
			return;
		}
		m_roomMiscDataTable.Clear();
		if (string.IsNullOrEmpty(RoomSaveData.RoomMiscData))
		{
			return;
		}
		string roomMiscData = RoomSaveData.RoomMiscData;
		int num = 0;
		int num2 = 0;
		while (num != -1 && num2 != -1)
		{
			num = roomMiscData.IndexOf('<', num2);
			if (num != -1)
			{
				num2 = roomMiscData.IndexOf('>', num);
				int num3 = roomMiscData.IndexOf(':', num);
				int length = num3 - (num + 1);
				int length2 = num2 - (num3 + 1);
				string key = roomMiscData.Substring(num + 1, length);
				string value = roomMiscData.Substring(num3 + 1, length2);
				if (m_roomMiscDataTable.ContainsKey(key))
				{
					m_roomMiscDataTable[key] = value;
				}
				else
				{
					m_roomMiscDataTable.Add(key, value);
				}
			}
		}
	}

	public string GetRoomMiscData(string id)
	{
		if (m_roomMiscDataTable != null && m_roomMiscDataTable.TryGetValue(id, out var value))
		{
			return value;
		}
		return null;
	}

	public void SetRoomMiscData(string id, string value)
	{
		if (m_roomMiscDataTable == null)
		{
			m_roomMiscDataTable = new Dictionary<string, string>();
		}
		if (m_roomMiscDataTable.ContainsKey(id))
		{
			m_roomMiscDataTable[id] = value;
		}
		else
		{
			m_roomMiscDataTable.Add(id, value);
		}
	}

	public virtual void RoomCompleted()
	{
		if (Room != null)
		{
			IsRoomComplete = true;
			m_roomCompletedRelay.Dispatch();
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.SpecialRoomCompleted, this, m_roomCompletedEventArgs);
		}
	}

	protected void ForceRoomCompleted()
	{
		if (Room != null)
		{
			IsRoomComplete = true;
		}
	}

	public virtual void SetRoom(BaseRoom room)
	{
		Room = room;
		if ((bool)Room)
		{
			Room room2 = room as Room;
			if ((bool)room2)
			{
				room2.SetCanMerge(canMerge: false);
			}
			Room.PlayerEnterRelay.AddListener(m_playerEnterRoomInitialization);
			Room.PlayerExitRelay.AddListener(m_onPlayerExitRoom);
			Room.SaveController.OnRoomDataLoadedRelay.AddListener(m_onRoomDataLoaded);
			Room.SaveController.OnRoomDataSavedRelay.AddListener(m_onRoomDataSaved);
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Room Field is null. Did you forget to connect the Room Component to the Room Field in the Inspector?</color>", this);
		}
	}

	protected virtual void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(m_playerEnterRoomInitialization);
			Room.PlayerExitRelay.RemoveListener(m_onPlayerExitRoom);
			Room.SaveController.OnRoomDataLoadedRelay.RemoveListener(m_onRoomDataLoaded);
			Room.SaveController.OnRoomDataSavedRelay.RemoveListener(m_onRoomDataSaved);
		}
	}

	private void PlayerEnterRoomInitialization(object sender, RoomViaDoorEventArgs eventArgs)
	{
		OnPlayerEnterRoom(sender, eventArgs);
		m_roomInitializedRelay.Dispatch();
	}

	protected virtual void OnRoomDataLoaded()
	{
		if (!(Room == null) && Room.BiomeControllerIndex != -1)
		{
			RoomSaveData = SaveManager.StageSaveData.GetRoomSaveData(Room.BiomeType, Room.BiomeControllerIndex);
			if (!RoomSaveData.IsNativeNull())
			{
				LoadRoomMiscData();
				IsRoomComplete = RoomSaveData.IsRoomComplete;
			}
		}
	}

	protected virtual void OnRoomDataSaved(bool exitingToMainMenu)
	{
		if (!RoomSaveData.IsNativeNull())
		{
			SaveRoomMiscData();
			RoomSaveData.IsRoomComplete = IsRoomComplete;
		}
	}

	protected virtual void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
	}

	protected virtual void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
	}
}
