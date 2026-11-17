using System;
using System.Collections;
using Cinemachine;
using FMOD.Studio;
using RLAudio;
using RL_Windows;
using Sigtrap.Relays;
using UnityEngine;

public class BossRoomController : BaseSpecialRoomController, IAudioEventEmitter
{
	protected const string CHEST_SPAWN_LOOP_AUDIO = "event:/SFX/Interactables/sfx_env_prop_bossChest_spawn_loop";

	protected const string CAMERA_PAN_TO_BOSS_AUDIO = "event:/SFX/Enemies/sfx_dancingBoss_intro_zoomIn";

	protected const string CAMERA_PAN_TO_PLAYER_AUDIO = "event:/SFX/Enemies/sfx_dancingBoss_intro_zoomOut";

	[SerializeField]
	protected PlayerSaveFlag m_bossSaveFlag;

	[SerializeField]
	protected int m_numBosses;

	[SerializeField]
	protected EnemySpawnController m_bossSpawnController;

	[SerializeField]
	protected ChestSpawnController m_chestSpawnController;

	[SerializeField]
	protected TunnelSpawnController m_tunnelSpawnController;

	[Header("Intro Cutscene Variables")]
	[SerializeField]
	protected CinemachineVirtualCameraManager m_vcamManager;

	[SerializeField]
	protected AnimationCurve m_cameraBlendCurve;

	[SerializeField]
	protected GameObject m_bossStartCamPos;

	[SerializeField]
	protected GameObject m_bossEndCamPos;

	protected int m_numBossesKilled;

	protected int m_numBossesDeadOrDying;

	protected bool m_listenersAssigned;

	protected static CinemachineBlendDefinition m_followBlend;

	protected BossObjectiveCompleteHUDEventArgs m_bossDefeatedArgs;

	protected WaitRL_Yield m_waitYield;

	public Relay IntroStartRelay = new Relay();

	public Relay IntroCompleteRelay = new Relay();

	public Relay OutroStartRelay = new Relay();

	public Relay OutroCompleteRelay = new Relay();

	public Relay<float> BossTookDamageRelay = new Relay<float>();

	public Relay BossDefeatedRelay = new Relay();

	public Relay<int> PhaseChangedRelay = new Relay<int>();

	public Relay BossSpawnAnimRelay = new Relay();

	private string m_description;

	protected EventInstance m_chestAudioEventInstance;

	private Action<MonoBehaviour, EventArgs> m_onBossDeath;

	private Action<MonoBehaviour, EventArgs> m_onBossHealthChange;

	private Action<MonoBehaviour, EventArgs> m_onModeShift;

	public int NumBossesDeadOrDying => m_numBossesDeadOrDying;

	public int NumBossesDead => m_numBossesKilled;

	public int NumBosses => m_numBosses;

	public bool AllBossesDeadOrDying => NumBossesDeadOrDying >= NumBosses;

	public bool AllBossesDead => NumBossesDead >= NumBosses;

	public TunnelSpawnController TunnelSpawnController => m_tunnelSpawnController;

	public virtual EnemySpawnController BossSpawnController => m_bossSpawnController;

	public EnemyController Boss
	{
		get
		{
			if ((bool)BossSpawnController)
			{
				return BossSpawnController.EnemyInstance;
			}
			return null;
		}
	}

	public ChestObj Chest
	{
		get
		{
			if ((bool)m_chestSpawnController)
			{
				return m_chestSpawnController.ChestInstance;
			}
			return null;
		}
	}

	public CinemachineVirtualCameraManager VCamManager => m_vcamManager;

	public bool IsInitialized { get; protected set; }

