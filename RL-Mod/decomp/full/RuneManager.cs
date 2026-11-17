using System;
using System.Collections.Generic;
using UnityEngine;

public class RuneManager : MonoBehaviour
{
	private static bool m_isDisposed;

	private static bool m_isInitialized;

	private RuneFoundLevelChangeEventArgs m_onRuneBlueprintsFoundChangedArgs;

	private RuneEquippedLevelChangeEventArgs m_onRuneEquippedLevelChangeArgs;

	private RunePurchaseLevelChangeEventArgs m_onRunePurchaseLevelChangeArgs;

	private RuneFoundStateChangeEventArgs m_onRuneFoundStateChangeArgs;

	private static RuneManager m_runeManager = null;

	private static List<RuneType> m_runeTypeListHelper = new List<RuneType>();

	public static RuneManager Instance
	{
		get
		{
			_ = m_isDisposed;
			_ = m_isInitialized;
			if (m_runeManager == null)
			{
				m_runeManager = CDGHelper.FindStaticInstance<RuneManager>(createNewIfNull: false);
			}
			return m_runeManager;
		}
	}

	public static bool IsInitialized => m_isInitialized;

	public static bool IsDisposed => m_isDisposed;

	private void Awake()
	{
		Initialize();
	}

	private void Initialize()
	{
		m_onRuneBlueprintsFoundChangedArgs = new RuneFoundLevelChangeEventArgs(RuneType.None, 0);
		m_onRuneEquippedLevelChangeArgs = new RuneEquippedLevelChangeEventArgs(RuneType.None, 0);
		m_onRunePurchaseLevelChangeArgs = new RunePurchaseLevelChangeEventArgs(RuneType.None, 0);
		m_onRuneFoundStateChangeArgs = new RuneFoundStateChangeEventArgs(RuneType.None, FoundState.NotFound);
		if (!SaveManager.EquipmentSaveData.IsInitialized)
		{
			SaveManager.EquipmentSaveData.Initialize();
		}
		Array.Sort(RuneType_RL.TypeArray, (RuneType a, RuneType b) => (a != 0 && b != 0) ? RuneLibrary.GetRuneData(a).Location.CompareTo(RuneLibrary.GetRuneData(b).Location) : 0);
		m_isInitialized = true;
	}

	private void OnDestroy()
	{
		m_isDisposed = true;
	}

	public static bool DoesRuneExist(RuneType runeType)
	{
		return GetRune(runeType) != null;
	}

	public static bool CanAfford(RuneType runeType)
	{
		RuneObj rune = GetRune(runeType);
		if (SaveManager.PlayerSaveData.GoldCollectedIncludingBank >= rune.GoldCostToUpgrade && SaveManager.PlayerSaveData.RuneOreCollected >= rune.OreCostToUpgrade)
		{
			return true;
		}
		return false;
	}

