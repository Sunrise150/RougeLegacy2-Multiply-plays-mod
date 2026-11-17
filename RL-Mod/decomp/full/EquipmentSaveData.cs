using System;
using System.Collections.Generic;

[Serializable]
public class EquipmentSaveData : IVersionUpdateable
{
	public int REVISION_NUMBER = 8;

	public int FILE_NUMBER;

	public Dictionary<RuneType, RuneObj> RuneDict;

	public Dictionary<SkillTreeType, SkillTreeObj> SkillTreeDict;

	public Dictionary<EquipmentType, EquipmentObj> WeaponEquipmentDict;

	public Dictionary<EquipmentType, EquipmentObj> HeadEquipmentDict;

	public Dictionary<EquipmentType, EquipmentObj> ChestEquipmentDict;

	public Dictionary<EquipmentType, EquipmentObj> CapeEquipmentDict;

	public Dictionary<EquipmentType, EquipmentObj> TrinketEquipmentDict;

	public Dictionary<ClassType, EquipmentLoadout> EquipmentLoadoutDict;

	public Dictionary<ClassType, RuneLoadout> RuneLoadoutDict;

	public bool EquipmentLoadoutEnabled;

	public bool RuneLoadoutEnabled;

	public Dictionary<EquipmentType, SerializableVector2Int> EquipmentSetStateDict;

	public bool IsInitialized { get; private set; }

	public EquipmentSaveData()
	{
		Initialize();
	}

	public void Initialize()
	{
		InitializeSkills();
		InitializeRunes();
		InitializeEquipment();
		InitializeLoadouts();
		SetStartingEquipmentSaveData();
		IsInitialized = true;
	}

	private void InitializeSkills()
	{
		if (SkillTreeDict == null)
		{
			SkillTreeDict = new Dictionary<SkillTreeType, SkillTreeObj>();
		}
		foreach (SkillTreeType value in Enum.GetValues(typeof(SkillTreeType)))
		{
			if (value != 0 && !SkillTreeDict.ContainsKey(value))
			{
				SkillTreeDict.Add(value, new SkillTreeObj(value));
			}
		}
	}

	private void InitializeRunes()
	{
		if (RuneDict == null)
		{
			RuneDict = new Dictionary<RuneType, RuneObj>();
		}
		RuneType[] typeArray = RuneType_RL.TypeArray;
		foreach (RuneType runeType in typeArray)
		{
			if (runeType != 0 && !RuneDict.ContainsKey(runeType))
			{
				RuneDict.Add(runeType, new RuneObj(runeType));
			}
		}
	}

	private void InitializeEquipment()
	{
		if (WeaponEquipmentDict == null)
		{
			WeaponEquipmentDict = new Dictionary<EquipmentType, EquipmentObj>();
		}
		if (HeadEquipmentDict == null)
		{
			HeadEquipmentDict = new Dictionary<EquipmentType, EquipmentObj>();
		}
		if (ChestEquipmentDict == null)
		{
			ChestEquipmentDict = new Dictionary<EquipmentType, EquipmentObj>();
		}
		if (CapeEquipmentDict == null)
		{
			CapeEquipmentDict = new Dictionary<EquipmentType, EquipmentObj>();
		}
		if (TrinketEquipmentDict == null)
		{
			TrinketEquipmentDict = new Dictionary<EquipmentType, EquipmentObj>();
		}
		if (EquipmentSetStateDict == null)
		{
			EquipmentSetStateDict = new Dictionary<EquipmentType, SerializableVector2Int>();
		}
		EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
		foreach (EquipmentType equipmentType in typeArray)
		{
			if (equipmentType != 0)
			{
				if (!WeaponEquipmentDict.ContainsKey(equipmentType))
				{
					WeaponEquipmentDict.Add(equipmentType, new EquipmentObj(EquipmentCategoryType.Weapon, equipmentType));
				}
				if (!HeadEquipmentDict.ContainsKey(equipmentType))
				{
					HeadEquipmentDict.Add(equipmentType, new EquipmentObj(EquipmentCategoryType.Head, equipmentType));
				}
				if (!ChestEquipmentDict.ContainsKey(equipmentType))
				{
					ChestEquipmentDict.Add(equipmentType, new EquipmentObj(EquipmentCategoryType.Chest, equipmentType));
				}
				if (!CapeEquipmentDict.ContainsKey(equipmentType))
				{
					CapeEquipmentDict.Add(equipmentType, new EquipmentObj(EquipmentCategoryType.Cape, equipmentType));
				}
				if (!TrinketEquipmentDict.ContainsKey(equipmentType))
				{
					TrinketEquipmentDict.Add(equipmentType, new EquipmentObj(EquipmentCategoryType.Trinket, equipmentType));
				}
				if (!EquipmentSetStateDict.ContainsKey(equipmentType))
				{
					EquipmentSetStateDict.Add(equipmentType, new SerializableVector2Int(0, 0));
				}
			}
		}
	}

