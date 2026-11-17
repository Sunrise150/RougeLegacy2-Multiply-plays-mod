using UnityEngine;

public class HasDash_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private bool m_hasDash;

	public bool HasDash
	{
		get
		{
			return m_hasDash;
		}
		set
		{
			m_hasDash = value;
		}
	}

	public override SpawnScenarioType Type => SpawnScenarioType.HasDash;

	public override string GizmoDescription
	{
		get
		{
			if (m_hasDash)
			{
				return "DASH";
			}
			return "NO DASH";
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
		if (spawnConditionOverride.OverrideHasDash)
		{
			IsTrue = HasDash == spawnConditionOverride.HasDash;
		}
		else if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (HasDash)
			{
				IsTrue = playerController.CharacterDash.TotalDashesAllowed > 0;
			}
			else
			{
				IsTrue = playerController.CharacterDash.TotalDashesAllowed <= 0;
			}
		}
	}
}
