using UnityEngine;

public class ClassPassiveData : ScriptableObject
{
	public string ClassName;

	public float MaxHPMod;

	public float MaxManaMod;

	public float VitalityMod;

	public float StrengthMod;

	public float IntelligenceMod;

	public float DexterityMod;

	public float FocusMod;

	public float ArmorMod;

	public float WeaponCritChanceAdd;

	public float MagicCritChanceAdd;

	public float WeaponCritDamageAdd;

	public float MagicCritDamageAdd;

	public string Special;

	public ManaRegenType ManaRegenType;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Controls;

	public string Description;
}
