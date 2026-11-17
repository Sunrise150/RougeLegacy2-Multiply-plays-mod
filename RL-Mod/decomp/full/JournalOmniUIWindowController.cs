using System;
using UnityEngine;

public class JournalOmniUIWindowController : BaseOmniUIWindowController<JournalOmniUICategoryEntry, JournalOmniUIEntry>
{
	[SerializeField]
	private ScrollBarInput_RL m_descriptionBoxScrollInput;

	[SerializeField]
	private RectTransform m_descriptionBoxContentRectTransform;

	[SerializeField]
	private GameObject m_scrollArrow;

	public GameObject ScrollArrow => m_scrollArrow;

	public ScrollBarInput_RL DescriptionBoxScrollInput => m_descriptionBoxScrollInput;

	public override WindowID ID => WindowID.Journal;

	public JournalCategoryType HighlightedCategory
	{
		get
		{
			if (base.IsInitialized && base.HighlightedCategoryIndex != -1)
			{
				if (base.ActiveCategoryEntryArray != null)
				{
					if (base.HighlightedCategoryIndex >= 0 && base.HighlightedCategoryIndex < base.ActiveCategoryEntryArray.Length)
					{
						return base.ActiveCategoryEntryArray[base.HighlightedCategoryIndex].CategoryType;
					}
					Debug.LogFormat("<color=red>| {0} | The <b>HighlightedCategoryIndex ({1})</b> is out of bounds.</color>", this, base.HighlightedCategoryIndex);
				}
				else
				{
					Debug.LogFormat("<color=red>| {0} | The <b>ActiveCategoryEntryArray</b> is null.</color>", this);
				}
			}
			return JournalCategoryType.None;
		}
	}

	protected override void CreateCategoryEntries()
	{
		if (base.CategoryEntryArray != null)
		{
			Array.Clear(base.CategoryEntryArray, 0, base.CategoryEntryArray.Length);
			base.CategoryEntryArray = null;
		}
		Array sortedCategoryTypeArray = JournalType_RL.SortedCategoryTypeArray;
		base.CategoryEntryArray = new JournalOmniUICategoryEntry[sortedCategoryTypeArray.Length - 1];
		int num = 0;
		foreach (JournalCategoryType item in sortedCategoryTypeArray)
		{
			if (item != 0)
			{
				base.CategoryEntryArray[num] = UnityEngine.Object.Instantiate(m_categoryEntryPrefab);
				base.CategoryEntryArray[num].transform.SetParent(base.CategoryEntryLayoutGroup.transform);
				base.CategoryEntryArray[num].transform.localScale = Vector3.one;
				base.CategoryEntryArray[num].Initialize(item, num, this);
				num++;
			}
		}
	}

	protected override void CreateEntries()
	{
		if (base.EntryArray != null)
		{
			Array.Clear(base.EntryArray, 0, base.EntryArray.Length);
			base.EntryArray = null;
		}
		base.EntryArray = new JournalOmniUIEntry[20];
		for (int i = 0; i < base.EntryArray.Length; i++)
		{
			base.EntryArray[i] = UnityEngine.Object.Instantiate(m_entryPrefab);
			base.EntryArray[i].transform.SetParent(base.EntryLayoutGroup.transform);
			base.EntryArray[i].transform.localScale = Vector3.one;
			base.EntryArray[i].Initialize(this);
			base.EntryArray[i].SetEntryIndex(i);
			base.EntryArray[i].gameObject.SetActive(value: false);
		}
	}

	protected override void OnOpen()
	{
		UpdateCategoryVisibility();
		base.ChooseCategoryText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_SELECT_CATEGORY_1", isFemale: false);
		base.CommonFields.DescriptionBoxRaycastBlocker.gameObject.SetActive(value: false);
		base.PurchaseBox.gameObject.SetActive(value: false);
		ScrollArrow.SetActive(value: false);
		base.OnOpen();
	}

	protected override void OnConfirmButtonJustPressed()
	{
		base.OnConfirmButtonJustPressed();
		if (!m_warningMessageVisible && !base.IsInCategories && base.ActiveEntryArray.Length == 0)
		{
			base.ChooseCategoryText.text = LocalizationManager.GetString("LOC_ID_JOURNAL_NO_JOURNALS_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		}
	}

	protected override void OnCancelButtonJustPressed()
	{
		base.OnCancelButtonJustPressed();
		if (!m_warningMessageVisible && base.IsInCategories)
		{
			base.ChooseCategoryText.text = LocalizationManager.GetString("LOC_ID_BLACKSMITH_SELECT_CATEGORY_1", isFemale: false);
		}
	}

	private void UpdateCategoryVisibility()
	{
		JournalOmniUICategoryEntry[] componentsInChildren = base.CategoryEntryLayoutGroup.GetComponentsInChildren<JournalOmniUICategoryEntry>(includeInactive: true);
		foreach (JournalOmniUICategoryEntry journalOmniUICategoryEntry in componentsInChildren)
		{
			int numJournals = Journal_EV.GetNumJournals(journalOmniUICategoryEntry.CategoryType, JournalType.Journal);
			int numJournals2 = Journal_EV.GetNumJournals(journalOmniUICategoryEntry.CategoryType, JournalType.MemoryFragment);
			int journalsRead = SaveManager.PlayerSaveData.GetJournalsRead(journalOmniUICategoryEntry.CategoryType, JournalType.Journal);
			int journalsRead2 = SaveManager.PlayerSaveData.GetJournalsRead(journalOmniUICategoryEntry.CategoryType, JournalType.MemoryFragment);
			if ((numJournals == 0 || journalsRead == 0) && (numJournals2 == 0 || journalsRead2 == 0))
			{
				if (journalOmniUICategoryEntry.gameObject.activeSelf)
				{
					journalOmniUICategoryEntry.gameObject.SetActive(value: false);
				}
			}
			else if (!journalOmniUICategoryEntry.gameObject.activeSelf)
			{
				journalOmniUICategoryEntry.gameObject.SetActive(value: true);
			}
		}
		base.ActiveCategoryEntryArray = base.CategoryEntryLayoutGroup.GetComponentsInChildren<JournalOmniUICategoryEntry>(includeInactive: false);
	}

	public void ResetDescriptionScrollBar()
	{
		m_descriptionBoxScrollInput.ResetScrollBar();
	}
}
