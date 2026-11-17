using System.Collections.Generic;
using SceneManagement_RL;
using Sigtrap.Relays;
using UnityEngine;

public class RoomSaveController
{
	private Relay<bool> m_onRoomDataSavedRelay = new Relay<bool>();

	private Relay m_onRoomDataLoadedRelay = new Relay();

	private List<PropSpawnController> m_breakablePropsList;

	public IRelayLink<bool> OnRoomDataSavedRelay => m_onRoomDataSavedRelay.link;

	public IRelayLink OnRoomDataLoadedRelay => m_onRoomDataLoadedRelay.link;

	public static bool DisableCutsceneSaving { get; set; }

	public bool CanSaveRoom
	{
		get
		{
			if (!DisableCutsceneSaving && Room.RoomType != RoomType.Boss && Room.SpecialRoomType != SpecialRoomType.Boss && Room.SpecialRoomType != SpecialRoomType.Heirloom && Room.SpecialRoomType != SpecialRoomType.Clown)
			{
				return !Room.DisableRoomSaving;
			}
			return false;
		}
	}

	public BaseRoom Room { get; }

	public RoomSaveController(BaseRoom room)
	{
		Room = room;
	}

	public void OnPlayerEnter_LoadStageData()
	{
		InitializeBreakablePropsList();
		RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(Room.BiomeType, Room.BiomeControllerIndex);
		bool flag = false;
		if (roomSaveData != null && !roomSaveData.IsEmpty)
		{
			if (m_breakablePropsList != null && roomSaveData.BreakableStates != null && m_breakablePropsList.Count != roomSaveData.BreakableStates.Length)
			{
				flag = true;
			}
			else if (roomSaveData.ChestStates != null && Room.SpawnControllerManager.ChestSpawnControllers.Length != roomSaveData.ChestStates.Length)
			{
				flag = true;
			}
			else if (roomSaveData.EnemyStates != null && Room.SpawnControllerManager.EnemySpawnControllers.Length != roomSaveData.EnemyStates.Length)
			{
				flag = true;
			}
			if (!flag)
			{
				int num = 0;
				PropSpawnController[] propSpawnControllers = Room.SpawnControllerManager.PropSpawnControllers;
				foreach (PropSpawnController propSpawnController in propSpawnControllers)
				{
					if (propSpawnController.CameraLayer == CameraLayer.Game && propSpawnController.BreakableDecoInstances != null)
					{
						num += propSpawnController.BreakableDecoInstances.Length;
					}
				}
				if (roomSaveData.DecoBreakableStates != null && num != roomSaveData.DecoBreakableStates.Length)
				{
					flag = true;
				}
			}
		}
		if (flag)
		{
			string text = Room.gameObject.name;
			if (Room is Room)
			{
				text = (Room as Room).RoomID.ToString();
			}
			Debug.LogFormat("<color=red>WARNING: | Room ID - {0} | BiomeControllerIndex - {1} | Biome - {2} | Identified a save file mismatch with the room's actual spawners. Recreating Room Save Data</color>", text, Room.BiomeControllerIndex, Room.BiomeType);
		}
		if (roomSaveData != null)
		{
			if (roomSaveData.IsEmpty || flag)
			{
				InitializeRoomSaveData(roomSaveData, flag);
				LoadRoomState(roomSaveData);
			}
			else
			{
				LoadRoomState(roomSaveData);
			}
		}
	}

	private void InitializeBreakablePropsList()
	{
		if (m_breakablePropsList != null)
		{
			return;
		}
		m_breakablePropsList = new List<PropSpawnController>();
		PropSpawnController[] propSpawnControllers = Room.SpawnControllerManager.PropSpawnControllers;
		foreach (PropSpawnController propSpawnController in propSpawnControllers)
		{
			if (!(propSpawnController.PropInstance == null) && propSpawnController.CameraLayer == CameraLayer.Game && propSpawnController.IsBreakable)
			{
				m_breakablePropsList.Add(propSpawnController);
			}
		}
	}

