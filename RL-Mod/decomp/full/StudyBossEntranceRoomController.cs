public class StudyBossEntranceRoomController : BossEntranceRoomController
{
	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (!base.IsRoomComplete)
		{
			bool flag = false;
			if ((SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyMiniboss_SpearKnight_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyMiniboss_SwordKnight_Defeated)) || flag)
			{
				SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: true);
			}
			else
			{
				SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
			}
		}
	}
}
