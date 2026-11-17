using System;
using System.Collections;
using Cinemachine;
using Sigtrap.Relays;
using UnityEngine;

public class CastleBossEntranceRoomController : BossEntranceRoomController
{
	[SerializeField]
	private PropSpawnController m_torch1;

	[SerializeField]
	private PropSpawnController m_torch2;

	[SerializeField]
	private CinemachineVirtualCameraManager m_doorVcam;

	private TorchesRoomPropController m_leftTorchProp;

	private TorchesRoomPropController m_rightTorchProp;

	private Relay m_doorUnlockedRelay = new Relay();

	private Vector3 m_storedTorchPos1;

	private Vector3 m_storedTorchPos2;

	private WaitRL_Yield m_waitYield;

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs;

	private Action<TorchesRoomPropController> m_onTorchHit;

	public IRelayLink DoorUnlockedRelay => m_doorUnlockedRelay.link;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
		m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);
		m_onTorchHit = OnTorchHit;
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		m_leftTorchProp = m_torch1.PropInstance.gameObject.GetComponent<TorchesRoomPropController>();
		m_leftTorchProp.OnHitRelay.AddListener(m_onTorchHit);
		m_rightTorchProp = m_torch2.PropInstance.gameObject.GetComponent<TorchesRoomPropController>();
		m_rightTorchProp.OnHitRelay.AddListener(m_onTorchHit);
		if (TraitManager.IsTraitActive(TraitType.YouAreLarge))
		{
			m_leftTorchProp.transform.position += new Vector3(0f, 0f, 0f);
			m_rightTorchProp.transform.position += new Vector3(0f, 0f, 0f);
		}
		else if (TraitManager.IsTraitActive(TraitType.YouAreSmall))
		{
			m_leftTorchProp.transform.position += new Vector3(0f, 0f, 0f);
			m_rightTorchProp.transform.position += new Vector3(0f, 0f, 0f);
		}
		if (!base.IsRoomComplete)
		{
			if (SaveManager.PlayerSaveData.GetInsightState(InsightType.CastleBoss_DoorOpened) >= InsightState.ResolvedButNotViewed)
			{
				SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: true);
				m_leftTorchProp.OnHitRelay.RemoveListener(OnTorchHit);
				m_rightTorchProp.OnHitRelay.RemoveListener(OnTorchHit);
			}
			else
			{
				SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
			}
		}
	}

	protected override void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerExitRoom(sender, eventArgs);
		if ((bool)m_leftTorchProp)
		{
			m_leftTorchProp.OnHitRelay.RemoveListener(OnTorchHit);
		}
		if ((bool)m_rightTorchProp)
		{
			m_rightTorchProp.OnHitRelay.RemoveListener(OnTorchHit);
		}
	}

	private void OnTorchHit(TorchesRoomPropController torch)
	{
		if (!base.IsRoomComplete && (bool)m_leftTorchProp && (bool)m_rightTorchProp && m_leftTorchProp.IsFlameOn && m_rightTorchProp.IsFlameOn)
		{
			m_leftTorchProp.KeepFlameOn = true;
			m_rightTorchProp.KeepFlameOn = true;
			StartCoroutine(OpenDoorCoroutine());
		}
	}

	private IEnumerator OpenDoorCoroutine()
	{
		m_doorUnlockedRelay.Dispatch();
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		CinemachineVirtualCameraManager roomVcam = base.Room.CinemachineCamera;
		CinemachineBlendDefinition storedBlend = CameraController.CinemachineBrain.m_DefaultBlend;
		CinemachineBlendDefinition defaultBlend = default(CinemachineBlendDefinition);
		defaultBlend.m_Time = 1f;
		defaultBlend.m_Style = CinemachineBlendDefinition.Style.EaseInOut;
		CameraController.CinemachineBrain.m_DefaultBlend = defaultBlend;
		roomVcam.SetIsActiveCamera(isActiveCamera: false);
		m_doorVcam.gameObject.SetActive(value: true);
		m_doorVcam.SetIsActiveCamera(isActiveCamera: true);
		m_doorVcam.SetFollowTarget(base.BossTunnelSpawner.transform);
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: false);
		m_waitYield.CreateNew(1.5f);
		yield return m_waitYield;
		m_doorVcam.SetIsActiveCamera(isActiveCamera: false);
		m_doorVcam.gameObject.SetActive(value: false);
		roomVcam.SetIsActiveCamera(isActiveCamera: true);
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		CameraController.CinemachineBrain.m_DefaultBlend = storedBlend;
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.CastleBoss_DoorOpened) < InsightState.ResolvedButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.CastleBoss_DoorOpened, InsightState.ResolvedButNotViewed, forceOverride: false);
			m_insightEventArgs.Initialize(InsightType.CastleBoss_DoorOpened, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}
}
