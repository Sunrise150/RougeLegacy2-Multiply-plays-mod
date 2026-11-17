using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Gameplay Mods/Enemy HP Mod NG0")]
public class EnemyHPModNGZero_BiomeRule : GameplayMod_BiomeRule
{
	[SerializeField]
	private float m_maxHealthMod = 1f;

	public static float MaxHealthMod { get; private set; } = 1f;


	public override GameplayModID ID => GameplayModID.ChangeEnemyHP_NGZero;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (SaveManager.PlayerSaveData.NewGamePlusLevel == 0)
		{
			MaxHealthMod = m_maxHealthMod;
		}
		else
		{
			MaxHealthMod = 1f;
		}
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		MaxHealthMod = 1f;
	}
}
