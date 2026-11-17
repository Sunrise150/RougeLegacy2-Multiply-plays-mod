using System.Collections;
using FMODUnity;
using UnityEngine;

public class PlayAudioAtLifeSpanProjectileLogic : BaseProjectileLogic
{
	[SerializeField]
	private float m_triggerAtNormalizedLifeSpan = 0.8f;

	[SerializeField]
	private StudioEventEmitter m_eventEmitter;

	private void OnEnable()
	{
		if ((bool)base.SourceProjectile)
		{
			StartCoroutine(PlayAudioCoroutine());
		}
	}

	private IEnumerator PlayAudioCoroutine()
	{
		float num = base.SourceProjectile.Lifespan * m_triggerAtNormalizedLifeSpan;
		float endTime = Time.time + num;
		while (Time.time < endTime)
		{
			yield return null;
		}
		m_eventEmitter.Play();
	}
}
