using System;
using System.Collections.Generic;
using System.Linq;
using RL_Windows;

public class DialogueManager
{
	private static DialogueManager m_instance;

	private const string CARRIAGE_RETURN = "</CR>";

	private const string LINE_BREAK = "</LB>";

	private LinkedList<DialogueObj> m_dialogueObjLinkedList;

	private List<string> m_carriageParsedStrings;

	private NPCController m_npcController;

	private NPCState m_onCompleteNPCState;

	private bool m_isNPCDialogue;

	private string m_lastSpeakerLocID;

	private string m_lastTextLocID;

	private bool m_lastIsFemale;

	public static DialogueManager Instance
	{
		get
		{
			if (m_instance == null)
			{
				m_instance = new DialogueManager();
				m_instance.Initialize();
			}
			return m_instance;
		}
	}

	public static NPCState OnCompleteNPCState => Instance.m_onCompleteNPCState;

	public static NPCController NPCController => Instance.m_npcController;

	public static bool IsNPCDialogue => Instance.m_isNPCDialogue;

	public static string LastSpeakerLocID => Instance.m_lastSpeakerLocID;

	public static string LastTextLocID => Instance.m_lastTextLocID;

	public static bool LastIsFemaleState => Instance.m_lastIsFemale;

	public static LinkedList<DialogueObj> DialogueObjLinkedList => Instance.m_dialogueObjLinkedList;

	public static LinkedListNode<DialogueObj> GetFirstDialogueNode => Instance.m_dialogueObjLinkedList.First;

	public static LinkedListNode<DialogueObj> GetLastDialogueNode => Instance.m_dialogueObjLinkedList.Last;

	private void Initialize()
	{
		m_dialogueObjLinkedList = new LinkedList<DialogueObj>();
		m_carriageParsedStrings = new List<string>();
	}

	private void ForceRefreshText_Internal()
	{
		if (string.IsNullOrEmpty(LastTextLocID))
		{
			return;
		}
		DialogueObj dialogueObj = m_dialogueObjLinkedList.First();
		DialoguePortraitType portraitType = dialogueObj.PortraitType;
		float typewriterSpeed = dialogueObj.typewriterSpeed;
		DialogueWindowStyle windowStyle = dialogueObj.WindowStyle;
		NPCState nPCState = dialogueObj.NPCState;
		m_dialogueObjLinkedList.Clear();
		string @string = LocalizationManager.GetString(LastSpeakerLocID, LastIsFemaleState);
		string string2 = LocalizationManager.GetString(LastTextLocID, LastIsFemaleState);
		ParseCarriageReturns(string2);
		foreach (string carriageParsedString in m_carriageParsedStrings)
		{
			DialogueObj value = default(DialogueObj);
			value.speaker = @string;
			value.text = carriageParsedString;
			value.PortraitType = portraitType;
			value.typewriterSpeed = typewriterSpeed;
			value.WindowStyle = windowStyle;
			value.NPCState = nPCState;
			m_dialogueObjLinkedList.AddLast(value);
		}
	}

	private void AddDialogue_Internal(string speakerLocID, string textLocID, bool isFemale, DialogueWindowStyle windowStyle, DialoguePortraitType portraitType, NPCState npcState, float typewriteSpeed)
	{
		string @string = LocalizationManager.GetString(textLocID, isFemale);
		string string2 = LocalizationManager.GetString(speakerLocID, isFemale);
		ParseCarriageReturns(@string);
		m_lastSpeakerLocID = speakerLocID;
		m_lastTextLocID = textLocID;
		m_lastIsFemale = isFemale;
		foreach (string carriageParsedString in m_carriageParsedStrings)
		{
			DialogueObj value = default(DialogueObj);
			value.speaker = string2;
			value.text = carriageParsedString;
			value.PortraitType = portraitType;
			value.typewriterSpeed = typewriteSpeed;
			value.WindowStyle = windowStyle;
			value.NPCState = npcState;
			m_dialogueObjLinkedList.AddLast(value);
		}
	}