	private void InitializeRoomSaveData(RoomSaveData roomData, bool forceReinitialize)
	{
		if (forceReinitialize)
		{
			roomData.RoomVisited = false;
			roomData.IsRoomComplete = false;
		}
		if (roomData.BreakableStates == null || forceReinitialize)
		{
			roomData.BreakableStates = new RLBreakableSaveState[m_breakablePropsList.Count];
			for (int i = 0; i < m_breakablePropsList.Count; i++)
			{
				roomData.BreakableStates[i] = new RLBreakableSaveState();
			}
		}
		if (roomData.DecoBreakableStates == null || forceReinitialize)
		{
			int num = 0;
			PropSpawnController[] propSpawnControllers = Room.SpawnControllerManager.PropSpawnControllers;
			foreach (PropSpawnController propSpawnController in propSpawnControllers)
			{
				if (propSpawnController.CameraLayer == CameraLayer.Game && propSpawnController.BreakableDecoInstances != null)
				{
					num += propSpawnController.BreakableDecoInstances.Length;
				}
			}
			roomData.DecoBreakableStates = new RLBreakableSaveState[num];
			for (int k = 0; k < num; k++)
			{
				roomData.DecoBreakableStates[k] = new RLBreakableSaveState();
			}
		}
		if (roomData.ChestStates == null || forceReinitialize)
		{
			int num2 = Room.SpawnControllerManager.ChestSpawnControllers.Length;
			roomData.ChestStates = new RLSaveState[num2];
			for (int l = 0; l < num2; l++)
			{
				roomData.ChestStates[l] = new RLSaveState();
			}
		}
		if (roomData.EnemyStates == null || forceReinitialize)
		{
			int num3 = Room.SpawnControllerManager.EnemySpawnControllers.Length;
			roomData.EnemyStates = new RLSaveState[num3];
			for (int m = 0; m < num3; m++)
			{
				roomData.EnemyStates[m] = new RLSaveState();
			}
		}
	}

	public void SaveRoomState(RoomSaveData roomData, bool exitingToMainMenu)
	{
		if (roomData == null)
		{
			return;
		}
		roomData.RoomVisited = true;
		for (int i = 0; i < m_breakablePropsList.Count; i++)
		{
			Prop propInstance = m_breakablePropsList[i].PropInstance;
			if ((bool)propInstance && (bool)propInstance.Breakable)
			{
				roomData.BreakableStates[i].IsStateActive = !propInstance.Breakable.IsBroken;
				roomData.BreakableStates[i].AttackerIsOnRight = propInstance.Breakable.AttackerIsOnRight;
			}
		}
		int num = 0;
		PropSpawnController[] propSpawnControllers = Room.SpawnControllerManager.PropSpawnControllers;
		foreach (PropSpawnController propSpawnController in propSpawnControllers)
		{
			if (propSpawnController.CameraLayer != CameraLayer.Game || propSpawnController.BreakableDecoInstances == null)
			{
				continue;
			}
			Prop[] breakableDecoInstances = propSpawnController.BreakableDecoInstances;
			foreach (Prop prop in breakableDecoInstances)
			{
				if ((bool)prop && (bool)prop.Breakable)
				{
					roomData.DecoBreakableStates[num].IsStateActive = !prop.Breakable.IsBroken;
					num++;
				}
			}
		}
		ChestSpawnController[] chestSpawnControllers = Room.SpawnControllerManager.ChestSpawnControllers;
		for (int l = 0; l < chestSpawnControllers.Length; l++)
		{
			if (!chestSpawnControllers[l].ShouldSpawn || !chestSpawnControllers[l].ChestInstance)
			{
				roomData.ChestStates[l].IsSpawned = false;
				roomData.ChestStates[l].IsStateActive = false;
			}
			else
			{
				roomData.ChestStates[l].IsSpawned = true;
				roomData.ChestStates[l].IsStateActive = !chestSpawnControllers[l].ChestInstance.IsOpen;
			}
		}
		bool flag = false;
		if (Room.SpecialRoomType == SpecialRoomType.Fairy)
		{
			FairyRoomController component = Room.gameObject.GetComponent<FairyRoomController>();
			if ((bool)component && component.FairyRoomRuleEntries != null)
			{
				foreach (FairyRoomRuleEntry fairyRoomRuleEntry in component.FairyRoomRuleEntries)
				{
					if (fairyRoomRuleEntry.FairyRuleID == FairyRuleID.HiddenChest)
					{
						flag = true;
						break;
					}
				}
			}
		}
		if (Room.SpecialRoomType != SpecialRoomType.Fairy || flag)
		{
			EnemySpawnController[] enemySpawnControllers = Room.SpawnControllerManager.EnemySpawnControllers;
			for (int m = 0; m < enemySpawnControllers.Length; m++)
			{
				roomData.EnemyStates[m].IsStateActive = !enemySpawnControllers[m].IsDead;
			}
		}
		m_onRoomDataSavedRelay.Dispatch(exitingToMainMenu);
	}

