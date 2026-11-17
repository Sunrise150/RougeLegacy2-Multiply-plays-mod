using System.Collections;
using Cinemachine;
using FMODUnity;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.UI;

public class NewGamePlus_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition, IAudioEventEmitter
{
	[SerializeField]
	private Canvas m_canvas;

	[SerializeField]
	private CanvasGroup m_bgCanvasGroup;

	[SerializeField]
	private Image m_bgImage;

	[SerializeField]
	private StudioEventEmitter m_loopEventEmitter;

	private Vector2 m_storedPixelResolution;

	public override TransitionID ID => TransitionID.NewGamePlus;

	public string Description => ToString();

	public override IEnumerator Run()
	{
		yield break;
	}

	public IEnumerator TransitionIn()
	{
		m_canvas.renderMode = RenderMode.WorldSpace;
		RewiredMapController.SetIsInCutscene(inCutscene: true);
		PlayerController playerController = PlayerManager.GetPlayerController();
		CameraController.SoloCam.gameObject.SetActive(value: true);
		CameraController.SoloCam.transform.SetPositionX(CameraController.GameCamera.transform.position.x);
		CameraController.SoloCam.transform.SetPositionY(CameraController.GameCamera.transform.position.y);
		CameraController.SoloCam.Camera.orthographicSize = CameraController.GameCamera.orthographicSize;
		CameraController.SoloCam.AddToCameraLayer(playerController.Visuals);
		m_bgCanvasGroup.alpha = 0f;
		m_bgImage.color = Color.black;
		m_bgCanvasGroup.gameObject.SetActive(value: true);
		m_bgCanvasGroup.transform.SetPositionX(CameraController.GameCamera.transform.position.x);
		m_bgCanvasGroup.transform.SetPositionY(CameraController.GameCamera.transform.position.y);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.HidePlayerHUD, null, null);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.NGPlusChanged, null, null);
		playerController.Animator.SetBool("Victory", value: true);
		m_loopEventEmitter.Play();
		m_storedPixelResolution = CameraController.ForegroundPostProcessing.PixelResolution;
		float delay2 = Time.time + 0.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		CameraController.ForegroundPostProcessing.Pixelation = true;
		CameraController.ForegroundPostProcessing.PixelBaselineAmount = 1f;
		CameraController.ForegroundPostProcessing.PixelResolution = new Vector2(1920f, 1080f);
		TweenManager.TweenTo(CameraController.ForegroundPostProcessing, 1f, Ease.Quint.EaseOut, "PixelResolution.x", 64, "PixelResolution.y", 36);
		CameraController.ForegroundPostProcessing.Distortion = true;
		CameraController.ForegroundPostProcessing.LensDistortion = 0f;
		TweenManager.TweenTo(CameraController.ForegroundPostProcessing, 1f, Ease.Quint.EaseOut, "LensDistortion", 1);
		delay2 = Time.time + 0.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		playerController.Visuals.transform.SetLocalPositionY(-1f);
		_ = playerController.transform.position;
		_ = playerController.Visuals.transform.position;
		playerController.Pivot.transform.SetLocalPositionY(1f);
		TweenManager.TweenTo(playerController.Pivot.transform, 1.2f, Ease.Back.EaseInSmall, "localEulerAngles.z", 720);
		TweenManager.TweenTo(playerController.Pivot.transform, 1f, Ease.Quad.EaseIn, "delay", 0.2f, "localScale.x", 0, "localScale.y", 0);
		float num = playerController.BaseScaleToOffsetWith / playerController.transform.localScale.x;
		Vector2 vector = new Vector2(1.5f, 1.5f) * num;
		TweenManager.TweenBy(playerController.Pivot.transform, 1f, Ease.Quad.EaseIn, "delay", 0.2f, "localPosition.x", vector.x, "localPosition.y", vector.y);
		BaseEffect wormholeEffect = EffectManager.PlayEffect(playerController.gameObject, null, "NewGamePlusCharge_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
		CameraController.SoloCam.AddToCameraLayer(wormholeEffect.gameObject);
		yield return TweenManager.TweenTo(m_bgCanvasGroup, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
		delay2 = Time.time + 1.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		playerController.Visuals.SetActive(value: false);
		playerController.ControllerCorgi.GravityActive(state: false);
		delay2 = Time.time + 1.5f;
		while (Time.time < delay2)
		{
			yield return null;
		}
		m_bgImage.color = Color.white;
		wormholeEffect.Stop(EffectStopType.Immediate);
		m_loopEventEmitter.Stop();
		AudioManager.PlayOneShot(this, "event:/Cut_Scenes/sfx_cutscene_ngPlusTeleport_end");
		m_canvas.renderMode = RenderMode.ScreenSpaceOverlay;
	}

	public IEnumerator TransitionOut()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		PlayerController playerController = PlayerManager.GetPlayerController();
		float storedFallAscentMultiplier = playerController.FallMultiplierOverride;
		playerController.FallMultiplierOverride = 1f;
		playerController.Animator.SetBool("Victory", value: false);
		playerController.Visuals.SetActive(value: false);
		playerController.ControllerCorgi.GravityActive(state: false);
		GameObject gameObject = PlayerManager.GetCurrentPlayerRoom().gameObject.FindObjectReference("NGPlusSpawnPos");
		if ((bool)gameObject)
		{
			playerController.transform.localPosition = gameObject.transform.position;
			if (CameraController.CinemachineBrain.enabled)
			{
				CinemachineBrain.UpdateMethod updateMethod = CameraController.CinemachineBrain.m_UpdateMethod;
				CameraController.CinemachineBrain.m_UpdateMethod = CinemachineBrain.UpdateMethod.ManualUpdate;
				CameraController.CinemachineBrain.ManualUpdate();
				CameraController.CinemachineBrain.m_UpdateMethod = updateMethod;
			}
			CameraController.SoloCam.transform.SetPositionX(CameraController.GameCamera.transform.position.x);
			CameraController.SoloCam.transform.SetPositionY(CameraController.GameCamera.transform.position.y);
			CameraController.SoloCam.Camera.orthographicSize = CameraController.GameCamera.orthographicSize;
			m_bgCanvasGroup.transform.SetPositionX(CameraController.GameCamera.transform.position.x);
			m_bgCanvasGroup.transform.SetPositionY(CameraController.GameCamera.transform.position.y);
			yield return null;
			CameraController.ForegroundPostProcessing.Pixelation = true;
			CameraController.ForegroundPostProcessing.PixelBaselineAmount = 1f;
			CameraController.ForegroundPostProcessing.PixelResolution = new Vector2(64f, 36f);
		}
		TweenManager.TweenTo(m_bgCanvasGroup, 0.5f, Ease.None, "alpha", 0);
		TweenManager.TweenTo(CameraController.ForegroundPostProcessing, 1f, Ease.Quint.EaseIn, "LensDistortion", 0);
		yield return TweenManager.TweenTo(CameraController.ForegroundPostProcessing, 1f, Ease.Quint.EaseIn, "PixelResolution.x", 1920, "PixelResolution.y", 1080).TweenCoroutine;
		m_bgCanvasGroup.gameObject.SetActive(value: false);
		CameraController.ForegroundPostProcessing.Distortion = false;
		CameraController.ForegroundPostProcessing.Pixelation = false;
		CameraController.ForegroundPostProcessing.PixelBaselineAmount = 0f;
		if ((bool)TraitManager.GetActiveTrait(TraitType.EnemiesCensored))
		{
			CameraController.ForegroundPostProcessing.Pixelation = true;
			CameraController.ForegroundPostProcessing.PixelResolution = m_storedPixelResolution;
		}
		float delay3 = Time.time + 0.5f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		EffectManager.PlayEffect(playerController.gameObject, null, "NewGamePlusPoof_Effect", playerController.Midpoint, 0f, EffectStopType.Gracefully);
		playerController.Pivot.transform.localScale = new Vector3(1f, 1f, 1f);
		playerController.Pivot.transform.localPosition = Vector3.zero;
		playerController.Visuals.transform.SetLocalPositionY(0f);
		playerController.Visuals.SetActive(value: true);
		playerController.ControllerCorgi.GravityActive(state: true);
		AudioManager.PlayOneShotAttached(this, "event:/Cut_Scenes/sfx_cutscene_ngPlusTeleport_playerAppear", playerController.gameObject);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayPlayerHUD, null, null);
		playerController.Animator.SetTrigger("Death5");
		while (!playerController.IsGrounded)
		{
			yield return null;
		}
		delay3 = Time.time + 1f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		playerController.SetVelocity(0f, 5f, additive: false);
		playerController.Animator.Play("JumpBlendTree");
		delay3 = Time.time + 0.1f;
		while (Time.time < delay3)
		{
			yield return null;
		}
		while (!playerController.IsGrounded)
		{
			yield return null;
		}
		playerController.FallMultiplierOverride = storedFallAscentMultiplier;
		RewiredMapController.SetIsInCutscene(inCutscene: false);
	}
}
