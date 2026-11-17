using System;
using System.Collections;
using FMOD.Studio;
using RLAudio;
using Rewired;
using UnityEngine;

public class EarthShiftTriggerController : MonoBehaviour
{
	private enum ShiftState
	{
		Inactive,
		Prep,
		Active
	}

	private const string EARTHSHIFT_PLATFORM_MOVE_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_loop";

	private const string EARTHSHIFT_PLATFORM_MOVE_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_loop";

	private const string EARTHSHIFT_PLATFORM_MOVE_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_loop";

	private const string EARTHSHIFT_PLATFORM_STOP_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_stop";

	private const string EARTHSHIFT_PLATFORM_STOP_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_stop";

	private const string EARTHSHIFT_PLATFORM_STOP_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_stop";

	private const string EARTHSHIFT_PLATFORM_VANISH_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_reset_vanish";

	private const string EARTHSHIFT_PLATFORM_VANISH_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_reset_vanish";

	private const string EARTHSHIFT_PLATFORM_VANISH_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_reset_vanish";

	private const string EARTHSHIFT_PLATFORM_DEACTIVATE_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_small_deactivate";

	private const string EARTHSHIFT_PLATFORM_DEACTIVATE_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_med_deactivate";

	private const string EARTHSHIFT_PLATFORM_DEACTIVATE_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_large_deactivate";

	private const string EARTHSHIFT_PLATFORM_APPEAR_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_reset_appear";

	private const string EARTHSHIFT_PLATFORM_APPEAR_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_reset_appear";

	private const string EARTHSHIFT_PLATFORM_APPEAR_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_reset_appear";

	private const string EARTHSHIFT_STONE_CHARGE_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_small_charging_loop";

	private const string EARTHSHIFT_STONE_CHARGE_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_med_charging_loop";

	private const string EARTHSHIFT_STONE_CHARGE_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_large_charging_loop";

	private const string EARTHSHIFT_STONE_CHARGE_FINAL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_final_charging_loop";

	private const string EARTHSHIFT_STONE_PLAYER_RELEASE_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_small_player_release";

	private const string EARTHSHIFT_STONE_PLAYER_RELEASE_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_med_player_release";

	private const string EARTHSHIFT_STONE_PLAYER_RELEASE_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_large_player_release";

	private const string EARTHSHIFT_RESET_TELEGRAPH_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_reset_telegraph";

	private const string EARTHSHIFT_RESET_TELEGRAPH_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_reset_telegraph";

	private const string EARTHSHIFT_RESET_TELEGRAPH_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_reset_telegraph";

	private const string EARTHSHIFT_IDLE_PROXIMITY_LOOP_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_small_inactive_loop";

	private const string EARTHSHIFT_IDLE_PROXIMITY_LOOP_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_med_inactive_loop";

	private const string EARTHSHIFT_IDLE_PROXIMITY_LOOP_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_large_inactive_loop";

	private const string EARTHSHIFT_IDLE_PROXIMITY_LOOP_FINAL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_final_inactive_loop";

	private const string EARTHSHIFT_ACTIVE_LOOP_SMALL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_small_active_loop";

	private const string EARTHSHIFT_ACTIVE_LOOP_MEDIUM_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_med_active_loop";

	private const string EARTHSHIFT_ACTIVE_LOOP_LARGE_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_large_active_loop";

	private const string EARTHSHIFT_ACTIVE_LOOP_FINAL_SFX_NAME = "event:/SFX/Interactables/sfx_bindingStone_final_active_loop";

	private const string EARTHSHIFT_WATER_LOOP_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveThroughWater_fast_loop";

	private const string EARTHSHIFT_WATER_STOP_SFX_NAME = "event:/SFX/Interactables/sfx_stonePlatform_moveThroughWater_fast_stop";

	[SerializeField]
	private SpriteRenderer m_renderer;

	[SerializeField]
	private bool m_isLarge;

