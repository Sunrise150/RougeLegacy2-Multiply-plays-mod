using System;
using System.Collections.Generic;
using FMOD.Studio;
using FMODUnity;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace RLAudio;

public class AmbientSoundController : MonoBehaviour, IAudioEventEmitter
{
	private struct AmbientSoundControllerTableKey : IEquatable<AmbientSoundControllerTableKey>
	{
		public BiomeType Biome { get; }

		public RoomType RoomType { get; }

		public bool IsLarge { get; }

		public AmbientSoundControllerTableKey(BiomeType biome, RoomType roomType, bool isLarge)
		{
			Biome = biome;
			RoomType = roomType;
			IsLarge = isLarge;
		}

		public static bool operator ==(AmbientSoundControllerTableKey keyA, AmbientSoundControllerTableKey keyB)
		{
			return keyA.Equals(keyB);
		}

		public static bool operator !=(AmbientSoundControllerTableKey keyA, AmbientSoundControllerTableKey keyB)
		{
			return !keyA.Equals(keyB);
		}

		public bool Equals(AmbientSoundControllerTableKey other)
		{
			if (Biome == other.Biome && RoomType == other.RoomType)
			{
				return IsLarge == other.IsLarge;
			}
			return false;
		}

		public override bool Equals(object obj)
		{
			if (obj is AmbientSoundControllerTableKey)
			{
				return Equals((AmbientSoundControllerTableKey)obj);
			}
			return false;
		}

		public override int GetHashCode()
		{
			return ((-1554893279 * -1521134295 + Biome.GetHashCode()) * -1521134295 + RoomType.GetHashCode()) * -1521134295 + IsLarge.GetHashCode();
		}
	}

	[SerializeField]
	private bool m_muteAmbientSFX;

	private const string ANTIQUE_TRAIT_SNAPSHOT_PATH = "snapshot:/Trait_Antique";

	private const string TONE_DEAF_TRAIT_SNAPSHOT_PATH = "snapshot:/Trait_Tonedeaf";

	private Dictionary<AmbientSoundControllerTableKey, EventInstance[]> m_audioInstanceTable = new Dictionary<AmbientSoundControllerTableKey, EventInstance[]>();

	private Dictionary<AmbientSoundControllerTableKey, EventInstance> m_snapshotInstanceTable = new Dictionary<AmbientSoundControllerTableKey, EventInstance>();

	private EventInstance m_currentEvent;

	private EventInstance m_currentSnapshot;

	private EventInstance m_previousSnapshot;

	private bool m_inSkillTree;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	private Action<MonoBehaviour, EventArgs> m_onBiomeEnter;

	private Action<MonoBehaviour, EventArgs> m_onEnterMainMenu;

	private Action m_onTransitionStart;

	private UnityAction m_onSkillTreeWindowOpenedUnityEvent;

	private UnityAction m_onSkillTreeWindowClosedUnityEvent;

	private static AmbientSoundController m_instance;

	private static EventInstance m_antiquarianSnapshot;

	private static EventInstance m_toneDeafSnapshot;

	public bool StopOnTransition = true;

	public string Description => ToString();

	public EventInstance CurrentEvent
	{
		get
		{
			return m_currentEvent;
		}
		private set
		{
			m_currentEvent = value;
		}
	}

	public EventInstance CurrentSnapshot
	{
		get
		{
			return m_currentSnapshot;
		}
		private set
		{
			m_currentSnapshot = value;
		}
	}

