using System;
using System.Collections;
using FMODUnity;
using RL_Windows;
using Rewired;
using UnityEngine;
using UnityEngine.EventSystems;

public class RemapInputOptionItem : ExecuteImmediateOptionItem
{
	[SerializeField]
	private GenericInfoTextBox m_textBox;

	[SerializeField]
	private GameObject m_lockIcon;

	[SerializeField]
	private GamepadImageSelectorController m_gamepadImageSelectorController;

	[SerializeField]
	private StudioEventEmitter m_successfulRemapSFX;

	[SerializeField]
	private StudioEventEmitter m_failedRemapSFX;

	private bool m_lockInput;

	private RLInputRemapListener m_remapListener;

	private TextGlyphConverter m_textGlyphConverter;

	private Controller m_assignedController;

	private Action m_confirmResetToDefault;

	private Action<InputActionEventData> m_resetToDefault;

	protected override void Awake()
	{
		base.Awake();
		m_remapListener = GetComponent<RLInputRemapListener>();
		m_textGlyphConverter = m_titleText.GetComponent<TextGlyphConverter>();
		if (m_remapListener.UsesGamepad)
		{
			m_textGlyphConverter.ForcedControllerType = ControllerType.Joystick;
		}
		else
		{
			m_textGlyphConverter.ForcedControllerType = ControllerType.Keyboard;
		}
		m_textGlyphConverter.OverrideControllerType = true;
		m_confirmResetToDefault = ConfirmResetToDefault;
		m_resetToDefault = ResetToDefault;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		if ((bool)m_gamepadImageSelectorController && (bool)m_gamepadImageSelectorController.ActiveSelector)
		{
			m_gamepadImageSelectorController.ActiveSelector.EnableGamepadImage(Rewired_RL.GetString(m_remapListener.InputActionType), m_remapListener.Axis, disableAllOthers: true);
		}
	}

	private void UpdateLockState()
	{
		string @string = Rewired_RL.GetString(m_remapListener.InputActionType);
		if (m_remapListener.UsesGamepad)
		{
			m_lockInput = !Rewired_RL.DoesActionExistInMapCategory(@string, Rewired_RL.MapCategoryType.ActionRemappable, ControllerType.Joystick, m_remapListener.Axis);
		}
		else
		{
			m_lockInput = !Rewired_RL.DoesActionExistInMapCategory(@string, Rewired_RL.MapCategoryType.ActionRemappable, ControllerType.Keyboard, m_remapListener.Axis);
			if (m_lockInput)
			{
				m_lockInput = !Rewired_RL.DoesActionExistInMapCategory(@string, Rewired_RL.MapCategoryType.ActionRemappable, ControllerType.Mouse, m_remapListener.Axis);
			}
		}
		m_lockIcon.SetActive(m_lockInput);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		string text = "";
		if (m_remapListener.UsesAxisContribution)
		{
			text = ((m_remapListener.Axis == Pole.Positive) ? "+" : "-");
		}
		m_titleText.text = "[" + Rewired_RL.GetString(m_remapListener.InputActionType) + text + "]";
		m_assignedController = ReInput.controllers.GetLastActiveController();
		int firstAvailableJoystickControllerID = RewiredOnStartupController.GetFirstAvailableJoystickControllerID();
		if (m_assignedController.identifier.controllerType != ControllerType.Joystick && firstAvailableJoystickControllerID != -1)
		{
			m_assignedController = Rewired_RL.Player.controllers.GetController(ControllerType.Joystick, firstAvailableJoystickControllerID);
		}
		m_textGlyphConverter.ForcedGamepadType = ControllerGlyphLibrary.GetGlyphData(m_assignedController.hardwareTypeGuid).GamepadType;
		if (m_textGlyphConverter.IsInitialized)
		{
			m_textGlyphConverter.UpdateText(forceUpdate: true);
		}
		UpdateLockState();
		RLInputRemapper.OnRemapComplete = (Action<bool>)Delegate.Combine(RLInputRemapper.OnRemapComplete, new Action<bool>(UpdateInputName));
		ReInput.players.GetPlayer(0).AddInputEventDelegate(m_resetToDefault, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_R");
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		RLInputRemapper.OnRemapComplete = (Action<bool>)Delegate.Remove(RLInputRemapper.OnRemapComplete, new Action<bool>(UpdateInputName));
		if (ReInput.isReady)
		{
			ReInput.players.GetPlayer(0).RemoveInputEventDelegate(m_resetToDefault, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_R");
		}
	}

	private void ResetToDefault(InputActionEventData eventData)
	{
		if ((m_remapListener.UsesGamepad && !eventData.IsCurrentInputSource(ControllerType.Joystick)) || (!m_remapListener.UsesGamepad && eventData.IsCurrentInputSource(ControllerType.Joystick)))
		{
			return;
		}
		SuboptionsWindowController suboptionsWindowController = WindowManager.GetWindowController(WindowID.Suboptions) as SuboptionsWindowController;
		if ((bool)suboptionsWindowController && suboptionsWindowController.CurrentlySelectedOptionItem == this)
		{
			Debug.Log("Resetting controller map: " + m_textGlyphConverter.ForcedControllerType.ToString() + " to default");
			Rewired_RL.ResetControllerMapToDefault(m_remapListener.UsesGamepad);
			if (m_textGlyphConverter.ForcedControllerType == ControllerType.Joystick)
			{
				TextGlyphManager.CreateAllGlyphTables(ControllerType.Joystick);
			}
			else
			{
				TextGlyphManager.CreateAllGlyphTables(ControllerType.Keyboard);
			}
			RLInputRemapper.SetRemappingFlagDirty(m_remapListener.UsesGamepad);
			RLInputRemapper.OnRemapComplete(obj: false);
			if ((bool)m_gamepadImageSelectorController && (bool)m_gamepadImageSelectorController.ActiveSelector)
			{
				m_gamepadImageSelectorController.ActiveSelector.EnableGamepadImage(Rewired_RL.GetString(m_remapListener.InputActionType), m_remapListener.Axis, disableAllOthers: true);
			}
			LocalizationManager.ForceRefreshAllTextGlyphs();
			InitializeDefaultConfirmMenu();
		}
	}

	private void InitializeDefaultConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_OPTIONS_MENU_RESET_TO_DEFAULT_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_CONFIRM_MENU_RESET_TO_DEFAULT_1", isLocID: true);
		obj.SetNumberOfButtons(1);
		obj.SetOnCancelAction(m_confirmResetToDefault);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmResetToDefault);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void ConfirmResetToDefault()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}

