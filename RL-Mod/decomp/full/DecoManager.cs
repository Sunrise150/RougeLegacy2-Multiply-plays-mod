using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DecoManager : MonoBehaviour
{
	[SerializeField]
	private int m_poolSize = 5;

	private static DecoManager m_instance;

	private static Dictionary<int, GenericPool_RL<Deco>> m_decoTable;

	private static DecoManager Instance
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
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void OnDestroy()
	{
		DestroyPools();
		m_instance = null;
		m_decoTable = null;
	}

	private GenericPool_RL<Deco> CreatePool(Deco prefab, int poolSize)
	{
		if (!prefab)
		{
			return null;
		}
		GenericPool_RL<Deco> genericPool_RL = new GenericPool_RL<Deco>();
		genericPool_RL.Initialize(prefab, poolSize, disablePooling: false);
		return genericPool_RL;
	}

	public static Deco GetDeco(Deco deco)
	{
		return GetDeco(deco.NameHash);
	}

	public static Deco GetDeco(int decoNameHash)
	{
		if (m_decoTable.ContainsKey(decoNameHash))
		{
			return m_decoTable[decoNameHash].GetFreeObj();
		}
		Debug.LogFormat("<color=red>| DecoManager | Deco Object Pool Table does not contain an entry for Deco named ({0})</color>", decoNameHash);
		return null;
	}

	private void Initialize()
	{
		m_decoTable = new Dictionary<int, GenericPool_RL<Deco>>();
		IsInitialized = true;
	}

	private void Internal_CreateBiomePools(BiomeType biomeType)
	{
		Dictionary<Deco, int> dictionary = new Dictionary<Deco, int>();
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeType) : OnPlayManager.BiomeController);
		if (biomeController != null)
		{
			Dictionary<Deco, int> dictionary2 = new Dictionary<Deco, int>();
			List<BaseRoom> list = new List<BaseRoom>();
			list.AddRange(biomeController.Rooms);
			if (biomeType != BiomeType.Castle)
			{
				BiomeController biomeController2 = (GameUtility.IsInLevelEditor ? OnPlayManager.BiomeController : WorldBuilder.GetBiomeController(BiomeType.Castle));
				BaseRoom baseRoom = ((biomeController2 != null) ? biomeController2.TransitionRoom : null);
				if (baseRoom != null)
				{
					list.Add(baseRoom);
				}
			}
			foreach (BaseRoom item in list)
			{
				dictionary2.Clear();
				PropSpawnController[] propSpawnControllers = item.SpawnControllerManager.PropSpawnControllers;
				foreach (PropSpawnController propSpawnController in propSpawnControllers)
				{
					Prop propPrefab = propSpawnController.PropPrefab;
					if (!propPrefab)
					{
						continue;
					}
					DecoController[] prefabDecoControllers = PropSpawnController.GetPrefabDecoControllers(propPrefab);
					DecoSpawnData[][] decoSpawnData = propSpawnController.DecoSpawnData;
					int num = prefabDecoControllers.Length;
					for (int j = 0; j < num; j++)
					{
						DecoController decoController = prefabDecoControllers[j];
						int num2 = decoController.DecoLocations.Length;
						for (int k = 0; k < num2; k++)
						{
							DecoLocation decoLocation = decoController.DecoLocations[k];
							DecoSpawnData decoSpawnData2 = decoSpawnData[j][k];
							if (decoSpawnData2.ShouldSpawn)
							{
								Deco deco = decoLocation.PotentialDecos[decoSpawnData2.DecoPropIndex].Deco;
								if (dictionary2.ContainsKey(deco))
								{
									dictionary2[deco]++;
								}
								else
								{
									dictionary2.Add(deco, 1);
								}
							}
						}
					}
				}
				foreach (KeyValuePair<Deco, int> item2 in dictionary2)
				{
					if (dictionary.ContainsKey(item2.Key))
					{
						if (dictionary[item2.Key] < dictionary2[item2.Key])
						{
							dictionary[item2.Key] = dictionary2[item2.Key];
						}
					}
					else
					{
						dictionary.Add(item2.Key, item2.Value);
					}
				}
			}
		}
		List<int> list2 = m_decoTable.Keys.ToList();
		int num3 = 0;
		int num4 = 0;
		foreach (KeyValuePair<Deco, int> item3 in dictionary)
		{
			if (!item3.Key)
			{
				continue;
			}
			int nameHash = item3.Key.NameHash;
			list2.Remove(nameHash);
			if (!m_decoTable.ContainsKey(nameHash))
			{
				m_decoTable.Add(nameHash, CreatePool(item3.Key, item3.Value));
				foreach (Deco @object in m_decoTable[nameHash].ObjectList)
				{
					@object.NameHash = nameHash;
				}
				num4++;
			}
			else
			{
				m_decoTable[nameHash].ResizePool(item3.Value);
				num3++;
			}
		}
		foreach (int item4 in list2)
		{
			if (m_decoTable.ContainsKey(item4))
			{
				m_decoTable[item4].DestroyPool();
				m_decoTable.Remove(item4);
			}
		}
	}

	public static void DestroyPools()
	{
		foreach (KeyValuePair<int, GenericPool_RL<Deco>> item in m_decoTable)
		{
			item.Value.DestroyPool();
		}
		m_decoTable.Clear();
	}

	public static void CreateBiomePools(BiomeType biome)
	{
		Instance.Internal_CreateBiomePools(biome);
	}

	public static void DisableAllDecos()
	{
		foreach (KeyValuePair<int, GenericPool_RL<Deco>> item in m_decoTable)
		{
			item.Value.DisableAll();
		}
	}
}
