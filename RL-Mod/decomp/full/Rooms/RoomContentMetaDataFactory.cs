using System;
using UnityEngine;

namespace Rooms;

public static class RoomContentMetaDataFactory
{
	public const string ASSET_PATH = "Assets/Content/Scriptable Objects/Room Content";

	public const string RESOURCE_PATH = "Scriptable Objects/Room Content";

	public static string CreateContentMetaData(RoomMetaData room)
	{
		Room prefab = room.GetPrefab();
		string text = room.ID.ToString() + "_RoomContent";
		if (prefab != null)
		{
			SpawnLogicController[] componentsInChildren = prefab.GetComponentsInChildren<SpawnLogicController>(includeInactive: true);
			RoomContentMetaData roomContentMetaData = new RoomContentMetaData();
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				RoomContentType roomContentType = RoomContentType.Other;
				EnemySpawnController component = componentsInChildren[i].GetComponent<EnemySpawnController>();
				if (component != null)
				{
					roomContentType = RoomContentType.Enemy;
				}
				ChestSpawnController component2 = componentsInChildren[i].GetComponent<ChestSpawnController>();
				if (component2 != null)
				{
					roomContentType = RoomContentType.Chest;
				}
				if (component != null && component2 != null)
				{
					throw new Exception($"A GameObject in Room ({room.ID}) has both an Enemy and Chest Spawn Controller Component attached. This is not valid.");
				}
				if (roomContentType != RoomContentType.Other)
				{
					Vector2 localPosition = componentsInChildren[i].transform.localPosition;
					roomContentMetaData.AddEntry(roomContentType, localPosition, componentsInChildren[i].SpawnConditions);
				}
			}
			GlobalTeleporterController[] componentsInChildren2 = prefab.GetComponentsInChildren<GlobalTeleporterController>(includeInactive: true);
			for (int j = 0; j < componentsInChildren2.Length; j++)
			{
				Vector2 localPosition2 = componentsInChildren2[j].transform.localPosition;
				roomContentMetaData.AddEntry(RoomContentType.Teleporter, localPosition2, new SpawnConditionsEntry[0]);
			}
		}
		return "Scriptable Objects/Room Content/" + room.ID.SceneName + "/" + text;
	}
}
