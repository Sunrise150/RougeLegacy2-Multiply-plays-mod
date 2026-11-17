using UnityEngine;
using UnityEngine.Events;

public class OnEnableEventController : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_onEnableEvent;

	[SerializeField]
	private UnityEvent m_onDisableEvent;

	private void OnEnable()
	{
		if (m_onEnableEvent != null)
		{
			m_onEnableEvent.Invoke();
		}
	}

	private void OnDisable()
	{
		if (m_onDisableEvent != null)
		{
			m_onDisableEvent.Invoke();
		}
	}
}
