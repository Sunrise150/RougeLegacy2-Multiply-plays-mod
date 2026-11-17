using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RLWorldCreation;
using UnityEngine;

public class WorldBuilder : MonoBehaviour
{
	private const float ROOM_CONTENT_POSITION_X_MULTIPLIER = 1f;

	private const float ROOM_CONTENT_POSITION_Y_MULTIPLIER = 1f;

	[SerializeField]
	[HideInInspector]
	private BiomeLayer m_includedBiomes = BiomeLayer.Castle;

	[SerializeField]
	private BiomeCreator m_biomeCreator;

	[SerializeField]
	private bool m_instantiateRooms = true;

	[SerializeField]
	private bool m_deactivateRoomGameObjects = true;

	[SerializeField]
	private bool m_printSeedsToConsole = true;

	[SerializeField]
	private bool m_mergeRooms = true;

	[SerializeField]
	[HideInInspector]
	private List<BiomeType> m_biomeBuildOrder;

	private BiomeType m_firstBiome;

	private Dictionary<BiomeType, BiomeController> m_biomeControllers;

	public List<BiomeType> BiomeBuildOrder => m_biomeBuildOrder;

	public static Dictionary<BiomeType, BiomeController> BiomeControllers => Instance.m_biomeControllers;

	public static bool DeactivateRoomGameObjects => Instance.m_deactivateRoomGameObjects;

	public BiomeLayer IncludedBiomes
	{
		get
		{
			return m_includedBiomes;
		}
		set
		{
			m_includedBiomes = value;
		}
	}

	public BaseRoom StartingRoom
	{
		get
		{
			if (BiomeControllers != null && BiomeControllers.Count > 0)
			{
				return BiomeControllers[m_firstBiome].TransitionRoom;
			}
			return null;
		}
	}

	public static WorldBuilder Instance { get; private set; }

	public static bool IsInstantiated => Instance != null;

	public static BiomeBuildStateID State { get; private set; }

	public static bool CreateReports => Instance.LevelManager.Mode == LevelManagerMode.Debug;

	public static BiomeType FirstBiomeOverride { get; set; }

	public LevelManager_RL LevelManager { get; private set; }

