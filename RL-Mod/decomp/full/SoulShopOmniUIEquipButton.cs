using System;
using TMPro;
using UnityEngine;

public class SoulShopOmniUIEquipButton : OmniUIButton, ISoulShopOmniUIButton
{
	[SerializeField]
	private bool m_isRightArrow;

	[SerializeField]
	private GameObject m_buttonGO;

	[SerializeField]
	private TMP_Text m_levelText;

	private SoulShopOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public SoulShopType SoulShopType { get; set; }

	public SoulShopOmniUIEntry ParentEntry { get; set; }

	protected override void InitializeButtonEventArgs()
	{
		OmniUIButtonType buttonType = (m_isRightArrow ? OmniUIButtonType.Equipping : OmniUIButtonType.Unequipping);
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new SoulShopOmniUIDescriptionEventArgs(SoulShopType, buttonType);
		}
		else
		{
			m_descriptionEventArgs.Initialize(SoulShopType, buttonType);
		}
	}

	public override void UpdateState()
	{
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
		if (!soulShopObj.IsNativeNull())
		{
			int currentEquippedLevel = soulShopObj.CurrentEquippedLevel;
			int currentOwnedLevel = soulShopObj.CurrentOwnedLevel;
			IsButtonActive = true;
			m_levelText.text = currentEquippedLevel + "/" + currentOwnedLevel;
		}
	}

	public override void OnConfirmButtonPressed()
	{
		SoulShopObj soulShopObj = SaveManager.ModeSaveData.GetSoulShopObj(SoulShopType);
		if (soulShopObj.IsNativeNull())
		{
			return;
		}
		if (m_isRightArrow)
		{
			if (soulShopObj.CurrentEquippedLevel < soulShopObj.CurrentOwnedLevel)
			{
				soulShopObj.SetEquippedLevel(1, additive: true);
				InitializeButtonEventArgs();
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
			else
			{
				StartCoroutine(ShakeAnimCoroutine());
			}
		}
		else if (soulShopObj.CurrentEquippedLevel > 0)
		{
			soulShopObj.SetEquippedLevel(-1, additive: true);
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		}
		else
		{
			StartCoroutine(ShakeAnimCoroutine());
		}
		RunOnConfirmPressedAnimation();
		base.OnConfirmButtonPressed();
	}
}
