using UnityEngine;

public class BlackMinibossEntranceRoomController : BaseSpecialRoomController
{
	[SerializeField]
	protected PlayerSaveFlag m_chestOpenedFlag;

	[SerializeField]
	private TunnelSpawnController m_bossTunnel;

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CachedHealthOverride = 0f;
		playerController.CachedManaOverride = 0f;
		if (!base.IsRoomComplete)
		{
			if (SaveManager.PlayerSaveData.GetFlag(m_chestOpenedFlag))
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
	}
}
