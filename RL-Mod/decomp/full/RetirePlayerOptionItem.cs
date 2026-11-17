using System;
using RL_Windows;
using SceneManagement_RL;

public class RetirePlayerOptionItem : ExecuteImmediateOptionItem
{
	private Action m_cancelConfirmMenuSelection;

	private Action m_confirmQuitChallenge;

	private Action m_confirmRetirePlayer;

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_confirmQuitChallenge = ConfirmQuitChallenge;
		m_confirmRetirePlayer = ConfirmRetirePlayer;
	}

	public override void ActivateOption()
	{
		if (ChallengeManager.IsInChallenge)
		{
			InitializeConfirmQuitChallengeMenu();
		}
		else
		{
			InitializeConfirmRetireMenu();
		}
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void InitializeConfirmQuitChallengeMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_CONFIRM_MENU_QUIT_SCAR_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_CONFIRM_MENU_QUIT_SCAR_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmQuitChallenge);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void InitializeConfirmRetireMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_CONFIRM_MENU_RETIRE_HERO_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_CONFIRM_MENU_RETIRE_HERO_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmRetirePlayer);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void ConfirmQuitChallenge()
	{
		WindowManager.CloseAllOpenWindows();
		RLTimeScale.SetAllTimeScale(0f);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		SceneLoader_RL.RunTransitionWithLogic(ChallengeExitTransition, TransitionID.FadeToBlackNoLoading, cleanup: false);
	}

	private void ChallengeExitTransition()
	{
		ChallengeManager.ChallengeTunnelController.ReturnToDriftHouse();
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		RLTimeScale.Reset();
	}

	private void ConfirmRetirePlayer()
	{
		SaveManager.PlayerSaveData.CurrentCharacter.IsRetired = true;
		WindowManager.CloseAllOpenWindows();
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.ResetRendererArrayColor();
		playerController.BlinkPulseEffect.ResetAllBlackFills();
		playerController.KillCharacter(null, broadcastEvent: true);
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
