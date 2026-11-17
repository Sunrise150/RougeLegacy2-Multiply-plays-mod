using System;
using System.Collections;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using RLWorldCreation;
using SceneManagement_RL;
using UnityEngine;

namespace RLAudio;

public class MusicManager : MonoBehaviour, IAudioEventEmitter
{
	private const float START_PLAYING_MUSIC_AFTER_TIME = 2f;

	private const string LEAVE_LEVEL_PARAMETER = "leaveLevel";

	private const float LEAVE_LEVEL_VALUE = 0.51f;

	private const string LEAVE_INTRO_PARAMETER = "leaveIntro";

	private const float LEAVE_INTRO_VALUE = 0.51f;

	private const string BOSS_HEALTH_PARAMETER = "bossHealth";

	private const string BOSS_ENCOUNTER_PROGRESS_PARAMETER = "bossEncounterProgress";

	private const string BOSS_ENCOUNTER_PROGRESS_CAIN_PARAMETER = "bossEncounterProgress_cain";

	private const string SKIP_INTRO_PARAMETER = "skipIntro";

	private const float SKIP_INTRO_VALUE = 0.5f;

	private static EventInstance m_currentMusicInstance;

	private static Dictionary<SongID, EventInstance> m_musicInstanceLookupTable = new Dictionary<SongID, EventInstance>();

	private WaitRL_Yield m_waitBeforePlaying;

	private Coroutine changeTrackCoroutine;

	private BossRoomController m_currentBossRoomController;

	private static bool m_isPlayingOverride;

	private static SongID m_songToPlayInCastle;

	private static SongID m_songToPlayInBridge;

	private static SongID m_songToPlayInForest;

	private static SongID m_songToPlayInStudy;

	private static SongID m_songToPlayInTower;

	private static SongID m_songToPlayInCave;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onEnterBiome;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	private Action<float> m_onBossTookDamage;

	private Action m_onBossDefeated;

	private Action m_onBossOutroStart;

	public static SongID CurrentSong { get; private set; }

	public static SongID LastSongPlayed { get; private set; }

	public static EventInstance CurrentMusicInstance => m_currentMusicInstance;

	public static bool IsPlayingOverride => m_isPlayingOverride;

	public string Description => ToString();

