using System;
using RL_Windows;
using UnityEngine;

public class BlackTunnel : Tunnel
{
	private Action m_cancelConfirmMenu;

	private Action m_confirmEnterTunnel;

	protected override void Awake()
	{
		base.Awake();
		m_cancelConfirmMenu = CancelConfirmMenu;
		m_confirmEnterTunnel = ConfirmEnterTunnel;
	}

	protected override void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		InitializeConfirmMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void InitializeConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_BLACK_DOOR_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_BLACK_DOOR_DESCRIPTION_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenu);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmEnterTunnel);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenu);
	}

	private void ConfirmEnterTunnel()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		base.OnPlayerInteractedWithTunnel(null);
	}

	private void CancelConfirmMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
