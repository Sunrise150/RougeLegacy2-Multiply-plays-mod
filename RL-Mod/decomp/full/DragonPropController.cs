using System;
using System.Collections;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class DragonPropController : BaseSpecialPropController, IDisplaySpeechBubble, IAudioEventEmitter
{
	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	private GameObject m_playerPetPositionObj;

	[SerializeField]
	private GameObject[] m_chainsGO;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioEvent;

	private NPCController m_npcController;

	private WaitRL_Yield m_waitYield;

	private InsightObjectiveCompleteHUDEventArgs m_insightArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);

	private float m_storedInteractIconXPos;

	private bool m_endingSpeechBubblePlayed;

	private bool m_speechBubbleDisabled;

	private Action m_endInteraction;

	public SpeechBubbleType BubbleType => SpeechBubbleType.Dialogue;

	public bool ShouldDisplaySpeechBubble => HasEventDialogue();

	public string Description => ToString();

	private bool HasEventDialogue()
	{
		if (CutsceneManager.IsCutsceneActive)
		{
			return false;
		}
		if (m_speechBubbleDisabled)
		{
			return false;
		}
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.SeenParade))
			{
				return !m_endingSpeechBubblePlayed;
			}
			return false;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_Intro))
		{
			return true;
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated) && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_BossDoorOpen))
		{
			return true;
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated) && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal))
		{
			return true;
		}
		if (NPCDialogueManager.CanSpeak(NPCType.Dragon))
		{
			return true;
		}
		return false;
	}

	protected override void Awake()
	{
		base.Awake();
		m_npcController = GetComponent<NPCController>();
		m_waitYield = new WaitRL_Yield(0f);
		m_interactable.UseParentScaleForInteractIcon = true;
		m_storedInteractIconXPos = m_interactable.InteractIconPositionGO.transform.localPosition.x;
		m_endInteraction = EndInteraction;
	}

	public void SetEndingCutsceneStateEnabled(bool enabled)
	{
		SetChainsEnabled(enabled: false);
		m_interactable.SetIsInteractableActive(!enabled);
		m_speechBubbleDisabled = enabled;
		m_interactable.SpeechBubble.SetSpeechBubbleEnabled(!enabled);
		if (enabled)
		{
			m_npcController.HideHeart();
		}
		else
		{
			m_npcController.UpdateHeartState();
		}
	}

	private void SetChainsEnabled(bool enabled)
	{
		GameObject[] chainsGO = m_chainsGO;
		foreach (GameObject gameObject in chainsGO)
		{
			if (gameObject.activeSelf != enabled)
			{
				gameObject.SetActive(enabled);
			}
		}
	}

	protected override void InitializePooledPropOnEnter()
	{
		m_endingSpeechBubblePlayed = false;
		m_interactable.SetIsInteractableActive(!CutsceneManager.IsCutsceneActive);
		m_interactable.InteractIconPositionGO.transform.SetLocalPositionX(m_storedInteractIconXPos);
		m_interactable.SpeechBubble.transform.SetLocalPositionX(m_storedInteractIconXPos);
		bool flag = SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated);
		EndingSpawnRoomTypeController component = base.Room.GetComponent<EndingSpawnRoomTypeController>();
		bool flag2 = (bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.AboveGround;
		if (!flag2)
		{
			SetChainsEnabled(enabled: true);
			m_npcController.Animator.SetBool("IsChained", !flag);
		}
		else
		{
			SetChainsEnabled(enabled: false);
			m_npcController.Animator.SetTrigger("LeftChainBreakInstant");
			m_npcController.Animator.SetTrigger("RightChainBreakInstant");
			m_npcController.Animator.SetTrigger("CollarBreakInstant");
		}
		if (flag && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_BossDoorOpen) && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated) && !flag2)
		{
			m_interactable.SetIsInteractableActive(isActive: false);
		}
	}

	public void TalkToDragon()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(TalkToDragonCoroutine());
	}

	private IEnumerator TalkToDragonCoroutine()
	{
		m_npcController.SetNPCState(NPCState.AtAttention);
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated) || !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal) || NPCDialogueManager.CanSpeak(NPCType.Dragon))
		{
			yield return MovePlayerToDragon();
			m_waitYield.CreateNew(0.25f);
			yield return m_waitYield;
		}
		yield return RunDialogue();
	}

	public IEnumerator RunDialogue()
	{
		bool flag = SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated);
		bool flag2 = SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated);
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			RunEndingDialogue();
			yield break;
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated))
		{
			if (flag && flag2 && !SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_Intro))
			{
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.DragonDialogue_BossDoorOpen, value: true);
				SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.DragonDialogue_Intro, value: true);
				RewiredMapController.SetIsInCutscene(inCutscene: true);
				PlayDialogueHelper(PlayerSaveFlag.None, "LOC_ID_DRAGON_NPC_SECRET_OPENING_WITHOUT_SPEAKING_1", endInteraction: false);
				while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
				{
					yield return null;
				}
				base.Room.GetComponent<CaveBossEntranceRoomController>().SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: false);
				m_waitYield.CreateNew(2f);
				yield return m_waitYield;
				SaveManager.PlayerSaveData.SetInsightState(InsightType.CaveBoss_DoorOpened, InsightState.ResolvedButNotViewed, forceOverride: false);
				m_insightArgs.Initialize(InsightType.CaveBoss_DoorOpened, discovered: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightArgs);
				EndInteraction();
				m_interactable.SetIsInteractableActive(isActive: false);
				RewiredMapController.SetIsInCutscene(inCutscene: false);
			}
			else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_Intro))
			{
				PlayDialogueHelper(PlayerSaveFlag.DragonDialogue_Intro, "LOC_ID_DRAGON_NPC_INTRODUCTION_1", endInteraction: false);
				if (SaveManager.PlayerSaveData.GetInsightState(InsightType.CaveBoss_DoorOpened) <= InsightState.Undiscovered)
				{
					while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
					{
						yield return null;
					}
					SaveManager.PlayerSaveData.SetInsightState(InsightType.CaveBoss_DoorOpened, InsightState.DiscoveredButNotViewed, forceOverride: false);
					m_insightArgs.Initialize(InsightType.CaveBoss_DoorOpened, discovered: true);
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightArgs);
				}
				EndInteraction();
			}
			else if (!flag && !flag2)
			{
				PlayDialogueHelper(PlayerSaveFlag.None, "LOC_ID_DRAGON_NPC_TWO_CHAINS_REMAINING_1", endInteraction: true);
			}
			else if (!flag || !flag2)
			{
				PlayDialogueHelper(PlayerSaveFlag.None, "LOC_ID_DRAGON_NPC_ONE_CHAIN_REMAINING_1", endInteraction: true);
			}
			else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_BossDoorOpen))
			{
				RewiredMapController.SetIsInCutscene(inCutscene: true);
				PlayDialogueHelper(PlayerSaveFlag.DragonDialogue_BossDoorOpen, "LOC_ID_DRAGON_NPC_OPENING_MOUTH_1", endInteraction: false);
				while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
				{
					yield return null;
				}
				base.Room.GetComponent<CaveBossEntranceRoomController>().SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: false);
				m_waitYield.CreateNew(2f);
				yield return m_waitYield;
				SaveManager.PlayerSaveData.SetInsightState(InsightType.CaveBoss_DoorOpened, InsightState.ResolvedButNotViewed, forceOverride: false);
				m_insightArgs.Initialize(InsightType.CaveBoss_DoorOpened, discovered: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightArgs);
				EndInteraction();
				m_interactable.SetIsInteractableActive(isActive: false);
				RewiredMapController.SetIsInCutscene(inCutscene: false);
			}
			yield break;
		}
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.DragonDialogue_BossDoorOpen, value: true);
		SaveManager.PlayerSaveData.SetFlag(PlayerSaveFlag.DragonDialogue_Intro, value: true);
		CaveBossEntranceRoomController bossEntranceRoom = base.Room.GetComponent<CaveBossEntranceRoomController>();
		if (bossEntranceRoom.BossTunnelState == BossTunnelState.Closed)
		{
			string openMouthStateName = "OpenMouth_Exit";
			while (m_npcController.Animator.GetCurrentAnimatorStateInfo(0).IsName(openMouthStateName))
			{
				yield return null;
			}
		}
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal))
		{
			PlayDialogueHelper(PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal, "LOC_ID_DRAGON_NPC_KILLING_TUBAL_1", endInteraction: false);
			while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
			{
				yield return null;
			}
			bossEntranceRoom.SetBossTunnelState(BossTunnelState.Destroyed, skipToIdleState: false);
			m_interactable.InteractIconPositionGO.transform.SetLocalPositionX(m_storedInteractIconXPos - 1f);
			m_interactable.SpeechBubble.transform.SetLocalPositionX(m_storedInteractIconXPos - 1f);
			m_waitYield.CreateNew(5f);
			yield return m_waitYield;
			EndInteraction();
		}
		else if (NPCDialogueManager.CanSpeak(NPCType.Dragon))
		{
			m_npcController.RunNextNPCDialogue();
			while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
			{
				yield return null;
			}
			yield return null;
			yield return null;
			bossEntranceRoom.SetBossTunnelState(BossTunnelState.Destroyed, skipToIdleState: false);
			m_interactable.InteractIconPositionGO.transform.SetLocalPositionX(m_storedInteractIconXPos - 1f);
			m_interactable.SpeechBubble.transform.SetLocalPositionX(m_storedInteractIconXPos - 1f);
			m_waitYield.CreateNew(5f);
			yield return m_waitYield;
			EndInteraction();
		}
		else if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_Sleep))
		{
			PlayDialogueHelper(PlayerSaveFlag.DragonDialogue_Sleep, null, "LOC_ID_DRAGON_NPC_ASLEEP_1", endInteraction: true);
		}
		else
		{
			StartCoroutine(PetDragon());
		}
	}

	private void PlayDialogueHelper(PlayerSaveFlag playerFlag, string dialogueLocID, bool endInteraction)
	{
		if (playerFlag != 0)
		{
			SaveManager.PlayerSaveData.SetFlag(playerFlag, value: true);
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), dialogueLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, DialogueWindowStyle.HorizontalLower);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		if (endInteraction)
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
		}
	}

	private void PlayDialogueHelper(PlayerSaveFlag playerFlag, string speakerLocID, string dialogueLocID, bool endInteraction)
	{
		if (playerFlag != 0)
		{
			SaveManager.PlayerSaveData.SetFlag(playerFlag, value: true);
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(speakerLocID, dialogueLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale, DialogueWindowStyle.HorizontalLower);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		if (endInteraction)
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
		}
	}

	private IEnumerator PetDragon()
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPetPositionObj.transform.position;
		if (playerController.transform.localScale.x < 1.4f)
		{
			position.x += 1f;
		}
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (base.transform.localScale.x > 0f)
		{
			playerController.SetFacing(faceRight: false);
		}
		else
		{
			playerController.SetFacing(faceRight: true);
		}
		int id = Animator.StringToHash("Pet");
		playerController.Animator.SetBool(id, value: true);
		m_waitYield.CreateNew(2.5f);
		yield return m_waitYield;
		PlayerMovementHelper.ResumeAllMovementInput();
		EndInteraction();
		StoreAPIManager.GiveAchievement(AchievementType.PetDragon, StoreType.All);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (!m_endingSpeechBubblePlayed)
		{
			m_endingSpeechBubblePlayed = true;
			if (!m_npcController.IsBestFriend)
			{
				textLocID = "LOC_ID_DRAGON_OUTRO_STRANGER_INTRO_1";
			}
			else
			{
				textLocID = "LOC_ID_DRAGON_OUTRO_FRIENDS_INTRO_1";
				StoreAPIManager.GiveAchievement(AchievementType.TitleDrop, StoreType.All);
			}
		}
		else if (!m_npcController.IsBestFriend)
		{
			textLocID = "LOC_ID_DRAGON_OUTRO_STRANGER_REPEAT_1";
		}
		else
		{
			textLocID = "LOC_ID_DRAGON_OUTRO_FRIENDS_REPEAT_1";
			StoreAPIManager.GiveAchievement(AchievementType.TitleDrop, StoreType.All);
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue(NPCDialogue_EV.GetNPCTitleLocID(m_npcController.NPCType), textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private IEnumerator MovePlayerToDragon()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (base.transform.localScale.x > 0f)
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
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		AudioManager.PlayOneShotAttached(this, m_farewellAudioEvent, base.gameObject);
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
	}
}
