using System.Collections;
using RLWorldCreation;

public class CreateMergeRooms_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		MergeRoomStrategy mergeRoomStrategy = ((biomeController.Biome == BiomeType.Tower) ? new MergeRoomStrategy_Tower() : ((biomeController.Biome != BiomeType.Stone) ? new MergeRoomStrategy() : new MergeRoomStrategy_Bridge()));
		yield return mergeRoomStrategy.Run(this, biomeController);
	}
}
