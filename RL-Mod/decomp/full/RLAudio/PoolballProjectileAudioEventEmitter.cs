using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class PoolballProjectileAudioEventEmitter : ProjectileEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_bounceEventPath;

	private EventInstance m_bounceEventInstance;

	private ProjectileDamageAudioData m_damageAudioData;

	private const string POOL_BALL_PARAMETER = "poolBallSpeed";

	protected override void Awake()
	{
		base.Awake();
		base.gameObject.GetComponent<PoolBallProjectileLogic>().RicochetRelay.AddListener(OnRicochet);
		m_bounceEventInstance = AudioUtility.GetEventInstance(m_bounceEventPath, base.transform);
		m_damageAudioData = GetComponent<ProjectileDamageAudioData>();
	}

	protected override void OnDisable()
	{
		if (m_lifeTimeEventInstance.isValid())
		{
			m_lifeTimeEventInstance.setParameterByName("fireToggle", 0f);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_bounceEventInstance.isValid())
		{
			m_bounceEventInstance.release();
		}
	}

	private void OnRicochet(int bounceCount)
	{
		if (m_bounceEventInstance.isValid())
		{
			if (bounceCount == 1 && m_lifeTimeEventInstance.isValid())
			{
				m_lifeTimeEventInstance.setParameterByName("fireToggle", 1f);
				m_damageAudioData.AddHitAudioParameter("poolBallSpeed", 1f);
			}
			int num = 3;
			float value = (float)bounceCount / (float)num;
			m_bounceEventInstance.setParameterByName("poolBallSpeed", value);
			AudioManager.PlayAttached(this, m_bounceEventInstance, base.gameObject);
		}
	}
}
