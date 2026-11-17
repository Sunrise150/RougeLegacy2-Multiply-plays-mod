using UnityEngine;

public class ClassStatsData : ScriptableObject
{
	public string ClassName;

	public float HealthMod;

	public float ManaMod;

	public float StrengthDamageMod;

	public float MagicDamageMod;

	public float StrengthCritChanceMod;

	public float MagicCritChanceMod;

	public float CooldownMod;

	public float ManaRegenTimeMod;

	public float ManaRegenDamageMod;

	public float ManaRegenKillMod;

	public float ArmorEquipMod;

	public float RuneEquipMod;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Controls;

	public string Description;
}
