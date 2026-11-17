using UnityEngine;

public class BurstLogic : MonoBehaviour
{
	[SerializeField]
	private TrailRenderer m_trailRenderer;

	private float m_speed;

	private Vector2 m_heading;

	private float m_startTime;

	private float m_duration;

	private float m_internalSpeed;

	private Vector2 m_moveTowardsStartingPos;

	private bool m_movingAway;

	public Transform DestinationOverride { get; set; }

	public IMidpointObj DestinationMidpointOverride { get; set; }

	public void ClearTrail()
	{
		if ((bool)m_trailRenderer)
		{
			m_trailRenderer.Clear();
		}
	}

	public void Burst(float speed, float angle = -1f)
	{
		m_speed = speed;
		if (angle < 0f)
		{
			angle = Random.Range(0, 360);
		}
		m_heading = CDGHelper.AngleToVector(angle);
		m_duration = 0.25f;
		m_startTime = Time.time;
		m_movingAway = true;
		m_internalSpeed = 0f;
	}

	public void BurstTowards(float speed)
	{
		m_heading = Vector2.one;
		m_speed = speed;
		m_duration = 0.25f;
		m_startTime = Time.time;
		m_movingAway = false;
		m_internalSpeed = 0f;
		m_moveTowardsStartingPos = base.transform.position;
	}

	private void FixedUpdate()
	{
		if (m_movingAway)
		{
			Vector3 position = base.transform.position;
			position.x += m_heading.x * m_internalSpeed * Time.deltaTime;
			position.y += m_heading.y * m_internalSpeed * Time.deltaTime;
			base.transform.position = position;
			float num = (Time.time - m_startTime) / m_duration;
			m_internalSpeed = Mathf.Lerp(m_speed, 0f, num);
			if (num >= 1f)
			{
				m_movingAway = false;
				m_moveTowardsStartingPos = base.transform.position;
				m_startTime = Time.time;
			}
			return;
		}
		float num2 = (Time.time - m_startTime) / m_duration;
		Vector3 position2 = base.transform.position;
		if (DestinationOverride == null && DestinationMidpointOverride == null)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			position2.x = Mathf.Lerp(m_moveTowardsStartingPos.x, playerController.Midpoint.x, num2);
			position2.y = Mathf.Lerp(m_moveTowardsStartingPos.y, playerController.Midpoint.y, num2);
		}
		else
		{
			Vector2 vector = ((DestinationMidpointOverride != null) ? DestinationMidpointOverride.Midpoint : DestinationOverride.position);
			position2.x = Mathf.Lerp(m_moveTowardsStartingPos.x, vector.x, num2);
			position2.y = Mathf.Lerp(m_moveTowardsStartingPos.y, vector.y, num2);
		}
		base.transform.position = position2;
		if (num2 >= 1f)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
