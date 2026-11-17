using System;
using System.Collections;
using System.Collections.Generic;
using RL_Windows;
using Rewired;
using Sigtrap.Relays;
using UnityEngine;
using UnityEngine.UI;

public class DialogueWindowController : WindowController, ILocalizable
{
	public enum TextState
	{
		Disabled,
		Typewriting,
		TypewritingComplete,
		WaitingForInput
	}

	[SerializeField]
	private CanvasGroup m_fadeBGCanvasGroup;

	[SerializeField]
	private DialogueWindowEntry m_horzDialogueWindowEntry;

	[SerializeField]
	private DialogueWindowEntry m_vertDialogueWindowEntry;

	[SerializeField]
	private Vector2 m_dialogueWindowOffsets;

	[SerializeField]
	private RectTransform m_portraitRectTransform;

	[SerializeField]
	private Image m_portraitImage;

	[SerializeField]
	private CanvasGroup m_portraitCanvasGroup;

	[SerializeField]
	private RectTransform m_vertLeftPortraitPosRectTransform;

	[SerializeField]
	private RectTransform m_vertRightPortraitPosRectTransform;

	[SerializeField]
	private RectTransform m_horzUpperPortraitPosRectTransform;

	[SerializeField]
	private RectTransform m_horzLowerPortraitPosRectTransform;

	private LinkedListNode<DialogueObj> m_currentDialogueNode;

	private float m_inputDelayTime;

	private int m_currentDialogueNodeIndex;

	private DialogueWindowEntry m_currentDialogueWindowEntry;

	private DialogueWindowStyle m_currentWindowStyle;

	private string m_insightEndHandlerKey;

	private string m_scarEndHandlerKey;

	private InsightObjectiveCompleteHUDEventArgs m_insightEventArgs = new InsightObjectiveCompleteHUDEventArgs(InsightType.None, discovered: false);

	private ScarObjectiveCompleteHUDEventArgs m_scarEventArgs = new ScarObjectiveCompleteHUDEventArgs(ChallengeType.None);

	public Relay WriteLineOfDialogStartRelay = new Relay();

	public Relay WriteLineOfDialogEndRelay = new Relay();

	public Relay NextLineOfDialogRelay = new Relay();

	public Relay DialogCompleteRelay = new Relay();

	public Relay DialogWindowBeginClosingRelay = new Relay();

	public Relay WriteCharacterStartRelay = new Relay();

	private List<Action> m_queuedEndHandlers = new List<Action>();

	private Action<MonoBehaviour, EventArgs> m_refreshText;

	private Action m_onWriteSpecialCharacter;

	private Action m_onWriteLine;

	private Action m_dialogueComplete;

	private Action<InputActionEventData> m_onConfirmButtonPressed;

	private bool m_globalTimerIsRunning;

	private bool m_isRunningEndHandlers;

	private Relay m_onDialogueCompleteRelay = new Relay();

	public IRelayLink OnDialogueCompleteRelay => m_onDialogueCompleteRelay.link;

	public override WindowID ID => WindowID.Dialogue;

	private Vector2 GetDialogueWindowPosition(DialogueWindowStyle windowStyle, float appliedOffset)
	{
		return windowStyle switch
		{
			DialogueWindowStyle.HorizontalUpper => new Vector2(0f, m_dialogueWindowOffsets.y + appliedOffset), 
			DialogueWindowStyle.HorizontalLower => new Vector2(0f, 0f - m_dialogueWindowOffsets.y - appliedOffset - 90f), 
			DialogueWindowStyle.VerticalLeft => new Vector2(0f - m_dialogueWindowOffsets.x - appliedOffset, 0f), 
			DialogueWindowStyle.VerticalRight => new Vector2(m_dialogueWindowOffsets.x + appliedOffset, 0f), 
			_ => Vector2.zero, 
		};
	}

	private void Awake()
	{
		m_refreshText = RefreshText;
		m_onWriteSpecialCharacter = OnWriteSpecialCharacter;
		m_onWriteLine = OnWriteLine;
		m_onConfirmButtonPressed = OnConfirmButtonPressed;
		m_dialogueComplete = DialogueComplete;
	}

