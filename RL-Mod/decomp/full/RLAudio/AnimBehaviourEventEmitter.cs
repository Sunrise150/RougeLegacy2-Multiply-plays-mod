using System;
using FMOD.Studio;
using FMODUnity;
using UnityEngine;

namespace RLAudio;

public abstract class AnimBehaviourEventEmitter : StateMachineBehaviour, IAudioEventEmitter
{
	public static bool DisableEmitter_STATIC;

	[EventRef]
	public string Event;

	public ParamRef[] Params = new ParamRef[0];

	[SerializeField]
	protected bool m_useSurfaceParameter;

	[SerializeField]
	protected AnimBehaviourCondition[] m_conditions;

	private const string SURFACE_PARAMETER_NAME = "Surface";

	protected PARAMETER_ID m_surfaceParamaterID;

	protected EventInstance m_eventInstance;

	protected EventDescription m_eventDescription;

	private bool m_hasSearchedForSurfaceController;

	private SurfaceAudioController m_surfaceController;

	private bool m_isInitialized;

	public string Description { get; protected set; }

	protected virtual void InitializeEventInstance(Animator animator)
	{
		try
		{
			m_eventInstance = RuntimeManager.CreateInstance(Event);
			m_eventInstance.getDescription(out m_eventDescription);
			if (m_useSurfaceParameter)
			{
				m_surfaceParamaterID = AnimBehaviourEventEmitterUtility.GetParameterID(m_eventDescription, "Surface");
			}
		}
		catch (EventNotFoundException)
		{
			if (Event == string.Empty)
			{
				Debug.LogFormat("<color=red>| {0} | No FMOD Event path has been specified on a State in Animator ({1}). If you see this message, please add a bug report to Pivotal.</color>", this, animator.name);
			}
			else
			{
				Debug.LogFormat("<color=red>| {0} | No FMOD Event named ({1}). If you If you see this message, please add a bug report to Pivotal.</color>", this, Event);
			}
		}
		m_isInitialized = true;
	}

	public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
	{
		if (DisableEmitter_STATIC || !GetShouldPlay(animator))
		{
			return;
		}
		if (!m_isInitialized)
		{
			InitializeEventInstance(animator);
		}
		if (m_useSurfaceParameter && !m_hasSearchedForSurfaceController && !m_surfaceController)
		{
			m_hasSearchedForSurfaceController = true;
			m_surfaceController = animator.GetComponent<SurfaceAudioController>();
			if (!m_surfaceController)
			{
				Debug.LogFormat("<color=red>| {0} | No SurfaceController found on ({1}). If you see this message, please add a bug report to Pivotal.</color>", this, animator.name);
			}
			else
			{
				SurfaceAudioController surfaceController = m_surfaceController;
				surfaceController.StandingOnParameterChangeEvent = (EventHandler<float>)Delegate.Combine(surfaceController.StandingOnParameterChangeEvent, new EventHandler<float>(OnStandingOnParameterChange));
			}
		}
		if (string.IsNullOrEmpty(Description))
		{
			Description = $"({animator.name} : Animation State ({stateInfo.fullPathHash}) : {this})";
		}
	}

	private void OnDisable()
	{
		Stop();
	}

	private void OnDestroy()
	{
		if (m_eventInstance.isValid())
		{
			m_eventInstance.release();
		}
	}

	protected bool GetShouldPlay(Animator animator)
	{
		bool result = true;
		if (m_conditions.Length != 0)
		{
			for (int i = 0; i < m_conditions.Length; i++)
			{
				if (!m_conditions[i].IsTrue(animator))
				{
					result = false;
					break;
				}
			}
		}
		return result;
	}

	private void OnStandingOnParameterChange(object sender, float newValue)
	{
		if (m_eventInstance.isValid())
		{
			m_eventInstance.setParameterByID(m_surfaceParamaterID, newValue);
			m_eventInstance.getParameterByID(m_surfaceParamaterID, out var _);
		}
	}

	protected void Play(Animator animator)
	{
		if (!EffectTriggerAnimBehaviour.DISABLE_GLOBALLY && !EffectManager.AnimatorEffectsDisabled(animator) && m_eventInstance.isValid())
		{
			AudioManager.PlayAttached(this, m_eventInstance, animator.gameObject);
		}
	}

	protected void Stop()
	{
		if (m_eventInstance.isValid())
		{
			AudioManager.Stop(m_eventInstance);
		}
	}
}
