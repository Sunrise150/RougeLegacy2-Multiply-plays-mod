using System.Collections;
using RL_Windows;
using Rewired;
using UnityEngine;

public class CameraDebugInputHandler : MonoBehaviour
{
	private bool m_jumpPressed;

	private bool m_spellPressed;

	public bool JumpPressed
	{
		get
		{
			return m_jumpPressed;
		}
		private set
		{
			m_jumpPressed = value;
		}
	}

	public bool SpellPressed
	{
		get
		{
			return m_spellPressed;
		}
		private set
		{
			m_spellPressed = value;
		}
	}

	private IEnumerator Start()
	{
		while (Rewired_RL.Player == null)
		{
			yield return null;
		}
		Rewired_RL.Player.AddInputEventDelegate(OnJumpPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Jump");
		Rewired_RL.Player.AddInputEventDelegate(OnJumpPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Jump");
		Rewired_RL.Player.AddInputEventDelegate(OnSpellPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Spell");
		Rewired_RL.Player.AddInputEventDelegate(OnSpellPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Spell");
	}

	private void OnJumpPressed(InputActionEventData obj)
	{
		if (!WindowManager.GetIsWindowOpen(WindowID.CameraDebug))
		{
			if (obj.eventType == InputActionEventType.ButtonJustPressed)
			{
				JumpPressed = true;
			}
			else
			{
				JumpPressed = false;
			}
			if (JumpPressed && m_spellPressed)
			{
				WindowManager.SetWindowIsOpen(WindowID.CameraDebug, isOpen: true);
				JumpPressed = false;
				SpellPressed = false;
			}
			else
			{
				WindowManager.SetWindowIsOpen(WindowID.CameraDebug, isOpen: false);
			}
		}
	}

	private void OnSpellPressed(InputActionEventData obj)
	{
		if (!WindowManager.GetIsWindowOpen(WindowID.CameraDebug))
		{
			if (obj.eventType == InputActionEventType.ButtonJustPressed)
			{
				SpellPressed = true;
			}
			else
			{
				SpellPressed = false;
			}
			if (JumpPressed && SpellPressed)
			{
				WindowManager.SetWindowIsOpen(WindowID.CameraDebug, isOpen: true);
				JumpPressed = false;
				SpellPressed = false;
			}
			else
			{
				WindowManager.SetWindowIsOpen(WindowID.CameraDebug, isOpen: false);
			}
		}
	}
}
