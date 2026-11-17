public class JournalOmniUICategoryEntry : BaseOmniUICategoryEntry
{
	public JournalCategoryType CategoryType { get; protected set; }

	public void Initialize(JournalCategoryType categoryType, int entryIndex, JournalOmniUIWindowController windowController)
	{
		CategoryType = categoryType;
		Initialize(entryIndex, windowController);
		m_iconSprite.sprite = IconLibrary.GetJournalCategoryIcon(CategoryType);
	}

	public override void UpdateState()
	{
	}
}
