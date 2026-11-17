using System.Collections;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class HubtownDudCutsceneController : MonoBehaviour, IRoomConsumer, IAudioEventEmitter
{
	[SerializeField]
	private TunnelSpawnController m_tunnelSpawner;

	[SerializeField]
	private PropSpawnController m_oceanProp;

	[SerializeField]
	private GameObject m_cutsceneCameraGO;

	private WaitRL_Yield m_waitYield;

	private Transform m_oceanBG;

	private Transform m_oceanFG;

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
			m_oceanBG = m_oceanProp.PropInstance.transform.FindDeep("BGElements");
			m_oceanFG = m_oceanProp.PropInstance.transform.FindDeep("FGElements");
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
		BaseEffect cameraShake = EffectManager.PlayEffect(base.gameObject, null, "CameraShakeSmall_Effect", Vector2.zero, 999f, EffectStopType.Gracefully);
		CutsceneManager.SetTraitsEnabled(enabled: false);
		yield return null;
		PlayerController playerController = PlayerManager.GetPlayerController();
		Room.CinemachineCamera.SetFollowTarget(m_cutsceneCameraGO.transform);
		AudioManager.PlayOneShot(null, "event:/UI/FrontEnd/ui_fe_ending_risingWaters", m_cutsceneCameraGO.transform.position);
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
		TweenManager.TweenBy(m_oceanBG, 4f, Ease.Quad.EaseIn, "localPosition.y", 4);
		TweenManager.TweenBy(m_oceanFG, 4f, Ease.Quad.EaseIn, "localPosition.y", 4);
		m_waitYield.CreateNew(4.1f);
		yield return m_waitYield;
		m_tunnelSpawner.Tunnel.ForceEnterTunnel(animate: true, CutsceneManager.ExitRoomTunnel);
		RewiredMapController.SetIsInCutscene(inCutscene: false);
		CutsceneManager.ResetCutscene();
		cameraShake.Stop(EffectStopType.Immediate);
	}
}
