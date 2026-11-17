using UnityEngine;

public class StudyMinibossEntranceRoomController : BaseSpecialRoomController
{
	[SerializeField]
	protected PlayerSaveFlag m_bossBeatenFlag;

	[SerializeField]
	private TunnelSpawnController m_bossTunnel;

	private InsightObjectiveCompleteHUDEventArgs m_insightArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CachedHealthOverride = 0f;
		playerController.CachedManaOverride = 0f;
		if (SaveManager.PlayerSaveData.GetFlag(m_bossBeatenFlag) && !BossEntranceRoomController.RunDoorCrumbleAnimation)
		{
			ForceRoomCompleted();
		}
		if (!base.IsRoomComplete)
		{
			if (SaveManager.PlayerSaveData.GetFlag(m_bossBeatenFlag))
			{
				m_bossTunnel.Tunnel.Animator.SetTrigger("Disable");
				m_bossTunnel.Tunnel.SetIsLocked(isLocked: true);
				RoomCompleted();
			}
		}
		else
		{
			m_bossTunnel.Tunnel.Animator.Play("DisabledIdle");
			m_bossTunnel.Tunnel.SetIsLocked(isLocked: true);
		}
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.StudyBoss_DoorOpened) < InsightState.ResolvedButNotViewed && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyMiniboss_SpearKnight_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyMiniboss_SwordKnight_Defeated))
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.StudyBoss_DoorOpened, InsightState.ResolvedButNotViewed, forceOverride: false);
			m_insightArgs.Initialize(InsightType.StudyBoss_DoorOpened, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightArgs);
		}
	}

	private void OnDisable()
	{
		BossEntranceRoomController.RunDoorCrumbleAnimation = false;
	}
}
