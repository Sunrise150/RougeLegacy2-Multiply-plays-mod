using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class PlayerSaveData : IVersionUpdateable
{
	public int REVISION_NUMBER = 13;

	public int FILE_NUMBER;

	public string DateTimeString;

	public GameModeType GameModeType;

	public CharacterData CurrentCharacter = new CharacterData();

	public int MasterSeed = -1;

	public int PreviousMasterSeed = -1;

	public int GoldCollected;

	public int GoldCollectedBackup;

	public int GoldSaved;

	public int GoldGivenToCharon;

	public int GoldAcceptedByCharon;

	public int EquipmentOreCollected = 1500;

	public int RuneOreCollected = 1500;

	public int GoldSpent;

	public int GoldSpentOnEquipment;

	public int GoldSpentOnRunes;

	public int GoldSpentOnSkills;

	public int EquipmentOreSpent;

	public int RuneOreSpent;

	public int NumAncestors;

	public int NewGamePlusLevel;

	public uint SecondsPlayed;

	public int TimesDied;

	public int EnemiesKilled;

	public byte TimesCastleLocked;

	public byte TimesRolledRelic;

	public int HighestNGPlusBeaten = -1;

	public bool SpokenToTraitor;

	public int TimesBeatenTraitor;

	public byte TraitorPreFightRepeatDialogueIndex;

	public byte TraitorPostFightRepeatDialogueIndex;

	public bool SpokenToFinalBoss;

	public bool IsDead;

	public bool InHubTown;

	public bool InCastle;

	public bool FirstTimeGoldReceived;

	public bool GoldilocksReceived;

	public CastleLockState CastleLockState;

	public bool HasStartedGame;

	public bool UnlockAllTraitorMemories;

	public int TimesDiedSinceHestia;

	public byte HestiaCutsceneDisplayCount;

	public byte TreeCutsceneDisplayCount;

	public EndingSpawnRoomType EndingSpawnRoom;

	public float TemporaryMaxHealthMods;

	public bool EnableHouseRules;

	public float Assist_EnemyHealthMod = 1f;

	public float Assist_EnemyDamageMod = 1f;

	public float Assist_AimTimeSlow = 1f;

	public bool Assist_EnableFlightToggle;

	public bool Assist_DisableEnemyContactDamage;

	public bool Assist_DisableTraits;

	public bool Assist_EnableDifficultyDisplay;

	public float Assist_BurdenRequirementsMod = 1f;

	private CachedPlayerData m_cachedData;

	public int LineageSeed = -1;

	public int TimesRolledLineage;

	public Dictionary<PlayerSaveFlag, bool> FlagTable;

	public Dictionary<RelicType, RelicObj> RelicObjTable;

	public Dictionary<BurdenType, BurdenObj> BurdenObjTable;

	public Dictionary<BiomeType, bool> TeleporterUnlockTable;

	public SerializableVector2Int TeleporterUnlockDialogueIndex = new SerializableVector2Int(-1, -1);

	public Dictionary<TraitType, SerializableVector3Int> TraitSpawnOddsTable;

	public Dictionary<TraitType, TraitSeenState> TraitSeenTable;

	public Dictionary<AbilityType, bool> SpellSeenTable;

	public Dictionary<HeirloomType, int> HeirloomLevelTable;

	public Dictionary<SongID, FoundState> SongsFoundTable;

	public Dictionary<InsightType, InsightState> InsightStateTable;

	public Dictionary<JournalCategoryType, SerializableVector2Int> JournalsReadTable;

	public Dictionary<NPCType, SerializableVector2Int> NPCDialogueTable;

	public List<NPCType> PopulatedNPCDialoguesList;

	public int GlobalNPCDialogueCD;

	public bool TriggerGlobalNPCDialogueCD;

	public Dictionary<ClassType, int> MasteryXPTable;

	public int RunAccumulatedXP;

	[NonSerialized]
	private HeirloomLevelChangedEventArgs m_heirloomChangedEventArgs;

	[NonSerialized]
	private GoldChangedEventArgs m_goldChangedEventArgs;

	[NonSerialized]
	private GoldChangedEventArgs m_goldSavedEventArgs;

	public CachedPlayerData CachedData => m_cachedData;

	public bool IsInHeirloom
	{
		get
		{
			if (PlayerManager.IsInstantiated)
			{
				BaseRoom currentPlayerRoom = PlayerManager.GetCurrentPlayerRoom();
				if (currentPlayerRoom != null && !currentPlayerRoom.Equals(null))
				{
					return currentPlayerRoom.SpecialRoomType == SpecialRoomType.Heirloom;
				}
			}
			return false;
		}
	}

	[field: NonSerialized]
	public List<HeirloomType> TemporaryHeirloomList { get; private set; } = new List<HeirloomType>();


	public bool IsInitialized { get; private set; }

	public int GoldCollectedIncludingBank => GoldCollected + GoldSaved;

	public bool HasTrait(TraitType traitType)
	{
		if (CurrentCharacter.TraitOne != traitType)
		{
			return CurrentCharacter.TraitTwo == traitType;
		}
		return true;
	}

	public PlayerSaveData()
	{
		Initialize();
	}

	public void Initialize()
	{
		if (TemporaryHeirloomList == null)
		{
			TemporaryHeirloomList = new List<HeirloomType>();
		}
		if (m_cachedData == null)
		{
			m_cachedData = new CachedPlayerData();
		}
		if (PopulatedNPCDialoguesList == null)
		{
			PopulatedNPCDialoguesList = new List<NPCType>();
		}
		InitializeFlagTable();
		InitializeHeirloomTable();
		InitializeRelicTable();
		InitializeTraitOddsTable();
		InitializeTraitSeenTable();
		InitializeSpellSeenTable();
		InitializeSongTable();
		InitializeInsightTable();
		InitializeJournalsReadTable();
		InitializeMasteryXPTable();
		InitializeTeleporterUnlockTable();
		InitializeNPCDialogueReadTable();
		InitializeBurdenTable();
		IsInitialized = true;
	}

	private void InitializeHeirloomTable()
	{
		if (HeirloomLevelTable == null)
		{
			HeirloomLevelTable = new Dictionary<HeirloomType, int>();
		}
		HeirloomType[] typeArray = HeirloomType_RL.TypeArray;
		foreach (HeirloomType heirloomType in typeArray)
		{
			if (heirloomType != 0 && !HeirloomLevelTable.ContainsKey(heirloomType))
			{
				HeirloomLevelTable.Add(heirloomType, 0);
			}
		}
		HeirloomLevelTable[HeirloomType.Contract] = 1;
	}

	private void InitializeRelicTable()
	{
		if (RelicObjTable == null)
		{
			RelicObjTable = new Dictionary<RelicType, RelicObj>();
		}
		RelicType[] typeArray = RelicType_RL.TypeArray;
		foreach (RelicType relicType in typeArray)
		{
			if (relicType != 0 && !RelicObjTable.ContainsKey(relicType))
			{
				RelicObjTable.Add(relicType, new RelicObj(relicType));
			}
		}
	}

	private void InitializeBurdenTable()
	{
		if (BurdenObjTable == null)
		{
			BurdenObjTable = new Dictionary<BurdenType, BurdenObj>();
		}
		BurdenType[] typeArray = BurdenType_RL.TypeArray;
		foreach (BurdenType burdenType in typeArray)
		{
			if (burdenType != 0 && !BurdenObjTable.ContainsKey(burdenType))
			{
				BurdenObjTable.Add(burdenType, new BurdenObj(burdenType));
			}
		}
	}

	private void InitializeFlagTable()
	{
		if (FlagTable == null)
		{
			FlagTable = new Dictionary<PlayerSaveFlag, bool>();
		}
		foreach (PlayerSaveFlag value in Enum.GetValues(typeof(PlayerSaveFlag)))
		{
			if (value != 0 && !FlagTable.ContainsKey(value))
			{
				FlagTable.Add(value, value: false);
			}
		}
	}

	private void InitializeTraitOddsTable()
	{
		if (TraitSpawnOddsTable == null)
		{
			TraitSpawnOddsTable = new Dictionary<TraitType, SerializableVector3Int>();
		}
		TraitType[] typeArray = TraitType_RL.TypeArray;
		foreach (TraitType traitType in typeArray)
		{
			if (traitType != 0 && !TraitSpawnOddsTable.ContainsKey(traitType))
			{
				TraitSpawnOddsTable.Add(traitType, new Vector3Int(0, 0, -3));
			}
		}
	}

	private void InitializeTraitSeenTable()
	{
		if (TraitSeenTable == null)
		{
			TraitSeenTable = new Dictionary<TraitType, TraitSeenState>();
		}
		TraitType[] typeArray = TraitType_RL.TypeArray;
		foreach (TraitType traitType in typeArray)
		{
			if (traitType != 0 && !TraitSeenTable.ContainsKey(traitType))
			{
				TraitSeenTable.Add(traitType, TraitSeenState.NeverSeen);
			}
		}
	}

	private void InitializeSpellSeenTable()
	{
		if (SpellSeenTable == null)
		{
			SpellSeenTable = new Dictionary<AbilityType, bool>();
		}
		AbilityType[] spellAbilityArray = AbilityType_RL.SpellAbilityArray;
		foreach (AbilityType key in spellAbilityArray)
		{
			if (!SpellSeenTable.ContainsKey(key))
			{
				SpellSeenTable.Add(key, value: false);
			}
		}
		SpellSeenTable[AbilityType.FireballSpell] = true;
	}

	private void InitializeSongTable()
	{
		if (SongsFoundTable == null)
		{
			SongsFoundTable = new Dictionary<SongID, FoundState>();
		}
		foreach (SongID value in Enum.GetValues(typeof(SongID)))
		{
			if (value != 0 && !SongsFoundTable.ContainsKey(value))
			{
				SongsFoundTable.Add(value, FoundState.NotFound);
			}
		}
	}

	private void InitializeInsightTable()
	{
		if (InsightStateTable == null)
		{
			InsightStateTable = new Dictionary<InsightType, InsightState>();
		}
		InsightType[] typeArray = InsightType_RL.TypeArray;
		foreach (InsightType insightType in typeArray)
		{
			if (insightType != 0 && !InsightStateTable.ContainsKey(insightType))
			{
				InsightStateTable.Add(insightType, InsightState.Undiscovered);
			}
		}
	}

	private void InitializeJournalsReadTable()
	{
		if (JournalsReadTable == null)
		{
			JournalsReadTable = new Dictionary<JournalCategoryType, SerializableVector2Int>();
		}
		JournalCategoryType[] categoryTypeArray = JournalType_RL.CategoryTypeArray;
		foreach (JournalCategoryType journalCategoryType in categoryTypeArray)
		{
			if (journalCategoryType != 0 && !JournalsReadTable.ContainsKey(journalCategoryType))
			{
				JournalsReadTable.Add(journalCategoryType, new SerializableVector2Int(0, 0));
			}
		}
	}

	private void InitializeNPCDialogueReadTable()
	{
		if (NPCDialogueTable == null)
		{
			NPCDialogueTable = new Dictionary<NPCType, SerializableVector2Int>();
		}
		NPCType[] typeArray = NPCType_RL.TypeArray;
		foreach (NPCType nPCType in typeArray)
		{
			if (nPCType != NPCType.None && !NPCDialogueTable.ContainsKey(nPCType))
			{
				NPCDialogueTable.Add(nPCType, new SerializableVector2Int(-1, 0));
			}
		}
	}

	private void InitializeMasteryXPTable()
	{
		if (MasteryXPTable == null)
		{
			MasteryXPTable = new Dictionary<ClassType, int>();
		}
		ClassType[] typeArray = ClassType_RL.TypeArray;
		foreach (ClassType classType in typeArray)
		{
			if (classType != 0 && !MasteryXPTable.ContainsKey(classType))
			{
				MasteryXPTable.Add(classType, 0);
			}
		}
	}

	private void InitializeTeleporterUnlockTable()
	{
		if (TeleporterUnlockTable == null)
		{
			TeleporterUnlockTable = new Dictionary<BiomeType, bool>();
		}
	}

	public bool GetFlag(PlayerSaveFlag flag)
	{
		if (FlagTable.TryGetValue(flag, out var value))
		{
			return value;
		}
		throw new KeyNotFoundException("Cannot GET Flag: " + flag.ToString() + " not found in Player Save Data.");
	}

	public void SetFlag(PlayerSaveFlag flag, bool value)
	{
		if (FlagTable.ContainsKey(flag))
		{
			FlagTable[flag] = value;
			return;
		}
		throw new KeyNotFoundException("Cannot SET Flag: " + flag.ToString() + " not found in Player Save Data.");
	}

	public void SetAllFlags(bool value)
	{
		KeyValuePair<PlayerSaveFlag, bool>[] array = FlagTable.ToArray();
		foreach (KeyValuePair<PlayerSaveFlag, bool> keyValuePair in array)
		{
			FlagTable[keyValuePair.Key] = value;
		}
	}

	public int GetHeirloomLevel(HeirloomType heirloomType)
	{
		if (heirloomType == HeirloomType.None)
		{
			return 0;
		}
		if (HeirloomLevelTable.TryGetValue(heirloomType, out var value))
		{
			if (value == 0 && !TemporaryHeirloomList.IsNativeNull() && TemporaryHeirloomList.Contains(heirloomType))
			{
				return 1;
			}
			return value;
		}
		throw new KeyNotFoundException("Cannot GET level of Heirloom Type: " + heirloomType.ToString() + " because entry not found in Player Save Data.");
	}

	public void SetHeirloomLevel(HeirloomType heirloomType, int level, bool additive, bool broadcast)
	{
		if (HeirloomLevelTable.ContainsKey(heirloomType))
		{
			int num = HeirloomLevelTable[heirloomType];
			int value = level;
			if (additive)
			{
				value = num + level;
			}
			HeirloomLevelTable[heirloomType] = Mathf.Clamp(value, 0, 999);
			if (broadcast && num != HeirloomLevelTable[heirloomType])
			{
				if (m_heirloomChangedEventArgs == null)
				{
					m_heirloomChangedEventArgs = new HeirloomLevelChangedEventArgs(heirloomType, num, HeirloomLevelTable[heirloomType]);
				}
				else
				{
					m_heirloomChangedEventArgs.Initialize(heirloomType, num, HeirloomLevelTable[heirloomType]);
				}
				Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.HeirloomLevelChanged, null, m_heirloomChangedEventArgs);
			}
			return;
		}
		throw new KeyNotFoundException("Cannot SET level of Heirloom Type: " + heirloomType.ToString() + " because entry not found in Player Save Data.");
	}

	public void ResetAllHeirlooms()
	{
		HeirloomType[] typeArray = HeirloomType_RL.TypeArray;
		foreach (HeirloomType key in typeArray)
		{
			if (HeirloomLevelTable.ContainsKey(key))
			{
				HeirloomLevelTable[key] = 0;
			}
		}
	}

	public RelicObj GetRelic(RelicType relicType)
	{
		if (RelicObjTable.TryGetValue(relicType, out var value))
		{
			return value;
		}
		throw new KeyNotFoundException("Cannot GET Relic Type: " + relicType.ToString() + " because entry not found in Player Save Data.");
	}

	public BurdenObj GetBurden(BurdenType burdenType)
	{
		if (BurdenObjTable.TryGetValue(burdenType, out var value))
		{
			return value;
		}
		Debug.Log("Cannot GET Burden Type: " + burdenType.ToString() + " because entry not found in Player Save Data.");
		return null;
	}

	public bool GetSpellSeenState(AbilityType spellType)
	{
		if (SpellSeenTable.TryGetValue(spellType, out var value))
		{
			return value;
		}
		Debug.Log("Cannot GET SpellSeen state for spell type: " + spellType.ToString() + " because entry not found in Player Save Data.");
		return false;
	}

	public void SetSpellSeenState(AbilityType spellType, bool seenState)
	{
		if (SpellSeenTable.ContainsKey(spellType))
		{
			SpellSeenTable[spellType] = seenState;
		}
		else
		{
			Debug.Log("Cannot SET SpellSeen state for spell type: " + spellType.ToString() + " because entry not found in Player Save Data.");
		}
	}

	public int GetTotalUniqueRelics()
	{
		int num = 0;
		RelicType[] typeArray = RelicType_RL.TypeArray;
		foreach (RelicType relicType in typeArray)
		{
			if (relicType != 0 && GetRelic(relicType).Level > 0)
			{
				num++;
			}
		}
		return num;
	}

	public int GetTotalRelicLevel()
	{
		int num = 0;
		RelicType[] typeArray = RelicType_RL.TypeArray;
		foreach (RelicType relicType in typeArray)
		{
			if (relicType != 0)
			{
				num += GetRelic(relicType).Level;
			}
		}
		return num;
	}

	public float GetTotalRelicResolveCost()
	{
		float num = 0f;
		float relicCostMod = SkillTreeLogicHelper.GetRelicCostMod();
		RelicType[] typeArray = RelicType_RL.TypeArray;
		foreach (RelicType relicType in typeArray)
		{
			if (relicType != 0)
			{
				RelicObj relic = GetRelic(relicType);
				float num2 = RelicLibrary.GetRelicData(relicType).CostAmount;
				if (num2 > 0f)
				{
					num2 -= relicCostMod;
				}
				num += num2 * (float)relic.Level;
			}
		}
		return num;
	}

	public void ResetAllRelics(bool broadcast)
	{
		foreach (KeyValuePair<RelicType, RelicObj> item in RelicObjTable)
		{
			item.Value.SetFloatValue(0f, additive: false, broadcast: false);
			item.Value.SetIntValue(0, additive: false, broadcast: false);
			item.Value.SetLevel(0, additive: false, broadcast: false);
			item.Value.SetRelicMod(RelicModType.None);
			item.Value.IsFreeRelic = false;
		}
		if (broadcast)
		{
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicsReset, null, null);
		}
	}

	public FoundState GetSongFoundState(SongID songType)
	{
		if (SongsFoundTable.TryGetValue(songType, out var value))
		{
			return value;
		}
		throw new KeyNotFoundException("Cannot GET foundState for song: " + songType.ToString() + " not found in Player Save Data.");
	}

	public void SetSongFoundState(SongID songType, FoundState foundState)
	{
		if (SongsFoundTable.ContainsKey(songType))
		{
			SongsFoundTable[songType] = foundState;
			return;
		}
		throw new KeyNotFoundException("Cannot SET foundState for song: " + songType.ToString() + " not found in Player Save Data.");
	}

	public InsightState GetInsightState(InsightType insightType)
	{
		if (InsightStateTable.TryGetValue(insightType, out var value))
		{
			if (InsightState.Undiscovered > value)
			{
				return InsightState.Undiscovered;
			}
			return value;
		}
		throw new KeyNotFoundException("Cannot GET InsightState for Insight: " + insightType.ToString() + " not found in Player Save Data.");
	}

	public void SetInsightState(InsightType insightType, InsightState insightState, bool forceOverride)
	{
		if (forceOverride || insightState > GetInsightState(insightType))
		{
			if (!InsightStateTable.ContainsKey(insightType))
			{
				throw new KeyNotFoundException("Cannot SET InsightState for Insight: " + insightType.ToString() + " not found in Player Save Data.");
			}
			InsightStateTable[insightType] = insightState;
		}
	}

	public int GetJournalsRead(JournalCategoryType journalCategoryType, JournalType journalType)
	{
		if (JournalsReadTable.TryGetValue(journalCategoryType, out var value))
		{
			int a = ((journalType != 0) ? value.y : value.x);
			return Mathf.Min(a, Journal_EV.GetNumJournals(journalCategoryType, journalType));
		}
		throw new KeyNotFoundException("Cannot GET JournalsRead for Journal: " + journalCategoryType.ToString() + " not found in Player Save Data.");
	}

	public void SetJournalsRead(JournalCategoryType journalCategoryType, JournalType journalType, int value, bool additive, bool forceOverride)
	{
		if (JournalsReadTable.ContainsKey(journalCategoryType))
		{
			int journalsRead = GetJournalsRead(journalCategoryType, journalType);
			int value2 = (additive ? (journalsRead + 1) : value);
			value2 = Mathf.Clamp(value2, 0, Journal_EV.GetNumJournals(journalCategoryType, journalType));
			if (value2 > journalsRead || forceOverride)
			{
				SerializableVector2Int value3 = JournalsReadTable[journalCategoryType];
				if (journalType == JournalType.Journal)
				{
					value3.x = value2;
				}
				else
				{
					value3.y = value2;
				}
				JournalsReadTable[journalCategoryType] = value3;
			}
			return;
		}
		throw new KeyNotFoundException("Cannot SET JournalsRead for Journal: " + journalCategoryType.ToString() + " not found in Player Save Data.");
	}

	public int GetNPCDialoguesRead(NPCType npcType)
	{
		if (NPCDialogueTable.TryGetValue(npcType, out var value))
		{
			return value.x;
		}
		throw new KeyNotFoundException("Cannot GET NPCDialogueRead for type: " + npcType.ToString() + ". Not found in Player Save Data.");
	}

	public void SetNPCDialoguesRead(NPCType npcType, int value, bool additive)
	{
		if (NPCDialogueTable.ContainsKey(npcType))
		{
			int nPCDialoguesRead = GetNPCDialoguesRead(npcType);
			int value2 = (additive ? (nPCDialoguesRead + value) : value);
			int max = (NPCDialogue_EV.NPCDialogueTable.ContainsKey(npcType) ? NPCDialogue_EV.NPCDialogueTable[npcType].Length : 0);
			value2 = Mathf.Clamp(value2, -1, max);
			SerializableVector2Int value3 = NPCDialogueTable[npcType];
			value3.x = value2;
			NPCDialogueTable[npcType] = value3;
			return;
		}
		throw new KeyNotFoundException("Cannot SET NPCDialogueRead for type: " + npcType.ToString() + ". Not found in Player Save Data.");
	}

	public int GetNPCDialogueCooldown(NPCType npcType)
	{
		if (NPCDialogueTable.TryGetValue(npcType, out var value))
		{
			return value.y;
		}
		throw new KeyNotFoundException("Cannot GET NPCDialogueCD for type: " + npcType.ToString() + ". Not found in Player Save Data.");
	}

	public void SetNPCDialogueCooldown(NPCType npcType, int value, bool additive)
	{
		if (NPCDialogueTable.ContainsKey(npcType))
		{
			int nPCDialogueCooldown = GetNPCDialogueCooldown(npcType);
			int value2 = (additive ? (nPCDialogueCooldown + value) : value);
			value2 = Mathf.Clamp(value2, 0, int.MaxValue);
			SerializableVector2Int value3 = NPCDialogueTable[npcType];
			value3.y = value2;
			NPCDialogueTable[npcType] = value3;
			return;
		}
		throw new KeyNotFoundException("Cannot SET NPCDialogueRead for type: " + npcType.ToString() + ". Not found in Player Save Data.");
	}

	public int GetClassXP(ClassType classType)
	{
		if (MasteryXPTable.TryGetValue(classType, out var value))
		{
			return Mathf.Max(0, value);
		}
		throw new KeyNotFoundException("Cannot GET ClassXP for Class: " + classType.ToString() + " not found in Player Save Data.");
	}

	public void SetClassXP(ClassType classType, int value, bool additive, bool forceOverride, bool ignoreMaxLevelClamping)
	{
		if (MasteryXPTable.ContainsKey(classType))
		{
			int classXP = GetClassXP(classType);
			int num = (additive ? (classXP + value) : value);
			if (num > classXP || forceOverride)
			{
				num = (ignoreMaxLevelClamping ? Mathf.Clamp(num, 0, Mastery_EV.XP_REQUIRED[Mastery_EV.XP_REQUIRED.Length - 1]) : Mathf.Clamp(num, 0, Mastery_EV.XP_REQUIRED[Mastery_EV.GetMaxMasteryRank()]));
				MasteryXPTable[classType] = num;
			}
			return;
		}
		throw new KeyNotFoundException("Cannot SET ClassXP for Class: " + classType.ToString() + " not found in Player Save Data.");
	}

	public int GetClassMasteryRank(ClassType classType)
	{
		int num = Mastery_EV.CalculateRankV2(GetClassXP(classType));
		if (num < 0)
		{
			return 0;
		}
		return Mathf.Clamp(num, 0, Mastery_EV.GetMaxMasteryRank() + 1);
	}

	public int GetDriftingWorldsMasteryRank(ClassType classType)
	{
		int classXP = GetClassXP(classType);
		int result = 0;
		for (int i = 0; i < Mastery_EV.DRIFTING_WORLDS_XP_REQUIRED.Length; i++)
		{
			if (classXP < Mastery_EV.DRIFTING_WORLDS_XP_REQUIRED[i])
			{
				result = i;
				break;
			}
		}
		return result;
	}

	public bool GetTeleporterIsUnlocked(BiomeType biomeType)
	{
		if (!TeleporterUnlockTable.ContainsKey(biomeType))
		{
			TeleporterUnlockTable.Add(biomeType, value: false);
		}
		return TeleporterUnlockTable[biomeType];
	}

	public void SetTeleporterIsUnlocked(BiomeType biomeType, bool state)
	{
		if (!TeleporterUnlockTable.ContainsKey(biomeType))
		{
			TeleporterUnlockTable.Add(biomeType, value: false);
		}
		TeleporterUnlockTable[biomeType] = state;
	}

	public void SubtractFromGoldIncludingBank(int value)
	{
		if (value > GoldCollectedIncludingBank)
		{
			Debug.Log("<color=red>Attempting to subtract a value higher than the actual gold own.  This should not be possible.</color>");
			return;
		}
		int num = value;
		if (num >= GoldCollected)
		{
			num -= GoldCollected;
			GoldCollected = 0;
			GoldSaved -= num;
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldSavedChanged, null, null);
		}
		else
		{
			GoldCollected -= num;
		}
	}

	public string GetActualAvailableGoldString()
	{
		return GoldCollectedIncludingBank.ToString();
	}

	public void UpdateCachedData()
	{
		CachedData.UpdateData();
	}

	public void UpdateVersion()
	{
		Initialize();
		if (CurrentCharacter.TraitOne == TraitType.Antique_Old)
		{
			CurrentCharacter.TraitOne = TraitType.Antique;
		}
		if (CurrentCharacter.TraitTwo == TraitType.Antique_Old)
		{
			CurrentCharacter.TraitTwo = TraitType.Antique;
		}
		if (REVISION_NUMBER != 13)
		{
			if (REVISION_NUMBER <= 0)
			{
				SetFlag(PlayerSaveFlag.CastleBoss_Defeated, value: false);
			}
			if (REVISION_NUMBER <= 1)
			{
				UnequipAllGear();
				int num = (int)((float)GoldSpent * 0.7f);
				GoldCollected += num;
				GoldCollectedBackup = GoldCollected;
				GoldSpent = 0;
				RuneOreCollected += RuneOreSpent;
				RuneOreSpent = 0;
				EquipmentOreCollected += EquipmentOreSpent;
				EquipmentOreSpent = 0;
				SetFlag(PlayerSaveFlag.PlayFarShoresWarning, value: true);
			}
			if (REVISION_NUMBER <= 2)
			{
				int goldSpentOnSkills = GoldSpentOnSkills;
				GoldCollected += goldSpentOnSkills;
				GoldCollectedBackup = GoldCollected;
				GoldSpentOnSkills = 0;
				UnequipAllGear();
				SetFlag(PlayerSaveFlag.PlayArcaneHallowsWarning, value: true);
			}
			if (REVISION_NUMBER <= 3)
			{
				SetFlag(PlayerSaveFlag.CastleBoss_Defeated_FirstTime, GetFlag(PlayerSaveFlag.CastleBoss_Defeated));
				SetFlag(PlayerSaveFlag.BridgeBoss_Defeated_FirstTime, GetFlag(PlayerSaveFlag.BridgeBoss_Defeated));
				SetFlag(PlayerSaveFlag.ForestBoss_Defeated_FirstTime, GetFlag(PlayerSaveFlag.ForestBoss_Defeated));
				SetFlag(PlayerSaveFlag.StudyBoss_Defeated_FirstTime, GetFlag(PlayerSaveFlag.StudyBoss_Defeated));
				UnequipAllGear();
				int num2 = Mathf.RoundToInt(GoldSpentOnSkills);
				GoldCollected += num2;
				GoldCollectedBackup = GoldCollected;
				GoldSpentOnSkills = 0;
				ClassType[] typeArray = ClassType_RL.TypeArray;
				foreach (ClassType classType in typeArray)
				{
					if (classType != 0)
					{
						int classXP = GetClassXP(classType);
						SetClassXP(classType, classXP * 5, additive: false, forceOverride: true, ignoreMaxLevelClamping: true);
					}
				}
				SetFlag(PlayerSaveFlag.PlayDriftingWorldsWarning, value: true);
			}
			if (REVISION_NUMBER <= 4)
			{
				if (GetFlag(PlayerSaveFlag.CastleBoss_Defeated_FirstTime))
				{
					SaveManager.ModeSaveData.SetHighestNGBossBeaten(GameModeType.Regular, BossID.Castle_Boss, 0, forceOverride: false);
				}
				if (GetFlag(PlayerSaveFlag.BridgeBoss_Defeated_FirstTime))
				{
					SaveManager.ModeSaveData.SetHighestNGBossBeaten(GameModeType.Regular, BossID.Bridge_Boss, 0, forceOverride: false);
				}
				if (GetFlag(PlayerSaveFlag.ForestBoss_Defeated_FirstTime))
				{
					SaveManager.ModeSaveData.SetHighestNGBossBeaten(GameModeType.Regular, BossID.Forest_Boss, 0, forceOverride: false);
				}
				if (GetFlag(PlayerSaveFlag.StudyBoss_Defeated_FirstTime))
				{
					SaveManager.ModeSaveData.SetHighestNGBossBeaten(GameModeType.Regular, BossID.Study_Boss, 0, forceOverride: false);
				}
				if (GetFlag(PlayerSaveFlag.TowerBoss_Defeated_FirstTime))
				{
					SaveManager.ModeSaveData.SetHighestNGBossBeaten(GameModeType.Regular, BossID.Tower_Boss, 0, forceOverride: false);
				}
				if (NewGamePlusLevel > 0)
				{
					foreach (BiomeType item in TeleporterUnlockTable.Keys.ToList())
					{
						SetTeleporterIsUnlocked(item, state: false);
					}
				}
			}
			if (REVISION_NUMBER <= 5)
			{
				UnequipAllGear();
				SetFlag(PlayerSaveFlag.PlayPizzaMundiWarning, value: true);
			}
			if (REVISION_NUMBER <= 7)
			{
				GoldAcceptedByCharon = GoldGivenToCharon;
				if (!GetFlag(PlayerSaveFlag.TowerBoss_Defeated_FirstTime))
				{
					HighestNGPlusBeaten = -1;
				}
				Assist_EnemyHealthMod = 1f;
				Assist_EnemyDamageMod = 1f;
				ClassType[] typeArray = ClassType_RL.TypeArray;
				foreach (ClassType classType2 in typeArray)
				{
					if (classType2 != 0)
					{
						int classXP2 = GetClassXP(classType2);
						int driftingWorldsMasteryRank = GetDriftingWorldsMasteryRank(classType2);
						classXP2 = Mathf.RoundToInt((float)classXP2 * (1f + (float)driftingWorldsMasteryRank * 0.0675f));
						SetClassXP(classType2, classXP2, additive: false, forceOverride: true, ignoreMaxLevelClamping: true);
					}
				}
			}
			if (REVISION_NUMBER <= 8)
			{
				JournalCategoryType[] categoryTypeArray = JournalType_RL.CategoryTypeArray;
				foreach (JournalCategoryType journalCategoryType in categoryTypeArray)
				{
					if (journalCategoryType != 0)
					{
						int journalsRead = GetJournalsRead(journalCategoryType, JournalType.Journal);
						int journalsRead2 = GetJournalsRead(journalCategoryType, JournalType.MemoryFragment);
						if (journalsRead > 0 || journalsRead2 > 0)
						{
							SetFlag(PlayerSaveFlag.JournalReadOnce, value: true);
							break;
						}
					}
				}
			}
			if (REVISION_NUMBER <= 9)
			{
				SetFlag(PlayerSaveFlag.PlayDragonsVowWarning, value: true);
				UnequipAllGear();
				int num3 = Mathf.RoundToInt(GoldSpentOnSkills);
				GoldCollected += num3;
				GoldSpentOnSkills = 0;
				if (RelicObjTable.ContainsKey((RelicType)460))
				{
					RelicObjTable.Remove((RelicType)460);
				}
				if (Assist_EnemyDamageMod == 0f)
				{
					Assist_EnemyDamageMod = 1f;
				}
				if (Assist_EnemyHealthMod < 0.5f)
				{
					Assist_EnemyHealthMod = 0.5f;
				}
				GoldCollectedBackup = GoldCollected;
			}
			if (REVISION_NUMBER <= 10 && Assist_AimTimeSlow == 0f)
			{
				Assist_AimTimeSlow = 1f;
			}
			if (REVISION_NUMBER <= 11 && HeirloomLevelTable.ContainsKey(HeirloomType.Reliquary) && HeirloomLevelTable[HeirloomType.Reliquary] > 0)
			{
				HeirloomLevelTable[HeirloomType.Reliquary] = 0;
			}
			if (REVISION_NUMBER <= 12 && Assist_BurdenRequirementsMod == 0f)
			{
				Assist_BurdenRequirementsMod = 1f;
			}
		}
		REVISION_NUMBER = 13;
	}

	private void UnequipAllGear()
	{
		CurrentCharacter.EdgeEquipmentType = EquipmentType.None;
		CurrentCharacter.HeadEquipmentType = EquipmentType.None;
		CurrentCharacter.ChestEquipmentType = EquipmentType.None;
		CurrentCharacter.CapeEquipmentType = EquipmentType.None;
		CurrentCharacter.TrinketEquipmentType = EquipmentType.None;
	}

	public void ResetForNewGamePlus(bool resetRelics)
	{
		if (resetRelics)
		{
			RelicType relicType = RelicType.None;
			RelicType relicType2 = RelicType.None;
			if (SaveManager.PlayerSaveData.CurrentCharacter.TraitOne == TraitType.Antique)
			{
				relicType = SaveManager.PlayerSaveData.CurrentCharacter.AntiqueOneOwned;
			}
			if (SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo == TraitType.Antique)
			{
				relicType2 = SaveManager.PlayerSaveData.CurrentCharacter.AntiqueTwoOwned;
			}
			foreach (KeyValuePair<RelicType, RelicObj> item in RelicObjTable)
			{
				if (item.Key != RelicType.ExtraLife_Unity && item.Key != RelicType.ExtraLife_UnityUsed)
				{
					item.Value.SetFloatValue(0f, additive: false, broadcast: false);
					item.Value.SetIntValue(0, additive: false, broadcast: false);
					item.Value.SetRelicMod(RelicModType.None);
					if (item.Key == relicType || item.Key == relicType2)
					{
						item.Value.SetLevel(1, additive: false, broadcast: false);
						continue;
					}
					item.Value.SetLevel(0, additive: false, broadcast: false);
					item.Value.IsFreeRelic = false;
				}
			}
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.RelicsReset, null, null);
		}
		SetFlag(PlayerSaveFlag.CastleBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.BridgeBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.ForestBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.StudyBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.StudyMiniboss_SpearKnight_Defeated, value: false);
		SetFlag(PlayerSaveFlag.StudyMiniboss_SwordKnight_Defeated, value: false);
		SetFlag(PlayerSaveFlag.TowerBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.CaveBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.CaveMiniboss_White_Defeated, value: false);
		SetFlag(PlayerSaveFlag.CaveMiniboss_Black_Defeated, value: false);
		SetFlag(PlayerSaveFlag.CaveMiniboss_WhiteDoor_Opened, value: false);
		SetFlag(PlayerSaveFlag.CaveMiniboss_BlackDoor_Opened, value: false);
		SetFlag(PlayerSaveFlag.GardenBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.FinalBoss_Defeated, value: false);
		SetFlag(PlayerSaveFlag.BlackChest_Weapon_Opened, value: false);
		SetFlag(PlayerSaveFlag.BlackChest_Helm_Opened, value: false);
		SetFlag(PlayerSaveFlag.BlackChest_Chest_Opened, value: false);
		SetFlag(PlayerSaveFlag.BlackChest_Cape_Opened, value: false);
		SetFlag(PlayerSaveFlag.BlackChest_Trinket_Opened, value: false);
		SetInsightState(InsightType.CastleBoss_DoorOpened, InsightState.Undiscovered, forceOverride: true);
		SetInsightState(InsightType.ForestBoss_DoorOpened, InsightState.Undiscovered, forceOverride: true);
		SetInsightState(InsightType.BridgeBoss_GateRaised, InsightState.Undiscovered, forceOverride: true);
		SetInsightState(InsightType.StudyBoss_DoorOpened, InsightState.Undiscovered, forceOverride: true);
		SetInsightState(InsightType.CaveBoss_DoorOpened, InsightState.Undiscovered, forceOverride: true);
		SetFlag(PlayerSaveFlag.DragonDialogue_Intro, value: false);
		SetFlag(PlayerSaveFlag.DragonDialogue_BossDoorOpen, value: false);
		SetFlag(PlayerSaveFlag.DragonDialogue_AfterDefeatingTubal, value: false);
		SetFlag(PlayerSaveFlag.DragonDialogue_Sleep, value: false);
		SetFlag(PlayerSaveFlag.CaveTuningForkTriggered, value: false);
		SetFlag(PlayerSaveFlag.Johan_First_Death_Intro, value: false);
		SetFlag(PlayerSaveFlag.Johan_Getting_Memory_Heirloom, value: false);
		SetFlag(PlayerSaveFlag.Johan_Entering_Secret_Tower, value: false);
		SetFlag(PlayerSaveFlag.Johan_After_Beating_Castle_Boss, value: false);
		SetFlag(PlayerSaveFlag.Johan_Finding_On_Bridge, value: false);
		SetFlag(PlayerSaveFlag.Johan_After_Beating_Bridge_Boss, value: false);
		SetFlag(PlayerSaveFlag.Johan_Sitting_At_Far_Shore, value: false);
		SetFlag(PlayerSaveFlag.Johan_After_Beating_Forest_Boss, value: false);
		SetFlag(PlayerSaveFlag.Johan_Reaching_Sun_Tower_Top, value: false);
		SetFlag(PlayerSaveFlag.Johan_After_Beating_Study_Boss, value: false);
		SetFlag(PlayerSaveFlag.Johan_After_Beating_Tower_Boss, value: false);
		SetFlag(PlayerSaveFlag.Johan_After_Getting_Heirloom_Lantern, value: false);
		SetFlag(PlayerSaveFlag.SeenParade, value: false);
		SetFlag(PlayerSaveFlag.SeenNGConfirmWindow, value: false);
		SetFlag(PlayerSaveFlag.SeenTrueEnding, value: false);
		SetHeirloomLevel(HeirloomType.Fruit, 0, additive: false, broadcast: false);
		CastleLockState = CastleLockState.NotLocked;
		TimesCastleLocked = 0;
		TimesRolledRelic = 0;
		InCastle = false;
		InHubTown = false;
		SpokenToFinalBoss = false;
		SpokenToTraitor = false;
		EndingSpawnRoom = EndingSpawnRoomType.None;
		foreach (BiomeType item2 in TeleporterUnlockTable.Keys.ToList())
		{
			SetTeleporterIsUnlocked(item2, state: false);
		}
	}

	public void GiveMoneyToCharon(MonoBehaviour caller)
	{
		if (GoldCollected <= 0)
		{
			return;
		}
		int num = GoldCollected;
		float num2 = 0f;
		int num3 = 0;
		if (SkillTreeManager.GetSkillObjLevel(SkillTreeType.Gold_Saved_Unlock) > 0)
		{
			num2 = 0.1f;
			num2 += SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Saved_Amount_Saved).CurrentStatGain;
			num3 = 1000;
			num3 += (int)SkillTreeManager.GetSkillTreeObj(SkillTreeType.Gold_Saved_Cap_Up).CurrentStatGain;
			GoldSaved = Mathf.Clamp(GoldSaved, 0, num3);
			int num4 = num3 - GoldSaved;
			if (num4 > 0)
			{
				int num5 = Mathf.Clamp(Mathf.RoundToInt((float)num * num2), 0, num4);
				GoldSaved += num5;
				GoldSaved = Mathf.Clamp(GoldSaved, 0, num3);
				num -= num5;
				if (num5 > 0)
				{
					if (m_goldSavedEventArgs == null)
					{
						m_goldSavedEventArgs = new GoldChangedEventArgs(GoldSaved - num5, GoldSaved);
					}
					else
					{
						m_goldSavedEventArgs.Initialize(GoldSaved - num5, GoldSaved);
					}
					Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldSavedChanged, caller, m_goldSavedEventArgs);
				}
			}
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		Vector2 absPos = playerController.Midpoint;
		absPos.y += playerController.CollisionBounds.height / 2f;
		string text = string.Format(LocalizationManager.GetString("LOC_ID_GOLD_UI_NEGATIVE_GOLD_POPUP_1", isFemale: false), num);
		TextPopupManager.DisplayTextAtAbsPos(TextPopupType.PlayerHit, text, absPos);
		GoldGivenToCharon += num;
		GoldCollected = 0;
		if (GoldAcceptedByCharon < SkillTree_EV.GetMaxCharonAcceptableGold())
		{
			GoldAcceptedByCharon = Mathf.Min(GoldAcceptedByCharon + num, SkillTree_EV.GetMaxCharonAcceptableGold());
		}
		if (m_goldChangedEventArgs == null)
		{
			m_goldChangedEventArgs = new GoldChangedEventArgs(num, 0);
		}
		else
		{
			m_goldChangedEventArgs.Initialize(num, 0);
		}
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GoldChanged, caller, m_goldChangedEventArgs);
	}
}
