using UnityEngine;

public class JournalEntryOverride : MonoBehaviour
{
	[SerializeField]
	private string m_titleLocIDOverride;

	[SerializeField]
	private string m_entryLocIDOverride;

	[SerializeField]
	private JournalType m_journalType;

	[SerializeField]
	private JournalCategoryType m_journalCategoryTypeOverride;

	[SerializeField]
	private DialogueWindowStyle m_dialogueWindowStyle = DialogueWindowStyle.VerticalRight;

	[SerializeField]
	private DialoguePortraitType m_portraitType;

	public string TitleLocIDOverride => m_titleLocIDOverride;

	public string EntryLocIDOverride => m_entryLocIDOverride;

	public JournalType JournalType => m_journalType;

	public JournalCategoryType JournalCategoryTypeOverride => m_journalCategoryTypeOverride;

	public DialogueWindowStyle DialogueWindowStyle => m_dialogueWindowStyle;

	public DialoguePortraitType PortraitType => m_portraitType;
}
