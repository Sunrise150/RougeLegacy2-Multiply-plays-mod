using System;
using System.Collections.Generic;
using Rewired;
using UnityEngine;

public class Rewired_RL
{
	[Flags]
	public enum MapCategoryType
	{
		None = 0,
		Action = 1,
		ActionRemappable = 2,
		Window = 4,
		WindowRemappable = 8,
		IllegalAssignments = 0x10,
		GamepadGlyphs = 0x20
	}

	public enum InputActionType
	{
		None,
		Action_Jump,
		Action_MoveHorizontal,
		Action_MoveVertical,
		Action_Attack,
		Action_DashLeft,
		Action_DashRight,
		Action_Spell,
		Action_Talent,
		Action_Interact,
		Action_Start,
		Action_Select,
		Action_Pause,
		Action_DebugLB,
		Action_DebugRB,
		Action_DebugL3,
		Action_DebugR3,
		Action_UserReport,
		Action_Downstrike,
		Action_MoveHorizontalR,
		Action_MoveVerticalR,
		Action_FreeLook,
		Action_Flight
	}

	public enum WindowInputActionType
	{
		None,
		Window_Horizontal,
		Window_Vertical,
		Window_Start,
		Window_Select,
		Window_Confirm,
		Window_Cancel,
		Window_X,
		Window_Y,
		Window_LB,
		Window_RB,
		Window_LT,
		Window_RT,
		Window_Horizontal_RStick,
		Window_Vertical_RStick,
		Window_AllMovement_LStick,
		Window_AllMovement_RStick,
		Window_UserReport,
		Window_R
	}

	private static List<ActionElementMap> m_reusableAemList = new List<ActionElementMap>();

	private static List<ControllerType> m_controllerTypeHelperList = new List<ControllerType>();

	public const string ACTION_MAPCATEGORY_NAME = "Player";

	public const string ACTION_REMAPPABLE_MAPCATEGORY_NAME = "PlayerRemappable";

	public const string WINDOW_MAPCATEGORY_NAME = "Window";

	public const string WINDOW_REMAPPABLE_MAPCATEGORY_NAME = "WindowRemappable";

	public const string ILLEGAL_ASSIGNMENTS_MAPCATEGORY_NAME = "IllegalAssignments";

	public const string GAMEPAD_GLYPHS_CATEGORY_NAME = "GamepadGlyphs";

	public const string LAYOUT_NAME = "Default";

	public const string Action_Jump = "Jump";

	public const string Action_MoveHorizontal = "MoveHorizontal";

	public const string Action_MoveVertical = "MoveVertical";

	public const string Action_MoveHorizontalR = "MoveHorizontalR";

	public const string Action_MoveVerticalR = "MoveVerticalR";

	public const string Action_Attack = "Attack";

	public const string Action_DashLeft = "DashLeft";

	public const string Action_DashRight = "DashRight";

	public const string Action_Spell = "Spell";

	public const string Action_Talent = "Talent";

	public const string Action_Interact = "Interact";

	public const string Action_Downstrike = "Downstrike";

	public const string Action_FreeLook = "FreeLook";

	public const string Action_Flight = "Flight";

	public const string Action_Start = "Start";

	public const string Action_Select = "Select";

	public const string Action_Pause = "Pause";

	public const string Action_UserReport = "UserReport";

	public const string Action_DebugLB = "DEBUG_LB";

	public const string Action_DebugRB = "DEBUG_RB";

	public const string Action_DebugL3 = "DEBUG_L3";

	public const string Action_DebugR3 = "DEBUG_R3";

	public const string Window_Horizontal = "Window_Horizontal";

	public const string Window_Vertical = "Window_Vertical";

	public const string Window_Start = "Window_Start";

	public const string Window_Select = "Window_Select";

	public const string Window_Confirm = "Window_Confirm";

	public const string Window_Cancel = "Window_Cancel";

	public const string Window_X = "Window_X";

	public const string Window_Y = "Window_Y";