	protected override void OnOpen()
	{
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DialogueWindow_Opened, this, null);
		m_windowCanvas.gameObject.SetActive(value: true);
		m_inputDelayTime = Time.unscaledTime;
		m_insightEndHandlerKey = null;
		m_scarEndHandlerKey = null;
		AddListeners();
		m_currentDialogueNode = null;
		GoToNextDialogueNode();
		m_currentWindowStyle = m_currentDialogueNode.Value.WindowStyle;
		StartDialogue();
		m_globalTimerIsRunning = GlobalTimerHUDController.IsRunning;
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StopGlobalTimer, null, null);
		StartCoroutine(OnOpenAnimCoroutine());
	}

	private void SetCurrentDialogueWindowEntry(DialogueWindowStyle windowStyle)
	{
		switch (windowStyle)
		{
		case DialogueWindowStyle.HorizontalUpper:
			m_currentDialogueWindowEntry = m_horzDialogueWindowEntry;
			break;
		case DialogueWindowStyle.HorizontalLower:
			m_currentDialogueWindowEntry = m_horzDialogueWindowEntry;
			break;
		case DialogueWindowStyle.VerticalLeft:
			m_currentDialogueWindowEntry = m_vertDialogueWindowEntry;
			break;
		case DialogueWindowStyle.VerticalRight:
			m_currentDialogueWindowEntry = m_vertDialogueWindowEntry;
			break;
		}
	}

	private IEnumerator OnOpenAnimCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		m_fadeBGCanvasGroup.alpha = 0f;
		m_horzDialogueWindowEntry.CanvasGroup.alpha = 0f;
		m_vertDialogueWindowEntry.CanvasGroup.alpha = 0f;
		m_portraitCanvasGroup.alpha = 1f;
		m_vertDialogueWindowEntry.ArrowObj.SetActive(value: false);
		m_horzDialogueWindowEntry.ArrowObj.SetActive(value: false);
		m_portraitCanvasGroup.gameObject.SetActive(value: false);
		Vector2 dialogueWindowPosition = GetDialogueWindowPosition(m_currentWindowStyle, 20f);
		Vector2 dialogueWindowPosition2 = GetDialogueWindowPosition(m_currentWindowStyle, 0f);
		m_currentDialogueWindowEntry.RectTransform.anchoredPosition = dialogueWindowPosition;
		float duration = 0.2f;
		TweenManager.TweenTo_UnscaledTime(m_currentDialogueWindowEntry.RectTransform, duration, Ease.Quad.EaseOut, "anchoredPosition.x", dialogueWindowPosition2.x, "anchoredPosition.y", dialogueWindowPosition2.y);
		TweenManager.TweenTo_UnscaledTime(m_fadeBGCanvasGroup, duration, Ease.None, "alpha", 1);
		yield return TweenManager.TweenTo_UnscaledTime(m_currentDialogueWindowEntry.CanvasGroup, duration, Ease.None, "alpha", 1).TweenCoroutine;
		yield return TweenPortraitCoroutine();
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	private IEnumerator TweenPortraitCoroutine()
	{
		if (m_currentDialogueNode != null && m_currentDialogueNode.Value.PortraitType != 0)
		{
			m_portraitImage.sprite = IconLibrary.GetDialoguePortrait(m_currentDialogueNode.Value.PortraitType);
			m_portraitRectTransform.gameObject.SetActive(value: true);
			m_portraitRectTransform.anchoredPosition = Vector2.zero;
			RectTransform parent = null;
			float num = 0f;
			float num2 = 0f;
			switch (m_currentWindowStyle)
			{
			case DialogueWindowStyle.HorizontalLower:
				parent = m_horzLowerPortraitPosRectTransform;
				num2 = 200f;
				break;
			case DialogueWindowStyle.HorizontalUpper:
				parent = m_horzUpperPortraitPosRectTransform;
				num2 = -200f;
				break;
			case DialogueWindowStyle.VerticalLeft:
				parent = m_vertLeftPortraitPosRectTransform;
				num = 200f;
				break;
			case DialogueWindowStyle.VerticalRight:
				parent = m_vertRightPortraitPosRectTransform;
				num = -200f;
				break;
			}
			m_portraitRectTransform.SetParent(parent, worldPositionStays: false);
			m_portraitRectTransform.anchoredPosition = new Vector2(0f - num, 0f - num2);
			yield return TweenManager.TweenBy_UnscaledTime(m_portraitRectTransform, 0.25f, Ease.Quad.EaseOut, "anchoredPosition.x", num, "anchoredPosition.y", num2).TweenCoroutine;
		}
	}

	protected override void OnClose()
	{
		RemoveListeners();
		if (m_globalTimerIsRunning)
		{
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.StartGlobalTimer, null, null);
		}
		m_globalTimerIsRunning = false;
		m_windowCanvas.gameObject.SetActive(value: false);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DialogueWindow_Closed, this, null);
		StartCoroutine(RunEndHandlerCoroutine());
		if (m_insightEndHandlerKey != null)
		{
			StartCoroutine(RunInsightEndHandler());
		}
		else if (m_scarEndHandlerKey != null && ChallengeManager.GetFoundState(Challenge_EV.ScarUnlockTable[m_scarEndHandlerKey]) == FoundState.NotFound)
		{
			StartCoroutine(RunScarEndHandler());
		}
	}

	private IEnumerator OnExitAnimCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		Vector2 dialogueWindowPosition = GetDialogueWindowPosition(m_currentWindowStyle, 0f);
		Vector2 dialogueWindowPosition2 = GetDialogueWindowPosition(m_currentWindowStyle, 20f);
		DialogWindowBeginClosingRelay.Dispatch();
		m_currentDialogueWindowEntry.RectTransform.anchoredPosition = dialogueWindowPosition;
		float duration = 0.2f;
		TweenManager.TweenTo_UnscaledTime(m_currentDialogueWindowEntry.RectTransform, duration, Ease.Quad.EaseIn, "anchoredPosition.x", dialogueWindowPosition2.x, "anchoredPosition.y", dialogueWindowPosition2.y);
		TweenManager.TweenTo_UnscaledTime(m_fadeBGCanvasGroup, duration, Ease.None, "alpha", 0);
		TweenManager.TweenTo_UnscaledTime(m_portraitCanvasGroup, duration, Ease.None, "alpha", 0);
		yield return TweenManager.TweenTo_UnscaledTime(m_currentDialogueWindowEntry.CanvasGroup, duration, Ease.None, "alpha", 0).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
		WindowManager.SetWindowIsOpen(WindowID.Dialogue, isOpen: false);
		if ((bool)DialogueManager.NPCController)
		{
			DialogueManager.NPCController.SetNPCState(DialogueManager.OnCompleteNPCState);
		}
	}

	private IEnumerator RunInsightEndHandler()
	{
		yield return null;
		InsightUnlockEntry insightUnlockEntry = Insight_EV.InsightUnlockTable[m_insightEndHandlerKey];
		if (insightUnlockEntry.Discovered)
		{
			if (SaveManager.PlayerSaveData.GetInsightState(insightUnlockEntry.InsightToUnlock) < InsightState.DiscoveredButNotViewed)
			{
				SaveManager.PlayerSaveData.SetInsightState(insightUnlockEntry.InsightToUnlock, InsightState.DiscoveredButNotViewed, forceOverride: false);
				m_insightEventArgs.Initialize(insightUnlockEntry.InsightToUnlock, discovered: true);
				Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
			}
		}
		else if (SaveManager.PlayerSaveData.GetInsightState(insightUnlockEntry.InsightToUnlock) < InsightState.ResolvedButNotViewed)
		{
			SaveManager.PlayerSaveData.SetInsightState(insightUnlockEntry.InsightToUnlock, InsightState.ResolvedButNotViewed, forceOverride: false);
			m_insightEventArgs.Initialize(insightUnlockEntry.InsightToUnlock, discovered: false);
			Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_insightEventArgs);
		}
	}

	private IEnumerator RunScarEndHandler()
	{
		yield return null;
		ChallengeType challengeType = Challenge_EV.ScarUnlockTable[m_scarEndHandlerKey];
		ChallengeManager.SetFoundState(challengeType, FoundState.FoundButNotViewed, overrideValues: true);
		m_scarEventArgs.Initialize(challengeType);
		Messenger<UIMessenger, UIEvent>.Broadcast(UIEvent.DisplayObjectiveCompleteHUD, this, m_scarEventArgs);
	}

	private IEnumerator RunEndHandlerCoroutine()
	{
		m_isRunningEndHandlers = true;
		yield return null;
		RunEndHandler();
		m_isRunningEndHandlers = false;
	}

	private void RunEndHandler()
	{
		DialogueManager.UpdateNPCDialogueState();
		m_onDialogueCompleteRelay.Dispatch();
		m_onDialogueCompleteRelay.RemoveAll();
		foreach (Action queuedEndHandler in m_queuedEndHandlers)
		{
			OnDialogueCompleteRelay.AddListener(queuedEndHandler);
		}
		m_queuedEndHandlers.Clear();
	}

	public void AddEndHandler(Action action)
	{
		if (m_isRunningEndHandlers)
		{
			m_queuedEndHandlers.Add(action);
		}
		else
		{
			OnDialogueCompleteRelay.AddListener(action);
		}
	}

	private void AddListeners()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.LanguageChanged, m_refreshText);
		m_horzDialogueWindowEntry.Typewrite.OnTypewriteCompleteRelay.AddListener(m_dialogueComplete);
		m_vertDialogueWindowEntry.Typewrite.OnTypewriteCompleteRelay.AddListener(m_dialogueComplete);
		m_horzDialogueWindowEntry.Typewrite.OnTypewriteLongDelayRelay.AddListener(m_onWriteSpecialCharacter);
		m_vertDialogueWindowEntry.Typewrite.OnTypewriteLongDelayRelay.AddListener(m_onWriteSpecialCharacter);
		m_horzDialogueWindowEntry.Typewrite.OnTypewriteShortDelayRelay.AddListener(m_onWriteLine);
		m_vertDialogueWindowEntry.Typewrite.OnTypewriteShortDelayRelay.AddListener(m_onWriteLine);
		base.RewiredPlayer.AddInputEventDelegate(m_onConfirmButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
	}

	private void OnWriteSpecialCharacter()
	{
		WriteCharacterStartRelay.Dispatch();
	}

	private void OnWriteLine()
	{
		WriteLineOfDialogStartRelay.Dispatch();
	}

	private void RemoveListeners()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.LanguageChanged, m_refreshText);
		m_horzDialogueWindowEntry.Typewrite.OnTypewriteCompleteRelay.RemoveListener(m_dialogueComplete);
		m_vertDialogueWindowEntry.Typewrite.OnTypewriteCompleteRelay.RemoveListener(m_dialogueComplete);
		m_horzDialogueWindowEntry.Typewrite.OnTypewriteLongDelayRelay.RemoveListener(m_onWriteLine);
		m_vertDialogueWindowEntry.Typewrite.OnTypewriteLongDelayRelay.RemoveListener(m_onWriteLine);
		m_horzDialogueWindowEntry.Typewrite.OnTypewriteShortDelayRelay.RemoveListener(m_onWriteSpecialCharacter);
		m_vertDialogueWindowEntry.Typewrite.OnTypewriteShortDelayRelay.RemoveListener(m_onWriteSpecialCharacter);
		base.RewiredPlayer.RemoveInputEventDelegate(m_onConfirmButtonPressed, UpdateLoopType.Update, InputActionEventType.ButtonJustPressed, "Window_Confirm");
	}

	protected override void OnFocus()
	{
	}

	protected override void OnLostFocus()
	{
	}

	protected override void OnPause()
	{
		throw new NotImplementedException();
	}

	protected override void OnUnpause()
	{
		throw new NotImplementedException();
	}

	private void OnConfirmButtonPressed(InputActionEventData eventData)
	{
		if (!(m_inputDelayTime > Time.unscaledTime))
		{
			m_inputDelayTime = Time.unscaledTime;
			if (m_currentDialogueWindowEntry.Typewrite.IsTypewriting)
			{
				SkipTypewriting();
				return;
			}
			GoToNextDialogueNode();
			StartDialogue();
		}
	}

	protected void GoToNextDialogueNode()
	{
		if (m_currentDialogueNode == null)
		{
			m_currentDialogueNode = DialogueManager.GetFirstDialogueNode;
			m_currentDialogueNodeIndex = 0;
		}
		else
		{
			m_currentDialogueNode = m_currentDialogueNode.Next;
			m_currentDialogueNodeIndex++;
		}
		if (m_currentDialogueNode != null)
		{
			NextLineOfDialogRelay.Dispatch();
			SetCurrentDialogueWindowEntry(m_currentDialogueNode.Value.WindowStyle);
		}
	}

	protected void StartDialogue()
	{
		if (m_currentDialogueNode != null)
		{
			m_currentDialogueWindowEntry.ArrowObj.SetActive(value: false);
			m_currentDialogueWindowEntry.TitleText.text = m_currentDialogueNode.Value.speaker;
			m_currentDialogueWindowEntry.DialogueText.text = m_currentDialogueNode.Value.text;
			m_currentDialogueWindowEntry.Typewrite.TypewriteCharDelay = 0.015f;
			m_currentDialogueWindowEntry.Typewrite.TypewriteCharLongDelay = 0.1f;
			m_currentDialogueWindowEntry.Typewrite.StartTypewriting();
			string lastTextLocID = DialogueManager.LastTextLocID;
			if (!string.IsNullOrEmpty(lastTextLocID))
			{
				if (Insight_EV.InsightUnlockTable.ContainsKey(lastTextLocID))
				{
					m_insightEndHandlerKey = lastTextLocID;
				}
				else if (Challenge_EV.ScarUnlockTable.ContainsKey(lastTextLocID))
				{
					m_scarEndHandlerKey = lastTextLocID;
				}
			}
			if ((bool)DialogueManager.NPCController)
			{
				if (m_currentDialogueNode.Value.NPCState == NPCState.None)
				{
					DialogueManager.NPCController.SetNPCState(NPCState.Speaking);
				}
				else
				{
					DialogueManager.NPCController.SetNPCState(m_currentDialogueNode.Value.NPCState);
				}
			}
		}
		else
		{
			EndAllDialogue();
		}
	}

	protected void SkipTypewriting()
	{
		m_currentDialogueWindowEntry.Typewrite.StopTypewriting();
	}

	protected void EndAllDialogue()
	{
		DialogCompleteRelay.Dispatch();
		StartCoroutine(OnExitAnimCoroutine());
	}

	protected void DialogueComplete()
	{
		WriteLineOfDialogEndRelay.Dispatch();
		StartCoroutine(DialogueCompleteCoroutine());
	}

	private IEnumerator DialogueCompleteCoroutine()
	{
		RewiredMapController.SetCurrentMapEnabled(enabled: false);
		if ((bool)DialogueManager.NPCController)
		{
			DialogueManager.NPCController.SetNPCState(NPCState.SpeakingPause);
		}
		GameObject arrowObj = m_currentDialogueWindowEntry.ArrowObj;
		arrowObj.SetActive(value: true);
		Vector3 localScale = arrowObj.transform.localScale;
		Vector3 localScale2 = localScale * 0.25f;
		arrowObj.transform.localScale = localScale2;
		RectTransform component = arrowObj.GetComponent<RectTransform>();
		Vector2 anchoredPosition = component.anchoredPosition;
		anchoredPosition.y += 30f;
		component.anchoredPosition = anchoredPosition;
		TweenManager.TweenTo_UnscaledTime(arrowObj.transform, 0.15f, Ease.Back.EaseOut, "localScale.x", localScale.x, "localScale.y", localScale.y, "localScale.z", localScale.z);
		yield return TweenManager.TweenBy_UnscaledTime(component, 0.15f, Ease.Back.EaseOutLarge, "anchoredPosition.y", -30).TweenCoroutine;
		RewiredMapController.SetCurrentMapEnabled(enabled: true);
	}

	public void RefreshText(object sender, EventArgs args)
	{
		if ((bool)m_currentDialogueWindowEntry && !m_currentDialogueWindowEntry.Typewrite.IsTypewriting)
		{
			DialogueManager.ForceRefreshText();
			m_currentDialogueNode = DialogueManager.GetFirstDialogueNode;
			for (int i = 0; i < m_currentDialogueNodeIndex; i++)
			{
				m_currentDialogueNode = m_currentDialogueNode.Next;
			}
			StartDialogue();
		}
	}
}
