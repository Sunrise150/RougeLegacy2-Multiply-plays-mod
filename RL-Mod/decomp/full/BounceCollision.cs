using System;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.Events;

public class BounceCollision : MonoBehaviour
{
	[SerializeField]
	private bool m_bounceTowardsPlayer;

	[SerializeField]
	private UnityEvent BounceUnityEvent;

	private const float BOUNCE_CHECK_TIMER = 0.05f;

	private BaseCharacterController m_controller;

	private float m_bounceTimer;

	private bool m_bounceListenerAdded;

	private Action<CorgiController_RL> m_bounce;

	private Relay<GameObject> m_onBounceRelay = new Relay<GameObject>();

	public IRelayLink<GameObject> OnBounceRelay => m_onBounceRelay.link;

	public bool JustBounced => Time.time < m_bounceTimer;

	private void Awake()
	{
		GameObject root = this.GetRoot();
		m_controller = root.GetComponent<BaseCharacterController>();
		GetComponent<PreventPlatformDrop>();
		m_bounce = Bounce;
	}

	private void OnEnable()
	{
		if (!m_bounceListenerAdded && m_controller.IsInitialized)
		{
			m_bounceListenerAdded = true;
			m_controller.ControllerCorgi.RetainVelocity = true;
			m_controller.ControllerCorgi.OnCorgiCollisionRelay.AddListener(m_bounce);
		}
	}

	private void OnDisable()
	{
		if (m_bounceListenerAdded)
		{
			m_bounceListenerAdded = false;
			m_controller.ControllerCorgi.RetainVelocity = false;
			m_controller.ControllerCorgi.OnCorgiCollisionRelay.RemoveListener(m_bounce);
		}
	}

	private void Bounce(CorgiController_RL corgiController)
	{
		if (JustBounced)
		{
			return;
		}
		m_onBounceRelay.Dispatch(base.gameObject);
		if (BounceUnityEvent != null)
		{
			BounceUnityEvent.Invoke();
		}
		m_bounceTimer = 0.05f + Time.time;
		if (m_bounceTowardsPlayer)
		{
			BounceTowardsPlayer();
			return;
		}
		Vector2 velocity = m_controller.Velocity;
		float magnitude = velocity.magnitude;
		velocity.Normalize();
		foreach (RaycastHit2D contact in corgiController.ContactList)
		{
			Vector2 vector = new Vector2(contact.normal.y, contact.normal.x * -1f);
			Vector2 vector2 = 2f * (Vector2.Dot(velocity, vector) / Vector2.Dot(vector, vector)) * vector - velocity;
			vector2.Normalize();
			m_controller.SetVelocity(vector2.x * magnitude, vector2.y * magnitude, additive: false);
		}
	}

	private void BounceTowardsPlayer()
	{
		Vector2 vector = CDGHelper.VectorBetweenPts(m_controller.Midpoint, PlayerManager.GetPlayerController().Midpoint);
		vector.Normalize();
		m_controller.SetVelocity(vector.x * m_controller.Velocity.magnitude, vector.y * m_controller.Velocity.magnitude, additive: false);
	}

	private void LateUpdate()
	{
		if (!m_bounceListenerAdded && m_controller.IsInitialized)
		{
			m_bounceListenerAdded = true;
			m_controller.ControllerCorgi.RetainVelocity = true;
			m_controller.ControllerCorgi.OnCorgiCollisionRelay.AddListener(m_bounce);
		}
		ConstrainEnemyMovementToRoom();
	}

	private void ConstrainEnemyMovementToRoom()
	{
		Rect collisionBounds = m_controller.CollisionBounds;
		if (collisionBounds.width == 0f || collisionBounds.height == 0f)
		{
			return;
		}
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (!currentPlayerRoom)
		{
			return;
		}
		Rect boundsRect = currentPlayerRoom.BoundsRect;
		if ((collisionBounds.xMin < boundsRect.xMin && m_controller.Velocity.x < 0f) || (collisionBounds.xMax > boundsRect.xMax && m_controller.Velocity.x > 0f))
		{
			m_onBounceRelay.Dispatch(base.gameObject);
			if (BounceUnityEvent != null)
			{
				BounceUnityEvent.Invoke();
			}
			if (m_bounceTowardsPlayer)
			{
				BounceTowardsPlayer();
			}
			else
			{
				m_controller.SetVelocityX(0f - m_controller.Velocity.x, additive: false);
			}
		}
		if ((collisionBounds.yMin < boundsRect.yMin && m_controller.Velocity.y < 0f) || (collisionBounds.yMax > boundsRect.yMax && m_controller.Velocity.y > 0f))
		{
			m_onBounceRelay.Dispatch(base.gameObject);
			if (BounceUnityEvent != null)
			{
				BounceUnityEvent.Invoke();
			}
			if (m_bounceTowardsPlayer)
			{
				BounceTowardsPlayer();
			}
			else
			{
				m_controller.SetVelocityY(0f - m_controller.Velocity.y, additive: false);
			}
		}
	}
}
