using System;
using System.Collections;
using System.Collections.Generic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public class AudioManager : MonoBehaviour
{
	private struct EventParameterPair
	{
		public Guid AudioEvent { get; }

		public string Parameter { get; }

		public EventParameterPair(Guid audioEvent, string parameter)
		{
			AudioEvent = audioEvent;
			Parameter = parameter;
		}
	}

	[SerializeField]
	private float m_maxRandomDelay = 0.25f;

	[SerializeField]
	private bool m_logCallerDescriptionsToConsole;

	[SerializeField]
	[Tooltip("Only display Audio Events whose paths contain the following string.")]
	private string m_eventPathFilter = string.Empty;

	private const string SFX_BUS_PATH = "bus:/Master/User_Sfx";

	private const string SFX_GAMEPLAY_BUS_PATH = "bus:/Master/User_Sfx/Sfx_Master/Gameplay";

	private const string SFX_ENEMY_BUS_PATH = "bus:/Master/User_Sfx/Sfx_Master/Gameplay/Enemies";

	private const string SFX_PLAYER_BUS_PATH = "bus:/Master/User_Sfx/Sfx_Master/Gameplay/PlayerCharacter";

	private const string MUSIC_BUS_PATH = "bus:/Master/User_Music";

	private const string MASTER_BUS_PATH = "Bus:/";

	private static Dictionary<string, Guid> m_audioEventIDTable = new Dictionary<string, Guid>();

	private static Dictionary<EventParameterPair, PARAMETER_ID> m_parameterIDTable = new Dictionary<EventParameterPair, PARAMETER_ID>();

	private static Bus m_masterBus;

	private static Bus m_musicBus;

	private static Bus m_sfxBus;

	private static Bus m_sfxGameplayBus;

	private static Bus m_sfxEnemyBus;

	private static Bus m_sfxPlayerBus;

	private static AudioManager m_instance;

	private WaitRL_Yield m_waitDelay;

	private static AudioManager Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool IsMusicPaused
	{
		get
		{
			if (IsMasterPaused)
			{
				return true;
			}
			bool paused = false;
			if (m_musicBus.isValid())
			{
				m_musicBus.getPaused(out paused);
			}
			return paused;
		}
	}

	public static bool IsSFXPaused
	{
		get
		{
			if (IsMasterPaused)
			{
				return true;
			}
			bool paused = false;
			if (m_sfxGameplayBus.isValid())
			{
				m_sfxGameplayBus.getPaused(out paused);
			}
			return paused;
		}
	}

	public static bool IsMasterPaused
	{
		get
		{
			bool paused = false;
			if (m_masterBus.isValid())
			{
				m_masterBus.getPaused(out paused);
			}
			return paused;
		}
	}

	public static bool IsInitialized
	{
		get
		{
			if (RuntimeManager.IsInitialized && RuntimeManager.HasBanksLoaded && IsMasterBusLoaded && IsMusicBusLoaded)
			{
				return IsSFXBusLoaded;
			}
			return false;
		}
	}

	public static bool IsMasterBusLoaded => m_masterBus.isValid();

	public static bool IsMusicBusLoaded => m_musicBus.isValid();

	public static bool IsSFXBusLoaded
	{
		get
		{
			if (m_sfxBus.isValid())
			{
				return m_sfxGameplayBus.isValid();
			}
			return false;
		}
	}

	public static bool IsEnemySFXBusLoaded
	{
		get
		{
			if (m_sfxBus.isValid())
			{
				return m_sfxEnemyBus.isValid();
			}
			return false;
		}
	}

	public static bool IsPlayerSFXBusLoaded
	{
		get
		{
			if (m_sfxBus.isValid())
			{
				return m_sfxPlayerBus.isValid();
			}
			return false;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Initialize();
		}
	}

	private static void CheckAudioBuses()
	{
		if (!m_masterBus.isValid())
		{
			m_masterBus = RuntimeManager.GetBus("Bus:/");
		}
		if (!m_musicBus.isValid())
		{
			m_musicBus = RuntimeManager.GetBus("bus:/Master/User_Music");
		}
		if (!m_sfxBus.isValid())
		{
			m_sfxBus = RuntimeManager.GetBus("bus:/Master/User_Sfx");
		}
		if (!m_sfxGameplayBus.isValid())
		{
			m_sfxGameplayBus = RuntimeManager.GetBus("bus:/Master/User_Sfx/Sfx_Master/Gameplay");
		}
		if (!m_sfxEnemyBus.isValid())
		{
			m_sfxEnemyBus = RuntimeManager.GetBus("bus:/Master/User_Sfx/Sfx_Master/Gameplay/Enemies");
		}
		if (!m_sfxPlayerBus.isValid())
		{
			m_sfxPlayerBus = RuntimeManager.GetBus("bus:/Master/User_Sfx/Sfx_Master/Gameplay/PlayerCharacter");
		}
	}

	public static Guid GetEventID(string audioEvent)
	{
		if (!m_audioEventIDTable.ContainsKey(audioEvent))
		{
			RuntimeManager.GetEventDescription(audioEvent).getID(out var id);
			m_audioEventIDTable[audioEvent] = id;
		}
		return m_audioEventIDTable[audioEvent];
	}

	public static PARAMETER_ID GetParameterID(Guid audioEvent, string parameter)
	{
		EventParameterPair key = new EventParameterPair(audioEvent, parameter);
		if (!m_parameterIDTable.ContainsKey(key))
		{
			RuntimeManager.GetEventDescription(audioEvent).getParameterDescriptionByName(parameter, out var parameter2);
			m_parameterIDTable[key] = parameter2.id;
		}
		return m_parameterIDTable[key];
	}

	private static void Initialize()
	{
		CheckAudioBuses();
	}

	public static float GetMasterVolume()
	{
		return GetVolume(m_masterBus);
	}

	public static float GetMusicVolume()
	{
		return GetVolume(m_musicBus);
	}

	public static float GetSFXVolume()
	{
		return GetVolume(m_sfxBus);
	}

	private static float GetVolume(Bus bus)
	{
		CheckAudioBuses();
		float volume = 1f;
		if (bus.isValid())
		{
			bus.getVolume(out volume);
		}
		else
		{
			UnityEngine.Debug.LogFormat("<color=red>| AudioManager | Failed to find Bus ({0}). If you see this message please add a bug to Pivotal along with Stack Trace</color>", bus.ToString());
		}
		return volume;
	}

	public static void Play(IAudioEventEmitter caller, EventInstance eventInstance)
	{
		if (Instance.m_logCallerDescriptionsToConsole)
		{
			eventInstance.getDescription(out var description);
			description.getPath(out var path);
			LogAudioEventToConsole(caller, path);
		}
		eventInstance.start();
	}

	public static void Play(IAudioEventEmitter caller, StudioEventEmitter studioEventEmitter)
	{
		if (Instance.m_logCallerDescriptionsToConsole)
		{
			LogAudioEventToConsole(caller, studioEventEmitter.Event);
		}
		studioEventEmitter.Play();
	}

	private static void LogAudioEventToConsole(IAudioEventEmitter caller, string eventPath)
	{
	}

	public static void PlayAttached(IAudioEventEmitter caller, EventInstance eventInstance, GameObject gameObject)
	{
		Rigidbody2D rigidBody2D = null;
		RuntimeManager.AttachInstanceToGameObject(eventInstance, gameObject.transform, rigidBody2D);
		Play(caller, eventInstance);
	}

	public static void PlayOneShotAttached(IAudioEventEmitter caller, string path, GameObject gameObject)
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (Instance.m_logCallerDescriptionsToConsole)
			{
				LogAudioEventToConsole(caller, path);
			}
			RuntimeManager.PlayOneShotAttached(path, gameObject);
		}
	}

	public static void PlayOneShot(IAudioEventEmitter caller, string path, Vector3 position = default(Vector3))
	{
		if (!string.IsNullOrEmpty(path))
		{
			if (Instance.m_logCallerDescriptionsToConsole)
			{
				LogAudioEventToConsole(caller, path);
			}
			RuntimeManager.PlayOneShot(path, position);
		}
	}

	public static void PlayDelayedOneShot(IAudioEventEmitter caller, string path, Vector2 position, float delayTime = -1f)
	{
		Instance.StartCoroutine(Instance.PlayDelayedOneShotCoroutine(caller, path, position, delayTime));
	}

	private IEnumerator PlayDelayedOneShotCoroutine(IAudioEventEmitter caller, string path, Vector2 position, float time)
	{
		float waitTime = ((time >= 0f) ? time : UnityEngine.Random.Range(0f, m_maxRandomDelay));
		if (m_waitDelay == null)
		{
			m_waitDelay = new WaitRL_Yield(waitTime);
		}
		else
		{
			m_waitDelay.CreateNew(waitTime);
		}
		yield return m_waitDelay;
		PlayOneShot(caller, path, position);
	}

	public static void Play(IAudioEventEmitter caller, EventInstance eventInstance, Vector3 worldPosition)
	{
		eventInstance.set3DAttributes(worldPosition.To3DAttributes());
		Play(caller, eventInstance);
	}

	public static void Play(IAudioEventEmitter caller, string eventPath, Vector3 worldPosition = default(Vector3))
	{
		RuntimeManager.PlayOneShot(eventPath, worldPosition);
	}

	public static void SetMasterVolume(float volume)
	{
		SetVolume(m_masterBus, volume);
	}

	public static void SetMusicVolume(float volume)
	{
		SetVolume(m_musicBus, volume);
	}

	public static void SetSFXVolume(float volume)
	{
		SetVolume(m_sfxBus, volume);
	}

	private static void SetVolume(Bus bus, float volume)
	{
		CheckAudioBuses();
		if (bus.isValid())
		{
			bus.setVolume(volume);
			return;
		}
		string path = "";
		bus.getPath(out path);
		UnityEngine.Debug.LogFormat("<color=red>| AudioManager | Failed to find Bus ({0}). If you see this message please add a bug to Pivotal along with Stack Trace</color>", path);
	}

	public static void SetMasterPaused(bool pause)
	{
		if (IsMasterBusLoaded)
		{
			RESULT rESULT = m_masterBus.setPaused(pause);
			if (rESULT != 0)
			{
				UnityEngine.Debug.Log("Could not pause Master. FMOD.RESULT: " + rESULT);
			}
		}
	}

	public static void SetMusicPaused(bool pause)
	{
		if (IsMusicBusLoaded)
		{
			RESULT rESULT = m_musicBus.setPaused(pause);
			if (rESULT != 0)
			{
				UnityEngine.Debug.Log("Could not pause Music. FMOD.RESULT: " + rESULT);
			}
		}
	}

	public static void SetSFXPaused(bool pause)
	{
		if (IsSFXBusLoaded)
		{
			RESULT rESULT = m_sfxGameplayBus.setPaused(pause);
			if (rESULT != 0)
			{
				UnityEngine.Debug.Log("Could not pause SFX. FMOD.RESULT: " + rESULT);
			}
		}
	}

	public static void SetEnemySFXPaused(bool pause)
	{
		if (IsEnemySFXBusLoaded)
		{
			RESULT rESULT = m_sfxEnemyBus.setPaused(pause);
			if (rESULT != 0)
			{
				UnityEngine.Debug.Log("Could not pause SFX. FMOD.RESULT: " + rESULT);
			}
		}
	}

	public static void SetPlayerSFXPaused(bool pause)
	{
		if (IsPlayerSFXBusLoaded)
		{
			RESULT rESULT = m_sfxPlayerBus.setPaused(pause);
			if (rESULT != 0)
			{
				UnityEngine.Debug.Log("Could not pause SFX. FMOD.RESULT: " + rESULT);
			}
		}
	}

	public static void Stop(EventInstance eventInstance, FMOD.Studio.STOP_MODE stopMode = FMOD.Studio.STOP_MODE.ALLOWFADEOUT)
	{
		eventInstance.stop(stopMode);
	}

	public static void StopAllAudio(FMOD.Studio.STOP_MODE stopMode)
	{
		if (m_masterBus.isValid())
		{
			m_masterBus.stopAllEvents(stopMode);
		}
	}
}
