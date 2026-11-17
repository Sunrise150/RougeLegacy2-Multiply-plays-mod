using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Override Terminal Velocity")]
public class OverrideTerminalVelocity_BiomeRule : BiomeRule
{
	[SerializeField]
	private float m_terminalVelocity = -10f;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	public override IEnumerator RunRule(BiomeType biome)
	{
		Global_EV.SetTerminalVelocityOverride(isOverrideSet: true, m_terminalVelocity);
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		Global_EV.SetTerminalVelocityOverride(isOverrideSet: false);
	}
}
