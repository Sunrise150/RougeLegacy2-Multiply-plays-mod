using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class CastleBossEntranceTorchAudioController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	private StudioEventEmitter m_flameOnAudioEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_flameOffAudioEventEmitter;

	private string m_description = string.Empty;

	private EventInstance m_torchAudioEventInstance;

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
		GetComponent<TorchesRoomPropController>().FlameStateChangeRelay.AddListener(OnStateChange);
	}

	private void OnDisable()
	{
		m_flameOnAudioEventEmitter.Stop();
	}

	private void OnStateChange(bool isFlameOn)
	{
		if (isFlameOn)
		{
			m_flameOnAudioEventEmitter.Play();
			return;
		}
		m_flameOnAudioEventEmitter.Stop();
		m_flameOffAudioEventEmitter.Play();
	}
}
