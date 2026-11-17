using UnityEngine;

public class BiomeType_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private BiomeType m_type;

	[SerializeField]
	private bool m_isNot;

	private string m_description = string.Empty;

	private BiomeType m_previousType;

	private bool m_previousIsNot;

	public override string GizmoDescription
	{
		get
		{
			if (m_previousType != m_type || m_previousIsNot != m_isNot)
			{
				m_previousType = m_type;
				m_previousIsNot = m_isNot;
				m_description = string.Format("BiTy: {0}{1}", m_isNot ? "!" : "", m_type.ToString());
			}
			return m_description;
		}
	}

	public override SpawnScenarioType Type => SpawnScenarioType.BiomeType;

	public BiomeType BiomeType
	{
		get
		{
			return m_type;
		}
		set
		{
			m_type = value;
		}
	}

	public bool IsNot
	{
		get
		{
			return m_isNot;
		}
		set
		{
			m_isNot = value;
		}
	}

	public override void RunIsTrueCheck(BaseRoom room)
	{
		IsTrue = DoBiomeTypesMatch(room.AppearanceBiomeType);
	}

	public override void RunIsTrueCheck(GridPointManager gridPointManager)
	{
		if (gridPointManager.AppearanceOverride == BiomeType.None)
		{
			IsTrue = DoBiomeTypesMatch(gridPointManager.Biome);
		}
		else
		{
			IsTrue = DoBiomeTypesMatch(gridPointManager.AppearanceOverride);
		}
	}

	private bool DoBiomeTypesMatch(BiomeType currentBiome)
	{
		if (!m_isNot)
		{
			return currentBiome == m_type;
		}
		return currentBiome != m_type;
	}
}