	public static BaseRoom OldCastleTransitionRoom { get; private set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			LevelManager = GetComponent<LevelManager_RL>();
		}
		else
		{
			Debug.LogFormat("<color=red>|{0}| Multiple WorldBuilder's found in Scene. WorldBuilder is a Singleton. Destroying additional WorldBuilder</color>", this);
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator CreateBiome(BiomeType biome)
	{
		BiomeController biomeController = CreateBiomeController(biome, base.transform);
		StartCoroutine(m_biomeCreator.CreateBiome(biomeController));
		while (m_biomeCreator.State != BiomeBuildStateID.Complete && m_biomeCreator.State != BiomeBuildStateID.Failed)
		{
			yield return null;
		}
		if (m_biomeCreator.State == BiomeBuildStateID.Complete)
		{
			BiomeControllers.Add(biome, biomeController);
		}
		else if (m_biomeCreator.State == BiomeBuildStateID.Failed)
		{
			OnBiomeCreatorFail();
		}
	}

	public static BiomeController CreateBiomeController(BiomeType biome, Transform parent)
	{
		BiomeController biomeController = new GameObject(biome.ToString() + "_BiomeController").AddComponent<BiomeController>();
		biomeController.Initialise(biome);
		biomeController.gameObject.transform.SetParent(parent);
		biomeController.gameObject.transform.position = Vector3.zero;
		return biomeController;
	}

	public static void CreateWorld()
	{
		State = BiomeBuildStateID.Running;
		if (Instance.m_printSeedsToConsole)
		{
			RNGManager.PrintSeedsToConsole();
		}
		Instance.StartCoroutine(Instance.CreateWorldCoroutine());
	}

	public static void CreateWorld(StageSaveData stageSaveData)
	{
		if (stageSaveData == null)
		{
			throw new ArgumentNullException("stageSaveData");
		}
		State = BiomeBuildStateID.Running;
		Instance.StartCoroutine(Instance.CreateWorldCoroutine(stageSaveData));
	}

	private IEnumerator CreateWorldCoroutine(StageSaveData stageSaveData = null)
	{
		while (!SharedWorldObjects_Loader.IsInitialized)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		playerController.transform.SetLocalPositionX(0f);
		playerController.transform.SetLocalPositionY(0f);
		playerController.ControllerCorgi.SetRaysParameters();
		playerController.ControllerCorgi.GravityActive(state: false);
		m_biomeControllers = new Dictionary<BiomeType, BiomeController>();
		m_firstBiome = FirstBiomeOverride;
		FirstBiomeOverride = BiomeType.None;
		if (stageSaveData != null)
		{
			foreach (KeyValuePair<BiomeType, List<RoomSaveData>> item in stageSaveData.RoomSaveDataDict)
			{
				BiomeType key = item.Key;
				if (m_firstBiome == BiomeType.None)
				{
					m_firstBiome = key;
				}
				BiomeController biomeController = CreateBiomeController(key, base.transform);
				BiomeControllers.Add(key, biomeController);
				List<RoomSaveData> value = item.Value;
				m_biomeCreator.CreateBiomeFromSaveData(biomeController, value);
			}
		}
		else
		{
			int num = 0;
			foreach (BiomeType item2 in BiomeBuildOrder)
			{
				if (BiomeUtility.IsBiomeInBiomeLayerMask(item2, IncludedBiomes))
				{
					num++;
				}
			}
			if (num == 0)
			{
				throw new Exception($"|{this}| No Biomes in Included Biomes Field");
			}
			foreach (BiomeType item3 in BiomeBuildOrder)
			{
				if (!BiomeUtility.IsBiomeInBiomeLayerMask(item3, IncludedBiomes))
				{
					continue;
				}
				BiomeType connectsToBiome = BiomeUtility.GetConnectsToBiome(item3);
				if (m_firstBiome == BiomeType.None || connectsToBiome == BiomeType.None || BiomeControllers.ContainsKey(connectsToBiome))
				{
					if (m_firstBiome == BiomeType.None)
					{
						m_firstBiome = item3;
					}
					StartCoroutine(CreateBiome(item3));
					while (m_biomeCreator.State != BiomeBuildStateID.Complete && m_biomeCreator.State != BiomeBuildStateID.Failed)
					{
						yield return null;
					}
					if (m_biomeCreator.State == BiomeBuildStateID.Failed)
					{
						yield break;
					}
				}
				else
				{
					Debug.LogFormat("<color=red>|{0}| Can't build <b>{1}</b> Biome because the Biome that it connects to (<b>{2}</b>) has not been built.</color>", this, item3, BiomeUtility.GetConnectsToBiome(item3));
				}
			}
		}
		if (stageSaveData == null)
		{
			foreach (KeyValuePair<BiomeType, BiomeController> keyValuePair in BiomeControllers)
			{
				yield return Instance.RunBuildStage<GenerateRoomSeeds_BuildStage>(keyValuePair.Value);
				if (Instance.m_mergeRooms)
				{
					yield return Instance.RunBuildStage<CreateMergeRooms_BuildStage>(keyValuePair.Value);
				}
				if (m_biomeCreator.State == BiomeBuildStateID.Failed)
				{
					yield break;
				}
			}
		}
		if ((bool)LevelManager && LevelManager.Mode != LevelManagerMode.Debug)
		{
			foreach (KeyValuePair<BiomeType, BiomeController> biomeController2 in BiomeControllers)
			{
				foreach (GridPointManager gridPointManager in biomeController2.Value.GridPointManager.GridPointManagers)
				{
					RoomContentMetaData content = gridPointManager.RoomMetaData.GetContent();
					if (content != null)
					{
						gridPointManager.Content = new GridPointManagerContentEntry[content.ContentEntries.Length];
						for (int i = 0; i < content.ContentEntries.Length; i++)
						{
							RoomContentEntry roomContentEntry = content.ContentEntries[i];
							bool isSpawned = roomContentEntry.GetIsSpawned(gridPointManager);
							Vector2 roomContentWorldPosition = GetRoomContentWorldPosition(gridPointManager, roomContentEntry.LocalPosition);
							gridPointManager.Content[i] = new GridPointManagerContentEntry(roomContentEntry.ContentType, roomContentEntry.LocalPosition, roomContentWorldPosition, isSpawned);
						}
					}
				}
			}
			if (MapController.IsInitialized)
			{
				MapController.Reset();
			}
			MapController.Initialize();
			if (ItemDropManager.IsInitialized)
			{
				ItemDropManager.DisableAllItemDrops();
			}
			if (EnemyManager.IsInitialized)
			{
				EnemyManager.DisableAllEnemies();
				EnemyManager.DisableAllSummonedEnemies();
			}
		}
		if (m_instantiateRooms)
		{
			yield return InstantiateBiome(m_firstBiome);
			BiomeTransitionController.UpdateObjectPools(BiomeType.None, m_firstBiome);
			DeactivateRooms(m_firstBiome);
		}
		WorldBuildCompleteEventArgs eventArgs = new WorldBuildCompleteEventArgs(RNGSeedManager.GetCurrentSeed(base.gameObject.scene.name), RNGManager.GetSeed(RngID.BiomeCreation), BiomeControllers.Values.ToList(), isSuccess: true);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.WorldCreationComplete, this, eventArgs);
		State = BiomeBuildStateID.Complete;
	}

