using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class TouchAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_touchEnterAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_touchExitAudioPath = string.Empty;

	[SerializeField]
	[EventRef]
	private string m_touchStaySnapshotPath = string.Empty;

	private string m_description = string.Empty;

	private EventInstance m_snapshotEventInstance;

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

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (CollisionType_RL.IsProjectile(collision.gameObject) || !collision.GetRoot().CompareTag("Player"))
		{
			return;
		}
		Vector2 vector = PlayerManager.GetPlayerController().transform.position;
		AudioManager.PlayOneShot(this, m_touchEnterAudioPath, vector);
		if (!string.IsNullOrEmpty(m_touchStaySnapshotPath))
		{
			if (!m_snapshotEventInstance.isValid())
			{
				m_snapshotEventInstance = AudioUtility.GetEventInstance(m_touchStaySnapshotPath, base.transform);
			}
			AmbientSoundController.SetSnapshotOverride(m_snapshotEventInstance);
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!CollisionType_RL.IsProjectile(collision.gameObject) && collision.GetRoot().CompareTag("Player"))
		{
			Vector2 vector = PlayerManager.GetPlayerController().transform.position;
			AudioManager.PlayOneShot(this, m_touchExitAudioPath, vector);
			if (!string.IsNullOrEmpty(m_touchStaySnapshotPath))
			{
				AmbientSoundController.ClearSnapshotOverride();
			}
		}
	}

	private void OnDestroy()
	{
		if (m_snapshotEventInstance.isValid())
		{
			m_snapshotEventInstance.release();
		}
	}
}
