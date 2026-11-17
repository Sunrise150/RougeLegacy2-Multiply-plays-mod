using System.Collections.Generic;
using FMOD.Studio;
using SceneManagement_RL;
using UnityEngine;

namespace RLAudio;

public class WaterAudioSourceManager : MonoBehaviour, IAudioEventEmitter
{
	private static GameObject m_audioEventEmitterGameObject = null;

	private static EventInstance m_defaultWaterAudioEventInstance = default(EventInstance);

	private static EventInstance m_choppyWaterAudioEventInstance = default(EventInstance);

	private static EventInstance m_currentWaterEventInstance;

	private string m_description = string.Empty;

	private static bool m_isPlaying = false;

	private static List<WaterAudioSource> m_waterSources = new List<WaterAudioSource>();

	private static bool m_hasWarningBeenDisplayed = false;

	private const string DEFAULT_WATER_PATH = "event:/Environment/Spots/amb_spot_waves_light";

	private const string CHOPPY_WATER_PATH = "event:/Environment/Spots/amb_spot_waves_heavy";

	private const float IS_RAINING_PARAMETER_VALUE = 1f;

	private const float IS_NOT_RAINING_PARAMETER_VALUE = 0f;

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

	public static bool IsPlaying
	{
		get
		{
			return m_isPlaying;
		}
		private set
		{
			m_isPlaying = value;
		}
	}

	public static List<WaterAudioSource> WaterSources
	{
		get
		{
			return m_waterSources;
		}
		private set
		{
			m_waterSources = value;
		}
	}

	public static EventInstance CurrentWaterEventInstance
	{
		get
		{
			return m_currentWaterEventInstance;
		}
		private set
		{
			m_currentWaterEventInstance = value;
		}
	}

	private void Awake()
	{
		if (m_audioEventEmitterGameObject == null)
		{
			m_audioEventEmitterGameObject = new GameObject("Water Audio Source");
			m_audioEventEmitterGameObject.transform.SetParent(base.transform);
			m_defaultWaterAudioEventInstance = AudioUtility.GetEventInstance("event:/Environment/Spots/amb_spot_waves_light", m_audioEventEmitterGameObject.transform);
			m_choppyWaterAudioEventInstance = AudioUtility.GetEventInstance("event:/Environment/Spots/amb_spot_waves_heavy", m_audioEventEmitterGameObject.transform);
		}
		SceneLoader_RL.TransitionStartRelay.AddListener(OnTransitionStart);
	}

	private void OnDestroy()
	{
		if (m_defaultWaterAudioEventInstance.isValid())
		{
			m_defaultWaterAudioEventInstance.release();
		}
		if (m_choppyWaterAudioEventInstance.isValid())
		{
			m_choppyWaterAudioEventInstance.release();
		}
	}

	private void OnTransitionStart()
	{
		if (IsPlaying)
		{
			WaterSources.Clear();
			Stop();
		}
	}

	public static void AddWaterSource(WaterAudioSource source)
	{
		if ((WaterSources.Count <= 0 || source.WaterLevel == WaterSources[0].WaterLevel) && !WaterSources.Contains(source))
		{
			WaterSources.Add(source);
		}
	}

	public static void RemoveWaterSource(WaterAudioSource source)
	{
		if (WaterSources.Contains(source))
		{
			WaterSources.Remove(source);
		}
	}

	private void Update()
	{
		if (!(m_audioEventEmitterGameObject == null) && m_audioEventEmitterGameObject.activeInHierarchy && CameraController.IsInstantiated)
		{
			if (IsPlaying)
			{
				Stop();
			}
			else if (!IsPlaying)
			{
				Play();
			}
			if (IsPlaying)
			{
				UpdateAudioSourcePosition();
			}
		}
	}

	private static void UpdateAudioSourcePosition()
	{
		Vector2 vector = CameraController.GameCamera.transform.position;
		float num = WaterSources[0].transform.position.y - vector.y;
		m_audioEventEmitterGameObject.transform.position = vector + num * Vector2.up;
	}

	private void Play()
	{
		if (WaterSources.Count > 0)
		{
			CurrentWaterEventInstance = GetWaterInstance();
			AudioManager.PlayAttached(this, CurrentWaterEventInstance, m_audioEventEmitterGameObject);
			float weatherParameterValue = GetWeatherParameterValue();
			CurrentWaterEventInstance.setParameterByName("Weather", weatherParameterValue);
			IsPlaying = true;
		}
	}

	private EventInstance GetWaterInstance()
	{
		if (WaterSources[0].IsChoppy)
		{
			return m_choppyWaterAudioEventInstance;
		}
		return m_defaultWaterAudioEventInstance;
	}

	private static float GetWeatherParameterValue()
	{
		bool flag = false;
		BiomeType appearanceBiomeType = PlayerManager.GetCurrentPlayerRoom().AppearanceBiomeType;
		if (BiomeCreation_EV.IS_IT_RAINING_IN_BIOME_TABLE.ContainsKey(appearanceBiomeType))
		{
			flag = BiomeCreation_EV.IS_IT_RAINING_IN_BIOME_TABLE[appearanceBiomeType];
		}
		float result = 0f;
		if (flag)
		{
			result = 1f;
		}
		return result;
	}

	private void Stop()
	{
		if (WaterSources.Count == 0)
		{
			if (CurrentWaterEventInstance.isValid())
			{
				AudioManager.Stop(CurrentWaterEventInstance);
				CurrentWaterEventInstance = default(EventInstance);
			}
			IsPlaying = false;
		}
	}
}
