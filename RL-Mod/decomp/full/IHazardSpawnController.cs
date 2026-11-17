public interface IHazardSpawnController : IComplexSpawnController, ISpawnController, IRoomConsumer, IIDConsumer, IStateConsumer, ISetSpawnType, IHasProjectileNameArray
{
	HazardCategory Category { get; }

	HazardType Type { get; }

	IHazard Hazard { get; }

	void SetType(HazardType hazardType);
}
