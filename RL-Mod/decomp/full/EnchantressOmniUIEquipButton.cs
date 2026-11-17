using System;
using TMPro;
using UnityEngine;

public class EnchantressOmniUIEquipButton : OmniUIButton, IEnchantressOmniUIButton
{
	[SerializeField]
	private bool m_isRightArrow;

	[SerializeField]
	private GameObject m_buttonGO;

	[SerializeField]
	private TMP_Text m_levelText;

	private EnchantressOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public RuneType RuneType { get; set; }

	protected override void InitializeButtonEventArgs()
	{
		OmniUIButtonType buttonType = (m_isRightArrow ? OmniUIButtonType.Equipping : OmniUIButtonType.Unequipping);
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new EnchantressOmniUIDescriptionEventArgs(RuneType, buttonType);
		}
		else
		{
			m_descriptionEventArgs.Initialize(RuneType, buttonType);
		}
	}

	public override void UpdateState()
	{
		RuneManager.GetRune(RuneType);
		int runeEquippedLevel = RuneManager.GetRuneEquippedLevel(RuneType);
		int upgradeLevel = RuneManager.GetUpgradeLevel(RuneType);
		if (upgradeLevel > 0)
		{
			m_deselectedSprite.SetAlpha(1f);
			IsButtonActive = true;
		}
		else
		{
			m_deselectedSprite.SetAlpha(0.25f);
			IsButtonActive = false;
		}
		m_levelText.text = runeEquippedLevel + "/" + upgradeLevel;
	}

	public override void OnConfirmButtonPressed()
	{
		if (m_isRightArrow)
		{
			if (RuneManager.CanEquip(RuneType))
			{
				if (RuneManager.SetRuneEquippedLevel(RuneType, 1, additive: true))
				{
					InitializeButtonEventArgs();
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
				}
			}
			else
			{
				StartCoroutine(ShakeAnimCoroutine());
			}
		}
		else if (RuneManager.SetRuneEquippedLevel(RuneType, -1, additive: true))
		{
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		}
		RunOnConfirmPressedAnimation();
		base.OnConfirmButtonPressed();
	}
}
