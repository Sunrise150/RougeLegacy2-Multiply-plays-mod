public class Equipment_EV
{
	public const int EQUIPMENT_ATTRIBUTE_GAIN_1 = 1;

	public const int EQUIPMENT_ATTRIBUTE_GAIN_2 = 2;

	public const int EQUIPMENT_ATTRIBUTE_GAIN_3 = 3;

	public const float WEIGHT_PERCENT_LEVEL_UP = 0.2f;

	public const float CD_ADD_PER_WEIGHT_LEVEL = 0f;

	public const float MANA_REGEN_DOWN_PER_WEIGHT_LEVEL = 0f;

	public const float MANA_TOTAL_DOWN_PER_WEIGHT_LEVEL = 0f;

	public static readonly float[] CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL = new float[4];

	public static readonly float[] MAGIC_CRIT_DAMAGE_BONUS_PER_WEIGHT_LEVEL = new float[4];

	public static readonly float[] RESOLVE_BONUS_PER_WEIGHT_LEVEL = new float[4] { 1.25f, 1f, 0.75f, 0.5f };

	public static readonly EquipmentType[] BlacksmithOrder = new EquipmentType[15]
	{
		EquipmentType.GEAR_BONUS_WEIGHT,
		EquipmentType.GEAR_MAGIC_CRIT,
		EquipmentType.GEAR_STRENGTH_CRIT,
		EquipmentType.GEAR_LIFE_STEAL,
		EquipmentType.GEAR_ARMOR,
		EquipmentType.GEAR_MAGIC_DMG,
		EquipmentType.GEAR_MOBILITY,
		EquipmentType.GEAR_GOLD,
		EquipmentType.GEAR_RETURN_DMG,
		EquipmentType.GEAR_MAG_ON_HIT,
		EquipmentType.GEAR_LIFE_STEAL_2,
		EquipmentType.GEAR_REVIVE,
		EquipmentType.GEAR_FINAL_BOSS,
		EquipmentType.GEAR_EMPTY_1,
		EquipmentType.GEAR_EMPTY_2
	};

	public static string GetFormatterLocID(EquipmentCategoryType category)
	{
		return "LOC_ID_FORMATTER_" + category.ToString().ToUpper() + "_EQUIPMENT_1";
	}

	public static string GetCategoryNameLocID(EquipmentCategoryType category)
	{
		return "LOC_ID_EQUIPMENT_SLOT_" + category.ToString().ToUpper() + "_1";
	}

	public static string GetEquipmentTypeNameLocID(EquipmentType equipType)
	{
		return "LOC_ID_EQUIPMENT_MATERIAL_" + equipType.ToString().ToUpper() + "_1";
	}

	public static string GetFormattedEquipmentName(EquipmentCategoryType category, EquipmentType equipType)
	{
		string @string = LocalizationManager.GetString(GetFormatterLocID(category), isFemale: false, disableWordScramble: true);
		bool isFemale = false;
		string formatterGenderForcedString = LocalizationManager.GetFormatterGenderForcedString(@string, out isFemale);
		string string2 = LocalizationManager.GetString(GetCategoryNameLocID(category), isFemale, disableWordScramble: true);
		string string3 = LocalizationManager.GetString(GetEquipmentTypeNameLocID(equipType), isFemale, disableWordScramble: true);
		return string.Format(formatterGenderForcedString, string3, string2);
	}
}
