using System;
using System.Collections.Generic;
using Rewired.Integration.UnityUI;
using UnityEngine;

public class MouseInputManager : MonoBehaviour
{
	[SerializeField]
	private RewiredStandaloneInputModule m_inputModule;

	private List<WindowID> m_openWindows;

	private Action<MonoBehaviour, EventArgs> m_onWindowOpenOrClose;

	private void Awake()
	{
		m_onWindowOpenOrClose = OnWindowOpenOrClose;
	}

	private void OnWindowOpenOrClose(MonoBehaviour sender, EventArgs eventArgs)
	{
		if (m_openWindows == null)
		{
			m_openWindows = new List<WindowID>();
		}
		if (eventArgs is WindowStateChangeEventArgs windowStateChangeEventArgs)
		{
			if (windowStateChangeEventArgs.IsOpen)
			{
				if (!m_openWindows.Contains(windowStateChangeEventArgs.Window))
				{
					m_openWindows.Add(windowStateChangeEventArgs.Window);
					m_inputModule.allowMouseInput = false;
				}
				return;
			}
			if (m_openWindows.Contains(windowStateChangeEventArgs.Window))
			{
				m_openWindows.Remove(windowStateChangeEventArgs.Window);
			}
			if (m_openWindows.Count == 0)
			{
				m_inputModule.allowMouseInput = true;
			}
		}
		else
		{
			Debug.LogFormat("<color=red>{0}: Unable to cast eventArgs as UIWindowStateChangeEventArgs.</color>", Time.frameCount);
		}
	}
}
