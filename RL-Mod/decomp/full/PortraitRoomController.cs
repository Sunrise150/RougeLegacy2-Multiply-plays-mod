using System.Collections.Generic;
using RL_Windows;
using UnityEngine;

public class PortraitRoomController : BaseSpecialRoomController
{
	[SerializeField]
	private PortraitType m_portraitType;

	public void TriggerPortrait()
	{
		if (m_portraitType == PortraitType.Random)
		{
			List<PortraitType> list = new List<PortraitType>();
			PortraitType[] typeArray = PortraitType_RL.TypeArray;
			foreach (PortraitType portraitType in typeArray)
			{
				if (portraitType != 0 && portraitType != PortraitType.Random)
				{
					list.Add(portraitType);
				}
			}
			m_portraitType = list[Random.Range(0, list.Count)];
		}
		DialogueManager.StartNewDialogue();
		switch (m_portraitType)
		{
		case PortraitType.Story1:
			DialogueManager.AddNonLocDialogue("Narrator", "This is the first story.");
			break;
		case PortraitType.Story2:
			DialogueManager.AddNonLocDialogue("Narrator", "This is the second story.");
			break;
		}
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		RoomCompleted();
	}
}
