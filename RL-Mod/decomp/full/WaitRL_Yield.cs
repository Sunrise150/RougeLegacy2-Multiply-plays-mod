using System.Collections;
using UnityEngine;

public class WaitRL_Yield : IEnumerator
{
	private float m_startingTime;

	private bool m_needsReset;

	private bool m_useUnscaledTime;

	private bool m_isPaused;

	private float m_pauseTime;

	public float WaitTime { get; private set; }

	public object Current => null;

	public WaitRL_Yield(float waitTime, bool useUnscaledTime = false)
	{
		CreateNew(waitTime, useUnscaledTime);
		m_needsReset = true;
	}

	public void Pause()
	{
		m_isPaused = true;
		if (m_useUnscaledTime)
		{
			m_pauseTime = Time.unscaledTime;
		}
		else
		{
			m_pauseTime = Time.time;
		}
	}

	public void Unpause()
	{
		m_isPaused = false;
		if (m_useUnscaledTime)
		{
			m_pauseTime = Time.unscaledTime - m_pauseTime;
		}
		else
		{
			m_pauseTime = Time.time - m_pauseTime;
		}
	}

	public bool MoveNext()
	{
		if (m_needsReset)
		{
			CreateNew(WaitTime, m_useUnscaledTime);
		}
		bool flag = false;
		if (m_isPaused)
		{
			return true;
		}
		flag = (m_useUnscaledTime ? (Time.unscaledTime < m_startingTime + m_pauseTime + WaitTime) : (Time.time < m_startingTime + m_pauseTime + WaitTime));
		if (!flag)
		{
			m_needsReset = true;
		}
		return flag;
	}

	public void CreateNew(float waitTime, bool useUnscaledTime = false)
	{
		m_isPaused = false;
		m_pauseTime = 0f;
		m_needsReset = false;
		m_useUnscaledTime = useUnscaledTime;
		if (!m_useUnscaledTime)
		{
			m_startingTime = Time.time;
		}
		else
		{
			m_startingTime = Time.unscaledTime;
		}
		WaitTime = waitTime;
	}

	public void Reset()
	{
		CreateNew(WaitTime);
	}
}
