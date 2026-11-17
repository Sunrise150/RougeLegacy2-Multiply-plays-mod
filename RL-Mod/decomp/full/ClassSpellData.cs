using UnityEngine;

public class ClassSpellData : ScriptableObject
{
	public string ClassName;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Controls;

	public string Description;

	public AbilityType[] SpellAbilityArray;
}
