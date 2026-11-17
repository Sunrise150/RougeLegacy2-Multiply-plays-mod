using UnityEngine;

public class SoulShopData : ScriptableObject
{
	public string Name;

	public bool Disabled;

	public float FirstLevelStatGain;

	public float AdditionalLevelStatGain;

	public int Position;

	public int BaseCost;

	public int ScalingCost;

	public int MaxLevel;

	public int UnlockLevel;

	public int OverloadMaxLevel;

	public int MaxLevelScalingCap;

	public int MaxSoulCostCap;

	public int TotalCostCapFalseOverloadFalse;

	public int TotalCostCapTrueOverloadFalse;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Description;

	public string StatsTitle;
}
