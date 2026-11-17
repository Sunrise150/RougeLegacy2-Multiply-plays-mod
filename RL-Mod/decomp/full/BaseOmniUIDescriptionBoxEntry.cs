using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public abstract class BaseOmniUIDescriptionBoxEntry<T, U> : MonoBehaviour where T : EventArgs where U : Enum
{
	protected static Color BenefitColor = new Color(1f / 17f, 26f / 51f, 0f);

	protected static Color DeficitColor = new Color(0.8392157f, 0f, 0f);

	protected static Color OriginalColor = new Color(0.3254902f, 21f / 85f, 14f / 51f);

	[SerializeField]
	protected CanvasGroup m_textCanvasGroup;

	[SerializeField]
	protected U m_descriptionType;

	[SerializeField]
	protected Image m_icon;

	[SerializeField]
	protected GameObject m_iconGO;

	[SerializeField]
	protected GameObject m_inactiveIconGO;

	[SerializeField]
	protected TMP_Text m_titleText;

	[SerializeField]
	protected TMP_Text m_text1;

	[SerializeField]
	protected TMP_Text m_text2;

	private Action<MonoBehaviour, EventArgs> m_updateEntry;

	protected virtual void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		if ((bool)m_titleText)
		{
			m_titleText.text = "";
		}
		if ((bool)m_text1)
		{
			m_text1.text = "";
		}
		if ((bool)m_text2)
		{
			m_text2.text = "";
		}
	}

	protected abstract void DisplayDescriptionBox(T args);

	private void UpdateEntry(MonoBehaviour sender, EventArgs args)
	{
		if (args == null)
		{
			if ((bool)m_iconGO && m_iconGO.activeSelf)
			{
				m_iconGO.SetActive(value: false);
			}
			if ((bool)m_inactiveIconGO && !m_inactiveIconGO.activeSelf)
			{
				m_inactiveIconGO.SetActive(value: true);
			}
			DisplayNullDescriptionBox(sender);
			return;
		}
		if ((bool)m_iconGO && !m_iconGO.activeSelf)
		{
			m_iconGO.SetActive(value: true);
		}
		if ((bool)m_inactiveIconGO && m_inactiveIconGO.activeSelf)
		{
			m_inactiveIconGO.SetActive(value: false);
		}
		T val = (T)args;
		if (val == null)
		{
			throw new InvalidCastException("Cannot cast " + args.GetType()?.ToString() + " into " + typeof(T).GetType());
		}
		DisplayDescriptionBox(val);
	}

	protected virtual void Awake()
	{
		m_updateEntry = UpdateEntry;
	}

	protected void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.OmniUI_UpdateDescription, m_updateEntry);
	}

	protected void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.OmniUI_UpdateDescription, m_updateEntry);
	}

	protected string ColoredString(string text, Color color)
	{
		return "<color=#" + ColorUtility.ToHtmlStringRGBA(color) + ">" + text + "</color>";
	}

	protected string PercentString(float value)
	{
		value *= 100f;
		string text = value.ToString("F1", LocalizationManager.GetCurrentCultureInfo());
		if (text[text.Length - 1] == '0')
		{
			text = Mathf.RoundToInt(value).ToString();
		}
		return string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), text);
	}

	protected string PlusSymbolString(float value, bool isPercent, bool addSymbolToZero = true)
	{
		string text = (isPercent ? PercentString(value) : value.ToCIString());
		if (addSymbolToZero)
		{
			if (!(value >= 0f))
			{
				return text;
			}
			return "+" + text;
		}
		if (!(value > 0f))
		{
			return text;
		}
		return "+" + text;
	}

	protected string ColoredValueString(float value, bool addBrackets, bool isPercent, bool lowerIsBetter, bool hideZero = true)
	{
		if (hideZero && value == 0f)
		{
			return "";
		}
		Color color = OriginalColor;
		if (value != 0f)
		{
			color = (((!(value > 0f) || lowerIsBetter) && (!(value < 0f) || !lowerIsBetter)) ? DeficitColor : BenefitColor);
		}
		string text = (isPercent ? PercentString(value) : value.ToCIString());
		if (value >= 0f)
		{
			text = PlusSymbolString(value, isPercent);
		}
		if (addBrackets)
		{
			text = "(" + text + ")";
		}
		return ColoredString(text, color);
	}
}
