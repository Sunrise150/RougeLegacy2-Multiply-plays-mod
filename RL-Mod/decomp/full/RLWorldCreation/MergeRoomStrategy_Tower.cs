using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RLWorldCreation;

public class MergeRoomStrategy_Tower : MergeRoomStrategy
{
	public override IEnumerator Run(CreateMergeRooms_BuildStage buildStage, BiomeController biomeController)
	{
		m_buildStage = buildStage;
		MergeTopOfTower(biomeController);
		if (BiomeCreation_EV.USE_DEFAULT_MERGE_LOGIC_FOR_TOWER_INTERIOR)
		{
			List<GridPointManager> potentialMergeTargets_V = GetPotentialMergeTargets_V2(biomeController, BiomeType.Tower, RoomType.BossEntrance, RoomType.Fairy, RoomType.Transition, RoomType.Bonus);
			yield return MergeRooms_V2(biomeController, biomeController.Biome, potentialMergeTargets_V);
			MergeRoomTools.MergeConnectedGridPointManagers(biomeController, BiomeType.TowerExterior);
		}
		else
		{
			OriginalTowerMergeLogic(biomeController);
		}
		MergeRoomStrategy.UpdateBiomeControllerIndices(biomeController);
	}

	private void OriginalTowerMergeLogic(BiomeController biomeController)
	{
		MergeRoomTools.MergeConnectedGridPointManagers(biomeController, BiomeType.Tower);
		MergeRoomTools.MergeConnectedGridPointManagers(biomeController, BiomeType.TowerExterior);
	}

	private void MergeTopOfTower(BiomeController biomeController)
	{
		List<GridPointManager> list = biomeController.GridPointManager.GridPointManagers.Where((GridPointManager room) => room.RoomType == RoomType.BossEntrance).ToList();
		if (list != null && list.Count > 1)
		{
			MergeRoomTools.MergeGridPointManagers(biomeController, list);
			return;
		}
		Debug.LogFormat("<color=red>| {0} | Failed to merge top of Tower biome because we were unable to find the top of the Tower biome</color>", this);
	}
}
