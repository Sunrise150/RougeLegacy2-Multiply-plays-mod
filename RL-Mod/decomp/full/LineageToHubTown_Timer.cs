using System;
using System.Diagnostics;
using UnityEngine;

public class LineageToHubTown_Timer : MonoBehaviour
{
	private Action<MonoBehaviour, EventArgs> m_onConfirmHeirInLineage;

	private Action<MonoBehaviour, EventArgs> m_onSkillTreeOpened;

	private static Stopwatch m_stopwatch;

	private static LineageToHubTown_Timer Instance { get; set; }

	private void Awake()
	{
		if (!Instance)
		{
			Instance = this;
			m_onConfirmHeirInLineage = OnConfirmHeirInLineage;
			m_onSkillTreeOpened = OnSkillTreeOpened;
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	private void Start()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.Lineage_ConfirmHeir, m_onConfirmHeirInLineage);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpened);
	}

	private void OnSkillTreeOpened(MonoBehaviour sender, EventArgs eventArgs)
	{
		StopTimer();
	}

	private void OnConfirmHeirInLineage(MonoBehaviour sender, EventArgs eventArgs)
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
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.Lineage_ConfirmHeir, m_onConfirmHeirInLineage);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.SkillTree_Opened, m_onSkillTreeOpened);
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
			UnityEngine.Debug.LogFormat("<color=blue>Time from <b>Confirm Heir</b> in Lineage to <b>Skill Tree</b> in Hub Town = {0} ms</color>", m_stopwatch.ElapsedMilliseconds);
		}
	}
}
