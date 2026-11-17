using System;
using System.Collections.Generic;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class DummyDialogueController : MonoBehaviour, IDisplaySpeechBubble, IAudioEventEmitter
{
	private class DummyDialogueEntry
	{
		public string TitleLocID;

		public string[] TextLocIDArray;
	}

	private static Dictionary<ClassType, DummyDialogueEntry> m_dialogueDict = new Dictionary<ClassType, DummyDialogueEntry>
	{
		{
			ClassType.SwordClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_KNIGHT_SWORD_MOVE_1", "LOC_ID_DUMMY_DIALOGUE_KNIGHT_SWORD_DASH_1", "LOC_ID_DUMMY_DIALOGUE_KNIGHT_BLOCK_PERFECT_1", "LOC_ID_DUMMY_DIALOGUE_KNIGHT_BLOCK_FREE_1" }
			}
		},
		{
			ClassType.BowClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_ARCHER_BOW_AIR_1", "LOC_ID_DUMMY_DIALOGUE_ARCHER_PLATFORM_SPORE_1", "LOC_ID_DUMMY_DIALOGUE_ARCHER_DASH_CANCEL_1", "LOC_ID_DUMMY_DIALOGUE_ARCHER_SPIN_KICK_1" }
			}
		},
		{
			ClassType.AxeClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_BARBARIAN_AXE_AIR_1", "LOC_ID_DUMMY_DIALOGUE_BARBARIAN_AXE_GROUND_1", "LOC_ID_DUMMY_DIALOGUE_BARBARIAN_SHOUT_FREEZE_1", "LOC_ID_DUMMY_DIALOGUE_BARBARIAN_AXE_AIR_CHIP_HIT_1" }
			}
		},
		{
			ClassType.MagicWandClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_MAGE_DOUBLE_HIT_1", "LOC_ID_DUMMY_DIALOGUE_MAGE_SPELL_VARIETY_1", "LOC_ID_DUMMY_DIALOGUE_MAGE_MIXING_1", "LOC_ID_DUMMY_DIALOGUE_MAGE_MANA_LEECH_TARGET_1" }
			}
		},
		{
			ClassType.SaberClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_SABER_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_SABER_BASIC_COMBO_1", "LOC_ID_DUMMY_DIALOGUE_SABER_ROLL_1", "LOC_ID_DUMMY_DIALOGUE_SABER_ROLL_COMBO_1" }
			}
		},
		{
			ClassType.LadleClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[3] { "LOC_ID_DUMMY_DIALOGUE_COOK_ATTACK_REFLECT_1", "LOC_ID_DUMMY_DIALOGUE_COOK_HEAL_1", "LOC_ID_DUMMY_DIALOGUE_COOK_MANA_1" }
			}
		},
		{
			ClassType.SpearClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_SPEAR_DIRECTION_1", "LOC_ID_DUMMY_DIALOGUE_SPEAR_COUNTER_RESET_1", "LOC_ID_DUMMY_DIALOGUE_SPEAR_COUNTER_MANA_1", "LOC_ID_DUMMY_DIALOGUE_SPEAR_BOUNCE_1" }
			}
		},
		{
			ClassType.DualBladesClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[5] { "LOC_ID_DUMMY_DIALOGUE_DUALBLADES_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_DUALBLADES_CRITSTRIKE_1", "LOC_ID_DUMMY_DIALOGUE_DUALBLADES_CLOAK_1", "LOC_ID_DUMMY_DIALOGUE_DUALBLADES_DASH_ATTACK_1", "LOC_ID_DUMMY_DIALOGUE_DUALBLADES_MANA_1" }
			}
		},
		{
			ClassType.GunClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_GUNSLINGER_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_GUNSLINGER_DASH_1", "LOC_ID_DUMMY_DIALOGUE_GUNSLINGER_AMMO_1", "LOC_ID_DUMMY_DIALOGUE_GUNSLINGER_AIR_1" }
			}
		},
		{
			ClassType.LanceClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_LANCE_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_LANCE_CONTROL_MOVEMENT_1", "LOC_ID_DUMMY_DIALOGUE_LANCE_MAGIC_AND_STRENGTH_1", "LOC_ID_DUMMY_DIALOGUE_LANCE_TALENT_FREE_CAST_1" }
			}
		},
		{
			ClassType.BoxingGloveClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[5] { "LOC_ID_DUMMY_DIALOGUE_BOXER_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_BOXER_COMBO_1", "LOC_ID_DUMMY_DIALOGUE_BOXER_THROWING_1", "LOC_ID_DUMMY_DIALOGUE_BOXER_AIMING_1", "LOC_ID_DUMMY_DIALOGUE_BOXER_UPPERCUT_1" }
			}
		},
		{
			ClassType.KatanaClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[5] { "LOC_ID_DUMMY_DIALOGUE_RONIN_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_RONIN_ANGLE_1", "LOC_ID_DUMMY_DIALOGUE_RONIN_CRIT_1", "LOC_ID_DUMMY_DIALOGUE_RONIN_TELESLICE_1", "LOC_ID_DUMMY_DIALOGUE_RONIN_DASH_CANCEL_1" }
			}
		},
		{
			ClassType.LuteClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[6] { "LOC_ID_DUMMY_DIALOGUE_BARD_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_BARD_EXPLODING_NOTE_1", "LOC_ID_DUMMY_DIALOGUE_BARD_DANCE_1", "LOC_ID_DUMMY_DIALOGUE_BARD_DANCE_ADVANCED_1", "LOC_ID_DUMMY_DIALOGUE_BARD_CLIMB_1", "LOC_ID_DUMMY_DIALOGUE_BARD_CRESCENDO_1" }
			}
		},
		{
			ClassType.CannonClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_PIRATE_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_PIRATE_WEAKNESS_COMBO_1", "LOC_ID_DUMMY_DIALOGUE_PIRATE_DESTROY_PROJECTILES_1", "LOC_ID_DUMMY_DIALOGUE_PIRATE_BOAT_MOVEMENT_1" }
			}
		},
		{
			ClassType.AstroClass,
			new DummyDialogueEntry
			{
				TitleLocID = "LOC_ID_NAME_DUMMY_1",
				TextLocIDArray = new string[4] { "LOC_ID_DUMMY_DIALOGUE_ASTROMANCER_INTRO_1", "LOC_ID_DUMMY_DIALOGUE_ASTROMANCER_MULTIPLE_TARGETS_1", "LOC_ID_DUMMY_DIALOGUE_ASTROMANCER_PASSIVE_1", "LOC_ID_DUMMY_DIALOGUE_ASTROMANCER_COMET_1" }
			}
		}
	};

	[SerializeField]
	private SpeechBubbleController m_speechBubble;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioEvent;

	private Dictionary<ClassType, int> m_dialogueIndexDict = new Dictionary<ClassType, int>();

	private NPCController m_npcController;

	private Interactable m_interactable;

	private bool m_endingSpeechBubblePlayed;

	private Action m_closeDummyTips;

	public string Description => ToString();

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

	private bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			return !m_endingSpeechBubblePlayed;
		}
		return !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DummyDialogue_Intro);
	}

	private void Awake()
	{
		ClassType[] typeArray = ClassType_RL.TypeArray;
		foreach (ClassType classType in typeArray)
		{
			if (classType != 0)
			{
				m_dialogueIndexDict.Add(classType, 0);
			}
		}
		m_interactable = GetComponent<Interactable>();
		m_npcController = GetComponent<NPCController>();
		m_closeDummyTips = CloseDummyTips;
	}

	private void OnEnable()
	{
		m_endingSpeechBubblePlayed = false;
		if (PlayerManager.GetCurrentPlayerRoom().BiomeType == BiomeType.Garden)
		{
			m_speechBubble.DisplayOffscreen = false;
		}
		else
		{
			m_speechBubble.DisplayOffscreen = true;
		}
	}

	public void ActivateDialogue()
	{
		if (PlayerManager.IsInstantiated)
		{
			m_interactable.SetIsInteractableActive(isActive: false);
			AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
			if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
			{
				RunEndingDialogue();
			}
			else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DummyDialogue_Intro))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.DummyDialogue_Intro, value: true);
				DialogueManager.StartNewDialogue(m_npcController);
				DialogueManager.AddDialogue("LOC_ID_DUMMY_DIALOGUE_TITLE_1", "LOC_ID_DUMMY_DIALOGUE_INTRO_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
				DialogueManager.AddDialogueCompleteEndHandler(m_closeDummyTips);
			}
			else if (NPCDialogueManager.CanSpeak(m_npcController.NPCType))
			{
				m_npcController.RunNextNPCDialogue(m_closeDummyTips);
			}
			else
			{
				OpenDummyTips();
			}
		}
	}

	private void CloseDummyTips()
	{
		if (m_npcController.CurrentState != NPCState.Idle)
		{
			m_npcController.SetNPCState(NPCState.Idle);
		}
		m_interactable.SetIsInteractableActive(isActive: true);
		AudioManager.PlayOneShotAttached(this, m_farewellAudioEvent, base.gameObject);
	}

	private void OpenDummyTips()
	{
		ClassType classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		bool isFemale = SaveManager.PlayerSaveData.CurrentCharacter.IsFemale;
		if (m_dialogueDict.TryGetValue(classType, out var value))
		{
			int num = Mathf.Clamp(m_dialogueIndexDict[classType], 0, value.TextLocIDArray.Length - 1);
			string titleLocID = value.TitleLocID;
			string textLocID = value.TextLocIDArray[num];
			num++;
			if (num >= value.TextLocIDArray.Length)
			{
				num = 0;
			}
			m_dialogueIndexDict[classType] = num;
			DialogueManager.StartNewDialogue(m_npcController);
			DialogueManager.AddDialogue(titleLocID, textLocID, isFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			DialogueManager.AddDialogueCompleteEndHandler(CloseDummyTips);
		}
		else
		{
			CloseDummyTips();
		}
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_DUMMY_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_DUMMY_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_DUMMY_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_DUMMY_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeDummyTips);
	}
}
