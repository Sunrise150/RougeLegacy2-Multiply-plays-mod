using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using RLWorldCreation;
using SceneManagement_RL;
using Sigtrap.Relays;
using UnityEngine;

public abstract class BaseRoom : MonoBehaviour, ISummoner, ILevelConsumer
{
	protected CinemachineVirtualCameraManager m_cinemachineCamera;

	protected Collider2D m_collider;

	protected List<BaseRoom> m_connectedRooms = new List<BaseRoom>();

	protected List<Door> m_doors;

	protected BiomeType m_isInBiome;

	protected RoomViaDoorEventArgs m_roomViaDoorEventArgs;

	protected SpecialRoomType m_specialRoomType;

	protected Relay<object, RoomViaDoorEventArgs> m_playerEnterRelay = new Relay<object, RoomViaDoorEventArgs>();

	protected Relay<object, RoomViaDoorEventArgs> m_playerExitRelay = new Relay<object, RoomViaDoorEventArgs>();

	protected Relay<object, EventArgs> m_roomInitializedRelay = new Relay<object, EventArgs>();

	protected Relay<object, EventArgs> m_roomDestroyedRelay = new Relay<object, EventArgs>();

	public IRelayLink<object, RoomViaDoorEventArgs> PlayerEnterRelay => m_playerEnterRelay.link;

	public IRelayLink<object, RoomViaDoorEventArgs> PlayerExitRelay => m_playerExitRelay.link;

	public IRelayLink<object, EventArgs> RoomInitializedRelay => m_roomInitializedRelay.link;

	public IRelayLink<object, EventArgs> RoomDestroyedRelay => m_roomDestroyedRelay.link;

	public abstract bool SpawnedAsEasyRoom { get; }

	public abstract BiomeArtData BiomeArtDataOverride { get; }

	public BackgroundPoolEntry[] Backgrounds { get; protected set; }

	public BiomeType AppearanceBiomeType { get; protected set; }

	public abstract Bounds Bounds { get; }

	public Rect BoundsRect { get; protected set; }

	public int BiomeControllerIndex { get; protected set; }

	public CinemachineVirtualCameraManager CinemachineCamera
	{
		get
		{
			if (!m_cinemachineCamera)
			{
				m_cinemachineCamera = GetComponentInChildren<CinemachineVirtualCameraManager>();
			}
			return m_cinemachineCamera;
		}
	}

	public Collider2D Collider2D
	{
		get
		{
			if (!m_collider)
			{
				m_collider = GetComponent<Collider2D>();
			}
			return m_collider;
		}
	}

	public List<BaseRoom> ConnectedRooms
	{
		get
		{
			return m_connectedRooms;
		}
		protected set
		{
			m_connectedRooms = value;
		}
	}

	public virtual bool DisableRoomLetterBoxing => false;

	public virtual bool DisableRoomSaving => false;

	public virtual List<Door> Doors
	{
		get
		{
			if (m_doors == null)
			{
				m_doors = GetComponentsInChildren<Door>().ToList();
			}
			return m_doors;
		}
	}

	public BiomeType BiomeType
	{
		get
		{
			return m_isInBiome;
		}
		set
		{
			m_isInBiome = value;
		}
	}

	public bool IsPlayerInRoom { get; protected set; }

	public abstract int Level { get; }

	public virtual RoomType RoomType { get; protected set; }

	public RoomSaveController SaveController { get; protected set; }

	public RoomSpawnControllerManager SpawnControllerManager { get; protected set; }

	public virtual SpecialRoomType SpecialRoomType => m_specialRoomType;

	public RoomTerrainManager TerrainManager { get; protected set; }

	public virtual bool AllowItemDrops { get; }

	protected virtual void OnDestroy()
	{
		if (Application.isPlaying)
		{
			ConnectedRooms = null;
			m_roomDestroyedRelay.Dispatch(this, EventArgs.Empty);
		}
	}

