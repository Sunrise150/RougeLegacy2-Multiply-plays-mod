using System.Collections;
using UnityEngine;

public class CloseDoors_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		if (biomeController.StandaloneRooms != null)
		{
			foreach (Room standaloneRoom in biomeController.StandaloneRooms)
			{
				BiomeCreatorTools.CloseUnconnectedDoorsInRoom(standaloneRoom);
			}
			yield break;
		}
		Debug.LogFormat("<color=red>| {0} | {1} Biome's Biome Controller's list of standalone Rooms is null.</color>", this, biomeController.Biome);
	}
}
