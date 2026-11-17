using System;
using System.Collections.Generic;
using Rewired;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace RL_Windows;

public class OptionsWindowController : WindowController
{
	public static bool EnteredFromOtherSubmenu;

	[Header("Options Window Fields")]
	[SerializeField]
	private RectTransform m_selectionIndicator;

	[SerializeField]
	private RectTransform m_selectionIndicatorFrill;

	[SerializeField]
	private CanvasGroup m_fadeBGCanvasGroup;

	[SerializeField]
	private CanvasGroup m_settingsCanvasGroup;

	[SerializeField]
	private BaseOptionItem m_backOptionItem;

	[SerializeField]
	private BaseOptionItem m_quitOptionItem;

	[SerializeField]
	private BaseOptionItem m_retireOptionItem;

	[SerializeField]
	private BaseOptionItem m_restartChallengeOptionItem;

	[SerializeField]
	private BaseOptionItem m_skipTutorialOptionItem;

	[SerializeField]
	private BaseOptionItem m_houseRulesOptionItem;

	[SerializeField]
	private UnityEvent m_changeSelectedOptionEvent;

	[SerializeField]
	private UnityEvent m_selectOptionEvent;

	protected List<BaseOptionItem> m_optionItemList;

	protected BaseOptionItem m_currentSelectedOptionItem;

	protected int m_currentSelectedIndex;

	private bool m_loadedFromMainMenu;

	private Action<InputActionEventData> m_onConfirmButtonDown;

	private Action<InputActionEventData> m_onCancelButtonDown;

	private Action<InputActionEventData> m_invokeOptionIncrementOrDecrement;

	private Action<InputActionEventData> m_onVerticalInput;

	public BaseOptionItem CurrentlySelectedOptionItem => m_currentSelectedOptionItem;

	private bool IsSuboptionWindow => this is SuboptionsWindowController;

	public override WindowID ID => WindowID.Options;

	private int GetIndexOf(BaseOptionItem item)
	{
		for (int i = 0; i < m_optionItemList.Count; i++)
		{
			if (item == m_optionItemList[i])
			{
				return i;
			}
		}
		return -1;
	}

	private void Awake()
	{
		m_onConfirmButtonDown = OnConfirmButtonDown;
		m_onCancelButtonDown = OnCancelButtonDown;
		m_invokeOptionIncrementOrDecrement = InvokeOptionIncrementOrDecrement;
		m_onVerticalInput = OnVerticalInput;
	}

