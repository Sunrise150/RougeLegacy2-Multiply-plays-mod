using System;
using System.Collections.Generic;
using System.Linq;
using RLAudio;
using UnityEngine;

public class FairyRoomController : BaseSpecialRoomController
{
	public static EnemyType[] EnemyExceptionArray = new EnemyType[2]
	{
		EnemyType.BouncySpike,
		EnemyType.Eggplant
	};

	[SerializeField]
	private List<FairyRoomRuleEntry> m_fairyRoomRuleEntries;

	[SerializeField]
	private GameObject m_fairyRuleLocation;

	[SerializeField]
	private ChestSpawnController m_chestSpawnController;

	[SerializeField]
	private PropSpawnController m_fairyRoomTriggerSpawner;

	[SerializeField]
	private GameObject m_roomTriggerWall;

	private FairyRoomState m_state;

	private bool m_enemiesDeactivated;

	private bool m_isHiddenFairyRoom;

	private FairyRoomEnteredEventArgs m_fairyRoomEnteredEventArgs;

	private static List<EnemyController> m_deactivatedEnemies_STATIC = new List<EnemyController>();

	private Action m_onPlayerOpenedChest;

	private Action<FairyRoomRuleStateChangeEventArgs> m_onFairyRoomRuleStateChange;

	private Action<object, EventArgs> m_onPlayerDeath;

