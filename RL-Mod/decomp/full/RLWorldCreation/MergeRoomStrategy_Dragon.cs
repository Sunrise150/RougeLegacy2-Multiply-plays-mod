using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RLWorldCreation;

public class MergeRoomStrategy_Dragon : MergeRoomStrategy
{
	public IEnumerator Run(BiomeController biomeController)
	{
		List<GridPointManager> connectedRooms = biomeController.GridPointManager.GridPointManagers.Where((GridPointManager room) => room.RoomMetaData.CanMerge).ToList();
		MergeRoomTools.MergeGridPointManagers(biomeController, connectedRooms);
		yield break;
	}
}
