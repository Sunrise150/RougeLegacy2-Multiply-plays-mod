using System;
using System.Collections;
using System.Collections.Generic;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmMenuWindowController : WindowController, ILocalizable
{
	private const float DEFAULT_ONE_BUTTON_BG_HEIGHT = 765f;

	private const float DEFAULT_ONE_BUTTON_BIG_BG_HEIGHT = 970f;

	public const float BUTTON_HEIGHT = 100f;

	[SerializeField]
	private RectTransform m_confirmBoxBGRectTransform;

	[SerializeField]
	private CanvasGroup m_fadeBGCanvasGroup;

	[SerializeField]
	private Image m_backgroundImage;

	[SerializeField]
	private Material m_backgroundHackMaterial;

	[SerializeField]
	private TMP_Text m_titleText;

	[SerializeField]
	private TMP_Text m_descriptionText;

	[SerializeField]
	private GameObject m_confirmMenuBox;

	[SerializeField]
	private TMP_Text m_buttonDelayCountdownText;

	[SerializeField]
	private bool m_isBigWindow;

	[Space(10f)]
	[SerializeField]
	private UnityEvent m_windowOpenedEvent;

	[SerializeField]
	private UnityEvent m_windowClosedEvent;

	[SerializeField]
	private UnityEvent m_changeSelectedOptionEvent;

	[SerializeField]
	private UnityEvent m_selectOptionEvent;

	private ConfirmMenu_Button[] m_buttonArray;

	private List<ConfirmMenu_Button> m_activeButtonList;

	private int m_selectedButtonIndex;

	private Action m_onCancelAction;

	private Vector3 m_storedBoxScale;

	private int m_startingButtonIndex;

	private bool m_playerMATHackApplied;

	private float m_buttonDelayTime;

	private string m_refreshTitleTextLocID;

	private string m_refreshDescriptionTextLocID;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_onConfirmPressed;

	private Action<InputActionEventData> m_onCancelPressed;

	private Action<InputActionEventData> m_onVerticalPressed;

	protected bool InputDelayed => Time.unscaledTime < m_buttonDelayTime;

	public override WindowID ID
	{
		get
		{
			if (!m_isBigWindow)
			{
				return WindowID.ConfirmMenu;
			}
			return WindowID.ConfirmMenuBig;
		}
	}

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onConfirmPressed = OnConfirmPressed;
		m_onCancelPressed = OnCancelPressed;
		m_onVerticalPressed = OnVerticalPressed;
	}

	public override void Initialize()
	{
		base.Initialize();
		m_storedBoxScale = m_confirmMenuBox.transform.localScale;
		if (!m_isBigWindow)
		{
			SetBGHeight(765f);
		}
		else
		{
			SetBGHeight(970f);
		}
		m_buttonArray = GetComponentsInChildren<ConfirmMenu_Button>(includeInactive: true);
		for (int i = 0; i < m_buttonArray.Length; i++)
		{
			m_buttonArray[i].Initialize(i, OnPointerEnter, OnPointerExit, PlayClickSFX);
		}
		m_activeButtonList = new List<ConfirmMenu_Button>();
	}

	private void OnPointerExit()
	{
	}

	private void OnPointerEnter(ConfirmMenu_Button button)
	{
		SetSelectedButton(button.Index);
	}

	private void PlayClickSFX(ConfirmMenu_Button button)
	{
		if (m_selectOptionEvent != null)
		{
			m_selectOptionEvent.Invoke();
		}
	}

	private void SetSelectedButton(int index)
	{
		if (index == m_selectedButtonIndex)
		{
			return;
		}
		int selectedButtonIndex = m_selectedButtonIndex;
		m_selectedButtonIndex = index;
		if (selectedButtonIndex > -1 && selectedButtonIndex < m_activeButtonList.Count)
		{
			m_activeButtonList[selectedButtonIndex].OnDeselect(null);
		}
		if (index < 0)
		{
			foreach (ConfirmMenu_Button activeButton in m_activeButtonList)
			{
				activeButton.OnDeselect(null);
			}
		}
		else
		{
			m_activeButtonList[m_selectedButtonIndex].OnSelect(null);
		}
		if (m_changeSelectedOptionEvent != null)
		{
			m_changeSelectedOptionEvent.Invoke();
		}
	}

	protected override void OnFocus()
	{
		AddInputListeners();
	}

	protected override void OnLostFocus()
	{
		RemoveInputListeners();
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		m_windowCanvas.gameObject.SetActive(value: true);
		m_selectedButtonIndex = -1;
		SetSelectedButton(m_startingButtonIndex);
		StartCoroutine(RunOpenAnimCoroutine());
		if (m_windowOpenedEvent != null)
		{
			m_windowOpenedEvent.Invoke();
		}
	}

	private IEnumerator RunOpenAnimCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		foreach (ConfirmMenu_Button activeButton in m_activeButtonList)
		{
			activeButton.DisableClick = true;
		}
		if (m_buttonDelayTime > 0f)
		{
			StartCoroutine(ConfirmButtonDelayCoroutine());
		}
		else
		{
			m_buttonDelayCountdownText.gameObject.SetActive(value: false);
		}
		m_fadeBGCanvasGroup.alpha = 0f;
		TweenManager.TweenTo_UnscaledTime(m_fadeBGCanvasGroup, 0.1f, Ease.None, "alpha", 1);
		Vector3 storedBoxScale = m_storedBoxScale;
		storedBoxScale -= new Vector3(0.05f, 0.05f, 0f);
		m_confirmMenuBox.transform.localScale = storedBoxScale;
		yield return TweenManager.TweenTo_UnscaledTime(m_confirmMenuBox.transform, 0.15f, Ease.Back.EaseOutLarge, "localScale.x", m_storedBoxScale.x, "localScale.y", m_storedBoxScale.y).TweenCoroutine;
		foreach (ConfirmMenu_Button activeButton2 in m_activeButtonList)
		{
			activeButton2.DisableClick = false;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	private IEnumerator ConfirmButtonDelayCoroutine()
	{
		foreach (ConfirmMenu_Button activeButton in m_activeButtonList)
		{
			activeButton.gameObject.SetActive(value: false);
		}
		m_buttonDelayCountdownText.gameObject.SetActive(value: true);
		m_buttonDelayTime += Time.unscaledTime;
		while (Time.unscaledTime < m_buttonDelayTime)
		{
			int num = (int)(m_buttonDelayTime - Time.unscaledTime) + 1;
			m_buttonDelayCountdownText.text = "- " + num + " -";
			yield return null;
		}
		m_buttonDelayCountdownText.gameObject.SetActive(value: false);
		foreach (ConfirmMenu_Button activeButton2 in m_activeButtonList)
		{
			activeButton2.gameObject.SetActive(value: true);
		}
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		ConfirmMenu_Button[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			buttonArray[i].ResetValues();
		}
		m_onCancelAction = null;
		SetStartingSelectedButton(0);
		m_windowCanvas.gameObject.SetActive(value: false);
		if (m_windowClosedEvent != null)
		{
			m_windowClosedEvent.Invoke();
		}
		if (m_playerMATHackApplied)
		{
			m_backgroundImage.material = null;
			RectTransform component = m_backgroundImage.GetComponent<RectTransform>();
			Vector3 anchoredPosition3D = component.anchoredPosition3D;
			anchoredPosition3D.z = 0f;
			component.anchoredPosition3D = anchoredPosition3D;
			m_playerMATHackApplied = false;
			base.WindowCanvas.sortingLayerName = "Default";
		}
		m_buttonDelayTime = 0f;
	}

	public void SetStartingSelectedButton(int startingButtonIndex)
	{
		m_startingButtonIndex = startingButtonIndex;
	}

	public void SetNumberOfButtons(int numButtons)
	{
		if (numButtons < 1 || numButtons >= m_buttonArray.Length)
		{
			Debug.Log("<color=red>ConfirmMenuWindowController.SetNumberOfButtons(int numButtons) - Cannot set confirm menu buttons to: " + numButtons + ". Max number of buttons allowed is: " + (m_buttonArray.Length - 1) + ". Must also be greater than 0.</color>");
			return;
		}
		m_activeButtonList.Clear();
		for (int i = 0; i < numButtons; i++)
		{
			m_buttonArray[i].gameObject.SetActive(value: true);
			m_activeButtonList.Add(m_buttonArray[i]);
		}
		float num = 100f * (float)(numButtons - 1);
		if (!m_isBigWindow)
		{
			SetBGHeight(765f + num);
		}
		else
		{
			SetBGHeight(970f + num);
		}
	}

	public void SetButtonDelayTime(float delayTime)
	{
		m_buttonDelayTime = delayTime;
	}

	public ConfirmMenu_Button GetButtonAtIndex(int index)
	{
		if (index < 0 || index >= m_activeButtonList.Count)
		{
			Debug.Log("<color=red>ConfirmMenuWindowController.GetButtonAtIndex(int index) - Cannot get button at index: " + index + ". Index is out of bounds. Please call SetNumberOfButtons() first to turn on the required number of buttons.</color>");
			return null;
		}
		return m_activeButtonList[index];
	}

	private void SetBGHeight(float height)
	{
		Vector2 sizeDelta = m_confirmBoxBGRectTransform.sizeDelta;
		sizeDelta.y = height;
		m_confirmBoxBGRectTransform.sizeDelta = sizeDelta;
		LayoutRebuilder.ForceRebuildLayoutImmediate(m_confirmBoxBGRectTransform);
	}

	public void SetTitleText(string text, bool isLocID)
	{
		if (isLocID)
		{
			m_titleText.text = LocalizationManager.GetString(text, isFemale: false);
			m_refreshTitleTextLocID = text;
		}
		else
		{
			m_titleText.text = text;
			m_refreshTitleTextLocID = null;
		}
	}

	public void SetDescriptionText(string text, bool isLocID)
	{
		if (isLocID)
		{
			m_descriptionText.text = LocalizationManager.GetString(text, isFemale: false);
			m_refreshDescriptionTextLocID = text;
		}
		else
		{
			m_descriptionText.text = text;
			m_refreshDescriptionTextLocID = null;
		}
	}

	public void SetOnCancelAction(Action cancelAction)
	{
		m_onCancelAction = cancelAction;
	}

	public void ApplyMATHack()
	{
		if (!m_playerMATHackApplied)
		{
			m_backgroundImage.material = m_backgroundHackMaterial;
			RectTransform component = m_backgroundImage.GetComponent<RectTransform>();
			Vector3 anchoredPosition3D = component.anchoredPosition3D;
			anchoredPosition3D.z = -50f;
			component.anchoredPosition3D = anchoredPosition3D;
			base.WindowCanvas.sortingLayerName = "Game";
			m_playerMATHackApplied = true;
		}
	}

	private void OnConfirmPressed(InputActionEventData data)
	{
		if (!data.IsCurrentInputSource(ControllerType.Mouse) && m_selectedButtonIndex > -1 && m_selectedButtonIndex < m_activeButtonList.Count && !InputDelayed)
		{
			m_activeButtonList[m_selectedButtonIndex].ButtonPressed();
			if (m_selectOptionEvent != null)
			{
				m_selectOptionEvent.Invoke();
			}
		}
	}

	private void OnCancelPressed(InputActionEventData data)
	{
		if (!InputDelayed && m_onCancelAction != null)
		{
			m_onCancelAction();
		}
	}

	private void OnVerticalPressed(InputActionEventData data)
	{
		if (InputDelayed)
		{
			return;
		}
		int selectedButtonIndex = m_selectedButtonIndex;
		int num = selectedButtonIndex;
		if (num == -1)
		{
			num = 0;
		}
		else
		{
			float num2 = data.GetAxis();
			if (num2 == 0f)
			{
				num2 = 0f - data.GetAxisPrev();
			}
			num = ((!(num2 < 0f)) ? (num - 1) : (num + 1));
		}
		if (num < 0)
		{
			num = m_activeButtonList.Count - 1;
		}
		else if (num >= m_activeButtonList.Count)
		{
			num = 0;
		}
		if (num != selectedButtonIndex)
		{
			SetSelectedButton(num);
		}
	}

	private void AddInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onConfirmPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.AddInputEventDelegate(m_onCancelPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_onVerticalPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Vertical");
		}
	}

	private void RemoveInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onCancelPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Vertical");
			base.RewiredPlayer.RemoveInputEventDelegate(m_onVerticalPressed, UpdateLoopType.Update, InputActionEventType.NegativeButtonJustPressed, "Window_Vertical");
		}
	}

	public void RefreshText(object sender, EventArgs args)
	{
		if (!string.IsNullOrEmpty(m_refreshTitleTextLocID))
		{
			m_descriptionText.text = LocalizationManager.GetString(m_refreshTitleTextLocID, isFemale: false);
		}
		if (!string.IsNullOrEmpty(m_refreshDescriptionTextLocID))
		{
			m_descriptionText.text = LocalizationManager.GetString(m_refreshDescriptionTextLocID, isFemale: false);
		}
		ConfirmMenu_Button[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			buttonArray[i].ForceRefreshText();
		}
	}
}
