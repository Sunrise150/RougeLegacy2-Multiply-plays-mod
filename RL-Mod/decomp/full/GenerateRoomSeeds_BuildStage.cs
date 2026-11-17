using System.Collections;
using UnityEngine;

public class GenerateRoomSeeds_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		if (biomeController.GridPointManager != null && biomeController.GridPointManager.GridPointManagers != null && biomeController.GridPointManager.GridPointManagers.Count > 0)
		{
			foreach (GridPointManager gridPointManager in biomeController.GridPointManager.GridPointManagers)
			{
				int randomNumber = RNGManager.GetRandomNumber(RngID.Room_RNGSeed, "GenerateRoomSeeds_BuildStage.Run()", 0, int.MaxValue);
				gridPointManager.SetRoomSeed(randomNumber);
			}
			yield break;
		}
		Debug.LogFormat("<color=red>[{0}] BiomeController ({1})'s GridPointManager Property is null or empty.</color>", this, biomeController.Biome);
	}
}
