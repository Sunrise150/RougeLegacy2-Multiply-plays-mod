using System;
using System.Collections;
using FMODUnity;
using RLAudio;
using RL_Windows;
using TMPro;
using UnityEngine;

public class ArchitectShop : MonoBehaviour, IDisplaySpeechBubble, IRoomConsumer, IAudioEventEmitter
{
	private const int NPC_ANIM_LAYER = 0;

	private const int DRILL_ANIM_LAYER = 1;

	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private TMP_Text m_temporaryLockText;

	[SerializeField]
	private NPCController m_npcController;

	[SerializeField]
	private GameObject[] m_drillGOs;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_mechanismLockAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_mechanismUnlockAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_interactOpenAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_selectLockOnceAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_selectLockAlwaysAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_selectDoNotLockAudioEvent;

	private bool m_canLockCastle;

	private Interactable m_interactable;

	private bool m_endingSpeechBubblePlayed;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeClosed;

	private Action m_closeArchitectShop;

	private Action m_displayArchitectWindow;

	private Action m_runArchitectFailedIntroDialogue;

	private Action m_cancelConfirmMenu;

	private Action m_unlockCastleConfirmMenu;

	private Action m_temporaryCastleLockConfirmMenu;

	private Action m_permanentCastleLockConfirmMenu;

	public BaseRoom Room { get; private set; }

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public string Description => ToString();

