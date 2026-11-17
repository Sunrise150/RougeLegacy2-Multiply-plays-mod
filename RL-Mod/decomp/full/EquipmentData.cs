using UnityEngine;

public class EquipmentData : ScriptableObject
{
	public string Name;

	public int BlacksmithUIIndex;

	public EquipmentCategoryType CategoryType;

	public EquipmentType EquipmentType;

	public int ChestLevelRequirement;

	public int ChestRarityRequirement;

	public int GoldCost;

	public int OreCost;

	public bool Disabled;

	public int BaseWeight;

	public int BaseHealth;

	public int BaseMana;

	public int BaseArmor;

	public int BaseStrengthDamage;

	public int BaseMagicDamage;

	public float BaseStrengthCritChance;

	public float BaseMagicCritChance;

	public float BaseStrengthCritDamage;

	public float BaseMagicCritDamage;

	public int MaximumLevel;

	public int ScalingItemLevel;

	public int ScalingGoldCost;

	public int ScalingOreCost;

	public int ScalingWeight;

	public int ScalingHealth;

	public int ScalingMana;

	public int ScalingArmor;

	public int ScalingStrengthDamage;

	public int ScalingMagicDamage;

	public float ScalingStrengthCritChance;

	public float ScalingMagicCritChance;

	public float ScalingStrengthCritDamage;

	public float ScalingMagicCritDamage;

	public int ScalingGoldSpike01;

	public int ScalingOreSpike01;

	public int ScalingGoldSpike02;

	public int ScalingOreSpike02;

	public int BaseEquipmentSetLevel;

	public int ScalingEquipmentSetLevel;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Description;
}
