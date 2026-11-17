using System;
using System.Linq;
using UnityEngine;

[Serializable]
public class RoomContentEntry
{
	[SerializeField]
	private RoomContentType m_contentType;

	[SerializeField]
	private Vector2 m_localPosition;

	[SerializeField]
	private SpawnConditionsEntry[] m_spawnConditions;

	public RoomContentType ContentType => m_contentType;

	public Vector2 LocalPosition => m_localPosition;

	public SpawnConditionsEntry[] SpawnConditions => m_spawnConditions;

	public RoomContentEntry(RoomContentType contentType, Vector2 localPosition, SpawnConditionsEntry[] spawnConditions)
	{
		m_contentType = contentType;
		m_localPosition = localPosition;
		m_spawnConditions = spawnConditions;
	}

	public bool GetIsSpawned(GridPointManager gridPointManager)
	{
		SpawnConditionsEntry[] spawnConditions = SpawnConditions;
		foreach (SpawnConditionsEntry spawnConditionsEntry in spawnConditions)
		{
			if (spawnConditionsEntry.ScenarioData == null)
			{
				continue;
			}
			spawnConditionsEntry.Scenarios = new SpawnScenarioEntry[spawnConditionsEntry.ScenarioData.Length];
			for (int j = 0; j < spawnConditionsEntry.ScenarioData.Length; j++)
			{
				Type type = Type.GetType(spawnConditionsEntry.ScenarioData[j].DataType);
				if (type != null)
				{
					if (spawnConditionsEntry.Scenarios[j] == null)
					{
						spawnConditionsEntry.Scenarios[j] = new SpawnScenarioEntry();
					}
					SpawnScenario scenario = (SpawnScenario)JsonUtility.FromJson(spawnConditionsEntry.ScenarioData[j].Data, type);
					spawnConditionsEntry.Scenarios[j].Scenario = scenario;
				}
			}
		}
		bool result = true;
		if (SpawnConditions != null && SpawnConditions.Length != 0)
		{
			spawnConditions = SpawnConditions;
			foreach (SpawnConditionsEntry spawnConditionsEntry2 in spawnConditions)
			{
				if (spawnConditionsEntry2.Scenarios == null)
				{
					continue;
				}
				SpawnScenarioEntry[] scenarios = spawnConditionsEntry2.Scenarios;
				foreach (SpawnScenarioEntry spawnScenarioEntry in scenarios)
				{
					if (spawnScenarioEntry.Scenario != null)
					{
						spawnScenarioEntry.Scenario.RunIsTrueCheck(gridPointManager);
					}
				}
			}
			result = SpawnConditions.Any((SpawnConditionsEntry condition) => condition.GetIsTrue());
		}
		return result;
	}
}
