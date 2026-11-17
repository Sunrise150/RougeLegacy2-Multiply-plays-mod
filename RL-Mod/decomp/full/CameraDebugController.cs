using System.Collections.Generic;
using RL_Windows;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraDebugController : WindowController
{
	[SerializeField]
	private Toggle m_backgroundPersp;

	[SerializeField]
	private Toggle m_backgroundOrtho;

	[SerializeField]
	private Toggle m_game;

	[SerializeField]
	private Toggle m_foregroundOrtho;

	[SerializeField]
	private Toggle m_foregroundPersp;

	[SerializeField]
	private Toggle m_character;

	[SerializeField]
	private Toggle m_ui;

	[SerializeField]
	private Toggle m_traitMask;

	[SerializeField]
	private Toggle m_map;

	[SerializeField]
	private Toggle m_letterbox;

	[SerializeField]
	private Toggle m_weather;

	[SerializeField]
	private Toggle m_sky;

	[SerializeField]
	private TMP_Dropdown m_layerDropdown;

	[SerializeField]
	private Toggle m_enableProps;

	[SerializeField]
	private Toggle m_enableTerrain;

	[SerializeField]
	private Button m_executeButton;

	private List<GameObject> m_activeSkies = new List<GameObject>();

	private List<GameObject> m_activeWeather = new List<GameObject>();

	private bool m_isTerrainEnabled = true;

	private bool m_arePropsEnabled = true;

	private int m_selectedLayerIndex;

	public override WindowID ID => WindowID.CameraDebug;

	private void Start()
	{
		m_backgroundPersp.onValueChanged.AddListener(OnBackgroundPerspToggled);
		m_backgroundOrtho.onValueChanged.AddListener(OnBackgroundOrthoToggled);
		m_game.onValueChanged.AddListener(OnGameToggled);
		m_foregroundOrtho.onValueChanged.AddListener(OnForegroundOrthoToggled);
		m_foregroundPersp.onValueChanged.AddListener(OnForegroundPerspToggled);
		m_character.onValueChanged.AddListener(OnCharacterToggled);
		m_ui.onValueChanged.AddListener(OnUIToggled);
		m_traitMask.onValueChanged.AddListener(OnTraitMaskToggled);
		m_map.onValueChanged.AddListener(OnMapToggled);
		m_letterbox.onValueChanged.AddListener(OnLetterboxToggled);
		m_weather.onValueChanged.AddListener(OnWeatherToggled);
		m_sky.onValueChanged.AddListener(OnSkyToggled);
		m_enableProps.onValueChanged.AddListener(OnPropsToggled);
		m_enableTerrain.onValueChanged.AddListener(OnTerrainToggled);
		m_executeButton.onClick.AddListener(OnExecuteButtonPressed);
		m_layerDropdown.onValueChanged.AddListener(OnDropdownChanged);
	}

	protected override void OnOpen()
	{
		m_windowCanvas.gameObject.SetActive(value: true);
		m_backgroundPersp.Select();
	}

	protected override void OnClose()
	{
		m_windowCanvas.gameObject.SetActive(value: false);
	}

	protected override void OnFocus()
	{
		base.RewiredPlayer.AddInputEventDelegate(OnCancelPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	private void OnCancelPressed(InputActionEventData obj)
	{
		WindowManager.SetWindowIsOpen(ID, isOpen: false);
	}

	protected override void OnLostFocus()
	{
		base.RewiredPlayer.RemoveInputEventDelegate(OnCancelPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
	}

	private void OnSkyToggled(bool isOn)
	{
		if (isOn)
		{
			for (int i = 0; i < m_activeSkies.Count; i++)
			{
				m_activeSkies[i].SetActive(value: true);
			}
			m_activeSkies.Clear();
			return;
		}
		Sky[] array = Object.FindObjectsOfType<Sky>();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].gameObject.activeInHierarchy)
			{
				m_activeSkies.Add(array[j].gameObject);
				array[j].gameObject.SetActive(value: false);
			}
		}
	}

	private void OnWeatherToggled(bool isOn)
	{
		if (isOn)
		{
			for (int i = 0; i < m_activeWeather.Count; i++)
			{
				m_activeWeather[i].SetActive(value: true);
			}
			m_activeWeather.Clear();
			return;
		}
		Weather[] array = Object.FindObjectsOfType<Weather>();
		for (int j = 0; j < array.Length; j++)
		{
			if (array[j].gameObject.activeInHierarchy)
			{
				m_activeWeather.Add(array[j].gameObject);
				array[j].gameObject.SetActive(value: false);
			}
		}
	}

	private void OnBackgroundPerspToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(0, isOn);
	}

	private void OnBackgroundOrthoToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(1, isOn);
	}

	private void OnGameToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(2, isOn);
	}

	private void OnForegroundOrthoToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(3, isOn);
	}

	private void OnForegroundPerspToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(4, isOn);
	}

	private void OnCharacterToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(5, isOn);
	}

	private void OnUIToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(6, isOn);
	}

	private void OnTraitMaskToggled(bool isOn)
	{
		CameraController.SetCameraEnabled(7, isOn);
	}

	private void OnLetterboxToggled(bool isOn)
	{
	}

	private void OnMapToggled(bool isOn)
	{
		if (MapController.IsInitialized && MapController.Camera != null)
		{
			MapController.Camera.enabled = isOn;
		}
	}

	private void OnDropdownChanged(int index)
	{
		m_selectedLayerIndex = index;
	}

	private void OnExecuteButtonPressed()
	{
		CameraLayer cameraLayer = CameraLayer.None;
		switch (m_selectedLayerIndex)
		{
		case 0:
			cameraLayer = CameraLayer.Background_Far_PERSP;
			break;
		case 1:
			cameraLayer = CameraLayer.Background_Near_PERSP;
			break;
		case 2:
			cameraLayer = CameraLayer.Background_ORTHO;
			break;
		case 3:
			cameraLayer = CameraLayer.Game;
			break;
		case 4:
			cameraLayer = CameraLayer.Foreground_ORTHO;
			break;
		case 5:
			cameraLayer = CameraLayer.Foreground_PERSP;
			break;
		}
		if (cameraLayer == CameraLayer.None)
		{
			return;
		}
		PropSpawnController[] array = Object.FindObjectsOfType<PropSpawnController>();
		for (int i = 0; i < array.Length; i++)
		{
			if (!(array[i].PropInstance != null))
			{
				continue;
			}
			Renderer[] componentsInChildren = array[i].PropInstance.GetComponentsInChildren<Renderer>();
			for (int j = 0; j < componentsInChildren.Length; j++)
			{
				if (componentsInChildren[j].gameObject.layer == (int)CameraLayerUtility.GetLayer(cameraLayer))
				{
					componentsInChildren[j].enabled = m_arePropsEnabled;
				}
			}
		}
		Ferr2DT_PathTerrain[] array2 = Object.FindObjectsOfType<Ferr2DT_PathTerrain>();
		for (int k = 0; k < array2.Length; k++)
		{
			if (!array2[k].DisableMerging && !array2[k].name.Contains("Deco"))
			{
				continue;
			}
			if (cameraLayer == CameraLayer.Game)
			{
				if (!LayerMask.LayerToName(array2[k].gameObject.layer).Contains("Collides"))
				{
					continue;
				}
			}
			else if (array2[k].gameObject.layer != (int)CameraLayerUtility.GetLayer(cameraLayer))
			{
				continue;
			}
			Renderer componentInChildren = array2[k].GetComponentInChildren<Renderer>();
			if (componentInChildren != null)
			{
				componentInChildren.enabled = m_isTerrainEnabled;
			}
		}
	}

	private void OnTerrainToggled(bool isOn)
	{
		m_isTerrainEnabled = isOn;
	}

	private void OnPropsToggled(bool isOn)
	{
		m_arePropsEnabled = isOn;
	}
}
