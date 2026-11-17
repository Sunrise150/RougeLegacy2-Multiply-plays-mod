using UnityEngine;

public class SingleLine_Multi_Hazard : Multi_Hazard
{
	protected Vector2 m_startingLocalPos;

	protected PivotPoint m_pivotPoint;

	protected float m_halfWidth = 10f;

	public override void Initialize(PivotPoint pivot, int width, HazardArgs hazardArgs)
	{
		m_halfWidth = (float)width / 2f;
		Vector3 zero = Vector3.zero;
		switch (pivot)
		{
		case PivotPoint.Left:
			zero.x += m_halfWidth;
			break;
		case PivotPoint.Right:
			zero.x -= m_halfWidth;
			break;
		}
		m_startingLocalPos = zero;
		if (m_pivot != null)
		{
			m_pivot.transform.localPosition = zero;
			m_pivotPoint = pivot;
		}
		base.InitialState = hazardArgs.InitialState;
		ResetHazard();
	}
}
