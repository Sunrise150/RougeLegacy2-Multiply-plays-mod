using System;
using System.Collections.Generic;
using System.Linq;

public static class AbilityType_RL
{
	private static AbilityType[] m_typeArray;

	private static AbilityType[] m_weaponTypeArray;

	private static AbilityType[] m_spellTypeArray;

	private static AbilityType[] m_talentTypeArray;

	private static AbilityType[] m_validWeaponTypeArray;

	private static AbilityType[] m_validSpellTypeArray;

	private static AbilityType[] m_validTalentTypeArray;

	public static AbilityType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(AbilityType)) as AbilityType[];
			}
			return m_typeArray;
		}
	}

	public static AbilityType[] WeaponAbilityArray
	{
		get
		{
			if (m_weaponTypeArray == null)
			{
				int num = TypeArray.IndexOf(AbilityType.____WEAPON_ABILITIES____) + 1;
				int num2 = TypeArray.IndexOf(AbilityType.____SPELL_ABILITIES____);
				int num3 = num2 - num;
				if (num > -1 && num2 > -1 && num3 > 0)
				{
					m_weaponTypeArray = new AbilityType[num3];
					int num4 = 0;
					for (int i = num; i < num2; i++)
					{
						m_weaponTypeArray[num4] = TypeArray[i];
						num4++;
					}
				}
			}
			return m_weaponTypeArray;
		}
	}

	public static AbilityType[] ValidWeaponTypeArray
	{
		get
		{
			if (m_validWeaponTypeArray == null)
			{
				AbilityType[] weaponAbilityArray = ClassLibrary.GetClassData(ClassType.CURIO_SHOPPE_CLASS).WeaponData.WeaponAbilityArray;
				m_validWeaponTypeArray = new AbilityType[weaponAbilityArray.Length];
				Array.Copy(weaponAbilityArray, m_validWeaponTypeArray, weaponAbilityArray.Length);
			}
			return m_validWeaponTypeArray;
		}
	}

	public static AbilityType[] SpellAbilityArray
	{
		get
		{
			if (m_spellTypeArray == null)
			{
				int num = TypeArray.IndexOf(AbilityType.____SPELL_ABILITIES____) + 1;
				int num2 = TypeArray.IndexOf(AbilityType.____TALENT_ABILITIES____);
				int num3 = num2 - num;
				if (num > -1 && num2 > -1 && num3 > 0)
				{
					m_spellTypeArray = new AbilityType[num3];
					int num4 = 0;
					for (int i = num; i < num2; i++)
					{
						m_spellTypeArray[num4] = TypeArray[i];
						num4++;
					}
				}
			}
			return m_spellTypeArray;
		}
	}

	public static AbilityType[] ValidSpellTypeArray
	{
		get
		{
			if (m_validSpellTypeArray == null)
			{
				AbilityType[] spellAbilityArray = ClassLibrary.GetClassData(ClassType.CURIO_SHOPPE_CLASS).SpellData.SpellAbilityArray;
				m_validSpellTypeArray = new AbilityType[spellAbilityArray.Length];
				Array.Copy(spellAbilityArray, m_validSpellTypeArray, spellAbilityArray.Length);
			}
			return m_validSpellTypeArray;
		}
	}

	public static AbilityType[] TalentAbilityArray
	{
		get
		{
			if (m_talentTypeArray == null)
			{
				int num = TypeArray.IndexOf(AbilityType.____TALENT_ABILITIES____) + 1;
				int num2 = TypeArray.IndexOf(AbilityType.____SPECIAL_ABILITIES____);
				int num3 = num2 - num;
				if (num > -1 && num2 > -1 && num3 > 0)
				{
					m_talentTypeArray = new AbilityType[num3];
					int num4 = 0;
					for (int i = num; i < num2; i++)
					{
						m_talentTypeArray[num4] = TypeArray[i];
						num4++;
					}
				}
			}
			return m_talentTypeArray;
		}
	}

	public static AbilityType[] ValidTalentTypeArray
	{
		get
		{
			if (m_validTalentTypeArray == null)
			{
				AbilityType[] talentAbilityArray = ClassLibrary.GetClassData(ClassType.CURIO_SHOPPE_CLASS).TalentData.TalentAbilityArray;
				m_validTalentTypeArray = new AbilityType[talentAbilityArray.Length];
				Array.Copy(talentAbilityArray, m_validTalentTypeArray, talentAbilityArray.Length);
			}
			return m_validTalentTypeArray;
		}
	}

	public static List<AbilityType> GetWeaponAbilityList()
	{
		List<AbilityType> list = ((AbilityType[])Enum.GetValues(typeof(AbilityType))).ToList();
		List<AbilityType> list2 = new List<AbilityType>();
		int num = list.IndexOf(AbilityType.____WEAPON_ABILITIES____) + 1;
		int num2 = list.IndexOf(AbilityType.____SPELL_ABILITIES____);
		for (int i = num; i < num2; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public static List<AbilityType> GetSpellAbilityList()
	{
		List<AbilityType> list = ((AbilityType[])Enum.GetValues(typeof(AbilityType))).ToList();
		List<AbilityType> list2 = new List<AbilityType>();
		int num = list.IndexOf(AbilityType.____SPELL_ABILITIES____) + 1;
		int num2 = list.IndexOf(AbilityType.____TALENT_ABILITIES____);
		for (int i = num; i < num2; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public static List<AbilityType> GetTalentAbilityList()
	{
		List<AbilityType> list = ((AbilityType[])Enum.GetValues(typeof(AbilityType))).ToList();
		List<AbilityType> list2 = new List<AbilityType>();
		int num = list.IndexOf(AbilityType.____TALENT_ABILITIES____) + 1;
		int num2 = list.IndexOf(AbilityType.____SPECIAL_ABILITIES____);
		for (int i = num; i < num2; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	public static bool IsMageSpellOrTalent(AbilityType abilityType)
	{
		ClassData classData = ClassLibrary.GetClassData(ClassType.MagicWandClass);
		if (!classData.SpellData.SpellAbilityArray.Contains(abilityType))
		{
			return classData.TalentData.TalentAbilityArray.Contains(abilityType);
		}
		return true;
	}
}