	public const string Window_LB = "Window_LB";

	public const string Window_RB = "Window_RB";

	public const string Window_LT = "Window_LT";

	public const string Window_RT = "Window_RT";

	public const string Window_Horizontal_RStick = "Window_Horizontal_RStick";

	public const string Window_Vertical_RStick = "Window_Vertical_RStick";

	public const string Window_UserReport = "Window_UserReport";

	public const string Window_R = "Window_R";

	public const string Window_AllMovement_LStick = "Window_AllMovement_LStick";

	public const string Window_AllMovement_RStick = "Window_AllMovement_RStick";

	private static InputActionType[] m_inputActionTypeArray;

	private static WindowInputActionType[] m_windowInputActionTypeArray;

	public static Player Player
	{
		get
		{
			if (!ReInput.isReady)
			{
				return null;
			}
			return ReInput.players.GetPlayer(0);
		}
	}

	public static bool IsGamepadConnected
	{
		get
		{
			if (!ReInput.isReady)
			{
				return false;
			}
			return Player.controllers.joystickCount > 0;
		}
	}

	public static InputActionType[] InputActionTypeArray
	{
		get
		{
			if (m_inputActionTypeArray == null)
			{
				m_inputActionTypeArray = Enum.GetValues(typeof(InputActionType)) as InputActionType[];
			}
			return m_inputActionTypeArray;
		}
	}

	public static WindowInputActionType[] WindowInputActionTypeArray
	{
		get
		{
			if (m_windowInputActionTypeArray == null)
			{
				m_windowInputActionTypeArray = Enum.GetValues(typeof(WindowInputActionType)) as WindowInputActionType[];
			}
			return m_windowInputActionTypeArray;
		}
	}

	public static int GetMapCategoryID(MapCategoryType categoryType)
	{
		return ReInput.mapping.GetMapCategoryId(GetMapCategoryName(categoryType));
	}

	public static InputActionType GetActionInputType(string actionInputName)
	{
		return actionInputName switch
		{
			"Jump" => InputActionType.Action_Jump, 
			"MoveHorizontal" => InputActionType.Action_MoveHorizontal, 
			"MoveVertical" => InputActionType.Action_MoveVertical, 
			"Attack" => InputActionType.Action_Attack, 
			"DashLeft" => InputActionType.Action_DashLeft, 
			"DashRight" => InputActionType.Action_DashRight, 
			"Spell" => InputActionType.Action_Spell, 
			"Talent" => InputActionType.Action_Talent, 
			"Interact" => InputActionType.Action_Interact, 
			"Start" => InputActionType.Action_Start, 
			"Select" => InputActionType.Action_Select, 
			"Pause" => InputActionType.Action_Pause, 
			"DEBUG_LB" => InputActionType.Action_DebugLB, 
			"DEBUG_RB" => InputActionType.Action_DebugRB, 
			"DEBUG_L3" => InputActionType.Action_DebugL3, 
			"DEBUG_R3" => InputActionType.Action_DebugR3, 
			"UserReport" => InputActionType.Action_UserReport, 
			"Downstrike" => InputActionType.Action_Downstrike, 
			"MoveHorizontalR" => InputActionType.Action_MoveHorizontalR, 
			"MoveVerticalR" => InputActionType.Action_MoveVerticalR, 
			"FreeLook" => InputActionType.Action_FreeLook, 
			"Flight" => InputActionType.Action_Flight, 
			_ => InputActionType.None, 
		};
	}

