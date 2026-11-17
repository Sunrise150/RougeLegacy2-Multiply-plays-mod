using System;
using System.Collections.Generic;
using SceneManagement_RL;
using UnityEngine;

public class ChallengeManager : MonoBehaviour
{
	private static bool m_isDisposed;

	private static bool m_isInitialized;

	private CharacterData m_storedCharData;

	private Dictionary<RelicType, int> m_storedRelicDict = new Dictionary<RelicType, int>();

	private Dictionary<RuneType, int> m_storedRuneDict = new Dictionary<RuneType, int>();

	private Dictionary<ClassType, int> m_storedMasteryXPTable = new Dictionary<ClassType, int>();

	private Dictionary<SkillTreeType, int> m_storedSkillTreeDict = new Dictionary<SkillTreeType, int>();

	private Dictionary<BurdenType, int> m_storedBurdenDict = new Dictionary<BurdenType, int>();

	private Dictionary<SoulShopType, int> m_storedSoulShopDict = new Dictionary<SoulShopType, int>();

	private Dictionary<HeirloomType, int> m_storedHeirloomLevelDict = new Dictionary<HeirloomType, int>();

	private float m_storedTemporaryMaxHealthMods;

	private int m_storedNGPlusLevel;

	private TraitChangedEventArgs m_traitChangedArgs;

	private Action<MonoBehaviour, EventArgs> m_onStartGlobalTimer;

	private Action<MonoBehaviour, EventArgs> m_onStopGlobalTimer;

	private Action<MonoBehaviour, EventArgs> m_onPlayerHitIncreaseTimer;

	private static ChallengeManager m_challengeManager = null;

	private static List<ChallengeType> m_challengeTypeListHelper = new List<ChallengeType>();

	private static readonly string[] m_letterGradeArray = new string[20]
	{
		"C+", "C+", "C+", "C++", "C++", "C++", "B", "B", "B", "B+",
		"B+", "B+", "B++", "B++", "A", "A+", "A++", "S", "S+", "S++"
	};

	public static ChallengeManager Instance
	{
		get
		{
			_ = m_isDisposed;
			_ = m_isInitialized;
			if (!m_challengeManager)
			{
				m_challengeManager = CDGHelper.FindStaticInstance<ChallengeManager>(createNewIfNull: false);
			}
			return m_challengeManager;
		}
	}

	public static bool IsInitialized => m_isInitialized;

	public static bool IsDisposed => m_isDisposed;

	public static bool IsInChallenge => !ActiveChallenge.IsNativeNull();

	public static ChallengeObj ActiveChallenge { get; private set; }

	public static ChallengeTunnelController ChallengeTunnelController { get; set; }

	public static int HitsTaken { get; set; }

	public static bool NeedsSave { get; set; }

	private void Awake()
	{
		m_onStartGlobalTimer = OnStartGlobalTimer;
		m_onStopGlobalTimer = OnStopGlobalTimer;
		m_onPlayerHitIncreaseTimer = OnPlayerHitIncreaseTimer;
		Initialize();
	}

