using UnityEngine;

public class ChoosePrimaryDisplayOptionItem : SelectionListOptionItem
{
	protected override bool UsesLocID => false;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = PlayerPrefs.GetInt("UnitySelectMonitor");
			if (m_selectedIndex >= m_selectionLocIDArray.Length)
			{
				m_selectedIndex = 0;
			}
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		Display[] displays = Display.displays;
		m_selectionLocIDArray = new string[displays.Length];
		for (int i = 0; i < m_selectionLocIDArray.Length; i++)
		{
			m_selectionLocIDArray[i] = (i + 1).ToString();
		}
		m_selectedIndex = PlayerPrefs.GetInt("UnitySelectMonitor");
		if (m_selectedIndex >= m_selectionLocIDArray.Length)
		{
			m_selectedIndex = 0;
		}
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Change Primary Display to monitor: " + m_selectionLocIDArray[m_selectedIndex]);
	}

	public override void ConfirmOptionChange()
	{
		PlayerPrefs.SetInt("UnitySelectMonitor", m_selectedIndex);
	}
}
