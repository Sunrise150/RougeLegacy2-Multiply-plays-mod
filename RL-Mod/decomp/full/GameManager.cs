using System.Collections;
using RL_Windows;
using Steamworks;
using UnityEngine;

public class GameManager : MonoBehaviour
{
	private static GameManager m_instance;

	private static GamePauseStateChangeEventArgs m_pauseEventArgs;

	private bool m_isGamePaused;

	protected Callback<GameOverlayActivated_t> m_steamOverlayActivated;

	public static bool IsApplicationClosing { get; private set; }

	private static GameManager Instance
	{
		get
		{
			return m_instance;
		}
		set
		{
			m_instance = value;
		}
	}

	public static bool IsGameManagerInstantiated => m_instance;

	public static bool IsGamePaused
	{
		get
		{
			return Instance.m_isGamePaused;
		}
		private set
		{
			Instance.m_isGamePaused = value;
		}
	}

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			m_pauseEventArgs = new GamePauseStateChangeEventArgs(isPaused: false);
		}
		else if (Instance != this)
		{
			Object.Destroy(base.gameObject);
		}
	}

	private IEnumerator Start()
	{
		while (!SteamManager.Initialized)
		{
			yield return null;
		}
		m_steamOverlayActivated = Callback<GameOverlayActivated_t>.Create(OnSteamOverlayActivated);
	}

	private void OnSteamOverlayActivated(GameOverlayActivated_t overlayCallback)
	{
		if (overlayCallback.m_bActive != 0)
		{
			WindowManager.PauseWhenPossible(shouldPause: true);
		}
		else
		{
			WindowManager.PauseWhenPossible(shouldPause: false);
		}
	}

	private void OnApplicationFocus(bool focus)
	{
		WindowManager.PauseWhenPossible(!focus);
	}

	private void OnApplicationQuit()
	{
		Debug.Log("Application is closing...");
		IsApplicationClosing = true;
	}

	public static void SetIsPaused(bool isPaused)
	{
		if (IsGamePaused != isPaused)
		{
			IsGamePaused = isPaused;
			RumbleManager.SetRumblePaused(isPaused);
			if (IsGamePaused)
			{
				RLTimeScale.SetTimeScale(TimeScaleType.Game, 0f);
				OnGameLoadManager.ConfineMouseToGameWindow(isConfined: false);
			}
			else
			{
				RLTimeScale.SetTimeScale(TimeScaleType.Game, 1f);
				OnGameLoadManager.ConfineMouseToGameWindow(!SaveManager.ConfigData.DisableCursorConfine);
			}
			m_pauseEventArgs.Initialize(IsGamePaused);
			Messenger<GameMessenger, GameEvent>.Broadcast(GameEvent.GamePauseStateChange, Instance, m_pauseEventArgs);
		}
	}
}
