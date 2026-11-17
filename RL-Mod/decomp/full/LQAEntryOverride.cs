using System;
using System.IO;
using System.Text;
using RL_Windows;
using UnityEngine;

public class LQAEntryOverride : MonoBehaviour
{
	private enum LQADisplayType
	{
		None,
		DialogueWindow,
		ConfirmWindow,
		ObjectiveComplete
	}

	private const string FILE_NAME = "LocIDTester_V2.txt";

	private static readonly string[] TESTER_INSTRUCTIONS = new string[20]
	{
		"// Change the lines below to test custom LocIDs.", "", "// Dialogue Window", "// WindowStyle can be: HorizontalUpper, HorizontalLower, VerticalLeft, or VerticalRight.", "Dialogue_TitleLocID=", "Dialogue_BodyLocID=", "Dialogue_DisplayFemale=false", "Dialogue_WindowStyle=HorizontalUpper", "", "// Confirm Window",
		"Confirm_TitleLocID=", "Confirm_BodyLocID=", "Confirm_Button1LocID=", "Confirm_Button2LocID=", "", "// Objective Complete Window", "ObjComplete_TitleLocID=", "ObjComplete_SubTitleLocID=", "ObjComplete_BodyLocID=", "ObjComplete_DisplayPlayer=false"
	};

	[SerializeField]
	private LQADisplayType m_displayType;

	private string m_dialogueTitleLocID;

	private string m_dialogueBodyLocID;

	private bool m_dialogueIsFemale;

	private DialogueWindowStyle m_dialogueWindowStyle;

	private string m_confirmTitleLocID;

	private string m_confirmBodyLocID;

	private string m_button1LocID;

	private string m_button2LocID;

	private string m_objCompleteTitleLocID;

	private string m_objCompleteSubTitleLocID;

	private string m_objCompleteBodyLocID;

	private bool m_objCompleteDisplayPlayer;

	private Action m_closeConfirmWindow;

	private bool m_dataLoadedSuccessfully;

	private void Awake()
	{
		InitializeDirectoryAndFile();
		m_closeConfirmWindow = CloseConfirmWindow;
	}

