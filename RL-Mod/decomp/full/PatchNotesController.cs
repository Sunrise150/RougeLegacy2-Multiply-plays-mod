using System;
using System.Collections;
using Rewired;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

public class PatchNotesController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private GameObject m_scrollArrow;

	[SerializeField]
	private GameObject m_raycastBlocker;

	[SerializeField]
	private ScrollBarInput_RL m_scrollInput;

	[SerializeField]
	private MainMenuWindowController m_mainWindow;

	[SerializeField]
	private TextAsset m_testTextFile;

	private Action<InputActionEventData> m_onCancelButtonPressed;

	private void Awake()
	{
		m_scrollArrow.SetActive(value: false);
		m_raycastBlocker.SetActive(value: false);
		m_onCancelButtonPressed = OnCancelButtonPressed;
	}

	private void Start()
	{
	}

	public void SetEnablePatchNotes(bool enable)
	{
		m_scrollArrow.SetActive(enable);
		m_raycastBlocker.SetActive(enable);
		if (enable)
		{
			Rewired_RL.Player.AddInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			Rewired_RL.Player.AddInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			m_scrollInput.AssignButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
		else
		{
			Rewired_RL.Player.RemoveInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Cancel");
			Rewired_RL.Player.RemoveInputEventDelegate(m_onCancelButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
			m_scrollInput.RemoveButtonToScroll(Rewired_RL.WindowInputActionType.Window_Vertical);
		}
	}

	private void OnCancelButtonPressed(InputActionEventData eventData)
	{
		m_mainWindow.SetEnablePatchNotes(enable: false);
	}

	private IEnumerator WWWRequestPatchNotes()
	{
		UnityWebRequest webRequest = UnityWebRequest.Get("URL");
		yield return webRequest.SendWebRequest();
		if (webRequest.error != null)
		{
			Debug.Log("Error .. " + webRequest.error);
			yield break;
		}
		string text = webRequest.downloadHandler.text;
		Debug.Log("Found ... ==>" + text + "<==");
		ParsePatchNoteText(text);
	}

	private void ParsePatchNoteText(string textToParse)
	{
		string text = "";
		string[] array = textToParse.Split(new string[1] { "\r\n" }, StringSplitOptions.None);
		foreach (string text2 in array)
		{
			if (text2 != null)
			{
				int num = text2.IndexOf("=");
				if (num != -1)
				{
					string text3 = text2.Substring(0, num).ToLower();
					string text4 = text2.Substring(num + 1);
					text = text3 switch
					{
						"title" => text + "<size=120%>" + text4 + "</size>", 
						"bullet" => text + "• <indent=5%>" + text4 + "</indent>", 
						_ => text + text4, 
					};
				}
				else
				{
					text += text2;
				}
				text += "\n";
			}
		}
		m_text.text = text;
	}
}
