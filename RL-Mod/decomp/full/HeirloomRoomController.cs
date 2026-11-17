using System.Collections.Generic;
using UnityEngine;

public class HeirloomRoomController : BaseSpecialRoomController
{
	[SerializeField]
	private HeirloomType m_heirloomType;

	[SerializeField]
	private bool m_isFinalRoom;

	[SerializeField]
	private TunnelSpawnController m_heirloomTunnelSpawnController;

	[SerializeField]
	private bool m_faceRightAfterTeleport;

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs;

	private List<ParticleSystem> m_nonAllocParticleSystemList = new List<ParticleSystem>();

	public HeirloomType HeirloomType => m_heirloomType;

	public TunnelSpawnController HeirloomTunnelSpawnController => m_heirloomTunnelSpawnController;

	public bool FaceRightAfterTeleport => m_faceRightAfterTeleport;

	public byte HeirloomDialogueIndex { get; set; }

	public bool HeirloomLockedRepeatDialogue { get; set; }

	public bool IsFinalRoom => m_isFinalRoom;

	protected override void Awake()
	{
		base.Awake();
		m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (!IsFinalRoom)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.CachedHealthOverride = 0f;
			playerController.CachedManaOverride = 0f;
		}
		if (!IsFinalRoom)
		{
			SaveManager.PlayerSaveData.TemporaryHeirloomList.Clear();
		}
		if (!base.IsRoomComplete && SaveManager.PlayerSaveData.GetHeirloomLevel(HeirloomType) > 0 && !IsFinalRoom)
		{
			RoomCompleted();
		}
		if (!base.IsRoomComplete)
		{
			return;
		}
		ISpawnController[] spawnControllers = base.Room.SpawnControllerManager.SpawnControllers;
		foreach (ISpawnController spawnController in spawnControllers)
		{
			if (!spawnController.ShouldSpawn)
			{
				continue;
			}
			PropSpawnController propSpawnController = spawnController as PropSpawnController;
			if (propSpawnController == null || propSpawnController.PropInstance == null || !propSpawnController.PropInstance.name.StartsWith("Heirloom"))
			{
				continue;
			}
			propSpawnController.PropInstance.gameObject.GetComponentsInChildren(includeInactive: false, m_nonAllocParticleSystemList);
			foreach (ParticleSystem nonAllocParticleSystem in m_nonAllocParticleSystemList)
			{
				nonAllocParticleSystem.Stop(withChildren: true, ParticleSystemStopBehavior.StopEmittingAndClear);
			}
		}
	}

	public override void RoomCompleted()
	{
		base.RoomCompleted();
		RunInsightResolved();
		Debug.Log("You've been awarded the " + HeirloomType.ToString() + " heirloom!");
	}

	private void RunInsightResolved()
	{
		InsightType insightFromHeirloomType = InsightType_RL.GetInsightFromHeirloomType(HeirloomType);
		if (insightFromHeirloomType != 0)
		{
			if (SaveManager.PlayerSaveData.GetInsightState(insightFromHeirloomType) < InsightState.ResolvedButNotViewed)
			{
				m_insightEventArgs.Initialize(insightFromHeirloomType, discovered: false);
				SaveManager.PlayerSaveData.SetInsightState(insightFromHeirloomType, InsightState.ResolvedButNotViewed, forceOverride: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
			}
		}
		else
		{
			Debug.Log("<color=yellow>Could not run 'Insight Resolved' from Heirloom type: " + HeirloomType.ToString() + "</color>");
		}
	}
}
