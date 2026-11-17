using System;
using System.Collections;
using FMOD.Studio;
using FMODUnity;
using SceneManagement_RL;
using UnityEngine;

namespace RLAudio;

public class FairyRoomAudioEventController : MonoBehaviour, IAudioEventEmitter
{
	public static bool DisableAudio;

	[SerializeField]
	[EventRef]
	private string m_countdownTimerEventPath;

	[SerializeField]
	[EventRef]
	private string m_countdownTimerEndEventPath;

	[SerializeField]
	[EventRef]
	private string m_victoryEventPath;

	[SerializeField]
	[EventRef]
	private string m_defeatEventPath;

	[SerializeField]
	[EventRef]
	private string m_ruleFailedEventPath;

	private string m_description;

	private EventInstance m_countdownEvent;

	private const string COUNTDOWN_PARAMETER_NAME = "fairyTimer";

	private Coroutine m_countdownCoroutine;

	private Action<MonoBehaviour, EventArgs> m_onPlayerTriggerFairyRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerExitFairyRoom;

	private Action<MonoBehaviour, EventArgs> m_onPlayerDeath;

	private Action<MonoBehaviour, EventArgs> m_onFairyRuleStateChange;

	private Action<MonoBehaviour, EventArgs> m_onSpecialRoomComplete;

	private Action m_onTransitionStart;

	public string Description
	{
		get
		{
			if (string.IsNullOrEmpty(m_description))
			{
				m_description = ToString();
			}
			return m_description;
		}
	}

	private void Awake()
	{
		m_onPlayerTriggerFairyRoom = OnPlayerTriggerFairyRoom;
		m_onPlayerExitFairyRoom = OnPlayerExitFairyRoom;
		m_onPlayerDeath = OnPlayerDeath;
		m_onFairyRuleStateChange = OnFairyRuleStateChange;
		m_onSpecialRoomComplete = OnSpecialRoomComplete;
		m_onTransitionStart = OnTransitionStart;
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerFairyRoomTriggered, m_onPlayerTriggerFairyRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitFairyRoom, m_onPlayerExitFairyRoom);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.FairyRoomRuleStateChange, m_onFairyRuleStateChange);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.SpecialRoomCompleted, m_onSpecialRoomComplete);
		SceneLoader_RL.TransitionStartRelay.AddListener(m_onTransitionStart);
	}

	private void OnDestroy()
	{
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerFairyRoomTriggered, m_onPlayerTriggerFairyRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitFairyRoom, m_onPlayerExitFairyRoom);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerDeath, m_onPlayerDeath);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.FairyRoomRuleStateChange, m_onFairyRuleStateChange);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.SpecialRoomCompleted, m_onSpecialRoomComplete);
		if (SceneLoader_RL.TransitionStartRelay != null)
		{
			SceneLoader_RL.TransitionStartRelay.RemoveListener(m_onTransitionStart);
		}
		if (m_countdownEvent.isValid())
		{
			m_countdownEvent.release();
		}
	}

	private void OnSpecialRoomComplete(MonoBehaviour arg1, EventArgs arg2)
	{
		if (arg2 is SpecialRoomCompletedEventArgs specialRoomCompletedEventArgs && specialRoomCompletedEventArgs.SpecialRoomController.SpecialRoomType == SpecialRoomType.Fairy)
		{
			if ((specialRoomCompletedEventArgs.SpecialRoomController as FairyRoomController).State == FairyRoomState.Passed)
			{
				AudioManager.PlayOneShot(this, m_victoryEventPath);
			}
			else
			{
				AudioManager.PlayOneShot(this, m_defeatEventPath);
			}
		}
		StopCountdownTimer();
	}

	private void OnFairyRuleStateChange(MonoBehaviour arg1, EventArgs arg2)
	{
		if (!DisableAudio && arg2 is FairyRoomRuleStateChangeEventArgs fairyRoomRuleStateChangeEventArgs && fairyRoomRuleStateChangeEventArgs.Rule.State == FairyRoomState.Failed)
		{
			AudioManager.PlayOneShot(this, m_ruleFailedEventPath);
		}
	}

	private void OnPlayerDeath(MonoBehaviour arg1, EventArgs arg2)
	{
		StopCountdownTimer();
	}

	private void OnTransitionStart()
	{
		StopCountdownTimer();
	}

	private void StopCountdownTimer()
	{
		if (m_countdownEvent.isValid())
		{
			AudioManager.Stop(m_countdownEvent);
			if (m_countdownCoroutine != null)
			{
				StopCoroutine(m_countdownCoroutine);
				m_countdownCoroutine = null;
				AudioManager.PlayOneShot(this, m_countdownTimerEndEventPath);
			}
		}
	}

	private void OnPlayerExitFairyRoom(MonoBehaviour arg1, EventArgs arg2)
	{
		StopCountdownTimer();
	}

	private void OnPlayerTriggerFairyRoom(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (!(eventArgs is FairyRoomEnteredEventArgs fairyRoomEnteredEventArgs) || fairyRoomEnteredEventArgs.FairyRoomController.State == FairyRoomState.Failed || fairyRoomEnteredEventArgs.FairyRoomController.State == FairyRoomState.Passed)
		{
			return;
		}
		for (int i = 0; i < fairyRoomEnteredEventArgs.FairyRoomController.FairyRoomRuleEntries.Count; i++)
		{
			if (fairyRoomEnteredEventArgs.FairyRoomController.FairyRoomRuleEntries[i].FairyRuleID == FairyRuleID.TimeLimit)
			{
				if (!m_countdownEvent.isValid())
				{
					m_countdownEvent = AudioUtility.GetEventInstance(m_countdownTimerEventPath, base.transform);
				}
				m_countdownCoroutine = StartCoroutine(CountdownTimerCoroutine(fairyRoomEnteredEventArgs.FairyRoomController.FairyRoomRuleEntries[i].FairyRule as TimeLimit_FairyRule));
				break;
			}
		}
	}

	private IEnumerator CountdownTimerCoroutine(TimeLimit_FairyRule fairyRule)
	{
		AudioManager.Play(this, m_countdownEvent);
		float timeLimit = fairyRule.TimeRemaining;
		while (fairyRule.TimeRemaining > 0f)
		{
			float value = 1f - fairyRule.TimeRemaining / timeLimit;
			m_countdownEvent.setParameterByName("fairyTimer", value);
			yield return null;
		}
		m_countdownCoroutine = null;
	}
}
