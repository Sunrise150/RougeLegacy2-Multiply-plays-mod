using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using UnityEngine;

namespace RLAudio;

public class StatusEffectAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	private string m_description = string.Empty;

	private Dictionary<StatusEffectType, EventInstance> m_lifetimeAudioEventTable = new Dictionary<StatusEffectType, EventInstance>();

	private static Dictionary<StatusEffectType, string> START_AUDIO_TABLE = new Dictionary<StatusEffectType, string> { 
	{
		StatusEffectType.Enemy_SporeBurst,
		"event:/SFX/Spells/sfx_spell_sporeShot_hit"
	} };

	private static Dictionary<StatusEffectType, string> LIFETIME_AUDIO_TABLE = new Dictionary<StatusEffectType, string>
	{
		{
			StatusEffectType.Enemy_ManaBurn,
			"event:/SFX/Spells/sfx_spell_manaBurn_loop"
		},
		{
			StatusEffectType.Enemy_SporeBurst,
			"event:/SFX/Spells/sfx_spell_sporeShot_buildup"
		},
		{
			StatusEffectType.Enemy_Freeze,
			"event:/SFX/Spells/sfx_spell_iceBlast_freezeLoop"
		}
	};

	private static Dictionary<StatusEffectType, string> LIFETIME_PARAMETER_NAME_TABLE = new Dictionary<StatusEffectType, string> { 
	{
		StatusEffectType.Enemy_SporeBurst,
		"spore_buildUp"
	} };

	private static Dictionary<StatusEffectType, string> STOP_AUDIO_TABLE = new Dictionary<StatusEffectType, string>
	{
		{
			StatusEffectType.Enemy_ManaBurn,
			"event:/SFX/Spells/sfx_spell_manaBurn_end"
		},
		{
			StatusEffectType.Enemy_SporeBurst,
			"event:/SFX/Spells/sfx_spell_sporeShot_explode"
		},
		{
			StatusEffectType.Enemy_Freeze,
			"event:/SFX/Enemies/sfx_enemy_elemental_iceShard_crystalize"
		}
	};

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
		BaseStatusEffect[] components = base.gameObject.GetComponents<BaseStatusEffect>();
		foreach (BaseStatusEffect baseStatusEffect in components)
		{
			if (LIFETIME_AUDIO_TABLE.ContainsKey(baseStatusEffect.StatusEffectType))
			{
				baseStatusEffect.StartEffectRelay.AddListener(OnStatusEffectStart);
				EventInstance eventInstance = AudioUtility.GetEventInstance(LIFETIME_AUDIO_TABLE[baseStatusEffect.StatusEffectType], base.transform);
				m_lifetimeAudioEventTable.Add(baseStatusEffect.StatusEffectType, eventInstance);
			}
			if (STOP_AUDIO_TABLE.ContainsKey(baseStatusEffect.StatusEffectType))
			{
				baseStatusEffect.StopEffectRelay.AddListener(OnStatusEffectStop);
			}
		}
	}

	private void OnDestroy()
	{
		foreach (KeyValuePair<StatusEffectType, EventInstance> item in m_lifetimeAudioEventTable)
		{
			EventInstance value = item.Value;
			if (value.isValid())
			{
				value.release();
			}
		}
	}

	protected virtual void OnStatusEffectStart(BaseStatusEffect statusEffect)
	{
		if (START_AUDIO_TABLE.ContainsKey(statusEffect.StatusEffectType))
		{
			string path = START_AUDIO_TABLE[statusEffect.StatusEffectType];
			AudioManager.PlayOneShotAttached(this, path, base.gameObject);
		}
		if (m_lifetimeAudioEventTable.ContainsKey(statusEffect.StatusEffectType))
		{
			AudioManager.PlayAttached(this, m_lifetimeAudioEventTable[statusEffect.StatusEffectType], base.gameObject);
			if (LIFETIME_PARAMETER_NAME_TABLE.ContainsKey(statusEffect.StatusEffectType))
			{
				StartCoroutine(UpdateStatusEffectParameter(statusEffect));
			}
		}
	}

	protected virtual IEnumerator UpdateStatusEffectParameter(BaseStatusEffect statusEffect)
	{
		EventInstance eventInstance = m_lifetimeAudioEventTable[statusEffect.StatusEffectType];
		string parameterName = LIFETIME_PARAMETER_NAME_TABLE[statusEffect.StatusEffectType];
		if (AudioUtility.GetHasParameter(eventInstance, parameterName))
		{
			float duration = statusEffect.Duration;
			if (statusEffect.StartingDurationOverride > 0f)
			{
				duration = statusEffect.StartingDurationOverride;
			}
			if (duration > 0f)
			{
				float timeStart = Time.time;
				while (Time.time - timeStart < duration)
				{
					float value = (Time.time - timeStart) / duration;
					eventInstance.setParameterByName(parameterName, value);
					yield return null;
				}
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | The <b>{1}</b> Status Effect's audio event instance does not have a parameter named <b>{2}</b></color>", this, statusEffect.StatusEffectType, parameterName);
		}
	}

	protected virtual void OnStatusEffectStop(BaseStatusEffect statusEffect)
	{
		if (m_lifetimeAudioEventTable.ContainsKey(statusEffect.StatusEffectType))
		{
			AudioManager.Stop(m_lifetimeAudioEventTable[statusEffect.StatusEffectType]);
		}
		if (STOP_AUDIO_TABLE.ContainsKey(statusEffect.StatusEffectType))
		{
			string path = STOP_AUDIO_TABLE[statusEffect.StatusEffectType];
			AudioManager.PlayOneShotAttached(this, path, base.gameObject);
		}
	}
}