	private void AddNonLocDialogue_Internal(string speakerTitle, string text, bool disableWordScramble, DialogueWindowStyle windowStyle, DialoguePortraitType portraitType, NPCState npcState, float typewriteSpeed)
	{
		text = text.Replace("</LB>", "\n");
		ParseCarriageReturns(text);
		m_lastSpeakerLocID = null;
		m_lastTextLocID = null;
		m_lastIsFemale = false;
		foreach (string carriageParsedString in m_carriageParsedStrings)
		{
			DialogueObj value = default(DialogueObj);
			string text2 = carriageParsedString;
			value.speaker = speakerTitle;
			value.text = text2;
			value.PortraitType = portraitType;
			value.typewriterSpeed = typewriteSpeed;
			value.WindowStyle = windowStyle;
			value.NPCState = npcState;
			m_dialogueObjLinkedList.AddLast(value);
		}
	}

	private void ClearLinkedList()
	{
		m_dialogueObjLinkedList.Clear();
	}

	private void ParseCarriageReturns(string text)
	{
		m_carriageParsedStrings.Clear();
		int length = "</CR>".Length;
		int num = 0;
		while (num != -1)
		{
			num = text.IndexOf("</CR>");
			if (num != -1)
			{
				string item = text.Substring(0, num);
				m_carriageParsedStrings.Add(item);
				text = text.Substring(num + length).TrimStart();
			}
		}
		m_carriageParsedStrings.Add(text);
	}

	public static LinkedListNode<DialogueObj> GetDialogueNodeAtIndex(int index)
	{
		if (index >= Instance.m_dialogueObjLinkedList.Count)
		{
			return null;
		}
		LinkedListNode<DialogueObj> linkedListNode = GetFirstDialogueNode;
		for (int i = 0; i < index; i++)
		{
			linkedListNode = linkedListNode.Next;
		}
		return linkedListNode;
	}

	public static void StartNewNPCDialogue(NPCController npcController, NPCState onCompleteNPCState = NPCState.Idle)
	{
		StartNewDialogue(npcController, onCompleteNPCState);
		Instance.m_isNPCDialogue = true;
	}

	public static void StartNewDialogue(NPCController npcController = null, NPCState onCompleteNPCState = NPCState.Idle)
	{
		if (!WindowManager.GetIsWindowLoaded(WindowID.Dialogue))
		{
			WindowManager.LoadWindow(WindowID.Dialogue);
		}
		Instance.m_npcController = npcController;
		Instance.m_onCompleteNPCState = onCompleteNPCState;
		Instance.ClearLinkedList();
		Instance.m_isNPCDialogue = false;
	}

	public static void UpdateNPCDialogueState()
	{
		if ((bool)Instance.m_npcController)
		{
			NPCDialogueManager.MarkNPCAsSpoken(Instance.m_npcController.NPCType, Instance.m_isNPCDialogue, Instance.m_npcController);
		}
		Instance.m_npcController = null;
		Instance.m_isNPCDialogue = false;
	}

	public static void AddDialogue(string speakerLocID, string textLocID, bool isFemale, DialogueWindowStyle windowStyle = DialogueWindowStyle.HorizontalUpper, DialoguePortraitType portraitType = DialoguePortraitType.None, NPCState npcState = NPCState.None, float typewriterSpeed = 0.015f)
	{
		Instance.AddDialogue_Internal(speakerLocID, textLocID, isFemale, windowStyle, portraitType, npcState, typewriterSpeed);
	}

	public static void AddNonLocDialogue(string speaker, string text, bool disableWordScramble = false, DialogueWindowStyle windowStyle = DialogueWindowStyle.HorizontalUpper, DialoguePortraitType portraitType = DialoguePortraitType.None, NPCState npcState = NPCState.None, float typeWriterSpeed = 0.015f)
	{
		Instance.AddNonLocDialogue_Internal(speaker, text, disableWordScramble, windowStyle, portraitType, npcState, typeWriterSpeed);
	}

	public static void AddDialogueCompleteEndHandler(Action action)
	{
		(WindowManager.GetWindowController(WindowID.Dialogue) as DialogueWindowController).AddEndHandler(action);
	}

	public static void ForceRefreshText()
	{
		Instance.ForceRefreshText_Internal();
	}
}
