using System;
using FMODUnity;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Serialization;

public class BossEntranceRoomController : BaseSpecialRoomController
{
	public static bool RunDoorCrumbleAnimation;

	protected static bool CutscenePlayed_DoNotDisableCrumble;

	[SerializeField]
	protected TunnelSpawnController m_bossTunnel;

	[SerializeField]
	[FormerlySerializedAs("m_firstTimeLockedFlagCheck")]
	protected PlayerSaveFlag m_bossBeatenFlag;

	[SerializeField]
	protected TunnelSpawnController m_bossUpTunnel;

	[SerializeField]
	private BurdenType m_bossUpBurden;

	[SerializeField]
	private PropSpawnController[] m_memoryProps;

	protected ProximityEventController m_proximityEventController;

	protected Relay<bool> m_doorPartlyOpenedRelay = new Relay<bool>();

	private Action m_displayNextMemory;

	protected Relay m_doorOpenedRelay = new Relay();

	protected Relay<bool> m_doorDestroyedRelay = new Relay<bool>();

	private int m_memoryIndex;

	public IRelayLink<bool> DoorPartlyOpenedRelay => m_doorPartlyOpenedRelay.link;

	public IRelayLink DoorOpenedRelay => m_doorOpenedRelay.link;

	public IRelayLink<bool> DoorDestroyedRelay => m_doorDestroyedRelay.link;

	public BossTunnelState BossTunnelState
	{
		get
		{
			if ((bool)BossTunnelSpawner && (bool)BossTunnelSpawner.Tunnel)
			{
				return (BossTunnelSpawner.Tunnel as BossTunnel).TunnelState;
			}
			return BossTunnelState.Closed;
		}
	}

