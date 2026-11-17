using UnityEngine;

public class GoldenDoorsEntranceRoomController : BossEntranceRoomController
{
	private const bool UNLOCK_DOOR = true;

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (CutsceneManager.IsCutsceneActive && BossEntranceRoomController.RunDoorCrumbleAnimation)
		{
			BossEntranceRoomController.CutscenePlayed_DoNotDisableCrumble = true;
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CastleBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BridgeBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ForestBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TowerBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated) && !CutsceneManager.IsCutsceneActive)
		{
			SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: true);
		}
		else
		{
			SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
		}
	}

	public override void SetBossTunnelState(BossTunnelState state, bool skipToIdleState)
	{
		if (state == BossTunnelState.PartlyOpen)
		{
			state = BossTunnelState.Open;
		}
		Animator animator = base.BossTunnelSpawner.Tunnel.Animator;
		animator.ResetTrigger("UnderConstruction");
		animator.ResetTrigger("UnderConstructionInstant");
		_ = 2;
		base.SetBossTunnelState(state, skipToIdleState);
	}
}
