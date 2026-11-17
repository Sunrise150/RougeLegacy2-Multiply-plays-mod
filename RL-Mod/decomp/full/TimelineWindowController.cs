using System;
using System.Collections;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;

[Obsolete("Use QuestWindowController")]
public class TimelineWindowController : WindowController
{
	[SerializeField]
	private CanvasGroup m_leftPageCanvasGroup;

	[SerializeField]
	private CanvasGroup m_rightPageCanvasGroup;

	[SerializeField]
	private TMP_Text m_timelineText;

	[SerializeField]
	private TMP_Text m_noneText;

	[SerializeField]
	private GameObject m_content;

	[SerializeField]
	private TimelineWindowEntry m_burdenEntryPrefab;

	private TimelineWindowEntry[] m_burdenEntries;

	private Action<InputActionEventData> m_onCancelButtonDown;

	public override WindowID ID => WindowID.TimelineCard;

	private void Awake()
	{
		m_onCancelButtonDown = OnCancelButtonDown;
	}

	public override void Initialize()
	{
		m_burdenEntries = new TimelineWindowEntry[BurdenType_RL.TypeArray.Length - 1];
		int num = 0;
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0)
			{
				TimelineWindowEntry timelineWindowEntry = UnityEngine.Object.Instantiate(m_burdenEntryPrefab);
				timelineWindowEntry.SetBurdenType(burdenType);
				timelineWindowEntry.transform.SetParent(m_content.transform, worldPositionStays: false);
				m_burdenEntries[num] = timelineWindowEntry;
				num++;
			}
		}
		TimelineWindowEntry[] burdenEntries = m_burdenEntries;
		for (int i = 0; i < burdenEntries.Length; i++)
		{
			burdenEntries[i].gameObject.SetActive(value: false);
		}
		base.Initialize();
	}

	private void UpdateArrays()
	{
		if (SaveManager.PlayerSaveData.NewGamePlusLevel == 0)
		{
			m_timelineText.text = "ZERO";
		}
		else
		{
			m_timelineText.text = "NEW GAME+" + SaveManager.PlayerSaveData.NewGamePlusLevel;
		}
		TimelineWindowEntry[] burdenEntries = m_burdenEntries;
		foreach (TimelineWindowEntry timelineWindowEntry in burdenEntries)
		{
			if (timelineWindowEntry.gameObject.activeSelf)
			{
				timelineWindowEntry.gameObject.SetActive(value: false);
			}
		}
		int num = 0;
		bool flag = false;
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0)
			{
				if (BurdenManager.GetBurdenLevel(burdenType) > 0)
				{
					flag = true;
					m_burdenEntries[num].gameObject.SetActive(value: true);
				}
				num++;
			}
		}
		if (flag)
		{
			m_noneText.gameObject.SetActive(value: false);
		}
		else
		{
			m_noneText.gameObject.SetActive(value: true);
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
