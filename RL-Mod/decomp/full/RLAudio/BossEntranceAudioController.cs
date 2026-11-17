using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class BossEntranceAudioController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	protected string m_doorOpenedAudioPath;

	[SerializeField]
	[EventRef]
	protected string m_doorDestroyedAudioPath;

	private string m_description = string.Empty;

	protected BossEntranceRoomController m_bossEntranceRoomController;

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

	protected virtual void Awake()
	{
		m_bossEntranceRoomController = GetComponent<BossEntranceRoomController>();
		m_bossEntranceRoomController.DoorPartlyOpenedRelay.AddListener(OnDoorOpened);
		m_bossEntranceRoomController.DoorDestroyedRelay.AddListener(OnDoorDestroyed);
	}

	protected virtual void OnDoorOpened(bool setOnEnter)
	{
		if (!setOnEnter)
		{
			AudioManager.PlayOneShot(this, m_doorOpenedAudioPath);
		}
	}

	protected virtual void OnDoorDestroyed(bool setOnEnter)
	{
		if (!setOnEnter)
		{
			AudioManager.PlayOneShot(this, m_doorDestroyedAudioPath);
		}
	}
}
