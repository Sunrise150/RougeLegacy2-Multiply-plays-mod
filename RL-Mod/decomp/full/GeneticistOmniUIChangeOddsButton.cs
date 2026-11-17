using System;
using TMPro;
using UnityEngine;

public class GeneticistOmniUIChangeOddsButton : OmniUIButton, IGeneticistOmniUIButton
{
	[SerializeField]
	private bool m_isRightArrow;

	[SerializeField]
	private TMP_Text m_oddsText;

	private GeneticistOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public TraitType TraitType { get; set; }

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new GeneticistOmniUIDescriptionEventArgs(TraitType, m_isRightArrow);
		}
		else
		{
			m_descriptionEventArgs.Initialize(TraitType, m_isRightArrow);
		}
	}

	public override void UpdateState()
	{
		TraitSpawnOdds traitSpawnOdds = TraitManager.GetTraitSpawnOdds(TraitType);
		int traitUpgradeLevel = TraitManager.GetTraitUpgradeLevel(TraitType);
		if (m_isRightArrow)
		{
			if ((int)traitSpawnOdds < traitUpgradeLevel)
			{
				m_deselectedSprite.SetAlpha(1f);
				IsButtonActive = true;
			}
			else
			{
				m_deselectedSprite.SetAlpha(0.25f);
				IsButtonActive = false;
			}
		}
		else if (traitSpawnOdds > TraitSpawnOdds.Common)
		{
			m_deselectedSprite.SetAlpha(1f);
			IsButtonActive = true;
		}
		else
		{
			m_deselectedSprite.SetAlpha(0.25f);
			IsButtonActive = false;
		}
		switch (traitSpawnOdds)
		{
		case TraitSpawnOdds.Common:
			m_oddsText.text = "[Common]";
			break;
		case TraitSpawnOdds.Rare:
			m_oddsText.text = "[Rare]";
			break;
		case TraitSpawnOdds.VeryRare:
			m_oddsText.text = "[Very Rare]";
			break;
		case TraitSpawnOdds.Never:
			m_oddsText.text = "[Never]";
			break;
		}
	}

	public override void OnConfirmButtonPressed()
	{
		if (m_isRightArrow)
		{
			TraitManager.SetTraitSpawnOdds(TraitType, 1, additive: true);
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			RunOnConfirmPressedAnimation();
		}
		else
		{
			TraitManager.SetTraitSpawnOdds(TraitType, -1, additive: true);
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			RunOnConfirmPressedAnimation();
		}
		base.OnConfirmButtonPressed();
	}
}
