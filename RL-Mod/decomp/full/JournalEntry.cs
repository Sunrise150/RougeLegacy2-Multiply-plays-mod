public struct JournalEntry
{
	public string TitleLocID { get; private set; }

	public string TextLocID { get; private set; }

	public bool IsEmpty
	{
		get
		{
			if (!string.IsNullOrEmpty(TitleLocID))
			{
				return string.IsNullOrEmpty(TextLocID);
			}
			return true;
		}
	}

	public JournalEntry(string title, string text)
	{
		TitleLocID = title;
		TextLocID = text;
	}
}