	public static void InitializeDirectoryAndFile()
	{
		string text = Path.Combine(Application.persistentDataPath, "CustomData");
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
				Debug.Log("<color=green>Created CustomData directory</color>");
			}
			catch (Exception ex)
			{
				Debug.Log("<color=red>CustomData directory does not exist and could not be created.</color> " + ex);
				return;
			}
		}
		text = Path.Combine(text, StoreAPIManager.GetPlatformDirectoryName());
		if (!Directory.Exists(text))
		{
			try
			{
				Directory.CreateDirectory(text);
				Debug.Log("<color=green>Created CustomData/Platform subdirectory</color>");
			}
			catch (Exception ex2)
			{
				Debug.Log("<color=red>CustomData/Platform subdirectory does not exist and could not be created.</color> " + ex2);
				return;
			}
		}
		string path = Path.Combine(text, "LocIDTester_V2.txt");
		if (File.Exists(path))
		{
			return;
		}
		using StreamWriter streamWriter = File.CreateText(path);
		for (int i = 0; i < TESTER_INSTRUCTIONS.Length; i++)
		{
			streamWriter.WriteLine(TESTER_INSTRUCTIONS[i]);
		}
	}

	private void LoadCustomFile()
	{
		m_dataLoadedSuccessfully = true;
		string path = Path.Combine(Path.Combine(Path.Combine(Application.persistentDataPath, "CustomData"), StoreAPIManager.GetPlatformDirectoryName()), "LocIDTester_V2.txt");
		try
		{
			byte[] bytes = File.ReadAllBytes(path);
			using StringReader stringReader = new StringReader(Encoding.ASCII.GetString(bytes));
			string text = null;
			while ((text = stringReader.ReadLine()) != null)
			{
				int num = text.IndexOf("=");
				if (num == -1)
				{
					continue;
				}
				string text2 = text.Substring(0, num);
				string text3 = text.Substring(num + 1);
				if (!string.IsNullOrEmpty(text3))
				{
					switch (text2)
					{
					case "Dialogue_TitleLocID":
						m_dialogueTitleLocID = text3.Trim();
						break;
					case "Dialogue_BodyLocID":
						m_dialogueBodyLocID = text3.Trim();
						break;
					case "Dialogue_DisplayFemale":
						m_dialogueIsFemale = bool.Parse(text3.Trim());
						break;
					case "Dialogue_WindowStyle":
						m_dialogueWindowStyle = (DialogueWindowStyle)Enum.Parse(typeof(DialogueWindowStyle), text3.Trim());
						break;
					case "Confirm_TitleLocID":
						m_confirmTitleLocID = text3.Trim();
						break;
					case "Confirm_BodyLocID":
						m_confirmBodyLocID = text3.Trim();
						break;
					case "Confirm_Button1LocID":
						m_button1LocID = text3.Trim();
						break;
					case "Confirm_Button2LocID":
						m_button2LocID = text3.Trim();
						break;
					case "ObjComplete_TitleLocID":
						m_objCompleteTitleLocID = text3.Trim();
						break;
					case "ObjComplete_SubTitleLocID":
						m_objCompleteSubTitleLocID = text3.Trim();
						break;
					case "ObjComplete_BodyLocID":
						m_objCompleteBodyLocID = text3.Trim();
						break;
					case "ObjComplete_DisplayPlayer":
						m_objCompleteDisplayPlayer = bool.Parse(text3.Trim());
						break;
					}
				}
			}
			Debug.Log("<color=green>Config file loaded successfully.</color>");
		}
		catch (Exception ex)
		{
			Debug.Log("Could not load config file. Error: " + ex.Message);
			m_dataLoadedSuccessfully = false;
		}
	}

	public void DisplayCustomData()
	{
		m_dataLoadedSuccessfully = false;
		LoadCustomFile();
		if (!m_dataLoadedSuccessfully)
		{
			return;
		}
		switch (m_displayType)
		{
		case LQADisplayType.DialogueWindow:
			DialogueManager.StartNewDialogue();
			DialogueManager.AddDialogue(m_dialogueTitleLocID, m_dialogueBodyLocID, m_dialogueIsFemale, m_dialogueWindowStyle);
			WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: true);
			break;
		case LQADisplayType.ConfirmWindow:
		{
			if (!WindowManager.GetIsWindowLoaded(WindowID.ConfirmMenu))
			{
				WindowManager.LoadWindow(WindowID.ConfirmMenu);
			}
			ConfirmMenuWindowController obj = WindowManager.GetWindowController(WindowID.ConfirmMenu) as ConfirmMenuWindowController;
			obj.SetTitleText(m_confirmTitleLocID, isLocID: true);
			obj.SetDescriptionText(m_confirmBodyLocID, isLocID: true);
			obj.SetNumberOfButtons(2);
			obj.SetOnCancelAction(m_closeConfirmWindow);
			ConfirmMenu_Button buttonAtIndex = obj.GetButtonAtIndex(0);
			buttonAtIndex.SetButtonText(m_button1LocID, isLocID: true);
			buttonAtIndex.SetOnClickAction(m_closeConfirmWindow);
			ConfirmMenu_Button buttonAtIndex2 = obj.GetButtonAtIndex(1);
			buttonAtIndex2.SetButtonText(m_button2LocID, isLocID: true);
			buttonAtIndex2.SetOnClickAction(m_closeConfirmWindow);
			WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: true);
			break;
		}
		case LQADisplayType.ObjectiveComplete:
		{
			LQAObjectiveCompleteHUDEventArgs eventArgs = new LQAObjectiveCompleteHUDEventArgs(5f, m_objCompleteTitleLocID, m_objCompleteSubTitleLocID, m_objCompleteBodyLocID, m_objCompleteDisplayPlayer);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, eventArgs);
			break;
		}
		}
	}

	private void CloseConfirmWindow()
	{
		WindowManager.SetWindowIsOpen(WindowID.ConfirmMenu, isOpen: false);
	}
}
