using System;
using RL_Windows;
using TMPro;
using UnityEngine;

public class ChallengeOmniUIEnterButton : OmniUIButton, IChallengeOmniUIButton
{
	[SerializeField]
	private TMP_Text m_enterText;

	private ChallengeOmniUIDescriptionEventArgs m_descriptionEventArgs;

	private PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public ChallengeType ChallengeType { get; set; }

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new ChallengeOmniUIDescriptionEventArgs(ChallengeType, OmniUIButtonType.Purchasing);
		}
		else
		{
			m_descriptionEventArgs.Initialize(ChallengeType, OmniUIButtonType.Purchasing);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (IsButtonActive)
		{
			base.OnConfirmButtonPressed();
			RunOnConfirmPressedAnimation();
			EnterChallenge();
		}
	}

	public override void UpdateState()
	{
		m_deselectedSprite.SetAlpha(1f);
		if (ChallengeManager.GetFoundState(ChallengeType) > FoundState.NotFound)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			IsButtonActive = true;
		}
		else
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			IsButtonActive = false;
		}
	}

	private void EnterChallenge()
	{
		WindowManager.SetWindowIsOpen(WindowID.ChallengeNPC, isOpen: false);
		InitializeButtonEventArgs();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.ChallengeNPC_EnterChallenge, this, ButtonEventArgs);
	}
}