	[SerializeField]
	private float m_glowDistance = 4f;

	[SerializeField]
	private Color m_flickerColor = new Color(0.6f, 0.6f, 0.6f);

	private Interactable m_interactable;

	private Animator m_animator;

	private Coroutine m_prepCoroutine;

	private WaitRL_Yield m_prepWaitYield;

	private Coroutine m_blinkingCoroutine;

	private WaitRL_Yield m_blinkWaitYield;

	private Coroutine m_resetPositionCoroutine;

	private bool m_hasEarthShiftHeirloom;

	private ShiftState m_state;

	private EarthShiftPlatformController m_platform;

	private float m_initialDelayOverride = -1f;

	private float m_resetTimer = -1f;

	private MaterialPropertyBlock m_matPropBlock;

	private bool m_playingIdleProximityLoop;

	private Action<MonoBehaviour, EventArgs> m_onPlayerEnterRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHit;

	private Action<InputActionEventData> m_onInteractReleased;

	private EventInstance m_movePlatformSFXInstance;

	private EventInstance m_chargeSFXInstance;

	private EventInstance m_idleProximitySFXEvent;

	private EventInstance m_activeLoopSFXInstance;

	private bool m_largeHasPlayed;

	private void Awake()
	{
		m_interactable = GetComponent<Interactable>();
		m_animator = GetComponent<Animator>();
		m_prepWaitYield = new WaitRL_Yield(0f);
		m_blinkWaitYield = new WaitRL_Yield(0f);
		m_matPropBlock = new MaterialPropertyBlock();
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_onPlayerHit = OnPlayerHit;
		m_onInteractReleased = OnInteractReleased;
	}

