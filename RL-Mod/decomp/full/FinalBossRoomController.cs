using System;
using System.Collections;
using Cinemachine;
using FMOD.Studio;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class FinalBossRoomController : BossRoomController
{
	private const string TRANSITION_TO_LIGHT_SFX_NAME = "event:/SFX/Enemies/Cain/sfx_cain_arena_shift_darkToLight";

	private const string TRANSITION_TO_DARK_SFX_NAME = "event:/SFX/Enemies/Cain/sfx_cain_arena_shift_lightToDark";

	[SerializeField]
	private TunnelSpawnController m_earthShiftTunnelSpawner;

	[SerializeField]
	private MobilePostProcessingProfile m_defaultBGProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_lightBGProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_darkBGProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_lightBGAdvancedProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_darkBGAdvancedProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_defaultFGProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_lightFGProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_darkFGProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_lightFGAdvancedProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_darkFGAdvancedProfile;

	[SerializeField]
	private GameObject m_rootsGO;

	[SerializeField]
	private PropSpawnController[] m_windowsToSmashArray;

	[SerializeField]
	private EnemySpawnController m_bossUpSpawnController;

	private Vector3 m_storedStartingCamPos = Vector3.zero;

	private Action<bool, float> m_colourShiftRoom;

	private BaseEffect m_flameEffect;

	private Animator[] m_rootsArray;

	private EventInstance m_introSFXEventInstance_01;

	private EventInstance m_introSFXEventInstance_02;

	private EventInstance m_currentActiveEventInstance;

	private float m_introSFXStartTime;

	private bool m_bossIntroStopSpeedUpRelayAdded;

	private Action m_onBossIntroStopSpeedUp;

	private Action m_onBossIntroStartSpeedUp;

	public override EnemySpawnController BossSpawnController
	{
		get
		{
			if (BurdenManager.IsBurdenActive(BurdenType.FinalBossUp))
			{
				return m_bossUpSpawnController;
			}
			return base.BossSpawnController;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_colourShiftRoom = ColourShiftRoom;
		m_onBossIntroStopSpeedUp = OnBossIntroStopSpeedUp;
		m_onBossIntroStartSpeedUp = OnBossIntroStartSpeedUp;
		m_rootsArray = m_rootsGO.GetComponentsInChildren<Animator>();
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (m_introSFXEventInstance_01.isValid())
		{
			m_introSFXEventInstance_01.release();
		}
		if (m_introSFXEventInstance_02.isValid())
		{
			m_introSFXEventInstance_02.release();
		}
	}

	protected override void InitializeObjectiveCompleteArgs(float bossDefeatedDisplayDuration)
	{
		string @string = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_FINAL_BOSS_DEFEATED_TITLE_1", isFemale: false);
		m_bossDefeatedArgs.Initialize(base.Boss.EnemyType, EnemyRank.Basic, bossDefeatedDisplayDuration, @string);
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (!WindowManager.GetIsWindowLoaded(WindowID.BossIntro))
		{
			WindowManager.LoadWindow(WindowID.BossIntro);
		}
		(WindowManager.GetWindowController(WindowID.BossIntro) as BossIntroWindowController).StopSpeedUpRelay.AddListener(m_onBossIntroStopSpeedUp);
		(WindowManager.GetWindowController(WindowID.BossIntro) as BossIntroWindowController).StartSpeedUpRelay.AddListener(m_onBossIntroStartSpeedUp);
		m_bossIntroStopSpeedUpRelayAdded = true;
		m_currentActiveEventInstance = default(EventInstance);
		if (!m_introSFXEventInstance_01.isValid())
		{
			m_introSFXEventInstance_01 = AudioUtility.GetEventInstance("event:/SFX/Enemies/Cain/sfx_cain_intro_sequence", base.transform);
		}
		if (!m_introSFXEventInstance_02.isValid())
		{
			m_introSFXEventInstance_02 = AudioUtility.GetEventInstance("event:/SFX/Enemies/Cain/sfx_cain_intro_sequence_02", base.transform);
		}
		if (BurdenManager.IsBurdenActive(BurdenType.FinalBossUp))
		{
			if ((bool)m_bossSpawnController.EnemyInstance)
			{
				m_bossSpawnController.EnemyInstance.gameObject.SetActive(value: false);
			}
		}
		else if ((bool)m_bossUpSpawnController.EnemyInstance)
		{
			m_bossUpSpawnController.EnemyInstance.gameObject.SetActive(value: false);
		}
		if (m_storedStartingCamPos == Vector3.zero)
		{
			m_storedStartingCamPos = m_bossStartCamPos.transform.position;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		if ((playerController.transform.localPosition.x < base.Boss.transform.localPosition.x && !playerController.IsFacingRight) || (playerController.transform.localPosition.x > base.Boss.transform.localPosition.x && playerController.IsFacingRight))
		{
			playerController.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: false, ignoreTurnParam: true);
		}
		if (!SaveManager.PlayerSaveData.SpokenToFinalBoss)
		{
			StartCoroutine(DisableBoss());
		}
		else
		{
			StartCoroutine(SetupColourShiftRelayCoroutine());
		}
	}

	private IEnumerator DisableBoss()
	{
		while (!base.Boss.IsInitialized)
		{
			yield return null;
		}
		base.Boss.gameObject.SetActive(value: false);
		yield return SetupColourShiftRelayCoroutine();
	}

	private IEnumerator SetupColourShiftRelayCoroutine()
	{
		while (!base.Boss.IsInitialized)
		{
			yield return null;
		}
		FinalBoss_Basic_AIScript finalBoss_Basic_AIScript = base.Boss.LogicController.LogicScript as FinalBoss_Basic_AIScript;
		if ((bool)finalBoss_Basic_AIScript)
		{
			finalBoss_Basic_AIScript.ColourShiftRelay.AddListener(m_colourShiftRoom);
		}
	}

	protected override void OnDisable()
	{
		if (m_bossIntroStopSpeedUpRelayAdded)
		{
			m_bossIntroStopSpeedUpRelayAdded = false;
			(WindowManager.GetWindowController(WindowID.BossIntro) as BossIntroWindowController).StopSpeedUpRelay.RemoveListener(m_onBossIntroStopSpeedUp);
			(WindowManager.GetWindowController(WindowID.BossIntro) as BossIntroWindowController).StartSpeedUpRelay.RemoveListener(m_onBossIntroStartSpeedUp);
		}
		if ((bool)base.Boss && base.Boss.IsInitialized)
		{
			FinalBoss_Basic_AIScript finalBoss_Basic_AIScript = base.Boss.LogicController.LogicScript as FinalBoss_Basic_AIScript;
			if ((bool)finalBoss_Basic_AIScript)
			{
				finalBoss_Basic_AIScript.ColourShiftRelay.RemoveListener(m_colourShiftRoom);
			}
		}
		if ((bool)m_flameEffect && m_flameEffect.IsPlaying)
		{
			m_flameEffect.Stop(EffectStopType.Immediate);
		}
		m_flameEffect = null;
		base.OnDisable();
	}

	private void ColourShiftRoom(bool shiftToWhite, float lerpSpeed)
	{
		if (!base.Boss || base.Boss.IsDead)
		{
			return;
		}
		bool flag = base.Boss.EnemyRank > EnemyRank.Basic;
		if (shiftToWhite)
		{
			if (flag)
			{
				if ((bool)m_flameEffect && m_flameEffect.IsPlaying)
				{
					m_flameEffect.Stop(EffectStopType.Gracefully);
					m_flameEffect = null;
				}
				CameraController.LerpBGPostProcessToProfile(m_lightBGAdvancedProfile, lerpSpeed);
				CameraController.LerpFGPostProcessToProfile(m_lightFGAdvancedProfile, lerpSpeed);
			}
			else
			{
				CameraController.LerpBGPostProcessToProfile(m_lightBGProfile, lerpSpeed);
				CameraController.LerpFGPostProcessToProfile(m_lightFGProfile, lerpSpeed);
			}
			AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_arena_shift_darkToLight", CameraController.GameCamera.transform.position);
			return;
		}
		if (flag)
		{
			if (!m_flameEffect || !m_flameEffect.IsPlaying)
			{
				m_flameEffect = EffectManager.PlayEffect(base.gameObject, null, "CainPrime_Flames_Effect", base.transform.position, 99999f, EffectStopType.Gracefully);
			}
			CameraController.LerpBGPostProcessToProfile(m_darkBGAdvancedProfile, lerpSpeed);
			CameraController.LerpFGPostProcessToProfile(m_darkFGAdvancedProfile, lerpSpeed);
		}
		else
		{
			CameraController.LerpBGPostProcessToProfile(m_darkBGProfile, lerpSpeed);
			CameraController.LerpFGPostProcessToProfile(m_darkFGProfile, lerpSpeed);
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_arena_shift_lightToDark", CameraController.GameCamera.transform.position);
	}

	protected override IEnumerator StartIntro()
	{
		if (!SaveManager.PlayerSaveData.SpokenToFinalBoss)
		{
			_ = base.Room.gameObject.FindObjectReference<PropSpawnController>("CainProp").PropInstance;
			yield break;
		}
		if (base.Boss.EnemyRank > EnemyRank.Basic)
		{
			int trigger = Animator.StringToHash("Extend");
			Animator[] rootsArray = m_rootsArray;
			for (int i = 0; i < rootsArray.Length; i++)
			{
				rootsArray[i].SetTrigger(trigger);
			}
		}
		ColourShiftRoom(shiftToWhite: true, 0.75f);
		yield return base.StartIntro();
	}

	public void StartIntroManually()
	{
		StartCoroutine(StartIntro2());
	}

	private IEnumerator StartIntro2()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		m_bossStartCamPos.transform.position = playerController.transform.position;
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		if (AnimatorUtility.HasParameter(base.Boss.Animator, "Turn"))
		{
			base.Boss.Animator.ResetTrigger("Turn");
		}
		if ((base.Boss.transform.localPosition.x < playerController.transform.localPosition.x && !base.Boss.CharacterCorgi.IsFacingRight) || (base.Boss.transform.localPosition.x > playerController.transform.localPosition.x && base.Boss.CharacterCorgi.IsFacingRight))
		{
			base.Boss.CharacterCorgi.Flip(IgnoreFlipOnDirectionChange: false, ignoreTurnParam: true);
		}
		yield return null;
		IntroStartRelay.Dispatch();
		while (!base.Boss.IsInitialized)
		{
			yield return null;
		}
		while (!base.Boss.LogicController.IsInitialized)
		{
			yield return null;
		}
		PropSpawnController propSpawnController = base.Room.gameObject.FindObjectReference<PropSpawnController>("CainProp");
		CainPropController cainProp = propSpawnController.PropInstance.GetComponent<CainPropController>();
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, null, null);
		PlayAudio("event:/SFX/Enemies/sfx_dancingBoss_intro_zoomIn");
		base.Boss.Room.CinemachineCamera.SetFollowTarget(m_bossStartCamPos.transform);
		if (!WindowManager.GetIsWindowLoaded(WindowID.BossIntro))
		{
			WindowManager.LoadWindow(WindowID.BossIntro);
		}
		BossIntroWindowController bossWindow = WindowManager.GetWindowController(WindowID.BossIntro) as BossIntroWindowController;
		bossWindow.SetEnemyType(base.Boss.EnemyType, base.Boss.EnemyRank);
		bossWindow.FadeInBarsAtStart = true;
		WindowManager.SetWindowIsOpen(WindowID.BossIntro, isOpen: true);
		m_waitYield.CreateNew(1f, useUnscaledTime: true);
		yield return m_waitYield;
		CinemachineBlendDefinition storedBlend = CameraController.CinemachineBrain.m_DefaultBlend;
		CameraController.CinemachineBrain.m_DefaultBlend = BossRoomController.m_followBlend;
		float zoomLevel = CameraController.ZoomLevel;
		if (zoomLevel != 1f)
		{
			base.VCamManager.SetLensSize(CameraController.GetVirtualCameraLensSize(zoomLevel));
		}
		base.Boss.Room.CinemachineCamera.SetIsActiveCamera(isActiveCamera: false);
		base.VCamManager.VirtualCamera.gameObject.SetActive(value: true);
		base.VCamManager.SetIsActiveCamera(isActiveCamera: true);
		base.VCamManager.SetFollowTarget(m_bossEndCamPos.transform);
		m_bossStartCamPos.transform.position = m_storedStartingCamPos;
		cainProp.Animator.Play("Eat");
		m_waitYield.CreateNew(0.55f);
		yield return m_waitYield;
		if (m_introSFXEventInstance_01.isValid())
		{
			AudioManager.Play(null, m_introSFXEventInstance_01);
			m_currentActiveEventInstance = m_introSFXEventInstance_01;
			m_introSFXStartTime = Time.time;
		}
		m_waitYield.CreateNew(3.45f);
		yield return m_waitYield;
		cainProp.Animator.Play("Poisoned");
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		cainProp.Animator.speed = 0f;
		bossWindow.StopSpeedUp();
		if (m_introSFXEventInstance_01.isValid())
		{
			AudioManager.Stop(m_introSFXEventInstance_01);
		}
		m_currentActiveEventInstance = default(EventInstance);
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_NAME_CAIN_NAME_1", "LOC_ID_CAIN_ENDING_REFIGHT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		if (!WindowManager.GetIsWindowLoaded(WindowID.Dialogue))
		{
			WindowManager.LoadWindow(WindowID.Dialogue);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		m_waitYield.CreateNew(0.24f);
		yield return m_waitYield;
		if (m_introSFXEventInstance_02.isValid())
		{
			AudioManager.Play(null, m_introSFXEventInstance_02);
			m_currentActiveEventInstance = m_introSFXEventInstance_02;
			m_introSFXStartTime = Time.time;
		}
		cainProp.Animator.speed = 1f;
		StartCoroutine(MovePlayerToEndPos());
		m_waitYield.CreateNew(0.26f);
		yield return m_waitYield;
		BossSpawnAnimRelay.Dispatch();
		base.Boss.gameObject.SetActive(value: true);
		if (TraitManager.IsTraitActive(TraitType.EnemiesCensored))
		{
			EnemiesCensored_Trait.ApplyCensoredEffect(base.Boss);
		}
		if (TraitManager.IsTraitActive(TraitType.CheerOnKills))
		{
			(TraitManager.GetActiveTrait(TraitType.CheerOnKills) as CheerOnKills_Trait).ApplyOnEnemy(base.Boss, increment: false);
		}
		cainProp.gameObject.SetLayerRecursively(29);
		cainProp.Cape.gameObject.SetLayerRecursively(0);
		cainProp.Animator.Play("Transform");
		yield return base.Boss.LogicController.LogicScript.SpawnAnim();
		if (base.Boss.EnemyRank > EnemyRank.Basic)
		{
			int trigger = Animator.StringToHash("Extend");
			Animator[] rootsArray = m_rootsArray;
			for (int i = 0; i < rootsArray.Length; i++)
			{
				rootsArray[i].SetTrigger(trigger);
			}
		}
		ColourShiftRoom(shiftToWhite: true, 0.75f);
		cainProp.gameObject.SetActive(value: false);
		cainProp.gameObject.SetLayerRecursively(0);
		bossWindow.DisplayBossName = true;
		while (WindowManager.GetIsWindowOpen(WindowID.BossIntro))
		{
			yield return null;
		}
		base.Boss.Room.CinemachineCamera.VirtualCamera.gameObject.SetActive(value: true);
		base.Boss.Room.CinemachineCamera.SetIsActiveCamera(isActiveCamera: true);
		PlayAudio("event:/SFX/Enemies/sfx_dancingBoss_intro_zoomOut");
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayPlayerHUD, null, new PlayerHUDVisibilityEventArgs(0.5f));
		base.Boss.LogicController.DisableLogicActivationByDistance = false;
		base.VCamManager.VirtualCamera.gameObject.SetActive(value: false);
		CameraController.CinemachineBrain.m_DefaultBlend = storedBlend;
		IntroCompleteRelay.Dispatch();
		SaveManager.PlayerSaveData.SpokenToFinalBoss = true;
		SaveManager.PlayerSaveData.SetHeirloomLevel(HeirloomType.Fruit, 0, additive: false, broadcast: false);
		if (m_introSFXEventInstance_02.isValid())
		{
			AudioManager.Stop(m_introSFXEventInstance_02);
		}
		m_currentActiveEventInstance = default(EventInstance);
		RewiredMapController.SetIsInCutscene(inCutscene: false);
	}

	private IEnumerator MovePlayerToEndPos()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.SetVelocity(0f, 0f, additive: false);
		Vector3 storedStartingCamPos = m_storedStartingCamPos;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(storedStartingCamPos, checkXAxisOnly: true);
		playerController.SetFacing(faceRight: true);
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private void OnBossIntroStartSpeedUp()
	{
		if (m_currentActiveEventInstance.isValid())
		{
			m_currentActiveEventInstance.setPaused(paused: true);
		}
	}

	private void OnBossIntroStopSpeedUp()
	{
		if (m_currentActiveEventInstance.isValid())
		{
			float num = Time.time - m_introSFXStartTime;
			m_currentActiveEventInstance.setTimelinePosition((int)(num * 1000f));
			m_currentActiveEventInstance.setPaused(paused: false);
		}
	}

	public void SmashWindows()
	{
		PropSpawnController[] windowsToSmashArray = m_windowsToSmashArray;
		foreach (PropSpawnController propSpawnController in windowsToSmashArray)
		{
			if ((bool)propSpawnController.PropInstance)
			{
				propSpawnController.PropInstance.Animators[0].SetTrigger("Smash");
				propSpawnController.PropInstance.Animators[0].Play("Smash", 0, 0.8f);
			}
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_death_windowsShatter", CameraController.GameCamera.transform.position);
	}

	public void ColourShiftToDefault()
	{
		CameraController.LerpBGPostProcessToProfile(m_defaultBGProfile, 2f);
		CameraController.LerpFGPostProcessToProfile(m_defaultFGProfile, 2f);
		if (base.Boss.EnemyRank > EnemyRank.Basic)
		{
			if ((bool)m_flameEffect && m_flameEffect.IsPlaying)
			{
				m_flameEffect.Stop(EffectStopType.Gracefully);
			}
			m_flameEffect = null;
			int trigger = Animator.StringToHash("Retract");
			Animator[] rootsArray = m_rootsArray;
			for (int i = 0; i < rootsArray.Length; i++)
			{
				rootsArray[i].SetTrigger(trigger);
			}
		}
	}

	protected override IEnumerator StartOutro()
	{
		yield return base.StartOutro();
		Tunnel earthShiftTunnel = m_earthShiftTunnelSpawner.Tunnel;
		earthShiftTunnel.SetIsVisible(isVisible: true);
		earthShiftTunnel.Interactable.ForceDisableInteractPrompt(forceDisable: false);
		if (base.Boss.transform.position.x > base.Room.Bounds.center.x)
		{
			float num = (earthShiftTunnel.transform.position.x - base.Room.Bounds.center.x) * 2f;
			Vector3 position = earthShiftTunnel.transform.position;
			position.x -= num;
			earthShiftTunnel.transform.position = position;
		}
		AudioManager.PlayOneShot(null, "event:/SFX/Enemies/Cain/sfx_cain_defeated_doorAppear", earthShiftTunnel.transform.position);
		SpriteRenderer tunnelRenderer = earthShiftTunnel.GetComponentInChildren<SpriteRenderer>(includeInactive: true);
		Color tunnelColour = tunnelRenderer.color;
		tunnelColour.a = 0f;
		tunnelRenderer.color = tunnelColour;
		float fadeDuration = 1f;
		float delay = Time.time + fadeDuration;
		while (Time.time < delay)
		{
			float a = 1f - (delay - Time.time) / fadeDuration;
			tunnelColour.a = a;
			tunnelRenderer.color = tunnelColour;
			yield return null;
		}
		tunnelColour.a = 1f;
		tunnelRenderer.color = tunnelColour;
		earthShiftTunnel.SetIsLocked(isLocked: false);
	}

	protected override void TeleportOut()
	{
	}

	protected override void SetBossFlagDefeated()
	{
		base.SetBossFlagDefeated();
		SaveManager.PlayerSaveData.EndingSpawnRoom = EndingSpawnRoomType.Hallway;
	}
}
