using System.Collections;
using System.Diagnostics;
using UnityEngine;

public class SetSpawnTypes_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		Stopwatch timer = new Stopwatch();
		timer.Start();
		if (biomeController.Rooms != null && biomeController.Rooms.Count > 0)
		{
			foreach (BaseRoom room2 in biomeController.Rooms)
			{
				Room room = room2 as Room;
				if (room != null)
				{
					SetSpawnTypes(room);
				}
				else
				{
					MergeRoom spawnTypes = room2 as MergeRoom;
					SetSpawnTypes(spawnTypes);
				}
				if (timer.Elapsed.TotalMilliseconds >= 30.0)
				{
					yield return null;
					timer.Restart();
				}
			}
		}
		else
		{
			UnityEngine.Debug.LogFormat("<color=red>[{0}] BiomeController ({1})'s Rooms Property is null or empty.</color>", this, biomeController.Biome);
		}
	}

	private void SetSpawnTypes(Room room)
	{
		if (room.GridPointManager == null)
		{
			return;
		}
		RNGManager.SetSeed(RngID.Prop_RoomSeed, room.GridPointManager.PropSeed);
		RNGManager.SetSeed(RngID.Deco_RoomSeed, room.GridPointManager.DecoSeed);
		RNGManager.SetSeed(RngID.Enemy_RoomSeed, room.GridPointManager.EnemySeed);
		RNGManager.SetSeed(RngID.Chest_RoomSeed, room.GridPointManager.ChestSeed);
		RNGManager.SetSeed(RngID.Hazards_RoomSeed, room.GridPointManager.HazardSeed);
		ISpawnController[] spawnControllers = room.SpawnControllerManager.SpawnControllers;
		for (int i = 0; i < spawnControllers.Length; i++)
		{
			if (spawnControllers[i] is ISetSpawnType setSpawnType)
			{
				setSpawnType.SetSpawnType();
			}
		}
		room.RoomEnemyManager.SetSpawnType();
	}

	private void SetSpawnTypes(MergeRoom room)
	{
		for (int i = 0; i < room.StandaloneGridPointManagers.Length; i++)
		{
			if (room.StandaloneGridPointManagers[i] == null)
			{
				continue;
			}
			GridPointManager gridPointManager = room.StandaloneGridPointManagers[i];
			RNGManager.SetSeed(RngID.Prop_RoomSeed, gridPointManager.PropSeed);
			RNGManager.SetSeed(RngID.Deco_RoomSeed, gridPointManager.DecoSeed);
			RNGManager.SetSeed(RngID.Enemy_RoomSeed, gridPointManager.EnemySeed);
			RNGManager.SetSeed(RngID.Chest_RoomSeed, gridPointManager.ChestSeed);
			RNGManager.SetSeed(RngID.Hazards_RoomSeed, gridPointManager.HazardSeed);
			ISpawnController[] spawnControllers = room.SpawnControllerManager.SpawnControllers;
			for (int j = 0; j < spawnControllers.Length; j++)
			{
				if (spawnControllers[j] is ISetSpawnType setSpawnType)
				{
					setSpawnType.SetSpawnType();
				}
			}
			RoomEnemyManager[] standaloneRoomEnemyManagers = room.StandaloneRoomEnemyManagers;
			for (int j = 0; j < standaloneRoomEnemyManagers.Length; j++)
			{
				standaloneRoomEnemyManagers[j].SetSpawnType();
			}
		}
	}
}
