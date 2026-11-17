using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class TextGlyphManager
{
	private static bool DISABLE_WARNINGS = true;

	private const string KEYBOARD_SPRITEASSET_NAME = "keyboard_glyphs_texture";

	private const string TMP_SPRITE_RICHTEXT_STRING_FORMAT = "<sprite=\"{0}\" name=\"{1}\">";

	private static Dictionary<string, KeyboardKeyCode> m_keyboardGlyphTable = new Dictionary<string, KeyboardKeyCode>();

	private static Dictionary<string, int> m_mouseGlyphTable = new Dictionary<string, int>();

	private static Dictionary<GamepadType, Dictionary<string, string>> m_gamepadGlyphTable = new Dictionary<GamepadType, Dictionary<string, string>>();

	private static List<ControllerTemplateActionElementMap> m_gamepadElementMapList = new List<ControllerTemplateActionElementMap>();

	private static string[] m_kbmActionNameArray;

	public static bool GamepadGlyphTableInitialized { get; private set; }

	public static bool KBMGlyphTableInitialized { get; private set; }

	private static string[] KBMActionNameArray
	{
		get
		{
			if (m_kbmActionNameArray == null)
			{
				List<string> list = new List<string>();
				Rewired_RL.InputActionType[] inputActionTypeArray = Rewired_RL.InputActionTypeArray;
				foreach (Rewired_RL.InputActionType inputActionType in inputActionTypeArray)
				{
					if (inputActionType != 0)
					{
						list.Add(Rewired_RL.GetString(inputActionType));
					}
				}
				Rewired_RL.WindowInputActionType[] windowInputActionTypeArray = Rewired_RL.WindowInputActionTypeArray;
				foreach (Rewired_RL.WindowInputActionType windowInputActionType in windowInputActionTypeArray)
				{
					if (windowInputActionType != 0)
					{
						list.Add(Rewired_RL.GetString(windowInputActionType));
					}
				}
				m_kbmActionNameArray = list.ToArray();
			}
			return m_kbmActionNameArray;
		}
	}

	public static string GetGamepadTextGlyph(string actionName, GamepadType gamepadTypeOverride = GamepadType.None)
	{
		if (GamepadGlyphTableInitialized)
		{
			GamepadType gamepadType = gamepadTypeOverride;
			if (gamepadType == GamepadType.None)
			{
				gamepadType = ControllerGlyphLibrary.GetGamepadTypeFromInputIconSetting(SaveManager.ConfigData.InputIconSetting);
				if (gamepadType == GamepadType.None && SaveManager.ConfigData.InputIconSetting == InputIconSetting.Auto)
				{
					Debug.Log("Could not auto find valid gamepad type. Check to ensure the GamepadType is properly set on the controller's correspondingControllerGlyphData file.");
				}
			}
			if (m_gamepadGlyphTable.TryGetValue(gamepadType, out var value))
			{
				if (value.TryGetValue(actionName, out var value2))
				{
					return value2;
				}
				if (!DISABLE_WARNINGS)
				{
					Debug.Log("<color=red>Could not get text glyph: " + actionName + " for gamepadType: " + gamepadType.ToString() + ". No text glyph found.</color>");
				}
			}
			else if (!DISABLE_WARNINGS)
			{
				Debug.Log("<color=red>Could not get text glyph: " + actionName + ". No glyph table of type: " + gamepadType.ToString() + " found.</color>");
			}
		}
		else if (!DISABLE_WARNINGS)
		{
			Debug.Log("<color=red>Could not get text glyph: " + actionName + ". TextGlyphManager.GamepadGlyphTable has not been initialized.</color>");
		}
		return null;
	}

	public static int GetKeyboardTextGlyph(string actionName, out bool isKeyCode)
	{
		if (!KBMGlyphTableInitialized)
		{
			CreateAllGlyphTables(ControllerType.Keyboard);
		}
		if (KBMGlyphTableInitialized)
		{
			if (m_keyboardGlyphTable.TryGetValue(actionName, out var value))
			{
				isKeyCode = true;
				return (int)value;
			}
			if (m_mouseGlyphTable.TryGetValue(actionName, out var value2))
			{
				isKeyCode = false;
				return value2;
			}
			if (!DISABLE_WARNINGS)
			{
				Debug.Log("<color=red>Could not get text glyph: " + actionName + " for keyboard/mouse. No text glyph found.</color>");
			}
		}
		else if (!DISABLE_WARNINGS)
		{
			Debug.Log("<color=red>Could not get text glyph: " + actionName + ". TextGlyphManager.KeyboardGlyphTableInitialized has not been initialized.</color>");
		}
		isKeyCode = true;
		return -1;
	}

	public static string GetFullTMPTextGlyph(string actionName, bool getGamepad, GamepadType gamepadTypeOverride = GamepadType.None)
	{
		bool flag = actionName == "Window_AllMovement_LStick";
		bool flag2 = actionName == "Window_AllMovement_RStick";
		if (flag)
		{
			actionName = "Window_Horizontal";
		}
		else if (flag2)
		{
			actionName = "Window_Horizontal_RStick";
		}
		if (actionName == "FreeLook" && getGamepad)
		{
			actionName = "Window_Horizontal_RStick";
			flag2 = true;
		}
		if (getGamepad)
		{
			GamepadType gamepadType = gamepadTypeOverride;
			if (gamepadType == GamepadType.None)
			{
				gamepadType = ControllerGlyphLibrary.GetGamepadTypeFromInputIconSetting(SaveManager.ConfigData.InputIconSetting);
			}
			ControllerGlyphData glyphData = ControllerGlyphLibrary.GetGlyphData(gamepadType);
			string gamepadTextGlyph = GetGamepadTextGlyph(actionName, gamepadTypeOverride);
			string spriteAssetName = glyphData.SpriteAssetName;
			if (!string.IsNullOrEmpty(gamepadTextGlyph))
			{
				return $"<sprite=\"{spriteAssetName}\" name=\"{gamepadTextGlyph}\">";
			}
		}
		else
		{
			bool isKeyCode;
			int keyboardTextGlyph = GetKeyboardTextGlyph(actionName, out isKeyCode);
			if (keyboardTextGlyph != -1 || flag || flag2)
			{
				if (isKeyCode || flag || flag2)
				{
					KeyboardKeyCode keyboardKeyCode = (KeyboardKeyCode)keyboardTextGlyph;
					string text = KeycodeConverter.GetKeycodeStringName(keyboardKeyCode, shift: false, capsLock: false, numLock: false).ToUpper();
					if (flag || flag2)
					{
						return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ArrowKeys_Glyph");
					}
					switch (keyboardKeyCode)
					{
					case KeyboardKeyCode.LeftArrow:
						return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ArrowLeft_Glyph");
					case KeyboardKeyCode.RightArrow:
						return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ArrowRight_Glyph");
					case KeyboardKeyCode.UpArrow:
						return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ArrowUp_Glyph");
					case KeyboardKeyCode.DownArrow:
						return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ArrowDown_Glyph");
					case KeyboardKeyCode.Space:
						return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ButtonLarge_Glyph");
					default:
						if (text.Length > 1)
						{
							return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ButtonMedium_Glyph");
						}
						return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_ButtonSmall_Glyph");
					}
				}
				switch (keyboardTextGlyph)
				{
				case 3:
					return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_MouseLeft_Glyph");
				case 4:
					return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_MouseRight_Glyph");
				case 5:
					return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_MouseMiddle_Glyph");
				case 6:
					return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_Mouse4_Glyph");
				case 7:
					return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_Mouse5_Glyph");
				case 8:
					return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_Mouse6_Glyph");
				case 9:
					return string.Format("<sprite=\"{0}\" name=\"{1}\">", "keyboard_glyphs_texture", "KB_Mouse7_Glyph");
				}
			}
		}
		return null;
	}

	public static void CreateAllGlyphTables(ControllerType controllerType)
	{
		if (controllerType == ControllerType.Joystick)
		{
			m_gamepadGlyphTable.Clear();
			m_gamepadElementMapList.Clear();
			int firstAvailableJoystickControllerID = RewiredOnStartupController.GetFirstAvailableJoystickControllerID();
			if (firstAvailableJoystickControllerID == -1)
			{
				return;
			}
			ControllerMap map = Rewired_RL.Player.controllers.maps.GetMap(ControllerType.Joystick, firstAvailableJoystickControllerID, Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.Action), 0);
			ControllerMap map2 = Rewired_RL.Player.controllers.maps.GetMap(ControllerType.Joystick, firstAvailableJoystickControllerID, Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.ActionRemappable), 0);
			ControllerMap map3 = Rewired_RL.Player.controllers.maps.GetMap(ControllerType.Joystick, firstAvailableJoystickControllerID, Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.Window), 0);
			ControllerMap map4 = Rewired_RL.Player.controllers.maps.GetMap(ControllerType.Joystick, firstAvailableJoystickControllerID, Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.WindowRemappable), 0);
			m_gamepadElementMapList.AddRange(map.ToControllerTemplateMap<IGamepadTemplate>().ElementMaps);
			m_gamepadElementMapList.AddRange(map2.ToControllerTemplateMap<IGamepadTemplate>().ElementMaps);
			m_gamepadElementMapList.AddRange(map3.ToControllerTemplateMap<IGamepadTemplate>().ElementMaps);
			m_gamepadElementMapList.AddRange(map4.ToControllerTemplateMap<IGamepadTemplate>().ElementMaps);
			GamepadType[] gamepadTypeArray = GamepadType_RL.GamepadTypeArray;
			foreach (GamepadType gamepadType in gamepadTypeArray)
			{
				if (gamepadType != 0)
				{
					CreateOrReplaceGamepadGlyphTable(gamepadType);
				}
			}
			GamepadGlyphTableInitialized = true;
		}
		else
		{
			m_keyboardGlyphTable.Clear();
			m_mouseGlyphTable.Clear();
			CreateOrReplaceKBMGlyphTable();
			CreateOrReplaceKBMGlyphTable();
			KBMGlyphTableInitialized = true;
		}
	}

	public static void CreateOrReplaceGamepadGlyphTable(GamepadType gamepadType)
	{
		ControllerGlyphData glyphData = ControllerGlyphLibrary.GetGlyphData(gamepadType);
		foreach (ControllerTemplateActionElementMap gamepadElementMap in m_gamepadElementMapList)
		{
			string name = ReInput.mapping.GetAction(gamepadElementMap.actionId).name;
			int elementIdentifierId = gamepadElementMap.elementIdentifierId;
			AddOrReplaceGamepadGlyph(name, glyphData.GetGlyphName(elementIdentifierId, AxisRange.Full), gamepadType);
			if (Rewired_RL.HasPoleAxes(name))
			{
				AddOrReplaceGamepadGlyph(name + "+", glyphData.GetGlyphName(elementIdentifierId, AxisRange.Positive), gamepadType);
				AddOrReplaceGamepadGlyph(name + "-", glyphData.GetGlyphName(elementIdentifierId, AxisRange.Negative), gamepadType);
			}
		}
		AddOrReplaceGamepadGlyph("Window_AllMovement_LStick", glyphData.GetGlyphName(23, AxisRange.Full), gamepadType);
		AddOrReplaceGamepadGlyph("Window_AllMovement_RStick", glyphData.GetGlyphName(24, AxisRange.Full), gamepadType);
		AddOrReplaceGamepadGlyph("FreeLook", glyphData.GetGlyphName(24, AxisRange.Full), gamepadType);
	}

	public static void CreateOrReplaceKBMGlyphTable()
	{
		string[] kBMActionNameArray = KBMActionNameArray;
		for (int i = 0; i < kBMActionNameArray.Length; i++)
		{
			string text = kBMActionNameArray[i];
			bool num = text == "Window_AllMovement_LStick";
			bool flag = text == "Window_AllMovement_RStick";
			if (num)
			{
				text = "Window_Horizontal";
			}
			else if (flag)
			{
				text = "Window_Horizontal_RStick";
			}
			ActionElementMap actionElementMap = Rewired_RL.GetActionElementMap(useGamepad: false, text, Pole.Positive, prioritizeNonRemappable: false, 0);
			if (actionElementMap == null)
			{
				if (!DISABLE_WARNINGS)
				{
					Debug.Log("<color=yellow>FAILED TO CREATE ACTION GLYPH: Action null on: " + text + " for controller KB/M.</color>");
				}
				continue;
			}
			switch (actionElementMap.controllerMap.controllerType)
			{
			case ControllerType.Keyboard:
			{
				KeyboardKeyCode keyboardKeyCode = actionElementMap.keyboardKeyCode;
				AddOrReplaceKeyboardGlyph(text, keyboardKeyCode);
				break;
			}
			case ControllerType.Mouse:
				AddOrReplaceMouseGlyph(text, actionElementMap.elementIdentifierId);
				break;
			}
			if (!Rewired_RL.HasPoleAxes(text))
			{
				continue;
			}
			string actionName = text + "+";
			if (actionElementMap.controllerMap.controllerType == ControllerType.Keyboard)
			{
				AddOrReplaceKeyboardGlyph(actionName, actionElementMap.keyboardKeyCode);
			}
			else if (actionElementMap.controllerMap.controllerType == ControllerType.Mouse)
			{
				AddOrReplaceMouseGlyph(actionName, actionElementMap.elementIdentifierId);
			}
			ActionElementMap actionElementMap2 = Rewired_RL.GetActionElementMap(useGamepad: false, text, Pole.Negative, prioritizeNonRemappable: false, 0);
			string text2 = text + "-";
			if (actionElementMap2 != null)
			{
				if (actionElementMap2.controllerMap.controllerType == ControllerType.Keyboard)
				{
					AddOrReplaceKeyboardGlyph(text2, actionElementMap2.keyboardKeyCode);
				}
				else if (actionElementMap2.controllerMap.controllerType == ControllerType.Mouse)
				{
					AddOrReplaceMouseGlyph(text2, actionElementMap2.elementIdentifierId);
				}
			}
			else if (!DISABLE_WARNINGS)
			{
				Debug.Log("<color=yellow>FAILED TO CREATE ACTION GLYPH: Action null on: " + text2 + " for controller type: " + ControllerType.Keyboard.ToString() + "</color>");
			}
		}
	}

	private static void AddOrReplaceGamepadGlyph(string actionName, string textGlyph, GamepadType gamepadType)
	{
		Dictionary<string, string> dictionary;
		if (m_gamepadGlyphTable.ContainsKey(gamepadType))
		{
			dictionary = m_gamepadGlyphTable[gamepadType];
		}
		else
		{
			dictionary = new Dictionary<string, string>();
			m_gamepadGlyphTable.Add(gamepadType, dictionary);
		}
		if (dictionary.ContainsKey(actionName))
		{
			dictionary[actionName] = textGlyph;
		}
		else
		{
			dictionary.Add(actionName, textGlyph);
		}
	}

	private static void AddOrReplaceKeyboardGlyph(string actionName, KeyboardKeyCode keyCode)
	{
		if (m_keyboardGlyphTable.ContainsKey(actionName))
		{
			m_keyboardGlyphTable[actionName] = keyCode;
		}
		else
		{
			m_keyboardGlyphTable.Add(actionName, keyCode);
		}
	}

	private static void AddOrReplaceMouseGlyph(string actionName, int elementIdentifier)
	{
		if (m_mouseGlyphTable.ContainsKey(actionName))
		{
			m_mouseGlyphTable[actionName] = elementIdentifier;
		}
		else
		{
			m_mouseGlyphTable.Add(actionName, elementIdentifier);
		}
	}
}
