using System;
using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class InstantiateRooms_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		Stopwatch timer = new Stopwatch();
		timer.Start();
		foreach (GridPointManager gridPointManager in biomeController.GridPointManager.GridPointManagers)
		{
			if (gridPointManager.RoomMetaData.GetPrefab() == null)
			{
				throw new Exception("Prefab for RoomMetaData: " + gridPointManager.RoomMetaData.ID.ToString() + " is null");
			}
			Room room = CreateRoomInstance(biomeController, gridPointManager);
			if (gridPointManager.RoomType == RoomType.Transition)
			{
				biomeController.SetTransitionRoom(room);
			}
			biomeController.StandardRoomCreatedByWorldBuilder(room);
			int biomeControllerIndex = gridPointManager.BiomeControllerIndex;
			room.SetBiomeControllerIndex(biomeControllerIndex);
			if (timer.Elapsed.TotalMilliseconds >= 30.0)
			{
				yield return null;
				timer.Restart();
			}
		}
	}

	public static Room CreateRoomInstance(BiomeController biomeController, GridPointManager gridPointManager)
	{
		Room room = null;
		if (!gridPointManager.IsRoomMirrored)
		{
			room = UnityEngine.Object.Instantiate(gridPointManager.RoomMetaData.GetPrefab(), biomeController.StandardRoomStorageLocation).GetComponent<Room>();
		}
		else
		{
			room = RoomUtility.CreateMirrorVersionOfRoom(gridPointManager.RoomMetaData.GetPrefab());
			room.gameObject.transform.SetParent(biomeController.StandardRoomStorageLocation);
		}
		room.MoveToCoordinates(GridController.GetRoomCoordinatesFromGridCoordinates(gridPointManager.GridCoordinates));
		room.gameObject.name = gridPointManager.RoomNumber + "*" + gridPointManager.RoomType.ToString() + "*";
		room.BiomeType = biomeController.Biome;
		room.Initialise(gridPointManager);
		return room;
	}
}
