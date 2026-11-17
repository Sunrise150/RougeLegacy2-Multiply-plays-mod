using System.Collections;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class PortraitCutsceneController : MonoBehaviour, IRoomConsumer, IAudioEventEmitter
{
	private const string DOOR_UNLOCK_LEFT_AUDIO_PATH = "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_left";

	private const string DOOR_UNLOCK_CENTER_AUDIO_PATH = "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_center";

	private const string DOOR_UNLOCK_RIGHT_AUDIO_PATH = "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_right";

	private const string DOOR_OPEN_AUDIO_PATH = "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_doorOpen";

	[SerializeField]
	private TunnelSpawnController m_tunnelSpawner;

	[SerializeField]
	private GameObject m_cutsceneCameraGO;

	private WaitRL_Yield m_waitYield;

	public TunnelSpawnController TunnelSpawner => m_tunnelSpawner;

	public BaseRoom Room { get; private set; }

	public string Description => ToString();

	private void Awake()
	{
		m_waitYield = new WaitRL_Yield(0f);
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnterRoom);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if (CutsceneManager.IsCutsceneActive)
		{
			RunBossDoorAnimation();
		}
	}

	private void RunBossDoorAnimation()
	{
		StopAllCoroutines();
		StartCoroutine(BossDoorAnimationCoroutine());
	}

	private IEnumerator BossDoorAnimationCoroutine()
	{
		CutsceneManager.SetTraitsEnabled(enabled: false);
		yield return null;
		BossEntranceRoomController bossEntranceController = Room.gameObject.GetComponent<BossEntranceRoomController>();
		PortraitTunnelComponent portraitTunnel = ((!bossEntranceController) ? m_tunnelSpawner.Tunnel.GameObject.GetComponent<PortraitTunnelComponent>() : bossEntranceController.BossTunnelSpawner.Tunnel.gameObject.GetComponent<PortraitTunnelComponent>());
		portraitTunnel.SetBossDefeated(CutsceneManager.CutsceneSaveFlag, defeated: false);
		if ((bool)bossEntranceController)
		{
			bossEntranceController.SetBossTunnelState(BossTunnelState.Closed, skipToIdleState: true);
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		Room.CinemachineCamera.SetFollowTarget(m_cutsceneCameraGO.transform);
		playerController.StatusBarController.SetCanvasVisible(visible: false);
		playerController.Visuals.SetActive(value: false);
		playerController.ControllerCorgi.enabled = false;
		playerController.HitboxController.DisableAllCollisions = true;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, this, null);
		switch (CutsceneManager.CutsceneSaveFlag)
		{
		case PlayerSaveFlag.CastleBoss_Defeated:
		case PlayerSaveFlag.BridgeBoss_Defeated:
		case PlayerSaveFlag.StudyMiniboss_SpearKnight_Defeated:
			AudioManager.PlayOneShotAttached(this, "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_left", base.gameObject);
			break;
		case PlayerSaveFlag.ForestBoss_Defeated:
		case PlayerSaveFlag.StudyBoss_Defeated:
			AudioManager.PlayOneShotAttached(this, "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_center", base.gameObject);
			break;
		case PlayerSaveFlag.StudyMiniboss_SwordKnight_Defeated:
		case PlayerSaveFlag.TowerBoss_Defeated:
		case PlayerSaveFlag.CaveBoss_Defeated:
			AudioManager.PlayOneShotAttached(this, "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_right", base.gameObject);
			break;
		default:
			Debug.LogWarning($"<color=red>The PlayerSaveFlag {CutsceneManager.CutsceneSaveFlag} is not recognized by PortraitCutsceneController. Please add a report to Pivotal.</color>");
			break;
		}
		while (SceneLoader_RL.IsRunningTransitionWithLogic)
		{
			yield return null;
		}
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		yield return portraitTunnel.BossDefeatedCoroutine(CutsceneManager.CutsceneSaveFlag);
		if (portraitTunnel.IsPortraitComplete)
		{
			if ((bool)bossEntranceController)
			{
				m_waitYield.CreateNew(1f);
				yield return m_waitYield;
				AudioManager.PlayOneShotAttached(this, "event:/Cut_Scenes/sfx_Cutscene_doorUnlock_doorOpen", base.gameObject);
				bossEntranceController.SetBossTunnelState(BossTunnelState.PartlyOpen, skipToIdleState: false);
				m_waitYield.CreateNew(2f);
				yield return m_waitYield;
			}
		}
		else
		{
			m_waitYield.CreateNew(2f);
			yield return m_waitYield;
		}
		m_tunnelSpawner.Tunnel.ForceEnterTunnel(animate: true, CutsceneManager.ExitRoomTunnel);
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		CutsceneManager.ResetCutscene();
	}
}
