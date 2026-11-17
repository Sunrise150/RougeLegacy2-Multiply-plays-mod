using System.Collections;
using System.Collections.Generic;

public class CreateRoomBackgrounds_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		foreach (Room standaloneRoom in biomeController.StandaloneRooms)
		{
			Room room = standaloneRoom as Room;
			if ((bool)room)
			{
				List<GridPointManager> mergeWithGridPointManagers = room.GridPointManager.MergeWithGridPointManagers;
				if (mergeWithGridPointManagers != null && mergeWithGridPointManagers.Count > 0)
				{
					bool flag = true;
					foreach (GridPointManager item in mergeWithGridPointManagers)
					{
						if (item.RoomNumber < room.GridPointManager.RoomNumber)
						{
							flag = false;
							break;
						}
					}
					if (!flag)
					{
						continue;
					}
				}
			}
			BiomeCreatorTools.CreateBackground(standaloneRoom);
		}
		yield break;
	}
}
