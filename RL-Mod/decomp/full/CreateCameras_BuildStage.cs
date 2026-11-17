using System.Collections;
using UnityEngine;

public class CreateCameras_BuildStage : IBiomeBuildStage
{
	public IEnumerator Run(BiomeController biomeController)
	{
		if (biomeController.Rooms != null && biomeController.Rooms.Count > 0)
		{
			foreach (BaseRoom room in biomeController.Rooms)
			{
				biomeController.CinemachineVirtualCameraCreatedByWorldBuilder(BiomeCreatorTools.CreateCinemachineVirtualCamera(room));
			}
			yield break;
		}
		Debug.LogFormat("<color=red>[{0}] BiomeController ({1})'s Rooms Property is null or empty.</color>", this, biomeController.Biome);
	}
}
