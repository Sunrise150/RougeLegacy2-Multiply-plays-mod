using UnityEngine;

public class ToggleSuperInputOptionItem : SelectionListOptionItem
{
	protected override void Awake()
	{
		base.Awake();
		base.gameObject.SetActive(value: false);
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = (SaveManager.ConfigData.EnableVsync ? 1 : 0);
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[2] { "LOC_ID_GENERAL_UI_ON_1", "LOC_ID_GENERAL_UI_OFF_1" };
		m_selectedIndex = (SaveManager.ConfigData.EnableVsync ? 1 : 0);
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Toggle Super input Vsync to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		if (m_selectedIndex == 1)
		{
			SaveManager.ConfigData.EnableVsync = true;
		}
		else
		{
			SaveManager.ConfigData.EnableVsync = false;
		}
		GameResolutionManager.SetVsyncEnable(SaveManager.ConfigData.EnableVsync);
	}
}
