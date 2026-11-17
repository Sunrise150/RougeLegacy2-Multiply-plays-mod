using System;
using System.Collections;
using FMODUnity;
using RLAudio;
using RL_Windows;
using UnityEngine;

public class JohanPropController : BaseSpecialPropController, IDisplaySpeechBubble, IAudioEventEmitter
{
	public enum Johan_SpawnCondition
	{
		None = 0,
		DiedAtLeastOnce = 10,
		TowerBossBeatenAndNotCollectedLantern = 20,
		MemoryHeirloomObtained = 30,
		TowerBossNotBeatenAndLanternInsightNotObtained = 40,
		CastleBossDefeated = 1000,
		BridgeBossDefeated = 1010,
		ForestBossDefeated = 1020,
		StudyBossDefeated = 1030,
		TowerBossDefeated = 1040,
		CaveBossDefeated = 1050,
		GardenBossDefeated = 1055,
		FinalBossDefeated = 1060
	}

	[SerializeField]
	private GameObject m_playerPositionObj;

	[SerializeField]
	[EventRef]
	private string m_greetingAudioEvent;

	[SerializeField]
	[EventRef]
	private string m_farewellAudioEvent;

	private Prop m_prop;

	private NPCController m_npcController;

	private bool m_canGiveLantern;

	private bool m_endingSpeechBubblePlayed;

	private bool m_speechBubbleDisabled;

	private Action m_endInteraction;

	private Action m_teleportToTraitorFight;

	public string Description => ToString();

	public bool ShouldDisplaySpeechBubble
	{
		get
		{
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
			if (base.Room.BiomeType == BiomeType.Garden)
			{
				return !m_endingSpeechBubblePlayed;
			}
			if (m_canGiveLantern)
			{
				return true;
			}
			DialogueDisplayOverride_Johan component = m_prop.PropSpawnController.GetComponent<DialogueDisplayOverride_Johan>();
			if ((bool)component)
			{
				bool flag = IsJohanSpawnConditionTrue(component.SpawnCondition);
				PlayerSaveFlag spokenFlagOverride = component.SpokenFlagOverride;
				if (spokenFlagOverride == PlayerSaveFlag.None || !SaveManager.PlayerSaveData.GetFlag(spokenFlagOverride))
				{
					if (!flag || component.SpawnIfFalse)
					{
						if (!flag)
						{
							return component.SpawnIfFalse;
						}
						return false;
					}
					return true;
				}
				return false;
			}
			return false;
		}
	}

