using System;
using System.Collections;
using FMODUnity;
using RLAudio;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RL_Windows;

public class PauseWindowController : WindowController, IAudioEventEmitter, ILocalizable
{
	public const float BG_FADE_IN_AMOUNT = 0.66667f;

	public const float BG_FADE_IN_DURATION = 0.15f;

	private const WindowID DEFAULT_WINDOW_ID = WindowID.Options;

	[SerializeField]
	private RectTransform m_menuRectTransform;

	[SerializeField]
	private RectTransform m_flapRect;

	[SerializeField]
	private StudioEventEmitter m_snapshotEventEmitter;

	[SerializeField]
	private UnityEvent m_tabOverEvent;

	[SerializeField]
	private UnityEvent m_selectTabEvent;

	private PauseTabButton[] m_pauseTabArray;

	private TMP_Text m_flapText;

	private int m_selectedTabIndex;

	private int m_previousTabIndex;

	private bool m_rectTransformUpdated;

	private bool m_controlsEnabled = true;

	private bool m_insightFound;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_onWindowsStartPressed;

	private Action<InputActionEventData> m_onWindowsLBPressed;

	private Action<InputActionEventData> m_onWindowsRBPressed;

	public override int SortOrderOverride => 500;

	public override WindowID ID => WindowID.Pause;

	public string Description => ToString();

	public void SetStartingSubWindow(WindowID id)
	{
		for (int i = 0; i < m_pauseTabArray.Length; i++)
		{
			if (m_pauseTabArray[i].WindowToDisplay == id)
			{
				m_selectedTabIndex = i;
				break;
			}
		}
	}

	private int GetTabIndex(PauseTabButton tabButton)
	{
		for (int i = 0; i < m_pauseTabArray.Length; i++)
		{
			if (tabButton == m_pauseTabArray[i])
			{
				return i;
			}
		}
		return -1;
	}

	private void Awake()
	{
		m_flapText = m_flapRect.GetComponentInChildren<TMP_Text>();
		m_refreshText = RefreshText;
		m_onWindowsStartPressed = OnWindowsStartPressed;
		m_onWindowsLBPressed = OnWindowsLBPressed;
		m_onWindowsRBPressed = OnWindowsRBPressed;
	}

	public void EnableControls(bool enable)
	{
		m_controlsEnabled = enable;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_pauseTabArray = m_menuRectTransform.GetComponentsInChildren<PauseTabButton>();
		for (int i = 0; i < m_pauseTabArray.Length; i++)
		{
			PauseTabButton obj = m_pauseTabArray[i];
			obj.OnSelectEvent = (PauseTabSelectedHandler)Delegate.Combine(obj.OnSelectEvent, new PauseTabSelectedHandler(SetSelectedTab));
		}
		SetStartingSubWindow(WindowID.Options);
	}

