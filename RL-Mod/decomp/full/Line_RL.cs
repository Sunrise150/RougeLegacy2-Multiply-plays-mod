using UnityEngine;

public struct Line_RL
{
	public Vector2 PointA { get; }

	public Vector2 PointB { get; }

	public Line_RL(Vector2 pointA, Vector2 pointB)
	{
		PointA = pointA;
		PointB = pointB;
	}

	public override bool Equals(object obj)
	{
		Line_RL line_RL = (Line_RL)obj;
		bool result = false;
		if (PointA == line_RL.PointA && PointB == line_RL.PointB)
		{
			result = true;
		}
		else if (PointA == line_RL.PointB && PointB == line_RL.PointA)
		{
			result = true;
		}
		return result;
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}
}
