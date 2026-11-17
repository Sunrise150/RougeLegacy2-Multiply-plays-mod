using System;
using System.Collections;
using System.Collections.Generic;
using RLAudio;
using RL_Windows;
using SceneManagement_RL;
using UnityEngine;

public class HeirloomStatuePropController : BaseSpecialPropController, IRoomConsumer
{
	private const string SFX_GREETING_NAME = "event:/SFX/Interactables/sfx_heirloom_greeting";

	private const string SFX_FAREWELL_NAME = "event:/SFX/Interactables/sfx_heirloom_farewell";

	[SerializeField]
	private GameObject m_playerPosObj;

	[SerializeField]
	private GameObject m_distortPosObj;

	[SerializeField]
	private GameObject m_heirloomParticlesGO;

	[SerializeField]
	private SpriteRenderer m_iconSprite;

	private WaitRL_Yield m_waitYield;

	private HeirloomWarp_Effect m_warpEffect;

	private float m_iconStartingY;

	private HeirloomRoomController m_heirloomRoom;

	private Vector3 m_storedIconScale;

	private bool m_disableIconHoverAnim;

	private List<ParticleSystem> m_nonAllocParticleSystemList = new List<ParticleSystem>();

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs;

	private float m_storedFallMultiplier;

	private Action m_cancelConfirmMenuSelection;

