using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemyManager : MonoBehaviour
{
	public struct EnemyPreSummonedState
	{
		public bool StoredActivationState;

		public bool StoredLockFlip;

		public float StoredLocalScaleX;
	}

	private const int CULLING_GROUP_SIZE = 100;

	private const string MANAGER_NAME = "EnemyManager";

	private const string RESOURCE_PATH = "Prefabs/Managers/EnemyManager";

	private static readonly int NEUTRAL_ANIM_PARAM = Animator.StringToHash("Neutral");

	private Dictionary<EnemyType, Dictionary<EnemyRank, GenericPool_RL<EnemyController>>> m_enemyPoolTable;

	private List<EnemyController> m_summonedEnemyList = new List<EnemyController>();

	private CullingGroup m_cullingGroup;

	private List<EnemyController> m_cullingEnemyList = new List<EnemyController>();

	private BoundingSphere[] m_cullingSpheres;

	private MaterialPropertyBlock m_matPropertyBlock;

	private EnemySummonedEventArgs m_enemySummonedArgs;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitRoom;

	private Action<MonoBehaviour, EventArgs> m_refreshCullingGroup;

	private static bool m_isInitialized;

	private static EnemyManager m_enemyManager = null;

	public static List<EnemyController> SummonedEnemyList => Instance.m_summonedEnemyList;

	private static EnemyManager Instance
	{
		get
		{
			if (!m_enemyManager)
			{
				m_enemyManager = CDGHelper.FindStaticInstance<EnemyManager>(createNewIfNull: false);
			}
			return m_enemyManager;
		}
	}

	public static int NumActiveSummonedEnemies
	{
		get
		{
			int num = 0;
			foreach (EnemyController summonedEnemy in Instance.m_summonedEnemyList)
			{
				if (!summonedEnemy.IsDead || summonedEnemy.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_DeathDelay))
				{
					num++;
				}
			}
			return num;
		}
	}

	public static int NumActiveEnemies
	{
		get
		{
			int num = 0;
			if (PlayerManager.IsInstantiated)
			{
				BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
				if ((bool)currentPlayerRoom)
				{
					EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
					for (int i = 0; i < enemySpawnControllers.Length; i++)
					{
						EnemyController enemyInstance = enemySpawnControllers[i].EnemyInstance;
						if ((bool)enemyInstance && (!enemyInstance.IsDead || enemyInstance.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_DeathDelay)))
						{
							num++;
						}
					}
				}
			}
			return num;
		}
	}

	public static bool IsInitialized => m_isInitialized;

	private void Awake()
	{
		m_onPlayerExitRoom = OnPlayerExitRoom;
		m_refreshCullingGroup = RefreshCullingGroup;
		Initialize();
		Debug.Log("<color=green>Creating Enemy Manager...</color>");
		SceneManager.sceneLoaded += OnSceneLoaded;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerExitRoom);
	}

	private void OnPlayerExitRoom(object sender, EventArgs args)
	{
		StopAllCoroutines();
		DisableAllSummonedEnemies();
	}

	private void OnSceneLoaded(Scene arg0, LoadSceneMode arg1)
	{
		if (IsInitialized)
		{
			StopAllCoroutines();
			DisableAllEnemies();
			DisableAllSummonedEnemies();
		}
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
			m_cullingSpheres[i] = new BoundingSphere(Vector3.zero, 18f);
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
		m_cullingEnemyList.Clear();
		EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (enemySpawnController.ShouldSpawn && !enemySpawnController.IsDead && enemySpawnController.EnemyInstance != null && !enemySpawnController.EnemyInstance.Equals(null))
			{
				m_cullingEnemyList.Add(enemySpawnController.EnemyInstance);
			}
		}
		if (m_cullingEnemyList.Count > 100)
		{
			throw new Exception("Culling group size for enemies is " + 100 + ".  Please increase EnemyManager.CULLING_GROUP_SIZE to fix this error.");
		}
		m_cullingGroup.SetBoundingSphereCount(m_cullingEnemyList.Count);
		for (int j = 0; j < m_cullingEnemyList.Count; j++)
		{
			EnemyController enemyController = m_cullingEnemyList[j];
			m_cullingSpheres[j].position = enemyController.transform.localPosition;
			StartCoroutine(SetEnemyCulled(enemyController, culled: false));
		}
		StartCoroutine(UpdateStartingCullStatesCoroutine());
	}

	private IEnumerator UpdateStartingCullStatesCoroutine()
	{
		yield return null;
		for (int i = 0; i < m_cullingEnemyList.Count; i++)
		{
			if (!m_cullingGroup.IsVisible(i))
			{
				StartCoroutine(SetEnemyCulled(m_cullingEnemyList[i], culled: true));
			}
		}
	}

	private void CullingSphereStateChanged(CullingGroupEvent evt)
	{
		if (m_cullingEnemyList.Count > evt.index)
		{
			EnemyController enemy = m_cullingEnemyList[evt.index];
			if (evt.hasBecomeVisible)
			{
				StartCoroutine(SetEnemyCulled(enemy, culled: false));
			}
			else if (evt.hasBecomeInvisible)
			{
				StartCoroutine(SetEnemyCulled(enemy, culled: true));
			}
		}
	}

	private void FixedUpdate()
	{
		for (int i = 0; i < m_cullingEnemyList.Count; i++)
		{
			if ((bool)m_cullingEnemyList[i])
			{
				m_cullingSpheres[i].position = m_cullingEnemyList[i].gameObject.transform.localPosition;
			}
		}
	}

	public static IEnumerator SetEnemyCulled(EnemyController enemy, bool culled, bool isSpawning = false)
	{
		if ((!enemy.IsCulled && (enemy.DisableCulling || enemy.IsBoss)) || enemy.IsDead || !enemy.gameObject.activeSelf)
		{
			yield break;
		}
		while (!enemy.IsInitialized)
		{
			yield return null;
		}
		if (!culled)
		{
			if (!enemy.IsCulled)
			{
				yield break;
			}
			GameObject[] culledObjectsArray = enemy.CulledObjectsArray;
			foreach (GameObject gameObject in culledObjectsArray)
			{
				if (!gameObject.activeSelf)
				{
					gameObject.SetActive(value: true);
				}
			}
			enemy.HitboxController.SetCulledState(cull: false, includeRigidbody: true);
			enemy.VisualBoundsObj.Rigidbody.simulated = true;
			enemy.CharacterCorgi.enabled = true;
			enemy.ControllerCorgi.enabled = true;
			enemy.Animator.enabled = true;
			enemy.LogicController.LogicScript.enabled = true;
			enemy.LogicController.enabled = true;
			if (isSpawning)
			{
				enemy.Visuals.SetActive(value: false);
			}
			if (enemy.ResetToNeutralWhenUnculling)
			{
				enemy.Animator.Play(NEUTRAL_ANIM_PARAM, 0, 1f);
			}
			if ((bool)enemy.PreventPlatformDropObj && !enemy.FallLedge)
			{
				enemy.PreventPlatformDropObj.enabled = true;
			}
			enemy.IsCulled = false;
		}
		else
		{
			if (!culled || enemy.IsCulled)
			{
				yield break;
			}
			GameObject[] culledObjectsArray = enemy.CulledObjectsArray;
			foreach (GameObject gameObject2 in culledObjectsArray)
			{
				if (gameObject2.activeSelf)
				{
					gameObject2.SetActive(value: false);
				}
			}
			enemy.HitboxController.SetCulledState(cull: true, includeRigidbody: true);
			enemy.VisualBoundsObj.Rigidbody.simulated = false;
			enemy.CharacterCorgi.enabled = false;
			enemy.ControllerCorgi.enabled = false;
			enemy.Animator.enabled = false;
			enemy.LogicController.LogicScript.enabled = false;
			enemy.LogicController.enabled = false;
			if ((bool)enemy.PreventPlatformDropObj)
			{
				enemy.PreventPlatformDropObj.enabled = false;
			}
			enemy.IsCulled = true;
		}
	}

	private void OnDestroy()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
		DestroyPools();
		m_enemyManager = null;
		m_isInitialized = false;
		if (m_cullingGroup != null)
		{
			m_cullingGroup.Dispose();
		}
		m_cullingGroup = null;
		if (m_cullingSpheres != null)
		{
			Array.Clear(m_cullingSpheres, 0, m_cullingSpheres.Length);
		}
		m_cullingSpheres = null;
		if (m_cullingEnemyList != null)
		{
			m_cullingEnemyList.Clear();
		}
		m_cullingEnemyList = null;
		m_refreshCullingGroup = null;
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerEnterRoom, m_refreshCullingGroup);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitRoom, m_onPlayerExitRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerExitRoom);
	}

	private void Initialize()
	{
		m_matPropertyBlock = new MaterialPropertyBlock();
		m_enemySummonedArgs = new EnemySummonedEventArgs(null, null);
		m_enemyPoolTable = new Dictionary<EnemyType, Dictionary<EnemyRank, GenericPool_RL<EnemyController>>>();
		m_isInitialized = true;
	}

	private void Internal_CreateBiomePools(BiomeType biomeType)
	{
		Dictionary<EnemyController, int> dictionary = new Dictionary<EnemyController, int>();
		BiomeController biomeController = null;
		biomeController = ((!GameUtility.IsInLevelEditor || !OnPlayManager.BiomeController) ? WorldBuilder.GetBiomeController(biomeType) : OnPlayManager.BiomeController);
		if ((bool)biomeController)
		{
			bool flag = BurdenManager.GetBurdenLevel(BurdenType.EnemyEvolve) > 0;
			Dictionary<EnemyController, int> dictionary2 = new Dictionary<EnemyController, int>();
			List<SummonRuleController> list = new List<SummonRuleController>();
			List<EnemyTypeAndRank> list2 = new List<EnemyTypeAndRank>();
			List<EnemyTypeAndRank> list3 = new List<EnemyTypeAndRank>();
			foreach (BaseRoom room in biomeController.Rooms)
			{
				dictionary2.Clear();
				EnemySpawnController[] enemySpawnControllers = room.SpawnControllerManager.EnemySpawnControllers;
				foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
				{
					if (!enemySpawnController.ShouldSpawn)
					{
						continue;
					}
					EnemySpawnController enemySpawnController2 = enemySpawnController;
					EnemyController enemyPrefab = EnemyLibrary.GetEnemyPrefab(enemySpawnController2.Type, enemySpawnController2.Rank);
					if ((bool)enemyPrefab)
					{
						if (dictionary2.ContainsKey(enemyPrefab))
						{
							dictionary2[enemyPrefab]++;
						}
						else
						{
							dictionary2.Add(enemyPrefab, 1);
						}
					}
				}
				list.Clear();
				room.gameObject.GetComponentsInChildren(includeInactive: true, list);
				EnemyRank enemyRank = EnemyRank.None;
				foreach (SummonRuleController item3 in list)
				{
					BaseSummonRule[] summonRuleArray = item3.SummonRuleArray;
					foreach (BaseSummonRule baseSummonRule in summonRuleArray)
					{
						if (baseSummonRule.RuleType == SummonRuleType.SetSummonPoolDifficulty)
						{
							enemyRank = (baseSummonRule as SetSummonPoolDifficulty_SummonRule).SummonPoolDifficulty;
						}
						else
						{
							if (baseSummonRule.RuleType != SummonRuleType.SetSummonPool)
							{
								continue;
							}
							SetSummonPool_SummonRule setSummonPool_SummonRule = baseSummonRule as SetSummonPool_SummonRule;
							list2.Clear();
							if (setSummonPool_SummonRule.PoolIsBiomeSpecific)
							{
								list2.AddRange(EnemyUtility.GetAllEnemiesInBiome(room.BiomeType));
								for (int j = 0; j < list2.Count; j++)
								{
									if (SetSummonPool_SummonRule.SummonExceptionArray.Contains(list2[j].Type))
									{
										list2.RemoveAt(j);
										j--;
									}
								}
								if (flag && enemyRank == EnemyRank.None)
								{
									list3.Clear();
									for (int k = 0; k < list2.Count; k++)
									{
										EnemyTypeAndRank enemyTypeAndRank = list2[k];
										if (enemyTypeAndRank.Rank < EnemyRank.Advanced)
										{
											EnemyTypeAndRank item = new EnemyTypeAndRank(enemyTypeAndRank.Type, EnemyRank.Advanced);
											if (!list2.Contains(item) && !list3.Contains(item))
											{
												list3.Add(item);
											}
										}
										if (enemyTypeAndRank.Rank < EnemyRank.Expert)
										{
											EnemyTypeAndRank item2 = new EnemyTypeAndRank(enemyTypeAndRank.Type, EnemyRank.Expert);
											if (!list2.Contains(item2) && !list3.Contains(item2))
											{
												list3.Add(item2);
											}
										}
									}
									list2.AddRange(list3);
								}
							}
							else
							{
								list2.AddRange(setSummonPool_SummonRule.EnemiesToSummonArray);
							}
							foreach (EnemyTypeAndRank item4 in list2)
							{
								EnemyRank enemyRank2 = item4.Rank;
								if (enemyRank != EnemyRank.None && enemyRank != EnemyRank.Any)
								{
									enemyRank2 = enemyRank;
								}
								EnemyController enemyPrefab2 = EnemyLibrary.GetEnemyPrefab(item4.Type, enemyRank2);
								if ((bool)enemyPrefab2)
								{
									if (dictionary2.ContainsKey(enemyPrefab2))
									{
										dictionary2[enemyPrefab2]++;
									}
									else
									{
										dictionary2.Add(enemyPrefab2, 1);
									}
								}
							}
						}
					}
				}
				foreach (KeyValuePair<EnemyController, int> item5 in dictionary2)
				{
					if (dictionary.ContainsKey(item5.Key))
					{
						if (dictionary[item5.Key] < dictionary2[item5.Key])
						{
							dictionary[item5.Key] = dictionary2[item5.Key];
						}
					}
					else
					{
						dictionary.Add(item5.Key, item5.Value);
					}
				}
			}
		}
		Dictionary<EnemyController, int> dictionary3 = new Dictionary<EnemyController, int>(dictionary);
		foreach (KeyValuePair<EnemyController, int> item6 in dictionary)
		{
			SummonEnemyController component = item6.Key.GetComponent<SummonEnemyController>();
			if ((bool)component)
			{
				SummonEnemyController.SummonEnemyEntry[] enemiesToSummon = component.EnemiesToSummon;
				foreach (SummonEnemyController.SummonEnemyEntry summonEntry in enemiesToSummon)
				{
					RecursivelyAddSummonedEnemies(dictionary3, summonEntry);
				}
			}
		}
		List<Vector2Int> list4 = new List<Vector2Int>();
		foreach (KeyValuePair<EnemyType, Dictionary<EnemyRank, GenericPool_RL<EnemyController>>> item7 in m_enemyPoolTable)
		{
			foreach (KeyValuePair<EnemyRank, GenericPool_RL<EnemyController>> item8 in item7.Value)
			{
				list4.Add(new Vector2Int((int)item7.Key, (int)item8.Key));
			}
		}
		int num = 0;
		int num2 = 0;
		foreach (KeyValuePair<EnemyController, int> item9 in dictionary3)
		{
			EnemyController key = item9.Key;
			if (!key)
			{
				continue;
			}
			list4.Remove(new Vector2Int((int)key.EnemyType, (int)key.EnemyRank));
			if (m_enemyPoolTable.ContainsKey(key.EnemyType))
			{
				Dictionary<EnemyRank, GenericPool_RL<EnemyController>> dictionary4 = m_enemyPoolTable[key.EnemyType];
				if (!dictionary4.ContainsKey(key.EnemyRank))
				{
					dictionary4.Add(key.EnemyRank, CreateEnemyPool(key, item9.Value));
					num2++;
				}
				else
				{
					dictionary4[key.EnemyRank].ResizePool(item9.Value);
					num++;
				}
			}
			else
			{
				Dictionary<EnemyRank, GenericPool_RL<EnemyController>> dictionary5 = new Dictionary<EnemyRank, GenericPool_RL<EnemyController>>();
				dictionary5.Add(key.EnemyRank, CreateEnemyPool(key, item9.Value));
				m_enemyPoolTable.Add(key.EnemyType, dictionary5);
				num2++;
			}
		}
		foreach (Vector2Int item10 in list4)
		{
			EnemyType x = (EnemyType)item10.x;
			EnemyRank y = (EnemyRank)item10.y;
			if (m_enemyPoolTable.ContainsKey(x))
			{
				Dictionary<EnemyRank, GenericPool_RL<EnemyController>> dictionary6 = m_enemyPoolTable[x];
				if (dictionary6.ContainsKey(y))
				{
					dictionary6[y].DestroyPool();
					dictionary6.Remove(y);
				}
				if (dictionary6.Count <= 0)
				{
					m_enemyPoolTable.Remove(x);
				}
			}
		}
		ProjectileManager.Instance.CreateEnemyProjectilePools(dictionary3.Keys.ToList());
	}

	private void RecursivelyAddSummonedEnemies(Dictionary<EnemyController, int> enemyCountDict, SummonEnemyController.SummonEnemyEntry summonEntry)
	{
		if (summonEntry.NumSummons <= 0)
		{
			return;
		}
		EnemyController enemyPrefab = EnemyLibrary.GetEnemyPrefab(summonEntry.EnemyType, summonEntry.EnemyRank);
		if (!enemyPrefab)
		{
			return;
		}
		if (!enemyCountDict.ContainsKey(enemyPrefab))
		{
			enemyCountDict.Add(enemyPrefab, summonEntry.NumSummons);
			SummonEnemyController component = enemyPrefab.GetComponent<SummonEnemyController>();
			if ((bool)component)
			{
				SummonEnemyController.SummonEnemyEntry[] enemiesToSummon = component.EnemiesToSummon;
				foreach (SummonEnemyController.SummonEnemyEntry summonEntry2 in enemiesToSummon)
				{
					RecursivelyAddSummonedEnemies(enemyCountDict, summonEntry2);
				}
			}
		}
		else if (enemyCountDict.ContainsKey(enemyPrefab) && enemyCountDict[enemyPrefab] < summonEntry.NumSummons)
		{
			enemyCountDict[enemyPrefab] = summonEntry.NumSummons;
		}
	}

	private GenericPool_RL<EnemyController> CreateEnemyPool(EnemyController prefab, int poolSize)
	{
		if (!prefab)
		{
			return null;
		}
		GenericPool_RL<EnemyController> genericPool_RL = new GenericPool_RL<EnemyController>();
		genericPool_RL.Initialize(prefab, poolSize, disablePooling: false);
		return genericPool_RL;
	}

	public static bool Contains(EnemyType enemyType, EnemyRank enemyRank)
	{
		return !string.IsNullOrEmpty(EnemyLibrary.GetEnemyPrefabPath(enemyType, enemyRank));
	}

	public static int GetNumActiveSummonedEnemiesOfType(EnemyType enemyType, EnemyRank enemyRank)
	{
		int num = 0;
		foreach (EnemyController summonedEnemy in Instance.m_summonedEnemyList)
		{
			if ((!summonedEnemy.IsDead || summonedEnemy.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_DeathDelay)) && (summonedEnemy.EnemyType == enemyType || enemyType == EnemyType.Any) && (summonedEnemy.EnemyRank == enemyRank || enemyRank == EnemyRank.Any))
			{
				num++;
			}
		}
		return num;
	}

	public static int GetNumActiveEnemiesOfType(EnemyType enemyType, EnemyRank enemyRank)
	{
		int num = 0;
		if (PlayerManager.IsInstantiated)
		{
			BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
			if (currentPlayerRoom != null)
			{
				EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
				foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
				{
					if (!enemySpawnController.EnemyInstance.IsNativeNull() && (!enemySpawnController.EnemyInstance.IsDead || enemySpawnController.EnemyInstance.StatusEffectController.HasStatusEffect(StatusEffectType.Enemy_DeathDelay)) && (enemySpawnController.EnemyInstance.EnemyType == enemyType || enemyType == EnemyType.Any) && (enemySpawnController.EnemyInstance.EnemyRank == enemyRank || enemyRank == EnemyRank.Any))
					{
						num++;
					}
				}
			}
		}
		return num;
	}

	public static EnemyController GetEnemyFromPool(EnemyType enemyType, EnemyRank enemyRank)
	{
		if (!IsInitialized)
		{
			Debug.LogFormat("<color=yellow>You tried to get an Enemy from the Enemy Object Pool before it was initialised<color>");
			return null;
		}
		Dictionary<EnemyRank, GenericPool_RL<EnemyController>> value = null;
		if (Instance.m_enemyPoolTable.TryGetValue(enemyType, out value))
		{
			GenericPool_RL<EnemyController> value2 = null;
			if (value.TryGetValue(enemyRank, out value2))
			{
				EnemyController freeObj = value2.GetFreeObj();
				freeObj.gameObject.SetActive(value: true);
				return freeObj;
			}
			Debug.LogFormat("Unable to find entry for Enemy Rank ({0}) for Enemy Type ({1}) in Enemy Pool", enemyRank, enemyType);
			return null;
		}
		Debug.LogFormat("Unable to find entry for Enemy Type ({0}) in Enemy Manager", enemyType);
		return null;
	}

	public static EnemyController SummonEnemy(ISummoner summoner, EnemyType enemyType, EnemyRank enemyRank, Vector2 spawnPosOffset, bool useAbsPos = false, bool runSummonAnim = true, float speedMod = 1f, float hpMod = 1f)
	{
		EnemyController enemyFromPool = GetEnemyFromPool(enemyType, enemyRank);
		if ((bool)enemyFromPool)
		{
			enemyFromPool.Summoner = summoner;
			if (summoner != null)
			{
				enemyFromPool.SetLevel(summoner.Level);
			}
			else
			{
				enemyFromPool.SetLevel(PlayerManager.GetCurrentPlayerRoom().Level);
			}
			enemyFromPool.SetRoom(PlayerManager.GetCurrentPlayerRoom());
			enemyFromPool.EnemySpawnController = null;
			Vector3 localPosition = spawnPosOffset;
			if (!useAbsPos && summoner != null)
			{
				localPosition.x += summoner.gameObject.transform.position.x;
				localPosition.y += summoner.gameObject.transform.position.y;
			}
			enemyFromPool.gameObject.transform.localPosition = localPosition;
			enemyFromPool.ForceFaceTarget();
			enemyFromPool.IsCommander = false;
			Instance.m_enemySummonedArgs.Initialize(enemyFromPool, summoner);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.EnemySummoned, Instance, Instance.m_enemySummonedArgs);
			if (runSummonAnim)
			{
				Instance.StartCoroutine(Instance.Internal_RunSummonAnimCoroutine(enemyFromPool, speedMod, disableEnemyOffset: false, hpMod));
			}
		}
		return enemyFromPool;
	}

	public static IEnumerator RunSummonAnimCoroutine(EnemyController enemy, float speedMod = 1f, bool disableEnemyOffset = false, float hpMod = 1f)
	{
		yield return Instance.Internal_RunSummonAnimCoroutine(enemy, speedMod, disableEnemyOffset, hpMod);
	}

	private IEnumerator Internal_RunSummonAnimCoroutine(EnemyController enemy, float speedMod = 1f, bool disableEnemyOffset = false, float hpMod = 1f)
	{
		enemy.ResetCharacter();
		enemy.HitboxController.DisableAllCollisions = true;
		EffectManager.AddAnimatorToDisableList(enemy.Animator);
		enemy.gameObject.SetActive(value: true);
		enemy.Visuals.SetActive(value: false);
		Instance.m_summonedEnemyList.Add(enemy);
		while (!enemy.IsInitialized)
		{
			yield return null;
		}
		enemy.IsBeingSummoned = true;
		enemy.InitializeCommanderStatusEffects();
		yield return SetEnemyCulled(enemy, culled: false, isSpawning: true);
		if (!disableEnemyOffset && !enemy.ForceDisableSummonOffset)
		{
			Vector3 localPosition = enemy.gameObject.transform.localPosition;
			localPosition.y -= enemy.Midpoint.y - enemy.gameObject.transform.localPosition.y;
			enemy.gameObject.transform.localPosition = localPosition;
		}
		enemy.OnPositionedForSummoningRelay.Dispatch(null, null);
		enemy.UpdateBounds();
		enemy.ResetCollisionState();
		enemy.ControllerCorgi.enabled = false;
		float duration = 1.2f / speedMod;
		BaseEffect baseEffect = EffectManager.PlayEffect(null, null, "SpellCircle_Effect", enemy.Midpoint, duration, EffectStopType.Gracefully);
		baseEffect.GetComponent<Animator>().speed = speedMod;
		Bounds visualBounds = enemy.VisualBounds;
		bool num = visualBounds.size.y > visualBounds.size.x;
		Bounds bounds = new Bounds(baseEffect.transform.localPosition, new Vector3(2f, 2f, 2f));
		float num2 = (num ? (visualBounds.size.y / bounds.size.y) : (visualBounds.size.x / bounds.size.x));
		num2 *= 1f;
		num2 = Mathf.Clamp(num2, 1.25f, 999f);
		baseEffect.transform.localScale = new Vector3(num2, num2, num2);
		List<Renderer> rendererArray = enemy.RendererArray;
		if (rendererArray != null)
		{
			foreach (Renderer item in rendererArray)
			{
				item.GetPropertyBlock(m_matPropertyBlock);
				m_matPropertyBlock.SetColor(ShaderID_RL._AlphaBlendColor, Color.white);
				item.SetPropertyBlock(m_matPropertyBlock);
			}
		}
		EnemyPreSummonedState preSummonedState = default(EnemyPreSummonedState);
		Vector3 localScale = enemy.gameObject.transform.localScale;
		preSummonedState.StoredLocalScaleX = localScale.x;
		localScale.x = 0.01f;
		enemy.gameObject.transform.localScale = localScale;
		enemy.SetHealth(Mathf.Max((float)enemy.ActualMaxHealth * hpMod, 1f), additive: false, runEvents: false);
		preSummonedState.StoredLockFlip = enemy.LockFlip;
		enemy.Animator.enabled = false;
		enemy.LogicController.enabled = false;
		enemy.LockFlip = true;
		enemy.SetVelocity(0f, 0f, additive: false);
		preSummonedState.StoredActivationState = enemy.LogicController.DisableLogicActivationByDistance;
		enemy.LogicController.DisableLogicActivationByDistance = true;
		enemy.PreSummonedState = preSummonedState;
		float yieldDuration2 = Time.time + 0.6f / speedMod;
		while (Time.time < yieldDuration2)
		{
			yield return null;
		}
		if (!enemy.InvisibleDuringSummonAnim)
		{
			enemy.Visuals.SetActive(value: true);
		}
		enemy.Animator.enabled = true;
		enemy.Animator.Play(NEUTRAL_ANIM_PARAM, 0, 1f);
		yield return null;
		enemy.Animator.enabled = false;
		yield return TweenManager.TweenTo(enemy.gameObject.transform, 0.2f, Ease.Back.EaseOut, "localScale.x", preSummonedState.StoredLocalScaleX);
		yieldDuration2 = Time.time + 0.6f / speedMod;
		while (Time.time < yieldDuration2)
		{
			yield return null;
		}
		ResetEnemySummonState(enemy);
		enemy.LogicController.TriggerAggro(null, null);
	}

	public static void KillAllSummonedEnemies()
	{
		foreach (EnemyController summonedEnemy in Instance.m_summonedEnemyList)
		{
			if ((bool)summonedEnemy && !summonedEnemy.IsDead)
			{
				summonedEnemy.KillCharacter(PlayerManager.GetPlayerController().gameObject, broadcastEvent: true);
			}
		}
	}

	public static void DisableAllSummonedEnemies()
	{
		foreach (EnemyController summonedEnemy in Instance.m_summonedEnemyList)
		{
			if ((bool)summonedEnemy)
			{
				ResetEnemySummonState(summonedEnemy);
				summonedEnemy.gameObject.SetActive(value: false);
			}
		}
		Instance.m_summonedEnemyList.Clear();
	}

	public static void KillAllNonSummonedEnemies()
	{
		if (!PlayerManager.IsInstantiated)
		{
			return;
		}
		BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
		if (!currentPlayerRoom)
		{
			return;
		}
		EnemySpawnController[] enemySpawnControllers = currentPlayerRoom.SpawnControllerManager.EnemySpawnControllers;
		for (int i = 0; i < enemySpawnControllers.Length; i++)
		{
			EnemyController enemyInstance = enemySpawnControllers[i].EnemyInstance;
			if ((bool)enemyInstance && !enemyInstance.IsDead)
			{
				enemyInstance.KillCharacter(PlayerManager.GetPlayerController().gameObject, broadcastEvent: true);
			}
		}
	}

	private static void ResetEnemySummonState(EnemyController enemy)
	{
		if (enemy.IsBeingSummoned)
		{
			enemy.transform.SetLocalScaleX(enemy.PreSummonedState.StoredLocalScaleX);
			enemy.HitboxController.DisableAllCollisions = false;
			enemy.LogicController.DisableLogicActivationByDistance = enemy.PreSummonedState.StoredActivationState;
			enemy.Animator.enabled = true;
			enemy.LogicController.AssignAnimParamRank();
			enemy.LogicController.enabled = true;
			enemy.StatusEffectController.SetAllStatusEffectsHidden(hide: false);
			enemy.ControllerCorgi.enabled = true;
			enemy.LockFlip = enemy.PreSummonedState.StoredLockFlip;
			enemy.IsBeingSummoned = false;
			if (!enemy.Visuals.activeSelf)
			{
				enemy.Visuals.SetActive(value: true);
			}
			EffectManager.RemoveAnimatorFromDisableList(enemy.Animator);
			enemy.UseOverrideDefaultTint = false;
			enemy.ResetRendererArrayColor();
			if (TraitManager.IsTraitActive(TraitType.EnemiesBlackFill))
			{
				enemy.BlinkPulseEffect.ResetAllBlackFills();
				enemy.BlinkPulseEffect.ActivateBlackFill(BlackFillType.EnemiesBlackFill_Trait, 0f);
			}
		}
	}

	public static void DisableAllEnemies()
	{
		foreach (KeyValuePair<EnemyType, Dictionary<EnemyRank, GenericPool_RL<EnemyController>>> item in Instance.m_enemyPoolTable)
		{
			foreach (KeyValuePair<EnemyRank, GenericPool_RL<EnemyController>> item2 in item.Value)
			{
				item2.Value.DisableAll();
			}
		}
	}

	private void LateUpdate()
	{
		for (int i = 0; i < m_summonedEnemyList.Count; i++)
		{
			if (m_summonedEnemyList[i].IsNativeNull() || !m_summonedEnemyList[i].gameObject.activeInHierarchy)
			{
				m_summonedEnemyList.RemoveAt(i);
				i--;
			}
		}
	}

	public static void DestroyPools()
	{
		if (GameManager.IsApplicationClosing || !IsInitialized)
		{
			return;
		}
		if (Instance.m_cullingEnemyList != null)
		{
			Instance.m_cullingEnemyList.Clear();
		}
		foreach (KeyValuePair<EnemyType, Dictionary<EnemyRank, GenericPool_RL<EnemyController>>> item in Instance.m_enemyPoolTable)
		{
			foreach (KeyValuePair<EnemyRank, GenericPool_RL<EnemyController>> item2 in item.Value)
			{
				item2.Value.DestroyPool();
			}
			item.Value.Clear();
		}
		Instance.m_enemyPoolTable.Clear();
	}

	public static void CreateBiomePools(BiomeType biome)
	{
		Instance.Internal_CreateBiomePools(biome);
	}
}
