using UnityEngine;

public class RuneData : ScriptableObject
{
	public RuneType RuneType;

	public int Location;

	public int BaseItemLevel;

	public int GoldCost;

	public int BlackStoneCost;

	public bool Disabled;

	public int BaseWeight;

	public float StatMod01;

	public float StatMod02;

	public float StatMod03;

	public int MaximumLevel;

	public int ScalingItemLevel;

	public int ScalingGoldCost;

	public int ScalingBlackStoneCost;

	public int ScalingWeight;

	public float ScalingStatMod01;

	public float ScalingStatMod02;

	public float ScalingStatMod03;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Description;

	public string Controls;
}
