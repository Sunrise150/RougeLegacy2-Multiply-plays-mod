using System.Collections;
using FMODUnity;
using RLAudio;
using RL_Windows;

public class TraitorBossRoomController : BossRoomController
{
	protected override void InitializeObjectiveCompleteArgs(float bossDefeatedDisplayDuration)
	{
		string @string = LocalizationManager.GetString("LOC_ID_BIG_TEXT_UI_TRAITOR_BOSS_DEFEATED_TITLE_1", isFemale: false);
		m_bossDefeatedArgs.Initialize(base.Boss.EnemyType, EnemyRank.Basic, bossDefeatedDisplayDuration, @string);
	}

	protected override IEnumerator StartIntro()
	{
		if (MusicManager.CurrentMusicInstance.isValid())
		{
			RuntimeManager.StudioSystem.setParameterByName("bossEncounterProgress_cain", 0.5f);
		}
		yield return base.StartIntro();
	}

	protected override IEnumerator StartOutro()
	{
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		bool awardRebelKey = false;
		int num = SaveManager.PlayerSaveData.TimesBeatenTraitor - 1;
		if (num != Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 2)
		{
			if (num > Ending_EV.GARDEN_POSTFIGHT_DIALOGUE_LOCIDS.Length - 2)
			{
				num = Ending_EV.GARDEN_POSTFIGHT_DIALOGUE_LOCIDS.Length - 2;
				awardRebelKey = true;
			}
		}
		else
		{
			num = Ending_EV.GARDEN_POSTFIGHT_DIALOGUE_LOCIDS.Length - 1;
			awardRebelKey = true;
		}
		string textLocID = Ending_EV.GARDEN_POSTFIGHT_DIALOGUE_LOCIDS[num];
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_NAME_J_REVEALED_1", textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		yield return null;
		while (WindowManager.GetIsWindowOpen(WindowID.Dialogue))
		{
			yield return null;
		}
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.Animator.SetBool("Victory", value: true);
		m_waitYield.CreateNew(0.75f);
		yield return m_waitYield;
		if (!WindowManager.GetIsWindowLoaded(WindowID.SpecialItemDrop))
		{
			WindowManager.LoadWindow(WindowID.SpecialItemDrop);
		}
		SpecialItemDropWindowController specialItemDropWindowController = WindowManager.GetWindowController(WindowID.SpecialItemDrop) as SpecialItemDropWindowController;
		HeirloomDrop heirloomDrop = new HeirloomDrop(HeirloomType.Fruit);
		specialItemDropWindowController.AddSpecialItemDrop(heirloomDrop);
		SaveManager.PlayerSaveData.SetHeirloomLevel(HeirloomType.Fruit, 1, additive: false, broadcast: true);
		if (awardRebelKey && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType.RebelKey) < 1)
		{
			HeirloomDrop heirloomDrop2 = new HeirloomDrop(HeirloomType.RebelKey);
			specialItemDropWindowController.AddSpecialItemDrop(heirloomDrop2);
			SaveManager.PlayerSaveData.SetHeirloomLevel(HeirloomType.RebelKey, 1, additive: false, broadcast: true);
		}
		WindowManager.SetWindowIsOpen(WindowID.SpecialItemDrop, isOpen: true);
		while (WindowManager.GetIsWindowOpen(WindowID.SpecialItemDrop))
		{
			yield return null;
		}
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		playerController.Animator.SetBool("Victory", value: false);
		m_waitYield.CreateNew(0.25f);
		yield return m_waitYield;
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		if (awardRebelKey && SaveManager.PlayerSaveData.GetInsightState(InsightType.Ending_RebelsHidout) < InsightState.DiscoveredButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.Ending_RebelsHidout, InsightState.DiscoveredButNotViewed, forceOverride: false);
			InsightObjectiveCompleteHUDEventArgs eventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.Ending_RebelsHidout, discovered: true);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, null, eventArgs);
			m_waitYield.CreateNew(4f);
			yield return m_waitYield;
		}
		yield return base.StartOutro();
	}

	protected override void TeleportOut()
	{
		if ((bool)base.TunnelSpawnController && (bool)base.TunnelSpawnController.Tunnel)
		{
			base.TunnelSpawnController.Tunnel.ForceEnterTunnel(animate: true);
		}
	}
}
