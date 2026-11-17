public struct HeirloomTextEntry
{
	public string DialogueTitleLocID;

	public string DialogueTextLocID;

	public HeirloomTextEntry(string titleLocID, string textLocID)
	{
		DialogueTitleLocID = titleLocID;
		DialogueTextLocID = textLocID;
	}
}