	public static AmbientSoundController Instance
	{
		get
		{
			return m_instance;
		}
		private set
		{
			m_instance = value;
		}
	}

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			m_onPlayerEnterRoom = OnPlayerEnterRoom;
			m_onPlayerDeath = OnPlayerDeath;
			m_onBiomeEnter = OnBiomeEnter;
			m_onEnterMainMenu = OnEnterMainMenu;
			m_onSkillTreeWindowOpenedUnityEvent = OnSkillTreeWindowOpened;
			m_onSkillTreeWindowClosedUnityEvent = OnSkillTreeWindowClosed;
			m_onTransitionStart = OnTransitionStart;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeEnter, m_onBiomeEnter);
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.EnterMainMenu, m_onEnterMainMenu);
		SceneLoader_RL.TransitionStartRelay.AddListener(m_onTransitionStart);
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeEnter, m_onBiomeEnter);
			Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.EnterMainMenu, m_onEnterMainMenu);
			SceneLoader_RL.TransitionStartRelay.RemoveListener(m_onTransitionStart);
			if (m_antiquarianSnapshot.isValid())
			{
				m_antiquarianSnapshot.release();
			}
			if (m_toneDeafSnapshot.isValid())
			{
				m_toneDeafSnapshot.release();
			}
		}
	}

	private void OnBiomeEnter(MonoBehaviour sender, EventArgs args)
	{
		if (!(args is BiomeEventArgs))
		{
			return;
		}
		BiomeEventArgs biomeEventArgs = args as BiomeEventArgs;
		if (biomeEventArgs.Biome == BiomeType.Lineage && SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.Lineage))
		{
			AmbientSoundControllerTableKey tableKey = GetTableKey(BiomeType.Lineage, RoomType.Transition, isRoomLarge: false);
			if (!m_audioInstanceTable.ContainsKey(tableKey))
			{
				AddTableEntry(tableKey);
			}
			PlayAudio(tableKey, null);
		}
		else if (biomeEventArgs.Biome == BiomeType.HubTown)
		{
			OnEnterHubTown();
		}
	}

	private void OnTransitionStart()
	{
		if (StopOnTransition)
		{
			StopCurrentAmbientSoundAndSnapshot();
		}
	}

	private void OnEnterHubTown()
	{
		SceneManager.sceneLoaded -= OnHubTownSceneClosed;
		SceneManager.sceneLoaded += OnHubTownSceneClosed;
		WindowController windowController = WindowManager.GetWindowController(WindowID.SkillTree);
		if ((bool)windowController)
		{
			windowController.WindowOpenedEvent.RemoveListener(m_onSkillTreeWindowOpenedUnityEvent);
			windowController.WindowClosedEvent.RemoveListener(m_onSkillTreeWindowClosedUnityEvent);
			windowController.WindowOpenedEvent.AddListener(m_onSkillTreeWindowOpenedUnityEvent);
			windowController.WindowClosedEvent.AddListener(m_onSkillTreeWindowClosedUnityEvent);
		}
		GetTableKey(BiomeType.HubTown, RoomType.Standard, isRoomLarge: false);
		GetTableKey(BiomeType.HubTown, RoomType.Transition, isRoomLarge: true);
	}

	private void OnHubTownSceneClosed(Scene scene, LoadSceneMode mode)
	{
		SceneManager.sceneLoaded -= OnHubTownSceneClosed;
		if (WindowManager.GetIsWindowLoaded(WindowID.SkillTree))
		{
			WindowController windowController = WindowManager.GetWindowController(WindowID.SkillTree);
			if (windowController != null)
			{
				windowController.WindowOpenedEvent.RemoveListener(OnSkillTreeWindowOpened);
				windowController.WindowClosedEvent.RemoveListener(OnSkillTreeWindowClosed);
			}
		}
	}

	private void OnSkillTreeWindowOpened()
	{
		AmbientSoundControllerTableKey tableKey = GetTableKey(BiomeType.HubTown, RoomType.Standard, isRoomLarge: false);
		if (!m_audioInstanceTable.ContainsKey(tableKey))
		{
			AddTableEntry(tableKey);
		}
		PlayAudio(tableKey, null);
		m_inSkillTree = true;
	}

	private void OnSkillTreeWindowClosed()
	{
		AmbientSoundControllerTableKey tableKey = GetTableKey(BiomeType.HubTown, RoomType.Transition, isRoomLarge: true);
		if (!m_audioInstanceTable.ContainsKey(tableKey))
		{
			AddTableEntry(tableKey);
		}
		PlayAudio(tableKey, null);
		m_inSkillTree = false;
	}

	private void OnEnterMainMenu(MonoBehaviour sender, EventArgs args)
	{
		StopCurrentAmbientSoundAndSnapshot();
	}

	public static EventInstance GetCurrentEventInstance()
	{
		return Instance.CurrentEvent;
	}

	private void OnPlayerDeath(MonoBehaviour sender, EventArgs args)
	{
		StopCurrentAmbientSoundAndSnapshot();
	}

	private void StopCurrentAmbientSoundAndSnapshot()
	{
		if (CurrentEvent.isValid())
		{
			AudioManager.Stop(CurrentEvent);
			CurrentEvent = default(EventInstance);
		}
		if (CurrentSnapshot.isValid())
		{
			AudioManager.Stop(CurrentSnapshot);
			CurrentSnapshot = default(EventInstance);
		}
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		if (m_inSkillTree)
		{
			return;
		}
		if (args is RoomViaDoorEventArgs roomViaDoorEventArgs)
		{
			AmbientSoundControllerTableKey tableKey = GetTableKey(roomViaDoorEventArgs.Room);
			if (!m_audioInstanceTable.ContainsKey(tableKey))
			{
				AddTableEntry(tableKey);
			}
			AmbientSoundOverride component = roomViaDoorEventArgs.Room.gameObject.GetComponent<AmbientSoundOverride>();
			PlayAudio(tableKey, component);
			if ((bool)component && AudioUtility.GetHasParameter(CurrentEvent, "finalBindingStone_activating"))
			{
				CurrentEvent.setParameterByName("finalBindingStone_activating", 0f);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | Failed to cast event args as RoomViaDoorEventArgs</color>", this);
		}
	}

	private void AddTableEntry(AmbientSoundControllerTableKey key)
	{
		string ambientAudioSnapshotPath = AmbientSoundLibrary.GetAmbientAudioSnapshotPath(key.Biome, key.RoomType, key.IsLarge);
		EventInstance value = default(EventInstance);
		if (!string.IsNullOrEmpty(ambientAudioSnapshotPath))
		{
			value = RuntimeManager.CreateInstance(ambientAudioSnapshotPath);
		}
		m_snapshotInstanceTable.Add(key, value);
		string[] ambientAudioEventPaths = AmbientSoundLibrary.GetAmbientAudioEventPaths(key.Biome, key.RoomType, key.IsLarge);
		m_audioInstanceTable.Add(key, new EventInstance[ambientAudioEventPaths.Length]);
		for (int i = 0; i < ambientAudioEventPaths.Length; i++)
		{
			m_audioInstanceTable[key][i] = RuntimeManager.CreateInstance(ambientAudioEventPaths[i]);
		}
	}

	private EventInstance[] GetPotentialAudio(AmbientSoundControllerTableKey key, AmbientSoundOverride soundOverride)
	{
		if (soundOverride != null && soundOverride.HasAudioOverride)
		{
			return soundOverride.Audio;
		}
		return m_audioInstanceTable[key];
	}

	private EventInstance GetSnapshot(AmbientSoundControllerTableKey key, AmbientSoundOverride soundOverride)
	{
		bool flag = TraitManager.IsInitialized && TraitManager.IsTraitActive(TraitType.OldYellowTint);
		bool flag2 = TraitManager.IsInitialized && TraitManager.IsTraitActive(TraitType.ToneDeaf);
		if (!flag && !flag2)
		{
			if (soundOverride != null && soundOverride.HasSnapshotOverride)
			{
				return soundOverride.Snapshot;
			}
			return m_snapshotInstanceTable[key];
		}
		if (flag)
		{
			if (!m_antiquarianSnapshot.isValid())
			{
				m_antiquarianSnapshot = AudioUtility.GetEventInstance("snapshot:/Trait_Antique", base.transform);
			}
			return m_antiquarianSnapshot;
		}
		if (!m_toneDeafSnapshot.isValid())
		{
			m_toneDeafSnapshot = AudioUtility.GetEventInstance("snapshot:/Trait_Tonedeaf", base.transform);
		}
		return m_toneDeafSnapshot;
	}

	private AmbientSoundControllerTableKey GetTableKey(BaseRoom room)
	{
		BiomeType appearanceBiomeType = room.AppearanceBiomeType;
		RoomType roomType = GetRoomType(room);
		bool isRoomLarge = RoomUtility.GetIsRoomLarge(room);
		return GetTableKey(appearanceBiomeType, roomType, isRoomLarge);
	}

	private AmbientSoundControllerTableKey GetTableKey(BiomeType biome, RoomType roomType, bool isRoomLarge)
	{
		return new AmbientSoundControllerTableKey(biome, roomType, isRoomLarge);
	}

	private RoomType GetRoomType(BaseRoom room)
	{
		RoomType result = RoomType.Standard;
		if (room is Room)
		{
			Room room2 = room as Room;
			result = room2.RoomType;
			if (room2.RoomType == RoomType.Fairy)
			{
				FairyRoomController component = room2.gameObject.GetComponent<FairyRoomController>();
				if (component != null && component.FairyRoomRuleEntries != null)
				{
					if (component.State == FairyRoomState.Failed || component.State == FairyRoomState.Passed)
					{
						result = RoomType.Standard;
					}
					else
					{
						for (int i = 0; i < component.FairyRoomRuleEntries.Count; i++)
						{
							if (component.FairyRoomRuleEntries[i].FairyRuleID == FairyRuleID.HiddenChest)
							{
								result = RoomType.Standard;
								break;
							}
						}
					}
				}
			}
		}
		return result;
	}

	private void PlayAudio(AmbientSoundControllerTableKey key, AmbientSoundOverride ambientSoundOverride)
	{
		if (!m_muteAmbientSFX)
		{
			EventInstance[] potentialAudio = GetPotentialAudio(key, ambientSoundOverride);
			bool isRaining = false;
			if (BiomeCreation_EV.IS_IT_RAINING_IN_BIOME_TABLE.ContainsKey(key.Biome))
			{
				isRaining = BiomeCreation_EV.IS_IT_RAINING_IN_BIOME_TABLE[key.Biome];
			}
			PlayRandomAmbientAudio(potentialAudio, isRaining);
			EventInstance snapshot = GetSnapshot(key, ambientSoundOverride);
			PlaySnapshot(snapshot);
		}
	}

	public void PlayManorAmbientAudio()
	{
		if (!m_muteAmbientSFX)
		{
			AmbientSoundControllerTableKey tableKey = GetTableKey(BiomeType.Lineage, RoomType.Standard, isRoomLarge: true);
			if (!m_audioInstanceTable.ContainsKey(tableKey))
			{
				AddTableEntry(tableKey);
			}
			PlayAudio(tableKey, null);
			PlaySnapshot(GetSnapshot(tableKey, null));
		}
	}

	private void PlayRandomAmbientAudio(EventInstance[] eventInstances, bool isRaining)
	{
		if (eventInstances.Length == 0)
		{
			return;
		}
		int num = 0;
		if (eventInstances.Length > 1)
		{
			num = UnityEngine.Random.Range(0, eventInstances.Length);
		}
		if (!CurrentEvent.Equals(eventInstances[num]))
		{
			AudioManager.Stop(CurrentEvent);
			CurrentEvent = eventInstances[num];
			AudioManager.Play(this, CurrentEvent);
			float value = 0f;
			if (isRaining)
			{
				value = 1f;
			}
			CurrentEvent.setParameterByName("Weather", value);
		}
	}

	private void PlaySnapshot(EventInstance eventInstance)
	{
		if (eventInstance.isValid() && !CurrentSnapshot.Equals(eventInstance))
		{
			AudioManager.Stop(CurrentSnapshot);
			CurrentSnapshot = eventInstance;
			AudioManager.Play(this, CurrentSnapshot);
		}
	}

	public static void SetSnapshotOverride(EventInstance eventInstance)
	{
		Instance.m_previousSnapshot = Instance.CurrentSnapshot;
		Instance.PlaySnapshot(eventInstance);
	}

	public static void ClearSnapshotOverride()
	{
		Instance.PlaySnapshot(Instance.m_previousSnapshot);
	}
}
