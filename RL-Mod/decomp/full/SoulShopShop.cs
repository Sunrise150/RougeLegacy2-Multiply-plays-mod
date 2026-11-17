using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;
using UnityEngine.Events;

public class SoulShopShop : MonoBehaviour, IDisplaySpeechBubble, IRootObj, IAudioEventEmitter, IRoomConsumer
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private NPCController m_npcController;

	[SerializeField]
	private SpeechBubbleController m_speechBubble;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	private Interactable m_interactable;

	private bool m_endingSpeechBubblePlayed;

	private Action m_displayShopWindow;

	private Action m_closeSoulShop;

	private UnityAction m_closeSoulShopUnityEvent;

	public string Description => ToString();

	public BaseRoom Room { get; private set; }

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

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

	private bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			return !m_endingSpeechBubblePlayed;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SoulShopDialogue_Intro))
		{
			return true;
		}
		return false;
	}

	private void Awake()
	{
		m_interactable = GetComponent<Interactable>();
		m_displayShopWindow = DisplayShopWindow;
		m_closeSoulShop = CloseSoulShop;
		m_closeSoulShopUnityEvent = CloseSoulShop;
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
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
		StartCoroutine(SetSpeechBubbleCoroutine());
	}

	private IEnumerator SetSpeechBubbleCoroutine()
	{
		yield return null;
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			m_speechBubble.DisplayOffscreen = false;
		}
		else
		{
			m_speechBubble.DisplayOffscreen = true;
		}
	}

	public void OpenSoulShop()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SoulShop_Unlocked))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.SoulShop_Unlocked, value: true);
		}
		StopAllCoroutines();
		StartCoroutine(OpenSoulShopCoroutine());
	}

	private IEnumerator OpenSoulShopCoroutine()
	{
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		m_npcController.SetNPCState(NPCState.AtAttention);
		yield return MovePlayerToNPC();
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			RunEndingDialogue();
			yield break;
		}
		bool flag = false;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SoulShopDialogue_Intro))
		{
			RunNPCIntroDialogue();
		}
		else
		{
			flag = true;
		}
		if (flag)
		{
			if (NPCDialogueManager.CanSpeak(m_npcController.NPCType))
			{
				m_npcController.RunNextNPCDialogue(m_displayShopWindow);
			}
			else
			{
				DisplayShopWindow();
			}
		}
	}

	private void RunNPCIntroDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.SoulShopDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_SOULSHOP_HOOD_1", "LOC_ID_SOULSHOP_MISC_OWNER_INTRO_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayShopWindow);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_KERES_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_KERES_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_KERES_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_KERES_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeSoulShop);
	}

	private void DisplayShopWindow()
	{
		StartCoroutine(DisplayShopWindowCoroutine());
	}

	private IEnumerator DisplayShopWindowCoroutine()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.SoulShop))
		{
			WindowManager.LoadWindow(WindowID.SoulShop);
		}
		yield return null;
		WindowManager.GetWindowController(WindowID.SoulShop).WindowClosedEvent.AddListener(m_closeSoulShopUnityEvent);
		WindowManager.SetWindowIsOpen(WindowID.SoulShop, isOpen: true);
	}

	public void CloseSoulShop()
	{
		if (WindowManager.GetIsWindowLoaded(WindowID.SoulShop))
		{
			WindowManager.GetWindowController(WindowID.SoulShop).WindowClosedEvent.RemoveListener(m_closeSoulShopUnityEvent);
		}
		m_interactable.SetIsInteractableActive(isActive: true);
		m_npcController.SetNPCState(NPCState.Idle);
	}

	private void OnDisable()
	{
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(isActive: true);
		}
	}

	private IEnumerator MovePlayerToNPC()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (base.transform.lossyScale.x > 0f)
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: false);
		}
		else
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		}
		float startTime = Time.time;
		while (Time.time < startTime + 0.25f)
		{
			yield return null;
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
