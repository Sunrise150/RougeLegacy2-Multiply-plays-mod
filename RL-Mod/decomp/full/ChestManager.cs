using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChestManager : MonoBehaviour
{
	private const int CULLING_GROUP_SIZE = 100;

	[SerializeField]
	private int m_poolSize = 5;

	private static ChestManager m_instance;

	private static Dictionary<ChestType, GenericPool_RL<ChestObj>> m_chestTable;

	public const string RESOURCES_PATH = "Prefabs/Managers/HazardManager";

	private static ChestManager Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			Initialize();
			SceneManager.sceneLoaded += OnSceneLoaded;
			Debug.Log("<color=green>Creating Chest Manager...</color>");
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (IsInitialized)
		{
			DisableAllChests();
		}
	}

	private void OnDestroy()
	{
		DestroyPools();
		m_instance = null;
		m_chestTable = null;
		IsInitialized = false;
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void Initialize()
	{
		m_chestTable = new Dictionary<ChestType, GenericPool_RL<ChestObj>>();
		IsInitialized = true;
	}

	private void Internal_CreateBiomePools(BiomeType biomeType)
	{
		Dictionary<ChestType, int> dictionary = new Dictionary<ChestType, int>();
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeType) : OnPlayManager.BiomeController);
		if (biomeController != null)
		{
			Dictionary<ChestType, int> dictionary2 = new Dictionary<ChestType, int>();
			foreach (BaseRoom room in biomeController.Rooms)
			{
				dictionary2.Clear();
				ChestSpawnController[] chestSpawnControllers = room.SpawnControllerManager.ChestSpawnControllers;
				foreach (ChestSpawnController chestSpawnController in chestSpawnControllers)
				{
					if (dictionary2.ContainsKey(chestSpawnController.ChestType))
					{
						dictionary2[chestSpawnController.ChestType]++;
					}
					else
					{
						dictionary2.Add(chestSpawnController.ChestType, 1);
					}
				}
				foreach (KeyValuePair<ChestType, int> item in dictionary2)
				{
					if (dictionary.ContainsKey(item.Key))
					{
						if (dictionary[item.Key] < dictionary2[item.Key])
						{
							dictionary[item.Key] = dictionary2[item.Key];
						}
					}
					else
					{
						dictionary.Add(item.Key, item.Value);
					}
				}
			}
		}
		List<ChestType> list = m_chestTable.Keys.ToList();
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<ChestType, int> item2 in dictionary)
		{
			if (item2.Key != 0)
			{
				list.Remove(item2.Key);
				if (!m_chestTable.ContainsKey(item2.Key))
				{
					ChestObj chestPrefab = ChestLibrary.GetChestPrefab(item2.Key);
					m_chestTable.Add(item2.Key, CreatePool(chestPrefab, item2.Value));
					num2++;
				}
				else
				{
					m_chestTable[item2.Key].ResizePool(item2.Value);
					num++;
				}
			}
		}
		foreach (ChestType item3 in list)
		{
			if (m_chestTable.ContainsKey(item3))
			{
				m_chestTable[item3].DestroyPool();
				m_chestTable.Remove(item3);
			}
		}
	}

	private GenericPool_RL<ChestObj> CreatePool(ChestObj prefab, int poolSize)
	{
		if (!prefab)
		{
			return null;
		}
		GenericPool_RL<ChestObj> genericPool_RL = new GenericPool_RL<ChestObj>();
		genericPool_RL.Initialize(prefab, poolSize, disablePooling: false);
		return genericPool_RL;
	}

	public static ChestObj GetChest(ChestType chestType)
	{
		if (m_chestTable.ContainsKey(chestType))
		{
			return m_chestTable[chestType].GetFreeObj();
		}
		Debug.LogFormat("<color=red>| ChestManager | Chest Object Pool Table does not contain an entry for Chest Type ({0})</color>", chestType);
		return null;
	}

	public static void DestroyPools()
	{
		foreach (KeyValuePair<ChestType, GenericPool_RL<ChestObj>> item in m_chestTable)
		{
			item.Value.DestroyPool();
		}
		m_chestTable.Clear();
	}

	public static void CreateBiomePools(BiomeType biome)
	{
		Instance.Internal_CreateBiomePools(biome);
	}

	public static void DisableAllChests()
	{
		foreach (KeyValuePair<ChestType, GenericPool_RL<ChestObj>> item in m_chestTable)
		{
			item.Value.DisableAll();
		}
	}
}
