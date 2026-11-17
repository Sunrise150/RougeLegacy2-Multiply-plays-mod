using System;
using System.Collections;
using RL_Windows;
using Rewired;
using Rewired.Integration.UnityUI;
using UnityEngine;

public class UserReportWindowController : WindowController
{
	[SerializeField]
	private UserReportingScript_TMP m_userReportScript;

	[SerializeField]
	private CanvasGroup m_userReportCanvasGroup;

	private RewiredStandaloneInputModule m_standaloneInputModule;

	private bool m_isLoading;

	private Action<InputActionEventData> m_onCancelInputHandler;

	public override int SortOrderOverride => 1000;

	public override WindowID ID => WindowID.UserReport;

	private void Awake()
	{
		m_onCancelInputHandler = OnCancelInputHandler;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_standaloneInputModule = UnityEngine.Object.FindObjectOfType<RewiredStandaloneInputModule>();
		m_windowCanvas.planeDistance = 99f;
	}

	protected override void OnOpen()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_windowCanvas.gameObject.SetActive(value: true);
		StartCoroutine(OnOpenCoroutine());
	}

	private IEnumerator OnOpenCoroutine()
	{
		m_userReportCanvasGroup.alpha = 0f;
		m_isLoading = true;
		while (!m_userReportScript.IsInitialized)
		{
			yield return null;
		}
		m_userReportScript.CreateUserReport();
		while (m_userReportScript.State != UserReportingState.ShowingForm)
		{
			yield return null;
		}
		Vector3 localPosition = m_userReportCanvasGroup.gameObject.transform.localPosition;
		localPosition.y += 50f;
		m_userReportCanvasGroup.gameObject.transform.localPosition = localPosition;
		float duration = 0.1f;
		TweenManager.TweenBy_UnscaledTime(m_userReportCanvasGroup.transform, duration, Ease.Quad.EaseOut, "localPosition.y", -50);
		yield return TweenManager.TweenTo_UnscaledTime(m_userReportCanvasGroup, duration, Ease.None, "alpha", 1).TweenCoroutine;
		m_isLoading = false;
	}

	protected override void OnClose()
	{
		m_userReportScript.CancelUserReport();
		m_windowCanvas.gameObject.SetActive(value: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnFocus()
	{
		AddInputListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	private void OnCancelInputHandler(InputActionEventData eventData)
	{
		CancelUserReport();
	}

	public void CompleteUserReport()
	{
		WindowManager.SetWindowIsOpen(WindowID.UserReport, isOpen: false);
	}

	public void CancelUserReport()
	{
		if (m_userReportScript.State != UserReportingState.CreatingUserReport && m_userReportScript.State != UserReportingState.SubmittingForm)
		{
			WindowManager.SetWindowIsOpen(WindowID.UserReport, isOpen: false);
		}
	}

	private void AddInputListeners()
	{
		base.RewiredPlayer.AddInputEventDelegate(m_onCancelInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		m_standaloneInputModule.submitButton = null;
	}

	private void RemoveInputListeners()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		m_standaloneInputModule.submitButton = "Window_Confirm";
	}
}
