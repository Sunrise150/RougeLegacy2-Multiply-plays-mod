using System;
using RL_Windows;
using UnityEngine;

public class ArenaTunnel : Tunnel
{
	private Action m_cancelConfirmMenuSelection;

	private Action m_confirmEnterArena;

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_confirmEnterArena = ConfirmEnterArena;
	}

	protected override void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		if (SaveManager.PlayerSaveData.CurrentCharacter.TraitOne == TraitType.CantAttack || SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo == TraitType.CantAttack)
		{
			InitializeWarningConfirmMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
		else
		{
			base.OnPlayerInteractedWithTunnel(otherObj);
		}
	}

	private void InitializeWarningConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_ARENA_MENU_PACIFIST_WARNING_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_ARENA_MENU_PACIFIST_WARNING_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_ARENA_MENU_PACIFIST_BUTTON_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmEnterArena);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_ARENA_MENU_PACIFIST_BUTTON_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void ConfirmEnterArena()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		base.OnPlayerInteractedWithTunnel(null);
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
