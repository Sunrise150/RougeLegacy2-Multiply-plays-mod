using RLAudio;
using UnityEngine;

public class FireworksAudioController : MonoBehaviour
{
	private ParticleSystem m_partSys;

	private int m_particleCount;

	private void Awake()
	{
		m_partSys = GetComponent<ParticleSystem>();
	}

	private void OnEnable()
	{
		m_particleCount = m_partSys.particleCount;
	}

	private void FixedUpdate()
	{
		if (m_particleCount != m_partSys.particleCount)
		{
			if (m_partSys.particleCount < m_particleCount)
			{
				AudioManager.Play(null, "event:/SFX/Interactables/sfx_env_prop_fireworks_explode", base.transform.position);
			}
			if (m_partSys.particleCount > m_particleCount)
			{
				AudioManager.Play(null, "event:/SFX/Interactables/sfx_env_prop_fireworks_launch", base.transform.position);
			}
			m_particleCount = m_partSys.particleCount;
		}
	}
}
