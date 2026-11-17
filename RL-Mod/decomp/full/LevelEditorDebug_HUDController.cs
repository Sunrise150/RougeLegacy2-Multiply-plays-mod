using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelEditorDebug_HUDController : MonoBehaviour
{
	[SerializeField]
	private GameObject m_panel;

	[SerializeField]
	private Toggle m_cameraConstrainerToggle;

	[SerializeField]
	private TMP_Dropdown m_dropdown;

	[SerializeField]
	private TMP_Dropdown m_transitionPointDropdown;

	[SerializeField]
	private TextMeshProUGUI m_cycleBiomeHotKey;

	[SerializeField]
	private TextMeshProUGUI m_cycleCloseDoorHotKey;

	[SerializeField]
	private TextMeshProUGUI m_cycleCloseAllDoorsHotKey;

	[SerializeField]
	private TextMeshProUGUI m_flipRoom;

	[SerializeField]
	private TextMeshProUGUI m_randomizeHazardsHotKey;

	[SerializeField]
	private TextMeshProUGUI m_difficultyHotKey;

	private Dictionary<int, BiomeType> m_indexToBiomeTable;

	private Dictionary<int, BiomeType> m_indexToTransitionPointBiomeTable = new Dictionary<int, BiomeType> { 
	{
		0,
		BiomeType.None
	} };

	private bool m_isDropdownInitialised;

	private void Start()
	{
		m_panel.SetActive(value: false);
		if (!Application.isEditor || !GameUtility.IsInLevelEditor)
		{
			base.enabled = false;
		}
	}

	private void OnWorldSetup(object sender, EventArgs e)
	{
		m_isDropdownInitialised = false;
		int num = 0;
		int valueWithoutNotify = 0;
		foreach (KeyValuePair<int, BiomeType> item in m_indexToBiomeTable)
		{
			if (item.Value == OnPlayManager.CurrentBiome)
			{
				valueWithoutNotify = num;
			}
			num++;
		}
		m_dropdown.SetValueWithoutNotify(valueWithoutNotify);
		num = 0;
		int valueWithoutNotify2 = 0;
		foreach (KeyValuePair<int, BiomeType> item2 in m_indexToTransitionPointBiomeTable)
		{
			if (item2.Value == OnPlayManager.TransitionPointBiome)
			{
				valueWithoutNotify2 = num;
			}
			num++;
		}
		m_transitionPointDropdown.SetValueWithoutNotify(valueWithoutNotify2);
		m_isDropdownInitialised = true;
	}

	private void Update()
	{
		if (Input.GetKeyDown("`"))
		{
			TogglePanel();
		}
	}

	public void SetBiome(int selectionIndex)
	{
		_ = m_isDropdownInitialised;
	}

	public void SetTransitionPointBiome(int selectionIndex)
	{
		_ = m_isDropdownInitialised;
	}

	private void SetHotKeyText()
	{
	}

	public void ToggleCameraConstrainerIsEnabled()
	{
	}

	private void TogglePanel()
	{
		m_panel.SetActive(!m_panel.activeInHierarchy);
	}
}
