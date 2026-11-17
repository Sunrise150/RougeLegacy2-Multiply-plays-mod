using UnityEngine;

public class SkillTreeData : ScriptableObject
{
	public string Name;

	public float FirstLevelStatGain;

	public float AdditionalLevelStatGain;

	public int BaseCost;

	public int Appreciation;

	public int MaxLevel;

	public int OverloadLevelCap;

	public int AdditiveSoulShopLevels;

	public bool DisplayBaseStat;

	public SkillUnlockState SkillUnlockState;

	public int SkillUnlockLevel;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string UnitOfMeasurement;

	public string StatTitle;

	public string Description;

	public string SoulShopTag;
}
