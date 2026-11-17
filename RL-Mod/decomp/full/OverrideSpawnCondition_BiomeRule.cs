using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Override Spawn Condition")]
public class OverrideSpawnCondition_BiomeRule : BiomeRule
{
	[SerializeField]
	private SpawnConditionOverrideID m_condition = SpawnConditionOverrideID.Dash;

	[SerializeField]
	private bool m_value = true;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.WorldCreationComplete;

	private SpawnConditionOverride GetSpawnConditionOverride(BiomeType biome)
	{
		SpawnConditionOverride spawnConditionOverride = BiomeRuleManager.GetSpawnConditionOverride(biome);
		if (spawnConditionOverride == null)
		{
			Debug.LogFormat("<color=red>| {0} | No SpawnConditionOverride found for {1} Biome. If you see this message, please add a bug report to pivotal along with the Stack Trace</color>", this, biome);
		}
		return spawnConditionOverride;
	}

	public override IEnumerator RunRule(BiomeType biome)
	{
		SpawnConditionOverride spawnConditionOverride = GetSpawnConditionOverride(biome);
		if (spawnConditionOverride != null)
		{
			SetOverrideIsOn(spawnConditionOverride, isOn: true);
			spawnConditionOverride.SetOverrideValue(m_condition, m_value);
		}
		yield break;
	}

	private void SetOverrideIsOn(SpawnConditionOverride spawnConditionOverride, bool isOn)
	{
		spawnConditionOverride.SetIsOverrideOn(m_condition, isOn);
	}

	public override void UndoRule(BiomeType biome)
	{
		SpawnConditionOverride spawnConditionOverride = GetSpawnConditionOverride(biome);
		if (spawnConditionOverride != null)
		{
			SetOverrideIsOn(spawnConditionOverride, isOn: false);
		}
	}
}
