using System;
using RL_Windows;
using SceneManagement_RL;

public class RestartChallengeOptionItem : ExecuteImmediateOptionItem
{
	private Action m_cancelConfirmMenuSelection;

	private Action m_confirmRestartChallenge;

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_confirmRestartChallenge = ConfirmRestartChallenge;
	}

	public override void ActivateOption()
	{
		InitializeConfirmRestartChallengeMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void InitializeConfirmRestartChallengeMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_CONFIRM_MENU_RESTART_SCAR_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_CONFIRM_MENU_RESTART_SCAR_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmRestartChallenge);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void ConfirmRestartChallenge()
	{
		WindowManager.CloseAllOpenWindows();
		RLTimeScale.SetAllTimeScale(0f);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		SceneLoader_RL.RunTransitionWithLogic(ChallengeRestartTransition, TransitionID.FadeToBlackNoLoading, cleanup: false);
	}

	private void ChallengeRestartTransition()
	{
		ChallengeType challengeType = ChallengeManager.ActiveChallenge.ChallengeType;
		ChallengeManager.ChallengeTunnelController.ReturnToDriftHouse(broadcastExitEvent: false);
		ChallengeManager.ChallengeTunnelController.EnterChallenge(challengeType, animateTransition: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		RLTimeScale.Reset();
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
