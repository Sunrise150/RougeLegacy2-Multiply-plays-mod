using System.Collections;

public class MoveRoomsToRoot_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		foreach (BaseRoom room in biomeController.Rooms)
		{
			room.transform.SetParent(null, worldPositionStays: true);
		}
		yield break;
	}
}