	public static WindowInputActionType GetWindowInputType(string windowInputName)
	{
		return windowInputName switch
		{
			"Window_Horizontal" => WindowInputActionType.Window_Horizontal, 
			"Window_Vertical" => WindowInputActionType.Window_Vertical, 
			"Window_Start" => WindowInputActionType.Window_Start, 
			"Window_Select" => WindowInputActionType.Window_Select, 
			"Window_Confirm" => WindowInputActionType.Window_Confirm, 
			"Window_Cancel" => WindowInputActionType.Window_Cancel, 
			"Window_X" => WindowInputActionType.Window_X, 
			"Window_Y" => WindowInputActionType.Window_Y, 
			"Window_R" => WindowInputActionType.Window_R, 
			"Window_LB" => WindowInputActionType.Window_LB, 
			"Window_RB" => WindowInputActionType.Window_RB, 
			"Window_LT" => WindowInputActionType.Window_LT, 
			"Window_RT" => WindowInputActionType.Window_RT, 
			"Window_Horizontal_RStick" => WindowInputActionType.Window_Horizontal_RStick, 
			"Window_Vertical_RStick" => WindowInputActionType.Window_Vertical_RStick, 
			"Window_AllMovement_LStick" => WindowInputActionType.Window_AllMovement_LStick, 
			"Window_AllMovement_RStick" => WindowInputActionType.Window_AllMovement_RStick, 
			"Window_UserReport" => WindowInputActionType.Window_UserReport, 
			_ => WindowInputActionType.None, 
		};
	}

	public static string GetString(InputActionType actionInputType)
	{
		return actionInputType switch
		{
			InputActionType.Action_Jump => "Jump", 
			InputActionType.Action_MoveHorizontal => "MoveHorizontal", 
			InputActionType.Action_MoveVertical => "MoveVertical", 
			InputActionType.Action_Attack => "Attack", 
			InputActionType.Action_DashLeft => "DashLeft", 
			InputActionType.Action_DashRight => "DashRight", 
			InputActionType.Action_Spell => "Spell", 
			InputActionType.Action_Talent => "Talent", 
			InputActionType.Action_Interact => "Interact", 
			InputActionType.Action_Start => "Start", 
			InputActionType.Action_Select => "Select", 
			InputActionType.Action_Pause => "Pause", 
			InputActionType.Action_DebugLB => "DEBUG_LB", 
			InputActionType.Action_DebugRB => "DEBUG_RB", 
			InputActionType.Action_DebugL3 => "DEBUG_L3", 
			InputActionType.Action_DebugR3 => "DEBUG_R3", 
			InputActionType.Action_UserReport => "UserReport", 
			InputActionType.Action_Downstrike => "Downstrike", 
			InputActionType.Action_MoveHorizontalR => "MoveHorizontalR", 
			InputActionType.Action_MoveVerticalR => "MoveVerticalR", 
			InputActionType.Action_FreeLook => "FreeLook", 
			InputActionType.Action_Flight => "Flight", 
			_ => "", 
		};
	}

	public static string GetString(WindowInputActionType windowInputType)
	{
		return windowInputType switch
		{
			WindowInputActionType.Window_Horizontal => "Window_Horizontal", 
			WindowInputActionType.Window_Vertical => "Window_Vertical", 
			WindowInputActionType.Window_Start => "Window_Start", 
			WindowInputActionType.Window_Select => "Window_Select", 
			WindowInputActionType.Window_Confirm => "Window_Confirm", 
			WindowInputActionType.Window_Cancel => "Window_Cancel", 
			WindowInputActionType.Window_X => "Window_X", 
			WindowInputActionType.Window_Y => "Window_Y", 
			WindowInputActionType.Window_R => "Window_R", 
			WindowInputActionType.Window_LB => "Window_LB", 
			WindowInputActionType.Window_RB => "Window_RB", 
			WindowInputActionType.Window_LT => "Window_LT", 
			WindowInputActionType.Window_RT => "Window_RT", 
			WindowInputActionType.Window_Horizontal_RStick => "Window_Horizontal_RStick", 
			WindowInputActionType.Window_Vertical_RStick => "Window_Vertical_RStick", 
			WindowInputActionType.Window_AllMovement_LStick => "Window_AllMovement_LStick", 
			WindowInputActionType.Window_AllMovement_RStick => "Window_AllMovement_RStick", 
			WindowInputActionType.Window_UserReport => "Window_UserReport", 
			_ => "", 
		};
	}

