using System.Collections.Generic;
using UnityEngine;

public static class NPC_EV
{
	public const float TELEPORTER_NG_COST_MULTIPLIER = 2.5f;

	public const float TELEPORTER_NG_COST_EXPONENTIAL = 250f;

	public const float ARCHITECT_BASE_PRICE_MOD = 0.1f;

	public const float ARCHITECT_GOLD_REDUCTION_MOD = 0.2f;

	public static Dictionary<BiomeType, int> PIZZA_GIRL_TELEPORTER_COST_TABLE = new Dictionary<BiomeType, int>
	{
		{
			BiomeType.Stone,
			1750
		},
		{
			BiomeType.Study,
			1750
		},
		{
			BiomeType.Cave,
			1750
		},
		{
			BiomeType.Tower,
			1750
		},
		{
			BiomeType.Forest,
			1750
		}
	};

	public static float GetArchitectGoldMod(int timesCastleLocked = -1)
	{
		if (timesCastleLocked == -1)
		{
			timesCastleLocked = SaveManager.PlayerSaveData.TimesCastleLocked;
		}
		float architectCostMods = SkillTreeLogicHelper.GetArchitectCostMods();
		if (timesCastleLocked >= 1)
		{
			return Mathf.Clamp(1f - (0.1f + (float)timesCastleLocked * (0.2f - architectCostMods)), 0f, 1f);
		}
		return 1f;
	}
}