	private static MusicManager Instance { get; set; }

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			m_onPlayerEnterRoom = OnPlayerEnterRoom;
			m_onEnterBiome = OnEnterBiome;
			m_onPlayerDeath = OnPlayerDeath;
			m_onBossTookDamage = OnBossTookDamage;
			m_onBossDefeated = OnBossDefeated;
			m_onBossOutroStart = OnBossOutroStart;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		if (Instance == this)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeEnter, m_onEnterBiome);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
			SceneLoader_RL.SceneLoadingStartRelay.AddListener(OnSceneStartedLoading);
			BiomeTransitionController.TransitionStartRelay.AddListener(OnBiomeTransitionStart);
			MusicLibrary.Initialize();
			InitializeInstanceLookupTable();
			m_waitBeforePlaying = new WaitRL_Yield(2f, useUnscaledTime: true);
		}
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeEnter, m_onEnterBiome);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
			SceneLoader_RL.SceneLoadingStartRelay.RemoveListener(OnSceneStartedLoading);
			BiomeTransitionController.TransitionStartRelay.RemoveListener(OnBiomeTransitionStart);
		}
	}

	private void InitializeInstanceLookupTable()
	{
		SongID[] typeArray = SongType_RL.TypeArray;
		foreach (SongID songID in typeArray)
		{
			string fMODEventPath = MusicLibrary.GetFMODEventPath(songID);
			if (!(fMODEventPath != string.Empty))
			{
				continue;
			}
			try
			{
				m_musicInstanceLookupTable.Add(songID, RuntimeManager.CreateInstance(fMODEventPath));
			}
			catch (Exception ex)
			{
				if (ex is EventNotFoundException)
				{
					Debug.LogFormat("<color=red>| MusicManager | No FMOD Event found with path ({0})</color>", fMODEventPath);
				}
			}
		}
	}

	private void OnEnterBiome(MonoBehaviour sender, EventArgs args)
	{
		if (m_currentMusicInstance.isValid())
		{
			StopMusic();
		}
		if (args is BiomeEventArgs { Biome: BiomeType.Lineage } && SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.Lineage))
		{
			PlayBiomeMusic(BiomeType.Lineage);
		}
	}

	private void OnSceneStartedLoading(string sceneName)
	{
		StopMusic();
		m_songToPlayInCastle = SongID.None;
		m_songToPlayInBridge = SongID.None;
		m_songToPlayInForest = SongID.None;
		m_songToPlayInStudy = SongID.None;
		m_songToPlayInTower = SongID.None;
		m_songToPlayInCave = SongID.None;
	}

	private void OnBiomeTransitionStart(BiomeType origin, BiomeType destination)
	{
		if (AudioUtility.GetHasParameter(m_currentMusicInstance, "leaveLevel"))
		{
			m_currentMusicInstance.setParameterByName("leaveLevel", 0.51f);
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "leaveIntro"))
			{
				m_currentMusicInstance.setParameterByName("leaveIntro", 0.51f);
			}
		}
		else
		{
			StopMusic();
		}
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		if (!CutsceneManager.IsCutsceneActive && args is RoomViaDoorEventArgs eventArgs)
		{
			UpdateMusicOnRoomChange(eventArgs);
		}
	}

	private void OnPlayerDeath(MonoBehaviour sender, EventArgs args)
	{
		if ((bool)m_currentBossRoomController)
		{
			PlayerDiedInBossRoom();
		}
	}

	private void UpdateMusicOnRoomChange(RoomViaDoorEventArgs eventArgs)
	{
		if (eventArgs.Room.RoomType == RoomType.Boss && eventArgs.Room.SpecialRoomType != SpecialRoomType.Subboss)
		{
			PlayerEnteredBossRoom(eventArgs);
			return;
		}
		bool hasOverride;
		SongID musicOverride = GetMusicOverride(eventArgs, out hasOverride);
		if (hasOverride)
		{
			if (musicOverride != CurrentSong)
			{
				StopMusic();
				if (musicOverride != 0)
				{
					PlayMusic(musicOverride, isOverride: true);
				}
			}
		}
		else if (CurrentSong == SongID.None || m_isPlayingOverride)
		{
			PlayBiomeMusic(eventArgs.Room.BiomeType);
		}
		else if (!m_isPlayingOverride && AudioUtility.GetHasParameter(m_currentMusicInstance, "leaveIntro"))
		{
			m_currentMusicInstance.setParameterByName("leaveIntro", 0.51f);
		}
	}

	private static SongID GetMusicOverride(RoomViaDoorEventArgs eventArgs, out bool hasOverride)
	{
		SongID result = SongID.None;
		hasOverride = false;
		if (eventArgs.Room is Room)
		{
			RoomMusicOverrideController component = eventArgs.Room.gameObject.GetComponent<RoomMusicOverrideController>();
			if ((bool)component)
			{
				result = component.Music;
				hasOverride = true;
			}
		}
		return result;
	}

	private SongID GetBiomeMusicForThisRun(BiomeType biome)
	{
		switch (biome)
		{
		case BiomeType.Castle:
			return m_songToPlayInCastle;
		case BiomeType.Stone:
			return m_songToPlayInBridge;
		case BiomeType.Forest:
			return m_songToPlayInForest;
		case BiomeType.Study:
			return m_songToPlayInStudy;
		case BiomeType.Tower:
		case BiomeType.TowerExterior:
			return m_songToPlayInTower;
		case BiomeType.Cave:
			return m_songToPlayInCave;
		default:
			return SongID.None;
		}
	}

	private void SetBiomeMusicForThisRun(BiomeType biome, SongID song)
	{
		switch (biome)
		{
		case BiomeType.Castle:
			m_songToPlayInCastle = song;
			break;
		case BiomeType.Stone:
			m_songToPlayInBridge = song;
			break;
		case BiomeType.Forest:
			m_songToPlayInForest = song;
			break;
		case BiomeType.Study:
			m_songToPlayInStudy = song;
			break;
		case BiomeType.Tower:
		case BiomeType.TowerExterior:
			m_songToPlayInTower = song;
			break;
		case BiomeType.Cave:
			m_songToPlayInCave = song;
			break;
		}
	}

	private void PlayBiomeMusic(BiomeType biome)
	{
		SongID songID = GetBiomeMusicForThisRun(biome);
		bool skipIntro = false;
		if (songID == SongID.None)
		{
			SongID[] biomeMusic = MusicLibrary.GetBiomeMusic(biome);
			if (biomeMusic.Length != 0)
			{
				int num = UnityEngine.Random.Range(0, biomeMusic.Length);
				songID = biomeMusic[num];
			}
		}
		else
		{
			skipIntro = true;
		}
		if (songID != 0)
		{
			SetBiomeMusicForThisRun(biome, songID);
			PlayMusic(songID, isOverride: false, skipIntro);
		}
	}

	public static void PlayMusic(SongID music, bool isOverride, bool skipIntro = false)
	{
		if (!MusicLibrary.IsInitialized)
		{
			MusicLibrary.Initialize();
		}
		if (Instance.changeTrackCoroutine != null)
		{
			Instance.StopCoroutine(Instance.changeTrackCoroutine);
		}
		if (CurrentSong != music)
		{
			Instance.changeTrackCoroutine = Instance.StartCoroutine(Instance.MusicTrackChangeCoroutine(music, isOverride, skipIntro));
		}
	}

	private IEnumerator MusicTrackChangeCoroutine(SongID music, bool isOverride, bool skipIntro)
	{
		if (m_currentMusicInstance.isValid() && AudioUtility.GetHasParameter(m_currentMusicInstance, "leaveLevel"))
		{
			m_currentMusicInstance.setParameterByName("leaveLevel", 0.51f);
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "leaveIntro"))
			{
				m_currentMusicInstance.setParameterByName("leaveIntro", 0.51f);
			}
			if (CurrentSong != 0)
			{
				yield return m_waitBeforePlaying;
			}
		}
		AudioManager.Stop(m_currentMusicInstance);
		if (!m_musicInstanceLookupTable.TryGetValue(music, out m_currentMusicInstance))
		{
			Debug.Log("Music track SongID: " + music.ToString() + " not found in music lookup table.");
		}
		if (m_currentMusicInstance.isValid())
		{
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "bossHealth"))
			{
				m_currentMusicInstance.setParameterByName("bossHealth", 1f);
			}
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "bossEncounterProgress"))
			{
				RuntimeManager.StudioSystem.setParameterByName("bossEncounterProgress", 0f);
			}
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "bossEncounterProgress_cain"))
			{
				RuntimeManager.StudioSystem.setParameterByName("bossEncounterProgress_cain", 0f);
			}
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "skipIntro"))
			{
				if (skipIntro)
				{
					m_currentMusicInstance.setParameterByName("skipIntro", 0.5f);
				}
				else
				{
					m_currentMusicInstance.setParameterByName("skipIntro", 0f);
				}
			}
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "leaveLevel"))
			{
				m_currentMusicInstance.setParameterByName("leaveLevel", 0f);
			}
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "leaveIntro"))
			{
				m_currentMusicInstance.setParameterByName("leaveIntro", 0f);
			}
			AudioManager.Play(Instance, m_currentMusicInstance);
		}
		LastSongPlayed = CurrentSong;
		CurrentSong = music;
		m_isPlayingOverride = isOverride;
		changeTrackCoroutine = null;
	}

	public static void StopMusic()
	{
		if (m_currentMusicInstance.isValid())
		{
			AudioManager.Stop(m_currentMusicInstance);
		}
		if ((bool)Instance && Instance.changeTrackCoroutine != null)
		{
			Instance.StopCoroutine(Instance.changeTrackCoroutine);
		}
		CurrentSong = SongID.None;
	}

	public static void StopMusicOverride()
	{
		if (m_isPlayingOverride)
		{
			StopMusic();
			PlayMusic(LastSongPlayed, isOverride: false, skipIntro: true);
		}
	}

	private void PlayerEnteredBossRoom(RoomViaDoorEventArgs eventArgs)
	{
		m_currentBossRoomController = eventArgs.Room.gameObject.GetComponentInChildren<BossRoomController>();
		if ((bool)m_currentBossRoomController)
		{
			m_currentBossRoomController.BossTookDamageRelay.AddListener(m_onBossTookDamage);
			m_currentBossRoomController.BossDefeatedRelay.AddListener(m_onBossDefeated);
			m_currentBossRoomController.OutroStartRelay.AddListener(m_onBossOutroStart);
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to get BossRoomController in boss room. If you see this message please add a bug report to Pivotal along with the Stack Trace.</color>", this);
		}
	}

	private void PlayerDiedInBossRoom()
	{
		UnsubscribeFromBossRoomEvents();
	}

	private void UnsubscribeFromBossRoomEvents()
	{
		m_currentBossRoomController.BossTookDamageRelay.RemoveListener(m_onBossTookDamage);
		m_currentBossRoomController.BossDefeatedRelay.RemoveListener(m_onBossDefeated);
		m_currentBossRoomController.OutroStartRelay.RemoveListener(m_onBossOutroStart);
		m_currentBossRoomController = null;
	}

	private void OnBossTookDamage(float bossHealthAsPercentage)
	{
		if (m_currentMusicInstance.isValid() && AudioUtility.GetHasParameter(m_currentMusicInstance, "bossHealth"))
		{
			m_currentMusicInstance.setParameterByName("bossHealth", bossHealthAsPercentage);
		}
	}

	private void OnBossOutroStart()
	{
		if (m_currentMusicInstance.isValid())
		{
			if (AudioUtility.GetHasParameter(m_currentMusicInstance, "bossEncounterProgress"))
			{
				RuntimeManager.StudioSystem.setParameterByName("bossEncounterProgress", 1f);
			}
			else if (!AudioUtility.GetHasParameter(m_currentMusicInstance, "bossHealth"))
			{
				StopMusic();
			}
		}
	}

	private void OnBossDefeated()
	{
		UnsubscribeFromBossRoomEvents();
		CurrentSong = SongID.None;
	}

	public static void SetBossEncounterParam(float value)
	{
		if (AudioUtility.GetHasParameter(m_currentMusicInstance, "bossEncounterProgress"))
		{
			RuntimeManager.StudioSystem.setParameterByName("bossEncounterProgress", value);
		}
	}
}