	public void LoadRoomState(RoomSaveData roomData)
	{
		if (roomData == null)
		{
			Debug.LogFormat("<color=red>| {0} | <b>LoadState</b> method was passed null Room Save Data</color>, this");
			return;
		}
		for (int i = 0; i < m_breakablePropsList.Count; i++)
		{
			Prop propInstance = m_breakablePropsList[i].PropInstance;
			if (propInstance != null)
			{
				propInstance.Breakable.AttackerIsOnRight = roomData.BreakableStates[i].AttackerIsOnRight;
				bool isBroken = !roomData.BreakableStates[i].IsStateActive;
				propInstance.Breakable.ForceBrokenState(isBroken);
			}
		}
		int num = 0;
		PropSpawnController[] propSpawnControllers = Room.SpawnControllerManager.PropSpawnControllers;
		foreach (PropSpawnController propSpawnController in propSpawnControllers)
		{
			if (propSpawnController.CameraLayer == CameraLayer.Game && propSpawnController.BreakableDecoInstances != null)
			{
				Prop[] breakableDecoInstances = propSpawnController.BreakableDecoInstances;
				for (int k = 0; k < breakableDecoInstances.Length; k++)
				{
					breakableDecoInstances[k].Breakable.ForceBrokenState(!roomData.DecoBreakableStates[num].IsStateActive);
					num++;
				}
			}
		}
		ChestSpawnController[] chestSpawnControllers = Room.SpawnControllerManager.ChestSpawnControllers;
		for (int l = 0; l < chestSpawnControllers.Length; l++)
		{
			if (chestSpawnControllers[l].ShouldSpawn && !roomData.ChestStates[l].IsStateActive)
			{
				chestSpawnControllers[l].ChestInstance.ForceOpenChest();
			}
		}
		EnemySpawnController[] enemySpawnControllers = Room.SpawnControllerManager.EnemySpawnControllers;
		for (int m = 0; m < enemySpawnControllers.Length; m++)
		{
			if (!roomData.EnemyStates[m].IsStateActive)
			{
				enemySpawnControllers[m].ForceEnemyDead();
			}
			else
			{
				enemySpawnControllers[m].ResetIsDead();
			}
		}
		m_onRoomDataLoadedRelay.Dispatch();
	}

	public void OnPlayerExit_SaveStageData()
	{
		SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(SaveManager.CurrentProfile);
		bool flag = SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.World);
		if (CanSaveRoom)
		{
			RoomSaveData roomSaveData = SaveManager.StageSaveData.GetRoomSaveData(Room.BiomeType, Room.BiomeControllerIndex);
			SaveRoomState(roomSaveData, exitingToMainMenu: false);
			if (flag)
			{
				SaveManager.SaveCurrentProfileGameData(saveBatch, SaveDataType.Stage, SavingType.FileOnly, ignoreTimeRestriction: false);
			}
		}
		if (flag)
		{
			SaveManager.PlayerSaveData.SecondsPlayed += (uint)GameTimer.TotalSessionAccumulatedTime;
			GameTimer.ClearSessionAccumulatedTime();
			SaveManager.SaveCurrentProfileGameData(saveBatch, SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: false);
			SaveManager.SaveCurrentProfileGameData(saveBatch, SaveDataType.GameMode, SavingType.FileOnly, ignoreTimeRestriction: false);
		}
		saveBatch.End();
	}
}
