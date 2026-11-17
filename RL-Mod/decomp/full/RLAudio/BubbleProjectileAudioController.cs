using FMOD.Studio;
using UnityEngine;

namespace RLAudio;

public class BubbleProjectileAudioController : MonoBehaviour, IAudioEventEmitter
{
	private EventInstance m_spawnEventInstance;

	private EventInstance m_popEventInstance;

	public string Description => "Bubble Projectile Audio Controller";

	public void PlaySpawnAudio(float pitch)
	{
		if (!m_spawnEventInstance.isValid())
		{
			m_spawnEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_blobfish_bubble_spawn", base.transform);
		}
		if (!m_popEventInstance.isValid())
		{
			m_popEventInstance = AudioUtility.GetEventInstance("event:/SFX/Enemies/sfx_enemy_blobfish_bubble_pop", base.transform);
		}
		m_spawnEventInstance.setParameterByName("enemyBlobfishBubbleSpawn", pitch);
		m_popEventInstance.setParameterByName("enemyBlobfishBubbleSpawn", pitch);
		AudioManager.Play(this, m_spawnEventInstance);
	}

	private void OnDisable()
	{
		AudioManager.Play(this, m_popEventInstance);
	}

	private void OnDestroy()
	{
		if (m_spawnEventInstance.isValid())
		{
			m_spawnEventInstance.release();
		}
		if (m_popEventInstance.isValid())
		{
			m_popEventInstance.release();
		}
	}
}
