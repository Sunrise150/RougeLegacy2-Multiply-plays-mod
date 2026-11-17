using UnityEngine;

public class CaveBossEntranceRoomController : BossEntranceRoomController
{
	[SerializeField]
	private PropSpawnController m_dragonPropSpawner;

	private InsightObjectiveCompleteHUDEventArgs m_insightArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (!CutsceneManager.IsCutsceneActive)
		{
			Animator component = m_dragonPropSpawner.PropInstance.GetComponent<Animator>();
			bool flag = SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated);
			bool flag2 = SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated);
			if (flag)
			{
				component.SetTrigger("LeftChainBreakInstant");
			}
			if (flag2)
			{
				component.SetTrigger("RightChainBreakInstant");
			}
			if (flag && flag2)
			{
				component.SetTrigger("CollarBreakInstant");
			}
			if (!base.IsRoomComplete)
			{
				if (flag && flag2 && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_BossDoorOpen))
				{
					SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: true);
				}
				else
				{
					SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
				}
			}
		}
		else
		{
			SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
		}
		m_bossTunnel.Tunnel.SetIsVisible(isVisible: false);
		m_bossUpTunnel.Tunnel.SetIsVisible(isVisible: false);
	}

	public override void SetBossTunnelState(BossTunnelState state, bool skipToIdleState)
	{
		Animator component = m_dragonPropSpawner.PropInstance.GetComponent<Animator>();
		if (state == BossTunnelState.Destroyed && (!SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal) || NPCDialogueManager.CanSpeak(NPCType.Dragon)))
		{
			state = BossTunnelState.Closed;
		}
		(base.BossTunnelSpawner.Tunnel as BossTunnel).TunnelState = state;
		component.SetBool("CloseMouth", value: false);
		component.SetBool("OpenMouth", value: false);
		component.SetBool("OpenMouthInstant", value: false);
		component.SetBool("Sleep", value: false);
		component.SetBool("SleepInstant", value: false);
		string text = null;
		switch (state)
		{
		case BossTunnelState.Closed:
			if (!skipToIdleState)
			{
				component.Update(0f);
				component.Update(0f);
				component.SetBool("OpenMouthInstant", value: true);
			}
			text = "CloseMouth";
			base.BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: true);
			base.BossTunnelSpawner.Tunnel.Interactable.SetIsInteractableActive(isActive: false);
			break;
		case BossTunnelState.PartlyOpen:
			m_doorPartlyOpenedRelay.Dispatch(skipToIdleState);
			text = "OpenMouth";
			base.BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: false);
			base.BossTunnelSpawner.Tunnel.Interactable.SetIsInteractableActive(isActive: true);
			break;
		case BossTunnelState.Destroyed:
			m_doorDestroyedRelay.Dispatch(skipToIdleState);
			text = "Sleep";
			base.BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: true);
			base.BossTunnelSpawner.Tunnel.Interactable.SetIsInteractableActive(isActive: false);
			break;
		}
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		if (skipToIdleState)
		{
			if (state != 0)
			{
				text += "Instant";
			}
			component.Update(0f);
			component.Update(0f);
			component.SetBool(text, value: true);
		}
		else
		{
			component.SetBool(text, value: true);
		}
	}
}
