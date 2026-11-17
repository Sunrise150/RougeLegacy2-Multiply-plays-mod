using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;

public abstract class IncrementDecrementOptionItem : BaseOptionItem
{
	[SerializeField]
	protected TMP_Text m_invertedIncrementText;

	[SerializeField]
	protected RectTransform m_invertedIncrementMaskRectTransform;

	[SerializeField]
	protected UnityEvent m_scrollEvent;

	protected float m_minValue;

	protected float m_maxValue = 1f;

	protected int m_numberOfIncrements = 10;

	private float m_incrementAmount;

	protected float m_currentIncrementValue;

	protected int m_snapToMultiplesOf = 1;

	protected float m_storedOnActivateValue;

	private float m_storedIncrementBarWidth;

	public override OptionsControlType OptionsControlType => OptionsControlType.IncrementDecrement;

	public override bool PressAndHoldEnabled => true;

	protected override void Awake()
	{
		base.Awake();
		m_storedIncrementBarWidth = m_incrementMaskRectTransform.sizeDelta.x;
	}

	public override void Initialize()
	{
		m_incrementAmount = (m_maxValue - m_minValue) / (float)m_numberOfIncrements;
		m_currentIncrementValue = m_minValue;
		UpdateIncrementBar();
		base.Initialize();
		m_incrementValueText.color = Color.white;
	}

	public override void InvokeIncrement()
	{
		if (m_currentIncrementValue < m_maxValue)
		{
			m_currentIncrementValue = Mathf.Clamp(m_currentIncrementValue + m_incrementAmount, m_minValue, m_maxValue);
			if (!CDGHelper.IsPercent(m_incrementAmount))
			{
				m_currentIncrementValue = Mathf.RoundToInt(m_currentIncrementValue / (float)m_snapToMultiplesOf) * m_snapToMultiplesOf;
			}
			UpdateIncrementBar();
			Increment();
			if (m_scrollEvent != null)
			{
				m_scrollEvent.Invoke();
			}
		}
	}

	public override void InvokeDecrement()
	{
		if (m_currentIncrementValue > m_minValue)
		{
			m_currentIncrementValue = Mathf.Clamp(m_currentIncrementValue - m_incrementAmount, m_minValue, m_maxValue);
			if (!CDGHelper.IsPercent(m_incrementAmount))
			{
				m_currentIncrementValue = Mathf.RoundToInt(m_currentIncrementValue / (float)m_snapToMultiplesOf) * m_snapToMultiplesOf;
			}
			UpdateIncrementBar();
			Decrement();
			if (m_scrollEvent != null)
			{
				m_scrollEvent.Invoke();
			}
		}
	}

	public override void ActivateOption()
	{
		base.ActivateOption();
		m_storedOnActivateValue = m_currentIncrementValue;
	}

	public override void CancelOptionChange()
	{
		m_currentIncrementValue = m_storedOnActivateValue;
		UpdateIncrementBar();
	}

	protected abstract void Increment();

	protected abstract void Decrement();

	public override void OnSelect(BaseEventData eventData)
	{
		base.OnSelect(eventData);
		m_incrementValueText.color = Color.white;
	}

	public override void OnDeselect(BaseEventData eventData)
	{
		base.OnDeselect(eventData);
		m_incrementValueText.color = Color.white;
	}

	protected virtual void UpdateIncrementBar()
	{
		if (Mathf.Floor(m_currentIncrementValue) == m_currentIncrementValue)
		{
			m_incrementValueText.SetText(m_currentIncrementValue.ToString());
			m_invertedIncrementText.SetText(m_currentIncrementValue.ToString());
		}
		else
		{
			m_incrementValueText.SetText(string.Format(LocalizationManager.GetCurrentCultureInfo(), "{0:F2}", m_currentIncrementValue));
			m_invertedIncrementText.SetText(string.Format(LocalizationManager.GetCurrentCultureInfo(), "{0:F2}", m_currentIncrementValue));
		}
		m_incrementValueText.color = Color.white;
		float num = m_currentIncrementValue / m_maxValue;
		float x = m_storedIncrementBarWidth * num;
		m_incrementMaskRectTransform.sizeDelta = new Vector2(x, m_incrementMaskRectTransform.sizeDelta.y);
		m_invertedIncrementMaskRectTransform.sizeDelta = new Vector2(x, m_incrementMaskRectTransform.sizeDelta.y);
	}

	public virtual void SetCurrentIncrementValue(float value, bool useNormalizedValue)
	{
		float currentIncrementValue = m_currentIncrementValue;
		if (!useNormalizedValue)
		{
			m_currentIncrementValue = Mathf.Clamp(value, m_minValue, m_maxValue);
		}
		else
		{
			value = (m_maxValue - m_minValue) * value;
			m_currentIncrementValue = Mathf.Clamp(value, m_minValue, m_maxValue);
		}
		float b = m_currentIncrementValue;
		if (!CDGHelper.IsPercent(currentIncrementValue))
		{
			b = Mathf.RoundToInt(m_currentIncrementValue);
		}
		if (!Mathf.Approximately(currentIncrementValue, b))
		{
			if (m_scrollEvent != null)
			{
				m_scrollEvent.Invoke();
			}
			UpdateIncrementBar();
		}
	}
}
