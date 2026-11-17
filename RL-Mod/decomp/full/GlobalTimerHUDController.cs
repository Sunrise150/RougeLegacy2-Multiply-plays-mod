using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class GlobalTimerHUDController : MonoBehaviour
{
	private const string INITIAL_TIMER_FORMAT = "{0:D2}:{1:D2}:{2:D2}";

	private const string TIMER_FORMAT_A = "{3}{0:D2}:{1:D2}:{2:D2}";

	private const string TIMER_FORMAT_B = "{4}{0}:{1:D2}:{2:D2}:{3:D2}";

	[SerializeField]
	private GameObject m_canvasGO;

	[SerializeField]
	private TMP_Text m_timerTextObj;

	private Action<MonoBehaviour, EventArgs> m_onDisplayTimer;

	private Action<MonoBehaviour, EventArgs> m_onHideTimer;

	private Action<MonoBehaviour, EventArgs> m_onStartTimer;

	private Action<MonoBehaviour, EventArgs> m_onStopTimer;

	private Action<MonoBehaviour, EventArgs> m_onResetTimer;

	public static float ElapsedTime { get; set; }

	public static bool IsRunning { get; private set; }

	public static bool ReverseTimer { get; set; }

	public static bool SlowTime { get; set; }

	public static float ReverseStartTime { get; set; }

	public static bool TrackNegativeTimeAchievement { get; set; }

	private void Awake()
	{
		OnResetTimer(null, null);
		OnHideTimer(null, null);
		m_onDisplayTimer = OnDisplayTimer;
		m_onHideTimer = OnHideTimer;
		m_onStartTimer = OnStartTimer;
		m_onStopTimer = OnStopTimer;
		m_onResetTimer = OnResetTimer;
	}

	private void OnEnable()
	{
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.DisplayGlobalTimer, m_onDisplayTimer);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.HideGlobalTimer, m_onHideTimer);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.StartGlobalTimer, m_onStartTimer);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.StopGlobalTimer, m_onStopTimer);
		Messenger<UIMessenger, UIEvent>.AddListener(UIEvent.ResetGlobalTimer, m_onResetTimer);
	}

	private void OnDisable()
	{
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.DisplayGlobalTimer, m_onDisplayTimer);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.HideGlobalTimer, m_onHideTimer);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.StartGlobalTimer, m_onStartTimer);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.StopGlobalTimer, m_onStopTimer);
		Messenger<UIMessenger, UIEvent>.RemoveListener(UIEvent.ResetGlobalTimer, m_onResetTimer);
	}

	private void OnDisplayTimer(object sender, EventArgs args)
	{
		if (!m_canvasGO.activeSelf)
		{
			m_canvasGO.SetActive(value: true);
		}
	}

	private void OnHideTimer(object sender, EventArgs args)
	{
		if (m_canvasGO.activeSelf)
		{
			m_canvasGO.SetActive(value: false);
		}
	}

	private void OnStartTimer(object sender, EventArgs args)
	{
		StopAllCoroutines();
		IsRunning = true;
		StartCoroutine(TimerCoroutine());
	}

	private void OnStopTimer(object sender, EventArgs args)
	{
		IsRunning = false;
		StopAllCoroutines();
	}

	private void OnResetTimer(object sender, EventArgs args)
	{
		ElapsedTime = 0f;
		ReverseTimer = false;
		ReverseStartTime = 0f;
		SlowTime = false;
		TrackNegativeTimeAchievement = false;
		m_timerTextObj.text = $"{0:D2}:{0:D2}:{0:D2}";
	}

	private IEnumerator TimerCoroutine()
	{
		while (true)
		{
			if (SlowTime)
			{
				float slowTimeDelay = Time.time + 0.75f;
				while (Time.time <= slowTimeDelay)
				{
					yield return null;
				}
			}
			yield return null;
			bool flag = ElapsedTime < 0f;
			if (ReverseTimer)
			{
				flag = ReverseStartTime - Mathf.Abs(ElapsedTime) < 0f;
			}
			if (SlowTime)
			{
				ElapsedTime += 0.1373f;
			}
			else
			{
				ElapsedTime += Time.deltaTime;
			}
			if (TrackNegativeTimeAchievement)
			{
				bool flag2 = ElapsedTime < 0f;
				if (ReverseTimer)
				{
					flag2 = ReverseStartTime - Mathf.Abs(ElapsedTime) < 0f;
				}
				if (!flag && flag2)
				{
					StoreAPIManager.GiveAchievement(AchievementType.EndTimerFail, StoreType.All);
				}
			}
			m_timerTextObj.text = GetTimerString();
		}
	}

	public static string GetTimerString()
	{
		float num = Mathf.Abs(ElapsedTime);
		bool flag = ElapsedTime < 0f;
		if (ReverseTimer)
		{
			num = ReverseStartTime - num;
			flag = num < 0f;
			num = Mathf.Abs(num);
		}
		int num2 = (int)(num / 3600f);
		int num3 = (int)(num % 3600f / 60f);
		int num4 = (int)(num % 60f);
		int num5 = (int)((num - (float)(int)num) * 100f);
		if (num2 == 0)
		{
			return string.Format("{3}{0:D2}:{1:D2}:{2:D2}", num3, num4, num5, (!flag) ? "" : "-");
		}
		return string.Format("{4}{0}:{1:D2}:{2:D2}:{3:D2}", num2, num3, num4, num5, (!flag) ? "" : "-");
	}
}
