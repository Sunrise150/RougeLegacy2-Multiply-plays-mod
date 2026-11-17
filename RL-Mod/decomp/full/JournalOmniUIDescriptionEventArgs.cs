using System;

public class JournalOmniUIDescriptionEventArgs : EventArgs
{
	public JournalCategoryType JournalCategoryType { get; private set; }

	public int JournalIndex { get; private set; }

	public JournalType JournalType { get; private set; }

	public int EntryIndex { get; private set; }

	public JournalOmniUIDescriptionEventArgs(int entryIndex, JournalCategoryType categoryType, int journalIndex, JournalType journalType)
	{
		Initialize(entryIndex, categoryType, journalIndex, journalType);
	}

	public void Initialize(int entryIndex, JournalCategoryType categoryType, int journalIndex, JournalType journalType)
	{
		EntryIndex = entryIndex;
		JournalIndex = journalIndex;
		JournalType = journalType;
		JournalCategoryType = categoryType;
	}
}
