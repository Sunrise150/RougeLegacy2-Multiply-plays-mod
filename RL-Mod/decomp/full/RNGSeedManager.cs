using System;
using System.Collections.Generic;
using System.Globalization;
using SceneManagement_RL;
using UnityEngine;

[CreateAssetMenu(menuName = "Custom/Data/RNG Seed Manager")]
public class RNGSeedManager : ScriptableObject
{
	[SerializeField]
	private string m_projectSeed = "";

	private static RNGSeedManager m_instance = null;

	private static int m_masterSeedOverride = -1;

	private static Dictionary<string, int> m_sceneToCurrentSeedTable = new Dictionary<string, int>();

	private static Dictionary<string, int> m_sceneToPreviousSeedTable = new Dictionary<string, int>();

	private static Queue<int> m_previousMasterSeedOverrides = new Queue<int>(5);

	public const string RESOURCES_PATH = "Scriptable Objects/RNGSeedManager";

	private const string ASSET_PATH = "Assets/Content/Scriptable Objects/RNGSeedManager.asset";

	private const string EDITOR_PREFS_SEED_KEY = "Temp Seed (HEX)";

	private const string WORLD_SCENE_NAME = "World";

	public static int MasterSeedOverride
	{
		get
		{
			return m_masterSeedOverride;
		}
		private set
		{
			if (PreviousMasterSeedOverrides.Count == 5)
			{
				PreviousMasterSeedOverrides.Dequeue();
			}
			PreviousMasterSeedOverrides.Enqueue(value);
			m_masterSeedOverride = value;
		}
	}

	public static string CurrentWorldSeed => GetSeedAsHex(GetCurrentSeed("World"));

	private static RNGSeedManager Instance
	{
		get
		{
			if (m_instance == null && Application.isPlaying)
			{
				m_instance = CDGResources.Load<RNGSeedManager>("Scriptable Objects/RNGSeedManager");
			}
			return m_instance;
		}
	}

	public static bool IsMasterSeedOverrideSet => MasterSeedOverride != -1;

	public static bool IsOverrideSet
	{
		get
		{
			if (Application.isEditor && IsTempSeedSet)
			{
				return true;
			}
			if (!IsProjectSeedSet)
			{
				return IsMasterSeedOverrideSet;
			}
			return true;
		}
	}

	public static bool IsProjectSeedSet => !string.IsNullOrEmpty(ProjectSeed);

	public static bool IsTempSeedSet
	{
		get
		{
			if (Application.isEditor)
			{
				return !string.IsNullOrEmpty(TempSeed);
			}
			return false;
		}
	}

	public static string ProjectSeed
	{
		get
		{
			return Instance.m_projectSeed;
		}
		private set
		{
			Instance.m_projectSeed = value;
		}
	}

	public static string TempSeed
	{
		get
		{
			string empty = string.Empty;
			_ = Application.isEditor;
			return empty;
		}
		private set
		{
		}
	}

	public static Queue<int> PreviousMasterSeedOverrides
	{
		get
		{
			return m_previousMasterSeedOverrides;
		}
		private set
		{
			m_previousMasterSeedOverrides = value;
		}
	}

	private void OnEnable()
	{
		SceneLoader_RL.SceneLoadingEndRelay.AddListener(OnSceneLoaded);
	}

	private void OnSceneLoaded(string sceneName)
	{
		if (SceneLoadingUtility.GetSceneID(sceneName) == SceneID.MainMenu || SceneLoadingUtility.GetSceneID(sceneName) == SceneID.Tutorial)
		{
			ClearOverride();
		}
	}

	public static void GenerateNewSeed(string sceneName)
	{
		if (m_sceneToPreviousSeedTable.ContainsKey(sceneName))
		{
			m_sceneToPreviousSeedTable[sceneName] = m_sceneToCurrentSeedTable[sceneName];
		}
		else
		{
			int value = -1;
			if (m_sceneToCurrentSeedTable.ContainsKey(sceneName))
			{
				value = m_sceneToCurrentSeedTable[sceneName];
			}
			m_sceneToPreviousSeedTable.Add(sceneName, value);
		}
		if (SceneLoadingUtility.ActiveScene.name == "World" && IsOverrideSet)
		{
			_ = string.Empty;
			if (IsTempSeedSet)
			{
				m_sceneToCurrentSeedTable[sceneName] = GetSeedAsInt(TempSeed);
			}
			else if (IsProjectSeedSet)
			{
				m_sceneToCurrentSeedTable[sceneName] = GetSeedAsInt(ProjectSeed);
			}
			else if (IsMasterSeedOverrideSet && sceneName.Equals("World"))
			{
				m_sceneToCurrentSeedTable[sceneName] = MasterSeedOverride;
			}
		}
		else
		{
			m_sceneToCurrentSeedTable[sceneName] = Environment.TickCount;
		}
	}

	public static int GetCurrentSeed(string sceneName)
	{
		int result = -1;
		if (m_sceneToCurrentSeedTable.ContainsKey(sceneName))
		{
			result = m_sceneToCurrentSeedTable[sceneName];
		}
		return result;
	}

	public static int GetPreviousSeed(string sceneName)
	{
		if (m_sceneToPreviousSeedTable.ContainsKey(sceneName))
		{
			return m_sceneToPreviousSeedTable[sceneName];
		}
		return -1;
	}

	public static string GetSeedAsHex(int seed)
	{
		return string.Format("{0}", seed.ToString("X"));
	}

	private static int GetSeedAsInt(string seed)
	{
		try
		{
			return int.Parse(seed, NumberStyles.HexNumber);
		}
		catch (Exception)
		{
			return -1;
		}
	}

	public static bool IsSeedValid(string seed)
	{
		if (!string.IsNullOrEmpty(seed))
		{
			try
			{
				int.Parse(seed, NumberStyles.HexNumber);
			}
			catch (Exception)
			{
				return false;
			}
		}
		return true;
	}

	public static void SetMasterSeedOverride(int seed)
	{
		MasterSeedOverride = seed;
		if (SaveManager.IsInitialized)
		{
			SaveManager.PlayerSaveData.PreviousMasterSeed = SaveManager.PlayerSaveData.MasterSeed;
			SaveManager.PlayerSaveData.MasterSeed = seed;
		}
	}

	public static void SetProjectSeed(string seed)
	{
	}

	public static void SetTempSeed(string seed)
	{
		TempSeed = seed;
	}

	public static void ClearOverride()
	{
		MasterSeedOverride = -1;
		TempSeed = string.Empty;
		ProjectSeed = string.Empty;
	}
}
