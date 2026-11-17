using System;
using System.Collections.Generic;
using UnityEngine;

namespace GameEventTracking;

public class ItemEventTracker : MonoBehaviour, IGameEventTracker<IItemEventTrackerState>
{
	private IItemEventTrackerState m_state;

	private Action<MonoBehaviour, EventArgs> m_onItemCollected;

	private Action<MonoBehaviour, EventArgs> m_onChestOpened;

	public List<ChestTrackerData> ChestsOpened { get; private set; } = new List<ChestTrackerData>();


	public List<ItemTrackerData> ItemsCollected { get; private set; } = new List<ItemTrackerData>();


	private void Awake()
	{
		m_onItemCollected = OnItemCollected;
		m_onChestOpened = OnChestOpened;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ItemCollected, m_onItemCollected);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.ChestOpened, m_onChestOpened);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ItemCollected, m_onItemCollected);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.ChestOpened, m_onChestOpened);
	}

	public IEnumerable<IGameEventData> GetGameEvents()
	{
		foreach (ChestTrackerData item in ChestsOpened)
		{
			yield return item;
		}
		foreach (ItemTrackerData item2 in ItemsCollected)
		{
			yield return item2;
		}
	}

	private void OnChestOpened(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is ChestOpenedEventArgs)
		{
			ChestOpenedEventArgs obj = eventArgs as ChestOpenedEventArgs;
			bool containsGold = obj.GoldValue > 0;
			ChestObj chest = obj.Chest;
			ChestTrackerData item = new ChestTrackerData(chest.ChestType, containsGold, chest.Room.BiomeType, chest.Room.BiomeControllerIndex, chest.ChestIndex);
			ChestsOpened.Add(item);
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast event args as ChestOpenedEventArgs</color>", this);
		}
	}

	private void OnItemCollected(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is ItemCollectedEventArgs { Item: var item })
		{
			int num = Economy_EV.GetItemDropValue(item.ItemDropType);
			if (item.ValueOverride != -1)
			{
				num = item.ValueOverride;
			}
			ItemsCollected.Add(new ItemTrackerData(num, item.ItemDropType));
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast event args as ItemCollectedEventArgs</color>", this);
		}
	}

	public void Reset()
	{
		if (ChestsOpened != null)
		{
			ChestsOpened.Clear();
		}
		if (ItemsCollected != null)
		{
			ItemsCollected.Clear();
		}
	}

	public void RestoreState(IItemEventTrackerState state)
	{
		ChestsOpened = state.ChestsOpened;
		ItemsCollected = state.ItemsCollected;
	}

	public IItemEventTrackerState SaveState()
	{
		if (m_state == null)
		{
			m_state = new ItemEventTrackerState(ChestsOpened, ItemsCollected);
		}
		else
		{
			m_state.Initialise(ChestsOpened, ItemsCollected);
		}
		return m_state;
	}
}
