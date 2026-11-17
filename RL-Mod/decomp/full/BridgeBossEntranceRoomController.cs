using System.Collections;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class BridgeBossEntranceRoomController : BossEntranceRoomController, IAudioEventEmitter
{
	[SerializeField]
	private PropSpawnController m_gateSpawner;

	[SerializeField]
	private GameObject m_gateEndingPosGO;

	[SerializeField]
	private Transform m_cameraMovementObj;

	private Vector3 m_gateStartingPos;

	private WaitRL_Yield m_waitYield;

	private string m_description = string.Empty;

	private const string GATE_OPENING_SFX_AUDIO_PATH = "event:/SFX/Interactables/sfx_env_gate_openSequence";

	public string Description
	{
		get
		{
			if (m_description == string.Empty)
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f);
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		bool num = SaveManager.PlayerSaveData.GetFlag(m_bossBeatenFlag) && BossEntranceRoomController.RunDoorCrumbleAnimation;
		base.OnPlayerEnterRoom(sender, eventArgs);
		if (num)
		{
			StartCoroutine(GateOpenAnimCoroutine());
		}
		else if (base.IsRoomComplete)
		{
			Vector3 position = m_gateEndingPosGO.transform.position;
			position.z = m_gateSpawner.PropInstance.gameObject.transform.position.z;
			m_gateSpawner.PropInstance.gameObject.transform.position = position;
		}
	}

	private IEnumerator GateOpenAnimCoroutine()
	{
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		while (SceneLoader_RL.IsRunningTransitionWithLogic)
		{
			yield return null;
		}
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_waitYield.CreateNew(2f);
		yield return m_waitYield;
		float num = CameraController.GameCamera.orthographicSize * (float)Screen.width / (float)Screen.height;
		float num2 = CameraController.GameCamera.transform.localPosition.x + num;
		float x = base.Room.Bounds.max.x;
		float transitionAmount = x - num2;
		m_cameraMovementObj.transform.position = PlayerManager.GetPlayerController().transform.position;
		base.Room.CinemachineCamera.SetFollowTarget(m_cameraMovementObj);
		yield return TweenManager.TweenBy(m_cameraMovementObj, 1f, Ease.Quad.EaseInOut, "position.x", transitionAmount).TweenCoroutine;
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		EffectManager.PlayEffect(base.gameObject, null, "CameraShakeSmall_Effect", Vector3.zero, 3f, EffectStopType.Gracefully);
		AudioManager.PlayOneShotAttached(this, "event:/SFX/Interactables/sfx_env_gate_openSequence", m_gateSpawner.PropInstance.gameObject);
		yield return TweenManager.TweenTo(m_gateSpawner.PropInstance.gameObject.transform, 3f, Ease.Quad.EaseOut, "position.x", m_gateEndingPosGO.transform.position.x, "position.y", m_gateEndingPosGO.transform.position.y).TweenCoroutine;
		m_waitYield.CreateNew(0.5f);
		yield return m_waitYield;
		yield return TweenManager.TweenBy(m_cameraMovementObj, 1f, Ease.Quad.EaseInOut, "position.x", 0f - transitionAmount).TweenCoroutine;
		base.Room.CinemachineCamera.SetFollowTarget(PlayerManager.GetPlayerController().FollowTargetGO.transform);
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		if (SaveManager.PlayerSaveData.GetInsightState(InsightType.BridgeBoss_GateRaised) < InsightState.ResolvedButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(InsightType.BridgeBoss_GateRaised, InsightState.ResolvedButNotViewed, forceOverride: false);
			InsightObjectiveCompleteHUDEventArgs eventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.BridgeBoss_GateRaised, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, eventArgs);
		}
		RewiredMapController.SetIsInCutscene(inCutscene: false);
	}
}
