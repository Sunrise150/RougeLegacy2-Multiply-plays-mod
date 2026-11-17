using UnityEngine;

public class RelicData : ScriptableObject
{
	public string Name;

	public int Rarity;

	public int MaxStack;

	public RelicCostType CostType;

	public float CostAmount;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Description;

	public string Description02;
}
