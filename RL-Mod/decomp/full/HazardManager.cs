using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HazardManager : MonoBehaviour
{
	private const int CULLING_GROUP_SIZE = 100;

	[SerializeField]
	private int m_poolSize = 5;

	private static HazardManager m_instance;

	private static Dictionary<HazardType, GenericPool_RL<Hazard>> m_hazardTable;

	private CullingGroup m_cullingGroup;

	private BoundingSphere[] m_cullingSpheres;

	private List<IHazard> m_cullingHazardList = new List<IHazard>();

	private Action<MonoBehaviour, EventArgs> m_refreshCullingGroup;

	public const string RESOURCES_PATH = "Prefabs/Managers/HazardManager";

	private static HazardManager Instance
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
			Debug.Log("<color=green>Creating Hazard Manager...</color>");
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (IsInitialized)
		{
			DisableAllHazards();
		}
	}

	private void OnDestroy()
	{
		DestroyPools();
		m_instance = null;
		m_hazardTable = null;
		IsInitialized = false;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		m_cullingGroup.Dispose();
		m_cullingGroup = null;
		Array.Clear(m_cullingSpheres, 0, m_cullingSpheres.Length);
		m_cullingSpheres = null;
		m_cullingHazardList.Clear();
		m_cullingHazardList = null;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_refreshCullingGroup);
	}

	private void Initialize()
	{
		m_hazardTable = new Dictionary<HazardType, GenericPool_RL<Hazard>>();
		m_refreshCullingGroup = RefreshCullingGroup;
		IsInitialized = true;
	}

	private IEnumerator Start()
	{
		while (!CameraController.IsInstantiated)
		{
			yield return null;
		}
		InitializeCullingGroup();
	}

	private void InitializeCullingGroup()
	{
		m_cullingGroup = new CullingGroup();
		m_cullingSpheres = new BoundingSphere[100];
		for (int i = 0; i < m_cullingSpheres.Length; i++)
		{
			m_cullingSpheres[i] = new BoundingSphere(Vector3.zero, 55f);
		}
		m_cullingGroup.SetBoundingSpheres(m_cullingSpheres);
		m_cullingGroup.SetBoundingSphereCount(100);
		m_cullingGroup.targetCamera = CameraController.GameCamera;
		CullingGroup cullingGroup = m_cullingGroup;
		cullingGroup.onStateChanged = (CullingGroup.StateChanged)Delegate.Combine(cullingGroup.onStateChanged, new CullingGroup.StateChanged(CullingSphereStateChanged));
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_refreshCullingGroup);
	}

	private void RefreshCullingGroup(object sender, EventArgs args)
	{
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		m_cullingHazardList.Clear();
		IHazardSpawnController[] hazardSpawnControllers = currentPlayerRoom.SpawnControllerManager.HazardSpawnControllers;
		foreach (IHazardSpawnController hazardSpawnController in hazardSpawnControllers)
		{
			if (hazardSpawnController.ShouldSpawn && hazardSpawnController.Hazard != null)
			{
				m_cullingHazardList.Add(hazardSpawnController.Hazard);
			}
		}
		if (m_cullingHazardList.Count > 100)
		{
			throw new Exception("Culling group size for hazards is " + 100 + ".  Please increase HazardManager.CULLING_GROUP_SIZE to fix this error.");
		}
		m_cullingGroup.SetBoundingSphereCount(m_cullingHazardList.Count);
		for (int j = 0; j < m_cullingHazardList.Count; j++)
		{
			IHazard hazard = m_cullingHazardList[j];
			m_cullingSpheres[j].position = hazard.gameObject.transform.localPosition;
			hazard.SetIsCulled(enabled: false);
		}
		StartCoroutine(UpdateStartingCullStatesCoroutine());
	}

	private IEnumerator UpdateStartingCullStatesCoroutine()
	{
		yield return null;
		for (int i = 0; i < m_cullingHazardList.Count; i++)
		{
			if (!m_cullingGroup.IsVisible(i))
			{
				m_cullingHazardList[i].SetIsCulled(enabled: true);
			}
		}
	}

	private void CullingSphereStateChanged(CullingGroupEvent evt)
	{
		if (m_cullingHazardList.Count <= evt.index)
		{
			return;
		}
		IHazard hazard = m_cullingHazardList[evt.index];
		if (hazard != null)
		{
			if (evt.hasBecomeVisible)
			{
				hazard.SetIsCulled(enabled: false);
			}
			else if (evt.hasBecomeInvisible)
			{
				hazard.SetIsCulled(enabled: true);
			}
		}
	}

	private void Internal_CreateBiomePools(BiomeType biomeType)
	{
		Dictionary<HazardType, int> dictionary = new Dictionary<HazardType, int>();
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeType) : OnPlayManager.BiomeController);
		if ((bool)biomeController)
		{
			Dictionary<HazardType, int> dictionary2 = new Dictionary<HazardType, int>();
			foreach (BaseRoom room in biomeController.Rooms)
			{
				dictionary2.Clear();
				ISpawnController[] spawnControllers = room.SpawnControllerManager.SpawnControllers;
				foreach (ISpawnController spawnController in spawnControllers)
				{
					if (!(spawnController is IHazardSpawnController { Type: var type } hazardSpawnController))
					{
						continue;
					}
					if ((bool)(spawnController as Ferr2DHazardSpawnController))
					{
						if (!dictionary2.ContainsKey(type))
						{
							dictionary2.Add(type, 1);
						}
						continue;
					}
					LineHazardSpawnController lineHazardSpawnController = spawnController as LineHazardSpawnController;
					int num = 1;
					if (lineHazardSpawnController != null)
					{
						num = lineHazardSpawnController.Width;
						HazardType lineHazardType = lineHazardSpawnController.GetLineHazardType(type);
						if (dictionary2.ContainsKey(lineHazardType))
						{
							dictionary2[lineHazardType]++;
						}
						else
						{
							dictionary2.Add(lineHazardType, 1);
						}
					}
					else
					{
						if (hazardSpawnController.Type == HazardType.Triple_Orbiter)
						{
							if (dictionary2.ContainsKey(HazardType.Orbiter))
							{
								dictionary2[HazardType.Orbiter] += 3;
							}
							else
							{
								dictionary2.Add(HazardType.Orbiter, 3);
							}
						}
						if (hazardSpawnController.Type == HazardType.SentryWithIce)
						{
							if (dictionary2.ContainsKey(HazardType.Sentry))
							{
								dictionary2[HazardType.Sentry]++;
							}
							else
							{
								dictionary2.Add(HazardType.Sentry, 1);
							}
							if (dictionary2.ContainsKey(HazardType.IceCrystal))
							{
								dictionary2[HazardType.IceCrystal]++;
							}
							else
							{
								dictionary2.Add(HazardType.IceCrystal, 1);
							}
						}
					}
					if (dictionary2.ContainsKey(type))
					{
						dictionary2[type] += num;
					}
					else
					{
						dictionary2.Add(type, num);
					}
				}
				foreach (KeyValuePair<HazardType, int> item in dictionary2)
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
		List<HazardType> list = m_hazardTable.Keys.ToList();
		int num2 = 0;
		int num3 = 0;
		foreach (KeyValuePair<HazardType, int> item2 in dictionary)
		{
			if (item2.Key != HazardType.None)
			{
				list.Remove(item2.Key);
				if (!m_hazardTable.ContainsKey(item2.Key))
				{
					Hazard prefab = HazardLibrary.GetPrefab(item2.Key);
					m_hazardTable.Add(item2.Key, CreatePool(prefab, item2.Value));
					num3++;
				}
				else
				{
					m_hazardTable[item2.Key].ResizePool(item2.Value);
					num2++;
				}
			}
		}
		foreach (HazardType item3 in list)
		{
			if (m_hazardTable.ContainsKey(item3))
			{
				m_hazardTable[item3].DestroyPool();
				m_hazardTable.Remove(item3);
			}
		}
	}

	private GenericPool_RL<Hazard> CreatePool(Hazard prefab, int poolSize)
	{
		if (!prefab)
		{
			return null;
		}
		GenericPool_RL<Hazard> genericPool_RL = new GenericPool_RL<Hazard>();
		if (prefab is IFerr2DHazard)
		{
			genericPool_RL.Initialize(prefab, poolSize, disablePooling: true);
		}
		else
		{
			genericPool_RL.Initialize(prefab, poolSize, disablePooling: false);
		}
		return genericPool_RL;
	}

	public static IHazard GetHazard(HazardType hazardType)
	{
		if (m_hazardTable.ContainsKey(hazardType))
		{
			return m_hazardTable[hazardType].GetFreeObj();
		}
		Debug.LogFormat("<color=red>| HazardManager | Hazard Object Pool Table does not contain an entry for Hazard Type ({0})</color>", hazardType);
		return null;
	}

	public static void DestroyPools()
	{
		foreach (KeyValuePair<HazardType, GenericPool_RL<Hazard>> item in m_hazardTable)
		{
			item.Value.DestroyPool();
		}
		m_hazardTable.Clear();
	}

	public static void CreateBiomePools(BiomeType biome)
	{
		Instance.Internal_CreateBiomePools(biome);
	}

	public static void DisableAllHazards()
	{
		foreach (KeyValuePair<HazardType, GenericPool_RL<Hazard>> item in m_hazardTable)
		{
			item.Value.DisableAll();
		}
	}
}
