using System.Collections.Generic;
using UnityEngine;

public class BiomeHasTag_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private BiomeTag m_tag;

	private string m_description = string.Empty;

	private BiomeTag m_previousTag;

	public override string GizmoDescription
	{
		get
		{
			if (m_previousTag != m_tag)
			{
				m_previousTag = m_tag;
				m_description = $"BiT: {m_tag.ToString()}";
			}
			return m_description;
		}
	}

	public override SpawnScenarioType Type => SpawnScenarioType.BiomeHasTag;

	public BiomeTag Tag
	{
		get
		{
			return m_tag;
		}
		set
		{
			m_tag = value;
		}
	}

	public override void RunIsTrueCheck(BaseRoom room)
	{
		IsTrue = GetHasTag(room.AppearanceBiomeType);
	}

	public override void RunIsTrueCheck(GridPointManager gridPointManager)
	{
		IsTrue = GetHasTag(gridPointManager.Biome);
	}

	private bool GetHasTag(BiomeType biome)
	{
		bool result = false;
		if (BiomeCreation_EV.BIOME_TAGS.ContainsKey(biome))
		{
			List<BiomeTag> list = BiomeCreation_EV.BIOME_TAGS[biome];
			if (list != null)
			{
				for (int i = 0; i < list.Count; i++)
				{
					if (list[i] == Tag)
					{
						result = true;
					}
				}
			}
		}
		return result;
	}
}
