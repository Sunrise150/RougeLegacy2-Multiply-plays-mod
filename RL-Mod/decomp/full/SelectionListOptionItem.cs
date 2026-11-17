using System;
using UnityEngine;
using UnityEngine.Events;

public abstract class SelectionListOptionItem : BaseOptionItem
{
	[SerializeField]
	protected UnityEvent m_selectionChangeEvent;

	protected string[] m_selectionLocIDArray;

	protected int m_selectedIndex;

	protected bool m_applyChangeOnlyOnExit = true;

	private int m_storedOnActivateIndex;

	protected virtual bool UsesLocID => true;

	protected string CurrentSelectionString
	{
		get
		{
			if (m_selectionLocIDArray != null && m_selectedIndex >= 0 && m_selectedIndex < m_selectionLocIDArray.Length)
			{
				if (UsesLocID)
				{
					return LocalizationManager.GetString(m_selectionLocIDArray[m_selectedIndex], isFemale: false);
				}
				return m_selectionLocIDArray[m_selectedIndex];
			}
			return string.Empty;
		}
	}

	public override OptionsControlType OptionsControlType => OptionsControlType.SelectionList;

	public override bool PressAndHoldEnabled => false;

	public override void Initialize()
	{
		base.Initialize();
		RefreshText(null, null);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		RefreshText(null, null);
	}

	public override void InvokeIncrement()
	{
		base.InvokeIncrement();
		m_selectedIndex++;
		if (m_selectedIndex >= m_selectionLocIDArray.Length)
		{
			m_selectedIndex = 0;
		}
		m_incrementValueText.SetText(CurrentSelectionString);
		if (m_selectionChangeEvent != null)
		{
			m_selectionChangeEvent.Invoke();
		}
		if (!m_applyChangeOnlyOnExit)
		{
			InvokeValueChange();
		}
	}

	public override void InvokeDecrement()
	{
		base.InvokeDecrement();
		m_selectedIndex--;
		if (m_selectedIndex < 0)
		{
			m_selectedIndex = m_selectionLocIDArray.Length - 1;
		}
		m_incrementValueText.SetText(CurrentSelectionString);
		if (m_selectionChangeEvent != null)
		{
			m_selectionChangeEvent.Invoke();
		}
		if (!m_applyChangeOnlyOnExit)
		{
			InvokeValueChange();
		}
	}

	public override void ActivateOption()
	{
		m_storedOnActivateIndex = m_selectedIndex;
		base.ActivateOption();
	}

	public override void DeactivateOption(bool confirmOptionChange)
	{
		if (confirmOptionChange && m_applyChangeOnlyOnExit)
		{
			InvokeValueChange();
		}
		base.DeactivateOption(confirmOptionChange);
	}

	public override void CancelOptionChange()
	{
		m_selectedIndex = m_storedOnActivateIndex;
		if (!m_applyChangeOnlyOnExit)
		{
			InvokeValueChange();
		}
		m_incrementValueText.SetText(CurrentSelectionString);
	}

	public abstract void InvokeValueChange();

	public override void RefreshText(object sender, EventArgs args)
	{
		if (base.IsInitialized)
		{
			m_incrementValueText.SetText(CurrentSelectionString);
		}
	}
}
