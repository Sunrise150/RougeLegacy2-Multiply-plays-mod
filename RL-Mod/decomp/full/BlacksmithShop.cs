using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;
using UnityEngine.Events;

public class BlacksmithShop : MonoBehaviour, IRootObj, IDisplaySpeechBubble, IAudioEventEmitter, IRoomConsumer
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private NPCController m_blacksmith;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	private bool m_isBlacksmithOpen;

	private Interactable m_interactable;

	private WaitRL_Yield m_waitYield;

	private bool m_endingSpeechBubblePlayed;

	private Action m_displayBlacksmithWindow;

	private Action m_blacksmithWindowClosed;

	private UnityAction m_blacksmithWindowClosedUnityEvent;

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
				return HasUnseenGear();
			}
			return true;
		}
	}

	public bool IsBlacksmithOpen => m_isBlacksmithOpen;

	public BaseRoom Room { get; private set; }

	public string Description => ToString();

	private bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			return !m_endingSpeechBubblePlayed;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BlacksmithDialogue_Intro))
		{
			return true;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BlacksmithDialogue_Upgrades))
		{
			bool flag = false;
			EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
			foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
			{
				if (equipmentCategoryType == EquipmentCategoryType.None)
				{
					continue;
				}
				EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
				foreach (EquipmentType equipmentType in typeArray)
				{
					if (equipmentType != 0 && EquipmentManager.GetUpgradeBlueprintsFound(equipmentCategoryType, equipmentType, ignoreInfinitePurchasePower: true) >= 1)
					{
						flag = true;
						break;
					}
				}
				if (flag)
				{
					break;
				}
			}
			if (flag)
			{
				return true;
			}
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BlacksmithDialogue_EquipmentSets))
		{
			bool flag2 = false;
			EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
			foreach (EquipmentType equipmentType2 in typeArray)
			{
				if (equipmentType2 != 0 && EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(equipmentType2) >= 3)
				{
					flag2 = true;
					break;
				}
			}
			if (flag2)
			{
				return true;
			}
		}
		return false;
	}

	private bool HasUnseenGear()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			return false;
		}
		EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
		{
			if (equipmentCategoryType == EquipmentCategoryType.None)
			{
				continue;
			}
			EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
			foreach (EquipmentType equipmentType in typeArray)
			{
				if (equipmentType != 0)
				{
					EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData(equipmentCategoryType, equipmentType);
					if ((bool)equipmentData && !equipmentData.Disabled && EquipmentManager.GetFoundState(equipmentCategoryType, equipmentType) == FoundState.FoundButNotViewed)
					{
						return true;
					}
				}
			}
		}
		return false;
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
	}

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_interactable = GetComponent<Interactable>();
		m_displayBlacksmithWindow = DisplayBlacksmithWindow;
		m_blacksmithWindowClosed = BlacksmithWindowClosed;
		m_blacksmithWindowClosedUnityEvent = BlacksmithWindowClosed;
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
		EndingSpawnRoomTypeController component = Room.GetComponent<EndingSpawnRoomTypeController>();
		if ((bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround)
		{
			m_blacksmith.SetNPCState(NPCState.AtAttention);
		}
	}

	private void Start()
	{
		m_playerPositionObj.SetActive(value: false);
		OnPlayerEnterRoom(null, null);
	}

	public void OpenBlacksmith()
	{
		m_isBlacksmithOpen = true;
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(OpenBlacksmithCoroutine());
	}

	private IEnumerator OpenBlacksmithCoroutine()
	{
		m_blacksmith.SetNPCState(NPCState.AtAttention);
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToBlacksmith();
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			RunEndingDialogue();
			yield break;
		}
		bool flag = false;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BlacksmithDialogue_Intro))
		{
			RunBlacksmithIntroDialogue();
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BlacksmithDialogue_Upgrades))
		{
			bool flag2 = false;
			EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
			foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
			{
				if (equipmentCategoryType == EquipmentCategoryType.None)
				{
					continue;
				}
				EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
				foreach (EquipmentType equipmentType in typeArray)
				{
					if (equipmentType != 0 && EquipmentManager.GetUpgradeBlueprintsFound(equipmentCategoryType, equipmentType, ignoreInfinitePurchasePower: true) >= 1)
					{
						flag2 = true;
						break;
					}
				}
				if (flag2)
				{
					break;
				}
			}
			if (flag2)
			{
				RunBlacksmithUpgradingDialogue();
			}
			else
			{
				flag = true;
			}
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BlacksmithDialogue_EquipmentSets))
		{
			bool flag3 = false;
			EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
			foreach (EquipmentType equipmentType2 in typeArray)
			{
				if (equipmentType2 != 0 && EquipmentManager.Get_EquipmentSet_TotalEquippedLevel(equipmentType2) >= 3)
				{
					flag3 = true;
					break;
				}
			}
			if (flag3)
			{
				RunBlacksmithEquipmentSetsDialogue();
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
			if (NPCDialogueManager.CanSpeak(m_blacksmith.NPCType))
			{
				m_blacksmith.RunNextNPCDialogue(m_displayBlacksmithWindow);
			}
			else
			{
				DisplayBlacksmithWindow();
			}
		}
	}

	private void RunBlacksmithIntroDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.BlacksmithDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_blacksmith, NPCState.AtAttention);
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			DialogueManager.AddDialogue("LOC_ID_NAME_BLACKSMITH_1", "LOC_ID_BLACKSMITH_DIALOGUE_SPECIAL_INTRO_TOPSIDE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			DialogueManager.AddDialogue("LOC_ID_NAME_BLACKSMITH_1", "LOC_ID_BLACKSMITH_DIALOGUE_INTRODUCTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayBlacksmithWindow);
	}

	private void RunBlacksmithUpgradingDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.BlacksmithDialogue_Upgrades, value: true);
		DialogueManager.StartNewDialogue(m_blacksmith, NPCState.AtAttention);
		DialogueManager.AddDialogue("LOC_ID_NAME_BLACKSMITH_1", "LOC_ID_BLACKSMITH_DIALOGUE_EXPLAINING_UPGRADES_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayBlacksmithWindow);
	}

	private void RunBlacksmithEquipmentSetsDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.BlacksmithDialogue_EquipmentSets, value: true);
		DialogueManager.StartNewDialogue(m_blacksmith, NPCState.AtAttention);
		DialogueManager.AddDialogue("LOC_ID_NAME_BLACKSMITH_1", "LOC_ID_BLACKSMITH_DIALOGUE_UNITY_HINT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayBlacksmithWindow);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_blacksmith.IsBestFriend ? "LOC_ID_BLACKSMITH_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_BLACKSMITH_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_blacksmith.IsBestFriend ? "LOC_ID_BLACKSMITH_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_BLACKSMITH_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_blacksmith);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_blacksmith.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_blacksmithWindowClosed);
	}

	private void DisplayBlacksmithWindow()
	{
		StartCoroutine(DisplayBlacksmithCoroutine());
	}

	private IEnumerator DisplayBlacksmithCoroutine()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.Blacksmith))
		{
			WindowManager.LoadWindow(WindowID.Blacksmith);
		}
		yield return null;
		WindowManager.SetWindowIsOpen(WindowID.Blacksmith, isOpen: true);
		WindowManager.GetWindowController(WindowID.Blacksmith).WindowClosedEvent.AddListener(m_blacksmithWindowClosedUnityEvent);
	}

	private IEnumerator MovePlayerToBlacksmith()
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

	private void BlacksmithWindowClosed()
	{
		if (WindowManager.GetIsWindowLoaded(WindowID.Blacksmith))
		{
			WindowManager.GetWindowController(WindowID.Blacksmith).WindowClosedEvent.RemoveListener(m_blacksmithWindowClosedUnityEvent);
		}
		m_isBlacksmithOpen = false;
		m_interactable.SetIsInteractableActive(isActive: true);
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			m_blacksmith.SetNPCState(NPCState.AtAttention);
		}
		else
		{
			m_blacksmith.SetNPCState(NPCState.Idle);
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
