using System;
using System.Collections;
using Cinemachine;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class AboveGroundRoomController : BaseSpecialRoomController
{
	private enum TrueEndPos
	{
		SilhouetteCamPos,
		DragonCamPos,
		TraitorSpawnPosStart,
		TraitorSpawnPosEnd
	}

	private const string SFX_DRAGON_CALL_NAME = "event:/UI/FrontEnd/ui_fe_trueEnding_01_dragonCall";

	private const string SFX_DRAGON_SWOOP_NAME = "event:/UI/FrontEnd/ui_fe_trueEnding_02_dragonSwoop";

	private const string SFX_DRAGON_APPROACH_NAME = "event:/UI/FrontEnd/ui_fe_trueEnding_03_dragonApproach";

	private const string SFX_DRAGON_LAND_NAME = "event:/UI/FrontEnd/ui_fe_trueEnding_04_dragonLand";

	private const string SFX_DRAGON_SETTLE_NAME = "event:/UI/FrontEnd/ui_fe_trueEnding_05_dragonSettle";

	private const string SFX_DRAGON_SPIT_NAME = "event:/UI/FrontEnd/ui_fe_trueEnding_06_dragonSpit";

	private const string SFX_PIZZA_GIRL_NOTICE_NAME = "event:/SFX/Interactables/sfx_pizzaGirl_farewell";

	[SerializeField]
	private PropSpawnController m_paradeSign;

	[SerializeField]
	private GameObject m_paradeWall;

	[SerializeField]
	private GameObject m_skillTreeWall;

	[SerializeField]
	private GameObject m_charonPostParadeSpawnPos;

	[SerializeField]
	private PropSpawnController m_charonProp;

	[SerializeField]
	private PropSpawnController m_manorDoorProp;

	[Header("True Ending Props")]
	[SerializeField]
	private GameObject m_trueEndTrigger;

	[SerializeField]
	private PropSpawnController m_traitorSpawner;

	[SerializeField]
	private PropSpawnController m_dragonSilhouetteSpawner;

	[SerializeField]
	private PropSpawnController m_dragonSpawner;

	[SerializeField]
	private PropSpawnController m_pizzaGirlProp;

	[SerializeField]
	private SpriteRenderer m_hugPicRenderer;

	[SerializeField]
	private Transform m_trueEndFollowObj;

	[SerializeField]
	private CinemachineVirtualCameraManager m_trueEndVcam;

	[SerializeField]
	private AnimationCurve m_trueEndBlend;

	[SerializeField]
	private Transform m_trueEndPosArrayTransform;

	private WaitRL_Yield m_waitYield;

	private CinemachineBlendDefinition m_followBlend;

	private Action m_newGamePlusConfirmComplete;

	private Action<object, EventArgs> m_skillTreeWindowClosed;

	private float m_regenTick;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_followBlend.m_Time = 1f;
		m_followBlend.m_Style = CinemachineBlendDefinition.Style.Custom;
		m_followBlend.m_CustomCurve = m_trueEndBlend;
		m_newGamePlusConfirmComplete = NewGamePlusConfirmComplete;
		m_skillTreeWindowClosed = SkillTreeWindowClosed;
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Closed, m_skillTreeWindowClosed);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Closed, m_skillTreeWindowClosed);
	}

	private void SkillTreeWindowClosed(object sender, EventArgs eventArgs)
	{
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().ResetCharacter();
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerEnterHubTown, this, null);
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CurrentArmor = playerController.ActualArmor;
		playerController.ResetMana();
		playerController.ResetHealth();
		playerController.ResetAllAbilityAmmo();
		playerController.ResetAbilityCooldowns();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StopGlobalTimer, null, null);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HideGlobalTimer, null, null);
		m_hugPicRenderer.gameObject.SetActive(value: false);
		bool flag = !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenTrueEnding) && SaveManager.PlayerSaveData.TimesBeatenTraitor >= Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 2 && BurdenManager.GetBurdenLevel(BurdenType.CastleBossUp) > 0 && BurdenManager.GetBurdenLevel(BurdenType.BridgeBossUp) > 0 && BurdenManager.GetBurdenLevel(BurdenType.ForestBossUp) > 0 && BurdenManager.GetBurdenLevel(BurdenType.StudyBossUp) > 0 && BurdenManager.GetBurdenLevel(BurdenType.TowerBossUp) > 0 && BurdenManager.GetBurdenLevel(BurdenType.CaveBossUp) > 0 && BurdenManager.GetBurdenLevel(BurdenType.FinalBossUp) > 0;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenTrueEnding))
			{
				m_dragonSpawner.PropInstance.gameObject.SetActive(value: false);
				m_traitorSpawner.PropInstance.gameObject.SetActive(value: false);
			}
			if (flag)
			{
				m_trueEndTrigger.SetActive(value: true);
			}
			else
			{
				m_trueEndTrigger.SetActive(value: false);
			}
			m_paradeWall.gameObject.SetActive(value: false);
			m_skillTreeWall.gameObject.SetActive(value: true);
		}
		else
		{
			m_trueEndTrigger.SetActive(value: false);
			m_paradeSign.PropInstance.gameObject.SetActive(value: false);
			m_paradeWall.gameObject.SetActive(value: true);
			m_skillTreeWall.gameObject.SetActive(value: false);
			m_manorDoorProp.PropInstance.gameObject.SetActive(value: false);
			m_charonProp.PropInstance.transform.position = m_charonPostParadeSpawnPos.transform.position;
			m_charonProp.PropInstance.transform.SetLocalScaleX(Mathf.Abs(m_charonProp.PropInstance.transform.localScale.x));
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenTrueEnding))
			{
				m_dragonSpawner.PropInstance.gameObject.SetActive(value: false);
				m_traitorSpawner.PropInstance.gameObject.SetActive(value: false);
			}
		}
		StartCoroutine(FlipPlayer());
		StartCoroutine(UpdateHeartStateCoroutine());
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			StartCoroutine(GiveAchievementsCoroutine());
		}
		else
		{
			StartCoroutine(OpenSkillTreeCoroutine());
		}
		if (SaveManager.PlayerSaveData.NewGamePlusLevel > SaveManager.PlayerSaveData.HighestNGPlusBeaten)
		{
			SaveManager.PlayerSaveData.HighestNGPlusBeaten = SaveManager.PlayerSaveData.NewGamePlusLevel;
			SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
		}
	}

	private IEnumerator OpenSkillTreeCoroutine()
	{
		while (!PlayerManager.IsInstantiated)
		{
			yield return null;
		}
		while (!base.Room.CinemachineCamera.IsActiveVirtualCamera)
		{
			yield return null;
		}
		if (!WindowManager.GetIsWindowLoaded(WindowID.SkillTree))
		{
			WindowManager.LoadWindow(WindowID.SkillTree);
		}
		WindowManager.SetWindowIsOpen(WindowID.SkillTree, isOpen: true);
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_castle_open");
		while (WindowManager.GetIsWindowOpen(WindowID.SkillTree))
		{
			yield return null;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenNGConfirmWindow))
		{
			yield return OpenNewGamePlusConfirmWindowCoroutine(2f);
		}
	}

	private IEnumerator GiveAchievementsCoroutine()
	{
		float delay = Time.time + 2f;
		while (Time.time < delay)
		{
			yield return null;
		}
		StoreAPIManager.GiveAchievement(AchievementType.PrequelReveal, StoreType.All);
		bool flag = true;
		NPCType[] typeArray = NPCType_RL.TypeArray;
		foreach (NPCType nPCType in typeArray)
		{
			if (nPCType != NPCType.None && nPCType != NPCType.Johan && nPCType != NPCType.Jukebox && !NPCController.GetBestFriendState(nPCType))
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			StoreAPIManager.GiveAchievement(AchievementType.AllFriends, StoreType.All);
		}
	}

	private IEnumerator UpdateHeartStateCoroutine()
	{
		yield return null;
		UpdateHeartState("Charon", isEnemySpawner: false);
		UpdateHeartState("Architect", isEnemySpawner: false);
		UpdateHeartState("OffshoreBank", isEnemySpawner: false);
		UpdateHeartState("Dummy", isEnemySpawner: true);
		UpdateHeartState("Totem", isEnemySpawner: false);
		UpdateHeartState("Geras", isEnemySpawner: false);
		UpdateHeartState("Elpis", isEnemySpawner: false);
		UpdateHeartState("Keres", isEnemySpawner: false);
		UpdateHeartState("Blacksmith", isEnemySpawner: false);
		UpdateHeartState("Enchantress", isEnemySpawner: false);
		UpdateHeartState("PizzaGirl", isEnemySpawner: false);
		UpdateHeartState("Traitor", isEnemySpawner: false);
		UpdateHeartState("Dragon", isEnemySpawner: false);
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
		{
			m_traitorSpawner.PropInstance.GetComponent<NPCController>().ShowHeart();
		}
	}

	private void UpdateHeartState(string objName, bool isEnemySpawner)
	{
		if (!isEnemySpawner)
		{
			PropSpawnController propSpawnController = base.Room.gameObject.FindObjectReference<PropSpawnController>(objName);
			if ((bool)propSpawnController && (bool)propSpawnController.PropInstance)
			{
				NPCController component = propSpawnController.PropInstance.GetComponent<NPCController>();
				if ((bool)component)
				{
					component.UpdateHeartState();
				}
			}
			return;
		}
		EnemySpawnController enemySpawnController = base.Room.gameObject.FindObjectReference<EnemySpawnController>(objName);
		if ((bool)enemySpawnController && (bool)enemySpawnController.EnemyInstance)
		{
			NPCController component2 = enemySpawnController.EnemyInstance.GetComponent<NPCController>();
			if ((bool)component2)
			{
				component2.UpdateHeartState();
			}
		}
	}

	private IEnumerator FlipPlayer()
	{
		yield return null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.IsFacingRight)
		{
			playerController.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: true, ignoreTurnParam: true);
		}
	}

	public void PlayTrueEnding()
	{
		StartCoroutine(TrueEndingCoroutine());
	}

	private IEnumerator TrueEndingCoroutine()
	{
		PlayerController player = PlayerManager.GetPlayerController();
		Prop dragon = m_dragonSpawner.PropInstance;
		Animator dragonAnimator = dragon.Animators[0];
		Prop dragonSilhouette = m_dragonSilhouetteSpawner.PropInstance;
		Prop traitor = m_traitorSpawner.PropInstance;
		Animator traitorAnimator = traitor.Animators[0];
		CorgiController_RL traitorCorgi = traitor.GetComponent<CorgiController_RL>();
		traitorCorgi.PermanentlyDisableUponTouchingPlatform = false;
		Prop pizzaGirl = m_pizzaGirlProp.PropInstance;
		_ = base.Room.CinemachineCamera;
		CinemachineBlendDefinition storedBlend = CameraController.CinemachineBrain.m_DefaultBlend;
		CameraController.CinemachineBrain.m_DefaultBlend = m_followBlend;
		_ = Vector2.zero;
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		player.SetVelocity(0f, 0f, additive: false);
		player.StopActiveAbilities(stopPersistentAbilities: true);
		while (!player.IsGrounded)
		{
			yield return null;
		}
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		base.Room.CinemachineCamera.SetIsActiveCamera(isActiveCamera: false);
		m_trueEndVcam.VirtualCamera.gameObject.SetActive(value: true);
		float zoomLevel = CameraController.ZoomLevel;
		if (zoomLevel != 1f)
		{
			m_trueEndVcam.SetLensSize(CameraController.GetVirtualCameraLensSize(zoomLevel));
		}
		m_trueEndVcam.SetIsActiveCamera(isActiveCamera: true);
		m_trueEndFollowObj.position = GetTrueEndPosition(TrueEndPos.SilhouetteCamPos);
		m_trueEndVcam.SetFollowTarget(m_trueEndFollowObj);
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_trueEnding_01_dragonCall", GetTrueEndPosition(TrueEndPos.DragonCamPos));
		m_waitYield.CreateNew(2f);
		yield return m_waitYield;
		AudioManager.PlayOneShotAttached(null, "event:/UI/FrontEnd/ui_fe_trueEnding_02_dragonSwoop", dragonSilhouette.gameObject);
		TweenManager.TweenBy(dragonSilhouette.transform, 0.5f, Ease.None, "position.y", 100);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		EffectManager.PlayEffect(player.gameObject, null, "CameraShakeVerySmall_Effect", Vector3.zero, 0.5f, EffectStopType.Gracefully);
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		Vector2 vector = GetTrueEndPosition(TrueEndPos.DragonCamPos);
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_trueEnding_03_dragonApproach", vector);
		yield return TweenManager.TweenTo(m_trueEndFollowObj, 1f, Ease.Quad.EaseInOut, "position.x", vector.x, "position.y", vector.y).TweenCoroutine;
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		dragon.gameObject.SetActive(value: true);
		dragon.transform.SetPositionX(dragon.transform.position.x - 5f);
		dragon.transform.SetPositionY(dragon.transform.position.y + 30f);
		dragon.GetComponent<DragonPropController>().SetEndingCutsceneStateEnabled(enabled: true);
		dragonAnimator.enabled = true;
		dragonAnimator.SetBool("Idle", value: false);
		dragonAnimator.SetTrigger("Fall");
		AudioManager.PlayDelayedOneShot(null, "event:/UI/FrontEnd/ui_fe_trueEnding_04_dragonLand", GetTrueEndPosition(TrueEndPos.DragonCamPos), 1.5f);
		yield return TweenManager.TweenBy(dragon.transform, 2f, Ease.Expo.EaseIn, "position.x", 5, "position.y", -30).TweenCoroutine;
		dragonAnimator.SetBool("Idle", value: true);
		EffectManager.PlayEffect(player.gameObject, null, "CameraShakeSmall_Effect", Vector3.zero, 1f, EffectStopType.Gracefully);
		m_waitYield.CreateNew(2f);
		yield return m_waitYield;
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_trueEnding_05_dragonSettle", GetTrueEndPosition(TrueEndPos.DragonCamPos));
		dragonAnimator.SetBool("OpenMouth", value: true);
		m_waitYield.CreateNew(2.5f);
		yield return m_waitYield;
		vector = GetTrueEndPosition(TrueEndPos.TraitorSpawnPosStart);
		AudioManager.PlayOneShotAttached(null, "event:/UI/FrontEnd/ui_fe_trueEnding_06_dragonSpit", traitor.gameObject);
		traitor.gameObject.SetActive(value: true);
		traitor.GetComponent<JohanPropController>().SetEndingCutsceneStateEnabled(enabled: true);
		traitorAnimator.enabled = true;
		traitor.transform.position = vector;
		traitorAnimator.Play("Idle", 0);
		traitorAnimator.Play("DodgeRoll_Hold", 1);
		traitorCorgi.SetVerticalForce(15f);
		EffectManager.PlayEffect(dragon.gameObject, dragonAnimator, "DragonSpit_Effect", vector, 0f, EffectStopType.Gracefully);
		vector = GetTrueEndPosition(TrueEndPos.TraitorSpawnPosEnd);
		TweenManager.TweenTo(traitor.transform, 3f, Ease.Quad.EaseOut, "position.x", vector.x);
		TweenManager.TweenTo(m_trueEndFollowObj, 3f, Ease.Quad.EaseInOut, "position.x", vector.x, "position.y", vector.y);
		while (Mathf.Abs(traitor.transform.position.x - player.transform.position.x) > 3f)
		{
			yield return null;
		}
		StartCoroutine(PlayerJumpTraitorCoroutine());
		dragonAnimator.SetBool("OpenMouth", value: false);
		dragonAnimator.Play("Idle_Hold");
		traitorAnimator.Play("Empty", 1);
		traitorAnimator.Play("Death_4", 0, 1f);
		m_waitYield.CreateNew(2.5f);
		yield return m_waitYield;
		pizzaGirl.transform.SetLocalScaleX(pizzaGirl.transform.localScale.x * -1f);
		AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_pizzaGirl_farewell", pizzaGirl.transform.position);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		if (!WindowManager.GetIsWindowLoaded(WindowID.Dialogue))
		{
			WindowManager.LoadWindow(WindowID.Dialogue);
		}
		DialogueManager.StartNewDialogue(pizzaGirl.GetComponent<NPCController>());
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_CUTSCENE_J_AND_Z_Z_SEES_JONAH_1", isFemale: false);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		traitorCorgi.SetVerticalForce(5f);
		traitorAnimator.Play("JumpBlendTree");
		m_waitYield.CreateNew(0.1f);
		yield return m_waitYield;
		while (!traitorCorgi.State.IsGrounded)
		{
			yield return null;
		}
		traitorAnimator.Play("Idle");
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		DialogueManager.StartNewDialogue(traitor.GetComponent<NPCController>());
		DialogueManager.AddDialogue("LOC_ID_NAME_J_REVEALED_1", "LOC_ID_CUTSCENE_J_AND_Z_JONAH_SAYS_SOMETHING_AFTER_GRINDING_1", isFemale: false);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		m_hugPicRenderer.gameObject.SetActive(value: true);
		Color color = m_hugPicRenderer.color;
		color.a = 0f;
		m_hugPicRenderer.color = color;
		Vector3 position = m_hugPicRenderer.transform.position;
		position.y -= 1f;
		m_hugPicRenderer.transform.position = position;
		TweenManager.TweenBy(m_hugPicRenderer.transform, 0.5f, Ease.Quad.EaseOut, "position.y", 1);
		yield return TweenManager.TweenTo(m_hugPicRenderer, 0.5f, Ease.None, "color.a", 1).TweenCoroutine;
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_CUTSCENE_J_AND_Z_Z_TALKS_HUG_1", isFemale: false);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_NAME_J_REVEALED_1", "LOC_ID_CUTSCENE_J_AND_Z_J_TALKS_GOOP_1", isFemale: false, DialogueWindowStyle.HorizontalLower);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(NPCType.PizzaGirl), "LOC_ID_CUTSCENE_J_AND_Z_Z_TALKS_MISSED_YOU_1", isFemale: false);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_NAME_J_REVEALED_1", "LOC_ID_CUTSCENE_J_AND_Z_J_TALKS_GRATEFUL_1", isFemale: false, DialogueWindowStyle.HorizontalLower);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		pizzaGirl.transform.SetLocalScaleX(Mathf.Abs(pizzaGirl.transform.localScale.x));
		TweenManager.TweenBy(m_hugPicRenderer.transform, 0.5f, Ease.Quad.EaseIn, "position.y", -1);
		yield return TweenManager.TweenTo(m_hugPicRenderer, 0.5f, Ease.None, "color.a", 0).TweenCoroutine;
		m_hugPicRenderer.gameObject.SetActive(value: false);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		m_trueEndVcam.VirtualCamera.gameObject.SetActive(value: false);
		m_trueEndVcam.SetIsActiveCamera(isActiveCamera: false);
		base.Room.CinemachineCamera.SetIsActiveCamera(isActiveCamera: true);
		m_waitYield.CreateNew(1f);
		yield return null;
		dragon.GetComponent<DragonPropController>().SetEndingCutsceneStateEnabled(enabled: false);
		traitor.GetComponent<JohanPropController>().SetEndingCutsceneStateEnabled(enabled: false);
		CameraController.CinemachineBrain.m_DefaultBlend = storedBlend;
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.SeenTrueEnding, value: true);
		pizzaGirl.GetComponent<Interactable>().SpeechBubble.SetSpeechBubbleEnabled(enable: true);
	}

	private Vector3 GetTrueEndPosition(TrueEndPos posType)
	{
		if ((int)posType < m_trueEndPosArrayTransform.childCount)
		{
			return m_trueEndPosArrayTransform.GetChild((int)posType).position;
		}
		return Vector3.zero;
	}

	private IEnumerator PlayerJumpTraitorCoroutine()
	{
		yield return PlayerMovementHelper.JumpPlayer(1.5f);
		PlayerManager.GetPlayerController().CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: true);
	}

	private IEnumerator OpenNewGamePlusConfirmWindowCoroutine(float delay)
	{
		delay = Time.time + delay;
		while (Time.time < delay)
		{
			yield return null;
		}
		string text = ((SaveManager.PlayerSaveData.TimesBeatenTraitor <= 1) ? "LOC_ID_COMPLETION_NG0_1" : "LOC_ID_COMPLETION_NG_PLUS_1");
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenTrueEnding))
		{
			text = "LOC_ID_COMPLETION_NG_FINAL_1";
		}
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenuBig))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenuBig);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenuBig) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_COMPLETION_NG_TITLE_1", isLocID: true);
		obj.SetDescriptionText(text, isLocID: true);
		obj.SetOnCancelAction(m_newGamePlusConfirmComplete);
		obj.SetNumberOfButtons(1);
		obj.SetButtonDelayTime(3f);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_OK_NEWGAME_INFO_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_newGamePlusConfirmComplete);
		AudioManager.SetSFXPaused(pause: true);
		AudioManager.PlayOneShot(null, "event:/Stingers/sting_challengComplete");
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: true);
	}

	private void NewGamePlusConfirmComplete()
	{
		AudioManager.SetSFXPaused(pause: false);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.SeenNGConfirmWindow, value: true);
	}

	private void FixedUpdate()
	{
		if (Time.time < m_regenTick + 0.05f)
		{
			return;
		}
		m_regenTick = Time.time;
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		float currentMana = playerController.CurrentMana;
		float num = playerController.ActualMaxMana;
		if (currentMana < num)
		{
			float num2 = 100f * Time.fixedDeltaTime;
			if (currentMana + num2 > num)
			{
				playerController.SetMana(num, additive: false, runEvents: true);
			}
			else
			{
				playerController.SetMana(100f * Time.fixedDeltaTime, additive: true, runEvents: true);
			}
		}
		if (playerController.CurrentHealth < (float)playerController.ActualMaxHealth)
		{
			playerController.SetHealth(100f * Time.fixedDeltaTime, additive: true, runEvents: true);
		}
		if (playerController.CurrentArmor < playerController.ActualArmor)
		{
			playerController.CurrentArmor += Mathf.CeilToInt(100f * Time.fixedDeltaTime);
			playerController.SetHealth(0f, additive: true, runEvents: true);
		}
	}
}
