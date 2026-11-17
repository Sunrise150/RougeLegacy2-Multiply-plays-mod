using System;
using UnityEngine;

public class GameObjectQualityController : MonoBehaviour
{
	[SerializeField]
	private QualitySetting m_qualitySetting;

	[SerializeField]
	[Tooltip("Resets the enabled states for the GameObjects if you are NOT running on the specified quality setting")]
	private bool m_resetGOStatesOnOtherSettings = true;

	[SerializeField]
	private bool m_enableForAllLowerQualitySettings;

	[SerializeField]
	private GameObject[] m_gosToDisable;

	[SerializeField]
	private GameObject[] m_gosToEnable;

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
		if (SaveManager.ConfigData.QualitySetting == (int)m_qualitySetting || (m_enableForAllLowerQualitySettings && SaveManager.ConfigData.QualitySetting <= (int)m_qualitySetting))
		{
			GameObject[] gosToDisable = m_gosToDisable;
			foreach (GameObject gameObject in gosToDisable)
			{
				if ((bool)gameObject && gameObject.activeSelf)
				{
					gameObject.SetActive(value: false);
				}
			}
			gosToDisable = m_gosToEnable;
			foreach (GameObject gameObject2 in gosToDisable)
			{
				if ((bool)gameObject2 && !gameObject2.activeSelf)
				{
					gameObject2.SetActive(value: true);
				}
			}
		}
		else
		{
			if (!m_resetGOStatesOnOtherSettings)
			{
				return;
			}
			GameObject[] gosToDisable = m_gosToDisable;
			foreach (GameObject gameObject3 in gosToDisable)
			{
				if ((bool)gameObject3 && !gameObject3.activeSelf)
				{
					gameObject3.SetActive(value: true);
				}
			}
			gosToDisable = m_gosToEnable;
			foreach (GameObject gameObject4 in gosToDisable)
			{
				if ((bool)gameObject4 && gameObject4.activeSelf)
				{
					gameObject4.SetActive(value: false);
				}
			}
		}
	}
}
