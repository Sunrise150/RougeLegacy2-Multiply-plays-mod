using System.Collections;
using RLAudio;
using UnityEngine;

public class WispEnemyHitResponse : EnemyHitResponse
{
	[SerializeField]
	private GameObject m_sprite;

	private const float m_blinkInterval = 0.05f;

	private Coroutine m_blinkCoroutine;

	private WispAudioEventEmitterController m_audioEventController;

	private void OnEnable()
	{
		m_sprite.SetActive(value: true);
	}

	public override void StartHitResponse(GameObject otherRootGameObj, IDamageObj damageObj, float damageOverride = -1f, bool trueDamage = false, bool fireEvents = true)
	{
		if (damageObj is DownstrikeProjectile_RL)
		{
			base.StartHitResponse(otherRootGameObj, damageObj, damageOverride, trueDamage, fireEvents);
			return;
		}
		if (m_blinkCoroutine != null)
		{
			StopCoroutine(m_blinkCoroutine);
		}
		m_blinkCoroutine = StartCoroutine(BlinkCoroutine(0.3f));
		if (!m_audioEventController)
		{
			m_audioEventController = GetComponent<WispAudioEventEmitterController>();
		}
		m_audioEventController.PlayImmuneHit();
	}

	private IEnumerator BlinkCoroutine(float duration)
	{
		m_sprite.SetActive(value: false);
		float timer = Time.time + duration;
		float intervalCounter = Time.time + 0.05f;
		while (Time.time < timer)
		{
			if (Time.time >= intervalCounter)
			{
				intervalCounter = Time.time + 0.05f;
				if (m_sprite.activeSelf)
				{
					m_sprite.SetActive(value: false);
				}
				else
				{
					m_sprite.SetActive(value: true);
				}
			}
			yield return null;
		}
		m_sprite.SetActive(value: true);
	}
}
