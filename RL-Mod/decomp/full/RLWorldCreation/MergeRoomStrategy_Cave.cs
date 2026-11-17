using System.Collections;

namespace RLWorldCreation;

public class MergeRoomStrategy_Cave : MergeRoomStrategy
{
	public IEnumerator Run(BiomeController biomeController)
	{
		MergeTopOfCave(biomeController);
		yield break;
	}

	private void MergeTopOfCave(BiomeController biomeController)
	{
		MergeRoomTools.MergeConnectedGridPointManagers(biomeController, BiomeType.Cave);
	}
}
