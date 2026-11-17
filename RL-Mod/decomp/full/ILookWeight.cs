public interface ILookWeight
{
	float FemaleWeight { get; }

	float MaleWeight { get; }

	string[] Tags { get; }

	bool ExcludeFromWeighing { get; }
}