	public override void ActivateOption()
	{
		if ((!m_remapListener.UsesGamepad || ReInput.controllers.GetLastActiveController() == m_assignedController) && (m_remapListener.UsesGamepad || ReInput.controllers.GetLastActiveController().identifier.controllerType != ControllerType.Joystick))
		{
			base.ActivateOption();
			if (!m_lockInput)
			{
				StartCoroutine(RunInputRemapListenerCoroutine());
				return;
			}
			m_lockIcon.transform.localScale = new Vector3(1.25f, 1.25f, 1f);
			TweenManager.TweenTo_UnscaledTime(m_lockIcon.transform, 0.1f, Ease.None, "localScale.x", 1, "localScale.y", 1);
		}
	}

	private IEnumerator RunInputRemapListenerCoroutine()
	{
		m_textBox.transform.parent.gameObject.SetActive(value: true);
		m_textBox.transform.localScale = new Vector3(0f, 0f, 1f);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		RLInputRemapper.OnRemapComplete = (Action<bool>)Delegate.Combine(RLInputRemapper.OnRemapComplete, new Action<bool>(InputRemapListenerComplete));
		yield return TweenManager.TweenTo_UnscaledTime(m_textBox.transform, 0.15f, Ease.Back.EaseOut, "localScale.x", 1, "localScale.y", 1).TweenCoroutine;
		m_remapListener.ListenForRemapInput();
	}

	private void InputRemapListenerComplete(bool remapSuccessful)
	{
		if (remapSuccessful)
		{
			m_successfulRemapSFX.Play();
		}
		else
		{
			m_failedRemapSFX.Play();
		}
		RLInputRemapper.OnRemapComplete = (Action<bool>)Delegate.Remove(RLInputRemapper.OnRemapComplete, new Action<bool>(InputRemapListenerComplete));
		StartCoroutine(EndInputRemapListenerCoroutine());
	}

	private IEnumerator EndInputRemapListenerCoroutine()
	{
		LocalizationManager.ForceRefreshAllTextGlyphs();
		yield return TweenManager.TweenTo_UnscaledTime(m_textBox.transform, 0.15f, Ease.Back.EaseIn, "localScale.x", 0, "localScale.y", 0).TweenCoroutine;
		m_textBox.transform.parent.gameObject.SetActive(value: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		if ((bool)m_gamepadImageSelectorController && (bool)m_gamepadImageSelectorController.ActiveSelector)
		{
			m_gamepadImageSelectorController.ActiveSelector.EnableGamepadImage(Rewired_RL.GetString(m_remapListener.InputActionType), m_remapListener.Axis, disableAllOthers: true);
		}
	}

	private void UpdateInputName(bool remapSuccessful)
	{
		string text = "";
		if (m_remapListener.UsesAxisContribution)
		{
			text = ((m_remapListener.Axis == Pole.Positive) ? "+" : "-");
		}
		m_titleText.text = "[" + Rewired_RL.GetString(m_remapListener.InputActionType) + text + "]";
		if (m_textGlyphConverter.IsInitialized)
		{
			m_textGlyphConverter.UpdateText(forceUpdate: true);
		}
	}
}