	public void EnableSnapshotEmitter(bool enable)
	{
		if (enable)
		{
			m_snapshotEventEmitter.Play();
		}
		else
		{
			m_snapshotEventEmitter.Stop();
		}
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		m_controlsEnabled = true;
		AudioManager.SetSFXPaused(pause: true);
		m_windowCanvas.gameObject.SetActive(value: true);
		if (!m_rectTransformUpdated)
		{
			m_rectTransformUpdated = true;
			RectTransform[] componentsInChildren = m_windowCanvas.GetComponentsInChildren<RectTransform>();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				LayoutRebuilder.ForceRebuildLayoutImmediate(componentsInChildren[i]);
			}
		}
		AddInputListeners();
		CheckForQuest();
		EnableSnapshotEmitter(enable: true);
		StartCoroutine(RunOpenAnimation());
	}

	private IEnumerator RunOpenAnimation()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		Vector3 vector = m_menuRectTransform.anchoredPosition;
		vector.y += 100f;
		m_menuRectTransform.anchoredPosition = vector;
		UpdateFlap();
		yield return null;
		UpdateSelectedTab(enteredFromOtherSubmenu: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return TweenManager.TweenBy_UnscaledTime(m_menuRectTransform, 0.15f, Ease.Quad.EaseOut, "anchoredPosition.y", -100).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.PauseWindow_Opened, this, null);
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		AudioManager.SetSFXPaused(pause: false);
		RemoveInputListeners();
		m_windowCanvas.gameObject.SetActive(value: false);
		SetStartingSubWindow(WindowID.Options);
		EnableSnapshotEmitter(enable: false);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.PauseWindow_Closed, this, null);
	}

	private void AddInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onWindowsStartPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Start");
			base.RewiredPlayer.AddInputEventDelegate(m_onWindowsStartPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Select");
			base.RewiredPlayer.AddInputEventDelegate(m_onWindowsLBPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_LB");
			base.RewiredPlayer.AddInputEventDelegate(m_onWindowsRBPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_RB");
		}
	}

	private void RemoveInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onWindowsStartPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Start");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onWindowsStartPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Select");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onWindowsLBPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_LB");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onWindowsRBPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_RB");
		}
	}

	private void OnWindowsStartPressed(InputActionEventData obj)
	{
		if (m_controlsEnabled)
		{
			ClosePauseMenu();
		}
	}

	private void ClosePauseMenu()
	{
		WindowManager.CloseAllOpenWindows();
	}

	protected override void OnFocus()
	{
	}

	protected override void OnLostFocus()
	{
	}

	private void OnWindowsLBPressed(InputActionEventData obj)
	{
		if (m_controlsEnabled)
		{
			SelectLeftTab();
		}
	}

	private void OnWindowsRBPressed(InputActionEventData obj)
	{
		if (m_controlsEnabled)
		{
			SelectRightTab();
		}
	}

	public void SelectLeftTab()
	{
		m_previousTabIndex = m_selectedTabIndex;
		m_selectedTabIndex--;
		if (m_selectedTabIndex >= 0 && m_pauseTabArray[m_selectedTabIndex].WindowToDisplay == WindowID.Quest && !m_insightFound)
		{
			m_selectedTabIndex--;
		}
		if (m_selectedTabIndex < 0)
		{
			m_selectedTabIndex = m_pauseTabArray.Length - 1;
		}
		if (m_tabOverEvent != null)
		{
			m_tabOverEvent.Invoke();
		}
		UpdateSelectedTab(enteredFromOtherSubmenu: true);
	}

	public void SelectRightTab()
	{
		m_previousTabIndex = m_selectedTabIndex;
		m_selectedTabIndex++;
		if (m_selectedTabIndex < m_pauseTabArray.Length && m_pauseTabArray[m_selectedTabIndex].WindowToDisplay == WindowID.Quest && !m_insightFound)
		{
			m_selectedTabIndex++;
		}
		if (m_selectedTabIndex > m_pauseTabArray.Length - 1)
		{
			m_selectedTabIndex = 0;
		}
		if (m_tabOverEvent != null)
		{
			m_tabOverEvent.Invoke();
		}
		UpdateSelectedTab(enteredFromOtherSubmenu: true);
	}

	private void SetSelectedTab(PauseTabButton tabButton)
	{
		m_previousTabIndex = m_selectedTabIndex;
		m_selectedTabIndex = GetTabIndex(tabButton);
		UpdateSelectedTab(enteredFromOtherSubmenu: true);
	}

	public void SetSelectedTab(WindowID tabID)
	{
		if (m_selectTabEvent != null)
		{
			m_selectTabEvent.Invoke();
		}
		PauseTabButton selectedTab = null;
		for (int i = 0; i < m_pauseTabArray.Length; i++)
		{
			PauseTabButton pauseTabButton = m_pauseTabArray[i];
			if (pauseTabButton.WindowToDisplay == tabID)
			{
				selectedTab = pauseTabButton;
				break;
			}
		}
		SetSelectedTab(selectedTab);
	}

	private void UpdateFlap()
	{
		PauseTabButton pauseTabButton = m_pauseTabArray[m_selectedTabIndex];
		m_flapRect.position = pauseTabButton.GetComponent<RectTransform>().position;
		Vector3 position = m_flapRect.position;
		m_flapRect.position = position;
		m_flapText.text = pauseTabButton.GetComponentInChildren<TMP_Text>().text;
	}

	private void UpdateSelectedTab(bool enteredFromOtherSubmenu)
	{
		PauseTabButton pauseTabButton = m_pauseTabArray[m_selectedTabIndex];
		PauseTabButton pauseTabButton2 = m_pauseTabArray[m_previousTabIndex];
		UpdateFlap();
		if (pauseTabButton2.WindowToDisplay != 0 && WindowManager.GetIsWindowOpen(pauseTabButton2.WindowToDisplay))
		{
			WindowManager.SetWindowIsOpen(pauseTabButton2.WindowToDisplay, isOpen: false);
		}
		if (pauseTabButton.WindowToDisplay == WindowID.None)
		{
			return;
		}
		if (enteredFromOtherSubmenu)
		{
			if (pauseTabButton.WindowToDisplay == WindowID.Map)
			{
				MapWindowController.EnteredFromOtherSubmenu = true;
			}
			else if (pauseTabButton.WindowToDisplay == WindowID.Options)
			{
				OptionsWindowController.EnteredFromOtherSubmenu = true;
			}
		}
		WindowManager.SetWindowIsOpen(pauseTabButton.WindowToDisplay, isOpen: true);
	}

	private void CheckForQuest()
	{
		PauseTabButton pauseTabButton = null;
		PauseTabButton[] pauseTabArray = m_pauseTabArray;
		foreach (PauseTabButton pauseTabButton2 in pauseTabArray)
		{
			if (pauseTabButton2.WindowToDisplay == WindowID.Quest)
			{
				pauseTabButton = pauseTabButton2;
				break;
			}
		}
		if (!pauseTabButton)
		{
			return;
		}
		InsightType[] typeArray = InsightType_RL.TypeArray;
		foreach (InsightType insightType in typeArray)
		{
			if (insightType != 0 && SaveManager.PlayerSaveData.GetInsightState(insightType) > InsightState.Undiscovered)
			{
				m_insightFound = true;
				break;
			}
		}
		TMP_Text componentInChildren = pauseTabButton.GetComponentInChildren<TMP_Text>();
		if (m_insightFound)
		{
			componentInChildren.text = LocalizationManager.GetString("LOC_ID_OPTIONS_MENU_QUEST_1", isFemale: false);
		}
		else
		{
			componentInChildren.text = "???";
		}
	}

	private void OnDestroy()
	{
		PauseTabButton[] pauseTabArray = m_pauseTabArray;
		foreach (PauseTabButton obj in pauseTabArray)
		{
			obj.OnSelectEvent = (PauseTabSelectedHandler)Delegate.Remove(obj.OnSelectEvent, new PauseTabSelectedHandler(SetSelectedTab));
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		UpdateFlap();
		CheckForQuest();
	}
}
