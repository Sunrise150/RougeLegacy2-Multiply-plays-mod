using System;
using System.Collections;
using System.Diagnostics;
using GameEventTracking;
using SceneManagement_RL;
using UnityEngine;

[RequireComponent(typeof(BiomeCreator))]
public class LevelManager_RL : MonoBehaviour
{
	[SerializeField]
	private bool m_instantiatePlayer = true;

	[SerializeField]
	private int m_maxNumberAttempts = 10;

	[SerializeField]
	private LevelManagerMode m_mode;

	[SerializeField]
	private Vector2Int m_seedRange = new Vector2Int(0, 0);

	[SerializeField]
	private float m_timeBetweenLoops = 5f;

	[SerializeField]
	private bool m_exportRoomLibrary;

	[SerializeField]
	private int m_testProfileIndex = -1;

	[SerializeField]
	private bool m_generateRoomWeights;

	private static Coroutine m_buildWorldCoroutine;

	private static LevelManager_RL m_instance;

	private WorldBuilder m_worldBuilder;

	private WaitUntil m_waitUntilWorldBuilderNotRunning;

	private WaitForSeconds m_waitBetweenLoops;

	private bool m_resetSeedOnFail;

	public bool IsComplete { get; private set; }

	public LevelManagerMode Mode
	{
		get
		{
			return LevelManagerMode.Standard;
		}
		private set
		{
			m_mode = value;
		}
	}

	public bool InstantiatePlayer => true;

	public bool GenerateRoomWeights
	{
		get
		{
			if (Mode == LevelManagerMode.Debug)
			{
				return m_generateRoomWeights;
			}
			return false;
		}
		set
		{
			m_generateRoomWeights = value;
		}
	}

	private void Awake()
	{
		if (!m_instance)
		{
			m_instance = this;
			m_worldBuilder = base.gameObject.GetComponent<WorldBuilder>();
			m_resetSeedOnFail = true;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Reset();
		if (Mode == LevelManagerMode.Standard)
		{
			m_buildWorldCoroutine = StartCoroutine(BuildWorldCoroutine());
		}
		else
		{
			m_buildWorldCoroutine = StartCoroutine(BuildDebugWorldCoroutine());
		}
	}

	private IEnumerator BuildDebugWorldCoroutine()
	{
		UnityEngine.Debug.LogFormat("| {0} | <color=yellow>WARNING: Debug Mode is enabled! Unless you're Paul, you probably don't want this.</color>", this);
		yield return new WaitUntil(() => RNGManager.IsInstantiated);
		Stopwatch stopWatch = new Stopwatch();
		stopWatch.Start();
		for (int seed = m_seedRange.x; seed <= m_seedRange.y; seed++)
		{
			if (seed != m_seedRange.x)
			{
				if (m_waitBetweenLoops == null)
				{
					m_waitBetweenLoops = new WaitForSeconds(m_timeBetweenLoops);
				}
				yield return m_waitBetweenLoops;
			}
			Reset();
			RNGManager.Reset(seed);
			WorldBuilder.CreateWorld();
			if (m_waitUntilWorldBuilderNotRunning == null)
			{
				m_waitUntilWorldBuilderNotRunning = new WaitUntil(() => WorldBuilder.State != BiomeBuildStateID.Running);
			}
			yield return m_waitUntilWorldBuilderNotRunning;
			IsComplete = true;
		}
		stopWatch.Stop();
		m_buildWorldCoroutine = null;
	}

	private IEnumerator BuildWorldCoroutine()
	{
		if (!m_resetSeedOnFail)
		{
			m_maxNumberAttempts = 1;
		}
		int attemptCount = 0;
		while (WorldBuilder.State != BiomeBuildStateID.Complete && attemptCount < m_maxNumberAttempts)
		{
			if (m_testProfileIndex != -1)
			{
				if (true)
				{
					LoadStageSaveData(m_testProfileIndex);
				}
			}
			else if ((SceneLoader_RL.PreviousScene == SceneLoadingUtility.GetSceneName(SceneID.MainMenu) || (SceneLoader_RL.PreviousScene != SceneLoadingUtility.GetSceneName(SceneID.MainMenu) && SaveManager.PlayerSaveData.CastleLockState != 0)) && SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.World))
			{
				if (SceneLoader_RL.PreviousScene != SceneLoadingUtility.GetSceneName(SceneID.MainMenu) && SaveManager.PlayerSaveData.CastleLockState != 0)
				{
					RNGSeedManager.SetMasterSeedOverride(SaveManager.PlayerSaveData.PreviousMasterSeed);
				}
				else
				{
					RNGSeedManager.SetMasterSeedOverride(SaveManager.PlayerSaveData.MasterSeed);
				}
				RNGSeedManager.GenerateNewSeed(SceneLoadingUtility.GetSceneName(SceneID.World));
				RNGManager.InitializeRandomNumberGenerators(RNGSeedManager.MasterSeedOverride);
				RNGManager.PrintSeedsToConsole();
				LoadStageSaveData(SaveManager.CurrentProfile);
			}
			else
			{
				if (SceneLoadingUtility.ActiveScene.name == SceneLoadingUtility.GetSceneName(SceneID.World))
				{
					if (!RNGSeedManager.IsMasterSeedOverrideSet)
					{
						RNGSeedManager.SetMasterSeedOverride(Environment.TickCount);
					}
					RNGSeedManager.GenerateNewSeed(SceneLoadingUtility.GetSceneName(SceneID.World));
					RNGManager.InitializeRandomNumberGenerators(RNGSeedManager.GetCurrentSeed(SceneLoadingUtility.GetSceneName(SceneID.World)));
				}
				else
				{
					RNGManager.Reset();
				}
				WorldBuilder.CreateWorld();
			}
			if (m_waitUntilWorldBuilderNotRunning == null)
			{
				m_waitUntilWorldBuilderNotRunning = new WaitUntil(() => WorldBuilder.State != BiomeBuildStateID.Running);
			}
			yield return m_waitUntilWorldBuilderNotRunning;
			if (WorldBuilder.State == BiomeBuildStateID.Complete)
			{
				OnStandardBuildComplete();
			}
			else
			{
				OnStandardBuildFail();
			}
			attemptCount++;
		}
		m_buildWorldCoroutine = null;
	}

