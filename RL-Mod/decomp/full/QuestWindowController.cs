using System;
using System.Collections;
using System.Collections.Generic;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class QuestWindowController : WindowController, ILocalizable
{
	private const int STARTING_ARRAY_SIZE = 5;

	[SerializeField]
	private CanvasGroup m_insightPageCanvasGroup;

	[SerializeField]
	private CanvasGroup m_timelinePageCanvasGroup;

	[SerializeField]
	private GameObject m_noInsightsDiscoveredTextGO;

	[SerializeField]
	private GameObject m_noInsightsResolvedTextGO;

	[SerializeField]
	private TMP_Text m_timelineText;

	[SerializeField]
	private TMP_Text m_noneText;

	[SerializeField]
	private GameObject m_timelineScrollViewGO;

	[SerializeField]
	private GameObject m_insightPageContentGO;

	[SerializeField]
	private GameObject m_timelinePageContentGO;

	[SerializeField]
	private TimelineWindowEntry m_burdenEntryPrefab;

	[SerializeField]
	private InsightWindowEntry m_insightEntryPrefab;

	[SerializeField]
	private GameObject m_castleBGImageGO;

	[SerializeField]
	private Image m_leftScrollArrow;

	[SerializeField]
	private Image m_rightScrollArrow;

	[SerializeField]
	private ScrollBarInput_RL m_leftScrollBarInput;

	[SerializeField]
	private ScrollBarInput_RL m_rightScrollBarInput;

	[SerializeField]
	private RectTransform m_insightViewRectTransform;

	[SerializeField]
	private RectTransform m_threadViewRectTransform;

	private TimelineWindowEntry[] m_burdenArray;

	private InsightWindowEntry[] m_discoveredInsightsArray;

	private InsightWindowEntry[] m_resolvedInsightsArray;

	private ScrollBarInput_RL m_selectedScrollBarInput;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_onCancelButtonDown;

	private Action<InputActionEventData> m_onHorizontalInputHandler;

	private static List<InsightType> m_discoveredInsightsHelper = new List<InsightType>();

	private static List<InsightType> m_resolvedInsightsHelper = new List<InsightType>();

	private static List<BurdenType> m_activeBurdensHelper = new List<BurdenType>();

	public override WindowID ID => WindowID.Quest;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onCancelButtonDown = OnCancelButtonDown;
		m_onHorizontalInputHandler = OnHorizontalInputHandler;
	}

	public override void Initialize()
	{
		m_discoveredInsightsArray = new InsightWindowEntry[5];
		m_resolvedInsightsArray = new InsightWindowEntry[5];
		int siblingIndex = m_noInsightsDiscoveredTextGO.transform.GetSiblingIndex();
		for (int i = 0; i < 5; i++)
		{
			InsightWindowEntry insightWindowEntry = UnityEngine.Object.Instantiate(m_insightEntryPrefab, m_insightPageContentGO.transform, worldPositionStays: false);
			insightWindowEntry.gameObject.SetActive(value: false);
			insightWindowEntry.transform.SetSiblingIndex(siblingIndex);
			m_discoveredInsightsArray[i] = insightWindowEntry;
			InsightWindowEntry insightWindowEntry2 = UnityEngine.Object.Instantiate(m_insightEntryPrefab, m_insightPageContentGO.transform, worldPositionStays: false);
			insightWindowEntry2.gameObject.SetActive(value: false);
			insightWindowEntry2.transform.SetSiblingIndex(m_insightPageContentGO.transform.childCount);
			m_resolvedInsightsArray[i] = insightWindowEntry2;
		}
		m_burdenArray = new TimelineWindowEntry[5];
		for (int j = 0; j < 5; j++)
		{
			TimelineWindowEntry timelineWindowEntry = UnityEngine.Object.Instantiate(m_burdenEntryPrefab, m_timelinePageContentGO.transform, worldPositionStays: false);
			timelineWindowEntry.gameObject.SetActive(value: false);
			m_burdenArray[j] = timelineWindowEntry;
		}
		base.Initialize();
	}

	private void UpdateArrays()
	{
		UpdateInsights();
		UpdateTimeline();
	}

	private void UpdateInsights()
	{
		m_discoveredInsightsHelper.Clear();
		m_resolvedInsightsHelper.Clear();
		InsightType[] typeArray = InsightType_RL.TypeArray;
		foreach (InsightType insightType in typeArray)
		{
			if (insightType != 0)
			{
				switch (SaveManager.PlayerSaveData.GetInsightState(insightType))
				{
				case InsightState.DiscoveredButNotViewed:
				case InsightState.DiscoveredAndViewed:
					m_discoveredInsightsHelper.Add(insightType);
					break;
				case InsightState.ResolvedButNotViewed:
				case InsightState.ResolvedAndViewed:
					m_resolvedInsightsHelper.Add(insightType);
					break;
				}
			}
		}
		int count = m_discoveredInsightsHelper.Count;
		if (count > m_discoveredInsightsArray.Length)
		{
			int num = m_discoveredInsightsArray.Length;
			m_discoveredInsightsArray = ExpandArray(m_discoveredInsightsArray, count + 5);
			int siblingIndex = m_noInsightsDiscoveredTextGO.transform.GetSiblingIndex();
			for (int j = num; j < m_discoveredInsightsArray.Length; j++)
			{
				InsightWindowEntry insightWindowEntry = UnityEngine.Object.Instantiate(m_insightEntryPrefab, m_insightPageContentGO.transform, worldPositionStays: false);
				insightWindowEntry.gameObject.SetActive(value: false);
				insightWindowEntry.transform.SetSiblingIndex(siblingIndex);
				m_discoveredInsightsArray[j] = insightWindowEntry;
			}
		}
		int count2 = m_resolvedInsightsHelper.Count;
		if (count2 > m_resolvedInsightsArray.Length)
		{
			int num2 = m_resolvedInsightsArray.Length;
			m_resolvedInsightsArray = ExpandArray(m_resolvedInsightsArray, count2 + 5);
			for (int k = num2; k < m_resolvedInsightsArray.Length; k++)
			{
				InsightWindowEntry insightWindowEntry2 = UnityEngine.Object.Instantiate(m_insightEntryPrefab, m_insightPageContentGO.transform, worldPositionStays: false);
				insightWindowEntry2.gameObject.SetActive(value: false);
				insightWindowEntry2.transform.SetSiblingIndex(m_insightPageContentGO.transform.childCount);
				m_resolvedInsightsArray[k] = insightWindowEntry2;
			}
		}
		if (count > 0)
		{
			m_noInsightsDiscoveredTextGO.SetActive(value: false);
		}
		else
		{
			m_noInsightsDiscoveredTextGO.SetActive(value: true);
		}
		for (int l = 0; l < m_discoveredInsightsArray.Length; l++)
		{
			InsightWindowEntry insightWindowEntry3 = m_discoveredInsightsArray[l];
			if (l < count)
			{
				insightWindowEntry3.SetInsightType(m_discoveredInsightsHelper[l], isResolvedEntry: false);
				if (!insightWindowEntry3.gameObject.activeSelf)
				{
					insightWindowEntry3.gameObject.SetActive(value: true);
				}
			}
			else if (insightWindowEntry3.gameObject.activeSelf)
			{
				insightWindowEntry3.gameObject.SetActive(value: false);
			}
		}
		if (count2 > 0)
		{
			m_noInsightsResolvedTextGO.SetActive(value: false);
		}
		else
		{
			m_noInsightsResolvedTextGO.SetActive(value: true);
		}
		for (int m = 0; m < m_resolvedInsightsArray.Length; m++)
		{
			InsightWindowEntry insightWindowEntry4 = m_resolvedInsightsArray[m];
			if (m < count2)
			{
				insightWindowEntry4.SetInsightType(m_resolvedInsightsHelper[m], isResolvedEntry: true);
				if (!insightWindowEntry4.gameObject.activeSelf)
				{
					insightWindowEntry4.gameObject.SetActive(value: true);
				}
			}
			else if (insightWindowEntry4.gameObject.activeSelf)
			{
				insightWindowEntry4.gameObject.SetActive(value: false);
			}
		}
	}

	private void UpdateTimeline()
	{
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Timeline_Unlocked) && BurdenManager.GetTotalBurdenLevel() <= 0)
		{
			if (m_timelineScrollViewGO.activeSelf)
			{
				m_timelineScrollViewGO.SetActive(value: false);
			}
			if (!m_castleBGImageGO.activeSelf)
			{
				m_castleBGImageGO.SetActive(value: true);
			}
			return;
		}
		if (!m_timelineScrollViewGO.activeSelf)
		{
			m_timelineScrollViewGO.SetActive(value: true);
		}
		if (m_castleBGImageGO.activeSelf)
		{
			m_castleBGImageGO.SetActive(value: false);
		}
		m_timelineText.text = string.Format(LocalizationManager.GetString("LOC_ID_NEWGAMEPLUS_ENTER_NG_SHORT_TITLE_1", isFemale: false), SaveManager.PlayerSaveData.NewGamePlusLevel);
		m_activeBurdensHelper.Clear();
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0 && BurdenManager.GetBurdenLevel(burdenType) > 0)
			{
				m_activeBurdensHelper.Add(burdenType);
			}
		}
		int count = m_activeBurdensHelper.Count;
		if (count > m_burdenArray.Length)
		{
			int num = m_burdenArray.Length;
			m_burdenArray = ExpandArray(m_burdenArray, count + 5);
			for (int j = num; j < m_burdenArray.Length; j++)
			{
				TimelineWindowEntry timelineWindowEntry = UnityEngine.Object.Instantiate(m_burdenEntryPrefab, m_timelinePageContentGO.transform, worldPositionStays: false);
				timelineWindowEntry.gameObject.SetActive(value: false);
				m_burdenArray[j] = timelineWindowEntry;
			}
		}
		if (count > 0)
		{
			m_noneText.gameObject.SetActive(value: false);
		}
		else
		{
			m_noneText.gameObject.SetActive(value: true);
		}
		for (int k = 0; k < m_burdenArray.Length; k++)
		{
			TimelineWindowEntry timelineWindowEntry2 = m_burdenArray[k];
			if (k < count)
			{
				timelineWindowEntry2.SetBurdenType(m_activeBurdensHelper[k]);
				if (!timelineWindowEntry2.gameObject.activeSelf)
				{
					timelineWindowEntry2.gameObject.SetActive(value: true);
				}
			}
			else if (timelineWindowEntry2.gameObject.activeSelf)
			{
				timelineWindowEntry2.gameObject.SetActive(value: false);
			}
		}
	}

	private static T[] ExpandArray<T>(T[] arr, int size)
	{
		T[] array = new T[size];
		for (int i = 0; i < arr.Length; i++)
		{
			array[i] = arr[i];
		}
		return array;
	}

	protected void SelectScrollBar(bool selectLeft)
	{
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Timeline_Unlocked) && BurdenManager.GetTotalBurdenLevel() <= 0)
		{
			selectLeft = true;
		}
		if (selectLeft)
		{
			m_selectedScrollBarInput = m_leftScrollBarInput;
			m_leftScrollArrow.gameObject.SetActive(value: true);
			m_rightScrollArrow.gameObject.SetActive(value: false);
			m_leftScrollBarInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
			m_rightScrollBarInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
		else
		{
			m_selectedScrollBarInput = m_rightScrollBarInput;
			m_rightScrollArrow.gameObject.SetActive(value: true);
			m_leftScrollArrow.gameObject.SetActive(value: false);
			m_leftScrollBarInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
			m_rightScrollBarInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		UpdateArrays();
		SelectScrollBar(selectLeft: true);
		m_windowCanvas.gameObject.SetActive(value: true);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_insightViewRectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_threadViewRectTransform);
		StartCoroutine(RunOpenAnimation());
	}

	private IEnumerator RunOpenAnimation()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_insightPageCanvasGroup.alpha = 0f;
		m_timelinePageCanvasGroup.alpha = 0f;
		float num = 50f;
		float duration = 0.15f;
		RectTransform component = m_insightPageCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		vector.y += num;
		component.anchoredPosition = vector;
		TweenManager.TweenBy_UnscaledTime(component, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		TweenManager.TweenTo_UnscaledTime(m_insightPageCanvasGroup, duration, Ease.None, "alpha", 1);
		RectTransform component2 = m_timelinePageCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector2 = component2.anchoredPosition;
		vector2.y += num;
		component2.anchoredPosition = vector2;
		TweenManager.TweenBy_UnscaledTime(component2, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		yield return TweenManager.TweenTo_UnscaledTime(m_timelinePageCanvasGroup, duration, Ease.None, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
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
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
		}
	}

	protected void RemoveInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
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

	private void OnHorizontalInputHandler(InputActionEventData eventData)
	{
		SelectScrollBar(m_selectedScrollBarInput != m_leftScrollBarInput);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		m_timelineText.text = string.Format(LocalizationManager.GetString("LOC_ID_NEWGAMEPLUS_ENTER_NG_SHORT_TITLE_1", isFemale: false), SaveManager.PlayerSaveData.NewGamePlusLevel);
		UpdateArrays();
	}
}
