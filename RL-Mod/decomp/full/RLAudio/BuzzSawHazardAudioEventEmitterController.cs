using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class BuzzSawHazardAudioEventEmitterController : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter m_sawAudioEmitter;

	private Buzzsaw_Hazard m_saw;

	private const string PARAMETER_NAME = "sawPosition";

	private void Start()
	{
		m_saw = GetComponent<Buzzsaw_Hazard>();
	}

	private void FixedUpdate()
	{
		m_sawAudioEmitter.SetParameter("sawPosition", m_saw.SawPosition);
	}
}
