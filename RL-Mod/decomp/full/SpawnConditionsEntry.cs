using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class SpawnConditionsEntry
{
	[SerializeField]
	private ScenarioData[] m_scenarioData;

	private SpawnScenarioEntry[] m_scenarios;

	public bool IsTrue { get; set; }

	public SpawnScenarioEntry[] Scenarios
	{
		get
		{
			return m_scenarios;
		}
		set
		{
			m_scenarios = value;
		}
	}

	public ScenarioData[] ScenarioData
	{
		get
		{
			return m_scenarioData;
		}
		set
		{
			m_scenarioData = value;
		}
	}

	public SpawnConditionsEntry()
	{
		AddScenario();
	}

	public void AddScenario()
	{
		List<SpawnScenarioEntry> list = ((Scenarios != null) ? Scenarios.ToList() : new List<SpawnScenarioEntry>());
		list.Add(new SpawnScenarioEntry());
		Scenarios = list.ToArray();
	}

	public void DeleteScenario(SpawnScenarioEntry scenarioEntry)
	{
		List<SpawnScenarioEntry> list = Scenarios.ToList();
		if (list.Remove(scenarioEntry))
		{
			Scenarios = list.ToArray();
		}
	}

	public bool GetIsTrue()
	{
		if (Scenarios != null && Scenarios.Length != 0)
		{
			return Scenarios.All((SpawnScenarioEntry entry) => entry.Scenario.IsTrue);
		}
		return false;
	}
}
