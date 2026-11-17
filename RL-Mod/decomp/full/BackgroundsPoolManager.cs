using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BackgroundsPoolManager : MonoBehaviour
{
	private Dictionary<Background, GenericPool_RL<Background>> m_backgroundsTable;

	private static BackgroundsPoolManager Instance { get; set; }

	public static bool IsInitialized { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			Initialize();
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	private void Initialize()
	{
		m_backgroundsTable = new Dictionary<Background, GenericPool_RL<Background>>();
		IsInitialized = true;
	}

	private GenericPool_RL<Background> CreatePool(Background backgroundPrefab, int poolSize)
	{
		GenericPool_RL<Background> genericPool_RL = new GenericPool_RL<Background>();
		genericPool_RL.Initialize(backgroundPrefab, poolSize, disablePooling: false);
		return genericPool_RL;
	}

	private void CreateBackgroundPools_Internal(BiomeType biomeType)
	{
		Dictionary<Background, int> dictionary = new Dictionary<Background, int>();
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeType) : OnPlayManager.BiomeController);
		if (biomeController != null)
		{
			Dictionary<Background, int> dictionary2 = new Dictionary<Background, int>();
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
				if (item.Backgrounds == null)
				{
					continue;
				}
				dictionary2.Clear();
				BackgroundPoolEntry[] backgrounds = item.Backgrounds;
				foreach (BackgroundPoolEntry backgroundPoolEntry in backgrounds)
				{
					if ((bool)backgroundPoolEntry.BackgroundPrefab)
					{
						if (dictionary2.ContainsKey(backgroundPoolEntry.BackgroundPrefab))
						{
							dictionary2[backgroundPoolEntry.BackgroundPrefab]++;
						}
						else
						{
							dictionary2.Add(backgroundPoolEntry.BackgroundPrefab, 1);
						}
					}
				}
				foreach (KeyValuePair<Background, int> item2 in dictionary2)
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
		List<Background> list2 = m_backgroundsTable.Keys.ToList();
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<Background, int> item3 in dictionary)
		{
			if ((bool)item3.Key)
			{
				list2.Remove(item3.Key);
				if (!m_backgroundsTable.ContainsKey(item3.Key))
				{
					m_backgroundsTable.Add(item3.Key, CreatePool(item3.Key, item3.Value));
					num2++;
				}
				else
				{
					m_backgroundsTable[item3.Key].ResizePool(item3.Value);
					num++;
				}
			}
		}
		foreach (Background item4 in list2)
		{
			if (m_backgroundsTable.ContainsKey(item4))
			{
				m_backgroundsTable[item4].DestroyPool();
				m_backgroundsTable.Remove(item4);
			}
		}
	}

	public static void CreateBackgroundPools(BiomeType biomeType)
	{
		Instance.CreateBackgroundPools_Internal(biomeType);
	}

	public static Background GetBackground(Background bgPrefab)
	{
		return Instance.GetBackground_Internal(bgPrefab);
	}

	private Background GetBackground_Internal(Background bgPrefab)
	{
		if (m_backgroundsTable.ContainsKey(bgPrefab))
		{
			return m_backgroundsTable[bgPrefab].GetFreeObj();
		}
		Debug.LogFormat("<color=red>| BackgroundsPoolManager | Background Table does not contain an entry for Background named ({0})</color>", bgPrefab.name);
		return null;
	}

	public static void DestroyPools()
	{
		foreach (KeyValuePair<Background, GenericPool_RL<Background>> item in Instance.m_backgroundsTable)
		{
			item.Value.DestroyPool();
		}
		Instance.m_backgroundsTable.Clear();
	}

	private void OnDestroy()
	{
		DestroyPools();
		Instance = null;
		IsInitialized = false;
	}
}
