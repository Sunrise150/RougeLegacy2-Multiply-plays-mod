using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class OrbiterHazardAudioEventEmitterController : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter m_expansionAudioEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_expansionCompleteAudioEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_swingAudioEventEmitter;

	[SerializeField]
	private StudioEventEmitter m_orbiterAudioEventEmitter;

	[SerializeField]
	private float m_maxSquareMagnitude = 100f;

	private const string EXPANSION_PARAMETER_NAME = "chainLength";

	private const int FULL_ROTATION_VALUE = 350;

	private const string ORBITER_PROXIMITY_PARAMETER_NAME = "orbiterProximity";

	private Orbiter_Hazard m_orbiter;

	private float m_swingAngle;

	private bool m_hasSwingAudioPlayed;

	private bool m_hasExpansionCompleteAudioPlayed;

	private void Awake()
	{
		m_orbiter = GetComponent<Orbiter_Hazard>();
		m_swingAngle = 90f;
	}

	private void OnEnable()
	{
		m_hasSwingAudioPlayed = false;
		m_hasExpansionCompleteAudioPlayed = false;
	}

	private void Update()
	{
		if (m_orbiter == null)
		{
			m_orbiter = GetComponent<Orbiter_Hazard>();
		}
		PlayExpansionAudio();
		PlaySwingAudio();
		PlayOrbiterProximityAudio();
	}

	private void PlayOrbiterProximityAudio()
	{
		if (PlayerManager.IsInstantiated)
		{
			Vector2 vector = PlayerManager.GetPlayerController().transform.position;
			Vector2 vector2 = m_orbiter.OrbiterBall.transform.position;
			float value = (vector - vector2).sqrMagnitude / m_maxSquareMagnitude;
			value = Mathf.Clamp(value, 0f, 1f);
			m_orbiterAudioEventEmitter.SetParameter("orbiterProximity", value);
		}
	}

	private void PlayExpansionAudio()
	{
		if (m_orbiter.ExpansionPercent < 1f)
		{
			m_expansionAudioEventEmitter.SetParameter("chainLength", m_orbiter.ExpansionPercent);
			return;
		}
		m_expansionAudioEventEmitter.Stop();
		if (!m_hasExpansionCompleteAudioPlayed)
		{
			m_hasExpansionCompleteAudioPlayed = true;
			m_expansionCompleteAudioEventEmitter.Play();
		}
	}

	private void PlaySwingAudio()
	{
		float num = m_orbiter.CurrentRotation - m_orbiter.InitialRotation;
		if (m_hasSwingAudioPlayed)
		{
			if (num <= 350f)
			{
				m_hasSwingAudioPlayed = false;
			}
		}
		else if (num > 350f)
		{
			m_hasSwingAudioPlayed = true;
			m_swingAudioEventEmitter.Play();
		}
	}
}
