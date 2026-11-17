using System.Collections;
using FMOD.Studio;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.Video;

public class PlayEndingVideo_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition
{
	[SerializeField]
	private float m_timeToFade = 1f;

	[SerializeField]
	private CanvasGroup m_canvasGroup;

	private VideoPlayer m_videoPlayer;

	private EventInstance m_audioInstance;

	private Coroutine m_syncCoroutine;

	private VideoPlayer.ErrorEventHandler m_videoPlayer_ErrorReceived;

	private bool m_videoPreparationFailed;

	public override TransitionID ID => TransitionID.PlayEndingVideo;

	protected override void Awake()
	{
		base.Awake();
		m_videoPlayer = GetComponent<VideoPlayer>();
		base.gameObject.SetLayerRecursively(LayerMask.NameToLayer("UI"));
		m_audioInstance = AudioUtility.GetEventInstance("event:/UI/FrontEnd/ui_fe_endingCinematic", base.transform);
		m_videoPlayer_ErrorReceived = VideoPlayer_ErrorReceived;
	}

	private void OnDestroy()
	{
		if (m_audioInstance.isValid())
		{
			m_audioInstance.release();
		}
	}

	public IEnumerator TransitionIn()
	{
		AudioManager.StopAllAudio(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
		m_canvasGroup.alpha = 1f;
		m_videoPlayer.targetCamera = CameraController.UICamera;
		m_videoPlayer.errorReceived += m_videoPlayer_ErrorReceived;
		m_videoPreparationFailed = false;
		m_videoPlayer.Prepare();
		while (!m_videoPlayer.isPrepared && !m_videoPreparationFailed)
		{
			yield return null;
		}
		if (m_videoPreparationFailed)
		{
			yield break;
		}
		m_videoPlayer.Play();
		while (!m_videoPlayer.isPlaying)
		{
			yield return null;
		}
		if (m_audioInstance.isValid())
		{
			AudioManager.Play(null, m_audioInstance);
			if (m_syncCoroutine != null)
			{
				StopCoroutine(m_syncCoroutine);
			}
			m_syncCoroutine = StartCoroutine(SyncAudioCoroutine());
		}
		TweenManager.TweenTo_UnscaledTime(m_canvasGroup, 0.1f, Ease.None, "alpha", 0);
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.1f);
		while (m_videoPlayer.time < 2.5999999046325684)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.4f);
		while (m_videoPlayer.time < 4.0)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.1f);
		while (m_videoPlayer.time < 6.0)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0f);
		while (m_videoPlayer.time < 6.5)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.2f, 0.2f);
		while (m_videoPlayer.time < 7.349999904632568)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.4f, 0.2f);
		while (m_videoPlayer.time < 7.800000190734863)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.4f);
		while (m_videoPlayer.time < 10.300000190734863)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.1f);
		while (m_videoPlayer.time < 16.700000762939453)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0f);
		while (m_videoPlayer.time < 24.5)
		{
			yield return null;
		}
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.4f);
		while (m_videoPlayer.time < 28.0)
		{
			yield return null;
		}
		RumbleManager.StopRumble();
		while (m_videoPlayer.time < 31.0)
		{
			yield return null;
		}
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, 0.5f, Ease.None, "alpha", 1).TweenCoroutine;
	}

	public IEnumerator TransitionOut()
	{
		while (m_videoPlayer.isPlaying && !m_videoPreparationFailed)
		{
			yield return null;
		}
		if (m_syncCoroutine != null)
		{
			StopCoroutine(m_syncCoroutine);
		}
		m_videoPlayer.Stop();
		m_videoPlayer.targetCamera = null;
		m_canvasGroup.alpha = 1f;
		yield return TweenManager.TweenTo_UnscaledTime(m_canvasGroup, m_timeToFade, Ease.None, "alpha", 0).TweenCoroutine;
	}

	private IEnumerator SyncAudioCoroutine()
	{
		float syncTimer = Time.unscaledTime;
		while (true)
		{
			if (Time.unscaledTime < syncTimer)
			{
				yield return null;
				continue;
			}
			syncTimer = Time.unscaledTime + 5f;
			if (!m_audioInstance.isValid() || !m_videoPlayer || !m_videoPlayer.isPlaying)
			{
				break;
			}
			float num = 0.3f;
			m_audioInstance.setTimelinePosition(Mathf.RoundToInt((float)((m_videoPlayer.time + (double)num) * 1000.0)));
		}
		m_syncCoroutine = null;
	}

	private void VideoPlayer_ErrorReceived(VideoPlayer source, string message)
	{
		source.errorReceived -= m_videoPlayer_ErrorReceived;
		m_videoPreparationFailed = true;
		Debug.Log("Failed to play video. Error: " + message);
	}

	public override IEnumerator Run()
	{
		yield break;
	}
}
