public interface IJournalOmniUIButton
{
	int JournalIndex { get; set; }

	int EntryIndex { get; set; }

	JournalType JournalType { get; set; }

	JournalOmniUIWindowController JournalWindowController { get; set; }
}
