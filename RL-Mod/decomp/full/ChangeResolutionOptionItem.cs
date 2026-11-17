using System;
using System.Collections.Generic;
using UnityEngine;

public class ChangeResolutionOptionItem : SelectionListOptionItem
{
	private List<Resolution> m_resolutionsToUse = new List<Resolution>();

	private Action<MonoBehaviour, EventArgs> m_onPrimaryDisplayChanged;

	protected override bool UsesLocID => false;

	protected override void Awake()
	{
		base.Awake();
		m_onPrimaryDisplayChanged = OnPrimaryDisplayChanged;
	}

	public override void Initialize()
	{
		Resolution[] resolutions = Screen.resolutions;
		m_resolutionsToUse.Clear();
		for (int i = 0; i < resolutions.Length; i++)
		{
			Resolution item = resolutions[i];
			bool flag = true;
			foreach (Resolution item2 in m_resolutionsToUse)
			{
				if (item.width == item2.width && item.height == item2.height)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				m_resolutionsToUse.Add(item);
			}
		}
		m_selectionLocIDArray = new string[m_resolutionsToUse.Count];
		for (int j = 0; j < m_resolutionsToUse.Count; j++)
		{
			m_selectionLocIDArray[j] = m_resolutionsToUse[j].width + "x" + m_resolutionsToUse[j].height;
		}
		m_selectedIndex = 0;
		for (int k = 0; k < m_resolutionsToUse.Count; k++)
		{
			if (m_resolutionsToUse[k].width == SaveManager.ConfigData.ScreenWidth && m_resolutionsToUse[k].height == SaveManager.ConfigData.ScreenHeight)
			{
				m_selectedIndex = k;
				break;
			}
		}
		base.Initialize();
	}

	private void OnPrimaryDisplayChanged(object sender, EventArgs args)
	{
		Initialize();
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (base.IsInitialized)
		{
			m_selectedIndex = 0;
			for (int i = 0; i < m_resolutionsToUse.Count; i++)
			{
				if (SaveManager.ConfigData.ScreenWidth == m_resolutionsToUse[i].width && SaveManager.ConfigData.ScreenHeight == m_resolutionsToUse[i].height)
				{
					m_selectedIndex = i;
				}
			}
			m_incrementValueText.SetText(base.CurrentSelectionString);
		}
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.PrimaryDisplayChanged, m_onPrimaryDisplayChanged);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.PrimaryDisplayChanged, m_onPrimaryDisplayChanged);
	}

	public override void InvokeValueChange()
	{
		Debug.Log("Change resolution to: " + m_selectionLocIDArray[m_selectedIndex]);
	}

	public override void ConfirmOptionChange()
	{
		Resolution resolution = m_resolutionsToUse[m_selectedIndex];
		GameResolutionManager.SetResolution(resolution.width, resolution.height, GameResolutionManager.FullscreenMode);
	}
}