	private bool HasEventDialogue()
	{
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
			{
				return !m_endingSpeechBubblePlayed;
			}
			return false;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ArchitectDialogue_Intro))
		{
			return true;
		}
		return false;
	}

	private void Awake()
	{
		m_interactable = GetComponent<Interactable>();
		m_onSkillTreeClosed = OnSkillTreeClosed;
		m_closeArchitectShop = CloseArchitectShop;
		m_displayArchitectWindow = DisplayArchitectWindow;
		m_runArchitectFailedIntroDialogue = RunArchitectFailedIntroDialogue;
		m_cancelConfirmMenu = CancelConfirmMenu;
		m_unlockCastleConfirmMenu = UnlockCastleConfirmMenu;
		m_temporaryCastleLockConfirmMenu = TemporaryCastleLockConfirmMenu;
		m_permanentCastleLockConfirmMenu = PermanentCastleLockConfirmMenu;
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

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_onSkillTreeClosed);
	}

	private void Start()
	{
		OnPlayerEnterRoom(null, null);
	}

	private void OnPlayerEnterRoom(object sender, EventArgs args)
	{
		m_endingSpeechBubblePlayed = false;
		EndingSpawnRoomTypeController component = Room.GetComponent<EndingSpawnRoomTypeController>();
		bool flag = (bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround;
		if (flag)
		{
			GameObject[] drillGOs = m_drillGOs;
			foreach (GameObject gameObject in drillGOs)
			{
				if (gameObject.activeSelf)
				{
					gameObject.SetActive(value: false);
				}
			}
		}
		else
		{
			GameObject[] drillGOs = m_drillGOs;
			foreach (GameObject gameObject2 in drillGOs)
			{
				if (!gameObject2.activeSelf)
				{
					gameObject2.SetActive(value: true);
				}
			}
		}
		Vector3 localScale = m_temporaryLockText.transform.localScale;
		if (m_temporaryLockText.transform.lossyScale.x < 0f)
		{
			localScale.x *= -1f;
		}
		m_temporaryLockText.transform.localScale = localScale;
		m_canLockCastle = SaveManager.DoesSaveExist(SaveManager.CurrentProfile, SaveDataType.Stage, checkForBackup: false) && !SaveManager.StageSaveData.ForceResetWorld;
		if (!SaveManager.IsRunning && m_canLockCastle)
		{
			Debug.Log("<color=red>WARNING: Your Kingdom is locked while the SaveManager is not running. This can happen in the Unity Editor. If you are playing in the editor, note that locking the castle will load the save file ON DISK, not the castle you just played, regardless of whether the castle is outdated or corrupt.</color>");
		}
		if (!m_canLockCastle)
		{
			SaveManager.PlayerSaveData.CastleLockState = CastleLockState.NotLocked;
		}
		InitializeDrillState(flag);
		UpdateText();
	}

	private void OnSkillTreeClosed(object sender, EventArgs args)
	{
		UpdateText();
	}

	private void InitializeDrillState(bool isInAboveGroundRoom)
	{
		if (SaveManager.PlayerSaveData.CastleLockState == CastleLockState.NotLocked || isInAboveGroundRoom)
		{
			m_npcController.Animator.SetBool("Vibrate", value: false);
			m_npcController.Animator.Play("Architect_Drill_Unlocked", 1, 1f);
		}
		else
		{
			m_npcController.Animator.SetBool("Vibrate", value: true);
			m_npcController.Animator.Play("Architect_Drill_Locked", 1, 1f);
		}
	}

	private void UpdateText()
	{
		float num = 0f - (1f - NPC_EV.GetArchitectGoldMod(SaveManager.PlayerSaveData.TimesCastleLocked + 1));
		string text = string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), Mathf.RoundToInt(num * 100f));
		switch (SaveManager.PlayerSaveData.CastleLockState)
		{
		case CastleLockState.NotLocked:
			m_temporaryLockText.text = "";
			break;
		case CastleLockState.TemporaryLock:
		case CastleLockState.PermanentLock:
			m_temporaryLockText.text = "[Coin_Icon] " + text;
			break;
		}
	}

	public void OpenArchitect()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		StopAllCoroutines();
		StartCoroutine(OpenArchitectCoroutine());
	}

	public void CloseArchitectShop()
	{
		if (m_npcController.CurrentState != NPCState.Idle)
		{
			m_npcController.SetNPCState(NPCState.Idle);
		}
		AudioManager.PlayOneShotAttached(this, m_farewellAudioEvent, base.gameObject);
		m_interactable.SetIsInteractableActive(isActive: true);
	}

	private IEnumerator OpenArchitectCoroutine()
	{
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		m_npcController.SetNPCState(NPCState.AtAttention);
		yield return MovePlayerToArchitect();
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			RunEndingDialogue();
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ArchitectDialogue_Intro))
		{
			RunArchitectIntroDialogue();
		}
		else if (NPCDialogueManager.CanSpeak(m_npcController.NPCType))
		{
			m_npcController.RunNextNPCDialogue(m_closeArchitectShop);
		}
		else
		{
			DisplayArchitectWindow();
		}
	}

	private void DisplayArchitectWindow()
	{
		if (m_canLockCastle)
		{
			InitializeArchitectConfirmMenu();
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: true);
		}
		else
		{
			RunArchitectFailedIntroDialogue();
		}
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_ARCHITECT_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_ARCHITECT_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_ARCHITECT_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_ARCHITECT_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeArchitectShop);
	}

	private void RunArchitectFailedIntroDialogue()
	{
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_ARCHITECT_1", "LOC_ID_ARCHITECT_DIALOGUE_FAILED_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeArchitectShop);
	}

	private void RunArchitectIntroDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.ArchitectDialogue_Intro, value: true);
		DialogueManager.StartNewDialogue(m_npcController, NPCState.AtAttention);
		DialogueManager.AddDialogue("LOC_ID_NAME_ARCHITECT_1", "LOC_ID_ARCHITECT_DIALOGUE_INTRODUCTION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		if (m_canLockCastle)
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_displayArchitectWindow);
		}
		else
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_runArchitectFailedIntroDialogue);
		}
	}

	private void InitializeArchitectConfirmMenu()
	{
		string arg = Mathf.RoundToInt((1f - NPC_EV.GetArchitectGoldMod(SaveManager.PlayerSaveData.TimesCastleLocked + 1)) * 100f).ToString();
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenuBig) as ConfirmMenuWindowController;
		obj.SetTitleText(string.Format(LocalizationManager.GetString("LOC_ID_ARCHITECT_SMALL_TALK_LOCK_CASTLE_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale)), isLocID: false);
		obj.SetDescriptionText(string.Format(LocalizationManager.GetString("LOC_ID_ARCHITECT_SMALL_TALK_LOCK_CASTLE_TEXT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), arg), isLocID: false);
		obj.SetNumberOfButtons(3);
		obj.SetOnCancelAction(m_cancelConfirmMenu);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_ARCHITECT_SMALL_TALK_CANCEL_LOCK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_unlockCastleConfirmMenu);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_ARCHITECT_SMALL_TALK_LOCK_ONCE_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_temporaryCastleLockConfirmMenu);
		ConfirmMenu_Button buttonAtIndex3 = obj.GetButtonAtIndex(2);
		buttonAtIndex3.SetButtonText("LOC_ID_ARCHITECT_SMALL_TALK_LOCK_FOREVER_1", isLocID: true);
		buttonAtIndex3.SetOnClickAction(m_permanentCastleLockConfirmMenu);
		AudioManager.PlayOneShot(this, m_interactOpenAudioEvent);
	}

	private void CancelConfirmMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		CloseArchitectShop();
	}

	private void UnlockCastleConfirmMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		StartCoroutine(SetLockState(CastleLockState.NotLocked));
		AudioManager.PlayOneShot(this, m_selectDoNotLockAudioEvent);
	}

	private void TemporaryCastleLockConfirmMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		StartCoroutine(SetLockState(CastleLockState.TemporaryLock));
		AudioManager.PlayOneShot(this, m_selectLockOnceAudioEvent);
	}

	private void PermanentCastleLockConfirmMenu()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		StartCoroutine(SetLockState(CastleLockState.PermanentLock));
		AudioManager.PlayOneShot(this, m_selectLockAlwaysAudioEvent);
	}

	private IEnumerator SetLockState(CastleLockState lockState)
	{
		m_npcController.SetNPCState(NPCState.Idle);
		CastleLockState castleLockState = SaveManager.PlayerSaveData.CastleLockState;
		SaveManager.PlayerSaveData.CastleLockState = lockState;
		UpdateText();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
		if (castleLockState != lockState)
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
			if (lockState == CastleLockState.NotLocked)
			{
				m_npcController.Animator.SetBool("Vibrate", value: false);
				m_npcController.Animator.SetTrigger("Retract");
				AudioManager.PlayOneShotAttached(this, m_mechanismUnlockAudioEvent, base.gameObject);
				yield return null;
				while (m_npcController.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1f)
				{
					yield return null;
				}
			}
			else if (castleLockState == CastleLockState.NotLocked)
			{
				m_npcController.Animator.SetBool("Vibrate", value: true);
				m_npcController.Animator.SetTrigger("Fire");
				AudioManager.PlayOneShotAttached(this, m_mechanismLockAudioEvent, base.gameObject);
				yield return null;
				while (m_npcController.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1f)
				{
					yield return null;
				}
			}
			else
			{
				m_npcController.Animator.SetBool("Vibrate", value: false);
				m_npcController.Animator.SetTrigger("Retract");
				AudioManager.PlayOneShotAttached(this, m_mechanismUnlockAudioEvent, base.gameObject);
				yield return null;
				while (m_npcController.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1f)
				{
					yield return null;
				}
				m_npcController.Animator.SetBool("Vibrate", value: true);
				m_npcController.Animator.SetTrigger("Fire");
				AudioManager.PlayOneShotAttached(this, m_mechanismLockAudioEvent, base.gameObject);
				yield return null;
				while (m_npcController.Animator.GetCurrentAnimatorStateInfo(1).normalizedTime < 1f)
				{
					yield return null;
				}
			}
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		CloseArchitectShop();
	}

	private IEnumerator MovePlayerToArchitect()
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
}