	private Vector2 GetRoomContentWorldPosition(GridPointManager room, Vector2 roomContentLocalPosition)
	{
		Vector2 worldPositionFromRoomCoordinates = GridController.GetWorldPositionFromRoomCoordinates(GridController.GetRoomCoordinatesFromGridCoordinates(room.GridCoordinates), room.Size);
		Vector2 vector = new Vector2(1f, 1f);
		if (room.IsRoomMirrored)
		{
			vector = new Vector2(-1f, 1f);
		}
		return worldPositionFromRoomCoordinates + vector * roomContentLocalPosition;
	}

	private void DeactivateRooms(BiomeType biome)
	{
		if (!DeactivateRoomGameObjects)
		{
			return;
		}
		foreach (BaseRoom room in BiomeControllers[biome].Rooms)
		{
			room.gameObject.SetActive(value: false);
		}
	}

	public static IEnumerator InstantiateBiome(BiomeType biome)
	{
		if (biome == BiomeType.Castle)
		{
			BiomeController biomeController = BiomeControllers[biome];
			List<BaseRoom> rooms = biomeController.Rooms;
			if (rooms != null && rooms.Count == 1 && rooms[0].RoomType == RoomType.Transition)
			{
				OldCastleTransitionRoom = rooms[0];
				biomeController.Rooms.Clear();
				biomeController.StandaloneRooms.Clear();
			}
		}
		MergeRoomTools.ResetMergeRoomCounter();
		yield return Instance.RunBuildStage<InstantiateRooms_BuildStage>(BiomeControllers[biome]);
		yield return new WaitForFixedUpdate();
		yield return Instance.RunBuildStage<CreateOneWays_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<CloseDoors_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<CreateTunnelRooms_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<RunIsSpawnedCheck_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<CreateRoomBackgrounds_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<SetSpawnTypes_BuildStage>(BiomeControllers[biome]);
		if (Instance.m_mergeRooms)
		{
			yield return Instance.RunBuildStage<CreateMergeRoomInstances_BuildStage>(BiomeControllers[biome]);
		}
		yield return Instance.RunBuildStage<SpawnConditionalObjects_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<MergeTerrain_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<CreateCameras_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<CreateSky_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<CreateWeather_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<CreateTransitionPoints_BuildStage>(BiomeControllers[biome]);
		yield return Instance.RunBuildStage<MoveRoomsToRoot_BuildStage>(BiomeControllers[biome]);
		foreach (BaseRoom room in BiomeControllers[biome].Rooms)
		{
			room.gameObject.SetActive(value: false);
		}
		BiomeEventArgs eventArgs = new BiomeEventArgs(biome);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.BiomeCreationComplete, Instance, eventArgs);
	}

	public static BiomeController GetBiomeController(BiomeType biome)
	{
		if (BiomeControllers != null && BiomeControllers.ContainsKey(biome))
		{
			return BiomeControllers[biome];
		}
		return null;
	}

	public void PreserveFirstBiomeOverride()
	{
		if (FirstBiomeOverride == BiomeType.None)
		{
			FirstBiomeOverride = m_firstBiome;
		}
	}

	private void OnBiomeCreatorFail()
	{
		State = BiomeBuildStateID.Failed;
		WorldBuildCompleteEventArgs eventArgs = new WorldBuildCompleteEventArgs(RNGSeedManager.GetCurrentSeed(base.gameObject.scene.name), RNGManager.GetSeed(RngID.BiomeCreation), BiomeControllers.Values.ToList(), isSuccess: false);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.WorldCreationFailed, this, eventArgs);
		PreserveFirstBiomeOverride();
		Cleanup();
	}

	private void Cleanup()
	{
		StopAllCoroutines();
		if (BiomeControllers == null)
		{
			return;
		}
		foreach (KeyValuePair<BiomeType, BiomeController> biomeController in BiomeControllers)
		{
			if ((bool)biomeController.Value.gameObject)
			{
				UnityEngine.Object.Destroy(biomeController.Value.gameObject);
			}
		}
		m_biomeControllers = null;
	}

	private void OnDestroy()
	{
		Instance = null;
	}

	public static void Reset()
	{
		State = BiomeBuildStateID.None;
		Instance.Cleanup();
		if (Instance.m_biomeCreator != null)
		{
			Instance.m_biomeCreator.Reset();
		}
	}

	private IEnumerator RunBuildStage<T>(BiomeController biomeController) where T : IBiomeBuildStage, new()
	{
		yield return new T().Run(biomeController);
	}
}
