using System;
using System.Diagnostics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HubTownToWorld_Timer : MonoBehaviour
{
	private Action<MonoBehaviour, EventArgs> m_onExitHubTown;

	private Action<MonoBehaviour, EventArgs> m_onLevelManagerStateChange;

	private static Stopwatch m_stopwatch;

	private static HubTownToWorld_Timer Instance { get; set; }

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			m_onExitHubTown = OnExitHubTown;
			m_onLevelManagerStateChange = OnLevelManagerStateChange;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.PlayerExitHubTown, m_onExitHubTown);
		Messenger<GameMessenger, GameEvent>.AddListener(GameEvent.LevelManagerStateChange, m_onLevelManagerStateChange);
	}

	private void OnLevelManagerStateChange(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (eventArgs is LevelManagerStateChangeEventArgs && (eventArgs as LevelManagerStateChangeEventArgs).State == LevelManagerState.BuildComplete)
		{
			StopTimer();
		}
	}

	private void OnExitHubTown(MonoBehaviour sender, EventArgs eventArgs)
	{
		StartTimer();
	}

	private void OnDestroy()
	{
		if (Instance == this)
		{
			Instance = null;
			m_stopwatch = null;
		}
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.PlayerExitHubTown, m_onExitHubTown);
		Messenger<GameMessenger, GameEvent>.RemoveListener(GameEvent.LevelManagerStateChange, m_onLevelManagerStateChange);
	}

	public static void StartTimer()
	{
		if (m_stopwatch == null)
		{
			m_stopwatch = new Stopwatch();
		}
		m_stopwatch.Restart();
	}

	public static void StopTimer()
	{
		if (SceneManager.GetActiveScene().name.StartsWith("World") && m_stopwatch != null && m_stopwatch.IsRunning)
		{
			m_stopwatch.Stop();
			UnityEngine.Debug.LogFormat("<color=blue>Time from <b>Charon</b> in Hub Town to <b>Gate Transition</b> in World = {0} ms</color>", m_stopwatch.ElapsedMilliseconds);
		}
	}
}