	private Action m_challengeHeirloom;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_warpEffect = CameraController.ForegroundPerspCam.GetComponent<HeirloomWarp_Effect>();
		m_storedIconScale = m_iconSprite.transform.localScale;
		m_iconStartingY = m_iconSprite.transform.localPosition.y;
		base.gameObject.GetComponentsInChildren(includeInactive: false, m_nonAllocParticleSystemList);
		m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);
		m_cancelConfirmMenuSelection = CancelConfirmMenuSelection;
		m_challengeHeirloom = ChallengeHeirloom;
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
		base.DisableProp(firstTimeDisabled);
		m_heirloomParticlesGO.SetActive(value: false);
		m_iconSprite.gameObject.SetActive(value: false);
	}

	protected override void InitializePooledPropOnEnter()
	{
		m_heirloomRoom = base.Room.gameObject.GetComponent<HeirloomRoomController>();
		if (m_heirloomRoom != null)
		{
			m_iconSprite.sprite = IconLibrary.GetHeirloomSprite(m_heirloomRoom.HeirloomType);
		}
		m_disableIconHoverAnim = false;
		m_iconSprite.transform.localScale = m_storedIconScale;
		m_heirloomParticlesGO.SetActive(value: true);
		m_iconSprite.gameObject.SetActive(value: true);
	}

	public void TriggerHeirloom()
	{
		if (!m_heirloomRoom.IsFinalRoom)
		{
			InitializeConfirmMenu();
			StartCoroutine(TriggerHeirloomCoroutine());
		}
		else
		{
			StartCoroutine(TriggerHeirloomCompleteCoroutine());
		}
	}

	private void InitializeConfirmMenu()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenu);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
		obj.SetTitleText("LOC_ID_HEIRLOOM_UI_CHALLENGE_TITLE_1", isLocID: true);
		obj.SetDescriptionText("LOC_ID_HEIRLOOM_UI_CHALLENGE_TEXT_1", isLocID: true);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenuSelection);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_challengeHeirloom);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenuSelection);
	}

	private IEnumerator TriggerHeirloomCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPosObj.transform.position;
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
		PlayerMovementHelper.ResumeAllMovementInput();
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		if ((bool)m_heirloomRoom)
		{
			if (Heirloom_EV.IsHeirloomLocked(m_heirloomRoom.HeirloomType))
			{
				HeirloomDialogueEntry dialogueEntry = Heirloom_EV.GetDialogueEntry(m_heirloomRoom.HeirloomType);
				if (dialogueEntry != null)
				{
					HeirloomTextEntry heirloomTextEntry = dialogueEntry.HeirloomDialogue[0];
					string lockedHeirloomLocID = Heirloom_EV.GetLockedHeirloomLocID(m_heirloomRoom.HeirloomType, m_heirloomRoom.HeirloomLockedRepeatDialogue);
					m_heirloomRoom.HeirloomLockedRepeatDialogue = true;
					DialogueManager.StartNewDialogue();
					DialogueManager.AddDialogue(heirloomTextEntry.DialogueTitleLocID, lockedHeirloomLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
					WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
					AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_heirloom_greeting", base.transform.position);
				}
			}
			else
			{
				HeirloomDialogueEntry dialogueEntry2 = Heirloom_EV.GetDialogueEntry(m_heirloomRoom.HeirloomType);
				if (dialogueEntry2 != null)
				{
					HeirloomTextEntry heirloomTextEntry2;
					if (m_heirloomRoom.HeirloomDialogueIndex == 0)
					{
						heirloomTextEntry2 = dialogueEntry2.HeirloomDialogue[m_heirloomRoom.HeirloomDialogueIndex];
						m_heirloomRoom.HeirloomDialogueIndex++;
					}
					else
					{
						if (dialogueEntry2.RandomizeRepeatDialogues)
						{
							m_heirloomRoom.HeirloomDialogueIndex = (byte)UnityEngine.Random.Range(1, dialogueEntry2.HeirloomDialogue.Length);
						}
						else
						{
							m_heirloomRoom.HeirloomDialogueIndex++;
						}
						m_heirloomRoom.HeirloomDialogueIndex = (byte)Mathf.Clamp(m_heirloomRoom.HeirloomDialogueIndex, 1, dialogueEntry2.HeirloomDialogue.Length - 1);
						heirloomTextEntry2 = dialogueEntry2.HeirloomDialogue[m_heirloomRoom.HeirloomDialogueIndex];
					}
					DialogueManager.StartNewDialogue();
					DialogueManager.AddDialogue(heirloomTextEntry2.DialogueTitleLocID, heirloomTextEntry2.DialogueTextLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
					WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
					AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_heirloom_greeting", base.transform.position);
				}
			}
		}
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		if (!Heirloom_EV.IsHeirloomLocked(m_heirloomRoom.HeirloomType))
		{
			RewiredMapController.SetCurrentMapEnabled(enabled: false);
			m_waitYield.CreateNew(0.25f);
			yield return m_waitYield;
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
		}
	}

	private void CancelConfirmMenuSelection()
	{
		RunInsightDiscovered();
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_heirloom_farewell", base.transform.position);
	}

	private void ChallengeHeirloom()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
		StartCoroutine(ChallengeHeirloomCoroutine());
	}

	private IEnumerator ChallengeHeirloomCoroutine()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_interactable.SetIsInteractableActive(isActive: false);
		m_storedFallMultiplier = playerController.FallMultiplierOverride;
		playerController.FallMultiplierOverride = 0.1f;
		Vector3 vector = CameraController.GameCamera.WorldToViewportPoint(m_distortPosObj.transform.position);
		m_warpEffect.WarpCenterX = vector.x;
		m_warpEffect.WarpCenterY = vector.y;
		m_warpEffect.DistortionAmount = 0f;
		m_warpEffect.enabled = true;
		TweenManager.TweenTo(m_warpEffect, 1.5f, Ease.Quad.EaseInOut, "DistortionAmount", 0.025f);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		yield return TweenManager.TweenBy(playerController.transform, 0.5f, Ease.Quad.EaseIn, "localPosition.x", -0.25f).TweenCoroutine;
		TweenManager.TweenBy(playerController.transform, 1f, Ease.None, "localPosition.x", -0.5f, "localPosition.y", 1);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		ScreenDistortion_SceneTransition obj = TransitionLibrary.GetTransitionInstance(TransitionID.ScreenDistortion) as ScreenDistortion_SceneTransition;
		obj.SetDistortInPosition(m_distortPosObj);
		obj.SetDistortOutPosition(playerController.gameObject);
		SceneLoader_RL.RunTransitionWithLogic(TeleportPlayer(), TransitionID.ScreenDistortion, cleanup: false);
	}

	private IEnumerator TeleportPlayer()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		TweenManager.StopAllTweensContaining(playerController.transform, runEndHandlers: false);
		playerController.FallMultiplierOverride = m_storedFallMultiplier;
		playerController.SetFacing((base.SpecialRoomController as HeirloomRoomController).FaceRightAfterTeleport);
		playerController.Animator.SetBool("Victory", value: false);
		if (!m_heirloomRoom.IsFinalRoom)
		{
			playerController.CachedHealthOverride = playerController.CurrentHealth;
			playerController.CachedManaOverride = playerController.CurrentMana;
			if (!TraitManager.IsTraitActive(TraitType.MegaHealth))
			{
				playerController.SetHealth(playerController.ActualMaxHealth, additive: false, runEvents: true);
				playerController.SetMana(playerController.ActualMaxMana, additive: false, runEvents: true);
			}
		}
		if (!m_heirloomRoom.IsFinalRoom)
		{
			ApplyHeirloomAbility();
		}
		if ((bool)m_heirloomRoom.HeirloomTunnelSpawnController.Tunnel)
		{
			m_heirloomRoom.HeirloomTunnelSpawnController.Tunnel.ForceEnterTunnel(animate: false);
		}
		else
		{
			Debug.Log("<color=red>Could not enter Heirloom. Tunnel not found on assigned tunnelSpawnController.</color>");
		}
		bool flag = false;
		if (!m_heirloomRoom.IsFinalRoom)
		{
			flag = RunInsightDiscovered();
		}
		float enterTime = Time.time;
		float delayDuration = 1.5f;
		if (flag)
		{
			delayDuration = 4f;
		}
		while (Time.time < enterTime + delayDuration)
		{
			yield return null;
		}
	}

	private void ApplyHeirloomAbility()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		SaveManager.PlayerSaveData.TemporaryHeirloomList.Add((base.SpecialRoomController as HeirloomRoomController).HeirloomType);
		playerController.InitializeAbilities();
	}

	private IEnumerator TriggerHeirloomCompleteCoroutine()
	{
		SaveManager.PlayerSaveData.SetHeirloomLevel(m_heirloomRoom.HeirloomType, 1, additive: false, broadcast: true);
		PlayerManager.GetPlayerController().InitializeAbilities();
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_interactable.SetIsInteractableActive(isActive: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPosObj.transform.position;
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
		PlayerMovementHelper.ResumeAllMovementInput();
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		HeirloomDialogueEntry dialogueEntry = Heirloom_EV.GetDialogueEntry(m_heirloomRoom.HeirloomType);
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue(dialogueEntry.HeirloomCompleteDialogue.DialogueTitleLocID, dialogueEntry.HeirloomCompleteDialogue.DialogueTextLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		AudioManager.PlayOneShot(null, "event:/SFX/Interactables/sfx_heirloom_greeting", base.transform.position);
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		playerController.Animator.SetBool("Victory", value: true);
		m_waitYield.CreateNew(0.75f);
		yield return m_waitYield;
		if (!WindowManager.GetIsWindowLoaded(WindowID.SpecialItemDrop))
		{
			WindowManager.LoadWindow(WindowID.SpecialItemDrop);
		}
		HeirloomDrop heirloomDrop = new HeirloomDrop(m_heirloomRoom.HeirloomType);
		(WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController).AddSpecialItemDrop(heirloomDrop);
		WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.SpecialItemDrop))
		{
			yield return null;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		ScreenDistortion_SceneTransition obj = TransitionLibrary.GetTransitionInstance(TransitionID.ScreenDistortion) as ScreenDistortion_SceneTransition;
		obj.SetDistortInPosition(m_distortPosObj);
		obj.SetDistortOutPosition(playerController.gameObject);
		SceneLoader_RL.RunTransitionWithLogic(TeleportPlayer(), TransitionID.ScreenDistortion, cleanup: false);
	}

	private bool RunInsightDiscovered()
	{
		InsightType insightFromHeirloomType = InsightType_RL.GetInsightFromHeirloomType(m_heirloomRoom.HeirloomType);
		if (insightFromHeirloomType != 0)
		{
			if (SaveManager.PlayerSaveData.GetInsightState(insightFromHeirloomType) < InsightState.DiscoveredButNotViewed)
			{
				m_insightEventArgs.Initialize(insightFromHeirloomType, discovered: true);
				SaveManager.PlayerSaveData.SetInsightState(insightFromHeirloomType, InsightState.DiscoveredButNotViewed, forceOverride: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
				return true;
			}
		}
		else
		{
			Debug.Log("<color=yellow>Could not 'Insight Discovered' from Heirloom type: " + m_heirloomRoom.HeirloomType.ToString() + "</color>");
		}
		return false;
	}

	protected void Update()
	{
		if (!m_disableIconHoverAnim)
		{
			Vector3 localPosition = m_iconSprite.transform.localPosition;
			localPosition.y = m_iconStartingY + Mathf.Sin(Time.time * 2f) * 0.1f;
			m_iconSprite.transform.localPosition = localPosition;
		}
	}
}
