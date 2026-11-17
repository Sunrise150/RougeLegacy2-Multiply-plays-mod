using System;
using System.Collections;
using System.Linq;
using RL_Windows;
using Rewired;
using UnityEngine;

public class InsightWindowController : WindowController
{
	[SerializeField]
	private CanvasGroup m_leftPageCanvasGroup;

	[SerializeField]
	private CanvasGroup m_rightPageCanvasGroup;

	[SerializeField]
	private GameObject m_noInsightsDiscoveredTextGO;

	[SerializeField]
	private GameObject m_noInsightsResolvedTextGO;

	private InsightWindowEntry[] m_discoveredEntries;

	private InsightWindowEntry[] m_resolvedEntries;

	private Action<InputActionEventData> m_onCancelButtonDown;

	public override WindowID ID => WindowID.InsightCard;

	private void Awake()
	{
		m_onCancelButtonDown = OnCancelButtonDown;
	}

	public override void Initialize()
	{
		InsightWindowEntry[] componentsInChildren = m_rightPageCanvasGroup.GetComponentsInChildren<InsightWindowEntry>();
		m_discoveredEntries = componentsInChildren.Where((InsightWindowEntry entry) => !entry.IsResolvedEntry).ToArray();
		m_resolvedEntries = componentsInChildren.Where((InsightWindowEntry entry) => entry.IsResolvedEntry).ToArray();
		InsightWindowEntry[] array = componentsInChildren;
		for (int i = 0; i < array.Length; i++)
		{
			array[i].gameObject.SetActive(value: false);
		}
		base.Initialize();
		if (m_discoveredEntries.Length < InsightType_RL.TypeArray.Length || m_resolvedEntries.Length < InsightType_RL.TypeArray.Length)
		{
			throw new Exception("INSIGHT WINDOW DOES NOT HAVE ENOUGH ENTRIES. UPDATE INSIGHTWINDOW PREFAB TO HOLD MORE INSIGHT ENTRIES.");
		}
	}

	private void UpdateArrays()
	{
		int num = 0;
		int num2 = 0;
		InsightWindowEntry[] discoveredEntries = m_discoveredEntries;
		foreach (InsightWindowEntry insightWindowEntry in discoveredEntries)
		{
			if (insightWindowEntry.gameObject.activeSelf)
			{
				insightWindowEntry.gameObject.SetActive(value: false);
			}
		}
		discoveredEntries = m_resolvedEntries;
		foreach (InsightWindowEntry insightWindowEntry2 in discoveredEntries)
		{
			if (insightWindowEntry2.gameObject.activeSelf)
			{
				insightWindowEntry2.gameObject.SetActive(value: false);
			}
		}
		InsightType[] typeArray = InsightType_RL.TypeArray;
		foreach (InsightType insightType in typeArray)
		{
			if (insightType != 0)
			{
				switch (SaveManager.PlayerSaveData.GetInsightState(insightType))
				{
				case InsightState.DiscoveredButNotViewed:
				case InsightState.DiscoveredAndViewed:
				{
					InsightWindowEntry obj2 = m_discoveredEntries[num];
					obj2.gameObject.SetActive(value: true);
					obj2.SetInsightType(insightType, isResolvedEntry: false);
					num++;
					break;
				}
				case InsightState.ResolvedButNotViewed:
				case InsightState.ResolvedAndViewed:
				{
					InsightWindowEntry obj = m_resolvedEntries[num2];
					obj.gameObject.SetActive(value: true);
					obj.SetInsightType(insightType, isResolvedEntry: true);
					num2++;
					break;
				}
				}
			}
		}
		if (m_discoveredEntries[0].gameObject.activeSelf)
		{
			m_noInsightsDiscoveredTextGO.gameObject.SetActive(value: false);
		}
		else
		{
			m_noInsightsDiscoveredTextGO.gameObject.SetActive(value: true);
		}
		if (m_resolvedEntries[0].gameObject.activeSelf)
		{
			m_noInsightsResolvedTextGO.gameObject.SetActive(value: false);
		}
		else
		{
			m_noInsightsResolvedTextGO.gameObject.SetActive(value: true);
		}
	}

	protected override void OnOpen()
	{
		UpdateArrays();
		m_windowCanvas.gameObject.SetActive(value: true);
		StartCoroutine(RunOpenAnimation());
	}

	private IEnumerator RunOpenAnimation()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_leftPageCanvasGroup.alpha = 0f;
		m_rightPageCanvasGroup.alpha = 0f;
		float num = 50f;
		float duration = 0.15f;
		RectTransform component = m_leftPageCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		vector.y += num;
		component.anchoredPosition = vector;
		TweenManager.TweenBy_UnscaledTime(component, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		TweenManager.TweenTo_UnscaledTime(m_leftPageCanvasGroup, duration, Ease.None, "alpha", 1);
		RectTransform component2 = m_rightPageCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector2 = component2.anchoredPosition;
		vector2.y += num;
		component2.anchoredPosition = vector2;
		TweenManager.TweenBy_UnscaledTime(component2, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		yield return TweenManager.TweenTo_UnscaledTime(m_rightPageCanvasGroup, duration, Ease.None, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	protected override void OnFocus()
	{
		AddInputListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	protected void AddInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		}
	}

	protected void RemoveInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		}
	}

	protected virtual void OnCancelButtonDown(InputActionEventData obj)
	{
		if (WindowManager.GetIsWindowOpen(WindowID.Pause))
		{
			WindowManager.CloseAllOpenWindows();
		}
		else
		{
			WindowManager.SetWindowIsOpen(ID, isOpen: false);
		}
	}
}