	public virtual float BossHealthAsPercentage
	{
		get
		{
			if ((bool)Boss)
			{
				return Boss.CurrentHealth / (float)Boss.ActualMaxHealth;
			}
			return 0f;
		}
	}

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_bossDefeatedArgs = new BossObjectiveCompleteHUDEventArgs(EnemyType.None, EnemyRank.None);
		m_followBlend.m_Time = 1f;
		m_followBlend.m_Style = CinemachineBlendDefinition.Style.Custom;
		m_followBlend.m_CustomCurve = m_cameraBlendCurve;
		m_onBossDeath = OnBossDeath;
		m_onBossHealthChange = OnBossHealthChange;
		m_onModeShift = OnModeShift;
	}

	protected virtual void Initialize()
	{
		if (!BossSpawnController)
		{
			Debug.LogFormat("<color=red>| {0} | The <b>Boss Room Controller</b> field in the Inspector is null. This needs to be set for the Boss Room Controller to work correctly. If you're in a Level Editor Scene and this isn't a Room you're testing, you can safely ignore this message.</color>", this);
		}
		else if (!m_chestSpawnController)
		{
			Debug.LogFormat("<color=red>| {0} | The <b>Chest Spawn Controller</b> field in the Inspector is null. This needs to be set for the Boss Room Controller to work correctly. If you're in a Level Editor Scene and this isn't a Room you're testing, you can safely ignore this message.</color>", this);
		}
		else
		{
			Chest.SetOpacity(0f);
			Chest.SetChestLockState(ChestLockState.Locked);
			Chest.gameObject.SetActive(value: false);
		}
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (!IsInitialized)
		{
			Initialize();
		}
		AddListeners();
		StopAllCoroutines();
		StartCoroutine(StartIntro());
	}

	protected override void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerExitRoom(sender, eventArgs);
		m_numBossesKilled = 0;
		m_numBossesDeadOrDying = 0;
		RemoveListeners();
	}

	protected virtual void OnDisable()
	{
		if (m_chestAudioEventInstance.isValid())
		{
			AudioManager.Stop(m_chestAudioEventInstance);
		}
		m_numBossesKilled = 0;
		m_numBossesDeadOrDying = 0;
		RemoveListeners();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_chestAudioEventInstance.isValid())
		{
			m_chestAudioEventInstance.release();
		}
	}

	protected virtual void AddListeners()
	{
		if (!m_listenersAssigned)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyDeath, m_onBossDeath);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyHealthChange, m_onBossHealthChange);
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.EnemyModeShift, m_onModeShift);
			m_listenersAssigned = true;
		}
	}

	protected virtual void RemoveListeners()
	{
		if (m_listenersAssigned)
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyDeath, m_onBossDeath);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyHealthChange, m_onBossHealthChange);
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.EnemyModeShift, m_onModeShift);
			m_listenersAssigned = false;
		}
	}

	protected virtual void OnBossDeath(object sender, EventArgs args)
	{
		if (args is EnemyDeathEventArgs enemyDeathEventArgs && (bool)enemyDeathEventArgs.Victim && enemyDeathEventArgs.Victim.IsBoss)
		{
			m_numBossesKilled++;
		}
		if (AllBossesDead)
		{
			StopAllCoroutines();
			StartCoroutine(StartOutro());
		}
	}

	protected virtual void OnBossHealthChange(object sender, EventArgs args)
	{
		HealthChangeEventArgs healthChangeEventArgs = args as HealthChangeEventArgs;
		if (healthChangeEventArgs.PrevHealthValue > 0f && healthChangeEventArgs.NewHealthValue <= 0f)
		{
			m_numBossesDeadOrDying++;
			if (AllBossesDeadOrDying)
			{
				ProjectileManager.DisableAllProjectiles(true, "EnemyProjectile");
				PlayerManager.GetPlayerController().TakesNoDamage = true;
			}
		}
		BossTookDamageRelay.Dispatch(BossHealthAsPercentage);
	}

	protected virtual void OnModeShift(object sender, EventArgs args)
	{
		PhaseChangedRelay.Dispatch(1);
	}

	protected virtual IEnumerator StartIntro()
	{
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (AnimatorUtility.HasParameter(Boss.Animator, "Turn"))
		{
			Boss.Animator.ResetTrigger("Turn");
		}
		if ((Boss.transform.localPosition.x < playerController.transform.localPosition.x && !Boss.CharacterCorgi.IsFacingRight) || (Boss.transform.localPosition.x > playerController.transform.localPosition.x && Boss.CharacterCorgi.IsFacingRight))
		{
			Boss.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: false, ignoreTurnParam: true);
		}
		if ((playerController.transform.localPosition.x < Boss.transform.localPosition.x && !playerController.IsFacingRight) || (playerController.transform.localPosition.x > Boss.transform.localPosition.x && playerController.IsFacingRight))
		{
			playerController.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: false, ignoreTurnParam: true);
		}
		yield return null;
		IntroStartRelay.Dispatch();
		while (!Boss.IsInitialized)
		{
			yield return null;
		}
		while (!Boss.LogicController.IsInitialized)
		{
			yield return null;
		}
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, null, null);
		PlayAudio("event:/SFX/Enemies/sfx_dancingBoss_intro_zoomIn");
		CinemachineConfiner_RL confiner = Boss.Room.CinemachineCamera.GetComponent<CinemachineConfiner_RL>();
		confiner.enabled = false;
		Boss.Room.CinemachineCamera.SetFollowTarget(m_bossStartCamPos.transform);
		if (AnimatorUtility.HasState(Boss.Animator, "Intro_Idle"))
		{
			Boss.Animator.Play("Intro_Idle");
		}
		if (!WindowManager.GetIsWindowLoaded(WindowID.BossIntro))
		{
			WindowManager.LoadWindow(WindowID.BossIntro);
		}
		BossIntroWindowController bossWindow = WindowManager.GetWindowController(WindowID.BossIntro) as BossIntroWindowController;
		bossWindow.SetEnemyType(Boss.EnemyType, Boss.EnemyRank);
		WindowManager.SetWindowIsOpen(WindowID.BossIntro, isOpen: true);
		m_waitYield.CreateNew(1f, useUnscaledTime: true);
		yield return m_waitYield;
		CinemachineBlendDefinition storedBlend = CameraController.CinemachineBrain.m_DefaultBlend;
		CameraController.CinemachineBrain.m_DefaultBlend = m_followBlend;
		float zoomLevel = CameraController.ZoomLevel;
		if (zoomLevel != 1f)
		{
			VCamManager.SetLensSize(CameraController.GetVirtualCameraLensSize(zoomLevel));
		}
		Boss.Room.CinemachineCamera.SetIsActiveCamera(isActiveCamera: false);
		VCamManager.VirtualCamera.gameObject.SetActive(value: true);
		VCamManager.SetIsActiveCamera(isActiveCamera: true);
		VCamManager.SetFollowTarget(m_bossEndCamPos.transform);
		BossSpawnAnimRelay.Dispatch();
		yield return Boss.LogicController.LogicScript.SpawnAnim();
		bossWindow.DisplayBossName = true;
		while (WindowManager.GetIsWindowOpen(WindowID.BossIntro))
		{
			yield return null;
		}
		confiner.enabled = true;
		Boss.Room.CinemachineCamera.VirtualCamera.gameObject.SetActive(value: true);
		Boss.Room.CinemachineCamera.SetIsActiveCamera(isActiveCamera: true);
		PlayAudio("event:/SFX/Enemies/sfx_dancingBoss_intro_zoomOut");
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayPlayerHUD, null, new PlayerHUDVisibilityEventArgs(0.5f));
		Boss.LogicController.DisableLogicActivationByDistance = false;
		VCamManager.VirtualCamera.gameObject.SetActive(value: false);
		CameraController.CinemachineBrain.m_DefaultBlend = storedBlend;
		IntroCompleteRelay.Dispatch();
		RewiredMapController.SetIsInCutscene(inCutscene: false);
	}

	protected virtual IEnumerator StartOutro()
	{
		OutroStartRelay.Dispatch();
		m_waitYield.CreateNew(2f);
		yield return m_waitYield;
		float num = 4f;
		InitializeObjectiveCompleteArgs(num);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_bossDefeatedArgs);
		m_waitYield.CreateNew(num);
		yield return m_waitYield;
		float chestOpacity = 0f;
		Chest.gameObject.SetActive(value: true);
		Chest.transform.SetParent(base.transform);
		Vector3 position = Chest.transform.position;
		position.y += 1f;
		Chest.transform.position = position;
		Chest.Interactable.SetIsInteractableActive(isActive: false);
		TweenManager.TweenBy(Chest.transform, 1f, Ease.Quad.EaseOut, "position.y", -1);
		m_chestAudioEventInstance = AudioUtility.GetEventInstance("event:/SFX/Interactables/sfx_env_prop_bossChest_spawn_loop", Chest.transform);
		AudioManager.PlayAttached(this, m_chestAudioEventInstance, Chest.gameObject);
		while (chestOpacity < 1f)
		{
			Chest.SetOpacity(chestOpacity);
			chestOpacity += Time.deltaTime;
			yield return null;
		}
		AudioManager.Stop(m_chestAudioEventInstance);
		Chest.Interactable.SetIsInteractableActive(isActive: true);
		Chest.SetChestLockState(ChestLockState.Unlocked);
		while (!Chest.IsOpen)
		{
			yield return null;
		}
		SetBossFlagDefeated();
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		float itemDropDuration = Time.time + 5f;
		while (ItemDropManager.HasActiveItemDrops && Time.time < itemDropDuration)
		{
			yield return null;
		}
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		PlayerManager.GetPlayerController().TakesNoDamage = false;
		TeleportOut();
		OutroCompleteRelay.Dispatch();
	}

	protected virtual void TeleportOut()
	{
		BiomeController biomeController = (GameUtility.IsInLevelEditor ? OnPlayManager.BiomeController : WorldBuilder.GetBiomeController(BiomeType.Castle));
		BaseRoom baseRoom = (biomeController ? biomeController.TransitionRoom : null);
		if ((bool)baseRoom)
		{
			if ((bool)TunnelSpawnController && (bool)TunnelSpawnController.Tunnel)
			{
				RewiredMapController.SetIsInCutscene(inCutscene: true);
				CutsceneManager.InitializeCutscene(m_bossSaveFlag, TunnelSpawnController.Tunnel.Destination);
				PortraitCutsceneController component = baseRoom.gameObject.GetComponent<PortraitCutsceneController>();
				TunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true, component.TunnelSpawner.Tunnel);
			}
			else
			{
				Debug.Log("Could not exit boss room, tunnel spawn controller is null.");
			}
		}
		else if ((bool)TunnelSpawnController && (bool)TunnelSpawnController.Tunnel)
		{
			TunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true);
		}
		else
		{
			Debug.Log("Could not exit boss room, tunnel spawn controller is null.");
		}
	}

	protected virtual void InitializeObjectiveCompleteArgs(float bossDefeatedDisplayDuration)
	{
		m_bossDefeatedArgs.Initialize(Boss.EnemyType, EnemyRank.Basic, bossDefeatedDisplayDuration);
	}

	protected virtual void SetBossFlagDefeated()
	{
		BossDefeatedRelay.Dispatch();
		SaveManager.PlayerSaveData.SetFlag(m_bossSaveFlag, value: true);
		SetHighestBossNGBeaten(m_bossSaveFlag);
		SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(SaveManager.CurrentProfile);
		SaveManager.SaveCurrentProfileGameData(saveBatch, SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
		SaveManager.SaveCurrentProfileGameData(saveBatch, SaveDataType.GameMode, SavingType.FileOnly, ignoreTimeRestriction: true);
		saveBatch.End();
		BossEntranceRoomController.RunDoorCrumbleAnimation = true;
	}

	private void SetHighestBossNGBeaten(PlayerSaveFlag saveFlag)
	{
		switch (saveFlag)
		{
		case PlayerSaveFlag.CastleBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CastleBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Castle_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossCastleDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.CastleBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CastleBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossCastleAdvancedDefeated, StoreType.All);
			}
			break;
		case PlayerSaveFlag.BridgeBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.BridgeBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Bridge_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossBridgeDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.BridgeBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.BridgeBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossBridgeAdvancedDefeated, StoreType.All);
			}
			break;
		case PlayerSaveFlag.ForestBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.ForestBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Forest_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossForestDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.ForestBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.ForestBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossForestAdvancedDefeated, StoreType.All);
			}
			break;
		case PlayerSaveFlag.StudyBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.StudyBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Study_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossStudyDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.StudyBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.StudyBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossStudyAdvancedDefeated, StoreType.All);
			}
			break;
		case PlayerSaveFlag.TowerBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.TowerBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Tower_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossTowerDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.TowerBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.TowerBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossTowerAdvancedDefeated, StoreType.All);
			}
			break;
		case PlayerSaveFlag.CaveBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CaveBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Cave_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossCaveDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.CaveBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.CaveBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossCaveAdvancedDefeated, StoreType.All);
			}
			break;
		case PlayerSaveFlag.GardenBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.GardenBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Garden_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossGardenDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.GardenBossUp) || BurdenManager.IsBurdenActive(BurdenType.FinalBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.GardenBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossGardenAdvancedDefeated, StoreType.All);
			}
			break;
		case PlayerSaveFlag.FinalBoss_Defeated:
			SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.FinalBoss_Defeated_FirstTime, value: true);
			SaveManager.ModeSaveData.SetHighestNGBossBeaten(SaveManager.PlayerSaveData.GameModeType, BossID.Final_Boss, SaveManager.PlayerSaveData.NewGamePlusLevel, forceOverride: false);
			StoreAPIManager.GiveAchievement(AchievementType.BossFinalDefeated, StoreType.All);
			if (BurdenManager.IsBurdenActive(BurdenType.FinalBossUp))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.FinalBoss_Prime_Defeated_FirstTime, value: true);
				StoreAPIManager.GiveAchievement(AchievementType.BossFinalAdvancedDefeated, StoreType.All);
			}
			break;
		}
	}

	protected void PlayAudio(string audioPath)
	{
		if (!string.IsNullOrEmpty(audioPath))
		{
			AudioManager.Play(this, audioPath);
		}
	}
}
