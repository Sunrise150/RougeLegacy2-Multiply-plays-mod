using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public static class AudioUtility
{
	public const string PLAYER_SIZE_PARAMETER = "Player_Size";

	public const string GENDER_PARAMETER = "gender";

	public static EventInstance GetEventInstance(string eventPath, Transform transform)
	{
		if (!string.IsNullOrEmpty(eventPath))
		{
			EventDescription eventDescription;
			try
			{
				eventDescription = RuntimeManager.GetEventDescription(eventPath);
			}
			catch (Exception)
			{
				throw;
			}
			EventInstance result = RuntimeManager.CreateInstance(eventPath);
			eventDescription.is3D(out var is3D);
			if (is3D)
			{
				if (transform != null)
				{
					result.set3DAttributes(transform.To3DAttributes());
				}
				else
				{
					UnityEngine.Debug.LogFormat("<color=red>| AudioUtility | The FMOD Event at <b>({0})</b> is 3D but you didn't pass in a reference to the GameObject's Transform. If you see this message, please add a bug report to Pivotal.</color>", eventPath);
				}
			}
			return result;
		}
		return default(EventInstance);
	}

	public static float GetPlayerSizeAudioParameterValue()
	{
		float result = 0.5f;
		if (PlayerManager.IsInstantiated)
		{
			if (PlayerManager.GetPlayerController().transform.localScale.x < 1.4f)
			{
				result = 1f;
			}
			else if (PlayerManager.GetPlayerController().transform.localScale.x > 1.4f)
			{
				result = 0f;
			}
		}
		return result;
	}

	public static float GetPlayerGenderAudioParameterValue()
	{
		float result = 1f;
		if (SaveManager.IsInitialized)
		{
			bool flag = SaveManager.PlayerSaveData.CurrentCharacter.IsFemale;
			if (SaveManager.PlayerSaveData.CurrentCharacter.Disposition_ID == 1)
			{
				flag = !flag;
			}
			if (flag)
			{
				result = 0f;
			}
		}
		return result;
	}

	public static bool GetHasParameter(EventInstance eventInstance, string parameterName)
	{
		float value;
		if (eventInstance.isValid() && parameterName != string.Empty)
		{
			return eventInstance.getParameterByName(parameterName, out value) == RESULT.OK;
		}
		return false;
	}

	public static PLAYBACK_STATE GetCurrentPlaybackState(EventInstance eventInstance)
	{
		eventInstance.getPlaybackState(out var state);
		return state;
	}
}
