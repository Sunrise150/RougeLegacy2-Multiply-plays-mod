using System;
using System.Collections;
using RL_Windows;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class GlossaryWindowController : WindowController, ILocalizable
{
	[SerializeField]
	private CanvasGroup m_leftCardCanvasGroup;

	[SerializeField]
	private CanvasGroup m_rightCardCanvasGroup;

	[SerializeField]
	private Image m_leftScrollArrow;

	[SerializeField]
	private Image m_rightScrollArrow;

	[SerializeField]
	private ScrollBarInput_RL m_leftScrollBarInput;

	[SerializeField]
	private ScrollBarInput_RL m_rightScrollBarInput;

	[SerializeField]
	private RectTransform m_leftScrollViewRectTransform;

	[SerializeField]
	private RectTransform m_rightScrollViewRectTransform;

	private GlossaryEntry[] m_leftGlossaryEntryArray;

	private GlossaryEntry[] m_rightGlossaryEntryArray;

	private ScrollBarInput_RL m_selectedScrollBarInput;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_onCancelButtonDown;

	private Action<InputActionEventData> m_onHorizontalInputHandler;

	public override WindowID ID => WindowID.Glossary;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onCancelButtonDown = OnCancelButtonDown;
		m_onHorizontalInputHandler = OnHorizontalInputHandler;
	}

	public override void Initialize()
	{
		m_leftGlossaryEntryArray = m_leftCardCanvasGroup.GetComponentsInChildren<GlossaryEntry>();
		m_rightGlossaryEntryArray = m_rightCardCanvasGroup.GetComponentsInChildren<GlossaryEntry>();
		GlossaryEntry[] leftGlossaryEntryArray = m_leftGlossaryEntryArray;
		for (int i = 0; i < leftGlossaryEntryArray.Length; i++)
		{
			leftGlossaryEntryArray[i].Initialize();
		}
		leftGlossaryEntryArray = m_rightGlossaryEntryArray;
		for (int i = 0; i < leftGlossaryEntryArray.Length; i++)
		{
			leftGlossaryEntryArray[i].Initialize();
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_leftScrollViewRectTransform);
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_rightScrollViewRectTransform);
		base.Initialize();
	}

	protected void SelectScrollBar(bool selectLeft)
	{
		if (selectLeft)
		{
			m_selectedScrollBarInput = m_leftScrollBarInput;
			m_leftScrollArrow.gameObject.SetActive(value: true);
			m_rightScrollArrow.gameObject.SetActive(value: false);
			m_leftScrollBarInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
			m_rightScrollBarInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
		else
		{
			m_selectedScrollBarInput = m_rightScrollBarInput;
			m_rightScrollArrow.gameObject.SetActive(value: true);
			m_leftScrollArrow.gameObject.SetActive(value: false);
			m_leftScrollBarInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
			m_rightScrollBarInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		RefreshText(null, null);
		SelectScrollBar(selectLeft: true);
		m_windowCanvas.gameObject.SetActive(value: true);
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
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	protected override void OnFocus()
	{
		AddInputListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	protected void AddInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
		}
	}

	protected void RemoveInputListeners()
	{
		if (ReInput.isReady && base.RewiredPlayer != null)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelButtonDown, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Horizontal");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onHorizontalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Horizontal");
		}
	}

	protected virtual void OnCancelButtonDown(InputActionEventData obj)
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

	private void OnHorizontalInputHandler(InputActionEventData eventData)
	{
		SelectScrollBar(m_selectedScrollBarInput != m_leftScrollBarInput);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		GlossaryEntry[] leftGlossaryEntryArray = m_leftGlossaryEntryArray;
		for (int i = 0; i < leftGlossaryEntryArray.Length; i++)
		{
			leftGlossaryEntryArray[i].Initialize();
		}
		leftGlossaryEntryArray = m_rightGlossaryEntryArray;
		for (int i = 0; i < leftGlossaryEntryArray.Length; i++)
		{
			leftGlossaryEntryArray[i].Initialize();
		}
	}
}
