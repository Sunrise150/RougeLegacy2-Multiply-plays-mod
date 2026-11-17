using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;
using GameEventTracking;
using RLAudio;
using Rewired;
using SceneManagement_RL;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
	private const string SAVE_PATH_SAVE_FOLDER = "Saves";

	private const string SAVE_PATH_PROFILE_FOLDER = "Profile";

	public const string DATE_FORMAT = "dd_MM_yyyy__HH_mm_ss";

	private const string FOREST_LILY_ROOM1_ID = "Levels Forest Mandatory 1";

	private const string FOREST_LILY_ROOM2_ID = "Levels Forest Mandatory 2";

	private const string FOREST_LILY_ROOM3_ID = "Levels Forest Mandatory 3";

	private const string CAVE_WHITE_KEY_ROOM_ID = "Levels Cave Mandatory 22";

	private const string CAVE_BLACK_KEY_ROOM_ID = "Levels Cave Mandatory 19";

	public static PlayerSaveData PlayerSaveData = new PlayerSaveData();

	public static StageSaveData StageSaveData = new StageSaveData();

	public static EquipmentSaveData EquipmentSaveData = new EquipmentSaveData();

	public static LineageSaveData LineageSaveData = new LineageSaveData();

	public static ModeSaveData ModeSaveData = new ModeSaveData();

	private static Task m_lastAsyncTask;

	private static bool m_isRunning;

	private static bool m_hasLevelEditorSaveFailedMessageBeenLogged = false;

	private static bool m_hasMainMenuSaveFailedMessageBeenLogged = false;

	private static SaveManager m_instance;

	private List<string>[] m_backFilePathsArray = new List<string>[5];

	private List<string> m_backupFilesListHelper = new List<string>();

	private bool m_checkedForSaveDirectories;

	private Action<MonoBehaviour, EventArgs> m_onWorldCreationComplete_UpdateSaveData;

	private Action<MonoBehaviour, EventArgs> m_onLevelEditorWorldCreationComplete;

	private Action<MonoBehaviour, EventArgs> m_onBiomeCreationComplete_UpdateSaveData;

	private const string CONFIG_FILE_NAME = "GameConfig.ini";

	public static ConfigSaveData ConfigData = new ConfigSaveData();

	private const string CONTROLLERMAP_FILE_NAME = "ControllerMapV3RL1.xml";

	private const string PROFILE_CONFIG_FILE_NAME = "ProfileConfig.ini";

	public static ProfileConfigSaveData ProfileConfigData = new ProfileConfigSaveData();

	private const string TRAILER_CONFIG_FILE_NAME = "TrailerConfig.ini";

	public static TrailerConfigData TrailerConfigData = new TrailerConfigData();

	private static SaveManager Instance => m_instance;

	public static bool IsRunning
	{
		get
		{
			return m_isRunning;
		}
		private set
		{
			m_isRunning = value;
		}
	}

	public static bool LoadingFailed { get; set; }

	public static float TimeSinceLastSave { get; private set; }

	private static int FrameCountSinceLastSave { get; set; }

	public static int CurrentProfile => ConfigData.CurrentProfile;

	public static bool DisableSaving { get; set; }

	public static bool IsCopyingBackupFiles { get; set; }

	private BinaryFormatter BinaryFormatter { get; set; }

	private static CultureInfo ForcedCulture { get; set; }

	private bool Internal_IsInitialized { get; set; }

	public static CultureInfo CultureInfo => ForcedCulture;

	public static bool IsInitialized => Instance.Internal_IsInitialized;

	public static bool IsConfigFileLoaded { get; private set; }

	public static bool IsProfileConfigFileLoaded { get; private set; }

	public static bool IsTrailerConfigFileLoaded { get; private set; }

	private static string GetSaveDirectoryPath(int profileSlot, bool getBackupDirectory)
	{
		string persistentDataPath = SaveFileSystem.PersistentDataPath;
		persistentDataPath = Path.Combine(persistentDataPath, "Saves");
		string platformDirectoryName = StoreAPIManager.GetPlatformDirectoryName();
		persistentDataPath = Path.Combine(persistentDataPath, platformDirectoryName);
		string userIDString = StoreAPIManager.GetUserIDString();
		if (!string.IsNullOrEmpty(userIDString))
		{
			persistentDataPath = Path.Combine(persistentDataPath, userIDString);
		}
		if (profileSlot != -1)
		{
			persistentDataPath = Path.Combine(persistentDataPath, "Profile" + profileSlot);
			if (getBackupDirectory)
			{
				persistentDataPath = Path.Combine(persistentDataPath, SaveFileSystem.BackupFolderName);
			}
		}
		return persistentDataPath;
	}

	private static void CreateBaseSaveDirectories()
	{
		string persistentDataPath = SaveFileSystem.PersistentDataPath;
		persistentDataPath = Path.Combine(persistentDataPath, "Saves");
		SaveFileSystem.CreateDirectory(persistentDataPath);
		string platformDirectoryName = StoreAPIManager.GetPlatformDirectoryName();
		if (!string.IsNullOrEmpty(platformDirectoryName))
		{
			persistentDataPath = Path.Combine(persistentDataPath, platformDirectoryName);
			SaveFileSystem.CreateDirectory(persistentDataPath);
		}
		string userIDString = StoreAPIManager.GetUserIDString();
		if (!string.IsNullOrEmpty(userIDString))
		{
			persistentDataPath = Path.Combine(persistentDataPath, userIDString);
			SaveFileSystem.CreateDirectory(persistentDataPath);
		}
		string path = persistentDataPath;
		for (int i = 0; i < 5; i++)
		{
			persistentDataPath = Path.Combine(path, "Profile" + i);
			SaveFileSystem.CreateDirectory(persistentDataPath);
		}
		Debug.Log("Created base save directories");
	}

	private static string GetFullFilePath(int profileSlot, SaveDataType saveType, bool useBackupFileName)
	{
		string saveDirectoryPath = GetSaveDirectoryPath(profileSlot, useBackupFileName);
		string text = null;
		text = (useBackupFileName ? string.Concat(str2: DateTime.Now.ToString("dd_MM_yyyy__HH_mm_ss", ForcedCulture), str0: saveType.ToString(), str1: "_", str3: ".rc2dat") : (saveType.ToString() + ".rc2dat"));
		return Path.Combine(saveDirectoryPath, text);
	}

	private List<string> GetBackupFullFilePathes(int profileSlot, SaveDataType saveType)
	{
		List<string> obj = m_backFilePathsArray[profileSlot];
		m_backupFilesListHelper.Clear();
		string saveTypeString = saveType.ToString();
		foreach (string item in obj)
		{
			if (IsValidBackupPath(item, saveTypeString))
			{
				m_backupFilesListHelper.Add(item);
			}
		}
		m_backupFilesListHelper.Sort(DateTimeComparator);
		m_backupFilesListHelper.Reverse();
		return m_backupFilesListHelper;
	}

	private bool IsValidBackupPath(string path, string saveTypeString)
	{
		if (!path.Contains(saveTypeString))
		{
			return false;
		}
		if (!path.Contains(".rc2dat"))
		{
			return false;
		}
		int length = "dd_MM_yyyy__HH_mm_ss".Length;
		int num = path.IndexOf(".rc2dat");
		DateTime result;
		return DateTime.TryParseExact(path.Substring(num - length, length), "dd_MM_yyyy__HH_mm_ss", ForcedCulture, DateTimeStyles.None, out result);
	}

	private void Awake()
	{
		if (m_instance == null)
		{
			m_instance = this;
			return;
		}
		throw new Exception("SaveManager has been instantiated twice.  This should never happen");
	}

	private IEnumerator Start()
	{
		while (StoreAPIManager.InitState != StoreAPIManager.StoreInitState.Succeeded)
		{
			yield return null;
		}
		m_instance.Initialize();
		m_instance.OnAwakeLoadConfig();
	}

	public static void SetCultureInfo(CultureInfo cultureInfo)
	{
		ForcedCulture = cultureInfo;
	}

	private void Initialize()
	{
		BinaryFormatter = new BinaryFormatter();
		if (ForcedCulture == null)
		{
			ForcedCulture = new CultureInfo("en-US", useUserOverride: false);
			ForcedCulture.NumberFormat.CurrencyDecimalSeparator = ".";
			CultureInfo.CurrentCulture = ForcedCulture;
		}
		for (int i = 0; i < 5; i++)
		{
			string saveDirectoryPath = GetSaveDirectoryPath(i, getBackupDirectory: true);
			if (SaveFileSystem.DirectoryExists(saveDirectoryPath))
			{
				Instance.m_backFilePathsArray[i] = SaveFileSystem.GetBackupFilesInDirectory(saveDirectoryPath);
			}
		}
		m_onWorldCreationComplete_UpdateSaveData = OnWorldCreationComplete_UpdateSaveData;
		m_onLevelEditorWorldCreationComplete = OnLevelEditorWorldCreationComplete;
		m_onBiomeCreationComplete_UpdateSaveData = OnBiomeCreationComplete_UpdateSaveData;
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete_UpdateSaveData);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelEditorWorldCreationComplete, m_onLevelEditorWorldCreationComplete);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.BiomeCreationComplete, m_onBiomeCreationComplete_UpdateSaveData);
		Internal_IsInitialized = true;
	}

	private static void CreateSaveDirectoriesIfNeeded()
	{
		if (Instance.m_checkedForSaveDirectories)
		{
			return;
		}
		if (!SaveFileSystem.DirectoryExists(GetSaveDirectoryPath(-1, getBackupDirectory: false)))
		{
			CreateBaseSaveDirectories();
		}
		for (int i = 0; i < 5; i++)
		{
			string saveDirectoryPath = GetSaveDirectoryPath(i, getBackupDirectory: true);
			if (!SaveFileSystem.DirectoryExists(saveDirectoryPath))
			{
				SaveFileSystem.CreateDirectory(saveDirectoryPath);
			}
			if (Instance.m_backFilePathsArray[i] == null)
			{
				Instance.m_backFilePathsArray[i] = new List<string>();
			}
		}
		Instance.m_checkedForSaveDirectories = true;
	}

	private void OnAwakeLoadConfig()
	{
		if (!IsConfigFileLoaded)
		{
			LoadConfigFile();
		}
		if (ConfigData.ScreenWidth == -1 || ConfigData.ScreenHeight == -1)
		{
			Resolution currentResolution = Screen.currentResolution;
			ConfigData.ScreenWidth = currentResolution.width;
			ConfigData.ScreenHeight = currentResolution.height;
			SaveConfigFile();
		}
		AudioManager.SetMasterVolume(ConfigData.MasterVolume);
		AudioManager.SetMusicVolume(ConfigData.MusicVolume);
		AudioManager.SetSFXVolume(ConfigData.SFXVolume);
		QualitySettings.SetQualityLevel(ConfigData.QualitySetting, applyExpensiveChanges: true);
		GameResolutionManager.SetVsyncEnable(ConfigData.EnableVsync);
		LoadProfileConfigFile();
	}

	public static void LoadCurrentProfileData()
	{
		IsRunning = true;
		if (DoesSaveExist(CurrentProfile, SaveDataType.Player, checkForBackup: false))
		{
			LOAD_RESULT lOAD_RESULT = LoadAllGameData(CurrentProfile);
			switch (lOAD_RESULT)
			{
			case LOAD_RESULT.FAILED_STAGE:
				if (PlayerSaveData.InCastle)
				{
					StageSaveData = new StageSaveData();
					StageSaveData.ForceResetWorld = true;
				}
				break;
			default:
				CreateNewSaveData();
				LoadingFailed = true;
				Debug.Log("Failed to load Current Profile Data. RESULT: " + lOAD_RESULT);
				break;
			case LOAD_RESULT.OK:
				break;
			}
		}
		else
		{
			CreateNewSaveData();
		}
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.WorldCreationComplete, m_onWorldCreationComplete_UpdateSaveData);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelEditorWorldCreationComplete, m_onLevelEditorWorldCreationComplete);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.BiomeCreationComplete, m_onBiomeCreationComplete_UpdateSaveData);
	}

	private void OnBiomeCreationComplete_UpdateSaveData(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is BiomeEventArgs biomeEventArgs)
		{
			StageSaveData.VerifyBiomeSaveData(biomeEventArgs.Biome);
		}
	}

	private void OnWorldCreationComplete_UpdateSaveData(MonoBehaviour sender, EventArgs args)
	{
		PlayerSaveData.HasStartedGame = true;
		bool flag = false;
		if (SceneLoader_RL.CurrentScene != SceneLoadingUtility.GetSceneName(SceneID.World) && (WorldBuilder.BiomeControllers.ContainsKey(BiomeType.HubTown) || WorldBuilder.BiomeControllers.ContainsKey(BiomeType.Tutorial)))
		{
			bool forceResetWorld = StageSaveData.ForceResetWorld;
			StageSaveData = new StageSaveData();
			StageSaveData.ForceResetWorld = forceResetWorld;
			StageSaveData.CreateEmptyRoomSaveDataList();
			if (PlayerManager.IsInstantiated)
			{
				PlayerManager.GetPlayerController().ResetHealth();
				PlayerManager.GetPlayerController().ResetMana();
			}
			PlayerSaveData.UpdateCachedData();
			if (!PlayerSaveData.GetFlag(PlayerSaveFlag.DriftHouseUnlocked) && Souls_EV.GetTotalSoulsCollected(PlayerSaveData.GameModeType, deductSpentSouls: false) > 0)
			{
				PlayerSaveData.SetFlag(PlayerSaveFlag.DriftHouseUnlocked, value: true);
			}
		}
		else if ((!PlayerSaveData.InCastle && PlayerSaveData.CastleLockState == CastleLockState.NotLocked) || !DoesSaveExist(CurrentProfile, SaveDataType.Stage, checkForBackup: false) || (!PlayerSaveData.InHubTown && !IsRunning))
		{
			Debug.Log("Creating new StageSaveData.");
			StageSaveData = new StageSaveData();
			StageSaveData.CreateEmptyRoomSaveDataList();
			StageSaveData.BiomeCreationSeed = RNGManager.GetSeed(RngID.BiomeCreation);
			StageSaveData.MergeRoomSeed = RNGManager.GetSeed(RngID.MergeRooms);
			StageSaveData.PropSeed = RNGManager.GetSeed(RngID.Prop);
			StageSaveData.EnemySeed = RNGManager.GetSeed(RngID.Enemy);
			flag = true;
		}
		else if (PlayerSaveData.InCastle)
		{
			Debug.Log("Loading old StageSaveData - Resuming Player Save progress");
			StageSaveData.TimesTrackerWasLoaded++;
			GameEventTrackerManager.RoomEventTracker.RoomsEntered.AddRange(StageSaveData.RoomTrackerDataList);
			GameEventTrackerManager.EnemyEventTracker.EnemiesKilled.AddRange(StageSaveData.EnemyTrackerDataList);
			GameEventTrackerManager.ItemEventTracker.ChestsOpened.AddRange(StageSaveData.ChestTrackerDataList);
			GameEventTrackerManager.ItemEventTracker.ItemsCollected.AddRange(StageSaveData.ItemTrackerDataList);
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.CurrentArmor = PlayerSaveData.CachedData.CurrentArmor;
			playerController.SpellOrbs = PlayerSaveData.CachedData.SpellOrbs;
			playerController.SetHealth(PlayerSaveData.CachedData.CurrentHealth, additive: false, runEvents: true);
			playerController.SetMana(PlayerSaveData.CachedData.CurrentMana, additive: false, runEvents: true, canExceedMax: true);
			playerController.CastAbility.SetAbilityAmmo(CastAbilityType.Weapon, PlayerSaveData.CachedData.CurrentWeaponAmmo);
			playerController.CastAbility.SetAbilityAmmo(CastAbilityType.Spell, PlayerSaveData.CachedData.CurrentSpellAmmo);
			playerController.CastAbility.SetAbilityAmmo(CastAbilityType.Talent, PlayerSaveData.CachedData.CurrentTalentAmmo);
		}
		else
		{
			Debug.Log("Loading old StageSaveData - Resetting Player Save progress");
			GameEventTrackerManager.RoomEventTracker.Reset();
			GameEventTrackerManager.EnemyEventTracker.Reset();
			GameEventTrackerManager.ItemEventTracker.Reset();
			StageSaveData.TimesTrackerWasLoaded = 0;
			foreach (KeyValuePair<BiomeType, List<RoomSaveData>> item in StageSaveData.RoomSaveDataDict)
			{
				Dictionary<int, RoomSaveData> roomDataLookupTable = StageSaveData.GetRoomDataLookupTable(item.Key);
				if (roomDataLookupTable == null)
				{
					continue;
				}
				foreach (KeyValuePair<int, RoomSaveData> item2 in roomDataLookupTable)
				{
					RoomSaveData value = item2.Value;
					if (value.IsEmpty)
					{
						continue;
					}
					if (item.Key == BiomeType.Forest && PlayerSaveData.GetInsightState(InsightType.ForestBoss_DoorOpened) < InsightState.ResolvedButNotViewed)
					{
						switch (value.RoomID.ToString())
						{
						case "Levels Forest Mandatory 1":
						case "Levels Forest Mandatory 2":
						case "Levels Forest Mandatory 3":
							value.IsRoomComplete = false;
							break;
						}
					}
					if (item.Key == BiomeType.Cave)
					{
						if (!PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened) && value.RoomID.ToString() == "Levels Cave Mandatory 22")
						{
							value.IsRoomComplete = false;
						}
						if (!PlayerSaveData.GetFlag(PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened) && value.RoomID.ToString() == "Levels Cave Mandatory 19")
						{
							value.IsRoomComplete = false;
						}
					}
					if (value.RoomType == RoomType.Fairy && value.IsRoomComplete)
					{
						bool flag2 = false;
						for (int i = 0; i < value.ChestStates.Length; i++)
						{
							if (value.ChestStates[i].IsStateActive)
							{
								flag2 = true;
							}
						}
						if (flag2)
						{
							value.IsRoomComplete = false;
						}
					}
					for (int j = 0; j < value.BreakableStates.Length; j++)
					{
						value.BreakableStates[j].IsStateActive = true;
					}
					for (int k = 0; k < value.DecoBreakableStates.Length; k++)
					{
						value.DecoBreakableStates[k].IsStateActive = true;
					}
					for (int l = 0; l < value.EnemyStates.Length; l++)
					{
						value.EnemyStates[l].IsStateActive = true;
					}
				}
			}
			flag = true;
		}
		if (flag)
		{
			PlayerSaveData.InHubTown = false;
			PlayerSaveData.InCastle = true;
			PlayerSaveData.RunAccumulatedXP = 0;
			PlayerSaveData.SetFlag(PlayerSaveFlag.CastleBoss_FreeHeal_Used, value: false);
			PlayerSaveData.SetFlag(PlayerSaveFlag.BridgeBoss_FreeHeal_Used, value: false);
			PlayerSaveData.SetFlag(PlayerSaveFlag.ForestBoss_FreeHeal_Used, value: false);
			PlayerSaveData.SetFlag(PlayerSaveFlag.StudyBoss_FreeHeal_Used, value: false);
			PlayerSaveData.SetFlag(PlayerSaveFlag.TowerBoss_FreeHeal_Used, value: false);
			PlayerSaveData.SetFlag(PlayerSaveFlag.CaveBoss_FreeHeal_Used, value: false);
			PlayerSaveData.SetFlag(PlayerSaveFlag.GardenBoss_FreeHeal_Used, value: false);
			PlayerSaveData.SetFlag(PlayerSaveFlag.FinalBoss_FreeHeal_Used, value: false);
			NPCDialogueManager.PopulateNPCDialogues(inHubtown: false);
			NPCDialogueManager.InitializeGlobalDialogueCD();
			if (PlayerSaveData.TreeCutsceneDisplayCount < Ending_EV.TREE_CUTSCENE_DIALOGUE_1_LOCID.Length || PlayerSaveData.HestiaCutsceneDisplayCount < Ending_EV.HESTIA_CUTSCENE_DIALOGUE_1_LOCID.Length)
			{
				float num = 0f;
				int num2 = PlayerSaveData.TimesDiedSinceHestia - 6 + 1;
				if (num2 > 0)
				{
					num = Mathf.Clamp((float)num2 * 0.25f, 0f, 1f);
				}
				float num3 = UnityEngine.Random.Range(0f, 1f);
				if (num3 > 0f && num3 <= num)
				{
					if (PlayerSaveData.HestiaCutsceneDisplayCount < Ending_EV.HESTIA_CUTSCENE_DIALOGUE_1_LOCID.Length && (PlayerSaveData.TreeCutsceneDisplayCount > PlayerSaveData.HestiaCutsceneDisplayCount || PlayerSaveData.TreeCutsceneDisplayCount >= Ending_EV.TREE_CUTSCENE_DIALOGUE_1_LOCID.Length))
					{
						PlayerSaveData.SetFlag(PlayerSaveFlag.Play_Hestia_DeathCutscene, value: true);
					}
					else
					{
						PlayerSaveData.SetFlag(PlayerSaveFlag.Play_Tree_DeathCutscene, value: true);
					}
				}
			}
			SaveAllCurrentProfileGameData(SavingType.FileAndBackup, saveStageData: true, ignoreTimeRestriction: true);
		}
		StageSaveData.LinkTrackerData();
		MapController.LoadStageSaveData();
	}

	private void OnLevelEditorWorldCreationComplete(MonoBehaviour sender, EventArgs args)
	{
		LevelEditorWorldCreationCompleteEventArgs levelEditorWorldCreationCompleteEventArgs = args as LevelEditorWorldCreationCompleteEventArgs;
		StageSaveData.CreateLevelEditorRoomSaveDataList(levelEditorWorldCreationCompleteEventArgs.BuiltRoom);
		if (!PlayerSaveData.GetFlag(PlayerSaveFlag.DriftHouseUnlocked) && Souls_EV.GetTotalSoulsCollected(PlayerSaveData.GameModeType, deductSpentSouls: false) > 0)
		{
			PlayerSaveData.SetFlag(PlayerSaveFlag.DriftHouseUnlocked, value: true);
		}
	}

	private static void CreateSaveFile(SaveFileSystem.SaveBatch saveBatch, string fileNameAndPath, object saveObject, BinaryFormatter binaryFormatter)
	{
		try
		{
			MemoryStream memoryStream = new MemoryStream();
			binaryFormatter.Serialize(memoryStream, saveObject);
			memoryStream.Close();
			SaveFileSystem.WriteAllBytes(saveBatch, fileNameAndPath, memoryStream.ToArray());
		}
		catch (Exception ex)
		{
			Debug.Log("<color=red>FILE SAVE FAILED.</color>  Save Path: " + fileNameAndPath + "  Error: " + ex.Message);
		}
	}

	private void SaveFileAsync(SaveFileSystem.SaveBatch saveBatch, int profileSlot, SaveDataType saveType, bool saveBackup, object saveObject = null)
	{
		if (saveObject == null)
		{
			switch (saveType)
			{
			case SaveDataType.Player:
				PlayerSaveData.UpdateCachedData();
				saveObject = PlayerSaveData;
				break;
			case SaveDataType.Equipment:
				saveObject = EquipmentSaveData;
				break;
			case SaveDataType.Lineage:
				saveObject = LineageSaveData;
				break;
			case SaveDataType.Stage:
				saveObject = StageSaveData;
				break;
			case SaveDataType.GameMode:
				saveObject = ModeSaveData;
				break;
			}
		}
		string fullFilePath = GetFullFilePath(profileSlot, saveType, saveBackup);
		Task lastAsyncTask = m_lastAsyncTask;
		m_lastAsyncTask = SaveAsync(lastAsyncTask, saveBatch, fullFilePath, profileSlot, saveType, saveBackup, saveObject);
	}

	private async Task SaveAsync(Task lastAsyncTask, SaveFileSystem.SaveBatch saveBatch, string filePath, int profileSlot, SaveDataType saveType, bool saveBackup, object saveObject)
	{
		saveBatch.IncrementRefCount();
		if (lastAsyncTask != null)
		{
			await lastAsyncTask;
		}
		await Task.Run(delegate
		{
			CreateSaveFile(saveBatch, filePath, saveObject, BinaryFormatter);
			if (saveBackup)
			{
				m_backFilePathsArray[profileSlot].Add(filePath);
				DeleteOldBackups(saveBatch, profileSlot, saveType);
			}
			saveBatch.DecrementRefCount();
		});
	}

	private void DeleteOldBackups(SaveFileSystem.SaveBatch saveBatch, int profileSlot, SaveDataType saveType)
	{
		List<string> backupFullFilePathes = GetBackupFullFilePathes(profileSlot, saveType);
		if (backupFullFilePathes.Count <= 5)
		{
			return;
		}
		backupFullFilePathes.Sort(DateTimeComparator);
		backupFullFilePathes.Reverse();
		backupFullFilePathes.RemoveRange(0, 5);
		foreach (string item in backupFullFilePathes)
		{
			SaveFileSystem.DeleteFile(saveBatch, item);
			m_backFilePathsArray[profileSlot].Remove(item);
		}
	}

	private int DateTimeComparator(string fileA, string fileB)
	{
		DateTime t = PathToDateTime(fileA);
		DateTime t2 = PathToDateTime(fileB);
		return DateTime.Compare(t, t2);
	}

	public static DateTime PathToDateTime(string filePath)
	{
		int length = "dd_MM_yyyy__HH_mm_ss".Length;
		int num = filePath.IndexOf(".rc2dat");
		return DateTime.ParseExact(filePath.Substring(num - length, length), "dd_MM_yyyy__HH_mm_ss", ForcedCulture);
	}

	private static LOAD_RESULT LoadFile(string filePath, out object output, BinaryFormatter binaryFormatter)
	{
		output = null;
		if (!SaveFileSystem.FileExists(filePath))
		{
			Debug.Log("<color=red>Could not load file: " + filePath + ". File does not exist.</color>");
			if (filePath.Contains(SaveDataType.Player.ToString()))
			{
				return LOAD_RESULT.NO_FILE_FOUND_PLAYER;
			}
			if (filePath.Contains(SaveDataType.Equipment.ToString()))
			{
				return LOAD_RESULT.NO_FILE_FOUND_EQUIPMENT;
			}
			if (filePath.Contains(SaveDataType.Lineage.ToString()))
			{
				return LOAD_RESULT.NO_FILE_FOUND_LINEAGE;
			}
			if (filePath.Contains(SaveDataType.Stage.ToString()))
			{
				return LOAD_RESULT.NO_FILE_FOUND_STAGE;
			}
			if (filePath.Contains(SaveDataType.GameMode.ToString()))
			{
				return LOAD_RESULT.NO_FILE_FOUND_GAMEMODE;
			}
			return LOAD_RESULT.NO_FILE_FOUND_UNKNOWN;
		}
		try
		{
			byte[] array = SaveFileSystem.ReadAllBytes(filePath);
			using (MemoryStream memoryStream = new MemoryStream())
			{
				memoryStream.Write(array, 0, array.Length);
				memoryStream.Position = 0L;
				output = binaryFormatter.Deserialize(memoryStream);
			}
			return LOAD_RESULT.OK;
		}
		catch (Exception ex)
		{
			Debug.Log("<color=red>FILE LOAD FAILED.  File Path: " + filePath + "  Error: " + ex.Message + "</color>");
			if (filePath.Contains(SaveDataType.Player.ToString()))
			{
				return LOAD_RESULT.FAILED_PLAYER;
			}
			if (filePath.Contains(SaveDataType.Equipment.ToString()))
			{
				return LOAD_RESULT.FAILED_EQUIPMENT;
			}
			if (filePath.Contains(SaveDataType.Lineage.ToString()))
			{
				return LOAD_RESULT.FAILED_LINEAGE;
			}
			if (filePath.Contains(SaveDataType.Stage.ToString()))
			{
				return LOAD_RESULT.FAILED_STAGE;
			}
			if (filePath.Contains(SaveDataType.GameMode.ToString()))
			{
				return LOAD_RESULT.FAILED_GAMEMODE;
			}
			return LOAD_RESULT.FAILED_UNKNOWN;
		}
	}

	public static void ForceInitialize()
	{
		_ = IsInitialized;
	}

	public static bool DoesSaveExist(int profileSlot, SaveDataType saveType, bool checkForBackup)
	{
		if (!checkForBackup)
		{
			return SaveFileSystem.FileExists(GetFullFilePath(profileSlot, saveType, checkForBackup));
		}
		return Instance.GetBackupFullFilePathes(profileSlot, saveType).Count > 0;
	}

	public static List<string> GetBackupPathes(int profileSlot, SaveDataType saveType)
	{
		return Instance.GetBackupFullFilePathes(profileSlot, saveType);
	}

	public static void SaveCurrentProfileGameData(SaveFileSystem.SaveBatch saveBatch, SaveDataType saveType, SavingType backupType, bool ignoreTimeRestriction, object saveObject = null)
	{
		SaveGameData(saveBatch, CurrentProfile, saveType, backupType, ignoreTimeRestriction, saveObject);
	}

	public static void SaveCurrentProfileGameData(SaveDataType saveType, SavingType backupType, bool ignoreTimeRestriction, object saveObject = null)
	{
		SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(CurrentProfile);
		SaveCurrentProfileGameData(saveBatch, saveType, backupType, ignoreTimeRestriction, saveObject);
		saveBatch.End();
	}

	public static void SaveAllCurrentProfileGameData(SavingType savingType, bool saveStageData, bool ignoreTimeRestriction)
	{
		SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(CurrentProfile);
		SaveCurrentProfileGameData(saveBatch, SaveDataType.Player, savingType, ignoreTimeRestriction);
		SaveCurrentProfileGameData(saveBatch, SaveDataType.Equipment, savingType, ignoreTimeRestriction);
		SaveCurrentProfileGameData(saveBatch, SaveDataType.Lineage, savingType, ignoreTimeRestriction);
		SaveCurrentProfileGameData(saveBatch, SaveDataType.GameMode, savingType, ignoreTimeRestriction);
		if (saveStageData)
		{
			SaveCurrentProfileGameData(saveBatch, SaveDataType.Stage, savingType, ignoreTimeRestriction);
		}
		saveBatch.End();
	}

	private static void SaveGameData(SaveFileSystem.SaveBatch saveBatch, int profileSlot, SaveDataType saveType, SavingType backupType, bool ignoreTimeRestriction, object saveObject = null)
	{
		if (DisableSaving)
		{
			return;
		}
		if (Application.isPlaying)
		{
			CreateSaveDirectoriesIfNeeded();
			if (!IsRunning)
			{
				if (!m_hasMainMenuSaveFailedMessageBeenLogged)
				{
					m_hasMainMenuSaveFailedMessageBeenLogged = true;
					Debug.Log("Save failed. Must enter Main Menu at least once for auto-saving to work");
				}
				return;
			}
			if (LoadingFailed)
			{
				Debug.Log("Save failed. Corrupted save file detected. Please load backup save file.");
				return;
			}
			if (!ignoreTimeRestriction && FrameCountSinceLastSave != Time.frameCount && Time.unscaledTime < TimeSinceLastSave + 10f)
			{
				return;
			}
			if (saveType == SaveDataType.Stage && !IsCopyingBackupFiles && SceneLoader_RL.CurrentScene != SceneLoadingUtility.GetSceneName(SceneID.World) && !StageSaveData.ForceResetWorld)
			{
				Debug.Log("Cannot save StageData. You are not in the world scene.");
				return;
			}
		}
		if (!ignoreTimeRestriction)
		{
			FrameCountSinceLastSave = Time.frameCount;
			TimeSinceLastSave = Time.unscaledTime;
		}
		switch (backupType)
		{
		case SavingType.BackupOnly:
			Instance.SaveFileAsync(saveBatch, profileSlot, saveType, saveBackup: true, saveObject);
			break;
		case SavingType.FileAndBackup:
			Instance.SaveFileAsync(saveBatch, profileSlot, saveType, saveBackup: false, saveObject);
			Instance.SaveFileAsync(saveBatch, profileSlot, saveType, saveBackup: true, saveObject);
			break;
		case SavingType.FileOnly:
			Instance.SaveFileAsync(saveBatch, profileSlot, saveType, saveBackup: false, saveObject);
			break;
		}
	}

	public static void CreateNewSaveData()
	{
		PlayerSaveData = new PlayerSaveData();
		EquipmentSaveData = new EquipmentSaveData();
		LineageSaveData = new LineageSaveData();
		ModeSaveData = new ModeSaveData();
	}

	public static LOAD_RESULT LoadGameDataAndUpdate(int profileSlot, SaveDataType saveType)
	{
		object loadDataOutput = null;
		LOAD_RESULT lOAD_RESULT = LoadGameData(profileSlot, saveType, out loadDataOutput);
		if (lOAD_RESULT == LOAD_RESULT.OK)
		{
			if (profileSlot == CurrentProfile)
			{
				(loadDataOutput as IVersionUpdateable).UpdateVersion();
			}
			switch (saveType)
			{
			case SaveDataType.Player:
				PlayerSaveData = loadDataOutput as PlayerSaveData;
				if (PlayerSaveData == null)
				{
					lOAD_RESULT = LOAD_RESULT.FAILED_PLAYER;
				}
				break;
			case SaveDataType.Stage:
				StageSaveData = loadDataOutput as StageSaveData;
				if (StageSaveData == null)
				{
					lOAD_RESULT = LOAD_RESULT.FAILED_STAGE;
				}
				break;
			case SaveDataType.Equipment:
				EquipmentSaveData = loadDataOutput as EquipmentSaveData;
				if (EquipmentSaveData == null)
				{
					lOAD_RESULT = LOAD_RESULT.FAILED_EQUIPMENT;
				}
				break;
			case SaveDataType.Lineage:
				LineageSaveData = loadDataOutput as LineageSaveData;
				if (LineageSaveData == null)
				{
					lOAD_RESULT = LOAD_RESULT.FAILED_LINEAGE;
				}
				break;
			case SaveDataType.GameMode:
				ModeSaveData = loadDataOutput as ModeSaveData;
				if (ModeSaveData == null)
				{
					lOAD_RESULT = LOAD_RESULT.FAILED_GAMEMODE;
				}
				break;
			}
		}
		return lOAD_RESULT;
	}

	public static LOAD_RESULT LoadGameDataByFilePath(string filePath, out object loadDataOutput)
	{
		return LoadFile(filePath, out loadDataOutput, Instance.BinaryFormatter);
	}

	public static LOAD_RESULT LoadGameData(int profileSlot, SaveDataType saveType, out object loadDataOutput)
	{
		return LoadFile(GetFullFilePath(profileSlot, saveType, useBackupFileName: false), out loadDataOutput, Instance.BinaryFormatter);
	}

	public static LOAD_RESULT LoadAllGameData(int profileSlot)
	{
		LOAD_RESULT lOAD_RESULT = LoadGameDataAndUpdate(profileSlot, SaveDataType.GameMode);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("<color=red>Failed to load SaveType: " + SaveDataType.GameMode.ToString() + ".</color>");
			return lOAD_RESULT;
		}
		lOAD_RESULT = LoadGameDataAndUpdate(profileSlot, SaveDataType.Player);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("<color=red>Failed to load SaveType: " + SaveDataType.Player.ToString() + ".</color>");
			return lOAD_RESULT;
		}
		lOAD_RESULT = LoadGameDataAndUpdate(profileSlot, SaveDataType.Equipment);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("<color=red>Failed to load SaveType: " + SaveDataType.Equipment.ToString() + ".</color>");
			return lOAD_RESULT;
		}
		lOAD_RESULT = LoadGameDataAndUpdate(profileSlot, SaveDataType.Lineage);
		if (lOAD_RESULT != 0)
		{
			Debug.Log("<color=red>Failed to load SaveType: " + SaveDataType.Lineage.ToString() + ".</color>");
			return lOAD_RESULT;
		}
		if (DoesSaveExist(profileSlot, SaveDataType.Stage, checkForBackup: false))
		{
			lOAD_RESULT = LoadGameDataAndUpdate(profileSlot, SaveDataType.Stage);
			if (lOAD_RESULT != 0)
			{
				Debug.Log("<color=red>Failed to load SaveType: " + SaveDataType.Stage.ToString() + ".</color>");
				return lOAD_RESULT;
			}
			if (lOAD_RESULT == LOAD_RESULT.OK && StageSaveData.RoomSaveDataDict.ContainsKey(BiomeType.HubTown) && StageSaveData.RoomSaveDataDict.Count == 1)
			{
				lOAD_RESULT = LOAD_RESULT.FAILED_STAGE;
				Debug.Log("<color=red>Failed to load SaveType: " + SaveDataType.Stage.ToString() + " because stage data was hubtown.</color>");
				return lOAD_RESULT;
			}
		}
		return LOAD_RESULT.OK;
	}

	public static LOAD_RESULT LoadGameDataViaEditor(int profileSlot, SaveDataType saveType, out object loadDataOutput)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		return LoadFile(GetFullFilePath(profileSlot, saveType, useBackupFileName: false), out loadDataOutput, binaryFormatter);
	}

	public static void SaveGameDataViaEditor(int profileSlot, SaveDataType saveType, object saveObject)
	{
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		string fullFilePath = GetFullFilePath(profileSlot, saveType, useBackupFileName: false);
		CreateSaveFile(new SaveFileSystem.SaveBatch(), fullFilePath, saveObject, binaryFormatter);
	}

	public static void DeleteSaveFile(SaveFileSystem.SaveBatch saveBatch, int profileSlot, SaveDataType saveType)
	{
		string fullFilePath = GetFullFilePath(profileSlot, saveType, useBackupFileName: false);
		if (SaveFileSystem.FileExists(fullFilePath))
		{
			SaveFileSystem.DeleteFile(saveBatch, fullFilePath);
			Debug.Log("Deleted save file: " + saveType.ToString() + " for profile slot " + profileSlot);
		}
	}

	public static void SaveConfigFile()
	{
		CreateSaveDirectoriesIfNeeded();
		string configPath = GetConfigPath();
		using StringWriter stringWriter = new StringWriter();
		try
		{
			stringWriter.WriteLine("[VERSION]");
			stringWriter.WriteLine("RevisionNumber=" + ConfigData.REVISION_NUMBER);
			stringWriter.WriteLine();
			stringWriter.WriteLine("[Selected Profile]");
			stringWriter.WriteLine("Profile=" + ConfigData.CurrentProfile);
			stringWriter.WriteLine();
			stringWriter.WriteLine("[Screen Resolution]");
			stringWriter.WriteLine("ScreenWidth=" + ConfigData.ScreenWidth);
			stringWriter.WriteLine("ScreenHeight=" + ConfigData.ScreenHeight);
			stringWriter.WriteLine();
			stringWriter.WriteLine("[Screen Mode]");
			stringWriter.WriteLine("PrimaryDisplay=" + ConfigData.PrimaryDisplay);
			stringWriter.WriteLine("ScreenMode=" + ConfigData.ScreenMode);
			stringWriter.WriteLine("Disable_16_9=" + ConfigData.Disable_16_9);
			stringWriter.WriteLine("DisableCursorConfine=" + ConfigData.DisableCursorConfine);
			stringWriter.WriteLine();
			stringWriter.WriteLine("[Graphics Quality]");
			stringWriter.WriteLine("QualitySetting=" + ConfigData.QualitySetting);
			stringWriter.WriteLine("EnableVsync=" + ConfigData.EnableVsync);
			stringWriter.WriteLine();
			stringWriter.WriteLine("[Game Volume]");
			stringWriter.WriteLine("MasterVol=" + $"{ConfigData.MasterVolume:F2}");
			stringWriter.WriteLine("MusicVol=" + $"{ConfigData.MusicVolume:F2}");
			stringWriter.WriteLine("SFXVol=" + $"{ConfigData.SFXVolume:F2}");
			stringWriter.WriteLine();
			stringWriter.WriteLine("[Controls]");
			stringWriter.WriteLine("DeadZone=" + $"{ConfigData.DeadZone:F2}");
			stringWriter.WriteLine("AimFidelity=" + ConfigData.AimFidelity);
			stringWriter.WriteLine("InputIconSetting=" + ConfigData.InputIconSetting);
			stringWriter.WriteLine("DisableRumble=" + ConfigData.DisableRumble);
			stringWriter.WriteLine();
			stringWriter.WriteLine("[Game Settings]");
			stringWriter.WriteLine("UseNonScientificNames=" + ConfigData.UseNonScientificNames);
			stringWriter.WriteLine("EnableDualButtonDash=" + ConfigData.EnableDualButtonDash);
			stringWriter.WriteLine("EnableQuickDrop=" + ConfigData.EnableQuickDrop);
			stringWriter.WriteLine("DisableMouseAiming=" + ConfigData.ToggleMouseAiming);
			stringWriter.WriteLine("EnableMouseAttackFlip=" + ConfigData.ToggleMouseAttackFlip);
			stringWriter.WriteLine("DisableHitSlowdown=" + ConfigData.DisableSlowdownOnHit);
			stringWriter.WriteLine("DisablePressDownSpinKick=" + ConfigData.DisablePressDownSpinKick);
			stringWriter.WriteLine("DisableReloadInteractButton=" + ConfigData.DisableReloadInteractButton);
			stringWriter.WriteLine("DisableHUDFadeOut=" + ConfigData.DisableHUDFadeOut);
			stringWriter.WriteLine("Language=" + ConfigData.Language);
			stringWriter.WriteLine();
			stringWriter.WriteLine("[User Reports]");
			stringWriter.WriteLine("UserReportName=" + $"{ConfigData.UserReportName:F2}");
			stringWriter.WriteLine("UserReportEmail=" + $"{ConfigData.UserReportEmail:F2}");
			stringWriter.WriteLine();
			SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(-1);
			SaveFileSystem.WriteAllBytes(saveBatch, configPath, Encoding.ASCII.GetBytes(stringWriter.ToString()));
			saveBatch.End();
		}
		catch (Exception ex)
		{
			throw new Exception("Could not save config file. Error: " + ex.Message);
		}
		Debug.Log("<color=green>Config file saved successfully.</color>");
	}

	public static string GetConfigPath()
	{
		string path = Path.Combine(SaveFileSystem.PersistentDataPath, "Saves");
		string platformDirectoryName = StoreAPIManager.GetPlatformDirectoryName();
		return Path.Combine(Path.Combine(path, platformDirectoryName), "GameConfig.ini");
	}

	public static void LoadConfigFile()
	{
		string configPath = GetConfigPath();
		if (!SaveFileSystem.FileExists(configPath))
		{
			Debug.Log("<color=yellow>Could not find config file.  Creating default config file.</color>");
			SaveConfigFile();
		}
		try
		{
			byte[] bytes = SaveFileSystem.ReadAllBytes(configPath);
			using StringReader stringReader = new StringReader(Encoding.ASCII.GetString(bytes));
			string text = null;
			while ((text = stringReader.ReadLine()) != null)
			{
				int num = text.IndexOf("=");
				if (num == -1)
				{
					continue;
				}
				string text2 = text.Substring(0, num);
				string text3 = text.Substring(num + 1);
				if (!string.IsNullOrEmpty(text3))
				{
					switch (text2)
					{
					case "RevisionNumber":
						ConfigData.REVISION_NUMBER = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "ScreenWidth":
						ConfigData.ScreenWidth = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "ScreenHeight":
						ConfigData.ScreenHeight = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "ScreenMode":
						ConfigData.ScreenMode = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "Disable_16_9":
						ConfigData.Disable_16_9 = bool.Parse(text3);
						break;
					case "DisableCursorConfine":
						ConfigData.DisableCursorConfine = bool.Parse(text3);
						break;
					case "Profile":
						ConfigData.CurrentProfile = byte.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "MasterVol":
						ConfigData.MasterVolume = Mathf.Clamp(float.Parse(text3, NumberStyles.Any, CultureInfo), 0f, 1f);
						break;
					case "MusicVol":
						ConfigData.MusicVolume = Mathf.Clamp(float.Parse(text3, NumberStyles.Any, CultureInfo), 0f, 1f);
						break;
					case "SFXVol":
						ConfigData.SFXVolume = Mathf.Clamp(float.Parse(text3, NumberStyles.Any, CultureInfo), 0f, 1f);
						break;
					case "DeadZone":
						ConfigData.DeadZone = Mathf.Clamp(float.Parse(text3, NumberStyles.Any, CultureInfo), 0f, 1f);
						break;
					case "AimFidelity":
						ConfigData.AimFidelity = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "QualitySetting":
						ConfigData.QualitySetting = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "UserReportName":
						ConfigData.UserReportName = text3;
						break;
					case "UserReportEmail":
						ConfigData.UserReportEmail = text3;
						break;
					case "InputIconSetting":
						ConfigData.InputIconSetting = (InputIconSetting)Enum.Parse(typeof(InputIconSetting), text3);
						break;
					case "UseNonScientificNames":
						ConfigData.UseNonScientificNames = bool.Parse(text3);
						break;
					case "EnableDualButtonDash":
						ConfigData.EnableDualButtonDash = bool.Parse(text3);
						break;
					case "EnableQuickDrop":
						ConfigData.EnableQuickDrop = bool.Parse(text3);
						break;
					case "DisableMouseAiming":
						ConfigData.ToggleMouseAiming = bool.Parse(text3);
						break;
					case "EnableMouseAttackFlip":
						ConfigData.ToggleMouseAttackFlip = bool.Parse(text3);
						break;
					case "DisableReloadInteractButton":
						ConfigData.DisableReloadInteractButton = bool.Parse(text3);
						break;
					case "DisableHitSlowdown":
						ConfigData.DisableSlowdownOnHit = bool.Parse(text3);
						break;
					case "DisablePressDownSpinKick":
						ConfigData.DisablePressDownSpinKick = bool.Parse(text3);
						break;
					case "DisableHUDFadeOut":
						ConfigData.DisableHUDFadeOut = bool.Parse(text3);
						break;
					case "EnableVsync":
						ConfigData.EnableVsync = bool.Parse(text3);
						break;
					case "PrimaryDisplay":
						ConfigData.PrimaryDisplay = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "Language":
						ConfigData.Language = (LanguageType)Enum.Parse(typeof(LanguageType), text3);
						break;
					case "DisableRumble":
						ConfigData.DisableRumble = bool.Parse(text3);
						break;
					}
				}
			}
			Debug.Log("<color=green>Config file loaded successfully.</color>");
		}
		catch (Exception ex)
		{
			throw new Exception("Could not load config file. Error: " + ex.Message);
		}
		ConfigData.UpdateVersion();
		RewiredOnStartupController.UpdateJoystickCalibrationMap();
		IsConfigFileLoaded = true;
	}

	public static void SaveAllControllerMaps()
	{
		SaveControllerMap(saveGamepad: false);
		SaveControllerMap(saveGamepad: true);
	}

	public static void SaveControllerMap(bool saveGamepad)
	{
		int mapCategoryID = Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.ActionRemappable);
		int mapCategoryID2 = Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.WindowRemappable);
		Player player = ReInput.players.GetPlayer(0);
		if (!saveGamepad)
		{
			Debug.Log("Attempting to save Keyboard/Mouse controller map...");
			ControllerMap map = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, mapCategoryID, 0);
			ControllerMap map2 = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, mapCategoryID2, 0);
			ControllerMap map3 = player.controllers.maps.GetMap(ControllerType.Mouse, 0, mapCategoryID, 0);
			ControllerMap map4 = player.controllers.maps.GetMap(ControllerType.Mouse, 0, mapCategoryID2, 0);
			SaveMapToXML(map, "KBAction");
			SaveMapToXML(map2, "KBWindow");
			SaveMapToXML(map3, "MouseAction");
			SaveMapToXML(map4, "MouseWindow");
		}
		else
		{
			Debug.Log("Attempting to save Gamepad controller map...");
			int firstAvailableJoystickControllerID = RewiredOnStartupController.GetFirstAvailableJoystickControllerID();
			if (firstAvailableJoystickControllerID != -1)
			{
				ControllerMap map5 = player.controllers.maps.GetMap(ControllerType.Joystick, firstAvailableJoystickControllerID, mapCategoryID, 0);
				ControllerMap map6 = player.controllers.maps.GetMap(ControllerType.Joystick, firstAvailableJoystickControllerID, mapCategoryID2, 0);
				SaveMapToXML(map5, "GamepadAction");
				SaveMapToXML(map6, "GamepadWindow");
			}
			else
			{
				Debug.Log("<color=yellow>Failed to save Gamepad controller map. No viable Gamepad connected.</color>");
			}
		}
	}

	private static void SaveMapToXML(ControllerMap controllerMap, string mapPrependID)
	{
		CreateSaveDirectoriesIfNeeded();
		string controllerPath = GetControllerPath(mapPrependID);
		string text = mapPrependID + "ControllerMapV3RL1.xml";
		if (controllerMap == null)
		{
			Debug.Log("<color=yellow>Could not save Controller Map: " + text + ". Controller map is null (may not be connected).</color>");
			return;
		}
		string s = ((controllerMap.controllerType != ControllerType.Joystick) ? controllerMap.ToXmlString() : controllerMap.ToControllerTemplateMap<IGamepadTemplate>().ToXmlString());
		try
		{
			SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(-1);
			SaveFileSystem.WriteAllBytes(saveBatch, controllerPath, Encoding.ASCII.GetBytes(s));
			saveBatch.End();
		}
		catch (Exception ex)
		{
			throw new Exception("Could not save Controller Map: " + text + ". Error: " + ex.Message);
		}
		Debug.Log("<color=green>Controller Map " + text + " saved successfully.</color>");
	}

	private static string GetControllerPath(string mapPrependID)
	{
		string persistentDataPath = SaveFileSystem.PersistentDataPath;
		persistentDataPath = Path.Combine(persistentDataPath, "Saves");
		string platformDirectoryName = StoreAPIManager.GetPlatformDirectoryName();
		persistentDataPath = Path.Combine(persistentDataPath, platformDirectoryName);
		string userIDString = StoreAPIManager.GetUserIDString();
		if (!string.IsNullOrEmpty(userIDString))
		{
			persistentDataPath = Path.Combine(persistentDataPath, userIDString);
		}
		string path = mapPrependID + "ControllerMapV3RL1.xml";
		return Path.Combine(persistentDataPath, path);
	}

	public static void LoadAllControllerMaps()
	{
		LoadControllerMap(loadGamepad: false);
		foreach (Joystick joystick in ReInput.controllers.Joysticks)
		{
			if (!Rewired_RL.IsStandardJoystick(joystick))
			{
				ReInput.controllers.RemoveControllerFromAllPlayers(joystick);
			}
			else
			{
				LoadControllerMap(loadGamepad: true, joystick.identifier.controllerId);
			}
		}
	}

	public static void LoadControllerMap(bool loadGamepad, int controllerID = 0)
	{
		int mapCategoryID = Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.ActionRemappable);
		int mapCategoryID2 = Rewired_RL.GetMapCategoryID(Rewired_RL.MapCategoryType.WindowRemappable);
		Player player = ReInput.players.GetPlayer(0);
		if (!loadGamepad)
		{
			ControllerMap map = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, mapCategoryID, 0);
			ControllerMap map2 = player.controllers.maps.GetMap(ControllerType.Keyboard, 0, mapCategoryID2, 0);
			ControllerMap map3 = player.controllers.maps.GetMap(ControllerType.Mouse, 0, mapCategoryID, 0);
			ControllerMap map4 = player.controllers.maps.GetMap(ControllerType.Mouse, 0, mapCategoryID2, 0);
			LoadMapFromXML(map, "KBAction");
			LoadMapFromXML(map2, "KBWindow");
			LoadMapFromXML(map3, "MouseAction");
			LoadMapFromXML(map4, "MouseWindow");
		}
		else
		{
			ControllerMap map5 = player.controllers.maps.GetMap(ControllerType.Joystick, controllerID, mapCategoryID, 0);
			ControllerMap map6 = player.controllers.maps.GetMap(ControllerType.Joystick, controllerID, mapCategoryID2, 0);
			LoadMapFromXML(map5, "GamepadAction");
			LoadMapFromXML(map6, "GamepadWindow");
		}
	}

	private static void LoadMapFromXML(ControllerMap controllerMap, string mapPrependID)
	{
		string controllerPath = GetControllerPath(mapPrependID);
		string text = mapPrependID + "ControllerMapV3RL1.xml";
		if (controllerMap == null)
		{
			Debug.Log("<color=yellow>Could not load Controller Map: " + text + ". Controller map is null (controller may not be connected).</color>");
		}
		else if (SaveFileSystem.FileExists(controllerPath))
		{
			string text2 = null;
			try
			{
				text2 = Encoding.ASCII.GetString(SaveFileSystem.ReadAllBytes(controllerPath));
			}
			catch (Exception ex)
			{
				throw new Exception("Could not load Controller Map: " + text + ". Error: " + ex.Message);
			}
			Player player = ReInput.players.GetPlayer(0);
			if (controllerMap.controllerType == ControllerType.Joystick)
			{
				ControllerMap map = ControllerTemplateMap.FromXml(text2).ToControllerMap(controllerMap.controller);
				player.controllers.maps.AddMap(controllerMap.controller, map);
			}
			else
			{
				player.controllers.maps.AddMapFromXml(controllerMap.controllerType, controllerMap.controllerId, text2);
			}
			Debug.Log("<color=green>Controller Map " + text + " loaded successfully.</color>");
		}
	}

	public static void LoadProfileConfigFile()
	{
		string saveDirectoryPath = GetSaveDirectoryPath(CurrentProfile, getBackupDirectory: false);
		saveDirectoryPath = Path.Combine(saveDirectoryPath, "ProfileConfig.ini");
		if (!SaveFileSystem.FileExists(saveDirectoryPath))
		{
			return;
		}
		try
		{
			byte[] bytes = SaveFileSystem.ReadAllBytes(saveDirectoryPath);
			using StringReader stringReader = new StringReader(Encoding.ASCII.GetString(bytes));
			string text = null;
			while ((text = stringReader.ReadLine()) != null)
			{
				int num = text.IndexOf("=");
				if (num == -1)
				{
					continue;
				}
				string text2 = text.Substring(0, num);
				string text3 = text.Substring(num + 1);
				if (!string.IsNullOrEmpty(text3))
				{
					switch (text2)
					{
					case "RevisionNumber":
						ProfileConfigData.REVISION_NUMBER = int.Parse(text3, NumberStyles.Any, CultureInfo);
						break;
					case "UseNonScientificNames":
						ProfileConfigData.UseNonScientificNames = bool.Parse(text3);
						break;
					case "EnableDualButtonDash":
						ProfileConfigData.EnableDualButtonDash = bool.Parse(text3);
						break;
					case "EnableQuickDrop":
						ProfileConfigData.EnableQuickDrop = bool.Parse(text3);
						break;
					case "DisableMouseAiming":
						ProfileConfigData.ToggleMouseAiming = bool.Parse(text3);
						break;
					case "DisableHitSlowdown":
						ProfileConfigData.DisableSlowdownOnHit = bool.Parse(text3);
						break;
					case "DisablePressDownSpinKick":
						ProfileConfigData.DisablePressDownSpinKick = bool.Parse(text3);
						break;
					}
				}
			}
			Debug.Log("<color=green>Profile Config file loaded successfully.</color>");
		}
		catch (Exception ex)
		{
			throw new Exception("Could not load profile config file. Error: " + ex.Message);
		}
		ProfileConfigData.UpdateVersion();
		IsProfileConfigFileLoaded = true;
	}

	public static void SaveProfileConfigFile()
	{
		string saveDirectoryPath = GetSaveDirectoryPath(CurrentProfile, getBackupDirectory: false);
		saveDirectoryPath = Path.Combine(saveDirectoryPath, "ProfileConfig.ini");
		try
		{
			using (StringWriter stringWriter = new StringWriter())
			{
				stringWriter.WriteLine("[VERSION]");
				stringWriter.WriteLine("RevisionNumber=" + ProfileConfigData.REVISION_NUMBER);
				stringWriter.WriteLine();
				stringWriter.WriteLine("[Game Settings]");
				stringWriter.WriteLine("UseNonScientificNames=" + ProfileConfigData.UseNonScientificNames);
				stringWriter.WriteLine("EnableDualButtonDash=" + ProfileConfigData.EnableDualButtonDash);
				stringWriter.WriteLine("EnableQuickDrop=" + ProfileConfigData.EnableQuickDrop);
				stringWriter.WriteLine("DisableMouseAiming=" + ProfileConfigData.ToggleMouseAiming);
				stringWriter.WriteLine("DisableHitSlowdown=" + ProfileConfigData.DisableSlowdownOnHit);
				stringWriter.WriteLine("DisablePressDownSpinKick=" + ProfileConfigData.DisablePressDownSpinKick);
				stringWriter.WriteLine();
				SaveFileSystem.SaveBatch saveBatch = SaveFileSystem.BeginSaveBatch(-1);
				SaveFileSystem.WriteAllBytes(saveBatch, saveDirectoryPath, Encoding.ASCII.GetBytes(stringWriter.ToString()));
				saveBatch.End();
			}
			Debug.Log("<color=green>Profile Config file saved successfully.</color>");
		}
		catch (Exception ex)
		{
			throw new Exception("Could not save profile config file. Error: " + ex.Message);
		}
	}

	public static void LoadTrailerConfigFile()
	{
		string trailerPath = GetTrailerPath();
		if (!SaveFileSystem.FileExists(trailerPath))
		{
			Debug.Log("<color=yellow>Could not find trailer config file. Did not load successfully.</color>");
			return;
		}
		try
		{
			byte[] bytes = SaveFileSystem.ReadAllBytes(trailerPath);
			using (StringReader stringReader = new StringReader(Encoding.ASCII.GetString(bytes)))
			{
				string text = null;
				while ((text = stringReader.ReadLine()) != null)
				{
					int num = text.IndexOf("=");
					if (num == -1)
					{
						continue;
					}
					string text2 = text.Substring(0, num);
					string text3 = text.Substring(num + 1);
					if (!string.IsNullOrEmpty(text3))
					{
						switch (text2)
						{
						case "Trait1":
							TrailerConfigData.Trait1 = text3;
							break;
						case "Trait2":
							TrailerConfigData.Trait2 = text3;
							break;
						case "DeathAnim":
							TrailerConfigData.DeathAnim = int.Parse(text3, NumberStyles.Any, CultureInfo);
							break;
						}
					}
				}
			}
			Debug.Log("<color=green>Trailer Config file loaded successfully.</color>");
		}
		catch (Exception ex)
		{
			throw new Exception("Could not load trailer config file. Error: " + ex.Message);
		}
		IsTrailerConfigFileLoaded = true;
	}

	private static string GetTrailerPath()
	{
		string path = Path.Combine(SaveFileSystem.PersistentDataPath, "Saves");
		string platformDirectoryName = StoreAPIManager.GetPlatformDirectoryName();
		return Path.Combine(Path.Combine(path, platformDirectoryName), "TrailerConfig.ini");
	}
}
You are not using the latest version of the tool, please update.
Latest version is '11.2.0.10050' (yours is '8.2.0.7535-95108c96')
