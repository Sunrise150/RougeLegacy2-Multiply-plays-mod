using System;
using System.Collections;
using MoreMountains.CorgiEngine;
using Rewired;
using UnityEngine;

public class Interactable : MonoBehaviour, ITerrainOnEnterHitResponse, IHitResponse, ITerrainOnStayHitResponse, ITerrainOnExitHitResponse, IInteractable
{
	private const string m_interactString_STATIC = "[Interact]";

	private static Interactable m_closestInteractable_STATIC;

	private static WaitForEndOfFrame m_endOfFrameYield_STATIC = new WaitForEndOfFrame();

	[SerializeField]
	private GameObject m_interactIconPositionGO;

	[SerializeField]
	private SpeechBubbleController m_speechBubble;

	[SerializeField]
	private bool m_triggerOnTouch;

	[SerializeField]
	private bool m_onlyTriggerWhenPlayerIsGrounded;

	[SerializeField]
	private bool m_canOnlyInteractOnce = true;

	[Tooltip("If false, will reset the trigger state of the interactable only once the player has exited the trigger box. If true, will reset even the player has not yet exited first. Useful for things like spring traps that always need to check for triggers in case the player is standing on it while it is disabled.")]
	[SerializeField]
	[ConditionalHide("m_canOnlyInteractOnce", true, Inverse = true)]
	private bool m_resetInteractionImmediately;

	[SerializeField]
	private bool m_playerMustBeFacing;

	[SerializeField]
	private bool m_disableAbilitiesOnInteract = true;

	[Space(10f)]
	public UnityEvent_GameObject m_triggerOnEnterEvent;

	public UnityEvent_GameObject m_triggerOnExitEvent;

	[Header("Debug")]
	[SerializeField]
	[ReadOnly]
	private bool m_isPlayerInTrigger;

	[SerializeField]
	[ReadOnly]
	private bool m_hasPlayerInteractedWith;

	[SerializeField]
	[ReadOnly]
	private bool m_isInteractableActive = true;

	private TextPopupObj m_pooledInteractIcon;

	private bool m_interactPromptDisplayed;

	private bool m_forceDisableInteractPrompt;

	private Action<InputActionEventData> m_onInteractButtonPressed;

	public static bool PlayerIsInteracting => m_closestInteractable_STATIC != null;

	public GameObject InteractIconPositionGO => m_interactIconPositionGO;

	public bool UseParentScaleForInteractIcon { get; set; }

	public SpeechBubbleController SpeechBubble
	{
		get
		{
			return m_speechBubble;
		}
		set
		{
			m_speechBubble = value;
		}
	}

	public bool IsInteractableActive => m_isInteractableActive;

	public PlayerController PlayerController
	{
		get
		{
			if (PlayerManager.IsInstantiated)
			{
				return PlayerManager.GetPlayerController();
			}
			return null;
		}
	}

	public bool IsPlayerGrounded
	{
		get
		{
			if ((bool)PlayerController)
			{
				return PlayerController.IsGrounded;
			}
			return false;
		}
	}

	public bool IsPlayerInTrigger
	{
		get
		{
			return m_isPlayerInTrigger;
		}
		private set
		{
			m_isPlayerInTrigger = value;
		}
	}

	public bool IsPlayerFacing
	{
		get
		{
			if (PlayerController != null)
			{
				if ((PlayerController.IsFacingRight && PlayerController.transform.position.x < base.gameObject.transform.position.x) || (!PlayerController.IsFacingRight && PlayerController.transform.position.x > base.gameObject.transform.position.x))
				{
					return true;
				}
				return false;
			}
			return false;
		}
	}

	public bool IsInteractable { get; private set; }

	public UnityEvent_GameObject TriggerOnEnterEvent => m_triggerOnEnterEvent;

	public UnityEvent_GameObject TriggerOnExitEvent => m_triggerOnExitEvent;

	public bool TriggerOnTouch => m_triggerOnTouch;

	public TextPopupObj InteractIcon => m_pooledInteractIcon;

	private void Awake()
	{
		m_onInteractButtonPressed = OnInteractButtonPressed;
	}

