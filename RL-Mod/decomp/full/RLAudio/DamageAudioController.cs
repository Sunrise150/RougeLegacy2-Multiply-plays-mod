using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;
using UnityEngine.Serialization;

namespace RLAudio;

public class DamageAudioController : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	private bool m_isCharacter;

	[SerializeField]
	private StudioEventEmitter m_immuneAudioEvent;

	[SerializeField]
	private StudioEventEmitter m_critAudioEvent;

	[SerializeField]
	[FormerlySerializedAs("m_additionalAudioEvents")]
	private StudioEventEmitter[] m_additionalHitAudioEvents;

	[SerializeField]
	private StudioEventEmitter[] m_deathAudioEvents;

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

	protected virtual void Awake()
	{
		GetComponent<IEffectTriggerEvent_OnDamage>()?.OnDamageEffectTriggerRelay.AddListener(OnTakeDamage);
		GetComponent<IEffectTriggerEvent_OnDeath>()?.OnDeathEffectTriggerRelay.AddListener(OnDeath);
	}

	protected virtual void OnDestroy()
	{
		GetComponent<IEffectTriggerEvent_OnDamage>()?.OnDamageEffectTriggerRelay.RemoveListener(OnTakeDamage);
		GetComponent<IEffectTriggerEvent_OnDeath>()?.OnDeathEffectTriggerRelay.RemoveListener(OnDeath);
	}

	protected virtual void OnTakeDamage(GameObject attacker, float damageTaken, bool isCrit)
	{
		BaseDamageAudioData component = attacker.GetComponent<BaseDamageAudioData>();
		if (!component.IsNativeNull())
		{
			string text = component.BreakableDamageAudioPath;
			if (m_isCharacter)
			{
				text = component.CharacterDamageAudioPath;
			}
			if (!string.IsNullOrEmpty(text) && component.HitAudioParameters != null)
			{
				EventInstance eventInstance = AudioUtility.GetEventInstance(text, attacker.transform);
				foreach (KeyValuePair<string, float> hitAudioParameter in component.HitAudioParameters)
				{
					eventInstance.setParameterByName(hitAudioParameter.Key, hitAudioParameter.Value);
				}
				AudioManager.PlayAttached(this, eventInstance, attacker);
				eventInstance.release();
			}
			else if (!string.IsNullOrEmpty(text))
			{
				AudioManager.PlayOneShotAttached(this, text, base.gameObject);
			}
		}
		for (int i = 0; i < m_additionalHitAudioEvents.Length; i++)
		{
			if (!string.IsNullOrEmpty(m_additionalHitAudioEvents[i].Event))
			{
				AudioManager.Play(this, m_additionalHitAudioEvents[i]);
			}
		}
		if (isCrit && (bool)m_critAudioEvent)
		{
			AudioManager.Play(this, m_critAudioEvent);
		}
		if (damageTaken <= 0f && (bool)m_immuneAudioEvent && attacker.GetComponent<IDamageObj>().ActualDamage > 0f)
		{
			AudioManager.Play(this, m_immuneAudioEvent);
		}
	}

	protected virtual void OnDeath(GameObject otherObj)
	{
		for (int i = 0; i < m_deathAudioEvents.Length; i++)
		{
			if (!string.IsNullOrEmpty(m_deathAudioEvents[i].Event))
			{
				AudioManager.Play(this, m_deathAudioEvents[i]);
			}
		}
	}
}
