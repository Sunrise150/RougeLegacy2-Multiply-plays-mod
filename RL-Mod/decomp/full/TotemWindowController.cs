using System;
using System.Collections;
using RL_Windows;
using Rewired;
using UnityEngine;

public class TotemWindowController : WindowController
{
	[SerializeField]
	private CanvasGroup m_pageCanvasGroup;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	private Action<InputActionEventData> m_onConfirmPressed;

	public override WindowID ID => WindowID.Totem;

	private void Awake()
	{
		m_onConfirmPressed = OnConfirmPressed;
	}

	protected override void OnOpen()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
		StartCoroutine(OnOpenCoroutine());
	}

	private IEnumerator OnOpenCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_pageCanvasGroup.alpha = 0f;
		m_bgCanvasGroup.alpha = 0f;
		RectTransform component = m_pageCanvasGroup.GetComponent<RectTransform>();
		Vector2 anchoredPosition = component.anchoredPosition;
		anchoredPosition.y += 100f;
		component.anchoredPosition = anchoredPosition;
		TweenManager.TweenTo_UnscaledTime(m_pageCanvasGroup, 0.15f, Ease.None, "alpha", 1);
		TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, 0.15f, Ease.None, "alpha", 1);
		yield return TweenManager.TweenBy_UnscaledTime(component, 0.15f, Ease.Quad.EaseOut, "anchoredPosition.y", -100).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
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
		WindowManager.SetWindowIsOpen(WindowID.Totem, isOpen: false);
	}
}
