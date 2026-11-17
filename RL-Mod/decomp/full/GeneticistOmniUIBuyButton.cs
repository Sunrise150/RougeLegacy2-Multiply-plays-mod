using System;
using TMPro;
using UnityEngine;

public class GeneticistOmniUIBuyButton : OmniUIButton, IGeneticistOmniUIButton
{
	[SerializeField]
	private TMP_Text m_buyText;

	private bool m_buttonIsUpgrade;

	private GeneticistOmniUIDescriptionEventArgs m_descriptionEventArgs;

	public override EventArgs ButtonEventArgs => m_descriptionEventArgs;

	public TraitType TraitType { get; set; }

	protected override void InitializeButtonEventArgs()
	{
		if (m_descriptionEventArgs == null)
		{
			m_descriptionEventArgs = new GeneticistOmniUIDescriptionEventArgs(TraitType, isIncreasingOdds: true);
		}
		else
		{
			m_descriptionEventArgs.Initialize(TraitType, isIncreasingOdds: true);
		}
	}

	public override void OnConfirmButtonPressed()
	{
		base.OnConfirmButtonPressed();
		if (!m_buttonIsUpgrade)
		{
			if (TraitManager.CanPurchaseTrait(TraitType))
			{
				TraitManager.SetTraitFoundState(TraitType, FoundState.Purchased);
				TraitManager.SetTraitSpawnOdds(TraitType, 1, additive: false);
				InitializeButtonEventArgs();
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
		}
		else if (TraitManager.CanUpgradeTrait(TraitType))
		{
			TraitManager.SetTraitUpgradeLevel(TraitType, 1, additive: true, overrideValues: false);
			TraitManager.SetTraitSpawnOdds(TraitType, 1, additive: true);
			InitializeButtonEventArgs();
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, ButtonEventArgs);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
		}
		RunOnConfirmPressedAnimation();
	}

	public override void UpdateState()
	{
		if (TraitManager.GetTraitFoundState(TraitType) != 0)
		{
			m_buyText.text = "Buy";
			m_buttonIsUpgrade = false;
		}
		else
		{
			m_buyText.text = "Upgrade";
			m_buttonIsUpgrade = true;
		}
		if (TraitManager.GetTraitUpgradeLevel(TraitType) < TraitManager.GetTraitBlueprintLevel(TraitType))
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
			m_deselectedSprite.SetAlpha(1f);
		}
		else if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}
}