	public FairyRoomState State
	{
		get
		{
			return m_state;
		}
		private set
		{
			m_state = value;
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.FairyRoomStateChange, this, m_fairyRoomEnteredEventArgs);
		}
	}

	public List<FairyRoomRuleEntry> FairyRoomRuleEntries
	{
		get
		{
			return m_fairyRoomRuleEntries;
		}
		set
		{
			m_fairyRoomRuleEntries = value;
		}
	}

	public GameObject FairyRuleLocation
	{
		get
		{
			return m_fairyRuleLocation;
		}
		set
		{
			m_fairyRuleLocation = value;
		}
	}

	public ChestSpawnController ChestSpawnController
	{
		get
		{
			return m_chestSpawnController;
		}
		set
		{
			m_chestSpawnController = value;
		}
	}

	public PropSpawnController FairyRoomTriggerSpawner
	{
		get
		{
			return m_fairyRoomTriggerSpawner;
		}
		set
		{
			m_fairyRoomTriggerSpawner = value;
		}
	}

	public GameObject RoomTriggerWall
	{
		get
		{
			return m_roomTriggerWall;
		}
		set
		{
			m_roomTriggerWall = value;
		}
	}

	public ChestObj Chest { get; private set; }

	public override SpecialRoomType SpecialRoomType => SpecialRoomType.Fairy;

	public bool IsHiddenFairyRoom => m_isHiddenFairyRoom;

	protected override void Awake()
	{
		base.Awake();
		m_isHiddenFairyRoom = false;
		if (FairyRoomRuleEntries != null)
		{
			foreach (FairyRoomRuleEntry fairyRoomRuleEntry in FairyRoomRuleEntries)
			{
				if (fairyRoomRuleEntry.FairyRuleID == FairyRuleID.HiddenChest)
				{
					m_isHiddenFairyRoom = true;
					break;
				}
			}
		}
		m_fairyRoomEnteredEventArgs = new FairyRoomEnteredEventArgs(this);
		m_onPlayerOpenedChest = OnPlayerOpenedChest;
		m_onFairyRoomRuleStateChange = OnFairyRoomRuleStateChange;
		m_onPlayerDeath = OnPlayerDeath;
	}

	private void OnEnable()
	{
		SubscribeToFairyRuleStateChangeEvents(addListener: true);
	}

	private void OnDisable()
	{
		SubscribeToFairyRuleStateChangeEvents(addListener: false);
	}

	protected override void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		base.OnPlayerEnterRoom(sender, eventArgs);
		FairyRoomAudioEventController.DisableAudio = true;
		if (!Chest)
		{
			Chest = m_chestSpawnController.ChestInstance;
			if (!Chest && (bool)base.Room)
			{
				Debug.LogFormat("<color=red>[{0}] No Chest was spawned in Room ({0}). Fairy Rooms require a Chest</color>", base.Room.gameObject.name);
			}
		}
		Chest.OnOpenedRelay.AddListener(m_onPlayerOpenedChest);
		Chest.Interactable.SetIsInteractableActive(isActive: false);
		Chest.Interactable.ForceDisableInteractPrompt(forceDisable: false);
		Chest.SetOpacity(0f);
		if ((bool)m_roomTriggerWall)
		{
			m_roomTriggerWall.SetActive(value: true);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerEnterFairyRoom, this, m_fairyRoomEnteredEventArgs);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		m_deactivatedEnemies_STATIC.Clear();
		if (!m_isHiddenFairyRoom)
		{
			DeactivateAllEnemies();
		}
		if (!base.IsRoomComplete)
		{
			if (!m_fairyRoomTriggerSpawner)
			{
				StartFairyRoom();
			}
		}
		else
		{
			FairyRoomTriggered();
			if (Chest.IsOpen)
			{
				SetAllFairyRulesPassed();
			}
			else
			{
				SetAllFairyRulesFailed();
			}
		}
		FairyRoomAudioEventController.DisableAudio = false;
	}

	private void FairyRoomTriggered()
	{
		if ((bool)m_roomTriggerWall)
		{
			m_roomTriggerWall.SetActive(value: false);
		}
		Chest.SetOpacity(1f);
		if (!Chest.IsOpen)
		{
			Chest.Interactable.SetIsInteractableActive(isActive: true);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerFairyRoomTriggered, this, m_fairyRoomEnteredEventArgs);
	}

	public void StartFairyRoom()
	{
		if (State == FairyRoomState.Failed || State == FairyRoomState.Passed || FairyRoomRuleEntries == null || FairyRoomRuleEntries.Count <= 0)
		{
			return;
		}
		FairyRoomTriggered();
		ActivateAllEnemies();
		if (State == FairyRoomState.NotRunning)
		{
			State = FairyRoomState.Running;
			if ((bool)Chest)
			{
				if (FairyRoomRuleEntries.Any((FairyRoomRuleEntry entry) => entry.FairyRule.LockChestAtStart))
				{
					Chest.SetChestLockState(ChestLockState.Locked);
				}
				else
				{
					Chest.SetChestLockState(ChestLockState.Unlocked);
				}
			}
			RunAllFairyRules(areRunning: true);
		}
		else if (State == FairyRoomState.Running)
		{
			Debug.LogFormat("<color=red>[{0}] Fairy Room Controller's State was (Running) when Player entered Room. Valid States are (NotRunning, Passed and Failed)</color>", this);
		}
	}

	private void OnPlayerOpenedChest()
	{
		SetAllFairyRulesPassed();
		RoomCompleted();
	}

	protected override void OnPlayerExitRoom(object sender, RoomViaDoorEventArgs eventArgs)
	{
		if ((bool)Chest)
		{
			Chest.OnOpenedRelay.RemoveListener(m_onPlayerOpenedChest);
		}
		if (!m_isHiddenFairyRoom)
		{
			if (State == FairyRoomState.Running)
			{
				SetAllFairyRulesFailed();
			}
		}
		else if (FairyRoomRuleEntries != null && FairyRoomRuleEntries.Count > 0)
		{
			if (State != FairyRoomState.Failed && State != FairyRoomState.Passed)
			{
				State = FairyRoomState.NotRunning;
			}
			RunAllFairyRules(areRunning: false);
			ResetAllFairyRules();
		}
		OnExitReactivateEnemies();
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.PlayerExitFairyRoom, this, m_fairyRoomEnteredEventArgs);
	}

	private void OnFairyRoomRuleStateChange(FairyRoomRuleStateChangeEventArgs eventArgs)
	{
		if (eventArgs.Rule.State == FairyRoomState.Passed)
		{
			bool flag = false;
			foreach (FairyRoomRuleEntry fairyRoomRuleEntry in FairyRoomRuleEntries)
			{
				if (fairyRoomRuleEntry.FairyRule.State == FairyRoomState.Running)
				{
					flag = true;
					break;
				}
			}
			if (!flag)
			{
				PlayerPassed();
			}
			else
			{
				bool flag2 = false;
				foreach (FairyRoomRuleEntry fairyRoomRuleEntry2 in FairyRoomRuleEntries)
				{
					if (fairyRoomRuleEntry2.FairyRule.State == FairyRoomState.Running && fairyRoomRuleEntry2.FairyRule.LockChestAtStart)
					{
						flag2 = true;
						break;
					}
				}
				if ((bool)Chest)
				{
					if (flag2)
					{
						Chest.SetChestLockState(ChestLockState.Locked);
					}
					else
					{
						Chest.SetChestLockState(ChestLockState.Unlocked);
					}
				}
			}
		}
		else if (eventArgs.Rule.State == FairyRoomState.Failed)
		{
			PlayerFailed();
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.FairyRoomRuleStateChange, this, eventArgs);
	}

	private void SubscribeToFairyRuleStateChangeEvents(bool addListener)
	{
		foreach (FairyRoomRuleEntry fairyRoomRuleEntry in FairyRoomRuleEntries)
		{
			if (fairyRoomRuleEntry.FairyRule != null)
			{
				if (addListener)
				{
					fairyRoomRuleEntry.FairyRule.StateChangeRelay.AddListener(m_onFairyRoomRuleStateChange);
				}
				else
				{
					fairyRoomRuleEntry.FairyRule.StateChangeRelay.RemoveListener(m_onFairyRoomRuleStateChange);
				}
			}
		}
		if (addListener)
		{
			Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		}
		else
		{
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		}
	}

	private void SetAllFairyRulesFailed()
	{
		foreach (FairyRoomRuleEntry fairyRoomRuleEntry in FairyRoomRuleEntries)
		{
			if (fairyRoomRuleEntry.FairyRule != null)
			{
				fairyRoomRuleEntry.FairyRule.SetIsFailed();
			}
		}
	}

	private void SetAllFairyRulesPassed()
	{
		foreach (FairyRoomRuleEntry fairyRoomRuleEntry in FairyRoomRuleEntries)
		{
			if (fairyRoomRuleEntry.FairyRule != null)
			{
				fairyRoomRuleEntry.FairyRule.SetIsPassed();
			}
		}
		SubscribeToFairyRuleStateChangeEvents(addListener: false);
	}

	private void DeactivateAllEnemies()
	{
		EnemySpawnController[] enemySpawnControllers = base.Room.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (!(enemySpawnController.EnemyInstance == null) && !enemySpawnController.EnemyInstance.Equals(null) && !enemySpawnController.IsDead)
			{
				EnemyController enemyInstance = enemySpawnController.EnemyInstance;
				if (enemyInstance.IsCulled && enemyInstance.IsInitialized)
				{
					StartCoroutine(EnemyManager.SetEnemyCulled(enemyInstance, culled: false));
				}
				enemyInstance.DisableCulling = true;
				enemyInstance.HitboxController.DisableAllCollisions = true;
				enemyInstance.Animator.enabled = false;
				enemyInstance.CharacterCorgi.enabled = false;
				enemyInstance.LogicController.LogicScript.enabled = false;
				enemyInstance.LogicController.StopAllLogic(resetCurrentState: true);
				enemyInstance.SetVelocity(0f, 0f, additive: false);
				enemyInstance.LogicController.enabled = false;
				enemyInstance.Visuals.SetActive(value: false);
				enemyInstance.ActivatedByFairyRoomTrigger = true;
				m_deactivatedEnemies_STATIC.Add(enemyInstance);
			}
		}
		m_enemiesDeactivated = true;
	}

	private void OnExitReactivateEnemies()
	{
		foreach (EnemyController item in m_deactivatedEnemies_STATIC)
		{
			item.DisableCulling = false;
			item.HitboxController.DisableAllCollisions = false;
			item.Animator.enabled = true;
			item.CharacterCorgi.enabled = true;
			item.LogicController.LogicScript.enabled = true;
			item.LogicController.enabled = true;
			item.Visuals.SetActive(value: true);
			item.ActivatedByFairyRoomTrigger = false;
			item.gameObject.SetActive(value: false);
		}
		m_deactivatedEnemies_STATIC.Clear();
	}

	private void OnPlayerDeath(object sender, EventArgs args)
	{
		OnExitReactivateEnemies();
	}

	private void ActivateAllEnemies()
	{
		if (!m_enemiesDeactivated)
		{
			return;
		}
		EnemySpawnController[] enemySpawnControllers = base.Room.SpawnControllerManager.EnemySpawnControllers;
		foreach (EnemySpawnController enemySpawnController in enemySpawnControllers)
		{
			if (!enemySpawnController.IsNativeNull() && !enemySpawnController.IsDead)
			{
				EnemyController enemyInstance = enemySpawnController.EnemyInstance;
				enemyInstance.Animator.enabled = true;
				enemyInstance.CharacterCorgi.enabled = true;
				enemyInstance.LogicController.LogicScript.enabled = true;
				enemyInstance.LogicController.enabled = true;
				enemyInstance.ForceFaceTarget();
				StartCoroutine(EnemyManager.RunSummonAnimCoroutine(enemyInstance, 3f, disableEnemyOffset: true));
			}
		}
		m_enemiesDeactivated = false;
	}

	private void RunAllFairyRules(bool areRunning)
	{
		foreach (FairyRoomRuleEntry fairyRoomRuleEntry in FairyRoomRuleEntries)
		{
			if (fairyRoomRuleEntry.FairyRule != null)
			{
				if (areRunning)
				{
					fairyRoomRuleEntry.FairyRule.RunRule(this);
				}
				else
				{
					fairyRoomRuleEntry.FairyRule.StopRule();
				}
			}
		}
	}

	private void ResetAllFairyRules()
	{
		foreach (FairyRoomRuleEntry fairyRoomRuleEntry in FairyRoomRuleEntries)
		{
			if (fairyRoomRuleEntry.FairyRule != null)
			{
				fairyRoomRuleEntry.FairyRule.ResetRule();
			}
		}
	}

	private void PlayerPassed(bool setRoomComplete = true)
	{
		State = FairyRoomState.Passed;
		if ((bool)Chest && Chest.IsLocked && !Chest.IsOpen)
		{
			Chest.SetChestLockState(ChestLockState.Unlocked);
		}
		RunAllFairyRules(areRunning: false);
		if (setRoomComplete)
		{
			RoomCompleted();
		}
	}

	private void PlayerFailed(bool setRoomComplete = true)
	{
		State = FairyRoomState.Failed;
		if ((bool)Chest && Chest.LockState != ChestLockState.Failed && !Chest.IsOpen)
		{
			Chest.SetChestLockState(ChestLockState.Failed);
		}
		RunAllFairyRules(areRunning: false);
		if (setRoomComplete)
		{
			RoomCompleted();
		}
	}

	protected override void OnRoomDataSaved(bool exitingToMainMenu)
	{
		if (!m_isHiddenFairyRoom && State == FairyRoomState.Running && !exitingToMainMenu)
		{
			base.IsRoomComplete = true;
		}
		base.OnRoomDataSaved(exitingToMainMenu);
	}
}