	private void InitializeLoadouts()
	{
		if (EquipmentLoadoutDict == null)
		{
			EquipmentLoadoutDict = new Dictionary<ClassType, EquipmentLoadout>();
		}
		if (RuneLoadoutDict == null)
		{
			RuneLoadoutDict = new Dictionary<ClassType, RuneLoadout>();
		}
		ClassType[] typeArray = ClassType_RL.TypeArray;
		foreach (ClassType classType in typeArray)
		{
			if (classType != 0)
			{
				if (!EquipmentLoadoutDict.ContainsKey(classType))
				{
					EquipmentLoadoutDict.Add(classType, new EquipmentLoadout());
				}
				if (!RuneLoadoutDict.ContainsKey(classType))
				{
					RuneLoadoutDict.Add(classType, new RuneLoadout());
				}
			}
		}
	}

	private void SetStartingEquipmentSaveData()
	{
		if (SkillTreeDict[SkillTreeType.Sword_Class_Unlock].Level < 1)
		{
			SkillTreeDict[SkillTreeType.Sword_Class_Unlock].Level = 1;
		}
		if (HeadEquipmentDict[EquipmentType.GEAR_BONUS_WEIGHT].UpgradeLevel <= -3)
		{
			HeadEquipmentDict[EquipmentType.GEAR_BONUS_WEIGHT].UpgradeLevel = -2;
		}
		if (RuneDict[RuneType.Lifesteal].UpgradeLevel <= -3)
		{
			RuneDict[RuneType.Lifesteal].UpgradeLevel = -2;
		}
		if (RuneDict[RuneType.Lifesteal].UpgradeBlueprintsFound < 1)
		{
			RuneDict[RuneType.Lifesteal].UpgradeBlueprintsFound = 1;
		}
		if (RuneDict[RuneType.Magnet].UpgradeLevel <= -3)
		{
			RuneDict[RuneType.Magnet].UpgradeLevel = -2;
		}
		if (RuneDict[RuneType.Magnet].UpgradeBlueprintsFound < 1)
		{
			RuneDict[RuneType.Magnet].UpgradeBlueprintsFound = 1;
		}
	}

	public Dictionary<EquipmentType, EquipmentObj> GetEquipmentDict(EquipmentCategoryType category)
	{
		return category switch
		{
			EquipmentCategoryType.Weapon => WeaponEquipmentDict, 
			EquipmentCategoryType.Head => HeadEquipmentDict, 
			EquipmentCategoryType.Chest => ChestEquipmentDict, 
			EquipmentCategoryType.Cape => CapeEquipmentDict, 
			EquipmentCategoryType.Trinket => TrinketEquipmentDict, 
			_ => null, 
		};
	}

	public EquipmentLoadout GetEquipmentLoadout(ClassType classType)
	{
		if (EquipmentLoadoutDict.TryGetValue(classType, out var value))
		{
			return value;
		}
		return null;
	}

	public RuneLoadout GetRuneLoadout(ClassType classType)
	{
		if (RuneLoadoutDict.TryGetValue(classType, out var value))
		{
			return value;
		}
		return null;
	}