	private void Start()
	{
		IHitboxController componentInChildren = GetComponentInChildren<IHitboxController>();
		if (!componentInChildren.IsNativeNull())
		{
			componentInChildren.RepeatHitDuration = 0f;
		}
		if ((bool)m_speechBubble)
		{
			m_speechBubble.SetSpeechBubbleEnabled(enable: true);
		}
	}

	private void OnInteractButtonPressed(InputActionEventData obj)
	{
		if (!IsInteractable)
		{
			return;
		}
		if (!m_closestInteractable_STATIC)
		{
			m_closestInteractable_STATIC = this;
			StartCoroutine(StartInteractAtEndOfFrameCoroutine());
			return;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		float num = CDGHelper.DistanceBetweenPts(m_closestInteractable_STATIC.transform.position, playerController.transform.localPosition);
		if (CDGHelper.DistanceBetweenPts(base.transform.position, playerController.transform.localPosition) < num)
		{
			m_closestInteractable_STATIC = this;
		}
	}

	private void OnInteractBoundsTouched()
	{
		if (IsInteractable)
		{
			m_hasPlayerInteractedWith = true;
			m_triggerOnEnterEvent.Invoke(base.gameObject);
		}
	}

	public void TerrainOnEnterHitResponse(IHitboxController otherHBController)
	{
		if (base.enabled && m_isInteractableActive)
		{
			IsPlayerInTrigger = true;
			if (!m_triggerOnTouch)
			{
				Rewired_RL.Player.AddInputEventDelegate(m_onInteractButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Interact");
			}
			StopAllCoroutines();
			if (!m_canOnlyInteractOnce || (m_canOnlyInteractOnce && !m_hasPlayerInteractedWith))
			{
				StartCoroutine(OnPlayerInTriggerCoroutine());
			}
		}
	}

	public void TerrainOnStayHitResponse(IHitboxController otherHBController)
	{
		if (m_isInteractableActive && !IsPlayerInTrigger)
		{
			TerrainOnEnterHitResponse(otherHBController);
		}
	}

	private IEnumerator OnPlayerInTriggerCoroutine()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		while (IsPlayerInTrigger)
		{
			bool isInteractable = true;
			if ((bool)playerController && ((playerController.ConditionState != 0 && playerController.ConditionState != CharacterStates.CharacterConditions.DisableHorizontalMovement) || RewiredMapController.CurrentGameInputMode != GameInputMode.Game || !RewiredMapController.IsCurrentMapEnabled || GameManager.IsGamePaused))
			{
				isInteractable = false;
			}
			IsInteractable = isInteractable;
			if (!m_triggerOnTouch)
			{
				if (m_canOnlyInteractOnce)
				{
					if (m_hasPlayerInteractedWith)
					{
						IsInteractable = false;
						SetIsInteractableActive(isActive: false);
					}
				}
				else
				{
					if (m_playerMustBeFacing && !IsPlayerFacing)
					{
						IsInteractable = false;
					}
					if (m_onlyTriggerWhenPlayerIsGrounded && !IsPlayerGrounded)
					{
						IsInteractable = false;
					}
				}
			}
			else
			{
				if (m_onlyTriggerWhenPlayerIsGrounded && !IsPlayerGrounded)
				{
					IsInteractable = false;
				}
				if (!m_hasPlayerInteractedWith)
				{
					OnInteractBoundsTouched();
				}
				if (m_resetInteractionImmediately)
				{
					m_hasPlayerInteractedWith = false;
				}
			}
			if (m_interactPromptDisplayed && (!IsInteractable || !IsInteractableActive || m_forceDisableInteractPrompt))
			{
				SetInteractIconIsEnabled(isEnabled: false);
			}
			else if (!m_interactPromptDisplayed && IsInteractable && IsInteractableActive && !m_forceDisableInteractPrompt)
			{
				SetInteractIconIsEnabled(isEnabled: true);
			}
			yield return null;
		}
	}

	private void SetInteractIconIsEnabled(bool isEnabled)
	{
		if (m_interactPromptDisplayed == isEnabled)
		{
			return;
		}
		m_interactPromptDisplayed = isEnabled;
		if (!isEnabled)
		{
			if ((bool)m_pooledInteractIcon && m_pooledInteractIcon.isActiveAndEnabled)
			{
				m_pooledInteractIcon.gameObject.SetActive(value: false);
			}
			if ((bool)m_speechBubble)
			{
				if (IsInteractableActive && (!IsPlayerInTrigger || (IsPlayerInTrigger && m_onlyTriggerWhenPlayerIsGrounded && !IsPlayerGrounded)))
				{
					m_speechBubble.SetSpeechBubbleEnabled(enable: true);
				}
				else
				{
					m_speechBubble.SetSpeechBubbleEnabled(enable: false);
				}
			}
			return;
		}
		if ((bool)m_interactIconPositionGO)
		{
			Vector2 absPos = m_interactIconPositionGO.transform.position;
			m_pooledInteractIcon = TextPopupManager.DisplayTextAtAbsPos(TextPopupType.Interact, "[Interact]", absPos);
			if (UseParentScaleForInteractIcon)
			{
				m_pooledInteractIcon.StopAllCoroutines();
				Vector3 lossyScale = base.transform.lossyScale;
				if (lossyScale.x < 0f)
				{
					lossyScale.x *= -1f;
				}
				m_pooledInteractIcon.transform.localScale = lossyScale;
				m_pooledInteractIcon.Spawn();
			}
			CameraLayerController component = GetComponent<CameraLayerController>();
			if ((bool)component && (component.CameraLayer == CameraLayer.Foreground_PERSP || component.CameraLayer == CameraLayer.ForegroundLights))
			{
				m_pooledInteractIcon.transform.SetParent(component.Visuals.transform);
				m_pooledInteractIcon.gameObject.SetLayerRecursively(25);
				m_pooledInteractIcon.transform.position = new Vector3(absPos.x, absPos.y, component.Visuals.transform.position.z);
			}
		}
		if ((bool)m_speechBubble)
		{
			m_speechBubble.SetSpeechBubbleEnabled(enable: false);
		}
	}

	public void TerrainOnExitHitResponse(IHitboxController otherHBController)
	{
		if (base.gameObject.activeInHierarchy && base.enabled && m_isInteractableActive)
		{
			if (m_triggerOnExitEvent != null)
			{
				m_triggerOnExitEvent.Invoke(base.gameObject);
			}
			Disable();
		}
	}

	private IEnumerator StartInteractAtEndOfFrameCoroutine()
	{
		if ((bool)m_closestInteractable_STATIC)
		{
			yield return m_endOfFrameYield_STATIC;
			m_closestInteractable_STATIC.TriggerInteractable();
		}
		m_closestInteractable_STATIC = null;
	}

	public void TriggerInteractable()
	{
		if (IsInteractable)
		{
			if (m_disableAbilitiesOnInteract)
			{
				PlayerController playerController = PlayerManager.GetPlayerController();
				playerController.StopActiveAbilities(stopPersistentAbilities: true);
				playerController.SetVelocity(0f, 0f, additive: false);
			}
			m_hasPlayerInteractedWith = true;
			m_triggerOnEnterEvent.Invoke(base.gameObject);
		}
	}

	private void Disable()
	{
		IsPlayerInTrigger = false;
		if (!m_triggerOnTouch)
		{
			if (ReInput.isReady)
			{
				Rewired_RL.Player.RemoveInputEventDelegate(m_onInteractButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Interact");
			}
			SetInteractIconIsEnabled(isEnabled: false);
			StopAllCoroutines();
		}
		else if (!m_canOnlyInteractOnce)
		{
			m_hasPlayerInteractedWith = false;
		}
	}

	public void SetIsInteractableActive(bool isActive)
	{
		m_isInteractableActive = isActive;
		if (!m_isInteractableActive)
		{
			Disable();
		}
		else if ((bool)m_speechBubble)
		{
			m_speechBubble.SetSpeechBubbleEnabled(enable: true);
		}
	}

	public void ForceUpdateSpeechBubbleState()
	{
		if ((bool)m_speechBubble)
		{
			m_speechBubble.SetSpeechBubbleEnabled(enable: true);
		}
	}

	public void ForceDisableInteractPrompt(bool forceDisable)
	{
		m_forceDisableInteractPrompt = forceDisable;
	}

	public void ForceResetInteractedState()
	{
		m_hasPlayerInteractedWith = false;
	}

	private void OnDisable()
	{
		if (m_closestInteractable_STATIC == this)
		{
			m_closestInteractable_STATIC = null;
		}
		Disable();
	}
}
