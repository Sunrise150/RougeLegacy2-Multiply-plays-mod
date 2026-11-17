using System;
using UnityEngine;

public class TriggerInsight_OnTriggerEnter : MonoBehaviour
{
	[SerializeField]
	private InsightType m_insightToTrigger;

	[SerializeField]
	private bool m_isResolved;

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs;

	private void Awake()
	{
		m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(m_insightToTrigger, !m_isResolved);
		BoxCollider2D component = GetComponent<BoxCollider2D>();
		if (component == null)
		{
			throw new Exception("BoxCollider2D not found on TriggerInsight_OnTriggerEnter");
		}
		base.gameObject.layer = 13;
		component.isTrigger = true;
	}

	private void OnTriggerEnter2D(Collider2D otherCollider)
	{
		if (!otherCollider.CompareTag("Player"))
		{
			return;
		}
		if (!m_isResolved)
		{
			if (SaveManager.PlayerSaveData.GetInsightState(m_insightToTrigger) < InsightState.DiscoveredButNotViewed)
			{
				SaveManager.PlayerSaveData.SetInsightState(m_insightToTrigger, InsightState.DiscoveredButNotViewed, forceOverride: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
			}
			base.gameObject.SetActive(value: false);
		}
		else
		{
			if (SaveManager.PlayerSaveData.GetInsightState(m_insightToTrigger) < InsightState.ResolvedButNotViewed)
			{
				SaveManager.PlayerSaveData.SetInsightState(m_insightToTrigger, InsightState.ResolvedButNotViewed, forceOverride: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
			}
			base.gameObject.SetActive(value: false);
		}
	}
}
