using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;
using UnityEngine.Events;

public class EnchantressShop : MonoBehaviour, IRootObj, IDisplaySpeechBubble, IAudioEventEmitter
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private NPCController m_enchantress;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	private bool m_isEnchantressOpen;

	private WaitRL_Yield m_waitYield;

	private Interactable m_interactable;

	private bool m_endingSpeechBubblePlayed;

	private Action m_displayEnchantressWindow;

	private Action m_enchantressWindowClosed;

	private UnityAction m_enchantressWindowClosedUnityEvent;

	public SpeechBubbleType BubbleType
	{
		get
		{
			if (HasEventDialogue())
			{
				return SpeechBubbleType.Dialogue;
			}
			return SpeechBubbleType.GearAvailable;
		}
	}

	public bool ShouldDisplaySpeechBubble
	{
		get
		{
			if (!HasEventDialogue())
			{
				return HasUnseenRunes();
			}
			return true;
		}
	}

	public bool IsEnchantressOpen => m_isEnchantressOpen;

	public BaseRoom Room { get; private set; }

	public string Description => ToString();

	private bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			return !m_endingSpeechBubblePlayed;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.EnchantressDialogue_Intro))
		{
			return true;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.EnchantressDialogue_Stacking))
		{
			bool flag = false;
			RuneType[] typeArray = RuneType_RL.TypeArray;
			foreach (RuneType runeType in typeArray)
			{
				if (runeType != 0 && RuneManager.GetUpgradeBlueprintsFound(runeType, ignoreInfinitePurchasePower: true) > 1)
				{
					flag = true;
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasUnseenRunes()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			return false;
		}
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType != 0)
			{
				RuneData runeData = RuneLibrary.GetRuneData(runeType);
				if ((bool)runeData && !runeData.Disabled && RuneManager.GetFoundState(runeType) == FoundState.FoundButNotViewed)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_interactable = GetComponent<Interactable>();
		m_displayEnchantressWindow = DisplayEnchantressWindow;
		m_enchantressWindowClosed = EnchantressWindowClosed;
		m_enchantressWindowClosedUnityEvent = EnchantressWindowClosed;
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		m_endingSpeechBubblePlayed = false;
	}

	private void Start()
	{
		m_playerPositionObj.SetActive(value: false);
		OnPlayerEnterRoom(null, null);
	}

	public void OpenEnchantress()
	{
		m_isEnchantressOpen = true;
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(OpenEnchantressCoroutine());
	}

	private IEnumerator OpenEnchantressCoroutine()
	{
		m_enchantress.SetNPCState(NPCState.AtAttention);
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToEnchantress();
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			RunEndingDialogue();
			yield break;
		}
		bool flag = false;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.EnchantressDialogue_Intro))
		{
			RunEnchantressIntroDialogue();
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.EnchantressDialogue_Stacking))
		{
			bool flag2 = false;
			RuneType[] typeArray = RuneType_RL.TypeArray;
			foreach (RuneType runeType in typeArray)
			{
				if (runeType != 0 && RuneManager.GetUpgradeBlueprintsFound(runeType, ignoreInfinitePurchasePower: true) > 1)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				RunEnchantressStackingDialogue();
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			if (NPCDialogueManager.CanSpeak(m_enchantress.NPCType))
			{
				m_enchantress.RunNextNPCDialogue(m_displayEnchantressWindow);
			}
			else
			{
				DisplayEnchantressWindow();
			}
		}
	}

	private void RunEnchantressIntroDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.EnchantressDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_enchantress, NPCState.AtAttention);
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			DialogueManager.AddDialogue("LOC_ID_NAME_ENCHANTRESS_1", "LOC_ID_ENCHANTRESS_DIALOGUE_SPECIAL_INTRO_TOPSIDE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			DialogueManager.AddDialogue("LOC_ID_NAME_ENCHANTRESS_1", "LOC_ID_ENCHANTRESS_DIALOGUE_INTRODUCTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayEnchantressWindow);
	}

	private void RunEnchantressStackingDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.EnchantressDialogue_Stacking, value: true);
		DialogueManager.StartNewDialogue(m_enchantress, NPCState.AtAttention);
		DialogueManager.AddDialogue("LOC_ID_NAME_ENCHANTRESS_1", "LOC_ID_ENCHANTRESS_DIALOGUE_EXPLAINING_STACKING_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayEnchantressWindow);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_enchantress.IsBestFriend ? "LOC_ID_ENCHANTRESS_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_ENCHANTRESS_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_enchantress.IsBestFriend ? "LOC_ID_ENCHANTRESS_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_ENCHANTRESS_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_enchantress);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_enchantress.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_enchantressWindowClosed);
	}

	private void DisplayEnchantressWindow()
	{
		StartCoroutine(DisplayEnchantressWindowCoroutine());
	}

	private IEnumerator DisplayEnchantressWindowCoroutine()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.Enchantress))
		{
			WindowManager.LoadWindow(WindowID.Enchantress);
		}
		yield return null;
		WindowManager.SetWindowIsOpen(WindowID.Enchantress, isOpen: true);
		WindowManager.GetWindowController(WindowID.Enchantress).WindowClosedEvent.AddListener(m_enchantressWindowClosedUnityEvent);
	}

	private IEnumerator MovePlayerToEnchantress()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			playerController.SetFacing(faceRight: false);
		}
		else
		{
			playerController.SetFacing(faceRight: true);
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private void EnchantressWindowClosed()
	{
		if (WindowManager.GetIsWindowLoaded(WindowID.Enchantress))
		{
			WindowManager.GetWindowController(WindowID.Enchantress).WindowClosedEvent.RemoveListener(m_enchantressWindowClosedUnityEvent);
		}
		m_isEnchantressOpen = false;
		m_interactable.SetIsInteractableActive(isActive: true);
		m_enchantress.SetNPCState(NPCState.Idle);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
