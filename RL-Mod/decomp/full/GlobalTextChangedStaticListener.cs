using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Rewired;
using TMPro;
using UnityEngine;

public static class GlobalTextChangedStaticListener
{
	private static Dictionary<UnityEngine.Object, ITextChangedObj> m_textChangedListenerTable = new Dictionary<UnityEngine.Object, ITextChangedObj>();

	private static HashSet<ITextChangedObj> m_controllerChangedListenerSet = new HashSet<ITextChangedObj>();

	private static StringBuilder m_stringHelper;

	private static Action<UnityEngine.Object> m_onTextChanged;

	public static bool IsInitialized { get; private set; }

	public static void Initialize()
	{
		if (Application.isPlaying)
		{
			m_onTextChanged = OnTextChanged;
			TMPro_EventManager.TEXT_CHANGED_EVENT.Add(m_onTextChanged);
			ReInput.controllers.AddLastActiveControllerChangedDelegate(OnControllerChanged);
			m_stringHelper = new StringBuilder();
			IsInitialized = true;
		}
	}

	public static void AddTextChangedListener(TMP_Text text, ITextChangedObj textGlyph)
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		if (!m_textChangedListenerTable.ContainsKey(text))
		{
			m_textChangedListenerTable.Add(text, textGlyph);
		}
	}

	public static void RemoveTextChangedListener(TMP_Text text)
	{
		if (m_textChangedListenerTable.ContainsKey(text))
		{
			m_textChangedListenerTable.Remove(text);
		}
	}

	public static bool HasTextChangedListener(TMP_Text text)
	{
		return m_textChangedListenerTable.ContainsKey(text);
	}

	public static void AddControllerChangedListener(ITextChangedObj textGlyph)
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		m_controllerChangedListenerSet.Add(textGlyph);
	}

	public static void RemoveControllerChangedListener(ITextChangedObj textGlyph)
	{
		m_controllerChangedListenerSet.Remove(textGlyph);
	}

	private static void OnTextChanged(UnityEngine.Object sender)
	{
		if (!Application.isPlaying)
		{
			Dispose();
			return;
		}
		if (m_textChangedListenerTable.TryGetValue(sender, out var value))
		{
			value.OnTextChanged();
		}
		if (LocalizationManager.CurrentLanguageType == LanguageType.Turkish)
		{
			ApplyTurkishCulture(sender as TMP_Text);
		}
	}

	public static void ApplyTurkishCulture(TMP_Text tmpText)
	{
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(m_onTextChanged);
		CultureInfo cultureInfo = LocalizationManager.GetCultureInfo(LanguageType.Turkish);
		if ((tmpText.fontStyle & FontStyles.UpperCase) != 0 || (tmpText.fontStyle & FontStyles.SmallCaps) != 0)
		{
			m_stringHelper.Clear();
			m_stringHelper.Append(tmpText.text);
			bool flag = false;
			int length = m_stringHelper.Length;
			for (int i = 0; i < length; i++)
			{
				if (!flag)
				{
					char c = m_stringHelper[i];
					switch (c)
					{
					case '<':
					case '[':
						flag = true;
						break;
					case '>':
					case ']':
						flag = false;
						break;
					default:
						m_stringHelper[i] = char.ToUpper(c, cultureInfo);
						break;
					}
				}
			}
			tmpText.text = m_stringHelper.ToString();
			tmpText.ForceMeshUpdate();
		}
		TMPro_EventManager.TEXT_CHANGED_EVENT.Add(m_onTextChanged);
	}

	private static void OnControllerChanged(Controller controller)
	{
		foreach (ITextChangedObj item in m_controllerChangedListenerSet)
		{
			item.LastControllerChanged(controller);
		}
	}

	public static void Dispose()
	{
		m_controllerChangedListenerSet.Clear();
		m_controllerChangedListenerSet = null;
		m_textChangedListenerTable.Clear();
		m_textChangedListenerTable = null;
		TMPro_EventManager.TEXT_CHANGED_EVENT.Remove(m_onTextChanged);
		if (Application.isPlaying)
		{
			ReInput.controllers.RemoveLastActiveControllerChangedDelegate(OnControllerChanged);
		}
		IsInitialized = false;
	}
}
