using System.Collections.Generic;
using RL_Windows;
using Rewired;
using UnityEngine;

public class GamepadImageSelectorController : MonoBehaviour
{
	[SerializeField]
	private TextGlyphConverter m_resetToDefaultGlyphConverter;

	private GamepadType m_currentGamepadType;

	private Dictionary<GamepadType, GamepadImageSelector> m_gamepadSelectorTable;

	public GamepadType CurrentGamepadType => m_currentGamepadType;

	public GamepadImageSelector ActiveSelector { get; private set; }

	public static int ControllerID { get; private set; } = -1;


	private void Awake()
	{
		m_gamepadSelectorTable = new Dictionary<GamepadType, GamepadImageSelector>();
		GamepadImageSelector[] componentsInChildren = GetComponentsInChildren<GamepadImageSelector>(includeInactive: true);
		foreach (GamepadImageSelector gamepadImageSelector in componentsInChildren)
		{
			if (gamepadImageSelector.GamepadType == GamepadType.Default_Xbox)
			{
				if (!gamepadImageSelector.IsXboxSeries)
				{
					m_gamepadSelectorTable.Add(gamepadImageSelector.GamepadType, gamepadImageSelector);
				}
			}
			else
			{
				m_gamepadSelectorTable.Add(gamepadImageSelector.GamepadType, gamepadImageSelector);
			}
		}
	}

	private void OnEnable()
	{
		if (ReInput.controllers.GetLastActiveController().identifier.controllerType == ControllerType.Joystick)
		{
			ControllerID = ReInput.controllers.GetLastActiveController().identifier.controllerId;
		}
		else
		{
			ControllerID = RewiredOnStartupController.GetFirstAvailableJoystickControllerID();
			if (ControllerID == -1)
			{
				SuboptionsWindowController suboptionsWindowController = WindowManager.GetWindowController(WindowID.Suboptions) as SuboptionsWindowController;
				if ((bool)suboptionsWindowController)
				{
					Debug.Log("<color=red>ERROR: Player somehow entered the gamepad remap window with a non-gamepad device.</color>");
					suboptionsWindowController.ForceCancelButtonDown(default(InputActionEventData));
				}
				return;
			}
		}
		foreach (KeyValuePair<GamepadType, GamepadImageSelector> item in m_gamepadSelectorTable)
		{
			item.Value.ControllerID = ControllerID;
			item.Value.gameObject.SetActive(value: false);
		}
		Controller controller = Rewired_RL.Player.controllers.GetController(ControllerType.Joystick, ControllerID);
		m_currentGamepadType = ControllerGlyphLibrary.GetGlyphData(controller.hardwareTypeGuid).GamepadType;
		if (m_currentGamepadType == GamepadType.None)
		{
			m_currentGamepadType = GamepadType.Default_Xbox;
		}
		if (m_gamepadSelectorTable.TryGetValue(m_currentGamepadType, out var value))
		{
			ActiveSelector = value;
		}
		else
		{
			ActiveSelector = null;
		}
		if ((bool)ActiveSelector)
		{
			ActiveSelector.gameObject.SetActive(value: true);
		}
		ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
		m_resetToDefaultGlyphConverter.ForcedGamepadType = m_currentGamepadType;
	}

	private void OnDisable()
	{
		ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
	}

	private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (args.controllerType == ControllerType.Joystick && args.controllerId == ControllerID)
		{
			(WindowManager.GetWindowController(WindowID.Suboptions) as SuboptionsWindowController).ForceCancelButtonDown(default(InputActionEventData));
		}
	}
}
