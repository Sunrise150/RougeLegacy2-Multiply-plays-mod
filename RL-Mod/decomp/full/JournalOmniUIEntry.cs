using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class JournalOmniUIEntry : BaseOmniUIEntry
{
	[SerializeField]
	private TMP_Text m_journalNumber;

	private JournalOmniUIDescriptionEventArgs m_eventArgs;

	public int JournalIndex { get; private set; }

	public JournalType JournalType { get; private set; }

	public override EventArgs EntryEventArgs
	{
		get
		{
			if (m_eventArgs == null)
			{
				m_eventArgs = new JournalOmniUIDescriptionEventArgs(base.EntryIndex, (m_windowController as JournalOmniUIWindowController).HighlightedCategory, JournalIndex, JournalType);
			}
			else
			{
				m_eventArgs.Initialize(base.EntryIndex, (m_windowController as JournalOmniUIWindowController).HighlightedCategory, JournalIndex, JournalType);
			}
			return m_eventArgs;
		}
	}

	public override bool IsEntryActive
	{
		get
		{
			JournalCategoryType highlightedCategory = (m_windowController as JournalOmniUIWindowController).HighlightedCategory;
			if (highlightedCategory == JournalCategoryType.None)
			{
				return false;
			}
			int numJournals = Journal_EV.GetNumJournals(highlightedCategory, JournalType.Journal);
			int numJournals2 = Journal_EV.GetNumJournals(highlightedCategory, JournalType.MemoryFragment);
			if (JournalType == JournalType.Journal && JournalIndex >= numJournals)
			{
				return false;
			}
			if (JournalType == JournalType.MemoryFragment && JournalIndex >= numJournals2)
			{
				return false;
			}
			if (SaveManager.PlayerSaveData.GetJournalsRead(highlightedCategory, JournalType) > JournalIndex)
			{
				return true;
			}
			return false;
		}
	}

	private void UpdateJournalTypeAndIndex()
	{
		JournalCategoryType highlightedCategory = (m_windowController as JournalOmniUIWindowController).HighlightedCategory;
		if (highlightedCategory != 0)
		{
			BiomeType biomeType = JournalType_RL.ConvertJournalCategoryTypeToBiome(highlightedCategory);
			int numJournals = Journal_EV.GetNumJournals(biomeType, JournalType.Journal);
			if (base.EntryIndex >= numJournals)
			{
				JournalType = JournalType.MemoryFragment;
				JournalIndex = base.EntryIndex - numJournals;
			}
			else
			{
				JournalType = JournalType.Journal;
				JournalIndex = base.EntryIndex;
			}
			OmniUIButton[] buttonArray = m_buttonArray;
			for (int i = 0; i < buttonArray.Length; i++)
			{
				IJournalOmniUIButton obj = (IJournalOmniUIButton)buttonArray[i];
				obj.EntryIndex = base.EntryIndex;
				obj.JournalIndex = JournalIndex;
				obj.JournalType = JournalType;
			}
			string arg = highlightedCategory.ToString();
			if (biomeType != 0)
			{
				arg = LocalizationManager.GetString(BiomeDataLibrary.GetData(biomeType).BiomeNameLocID, isFemale: false);
			}
			if (JournalType == JournalType.MemoryFragment)
			{
				m_titleText.text = string.Format(LocalizationManager.GetString("LOC_ID_JOURNAL_MEMORY_TITLE_1", isFemale: false), arg);
			}
			else
			{
				m_titleText.text = string.Format(LocalizationManager.GetString("LOC_ID_JOURNAL_JOURNAL_TITLE_1", isFemale: false), arg);
			}
		}
	}

	public override void UpdateActive()
	{
		JournalCategoryType highlightedCategory = (m_windowController as JournalOmniUIWindowController).HighlightedCategory;
		if (highlightedCategory == JournalCategoryType.None)
		{
			return;
		}
		int numJournals = Journal_EV.GetNumJournals(highlightedCategory, JournalType.Journal);
		int numJournals2 = Journal_EV.GetNumJournals(highlightedCategory, JournalType.MemoryFragment);
		if (base.EntryIndex < numJournals + numJournals2)
		{
			if (!base.gameObject.activeSelf)
			{
				base.gameObject.SetActive(value: true);
			}
		}
		else if (base.gameObject.activeSelf)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	public override void Initialize(IOmniUIWindowController windowController)
	{
		base.Initialize(windowController);
		OmniUIButton[] buttonArray = m_buttonArray;
		for (int i = 0; i < buttonArray.Length; i++)
		{
			IJournalOmniUIButton obj = (IJournalOmniUIButton)buttonArray[i];
			obj.JournalIndex = JournalIndex;
			obj.EntryIndex = base.EntryIndex;
			obj.JournalType = JournalType;
			obj.JournalWindowController = (JournalOmniUIWindowController)windowController;
		}
		m_titleText.text = "";
		if (m_newSymbol.gameObject.activeSelf)
		{
			m_newSymbol.gameObject.SetActive(value: false);
		}
	}

	public override void SetEntryIndex(int index)
	{
		base.SetEntryIndex(index);
		m_journalNumber.text = (index + 1).ToString();
		UpdateJournalTypeAndIndex();
	}

	public override void OnSelect(BaseEventData eventData)
	{
		(m_windowController as JournalOmniUIWindowController).ResetDescriptionScrollBar();
		base.OnSelect(eventData);
	}
}
