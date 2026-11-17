using UnityEngine;

public class EarthShiftEndHeirloomTunnel : Tunnel
{
	public override void ForceEnterTunnel(bool animate, Tunnel tempDestinationOverride = null)
	{
		BiomeController biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(BiomeType.Garden) : OnPlayManager.BiomeController);
		Room room = null;
		if ((bool)biomeController)
		{
			foreach (Room room2 in biomeController.Rooms)
			{
				EndingSpawnRoomTypeController component = room2.GetComponent<EndingSpawnRoomTypeController>();
				if ((bool)component && component.EndingSpawnRoomType == EndingSpawnRoomType.EarthShiftEscapeStart)
				{
					room = room2;
					break;
				}
			}
			if ((bool)room)
			{
				Tunnel tunnel = room.gameObject.FindObjectReference<TunnelSpawnController>("HeirloomTunnel").Tunnel;
				base.Destination = tunnel;
			}
		}
		if (!room)
		{
			Debug.Log("Could not find Heirloom Entrance room.");
		}
		base.ForceEnterTunnel(animate, tempDestinationOverride);
	}
}
