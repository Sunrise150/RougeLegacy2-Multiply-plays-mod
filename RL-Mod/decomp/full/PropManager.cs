using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PropManager : MonoBehaviour
{
	private const int DEFAULT_CULLING_GROUP_SIZE = 3000;

	private const float CULLING_DEACTIVATION_RADIUS = 16f;

	[SerializeField]
	private int m_poolSize = 5;

	private CullingGroup m_cullingGroup;

	private BoundingSphere[] m_cullingSpheres;

	private static PropManager m_instance;

	private static Dictionary<int, GenericPool_RL<Prop>> m_propTable;

	private List<Prop> m_cullingPropList = new List<Prop>();

	private Dictionary<Prop, int> m_cullingGravityPropTable = new Dictionary<Prop, int>();

	private int m_highestPropCount;

	private int m_cullingGroupSize;

	private bool m_cullingGroupsInitialized;

	private Action<MonoBehaviour, EventArgs> m_refreshCullingGroup;

	private static PropManager Instance
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
			DisableAllProps();
		}
	}

	private void OnDestroy()
	{
		DestroyPools();
		if (m_cullingGroup != null)
		{
			m_cullingGroup.Dispose();
		}
		m_cullingGroup = null;
		if (m_cullingPropList != null)
		{
			m_cullingPropList.Clear();
		}
		if (m_cullingGravityPropTable != null)
		{
			m_cullingGravityPropTable.Clear();
		}
		m_cullingPropList = null;
		if (m_cullingSpheres != null)
		{
			Array.Clear(m_cullingSpheres, 0, m_cullingSpheres.Length);
		}
		m_cullingSpheres = null;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_refreshCullingGroup);
		SceneManager.sceneLoaded -= OnSceneLoaded;
		m_instance = null;
		m_propTable = null;
		IsInitialized = false;
		m_cullingGroupsInitialized = false;
	}

	private GenericPool_RL<Prop> CreatePool(Prop prefab, int poolSize)
	{
		GenericPool_RL<Prop> genericPool_RL = new GenericPool_RL<Prop>();
		genericPool_RL.Initialize(prefab, poolSize, disablePooling: false);
		return genericPool_RL;
	}

	public static void AddPropToPool(Prop propPrefab, int poolSize)
	{
		if ((bool)propPrefab)
		{
			int key = Animator.StringToHash(propPrefab.name);
			if (!m_propTable.ContainsKey(key))
			{
				m_propTable.Add(key, Instance.CreatePool(propPrefab, poolSize));
			}
		}
	}

	public static Prop GetProp(Prop prop, int propNameHash)
	{
		if (m_propTable.ContainsKey(propNameHash))
		{
			return m_propTable[propNameHash].GetFreeObj();
		}
		Debug.LogFormat("<color=red>| PropManager | Prop Object Pool Table does not contain an entry for Prop named ({0})</color>", prop.name);
		return null;
	}

	private void Initialize()
	{
		m_propTable = new Dictionary<int, GenericPool_RL<Prop>>();
		m_refreshCullingGroup = RefreshCullingGroup;
		IsInitialized = true;
	}

	private void InitializeCullingGroup(int groupSize)
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_refreshCullingGroup);
		if (m_cullingGroup != null)
		{
			m_cullingGroup.Dispose();
			m_cullingGroup = null;
		}
		if (m_cullingSpheres != null)
		{
			Array.Clear(m_cullingSpheres, 0, m_cullingSpheres.Length);
			m_cullingSpheres = null;
		}
		if (groupSize <= 0)
		{
			groupSize = 3000;
		}
		m_cullingGroupSize = groupSize;
		m_cullingGroup = new CullingGroup();
		m_cullingSpheres = new BoundingSphere[m_cullingGroupSize];
		for (int i = 0; i < m_cullingSpheres.Length; i++)
		{
			m_cullingSpheres[i] = new BoundingSphere(Vector3.zero, 16f);
		}
		m_cullingGroup.SetBoundingSpheres(m_cullingSpheres);
		m_cullingGroup.SetBoundingSphereCount(m_cullingGroupSize);
		m_cullingGroup.targetCamera = CameraController.GameCamera;
		CullingGroup cullingGroup = m_cullingGroup;
		cullingGroup.onStateChanged = (CullingGroup.StateChanged)Delegate.Combine(cullingGroup.onStateChanged, new CullingGroup.StateChanged(CullingSphereStateChanged));
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerEnterRoom, m_refreshCullingGroup);
		m_cullingGroupsInitialized = true;
	}

	private void RefreshCullingGroup(object sender, EventArgs args)
	{
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		m_cullingPropList.Clear();
		m_cullingGravityPropTable.Clear();
		PropSpawnController[] propSpawnControllers = currentPlayerRoom.SpawnControllerManager.PropSpawnControllers;
		foreach (PropSpawnController propSpawnController in propSpawnControllers)
		{
			if (propSpawnController.ShouldSpawn && (bool)propSpawnController.PropInstance && !propSpawnController.DisableCulling)
			{
				m_cullingPropList.Add(propSpawnController.PropInstance);
				if (propSpawnController.CameraLayer == CameraLayer.Game && (bool)propSpawnController.PropInstance.CorgiController && !m_cullingGravityPropTable.ContainsKey(propSpawnController.PropInstance))
				{
					m_cullingGravityPropTable.Add(propSpawnController.PropInstance, m_cullingPropList.Count - 1);
				}
			}
		}
		if (m_cullingPropList.Count > m_cullingGroupSize)
		{
			throw new Exception("Culling group size for props is " + m_cullingGroupSize + ".  Look into the max props code or increase PropManager.DEFAULT_CULLING_GROUP_SIZE to fix this error.");
		}
		m_cullingGroup.SetBoundingSphereCount(m_cullingPropList.Count);
		for (int j = 0; j < m_cullingPropList.Count; j++)
		{
			Prop prop = m_cullingPropList[j];
			m_cullingSpheres[j].position = prop.gameObject.transform.localPosition;
			SetPropCullState(prop, cull: false);
		}
		StartCoroutine(UpdateStartingCullStatesCoroutine());
	}

	private IEnumerator UpdateStartingCullStatesCoroutine()
	{
		yield return null;
		for (int i = 0; i < m_cullingPropList.Count; i++)
		{
			if (!m_cullingGroup.IsVisible(i))
			{
				SetPropCullState(m_cullingPropList[i], cull: true);
			}
		}
	}

	private void CullingSphereStateChanged(CullingGroupEvent evt)
	{
		if (m_cullingPropList.Count <= evt.index)
		{
			return;
		}
		Prop prop = m_cullingPropList[evt.index];
		if ((bool)prop)
		{
			if (evt.hasBecomeVisible)
			{
				SetPropCullState(prop, cull: false);
			}
			else if (evt.hasBecomeInvisible)
			{
				SetPropCullState(prop, cull: true);
			}
		}
	}

	private void SetDecoCullStates(Prop prop, bool cull)
	{
		DecoController[] decoControllers = prop.DecoControllers;
		for (int i = 0; i < decoControllers.Length; i++)
		{
			DecoLocation[] decoLocations = decoControllers[i].DecoLocations;
			foreach (DecoLocation decoLocation in decoLocations)
			{
				if (!decoLocation.DecoInstance)
				{
					continue;
				}
				Prop[] props = decoLocation.DecoInstance.Props;
				foreach (Prop prop2 in props)
				{
					if (prop2.CameraLayerController.CameraLayer == CameraLayer.Game && prop2.HitboxController != null && prop2.HitboxesDisabled != cull)
					{
						prop2.HitboxController.SetCulledState(cull, includeRigidbody: true);
						prop2.HitboxesDisabled = cull;
					}
					if (prop2.IsCulled == cull)
					{
						continue;
					}
					if (prop2.Animators != null)
					{
						Animator[] animators = prop2.Animators;
						for (int l = 0; l < animators.Length; l++)
						{
							animators[l].enabled = !cull;
						}
					}
					if (prop2.Lights != null)
					{
						if (!cull)
						{
							if (QualitySettings.GetQualityLevel() != 0)
							{
								Light[] lights = prop.Lights;
								for (int l = 0; l < lights.Length; l++)
								{
									lights[l].enabled = true;
								}
							}
						}
						else
						{
							Light[] lights = prop.Lights;
							for (int l = 0; l < lights.Length; l++)
							{
								lights[l].enabled = false;
							}
						}
					}
					prop2.IsCulled = cull;
				}
			}
		}
	}

	private void SetPropCullState(Prop prop, bool cull)
	{
		if (prop.CameraLayerController.CameraLayer == CameraLayer.Game && prop.HitboxController != null)
		{
			if (!cull)
			{
				if (prop.HitboxesDisabled)
				{
					prop.HitboxController.SetCulledState(cull: false, includeRigidbody: true);
					prop.HitboxesDisabled = false;
				}
			}
			else if (!prop.HitboxesDisabled)
			{
				bool includeRigidbody = !prop.CorgiController && !prop.HitboxController.GetCollider(HitboxType.Platform);
				prop.HitboxController.SetCulledState(cull: true, includeRigidbody);
				prop.HitboxesDisabled = true;
			}
		}
		SetDecoCullStates(prop, cull);
		if (!cull)
		{
			if (!prop.IsCulled)
			{
				return;
			}
			if (prop.Animators != null)
			{
				Animator[] animators = prop.Animators;
				for (int i = 0; i < animators.Length; i++)
				{
					animators[i].enabled = true;
				}
			}
			if (prop.Lights != null && QualitySettings.GetQualityLevel() != 0)
			{
				Light[] lights = prop.Lights;
				for (int i = 0; i < lights.Length; i++)
				{
					lights[i].enabled = true;
				}
			}
			prop.IsCulled = false;
		}
		else
		{
			if (prop.IsCulled)
			{
				return;
			}
			if (prop.Animators != null)
			{
				Animator[] animators = prop.Animators;
				for (int i = 0; i < animators.Length; i++)
				{
					animators[i].enabled = false;
				}
			}
			if (prop.Lights != null)
			{
				Light[] lights = prop.Lights;
				for (int i = 0; i < lights.Length; i++)
				{
					lights[i].enabled = false;
				}
			}
			prop.IsCulled = true;
		}
	}

	private void SetPropCorgiControllerCullState(Prop prop, bool cull)
	{
		prop.HitboxController.SetCulledState(cull, includeRigidbody: true);
		if (prop.CorgiController.PermanentlyDisabled)
		{
			return;
		}
		if (cull)
		{
			if (prop.CorgiController.enabled)
			{
				prop.CorgiController.enabled = false;
			}
		}
		else if (!prop.CorgiController.enabled)
		{
			prop.CorgiController.enabled = true;
		}
	}

	private void FixedUpdate()
	{
		if (!m_cullingGroupsInitialized)
		{
			return;
		}
		Vector3 localPosition = m_cullingGroup.targetCamera.transform.localPosition;
		float num = 32f * CameraController.ZoomLevel;
		foreach (KeyValuePair<Prop, int> item in m_cullingGravityPropTable)
		{
			Vector3 localPosition2 = item.Key.gameObject.transform.localPosition;
			m_cullingSpheres[item.Value].position = localPosition2;
			float num2 = localPosition.x - localPosition2.x;
			bool cull = num2 < 0f - num || num2 > num;
			SetPropCorgiControllerCullState(item.Key, cull);
		}
	}

	private void Internal_CreateBiomePools(BiomeType biomeType)
	{
		m_highestPropCount = 0;
		Dictionary<Prop, int> dictionary = new Dictionary<Prop, int>();
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeType) : OnPlayManager.BiomeController);
		if ((bool)biomeController)
		{
			Dictionary<Prop, int> dictionary2 = new Dictionary<Prop, int>();
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
				int num = 0;
				dictionary2.Clear();
				PropSpawnController[] propSpawnControllers = item.SpawnControllerManager.PropSpawnControllers;
				for (int i = 0; i < propSpawnControllers.Length; i++)
				{
					Prop propPrefab = propSpawnControllers[i].PropPrefab;
					if ((bool)propPrefab)
					{
						if (dictionary2.ContainsKey(propPrefab))
						{
							dictionary2[propPrefab]++;
						}
						else
						{
							dictionary2.Add(propPrefab, 1);
						}
						num++;
					}
				}
				foreach (KeyValuePair<Prop, int> item2 in dictionary2)
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
				if (num > m_highestPropCount)
				{
					m_highestPropCount = num;
				}
			}
		}
		List<int> list2 = m_propTable.Keys.ToList();
		int num2 = 0;
		int num3 = 0;
		foreach (KeyValuePair<Prop, int> item3 in dictionary)
		{
			if ((bool)item3.Key)
			{
				int num4 = Animator.StringToHash(item3.Key.name);
				list2.Remove(num4);
				if (!m_propTable.ContainsKey(num4))
				{
					m_propTable.Add(num4, CreatePool(item3.Key, item3.Value));
					num3++;
				}
				else
				{
					m_propTable[num4].ResizePool(item3.Value);
					num2++;
				}
			}
		}
		foreach (int item4 in list2)
		{
			if (m_propTable.ContainsKey(item4))
			{
				m_propTable[item4].DestroyPool();
				m_propTable.Remove(item4);
			}
		}
		InitializeCullingGroup(m_highestPropCount);
	}

	public static void DestroyPools()
	{
		if (Instance.m_cullingGroupsInitialized)
		{
			Instance.m_cullingGravityPropTable.Clear();
			Instance.m_cullingPropList.Clear();
		}
		foreach (KeyValuePair<int, GenericPool_RL<Prop>> item in m_propTable)
		{
			item.Value.DestroyPool();
		}
		m_propTable.Clear();
	}

	public static void CreateBiomePools(BiomeType biome)
	{
		Instance.Internal_CreateBiomePools(biome);
	}

	public static void DisableAllProps()
	{
		foreach (KeyValuePair<int, GenericPool_RL<Prop>> item in m_propTable)
		{
			item.Value.DisableAll();
		}
	}
}
