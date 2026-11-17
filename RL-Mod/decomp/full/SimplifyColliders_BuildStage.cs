using System.Collections;

public class SimplifyColliders_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		foreach (BaseRoom room in biomeController.Rooms)
		{
			BiomeCreatorTools.SimplifyAllFerr2DColliders(room, rebuild: true, applyBoxConversion: true);
		}
		yield break;
	}
}
