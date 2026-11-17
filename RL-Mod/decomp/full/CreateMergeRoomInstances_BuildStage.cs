using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CreateMergeRoomInstances_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		List<int> list = new List<int>();
		foreach (GridPointManager gridPointManager in biomeController.GridPointManager.GridPointManagers)
		{
			if (gridPointManager.MergeWithGridPointManagers.Count > 0 && !list.Contains(gridPointManager.BiomeControllerIndex))
			{
				list.Add(gridPointManager.BiomeControllerIndex);
			}
		}
		foreach (int biomeControllerIndex in list)
		{
			List<Room> connectedRooms = biomeController.StandaloneRooms.Where((Room entry) => entry.BiomeControllerIndex == biomeControllerIndex).ToList();
			MergeRoomTools.MergeRooms(biomeController, connectedRooms).SetBiomeControllerIndex(biomeControllerIndex);
		}
		yield break;
	}
}
