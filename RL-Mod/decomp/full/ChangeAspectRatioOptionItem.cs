using UnityEngine;

public class ChangeAspectRatioOptionItem : SelectionListOptionItem
{
	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = (SaveManager.ConfigData.Disable_16_9 ? 1 : 0);
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void Initialize()
	{
		m_selectionLocIDArray = new string[2] { "LOC_ID_GENERAL_UI_ON_1", "LOC_ID_GENERAL_UI_OFF_1" };
		m_selectedIndex = (SaveManager.ConfigData.Disable_16_9 ? 1 : 0);
		base.Initialize();
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Changed Lock Aspect Ratio to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		bool disable_16_9_Aspect = AspectRatioManager.Disable_16_9_Aspect;
		if (m_selectedIndex == 0)
		{
			SaveManager.ConfigData.Disable_16_9 = false;
		}
		else
		{
			SaveManager.ConfigData.Disable_16_9 = true;
		}
		if (disable_16_9_Aspect != AspectRatioManager.Disable_16_9_Aspect)
		{
			GameResolutionManager.SetResolution(GameResolutionManager.Resolution.x, GameResolutionManager.Resolution.y, GameResolutionManager.FullscreenMode, saveConfig: false);
		}
	}
}
