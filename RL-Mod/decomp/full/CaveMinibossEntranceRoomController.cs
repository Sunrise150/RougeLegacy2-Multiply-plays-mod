using UnityEngine;

public class CaveMinibossEntranceRoomController : BaseSpecialRoomController
{
	[SerializeField]
	protected PlayerSaveFlag m_bossBeatenFlag;

	[SerializeField]
	private TunnelSpawnController m_bossTunnel;

	public bool IsWhiteDoor => m_bossBeatenFlag == PlayerSaveFlag.CaveMiniboss_White_Defeated;

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CachedHealthOverride = 0f;
		playerController.CachedManaOverride = 0f;
		(m_bossTunnel.Tunnel as CaveMinibossTunnel).IsWhiteTunnel = IsWhiteDoor;
		bool flag = (IsWhiteDoor && SaveManager.PlayerSaveData.GetRelic(RelicType.DragonKeyWhite).Level > 0) || (!IsWhiteDoor && SaveManager.PlayerSaveData.GetRelic(RelicType.DragonKeyBlack).Level > 0);
		bool flag2 = (IsWhiteDoor && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened)) || (!IsWhiteDoor && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened));
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
				return;
			}
			if (flag2)
			{
				m_bossTunnel.Tunnel.Animator.SetTrigger("GateOpenInstant");
			}
			else
			{
				m_bossTunnel.Tunnel.Animator.SetTrigger("Enable");
			}
			if (!flag && !flag2)
			{
				m_bossTunnel.Tunnel.SetIsLocked(isLocked: true);
			}
			else
			{
				m_bossTunnel.Tunnel.SetIsLocked(isLocked: false);
			}
		}
		else
		{
			m_bossTunnel.Tunnel.Animator.SetTrigger("DisableInstant");
			m_bossTunnel.Tunnel.SetIsLocked(isLocked: true);
		}
	}

	private void OnDisable()
	{
		BossEntranceRoomController.RunDoorCrumbleAnimation = false;
	}
}
