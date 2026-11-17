using System;
using FMOD.Studio;
using UnityEngine;
using UnityEngine.Serialization;

namespace RLAudio;

public class RLFMODEventEmitter : BaseFMODEventEmitter, IAudioEventEmitter
{
	[SerializeField]
	[FormerlySerializedAs("m_stopImmediatelyOnGameObjectDestroyed")]
	private bool m_isOneShot = true;

	[SerializeField]
	private bool m_attachToGameObject = true;

	public bool AttachToGameObject
	{
		get
		{
			if (base.Is3D)
			{
				return m_attachToGameObject;
			}
			return false;
		}
	}

	public string EventPath => m_eventPath;

	public bool IsOneShot => m_isOneShot;

	protected override void Initialize()
	{
		base.Initialize();
		base.IsInitialized = false;
		if (!IsOneShot && !string.IsNullOrEmpty(EventPath))
		{
			try
			{
				m_eventInstance = AudioUtility.GetEventInstance(EventPath, base.transform);
			}
			catch (Exception)
			{
				m_eventInstance = default(EventInstance);
				Debug.LogFormat("<color=red>| {0} | The given path (<b>{1}</b>) is not a valid FMOD Event Path</color>", this, EventPath);
			}
		}
		base.IsInitialized = true;
	}

	public override void Play()
	{
		if (base.Is3D)
		{
			Play3DAudio();
		}
		else
		{
			Play2DAudio();
		}
	}

	private void Play3DAudio()
	{
		if (base.Is3D)
		{
			if (IsOneShot)
			{
				if (AttachToGameObject)
				{
					AudioManager.PlayOneShotAttached(this, EventPath, base.gameObject);
				}
				else
				{
					AudioManager.PlayOneShot(this, EventPath, base.transform.position);
				}
			}
			else if (m_eventInstance.isValid())
			{
				if (AttachToGameObject)
				{
					AudioManager.PlayAttached(this, m_eventInstance, base.gameObject);
				}
				else
				{
					AudioManager.Play(this, m_eventInstance, base.transform.position);
				}
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | The FMOD Event is <b>NOT</b> 3D, but you're calling a method that requires that it is. If you see this message, please add a bug report to Pivotal.</color>", this);
		}
	}

	private void Play2DAudio()
	{
		if (!base.Is3D)
		{
			if (IsOneShot)
			{
				AudioManager.PlayOneShot(this, EventPath);
			}
			else if (m_eventInstance.isValid())
			{
				AudioManager.Play(this, m_eventInstance);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| { 0} | The FMOD Event is <b>NOT</b> 2D, but you're calling a method that requires that it is. If you see this message, please add a bug report to Pivotal.</color>", this);
		}
	}

	public void Play(Vector3 worldPosition)
	{
		if (base.Is3D)
		{
			if (IsOneShot)
			{
				AudioManager.PlayOneShot(this, EventPath, worldPosition);
			}
			else if (m_eventInstance.isValid())
			{
				AudioManager.Play(this, m_eventInstance, worldPosition);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | The FMOD Event is not 3D, but you're calling a method that requires that it is. If you see this message, please add a bug report to Pivotal.</color>", this);
		}
	}

	public void SetParameter(string parameterName, float value)
	{
		if (!IsOneShot)
		{
			if (m_eventInstance.isValid())
			{
				m_eventInstance.setParameterByName(parameterName, value);
			}
		}
		else
		{
			Debug.LogFormat("<color=red>| {0} | You called SetParameter on an Emitter marked as 'One Shot'. This is not allowed. If you see this message, please add a bug report to Pivotal.</color>", this);
		}
	}
}
