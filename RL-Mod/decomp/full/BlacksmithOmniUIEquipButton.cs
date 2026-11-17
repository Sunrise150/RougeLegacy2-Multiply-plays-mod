using System;
using RL_Windows;
using UnityEngine;
using UnityEngine.UI;

public class BlacksmithOmniUIEquipButton : OmniUIButton, IBlacksmithOmniUIButton
{
	public const float NOTCH_SHIFT_AMOUNT = 45f;

	[SerializeField]
	private Image m_sliderOnSprite;

	[SerializeField]
	private Image m_sliderOffSprite;

	[SerializeField]
	private GameObject m_notchGameObj;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private BlacksmithOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public EquipmentCategoryType CategoryType { get; set; }

	public EquipmentType EquipmentType { get; set; }

	protected override void InitializeButtonEventArgs()
	{
		OmniUIButtonType buttonType = OmniUIButtonType.Equipping;
		if (EquipmentManager.IsEquipped(CategoryType, EquipmentType))
		{
			buttonType = OmniUIButtonType.Unequipping;
		}
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new BlacksmithOmniUIDescriptionEventArgs(CategoryType, EquipmentType, buttonType);
		}
		else
		{
			m_descriptionEventArgs.Initialize(CategoryType, EquipmentType, buttonType);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		base.OnConfirmButtonPressed();
		if (!EquipmentManager.IsEquipped(CategoryType, EquipmentType))
		{
			if (EquipmentManager.CanEquip(CategoryType, EquipmentType))
			{
				if (SetEquipped(CategoryType, EquipmentType))
				{
					InitializeButtonEventArgs();
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
					Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
					RunOnConfirmPressedAnimation();
				}
			}
			else
			{
				StartCoroutine(ShakeAnimCoroutine());
			}
		}
		else if (SetEquipped(CategoryType, EquipmentType.None))
		{
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
			RunOnConfirmPressedAnimation();
		}
	}

	public static bool SetEquipped(EquipmentCategoryType category, EquipmentType equipType)
	{
		EquipmentObj equipped = EquipmentManager.GetEquipped(category);
		int num = ((equipped != null) ? EquipmentManager.Get_EquipmentSet_CurrentUnityTier(equipped.EquipmentType) : 0);
		int num2 = EquipmentManager.Get_EquipmentSet_CurrentUnityTier(equipType);
		bool result = EquipmentManager.SetEquipped(category, equipType);
		EquipmentObj equipped2 = EquipmentManager.GetEquipped(category);
		int num3 = EquipmentManager.Get_EquipmentSet_CurrentUnityTier(equipType);
		int num4 = ((equipped != null) ? EquipmentManager.Get_EquipmentSet_CurrentUnityTier(equipped.EquipmentType) : 0);
		if (equipped != null && num4 < num)
		{
			Vector3 unityDownTextPosition = (WindowManager.GetWindowController(WindowID.Blacksmith) as BlacksmithOmniUIWindowController).UnityDownTextPosition;
			string @string = LocalizationManager.GetString("LOC_ID_BLACKSMITH_UNITY_DOWN_1", isFemale: false);
			bool isFemale = false;
			@string = LocalizationManager.GetFormatterGenderForcedString(@string, out isFemale);
			string string2 = LocalizationManager.GetString(equipped.EquipmentData.Title, isFemale);
			if (string.IsNullOrEmpty(string2) || string2.Contains("LOC_ID"))
			{
				string2 = LocalizationManager.GetString(Equipment_EV.GetEquipmentTypeNameLocID(equipped.EquipmentType), isFemale, disableWordScramble: true);
			}
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.UnityLevelDown, string.Format(@string, string2), unityDownTextPosition);
			Debug.Log("Unity level has lowered for equipment type: " + equipped.EquipmentType);
			return result;
		}
		if (equipped2 != null && num3 > num2)
		{
			Vector3 unityDownTextPosition2 = (WindowManager.GetWindowController(WindowID.Blacksmith) as BlacksmithOmniUIWindowController).UnityDownTextPosition;
			string string3 = LocalizationManager.GetString("LOC_ID_BLACKSMITH_UNITY_UP_1", isFemale: false);
			bool isFemale2 = false;
			string3 = LocalizationManager.GetFormatterGenderForcedString(string3, out isFemale2);
			string string4 = LocalizationManager.GetString(equipped2.EquipmentData.Title, isFemale2);
			if (string.IsNullOrEmpty(string4) || string4.Contains("LOC_ID"))
			{
				string4 = LocalizationManager.GetString(Equipment_EV.GetEquipmentTypeNameLocID(equipped2.EquipmentType), isFemale2, disableWordScramble: true);
			}
			TextPopupManager.DisplayTextAtAbsPos(TextPopupType.UnityLevelDown, string.Format(string3, string4), unityDownTextPosition2);
			Debug.Log("Unity level has raised for equipment type: " + equipped2.EquipmentType);
		}
		return result;
	}

	public override void UpdateState()
	{
		if (EquipmentManager.GetFoundState(CategoryType, EquipmentType) != 0)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			m_canvasGroup.alpha = 1f;
			IsButtonActive = true;
		}
		EquipmentObj equipped = EquipmentManager.GetEquipped(CategoryType);
		if (equipped != null && equipped.EquipmentType == EquipmentType)
		{
			SetIconEquippedState(equipped: true);
		}
		else
		{
			SetIconEquippedState(equipped: false);
		}
	}

	private void SetIconEquippedState(bool equipped)
	{
		if (equipped)
		{
			if (!m_sliderOnSprite.gameObject.activeSelf)
			{
				m_sliderOnSprite.gameObject.SetActive(value: true);
			}
			if (m_sliderOffSprite.gameObject.activeSelf)
			{
				m_sliderOffSprite.gameObject.SetActive(value: false);
			}
			Vector3 localPosition = m_notchGameObj.transform.localPosition;
			localPosition.x = 45f;
			m_notchGameObj.transform.localPosition = localPosition;
		}
		else
		{
			if (m_sliderOnSprite.gameObject.activeSelf)
			{
				m_sliderOnSprite.gameObject.SetActive(value: false);
			}
			if (!m_sliderOffSprite.gameObject.activeInHierarchy)
			{
				m_sliderOffSprite.gameObject.SetActive(value: true);
			}
			Vector3 localPosition2 = m_notchGameObj.transform.localPosition;
			localPosition2.x = 0f;
			m_notchGameObj.transform.localPosition = localPosition2;
		}
	}
}
