using System;
using System.Collections;
using System.Collections.Generic;
using Rewired;
using UnityEngine;
using UnityEngine.UI;

public class ScrollBarInput_RL : MonoBehaviour
{
	[SerializeField]
	private ScrollRect m_scrollRect;

	[SerializeField]
	[Tooltip("Pixels per second.  A good base number is ~1000")]
	private float m_scrollMaxSpeed = 1000f;

	[SerializeField]
	private bool m_resetScrollBarOnEnable = true;

	[SerializeField]
	private List<Rewired_RL.WindowInputActionType> m_assignedScrollButtons;

	private float m_scrollAcceleration = 10000f;

	private Scrollbar m_scrollBar;

	private float m_scrollBarHeight;

	private float m_currentSpeed;

	private bool m_buttonPressed;

	private bool m_registeredInputThisFrame;

	private bool m_inputListenersAdded;

	private Action<InputActionEventData> m_onVerticalInputHandler;

	private void Awake()
	{
		m_onVerticalInputHandler = OnVerticalInputHandler;
	}

	public void AssignButtonToScroll(Rewired_RL.WindowInputActionType windowInputType)
	{
		if (m_inputListenersAdded)
		{
			RemoveInputListeners();
		}
		if (!m_assignedScrollButtons.Contains(windowInputType))
		{
			m_assignedScrollButtons.Add(windowInputType);
		}
		if (base.isActiveAndEnabled)
		{
			AddInputListeners();
		}
	}

	public void RemoveButtonToScroll(Rewired_RL.WindowInputActionType windowInputType)
	{
		if (m_inputListenersAdded)
		{
			RemoveInputListeners();
		}
		if (m_assignedScrollButtons.Contains(windowInputType))
		{
			m_assignedScrollButtons.Remove(windowInputType);
		}
		if (base.isActiveAndEnabled)
		{
			AddInputListeners();
		}
	}

	private void AddInputListeners()
	{
		Player player = Rewired_RL.Player;
		if (player == null)
		{
			return;
		}
		foreach (Rewired_RL.WindowInputActionType assignedScrollButton in m_assignedScrollButtons)
		{
			player.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonPressed, Rewired_RL.GetString(assignedScrollButton));
			player.AddInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonPressed, Rewired_RL.GetString(assignedScrollButton));
		}
		m_inputListenersAdded = true;
	}

	private void RemoveInputListeners()
	{
		Player player = Rewired_RL.Player;
		if (player == null)
		{
			return;
		}
		foreach (Rewired_RL.WindowInputActionType assignedScrollButton in m_assignedScrollButtons)
		{
			player.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.ButtonPressed, Rewired_RL.GetString(assignedScrollButton));
			player.RemoveInputEventDelegate(m_onVerticalInputHandler, UpdateLoopType.Update, InputActionEventType.NegativeButtonPressed, Rewired_RL.GetString(assignedScrollButton));
		}
		m_inputListenersAdded = false;
	}

	public void OnEnable()
	{
		m_registeredInputThisFrame = false;
		m_buttonPressed = false;
		m_currentSpeed = 0f;
		m_scrollBar = m_scrollRect.verticalScrollbar;
		m_scrollBarHeight = m_scrollRect.GetComponent<RectTransform>().rect.height;
		if (m_resetScrollBarOnEnable)
		{
			StartCoroutine(ResetScrollBarCoroutine());
		}
		if (!m_inputListenersAdded)
		{
			AddInputListeners();
		}
	}

	private IEnumerator ResetScrollBarCoroutine()
	{
		yield return null;
		m_scrollBar.value = 1f;
	}

	public void ResetScrollBar()
	{
		m_scrollBar.value = 1f;
	}

	public void OnDisable()
	{
		if (m_inputListenersAdded)
		{
			RemoveInputListeners();
		}
	}

	private void OnVerticalInputHandler(InputActionEventData eventData)
	{
		if (!eventData.IsCurrentInputSource(ControllerType.Mouse))
		{
			m_buttonPressed = true;
			float num = eventData.GetAxis();
			if (num == 0f)
			{
				num = 0f - eventData.GetAxisPrev();
			}
			if (num > 0f)
			{
				IncrementScroll(decrement: true, num);
			}
			else
			{
				IncrementScroll(decrement: false, num);
			}
		}
	}

	private void IncrementScroll(bool decrement, float axisAmount)
	{
		if (!m_registeredInputThisFrame)
		{
			axisAmount = Mathf.Abs(axisAmount);
			float num = m_scrollAcceleration * Time.unscaledDeltaTime;
			if (!decrement)
			{
				m_currentSpeed -= num;
				m_currentSpeed = Mathf.Clamp(m_currentSpeed, (0f - m_scrollMaxSpeed) * axisAmount, 0f);
			}
			else
			{
				m_currentSpeed += num;
				m_currentSpeed = Mathf.Clamp(m_currentSpeed, 0f, m_scrollMaxSpeed * axisAmount);
			}
			m_registeredInputThisFrame = true;
		}
	}

	public void Update()
	{
		if (m_currentSpeed != 0f)
		{
			float num = m_scrollBarHeight / m_scrollBar.size - m_scrollBarHeight;
			float num2 = m_currentSpeed / num * Time.unscaledDeltaTime;
			float value = m_scrollBar.value + num2;
			value = Mathf.Clamp01(value);
			m_scrollBar.value = value;
			if (!m_buttonPressed)
			{
				m_currentSpeed = Mathf.Lerp(m_currentSpeed, 0f, 0.1f);
			}
		}
		m_registeredInputThisFrame = false;
		m_buttonPressed = false;
	}
}
