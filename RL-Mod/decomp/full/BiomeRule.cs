using System.Collections;
using UnityEngine;

public abstract class BiomeRule : ScriptableObject
{
	public abstract BiomeRuleExecutionTime ExecutionTime { get; }

	public abstract IEnumerator RunRule(BiomeType biome);

	public abstract void UndoRule(BiomeType biome);
}