	private LOAD_RESULT LoadStageSaveData(int profileIndex)
	{
		object loadDataOutput = null;
		LOAD_RESULT num = SaveManager.LoadGameData(profileIndex, SaveDataType.Stage, out loadDataOutput);
		if (num == LOAD_RESULT.OK)
		{
			StageSaveData stageSaveData = (SaveManager.StageSaveData = loadDataOutput as StageSaveData);
			SaveManager.StageSaveData.CreateAllBiomeLookupTable();
			WorldBuilder.CreateWorld(stageSaveData);
		}
		return num;
	}

	private void OnDestroy()
	{
		m_instance = null;
		if (m_buildWorldCoroutine != null)
		{
			StopCoroutine(m_buildWorldCoroutine);
			m_buildWorldCoroutine = null;
		}
	}

	private void OnStandardBuildComplete()
	{
		IsComplete = true;
		if (InstantiatePlayer)
		{
			AnimatorUtility.ClearAnimatorTables();
			GC.Collect();
			if (!PlayerManager.IsInstantiated)
			{
				PlayerManager.GetPlayer();
			}
			m_worldBuilder.StartingRoom.PlacePlayerInRoom(null);
		}
		if (PlayerManager.IsInstantiated)
		{
			PlayerController playerController = PlayerManager.GetPlayerController();
			if (!playerController.IsFacingRight)
			{
				playerController.CharacterCorgi.Flip();
			}
			if (!RewiredMapController.IsInCutscene)
			{
				StartCoroutine(WaitForTransitionPlayerGravityCoroutine(playerController));
			}
		}
		MapController.SetCameraFollowIsOn(isOn: true);
		MapController.FollowMode = CameraFollowMode.Constant;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.LevelManagerStateChange, this, new LevelManagerStateChangeEventArgs(LevelManagerState.BuildComplete));
	}

	private IEnumerator WaitForTransitionPlayerGravityCoroutine(PlayerController playerController)
	{
		LoadingGate_SceneTransition gateTransition = TransitionLibrary.GetTransitionInstance(SceneLoader_RL.CurrentTransitionID) as LoadingGate_SceneTransition;
		if ((bool)gateTransition)
		{
			while (!gateTransition.DropPlayer)
			{
				yield return null;
			}
		}
		else
		{
			float delay = Time.unscaledTime + 1f;
			while (Time.unscaledTime < delay)
			{
				yield return null;
			}
		}
		playerController.ControllerCorgi.GravityActive(state: true);
	}

	private static void OnStandardBuildFail()
	{
		Reset();
		WorldBuilder.Instance.PreserveFirstBiomeOverride();
		if (m_instance.Mode == LevelManagerMode.Standard && m_instance.m_resetSeedOnFail)
		{
			if (RNGSeedManager.IsOverrideSet)
			{
				RNGSeedManager.ClearOverride();
			}
			RNGSeedManager.GenerateNewSeed(m_instance.gameObject.scene.name);
		}
	}

	private static void Reset()
	{
		m_instance.IsComplete = false;
		if (PlayerManager.IsInstantiated)
		{
			PlayerManager.GetPlayerController().ResetCharacter();
		}
		GridController.Reset();
		WorldBuilder.Reset();
		if (EnemyManager.IsInitialized)
		{
			EnemyManager.DisableAllEnemies();
		}
		if (MapController.IsInitialized)
		{
			MapController.Reset();
		}
		if (ProjectileManager.IsInitialized)
		{
			ProjectileManager.Reset();
		}
		if (ItemDropManager.IsInitialized)
		{
			ItemDropManager.Reset();
		}
		if (GameEventTrackerManager.IsInstantiated)
		{
			GameEventTrackerManager.Reset();
		}
	}
}
