using UnityEngine;
using UnityEngine.EventSystems;

public class OptionItemButton : MonoBehaviour, IPointerUpHandler, IEventSystemHandler, IPointerDownHandler
{
	private enum OptionItemButtonType
	{
		Increment,
		Decrement
	}

	[SerializeField]
	private OptionItemButtonType m_buttonType;

	private BaseOptionItem m_baseOptionItem;

	private float m_timeSinceClick;

	private bool m_pointerDown;

	private bool m_previousPointerDown;

	private void Awake()
	{
		m_baseOptionItem = GetComponentInParent<BaseOptionItem>();
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		m_pointerDown = false;
	}

	public void OnPointerDown(PointerEventData eventData)
	{
		if (m_baseOptionItem.Interactable)
		{
			if (m_baseOptionItem.PressAndHoldEnabled)
			{
				m_pointerDown = true;
				m_previousPointerDown = false;
			}
			else
			{
				HandleClick();
			}
		}
	}

	private void Update()
	{
		if (m_pointerDown && !(m_baseOptionItem == null) && m_baseOptionItem.PressAndHoldEnabled)
		{
			bool flag = false;
			if (!m_previousPointerDown)
			{
				m_previousPointerDown = true;
				m_timeSinceClick = Time.unscaledTime + 0.35f;
				flag = true;
			}
			else if (Time.unscaledTime > m_timeSinceClick + 0.1f)
			{
				flag = true;
				m_timeSinceClick = Time.unscaledTime;
			}
			if (flag)
			{
				HandleClick();
			}
		}
	}

	private void HandleClick()
	{
		if (m_buttonType == OptionItemButtonType.Increment)
		{
			m_baseOptionItem.InvokeIncrement();
		}
		else
		{
			m_baseOptionItem.InvokeDecrement();
		}
	}
}
