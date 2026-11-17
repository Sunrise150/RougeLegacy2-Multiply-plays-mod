using System.Collections;
using System.Runtime.CompilerServices;
using RLWorldCreation;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.Events;

public class GlobalTeleporterController : MonoBehaviour, IRootObj
{
	public static GlobalTeleporterController ActiveTeleporter;

	[SerializeField]
	private ParticleSystem m_onActivateParticleSystem;

	[SerializeField]
	private GlobalTeleporterType m_teleporterType;

	[SerializeField]
	private GameObject m_distortPosition;

	[SerializeField]
	private UnityEvent m_playerInteractedWithEvent;

	[SerializeField]
	private UnityEvent m_teleportStart;

	[SerializeField]
	private UnityEvent m_playerJumpedOnPlatformEvent;

	[SerializeField]
	private UnityEvent m_dematerializeStartEvent;

	[SerializeField]
	private UnityEvent m_dematerializeCompleteEvent;

	[SerializeField]
	private UnityEvent m_rematerializeStartEvent;

	[SerializeField]
	private UnityEvent m_transitionStartEvent;

	[SerializeField]
	private UnityEvent m_transitionCompleteEvent;

	private Interactable m_interactable;

	private IHitboxController m_hitboxController;

	private WaitRL_Yield m_animWaitYield;

	private WaitUntil m_animWaitUntilLandedYield;

	private WaitUntil m_animWaitUntilTransitionYield;

	private HeirloomWarp_Effect m_warpEffect;

	public static bool StayInCutsceneAfterTeleporting { get; set; }

	public static bool IsTeleporting { get; private set; }

	public GlobalTeleporterType TeleporterType => m_teleporterType;

	public Room Room { get; set; }

	private void OnDisable()
	{
		if (m_interactable != null)
		{
			m_interactable.TriggerOnEnterEvent.RemoveListener(OnInteract);
		}
	}

	private void OnEnable()
	{
		if (m_hitboxController.IsInitialized)
		{
			m_hitboxController.GetCollider(HitboxType.Platform).enabled = false;
		}
		if (m_interactable != null)
		{
			m_interactable.TriggerOnEnterEvent.AddListener(OnInteract);
		}
	}

	private void Awake()
	{
		Room = GetComponentInParent<Room>();
		m_interactable = GetComponent<Interactable>();
		m_hitboxController = GetComponentInChildren<IHitboxController>();
		m_animWaitUntilLandedYield = new WaitUntil(() => PlayerManager.GetPlayerController().IsGrounded);
		m_animWaitUntilTransitionYield = new WaitUntil(() => !SceneLoader_RL.IsRunningTransitionWithLogic);
		m_animWaitYield = new WaitRL_Yield(0f);
	}

	private IEnumerator Start()
	{
		while (!m_hitboxController.IsInitialized)
		{
			yield return null;
		}
		m_hitboxController.GetCollider(HitboxType.Platform).enabled = false;
		m_warpEffect = CameraController.ForegroundPerspCam.GetComponent<HeirloomWarp_Effect>();
	}

	private void OnInteract(GameObject otherObj)
	{
		ActiveTeleporter = this;
		if (m_playerInteractedWithEvent != null)
		{
			m_playerInteractedWithEvent.Invoke();
		}
		WindowManager.SetWindowIsOpen(WindowID.Teleporter, isOpen: true);
	}

	public void OnEnterTeleportPlayer_V2(BiomeType biomeType, GridPointManager gridPointManager)
	{
		StartCoroutine(RunOnEnterTeleporterCoroutine_V2(biomeType, gridPointManager));
	}