	public void UpdateVersion()
	{
		Initialize();
		if (REVISION_NUMBER != 8)
		{
			if (REVISION_NUMBER <= 0)
			{
				ResetEquipment(EquipmentCategoryType.Weapon);
				ResetEquipment(EquipmentCategoryType.Head);
				ResetEquipment(EquipmentCategoryType.Chest);
				ResetEquipment(EquipmentCategoryType.Cape);
				ResetEquipment(EquipmentCategoryType.Trinket);
				ResetEquipmentSetLevels();
				ResetSkillTree();
				ResetRunes();
				SetStartingEquipmentSaveData();
			}
			if (REVISION_NUMBER <= 1)
			{
				ResetSkillTree();
			}
			if (REVISION_NUMBER <= 2)
			{
				UnequipAllRunes();
			}
			if (REVISION_NUMBER <= 3)
			{
				UnequipAllRunes();
				ResetSkillTree();
			}
			if (REVISION_NUMBER <= 4)
			{
				foreach (KeyValuePair<RuneType, RuneObj> item in RuneDict)
				{
					RuneObj value = item.Value;
					if (value != null)
					{
						if (value.UpgradeLevel > -3 && value.UpgradeBlueprintsFound < 1)
						{
							value.UpgradeBlueprintsFound = 1;
						}
						else if (value.UpgradeBlueprintsFound > 0 && value.UpgradeLevel < -2)
						{
							value.UpgradeLevel = -2;
						}
					}
				}
			}
			if (REVISION_NUMBER <= 5)
			{
				UnequipAllRunes();
			}
			if (REVISION_NUMBER <= 6)
			{
				UnequipAllRunes();
				ResetSkillTree();
				SetStartingEquipmentSaveData();
			}
			if (REVISION_NUMBER <= 7)
			{
				SkillTreeDict[SkillTreeType.Gold_Gain_Up_4].Level = 0;
				SkillTreeDict[SkillTreeType.Gold_Gain_Up_5].Level = 0;
			}
		}
		REVISION_NUMBER = 8;
	}

	private void UnequipAllRunes()
	{
		foreach (KeyValuePair<RuneType, RuneObj> item in RuneDict)
		{
			RuneObj value = item.Value;
			if (value != null)
			{
				value.EquippedLevel = 0;
			}
		}
	}

	private void ResetEquipment(EquipmentCategoryType categoryType)
	{
		int num = -1;
		Dictionary<EquipmentType, EquipmentObj> equipmentDict = GetEquipmentDict(categoryType);
		if (equipmentDict == null)
		{
			return;
		}
		foreach (KeyValuePair<EquipmentType, EquipmentObj> item in equipmentDict)
		{
			EquipmentObj value = item.Value;
			if (value != null && value.UpgradeLevel > num)
			{
				value.UpgradeLevel = num;
			}
		}
	}

	private void ResetEquipmentSetLevels()
	{
		EquipmentType[] typeArray = EquipmentType_RL.TypeArray;
		foreach (EquipmentType equipmentType in typeArray)
		{
			if (equipmentType != 0 && EquipmentSetStateDict.ContainsKey(equipmentType))
			{
				SerializableVector2Int value = EquipmentSetStateDict[equipmentType];
				value.x = 0;
				EquipmentSetStateDict[equipmentType] = value;
			}
		}
	}

	private void ResetSkillTree()
	{
		foreach (KeyValuePair<SkillTreeType, SkillTreeObj> item in SkillTreeDict)
		{
			SkillTreeObj value = item.Value;
			if (value != null)
			{
				value.Level = 0;
			}
		}
	}

	private void ResetRunes()
	{
		int num = -1;
		foreach (KeyValuePair<RuneType, RuneObj> item in RuneDict)
		{
			RuneObj value = item.Value;
			if (value != null)
			{
				if (value.UpgradeLevel > num)
				{
					value.UpgradeLevel = num;
				}
				value.EquippedLevel = 0;
			}
		}
	}
}
