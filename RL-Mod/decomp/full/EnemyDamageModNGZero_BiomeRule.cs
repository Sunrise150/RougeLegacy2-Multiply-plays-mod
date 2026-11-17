using System.Collections;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Biome Rules/Gameplay Mods/Enemy Damage Mod NG0")]
public class EnemyDamageModNGZero_BiomeRule : GameplayMod_BiomeRule
{
	[SerializeField]
	private float m_damageMod = 1f;

	public static float StrengthMod { get; private set; } = 1f;


	public override GameplayModID ID => GameplayModID.ChangeEnemyHP_NGZero;

	public override BiomeRuleExecutionTime ExecutionTime => BiomeRuleExecutionTime.PlayerEnterBiome;

	public override IEnumerator RunRule(BiomeType biome)
	{
		if (SaveManager.PlayerSaveData.NewGamePlusLevel == 0)
		{
			StrengthMod = m_damageMod;
		}
		else
		{
			StrengthMod = 1f;
		}
		yield break;
	}

	public override void UndoRule(BiomeType biome)
	{
		StrengthMod = 1f;
	}
}
