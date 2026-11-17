using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GameEventTracking;

public class GameEventTrackerManager : MonoBehaviour
{
	[SerializeField]
	private RoomEventTracker m_roomEventTracker;

	[SerializeField]
	private EnemyEventTracker m_enemyEventTracker;

	[SerializeField]
	private ItemEventTracker m_itemEventTracker;

	private static GameEventTrackerManager m_instance;

	public static RoomEventTracker RoomEventTracker => m_instance.m_roomEventTracker;

	public static EnemyEventTracker EnemyEventTracker => m_instance.m_enemyEventTracker;

	public static ItemEventTracker ItemEventTracker => m_instance.m_itemEventTracker;

	public static bool IsInstantiated => m_instance != null;

	private void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		m_instance = null;
	}

	public static List<IGameEventData> GetGameEvents()
	{
		return (from gameEvent in GetGameEventsFromTrackers()
			orderby gameEvent.TimesLoaded, gameEvent.TimeStamp
			select gameEvent).ToList();
	}

	private static IEnumerable<IGameEventData> GetGameEventsFromTrackers()
	{
		foreach (IGameEventData gameEvent in RoomEventTracker.GetGameEvents())
		{
			yield return gameEvent;
		}
		foreach (IGameEventData gameEvent2 in EnemyEventTracker.GetGameEvents())
		{
			yield return gameEvent2;
		}
		foreach (IGameEventData gameEvent3 in ItemEventTracker.GetGameEvents())
		{
			yield return gameEvent3;
		}
	}

	public static void Reset()
	{
		RoomEventTracker.Reset();
		EnemyEventTracker.Reset();
		ItemEventTracker.Reset();
	}

	public static void RestoreStates(List<IGameEventTrackerState> states)
	{
		foreach (IGameEventTrackerState state in states)
		{
			if (state is IRoomEventTrackerState)
			{
				RoomEventTracker.RestoreState(state as IRoomEventTrackerState);
				continue;
			}
			if (state is IEnemyEventTrackerState)
			{
				EnemyEventTracker.RestoreState(state as IEnemyEventTrackerState);
				continue;
			}
			if (state is IItemEventTrackerState)
			{
				ItemEventTracker.RestoreState(state as IItemEventTrackerState);
				continue;
			}
			Debug.LogFormat("<color=red>| {0} | No condition exists for Game Tracker State ({1})", m_instance, state.GetType());
		}
	}

	public static void SaveStates(ref List<IGameEventTrackerState> trackerStateList)
	{
		trackerStateList.Clear();
		trackerStateList.Add(RoomEventTracker.SaveState());
		trackerStateList.Add(EnemyEventTracker.SaveState());
		trackerStateList.Add(ItemEventTracker.SaveState());
	}
}
