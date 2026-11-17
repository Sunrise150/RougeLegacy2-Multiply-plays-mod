using System;
using SceneManagement_RL;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameTimer : MonoBehaviour
{
	private SceneID[] SCENES_TO_TRACK_SESSION_TIME = new SceneID[2]
	{
		SceneID.World,
		SceneID.Town
	};

	private bool m_sessionStarted;

	private bool m_sessionPaused;

	private float m_sessionStartTime;

	private float m_sessionPauseStartTime;

	private float m_sessionAccumulatedPauseTime;

	private Action<MonoBehaviour, EventArgs> m_onGamePauseStateChanged;

	private Action m_onTransitionStarted;

	private Action m_onTransitionEnded;

	private GamePauseStateChangeEventArgs m_transitionPauseChangeEventArgs = new GamePauseStateChangeEventArgs(isPaused: false);

	private static GameTimer Instance { get; set; }

	public static float TotalSessionAccumulatedTime => SessionAccumulatedTime - SessionAccumulatedPauseTime;

	public static float SessionAccumulatedPauseTime => Instance.m_sessionAccumulatedPauseTime;

	public static float SessionAccumulatedTime => Time.unscaledTime - Instance.m_sessionStartTime;

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			m_onGamePauseStateChanged = OnGamePauseStateChanged;
			SceneManager.sceneLoaded += OnSceneLoaded;
			m_onTransitionStarted = OnTransitionStarted;
			m_onTransitionEnded = OnTransitionEnded;
			SceneLoader_RL.TransitionStartRelay.AddListener(m_onTransitionStarted);
			SceneLoader_RL.TransitionCompleteRelay.AddListener(m_onTransitionEnded);
			return;
		}
		throw new Exception("GameTimer instantiated twice. Should not happen.");
	}

	private void OnDestroy()
	{
		Instance = null;
		SceneManager.sceneLoaded -= OnSceneLoaded;
		if (!GameManager.IsApplicationClosing)
		{
			SceneLoader_RL.TransitionStartRelay.RemoveListener(m_onTransitionStarted);
			SceneLoader_RL.TransitionCompleteRelay.RemoveListener(m_onTransitionEnded);
		}
	}

	private void OnSceneLoaded(Scene scene, LoadSceneMode arg1)
	{
		bool flag = false;
		SceneID[] sCENES_TO_TRACK_SESSION_TIME = SCENES_TO_TRACK_SESSION_TIME;
		foreach (SceneID sceneID in sCENES_TO_TRACK_SESSION_TIME)
		{
			if (scene.name == SceneLoadingUtility.GetSceneName(sceneID))
			{
				flag = true;
				break;
			}
		}
		if (flag)
		{
			if (!m_sessionStarted)
			{
				m_sessionStarted = true;
				m_sessionPaused = false;
				Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.GamePauseStateChange, m_onGamePauseStateChanged);
				Debug.Log("Starting GameTimer playtime session.");
				ClearSessionAccumulatedTime();
			}
		}
		else if (m_sessionStarted)
		{
			m_sessionStarted = false;
			Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.GamePauseStateChange, m_onGamePauseStateChanged);
			if (!(SceneLoader_RL.CurrentScene == SceneLoadingUtility.GetSceneName(SceneID.MainMenu)))
			{
				Debug.Log("Ending GameTimer playtime session.");
				SaveManager.PlayerSaveData.SecondsPlayed += (uint)TotalSessionAccumulatedTime;
				Debug.Log("Saving session playtime: " + TotalSessionAccumulatedTime + " to profile " + SaveManager.CurrentProfile);
				SaveManager.SaveCurrentProfileGameData(SaveDataType.Player, SavingType.FileOnly, ignoreTimeRestriction: true);
			}
		}
	}

	public static void ClearSessionAccumulatedTime()
	{
		Instance.m_sessionAccumulatedPauseTime = 0f;
		Instance.m_sessionStartTime = Time.unscaledTime;
		Instance.m_sessionPauseStartTime = Time.unscaledTime;
	}

	private void OnTransitionStarted()
	{
		if (!GameManager.IsGamePaused)
		{
			m_transitionPauseChangeEventArgs.Initialize(isPaused: true);
			OnGamePauseStateChanged(null, m_transitionPauseChangeEventArgs);
		}
	}

	private void OnTransitionEnded()
	{
		if (!GameManager.IsGamePaused)
		{
			m_transitionPauseChangeEventArgs.Initialize(isPaused: false);
			OnGamePauseStateChanged(null, m_transitionPauseChangeEventArgs);
		}
	}

	private void OnGamePauseStateChanged(object sender, EventArgs args)
	{
		if (!m_sessionStarted)
		{
			return;
		}
		if ((args as GamePauseStateChangeEventArgs).IsPaused)
		{
			if (!m_sessionPaused)
			{
				m_sessionPaused = true;
				m_sessionPauseStartTime = Time.unscaledTime;
			}
		}
		else if (m_sessionPaused)
		{
			m_sessionPaused = false;
			m_sessionAccumulatedPauseTime += Time.unscaledTime - m_sessionPauseStartTime;
		}
	}
}
