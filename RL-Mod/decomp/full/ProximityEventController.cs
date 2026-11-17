using System;
using UnityEngine;
using UnityEngine.Events;

public class ProximityEventController : MonoBehaviour
{
	[SerializeField]
	private float m_proximityRadius = 10f;

	[SerializeField]
	private UnityEvent m_playerEnteredRangeEvent;

	[SerializeField]
	private UnityEvent m_playerExitedRangeEvent;

	private bool m_hasEventBeenFired;

	private float m_proximitySquareMagnitude;

	private void Awake()
	{
		if (m_proximityRadius < 0f)
		{
			throw new ArgumentOutOfRangeException("m_proximityRadius", "Value must be greater than 0");
		}
		m_proximitySquareMagnitude = m_proximityRadius * m_proximityRadius;
	}

	private void Update()
	{
		if (PlayerManager.IsInstantiated)
		{
			CheckProximity();
		}
	}

	private void OnDisable()
	{
		if (m_hasEventBeenFired)
		{
			m_hasEventBeenFired = false;
			if (m_playerExitedRangeEvent != null)
			{
				m_playerExitedRangeEvent.Invoke();
			}
		}
	}

	protected virtual void CheckProximity()
	{
		Vector2 vector = PlayerManager.GetPlayerController().transform.position;
		Vector2 vector2 = base.transform.position;
		Vector2 vector3 = vector - vector2;
		if (m_hasEventBeenFired)
		{
			if (vector3.sqrMagnitude > m_proximitySquareMagnitude)
			{
				m_hasEventBeenFired = false;
				if (m_playerExitedRangeEvent != null)
				{
					m_playerExitedRangeEvent.Invoke();
				}
			}
		}
		else if (vector3.sqrMagnitude <= m_proximitySquareMagnitude)
		{
			m_hasEventBeenFired = true;
			if (m_playerEnteredRangeEvent != null)
			{
				m_playerEnteredRangeEvent.Invoke();
			}
		}
	}
}
