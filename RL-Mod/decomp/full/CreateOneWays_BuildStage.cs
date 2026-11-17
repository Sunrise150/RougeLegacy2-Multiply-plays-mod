using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateOneWays_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		foreach (Room standaloneRoom in biomeController.StandaloneRooms)
		{
			if (BiomeCreation_EV.DO_NOT_ADD_ONE_WAYS_TO_BOTTOM_DOORS.Contains(standaloneRoom.AppearanceBiomeType) || standaloneRoom.DisableOneWaysAtBottomDoors)
			{
				continue;
			}
			if (standaloneRoom != null)
			{
				List<Door> doorsOnSide = standaloneRoom.GetDoorsOnSide(RoomSide.Bottom);
				if (doorsOnSide == null || doorsOnSide.Count <= 0)
				{
					continue;
				}
				foreach (Door item in standaloneRoom.GetDoorsOnSide(RoomSide.Bottom))
				{
					BiomeCreatorTools.PlaceOneWayAtDoor(item);
				}
			}
			else
			{
				Debug.LogFormat("<color=red>{0}: One of Biome Controller's ({1}) Standalone Rooms is null", Time.frameCount, biomeController.gameObject.name);
			}
		}
		yield break;
	}
}
