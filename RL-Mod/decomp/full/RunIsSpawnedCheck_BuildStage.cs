using System.Collections;

public class RunIsSpawnedCheck_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		foreach (BaseRoom room in biomeController.Rooms)
		{
			ISpawnController[] spawnControllers = room.SpawnControllerManager.SpawnControllers;
			foreach (ISpawnController spawnController in spawnControllers)
			{
				if (spawnController != null && spawnController.SpawnLogicController != null)
				{
					spawnController.SpawnLogicController.RunIsSpawnedCheck();
				}
			}
		}
		yield break;
	}
}
