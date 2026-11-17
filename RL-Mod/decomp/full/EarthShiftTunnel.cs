using UnityEngine;

public class EarthShiftTunnel : Tunnel
{
	protected override void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		Tunnel destination = base.Destination;
		Room room = null;
		BiomeController biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(BiomeType.Garden) : OnPlayManager.BiomeController);
		foreach (Room room2 in biomeController.Rooms)
		{
			EndingSpawnRoomTypeController component = room2.GetComponent<EndingSpawnRoomTypeController>();
			if ((bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.Docks)
			{
				room = room2;
				break;
			}
		}
		if ((bool)room)
		{
			TunnelSpawnController tunnelSpawnController = room.gameObject.FindObjectReference<TunnelSpawnController>("ManorTunnel");
			m_tempDestinationOverride = tunnelSpawnController.Tunnel;
			RewiredMapController.SetIsInCutscene(inCutscene: true);
			CutsceneManager.InitializeCutscene(PlayerSaveFlag.None, destination);
			RoomSaveController.DisableCutsceneSaving = true;
			base.OnPlayerInteractedWithTunnel(otherObj);
		}
		else
		{
			base.OnPlayerInteractedWithTunnel(otherObj);
		}
	}
}
