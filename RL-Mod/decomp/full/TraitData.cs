using UnityEngine;

public class TraitData : ScriptableObject
{
	public string Name;

	public int Rarity;

	public float GoldBonus;

	[Space]
	[Header("Text Fields")]
	[Space]
	public string Title;

	public string Description;

	public string Description_2;

	public string GetTraitTitleLocID()
	{
		if (!SaveManager.ConfigData.UseNonScientificNames)
		{
			return Title;
		}
		return Title.Replace("_1", "_2");
	}
}
