using System;
using TMPro;
using UnityEngine;

public abstract class BaseOmniUIPurchaseBoxEntry<T> : MonoBehaviour where T : EventArgs
{
	protected enum OmniUIPurchaseBoxType
	{
		None,
		FlavourText,
		MoneyOwned,
		MoneyCost
	}

	[SerializeField]
	protected OmniUIPurchaseBoxType m_descriptionType;

	[SerializeField]
	protected TMP_Text m_text1;

	[SerializeField]
	protected TMP_Text m_text2;

	private Action<MonoBehaviour, EventArgs> m_updateEntry;

	protected virtual void DisplayNullPurchaseBox()
	{
		switch (m_descriptionType)
		{
		case OmniUIPurchaseBoxType.FlavourText:
			if ((bool)m_text1)
			{
				m_text1.text = "";
			}
			break;
		case OmniUIPurchaseBoxType.MoneyOwned:
		case OmniUIPurchaseBoxType.MoneyCost:
			if ((bool)m_text1)
			{
				m_text1.text = "0";
			}
			if ((bool)m_text2)
			{
				m_text2.text = "0";
			}
			break;
		}
	}

	protected abstract void DisplayPurchaseBox(T args);

	private void Awake()
	{
		m_updateEntry = UpdateEntry;
	}

	protected virtual void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.OmniUI_UpdateDescription, m_updateEntry);
	}

	protected virtual void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.OmniUI_UpdateDescription, m_updateEntry);
	}

	private void UpdateEntry(MonoBehaviour sender, EventArgs args)
	{
		if (args == null)
		{
			DisplayNullPurchaseBox();
			return;
		}
		T val = (T)args;
		if (val == null)
		{
			throw new InvalidCastException("Cannot cast " + args.GetType()?.ToString() + " into " + typeof(T).GetType());
		}
		DisplayPurchaseBox(val);
	}
}
