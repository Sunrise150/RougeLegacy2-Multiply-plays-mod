using System;
using System.Collections;
using UnityEngine;

public class WallStickCollision : MonoBehaviour
{
	private BaseCharacterController m_controller;

	private bool m_stickListenerAdded;

	private Action<CorgiController_RL> m_stickToWall;

	public StickToWallCollisionDelegate OnStickEvent { get; set; }

	public bool StickingToWall => m_controller.Velocity == Vector2.zero;

	public RaycastHit2D LastHitRaycast { get; set; }

	private void Awake()
	{
		GameObject root = this.GetRoot();
		m_controller = root.GetComponent<BaseCharacterController>();
		GetComponent<PreventPlatformDrop>();
		m_stickToWall = StickToWall;
	}

	private void OnEnable()
	{
		if (!m_stickListenerAdded)
		{
			if (!m_controller.IsInitialized)
			{
				StartCoroutine(EnableCollisionCoroutine());
				return;
			}
			m_stickListenerAdded = true;
			m_controller.ControllerCorgi.OnCorgiCollisionRelay.AddListener(m_stickToWall);
		}
	}

	private IEnumerator EnableCollisionCoroutine()
	{
		yield return new WaitUntil(() => m_controller.IsInitialized);
		m_stickListenerAdded = true;
		m_controller.ControllerCorgi.OnCorgiCollisionRelay.AddListener(m_stickToWall);
	}

	private void OnDisable()
	{
		if (m_stickListenerAdded)
		{
			m_stickListenerAdded = false;
			m_controller.ControllerCorgi.OnCorgiCollisionRelay.RemoveListener(m_stickToWall);
		}
	}

	private void StickToWall(CorgiController_RL corgiController)
	{
		if (base.isActiveAndEnabled && !StickingToWall)
		{
			if (OnStickEvent != null)
			{
				OnStickEvent(base.gameObject);
			}
			m_controller.SetVelocity(0f, 0f, additive: false);
			if (corgiController != null && corgiController.ContactList.Count > 0)
			{
				RaycastHit2D lastHitRaycast = corgiController.ContactList[0];
				Vector3 localEulerAngles = m_controller.Pivot.transform.localEulerAngles;
				localEulerAngles.z = CDGHelper.VectorToAngle(lastHitRaycast.normal) - 90f;
				m_controller.Pivot.transform.localEulerAngles = localEulerAngles;
				LastHitRaycast = lastHitRaycast;
			}
		}
	}

	private void LateUpdate()
	{
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
		if ((bool)currentPlayerRoom)
		{
			Rect boundsRect = currentPlayerRoom.BoundsRect;
			if (collisionBounds.xMin <= boundsRect.xMin)
			{
				StickToWall(null);
				Vector3 localEulerAngles = m_controller.Pivot.transform.localEulerAngles;
				localEulerAngles.z = -90f;
				m_controller.Pivot.transform.localEulerAngles = localEulerAngles;
				LastHitRaycast = default(RaycastHit2D);
			}
			else if (collisionBounds.xMax >= boundsRect.xMax)
			{
				StickToWall(null);
				Vector3 localEulerAngles2 = m_controller.Pivot.transform.localEulerAngles;
				localEulerAngles2.z = 90f;
				m_controller.Pivot.transform.localEulerAngles = localEulerAngles2;
				LastHitRaycast = default(RaycastHit2D);
			}
			else if (collisionBounds.yMin <= boundsRect.yMin)
			{
				StickToWall(null);
				Vector3 localEulerAngles3 = m_controller.Pivot.transform.localEulerAngles;
				localEulerAngles3.z = 0f;
				m_controller.Pivot.transform.localEulerAngles = localEulerAngles3;
				LastHitRaycast = default(RaycastHit2D);
			}
			else if (collisionBounds.yMax >= boundsRect.yMax)
			{
				StickToWall(null);
				Vector3 localEulerAngles4 = m_controller.Pivot.transform.localEulerAngles;
				localEulerAngles4.z = 180f;
				m_controller.Pivot.transform.localEulerAngles = localEulerAngles4;
				LastHitRaycast = default(RaycastHit2D);
			}
		}
	}
}
