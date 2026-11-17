using System;
using System.Collections.Generic;
using UnityEngine;

public class BiomeRuleManager : MonoBehaviour
{
	[SerializeField]
	private List<BiomeRuleManagerEntry> m_biomeRules;

	private static Dictionary<BiomeType, SpawnConditionOverride> m_spawnConditionOverrideTable = new Dictionary<BiomeType, SpawnConditionOverride>();

	private Action<MonoBehaviour, EventArgs> m_onBiomeEnter;

	private Action<MonoBehaviour, EventArgs> m_onBiomeExit;

	private Action<MonoBehaviour, EventArgs> m_onWorldCreationComplete;

	private Action<MonoBehaviour, EventArgs> m_onBiomeCreationComplete;

	private Action<MonoBehaviour, EventArgs> m_onLevelEditorWorldCreationComplete;

	private List<BiomeRule> m_ruleHelper_STATIC = new List<BiomeRule>();

	private void Awake()
	{
		m_onBiomeEnter = OnBiomeEnter;
		m_onBiomeExit = OnBiomeExit;
		m_onWorldCreationComplete = OnWorldCreationComplete;
		m_onBiomeCreationComplete = OnBiomeCreationComplete;
		m_onLevelEditorWorldCreationComplete = OnLevelEditorWorldCreationComplete;
		foreach (BiomeRuleManagerEntry biomeRule in m_biomeRules)
		{
			for (int i = 0; i < biomeRule.Rules.Length; i++)
			{
				biomeRule.Rules[i] = UnityEngine.Object.Instantiate(biomeRule.Rules[i]);
			}
		}
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeEnter, m_onBiomeEnter);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeExit, m_onBiomeExit);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeCreationComplete, m_onBiomeCreationComplete);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelEditorWorldCreationComplete, m_onLevelEditorWorldCreationComplete);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeEnter, m_onBiomeEnter);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeExit, m_onBiomeExit);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeCreationComplete, m_onBiomeCreationComplete);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelEditorWorldCreationComplete, m_onLevelEditorWorldCreationComplete);
		ResetAllRules();
	}

	private void OnLevelEditorWorldCreationComplete(MonoBehaviour sender, EventArgs args)
	{
		LevelEditorWorldCreationCompleteEventArgs levelEditorWorldCreationCompleteEventArgs = args as LevelEditorWorldCreationCompleteEventArgs;
		if (levelEditorWorldCreationCompleteEventArgs != null)
		{
			RunRules_V2(levelEditorWorldCreationCompleteEventArgs.BuiltRoom.AppearanceBiomeType, BiomeRuleExecutionTime.WorldCreationComplete);
			return;
		}
		Debug.LogFormat("<color=red>|{0}| Failed to cast args as ({1})</color>", this, levelEditorWorldCreationCompleteEventArgs.GetType());
	}

	private void OnWorldCreationComplete(MonoBehaviour sender, EventArgs args)
	{
		WorldBuildCompleteEventArgs worldBuildCompleteEventArgs = args as WorldBuildCompleteEventArgs;
		if (worldBuildCompleteEventArgs != null)
		{
			foreach (BiomeController biomeController in worldBuildCompleteEventArgs.BiomeControllers)
			{
				RunRules_V2(biomeController.Biome, BiomeRuleExecutionTime.WorldCreationComplete);
			}
			return;
		}
		Debug.LogFormat("<color=red>|{0}| Failed to cast args as ({1})</color>", this, worldBuildCompleteEventArgs.GetType());
	}

	private void OnBiomeCreationComplete(MonoBehaviour sender, EventArgs args)
	{
		BiomeEventArgs biomeEventArgs = args as BiomeEventArgs;
		if (biomeEventArgs != null)
		{
			BiomeType biome = biomeEventArgs.Biome;
			RunRules_V2(biome, BiomeRuleExecutionTime.BiomeCreationComplete);
		}
		else
		{
			Debug.LogFormat("<color=red>|{0}| Failed to cast args as ({1})</color>", this, biomeEventArgs.GetType());
		}
	}

	private void OnBiomeEnter(MonoBehaviour sender, EventArgs args)
	{
		ResetAllRules();
		if (args is BiomeEventArgs biomeEventArgs)
		{
			RunRules_V2(biomeEventArgs.Biome, BiomeRuleExecutionTime.PlayerEnterBiome);
			return;
		}
		Debug.LogFormat("<color=red>|{0}| Failed to cast args as BiomeEventArgs</color>", this);
	}

	private void OnBiomeExit(MonoBehaviour sender, EventArgs args)
	{
		if (args is BiomeEventArgs biomeEventArgs)
		{
			UndoRules_V2(biomeEventArgs.Biome, BiomeRuleExecutionTime.PlayerEnterBiome);
			return;
		}
		Debug.LogFormat("<color=red>|{0}| Failed to cast args as BiomeEventArgs</color>", this);
	}

	private List<BiomeRule> GetRules_V2(BiomeType biome, BiomeRuleExecutionTime executionTime)
	{
		m_ruleHelper_STATIC.Clear();
		BiomeRuleManagerEntry biomeRuleManagerEntry = null;
		foreach (BiomeRuleManagerEntry biomeRule2 in m_biomeRules)
		{
			if (biomeRule2.Biome == biome)
			{
				if (biomeRuleManagerEntry != null)
				{
					Debug.Log("<color=red>ERROR: Multiple entries for the same BiomeType: " + biome.ToString() + " found in BiomeRuleManager.</color>");
					break;
				}
				biomeRuleManagerEntry = biomeRule2;
			}
		}
		if (biomeRuleManagerEntry != null)
		{
			BiomeRule[] rules = biomeRuleManagerEntry.Rules;
			foreach (BiomeRule biomeRule in rules)
			{
				if (biomeRule.ExecutionTime == executionTime)
				{
					m_ruleHelper_STATIC.Add(biomeRule);
				}
			}
		}
		return m_ruleHelper_STATIC;
	}

	public static SpawnConditionOverride GetSpawnConditionOverride(BiomeType biome)
	{
		if (!m_spawnConditionOverrideTable.ContainsKey(biome))
		{
			m_spawnConditionOverrideTable.Add(biome, new SpawnConditionOverride(biome));
		}
		return m_spawnConditionOverrideTable[biome];
	}

	private void RunRules_V2(BiomeType biomeType, BiomeRuleExecutionTime executionTime)
	{
		foreach (BiomeRule item in GetRules_V2(biomeType, executionTime))
		{
			StartCoroutine(item.RunRule(biomeType));
		}
		if (biomeType == BiomeType.Tower)
		{
			RunRules_V2(BiomeType.TowerExterior, executionTime);
		}
	}

	private void UndoRules_V2(BiomeType biomeType, BiomeRuleExecutionTime executionTime)
	{
		foreach (BiomeRule item in GetRules_V2(biomeType, executionTime))
		{
			item.UndoRule(biomeType);
		}
		if (biomeType == BiomeType.Tower)
		{
			UndoRules_V2(BiomeType.TowerExterior, executionTime);
		}
	}

	private void ResetAllRules()
	{
		foreach (BiomeRuleManagerEntry biomeRule in m_biomeRules)
		{
			BiomeRule[] rules = biomeRule.Rules;
			for (int i = 0; i < rules.Length; i++)
			{
				rules[i].UndoRule(biomeRule.Biome);
			}
		}
	}
}
