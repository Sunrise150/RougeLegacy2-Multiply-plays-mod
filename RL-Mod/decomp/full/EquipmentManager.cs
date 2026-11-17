using System.Collections.Generic;
using UnityEngine;

public class EquipmentManager : MonoBehaviour
{
	private const string EQUIPMENT_MANAGER = "EquipmentManager";

	private static bool m_isDisposed;

	private static bool m_isInitialized;

	private EquipmentPurchasedLevelChangeEventArgs m_purchasedLevelChangeArgs;

	private EquipmentFoundLevelChangeEventArgs m_foundLevelChangeArgs;

	private EquipmentFoundStateChangeEventArgs m_onFoundStateChangeArgs;

	private EquippedChangeEventArgs m_onEquippedChangeArgs;

	private static EquipmentManager m_equipmentManager = null;

	private static List<EquipmentType> m_equipTypesUsedHelper = new List<EquipmentType>();

	private static EquipmentManager Instance
	{
		get
		{
			_ = m_isDisposed;
			_ = m_isInitialized;
			if (m_equipmentManager == null)
			{
				m_equipmentManager = CDGHelper.FindStaticInstance<EquipmentManager>(createNewIfNull: false);
			}
			return m_equipmentManager;
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
		if (Application.isPlaying)
		{
			m_foundLevelChangeArgs = new EquipmentFoundLevelChangeEventArgs(EquipmentCategoryType.None, EquipmentType.None, 0);
			m_purchasedLevelChangeArgs = new EquipmentPurchasedLevelChangeEventArgs(EquipmentCategoryType.None, EquipmentType.None, 0);
			m_onFoundStateChangeArgs = new EquipmentFoundStateChangeEventArgs(EquipmentCategoryType.None, EquipmentType.None, FoundState.NotFound);
			m_onEquippedChangeArgs = new EquippedChangeEventArgs(EquipmentCategoryType.None, EquipmentType.None);
			if (!SaveManager.EquipmentSaveData.IsInitialized)
			{
				SaveManager.EquipmentSaveData.Initialize();
			}
			m_isInitialized = true;
		}
	}

	private void OnDestroy()
	{
		m_isDisposed = true;
	}

	public static bool DoesEquipmentExist(EquipmentCategoryType category, EquipmentType equipType)
	{
		return GetEquipment(category, equipType) != null;
	}

	public static List<EquipmentType> GetAllEquipmentWithFoundState(EquipmentCategoryType category, FoundState foundState)
	{
		List<EquipmentType> list = new List<EquipmentType>();
		GetAllEquipmentWithFoundState(category, foundState, list);
		return list;
	}

	public static void GetAllEquipmentWithFoundState(EquipmentCategoryType category, FoundState foundState, List<EquipmentType> results)
	{
		results.Clear();
		foreach (KeyValuePair<EquipmentType, EquipmentObj> item in GetEquipmentDict(category))
		{
			EquipmentObj value = item.Value;
			if (value.FoundState == foundState)
			{
				results.Add(value.EquipmentType);
			}
		}
	}

	public static bool CanAfford(EquipmentCategoryType category, EquipmentType equipType)
	{
		EquipmentObj equipment = GetEquipment(category, equipType);
		if (SaveManager.PlayerSaveData.GoldCollectedIncludingBank >= equipment.GoldCostToUpgrade && SaveManager.PlayerSaveData.EquipmentOreCollected >= equipment.OreCostToUpgrade)
		{
			return true;
		}
		return false;
	}

	public static bool CanPurchaseEquipment(EquipmentCategoryType category, EquipmentType equipType, bool suppressLogs = true)
	{
		if (!DoesEquipmentExist(category, equipType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>EquipmentManager.CanPurchaseEquipment({0}, {1}) returned false because equipment obj could not be found.</color>", category, equipType);
			}
			return false;
		}
		FoundState foundState = GetFoundState(category, equipType);
		if (foundState <= FoundState.NotFound)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>EquipmentManager.CanPurchaseEquipment({0}, {1}) returned false because equipment found state is 'Not Found'.</color>", category, equipType);
			}
			return false;
		}
		if (foundState == FoundState.Purchased)
		{
			int upgradeLevel = GetUpgradeLevel(category, equipType);
			int upgradeBlueprintsFound = GetUpgradeBlueprintsFound(category, equipType);
			if (upgradeLevel >= upgradeBlueprintsFound)
			{
				if (!suppressLogs)
				{
					Debug.LogFormat("<color=yellow>EquipmentManager.CanPurchaseEquipment({0}, {1}) returned false because Blueprints Found level is not high enough.</color>", category, equipType);
				}
				return false;
			}
		}
		if (!CanAfford(category, equipType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>EquipmentManager.CanPurchaseEquipment({0}, {1}) returned false because equipment could not be afforded.</color>", category, equipType);
			}
			return false;
		}
		return true;
	}

	public static bool CanEquip(EquipmentCategoryType category, EquipmentType equipType, bool suppressLogs = true)
	{
		if (!DoesEquipmentExist(category, equipType))
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>EquipmentManager.CanEquip({0}, {1}) returned false because equipment obj could not be found.</color>", category, equipType);
			}
			return false;
		}
		if (GetFoundState(category, equipType) != 0)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>EquipmentManager.CanEquip({0}, {1}) returned false because equipment has not been purchased yet.</color>", category, equipType);
			}
			return false;
		}
		EquipmentObj equipment = GetEquipment(category, equipType);
		int num = (int)GetTotalEquippedStatValue(EquipmentStatType.Weight);
		EquipmentObj equipped = GetEquipped(category);
		if (equipped != null)
		{
			num -= (int)equipped.GetCurrentStatValue(EquipmentStatType.Weight);
			num = Mathf.Clamp(num, 0, int.MaxValue);
		}
		int num2 = 10000;
		if (PlayerManager.IsInstantiated)
		{
			num2 = PlayerManager.GetPlayerController().ActualAllowedEquipmentWeight;
		}
		if (num + (int)equipment.GetCurrentStatValue(EquipmentStatType.Weight) > num2)
		{
			if (!suppressLogs)
			{
				Debug.LogFormat("<color=yellow>EquipmentManager.CanEquip({0}, {1}) returned false because resulting Equip Weight exceeds allowed Equip Weight.</color>", category, equipType);
			}
			return false;
		}
		return true;
	}

	public static int Get_EquipmentSet_TotalEquippedLevel(EquipmentType equipType)
	{
		int num = 0;
		EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
		{
			if (equipmentCategoryType != 0)
			{
				EquipmentObj equipped = GetEquipped(equipmentCategoryType);
				if (equipped != null && equipped.EquipmentType == equipType)
				{
					num += equipped.EquipmentSetLevel;
				}
			}
		}
		return num;
	}

	public static float Get_EquipmentSet_BonusTypeStatGain(EquipmentSetBonusType bonusType)
	{
		m_equipTypesUsedHelper.Clear();
		float num = 0f;
		EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
		{
			if (equipmentCategoryType == EquipmentCategoryType.None)
			{
				continue;
			}
			EquipmentObj equipped = GetEquipped(equipmentCategoryType);
			if (equipped == null)
			{
				continue;
			}
			EquipmentType equipmentType = equipped.EquipmentType;
			if (!m_equipTypesUsedHelper.Contains(equipmentType))
			{
				m_equipTypesUsedHelper.Add(equipmentType);
				EquipmentSetData equipmentSetData = EquipmentSetLibrary.GetEquipmentSetData(equipmentType);
				int num2 = Get_EquipmentSet_TotalEquippedLevel(equipmentType);
				if (num2 >= equipmentSetData.SetRequirement01 && equipmentSetData.SetBonus01.BonusType == bonusType)
				{
					num += equipmentSetData.SetBonus01.StatGain;
				}
				if (num2 >= equipmentSetData.SetRequirement02 && equipmentSetData.SetBonus02.BonusType == bonusType)
				{
					num += equipmentSetData.SetBonus02.StatGain;
				}
				if (num2 >= equipmentSetData.SetRequirement03 && equipmentSetData.SetBonus03.BonusType == bonusType)
				{
					num += equipmentSetData.SetBonus03.StatGain;
				}
			}
		}
		return num;
	}

	public static int Get_EquipmentSet_CurrentLevel(EquipmentType equipType)
	{
		if (!SaveManager.EquipmentSaveData.EquipmentSetStateDict.ContainsKey(equipType))
		{
			return 0;
		}
		return SaveManager.EquipmentSaveData.EquipmentSetStateDict[equipType].x;
	}

	public static void Set_EquipmentSet_CurrentLevel(EquipmentType equipType, int level, bool additive)
	{
		if (SaveManager.EquipmentSaveData.EquipmentSetStateDict.ContainsKey(equipType))
		{
			int num = Get_EquipmentSet_TotalEquippedLevel(equipType);
			int x = (additive ? (num + level) : level);
			SerializableVector2Int value = SaveManager.EquipmentSaveData.EquipmentSetStateDict[equipType];
			value.x = x;
			SaveManager.EquipmentSaveData.EquipmentSetStateDict[equipType] = value;
		}
	}

	public static int Get_EquipmentSet_LevelViewed(EquipmentType equipType)
	{
		if (!SaveManager.EquipmentSaveData.EquipmentSetStateDict.ContainsKey(equipType))
		{
			return 0;
		}
		return SaveManager.EquipmentSaveData.EquipmentSetStateDict[equipType].y;
	}

	public static void Set_EquipmentSet_LevelViewed(EquipmentType equipType, int level, bool additive)
	{
		if (SaveManager.EquipmentSaveData.EquipmentSetStateDict.ContainsKey(equipType))
		{
			int num = Get_EquipmentSet_TotalEquippedLevel(equipType);
			int y = (additive ? (num + level) : level);
			SerializableVector2Int value = SaveManager.EquipmentSaveData.EquipmentSetStateDict[equipType];
			value.y = y;
			SaveManager.EquipmentSaveData.EquipmentSetStateDict[equipType] = value;
		}
	}

	public static int GetUpgradeBlueprintsFound(EquipmentCategoryType category, EquipmentType equipType, bool ignoreInfinitePurchasePower = false)
	{
		EquipmentObj equipment = GetEquipment(category, equipType);
		if (equipment != null)
		{
			return Mathf.Clamp(equipment.UpgradeBlueprintsFound, 0, equipment.MaxLevel);
		}
		return 0;
	}

	public static bool SetUpgradeBlueprintsFound(EquipmentCategoryType category, EquipmentType equipType, int level, bool additive, bool runEvents = true)
	{
		EquipmentObj equipment = GetEquipment(category, equipType);
		if (equipment != null)
		{
			int upgradeBlueprintsFound = equipment.UpgradeBlueprintsFound;
			upgradeBlueprintsFound = ((!additive) ? level : (upgradeBlueprintsFound + level));
			if (upgradeBlueprintsFound > equipment.MaxLevel)
			{
				Debug.Log("<color=red>EquipmentManager.SetUpgradeBlueprintsFound(" + category.ToString() + ", " + equipType.ToString() + ") failed.  Upgrading will put blueprints found beyond max level.</color>");
				return false;
			}
			equipment.UpgradeBlueprintsFound = Mathf.Clamp(upgradeBlueprintsFound, 0, equipment.MaxLevel);
			if (equipment.FoundState == FoundState.NotFound && equipment.UpgradeBlueprintsFound > 0)
			{
				SetFoundState(category, equipType, FoundState.FoundButNotViewed, overrideValues: false, runEvents);
			}
			if (runEvents)
			{
				Instance.m_foundLevelChangeArgs.Initialize(category, equipType, equipment.UpgradeBlueprintsFound);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.EquipmentFoundLevelChanged, Instance, Instance.m_foundLevelChangeArgs);
			}
			return true;
		}
		return false;
	}

	public static int GetUpgradeLevel(EquipmentCategoryType category, EquipmentType equipType)
	{
		return GetEquipment(category, equipType)?.ClampedUpgradeLevel ?? 0;
	}

	public static bool SetEquipmentUpgradeLevel(EquipmentCategoryType category, EquipmentType equipType, int level, bool additive, bool overrideValues, bool runEvents = true)
	{
		EquipmentObj equipment = GetEquipment(category, equipType);
		if (equipment != null)
		{
			int upgradeLevel = GetUpgradeLevel(category, equipType);
			upgradeLevel = ((!additive) ? level : (upgradeLevel + level));
			if (GetFoundState(category, equipType) != 0 && !overrideValues)
			{
				Debug.LogFormat("<color=red>EquipmentManager.SetEquipmentUpgradeLevel({0}, {1}) failed. First blueprint not purchased yet. Use overrideValues = true if intended or set Found State to Purchased via EquipmentManager.SetFoundState().</color>", category, equipType);
				return false;
			}
			if (upgradeLevel > GetUpgradeBlueprintsFound(category, equipType) && !overrideValues)
			{
				Debug.LogFormat("<color=red>EquipmentManager.SetEquipmentUpgradeLevel({0}, {1}) failed. Blueprint Found Level is less than desired Upgrade Level. Use overrideValues = true if intended.</color>", category, equipType);
				return false;
			}
			if (upgradeLevel < GetUpgradeLevel(category, equipType) && !overrideValues)
			{
				Debug.LogFormat("<color=red>EquipmentManager.SetEquipmentUpgradeLevel({0}, {1}) failed. New Upgrade Level less than current Upgrade Level. Use overrideValues = true if intended.</color>", category, equipType);
				return false;
			}
			equipment.UpgradeLevel = Mathf.Clamp(upgradeLevel, 0, GetUpgradeBlueprintsFound(category, equipType));
			if (runEvents)
			{
				Instance.m_purchasedLevelChangeArgs.Initialize(category, equipType, equipment.UpgradeLevel);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.EquipmentPurchasedLevelChanged, Instance, Instance.m_purchasedLevelChangeArgs);
			}
			return true;
		}
		Debug.LogFormat("<color=red>EquipmentManager.SetEquipmentUpgradeLevel({0}, {1}) failed. Equipment obj not found.</color>", category, equipType);
		return false;
	}

	public static FoundState GetFoundState(EquipmentCategoryType category, EquipmentType equipType)
	{
		EquipmentObj equipment = GetEquipment(category, equipType);
		FoundState foundState = FoundState.NotFound;
		foundState = equipment?.FoundState ?? FoundState.NotFound;
		if (foundState < FoundState.NotFound)
		{
			foundState = FoundState.NotFound;
		}
		return foundState;
	}

	public static bool SetFoundState(EquipmentCategoryType category, EquipmentType equipType, FoundState foundState, bool overrideValues, bool runEvents = true)
	{
		EquipmentObj equipment = GetEquipment(category, equipType);
		if (equipment != null)
		{
			FoundState foundState2 = GetFoundState(category, equipType);
			if (foundState > foundState2 || overrideValues)
			{
				equipment.UpgradeLevel = (int)foundState;
				if (runEvents)
				{
					Instance.m_onFoundStateChangeArgs.Initialize(category, equipType, foundState);
					Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.EquipmentFoundStateChanged, Instance, Instance.m_onFoundStateChangeArgs);
				}
				return true;
			}
			Debug.LogFormat("<color=red>Could not set equipment {0} - {1} to FoundState: {2}. Current FoundState: {3} is a higher value. Please set overrideValues = true to override this if intended.</color>", category, equipType, foundState, GetFoundState(category, equipType));
			return false;
		}
		Debug.LogFormat("<color=red>Could not set equipment {0} - {1} to FoundState: {2}. Equipment obj not found.</color>", category, equipType, foundState);
		return false;
	}

	public static bool IsEquipped(EquipmentCategoryType category, EquipmentType equipType)
	{
		EquipmentObj equipped = GetEquipped(category);
		if (equipped != null)
		{
			return equipped.EquipmentType == equipType;
		}
		return false;
	}

	public static EquipmentObj GetEquipped(EquipmentCategoryType category)
	{
		return category switch
		{
			EquipmentCategoryType.Weapon => GetEquipment(category, SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType), 
			EquipmentCategoryType.Head => GetEquipment(category, SaveManager.PlayerSaveData.CurrentCharacter.HeadEquipmentType), 
			EquipmentCategoryType.Chest => GetEquipment(category, SaveManager.PlayerSaveData.CurrentCharacter.ChestEquipmentType), 
			EquipmentCategoryType.Cape => GetEquipment(category, SaveManager.PlayerSaveData.CurrentCharacter.CapeEquipmentType), 
			EquipmentCategoryType.Trinket => GetEquipment(category, SaveManager.PlayerSaveData.CurrentCharacter.TrinketEquipmentType), 
			_ => null, 
		};
	}

	public static bool SetEquipped(EquipmentCategoryType category, EquipmentType equipmentToEquip, bool runEvents = true)
	{
		bool flag = false;
		switch (category)
		{
		case EquipmentCategoryType.Weapon:
			_ = SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType;
			SaveManager.PlayerSaveData.CurrentCharacter.EdgeEquipmentType = equipmentToEquip;
			flag = true;
			break;
		case EquipmentCategoryType.Head:
			_ = SaveManager.PlayerSaveData.CurrentCharacter.HeadEquipmentType;
			SaveManager.PlayerSaveData.CurrentCharacter.HeadEquipmentType = equipmentToEquip;
			flag = true;
			break;
		case EquipmentCategoryType.Chest:
			_ = SaveManager.PlayerSaveData.CurrentCharacter.ChestEquipmentType;
			SaveManager.PlayerSaveData.CurrentCharacter.ChestEquipmentType = equipmentToEquip;
			flag = true;
			break;
		case EquipmentCategoryType.Cape:
			_ = SaveManager.PlayerSaveData.CurrentCharacter.CapeEquipmentType;
			SaveManager.PlayerSaveData.CurrentCharacter.CapeEquipmentType = equipmentToEquip;
			flag = true;
			break;
		case EquipmentCategoryType.Trinket:
			_ = SaveManager.PlayerSaveData.CurrentCharacter.TrinketEquipmentType;
			SaveManager.PlayerSaveData.CurrentCharacter.TrinketEquipmentType = equipmentToEquip;
			flag = true;
			break;
		}
		if (flag)
		{
			int num = Get_EquipmentSet_TotalEquippedLevel(equipmentToEquip);
			if (num > Get_EquipmentSet_CurrentLevel(equipmentToEquip))
			{
				Set_EquipmentSet_CurrentLevel(equipmentToEquip, num, additive: false);
			}
			if (runEvents)
			{
				Instance.m_onEquippedChangeArgs.Initialize(category, equipmentToEquip);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.EquippedChanged, Instance, Instance.m_onEquippedChangeArgs);
			}
			return flag;
		}
		return false;
	}

	public static float GetMinEquipmentLevelScale()
	{
		int num = 0;
		EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
		{
			if (equipmentCategoryType == EquipmentCategoryType.None)
			{
				continue;
			}
			foreach (KeyValuePair<EquipmentType, EquipmentObj> item in GetEquipmentDict(equipmentCategoryType))
			{
				if (item.Value.FoundState > FoundState.NotFound && item.Value.FoundState < FoundState.Purchased)
				{
					num++;
				}
				num += GetUpgradeBlueprintsFound(equipmentCategoryType, item.Key);
			}
		}
		return Mathf.Ceil((float)num * 0.75f);
	}

	public static EquipmentObj GetEquipment(EquipmentCategoryType category, EquipmentType equipType)
	{
		Dictionary<EquipmentType, EquipmentObj> equipmentDict = GetEquipmentDict(category);
		if (equipmentDict == null || !equipmentDict.TryGetValue(equipType, out var value))
		{
			return null;
		}
		return value;
	}

	public static float GetEquippedStatValue(EquipmentCategoryType categoryType, EquipmentStatType equipmentStatType)
	{
		return GetEquipped(categoryType)?.GetCurrentStatValue(equipmentStatType) ?? 0f;
	}

	public static float GetTotalEquippedStatValue(EquipmentStatType equipmentStatType)
	{
		return 0f + GetEquippedStatValue(EquipmentCategoryType.Weapon, equipmentStatType) + GetEquippedStatValue(EquipmentCategoryType.Head, equipmentStatType) + GetEquippedStatValue(EquipmentCategoryType.Chest, equipmentStatType) + GetEquippedStatValue(EquipmentCategoryType.Cape, equipmentStatType) + GetEquippedStatValue(EquipmentCategoryType.Trinket, equipmentStatType);
	}

	public static int GetWeightLevel()
	{
		float currentStatGain = SkillTreeManager.GetSkillTreeObj(SkillTreeType.Weight_CD_Reduce).CurrentStatGain;
		float num = 0.2f + currentStatGain;
		float num2 = (PlayerManager.IsInstantiated ? PlayerManager.GetPlayerController().ActualAllowedEquipmentWeight : 0);
		float num3 = ((num2 > 0f) ? (GetTotalEquippedStatValue(EquipmentStatType.Weight) / num2) : 0f);
		float num4 = num3 / num;
		float num5 = (int)(num3 / num);
		if (num4 == num5 && num5 > 0f)
		{
			num5 -= 1f;
		}
		return (int)num5;
	}

	public static int Get_EquipmentSet_CurrentUnityTier(EquipmentType equipType)
	{
		int num = Get_EquipmentSet_TotalEquippedLevel(equipType);
		EquipmentSetData equipmentSetData = EquipmentSetLibrary.GetEquipmentSetData(equipType);
		if (equipmentSetData != null)
		{
			int setRequirement = equipmentSetData.SetRequirement03;
			int setRequirement2 = equipmentSetData.SetRequirement02;
			int setRequirement3 = equipmentSetData.SetRequirement01;
			if (num >= setRequirement)
			{
				return 3;
			}
			if (num >= setRequirement2)
			{
				return 2;
			}
			if (num >= setRequirement3)
			{
				return 1;
			}
		}
		return 0;
	}

	public static Dictionary<EquipmentType, EquipmentObj> GetEquipmentDict(EquipmentCategoryType category)
	{
		return SaveManager.EquipmentSaveData.GetEquipmentDict(category);
	}

	public static int GetTotalAvailableBlueprints(List<EquipmentObj> populatedList = null)
	{
		populatedList?.Clear();
		int num = 0;
		EquipmentCategoryType[] categoryTypeArray = EquipmentType_RL.CategoryTypeArray;
		foreach (EquipmentCategoryType equipmentCategoryType in categoryTypeArray)
		{
			if (equipmentCategoryType == EquipmentCategoryType.None)
			{
				continue;
			}
			EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
			foreach (EquipmentType equipmentType in typeArray)
			{
				if (equipmentType != 0)
				{
					EquipmentObj equipment = GetEquipment(equipmentCategoryType, equipmentType);
					if (equipment != null && !equipment.EquipmentData.Disabled)
					{
						num++;
						populatedList?.Add(equipment);
					}
				}
			}
		}
		return num;
	}
}
