using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class SpearSpinAbilityAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	private string m_spinStartAudioPath;

	[SerializeField]
	[EventRef]
	private string m_singleProjectileHitAudioPath;

	[SerializeField]
	[EventRef]
	private string m_multipleProjectilesHitAudioPath;

	[SerializeField]
	[EventRef]
	private string m_abilitySuccessAudioPath;

	[SerializeField]
	[EventRef]
	private string m_abilityFailAudioPath;

	[SerializeField]
	private bool m_playSuccessFailAudio = true;

	[SerializeField]
	private float m_timeBetweenProjectileHits = 0.1f;

	private float m_timeOfProjectileLastHit;

	private string m_description = string.Empty;

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
		SpearSpin_Ability component = GetComponent<SpearSpin_Ability>();
		component.BeginCastingRelay.AddListener(OnSpearSpinStart);
		component.HitProjectileRelay.AddListener(OnProjectileHit);
		component.SpinCompleteRelay.AddListener(OnSpinComplete);
	}

	private void OnDestroy()
	{
		SpearSpin_Ability component = GetComponent<SpearSpin_Ability>();
		component.BeginCastingRelay.RemoveListener(OnSpearSpinStart);
		component.HitProjectileRelay.RemoveListener(OnProjectileHit);
		component.SpinCompleteRelay.RemoveListener(OnSpinComplete);
	}

	private void OnProjectileHit()
	{
		float time = Time.time;
		string path = m_singleProjectileHitAudioPath;
		if (m_timeOfProjectileLastHit - time <= m_timeBetweenProjectileHits)
		{
			path = m_multipleProjectilesHitAudioPath;
		}
		AudioManager.PlayOneShotAttached(this, path, base.gameObject);
		m_timeOfProjectileLastHit = time;
	}

	private void OnSpearSpinStart()
	{
		AudioManager.PlayOneShotAttached(this, m_spinStartAudioPath, base.gameObject);
	}

	private void OnSpinComplete(bool hitProjectileWhileActive)
	{
		if (m_playSuccessFailAudio)
		{
			string path = m_abilityFailAudioPath;
			if (hitProjectileWhileActive)
			{
				path = m_abilitySuccessAudioPath;
			}
			AudioManager.PlayOneShotAttached(this, path, base.gameObject);
		}
	}
}
