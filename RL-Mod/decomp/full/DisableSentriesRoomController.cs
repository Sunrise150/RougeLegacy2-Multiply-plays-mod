using System;
using UnityEngine;

public class DisableSentriesRoomController : MonoBehaviour, IRoomConsumer
{
	public BaseRoom Room { get; private set; }

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		Room.PlayerEnterRelay.AddListener(OnPlayerEnter);
	}

	private void OnDestroy()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(OnPlayerEnter);
		}
	}

	private void OnPlayerEnter(object sender, EventArgs args)
	{
		ISpawnController[] spawnControllers = Room.SpawnControllerManager.SpawnControllers;
		for (int i = 0; i < spawnControllers.Length; i++)
		{
			IHazardSpawnController hazardSpawnController = spawnControllers[i] as IHazardSpawnController;
			if (!hazardSpawnController.IsNativeNull() && hazardSpawnController.Type == HazardType.Sentry)
			{
				hazardSpawnController.Hazard.gameObject.SetActive(value: false);
			}
		}
	}
}
