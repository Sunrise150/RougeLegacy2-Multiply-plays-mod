using UnityEngine;

public class HasJump_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private bool m_hasDoubleJump;

	public bool HasDoubleJump
	{
		get
		{
			return m_hasDoubleJump;
		}
		set
		{
			m_hasDoubleJump = value;
		}
	}

	public override SpawnScenarioType Type => SpawnScenarioType.HasJump;

	public override string GizmoDescription
	{
		get
		{
			if (m_hasDoubleJump)
			{
				return "JUMP";
			}
			return "NO JUMP";
		}
	}

	public override void RunIsTrueCheck(BaseRoom room)
	{
		RunIsTrueCheck(room.AppearanceBiomeType);
	}

	public override void RunIsTrueCheck(GridPointManager gridPointManager)
	{
		RunIsTrueCheck(gridPointManager.Biome);
	}

	private void RunIsTrueCheck(BiomeType biome)
	{
		SpawnConditionOverride spawnConditionOverride = BiomeRuleManager.GetSpawnConditionOverride(biome);
		if (spawnConditionOverride.OverrideHasDoubleJump)
		{
			IsTrue = HasDoubleJump == spawnConditionOverride.HasDoubleJump;
		}
		else if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (HasDoubleJump)
			{
				IsTrue = playerController.CharacterJump.NumberOfJumps > 1;
			}
			else
			{
				IsTrue = playerController.CharacterJump.NumberOfJumps <= 1;
			}
		}
	}
}
