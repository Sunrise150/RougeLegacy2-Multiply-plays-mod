public class PointHazardArgs : HazardArgs
{
	public float Radius { get; }

	public float RotationSpeed { get; }

	public float ExpansionDuration { get; }

	public PointHazardArgs(StateID initialState, float radius, float rotationSpeed, float expansionDuration)
		: base(initialState)
	{
		Radius = radius;
		RotationSpeed = rotationSpeed;
		ExpansionDuration = expansionDuration;
	}
}
