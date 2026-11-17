using System;
using TMPro;
using UnityEngine;

public class ChallengeOmniUIEquipButton : OmniUIButton, IChallengeOmniUIButton
{
	[SerializeField]
	private bool m_isRightArrow;

	[SerializeField]
	private GameObject m_buttonGO;

	[SerializeField]
	private TMP_Text m_levelText;

	private ChallengeOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public ChallengeType ChallengeType { get; set; }

	protected override void InitializeButtonEventArgs()
	{
		OmniUIButtonType buttonType = (m_isRightArrow ? OmniUIButtonType.Equipping : OmniUIButtonType.Unequipping);
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new ChallengeOmniUIDescriptionEventArgs(ChallengeType, buttonType);
		}
		else
		{
			m_descriptionEventArgs.Initialize(ChallengeType, buttonType);
		}
	}

	public override void UpdateState()
	{
		ChallengeManager.GetChallenge(ChallengeType);
		int challengeEquippedLevel = ChallengeManager.GetChallengeEquippedLevel(ChallengeType);
		int upgradeBlueprintsFound = ChallengeManager.GetUpgradeBlueprintsFound(ChallengeType);
		if (upgradeBlueprintsFound > 0)
		{
			m_deselectedSprite.SetAlpha(1f);
			IsButtonActive = true;
			m_levelText.text = challengeEquippedLevel + "/" + upgradeBlueprintsFound;
		}
		else
		{
			m_deselectedSprite.SetAlpha(0f);
			IsButtonActive = false;
			m_levelText.text = "";
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (m_isRightArrow)
		{
			if (ChallengeManager.CanEquip(ChallengeType))
			{
				if (ChallengeManager.SetChallengeEquippedLevel(ChallengeType, 1, additive: true))
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
		else if (ChallengeManager.SetChallengeEquippedLevel(ChallengeType, -1, additive: true))
		{
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		}
		RunOnConfirmPressedAnimation();
		base.OnConfirmButtonPressed();
	}
}
