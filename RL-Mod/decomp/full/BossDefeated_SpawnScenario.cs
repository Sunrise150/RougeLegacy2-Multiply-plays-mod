using UnityEngine;

public class BossDefeated_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private bool m_spawnIfFalse;

	[SerializeField]
	private bool m_useBiomeBossID;

	[SerializeField]
	private BossID m_bossID;

	public bool SpawnIfFalse
	{
		get
		{
			return m_spawnIfFalse;
		}
		set
		{
			m_spawnIfFalse = value;
		}
	}

	public bool UseBossBiomeID
	{
		get
		{
			return m_useBiomeBossID;
		}
		set
		{
			m_useBiomeBossID = value;
		}
	}

	public BossID BossID
	{
		get
		{
			return m_bossID;
		}
		set
		{
			m_bossID = value;
		}
	}

	public override SpawnScenarioType Type => SpawnScenarioType.BossDefeated;

	public override string GizmoDescription
	{
		get
		{
			if (UseBossBiomeID)
			{
				return "BOSS DEFEATED - BIOME";
			}
			return "BOSS DEFEATED - CUSTOM";
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
		IsTrue = false;
		if (UseBossBiomeID)
		{
			BossID biomeToBossID = BossID_RL.GetBiomeToBossID(biome);
			IsTrue = BossID_RL.IsBossBeaten(biomeToBossID);
		}
		else
		{
			IsTrue = BossID_RL.IsBossBeaten(BossID);
		}
		if (SpawnIfFalse)
		{
			IsTrue = !IsTrue;
		}
	}
}
