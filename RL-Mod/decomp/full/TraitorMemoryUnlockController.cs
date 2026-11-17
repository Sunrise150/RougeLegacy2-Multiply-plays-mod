using System;
using RL_Windows;
using UnityEngine;

public class TraitorMemoryUnlockController : MonoBehaviour, IRoomConsumer, IDisplaySpeechBubble
{
	[SerializeField]
	private int m_traitorMemoryIndex = -1;

	[SerializeField]
	private ParticleSystem m_glowParticles;

	private Interactable m_interactable;

	private Action<object, RoomViaDoorEventArgs> m_onPlayerEnterRoom;

	private Action m_runConfirmMenu1;

	private Action m_runConfirmMenu2;

	private Action m_closeMemory;

	private Action m_cancelConfirmMenu;

	private Action m_unlockAllTraitorMemories;

	public BaseRoom Room { get; private set; }

	private bool NGPlusMismatchExists
	{
		get
		{
			if (SaveManager.PlayerSaveData.TimesBeatenTraitor < SaveManager.PlayerSaveData.HighestNGPlusBeaten + 1)
			{
				return SaveManager.PlayerSaveData.TimesBeatenTraitor < Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 2;
			}
			return false;
		}
	}

	public bool ShouldDisplaySpeechBubble => m_traitorMemoryIndex == -1;

	public SpeechBubbleType BubbleType => SpeechBubbleType.PointOfInterest;

	private void Awake()
	{
		m_onPlayerEnterRoom = OnPlayerEnterRoom;
		m_runConfirmMenu1 = RunConfirmMenu1;
		m_closeMemory = CloseMemory;
		m_cancelConfirmMenu = CancelConfirmMenu;
		m_runConfirmMenu2 = RunConfirmMenu2;
		m_unlockAllTraitorMemories = UnlockAllTraitorMemories;
		m_interactable = GetComponent<Interactable>();
	}

