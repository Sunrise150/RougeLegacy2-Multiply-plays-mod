using System;
using UnityEngine.EventSystems;

public class GeneticistOmniUIEntry : BaseOmniUIEntry
{
	private GeneticistOmniUIDescriptionEventArgs m_eventArgs;

	public TraitType TraitType { get; protected set; }

	public override EventArgs EntryEventArgs
	{
		get
		{
			if (m_eventArgs == null)
			{
				m_eventArgs = new GeneticistOmniUIDescriptionEventArgs(TraitType, isIncreasingOdds: true);
			}
			else
			{
				m_eventArgs.Initialize(TraitType, isIncreasingOdds: true);
			}
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive => TraitManager.GetTraitFoundState(TraitType) != FoundState.NotFound;

	public void Initialize(TraitType traitType, GeneticistOmniUIWindowController windowController)
	{
		TraitType = traitType;
		Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			((IGeneticistOmniUIButton)buttonArray[i]).TraitType = TraitType;
		}
		string @string = LocalizationManager.GetString(TraitLibrary.GetTraitData(TraitType).GetTraitTitleLocID(), isFemale: false);
		m_titleText.text = @string;
	}

	public override void UpdateActive()
	{
	}

	public override void UpdateState()
	{
		if (TraitManager.GetTraitFoundState(TraitType) == FoundState.FoundButNotViewed)
		{
			if (!m_newSymbol.gameObject.activeSelf)
			{
				m_newSymbol.gameObject.SetActive(value: true);
			}
		}
		else if (m_newSymbol.gameObject.activeSelf)
		{
			m_newSymbol.gameObject.SetActive(value: false);
		}
		base.UpdateState();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (base.Interactable)
		{
			if (TraitManager.GetTraitFoundState(TraitType) == FoundState.FoundButNotViewed)
			{
				TraitManager.SetTraitFoundState(TraitType, FoundState.FoundAndViewed);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
			base.OnSelect(eventData);
		}
	}
}
