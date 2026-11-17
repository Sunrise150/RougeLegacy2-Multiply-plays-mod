using System.Collections;

public class SetFerrEdges_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		foreach (BaseRoom room in biomeController.Rooms)
		{
			BiomeCreatorTools.SetOuterEdges(room.TerrainManager.Platforms, room.Bounds);
		}
		yield break;
	}
}
