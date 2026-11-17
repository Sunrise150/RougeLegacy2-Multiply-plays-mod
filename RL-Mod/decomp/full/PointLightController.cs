using System;
using UnityEngine;

public class PointLightController : MonoBehaviour
{
	[SerializeField]
	private PointLightLocation m_location;

	public const float DEFAULT_FOREGROUND_Z_POSITION = 0f;

	public const float DEFAULT_BACKGROUND_Z_POSITION = 1f;

	private Light m_light;

	private Action<MonoBehaviour, EventArgs> m_onQualitySettingsChanged;

	public PointLightLocation Location
	{
		get
		{
			return m_location;
		}
		private set
		{
			m_location = value;
		}
	}

	private void Awake()
	{
		m_light = GetComponent<Light>();
		m_onQualitySettingsChanged = OnQualitySettingsChanged;
		if ((bool)m_light && m_light.type != LightType.Point)
		{
			Debug.LogFormat("<color=red>| {0} | Light is not a Point Light. If you see this message, please add a bug report to Pivotal.</color>", this);
		}
	}

	private void OnEnable()
	{
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.QualitySettingsChanged, m_onQualitySettingsChanged);
		OnQualitySettingsChanged(null, null);
	}

	private void OnDisable()
	{
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.QualitySettingsChanged, m_onQualitySettingsChanged);
	}

	private void OnQualitySettingsChanged(object sender, EventArgs args)
	{
		if ((bool)m_light)
		{
			if (SaveManager.ConfigData.QualitySetting == 0)
			{
				m_light.enabled = false;
			}
			else
			{
				m_light.enabled = true;
			}
		}
	}

	public void UpdateLocation(CameraLayer cameraLayer)
	{
		if ((bool)m_light)
		{
			float z = 0f;
			if (m_location == PointLightLocation.Back)
			{
				z = 1f;
			}
			Vector3 position = m_light.transform.position;
			position.z = z;
			m_light.transform.position = position;
		}
	}

	public void SetLocation(PointLightLocation location)
	{
		Location = location;
	}
}
