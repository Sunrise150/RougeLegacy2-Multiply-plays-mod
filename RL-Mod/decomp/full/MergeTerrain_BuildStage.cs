using System.Collections;
using System.Diagnostics;

public class MergeTerrain_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		Stopwatch timer = new Stopwatch();
		timer.Start();
		foreach (BaseRoom room in biomeController.Rooms)
		{
			RoomUtility.BuildAllFerr2DTerrains(room);
			if (timer.Elapsed.TotalMilliseconds >= 30.0)
			{
				yield return null;
				timer.Restart();
			}
		}
	}
}
