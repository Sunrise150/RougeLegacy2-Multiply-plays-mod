using UnityEngine;

[CreateAssetMenu(menuName = "Custom/XML Processing Data/Burden")]
public class BurdenData : ScriptableObject
{
	public string Name;

	public int MaxBurdenLevel;

	public int InitialBurdenCost;

	public float ScalingBurdenCost;

	public bool Disabled;

	public float StatsGain;

	public string Title;

	public string Description;

	public string Description2;

	public string Hint;
}
