using System;
using System.Collections;
using System.Runtime.CompilerServices;
using Cinemachine;
using FMODUnity;
using RLAudio;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class CharonShop : MonoBehaviour, IRootObj, IDisplaySpeechBubble, IAudioEventEmitter, IRoomConsumer
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private GameObject m_pizzaGirlPositionObj;

	[SerializeField]
	private NPCController m_charon;

	[SerializeField]
	private bool m_useGardenLogic;

	[Header("Animation Timings")]
	[SerializeField]
	private float m_cameraShiftAmount = 2f;

	[SerializeField]
	private float m_cameraShiftDuration = 3f;

	[SerializeField]
	private float m_boatShiftAmount = 3f;

	[SerializeField]
	private float m_boatShiftDuration = 3f;

	[SerializeField]
	private float m_totalDelayBeforeLoading = 2f;

	[SerializeField]
	[EventRef]
	private string m_boatMoveAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_charonWindowOpenedAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_cancelEnterCastleAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_chooseEnterCastleAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_tollPaidAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	private WaitRL_Yield m_waitYield;

	private Interactable m_interactable;

	private bool m_hasSpoken;

	private Tween m_playerBoatMoveTween;

	private Tween m_boatBoatMoveTween;

	private Tween m_charonBoatMoveTween;

	private bool m_endingSpeechBubblePlayed;

	private Action m_runEnterCastleMenu;

	private Action m_closeCharon;

	private Action m_cancelEnterCastleSelection;

	private Action m_confirmEnterCastleSelection;

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

	public BaseRoom Room { get; private set; }

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
		if (!m_useGardenLogic)
		{
			return !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CharonDialogue_Intro);
		}
		return !m_hasSpoken;
	}

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_interactable = GetComponent<Interactable>();
		m_runEnterCastleMenu = RunEnterCastleMenu;
		m_closeCharon = CloseCharon;
		m_cancelEnterCastleSelection = CancelEnterCastleSelection;
		m_confirmEnterCastleSelection = ConfirmEnterCastleSelection;
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
		m_hasSpoken = false;
	}

	public void OpenCharon()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		StartCoroutine(OpenCharonCoroutine());
	}

	private void CloseCharon()
	{
		m_interactable.SetIsInteractableActive(isActive: true);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		m_charon.SetNPCState(NPCState.Idle);
		AudioManager.PlayOneShot(this, m_cancelEnterCastleAudioEvent);
	}

	private IEnumerator OpenCharonCoroutine()
	{
		m_charon.SetNPCState(NPCState.AtAttention);
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		yield return MovePlayerToCharon();
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			RunEndingDialogue();
		}
		else if (m_useGardenLogic)
		{
			RunGardenDialogue();
		}
		else if (HasEventDialogue())
		{
			RunCharonIntroDialogue();
		}
		else if (NPCDialogueManager.CanSpeak(m_charon.NPCType) && !m_useGardenLogic)
		{
			m_charon.RunNextNPCDialogue(m_runEnterCastleMenu);
		}
		else
		{
			RunEnterCastleMenu();
		}
	}

	private void RunCharonIntroDialogue()
	{
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CharonDialogue_Intro, value: true);
		string textLocID = "LOC_ID_DIALOGUE_CHARON_FEE_TAKE_1";
		DialogueManager.StartNewDialogue(m_charon, NPCState.AtAttention);
		DialogueManager.AddDialogue("LOC_ID_DIALOGUE_CHARON_TITLE_MAIN_1", textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_runEnterCastleMenu);
	}

	private void RunGardenDialogue()
	{
		string textLocID = "LOC_ID_GARDEN_CHARON_DENYING_TRAVEL_1";
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.GardenBoss_Defeated))
		{
			textLocID = "LOC_ID_GARDEN_CHARON_ACCEPTING_TRAVEL_1";
		}
		DialogueManager.StartNewDialogue(m_charon, NPCState.AtAttention);
		DialogueManager.AddDialogue("LOC_ID_DIALOGUE_CHARON_TITLE_MAIN_1", textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.GardenBoss_Defeated))
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_runEnterCastleMenu);
		}
		else
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_closeCharon);
		}
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (m_endingSpeechBubblePlayed)
		{
			textLocID = (m_charon.IsBestFriend ? "LOC_ID_CHARON_OUTRO_FRIENDS_REPEAT_1" : "LOC_ID_CHARON_OUTRO_STRANGER_REPEAT_1");
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = (m_charon.IsBestFriend ? "LOC_ID_CHARON_OUTRO_FRIENDS_INTRO_1" : "LOC_ID_CHARON_OUTRO_STRANGER_INTRO_1");
		}
		DialogueManager.StartNewDialogue(m_charon);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_charon.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_closeCharon);
	}

	private void RunEnterCastleMenu()
	{
		InitializeEnterCastleConfirmMenu();
		AudioManager.PlayOneShot(this, m_charonWindowOpenedAudioEvent);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
	}

	private void InitializeEnterCastleConfirmMenu()
	{
		if (WindowManager.GetWindowController(WindowID.ConfirmMenu) == null)
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		string text = "LOC_ID_CHARON_UI_ENTER_CASTLE_TITLE_1";
		string text2 = "LOC_ID_CHARON_UI_ENTER_CASTLE_TEXT_1";
		if (m_useGardenLogic)
		{
			text = "LOC_ID_CHARON_UI_ENTER_DOCKS_TITLE_1";
			text2 = "LOC_ID_CHARON_UI_ENTER_DOCKS_TEXT_1";
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText(text, isLocID: true);
		obj.SetDescriptionText(text2, isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelEnterCastleSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_confirmEnterCastleSelection);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelEnterCastleSelection);
	}

	private void CancelEnterCastleSelection()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		CloseCharon();
	}

	private void ConfirmEnterCastleSelection()
	{
		AudioManager.PlayOneShot(this, m_chooseEnterCastleAudioEvent);
		GetComponent<Interactable>().SetIsInteractableActive(isActive: false);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		StartCoroutine(LoadWorldCoroutine());
	}

	private IEnumerator MovePlayerToCharon()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if ((playerController.transform.position.x < base.transform.position.x && !playerController.IsFacingRight) || (playerController.transform.position.x > base.transform.position.x && playerController.IsFacingRight))
		{
			playerController.CharacterCorgi.Flip();
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	public IEnumerator LoadWorldCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: false);
		PlayerManager.GetPlayerController().ControllerCorgi.enabled = false;
		m_charon.SetNPCState(NPCState.Emote1);
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		if (SaveManager.PlayerSaveData.GoldCollected > 0 && !m_useGardenLogic)
		{
			AudioManager.PlayOneShot(this, m_tollPaidAudioEvent);
			SaveManager.PlayerSaveData.GiveMoneyToCharon(this);
		}
		m_charon.SetNPCState(NPCState.Emote2);
		Prop pizzaGirl = null;
		if (!m_useGardenLogic)
		{
			PropSpawnController propSpawnController = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<PropSpawnController>("PizzaGirl");
			if ((bool)propSpawnController)
			{
				pizzaGirl = propSpawnController.PropInstance;
			}
			if ((bool)pizzaGirl && NPCType_RL.IsNPCUnlocked(NPCType.PizzaGirl))
			{
				yield return PizzaGirlAnimCoroutine(pizzaGirl);
			}
		}
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		CinemachineVirtualCamera cinemachineVirtualCamera = CameraController.CinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
		if ((bool)cinemachineVirtualCamera)
		{
			cinemachineVirtualCamera.enabled = false;
			TweenManager.TweenBy(CameraController.GameCamera.transform, m_cameraShiftDuration, Ease.Quad.EaseInOut, "localPosition.x", m_cameraShiftAmount);
		}
		GameObject gameObject = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference("CharonBoat");
		if ((bool)gameObject)
		{
			AudioManager.PlayOneShotAttached(this, m_boatMoveAudioEvent, base.gameObject);
			PropSpawnController component = gameObject.GetComponent<PropSpawnController>();
			if ((bool)component.PropInstance)
			{
				m_boatBoatMoveTween = TweenManager.TweenBy(component.PropInstance.gameObject.transform, m_boatShiftDuration, Ease.Quad.EaseIn, "localPosition.x", m_boatShiftAmount);
				m_charonBoatMoveTween = TweenManager.TweenBy(m_charon.transform, m_boatShiftDuration, Ease.Quad.EaseIn, "localPosition.x", m_boatShiftAmount);
				m_playerBoatMoveTween = TweenManager.TweenBy(playerController.transform, m_boatShiftDuration, Ease.Quad.EaseIn, "localPosition.x", m_boatShiftAmount);
				if ((bool)pizzaGirl)
				{
					TweenManager.TweenBy(pizzaGirl.transform, m_boatShiftDuration, Ease.Quad.EaseIn, "position.x", m_boatShiftAmount);
				}
			}
		}
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if ((bool)currentPlayerRoom)
		{
			EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
			foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
			{
				if ((bool)enemySpawnController.EnemyInstance)
				{
					enemySpawnController.EnemyInstance.StatusEffectController.StopAllStatusEffects(interrupted: false);
				}
			}
		}
		m_waitYield.CreateNew(m_totalDelayBeforeLoading);
		yield return m_waitYield;
		playerController.HitboxController.SetHitboxActiveState(HitboxType.Body, active: true);
		if (!m_useGardenLogic)
		{
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerExitHubTown, this, EventArgs.Empty);
			SceneLoader_RL.LoadScene(SceneID.World, TransitionID.CastleGate);
		}
		else
		{
			SceneLoader_RL.RunTransitionWithLogic(EnterDocksCoroutine(), TransitionID.FadeToBlackNoLoading, cleanup: false);
		}
	}

	private IEnumerator PizzaGirlAnimCoroutine(Prop pizzaGirl)
	{
		pizzaGirl.GetComponentInChildren<SpeechBubbleController>(includeInactive: true).SetSpeechBubbleEnabled(enable: false);
		pizzaGirl.Animators[0].SetTrigger("Turn");
		pizzaGirl.transform.SetLocalScaleX(-1f);
		float delay2 = Time.time + 0.1f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		pizzaGirl.Animators[0].SetTrigger("Jump_Tell");
		delay2 = Time.time + 0.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		pizzaGirl.Animators[0].SetTrigger("Jump");
		Coroutine ySpeedCoroutine = StartCoroutine(YSpeedCoroutine(pizzaGirl));
		pizzaGirl.CorgiController.ResetPermanentDisable();
		pizzaGirl.CorgiController.PermanentlyDisableUponTouchingPlatform = false;
		pizzaGirl.CorgiController.enabled = true;
		pizzaGirl.CorgiController.SetVerticalForce(15f);
		yield return TweenManager.TweenTo(pizzaGirl.transform, 1f, Ease.Quad.EaseOut, "position.x", m_pizzaGirlPositionObj.transform.position.x).TweenCoroutine;
		while (!pizzaGirl.CorgiController.State.IsGrounded)
		{
			yield return null;
		}
		pizzaGirl.CorgiController.SetForce(Vector2.zero);
		StopCoroutine(ySpeedCoroutine);
		pizzaGirl.Animators[0].SetTrigger("Land");
		pizzaGirl.CorgiController.enabled = false;
	}

	private IEnumerator YSpeedCoroutine(Prop pizzaGirl)
	{
		while (true)
		{
			pizzaGirl.Animators[0].SetFloat("ySpeed", pizzaGirl.CorgiController.Velocity.y);
			yield return null;
		}
	}

	private IEnumerator EnterDocksCoroutine()
	{
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		m_boatBoatMoveTween.StopTween(runEndHandlers: false);
		m_charonBoatMoveTween.StopTween(runEndHandlers: false);
		m_playerBoatMoveTween.StopTween(runEndHandlers: false);
		PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<TunnelSpawnController>("DocksTunnel").Tunnel.ForceEnterTunnel(animate: false);
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
