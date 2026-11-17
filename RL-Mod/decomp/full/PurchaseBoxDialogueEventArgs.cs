using System;

public class PurchaseBoxDialogueEventArgs : EventArgs
{
	public PurchaseBoxDialogueType DialogueType { get; private set; }

	public PurchaseBoxDialogueEventArgs(PurchaseBoxDialogueType dialogueType)
	{
		Initialize(dialogueType);
	}

	public void Initialize(PurchaseBoxDialogueType dialogueType)
	{
		DialogueType = dialogueType;
	}
}
