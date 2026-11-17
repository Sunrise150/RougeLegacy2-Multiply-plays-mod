using UnityEngine;
using UnityEngine.Events;

public class OnTriggerEventController : MonoBehaviour
{
	[SerializeField]
	private UnityEvent m_triggerEnterEvent;

	[SerializeField]
	private UnityEvent m_triggerExitEvent;

	[SerializeField]
	private bool m_triggerByCharacterOnly = true;

	private bool GetDoesCollisionTriggerEvent(Collider2D collision)
	{
		bool result = true;
		IRootObj componentInParent = collision.gameObject.GetComponentInParent<IRootObj>();
		if (componentInParent != null)
		{
			if (m_triggerByCharacterOnly && !componentInParent.gameObject.CompareTag("Player") && !componentInParent.gameObject.CompareTag("Enemy"))
			{
				result = false;
			}
		}
		else if (m_triggerByCharacterOnly)
		{
			result = false;
		}
		return result;
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{
		if (GetDoesCollisionTriggerEvent(collision) && m_triggerEnterEvent != null)
		{
			m_triggerEnterEvent.Invoke();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (GetDoesCollisionTriggerEvent(collision) && m_triggerExitEvent != null)
		{
			m_triggerExitEvent.Invoke();
		}
	}
}
