using System.Collections.Generic;
using UnityEngine;

public class Ferr2DHazardArgs : HazardArgs
{
	public Vector3 LocalPosition { get; }

	public List<Vector2> Points { get; }

	public Ferr2DHazardArgs(StateID initialState, Vector3 localPosition, List<Vector2> points)
		: base(initialState)
	{
		LocalPosition = localPosition;
		Points = points;
	}
}
