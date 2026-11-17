using System;
using UnityEngine;

public class ChangeDisplayModeOptionItem : SelectionListOptionItem
{
	private FullScreenMode[] m_screenModeArray;

	public override void Initialize()
	{
		FullScreenMode[] array = Enum.GetValues(typeof(FullScreenMode)) as FullScreenMode[];
		m_screenModeArray = new FullScreenMode[array.Length - 1];
		m_selectionLocIDArray = new string[array.Length - 1];
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			if (array[i] != FullScreenMode.MaximizedWindow)
			{
				string text = null;
				switch (array[i])
				{
				case FullScreenMode.ExclusiveFullScreen:
					text = "LOC_ID_GRAPHICS_SETTING_FULLSCREEN_1";
					break;
				case FullScreenMode.FullScreenWindow:
					text = "LOC_ID_GRAPHICS_SETTING_FULLSCREEN_BORDERLESS_1";
					break;
				case FullScreenMode.Windowed:
					text = "LOC_ID_GRAPHICS_SETTING_WINDOW_1";
					break;
				}
				m_selectionLocIDArray[num] = text;
				m_screenModeArray[num] = array[i];
				num++;
			}
		}
		base.Initialize();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = m_screenModeArray.IndexOf(Screen.fullScreenMode);
			if (m_selectedIndex == -1)
			{
				m_selectedIndex = 0;
			}
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Change screen mod to: " + base.CurrentSelectionString);
	}

	public override void ConfirmOptionChange()
	{
		GameResolutionManager.SetResolution(GameResolutionManager.Resolution.x, GameResolutionManager.Resolution.y, m_screenModeArray[m_selectedIndex]);
	}
}