	public static string GetMapCategoryName(MapCategoryType categoryType)
	{
		return categoryType switch
		{
			MapCategoryType.Action => "Player", 
			MapCategoryType.ActionRemappable => "PlayerRemappable", 
			MapCategoryType.Window => "Window", 
			MapCategoryType.WindowRemappable => "WindowRemappable", 
			MapCategoryType.IllegalAssignments => "IllegalAssignments", 
			MapCategoryType.GamepadGlyphs => "GamepadGlyphs", 
			_ => null, 
		};
	}

	public static bool HasPoleAxes(string actionName)
	{
		switch (actionName)
		{
		case "MoveHorizontal":
		case "MoveHorizontalR":
		case "MoveVertical":
		case "MoveVerticalR":
		case "Window_Horizontal":
		case "Window_Horizontal_RStick":
		case "Window_Vertical":
		case "Window_Vertical_RStick":
			return true;
		default:
			return false;
		}
	}

	public static bool IgnoreNullAemOnKeyboard(string actionName)
	{
		switch (actionName)
		{
		case "DEBUG_L3":
		case "DEBUG_R3":
		case "DEBUG_RB":
		case "DEBUG_LB":
		case "MoveHorizontalR":
		case "MoveVerticalR":
		case "Window_Start":
		case "Window_Horizontal_RStick":
		case "Window_Vertical_RStick":
			return true;
		default:
			return false;
		}
	}

	public static bool IgnoreNullAemOnGamepad(string actionName)
	{
		switch (actionName)
		{
		case "UserReport":
		case "Window_UserReport":
			return true;
		default:
			return false;
		}
	}

	public static bool IsStandardJoystick(Controller controller)
	{
		return ReInput.players.GetPlayer(0).controllers.maps.GetFirstElementMapWithAction(controller, 4, skipDisabledMaps: false) != null;
	}

	public static bool DoesActionExistInMapCategory(string actionName, MapCategoryType mapCategory, ControllerType controllerType, Pole axis = Pole.Positive)
	{
		int mapCategoryID = GetMapCategoryID(mapCategory);
		m_reusableAemList.Clear();
		ReInput.players.GetPlayer(0).controllers.maps.GetElementMapsWithAction(controllerType, actionName, skipDisabledMaps: false, m_reusableAemList);
		foreach (ActionElementMap reusableAem in m_reusableAemList)
		{
			if (reusableAem.controllerMap.categoryId == mapCategoryID && reusableAem.axisContribution == axis)
			{
				return true;
			}
		}
		return false;
	}

	public static MapCategoryType GetMapCategoryTypeFlags(ControllerType controllerType, string actionName, Pole axis = Pole.Positive)
	{
		MapCategoryType mapCategoryType = MapCategoryType.ActionRemappable;
		MapCategoryType mapCategoryType2 = MapCategoryType.Action;
		if (GetWindowInputType(actionName) != 0)
		{
			mapCategoryType = MapCategoryType.WindowRemappable;
			mapCategoryType2 = MapCategoryType.Window;
		}
		bool num = DoesActionExistInMapCategory(actionName, mapCategoryType, controllerType, axis);
		bool flag = DoesActionExistInMapCategory(actionName, mapCategoryType2, controllerType, axis);
		MapCategoryType mapCategoryType3 = MapCategoryType.None;
		if (num)
		{
			mapCategoryType3 |= mapCategoryType;
		}
		if (flag)
		{
			mapCategoryType3 |= mapCategoryType2;
		}
		return mapCategoryType3;
	}

