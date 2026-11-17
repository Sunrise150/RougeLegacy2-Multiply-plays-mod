using UnityEngine;

public interface ISpawnController
{
	GameObject gameObject { get; }

	bool ShouldSpawn { get; }

	SpawnLogicController SpawnLogicController { get; }
}
