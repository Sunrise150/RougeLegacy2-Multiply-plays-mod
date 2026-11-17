using System;
using FMODUnity;
using RL_Windows;
using SceneManagement_RL;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Events;

public class JournalSpecialPropController : BaseSpecialPropController, IDisplaySpeechBubble
{
	[SerializeField]
	private int m_journalIndexOverride = -1;

	[SerializeField]
	private UnityEvent m_journalWindowOpenedUnityEvent;

	[SerializeField]
	private UnityEvent m_journalWindowClosedUnityEvent;

	private WindowID m_openWindow;

	private Relay m_finishedReadingRelay = new Relay();

	private Action m_onTransitionStart;

	private UnityAction m_onJournalWindowClosed;

	public string TitleOverrideLocID { get; private set; }

	public string EntryOverrideLocID { get; private set; }

	public JournalCategoryType JournalCategoryTypeOverride { get; private set; }

	public JournalType JournalType { get; private set; }

	public DialogueWindowStyle DialogueWindowStyle { get; private set; }

	public DialoguePortraitType PortraitType { get; private set; }

	public IRelayLink FinishedReadingRelay => m_finishedReadingRelay.link;

	public bool ShouldDisplaySpeechBubble
	{
		get
		{
			if (!base.IsPropComplete && string.IsNullOrEmpty(EntryOverrideLocID) && string.IsNullOrEmpty(TitleOverrideLocID))
			{
				BiomeType biomeType = PlayerManager.GetCurrentPlayerRoom().BiomeType;
				JournalCategoryType journalCategoryType = JournalType_RL.ConvertBiomeToJournalCategoryType(biomeType);
				if (journalCategoryType == JournalCategoryType.None)
				{
					return false;
				}
				if (JournalType == JournalType.MemoryFragment && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockMemory) < 1)
				{
					return false;
				}
				int numJournals = Journal_EV.GetNumJournals(biomeType, JournalType);
				if (SaveManager.PlayerSaveData.GetJournalsRead(journalCategoryType, JournalType) >= numJournals)
				{
					return false;
				}
				if (JournalCategoryTypeOverride == JournalCategoryType.Study && SaveManager.PlayerSaveData.GetJournalsRead(JournalCategoryType.Study, JournalType.Journal) <= 0)
				{
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public SpeechBubbleType BubbleType => SpeechBubbleType.PointOfInterest;

	protected override void Awake()
	{
		base.Awake();
		m_onJournalWindowClosed = OnJournalWindowClosed;
		m_onTransitionStart = OnTransitionStart;
	}

	protected override void InitializePooledPropOnEnter()
	{
		Prop component = GetComponent<Prop>();
		if (!component || !component.PropSpawnController)
		{
			return;
		}
		JournalEntryOverride component2 = component.PropSpawnController.gameObject.GetComponent<JournalEntryOverride>();
		if (!component2)
		{
			return;
		}
		TitleOverrideLocID = component2.TitleLocIDOverride;
		EntryOverrideLocID = component2.EntryLocIDOverride;
		JournalType = component2.JournalType;
		JournalCategoryTypeOverride = component2.JournalCategoryTypeOverride;
		DialogueWindowStyle = component2.DialogueWindowStyle;
		PortraitType = component2.PortraitType;
		if (JournalType != JournalType.ScarMemory)
		{
			return;
		}
		if (Challenge_EV.ScarUnlockTable.TryGetValue(EntryOverrideLocID, out var value))
		{
			bool num = ChallengeManager.GetFoundState(value) != FoundState.NotFound;
			bool flag = Challenge_EV.ScarBossRequirementTable.ContainsKey(value) && SaveManager.PlayerSaveData.GetFlag(Challenge_EV.ScarBossRequirementTable[value]);
			if (num || !flag)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			Debug.Log("<color=red>Could not spawn ScarMemory because the requested Loc ID is not in the Challenge_EV.ScarUnlockTable: " + EntryOverrideLocID + "</color>");
			base.gameObject.SetActive(value: false);
		}
	}

	public void TriggerJournal()
	{
		if (base.IsPropComplete && string.IsNullOrEmpty(EntryOverrideLocID) && string.IsNullOrEmpty(TitleOverrideLocID))
		{
			WindowManager.SetWindowIsOpen(WindowID.Journal, isOpen: true);
			m_openWindow = WindowID.Journal;
		}
		else if (JournalType == JournalType.MemoryFragment && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockMemory) <= 0)
		{
			DialogueManager.StartNewDialogue();
			DialogueManager.AddDialogue("LOC_ID_HEIRLOOM_TITLE_MEMORY_UNREADABLE_1", "LOC_ID_HEIRLOOM_TEXT_MEMORY_UNREADABLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, DialogueWindowStyle);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			m_openWindow = WindowID.Dialogue;
		}
		else if (string.IsNullOrEmpty(EntryOverrideLocID) || string.IsNullOrEmpty(TitleOverrideLocID))
		{
			BiomeType biomeType = PlayerManager.GetCurrentPlayerRoom().BiomeType;
			biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
			JournalCategoryType journalCategoryType = JournalType_RL.ConvertBiomeToJournalCategoryType(biomeType);
			if (JournalCategoryTypeOverride != 0)
			{
				journalCategoryType = JournalCategoryTypeOverride;
			}
			if (journalCategoryType == JournalCategoryType.None)
			{
				WindowManager.SetWindowIsOpen(WindowID.Journal, isOpen: true);
				m_openWindow = WindowID.Journal;
			}
			else
			{
				int num = -1;
				if (m_journalIndexOverride > -1)
				{
					num = m_journalIndexOverride;
				}
				else
				{
					num = SaveManager.PlayerSaveData.GetJournalsRead(journalCategoryType, JournalType);
					num++;
					SaveManager.PlayerSaveData.SetJournalsRead(journalCategoryType, JournalType, 1, additive: true, forceOverride: false);
					if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.JournalReadOnce))
					{
						SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.JournalReadOnce, value: true);
					}
				}
				if (num > 0 && num <= Journal_EV.GetNumJournals(journalCategoryType, JournalType))
				{
					JournalEntry journalEntry = Journal_EV.GetJournalEntry(journalCategoryType, JournalType, num - 1);
					if (!journalEntry.IsEmpty)
					{
						DialogueManager.StartNewDialogue();
						DialogueManager.AddDialogue(journalEntry.TitleLocID, journalEntry.TextLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, DialogueWindowStyle, PortraitType);
						WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
						m_openWindow = WindowID.Dialogue;
					}
					else
					{
						Debug.Log("<color=red>Could not display journal entry: " + (num - 1) + " in biome: " + biomeType.ToString() + ". Entry does not exist.</color>");
					}
				}
				else if (num > Journal_EV.GetNumJournals(journalCategoryType, JournalType))
				{
					WindowManager.SetWindowIsOpen(WindowID.Journal, isOpen: true);
					m_openWindow = WindowID.Journal;
				}
			}
			PropComplete();
		}
		else
		{
			if (EntryOverrideLocID == "LOC_ID_JOURNAL_DESCRIPTION_STUDY_BOSS_NEW_DIMENSIONS_4")
			{
				RuntimeManager.StudioSystem.setParameterByName("mus_fmf_memoryActive", 1f);
			}
			DialogueManager.StartNewDialogue();
			DialogueManager.AddDialogue(TitleOverrideLocID, EntryOverrideLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, DialogueWindowStyle, PortraitType);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			m_openWindow = WindowID.Dialogue;
		}
		if (m_openWindow == WindowID.None)
		{
			return;
		}
		WindowController windowController = WindowManager.GetWindowController(m_openWindow);
		if (windowController != null)
		{
			if (m_journalWindowOpenedUnityEvent != null)
			{
				m_journalWindowOpenedUnityEvent.Invoke();
			}
			windowController.WindowClosedEvent.AddListener(m_onJournalWindowClosed);
		}
	}

