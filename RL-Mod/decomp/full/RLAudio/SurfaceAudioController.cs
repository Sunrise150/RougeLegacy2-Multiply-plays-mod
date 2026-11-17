using System;
using UnityEngine;

namespace RLAudio;

public class SurfaceAudioController : MonoBehaviour
{
	private CorgiController_RL m_corgiController;

	private SurfaceType m_standingOnSurfaceType = SurfaceType.None;

	private SurfaceType m_surfaceOverride = SurfaceType.None;

	private float m_surfaceTypeParameter;

	public EventHandler<float> StandingOnParameterChangeEvent { get; set; }

	public float SurfaceTypeParameter
	{
		get
		{
			return m_surfaceTypeParameter;
		}
		private set
		{
			m_surfaceTypeParameter = value;
		}
	}

	public SurfaceType StandingOnSurfaceType
	{
		get
		{
			return m_standingOnSurfaceType;
		}
		private set
		{
			m_standingOnSurfaceType = value;
		}
	}

	public SurfaceType SurfaceOverride
	{
		get
		{
			return m_surfaceOverride;
		}
		private set
		{
			m_surfaceOverride = value;
		}
	}

	private void Awake()
	{
		m_corgiController = GetComponent<CorgiController_RL>();
		if (m_corgiController != null)
		{
			m_corgiController.StandingOnChangeRelay.AddListener(OnStandingOnChange);
			return;
		}
		Debug.LogFormat("<color=red>| {0} | Missing Corgi Controller Component. If you see this message, please add a bug report to Pivotal</color>", this);
	}

	private void OnStandingOnChange(object sender, GameObject standingOn)
	{
		if (SurfaceOverride == SurfaceType.None)
		{
			StandingOnSurfaceType = SurfaceType.Default;
			Surface componentInParent = standingOn.GetComponentInParent<Surface>();
			if (componentInParent != null)
			{
				StandingOnSurfaceType = componentInParent.SurfaceType;
			}
			SurfaceTypeParameter = SurfaceAudioManager.GetSurfaceParameter(StandingOnSurfaceType);
			if (StandingOnParameterChangeEvent != null)
			{
				StandingOnParameterChangeEvent(this, SurfaceTypeParameter);
			}
		}
	}

	public void SetSurfaceOverride(SurfaceType surfaceType)
	{
		if (SurfaceOverride != surfaceType)
		{
			SurfaceOverride = surfaceType;
			if (surfaceType != SurfaceType.None)
			{
				SurfaceTypeParameter = SurfaceAudioManager.GetSurfaceParameter(SurfaceOverride);
			}
			else
			{
				SurfaceTypeParameter = SurfaceAudioManager.GetSurfaceParameter(StandingOnSurfaceType);
			}
			if (StandingOnParameterChangeEvent != null)
			{
				StandingOnParameterChangeEvent(this, SurfaceTypeParameter);
			}
		}
	}
}
