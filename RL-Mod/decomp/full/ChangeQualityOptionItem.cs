using UnityEngine;

public class ChangeQualityOptionItem : SelectionListOptionItem
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = QualitySettings.GetQualityLevel();
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[4] { "LOC_ID_GRAPHICS_SETTING_QUALITY_LOW_1", "LOC_ID_GRAPHICS_SETTING_QUALITY_MEDIUM_1", "LOC_ID_GRAPHICS_SETTING_QUALITY_HIGH_1", "LOC_ID_GRAPHICS_SETTING_QUALITY_4K_1" };
		m_selectedIndex = QualitySettings.GetQualityLevel();
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed quality to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		QualitySettings.SetQualityLevel(m_selectedIndex, applyExpensiveChanges: true);
		SaveManager.ConfigData.QualitySetting = m_selectedIndex;
		Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.QualitySettingsChanged, null, null);
	}
}
