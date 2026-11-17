using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using RLAudio;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class PizzaGirlPropController : BaseSpecialPropController, IDisplaySpeechBubble, IAudioEventEmitter
{
	private enum PizzaGirlState
	{
		None,
		InTransitionRoom,
		InUnlockRoom,
		InHubTown,
		InCloset,
		InAboveGround
	}

	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioEvent;

	private PizzaGirlState m_currentState;

	private WaitRL_Yield m_waitYield;

	private byte m_genericDialogueIndex;

	private Prop m_prop;

	private EventInstance m_elevatorSFXInstance;

	private bool m_oneTimeSpeechBubblePlayed;

	private bool m_finalJonahSpeechPlayed;

	private GoldChangedEventArgs m_goldChangedEventArgs;

	private ObjectiveCompleteHUDEventArgs m_objectiveCompleteArgs;

	private Action m_endInteraction;

	private Action m_pizzaGirlUnlockedSkillTreePopup;

	private Action m_displayUnlockTeleporterConfirmMenu;

	private Action m_displayUnlockTeleporterObjectiveCompleteHUD;

	private Action m_initializeTeleportToManorConfirmMenu;

	private Action m_cancelConfirmMenuSelection;

	private Action m_unlockTeleporter;

	private Action m_cancelTeleportToManor;

	private Action m_teleportToManor;

	private NPCController m_npcController;

	private BaseEffect m_shakeEffect;

	public SpeechBubbleType BubbleType
	{
		get
		{
			if (SaveManager.PlayerSaveData.EndingSpawnRoom >= EndingSpawnRoomType.Manor && m_currentState == PizzaGirlState.InHubTown)
			{
				return SpeechBubbleType.TeleportToManor_PizzaGirlOnly;
			}
			return SpeechBubbleType.Dialogue;
		}
	}

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

	public string Description => ToString();

	private bool HasEventDialogue()
	{
		switch (m_currentState)
		{
		case PizzaGirlState.InTransitionRoom:
			if (!SaveManager.PlayerSaveData.GetTeleporterIsUnlocked(base.Room.BiomeType) && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirl_UnlockTeleporter_Dialogue_Intro))
			{
				return true;
			}
			break;
		case PizzaGirlState.InUnlockRoom:
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked))
			{
				return true;
			}
			break;
		case PizzaGirlState.InHubTown:
			if (SaveManager.PlayerSaveData.EndingSpawnRoom >= EndingSpawnRoomType.Manor)
			{
				return true;
			}
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirl_Dock_Dialogue_Intro))
			{
				return true;
			}
			break;
		case PizzaGirlState.InCloset:
		case PizzaGirlState.InAboveGround:
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
			{
				if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenTrueEnding))
				{
					return !m_oneTimeSpeechBubblePlayed;
				}
				return !m_finalJonahSpeechPlayed;
			}
			return false;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_goldChangedEventArgs = new GoldChangedEventArgs(0, 0);
		m_objectiveCompleteArgs = new ObjectiveCompleteHUDEventArgs(ObjectiveCompleteHUDType.PizzaGirl);
		m_npcController = GetComponent<NPCController>();
		m_prop = GetComponent<Prop>();
		m_endInteraction = EndInteraction;
		m_pizzaGirlUnlockedSkillTreePopup = PizzaGirlUnlockedSkillTreePopup;
		m_displayUnlockTeleporterConfirmMenu = DisplayUnlockTeleporterConfirmMenu;
		m_displayUnlockTeleporterObjectiveCompleteHUD = DisplayUnlockTeleporterObjectiveCompleteHUD;
		m_initializeTeleportToManorConfirmMenu = InitializeTeleportToManorConfirmMenu;
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_unlockTeleporter = UnlockTeleporter;
		m_cancelTeleportToManor = CancelTeleportToManor;
		m_teleportToManor = TeleportToManor;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if ((bool)m_shakeEffect && !m_shakeEffect.IsFreePoolObj)
		{
			m_shakeEffect.Stop(EffectStopType.Immediate);
		}
		m_shakeEffect = null;
		if (m_elevatorSFXInstance.isValid())
		{
			AudioManager.Stop(m_elevatorSFXInstance, FMOD.Studio.STOP_MODE.IMMEDIATE);
		}
	}

	private void OnDestroy()
	{
		if (m_elevatorSFXInstance.isValid())
		{
			m_elevatorSFXInstance.release();
		}
	}

	protected override void InitializePooledPropOnEnter()
	{
		if (!m_elevatorSFXInstance.isValid())
		{
			m_elevatorSFXInstance = AudioUtility.GetEventInstance("event:/UI/FrontEnd/ui_fe_ending_pizzaGirl_room_elevator_move_loop", base.transform);
		}
		m_oneTimeSpeechBubblePlayed = false;
		m_finalJonahSpeechPlayed = false;
		m_currentState = PizzaGirlState.None;
		EndingSpawnRoomTypeController component = base.Room.GetComponent<EndingSpawnRoomTypeController>();
		if ((bool)component)
		{
			if (component.EndingSpawnRoomType == EndingSpawnRoomType.Closet)
			{
				m_currentState = PizzaGirlState.InCloset;
			}
			else if (component.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround)
			{
				m_currentState = PizzaGirlState.InAboveGround;
			}
		}
		if (m_currentState == PizzaGirlState.None)
		{
			bool flag = false;
			if ((WorldBuilder.IsInstantiated && WorldBuilder.BiomeControllers != null && WorldBuilder.BiomeControllers.ContainsKey(BiomeType.HubTown)) || flag)
			{
				m_currentState = PizzaGirlState.InHubTown;
			}
			else if (base.Room.RoomType == RoomType.Transition)
			{
				m_currentState = PizzaGirlState.InTransitionRoom;
			}
			else
			{
				m_currentState = PizzaGirlState.InUnlockRoom;
			}
		}
		switch (m_currentState)
		{
		case PizzaGirlState.InTransitionRoom:
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked) || SaveManager.PlayerSaveData.GetTeleporterIsUnlocked(base.Room.BiomeType))
			{
				base.gameObject.SetActive(value: false);
			}
			break;
		case PizzaGirlState.InUnlockRoom:
			if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked) && !base.IsPropComplete)
			{
				base.gameObject.SetActive(value: false);
			}
			break;
		case PizzaGirlState.InHubTown:
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked))
			{
				base.gameObject.SetActive(value: false);
			}
			break;
		}
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
	}

	public void TalkToPizzaGirl()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(TalkToPizzaGirlCoroutine());
	}

	private IEnumerator TalkToPizzaGirlCoroutine()
	{
		m_npcController.SetNPCState(NPCState.AtAttention);
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToPizzaGirl();
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		switch (m_currentState)
		{
		case PizzaGirlState.InTransitionRoom:
			PlayUnlockTeleporterDialogue();
			break;
		case PizzaGirlState.InUnlockRoom:
			PlayPizzaGirlUnlockDialogue();
			break;
		case PizzaGirlState.InHubTown:
			if (SaveManager.PlayerSaveData.EndingSpawnRoom >= EndingSpawnRoomType.Manor)
			{
				PlayTeleportToManorDialogue();
			}
			else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirl_Dock_Dialogue_Intro))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PizzaGirl_Dock_Dialogue_Intro, value: true);
				PlayDockIntroDialogue();
			}
			else
			{
				PlayLoreDialogue();
			}
			break;
		case PizzaGirlState.InCloset:
			PlayClosetDialogue();
			break;
		case PizzaGirlState.InAboveGround:
			RunEndingDialogue();
			break;
		}
	}

	private void PlayPizzaGirlUnlockDialogue()
	{
		DialogueManager.StartNewDialogue(m_npcController);
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked))
		{
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PizzaGirlUnlocked, value: true);
			PropComplete();
			DialogueManager.AddDialogue("LOC_ID_DIALOGUE_TELEPORTER_NPC_TITLE_1", "LOC_ID_DIALOGUE_TELEPORTER_NPC_INTRO_TALK_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			DialogueManager.AddDialogueCompleteEndHandler(m_pizzaGirlUnlockedSkillTreePopup);
		}
		else
		{
			DialogueManager.AddDialogue("LOC_ID_NAME_PIZZA_GIRL_1", "LOC_ID_DIALOGUE_TELEPORTER_NPC_INTRO_REPEAT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
		}
	}

	private void PizzaGirlUnlockedSkillTreePopup()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTreePopUp))
		{
			WindowManager.LoadWindow(WindowID.SkillTreePopUp);
		}
		(WindowManager.GetWindowController(WindowID.SkillTreePopUp) as SkillTreePopUpWindowController).SetPopupType(SkillTreeType.PizzaGirl_Unlocked);
		WindowManager.SetWindowIsOpen(WindowID.SkillTreePopUp, isOpen: true);
		EndInteraction();
	}

	private void PlayUnlockTeleporterDialogue()
	{
		if (!SaveManager.PlayerSaveData.GetTeleporterIsUnlocked(base.Room.BiomeType))
		{
			int num = 0;
			if (NPC_EV.PIZZA_GIRL_TELEPORTER_COST_TABLE.ContainsKey(base.Room.BiomeType))
			{
				int num2 = NPC_EV.PIZZA_GIRL_TELEPORTER_COST_TABLE[base.Room.BiomeType];
				num = ((SaveManager.PlayerSaveData.NewGamePlusLevel <= 0) ? num2 : Mathf.RoundToInt((float)num2 * ((float)SaveManager.PlayerSaveData.NewGamePlusLevel * 2.5f) + (float)(SaveManager.PlayerSaveData.NewGamePlusLevel * SaveManager.PlayerSaveData.NewGamePlusLevel) * 250f));
			}
			string @string = LocalizationManager.GetString("LOC_ID_NAME_PIZZA_GIRL_1", isFemale: false);
			string empty = string.Empty;
			DialogueManager.StartNewDialogue(m_npcController);
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirl_UnlockTeleporter_Dialogue_Intro))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.PizzaGirl_UnlockTeleporter_Dialogue_Intro, value: true);
				empty = LocalizationManager.GetString("LOC_ID_DIALOGUE_TELEPORTER_NPC_PORTAL_EXPLAIN_TALK_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				empty = string.Format(empty, num);
			}
			else
			{
				empty = LocalizationManager.GetString("LOC_ID_DIALOGUE_TELEPORTER_NPC_PORTAL_EXPLAIN_REPEAT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
				empty = string.Format(empty, num);
			}
			DialogueManager.AddNonLocDialogue(@string, empty);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			DialogueManager.AddDialogueCompleteEndHandler(m_displayUnlockTeleporterConfirmMenu);
		}
		else
		{
			DialogueManager.StartNewDialogue(m_npcController);
			DialogueManager.AddDialogue("LOC_ID_NAME_PIZZA_GIRL_1", "LOC_ID_DIALOGUE_TELEPORTER_NPC_PORTAL_BUILD_DONE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
		}
	}

	private void DisplayUnlockTeleporterConfirmMenu()
	{
		InitializeUnlockTeleporterConfirmMenu();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void InitializeUnlockTeleporterConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		int num = 0;
		if (NPC_EV.PIZZA_GIRL_TELEPORTER_COST_TABLE.ContainsKey(base.Room.BiomeType))
		{
			int num2 = NPC_EV.PIZZA_GIRL_TELEPORTER_COST_TABLE[base.Room.BiomeType];
			num = ((SaveManager.PlayerSaveData.NewGamePlusLevel <= 0) ? num2 : Mathf.RoundToInt((float)num2 * ((float)SaveManager.PlayerSaveData.NewGamePlusLevel * 2.5f) + (float)(SaveManager.PlayerSaveData.NewGamePlusLevel * SaveManager.PlayerSaveData.NewGamePlusLevel) * 250f));
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_DIALOGUE_TELEPORTER_NPC_CONFIRM_MENU_TITLE_1", isLocID: true);
		string @string = LocalizationManager.GetString("LOC_ID_DIALOGUE_TELEPORTER_NPC_PORTAL_POP_UP_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		@string = string.Format(@string, num);
		obj.SetDescriptionText(@string, isLocID: false);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_DIALOGUE_TELEPORTER_NPC_CONFIRM_BUTTON_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_unlockTeleporter);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_DIALOGUE_TELEPORTER_NPC_CONFIRM_BUTTON_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private void UnlockTeleporter()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		int num = 0;
		if (NPC_EV.PIZZA_GIRL_TELEPORTER_COST_TABLE.ContainsKey(base.Room.BiomeType))
		{
			int num2 = NPC_EV.PIZZA_GIRL_TELEPORTER_COST_TABLE[base.Room.BiomeType];
			num = ((SaveManager.PlayerSaveData.NewGamePlusLevel <= 0) ? num2 : Mathf.RoundToInt((float)num2 * ((float)SaveManager.PlayerSaveData.NewGamePlusLevel * 2.5f) + (float)(SaveManager.PlayerSaveData.NewGamePlusLevel * SaveManager.PlayerSaveData.NewGamePlusLevel) * 250f));
		}
		DialogueManager.StartNewDialogue(m_npcController);
		if (SaveManager.PlayerSaveData.GoldCollected >= num)
		{
			int goldCollected = SaveManager.PlayerSaveData.GoldCollected;
			SaveManager.PlayerSaveData.GoldCollected -= num;
			SaveManager.PlayerSaveData.GoldSpent += num;
			SaveManager.PlayerSaveData.SetTeleporterIsUnlocked(base.Room.BiomeType, state: true);
			SaveManager.PlayerSaveData.TeleporterUnlockDialogueIndex.x++;
			m_goldChangedEventArgs.Initialize(goldCollected, SaveManager.PlayerSaveData.GoldCollected);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, this, m_goldChangedEventArgs);
			DialogueManager.AddDialogue("LOC_ID_NAME_PIZZA_GIRL_1", "LOC_ID_DIALOGUE_TELEPORTER_NPC_PORTAL_BUILD_YES_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			DialogueManager.AddDialogueCompleteEndHandler(m_displayUnlockTeleporterObjectiveCompleteHUD);
		}
		else
		{
			DialogueManager.AddDialogue("LOC_ID_NAME_PIZZA_GIRL_1", "LOC_ID_DIALOGUE_TELEPORTER_NPC_PORTAL_BUILD_FAIL_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		}
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private void DisplayUnlockTeleporterObjectiveCompleteHUD()
	{
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_objectiveCompleteArgs);
	}

	private void CancelConfirmMenuSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_PIZZA_GIRL_1", "LOC_ID_DIALOGUE_TELEPORTER_NPC_PORTAL_BUILD_NO_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private void PlayLoreDialogue()
	{
		if (NPCDialogueManager.CanSpeak(NPCType.PizzaGirl))
		{
			m_npcController.RunNextNPCDialogue(m_endInteraction);
			return;
		}
		string textLocID = NPCDialogue_EV.PIZZA_GIRL_HUBTOWN_GENERIC_DIALOGUES[m_genericDialogueIndex];
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_PIZZA_GIRL_1", textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
		m_genericDialogueIndex++;
		if (m_genericDialogueIndex >= NPCDialogue_EV.PIZZA_GIRL_HUBTOWN_GENERIC_DIALOGUES.Length)
		{
			m_genericDialogueIndex = 0;
		}
	}

	private void PlayDockIntroDialogue()
	{
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_HUB_TOWN_DIALOGUE_PIZZA_GIRL_TOWN_GREETINGS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private void PlayClosetDialogue()
	{
		if (!m_oneTimeSpeechBubblePlayed)
		{
			m_oneTimeSpeechBubblePlayed = true;
			StartCoroutine(ClosetCoroutine());
			return;
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_PIZZA_GIRL_ENDING_CLOSET_REPEAT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private IEnumerator ClosetCoroutine()
	{
		DialogueManager.StartNewDialogue(m_npcController);
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.PizzaGirlUnlocked))
		{
			DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_PIZZA_GIRL_ENDING_EXPLAINING_THE_HIDDEN_HEIRLOOM_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		else
		{
			DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_PIZZA_GIRL_ENDING_EXPLAINING_THE_HIDDEN_HEIRLOOM_2", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		if (m_elevatorSFXInstance.isValid())
		{
			AudioManager.Play(null, m_elevatorSFXInstance, base.Room.transform.position);
		}
		float delay2 = Time.time + 2f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		if (m_elevatorSFXInstance.isValid())
		{
			AudioManager.Stop(m_elevatorSFXInstance);
		}
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_ending_pizzaGirl_room_elevator_stop", base.Room.transform.position);
		m_shakeEffect = EffectManager.PlayEffect(base.gameObject, null, "CameraShakeSmall_Effect", Vector2.zero, 1f, EffectStopType.Gracefully);
		EffectManager.PlayEffect(base.gameObject, null, "Earthquake_Effect", PlayerManager.GetPlayerController().Midpoint, 1f, EffectStopType.Gracefully);
		delay2 = Time.time + 1.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		yield return null;
		ClosetTunnel obj = base.Room.gameObject.FindObjectReference<TunnelSpawnController>("ClosetTunnel").Tunnel as ClosetTunnel;
		obj.IsClosetUnlocked = true;
		obj.Animator.SetBool("Open", value: true);
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_ending_pizzaGirl_room_enter", CameraController.GameCamera.transform.position);
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_PIZZA_GIRL_ENDING_ARRIVED_AT_DESTINATION_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenTrueEnding))
		{
			if (!m_finalJonahSpeechPlayed)
			{
				m_finalJonahSpeechPlayed = true;
				textLocID = "LOC_ID_PIZZA_GIRL_OUTRO_J_REVEAL_INTRO_1";
			}
			else
			{
				textLocID = "LOC_ID_PIZZA_GIRL_OUTRO_J_REVEAL_REPEAT_1";
			}
		}
		else if (m_oneTimeSpeechBubblePlayed)
		{
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_PIZZA_GIRL_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_PIZZA_GIRL_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_oneTimeSpeechBubblePlayed = true;
			textLocID = (m_npcController.IsBestFriend ? "LOC_ID_PIZZA_GIRL_OUTRO_STRANGER_INTRO_1" : "LOC_ID_PIZZA_GIRL_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private void PlayTeleportToManorDialogue()
	{
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_PIZZA_GIRL_ENDING_RETURN_TO_HAMSON_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_initializeTeleportToManorConfirmMenu);
	}

	private void InitializeTeleportToManorConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_CHARON_UI_ENTER_HAMSON_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_CHARON_UI_ENTER_HAMSON_TEXT_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelTeleportToManor);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_teleportToManor);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_CANCEL_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelTeleportToManor);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void TeleportToManor()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		EndInteraction();
		WorldBuilder.FirstBiomeOverride = BiomeType.Garden;
		SceneLoader_RL.LoadScene(SceneID.World, TransitionID.FadeToBlackWithLoading);
	}

	private void CancelTeleportToManor()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		EndInteraction();
	}

	private IEnumerator MovePlayerToPizzaGirl()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (m_prop.Pivot.transform.localScale.x > 0f)
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: false);
		}
		else
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private void EndInteraction()
	{
		m_interactable.SetIsInteractableActive(isActive: true);
		AudioManager.PlayOneShotAttached(this, m_farewellAudioEvent, base.gameObject);
	}
}
