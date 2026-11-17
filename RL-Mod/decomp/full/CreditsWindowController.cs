using System;
using RL_Windows;
using Rewired;

public class CreditsWindowController : WindowController
{
	private Action<InputActionEventData> m_onConfirmPressed;

	public override WindowID ID => WindowID.Credits;

	private void Awake()
	{
		m_onConfirmPressed = OnConfirmPressed;
	}

	protected override void OnOpen()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	protected override void OnFocus()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	protected override void OnLostFocus()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	private void OnConfirmPressed(InputActionEventData obj)
	{
		WindowManager.SetWindowIsOpen(WindowID.Credits, isOpen: false);
	}
}
