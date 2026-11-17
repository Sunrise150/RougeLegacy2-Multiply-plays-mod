using UnityEngine;

[CreateAssetMenu(menuName = "Custom/XML Processing Behaviours/Enemy Data (Test)")]
public class EnemyData : ScriptableObject
{
	public float SummonValue;

	public int Health;

	public int WeaponDamage;

	public int MagicDamage;

	public float Speed;

	public float TurnSpeed;

	public float RestMinCooldown;

	public float RestMaxCooldown;

	public bool IsFlying;

	public bool AlwaysFace;

	public bool FallLedge;

	public bool CollidesWithPlatforms;

	public int StunDefence;

	public int KnockbackDefence;

	public float StunDuration;

	public float KnockbackModX;

	public float KnockbackModY;

	public float Scale;

	public float AnimationSpeed;

	public int GoldMin;

	public int GoldMax;

	public float DropOdds;

	public float MeleeRadius;

	public float ProjectileRadius;

	public float FarRadius;

	[Space]
	[Header("Spawn in Biome")]
	[Space]
	public bool SpawnInCastle;

	public bool SpawnInCave;

	public bool SpawnInDragon;

	public bool SpawnInForest;

	public bool SpawnInGarden;

	public bool SpawnInHome;

	public bool SpawnInLake;

	public bool SpawnInStone;

	public bool SpawnInStudy;

	public bool SpawnInSunken;

	public bool SpawnInTower;

	public bool SpawnInTowerExterior;

	public bool SpawnInTown;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string MoveList;

	public string Description01;

	public string Description02;

	public void SetSpawnInBiome(BiomeType biome, bool value)
	{
		switch (biome)
		{
		case BiomeType.Editor:
			SpawnInCastle = value;
			break;
		case BiomeType.Castle:
			SpawnInCastle = value;
			break;
		case BiomeType.Cave:
			SpawnInCave = value;
			break;
		case BiomeType.Dragon:
			SpawnInDragon = value;
			break;
		case BiomeType.Forest:
			SpawnInForest = value;
			break;
		case BiomeType.Garden:
			SpawnInGarden = value;
			break;
		case BiomeType.DriftHouse:
			SpawnInHome = value;
			break;
		case BiomeType.Lake:
			SpawnInLake = value;
			break;
		case BiomeType.Stone:
			SpawnInStone = value;
			break;
		case BiomeType.Study:
			SpawnInStudy = value;
			break;
		case BiomeType.Sunken:
			SpawnInSunken = value;
			break;
		case BiomeType.Tower:
			SpawnInTower = value;
			break;
		case BiomeType.TowerExterior:
			SpawnInTowerExterior = value;
			break;
		case BiomeType.Town:
			SpawnInTown = value;
			break;
		}
	}

	public bool GetSpawnInBiome(BiomeType biome)
	{
		return biome switch
		{
			BiomeType.Editor => SpawnInCastle, 
			BiomeType.Castle => SpawnInCastle, 
			BiomeType.Cave => SpawnInCave, 
			BiomeType.Dragon => SpawnInDragon, 
			BiomeType.Forest => SpawnInForest, 
			BiomeType.Garden => SpawnInGarden, 
			BiomeType.DriftHouse => SpawnInHome, 
			BiomeType.Lake => SpawnInLake, 
			BiomeType.Stone => SpawnInStone, 
			BiomeType.Study => SpawnInStudy, 
			BiomeType.Sunken => SpawnInSunken, 
			BiomeType.Tower => SpawnInTower, 
			BiomeType.TowerExterior => SpawnInTowerExterior, 
			BiomeType.Town => SpawnInTown, 
			_ => false, 
		};
	}
}
