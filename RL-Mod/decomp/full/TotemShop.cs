using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;
using UnityEngine.Events;

public class TotemShop : MonoBehaviour, IRootObj, IDisplaySpeechBubble, IAudioEventEmitter
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioEvent;

	private WaitRL_Yield m_waitYield;

	private Interactable m_interactable;

	private NPCController m_npcController;

	private bool m_endingSpeechBubblePlayed;

	private Prop m_prop;

	private Action m_displayTotemShop;

	private Action m_closeTotemShop;

	private UnityAction m_closeTotemShopUnityEvent;

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

	public string Description => ToString();

	private bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			return !m_endingSpeechBubblePlayed;
		}
		return !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TotemDialogue_Intro);
	}

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_interactable = GetComponent<Interactable>();
		m_npcController = GetComponent<NPCController>();
		m_prop = GetComponent<Prop>();
		m_displayTotemShop = DisplayTotemShop;
		m_closeTotemShop = CloseTotemShop;
		m_closeTotemShopUnityEvent = CloseTotemShop;
	}

	private void OnEnable()
	{
		m_endingSpeechBubblePlayed = false;
	}

	public void OpenTotemShop()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(OpenTotemCoroutine());
	}

	private IEnumerator OpenTotemCoroutine()
	{
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToTotem();
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			RunEndingDialogue();
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TotemDialogue_Intro))
		{
			RunTotemIntroDialogue();
		}
		else if (NPCDialogueManager.CanSpeak(m_npcController.NPCType))
		{
			m_npcController.RunNextNPCDialogue(m_closeTotemShop);
		}
		else
		{
			RunTotemStatsDialogue();
		}
	}

	private IEnumerator MovePlayerToTotem()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (playerController.transform.position.x > base.transform.position.x)
		{
			playerController.SetFacing(faceRight: false);
		}
		else
		{
			playerController.SetFacing(faceRight: true);
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private void RunTotemIntroDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.TotemDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), "LOC_ID_TOTEM_DIALOGUE_INTRO_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayTotemShop);
	}

	private void RunTotemStatsDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.TotemDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), "LOC_ID_TOTEM_DIALOGUE_STATS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayTotemShop);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_TOTEM_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_TOTEM_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_TOTEM_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_TOTEM_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeTotemShop);
	}

	private void DisplayTotemShop()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.Totem))
		{
			WindowManager.LoadWindow(WindowID.Totem);
		}
		WindowManager.SetWindowIsOpen(WindowID.Totem, isOpen: true);
		WindowManager.GetWindowController(WindowID.Totem).WindowClosedEvent.AddListener(m_closeTotemShopUnityEvent);
	}

	private void CloseTotemShop()
	{
		if (WindowManager.GetIsWindowLoaded(WindowID.Totem))
		{
			WindowManager.GetWindowController(WindowID.Totem).WindowClosedEvent.RemoveListener(m_closeTotemShopUnityEvent);
		}
		AudioManager.PlayOneShotAttached(this, m_farewellAudioEvent, base.gameObject);
		m_interactable.SetIsInteractableActive(isActive: true);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
