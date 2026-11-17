using System;
using UnityEngine;

[Serializable]
public class PostProcessingBiomeArtData
{
	[SerializeField]
	private MobilePostProcessingProfile m_foregroundProfile;

	[SerializeField]
	private MobilePostProcessingProfile m_backgroundProfile;

	public MobilePostProcessingProfile ForegroundProfile
	{
		get
		{
			return m_foregroundProfile;
		}
		private set
		{
			m_foregroundProfile = value;
		}
	}

	public MobilePostProcessingProfile BackgroundProfile
	{
		get
		{
			return m_backgroundProfile;
		}
		private set
		{
			m_backgroundProfile = value;
		}
	}

	public void SetMainProfile(MobilePostProcessingProfile behaviour)
	{
		if (!Application.isPlaying)
		{
			ForegroundProfile = behaviour;
		}
	}

	public void SetBackgroundProfile(MobilePostProcessingProfile behaviour)
	{
		if (!Application.isPlaying)
		{
			BackgroundProfile = behaviour;
		}
	}
}
