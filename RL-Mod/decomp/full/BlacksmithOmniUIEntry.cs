using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class BlacksmithOmniUIEntry : BaseOmniUIEntry
{
	[SerializeField]
	private TMP_Text m_maxText;

	private BlacksmithOmniUIDescriptionEventArgs m_eventArgs = new BlacksmithOmniUIDescriptionEventArgs(EquipmentCategoryType.None, EquipmentType.None, OmniUIButtonType.None);

	public EquipmentType EquipmentType { get; protected set; }

	public override EventArgs EntryEventArgs
	{
		get
		{
			EquipmentCategoryType highlightedCategory = (base.WindowController as BlacksmithOmniUIWindowController).HighlightedCategory;
			EquipmentType equipmentType = EquipmentType;
			m_eventArgs.Initialize(highlightedCategory, equipmentType, OmniUIButtonType.Purchasing);
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive
	{
		get
		{
			if (m_windowController != null)
			{
				return EquipmentManager.GetFoundState((m_windowController as BlacksmithOmniUIWindowController).HighlightedCategory, EquipmentType) != FoundState.NotFound;
			}
			return false;
		}
	}

	public void Initialize(EquipmentType equipType, BlacksmithOmniUIWindowController windowController)
	{
		EquipmentType = equipType;
		Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			((IBlacksmithOmniUIButton)buttonArray[i]).EquipmentType = EquipmentType;
		}
	}

	public override void UpdateActive()
	{
		EquipmentData equipmentData = EquipmentLibrary.GetEquipmentData((base.WindowController as BlacksmithOmniUIWindowController).HighlightedCategory, EquipmentType);
		if (equipmentData == null || equipmentData.Disabled)
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
		EquipmentCategoryType highlightedCategory = (m_windowController as BlacksmithOmniUIWindowController).HighlightedCategory;
		if (EquipmentManager.GetFoundState(highlightedCategory, EquipmentType) == FoundState.FoundButNotViewed)
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
		string text = LocalizationManager.GetString(EquipmentLibrary.GetEquipmentData(highlightedCategory, EquipmentType).Title, isFemale: false);
		if (string.IsNullOrEmpty(text) || text.Contains("LOC_ID"))
		{
			text = Equipment_EV.GetFormattedEquipmentName(highlightedCategory, EquipmentType);
		}
		int upgradeLevel = EquipmentManager.GetUpgradeLevel(highlightedCategory, EquipmentType);
		if (upgradeLevel > 0)
		{
			text = text + " +" + upgradeLevel;
		}
		m_titleText.text = text;
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			((IBlacksmithOmniUIButton)buttonArray[i]).CategoryType = highlightedCategory;
		}
		if (IsEntryActive)
		{
			m_icon.sprite = IconLibrary.GetEquipmentIcon(highlightedCategory, EquipmentType);
		}
		else
		{
			m_icon.sprite = IconLibrary.GetDefaultSprite();
		}
		base.UpdateState();
		if (!IsEntryActive)
		{
			m_titleText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_BLUEPRINT_NOT_FOUND_1", isFemale: false);
			m_maxText.gameObject.SetActive(value: false);
		}
	}

	public override void OnSelect(BaseEventData eventData)
	{
		if (base.Interactable)
		{
			EquipmentCategoryType highlightedCategory = (m_windowController as BlacksmithOmniUIWindowController).HighlightedCategory;
			if (EquipmentManager.GetFoundState(highlightedCategory, EquipmentType) == FoundState.FoundButNotViewed)
			{
				EquipmentManager.SetFoundState(highlightedCategory, EquipmentType, FoundState.FoundAndViewed, overrideValues: false);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateState, this, null);
			}
			base.OnSelect(eventData);
		}
	}

	public override void DeselectAllButtons()
	{
		base.DeselectAllButtons();
		EquipmentCategoryType highlightedCategory = (base.WindowController as BlacksmithOmniUIWindowController).HighlightedCategory;
		EquipmentType equipmentType = EquipmentType;
		m_eventArgs.Initialize(highlightedCategory, equipmentType, OmniUIButtonType.CategorySelection);
		if (IsEntryActive)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, m_eventArgs);
		}
		else
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.OmniUI_UpdateDescription, this, null);
		}
	}
}
