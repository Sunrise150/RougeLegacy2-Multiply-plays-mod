using UnityEngine;

public class ToggleMouseAimingOptionItem : SelectionListOptionItem
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = ((!SaveManager.ConfigData.ToggleMouseAiming) ? 1 : 0);
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[2] { "LOC_ID_GENERAL_UI_ON_1", "LOC_ID_GENERAL_UI_OFF_1" };
		m_selectedIndex = ((!SaveManager.ConfigData.ToggleMouseAiming) ? 1 : 0);
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed toggle mouse aiming to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		if (m_selectedIndex == 0)
		{
			SaveManager.ConfigData.ToggleMouseAiming = true;
		}
		else
		{
			SaveManager.ConfigData.ToggleMouseAiming = false;
		}
	}
}
