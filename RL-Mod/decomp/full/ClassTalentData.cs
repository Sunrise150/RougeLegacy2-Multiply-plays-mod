using UnityEngine;

public class ClassTalentData : ScriptableObject
{
	public string ClassName;

	public AbilityType[] TalentAbilityArray;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Controls;

	public string Description;
}