	public static MapCategoryType GetMapCategoryTypeFromFlag(MapCategoryType flag, bool prioritizeNonMappable)
	{
		MapCategoryType mapCategoryType = MapCategoryType.None;
		if ((MapCategoryType.ActionRemappable & flag) != 0)
		{
			mapCategoryType = MapCategoryType.ActionRemappable;
		}
		if (prioritizeNonMappable || (mapCategoryType == MapCategoryType.None && (MapCategoryType.Action & flag) != 0))
		{
			mapCategoryType = MapCategoryType.Action;
		}
		if (mapCategoryType == MapCategoryType.None)
		{
			if ((MapCategoryType.WindowRemappable & flag) != 0)
			{
				mapCategoryType = MapCategoryType.WindowRemappable;
			}
			if (prioritizeNonMappable || (mapCategoryType == MapCategoryType.None && (MapCategoryType.Window & flag) != 0))
			{
				mapCategoryType = MapCategoryType.Window;
			}
		}
		return mapCategoryType;
	}

	public static ActionElementMap GetActionElementMap(bool useGamepad, InputAction inputAction, Pole axis, bool prioritizeNonRemappable, int controllerID = 0)
	{
		ActionElementMap actionElementMap = null;
		Player player = ReInput.players.GetPlayer(0);
		m_controllerTypeHelperList.Clear();
		if (useGamepad)
		{
			m_controllerTypeHelperList.Add(ControllerType.Joystick);
		}
		else
		{
			m_controllerTypeHelperList.Add(ControllerType.Mouse);
			m_controllerTypeHelperList.Add(ControllerType.Keyboard);
		}
		foreach (ControllerType controllerTypeHelper in m_controllerTypeHelperList)
		{
			MapCategoryType mapCategoryTypeFlags = GetMapCategoryTypeFlags(controllerTypeHelper, inputAction.name, axis);
			if (mapCategoryTypeFlags == MapCategoryType.None)
			{
				continue;
			}
			int mapCategoryID = GetMapCategoryID(GetMapCategoryTypeFromFlag(mapCategoryTypeFlags, prioritizeNonRemappable));
			ControllerMap map = player.controllers.maps.GetMap(controllerTypeHelper, controllerID, mapCategoryID, 0);
			m_reusableAemList.Clear();
			map.GetElementMapsWithAction(inputAction.id, m_reusableAemList);
			if (m_reusableAemList.Count <= 0)
			{
				continue;
			}
			if (m_reusableAemList.Count > 1)
			{
				if (inputAction.type != 0)
				{
					continue;
				}
				int index = -1;
				for (int i = 0; i < m_reusableAemList.Count; i++)
				{
					if (m_reusableAemList[i].axisContribution == axis)
					{
						index = i;
					}
				}
				actionElementMap = m_reusableAemList[index];
				break;
			}
			actionElementMap = m_reusableAemList[m_reusableAemList.Count - 1];
			break;
		}
		bool flag = (!useGamepad && IgnoreNullAemOnKeyboard(inputAction.name)) || (useGamepad && IgnoreNullAemOnGamepad(inputAction.name));
		if (actionElementMap == null && !flag)
		{
			Debug.Log("ActionElementMap null. InputAction name: " + inputAction.name + ". Use Gamepad: " + useGamepad);
		}
		return actionElementMap;
	}

	public static ActionElementMap GetActionElementMap(bool useGamepad, string actionName, Pole axis, bool prioritizeNonRemappable, int controllerID)
	{
		InputAction action = ReInput.mapping.GetAction(actionName);
		if (action != null)
		{
			return GetActionElementMap(useGamepad, action, axis, prioritizeNonRemappable, controllerID);
		}
		Debug.Log("InputAction null on: " + actionName);
		return null;
	}

	public static void ResetControllerMapToDefault(bool resetGamepad)
	{
		ReInput.players.GetPlayer(0);
		if (!resetGamepad)
		{
			ResetControllerMap(ControllerType.Mouse);
			ResetControllerMap(ControllerType.Keyboard);
		}
		else
		{
			ResetControllerMap(ControllerType.Joystick);
		}
	}

	private static void ResetControllerMap(ControllerType controllerType)
	{
		ReInput.players.GetPlayer(0).controllers.maps.LoadDefaultMaps(controllerType);
	}
}
