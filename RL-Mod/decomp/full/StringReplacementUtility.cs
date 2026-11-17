using System;
using Rewired;
using TMPro;
using UnityEngine;

public class StringReplacementUtility : MonoBehaviour, ITextChangedObj
{
	[Serializable]
	private struct ReplacementStringEntry
	{
		public string TextToRemove;

		public string TextToAdd;
	}

	[SerializeField]
	private ReplacementStringEntry[] m_stringsToReplace;

	private TMP_Text m_text;

	private bool m_listenerApplied;

	public void LastControllerChanged(Controller controller)
	{
	}

	private void Awake()
	{
		m_text = GetComponent<TMP_Text>();
		if (!GetComponent<TextGlyphConverter>())
		{
			GlobalTextChangedStaticListener.AddTextChangedListener(m_text, this);
			m_listenerApplied = true;
		}
	}

	private void OnDestroy()
	{
		if (m_listenerApplied)
		{
			GlobalTextChangedStaticListener.RemoveTextChangedListener(m_text);
		}
	}

	public void OnTextChanged()
	{
		if (m_stringsToReplace != null && m_stringsToReplace.Length != 0)
		{
			bool flag = GlobalTextChangedStaticListener.HasTextChangedListener(m_text);
			if (flag)
			{
				GlobalTextChangedStaticListener.RemoveTextChangedListener(m_text);
			}
			string text = m_text.text;
			ReplacementStringEntry[] stringsToReplace = m_stringsToReplace;
			for (int i = 0; i < stringsToReplace.Length; i++)
			{
				ReplacementStringEntry replacementStringEntry = stringsToReplace[i];
				text = text.Replace(replacementStringEntry.TextToRemove, replacementStringEntry.TextToAdd);
			}
			m_text.SetText(text);
			m_text.ForceMeshUpdate(ignoreActiveState: true);
			if (flag)
			{
				GlobalTextChangedStaticListener.AddTextChangedListener(m_text, this);
			}
		}
	}
}