	public SpeechBubbleType BubbleType
	{
		get
		{
			if (m_canGiveLantern)
			{
				return SpeechBubbleType.PointOfInterest;
			}
			return SpeechBubbleType.Dialogue;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_prop = GetComponent<Prop>();
		m_npcController = GetComponent<NPCController>();
		m_endInteraction = EndInteraction;
		m_teleportToTraitorFight = TeleportToTraitorFight;
	}

	public void SetEndingCutsceneStateEnabled(bool enabled)
	{
		m_interactable.SetIsInteractableActive(!enabled);
		m_speechBubbleDisabled = enabled;
		m_interactable.SpeechBubble.SetSpeechBubbleEnabled(!enabled);
		if (enabled)
		{
			m_npcController.HideHeart();
		}
		else
		{
			m_npcController.ShowHeart();
		}
	}

	protected override void InitializePooledPropOnEnter()
	{
		m_endingSpeechBubblePlayed = false;
		m_canGiveLantern = false;
		if (base.Room.BiomeType == BiomeType.Cave && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.CaveLantern) > 0)
		{
			RunLanternInsightResolved();
		}
		if (base.Room.BiomeType != BiomeType.Garden && BossID_RL.IsBossBeaten(BossID.Castle_Boss) && BossID_RL.IsBossBeaten(BossID.Bridge_Boss) && BossID_RL.IsBossBeaten(BossID.Forest_Boss) && BossID_RL.IsBossBeaten(BossID.Study_Boss) && BossID_RL.IsBossBeaten(BossID.Tower_Boss) && BossID_RL.IsBossBeaten(BossID.Cave_Boss) && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.CaveLantern) > 0)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		DialogueDisplayOverride_Johan component = m_prop.PropSpawnController.GetComponent<DialogueDisplayOverride_Johan>();
		if ((bool)component)
		{
			bool flag = IsJohanSpawnConditionTrue(component.SpawnCondition);
			PlayerSaveFlag spokenFlagOverride = component.SpokenFlagOverride;
			bool flag2 = (spokenFlagOverride == PlayerSaveFlag.None || !SaveManager.PlayerSaveData.GetFlag(spokenFlagOverride)) && ((flag && !component.SpawnIfFalse) || (!flag && component.SpawnIfFalse));
			if (component.SpawnCondition == Johan_SpawnCondition.TowerBossBeatenAndNotCollectedLantern && flag2)
			{
				m_canGiveLantern = true;
			}
			if (!flag2)
			{
				base.gameObject.SetActive(value: false);
				return;
			}
			PropSpawnController propSpawnController = base.Room.gameObject.FindObjectReference<PropSpawnController>("PizzaGirlSpawner");
			if ((bool)propSpawnController && (bool)propSpawnController.PropInstance)
			{
				propSpawnController.PropInstance.gameObject.SetActive(value: false);
			}
		}
		else
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public void TalkToJohan()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		StartCoroutine(TalkToJohanCoroutine());
	}

	private IEnumerator TalkToJohanCoroutine()
	{
		m_npcController.SetNPCState(NPCState.AtAttention);
		AudioManager.PlayOneShotAttached(this, m_greetingAudioEvent, base.gameObject);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		yield return MovePlayerToJohan();
		float delay = Time.time + 0.25f;
		while (Time.time < delay)
		{
			yield return null;
		}
		if (SaveManager.PlayerSaveData.EndingSpawnRoom == EndingSpawnRoomType.AboveGround)
		{
			RunEndingDialogue();
			yield break;
		}
		bool flag = base.Room.BiomeType == BiomeType.Garden;
		if (m_canGiveLantern)
		{
			yield return GiveHeirloomCoroutine();
		}
		else if (flag)
		{
			RunGardenDialogue();
		}
		else
		{
			RunDialogue();
		}
	}

	private IEnumerator GiveHeirloomCoroutine()
	{
		m_canGiveLantern = false;
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		RunDialogue();
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		SaveManager.PlayerSaveData.SetHeirloomLevel(HeirloomType.CaveLantern, 1, additive: false, broadcast: true);
		float delay4 = Time.time + 0.25f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.Animator.SetBool("Victory", value: true);
		delay4 = Time.time + 0.75f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		if (!WindowManager.GetIsWindowLoaded(WindowID.SpecialItemDrop))
		{
			WindowManager.LoadWindow(WindowID.SpecialItemDrop);
		}
		HeirloomDrop heirloomDrop = new HeirloomDrop(HeirloomType.CaveLantern);
		(WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController).AddSpecialItemDrop(heirloomDrop);
		WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.SpecialItemDrop))
		{
			yield return null;
		}
		delay4 = Time.time + 0.25f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		playerController.Animator.SetBool("Victory", value: false);
		delay4 = Time.time + 0.25f;
		while (Time.time < delay4)
		{
			yield return null;
		}
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		RunLanternInsightResolved();
	}

	private void RunDialogue()
	{
		DialogueDisplayOverride component = m_prop.PropSpawnController.gameObject.GetComponent<DialogueDisplayOverride>();
		if ((bool)component)
		{
			string text = (string.IsNullOrEmpty(component.SpeakerOverride) ? NPCDialogue_EV.GetNPCTitleLocID(NPCType.Johan) : component.SpeakerOverride);
			PlayerSaveFlag spokenFlagOverride = component.SpokenFlagOverride;
			string text2;
			if ((spokenFlagOverride == PlayerSaveFlag.None || !SaveManager.PlayerSaveData.GetFlag(component.SpokenFlagOverride)) && !string.IsNullOrEmpty(component.DialogueOverride))
			{
				if (spokenFlagOverride != 0)
				{
					SaveManager.PlayerSaveData.SetFlag(component.SpokenFlagOverride, value: true);
				}
				text2 = component.DialogueOverride;
			}
			else
			{
				text2 = component.RepeatedDialogueOverride;
			}
			DialogueManager.StartNewDialogue(m_npcController);
			if (component.UseLocIDOverride)
			{
				DialogueManager.AddDialogue(text, text2, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			else
			{
				DialogueManager.AddNonLocDialogue(LocalizationManager.GetString(text, isFemale: false), text2, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			}
			DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		}
		else
		{
			EndInteraction();
		}
	}

	private void RunGardenDialogue()
	{
		string text = null;
		bool flag = false;
		if (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.GardenBoss_Defeated))
		{
			if (!SaveManager.PlayerSaveData.SpokenToTraitor)
			{
				SaveManager.PlayerSaveData.SpokenToTraitor = true;
				int num = Mathf.Clamp(SaveManager.PlayerSaveData.TimesBeatenTraitor, 0, Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 1);
				text = Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS[num];
			}
			else
			{
				text = Ending_EV.GARDEN_PREFIGHT_REPEAT_DIALOGUE_LOCIDS[SaveManager.PlayerSaveData.TraitorPreFightRepeatDialogueIndex];
				SaveManager.PlayerSaveData.TraitorPreFightRepeatDialogueIndex++;
				if (SaveManager.PlayerSaveData.TraitorPreFightRepeatDialogueIndex >= Ending_EV.GARDEN_PREFIGHT_REPEAT_DIALOGUE_LOCIDS.Length)
				{
					SaveManager.PlayerSaveData.TraitorPreFightRepeatDialogueIndex = 0;
				}
			}
			flag = true;
		}
		else
		{
			m_endingSpeechBubblePlayed = true;
			text = Ending_EV.GARDEN_POSTFIGHT_REPEAT_DIALOGUE_LOCIDS[SaveManager.PlayerSaveData.TraitorPostFightRepeatDialogueIndex];
			SaveManager.PlayerSaveData.TraitorPostFightRepeatDialogueIndex++;
			if (SaveManager.PlayerSaveData.TraitorPostFightRepeatDialogueIndex >= Ending_EV.GARDEN_POSTFIGHT_REPEAT_DIALOGUE_LOCIDS.Length)
			{
				SaveManager.PlayerSaveData.TraitorPostFightRepeatDialogueIndex = 0;
			}
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_J_REVEALED_1", text, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		if (flag)
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_teleportToTraitorFight);
		}
		else
		{
			DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
	}

	private void RunEndingDialogue()
	{
		string textLocID;
		if (!m_endingSpeechBubblePlayed)
		{
			m_endingSpeechBubblePlayed = true;
			textLocID = "LOC_ID_J_OUTRO_FRIENDS_INTRO_1";
		}
		else
		{
			textLocID = "LOC_ID_J_OUTRO_FRIENDS_REPEAT_1";
		}
		DialogueManager.StartNewDialogue(m_npcController);
		DialogueManager.AddDialogue("LOC_ID_NAME_J_REVEALED_1", textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		DialogueManager.AddDialogueCompleteEndHandler(m_endInteraction);
	}

	private void EndInteraction()
	{
		m_interactable.SetIsInteractableActive(isActive: true);
		AudioManager.PlayOneShotAttached(this, m_farewellAudioEvent, base.gameObject);
	}

	private void TeleportToTraitorFight()
	{
		TunnelSpawnController tunnelSpawnController = ((!BurdenManager.IsBurdenActive(BurdenType.FinalBossUp)) ? base.Room.gameObject.FindObjectReference<TunnelSpawnController>("FightTunnel") : base.Room.gameObject.FindObjectReference<TunnelSpawnController>("FightUpTunnel"));
		if ((bool)tunnelSpawnController && (bool)tunnelSpawnController.Tunnel)
		{
			tunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true);
			return;
		}
		Debug.Log("Could not teleport to traitor fight. Tunnel Spawner (FightTunnel) not found.");
		EndInteraction();
	}

	private IEnumerator MovePlayerToJohan()
	{
		PlayerManager.GetPlayerController().SetVelocity(0f, 0f, additive: false);
		Vector3 position = m_playerPositionObj.transform.position;
		PlayerMovementHelper.StopAllMovementInput();
		yield return PlayerMovementHelper.MoveTo(position, checkXAxisOnly: true);
		if (m_prop.transform.localScale.x > 0f)
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: false);
		}
		else
		{
			PlayerManager.GetPlayerController().SetFacing(faceRight: true);
		}
		PlayerMovementHelper.ResumeAllMovementInput();
	}

	private void RunLanternInsightResolved()
	{
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.HeirloomLantern) < InsightState.ResolvedButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.HeirloomLantern, InsightState.ResolvedButNotViewed, forceOverride: false);
			InsightObjectiveCompleteHUDEventArgs eventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.HeirloomLantern, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, null, eventArgs);
		}
	}

	protected override void DisableProp(bool firstTimeDisabled)
	{
	}

	public bool IsJohanSpawnConditionTrue(Johan_SpawnCondition spawnCondition)
	{
		switch (spawnCondition)
		{
		case Johan_SpawnCondition.DiedAtLeastOnce:
			return SaveManager.PlayerSaveData.TimesDied > 0;
		case Johan_SpawnCondition.MemoryHeirloomObtained:
			return SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.UnlockMemory) > 0;
		case Johan_SpawnCondition.TowerBossNotBeatenAndLanternInsightNotObtained:
			if (!BossID_RL.IsBossBeaten(BossID.Tower_Boss) && SaveManager.PlayerSaveData.GetInsightState(InsightType.HeirloomLantern) <= InsightState.Undiscovered)
			{
				return SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.CaveLantern) < 1;
			}
			return false;
		case Johan_SpawnCondition.TowerBossBeatenAndNotCollectedLantern:
			if (BossID_RL.IsBossBeaten(BossID.Tower_Boss))
			{
				return SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.CaveLantern) < 1;
			}
			return false;
		case Johan_SpawnCondition.CastleBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Castle_Boss);
		case Johan_SpawnCondition.BridgeBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Bridge_Boss);
		case Johan_SpawnCondition.ForestBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Forest_Boss);
		case Johan_SpawnCondition.StudyBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Study_Boss);
		case Johan_SpawnCondition.TowerBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Tower_Boss);
		case Johan_SpawnCondition.CaveBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Cave_Boss);
		case Johan_SpawnCondition.GardenBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Garden_Boss);
		case Johan_SpawnCondition.FinalBossDefeated:
			return BossID_RL.IsBossBeaten(BossID.Final_Boss);
		default:
			return true;
		}
	}
}
