using UnityEngine;

public class ElevatorPlatform : SpecialPlatform
{
	private float m_startingPosY;

	private bool m_startMovingDown;

	public override void SetState(StateID state)
	{
		switch (state)
		{
		case StateID.One:
			m_startMovingDown = false;
			break;
		case StateID.Two:
			m_startMovingDown = true;
			break;
		case StateID.Random:
			if (CDGHelper.RandomPlusMinus() > 0)
			{
				m_startMovingDown = true;
			}
			else
			{
				m_startMovingDown = false;
			}
			break;
		}
		m_startingPosY = base.transform.localPosition.y;
	}

	private void Update()
	{
		Vector3 localPosition = base.transform.localPosition;
		float num = Mathf.Sin(Time.time * 2f) / 2f;
		if (m_startMovingDown)
		{
			localPosition.y = m_startingPosY - num * 3f;
		}
		else
		{
			localPosition.y = m_startingPosY + num * 3f;
		}
		base.transform.localPosition = localPosition;
	}
}
