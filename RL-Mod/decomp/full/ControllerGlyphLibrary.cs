using System;
using System.Collections.Generic;
using Rewired;
using Rewired.Data.Mapping;
using TMPro;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Libraries/Controller Glyph Library")]
public class ControllerGlyphLibrary : ScriptableObject
{
	public const string RESOURCES_PATH = "Scriptable Objects/Libraries/ControllerGlyphLibrary";

	[SerializeField]
	private TextMeshProUGUI m_iconTextStyleTemplate;

	[SerializeField]
	private List<ControllerGlyphData> m_glyphLibrary;

	public const GamepadType DEFAULT_GAMEPAD_TYPE = GamepadType.Default_Xbox;

	private static ControllerGlyphLibrary m_instance;

	private static ControllerGlyphLibrary Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = CDGResources.Load<ControllerGlyphLibrary>("Scriptable Objects/Libraries/ControllerGlyphLibrary");
			}
			return m_instance;
		}
	}

	public static TextMeshProUGUI IconTextStyleTemplate => Instance.m_iconTextStyleTemplate;

	public static ControllerGlyphData GetGlyphData(GamepadType gamepadType, bool returnDefaultIfNull = true)
	{
		ControllerGlyphData controllerGlyphData = null;
		foreach (ControllerGlyphData item in Instance.m_glyphLibrary)
		{
			if (item.GamepadType == gamepadType)
			{
				controllerGlyphData = item;
			}
		}
		if (returnDefaultIfNull)
		{
			if (controllerGlyphData == null && gamepadType != GamepadType.Default_Xbox)
			{
				return GetGlyphData(GamepadType.Default_Xbox, returnDefaultIfNull: false);
			}
			if (controllerGlyphData == null && gamepadType == GamepadType.Default_Xbox)
			{
				Debug.Log("<color=red>Gamepad Type: " + gamepadType.ToString() + " not found in Controller Glyph Library. Please ensure the Gamepad exists in the Controller Glyph Library scriptable object.</color>");
			}
		}
		return controllerGlyphData;
	}

	public static ControllerGlyphData GetGlyphData(Guid guid, bool returnDefaultIfNull = true)
	{
		foreach (ControllerGlyphData item in Instance.m_glyphLibrary)
		{
			HardwareJoystickMap[] joystickMaps = item.JoystickMaps;
			for (int i = 0; i < joystickMaps.Length; i++)
			{
				if (joystickMaps[i].Guid == guid)
				{
					return item;
				}
			}
		}
		if (returnDefaultIfNull)
		{
			return GetGlyphData(GamepadType.Default_Xbox, returnDefaultIfNull: false);
		}
		return null;
	}

	public static GamepadType GetGamepadTypeFromInputIconSetting(InputIconSetting inputIconSetting)
	{
		return inputIconSetting switch
		{
			InputIconSetting.Auto => GetGlyphData(ReInput.controllers.GetLastActiveController().hardwareTypeGuid).GamepadType, 
			InputIconSetting.Xbox => GamepadType.Default_Xbox, 
			InputIconSetting.PlayStation => GamepadType.Playstation, 
			InputIconSetting.Switch => GamepadType.Switch, 
			_ => GamepadType.None, 
		};
	}
}
