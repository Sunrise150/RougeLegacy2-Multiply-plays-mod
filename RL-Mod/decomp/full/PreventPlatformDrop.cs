using System.Collections;
using MoreMountains.CorgiEngine;
using MoreMountains.Tools;
using UnityEngine;

[RequireComponent(typeof(CorgiController))]
public class PreventPlatformDrop : MonoBehaviour
{
	private CorgiController_RL m_controller;

	private EnemyController m_enemyController;

	private float m_raycastLength;

	private RaycastHit2D m_raycastLeft;

	private RaycastHit2D m_raycastRight;

	private void Awake()
	{
		m_controller = GetComponent<CorgiController_RL>();
		m_enemyController = GetComponent<EnemyController>();
	}

	private IEnumerator Start()
	{
		while (!m_controller.IsInitialized || !m_enemyController.IsInitialized)
		{
			yield return null;
		}
		m_raycastLength = m_enemyController.ControllerCorgi.BoundsWidth + 0.1f;
	}

	private void FixedUpdate()
	{
		if (m_controller.State != null && m_controller.State.IsGrounded && (bool)m_controller.StandingOnCollider && !m_enemyController.KnockedIntoAir && (m_controller.StandingOnCollider.CompareTag("Platform") || m_controller.StandingOnCollider.CompareTag("OneWay")))
		{
			CheckGroundRaycast_V2();
		}
	}

	private void CheckGroundRaycast_V2()
	{
		m_enemyController.LeftSidePlatformDropPrevented = false;
		m_enemyController.RightSidePlatformDropPrevented = false;
		Vector2 rayOriginPoint = (Vector2)m_controller.BoundsBottomLeftCorner + new Vector2(-0.1f, -0.1f);
		Vector2 rayOriginPoint2 = (Vector2)m_controller.BoundsBottomRightCorner + new Vector2(0.1f, -0.1f);
		m_raycastLeft = MMDebug.RayCast(rayOriginPoint, base.transform.right, m_raycastLength, m_controller.PlatformMask, Color.blue, m_controller.Parameters.DrawRaycastsGizmos);
		m_raycastRight = MMDebug.RayCast(rayOriginPoint2, -base.transform.right, m_raycastLength, m_controller.PlatformMask, Color.blue, m_controller.Parameters.DrawRaycastsGizmos);
		bool flag = true;
		if ((bool)m_raycastLeft && m_raycastLeft.distance > 0f && Mathf.Abs(Vector2.Angle(m_raycastLeft.normal, base.transform.up)) > m_enemyController.ControllerCorgi.Parameters.MaximumSlopeAngle)
		{
			flag = false;
		}
		bool flag2 = true;
		if ((bool)m_raycastRight && m_raycastRight.distance > 0f && Mathf.Abs(Vector2.Angle(m_raycastRight.normal, base.transform.up)) > m_enemyController.ControllerCorgi.Parameters.MaximumSlopeAngle)
		{
			flag2 = false;
		}
		if (!m_raycastLeft || ((bool)m_raycastLeft && !flag))
		{
			m_enemyController.LeftSidePlatformDropPrevented = true;
			if (m_controller.Velocity.x < 0f)
			{
				m_controller.SetForce(new Vector2(0f, m_controller.Velocity.y));
			}
		}
		if (!m_raycastRight || ((bool)m_raycastRight && !flag2))
		{
			m_enemyController.RightSidePlatformDropPrevented = true;
			if (m_controller.Velocity.x > 0f)
			{
				m_controller.SetForce(new Vector2(0f, m_controller.Velocity.y));
			}
		}
	}

	private void CheckGroundRaycast()
	{
		m_enemyController.LeftSidePlatformDropPrevented = false;
		m_enemyController.RightSidePlatformDropPrevented = false;
		Vector2 rayOriginPoint = m_controller.BoundsBottomLeftCorner;
		rayOriginPoint.y += 0.01f;
		Vector2 rayOriginPoint2 = m_controller.BoundsBottomRightCorner;
		rayOriginPoint2.y += 0.01f;
		m_raycastLeft = MMDebug.RayCast(rayOriginPoint, -base.transform.up, m_raycastLength, m_controller.PlatformMask, Color.blue, m_controller.Parameters.DrawRaycastsGizmos);
		m_raycastRight = MMDebug.RayCast(rayOriginPoint2, -base.transform.up, m_raycastLength, m_controller.PlatformMask, Color.blue, m_controller.Parameters.DrawRaycastsGizmos);
		float num = 0f;
		float num2 = 0f;
		bool flag = true;
		if ((bool)m_raycastLeft)
		{
			num2 = Mathf.Abs(Vector2.Angle(m_raycastLeft.normal, base.transform.up));
			if (num2 > m_enemyController.ControllerCorgi.Parameters.MaximumSlopeAngle)
			{
				flag = false;
			}
		}
		bool flag2 = true;
		if ((bool)m_raycastRight)
		{
			num = Mathf.Abs(Vector2.Angle(m_raycastRight.normal, base.transform.up));
			if (num > m_enemyController.ControllerCorgi.Parameters.MaximumSlopeAngle)
			{
				flag2 = false;
			}
		}
		if (num2 == 0f && m_raycastLeft.distance > 0.5f && num == 0f)
		{
			flag = false;
		}
		else if (num == 0f && m_raycastRight.distance > 0.5f && num2 == 0f)
		{
			flag2 = false;
		}
		if (!m_raycastLeft || ((bool)m_raycastLeft && !flag))
		{
			m_enemyController.LeftSidePlatformDropPrevented = true;
			if (m_controller.Velocity.x < 0f)
			{
				m_controller.SetForce(new Vector2(0f, m_controller.Velocity.y));
			}
		}
		if (!m_raycastRight || ((bool)m_raycastRight && !flag2))
		{
			m_enemyController.RightSidePlatformDropPrevented = true;
			if (m_controller.Velocity.x > 0f)
			{
				m_controller.SetForce(new Vector2(0f, m_controller.Velocity.y));
			}
		}
	}
}
