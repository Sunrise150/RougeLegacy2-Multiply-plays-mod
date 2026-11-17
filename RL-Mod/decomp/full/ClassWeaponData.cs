using UnityEngine;

public class ClassWeaponData : ScriptableObject
{
	public string ClassName;

	public AbilityType[] WeaponAbilityArray;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Controls;

	public string Description;
}
