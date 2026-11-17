using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class SentryHazardAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_playerWithinRangeAudioPath;

	[SerializeField]
	[EventRef]
	private string m_playerExitRangeAudioPath;

	private string m_description = string.Empty;

	private EventInstance m_playerWithinRangeEventInstance;

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		GetComponent<Sentry_Hazard>().PlayerWithinRangeChangeRelay.AddListener(OnPlayerWithinRangeChange);
		m_playerWithinRangeEventInstance = AudioUtility.GetEventInstance(m_playerWithinRangeAudioPath, base.transform);
	}

	private void OnDisable()
	{
		if (m_playerWithinRangeEventInstance.isValid())
		{
			AudioManager.Stop(m_playerWithinRangeEventInstance);
		}
	}

	private void OnDestroy()
	{
		if (m_playerWithinRangeEventInstance.isValid())
		{
			m_playerWithinRangeEventInstance.release();
		}
	}

	private void OnPlayerWithinRangeChange(bool isPlayerWithinRange)
	{
		if (isPlayerWithinRange)
		{
			AudioManager.PlayAttached(this, m_playerWithinRangeEventInstance, base.gameObject);
			return;
		}
		AudioManager.Stop(m_playerWithinRangeEventInstance);
		AudioManager.PlayOneShotAttached(this, m_playerExitRangeAudioPath, base.gameObject);
	}
}
