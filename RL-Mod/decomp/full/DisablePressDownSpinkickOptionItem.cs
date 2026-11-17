using UnityEngine;

public class DisablePressDownSpinkickOptionItem : SelectionListOptionItem
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = (SaveManager.ConfigData.DisablePressDownSpinKick ? 1 : 0);
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[2] { "LOC_ID_GENERAL_UI_ON_1", "LOC_ID_GENERAL_UI_OFF_1" };
		m_selectedIndex = (SaveManager.ConfigData.DisablePressDownSpinKick ? 1 : 0);
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed disable press down spin kick to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		if (m_selectedIndex == 0)
		{
			SaveManager.ConfigData.DisablePressDownSpinKick = false;
		}
		else
		{
			SaveManager.ConfigData.DisablePressDownSpinKick = true;
		}
	}
}
