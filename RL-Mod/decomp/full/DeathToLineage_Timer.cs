using System;
using System.Diagnostics;
using UnityEngine;

public class DeathToLineage_Timer : MonoBehaviour
{
	private Action<MonoBehaviour, EventArgs> m_onDeathWindowClosed;

	private Action<MonoBehaviour, EventArgs> m_onEnterLineageScreen;

	private static Stopwatch m_stopwatch;

	private static DeathToLineage_Timer Instance { get; set; }

	private void Awake()
	{
		if (Instance == null)
		{
			Instance = this;
			m_onDeathWindowClosed = OnDeathWindowClosed;
			m_onEnterLineageScreen = OnEnterLineageScreen;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.PlayerDeathWindow_Closed, m_onDeathWindowClosed);
		Messenger<SceneMessenger, SceneEvent>.AddListener(SceneEvent.EnterLineageScreen, m_onEnterLineageScreen);
	}

	private void OnEnterLineageScreen(MonoBehaviour sender, EventArgs eventArgs)
	{
		StopTimer();
	}

	private void OnDeathWindowClosed(MonoBehaviour sender, EventArgs eventArgs)
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
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.PlayerDeathWindow_Closed, m_onDeathWindowClosed);
		Messenger<SceneMessenger, SceneEvent>.RemoveListener(SceneEvent.EnterLineageScreen, m_onEnterLineageScreen);
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
		if (m_stopwatch != null && m_stopwatch.IsRunning)
		{
			m_stopwatch.Stop();
			UnityEngine.Debug.LogFormat("<color=blue>Time from <b>Exiting Death Screen</b> to <b>Being Able to Choose an Heir in Lineage Screen</b> = {0} ms</color>", m_stopwatch.ElapsedMilliseconds);
		}
	}
}