	private IEnumerator RunOnEnterTeleporterCoroutine_V2(BiomeType biomeType, GridPointManager gridPointManager)
	{
		IsTeleporting = true;
		m_interactable.SetIsInteractableActive(isActive: false);
		if (m_teleportStart != null)
		{
			m_teleportStart.Invoke();
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.StopActiveAbilities(stopPersistentAbilities: true);
		playerController.CharacterHitResponse.SetInvincibleTime(9999f, additive: false, playInvincibility: false);
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_hitboxController.GetCollider(HitboxType.Platform).enabled = true;
		float storedJump = playerController.CharacterJump.JumpHeight;
		playerController.CharacterJump.JumpHeight = 8.75f;
		yield return PlayerMovementHelper.JumpPlayer(0f);
		playerController.CharacterJump.JumpHeight = storedJump;
		yield return PlayerMovementHelper.MoveTo(base.transform.position, checkXAxisOnly: true);
		m_animWaitYield.CreateNew(0.2f);
		yield return m_animWaitYield;
		yield return m_animWaitUntilLandedYield;
		if (m_playerJumpedOnPlatformEvent != null)
		{
			m_playerJumpedOnPlatformEvent.Invoke();
		}
		yield return m_animWaitYield;
		playerController.Animator.SetBool("Victory", value: true);
		Vector3 vector = CameraController.GameCamera.WorldToViewportPoint(playerController.Midpoint);
		m_warpEffect.WarpCenterX = vector.x;
		m_warpEffect.WarpCenterY = vector.y;
		m_warpEffect.DistortionAmount = 0f;
		m_warpEffect.enabled = true;
		yield return TweenManager.TweenTo(m_warpEffect, 0.5f, Ease.Quad.EaseIn, "DistortionAmount", 0.025f).TweenCoroutine;
		yield return TweenManager.TweenTo(m_warpEffect, 0.1f, Ease.None, "DistortionAmount", 0.1f).TweenCoroutine;
		EffectManager.PlayEffect(base.gameObject, null, "GlobalTeleporterPoof_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
		playerController.Visuals.gameObject.SetActive(value: false);
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.5f, 0.2f);
		if (m_dematerializeStartEvent != null)
		{
			m_dematerializeStartEvent.Invoke();
		}
		yield return TweenManager.TweenTo(m_warpEffect, 0.25f, Ease.Back.EaseOut, "DistortionAmount", 0).TweenCoroutine;
		m_warpEffect.enabled = false;
		m_animWaitYield.CreateNew(1f);
		yield return m_animWaitYield;
		if (m_dematerializeCompleteEvent != null)
		{
			m_dematerializeCompleteEvent.Invoke();
		}
		if (m_transitionStartEvent != null)
		{
			m_transitionStartEvent.Invoke();
		}
		if (Room.BiomeType != biomeType)
		{
			BiomeTransitionController.TransitionStartRelay.Dispatch(Room.BiomeType, biomeType);
		}
		SceneLoader_RL.RunTransitionWithLogic(TeleportPlayer_V2(biomeType, gridPointManager), TransitionID.FadeToBlackWithLoading, cleanup: true);
	}

	private IEnumerator TeleportPlayer_V2(BiomeType biomeType, GridPointManager gridPointManager)
	{
		yield return MapController.TeleportPlayerToRoom_Coroutine(biomeType, gridPointManager);
		PlayerManager.GetPlayerController().Visuals.gameObject.SetActive(value: true);
		m_interactable.SetIsInteractableActive(isActive: true);
	}

	public void OnExitTeleportPlayer()
	{
		ActiveTeleporter = null;
		StartCoroutine(RunOnExitTeleporterCoroutine());
	}

	private IEnumerator RunOnExitTeleporterCoroutine()
	{
		yield return null;
		yield return m_animWaitUntilTransitionYield;
		if (m_transitionCompleteEvent != null)
		{
			m_transitionCompleteEvent.Invoke();
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.Animator.SetBool("Victory", value: false);
		m_animWaitYield.CreateNew(0.2f);
		yield return m_animWaitYield;
		if (m_rematerializeStartEvent != null)
		{
			m_rematerializeStartEvent.Invoke();
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		playerController.CharacterHitResponse.StopInvincibleTime();
		IsTeleporting = false;
		if (!StayInCutsceneAfterTeleporting)
		{
			RewiredMapController.SetIsInCutscene(inCutscene: false);
		}
	}

	[SpecialName]
	GameObject IRootObj.get_gameObject()
	{
		return base.gameObject;
	}
}
