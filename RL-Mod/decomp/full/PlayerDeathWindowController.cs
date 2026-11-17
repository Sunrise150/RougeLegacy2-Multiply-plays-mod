using System;
using System.Collections;
using System.Collections.Generic;
using FMODUnity;
using GameEventTracking;
using RLAudio;
using RL_Windows;
using Rewired;
using SceneManagement_RL;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PlayerDeathWindowController : WindowController, ILocalizable
{
	private const bool RUN_EVENT_TRACKER_TEST = false;

	[Space(10f)]
	[SerializeField]
	private ReenactmentController m_reenactmentController;

	[SerializeField]
	private PlayerDeathSlainByTextController m_slainByController;

	[SerializeField]
	private PlayerDeathPartingWordsTextController m_partingWordsController;

	[SerializeField]
	private PlayerDeathRankTextController m_rankController;

	[SerializeField]
	private GameObject m_deathPanelGO;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[SerializeField]
	private float m_fadeToBlackTime = 1f;

	[SerializeField]
	private TMP_Text m_enemiesDefeatedAmountText;

	[SerializeField]
	private TMP_Text m_goldCollectedAmountText;

	[SerializeField]
	private TMP_Text m_equipmentOreCollectedAmountText;

	[SerializeField]
	private TMP_Text m_runeOreCollectedAmountText;

	[SerializeField]
	private TMP_Text m_soulCollectedAmountText;

	[SerializeField]
	private CanvasGroup m_reenactmentCanvasGroup;

	[SerializeField]
	private GridLayoutGroup m_reenactmentGridLayout;

	[SerializeField]
	private Image m_line;

	[SerializeField]
	private RawImage m_portraitRT;

	[SerializeField]
	private CanvasGroup m_portraitCanvasGroup;

	[SerializeField]
	private LineagePortrait m_portrait;

	[SerializeField]
	private Camera m_portraitRenderCamera;

	[SerializeField]
	private GameObject m_portraitRenderGO;

	[SerializeField]
	private RectTransform m_playerPositionObj;

	[Space(10f)]
	[SerializeField]
	private UnityEvent m_portraitAppearedUnityEvent;

	[SerializeField]
	private UnityEvent m_deathRecapFadeInUnityEvent;

	[SerializeField]
	private UnityEvent m_deathRecapFadeOutUnityEvent;

	[SerializeField]
	private StudioEventEmitter m_deathSnapshot;

	private int m_enemiesKilledCounter;

	private int m_goldCollectedCounter;

	private int m_equipmentOreCollectedCounter;

	private int m_runeOreCollectedCounter;

	private int m_soulCollectedCounter;

	private WaitRL_Yield m_waitYield;

	private bool m_masteryUnlocked;

	private bool m_playTreeCutscene;

	private bool m_playHestiaCutscene;

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action<InputActionEventData> m_onRestartInLineagePressed;

	private Action<InputActionEventData> m_moveMapHorizontal;

	private Action<InputActionEventData> m_moveMapVertical;

	private Action<InputActionEventData> m_zoomMapVertical;

	private Tween m_cameraZoomTween;

	private Tween m_cameraMoveTween;

	private float m_desiredZoomSize;

	private Vector3 m_desiredCamPos;

	private Vector4 m_camBoundsRect = new Vector4(float.MaxValue, float.MaxValue, float.MinValue, float.MinValue);

	private BiomeType m_prevBiomeType = BiomeType.Castle;

	private bool RunVictoryDeathScreen => SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround;

	public override WindowID ID => WindowID.PlayerDeath;

	public Scene Scene => base.gameObject.scene;

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onRestartInLineagePressed = OnRestartInLineagePressed;
		m_moveMapHorizontal = MoveMapHorizontal;
		m_moveMapVertical = MoveMapVertical;
		m_zoomMapVertical = ZoomMapVertical;
	}

	public override void Initialize()
	{
		m_waitYield = new WaitRL_Yield(0f);
		m_reenactmentController.OnRoomTrackerTriggeredRelay.AddListener(OnRoomTrackerEventTriggered);
		m_reenactmentController.OnEnemyTrackerTriggeredRelay.AddListener(OnEnemyTrackerEventTriggered);
		m_reenactmentController.OnChestTrackerTriggeredRelay.AddListener(OnChestTrackerEventTriggered);
		m_reenactmentController.OnItemTrackerTriggeredRelay.AddListener(OnItemTrackerEventTriggered);
		m_portraitRenderGO.SetActive(value: false);
		m_portrait.gameObject.SetLayerRecursively(5, includeInactive: true);
		base.Initialize();
	}

	private void OnDestroy()
	{
		m_reenactmentController.OnRoomTrackerTriggeredRelay.RemoveListener(OnRoomTrackerEventTriggered);
		m_reenactmentController.OnEnemyTrackerTriggeredRelay.RemoveListener(OnEnemyTrackerEventTriggered);
		m_reenactmentController.OnChestTrackerTriggeredRelay.RemoveListener(OnChestTrackerEventTriggered);
		m_reenactmentController.OnItemTrackerTriggeredRelay.RemoveListener(OnItemTrackerEventTriggered);
	}

	public void StopDeathSnapshot()
	{
		m_deathSnapshot.Stop();
	}

	protected override void OnOpen()
	{
		if (PlayerManager.IsInstantiated && !ChallengeManager.IsInChallenge)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			if (!currentPlayerRoom.IsNativeNull() && currentPlayerRoom.SaveController.CanSaveRoom)
			{
				RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(currentPlayerRoom.BiomeType, currentPlayerRoom.BiomeControllerIndex);
				currentPlayerRoom.SaveController.SaveRoomState(roomSaveData, exitingToMainMenu: true);
			}
		}
		if (!RunVictoryDeathScreen)
		{
			AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_death_transition_in", CameraController.GameCamera.transform.position);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, null, new PlayerHUDVisibilityEventArgs(0.5f));
		}
		else
		{
			if (PlayerManager.IsInstantiated && !PlayerManager.GetPlayerController().IsFacingRight)
			{
				PlayerManager.GetPlayerController().CharacterCorgi.Flip();
			}
			AudioManager.PlayOneShot(null, "event:/Stingers/sting_chestItem", CameraController.GameCamera.transform.position);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, null, new PlayerHUDVisibilityEventArgs(0f));
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StopGlobalTimer, null, null);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HideGlobalTimer, null, null);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		m_masteryUnlocked = SkillTreeLogicHelper.IsTotemUnlocked();
		CameraController.CinemachineBrain.enabled = false;
		foreach (BaseTrait activeTrait in TraitManager.ActiveTraitList)
		{
			activeTrait.DisableOnDeath();
		}
		_ = RunVictoryDeathScreen;
		SaveManager.PlayerSaveData.IsDead = true;
		GlobalCharacterDeathResetLogic();
		SaveManager.PlayerSaveData.TimesDied++;
		if (RunVictoryDeathScreen)
		{
			SaveManager.PlayerSaveData.CurrentCharacter.IsVictory = true;
		}
		if (SaveManager.PlayerSaveData.GlobalNPCDialogueCD > 0)
		{
			SaveManager.PlayerSaveData.GlobalNPCDialogueCD--;
		}
		else
		{
			NPCDialogueManager.ReduceAllNPCDialogueCooldowns(-1);
		}
		SaveManager.PlayerSaveData.PopulatedNPCDialoguesList.Clear();
		if (SaveManager.PlayerSaveData.CastleLockState == CastleLockState.TemporaryLock)
		{
			SaveManager.PlayerSaveData.CastleLockState = CastleLockState.NotLocked;
		}
		if (!RunVictoryDeathScreen)
		{
			m_playTreeCutscene = SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Play_Tree_DeathCutscene);
			m_playHestiaCutscene = SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.Play_Hestia_DeathCutscene);
			if (m_playTreeCutscene)
			{
				m_playHestiaCutscene = false;
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.Play_Hestia_DeathCutscene, value: false);
			}
			if (m_playTreeCutscene || m_playHestiaCutscene)
			{
				SaveManager.PlayerSaveData.TimesDiedSinceHestia = 0;
			}
			else
			{
				SaveManager.PlayerSaveData.TimesDiedSinceHestia++;
			}
		}
		else
		{
			m_playTreeCutscene = false;
			m_playHestiaCutscene = false;
		}
		CharacterData currentCharacter = SaveManager.PlayerSaveData.CurrentCharacter;
		SaveManager.LineageSaveData.AddCharacterToLineage(currentCharacter);
		if (SaveManager.LineageSaveData.DuplicateNameCountDict.ContainsKey(currentCharacter.Name))
		{
			int value = SaveManager.LineageSaveData.DuplicateNameCountDict[currentCharacter.Name];
			SaveManager.LineageSaveData.DuplicateNameCountDict[currentCharacter.Name] = value;
		}
		else
		{
			SaveManager.LineageSaveData.DuplicateNameCountDict.Add(currentCharacter.Name, 0);
		}
		SaveManager.SaveAllCurrentProfileGameData(SavingType.FileOnly, saveStageData: true, ignoreTimeRestriction: true);
		AddInputListeners();
		m_slainByController.UpdateMessage(RunVictoryDeathScreen);
		m_partingWordsController.UpdateMessage(RunVictoryDeathScreen);
		m_playerPositionObj.gameObject.SetActive(value: false);
		m_reenactmentCanvasGroup.alpha = 0f;
		m_line.SetAlpha(0f);
		m_slainByController.Text.alpha = 0f;
		m_partingWordsController.Text.alpha = 0f;
		m_rankController.Text.alpha = 0f;
		m_rankController.CounterText.alpha = 0f;
		m_portraitCanvasGroup.gameObject.SetActive(value: true);
		m_portraitCanvasGroup.alpha = 0f;
		m_enemiesKilledCounter = 0;
		m_enemiesDefeatedAmountText.text = m_enemiesKilledCounter.ToString();
		m_goldCollectedCounter = 0;
		m_goldCollectedAmountText.text = m_goldCollectedCounter.ToString();
		m_equipmentOreCollectedCounter = 0;
		m_equipmentOreCollectedAmountText.text = m_equipmentOreCollectedCounter.ToString();
		m_runeOreCollectedCounter = 0;
		m_runeOreCollectedAmountText.text = m_runeOreCollectedCounter.ToString();
		m_soulCollectedCounter = 0;
		m_soulCollectedAmountText.text = m_soulCollectedCounter.ToString();
		MapController.SetCameraFollowIsOn(isOn: false);
		m_deathPanelGO.SetActive(value: true);
		CameraController.SoloCam.Camera.orthographicSize = CameraController.GameCamera.orthographicSize;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.PlayerDeathWindow_Opened, this, null);
		m_windowCanvas.gameObject.SetActive(value: true);
		m_rankController.UpdateMessage();
		StartCoroutine(OnOpenCoroutine());
	}

	public static void GlobalCharacterDeathResetLogic()
	{
		SaveManager.PlayerSaveData.InCastle = false;
		SaveManager.PlayerSaveData.ResetAllRelics(broadcast: true);
		SaveManager.PlayerSaveData.TemporaryMaxHealthMods = 0f;
		SaveManager.PlayerSaveData.TimesRolledRelic = 0;
		SaveManager.PlayerSaveData.GoldilocksReceived = false;
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CaveTuningForkTriggered, value: false);
	}

	private IEnumerator CreateTestTracker()
	{
		List<RoomTrackerData> roomTrackerList = GameEventTrackerManager.RoomEventTracker.RoomsEntered;
		List<EnemyTrackerData> enemyTrackerList = GameEventTrackerManager.EnemyEventTracker.EnemiesKilled;
		List<ItemTrackerData> itemTrackerList = GameEventTrackerManager.ItemEventTracker.ItemsCollected;
		List<ChestTrackerData> chestTrackerList = GameEventTrackerManager.ItemEventTracker.ChestsOpened;
		foreach (KeyValuePair<BiomeType, BiomeController> biomeController2 in WorldBuilder.BiomeControllers)
		{
			BiomeController biomeController = biomeController2.Value;
			if (biomeController.Rooms == null)
			{
				continue;
			}
			for (int i = 0; i < biomeController.Rooms.Count; i++)
			{
				BaseRoom room = biomeController.Rooms[i];
				if (ReenactmentController.IsTunnelRoom(room) || room.BiomeType == BiomeType.Garden)
				{
					continue;
				}
				roomTrackerList.Add(new RoomTrackerData(room.BiomeType, room.BiomeControllerIndex));
				yield return null;
				ChestSpawnController[] chestSpawnControllers = room.SpawnControllerManager.ChestSpawnControllers;
				foreach (ChestSpawnController chestSpawnController in chestSpawnControllers)
				{
					if (chestSpawnController.ShouldSpawn && CDGHelper.RandomPlusMinus() > 0)
					{
						chestTrackerList.Add(new ChestTrackerData(chestSpawnController.ChestType, containsGold: true, chestSpawnController.Room.BiomeType, chestSpawnController.Room.BiomeControllerIndex, chestSpawnController.ChestIndex));
						yield return null;
					}
				}
				EnemySpawnController[] enemySpawnControllers = room.SpawnControllerManager.EnemySpawnControllers;
				foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
				{
					if (CDGHelper.RandomPlusMinus() > 0)
					{
						itemTrackerList.Add(new ItemTrackerData(10f, ItemDropType.Coin));
						itemTrackerList.Add(new ItemTrackerData(10f, ItemDropType.EquipmentOre));
						itemTrackerList.Add(new ItemTrackerData(10f, ItemDropType.RuneOre));
						itemTrackerList.Add(new ItemTrackerData(100f, ItemDropType.Soul));
					}
					enemyTrackerList.Add(new EnemyTrackerData(room.BiomeType, room.BiomeControllerIndex, enemySpawnController.Type, enemySpawnController.Rank, enemySpawnController.EnemyIndex));
					enemyTrackerList.Add(new EnemyTrackerData(room.BiomeType, room.BiomeControllerIndex, enemySpawnController.Type, enemySpawnController.Rank, enemySpawnController.EnemyIndex));
					enemyTrackerList.Add(new EnemyTrackerData(room.BiomeType, room.BiomeControllerIndex, enemySpawnController.Type, enemySpawnController.Rank, enemySpawnController.EnemyIndex));
					yield return null;
				}
			}
		}
	}

	private IEnumerator OnOpenCoroutine()
	{
		if (WorldBuilder.IsInstantiated)
		{
		}
		yield return new WaitForEndOfFrame();
		m_portraitRenderGO.SetActive(value: true);
		m_portrait.SetPortraitLook(SaveManager.PlayerSaveData.CurrentCharacter);
		bool fog = RenderSettings.fog;
		RenderSettings.fog = false;
		m_portraitRenderCamera.Render();
		m_portraitRenderGO.SetActive(value: false);
		RenderSettings.fog = fog;
		if (!RunVictoryDeathScreen)
		{
			yield return DeathAnimationCoroutine();
		}
		else
		{
			yield return VictoryAnimationCoroutine();
		}
		while (!m_reenactmentController.IsComplete)
		{
			yield return null;
		}
		m_goldCollectedCounter = SaveManager.PlayerSaveData.GoldCollected;
		m_goldCollectedAmountText.text = m_goldCollectedCounter.ToString();
	}

	protected override void OnClose()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		CameraController.CinemachineBrain.enabled = true;
		m_reenactmentController.Reset();
		CameraController.SoloCam.gameObject.SetActive(value: false);
		m_bgCanvasGroup.gameObject.SetActive(value: false);
		RemoveInputListeners();
		m_windowCanvas.gameObject.SetActive(value: false);
		_ = RunVictoryDeathScreen;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.TraitsChanged, this, new TraitChangedEventArgs(TraitType.None, TraitType.None));
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.PlayerDeathWindow_Closed, this, null);
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().Pivot.SetActive(value: false);
			PlayerManager.GetPlayerController().ControllerCorgi.enabled = false;
		}
	}

	private IEnumerator DeathAnimationCoroutine()
	{
		PlayerController player = PlayerManager.GetPlayerController();
		if (!player)
		{
			yield break;
		}
		player.Animator.SetBool("Stunned", value: true);
		player.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
		Vector3 position = CameraController.GameCamera.transform.position;
		position.z = CameraController.SoloCam.transform.position.z;
		CameraController.SoloCam.transform.position = position;
		CameraController.SoloCam.gameObject.SetActive(value: true);
		CameraController.SoloCam.AddToCameraLayer(player.Visuals);
		m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitYield;
		if (CameraController.GameCamera.orthographicSize != 9f)
		{
			TweenManager.TweenTo_UnscaledTime(CameraController.SoloCam.Camera, 1f, Ease.Quad.EaseInOut, "orthographicSize", 9f);
		}
		Vector3 vector = player.transform.position - m_playerPositionObj.position;
		vector += CameraController.GameCamera.transform.position;
		TweenManager.TweenTo_UnscaledTime(CameraController.SoloCam.transform, 1f, Ease.Quad.EaseInOut, "position.x", vector.x, "position.y", vector.y);
		if (!Mathf.Approximately(AspectRatioManager.CurrentGameAspectRatio, 1.7777778f) && AspectRatioManager.CurrentGameAspectRatio < 1.7777778f)
		{
			float num = AspectRatioManager.CurrentGameAspectRatio / 1.7777778f;
			Vector3 vector2 = player.transform.localScale * num;
			TweenManager.TweenTo_UnscaledTime(player.transform, 1f, Ease.None, "localScale.x", vector2.x, "localScale.y", vector2.y, "localScale.z", vector2.z);
		}
		m_bgCanvasGroup.alpha = 0f;
		m_bgCanvasGroup.gameObject.SetActive(value: true);
		Vector3 position2 = CameraController.GameCamera.transform.position;
		position2.z = m_bgCanvasGroup.transform.position.z;
		m_bgCanvasGroup.transform.position = position2;
		yield return TweenManager.TweenTo_UnscaledTime(m_bgCanvasGroup, m_fadeToBlackTime, Ease.None, "alpha", 1).TweenCoroutine;
		m_waitYield.CreateNew(0.25f, useUnscaledTime: true);
		yield return m_waitYield;
		m_reenactmentController.Initialise(m_reenactmentGridLayout.constraintCount);
		player.Animator.SetBool("Stunned", value: false);
		if (SaveManager.PlayerSaveData.CurrentCharacter.IsRetired)
		{
			player.Animator.SetTrigger("Retire");
		}
		else
		{
			int num2 = Player_EV.PLAYER_DEATH_ANIM_OVERRIDE;
			if (num2 == -1 && (TraitManager.IsTraitActive(TraitType.Fart) || TraitManager.IsTraitActive(TraitType.SuperFart)))
			{
				num2 = 9;
			}
			if (SaveManager.PlayerSaveData.IsInHeirloom)
			{
				num2 = 8;
			}
			if (num2 > -1 && num2 < 12)
			{
				player.Animator.SetTrigger("Death" + num2);
			}
			else
			{
				int num3 = UnityEngine.Random.Range(0, 12);
				while (num3 == 9 || num3 == 8)
				{
					num3 = UnityEngine.Random.Range(0, 12);
				}
				player.Animator.SetTrigger("Death" + num3);
			}
			if (num2 == 9)
			{
				TweenManager.RunFunction_UnscaledTime(2f, this, "RunFartEffect");
			}
		}
		if (player.CharacterClass.WeaponAbilityType != AbilityType.BoxingGloveWeapon && player.CharacterClass.WeaponAbilityType != AbilityType.ExplosiveHandsWeapon)
		{
			if ((bool)player.LookController.CurrentWeaponGeo)
			{
				player.LookController.CurrentWeaponGeo.gameObject.SetActive(value: false);
			}
			if ((bool)player.LookController.SecondaryWeaponGeo)
			{
				player.LookController.SecondaryWeaponGeo.gameObject.SetActive(value: false);
			}
		}
		if (m_deathRecapFadeInUnityEvent != null)
		{
			m_deathRecapFadeInUnityEvent.Invoke();
		}
		TweenManager.TweenTo_UnscaledTime(m_reenactmentCanvasGroup, 1f, Ease.None, "alpha", 1);
		yield return null;
		while (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
		{
			yield return null;
		}
		TweenManager.TweenTo_UnscaledTime(m_portraitCanvasGroup, 0.25f, Ease.Quad.EaseIn, "alpha", 1);
		if (m_portraitAppearedUnityEvent != null)
		{
			m_portraitAppearedUnityEvent.Invoke();
		}
		if (m_masteryUnlocked)
		{
			TweenManager.TweenTo_UnscaledTime(m_rankController.Text, 0.5f, Ease.None, "alpha", 1);
			TweenManager.TweenTo_UnscaledTime(m_rankController.CounterText, 0.5f, Ease.None, "alpha", 1);
		}
		TweenManager.TweenTo_UnscaledTime(m_slainByController.Text, 0.5f, Ease.None, "alpha", 1);
		TweenManager.TweenTo_UnscaledTime(m_line, 0.5f, Ease.None, "delay", 0.25f, "color.a", 1);
		yield return TweenManager.TweenTo_UnscaledTime(m_partingWordsController.Text, 0.5f, Ease.None, "delay", 0.5f, "alpha", 1).TweenCoroutine;
		m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
		yield return m_waitYield;
		m_reenactmentController.Run();
		if (m_masteryUnlocked)
		{
			m_rankController.StartTally();
		}
	}

	private IEnumerator VictoryAnimationCoroutine()
	{
		PlayerController player = PlayerManager.GetPlayerController();
		if ((bool)player)
		{
			player.Animator.updateMode = AnimatorUpdateMode.UnscaledTime;
			Vector3 position = CameraController.GameCamera.transform.position;
			position.z = CameraController.SoloCam.transform.position.z;
			CameraController.SoloCam.transform.position = position;
			CameraController.SoloCam.gameObject.SetActive(value: true);
			CameraController.SoloCam.AddToCameraLayer(player.Visuals);
			if (CameraController.GameCamera.orthographicSize != 9f)
			{
				CameraController.SoloCam.Camera.orthographicSize = 9f;
			}
			Vector3 vector = player.transform.position - m_playerPositionObj.position;
			vector += CameraController.GameCamera.transform.position;
			Vector3 position2 = CameraController.SoloCam.transform.position;
			position2.x = vector.x;
			position2.y = vector.y;
			CameraController.SoloCam.transform.position = position2;
			if (!Mathf.Approximately(AspectRatioManager.CurrentGameAspectRatio, 1.7777778f) && AspectRatioManager.CurrentGameAspectRatio < 1.7777778f)
			{
				float num = AspectRatioManager.CurrentGameAspectRatio / 1.7777778f;
				Vector3 localScale = player.transform.localScale * num;
				player.transform.localScale = localScale;
			}
			m_bgCanvasGroup.alpha = 1f;
			m_bgCanvasGroup.gameObject.SetActive(value: true);
			Vector3 position3 = CameraController.GameCamera.transform.position;
			position3.z = m_bgCanvasGroup.transform.position.z;
			m_bgCanvasGroup.transform.position = position3;
			m_reenactmentController.Initialise(m_reenactmentGridLayout.constraintCount);
			m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
			yield return m_waitYield;
			player.Animator.SetBool("Victory", value: true);
			if (m_deathRecapFadeInUnityEvent != null)
			{
				m_deathRecapFadeInUnityEvent.Invoke();
			}
			TweenManager.TweenTo_UnscaledTime(m_reenactmentCanvasGroup, 1f, Ease.None, "alpha", 1);
			yield return null;
			while (player.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime < 1f)
			{
				yield return null;
			}
			if (m_masteryUnlocked)
			{
				TweenManager.TweenTo_UnscaledTime(m_rankController.Text, 0.5f, Ease.None, "alpha", 1);
				TweenManager.TweenTo_UnscaledTime(m_rankController.CounterText, 0.5f, Ease.None, "alpha", 1);
			}
			TweenManager.TweenTo_UnscaledTime(m_slainByController.Text, 0.5f, Ease.None, "alpha", 1);
			TweenManager.TweenTo_UnscaledTime(m_line, 0.5f, Ease.None, "delay", 0.25f, "color.a", 1);
			yield return TweenManager.TweenTo_UnscaledTime(m_partingWordsController.Text, 0.5f, Ease.None, "delay", 0.5f, "alpha", 1).TweenCoroutine;
			m_waitYield.CreateNew(0.5f, useUnscaledTime: true);
			yield return m_waitYield;
			m_reenactmentController.Run();
			if (m_masteryUnlocked)
			{
				m_rankController.StartTally();
			}
		}
	}

	private void RunFartEffect()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (TraitManager.IsTraitActive(TraitType.Fart))
		{
			Vector3 vector = new Vector3(-0.25f, 1f, 0f);
			BaseEffect baseEffect = EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "Death_Fart_Effect", playerController.transform.position + vector, 0f, EffectStopType.Gracefully);
			if (!playerController.IsFacingRight)
			{
				Vector3 position = baseEffect.transform.position;
				position.x -= vector.x * 2f;
				baseEffect.transform.position = position;
				baseEffect.Flip();
			}
			AudioManager.PlayOneShot(null, "event:/SFX/Spells/sfx_spell_fart_forDeath", baseEffect.transform.position);
			return;
		}
		Vector3 vector2 = new Vector3(-2f, 1f, 0f);
		Vector2 vector3 = new Vector2(1f, 1f);
		BaseEffect baseEffect2 = EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "Death_SuperFart_Effect", playerController.transform.position + vector2, 0f, EffectStopType.Gracefully);
		BaseEffect baseEffect3 = EffectManager.PlayEffect(playerController.gameObject, playerController.Animator, "Death_SuperFartExplosion_Effect", playerController.transform.position + vector2, 0f, EffectStopType.Gracefully);
		baseEffect3.transform.localScale = new Vector3(3f, 3f, 1f);
		if (!playerController.IsFacingRight)
		{
			Vector3 position2 = baseEffect2.transform.position;
			position2.x -= vector2.x * 2f;
			baseEffect2.transform.position = position2;
			position2 = baseEffect3.transform.position;
			position2.x -= vector2.x * 2f;
			baseEffect3.transform.position = position2;
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Spells/sfx_spell_superfart_forDeath", baseEffect2.transform.position);
		TweenManager.TweenBy_UnscaledTime(playerController.Visuals.transform, 0.5f, Ease.Quad.EaseOut, "localPosition.x", vector3.x * 7f);
	}

	private void AddInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.AddInputEventDelegate(m_onRestartInLineagePressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			base.RewiredPlayer.AddInputEventDelegate(m_moveMapHorizontal, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Horizontal");
			base.RewiredPlayer.AddInputEventDelegate(m_moveMapVertical, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Vertical");
			base.RewiredPlayer.AddInputEventDelegate(m_zoomMapVertical, UpdateLoopType.Update, InputActionEventType.AxisActive, "Window_Vertical_RStick");
		}
	}

	private void RemoveInputListeners()
	{
		if (ReInput.isReady)
		{
			base.RewiredPlayer.RemoveInputEventDelegate(m_onRestartInLineagePressed);
			base.RewiredPlayer.RemoveInputEventDelegate(m_moveMapHorizontal);
			base.RewiredPlayer.RemoveInputEventDelegate(m_moveMapVertical);
			base.RewiredPlayer.RemoveInputEventDelegate(m_zoomMapVertical);
		}
	}

	private void MoveMapVertical(InputActionEventData inputActionEventData)
	{
		if (m_reenactmentController.IsComplete)
		{
			float num = inputActionEventData.GetAxis();
			if (num == 0f)
			{
				num = 0f - inputActionEventData.GetAxisPrev();
			}
			Vector3 position = MapController.Camera.transform.position;
			float num2 = MapController.DeathMapCamera.orthographicSize / 6f;
			float num3 = num * num2 * 14f * Time.unscaledDeltaTime;
			position.y += num3;
			MapController.SetMapCameraPosition(position);
		}
	}

	private void MoveMapHorizontal(InputActionEventData inputActionEventData)
	{
		if (m_reenactmentController.IsComplete)
		{
			float num = inputActionEventData.GetAxis();
			if (num == 0f)
			{
				num = 0f - inputActionEventData.GetAxisPrev();
			}
			Vector3 position = MapController.Camera.transform.position;
			float num2 = MapController.DeathMapCamera.orthographicSize / 6f;
			float num3 = num * num2 * 14f * Time.unscaledDeltaTime;
			position.x += num3;
			MapController.SetMapCameraPosition(position);
		}
	}

	private void ZoomMapVertical(InputActionEventData eventData)
	{
		if (m_reenactmentController.IsComplete)
		{
			float num = 0f - eventData.GetAxis();
			float orthographicSize = Mathf.Clamp(MapController.DeathMapCamera.orthographicSize + num * 14f * Time.unscaledDeltaTime, 3f, 12f);
			MapController.DeathMapCamera.orthographicSize = orthographicSize;
		}
	}

	private void OnRestartInLineagePressed(InputActionEventData obj)
	{
		if (m_reenactmentController.IsComplete && (m_rankController.IsComplete || !m_masteryUnlocked))
		{
			if (m_deathRecapFadeOutUnityEvent != null)
			{
				m_deathRecapFadeOutUnityEvent.Invoke();
			}
			if (!RunVictoryDeathScreen)
			{
				m_deathPanelGO.SetActive(value: false);
				string text = m_slainByController.Text.text;
				string text2 = m_partingWordsController.Text.text;
				string text3 = m_rankController.Text.text;
				string text4 = m_rankController.CounterText.text;
				if (m_playHestiaCutscene)
				{
					SceneLoader_RL.LoadScene(SceneID.Lineage, TransitionID.HestiaDeath);
					return;
				}
				if (m_playTreeCutscene)
				{
					SceneLoader_RL.LoadScene(SceneID.Lineage, TransitionID.TreeDeath);
					return;
				}
				(TransitionLibrary.GetTransitionInstance(TransitionID.PlayerDeathToLineage) as PlayerDeathToLineage_SceneTransition).SetData(m_portraitRT.texture as RenderTexture, m_playerPositionObj.anchoredPosition, text, text2, text3, text4);
				SceneLoader_RL.LoadScene(SceneID.Lineage, TransitionID.PlayerDeathToLineage);
			}
			else
			{
				SceneLoader_RL.LoadScene(SceneID.Parade, TransitionID.FadeToBlackWithLoading);
			}
		}
		else
		{
			m_reenactmentController.FastForward();
			if (m_masteryUnlocked)
			{
				m_rankController.SkipTally();
			}
		}
	}

	public void DeactivatePortrait()
	{
		m_portrait.gameObject.SetActive(value: false);
		m_portraitRenderCamera.gameObject.SetActive(value: false);
		m_portraitCanvasGroup.gameObject.SetActive(value: false);
	}

	private void OnEnemyTrackerEventTriggered(EnemyTrackerData enemyData)
	{
		m_enemiesKilledCounter++;
		m_enemiesDefeatedAmountText.text = m_enemiesKilledCounter.ToString();
	}

	private void OnItemTrackerEventTriggered(ItemTrackerData itemData)
	{
		switch (Economy_EV.GetBaseItemDropType(itemData.ItemDropType))
		{
		case ItemDropType.Coin:
			m_goldCollectedCounter += (int)itemData.Value;
			m_goldCollectedAmountText.text = m_goldCollectedCounter.ToString();
			break;
		case ItemDropType.EquipmentOre:
			m_equipmentOreCollectedCounter += (int)itemData.Value;
			m_equipmentOreCollectedAmountText.text = m_equipmentOreCollectedCounter.ToString();
			break;
		case ItemDropType.RuneOre:
			m_runeOreCollectedCounter += (int)itemData.Value;
			m_runeOreCollectedAmountText.text = m_runeOreCollectedCounter.ToString();
			break;
		case ItemDropType.Soul:
			m_soulCollectedCounter += (int)itemData.Value;
			m_soulCollectedAmountText.text = m_soulCollectedCounter.ToString();
			break;
		}
	}

	private void OnChestTrackerEventTriggered(ChestTrackerData chestData)
	{
	}

	private void OnRoomTrackerEventTriggered(RoomTrackerData roomData)
	{
		MapRoomEntry mapRoomEntry = MapController.GetMapRoomEntry(roomData.Biome, roomData.BiomeControllerIndex);
		if (mapRoomEntry == null)
		{
			return;
		}
		bool flag = false;
		if (m_prevBiomeType != roomData.Biome && !roomData.ViaBiomeTransitionDoor)
		{
			flag = true;
		}
		m_prevBiomeType = roomData.Biome;
		if (roomData.WorldPosition != default(Vector2))
		{
			MapController.SetPlayerIconInRoom(roomData.Biome, roomData.BiomeControllerIndex, roomData.WorldPosition);
		}
		Vector3 playerIconPosition = MapController.PlayerIconPosition;
		if (MapController.DeathMapCamera.orthographicSize < 6f)
		{
			if (flag)
			{
				if (m_cameraZoomTween != null && m_cameraZoomTween.isActiveAndEnabled && m_cameraZoomTween.TweenedObj == MapController.DeathMapCamera && m_cameraZoomTween.ID == "CamZoom")
				{
					m_cameraZoomTween.StopTween(runEndHandlers: false);
				}
				MapController.DeathMapCamera.orthographicSize = 6f;
			}
			Vector3 vector = new Vector3(mapRoomEntry.AbsBounds.xMin, mapRoomEntry.AbsBounds.yMin, 1f);
			Vector3 vector2 = new Vector3(mapRoomEntry.AbsBounds.xMin, mapRoomEntry.AbsBounds.yMax, 1f);
			Vector3 vector3 = new Vector3(mapRoomEntry.AbsBounds.xMax, mapRoomEntry.AbsBounds.yMin, 1f);
			Vector3 vector4 = new Vector3(mapRoomEntry.AbsBounds.xMax, mapRoomEntry.AbsBounds.yMax, 1f);
			Vector3[] array = new Vector3[4] { vector, vector2, vector3, vector4 };
			for (int i = 0; i < 4; i++)
			{
				Vector3 vector5 = array[i];
				if (vector5.x < m_camBoundsRect.x)
				{
					m_camBoundsRect.x = vector5.x;
				}
				if (vector5.x > m_camBoundsRect.w)
				{
					m_camBoundsRect.w = vector5.x;
				}
				if (vector5.y < m_camBoundsRect.y)
				{
					m_camBoundsRect.y = vector5.y;
				}
				if (vector5.y > m_camBoundsRect.z)
				{
					m_camBoundsRect.z = vector5.y;
				}
			}
			array[0] = new Vector3(m_camBoundsRect.x, m_camBoundsRect.y, MapController.DeathMapCamera.transform.position.z);
			array[1] = new Vector3(m_camBoundsRect.x, m_camBoundsRect.z, MapController.DeathMapCamera.transform.position.z);
			array[2] = new Vector3(m_camBoundsRect.w, m_camBoundsRect.y, MapController.DeathMapCamera.transform.position.z);
			array[3] = new Vector3(m_camBoundsRect.w, m_camBoundsRect.z, MapController.DeathMapCamera.transform.position.z);
			float num = float.MaxValue;
			float num2 = float.MinValue;
			float num3 = float.MinValue;
			float num4 = float.MaxValue;
			float value = 0.25f;
			value = Mathf.Clamp(value, 0f, 0.5f);
			for (int j = 0; j < 4; j++)
			{
				Vector3 vector6 = MapController.DeathMapCamera.WorldToViewportPoint(array[j]);
				if (vector6.x < num)
				{
					num = vector6.x;
				}
				if (vector6.x > num2)
				{
					num2 = vector6.x;
				}
				if (vector6.y < num4)
				{
					num4 = vector6.y;
				}
				if (vector6.y > num3)
				{
					num3 = vector6.y;
				}
			}
			float num5 = 0f;
			if (num < value)
			{
				float num6 = (value - num) / (1f - value * 2f);
				if (num6 > num5)
				{
					num5 = num6;
				}
			}
			if (num2 > 1f - value)
			{
				float num7 = (num2 - (1f - value)) / (1f - value * 2f);
				if (num7 > num5)
				{
					num5 = num7;
				}
			}
			if (num4 < value)
			{
				float num8 = (value - num4) / (1f - value * 2f);
				if (num8 > num5)
				{
					num5 = num8;
				}
			}
			if (num3 > 1f - value)
			{
				float num9 = (num3 - (1f - value)) / (1f - value * 2f);
				if (num9 > num5)
				{
					num5 = num9;
				}
			}
			if (num5 > 0f && m_desiredZoomSize != num5)
			{
				if (m_cameraZoomTween != null && m_cameraZoomTween.isActiveAndEnabled && m_cameraZoomTween.TweenedObj == MapController.DeathMapCamera && m_cameraZoomTween.ID == "CamZoom")
				{
					m_cameraZoomTween.StopTween(runEndHandlers: false);
				}
				float num10 = Mathf.Min(6f, MapController.DeathMapCamera.orthographicSize * (1f + num5));
				if (!m_reenactmentController.IsFastforwarding)
				{
					m_cameraZoomTween = TweenManager.TweenTo_UnscaledTime(MapController.DeathMapCamera, 0.5f, Ease.Quad.EaseInOut, "orthographicSize", num10);
					m_cameraZoomTween.ID = "CamZoom";
				}
				else
				{
					MapController.DeathMapCamera.orthographicSize = num10;
				}
				m_desiredZoomSize = num5;
			}
		}
		if (m_desiredCamPos != playerIconPosition)
		{
			if (m_cameraMoveTween != null && m_cameraMoveTween.isActiveAndEnabled && m_cameraMoveTween.TweenedObj == MapController.Camera.transform && m_cameraMoveTween.ID == "CamMove")
			{
				m_cameraMoveTween.StopTween(runEndHandlers: false);
			}
			if (!m_reenactmentController.IsFastforwarding && !flag)
			{
				m_cameraMoveTween = TweenManager.TweenTo_UnscaledTime(MapController.Camera.transform, 0.5f, Ease.Quad.EaseInOut, "position.x", playerIconPosition.x, "position.y", playerIconPosition.y);
				m_cameraMoveTween.ID = "CamMove";
			}
			else
			{
				MapController.Camera.transform.position = new Vector3(playerIconPosition.x, playerIconPosition.y, 0f);
			}
			m_desiredCamPos = playerIconPosition;
		}
	}

	protected override void OnFocus()
	{
	}

	protected override void OnLostFocus()
	{
	}

	protected override void OnPause()
	{
		throw new NotImplementedException();
	}

	protected override void OnUnpause()
	{
		throw new NotImplementedException();
	}

	public void RefreshText(object sender, EventArgs args)
	{
		GameEventTrackerManager.EnemyEventTracker.ForceRefreshText();
		m_partingWordsController.UpdateMessage(RunVictoryDeathScreen);
		m_slainByController.UpdateMessage(RunVictoryDeathScreen);
		m_rankController.UpdateMessage();
	}
}
