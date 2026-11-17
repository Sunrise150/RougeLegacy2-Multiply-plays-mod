using System.Collections;
using Cinemachine;
using UnityEngine;

public class VirtualCameraShakeEffect : BaseEffect
{
	private CinemachineVirtualCamera m_activeVcam;

	private CinemachineConfiner_RL m_activeConfiner;

	private CinemachineBasicMultiChannelPerlin m_activeNoise;

	[SerializeField]
	private float m_shakeSpeed;

	[SerializeField]
	private float m_shakeAmplitude;

	private WaitRL_Yield m_waitYield;

	protected override void Awake()
	{
		base.Awake();
		m_waitYield = new WaitRL_Yield(0f, useUnscaledTime: true);
	}

	public override void Play(float duration = 0f, EffectStopType stopType = EffectStopType.Gracefully)
	{
		base.Play(duration, stopType);
		if ((bool)CameraController.CinemachineBrain && !CameraController.CinemachineBrain.ActiveVirtualCamera.IsNativeNull())
		{
			m_activeVcam = CameraController.CinemachineBrain.ActiveVirtualCamera as CinemachineVirtualCamera;
			if ((bool)m_activeVcam)
			{
				m_activeNoise = m_activeVcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
				m_activeConfiner = m_activeVcam.GetComponent<CinemachineConfiner_RL>();
			}
		}
		if ((bool)m_activeVcam && (bool)m_activeNoise && (bool)m_activeConfiner)
		{
			StartCoroutine(PlayTimedCameraShake(duration));
		}
		else
		{
			PlayComplete();
		}
	}

	private IEnumerator PlayTimedCameraShake(float duration)
	{
		ShakeVirtualCamera(m_shakeSpeed, m_shakeAmplitude);
		RumbleManager.StartRumble(useLeftMotor: true, useRightMotor: true, 0.5f, duration);
		if (duration > 0f)
		{
			m_waitYield.CreateNew(duration, useUnscaledTime: true);
			yield return m_waitYield;
			Stop(EffectStopType.Gracefully);
			yield break;
		}
		yield return null;
		if ((bool)base.SourceAnimator && duration == -1f)
		{
			int nameHash = base.SourceAnimator.GetCurrentAnimatorStateInfo(base.AnimatorLayer).shortNameHash;
			while ((bool)base.SourceAnimator && base.SourceAnimator.gameObject.activeInHierarchy && base.SourceAnimator.GetCurrentAnimatorStateInfo(base.AnimatorLayer).shortNameHash == nameHash)
			{
				yield return null;
			}
		}
		else
		{
			while ((bool)base.Source && base.Source.gameObject.activeInHierarchy)
			{
				yield return null;
			}
		}
		Stop(EffectStopType.Gracefully);
	}

	public override void Stop(EffectStopType stopType)
	{
		StopAllCoroutines();
		ShakeVirtualCamera(0f, 0f);
		m_activeVcam = null;
		RumbleManager.StopRumble();
		PlayComplete();
	}

	private void ShakeVirtualCamera(float shakeSpeed, float shakeSize)
	{
		if (m_activeNoise != null)
		{
			m_activeNoise.m_FrequencyGain = shakeSpeed;
			m_activeNoise.m_AmplitudeGain = shakeSize;
		}
	}

	protected override void OnDisable()
	{
		ShakeVirtualCamera(0f, 0f);
		RumbleManager.StopRumble();
		base.OnDisable();
	}
}
