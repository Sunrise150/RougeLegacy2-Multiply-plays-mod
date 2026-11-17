using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class CloudHazardAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_playerHitAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_teleportStartAudioEventPath;

	private Cloud m_cloud;

	private string m_description = string.Empty;

	private Vector2 m_playerHitPosition;

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
		m_cloud = base.transform.GetComponent<Cloud>();
		if (m_cloud == null)
		{
			throw new MissingComponentException("Cloud");
		}
		m_cloud.PlayerHitRelay.AddListener(OnPlayerHit);
		m_cloud.TeleportStartRelay.AddListener(OnTeleportStart);
	}

	private void OnPlayerHit(Vector2 position)
	{
		m_playerHitPosition = position;
		AudioManager.PlayOneShot(this, m_playerHitAudioEventPath, position);
	}

	private void OnTeleportStart()
	{
		AudioManager.PlayOneShot(this, m_teleportStartAudioEventPath, m_playerHitPosition);
	}
}
