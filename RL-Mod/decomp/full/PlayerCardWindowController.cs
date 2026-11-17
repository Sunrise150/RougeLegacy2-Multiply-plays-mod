using System;
using System.Collections;
using RL_Windows;
using Rewired;
using UnityEngine;

public class PlayerCardWindowController : WindowController
{
	[SerializeField]
	private CanvasGroup m_leftCardCanvasGroup;

	[SerializeField]
	private CanvasGroup m_rightCardCanvasGroup;

	[SerializeField]
	private PlayerLookController m_playerModel;

	private PlayerCardOpenedEventArgs m_playerCardEventArgs;

	private Action<InputActionEventData> m_onCancelButtonDown;

	public override WindowID ID => WindowID.PlayerCard;

	private void Awake()
	{
		m_onCancelButtonDown = OnCancelButtonDown;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_playerCardEventArgs = new PlayerCardOpenedEventArgs(null);
		if (m_playerModel.VisualsGameObject != null)
		{
			m_playerModel.VisualsGameObject.SetLayerRecursively(5, includeInactive: true);
		}
	}

	protected override void OnOpen()
	{
		SaveManager.PlayerSaveData.UpdateCachedData();
		m_windowCanvas.gameObject.SetActive(value: true);
		m_playerModel.InitializeLook(SaveManager.PlayerSaveData.CurrentCharacter);
		m_playerModel.Animator.SetBool("Victory", value: true);
		m_playerModel.Animator.Play("Victory", 0, 1f);
		m_playerModel.Animator.Update(1f);
		AddInputListeners();
		m_playerCardEventArgs.Initialize(SaveManager.PlayerSaveData);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.PlayerCardWindow_Opened, this, m_playerCardEventArgs);
		StartCoroutine(RunOpenAnimation());
	}

	private IEnumerator RunOpenAnimation()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_leftCardCanvasGroup.alpha = 0f;
		m_rightCardCanvasGroup.alpha = 0f;
		float num = 50f;
		float duration = 0.15f;
		RectTransform component = m_leftCardCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector = component.anchoredPosition;
		vector.y += num;
		component.anchoredPosition = vector;
		TweenManager.TweenBy_UnscaledTime(component, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		TweenManager.TweenTo_UnscaledTime(m_leftCardCanvasGroup, duration, Ease.None, "alpha", 1);
		RectTransform component2 = m_rightCardCanvasGroup.GetComponent<RectTransform>();
		Vector3 vector2 = component2.anchoredPosition;
		vector2.y += num;
		component2.anchoredPosition = vector2;
		TweenManager.TweenBy_UnscaledTime(component2, duration, Ease.Quad.EaseOut, "anchoredPosition.y", 0f - num);
		yield return TweenManager.TweenTo_UnscaledTime(m_rightCardCanvasGroup, duration, Ease.None, "alpha", 1).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
		RemoveInputListeners();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.PlayerCardWindow_Closed, this, null);
	}

	protected override void OnFocus()
	{
	}

	protected override void OnLostFocus()
	{
	}

	protected void AddInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		}
	}

	protected void RemoveInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
		}
	}

	protected virtual void OnCancelButtonDown(InputActionEventData eventData)
	{
		if (WindowManager.GetIsWindowOpen(WindowID.Pause))
		{
			WindowManager.CloseAllOpenWindows();
		}
		else
		{
			WindowManager.SetWindowIsOpen(ID, isOpen: false);
		}
	}
}
