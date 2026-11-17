using System;
using RL_Windows;
using UnityEngine;

public class ClosetTunnel : Tunnel
{
	[NonSerialized]
	public bool IsClosetUnlocked;

	protected override void OnEnable()
	{
		base.OnEnable();
		IsClosetUnlocked = false;
	}

	protected override void OnPlayerInteractedWithTunnel(GameObject otherObj)
	{
		if (IsClosetUnlocked)
		{
			base.OnPlayerInteractedWithTunnel(otherObj);
			return;
		}
		DialogueManager.StartNewDialogue();
		DialogueManager.AddDialogue("LOC_ID_DOOR_ENDING_LOCKED_DOOR_TITLE_1", "LOC_ID_DOOR_ENDING_LOCKED_DOOR_TEXT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
	}
}
