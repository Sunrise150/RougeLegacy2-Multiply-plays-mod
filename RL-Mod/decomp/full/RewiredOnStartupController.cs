using System.Collections;
using RL_Windows;
using Rewired;
using UnityEngine;

public class RewiredOnStartupController : MonoBehaviour
{
	private Controller m_activeControllerUsed;

	public static ControllerType CurrentActiveControllerType { get; private set; } = ControllerType.Custom;


	public static GamepadType CurrentActiveGamepadType { get; private set; } = GamepadType.Default_Xbox;


	public static int GetFirstAvailableJoystickControllerID()
	{
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			if (Rewired_RL.IsStandardJoystick(joystick))
			{
				return joystick.identifier.controllerId;
			}
		}
		return -1;
	}

	private void OnEnable()
	{
		ReInput.ControllerConnectedEvent += OnControllerConnected;
		ReInput.ControllerDisconnectedEvent += OnControllerDisconnected;
		ReInput.controllers.AddLastActiveControllerChangedDelegate(ActiveControllerChanged);
		UpdateJoystickCalibrationMap();
	}

	private IEnumerator Start()
	{
		while (StoreAPIManager.InitState != StoreAPIManager.StoreInitState.Succeeded || !SaveManager.IsInitialized)
		{
			yield return null;
		}
		SaveManager.LoadAllControllerMaps();
		TextGlyphManager.CreateAllGlyphTables(ControllerType.Joystick);
		TextGlyphManager.CreateAllGlyphTables(ControllerType.Keyboard);
	}

	private void OnDisable()
	{
		ReInput.ControllerConnectedEvent -= OnControllerConnected;
		ReInput.ControllerDisconnectedEvent -= OnControllerDisconnected;
		ReInput.controllers.RemoveLastActiveControllerChangedDelegate(ActiveControllerChanged);
	}

	private void OnControllerConnected(ControllerStatusChangedEventArgs args)
	{
		if (args.controllerType == ControllerType.Joystick && !Rewired_RL.IsStandardJoystick(args.controller))
		{
			ReInput.controllers.RemoveControllerFromAllPlayers(args.controller);
			return;
		}
		bool isCurrentMapEnabled = RewiredMapController.IsCurrentMapEnabled;
		RewiredMapController.SetMap(RewiredMapController.CurrentGameInputMode);
		RewiredMapController.SetCurrentMapEnabled(isCurrentMapEnabled);
		int controllerID = 0;
		bool flag = args.controllerType == ControllerType.Joystick;
		if (flag)
		{
			controllerID = args.controllerId;
		}
		if (SaveManager.IsInitialized)
		{
			SaveManager.LoadControllerMap(flag, controllerID);
		}
		if (flag && !TextGlyphManager.GamepadGlyphTableInitialized)
		{
			TextGlyphManager.CreateAllGlyphTables(ControllerType.Joystick);
		}
		else if (!flag && !TextGlyphManager.KBMGlyphTableInitialized)
		{
			TextGlyphManager.CreateAllGlyphTables(ControllerType.Keyboard);
		}
		UpdateJoystickCalibrationMap();
	}

	private void OnControllerDisconnected(ControllerStatusChangedEventArgs args)
	{
		if (!m_activeControllerUsed.isConnected)
		{
			WindowManager.PauseWhenPossible(shouldPause: true, gamepadDisconnected: true);
		}
	}

	public static void UpdateJoystickCalibrationMap()
	{
		if (!ReInput.isReady)
		{
			return;
		}
		foreach (Joystick joystick in Rewired_RL.Player.controllers.Joysticks)
		{
			if (!Rewired_RL.IsStandardJoystick(joystick))
			{
				continue;
			}
			int axisCount = joystick.axisCount;
			int axisCount2 = joystick.calibrationMap.axisCount;
			for (int i = 0; i < axisCount2; i++)
			{
				if (i >= axisCount || joystick.Axes[i].isMemberElement)
				{
					AxisCalibration axisCalibration = joystick.calibrationMap.Axes[i];
					axisCalibration.Reset();
					axisCalibration.applyRangeCalibration = false;
					axisCalibration.deadZone = Mathf.Clamp(SaveManager.ConfigData.DeadZone, 0.05f, 0.95f);
				}
			}
		}
	}

	private void ActiveControllerChanged(Controller newActiveController)
	{
		if (newActiveController.type == ControllerType.Joystick)
		{
			UpdateActiveGamepadType();
			MouseCursorManager.SetCursorVisible(visible: false);
		}
		else
		{
			MouseCursorManager.SetCursorVisible(visible: true);
			RumbleManager.StopRumble();
		}
		m_activeControllerUsed = newActiveController;
		CurrentActiveControllerType = newActiveController.type;
	}

	public static void UpdateActiveGamepadType()
	{
		CurrentActiveGamepadType = ControllerGlyphLibrary.GetGamepadTypeFromInputIconSetting(SaveManager.ConfigData.InputIconSetting);
	}
}
