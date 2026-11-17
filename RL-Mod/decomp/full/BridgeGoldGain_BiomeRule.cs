using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Gameplay Mods/Bridge Gold Gain")]
public class BridgeGoldGain_BiomeRule : GameplayMod_BiomeRule
{
	public static float GoldGainMod { get; private set; }

	public override GameplayModID ID => GameplayModID.ChangeGoldGain;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	public override IEnumerator RunRule(BiomeType biome)
	{
		GoldGainMod = 0.2f;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
		if (biome != BiomeType.Stone)
		{
			Debug.Log("<color=yellow>WARNING: The Bridge biome gold gain mod is being applied to a biome other than the Bridge.</color>");
		}
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		GoldGainMod = 0f;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, null, null);
	}
}
