using System;
using UnityEngine;

public class TriggerInsight_OnEnterRoom : MonoBehaviour, IRoomConsumer
{
	[SerializeField]
	private InsightType m_insightToTrigger;

	[SerializeField]
	private bool m_isResolved;

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerEnterRoom;

	public BaseRoom Room { get; private set; }

	private void Awake()
	{
		m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(m_insightToTrigger, !m_isResolved);
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room as Room;
		if (Room != null)
		{
			Room.PlayerEnterRelay.AddListener(m_onPlayerEnterRoom);
			return;
		}
		Debug.LogFormat("<color=red>| {0} | Room Field is null. Did you forget to connect the Room Component to the Room Field in the Inspector?</color>", this);
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (!m_isResolved)
		{
			if (SaveManager.PlayerSaveData.GetInsightState(m_insightToTrigger) < InsightState.DiscoveredButNotViewed)
			{
				SaveManager.PlayerSaveData.SetInsightState(m_insightToTrigger, InsightState.DiscoveredButNotViewed, forceOverride: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
			}
		}
		else if (SaveManager.PlayerSaveData.GetInsightState(m_insightToTrigger) < InsightState.ResolvedButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(m_insightToTrigger, InsightState.ResolvedButNotViewed, forceOverride: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
		}
	}

	private void OnDestroy()
	{
		if (Room != null)
		{
			Room.PlayerEnterRelay.RemoveListener(m_onPlayerEnterRoom);
		}
	}
}
