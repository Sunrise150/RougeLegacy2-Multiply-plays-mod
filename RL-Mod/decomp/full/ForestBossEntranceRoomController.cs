using System.Collections;
using UnityEngine;

public class ForestBossEntranceRoomController : BossEntranceRoomController
{
	[SerializeField]
	private Transform m_cameraMovementObj;

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs;

	private WaitRL_Yield m_waitYield;

	private bool m_openDoorCutsceneRunning;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		int num = 0;
		num += SaveManager.PlayerSaveData.GetRelic(RelicType.Lily1).Level;
		num += SaveManager.PlayerSaveData.GetRelic(RelicType.Lily2).Level;
		num += SaveManager.PlayerSaveData.GetRelic(RelicType.Lily3).Level;
		if (!base.IsRoomComplete)
		{
			if (SaveManager.PlayerSaveData.GetInsightState(InsightType.ForestBoss_DoorOpened) >= InsightState.ResolvedButNotViewed)
			{
				SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: true);
			}
			else if (num >= 2)
			{
				StartCoroutine(OpenDoorCoroutine());
			}
			else
			{
				SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
			}
		}
	}

	private IEnumerator OpenDoorCoroutine()
	{
		m_openDoorCutsceneRunning = true;
		GlobalTeleporterController.StayInCutsceneAfterTeleporting = true;
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		PlayerController playerController = PlayerManager.GetPlayerController();
		Vector3 localPosition = playerController.transform.localPosition;
		m_cameraMovementObj.position = localPosition;
		base.Room.CinemachineCamera.SetFollowTarget(m_cameraMovementObj);
		yield return TweenManager.TweenTo(m_cameraMovementObj, 2f, Ease.Quad.EaseInOut, "position.x", base.BossTunnelSpawner.transform.position.x, "position.y", base.BossTunnelSpawner.transform.position.y).TweenCoroutine;
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: false);
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		yield return TweenManager.TweenTo(m_cameraMovementObj, 2f, Ease.Quad.EaseInOut, "position.x", playerController.transform.localPosition.x, "position.y", playerController.transform.localPosition.y).TweenCoroutine;
		base.Room.CinemachineCamera.SetFollowTarget(playerController.FollowTargetGO.transform);
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.ForestBoss_DoorOpened) < InsightState.ResolvedButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.ForestBoss_DoorOpened, InsightState.ResolvedButNotViewed, forceOverride: false);
			m_insightEventArgs.Initialize(InsightType.ForestBoss_DoorOpened, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
		}
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		GlobalTeleporterController.StayInCutsceneAfterTeleporting = false;
		m_openDoorCutsceneRunning = false;
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (m_openDoorCutsceneRunning && !GameManager.IsApplicationClosing)
		{
			RewiredMapController.SetIsInCutscene(inCutscene: false);
			RewiredMapController.SetCurrentMapEnabled(enabled: true);
			GlobalTeleporterController.StayInCutsceneAfterTeleporting = false;
			m_openDoorCutsceneRunning = false;
		}
	}
}
