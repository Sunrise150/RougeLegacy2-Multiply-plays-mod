using UnityEngine;

public class TowerBossEntranceRoomController : BossEntranceRoomController
{
	[SerializeField]
	private PropSpawnController m_sunPropSpawner;

	public override void SetBossTunnelState(BossTunnelState state, bool skipToIdleState)
	{
		Animator animator = base.BossTunnelSpawner.Tunnel.Animator;
		Animator animator2 = m_sunPropSpawner.PropInstance.Animators[0];
		(base.BossTunnelSpawner.Tunnel as BossTunnel).TunnelState = state;
		string text = null;
		string text2 = null;
		switch (state)
		{
		case BossTunnelState.Closed:
			if (!skipToIdleState)
			{
				text = "Disable";
				text2 = "TurnYellow";
			}
			else
			{
				text = "IdleDisabled";
				text2 = "IdleYellow";
			}
			base.BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: true);
			base.BossTunnelSpawner.Tunnel.Interactable.SetIsInteractableActive(isActive: true);
			break;
		case BossTunnelState.PartlyOpen:
			m_doorPartlyOpenedRelay.Dispatch(skipToIdleState);
			text = "IdleActive";
			text2 = "IdleRed";
			base.BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: false);
			break;
		case BossTunnelState.Open:
			m_doorOpenedRelay.Dispatch();
			text = "IdleActive";
			text2 = "TurnRedInstant";
			base.BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: false);
			break;
		case BossTunnelState.Destroyed:
			m_doorDestroyedRelay.Dispatch(skipToIdleState);
			if (!skipToIdleState)
			{
				text = "Disable";
				text2 = "TurnYellow";
			}
			else
			{
				text = "IdleDisabled";
				text2 = "IdleYellow";
			}
			base.BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: true);
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			animator.Play(text);
		}
		if (!string.IsNullOrEmpty(text2))
		{
			animator2.Play(text2);
		}
	}
}
