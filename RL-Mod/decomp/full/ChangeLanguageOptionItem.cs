using UnityEngine;

public class ChangeLanguageOptionItem : SelectionListOptionItem
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = Mathf.Max(0, LocalizationManager.AvailableLanguages.IndexOf(SaveManager.ConfigData.Language));
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[LocalizationManager.AvailableLanguages.Length];
		for (int i = 0; i < m_selectionLocIDArray.Length; i++)
		{
			LanguageType languageType = LocalizationManager.AvailableLanguages[i];
			m_selectionLocIDArray[i] = LocalizationManager.GetLanguageLocID(languageType);
		}
		m_selectedIndex = Mathf.Max(0, LocalizationManager.AvailableLanguages.IndexOf(SaveManager.ConfigData.Language));
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed language to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		SaveManager.ConfigData.Language = LocalizationManager.AvailableLanguages[m_selectedIndex];
		LocalizationManager.ChangeLanguage(SaveManager.ConfigData.Language, broadcastEvent: true);
	}
}
