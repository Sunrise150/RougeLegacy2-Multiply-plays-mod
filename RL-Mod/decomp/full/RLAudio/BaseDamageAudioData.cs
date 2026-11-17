using System.Collections.Generic;
using UnityEngine;

namespace RLAudio;

public abstract class BaseDamageAudioData : MonoBehaviour
{
	public abstract string BreakableDamageAudioPath { get; }

	public abstract string CharacterDamageAudioPath { get; }

	public Dictionary<string, float> HitAudioParameters { get; private set; }

	public void AddHitAudioParameter(string parameterName, float parameterValue)
	{
		if (HitAudioParameters == null)
		{
			HitAudioParameters = new Dictionary<string, float>();
		}
		if (!HitAudioParameters.ContainsKey(parameterName))
		{
			HitAudioParameters.Add(parameterName, parameterValue);
		}
		else
		{
			HitAudioParameters[parameterName] = parameterValue;
		}
	}

	public void RemoveAudioParameter(string parameterName)
	{
		if (HitAudioParameters != null && HitAudioParameters.ContainsKey(parameterName))
		{
			HitAudioParameters.Remove(parameterName);
		}
	}

	protected virtual void OnDisable()
	{
		if (HitAudioParameters != null)
		{
			HitAudioParameters.Clear();
		}
	}
}
