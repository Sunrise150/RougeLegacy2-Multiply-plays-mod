using UnityEngine;

public class GeneticistOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<GeneticistOmniUIDescriptionEventArgs, GeneticistOmniUIDescriptionBoxEntry.GeneticistOmniUIDescriptionBoxType>
{
	public enum GeneticistOmniUIDescriptionBoxType
	{
		None,
		Title,
		Description,
		Stat
	}

	protected override void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		base.DisplayNullDescriptionBox(sender);
		if (m_descriptionType == GeneticistOmniUIDescriptionBoxType.Title)
		{
			m_titleText.text = "???";
		}
	}

	protected override void DisplayDescriptionBox(GeneticistOmniUIDescriptionEventArgs args)
	{
		TraitData traitData = TraitLibrary.GetTraitData(args.TraitType);
		switch (m_descriptionType)
		{
		case GeneticistOmniUIDescriptionBoxType.Title:
			m_titleText.text = LocalizationManager.GetString(traitData.GetTraitTitleLocID(), isFemale: false);
			break;
		case GeneticistOmniUIDescriptionBoxType.Description:
			m_titleText.text = LocalizationManager.GetString(traitData.GetTraitTitleLocID(), isFemale: false);
			break;
		}
	}
}
