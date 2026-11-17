using System.Collections;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class DragonCutsceneController : MonoBehaviour, IRoomConsumer, IAudioEventEmitter
{
	[SerializeField]
	private TunnelSpawnController m_tunnelSpawner;

	[SerializeField]
	private PropSpawnController m_dragonPropSpawner;

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
		Animator dragonAnimator = m_dragonPropSpawner.PropInstance.GetComponent<Animator>();
		if (CutsceneManager.CutsceneSaveFlag == PlayerSaveFlag.CaveMiniboss_White_Defeated)
		{
			if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated))
			{
				dragonAnimator.SetTrigger("RightChainBreakInstant");
			}
		}
		else if (CutsceneManager.CutsceneSaveFlag == PlayerSaveFlag.CaveMiniboss_Black_Defeated && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated))
		{
			dragonAnimator.SetTrigger("LeftChainBreakInstant");
		}
		CutsceneManager.SetTraitsEnabled(enabled: false);
		yield return null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		Room.CinemachineCamera.SetFollowTarget(m_cutsceneCameraGO.transform);
		playerController.StatusBarController.SetCanvasVisible(visible: false);
		playerController.Visuals.SetActive(value: false);
		playerController.ControllerCorgi.enabled = false;
		playerController.HitboxController.DisableAllCollisions = true;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, this, null);
		while (SceneLoader_RL.IsRunningTransitionWithLogic)
		{
			yield return null;
		}
		m_waitYield.CreateNew(1f);
		yield return m_waitYield;
		bool flag = false;
		if (CutsceneManager.CutsceneSaveFlag == PlayerSaveFlag.CaveMiniboss_White_Defeated)
		{
			dragonAnimator.SetTrigger("LeftChainBreak");
			flag = true;
		}
		else if (CutsceneManager.CutsceneSaveFlag == PlayerSaveFlag.CaveMiniboss_Black_Defeated)
		{
			dragonAnimator.SetTrigger("RightChainBreak");
			flag = true;
		}
		if (flag)
		{
			m_waitYield.CreateNew(4f);
			yield return m_waitYield;
		}
		if (SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated) && SaveManager.PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated))
		{
			dragonAnimator.SetTrigger("CollarBreak");
			yield return null;
			m_waitYield.CreateNew(6f);
			yield return m_waitYield;
		}
		m_tunnelSpawner.Tunnel.ForceEnterTunnel(animate: true, CutsceneManager.ExitRoomTunnel);
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		CutsceneManager.ResetCutscene();
	}
}