	private void Initialize()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.StartGlobalTimer, m_onStartGlobalTimer);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.StopGlobalTimer, m_onStopGlobalTimer);
		m_traitChangedArgs = new TraitChangedEventArgs(TraitType.None, TraitType.None);
		if (!SaveManager.ModeSaveData.IsInitialized)
		{
			SaveManager.ModeSaveData.Initialize();
		}
		m_isInitialized = true;
	}

	private void OnDestroy()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.StartGlobalTimer, m_onStartGlobalTimer);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.StopGlobalTimer, m_onStopGlobalTimer);
		m_isDisposed = true;
	}

	public static bool DoesChallengeExist(ChallengeType challengeType)
	{
		return GetChallenge(challengeType) != null;
	}

	public static bool CanEnterChallenge(ChallengeType challengeType, bool suppressLogs = true)
	{
		if (!DoesChallengeExist(challengeType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>ChallengeManager.CanEnterChallenge({0}) returned false because challenge obj could not be found.</color>", challengeType);
			}
			return false;
		}
		if (GetFoundState(challengeType) <= FoundState.NotFound)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>ChallengeManager.CanEnterChallenge({0}) returned false because challenge found state is 'Not Found'.</color>", challengeType);
			}
			return false;
		}
		return true;
	}

	public static bool CanEquip(ChallengeType challengeType, bool suppressLogs = true)
	{
		if (!DoesChallengeExist(challengeType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>ChallengeManager.CanEquip({0}) returned false because challenge obj could not be found.</color>", challengeType);
			}
			return false;
		}
		if (GetFoundState(challengeType) <= FoundState.NotFound)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>ChallengeManager.CanEquip({0}) returned false because challenge has not been found yet.</color>", challengeType);
			}
			return false;
		}
		int challengeEquippedLevel = GetChallengeEquippedLevel(challengeType);
		int upgradeBlueprintsFound = GetUpgradeBlueprintsFound(challengeType);
		if (challengeEquippedLevel >= upgradeBlueprintsFound)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>ChallengeManager.CanEquip({0}) returned false because player has already equipped all handicaps they own of this type.</color>", challengeType);
			}
			return false;
		}
		return true;
	}

	public static List<ChallengeType> GetAllChallengesWithFoundState(FoundState foundState)
	{
		GetAllChallengesWithFoundState(foundState, m_challengeTypeListHelper);
		return m_challengeTypeListHelper;
	}

	public static void GetAllChallengesWithFoundState(FoundState foundState, List<ChallengeType> results)
	{
		results.Clear();
		foreach (KeyValuePair<ChallengeType, ChallengeObj> item in SaveManager.ModeSaveData.ChallengeDict)
		{
			ChallengeObj value = item.Value;
			if (value.FoundState == foundState)
			{
				results.Add(value.ChallengeType);
			}
		}
	}

	public static FoundState GetFoundState(ChallengeType challengeType)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		FoundState foundState = FoundState.NotFound;
		foundState = challenge?.FoundState ?? FoundState.NotFound;
		if (foundState < FoundState.NotFound)
		{
			foundState = FoundState.NotFound;
		}
		return foundState;
	}

	public static bool SetFoundState(ChallengeType challengeType, FoundState foundState, bool overrideValues, bool runEvents = true)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		if (challenge != null)
		{
			FoundState foundState2 = GetFoundState(challengeType);
			if (foundState > foundState2 || overrideValues)
			{
				challenge.FoundLevel = (int)foundState;
				return true;
			}
			Debug.LogFormat("<color=red>Could not set challenge {0} to FoundState: {1}. Current FoundState: {2} is a higher value. Please set overrideValues = true to override this if intended.</color>", challengeType, foundState, GetFoundState(challengeType));
			return false;
		}
		Debug.LogFormat("<color=red>Could not set challenge {0} to FoundState: {1}. Challenge obj not found.</color>", challengeType, foundState);
		return false;
	}

	public static int GetUpgradeBlueprintsFound(ChallengeType challengeType, bool ignoreInfinitePurchasePower = false)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		if (challenge != null)
		{
			return Mathf.Clamp(challenge.UpgradeBlueprintsFound, 0, challenge.MaxLevel);
		}
		return 0;
	}

	public static bool SetUpgradeBlueprintsFound(ChallengeType challengeType, int level, bool additive, bool runEvents = true)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		if (challenge != null)
		{
			int upgradeBlueprintsFound = challenge.UpgradeBlueprintsFound;
			upgradeBlueprintsFound = ((!additive) ? level : (upgradeBlueprintsFound + level));
			if (upgradeBlueprintsFound > challenge.MaxLevel)
			{
				Debug.Log("<color=red>ChallengeManager.SetUpgradeBlueprintsFound(" + challengeType.ToString() + ") failed.  Upgrading will put blueprints found beyond max level.</color>");
				return false;
			}
			challenge.UpgradeBlueprintsFound = Mathf.Clamp(upgradeBlueprintsFound, 0, challenge.MaxLevel);
			return true;
		}
		return false;
	}

	public static int GetChallengeEquippedLevel(ChallengeType challengeType)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		if (challenge != null)
		{
			return Mathf.Clamp(challenge.EquippedLevel, 0, challenge.MaxEquippableLevel);
		}
		return 0;
	}

	public static bool SetChallengeEquippedLevel(ChallengeType challengeType, int level, bool additive, bool runEvents = true)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		if (challenge != null)
		{
			int equippedLevel = challenge.EquippedLevel;
			equippedLevel = ((!additive) ? level : (equippedLevel + level));
			equippedLevel = Mathf.Clamp(equippedLevel, 0, challenge.MaxEquippableLevel);
			if (equippedLevel != challenge.EquippedLevel)
			{
				challenge.EquippedLevel = equippedLevel;
				return true;
			}
		}
		return false;
	}

	public static ChallengeObj GetChallenge(ChallengeType challengeType)
	{
		return SaveManager.ModeSaveData.ChallengeDict[challengeType];
	}

	public static int GetTotalTrophiesEarned(bool includeTutorialPurified)
	{
		int num = 0;
		ChallengeType[] typeArray = ChallengeType_RL.TypeArray;
		foreach (ChallengeType challengeType in typeArray)
		{
			if (challengeType != 0 && (includeTutorialPurified || challengeType != ChallengeType.TutorialPurified) && GetChallengeTrophyRank(challengeType) != 0)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetTotalTrophiesEarnedOfRank(ChallengeTrophyRank trophyRank, bool includeTutorialPurified)
	{
		int num = 0;
		ChallengeType[] typeArray = ChallengeType_RL.TypeArray;
		foreach (ChallengeType challengeType in typeArray)
		{
			if (challengeType != 0 && (includeTutorialPurified || challengeType != ChallengeType.TutorialPurified) && GetChallengeTrophyRank(challengeType) == trophyRank)
			{
				num++;
			}
		}
		return num;
	}

	public static int GetTotalTrophiesEarnedOfRankOrHigher(ChallengeTrophyRank trophyRank, bool includeTutorialPurified)
	{
		int num = 0;
		ChallengeType[] typeArray = ChallengeType_RL.TypeArray;
		foreach (ChallengeType challengeType in typeArray)
		{
			if (challengeType != 0 && (includeTutorialPurified || challengeType != ChallengeType.TutorialPurified) && GetChallengeTrophyRank(challengeType) >= trophyRank)
			{
				num++;
			}
		}
		return num;
	}

	public static void SetActiveChallenge(ChallengeType challengeType)
	{
		if (challengeType == ChallengeType.None)
		{
			ActiveChallenge = null;
			return;
		}
		ActiveChallenge = GetChallenge(challengeType);
		HitsTaken = 0;
	}

	public static float GetActiveHandicapMod()
	{
		if (ActiveChallenge.ChallengeData.ScoringType == ChallengeScoringType.Battle)
		{
			return (float)ActiveChallenge.EquippedLevel * ActiveChallenge.ChallengeData.ScalingHandicap;
		}
		return 0f;
	}

	public static void ReturnToDriftHouseWithTransition()
	{
		if (!SceneLoader_RL.IsRunningTransitionWithLogic)
		{
			SceneLoader_RL.RunTransitionWithLogic(delegate
			{
				ChallengeTunnelController.ReturnToDriftHouse();
			}, TransitionID.ScreenDistortion, cleanup: false);
		}
	}

	public static ClassType GetChallengeClassOverride(ChallengeType challengeType)
	{
		return GetChallenge(challengeType).ChallengeData.ClassOverride;
	}

	public static void SetupCharacter()
	{
		Instance.m_storedCharData = SaveManager.PlayerSaveData.CurrentCharacter.Clone();
		ClassType classType = GetChallengeClassOverride(ActiveChallenge.ChallengeType);
		if (classType == ClassType.None && (SaveManager.PlayerSaveData.CurrentCharacter.TraitOne == TraitType.RandomizeKit || SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo == TraitType.RandomizeKit))
		{
			classType = SaveManager.PlayerSaveData.CurrentCharacter.ClassType;
		}
		if (classType != 0)
		{
			CharacterCreator.GenerateClass(classType, SaveManager.PlayerSaveData.CurrentCharacter);
			SaveManager.PlayerSaveData.CurrentCharacter.Spell = Instance.m_storedCharData.Spell;
		}
		if (SaveManager.PlayerSaveData.CurrentCharacter.Weapon == AbilityType.PacifistWeapon)
		{
			SaveManager.PlayerSaveData.CurrentCharacter.Weapon = CharacterCreator.GetAvailableWeapons(SaveManager.PlayerSaveData.CurrentCharacter.ClassType)[0];
		}
		SaveManager.PlayerSaveData.CurrentCharacter.TraitOne = TraitType.None;
		SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo = TraitType.None;
		Instance.m_traitChangedArgs.Initialize(TraitType.None, TraitType.None);
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.TraitsChanged, Instance, Instance.m_traitChangedArgs);
		SaveManager.PlayerSaveData.CurrentCharacter.AntiqueOneOwned = RelicType.None;
		SaveManager.PlayerSaveData.CurrentCharacter.AntiqueTwoOwned = RelicType.None;
		SaveManager.PlayerSaveData.CurrentCharacter.CapeEquipmentType = EquipmentType.None;
		SaveManager.PlayerSaveData.CurrentCharacter.ChestEquipmentType = EquipmentType.None;
		SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType = EquipmentType.None;
		SaveManager.PlayerSaveData.CurrentCharacter.HeadEquipmentType = EquipmentType.None;
		SaveManager.PlayerSaveData.CurrentCharacter.TrinketEquipmentType = EquipmentType.None;
		Instance.m_storedRuneDict.Clear();
		foreach (KeyValuePair<RuneType, RuneObj> item in SaveManager.EquipmentSaveData.RuneDict)
		{
			Instance.m_storedRuneDict.Add(item.Key, item.Value.EquippedLevel);
			item.Value.EquippedLevel = 0;
		}
		Instance.m_storedRelicDict.Clear();
		foreach (KeyValuePair<RelicType, RelicObj> item2 in SaveManager.PlayerSaveData.RelicObjTable)
		{
			Instance.m_storedRelicDict.Add(item2.Key, item2.Value.Level);
			item2.Value.SetLevel(0, additive: false);
		}
		Instance.m_storedMasteryXPTable.Clear();
		ClassType[] typeArray = ClassType_RL.TypeArray;
		foreach (ClassType classType2 in typeArray)
		{
			if (classType2 != 0)
			{
				Instance.m_storedMasteryXPTable.Add(classType2, SaveManager.PlayerSaveData.GetClassXP(classType2));
				SaveManager.PlayerSaveData.SetClassXP(classType2, 0, additive: false, forceOverride: true, ignoreMaxLevelClamping: true);
			}
		}
		Instance.m_storedSkillTreeDict.Clear();
		foreach (KeyValuePair<SkillTreeType, SkillTreeObj> item3 in SaveManager.EquipmentSaveData.SkillTreeDict)
		{
			Instance.m_storedSkillTreeDict.Add(item3.Key, item3.Value.Level);
			SkillTreeManager.SetSkillObjLevel(item3.Key, 0, additive: false);
		}
		SaveManager.EquipmentSaveData.SkillTreeDict[SkillTreeType.Relic_Cost_Down].Level = 5;
		SaveManager.EquipmentSaveData.SkillTreeDict[SkillTreeType.Reroll_Relic].Level = 3;
		SaveManager.EquipmentSaveData.SkillTreeDict[SkillTreeType.Reroll_Relic_Room_Cap].Level = 1;
		Instance.m_storedBurdenDict.Clear();
		foreach (KeyValuePair<BurdenType, BurdenObj> item4 in SaveManager.PlayerSaveData.BurdenObjTable)
		{
			Instance.m_storedBurdenDict.Add(item4.Key, item4.Value.CurrentLevel);
			item4.Value.SetLevel(0, additive: false);
		}
		Instance.m_storedNGPlusLevel = SaveManager.PlayerSaveData.NewGamePlusLevel;
		SaveManager.PlayerSaveData.NewGamePlusLevel = 0;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.NGPlusChanged, null, null);
		Instance.m_storedSoulShopDict.Clear();
		foreach (KeyValuePair<SoulShopType, SoulShopObj> item5 in SaveManager.ModeSaveData.SoulShopTable)
		{
			Instance.m_storedSoulShopDict.Add(item5.Key, item5.Value.CurrentEquippedLevel);
			item5.Value.SetEquippedLevel(0, additive: false);
		}
		Instance.m_storedTemporaryMaxHealthMods = SaveManager.PlayerSaveData.TemporaryMaxHealthMods;
		SaveManager.PlayerSaveData.TemporaryMaxHealthMods = 0f;
		Instance.m_storedHeirloomLevelDict.Clear();
		foreach (KeyValuePair<HeirloomType, int> item6 in SaveManager.PlayerSaveData.HeirloomLevelTable)
		{
			Instance.m_storedHeirloomLevelDict.Add(item6.Key, item6.Value);
		}
		_ = ActiveChallenge.ChallengeType;
		_ = 110;
		if (ActiveChallenge.ChallengeType == ChallengeType.PlatformRanger)
		{
			SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.BowWeapon;
		}
		if (ActiveChallenge.ChallengeType == ChallengeType.PlatformBoat)
		{
			SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.FireballSpell;
		}
		if (ActiveChallenge.ChallengeType == ChallengeType.PlatformAxe)
		{
			SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.StraightBoltSpell;
			SaveManager.PlayerSaveData.ResetAllHeirlooms();
			SaveManager.PlayerSaveData.SetHeirloomLevel(HeirloomType.UnlockAirDash, 1, additive: false, broadcast: false);
			SaveManager.PlayerSaveData.SetHeirloomLevel(HeirloomType.UnlockMemory, 1, additive: false, broadcast: false);
		}
		if (ActiveChallenge.ChallengeType == ChallengeType.PlatformKatana)
		{
			SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.LightningSpell;
		}
		if (ActiveChallenge.ChallengeType == ChallengeType.PlatformClimb)
		{
			SaveManager.PlayerSaveData.CurrentCharacter.Spell = AbilityType.GravityWellSpell;
		}
		PlayerController playerController = PlayerManager.GetPlayerController();
		LineageWindowController.CharacterLoadedFromLineage = true;
		playerController.ResetCharacter();
		LineageWindowController.CharacterLoadedFromLineage = false;
	}

	public static void RestoreCharacter(bool updateVisuals)
	{
		foreach (KeyValuePair<SoulShopType, int> item in Instance.m_storedSoulShopDict)
		{
			SaveManager.ModeSaveData.SoulShopTable[item.Key].SetEquippedLevel(item.Value, additive: false);
		}
		SaveManager.PlayerSaveData.CurrentCharacter = Instance.m_storedCharData;
		foreach (KeyValuePair<RuneType, int> item2 in Instance.m_storedRuneDict)
		{
			SaveManager.EquipmentSaveData.RuneDict[item2.Key].EquippedLevel = item2.Value;
		}
		foreach (KeyValuePair<RelicType, int> item3 in Instance.m_storedRelicDict)
		{
			RelicObj relicObj = SaveManager.PlayerSaveData.RelicObjTable[item3.Key];
			relicObj.SetLevel(item3.Value, additive: false, updateVisuals);
			relicObj.SetIntValue(0, additive: false, updateVisuals);
			relicObj.SetFloatValue(0f, additive: false, updateVisuals);
		}
		foreach (KeyValuePair<ClassType, int> item4 in Instance.m_storedMasteryXPTable)
		{
			SaveManager.PlayerSaveData.SetClassXP(item4.Key, item4.Value, additive: false, forceOverride: true, ignoreMaxLevelClamping: true);
		}
		foreach (KeyValuePair<SkillTreeType, int> item5 in Instance.m_storedSkillTreeDict)
		{
			SkillTreeManager.SetSkillObjLevel(item5.Key, item5.Value, additive: false, runEvents: true, ignoreMaxLevelClamping: true);
		}
		foreach (KeyValuePair<BurdenType, int> item6 in Instance.m_storedBurdenDict)
		{
			SaveManager.PlayerSaveData.GetBurden(item6.Key).SetLevel(item6.Value, additive: false);
		}
		SaveManager.PlayerSaveData.NewGamePlusLevel = Instance.m_storedNGPlusLevel;
		Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.NGPlusChanged, null, null);
		SaveManager.PlayerSaveData.TimesRolledRelic = 0;
		SaveManager.PlayerSaveData.TemporaryMaxHealthMods = Instance.m_storedTemporaryMaxHealthMods;
		foreach (KeyValuePair<HeirloomType, int> item7 in Instance.m_storedHeirloomLevelDict)
		{
			SaveManager.PlayerSaveData.SetHeirloomLevel(item7.Key, item7.Value, additive: false, broadcast: true);
		}
		if (updateVisuals)
		{
			Instance.m_traitChangedArgs.Initialize(SaveManager.PlayerSaveData.CurrentCharacter.TraitOne, SaveManager.PlayerSaveData.CurrentCharacter.TraitTwo);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.TraitsChanged, Instance, Instance.m_traitChangedArgs);
			LineageWindowController.CharacterLoadedFromLineage = true;
			PlayerController playerController = PlayerManager.GetPlayerController();
			playerController.ResetCharacter();
			playerController.CharacterHitResponse.BlinkPulseEffect.StopInvincibilityEffect();
			LineageWindowController.CharacterLoadedFromLineage = false;
		}
	}

	public static float ApplyStatCap(float actualStat, bool isDexterityOrFocus = false)
	{
		float num = ActiveChallenge.ChallengeData.BaseHandicap;
		if (!isDexterityOrFocus)
		{
			num += 15f;
		}
		return num;
	}

	private void OnStartGlobalTimer(MonoBehaviour sender, EventArgs args)
	{
		if (IsInChallenge)
		{
			if (ActiveChallenge.ChallengeData.ScoringType == ChallengeScoringType.Platform)
			{
				GlobalTimerHUDController.ElapsedTime = (float)ActiveChallenge.EquippedLevel * (0f - ActiveChallenge.ChallengeData.ScalingHandicap);
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerHit, m_onPlayerHitIncreaseTimer);
			}
			PlayerManager.GetPlayerController().TakesNoDamage = false;
		}
	}

	private void OnStopGlobalTimer(MonoBehaviour sender, EventArgs args)
	{
		if (IsInChallenge)
		{
			PlayerManager.GetPlayerController().TakesNoDamage = true;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerHit, m_onPlayerHitIncreaseTimer);
		}
	}

	private void OnPlayerHitIncreaseTimer(object sender, EventArgs args)
	{
		GlobalTimerHUDController.ElapsedTime += 0f;
	}

	public static void UpdateChallengeScore(ChallengeType challengeType, ClassType classType, int newScore, float newTime, bool usingHandicaps)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		if (!challenge.ClassHighScores.ContainsKey(classType))
		{
			challenge.ClassHighScores.Add(classType, 0);
			NeedsSave = true;
		}
		if (newScore > challenge.ClassHighScores[classType])
		{
			challenge.ClassHighScores[classType] = newScore;
			NeedsSave = true;
		}
		int num = 0;
		foreach (KeyValuePair<ClassType, int> classHighScore in challenge.ClassHighScores)
		{
			num += classHighScore.Value;
		}
		challenge.TotalHighScore = num;
		if (newTime < challenge.BestTime)
		{
			challenge.BestTime = newTime;
			NeedsSave = true;
		}
		if (!usingHandicaps && newTime < challenge.BestTimeWithoutHandicaps)
		{
			challenge.BestTimeWithoutHandicaps = newTime;
			NeedsSave = true;
		}
	}

	public static ChallengeTrophyRank GetChallengeTrophyRank(ChallengeType challengeType, bool checkForSisyphusTrophy = true)
	{
		ChallengeObj challenge = GetChallenge(challengeType);
		if (challenge != null)
		{
			if (challengeType == ChallengeType.TutorialPurified)
			{
				int num = GetTotalTrophyCount() - 1;
				int totalTrophiesEarnedOfRankOrHigher = GetTotalTrophiesEarnedOfRankOrHigher(ChallengeTrophyRank.Bronze, includeTutorialPurified: false);
				int totalTrophiesEarnedOfRankOrHigher2 = GetTotalTrophiesEarnedOfRankOrHigher(ChallengeTrophyRank.Silver, includeTutorialPurified: false);
				if (GetTotalTrophiesEarnedOfRankOrHigher(ChallengeTrophyRank.Gold, includeTutorialPurified: false) >= num && (SaveManager.ModeSaveData.HasGoldSisyphusTrophy || !checkForSisyphusTrophy))
				{
					return ChallengeTrophyRank.Gold;
				}
				if (totalTrophiesEarnedOfRankOrHigher2 >= num && (SaveManager.ModeSaveData.HasSilverSisyphusTrophy || !checkForSisyphusTrophy))
				{
					return ChallengeTrophyRank.Silver;
				}
				if (totalTrophiesEarnedOfRankOrHigher >= num && (SaveManager.ModeSaveData.HasBronzeSisyphusTrophy || !checkForSisyphusTrophy))
				{
					return ChallengeTrophyRank.Bronze;
				}
				return ChallengeTrophyRank.None;
			}
			ChallengeData challengeData = challenge.ChallengeData;
			if ((bool)challengeData)
			{
				if (challengeData.Disabled)
				{
					return ChallengeTrophyRank.None;
				}
				if (challengeData.ScoringType == ChallengeScoringType.Battle)
				{
					int totalHighScore = challenge.TotalHighScore;
					if (totalHighScore >= challengeData.GoldReq)
					{
						return ChallengeTrophyRank.Gold;
					}
					if (totalHighScore >= challengeData.SilverReq)
					{
						return ChallengeTrophyRank.Silver;
					}
					if (totalHighScore >= challengeData.BronzeReq)
					{
						return ChallengeTrophyRank.Bronze;
					}
					return ChallengeTrophyRank.None;
				}
				if (challengeData.ScoringType == ChallengeScoringType.Platform)
				{
					float num2 = Mathf.Max(0f, challenge.BestTime - challengeData.ParTime);
					if (num2 <= 0f)
					{
						return ChallengeTrophyRank.Gold;
					}
					if (num2 <= 3f)
					{
						return ChallengeTrophyRank.Silver;
					}
					if (num2 <= 10f)
					{
						return ChallengeTrophyRank.Bronze;
					}
					return ChallengeTrophyRank.None;
				}
			}
		}
		return ChallengeTrophyRank.None;
	}

	public static float CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType statType)
	{
		switch (statType)
		{
		case ChallengeCompleteStatsEntryType.BaseScore:
			return 2500f;
		case ChallengeCompleteStatsEntryType.HitsTaken:
			return Mathf.Max(7500 - HitsTaken * 500, 0);
		case ChallengeCompleteStatsEntryType.Resolve:
			return Mathf.Max(7500 - Mathf.RoundToInt(SaveManager.PlayerSaveData.GetTotalRelicResolveCost() * 100f) * 50, 0);
		case ChallengeCompleteStatsEntryType.Timer:
		{
			int num = Mathf.Max(Mathf.RoundToInt(GlobalTimerHUDController.ElapsedTime - ActiveChallenge.ChallengeData.ParTime), 0);
			return Mathf.Max(7500f - (float)num * 10f * 10f, 0f);
		}
		case ChallengeCompleteStatsEntryType.HandicapMod:
			return Mathf.Max(2f - (float)ActiveChallenge.EquippedLevel * 0.1f, 0.1f);
		case ChallengeCompleteStatsEntryType.FinalScore:
			return Mathf.RoundToInt((0f + CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.BaseScore) + CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.HitsTaken) + CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.Timer) + CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.Resolve)) * CalculateActiveChallengeScore(ChallengeCompleteStatsEntryType.HandicapMod));
		default:
			return 0f;
		}
	}

	public static string GetChallengeLetterGrade(ChallengeType challengeType, float score)
	{
		int num = m_letterGradeArray.Length;
		int num2 = Mathf.Clamp((int)(score / 50000f * (float)num), 0, num - 1);
		return m_letterGradeArray[num2];
	}

	public static int GetChallengeClassHighScore(ChallengeType challengeType, ClassType classType)
	{
		if (GetChallenge(challengeType).ClassHighScores.TryGetValue(classType, out var value))
		{
			return value;
		}
		return 0;
	}

	public static int GetTotalTrophyCount()
	{
		return Challenge_EV.CHALLENGE_ORDER.Length;
	}
}
