using System.Collections;
using FMODUnity;
using UnityEngine;

public class DelayEventEmitterInitialization : MonoBehaviour
{
	[SerializeField]
	private StudioEventEmitter m_eventEmitter;

	[SerializeField]
	private EmitterGameEvent m_playEvent;

	[SerializeField]
	private EmitterGameEvent m_stopEvent;

	private bool m_initialized;

	private void Awake()
	{
		m_eventEmitter.PlayEvent = EmitterGameEvent.None;
		m_eventEmitter.StopEvent = EmitterGameEvent.None;
	}

	private void OnEnable()
	{
		if (!m_initialized)
		{
			StartCoroutine(DelayEmitterInitialization());
		}
	}

	private IEnumerator DelayEmitterInitialization()
	{
		yield return null;
		m_eventEmitter.PlayEvent = m_playEvent;
		m_eventEmitter.StopEvent = m_stopEvent;
		m_initialized = true;
	}
}
