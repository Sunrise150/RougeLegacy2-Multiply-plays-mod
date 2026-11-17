using UnityEngine;

public class ChangeInputIconOptionItem : SelectionListOptionItem
{
	private int m_startingIndex;

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = (int)SaveManager.ConfigData.InputIconSetting;
			m_startingIndex = m_selectedIndex;
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[4] { "LOC_ID_GRAPHICS_SETTING_AUTO_1", "LOC_ID_GRAPHICS_SETTING_XBOX_1", "LOC_ID_GRAPHICS_SETTING_PLAYSTATION_1", "LOC_ID_GRAPHICS_SETTING_SWITCH_1" };
		m_selectedIndex = (int)SaveManager.ConfigData.InputIconSetting;
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed Input Icon Setting: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		if (m_startingIndex != m_selectedIndex)
		{
			SaveManager.ConfigData.InputIconSetting = (InputIconSetting)m_selectedIndex;
			RewiredOnStartupController.UpdateActiveGamepadType();
			LocalizationManager.ForceRefreshAllTextGlyphs();
		}
	}
}
