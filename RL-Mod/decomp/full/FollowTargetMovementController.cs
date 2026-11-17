using Rewired;
using UnityEngine;

public class FollowTargetMovementController : MonoBehaviour
{
	private const float MAX_MOVEMENT = 5.25f;

	private const float MOVEMENT_SPEED = 0.25f;

	private PlayerController m_playerController;

	private Vector2 m_normalizedInput;

	private Vector2 m_localPosition;

	private void Awake()
	{
		m_playerController = GetComponentInParent<PlayerController>();
	}

	public void Reset()
	{
		base.transform.localPosition = Vector3.zero;
	}

	private void Update()
	{
		if (((bool)m_playerController && !m_playerController.IsInitialized) || !ReInput.isReady)
		{
			return;
		}
		Controller lastActiveController = Rewired_RL.Player.controllers.GetLastActiveController();
		if (lastActiveController == null)
		{
			return;
		}
		float num = 0f;
		float num2 = 0f;
		m_localPosition = base.transform.localPosition;
		if (lastActiveController.type == ControllerType.Joystick)
		{
			if (!m_playerController.CastAbility.IsAiming)
			{
				num = Rewired_RL.Player.GetAxis("MoveHorizontalR");
				num2 = Rewired_RL.Player.GetAxis("MoveVerticalR");
			}
		}
		else if (Rewired_RL.Player.GetButton("FreeLook"))
		{
			num = Rewired_RL.Player.GetAxis("MoveHorizontal");
			num2 = Rewired_RL.Player.GetAxis("MoveVertical");
		}
		m_normalizedInput.x = num;
		m_normalizedInput.y = num2;
		m_normalizedInput.Normalize();
		HandleHorizontal(num, Mathf.Abs(m_normalizedInput.x));
		HandleVertical(num2, Mathf.Abs(m_normalizedInput.y));
		base.transform.localPosition = m_localPosition;
	}

	private void HandleHorizontal(float horizontalInput, float maxMove)
	{
		float num = horizontalInput * 21f * Time.deltaTime;
		if (horizontalInput != 0f)
		{
			m_localPosition.x = Mathf.Clamp(m_localPosition.x + num, -5.25f * maxMove, 5.25f * maxMove);
		}
		else if (horizontalInput == 0f && m_localPosition.x != 0f)
		{
			num = 21f * Time.deltaTime;
			if (m_localPosition.x > 0f)
			{
				m_localPosition.x = Mathf.Clamp(m_localPosition.x - num, 0f, 5.25f);
			}
			else
			{
				m_localPosition.x = Mathf.Clamp(m_localPosition.x + num, -5.25f, 0f);
			}
		}
	}

	private void HandleVertical(float verticalInput, float maxMove)
	{
		float num = verticalInput * 21f * Time.deltaTime;
		if (verticalInput != 0f)
		{
			m_localPosition.y = Mathf.Clamp(m_localPosition.y + num, -5.25f * maxMove, 5.25f * maxMove);
			base.transform.localPosition = m_localPosition;
		}
		else if (verticalInput == 0f && m_localPosition.y != 0f)
		{
			num = 21f * Time.deltaTime;
			if (m_localPosition.y > 0f)
			{
				m_localPosition.y = Mathf.Clamp(m_localPosition.y - num, 0f, 5.25f);
			}
			else
			{
				m_localPosition.y = Mathf.Clamp(m_localPosition.y + num, -5.25f, 0f);
			}
		}
	}
}