	public override void Initialize()
	{
		if (m_optionItemList == null)
		{
			m_optionItemList = new List<BaseOptionItem>();
			GetComponentsInChildren(m_optionItemList);
		}
		foreach (BaseOptionItem optionItem in m_optionItemList)
		{
			if (!optionItem.IsInitialized)
			{
				optionItem.Initialize();
				optionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(optionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
				optionItem.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(optionItem.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
			}
		}
		if ((bool)m_backOptionItem && !m_backOptionItem.IsInitialized)
		{
			m_backOptionItem.Initialize();
			BaseOptionItem backOptionItem = m_backOptionItem;
			backOptionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(backOptionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
			BaseOptionItem backOptionItem2 = m_backOptionItem;
			backOptionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(backOptionItem2.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
		}
		if ((bool)m_quitOptionItem && !m_quitOptionItem.IsInitialized)
		{
			m_quitOptionItem.Initialize();
			BaseOptionItem quitOptionItem = m_quitOptionItem;
			quitOptionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(quitOptionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
			BaseOptionItem quitOptionItem2 = m_quitOptionItem;
			quitOptionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(quitOptionItem2.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
		}
		if ((bool)m_retireOptionItem && !m_retireOptionItem.IsInitialized)
		{
			m_retireOptionItem.Initialize();
			BaseOptionItem retireOptionItem = m_retireOptionItem;
			retireOptionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(retireOptionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
			BaseOptionItem retireOptionItem2 = m_retireOptionItem;
			retireOptionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(retireOptionItem2.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
		}
		if ((bool)m_restartChallengeOptionItem && !m_restartChallengeOptionItem.IsInitialized)
		{
			m_restartChallengeOptionItem.Initialize();
			BaseOptionItem restartChallengeOptionItem = m_restartChallengeOptionItem;
			restartChallengeOptionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(restartChallengeOptionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
			BaseOptionItem restartChallengeOptionItem2 = m_restartChallengeOptionItem;
			restartChallengeOptionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(restartChallengeOptionItem2.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
		}
		if ((bool)m_skipTutorialOptionItem && !m_skipTutorialOptionItem.IsInitialized)
		{
			m_skipTutorialOptionItem.Initialize();
			BaseOptionItem skipTutorialOptionItem = m_skipTutorialOptionItem;
			skipTutorialOptionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(skipTutorialOptionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
			BaseOptionItem skipTutorialOptionItem2 = m_skipTutorialOptionItem;
			skipTutorialOptionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(skipTutorialOptionItem2.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
		}
		if ((bool)m_houseRulesOptionItem && !m_houseRulesOptionItem.IsInitialized)
		{
			m_houseRulesOptionItem.Initialize();
			BaseOptionItem houseRulesOptionItem = m_houseRulesOptionItem;
			houseRulesOptionItem.OptionItemSelected = (OptionItemSelectedHandler)Delegate.Combine(houseRulesOptionItem.OptionItemSelected, new OptionItemSelectedHandler(UpdateSelectedOptionItem));
			BaseOptionItem houseRulesOptionItem2 = m_houseRulesOptionItem;
			houseRulesOptionItem2.OptionItemActivated = (OptionItemSelectedHandler)Delegate.Combine(houseRulesOptionItem2.OptionItemActivated, new OptionItemSelectedHandler(PlaySelectedSFX));
		}
		RectTransform[] componentsInChildren = m_windowCanvas.GetComponentsInChildren<RectTransform>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			LayoutRebuilder.ForceRebuildLayoutImmediate(componentsInChildren[i]);
		}
		base.Initialize();
	}

	protected override void OnOpen()
	{
		m_loadedFromMainMenu = !WindowManager.GetIsWindowOpen(WindowID.Pause);
		m_windowCanvas.gameObject.SetActive(value: true);
		UpdateSelectedOptionItem(m_optionItemList[0]);
		if (IsSuboptionWindow)
		{
			return;
		}
		if (!m_loadedFromMainMenu)
		{
			m_backOptionItem.gameObject.SetActive(value: false);
			m_quitOptionItem.gameObject.SetActive(value: true);
			m_optionItemList[m_optionItemList.Count - 1] = m_quitOptionItem;
		}
		else
		{
			m_backOptionItem.gameObject.SetActive(value: true);
			m_quitOptionItem.gameObject.SetActive(value: false);
			m_optionItemList[m_optionItemList.Count - 1] = m_backOptionItem;
		}
		if ((SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.World) || ChallengeManager.IsInChallenge) && SaveManager.PlayerSaveData.EndingSpawnRoom != EndingSpawnRoomType.AboveGround)
		{
			if (!m_retireOptionItem.gameObject.activeSelf)
			{
				m_retireOptionItem.gameObject.SetActive(value: true);
				GetComponentsInChildren(m_optionItemList);
			}
			if (ChallengeManager.IsInChallenge)
			{
				m_retireOptionItem.IncrementValueText.text = LocalizationManager.GetString("LOC_ID_OPTIONS_MENU_QUIT_CHALLENGE_1", isFemale: false);
			}
			else
			{
				m_retireOptionItem.IncrementValueText.text = LocalizationManager.GetString("LOC_ID_OPTIONS_MENU_RETIRE_1", isFemale: false);
			}
		}
		else if (m_retireOptionItem.gameObject.activeSelf)
		{
			m_retireOptionItem.gameObject.SetActive(value: false);
			GetComponentsInChildren(m_optionItemList);
		}
		if (ChallengeManager.IsInChallenge)
		{
			if (!m_restartChallengeOptionItem.gameObject.activeSelf)
			{
				m_restartChallengeOptionItem.gameObject.SetActive(value: true);
				GetComponentsInChildren(m_optionItemList);
			}
		}
		else if (m_restartChallengeOptionItem.gameObject.activeSelf)
		{
			m_restartChallengeOptionItem.gameObject.SetActive(value: false);
			GetComponentsInChildren(m_optionItemList);
		}
		if (SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.Tutorial))
		{
			m_skipTutorialOptionItem.gameObject.SetActive(value: true);
			GetComponentsInChildren(m_optionItemList);
		}
		else
		{
			m_skipTutorialOptionItem.gameObject.SetActive(value: false);
			GetComponentsInChildren(m_optionItemList);
		}
		if (!(SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.MainMenu)))
		{
			m_houseRulesOptionItem.gameObject.SetActive(value: true);
			GetComponentsInChildren(m_optionItemList);
		}
		else
		{
			m_houseRulesOptionItem.gameObject.SetActive(value: false);
			GetComponentsInChildren(m_optionItemList);
		}
		if (!EnteredFromOtherSubmenu)
		{
			m_fadeBGCanvasGroup.alpha = 0f;
			TweenManager.TweenTo_UnscaledTime(m_fadeBGCanvasGroup, 0.15f, Ease.None, "alpha", 1);
		}
		else
		{
			m_fadeBGCanvasGroup.alpha = 1f;
		}
		m_settingsCanvasGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(m_settingsCanvasGroup, 0.15f, Ease.None, "alpha", 1);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
		if (SceneLoadingUtility.ActiveScene.name == SceneLoadingUtility.GetSceneName(SceneID.MainMenu))
		{
			PlaySelectedSFX(null);
		}
	}

	protected override void OnFocus()
	{
		AddInputListeners();
		foreach (BaseOptionItem optionItem in m_optionItemList)
		{
			optionItem.Interactable = true;
		}
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
		foreach (BaseOptionItem optionItem in m_optionItemList)
		{
			optionItem.Interactable = false;
		}
	}

	protected void AddInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_invokeOptionIncrementOrDecrement, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_invokeOptionIncrementOrDecrement, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInput, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalInput, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
		}
	}

	protected void RemoveInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_invokeOptionIncrementOrDecrement, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_invokeOptionIncrementOrDecrement, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInput, UpdateLoopType.Update, InputActionEventType.ButtonRepeating, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalInput, UpdateLoopType.Update, InputActionEventType.NegativeButtonRepeating, "Window_Vertical");
		}
	}

	protected virtual void OnConfirmButtonDown(InputActionEventData eventData)
	{
		if (!eventData.IsCurrentInputSource(ControllerType.Mouse) && m_currentSelectedOptionItem != null)
		{
			if (!m_currentSelectedOptionItem.IsActivated)
			{
				m_currentSelectedOptionItem.ActivateOption();
			}
			else
			{
				m_currentSelectedOptionItem.DeactivateOption(confirmOptionChange: true);
			}
		}
	}

	protected virtual void OnCancelButtonDown(InputActionEventData eventData)
	{
		if ((bool)m_currentSelectedOptionItem && m_currentSelectedOptionItem.IsActivated)
		{
			m_currentSelectedOptionItem.DeactivateOption(confirmOptionChange: false);
			return;
		}
		if (!m_loadedFromMainMenu)
		{
			WindowManager.CloseAllOpenWindows();
			return;
		}
		WindowManager.SetWindowIsOpen(ID, isOpen: false);
		WindowManager.SetWindowIsOpen(WindowID.MainMenu, isOpen: true);
		EnteredFromOtherSubmenu = false;
	}

	private void InvokeOptionIncrementOrDecrement(InputActionEventData eventData)
	{
		if (m_currentSelectedOptionItem != null && m_currentSelectedOptionItem.IsActivated && (eventData.GetButtonDown() || eventData.GetNegativeButtonDown() || m_currentSelectedOptionItem.PressAndHoldEnabled))
		{
			float num = eventData.GetAxis();
			if (num == 0f)
			{
				num = 0f - eventData.GetAxisPrev();
			}
			if (num > 0f)
			{
				m_currentSelectedOptionItem.InvokeIncrement();
			}
			else
			{
				m_currentSelectedOptionItem.InvokeDecrement();
			}
		}
	}

	private void OnVerticalInput(InputActionEventData eventData)
	{
		if (!(m_currentSelectedOptionItem != null) || !m_currentSelectedOptionItem.IsActivated)
		{
			int currentSelectedIndex = m_currentSelectedIndex;
			int num = -1;
			float num2 = eventData.GetAxis();
			if (num2 == 0f)
			{
				num2 = 0f - eventData.GetAxisPrev();
			}
			num = ((!(num2 > 0f)) ? ((m_currentSelectedIndex + 1 < m_optionItemList.Count) ? (m_currentSelectedIndex + 1) : 0) : ((m_currentSelectedIndex - 1 < 0) ? (m_optionItemList.Count - 1) : (m_currentSelectedIndex - 1)));
			if (currentSelectedIndex != num)
			{
				m_optionItemList[num].OnSelect(null);
			}
		}
	}

	protected virtual void UpdateSelectedOptionItem(BaseOptionItem menuItem)
	{
		if (!(menuItem == m_currentSelectedOptionItem))
		{
			if (m_currentSelectedOptionItem != null)
			{
				m_currentSelectedOptionItem.OnDeselect(null);
			}
			if (m_changeSelectedOptionEvent != null && base.IsInitialized)
			{
				m_changeSelectedOptionEvent.Invoke();
			}
			m_currentSelectedOptionItem = menuItem;
			m_currentSelectedIndex = GetIndexOf(menuItem);
			if (!IsSuboptionWindow)
			{
				float y = menuItem.transform.position.y;
				Vector3 position = m_selectionIndicator.position;
				position.y = y;
				m_selectionIndicator.position = position;
				Vector3 position2 = m_selectionIndicatorFrill.position;
				position2.y = y;
				m_selectionIndicatorFrill.position = position2;
			}
		}
	}

	public virtual void PlaySelectedSFX(BaseOptionItem menuItem)
	{
		if (m_selectOptionEvent != null)
		{
			m_selectOptionEvent.Invoke();
		}
	}
}