	private void OnEnable()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
	}

	private void OnPlayerEnterRoom(MonoBehaviour sender, EventArgs args)
	{
		m_largeHasPlayed = false;
		m_initialDelayOverride = GetComponent<Prop>().PropSpawnController.GetComponent<EarthShiftTrigger_InitialDelayOverrideHelper>().InitialDelayOverride;
		m_hasEarthShiftHeirloom = SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockEarthShift) > 0;
		LayerMask platformMask = PlayerManager.GetPlayerController().ControllerCorgi.PlatformMask;
		RaycastHit2D raycastHit2D = Physics2D.Raycast(base.transform.position, Vector2.down, 5f, platformMask);
		if ((bool)raycastHit2D)
		{
			base.transform.SetParent(raycastHit2D.transform);
		}
	}

	public void OnInteract()
	{
		m_state = ShiftState.Prep;
		if (m_isLarge)
		{
			m_largeHasPlayed = true;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.StopActiveAbilities(stopPersistentAbilities: true);
		playerController.SetVelocity(0f, 0f, additive: false);
		playerController.Animator.SetBool("Kneeling", value: true);
		m_animator.SetBool("SymbolLit", value: true);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHit);
		Rewired_RL.Player.AddInputEventDelegate(m_onInteractReleased, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Interact");
		m_prepCoroutine = StartCoroutine(PrepCoroutine());
	}

	private void StopInteraction()
	{
		ShiftState state = m_state;
		m_state = ShiftState.Inactive;
		if (m_prepCoroutine != null)
		{
			StopCoroutine(m_prepCoroutine);
			m_prepCoroutine = null;
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHit);
		Rewired_RL.Player.RemoveInputEventDelegate(m_onInteractReleased, UpdateLoopType.Update, InputActionEventType.ButtonJustReleased, "Interact");
		PlayerManager.GetPlayerController().Animator.SetBool("Kneeling", value: false);
		m_animator.SetBool("SymbolLit", value: false);
		m_animator.SetBool("Floating", value: false);
		if (m_chargeSFXInstance.isValid())
		{
			AudioManager.Stop(m_chargeSFXInstance);
		}
		if (m_activeLoopSFXInstance.isValid())
		{
			AudioManager.Stop(m_activeLoopSFXInstance);
		}
		if (m_movePlatformSFXInstance.isValid())
		{
			AudioManager.Stop(m_movePlatformSFXInstance);
		}
		if (!m_platform)
		{
			return;
		}
		string path = null;
		string path2 = null;
		switch (m_platform.PlatformSize)
		{
		case EarthShiftPlatformController.EarthShiftPlatformSize.Small:
			path = "event:/SFX/Interactables/sfx_bindingStone_small_deactivate";
			path2 = "event:/SFX/Interactables/sfx_bindingStone_small_player_release";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Medium:
			path = "event:/SFX/Interactables/sfx_bindingStone_med_deactivate";
			path2 = "event:/SFX/Interactables/sfx_bindingStone_med_player_release";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Large:
		case EarthShiftPlatformController.EarthShiftPlatformSize.Final:
			path = "event:/SFX/Interactables/sfx_bindingStone_large_deactivate";
			path2 = "event:/SFX/Interactables/sfx_bindingStone_large_player_release";
			break;
		}
		if (!m_isLarge)
		{
			if (state == ShiftState.Active)
			{
				AudioManager.PlayOneShot(null, path, m_platform.transform.position);
			}
			AudioManager.PlayOneShot(null, path2, base.transform.position);
		}
	}

	private void OnInteractReleased(InputActionEventData data)
	{
		if (!m_isLarge)
		{
			StopInteraction();
		}
	}

	private void OnPlayerHit(MonoBehaviour sender, EventArgs args)
	{
		StopInteraction();
	}

	private IEnumerator PrepCoroutine()
	{
		if (m_chargeSFXInstance.isValid())
		{
			AudioManager.PlayAttached(null, m_chargeSFXInstance, base.gameObject);
			m_chargeSFXInstance.setParameterByName("bindingStoneCharge", 0f);
		}
		float waitTime = ((m_initialDelayOverride == -1f) ? 1.25f : m_initialDelayOverride);
		if (m_chargeSFXInstance.isValid())
		{
			float waitTimeCounter = Time.time + waitTime;
			while (Time.time < waitTimeCounter)
			{
				yield return null;
				float value = Mathf.Clamp(1f - (waitTimeCounter - Time.time) / waitTime, 0f, 1f);
				m_chargeSFXInstance.setParameterByName("bindingStoneCharge", value);
			}
		}
		else
		{
			m_prepWaitYield.CreateNew(waitTime);
			yield return m_prepWaitYield;
		}
		if (m_chargeSFXInstance.isValid())
		{
			AudioManager.Stop(m_chargeSFXInstance);
		}
		m_state = ShiftState.Active;
		if (m_activeLoopSFXInstance.isValid())
		{
			AudioManager.PlayAttached(null, m_activeLoopSFXInstance, base.gameObject);
		}
		if (m_movePlatformSFXInstance.isValid())
		{
			AudioManager.PlayAttached(null, m_movePlatformSFXInstance, m_platform.gameObject);
		}
		if ((bool)m_animator)
		{
			m_animator.SetBool("Floating", value: true);
		}
		if (m_isLarge)
		{
			m_prepWaitYield.CreateNew(4f);
			yield return m_prepWaitYield;
			TunnelSpawnController tunnelSpawnController = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference<TunnelSpawnController>("AboveGroundTunnel");
			if ((bool)tunnelSpawnController && (bool)tunnelSpawnController.Tunnel)
			{
				tunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true);
			}
		}
		m_prepCoroutine = null;
	}

	private void Update()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		m_interactable.SetIsInteractableActive(m_hasEarthShiftHeirloom && m_platform.Progress < 1f && !m_animator.GetCurrentAnimatorStateInfo(0).IsTag("InteractingWith"));
		bool flag = false;
		if (!m_isLarge)
		{
			flag = m_hasEarthShiftHeirloom && m_platform.Progress < 1f && CDGHelper.DistanceBetweenPts(base.transform.position, playerController.Midpoint) <= m_glowDistance;
			m_animator.SetBool("PlayerNear", flag);
		}
		else if (playerController.IsGrounded)
		{
			flag = true;
			m_animator.SetBool("PlayerNear", flag);
		}
		if (!m_playingIdleProximityLoop && flag && m_state != ShiftState.Active)
		{
			if ((!m_isLarge || (m_isLarge && !m_largeHasPlayed)) && m_idleProximitySFXEvent.isValid())
			{
				AudioManager.Play(null, m_idleProximitySFXEvent, base.transform.position);
			}
			m_playingIdleProximityLoop = true;
		}
		else if (m_playingIdleProximityLoop && (!flag || m_state == ShiftState.Active))
		{
			if (m_idleProximitySFXEvent.isValid())
			{
				AudioManager.Stop(m_idleProximitySFXEvent);
			}
			m_playingIdleProximityLoop = false;
		}
		if (m_state != 0)
		{
			playerController.StopActiveAbilities(stopPersistentAbilities: true);
			playerController.SetVelocity(0f, 0f, additive: false);
			if (m_state == ShiftState.Active)
			{
				Vector3 vector = playerController.transform.position - base.transform.position;
				m_platform.Move();
				playerController.transform.position = base.transform.position + vector;
				if (m_platform.Progress >= 1f)
				{
					if (m_platform.MovesThroughWater)
					{
						AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_stonePlatform_moveThroughWater_fast_stop", m_platform.transform.position);
					}
					else if (!string.IsNullOrWhiteSpace(m_platform.SFXNameStopOverride))
					{
						AudioManager.PlayOneShot(null, m_platform.SFXNameStopOverride, m_platform.transform.position);
					}
					else
					{
						switch (m_platform.PlatformSize)
						{
						case EarthShiftPlatformController.EarthShiftPlatformSize.Small:
							AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_stop", m_platform.transform.position);
							break;
						case EarthShiftPlatformController.EarthShiftPlatformSize.Medium:
							AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_stonePlatform_moveMed_stop", m_platform.transform.position);
							break;
						case EarthShiftPlatformController.EarthShiftPlatformSize.Large:
							AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_stop", m_platform.transform.position);
							break;
						}
					}
					StopInteraction();
				}
			}
			if (m_platform.Progress > 0f)
			{
				m_resetTimer = 0f;
				StopBlinking();
			}
		}
		else if (m_resetTimer >= 0f)
		{
			m_resetTimer += Time.deltaTime;
			if (m_blinkingCoroutine == null && m_resetTimer >= 3f)
			{
				m_blinkingCoroutine = StartCoroutine(BlinkCoroutine());
			}
			if (m_resetTimer >= 5f)
			{
				m_resetPositionCoroutine = StartCoroutine(ResetPositionCoroutine());
			}
		}
	}

	private IEnumerator BlinkCoroutine()
	{
		string path = null;
		switch (m_platform.PlatformSize)
		{
		case EarthShiftPlatformController.EarthShiftPlatformSize.Small:
			path = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_reset_telegraph";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Medium:
			path = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_reset_telegraph";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Large:
		case EarthShiftPlatformController.EarthShiftPlatformSize.Final:
			path = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_reset_telegraph";
			break;
		}
		AudioManager.PlayOneShot(null, path, m_platform.transform.position);
		while (true)
		{
			m_renderer.color = m_flickerColor;
			m_platform.Renderer.GetPropertyBlock(m_matPropBlock);
			m_matPropBlock.SetColor(ShaderID_RL._MultiplyColor, m_flickerColor);
			m_platform.Renderer.SetPropertyBlock(m_matPropBlock);
			m_blinkWaitYield.CreateNew(0.1f);
			yield return m_blinkWaitYield;
			m_renderer.color = Color.white;
			m_platform.Renderer.GetPropertyBlock(m_matPropBlock);
			m_matPropBlock.SetColor(ShaderID_RL._MultiplyColor, Color.white);
			m_platform.Renderer.SetPropertyBlock(m_matPropBlock);
			m_blinkWaitYield.CreateNew(0.1f);
			yield return m_blinkWaitYield;
		}
	}

	private void StopBlinking()
	{
		if (m_blinkingCoroutine != null)
		{
			StopCoroutine(m_blinkingCoroutine);
			m_blinkingCoroutine = null;
			ResetColor();
		}
	}

	private IEnumerator ResetPositionCoroutine()
	{
		m_resetTimer = -1f;
		StopBlinking();
		float startTime2 = Time.time;
		float endTime2 = startTime2 + 0.25f;
		string path = null;
		string appearEventPath = null;
		switch (m_platform.PlatformSize)
		{
		case EarthShiftPlatformController.EarthShiftPlatformSize.Small:
			path = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_reset_vanish";
			appearEventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_reset_appear";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Medium:
			path = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_reset_vanish";
			appearEventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_reset_appear";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Large:
		case EarthShiftPlatformController.EarthShiftPlatformSize.Final:
			path = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_reset_vanish";
			appearEventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_reset_appear";
			break;
		}
		AudioManager.PlayOneShot(null, path, m_platform.transform.position);
		while (Time.time < endTime2)
		{
			float t = (Time.time - startTime2) / 0.25f;
			float num = Mathf.Lerp(1f, 0f, t);
			Color color = new Color(num, num, num);
			m_renderer.color = color;
			m_platform.Renderer.GetPropertyBlock(m_matPropBlock);
			m_matPropBlock.SetColor(ShaderID_RL._MultiplyColor, color);
			m_platform.Renderer.SetPropertyBlock(m_matPropBlock);
			yield return null;
		}
		m_renderer.color = Color.black;
		m_platform.Renderer.GetPropertyBlock(m_matPropBlock);
		m_matPropBlock.SetColor(ShaderID_RL._MultiplyColor, Color.black);
		m_platform.Renderer.SetPropertyBlock(m_matPropBlock);
		m_platform.ResetPosition();
		startTime2 = Time.time;
		endTime2 = startTime2 + 0.25f;
		while (Time.time < endTime2)
		{
			float t2 = (Time.time - startTime2) / 0.25f;
			float num2 = Mathf.Lerp(0f, 1f, t2);
			Color color2 = new Color(num2, num2, num2);
			m_renderer.color = color2;
			m_platform.Renderer.GetPropertyBlock(m_matPropBlock);
			m_matPropBlock.SetColor(ShaderID_RL._MultiplyColor, color2);
			m_platform.Renderer.SetPropertyBlock(m_matPropBlock);
			yield return null;
		}
		AudioManager.PlayOneShot(null, appearEventPath, m_platform.transform.position);
		m_resetPositionCoroutine = null;
	}

	public void SetPlatform(EarthShiftPlatformController platform)
	{
		m_platform = platform;
		string eventPath = null;
		string eventPath2 = null;
		string eventPath3 = null;
		string eventPath4 = null;
		switch (m_platform.PlatformSize)
		{
		case EarthShiftPlatformController.EarthShiftPlatformSize.Small:
			eventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveSmall_loop";
			eventPath2 = "event:/SFX/Interactables/sfx_bindingStone_small_charging_loop";
			eventPath3 = "event:/SFX/Interactables/sfx_bindingStone_small_inactive_loop";
			eventPath4 = "event:/SFX/Interactables/sfx_bindingStone_small_active_loop";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Medium:
			eventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveMed_loop";
			eventPath2 = "event:/SFX/Interactables/sfx_bindingStone_med_charging_loop";
			eventPath3 = "event:/SFX/Interactables/sfx_bindingStone_med_inactive_loop";
			eventPath4 = "event:/SFX/Interactables/sfx_bindingStone_med_active_loop";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Large:
			eventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_loop";
			eventPath2 = "event:/SFX/Interactables/sfx_bindingStone_large_charging_loop";
			eventPath3 = "event:/SFX/Interactables/sfx_bindingStone_large_inactive_loop";
			eventPath4 = "event:/SFX/Interactables/sfx_bindingStone_large_active_loop";
			break;
		case EarthShiftPlatformController.EarthShiftPlatformSize.Final:
			eventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveLarge_loop";
			eventPath2 = "event:/SFX/Interactables/sfx_bindingStone_final_charging_loop";
			eventPath3 = "event:/SFX/Interactables/sfx_bindingStone_final_inactive_loop";
			eventPath4 = "event:/SFX/Interactables/sfx_bindingStone_final_active_loop";
			break;
		}
		if (!string.IsNullOrWhiteSpace(platform.SFXNameLoopOverride))
		{
			eventPath4 = platform.SFXNameLoopOverride;
		}
		if (m_platform.MovesThroughWater)
		{
			eventPath = "event:/SFX/Interactables/sfx_stonePlatform_moveThroughWater_fast_loop";
		}
		if (!m_movePlatformSFXInstance.isValid())
		{
			m_movePlatformSFXInstance = AudioUtility.GetEventInstance(eventPath, base.transform);
		}
		if (!m_chargeSFXInstance.isValid())
		{
			m_chargeSFXInstance = AudioUtility.GetEventInstance(eventPath2, base.transform);
		}
		if (!m_idleProximitySFXEvent.isValid())
		{
			m_idleProximitySFXEvent = AudioUtility.GetEventInstance(eventPath3, base.transform);
		}
		if (!m_activeLoopSFXInstance.isValid())
		{
			m_activeLoopSFXInstance = AudioUtility.GetEventInstance(eventPath4, base.transform);
		}
	}

	private void ResetColor()
	{
		m_renderer.color = Color.white;
		m_platform.Renderer.GetPropertyBlock(m_matPropBlock);
		m_matPropBlock.SetColor(ShaderID_RL._MultiplyColor, Color.white);
		m_platform.Renderer.SetPropertyBlock(m_matPropBlock);
	}

	private void OnDisable()
	{
		m_state = ShiftState.Inactive;
		if (m_prepCoroutine != null)
		{
			StopCoroutine(m_prepCoroutine);
			m_prepCoroutine = null;
		}
		StopBlinking();
		ResetColor();
		if (m_resetPositionCoroutine != null)
		{
			StopCoroutine(m_resetPositionCoroutine);
			m_resetPositionCoroutine = null;
		}
		m_platform = null;
		m_resetTimer = -1f;
		m_hasEarthShiftHeirloom = false;
		m_playingIdleProximityLoop = false;
		if (!GameManager.IsApplicationClosing)
		{
			StopInteraction();
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_onPlayerEnterRoom);
		if (m_movePlatformSFXInstance.isValid())
		{
			AudioManager.Stop(m_movePlatformSFXInstance);
		}
		if (m_chargeSFXInstance.isValid())
		{
			AudioManager.Stop(m_chargeSFXInstance);
		}
		if (m_activeLoopSFXInstance.isValid())
		{
			AudioManager.Stop(m_activeLoopSFXInstance);
		}
		if (m_idleProximitySFXEvent.isValid())
		{
			AudioManager.Stop(m_idleProximitySFXEvent);
		}
	}

	private void OnDestroy()
	{
		if (m_movePlatformSFXInstance.isValid())
		{
			m_movePlatformSFXInstance.release();
		}
		if (m_chargeSFXInstance.isValid())
		{
			m_chargeSFXInstance.release();
		}
		if (m_activeLoopSFXInstance.isValid())
		{
			m_activeLoopSFXInstance.release();
		}
		if (m_idleProximitySFXEvent.isValid())
		{
			m_idleProximitySFXEvent.release();
		}
	}
}
