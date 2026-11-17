using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class WispAudioEventEmitterController : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter m_idleAudioEmitter;

	[SerializeField]
	private StudioEventEmitter m_dashAudioEmitter;

	[SerializeField]
	private StudioEventEmitter m_immuneAudioEmitter;

	private EnemyController m_enemyController;

	private const string PARAMETER_NAME = "agroState";

	private void Start()
	{
		m_enemyController = GetComponent<EnemyController>();
	}

	private void OnEnable()
	{
		if ((bool)m_idleAudioEmitter)
		{
			m_idleAudioEmitter.Play();
		}
	}

	private void Update()
	{
		if ((bool)m_idleAudioEmitter)
		{
			bool flag = m_enemyController.FollowTarget && m_enemyController.FlyingMovementType == FlyingMovementType.Towards;
			m_idleAudioEmitter.SetParameter("agroState", flag ? 1f : 0f);
		}
	}

	private void OnDisable()
	{
		if ((bool)m_dashAudioEmitter)
		{
			m_dashAudioEmitter.Stop();
		}
		if ((bool)m_idleAudioEmitter)
		{
			m_idleAudioEmitter.Stop();
		}
	}

	public void PlayDash()
	{
		if ((bool)m_dashAudioEmitter)
		{
			m_dashAudioEmitter.Play();
		}
	}

	public void StopDash()
	{
		if ((bool)m_dashAudioEmitter)
		{
			m_dashAudioEmitter.Stop();
		}
	}

	public void PlayImmuneHit()
	{
		m_immuneAudioEmitter.Play();
	}
}
