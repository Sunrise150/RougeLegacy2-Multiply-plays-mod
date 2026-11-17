using System;
using UnityEngine.EventSystems;

public class EnchantressOmniUIEntry : BaseOmniUIEntry
{
	private EnchantressOmniUIDescriptionEventArgs m_eventArgs;

	public RuneType RuneType { get; protected set; }

	public override EventArgs EntryEventArgs
	{
		get
		{
			if (m_eventArgs == null)
			{
				m_eventArgs = new EnchantressOmniUIDescriptionEventArgs(RuneType, OmniUIButtonType.Purchasing);
			}
			else
			{
				m_eventArgs.Initialize(RuneType, OmniUIButtonType.Purchasing);
			}
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive => RuneManager.GetFoundState(RuneType) != FoundState.NotFound;

	public void Initialize(RuneType runeType, EnchantressOmniUIWindowController windowController)
	{
		RuneType = runeType;
		Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			((IEnchantressOmniUIButton)buttonArray[i]).RuneType = RuneType;
		}
		string @string = LocalizationManager.GetString(RuneLibrary.GetRuneData(RuneType).Title, isFemale: false);
		m_titleText.text = @string;
	}

	public override void UpdateActive()
	{
		RuneData runeData = RuneLibrary.GetRuneData(RuneType);
		if (runeData == null || runeData.Disabled)
		{
			if (base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else if (!base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	public override void UpdateState()
	{
		if (RuneManager.GetFoundState(RuneType) == FoundState.FoundButNotViewed)
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
		if (IsEntryActive)
		{
			m_icon.sprite = IconLibrary.GetRuneIcon(RuneType);
		}
		else
		{
			m_icon.sprite = IconLibrary.GetDefaultSprite();
		}
		base.UpdateState();
		if (!IsEntryActive)
		{
			m_titleText.text = LocalizationManager.GetString("LOC_ID_ENCHANTRESS_RUNE_NOT_FOUND_1", isFemale: false);
			return;
		}
		string @string = LocalizationManager.GetString(RuneLibrary.GetRuneData(RuneType).Title, isFemale: false);
		m_titleText.text = @string;
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (base.Interactable)
		{
			if (RuneManager.GetFoundState(RuneType) == FoundState.FoundButNotViewed)
			{
				RuneManager.SetFoundState(RuneType, FoundState.FoundAndViewed, overrideValues: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
			base.OnSelect(eventData);
		}
	}
}
