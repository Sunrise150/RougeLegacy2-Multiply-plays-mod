using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class ShoutAbilityAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_shoutAudioPath;

	private string m_description = string.Empty;

	private EventInstance m_shoutEventInstance;

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
		GetComponent<Shout_Ability>().OnShoutEvent.AddListener(OnShout);
		m_shoutEventInstance = AudioUtility.GetEventInstance(m_shoutAudioPath, base.transform);
	}

	private void OnDestroy()
	{
		if (m_shoutEventInstance.isValid())
		{
			m_shoutEventInstance.release();
		}
	}

	private void OnShout(GameObject playerGameObject)
	{
		float playerGenderAudioParameterValue = AudioUtility.GetPlayerGenderAudioParameterValue();
		float playerSizeAudioParameterValue = AudioUtility.GetPlayerSizeAudioParameterValue();
		m_shoutEventInstance.setParameterByName("gender", playerGenderAudioParameterValue);
		m_shoutEventInstance.setParameterByName("Player_Size", playerSizeAudioParameterValue);
		AudioManager.PlayAttached(this, m_shoutEventInstance, base.gameObject);
	}
}
