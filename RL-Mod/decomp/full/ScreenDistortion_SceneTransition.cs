using System.Collections;
using FMODUnity;
using RLAudio;
using SceneManagement_RL;
using UnityEngine;

public class ScreenDistortion_SceneTransition : Transition_V2, ISceneLoadingTransition, ITransition, IAudioEventEmitter
{
	[SerializeField]
	private float m_distortDuration = 1f;

	[SerializeField]
	[EventRef]
	private string m_transitionInAudioEventPath;

	[SerializeField]
	[EventRef]
	private string m_transitionOutAudioEventPath;

	private GameObject m_distortInPosObj;

	private GameObject m_distortOutPosObj;

	private HeirloomWarp_Effect m_warpEffect;

	private string m_description = string.Empty;

	public override TransitionID ID => TransitionID.ScreenDistortion;

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
		m_warpEffect = CameraController.ForegroundPerspCam.GetComponent<HeirloomWarp_Effect>();
	}

	public void SetDistortInPosition(GameObject obj)
	{
		m_distortInPosObj = obj;
	}

	public void SetDistortOutPosition(GameObject obj)
	{
		m_distortOutPosObj = obj;
	}

	public override IEnumerator Run()
	{
		yield break;
	}

	public IEnumerator TransitionIn()
	{
		if (m_distortInPosObj == null)
		{
			m_distortInPosObj = PlayerManager.GetPlayerController().gameObject;
		}
		AudioManager.PlayOneShot(this, m_transitionInAudioEventPath, m_distortInPosObj.transform.position);
		Vector3 vector = CameraController.GameCamera.WorldToViewportPoint(m_distortInPosObj.transform.position);
		m_warpEffect.WarpCenterX = vector.x;
		m_warpEffect.WarpCenterY = vector.y;
		m_warpEffect.DistortionAmount = 0f;
		m_warpEffect.enabled = true;
		yield return TweenManager.TweenTo_UnscaledTime(m_warpEffect, m_distortDuration, Ease.Back.EaseIn, "DistortionAmount", 1).TweenCoroutine;
		m_warpEffect.DistortionAmount = 1f;
		yield return null;
	}

	public IEnumerator TransitionOut()
	{
		if (m_distortOutPosObj == null)
		{
			m_distortOutPosObj = PlayerManager.GetPlayerController().gameObject;
		}
		AudioManager.PlayOneShot(this, m_transitionOutAudioEventPath, m_distortInPosObj.transform.position);
		Vector3 vector = CameraController.GameCamera.WorldToViewportPoint(m_distortOutPosObj.transform.position);
		m_warpEffect.WarpCenterX = vector.x;
		m_warpEffect.WarpCenterY = vector.y;
		m_warpEffect.DistortionAmount = 1f;
		yield return TweenManager.TweenTo_UnscaledTime(m_warpEffect, m_distortDuration, Ease.Back.EaseOut, "DistortionAmount", 0).TweenCoroutine;
		m_warpEffect.DistortionAmount = 0f;
		m_warpEffect.enabled = false;
	}

	private void OnDisable()
	{
		m_distortInPosObj = null;
		m_distortOutPosObj = null;
	}
}
