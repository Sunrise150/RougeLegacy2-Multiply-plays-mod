using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public abstract class BaseFMODEventEmitter : MonoBehaviour, IAudioEventEmitter
{
	[SerializeField]
	[EventRef]
	protected string m_eventPath;

	[SerializeField]
	protected bool m_playOnEnable = true;

	[SerializeField]
	protected bool m_stopOnDisable = true;

	protected EventInstance m_eventInstance;

	public string Description => ToString();

	public bool Is3D { get; private set; }

	public bool IsInitialized { get; protected set; }

	private void Awake()
	{
		if (!IsInitialized)
		{
			Initialize();
		}
	}

	private void OnEnable()
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		if (m_playOnEnable)
		{
			Play();
		}
	}

	private void OnDisable()
	{
		if (m_stopOnDisable)
		{
			Stop();
		}
	}

	private void OnDestroy()
	{
		if (m_eventInstance.isValid())
		{
			m_eventInstance.release();
		}
	}

	protected virtual void Initialize()
	{
		if (!string.IsNullOrEmpty(m_eventPath))
		{
			try
			{
				RuntimeManager.GetEventDescription(m_eventPath).is3D(out var is3D);
				Is3D = is3D;
			}
			catch (Exception)
			{
				Debug.LogFormat("<color=red>| {0} | The given FMOD Event path ({1}) is invalid.</color>", this, m_eventPath);
			}
		}
		IsInitialized = true;
	}

	public abstract void Play();

	public void Stop()
	{
		if (m_eventInstance.isValid())
		{
			AudioManager.Stop(m_eventInstance);
		}
	}
}