	public static bool CanPurchaseRune(RuneType runeType, bool suppressLogs = true)
	{
		if (!DoesRuneExist(runeType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>RuneManager.CanPurchaseRune({0}) returned false because rune obj could not be found.</color>", runeType);
			}
			return false;
		}
		FoundState foundState = GetFoundState(runeType);
		if (foundState <= FoundState.NotFound)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>RuneManager.CanPurchaseRune({0}) returned false because rune found state is 'Not Found'.</color>", runeType);
			}
			return false;
		}
		if (foundState == FoundState.Purchased)
		{
			int upgradeLevel = GetUpgradeLevel(runeType);
			int upgradeBlueprintsFound = GetUpgradeBlueprintsFound(runeType);
			if (upgradeLevel >= upgradeBlueprintsFound)
			{
				if (!suppressLogs)
				{
					Debug.LogFormat("<color=yellow>RuneManager.CanPurchaseRune({0}) returned false because Blueprints Found level is not high enough.</color>", runeType);
				}
				return false;
			}
		}
		if (!CanAfford(runeType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>RuneManager.CanPurchaseRune({0}) returned false because rune could not be afforded.</color>", runeType);
			}
			return false;
		}
		return true;
	}

	public static bool CanEquip(RuneType runeType, bool suppressLogs = true)
	{
		if (!DoesRuneExist(runeType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>RuneManager.CanEquip({0}) returned false because rune obj could not be found.</color>", runeType);
			}
			return false;
		}
		if (GetFoundState(runeType) != 0)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>RuneManager.CanEquip({0}) returned false because rune has not been purchased yet.</color>", runeType);
			}
			return false;
		}
		int runeEquippedLevel = GetRuneEquippedLevel(runeType);
		int upgradeLevel = GetUpgradeLevel(runeType);
		if (runeEquippedLevel >= upgradeLevel)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>RuneManager.CanEquip({0}) returned false because player has already equipped all runes they own of this type.</color>", runeType);
			}
			return false;
		}
		RuneObj rune = GetRune(runeType);
		int totalEquippedWeight = GetTotalEquippedWeight();
		int num = 10000;
		if (PlayerManager.IsInstantiated)
		{
			num = PlayerManager.GetPlayerController().ActualRuneWeight;
		}
		int num2 = rune.GetWeightAtLevel(rune.EquippedLevel + 1) - rune.GetWeightAtLevel(rune.EquippedLevel);
		if (totalEquippedWeight + num2 > num)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>RuneManager.CanEquip({0}) returned false because resulting Rune Weight exceeds allowed Rune Weight.</color>", runeType);
			}
			return false;
		}
		return true;
	}

	public static List<RuneType> GetAllRunesWithFoundState(FoundState foundState)
	{
		GetAllRunesWithFoundState(foundState, m_runeTypeListHelper);
		return m_runeTypeListHelper;
	}

	public static void GetAllRunesWithFoundState(FoundState foundState, List<RuneType> results)
	{
		results.Clear();
		foreach (KeyValuePair<RuneType, RuneObj> item in SaveManager.EquipmentSaveData.RuneDict)
		{
			RuneObj value = item.Value;
			if (value.FoundState == foundState)
			{
				results.Add(value.RuneType);
			}
		}
	}

	public static int GetTotalEquippedWeight()
	{
		int num = 0;
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType != 0)
			{
				RuneObj rune = GetRune(runeType);
				if (rune != null && rune.EquippedLevel > 0)
				{
					num += rune.CurrentWeight;
				}
			}
		}
		return num;
	}

	public static FoundState GetFoundState(RuneType runeType)
	{
		RuneObj rune = GetRune(runeType);
		FoundState foundState = FoundState.NotFound;
		return rune?.FoundState ?? FoundState.NotFound;
	}

	public static bool SetFoundState(RuneType runeType, FoundState foundState, bool overrideValues, bool runEvents = true)
	{
		RuneObj rune = GetRune(runeType);
		if (rune != null)
		{
			FoundState foundState2 = GetFoundState(runeType);
			if (foundState > foundState2 || overrideValues)
			{
				rune.UpgradeLevel = (int)foundState;
				if (runEvents)
				{
					Instance.m_onRuneFoundStateChangeArgs.Initialize(runeType, foundState);
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.RuneFoundStateChanged, Instance, Instance.m_onRuneFoundStateChangeArgs);
				}
				return true;
			}
			Debug.LogFormat("<color=red>Could not set rune {0} to FoundState: {1}. Current FoundState: {2} is a higher value. Please set overrideValues = true to override this if intended.</color>", runeType, foundState, GetFoundState(runeType));
			return false;
		}
		Debug.LogFormat("<color=red>Could not set rune {0} to FoundState: {1}. Rune obj not found.</color>", runeType, foundState);
		return false;
	}

	public static int GetUpgradeBlueprintsFound(RuneType runeType, bool ignoreInfinitePurchasePower = false)
	{
		RuneObj rune = GetRune(runeType);
		int upgradeBlueprintsFound = rune.UpgradeBlueprintsFound;
		if (rune != null)
		{
			return Mathf.Clamp(upgradeBlueprintsFound, 0, rune.MaxLevel);
		}
		return 0;
	}

	public static bool SetUpgradeBlueprintsFound(RuneType runeType, int level, bool additive, bool runEvents = true)
	{
		RuneObj rune = GetRune(runeType);
		if (rune != null)
		{
			int upgradeBlueprintsFound = rune.UpgradeBlueprintsFound;
			upgradeBlueprintsFound = ((!additive) ? level : (upgradeBlueprintsFound + level));
			if (upgradeBlueprintsFound > rune.MaxLevel)
			{
				Debug.Log("<color=red>RuneManager.SetUpgradeBlueprintsFound(" + runeType.ToString() + ") failed.  Upgrading will put blueprints found beyond max level.</color>");
				return false;
			}
			if (upgradeBlueprintsFound > 0 && GetFoundState(runeType) < FoundState.FoundButNotViewed)
			{
				SetFoundState(runeType, FoundState.FoundButNotViewed, overrideValues: false);
			}
			rune.UpgradeBlueprintsFound = Mathf.Clamp(upgradeBlueprintsFound, 0, rune.MaxLevel);
			if (runEvents)
			{
				Instance.m_onRuneBlueprintsFoundChangedArgs.Initialize(runeType, rune.UpgradeBlueprintsFound);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.RuneBlueprintsFoundChanged, Instance, Instance.m_onRuneBlueprintsFoundChangedArgs);
			}
			return true;
		}
		return false;
	}

	public static int GetUpgradeLevel(RuneType runeType)
	{
		return GetRune(runeType)?.ClampedUpgradeLevel ?? 0;
	}

	public static bool SetRuneUpgradeLevel(RuneType runeType, int level, bool additive, bool overrideValues, bool runEvents = true)
	{
		RuneObj rune = GetRune(runeType);
		if (rune != null)
		{
			int upgradeLevel = GetUpgradeLevel(runeType);
			upgradeLevel = ((!additive) ? level : (upgradeLevel + level));
			if (GetFoundState(runeType) != 0 && !overrideValues)
			{
				Debug.LogFormat("<color=red>RuneManager.SetRuneUpgradeLevel({0}) failed. First blueprint not purchased yet. Use overrideValues = true if intended or set Found State to Purchased via RuneManager.SetFoundState().</color>", runeType);
				return false;
			}
			if (upgradeLevel > GetUpgradeBlueprintsFound(runeType) && !overrideValues)
			{
				Debug.LogFormat("<color=red>RuneManager.SetRuneUpgradeLevel({0}) failed. Blueprint Found Level is less than desired Upgrade Level. Use overrideValues = true if intended.</color>", runeType);
				return false;
			}
			if (upgradeLevel < GetUpgradeLevel(runeType) && !overrideValues)
			{
				Debug.LogFormat("<color=red>RuneManager.SetRuneUpgradeLevel({0}) failed. New Upgrade Level less than current Upgrade Level. Use overrideValues = true if intended.</color>", runeType);
				return false;
			}
			rune.UpgradeLevel = Mathf.Clamp(upgradeLevel, 0, GetUpgradeBlueprintsFound(runeType));
			if (runEvents)
			{
				Instance.m_onRunePurchaseLevelChangeArgs.Initialize(runeType, rune.UpgradeLevel);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.RunePurchaseLevelChanged, Instance, Instance.m_onRunePurchaseLevelChangeArgs);
			}
			return true;
		}
		Debug.LogFormat("<color=red>RuneManager.SetRuneUpgradeLevel({0}) failed. Rune obj not found.</color>", runeType);
		return false;
	}

	public static int GetRuneEquippedLevel(RuneType runeType)
	{
		RuneObj rune = GetRune(runeType);
		if (rune != null)
		{
			return Mathf.Clamp(rune.EquippedLevel, 0, rune.ClampedUpgradeLevel);
		}
		return 0;
	}

	public static bool SetRuneEquippedLevel(RuneType runeType, int level, bool additive, bool runEvents = true)
	{
		RuneObj rune = GetRune(runeType);
		if (rune != null)
		{
			int equippedLevel = rune.EquippedLevel;
			equippedLevel = ((!additive) ? level : (equippedLevel + level));
			equippedLevel = Mathf.Clamp(equippedLevel, 0, rune.ClampedUpgradeLevel);
			if (equippedLevel != rune.EquippedLevel)
			{
				rune.EquippedLevel = equippedLevel;
				if (runEvents)
				{
					Instance.m_onRuneEquippedLevelChangeArgs.Initialize(runeType, rune.EquippedLevel);
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.RuneEquippedLevelChanged, Instance, Instance.m_onRuneEquippedLevelChangeArgs);
				}
				return true;
			}
			return false;
		}
		return false;
	}

	public static float GetMinRuneLevelScale()
	{
		int num = 0;
		foreach (KeyValuePair<RuneType, RuneObj> item in SaveManager.EquipmentSaveData.RuneDict)
		{
			num += GetUpgradeBlueprintsFound(item.Key);
		}
		num -= 2;
		return (float)num * 1.25f;
	}

	public static int GetTotalAvailableRuneBlueprints(List<RuneObj> populatedList = null)
	{
		populatedList?.Clear();
		int num = 0;
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType != 0)
			{
				RuneObj rune = GetRune(runeType);
				if (rune != null && !rune.RuneData.Disabled)
				{
					num++;
					populatedList?.Add(rune);
				}
			}
		}
		return num;
	}

	public static RuneObj GetRune(RuneType runeType)
	{
		if (SaveManager.EquipmentSaveData.RuneDict.TryGetValue(runeType, out var value))
		{
			return value;
		}
		return null;
	}
}
