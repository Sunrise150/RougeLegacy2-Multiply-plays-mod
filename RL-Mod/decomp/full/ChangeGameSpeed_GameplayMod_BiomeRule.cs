using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Gameplay Mods/Change Game Speed")]
public class ChangeGameSpeed_GameplayMod_BiomeRule : GameplayMod_BiomeRule
{
	[SerializeField]
	private float m_speedMultiplier = 1f;

	public override GameplayModID ID => GameplayModID.ChangeGameSpeed;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (m_speedMultiplier <= 0f)
		{
			Debug.LogFormat("<color=red>|{0}| Speed Multiplier is set to ({1}). Its value must be greater than 0. Setting it to 1.</color>", this, m_speedMultiplier);
			m_speedMultiplier = 1f;
		}
		RLTimeScale.SetTimeScale(TimeScaleType.Game, m_speedMultiplier);
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		RLTimeScale.SetTimeScale(TimeScaleType.Game, 1f);
	}
}
