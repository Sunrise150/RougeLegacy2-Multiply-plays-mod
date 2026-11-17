using System;
using UnityEngine;

public class GeoQualityController : MonoBehaviour
{
	[SerializeField]
	private QualitySetting m_qualitySetting;

	[SerializeField]
	[Tooltip("Resets the enabled states for the GameObjects if you are NOT running on the specified quality setting")]
	private bool m_resetGOStatesOnOtherSettings = true;

	[SerializeField]
	private bool m_enableForAllLowerQualitySettings;

	[SerializeField]
	private MeshFilter[] m_geoToChange;

	[SerializeField]
	private MeshFilter m_originalGeo;

	[SerializeField]
	private MeshFilter m_geoToChangeTo;

	private bool m_usingChangedGeo;

	private Action<MonoBehaviour, EventArgs> m_onQualityChanged;

	private void Awake()
	{
		m_onQualityChanged = OnQualityChanged;
	}

	private void OnEnable()
	{
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.QualitySettingsChanged, m_onQualityChanged);
		OnQualityChanged(null, null);
	}

	private void OnDisable()
	{
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.QualitySettingsChanged, m_onQualityChanged);
	}

	private void OnQualityChanged(object sender, EventArgs args)
	{
		if (m_geoToChange == null)
		{
			return;
		}
		if (SaveManager.ConfigData.QualitySetting == (int)m_qualitySetting || (m_enableForAllLowerQualitySettings && SaveManager.ConfigData.QualitySetting <= (int)m_qualitySetting))
		{
			if (!m_usingChangedGeo)
			{
				m_usingChangedGeo = true;
				MeshFilter[] geoToChange = m_geoToChange;
				for (int i = 0; i < geoToChange.Length; i++)
				{
					geoToChange[i].sharedMesh = m_geoToChangeTo.sharedMesh;
				}
			}
		}
		else if (m_resetGOStatesOnOtherSettings && m_usingChangedGeo)
		{
			m_usingChangedGeo = false;
			MeshFilter[] geoToChange = m_geoToChange;
			for (int i = 0; i < geoToChange.Length; i++)
			{
				geoToChange[i].sharedMesh = m_originalGeo.sharedMesh;
			}
		}
	}
}
