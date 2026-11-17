using System;
using RL_Windows;
using TMPro;
using UnityEngine;

public class NewGamePlusOmniUIEnterButton : OmniUIButton, INewGamePlusOmniUIButton
{
	[SerializeField]
	private TMP_Text m_enterText;

	private Action m_cancelConfirmMenuSelection;

	private Action m_confirmEnterNewGamePlus;

	private NewGamePlusOmniUIDescriptionEventArgs m_descriptionEventArgs;

	private PurchaseBoxDialogueEventArgs m_purchaseDialogueArgs = new PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType.Welcome);

	public int CurrentNewGamePlusLevel => NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public BurdenType BurdenType { get; set; }

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_confirmEnterNewGamePlus = ConfirmEnterNewGamePlus;
	}

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new NewGamePlusOmniUIDescriptionEventArgs(BurdenType, OmniUIButtonType.Purchasing);
		}
		else
		{
			m_descriptionEventArgs.Initialize(BurdenType, OmniUIButtonType.Purchasing);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (IsButtonActive)
		{
			InitializeConfirmEnterNGPlus();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: true);
			base.OnConfirmButtonPressed();
			RunOnConfirmPressedAnimation();
		}
	}

	public override void UpdateState()
	{
		if (BurdenType != 0)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
			IsButtonActive = false;
			return;
		}
		IsButtonActive = false;
		m_deselectedSprite.SetAlpha(1f);
		if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
		if (BurdenManager.CanEnterNewGamePlus(CurrentNewGamePlusLevel))
		{
			IsButtonActive = true;
			m_deselectedSprite.SetAlpha(1f);
		}
		else
		{
			IsButtonActive = false;
			m_deselectedSprite.SetAlpha(0.25f);
		}
	}

	private void InitializeConfirmEnterNGPlus()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenuBig))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenuBig);
		}
		ConfirmMenuWindowController confirmMenuWindowController = WindowManager.GetWindowController(WindowID.ConfirmMenuBig) as ConfirmMenuWindowController;
		if (NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel == SaveManager.PlayerSaveData.NewGamePlusLevel)
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_NG_UI_RESET_THREAD_TITLE_1", isLocID: true);
			confirmMenuWindowController.SetDescriptionText("LOC_ID_NG_UI_RESET_THREAD_TEXT_CONFIRM_1", isLocID: true);
		}
		else if (NewGamePlusOmniUIWindowController.CurrentNewGamePlusLevel < SaveManager.PlayerSaveData.NewGamePlusLevel)
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_NG_UI_LOWER_THREAD_TITLE_1", isLocID: true);
			confirmMenuWindowController.SetDescriptionText("LOC_ID_NG_UI_LOWER_THREAD_TEXT_CONFIRM_1", isLocID: true);
		}
		else
		{
			confirmMenuWindowController.SetTitleText("LOC_ID_NG_UI_HIGHER_THREAD_TITLE_1", isLocID: true);
			confirmMenuWindowController.SetDescriptionText("LOC_ID_NG_UI_HIGHER_THREAD_TEXT_CONFIRM_1", isLocID: true);
		}
		confirmMenuWindowController.SetNumberOfButtons(2);
		confirmMenuWindowController.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = confirmMenuWindowController.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmEnterNewGamePlus);
		ConfirmMenu_Button buttonAtIndex2 = confirmMenuWindowController.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void ConfirmEnterNewGamePlus()
	{
		NewGamePlusOmniUIWindowController.EnteringNGPlus = true;
		SaveManager.PlayerSaveData.NewGamePlusLevel = CurrentNewGamePlusLevel;
		WindowManager.CloseAllOpenWindows();
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
	}
}