	public void SetRoom(BaseRoom room)
	{
		Room = room;
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.AddListener(m_onPlayerEnterRoom);
		}
	}

	private void OnDisable()
	{
		if ((bool)Room)
		{
			Room.PlayerEnterRelay.RemoveListener(m_onPlayerEnterRoom);
		}
	}

	private void OnPlayerEnterRoom(object sender, RoomViaDoorEventArgs args)
	{
		TraitorMemoryUnlockIndexOverride component = GetComponent<Prop>().PropSpawnController.GetComponent<TraitorMemoryUnlockIndexOverride>();
		if ((bool)component)
		{
			m_traitorMemoryIndex = component.TraitorMemoryUnlockIndex;
		}
		if (m_traitorMemoryIndex == -1)
		{
			ParticleSystem.MainModule main = m_glowParticles.main;
			main.startColor = Color.green;
			if (!NGPlusMismatchExists || SaveManager.PlayerSaveData.UnlockAllTraitorMemories)
			{
				base.gameObject.SetActive(value: false);
			}
		}
		else
		{
			ParticleSystem.MainModule main2 = m_glowParticles.main;
			main2.startColor = new Color(1f, 0.5f, 0f, 1f);
			if (m_traitorMemoryIndex >= Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 1 || m_traitorMemoryIndex >= SaveManager.PlayerSaveData.TimesBeatenTraitor)
			{
				base.gameObject.SetActive(value: false);
			}
		}
	}

	public void TriggerMemory()
	{
		if (m_traitorMemoryIndex == -1)
		{
			RunUnlockDialogue();
		}
		else
		{
			RunMemory();
		}
	}

	private void RunUnlockDialogue()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		if (!WindowManager.GetIsWindowLoaded(WindowID.Dialogue))
		{
			WindowManager.LoadWindow(WindowID.Dialogue);
		}
		DialogueManager.StartNewDialogue();
		string @string = LocalizationManager.GetString("LOC_ID_DESYNCH_CHECK_J_MEMORY_TITLE_1", isFemale: false);
		string text = string.Format(LocalizationManager.GetString("LOC_ID_DESYNCH_CHECK_J_MEMORY_TEXT_1", isFemale: false), Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 1);
		DialogueManager.AddNonLocDialogue(@string, text, disableWordScramble: false, DialogueWindowStyle.VerticalLeft);
		DialogueManager.AddDialogueCompleteEndHandler(m_runConfirmMenu1);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
	}

	private void RunConfirmMenu1()
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenuBig))
		{
			WindowManager.LoadWindow(WindowID.ConfirmMenuBig);
		}
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenuBig) as ConfirmMenuWindowController;
		int num = Mathf.Clamp(SaveManager.PlayerSaveData.HighestNGPlusBeaten, -1, Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 1);
		string text = string.Format(LocalizationManager.GetString("LOC_ID_DESYNCH_CHECK_J_MEMORY_CONFIRMATION_TITLE_1", isFemale: false), num);
		obj.SetTitleText(text, isLocID: false);
		string text2 = string.Format(LocalizationManager.GetString("LOC_ID_DESYNCH_CHECK_J_MEMORY_CONFIRMATION_TEXT_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), num);
		obj.SetDescriptionText(text2, isLocID: false);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenu);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_runConfirmMenu2);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenu);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: true);
	}

	private void RunConfirmMenu2()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenuBig) as ConfirmMenuWindowController;
		string text = string.Format(LocalizationManager.GetString("LOC_ID_DESYNCH_CHECK_J_MEMORY_CONFIRMATION_TITLE_1", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), 10);
		obj.SetTitleText(text, isLocID: false);
		string text2 = string.Format(LocalizationManager.GetString("LOC_ID_DESYNCH_CHECK_J_MEMORY_CONFIRMATION_TEXT_2", SaveManager.PlayerSaveData.CurrentCharacter.IsFemale), 10);
		obj.SetDescriptionText(text2, isLocID: false);
		obj.SetNumberOfButtons(2);
		obj.SetOnCancelAction(m_cancelConfirmMenu);
		ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
		buttonAtIndex.SetButtonText("LOC_ID_GENERAL_UI_YES_1", isLocID: true);
		buttonAtIndex.SetOnClickAction(m_unlockAllTraitorMemories);
		ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
		buttonAtIndex2.SetButtonText("LOC_ID_GENERAL_UI_NO_1", isLocID: true);
		buttonAtIndex2.SetOnClickAction(m_cancelConfirmMenu);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: true);
	}

	private void UnlockAllTraitorMemories()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
		SaveManager.PlayerSaveData.UnlockAllTraitorMemories = true;
		SaveManager.PlayerSaveData.TimesBeatenTraitor = Mathf.Min(SaveManager.PlayerSaveData.HighestNGPlusBeaten + 1, Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 2);
		GameObject gameObject = Room.gameObject.FindObjectReference("TraitorMemories");
		for (int i = 0; i < gameObject.transform.childCount; i++)
		{
			gameObject.transform.GetChild(i).GetComponent<PropSpawnController>().PropInstance.GetComponent<TraitorMemoryUnlockController>().UnlockMemory();
		}
		if (m_traitorMemoryIndex == -1)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void CancelConfirmMenu()
	{
		m_interactable.SetIsInteractableActive(isActive: true);
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenuBig, isOpen: false);
	}

	public void UnlockMemory()
	{
		if (m_traitorMemoryIndex > -1 && SaveManager.PlayerSaveData.UnlockAllTraitorMemories && m_traitorMemoryIndex < Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 1 && m_traitorMemoryIndex < SaveManager.PlayerSaveData.TimesBeatenTraitor)
		{
			base.gameObject.SetActive(value: true);
		}
	}

	private void RunMemory()
	{
		m_interactable.SetIsInteractableActive(isActive: false);
		if (m_traitorMemoryIndex < Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS.Length - 1)
		{
			string textLocID = Ending_EV.GARDEN_PREFIGHT_DIALOGUE_LOCIDS[m_traitorMemoryIndex];
			DialogueManager.StartNewDialogue();
			DialogueManager.AddDialogue("LOC_ID_NAME_J_REVEALED_1", textLocID, SaveManager.PlayerSaveData.CurrentCharacter.IsFemale);
			DialogueManager.AddDialogueCompleteEndHandler(m_closeMemory);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
		}
	}

	private void CloseMemory()
	{
		m_interactable.SetIsInteractableActive(isActive: true);
	}
}
