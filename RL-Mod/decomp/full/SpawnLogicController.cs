using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SpawnLogicController : MonoBehaviour, ISerializationCallbackReceiver
{
	[SerializeField]
	private SpawnConditionsEntry[] m_spawnConditions = new SpawnConditionsEntry[0];

	private const int TEXT_SIZE = 10;

	private BaseRoom m_room;

	public bool ShouldSpawn { get; private set; }

	public SpawnConditionsEntry[] SpawnConditions
	{
		get
		{
			return m_spawnConditions;
		}
		private set
		{
			m_spawnConditions = value;
		}
	}

	public BaseRoom Room => m_room;

	public void SetRoom(BaseRoom value)
	{
		m_room = value;
	}

	private void Start()
	{
		if (SpawnConditions == null)
		{
			return;
		}
		SpawnConditionsEntry[] spawnConditions = SpawnConditions;
		foreach (SpawnConditionsEntry spawnConditionsEntry in spawnConditions)
		{
			if (spawnConditionsEntry.Scenarios == null)
			{
				continue;
			}
			SpawnScenarioEntry[] scenarios = spawnConditionsEntry.Scenarios;
			foreach (SpawnScenarioEntry spawnScenarioEntry in scenarios)
			{
				if (spawnScenarioEntry.Scenario != null)
				{
					spawnScenarioEntry.Scenario.Start();
				}
			}
		}
	}

	private void OnDrawGizmos()
	{
	}

	public void RunIsSpawnedCheck(SpawnScenarioCheckStage checkStage = SpawnScenarioCheckStage.PreMerge)
	{
		ShouldSpawn = true;
		if (SpawnConditions == null || SpawnConditions.Length == 0)
		{
			return;
		}
		SpawnConditionsEntry[] spawnConditions = SpawnConditions;
		foreach (SpawnConditionsEntry spawnConditionsEntry in spawnConditions)
		{
			if (spawnConditionsEntry.Scenarios == null)
			{
				continue;
			}
			SpawnScenarioEntry[] scenarios = spawnConditionsEntry.Scenarios;
			foreach (SpawnScenarioEntry spawnScenarioEntry in scenarios)
			{
				if (spawnScenarioEntry.Scenario != null && spawnScenarioEntry.Scenario.CheckStage == checkStage)
				{
					spawnScenarioEntry.Scenario.RunIsTrueCheck(Room);
				}
			}
		}
		ShouldSpawn = SpawnConditions.Any((SpawnConditionsEntry condition) => condition.GetIsTrue());
	}

	public bool GetIsSpawnedBasedOnConditions()
	{
		return ShouldSpawn;
	}

	public SpawnConditionsEntry AddCondition()
	{
		return AddCondition(new SpawnConditionsEntry());
	}

	public SpawnConditionsEntry AddCondition(SpawnConditionsEntry condition)
	{
		if (Application.isPlaying)
		{
			return null;
		}
		List<SpawnConditionsEntry> list = SpawnConditions.ToList();
		list.Add(condition);
		SpawnConditions = list.ToArray();
		return condition;
	}

	public void DeleteCondition(SpawnConditionsEntry condition)
	{
		List<SpawnConditionsEntry> list = SpawnConditions.ToList();
		if (list.Remove(condition))
		{
			SpawnConditions = list.ToArray();
		}
	}

	public void OnBeforeSerialize()
	{
		SpawnConditionsEntry[] spawnConditions = SpawnConditions;
		foreach (SpawnConditionsEntry spawnConditionsEntry in spawnConditions)
		{
			if (spawnConditionsEntry.Scenarios == null)
			{
				continue;
			}
			if (spawnConditionsEntry.ScenarioData == null || spawnConditionsEntry.ScenarioData.Length != spawnConditionsEntry.Scenarios.Length)
			{
				spawnConditionsEntry.ScenarioData = new ScenarioData[spawnConditionsEntry.Scenarios.Length];
			}
			for (int j = 0; j < spawnConditionsEntry.Scenarios.Length; j++)
			{
				string dataType = "";
				string data = "";
				if (spawnConditionsEntry.Scenarios[j].Scenario != null)
				{
					dataType = spawnConditionsEntry.Scenarios[j].Scenario.GetTypeAsString();
					data = spawnConditionsEntry.Scenarios[j].Scenario.GetDataAsString();
				}
				spawnConditionsEntry.ScenarioData[j].DataType = dataType;
				spawnConditionsEntry.ScenarioData[j].Data = data;
			}
		}
	}

	public void OnAfterDeserialize()
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
	}
}
