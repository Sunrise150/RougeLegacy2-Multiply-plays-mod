using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class GamepadImageSelector : MonoBehaviour
{
	private static List<ActionElementMap> m_aemListHelper = new List<ActionElementMap>();

	private const string DpadUp_Name = "DpadUp";

	private const string DpadDown_Name = "DpadDown";

	private const string DpadLeft_Name = "DpadLeft";

	private const string DpadRight_Name = "DpadRight";

	private const string L3_Name = "L3";

	private const string R3_Name = "R3";

	private const string LB_Name = "Left Button";

	private const string LT_Name = "Left Trigger";

	private const string RB_Name = "Right Button";

	private const string RT_Name = "Right Trigger";

	private const string X_Name = "X";

	private const string A_Name = "A";

	private const string B_Name = "B";

	private const string Y_Name = "Y";

	private const string TouchPad_Name = "TouchPad";

	[SerializeField]
	private GamepadType m_gamepadType;

	[SerializeField]
	private Image m_rbButtonImage;

	[SerializeField]
	private Image m_lbButtonImage;

	[SerializeField]
	private Image m_rtButtonImage;

	[SerializeField]
	private Image m_ltButtonImage;

	[SerializeField]
	private Image m_l3ButtonImage;

	[SerializeField]
	private Image m_r3ButtonImage;

	[SerializeField]
	private Image m_xButtonImage;

	[SerializeField]
	private Image m_bButtonImage;

	[SerializeField]
	private Image m_yButtonImage;

	[SerializeField]
	private Image m_aButtonImage;

	[SerializeField]
	private Image m_dpadButtonImage;

	[SerializeField]
	private Image m_touchPadImage;

	[SerializeField]
	private bool m_isXboxSeries;

	public GamepadType GamepadType => m_gamepadType;

	public int ControllerID { get; set; }

	public bool IsXboxSeries => m_isXboxSeries;

	private void Awake()
	{
		DeactivateAllImages();
	}

	public void EnableGamepadImage(string actionName, Pole axis, bool disableAllOthers)
	{
		ActionElementMap actionElementMap = Rewired_RL.GetActionElementMap(useGamepad: true, actionName, axis, prioritizeNonRemappable: false, ControllerID);
		ControllerMap map = Rewired_RL.Player.controllers.maps.GetMap(ControllerType.Joystick, ControllerID, Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.GamepadGlyphs), 0);
		if (actionElementMap != null && map.ContainsElementIdentifier(actionElementMap.elementIdentifierId))
		{
			m_aemListHelper.Clear();
			map.GetElementMaps(skipDisabledMaps: false, m_aemListHelper);
			{
				foreach (ActionElementMap item in m_aemListHelper)
				{
					if (item.elementIdentifierId == actionElementMap.elementIdentifierId)
					{
						if (disableAllOthers)
						{
							DeactivateAllImages();
						}
						string actionName2 = ReInput.mapping.GetAction(item.actionId).name;
						Image imageFromActionName = GetImageFromActionName(actionName2);
						if ((bool)imageFromActionName)
						{
							imageFromActionName.gameObject.SetActive(value: true);
						}
						break;
					}
				}
				return;
			}
		}
		if (disableAllOthers)
		{
			DeactivateAllImages();
		}
	}

	private Image GetImageFromActionName(string actionName)
	{
		return actionName switch
		{
			"Left Button" => m_lbButtonImage, 
			"Right Button" => m_rbButtonImage, 
			"Left Trigger" => m_ltButtonImage, 
			"Right Trigger" => m_rtButtonImage, 
			"L3" => m_l3ButtonImage, 
			"R3" => m_r3ButtonImage, 
			"X" => m_xButtonImage, 
			"A" => m_aButtonImage, 
			"B" => m_bButtonImage, 
			"Y" => m_yButtonImage, 
			"DpadLeft" => m_dpadButtonImage, 
			"DpadRight" => m_dpadButtonImage, 
			"DpadUp" => m_dpadButtonImage, 
			"DpadDown" => m_dpadButtonImage, 
			"TouchPad" => m_touchPadImage, 
			_ => null, 
		};
	}

	private void DeactivateAllImages()
	{
		if (m_rbButtonImage.gameObject.activeSelf)
		{
			m_rbButtonImage.gameObject.SetActive(value: false);
		}
		if (m_lbButtonImage.gameObject.activeSelf)
		{
			m_lbButtonImage.gameObject.SetActive(value: false);
		}
		if (m_rtButtonImage.gameObject.activeSelf)
		{
			m_rtButtonImage.gameObject.SetActive(value: false);
		}
		if (m_ltButtonImage.gameObject.activeSelf)
		{
			m_ltButtonImage.gameObject.SetActive(value: false);
		}
		if (m_l3ButtonImage.gameObject.activeSelf)
		{
			m_l3ButtonImage.gameObject.SetActive(value: false);
		}
		if (m_r3ButtonImage.gameObject.activeSelf)
		{
			m_r3ButtonImage.gameObject.SetActive(value: false);
		}
		if (m_xButtonImage.gameObject.activeSelf)
		{
			m_xButtonImage.gameObject.SetActive(value: false);
		}
		if (m_bButtonImage.gameObject.activeSelf)
		{
			m_bButtonImage.gameObject.SetActive(value: false);
		}
		if (m_yButtonImage.gameObject.activeSelf)
		{
			m_yButtonImage.gameObject.SetActive(value: false);
		}
		if (m_aButtonImage.gameObject.activeSelf)
		{
			m_aButtonImage.gameObject.SetActive(value: false);
		}
		if (m_dpadButtonImage.gameObject.activeSelf)
		{
			m_dpadButtonImage.gameObject.SetActive(value: false);
		}
		if ((bool)m_touchPadImage && m_touchPadImage.gameObject.activeSelf)
		{
			m_touchPadImage.gameObject.SetActive(value: false);
		}
	}
}
