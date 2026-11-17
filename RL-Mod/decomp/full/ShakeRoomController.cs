using System.Collections;
using FMOD.Studio;
using RLAudio;
using UnityEngine;

public class ShakeRoomController : MonoBehaviour
{
	[SerializeField]
	private Vector2 m_shakeDuration = new Vector2(0.5f, 1f);

	[SerializeField]
	private Vector2 m_shakeInterval = new Vector2(1f, 2f);

	private EventInstance m_debrisSFXEventInstance;

	private EventInstance m_rumbleEventInstance;

	private BaseEffect m_shakeEffect;

	private void OnEnable()
	{
		if (!m_debrisSFXEventInstance.isValid())
		{
			m_debrisSFXEventInstance = AudioUtility.GetEventInstance("event:/UI/FrontEnd/ui_fe_story_castleShake_debris_loop", base.transform);
		}
		if (!m_rumbleEventInstance.isValid())
		{
			m_rumbleEventInstance = AudioUtility.GetEventInstance("event:/UI/FrontEnd/ui_fe_story_castleShake_rumble_loop", base.transform);
		}
		StartCoroutine(ShakeRoomCoroutine());
		StartCoroutine(StartRumbleSFX());
	}

	private IEnumerator StartRumbleSFX()
	{
		float delay = 0.1f + Time.time;
		while (Time.time < delay)
		{
			yield return null;
		}
		if (m_rumbleEventInstance.isValid())
		{
			AudioManager.Play(null, m_rumbleEventInstance);
		}
	}

	private void OnDestroy()
	{
		if (m_debrisSFXEventInstance.isValid())
		{
			m_debrisSFXEventInstance.release();
		}
		if (m_rumbleEventInstance.isValid())
		{
			m_rumbleEventInstance.release();
		}
	}

	private IEnumerator ShakeRoomCoroutine()
	{
		float delay3 = Random.Range(m_shakeInterval.x, m_shakeInterval.y);
		delay3 += Time.time;
		while (Time.time < delay3)
		{
			yield return null;
		}
		float num = Random.Range(m_shakeDuration.x, m_shakeDuration.y);
		float earthquakeDuration = num + Random.Range(0f, 1f);
		m_shakeEffect = EffectManager.PlayEffect(base.gameObject, null, "CameraShakeVerySmall_Effect", Vector2.zero, num, EffectStopType.Gracefully);
		StartCoroutine(PlayDebrisSFX(earthquakeDuration));
		delay3 = Time.time + Random.Range(0f, 0.5f);
		while (Time.time < delay3)
		{
			yield return null;
		}
		EffectManager.PlayEffect(base.gameObject, null, "Earthquake_Effect", PlayerManager.GetPlayerController().Midpoint, earthquakeDuration, EffectStopType.Gracefully);
		while (m_shakeEffect.IsPlaying)
		{
			yield return null;
		}
		StartCoroutine(ShakeRoomCoroutine());
	}

	private IEnumerator PlayDebrisSFX(float duration)
	{
		if (m_debrisSFXEventInstance.isValid())
		{
			AudioManager.Play(null, m_debrisSFXEventInstance);
		}
		duration += 1.5f;
		duration += Time.time;
		while (Time.time < duration)
		{
			yield return null;
		}
		if (m_debrisSFXEventInstance.isValid())
		{
			AudioManager.Stop(m_debrisSFXEventInstance);
		}
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_story_castleShake_debris_stop", base.transform.position);
	}

	private void OnDisable()
	{
		if ((bool)m_shakeEffect && !m_shakeEffect.IsFreePoolObj)
		{
			m_shakeEffect.Stop(EffectStopType.Immediate);
		}
		if (m_debrisSFXEventInstance.isValid())
		{
			AudioManager.Stop(m_debrisSFXEventInstance);
		}
		if (m_rumbleEventInstance.isValid())
		{
			AudioManager.Stop(m_rumbleEventInstance);
		}
		m_shakeEffect = null;
	}
}