	public TunnelSpawnController BossTunnelSpawner
	{
		get
		{
			if (BurdenManager.IsBurdenActive(m_bossUpBurden) && (bool)m_bossUpTunnel)
			{
				return m_bossUpTunnel;
			}
			return m_bossTunnel;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_displayNextMemory = DisplayNextMemory;
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (CutsceneManager.IsCutsceneActive && RunDoorCrumbleAnimation)
		{
			CutscenePlayed_DoNotDisableCrumble = true;
		}
		if ((bool)m_bossUpTunnel)
		{
			if (BurdenManager.IsBurdenActive(m_bossUpBurden))
			{
				m_bossTunnel.Tunnel.SetIsLocked(isLocked: true);
				m_bossTunnel.Tunnel.SetIsVisible(isVisible: false);
				m_bossTunnel.Tunnel.Interactable.SetIsInteractableActive(isActive: false);
				m_bossUpTunnel.Tunnel.SetIsLocked(isLocked: false);
				m_bossUpTunnel.Tunnel.SetIsVisible(isVisible: true);
				m_bossUpTunnel.Tunnel.Interactable.SetIsInteractableActive(isActive: true);
			}
			else
			{
				m_bossTunnel.Tunnel.SetIsLocked(isLocked: false);
				m_bossTunnel.Tunnel.SetIsVisible(isVisible: true);
				m_bossTunnel.Tunnel.Interactable.SetIsInteractableActive(isActive: true);
				m_bossUpTunnel.Tunnel.SetIsLocked(isLocked: true);
				m_bossUpTunnel.Tunnel.SetIsVisible(isVisible: false);
				m_bossUpTunnel.Tunnel.Interactable.SetIsInteractableActive(isActive: false);
			}
		}
		PropSpawnController[] memoryProps = m_memoryProps;
		foreach (PropSpawnController obj in memoryProps)
		{
			obj.PropInstance.Pivot.gameObject.SetActive(value: false);
			obj.PropInstance.GetComponent<StudioEventEmitter>().Stop();
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.CachedHealthOverride = 0f;
		playerController.CachedManaOverride = 0f;
		if (SaveManager.PlayerSaveData.GetFlag(m_bossBeatenFlag) && !RunDoorCrumbleAnimation)
		{
			ForceRoomCompleted();
		}
		if (!base.IsRoomComplete)
		{
			if (SaveManager.PlayerSaveData.GetFlag(m_bossBeatenFlag))
			{
				RoomCompleted();
				SetMemoriesEnabled();
			}
			else
			{
				SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: true);
			}
		}
		else
		{
			SetBossTunnelState(BossTunnelState.Destroyed, skipToIdleState: true);
			SetMemoriesEnabled();
		}
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.FinalDoorEntranceObjective) < InsightState.ResolvedButNotViewed && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CastleBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.BridgeBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.ForestBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.StudyBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.TowerBoss_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveBoss_Defeated))
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.FinalDoorEntranceObjective, InsightState.ResolvedButNotViewed, forceOverride: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, new InsightObjectiveCompleteHUDEventArgs(InsightType.FinalDoorEntranceObjective, discovered: false));
		}
	}

	private void SetMemoriesEnabled()
	{
		if (BurdenManager.IsBurdenActive(m_bossUpBurden) && !m_memoryProps.IsNativeNull() && m_memoryProps.Length != 0)
		{
			for (int i = 0; i <= m_memoryIndex; i++)
			{
				SetMemoryActive(i);
			}
		}
	}

	private void SetMemoryActive(int index)
	{
		Prop propInstance = m_memoryProps[index].PropInstance;
		propInstance.Pivot.gameObject.SetActive(value: true);
		propInstance.GetComponent<StudioEventEmitter>().Play();
		if (index == m_memoryIndex)
		{
			propInstance.GetComponent<JournalSpecialPropController>().FinishedReadingRelay.AddListener(m_displayNextMemory);
		}
	}

	private void DisplayNextMemory()
	{
		m_memoryProps[m_memoryIndex].PropInstance.GetComponent<JournalSpecialPropController>().FinishedReadingRelay.RemoveListener(m_displayNextMemory);
		if (m_memoryIndex < m_memoryProps.Length - 1)
		{
			m_memoryIndex++;
			SetMemoryActive(m_memoryIndex);
		}
	}

	public override void RoomCompleted()
	{
		base.RoomCompleted();
		RunDoorCrumbleAnimation = false;
		SetBossTunnelState(BossTunnelState.Destroyed, skipToIdleState: false);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayPlayerHUD, null, null);
		if ((bool)BossTunnelSpawner && (bool)BossTunnelSpawner.Tunnel)
		{
			if (!m_proximityEventController)
			{
				m_proximityEventController = BossTunnelSpawner.Tunnel.GameObject.GetComponent<ProximityEventController>();
			}
			if ((bool)m_proximityEventController)
			{
				m_proximityEventController.enabled = false;
			}
		}
	}

	public virtual void SetBossTunnelState(BossTunnelState state, bool skipToIdleState)
	{
		Animator animator = BossTunnelSpawner.Tunnel.Animator;
		(BossTunnelSpawner.Tunnel as BossTunnel).TunnelState = state;
		animator.SetBool("Closed", value: false);
		animator.SetBool("PartlyOpen", value: false);
		animator.SetBool("Open", value: false);
		animator.SetBool("Destroyed", value: false);
		string text = null;
		switch (state)
		{
		case BossTunnelState.Closed:
			text = "Closed";
			BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: true);
			BossTunnelSpawner.Tunnel.Interactable.SetIsInteractableActive(isActive: true);
			break;
		case BossTunnelState.PartlyOpen:
			m_doorPartlyOpenedRelay.Dispatch(skipToIdleState);
			text = "PartlyOpen";
			BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: false);
			break;
		case BossTunnelState.Open:
			m_doorOpenedRelay.Dispatch();
			text = "Open";
			BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: false);
			break;
		case BossTunnelState.Destroyed:
			m_doorDestroyedRelay.Dispatch(skipToIdleState);
			text = "Destroyed";
			BossTunnelSpawner.Tunnel.SetIsLocked(isLocked: true);
			break;
		}
		if (!string.IsNullOrEmpty(text))
		{
			animator.SetBool(text, value: true);
			if (skipToIdleState)
			{
				animator.Update(0f);
				animator.Update(0f);
				animator.Play(text + "Idle");
			}
			else
			{
				RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.5f, 0.5f);
			}
		}
	}

	protected virtual void OnDisable()
	{
		if (!CutscenePlayed_DoNotDisableCrumble)
		{
			RunDoorCrumbleAnimation = false;
		}
		CutscenePlayed_DoNotDisableCrumble = false;
	}
}
