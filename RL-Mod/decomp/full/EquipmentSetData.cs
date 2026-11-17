using UnityEngine;

public class EquipmentSetData : ScriptableObject
{
	public string Name;

	[Space]
	[Header("Bonus(es)")]
	[Space]
	public EquipmentSetBonus SetBonus01;

	public int SetRequirement01;

	public EquipmentSetBonus SetBonus02;

	public int SetRequirement02;

	public EquipmentSetBonus SetBonus03;

	public int SetRequirement03;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Description;
}
