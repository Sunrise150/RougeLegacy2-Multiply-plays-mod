using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;
using UnityEngine.Events;

public class ChallengeShop : MonoBehaviour, IRootObj, IDisplaySpeechBubble, IAudioEventEmitter, IRoomConsumer
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private NPCController m_npcController;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioPath;

	[SerializeField]
	[EventRef]
	private string m_wakeupAudioPath;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioPath;

	[SerializeField]
	private StudioEventEmitter m_snoreEventEmitter;

	private Interactable m_interactable;

	private bool m_isSleeping = true;

	private bool m_isFallingAsleep;

	private bool m_checkForSleep;

	private Coroutine m_sleepCoroutine;

	private WaitRL_Yield m_waitYield;

	private SpeechBubbleController m_speechBubble;

	private static bool m_endingSpeechBubblePlayed;

	private Action m_displayChallengeWindow;

	private Action m_closeChallengeWindow;

	private UnityAction m_closeChallengeWindowUnityEvent;

	public SpeechBubbleType BubbleType
	{
		get
		{
			if (m_isSleeping)
			{
				return SpeechBubbleType.Sleeping;
			}
			if (HasEventDialogue())
			{
				return SpeechBubbleType.Dialogue;
			}
			return SpeechBubbleType.PointOfInterest;
		}
	}

	public bool ShouldDisplaySpeechBubble
	{
		get
		{
			if (!HasEventDialogue() && !HasUnseenChallenges())
			{
				return m_isSleeping;
			}
			return true;
		}
	}

	public BaseRoom Room { get; private set; }

	public string Description => ToString();

	public static bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
			{
				return !m_endingSpeechBubblePlayed;
			}
			return false;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ChallengeDialogue_Intro))
		{
			return true;
		}
		return false;
	}

	private bool HasUnseenChallenges()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			return false;
		}
		ChallengeType[] typeArray = ChallengeType_RL.TypeArray;
		foreach (ChallengeType challengeType in typeArray)
		{
			if (challengeType != 0)
			{
				ChallengeData challengeData = ChallengeLibrary.GetChallengeData(challengeType);
				if ((bool)challengeData && !challengeData.Disabled && ChallengeManager.GetFoundState(challengeType) == FoundState.FoundButNotViewed)
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
		m_speechBubble = GetComponentInChildren<SpeechBubbleController>(includeInactive: true);
		m_displayChallengeWindow = DisplayChallengeWindow;
		m_closeChallengeWindow = CloseChallengeWindow;
		m_closeChallengeWindowUnityEvent = CloseChallengeWindow;
	}

	private void Start()
	{
		m_playerPositionObj.SetActive(value: false);
	}

	private void OnEnable()
	{
		m_isSleeping = true;
		m_isFallingAsleep = false;
		m_npcController.SetNPCState(NPCState.Idle, skipTransition: true);
		m_speechBubble.SetSpeechBubbleType(BubbleType);
		m_interactable.SpeechBubble.SetSpeechBubbleEnabled(enable: true);
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
	}

	public void OpenChallengeMenu()
	{
		m_checkForSleep = false;
		m_isSleeping = false;
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(OpenChallengeMenuCoroutine());
	}

	private IEnumerator OpenChallengeMenuCoroutine()
	{
		m_snoreEventEmitter.Stop();
		if (m_npcController.CurrentState == NPCState.Idle)
		{
			AudioManager.PlayOneShotAttached(this, m_wakeupAudioPath, base.gameObject);
		}
		else
		{
			AudioManager.PlayOneShotAttached(this, m_greetingAudioPath, base.gameObject);
		}
		m_npcController.SetNPCState(NPCState.AtAttention);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToNPC();
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			RunEndingDialogue();
			m_checkForSleep = true;
			yield break;
		}
		bool flag = false;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ChallengeDialogue_Intro))
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
				m_npcController.RunNextNPCDialogue(m_displayChallengeWindow);
			}
			else
			{
				DisplayChallengeWindow();
			}
		}
		m_checkForSleep = true;
	}

	private void RunNPCIntroDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.ChallengeDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_GREEN_HOOD_1", "LOC_ID_CHALLENGE_TEXT_Intro_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayChallengeWindow);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_GERAS_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_GERAS_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_GERAS_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_GERAS_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeChallengeWindow);
	}

	private void DisplayChallengeWindow()
	{
		StartCoroutine(DisplayChallengeWindowCoroutine());
	}

	private IEnumerator DisplayChallengeWindowCoroutine()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ChallengeNPC))
		{
			WindowManager.LoadWindow(WindowID.ChallengeNPC);
		}
		yield return null;
		WindowManager.SetWindowIsOpen(WindowID.ChallengeNPC, isOpen: true);
		WindowManager.GetWindowController(WindowID.ChallengeNPC).WindowClosedEvent.AddListener(m_closeChallengeWindowUnityEvent);
	}

	private IEnumerator MovePlayerToNPC()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		PlayerManager.GetPlayerController().SetFacing(faceRight: false);
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private void CloseChallengeWindow()
	{
		if (WindowManager.GetIsWindowLoaded(WindowID.ChallengeNPC))
		{
			WindowManager.GetWindowController(WindowID.ChallengeNPC).WindowClosedEvent.RemoveListener(m_closeChallengeWindowUnityEvent);
		}
		AudioManager.PlayOneShotAttached(this, m_farewellAudioPath, base.gameObject);
		m_interactable.SetIsInteractableActive(isActive: true);
		m_npcController.SetNPCState(NPCState.AtAttention);
	}

	private void FixedUpdate()
	{
		if (!m_checkForSleep || WindowManager.GetIsWindowOpen(WindowID.Dialogue) || m_isSleeping)
		{
			return;
		}
		if (m_interactable.IsPlayerInTrigger)
		{
			if (m_isFallingAsleep)
			{
				m_isFallingAsleep = false;
				StopCoroutine(m_sleepCoroutine);
			}
		}
		else if (!m_isFallingAsleep)
		{
			m_isFallingAsleep = true;
			m_sleepCoroutine = StartCoroutine(SleepCoroutine());
		}
	}

	private IEnumerator SleepCoroutine()
	{
		float num = UnityEngine.Random.Range(2f, 4f);
		float delay2 = Time.time + num;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_npcController.SetNPCState(NPCState.Idle);
		m_snoreEventEmitter.Play();
		delay2 = Time.time + 0.1f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_isSleeping = true;
		m_isFallingAsleep = false;
		m_speechBubble.SetSpeechBubbleType(BubbleType);
		m_speechBubble.SetSpeechBubbleEnabled(enable: true);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