	private void OnJournalWindowClosed()
	{
		WindowManager.GetWindowController(m_openWindow).WindowClosedEvent.RemoveListener(m_onJournalWindowClosed);
		if (m_journalWindowClosedUnityEvent != null)
		{
			m_journalWindowClosedUnityEvent.Invoke();
		}
		if (JournalType == JournalType.ScarMemory)
		{
			base.gameObject.SetActive(value: false);
		}
		if (m_openWindow == WindowID.Dialogue)
		{
			HandleAchievements();
		}
		m_finishedReadingRelay.Dispatch();
	}

	private void HandleAchievements()
	{
		if (JournalType == JournalType.MemoryFragment && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockMemory) <= 0)
		{
			return;
		}
		BiomeType biomeType = PlayerManager.GetCurrentPlayerRoom().BiomeType;
		biomeType = BiomeType_RL.GetGroupedBiomeType(biomeType);
		if (biomeType == BiomeType.Tutorial && EntryOverrideLocID == "LOC_ID_JOURNAL_DESCRIPTION_TUTORIAL_HIDDEN_MESSAGE_1")
		{
			StoreAPIManager.GiveAchievement(AchievementType.StoryTutorial, StoreType.All);
			return;
		}
		JournalCategoryType journalCategoryType = JournalType_RL.ConvertBiomeToJournalCategoryType(biomeType);
		if (JournalCategoryTypeOverride != 0)
		{
			journalCategoryType = JournalCategoryTypeOverride;
		}
		if (journalCategoryType == JournalCategoryType.None)
		{
			return;
		}
		int journalsRead = SaveManager.PlayerSaveData.GetJournalsRead(journalCategoryType, JournalType.Journal);
		int journalsRead2 = SaveManager.PlayerSaveData.GetJournalsRead(journalCategoryType, JournalType.MemoryFragment);
		if (journalsRead == Journal_EV.GetNumJournals(journalCategoryType, JournalType.Journal) && journalsRead2 == Journal_EV.GetNumJournals(journalCategoryType, JournalType.MemoryFragment))
		{
			switch (journalCategoryType)
			{
			case JournalCategoryType.Castle:
				StoreAPIManager.GiveAchievement(AchievementType.StoryJournalsCastle, StoreType.All);
				break;
			case JournalCategoryType.Bridge:
				StoreAPIManager.GiveAchievement(AchievementType.StoryJournalsBridge, StoreType.All);
				break;
			case JournalCategoryType.Forest:
				StoreAPIManager.GiveAchievement(AchievementType.StoryJournalsForest, StoreType.All);
				break;
			case JournalCategoryType.Study:
				StoreAPIManager.GiveAchievement(AchievementType.StoryJournalsStudy, StoreType.All);
				break;
			case JournalCategoryType.Tower:
				StoreAPIManager.GiveAchievement(AchievementType.StoryJournalsTower, StoreType.All);
				break;
			case JournalCategoryType.Cave:
				StoreAPIManager.GiveAchievement(AchievementType.StoryJournalsCave, StoreType.All);
				break;
			}
		}
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
	}

	private void OnEnable()
	{
		SceneLoader_RL.TransitionStartRelay.AddListener(m_onTransitionStart);
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		FinishedReadingRelay.RemoveAll();
		if (EntryOverrideLocID == "LOC_ID_JOURNAL_DESCRIPTION_STUDY_BOSS_NEW_DIMENSIONS_4")
		{
			RuntimeManager.StudioSystem.setParameterByName("mus_fmf_memoryActive", 0f);
		}
		if ((bool)base.Room && base.Room.BiomeType != BiomeType.HubTown)
		{
			EntryOverrideLocID = null;
			TitleOverrideLocID = null;
		}
		SceneLoader_RL.TransitionStartRelay.RemoveListener(m_onTransitionStart);
	}

	private void OnTransitionStart()
	{
		StudioEventEmitter component = GetComponent<StudioEventEmitter>();
		if ((bool)component)
		{
			component.Stop();
		}
	}
}
