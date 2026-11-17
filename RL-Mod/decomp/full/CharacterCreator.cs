using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class CharacterCreator
{
	private static List<ClassType> m_classList = new List<ClassType>();

	public static CharacterData GenerateRandomCharacter(List<CharacterData> charsAlreadyChosen = null, bool forceRandomizeKit = false)
	{
		CharacterData characterData = new CharacterData();
		string[] availableNames = GetAvailableNames(characterData.IsFemale = GetRandomGender(useUnityRandom: false));
		string name = availableNames[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomName", 0, availableNames.Length)];
		if (charsAlreadyChosen != null)
		{
			int num = 0;
			while (charsAlreadyChosen.Any((CharacterData x) => x.Name == name) && num < 50)
			{
				name = availableNames[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomName", 0, availableNames.Length)];
				num++;
			}
		}
		characterData.Name = name;
		int num2 = 0;
		if (SaveManager.LineageSaveData.DuplicateNameCountDict.ContainsKey(characterData.Name))
		{
			num2 = SaveManager.LineageSaveData.DuplicateNameCountDict[characterData.Name];
			num2++;
		}
		else
		{
			num2 = 0;
		}
		characterData.DuplicateNameCount = num2;
		List<ClassType> availableClasses = GetAvailableClasses();
		ClassType classType = availableClasses[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomClass", 0, availableClasses.Count)];
		if (charsAlreadyChosen != null)
		{
			int num3 = 0;
			while (charsAlreadyChosen.Any((CharacterData x) => x.ClassType == classType) && num3 < 50)
			{
				classType = availableClasses[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomClass", 0, availableClasses.Count)];
				num3++;
			}
		}
		GenerateClass(classType, characterData);
		Vector2Int randomTraits = GetRandomTraits(forceRandomizeKit);
		characterData.TraitOne = (TraitType)randomTraits.x;
		characterData.TraitTwo = (TraitType)randomTraits.y;
		if (characterData.TraitOne == TraitType.Antique)
		{
			RelicType randomRelic = RelicLibrary.GetRandomRelic(RngID.Lineage, limitMaxStacks: true, Antique_Trait.RelicExceptionArray);
			characterData.AntiqueOneOwned = randomRelic;
		}
		if (characterData.TraitTwo == TraitType.Antique)
		{
			RelicType randomRelic2 = RelicLibrary.GetRandomRelic(RngID.Lineage, limitMaxStacks: true, Antique_Trait.RelicExceptionArray);
			characterData.AntiqueTwoOwned = randomRelic2;
			while (characterData.AntiqueTwoOwned == characterData.AntiqueOneOwned)
			{
				randomRelic2 = RelicLibrary.GetRandomRelic(RngID.Lineage, limitMaxStacks: true, Antique_Trait.RelicExceptionArray);
				characterData.AntiqueTwoOwned = randomRelic2;
			}
		}
		if (characterData.TraitOne == TraitType.RandomizeKit || characterData.TraitTwo == TraitType.RandomizeKit)
		{
			ApplyRandomizeKitTrait(characterData);
		}
		if (characterData.TraitOne == TraitType.Disposition || characterData.TraitTwo == TraitType.Disposition)
		{
			DispositionIDType[] array = Enum.GetValues(typeof(DispositionIDType)) as DispositionIDType[];
			characterData.Disposition_ID = (byte)RNGManager.GetRandomNumber(RngID.Lineage, "GetDispositionID", 0, array.Length);
		}
		GenerateRandomLook(characterData);
		charsAlreadyChosen.Add(characterData);
		return characterData;
	}

	private static void ApplyRandomizeKitTrait(CharacterData charData)
	{
		AbilityType[] availableWeapons = GetAvailableWeapons(ClassType.CURIO_SHOPPE_CLASS);
		AbilityType abilityType = (charData.Weapon = ((availableWeapons.Length != 0) ? availableWeapons[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomWeapon", 0, availableWeapons.Length)] : AbilityType.None));
		AbilityType[] availableTalents = GetAvailableTalents(ClassType.CURIO_SHOPPE_CLASS);
		AbilityType abilityType2 = ((availableTalents.Length != 0) ? availableTalents[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTalent", 0, availableTalents.Length)] : AbilityType.None);
		int num = 0;
		while (abilityType2 != 0 && abilityType2 == abilityType && num < 50)
		{
			num++;
			abilityType2 = availableTalents[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTalent", 0, availableTalents.Length)];
		}
		if (num >= 50)
		{
			Debug.LogWarning("<color=yellow>Could not find non-duplicate talent in CharacterCreator.ApplyRandomizeKitTrait.</color>");
		}
		charData.Talent = abilityType2;
	}

	public static void GenerateClass(ClassType classType, CharacterData charDataToMod)
	{
		charDataToMod.ClassType = classType;
		AbilityType[] availableWeapons = GetAvailableWeapons(classType);
		AbilityType abilityType = (charDataToMod.Weapon = ((availableWeapons.Length != 0) ? availableWeapons[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomWeapon", 0, availableWeapons.Length)] : AbilityType.None));
		AbilityType[] availableSpells = GetAvailableSpells(classType);
		AbilityType abilityType2 = ((availableSpells.Length != 0) ? availableSpells[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomSpell", 0, availableSpells.Length)] : AbilityType.None);
		int num = 0;
		while (abilityType2 != 0 && abilityType2 == abilityType && num < 50)
		{
			num++;
			abilityType2 = availableSpells[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomSpell", 0, availableSpells.Length)];
		}
		if (num >= 50)
		{
			Debug.LogWarning("<color=yellow>Could not find non-duplicate spell in CharacterCreator.</color>");
		}
		charDataToMod.Spell = abilityType2;
		AbilityType[] availableTalents = GetAvailableTalents(classType);
		AbilityType abilityType3 = ((availableTalents.Length != 0) ? availableTalents[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTalent", 0, availableTalents.Length)] : AbilityType.None);
		num = 0;
		while (abilityType3 != 0 && (abilityType3 == abilityType || abilityType3 == abilityType2) && num < 50)
		{
			num++;
			abilityType3 = availableTalents[RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTalent", 0, availableTalents.Length)];
		}
		if (num >= 50)
		{
			Debug.LogWarning("<color=yellow>Could not find non-duplicate talent in CharacterCreator.</color>");
		}
		charDataToMod.Talent = abilityType3;
	}

	public static bool GetRandomGender(bool useUnityRandom)
	{
		int num = RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomGender", 0, 2);
		if (useUnityRandom)
		{
			num = UnityEngine.Random.Range(0, 2);
		}
		return num > 0;
	}

	public static string[] GetAvailableNames(bool isFemale)
	{
		if (!isFemale)
		{
			return LocalizationManager.MaleNameArray;
		}
		return LocalizationManager.FemaleNameArray;
	}

	public static List<ClassType> GetAvailableClasses()
	{
		m_classList.Clear();
		ClassType[] typeArray = ClassType_RL.TypeArray;
		for (int i = 0; i < typeArray.Length; i++)
		{
			ClassType classType = typeArray[i];
			if (classType == ClassType.None)
			{
				continue;
			}
			if (Player_EV.STARTING_UNLOCKED_CLASSES.Contains(classType) && !m_classList.Contains(classType))
			{
				m_classList.Add(classType);
				continue;
			}
			ClassData classData = ClassLibrary.GetClassData(classType);
			if (classData != null && classData.PassiveData != null && classData.SpellData != null && classData.TalentData != null && classData.WeaponData != null)
			{
				if (SkillTreeLogicHelper.IsClassUnlocked(classType) && !m_classList.Contains(classType))
				{
					m_classList.Add(classType);
				}
			}
			else
			{
				Debug.LogWarning("<color=yellow>WARNING: Could not add class " + classType.ToString() + " to CharacterCreator.GetAvailableClasses().  Some data is missing from the class.</color>");
			}
		}
		return m_classList;
	}

	public static AbilityType[] GetAvailableWeapons(ClassType classType)
	{
		return ClassLibrary.GetClassData(classType).WeaponData.WeaponAbilityArray;
	}

	public static AbilityType[] GetAvailableSpells(ClassType classType)
	{
		return ClassLibrary.GetClassData(classType).SpellData.SpellAbilityArray;
	}

	public static AbilityType[] GetAvailableTalents(ClassType classType)
	{
		ClassData classData = ClassLibrary.GetClassData(classType);
		_ = 30;
		return classData.TalentData.TalentAbilityArray;
	}

	public static Vector2Int GetRandomTraits(bool forceRandomizeKit = false)
	{
		TraitType traitType = TraitType.None;
		TraitType traitType2 = TraitType.None;
		List<TraitType> list = new List<TraitType>();
		List<TraitType> list2 = new List<TraitType>();
		List<TraitType> list3 = new List<TraitType>();
		bool flag = HolidayLookController.IsHoliday(HolidayType.Halloween);
		bool flag2 = HolidayLookController.IsHoliday(HolidayType.Christmas);
		TraitType[] typeArray = TraitType_RL.TypeArray;
		foreach (TraitType traitType3 in typeArray)
		{
			if (traitType3 == TraitType.None || (flag && traitType3 == TraitType.HalloweenHoliday) || (flag2 && traitType3 == TraitType.ChristmasHoliday))
			{
				continue;
			}
			TraitData traitData = TraitLibrary.GetTraitData(traitType3);
			if ((bool)traitData)
			{
				switch (traitData.Rarity)
				{
				case 1:
					list.Add(traitType3);
					break;
				case 2:
					list2.Add(traitType3);
					break;
				case 3:
					list3.Add(traitType3);
					break;
				}
			}
		}
		for (int j = 0; j < 2; j++)
		{
			if (forceRandomizeKit && j == 0)
			{
				traitType = TraitType.RandomizeKit;
				continue;
			}
			float num = 0f;
			num = ((j != 0) ? 0.365f : 0.685f);
			if (RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTraitSpawnChance", 0f, 1f) > num)
			{
				continue;
			}
			if (RNGManager.GetRandomNumber(RngID.Lineage, "GetAntiqueSpawnChance", 0f, 1f) < 0.22f)
			{
				if (j == 0)
				{
					traitType = TraitType.Antique;
				}
				else
				{
					traitType2 = TraitType.Antique;
				}
				continue;
			}
			bool flag3 = true;
			if (SaveManager.PlayerSaveData.EnableHouseRules && SaveManager.PlayerSaveData.Assist_DisableTraits)
			{
				flag3 = false;
			}
			if (!flag3)
			{
				continue;
			}
			float max = 1f;
			float randomNumber = RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTraitRarityRoll", 0f, max);
			List<TraitType> list4 = null;
			list4 = ((randomNumber <= 1f) ? list : ((!(randomNumber > 1f) || !(randomNumber <= 1f)) ? list3 : list2));
			int count = list4.Count;
			int randomNumber2 = RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTraitChoiceRoll - Trait #" + (j + 1), 0, count);
			if (j == 0)
			{
				traitType = list4[randomNumber2];
				continue;
			}
			traitType2 = list4[randomNumber2];
			if (traitType != 0 || traitType2 != 0)
			{
				int num2 = 0;
				while ((traitType2 == traitType || !AreTraitsCompatible(traitType, traitType2)) && num2 < 20)
				{
					randomNumber2 = RNGManager.GetRandomNumber(RngID.Lineage, "GetRandomTraitChoiceRoll (Trait Find Failed) - Trait #" + (j + 1), 0, count);
					traitType2 = list4[randomNumber2];
					num2++;
				}
			}
		}
		if ((traitType == TraitType.None || traitType2 == TraitType.None) && traitType == TraitType.None)
		{
			traitType = traitType2;
			traitType2 = TraitType.None;
		}
		if (traitType == TraitType.Antique && traitType2 != TraitType.Antique && traitType2 != 0)
		{
			TraitType num3 = traitType2;
			traitType2 = TraitType.Antique;
			traitType = num3;
		}
		return new Vector2Int((int)traitType, (int)traitType2);
	}

	private static bool AreTraitsCompatible(TraitType traitType1, TraitType traitType2)
	{
		BaseTrait trait = TraitLibrary.GetTrait(traitType1);
		BaseTrait trait2 = TraitLibrary.GetTrait(traitType2);
		bool num = traitType1 == TraitType.None || trait.IncompatibleTraits == null || !trait.IncompatibleTraits.Contains(traitType2);
		bool result = traitType2 == TraitType.None || trait2.IncompatibleTraits == null || !trait2.IncompatibleTraits.Contains(traitType1);
		if (num)
		{
			return result;
		}
		return false;
	}

	public static void GenerateRandomLook(CharacterData charData)
	{
		bool isFemale = charData.IsFemale;
		charData.EyeType = GetRandomLook(isFemale, LookType.Eyes);
		charData.MouthType = GetRandomLook(isFemale, LookType.Mouth);
		charData.HairType = GetRandomLook(isFemale, LookType.Hair);
		charData.FacialHairType = GetRandomLook(isFemale, LookType.FacialHair);
		charData.BodyType = 0;
		charData.SkinColorType = GetRandomLook(isFemale, LookType.SkinColor);
		charData.HairColorType = GetRandomLook(isFemale, LookType.HairColor);
	}

	public static int GetRandomLook(bool isFemale, LookType lookType)
	{
		IList list = null;
		switch (lookType)
		{
		case LookType.Body:
			list = LookLibrary.GetBodyLookData();
			break;
		case LookType.Eyes:
			list = LookLibrary.GetEyeLookData();
			break;
		case LookType.FacialHair:
			list = LookLibrary.GetFacialHairLookData();
			break;
		case LookType.Hair:
			list = LookLibrary.GetHairLookData();
			break;
		case LookType.Mouth:
			list = LookLibrary.GetMouthLookData();
			break;
		case LookType.SkinColor:
			list = LookLibrary.GetSkinColorLookData();
			break;
		case LookType.HairColor:
			list = LookLibrary.GetHairColorLookData();
			break;
		}
		float num = 0f;
		foreach (ILookWeight item in list)
		{
			if (!item.ExcludeFromWeighing)
			{
				num = (isFemale ? (num + item.FemaleWeight) : (num + item.MaleWeight));
			}
		}
		float num2 = RNGManager.GetRandomNumber(RngID.Lineage, "CreateRandomLook() - Random " + lookType, 0, (int)(num * 100f));
		num2 *= 0.01f;
		int num3 = 0;
		foreach (ILookWeight item2 in list)
		{
			float num4 = (isFemale ? item2.FemaleWeight : item2.MaleWeight);
			if (!item2.ExcludeFromWeighing && num4 > 0f)
			{
				num2 -= num4;
				if (num2 <= 0f)
				{
					break;
				}
			}
			num3++;
		}
		return num3;
	}
}
