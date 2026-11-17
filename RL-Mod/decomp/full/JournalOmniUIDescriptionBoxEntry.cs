using System.Text;
using TMPro;
using UnityEngine;

public class JournalOmniUIDescriptionBoxEntry : BaseOmniUIDescriptionBoxEntry<JournalOmniUIDescriptionEventArgs, JournalOmniUIDescriptionBoxEntry.JournalOmniUIDescriptionBoxType>
{
	public enum JournalOmniUIDescriptionBoxType
	{
		None,
		Title,
		Text
	}

	private const string CARRIAGE_RETURN_REPLACEMENT = "\n\n";

	private readonly string[] CARRIAGE_RETURN_ARRAY = new string[5] { "</CR>    ", "</CR>   ", "</CR>  ", "</CR> ", "</CR>" };

	[SerializeField]
	private TMP_Text m_journalNumberText;

	private StringBuilder m_journalStringBuilder;

	protected override void Awake()
	{
		base.Awake();
		m_journalStringBuilder = new StringBuilder();
	}

	protected override void DisplayNullDescriptionBox(MonoBehaviour sender)
	{
		base.DisplayNullDescriptionBox(sender);
		if (m_descriptionType == JournalOmniUIDescriptionBoxType.Title)
		{
			m_titleText.text = "???";
		}
		if (m_journalNumberText != null)
		{
			m_journalNumberText.text = "";
		}
	}

	protected override void DisplayDescriptionBox(JournalOmniUIDescriptionEventArgs args)
	{
		JournalCategoryType journalCategoryType = args.JournalCategoryType;
		int journalIndex = args.JournalIndex;
		JournalType journalType = args.JournalType;
		JournalEntry journalEntry = Journal_EV.GetJournalEntry(journalCategoryType, journalType, journalIndex);
		if ((bool)m_journalNumberText)
		{
			m_journalNumberText.text = (args.EntryIndex + 1).ToString();
		}
		if (!journalEntry.IsEmpty)
		{
			switch (m_descriptionType)
			{
			case JournalOmniUIDescriptionBoxType.Title:
				m_titleText.text = LocalizationManager.GetString(journalEntry.TitleLocID, isFemale: false);
				break;
			case JournalOmniUIDescriptionBoxType.Text:
			{
				m_journalStringBuilder.Clear();
				m_journalStringBuilder.Append(LocalizationManager.GetString(journalEntry.TextLocID, isFemale: false));
				for (int i = 0; i < CARRIAGE_RETURN_ARRAY.Length; i++)
				{
					m_journalStringBuilder.Replace(CARRIAGE_RETURN_ARRAY[i], "\n\n");
				}
				m_titleText.SetText(m_journalStringBuilder);
				break;
			}
			}
		}
		else
		{
			m_titleText.text = "NO JOURNAL ENTRIES EXIST IN JOURNAL CATEGORY: " + journalCategoryType;
		}
	}
}
