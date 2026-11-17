using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TerrainRicochetProjectile_Logic : BaseProjectileLogic
{
	[SerializeField]
	private UnityEvent m_bounceUnityEvent;

	private const float BOUNCE_CHECK_TIMER = 0.05f;

	private CorgiController_RL m_controller;

	private IHitboxController m_hbController;

	private float m_bounceTimer;

	public bool JustBounced => Time.time < m_bounceTimer;

	public event EventHandler OnRicochet;

	protected override void Awake()
	{
		base.Awake();
		GameObject root = this.GetRoot();
		m_controller = root.GetComponent<CorgiController_RL>();
		m_hbController = root.GetComponentInChildren<IHitboxController>();
	}

	private IEnumerator Start()
	{
		while (!m_controller.IsInitialized)
		{
			yield return null;
		}
		m_controller.OnCorgiCollisionRelay.AddListener(Bounce);
		m_controller.RetainVelocity = true;
	}

	private void OnDestroy()
	{
		m_controller.OnCorgiCollisionRelay.RemoveListener(Bounce);
	}

	private void Bounce(CorgiController_RL corgiController)
	{
		if (JustBounced)
		{
			return;
		}
		if (m_bounceUnityEvent != null)
		{
			m_bounceUnityEvent.Invoke();
		}
		m_bounceTimer = 0.05f + Time.time;
		Vector2 velocity = base.SourceProjectile.Velocity;
		float magnitude = velocity.magnitude;
		foreach (RaycastHit2D contact in corgiController.ContactList)
		{
			Vector2 vector = new Vector2(contact.normal.y, contact.normal.x * -1f);
			Vector2 heading = 2f * (Vector2.Dot(velocity, vector) / Vector2.Dot(vector, vector)) * vector - velocity;
			base.SourceProjectile.Heading = heading;
			base.SourceProjectile.SetCorgiVelocity(base.SourceProjectile.Heading * magnitude);
		}
		if (this.OnRicochet != null)
		{
			this.OnRicochet(this, null);
		}
	}

	private void LateUpdate()
	{
		ConstrainEnemyMovementToRoom();
	}

	private void ConstrainEnemyMovementToRoom()
	{
		Bounds bounds = m_hbController.GetCollider(HitboxType.Terrain).bounds;
		Room room = PlayerManager.GetCurrentPlayerRoom() as Room;
		if (!room)
		{
			return;
		}
		Rect boundsRect = room.BoundsRect;
		float num = bounds.center.x - bounds.extents.x;
		float num2 = bounds.center.x + bounds.extents.x;
		float num3 = bounds.center.y - bounds.extents.y;
		float num4 = bounds.center.y + bounds.extents.y;
		bool flag = false;
		if ((num < boundsRect.xMin && m_controller.Velocity.x < 0f) || (num2 > boundsRect.xMax && m_controller.Velocity.x > 0f))
		{
			m_controller.SetHorizontalForce(0f - m_controller.Velocity.x);
			flag = true;
		}
		if ((num3 < boundsRect.yMin && m_controller.Velocity.y < 0f) || (num4 > boundsRect.yMax && m_controller.Velocity.y > 0f))
		{
			m_controller.SetVerticalForce(0f - m_controller.Velocity.y);
			flag = true;
		}
		if (flag)
		{
			base.SourceProjectile.Heading = base.SourceProjectile.Velocity;
			if (this.OnRicochet != null)
			{
				this.OnRicochet(this, null);
			}
			if (m_bounceUnityEvent != null)
			{
				m_bounceUnityEvent.Invoke();
			}
		}
	}
}
