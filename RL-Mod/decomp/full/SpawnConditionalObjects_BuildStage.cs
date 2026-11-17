using System.Collections;

public class SpawnConditionalObjects_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		foreach (BaseRoom room in biomeController.Rooms)
		{
			ISimpleSpawnController[] simpleSpawnControllers_NoProps = room.SpawnControllerManager.SimpleSpawnControllers_NoProps;
			foreach (ISimpleSpawnController simpleSpawnController in simpleSpawnControllers_NoProps)
			{
				if (simpleSpawnController.gameObject.activeInHierarchy)
				{
					simpleSpawnController.Spawn();
				}
			}
		}
		yield break;
	}
}