	public void SetSpawnControllerManager(RoomSpawnControllerManager spawnControllerManager)
	{
		SpawnControllerManager = spawnControllerManager;
	}

	public void SetTerrainManager(RoomTerrainManager terrainManager)
	{
		TerrainManager = terrainManager;
	}

	public void SetBiomeControllerIndex(int index)
	{
		BiomeControllerIndex = index;
	}

	public void InjectRoom()
	{
		IRoomConsumer[] componentsInChildren = base.gameObject.GetComponentsInChildren<IRoomConsumer>();
		for (int i = 0; i < componentsInChildren.Length; i++)
		{
			componentsInChildren[i].SetRoom(this);
		}
	}

	public void SetBackgroundPoolEntries(BackgroundPoolEntry[] backgrounds)
	{
		Backgrounds = backgrounds;
	}

	public abstract void SetLevel(int level);

	public void PlacePlayerInRoom(Door door)
	{
		PlacePlayerInRoom_Start();
		if (door != null)
		{
			PlayerManager.GetPlayerController().EnterRoom(door);
		}
		else
		{
			PlayerManager.GetPlayerController().EnterRoom(this);
		}
		PlacePlayerInRoom_End(door);
	}

	public void PlacePlayerInRoom(Vector3 localPosition)
	{
		PlacePlayerInRoom_Start();
		PlayerManager.GetPlayerController().EnterRoom(this, null, localPosition);
		PlacePlayerInRoom_End(null);
	}

	private void PlacePlayerInRoom_Start()
	{
		IsPlayerInRoom = true;
	}

	private void PlacePlayerInRoom_End(Door door)
	{
		RNGManager.SetSeed(RngID.SpecialProps_RoomSeed, GetSpecialPropSeed());
		InitializeSpawnControllerInstances();
		InitializeBackgrounds();
		SaveController.OnPlayerEnter_LoadStageData();
		if (m_roomViaDoorEventArgs == null)
		{
			m_roomViaDoorEventArgs = new RoomViaDoorEventArgs(this, door);
		}
		else
		{
			m_roomViaDoorEventArgs.Initialise(this, door);
		}
		CameraController.UpdateRoomCameraSettings(m_roomViaDoorEventArgs);
		BroadcastPlayerEnterRoomEvents(door);
	}

	protected abstract int GetDecoSeed();

	protected abstract int GetSpecialPropSeed();

	private void InitializeSpawnControllerInstances()
	{
		PropSpawnController[] propSpawnControllers = SpawnControllerManager.PropSpawnControllers;
		for (int i = 0; i < propSpawnControllers.Length; i++)
		{
			propSpawnControllers[i].InitializePropInstance();
		}
		EnemySpawnController[] enemySpawnControllers = SpawnControllerManager.EnemySpawnControllers;
		for (int i = 0; i < enemySpawnControllers.Length; i++)
		{
			enemySpawnControllers[i].InitializeEnemyInstance();
		}
		ChestSpawnController[] chestSpawnControllers = SpawnControllerManager.ChestSpawnControllers;
		for (int i = 0; i < chestSpawnControllers.Length; i++)
		{
			chestSpawnControllers[i].InitializeChestInstance();
		}
	}

	private void InitializeBackgrounds()
	{
		if (Backgrounds == null)
		{
			return;
		}
		BackgroundPoolEntry[] backgrounds = Backgrounds;
		foreach (BackgroundPoolEntry backgroundPoolEntry in backgrounds)
		{
			if ((bool)backgroundPoolEntry.BackgroundPrefab)
			{
				Background background = BackgroundsPoolManager.GetBackground(backgroundPoolEntry.BackgroundPrefab);
				if (!background)
				{
					background = BackgroundsPoolManager.GetBackground(backgroundPoolEntry.BackgroundPrefab);
					throw new Exception("Failed to find pooled background: " + backgroundPoolEntry.BackgroundPrefab.name + ". BackgroundsPoolManager did not create pools correctly.");
				}
				background.transform.SetParent(base.transform);
				background.transform.position = backgroundPoolEntry.Position;
				background.gameObject.SetActive(value: true);
			}
		}
	}

