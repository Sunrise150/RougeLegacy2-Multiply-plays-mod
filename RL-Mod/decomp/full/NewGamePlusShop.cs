using System;
using System.Collections;
using System.Runtime.CompilerServices;
using FMODUnity;
using RLAudio;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.Events;

public class NewGamePlusShop : MonoBehaviour, IDisplaySpeechBubble, IRoomConsumer, IAudioEventEmitter, IRootObj
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
	private string m_farewellAudioPath;

	private Interactable m_interactable;

	private static bool m_endingSpeechBubblePlayed;

	private Action m_displayShopWindow;

	private Action m_closeNGPlusShop;

	private Action m_startNewGamePlusTransition;

	private UnityAction m_closeNGPlusShopUnityEvent;

	private bool m_isAboveGround;

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
			return SpeechBubbleType.PointOfInterest;
		}
	}

	public string Description => ToString();

	public static bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			return !m_endingSpeechBubblePlayed;
		}
		bool flag = SaveManager.PlayerSaveData.HighestNGPlusBeaten >= 0;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TimelineDialogue_Intro) && flag)
		{
			return true;
		}
		return false;
	}

	private void Awake()
	{
		m_interactable = GetComponent<Interactable>();
		m_displayShopWindow = DisplayShopWindow;
		m_closeNGPlusShop = CloseNGPlusShop;
		m_startNewGamePlusTransition = StartNewGamePlusTransition;
		m_closeNGPlusShopUnityEvent = CloseNGPlusShop;
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnter);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnter);
		}
	}

	private void OnPlayerEnter(object sender, EventArgs args)
	{
		m_endingSpeechBubblePlayed = false;
		EndingSpawnRoomTypeController component = Room.GetComponent<EndingSpawnRoomTypeController>();
		m_isAboveGround = (bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround;
		if (m_isAboveGround)
		{
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Timeline_Unlocked))
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (SaveManager.PlayerSaveData.HighestNGPlusBeaten < 0 && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Timeline_Unlocked))
		{
			base.gameObject.SetActive(value: false);
			PropSpawnController propSpawnController = Room.gameObject.FindObjectReference<PropSpawnController>("RedHoodPlaque");
			if ((bool)propSpawnController)
			{
				propSpawnController.PropInstance.gameObject.SetActive(value: false);
			}
		}
	}

	public void OpenNGPlusShop()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		NewGamePlusOmniUIWindowController.EnteringNGPlus = false;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Timeline_Unlocked))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.Timeline_Unlocked, value: true);
		}
		StopAllCoroutines();
		StartCoroutine(OpenNGPlusShopCoroutine());
	}

	private IEnumerator OpenNGPlusShopCoroutine()
	{
		AudioManager.PlayOneShotAttached(this, m_greetingAudioPath, base.gameObject);
		m_npcController.SetNPCState(NPCState.AtAttention);
		yield return MovePlayerToRedHood();
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			RunEndingDialogue();
			yield break;
		}
		bool flag = false;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TimelineDialogue_Intro))
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
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.TimelineDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_RED_HOOD_1", "LOC_ID_NG_TEXT_INTRO_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_displayShopWindow);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_ELPIS_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_ELPIS_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_ELPIS_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_ELPIS_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeNGPlusShop);
	}

	private void DisplayShopWindow()
	{
		StartCoroutine(DisplayShopWindowCoroutine());
	}

	private IEnumerator DisplayShopWindowCoroutine()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.NewGamePlusNPC))
		{
			WindowManager.LoadWindow(WindowID.NewGamePlusNPC);
		}
		yield return null;
		WindowManager.GetWindowController(WindowID.NewGamePlusNPC).WindowClosedEvent.AddListener(m_closeNGPlusShopUnityEvent);
		WindowManager.SetWindowIsOpen(WindowID.NewGamePlusNPC, isOpen: true);
	}

	public void CloseNGPlusShop()
	{
		if (WindowManager.GetIsWindowLoaded(WindowID.NewGamePlusNPC))
		{
			WindowManager.GetWindowController(WindowID.NewGamePlusNPC).WindowClosedEvent.RemoveListener(m_closeNGPlusShopUnityEvent);
		}
		AudioManager.PlayOneShotAttached(this, m_farewellAudioPath, base.gameObject);
		if (NewGamePlusOmniUIWindowController.EnteringNGPlus)
		{
			StartCoroutine(StartNewGamePlusCoroutine());
			return;
		}
		m_npcController.SetNPCState(NPCState.Idle);
		m_interactable.SetIsInteractableActive(isActive: true);
	}

	private void OnDisable()
	{
		if ((bool)m_interactable)
		{
			m_interactable.SetIsInteractableActive(isActive: true);
		}
	}

	private IEnumerator StartNewGamePlusCoroutine()
	{
		SaveManager.PlayerSaveData.ResetForNewGamePlus(SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround);
		SaveManager.StageSaveData.ForceResetWorld = true;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerExitHubTown, this, EventArgs.Empty);
		SaveManager.PlayerSaveData.GiveMoneyToCharon(this);
		SaveManager.SaveAllCurrentProfileGameData(SavingType.FileOnly, saveStageData: true, ignoreTimeRestriction: true);
		WindowManager.SetWindowIsOpen(WindowID.NewGamePlusNPC, isOpen: false);
		while (WindowManager.IsAnyWindowOpen)
		{
			yield return null;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		float delay = Time.time + 0.25f;
		while (Time.time < delay)
		{
			yield return null;
		}
		int num = UnityEngine.Random.Range(0, NPCDialogue_EV.NEW_GAME_PLUS_ACTIVATE_DIALOGUES.Length);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), NPCDialogue_EV.NEW_GAME_PLUS_ACTIVATE_DIALOGUES[num], SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		DialogueManager.AddDialogueCompleteEndHandler(m_startNewGamePlusTransition);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
	}

	private void StartNewGamePlusTransition()
	{
		SceneLoader_RL.LoadScene(SceneID.Tutorial, TransitionID.NewGamePlus);
	}

	private IEnumerator TeleportPlayerToHubTown()
	{
		float delay = Time.time + 1f;
		while (Time.time < delay)
		{
			yield return null;
		}
		TunnelSpawnController tunnelSpawnController = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<TunnelSpawnController>("EntranceTunnel");
		if ((bool)tunnelSpawnController && !tunnelSpawnController.Tunnel.IsNativeNull())
		{
			tunnelSpawnController.Tunnel.ForceEnterTunnel(animate: false);
		}
	}

	private IEnumerator MovePlayerToRedHood()
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