	private void BroadcastPlayerEnterRoomEvents(Door door)
	{
		m_playerEnterRelay.Dispatch(this, m_roomViaDoorEventArgs);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerEnterRoom, this, m_roomViaDoorEventArgs);
	}

	protected void OnPlayerExitViaDoor(object sender, DoorEventArgs eventArgs)
	{
		PlayerExit(eventArgs.Door);
	}

	public void PlayerExit(Door door)
	{
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (playerController.IsDead)
		{
			return;
		}
		if (playerController.CurrentHealth <= 0f && !ChallengeManager.IsInChallenge)
		{
			StartCoroutine(DelayEnterRoomCoroutine(door));
			return;
		}
		bool flag = true;
		if (door != null)
		{
			if (door.IsBiomeTransitionPoint)
			{
				BiomeTransitionController.Run(door);
				flag = false;
			}
			else if (door.ConnectedDoor != null && door.ConnectedDoor.Room != null)
			{
				Door connectedDoor = door.ConnectedDoor;
				BaseRoom room = connectedDoor.Room;
				bool num = room.Bounds.size.x > 160f || room.Bounds.size.y > 90f || Bounds.size.x > 160f || Bounds.size.y > 90f;
				bool flag2 = BiomeType == BiomeType.Tower && ((AppearanceBiomeType == BiomeType.Tower && room.AppearanceBiomeType == BiomeType.TowerExterior) || (AppearanceBiomeType == BiomeType.TowerExterior && room.AppearanceBiomeType == BiomeType.Tower));
				if (num || flag2)
				{
					flag = false;
					SceneLoader_RL.RunTransitionWithLogic(LargeRoomTransitionCoroutine(door, connectedDoor, room), TransitionID.QuickSwipe, cleanup: false);
				}
				else
				{
					BroadcastPlayerExitRoomEvents(door);
					if (flag)
					{
						flag = false;
						base.gameObject.SetActive(value: false);
					}
					room.PlacePlayerInRoom(connectedDoor);
				}
			}
			else if (door.ConnectedDoor == null)
			{
				Debug.LogFormat("<color=red>You entered a Door whose Connected Door is null</color>");
			}
			else if (door.ConnectedDoor.Room == null)
			{
				Debug.LogFormat("<color=red>You entered a Door whose Connected Door's Room is null</color>");
			}
		}
		else
		{
			BroadcastPlayerExitRoomEvents(null);
		}
		if (flag)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private IEnumerator DelayEnterRoomCoroutine(Door door)
	{
		yield return null;
		while (GameManager.IsGamePaused)
		{
			yield return null;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		if (!playerController.IsDead && playerController.CurrentHealth > 0f)
		{
			PlayerExit(door);
		}
	}

	private IEnumerator LargeRoomTransitionCoroutine(Door doorBeingExited, Door doorBeingEntered, BaseRoom roomBeingEntered)
	{
		BroadcastPlayerExitRoomEvents(doorBeingExited);
		base.gameObject.SetActive(value: false);
		roomBeingEntered.PlacePlayerInRoom(doorBeingEntered);
		yield break;
	}

	public void BroadcastPlayerExitRoomEvents(Door door)
	{
		IsPlayerInRoom = false;
		if (m_roomViaDoorEventArgs == null)
		{
			m_roomViaDoorEventArgs = new RoomViaDoorEventArgs(this, door);
		}
		else
		{
			m_roomViaDoorEventArgs.Initialise(this, door);
		}
		SaveController.OnPlayerExit_SaveStageData();
		m_playerExitRelay.Dispatch(this, m_roomViaDoorEventArgs);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerExitRoom, this, m_roomViaDoorEventArgs);
	}

	[SpecialName]
	GameObject ISummoner.get_gameObject()
	{
		return base.gameObject;
	}
}
