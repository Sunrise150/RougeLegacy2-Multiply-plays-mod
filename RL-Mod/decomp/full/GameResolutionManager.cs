using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public static class GameResolutionManager
{
	private delegate bool MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData);

	public struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 4)]
	public class MONITORINFOEX
	{
		public int cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));

		public RECT rcMonitor;

		public RECT rcWork;

		public int dwFlags;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 32, ArraySubType = UnmanagedType.U2)]
		public char[] szDevice = new char[32];
	}

	public struct POINTSTRUCT
	{
		public int x;

		public int y;

		public POINTSTRUCT(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}

	public class DisplayInfo
	{
		public string Availability { get; set; }

		public int ScreenHeight { get; set; }

		public int ScreenWidth { get; set; }

		public RECT MonitorArea { get; set; }

		public RECT WorkArea { get; set; }
	}

	private static List<DisplayInfo> m_displayInfoList = new List<DisplayInfo>();

	public static int ActiveDisplayIndex { get; private set; } = -1;


	public static Vector2Int Resolution { get; private set; }

	public static FullScreenMode FullscreenMode { get; private set; }

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	internal static extern void MoveWindow(IntPtr hwnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

	[DllImport("user32.dll")]
	private static extern IntPtr GetActiveWindow();

	[DllImport("user32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	internal static extern bool GetWindowRect(IntPtr hWnd, ref RECT rect);

	[DllImport("user32.dll")]
	private static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	public static extern bool GetMonitorInfo(IntPtr hmonitor, [In][Out] MONITORINFOEX info);

	[DllImport("User32.dll", ExactSpelling = true)]
	public static extern IntPtr MonitorFromPoint(POINTSTRUCT pt, int flags);

	public static void SetResolution(int xResolution, int yResolution, FullScreenMode fullScreenMode, bool saveConfig = true)
	{
		Screen.SetResolution(xResolution, yResolution, fullScreenMode, 0);
		Resolution = new Vector2Int(xResolution, yResolution);
		FullscreenMode = fullScreenMode;
		SaveManager.ConfigData.ScreenWidth = xResolution;
		SaveManager.ConfigData.ScreenHeight = yResolution;
		SaveManager.ConfigData.ScreenMode = (int)FullscreenMode;
		if (saveConfig)
		{
			SaveManager.SaveConfigFile();
		}
		Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.ResolutionChanged, null, null);
	}

	public static void SetVsyncEnable(bool enable)
	{
		if (enable)
		{
			Debug.Log("Enabling Vsync");
			if (QualitySettings.vSyncCount != 1)
			{
				QualitySettings.vSyncCount = 1;
			}
		}
		else
		{
			Debug.Log("Disabling Vsync");
			if (QualitySettings.vSyncCount != 0)
			{
				QualitySettings.vSyncCount = 0;
			}
		}
	}

	public static void SetPrimaryDisplay(int displayIndex)
	{
		List<DisplayInfo> displays = GetDisplays();
		if (displayIndex >= displays.Count)
		{
			Debug.Log("Failed to set Display Index: " + displayIndex + " as primary display. Index out of bounds. Using index 0.");
			displayIndex = 0;
			SaveManager.ConfigData.PrimaryDisplay = displayIndex;
			SaveManager.SaveConfigFile();
		}
		if (ActiveDisplayIndex == -1)
		{
			ActiveDisplayIndex = PlayerPrefs.GetInt("UnitySelectMonitor");
		}
		if (ActiveDisplayIndex == displayIndex)
		{
			return;
		}
		ActiveDisplayIndex = displayIndex;
		PlayerPrefs.SetInt("UnitySelectMonitor", displayIndex);
		if (SaveManager.ConfigData.ScreenMode == 0)
		{
			SharedGameObjects_Loader sharedGameObjects_Loader = UnityEngine.Object.FindObjectOfType<SharedGameObjects_Loader>();
			if ((bool)sharedGameObjects_Loader)
			{
				sharedGameObjects_Loader.StartCoroutine(MoveWindowFullscreenCoroutine(displayIndex));
			}
			return;
		}
		DisplayInfo displayInfo = displays[displayIndex];
		int screenWidth = SaveManager.ConfigData.ScreenWidth;
		int screenHeight = SaveManager.ConfigData.ScreenHeight;
		bool flag = false;
		if (displayInfo.ScreenWidth < screenWidth)
		{
			screenWidth = displayInfo.ScreenWidth;
			flag = true;
		}
		if (displayInfo.ScreenHeight < screenHeight)
		{
			screenHeight = displayInfo.ScreenHeight;
			flag = true;
		}
		if (flag)
		{
			SaveManager.ConfigData.ScreenWidth = screenWidth;
			SaveManager.ConfigData.ScreenHeight = screenHeight;
			SaveManager.SaveConfigFile();
		}
		if (SaveManager.ConfigData.ScreenMode == 3)
		{
			int targetX = displayInfo.WorkArea.left + (displayInfo.WorkArea.right - displayInfo.WorkArea.left) / 2 - screenWidth / 2;
			int targetY = displayInfo.WorkArea.top + (displayInfo.WorkArea.bottom - displayInfo.WorkArea.top) / 2 - screenHeight / 2;
			MoveToDisplay(screenWidth, screenHeight, targetX, targetY);
		}
		else
		{
			MoveToDisplay(screenWidth, screenHeight, displayInfo.WorkArea.left, displayInfo.WorkArea.top);
		}
		Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.PrimaryDisplayChanged, null, null);
	}

	private static IEnumerator MoveWindowFullscreenCoroutine(int displayIndex)
	{
		int screenWidth = SaveManager.ConfigData.ScreenWidth;
		int screenHeight = SaveManager.ConfigData.ScreenHeight;
		SetResolution(screenWidth, screenHeight, FullScreenMode.FullScreenWindow, saveConfig: false);
		float delay = Time.unscaledTime + 1f;
		while (Time.unscaledTime < delay)
		{
			yield return null;
		}
		DisplayInfo displayInfo = GetDisplays()[displayIndex];
		bool saveConfig = false;
		if (displayInfo.ScreenWidth < screenWidth)
		{
			screenWidth = displayInfo.ScreenWidth;
			saveConfig = true;
		}
		if (displayInfo.ScreenHeight < screenHeight)
		{
			screenHeight = displayInfo.ScreenHeight;
			saveConfig = true;
		}
		MoveToDisplay(screenWidth, screenHeight, displayInfo.WorkArea.left, displayInfo.WorkArea.top);
		SetResolution(screenWidth, screenHeight, FullScreenMode.ExclusiveFullScreen, saveConfig);
		Messenger<SceneMessenger, SceneEvent>.Broadcast(SceneEvent.PrimaryDisplayChanged, null, null);
	}

	public static List<DisplayInfo> GetDisplays()
	{
		m_displayInfoList.Clear();
		EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate(IntPtr hMonitor, IntPtr hdcMonitor, ref RECT lprcMonitor, IntPtr dwData)
		{
			MONITORINFOEX mONITORINFOEX = new MONITORINFOEX();
			mONITORINFOEX.cbSize = Marshal.SizeOf(mONITORINFOEX);
			if (GetMonitorInfo(hMonitor, mONITORINFOEX))
			{
				DisplayInfo item = new DisplayInfo
				{
					ScreenWidth = mONITORINFOEX.rcMonitor.right - mONITORINFOEX.rcMonitor.left,
					ScreenHeight = mONITORINFOEX.rcMonitor.bottom - mONITORINFOEX.rcMonitor.top,
					MonitorArea = mONITORINFOEX.rcMonitor,
					WorkArea = mONITORINFOEX.rcWork,
					Availability = mONITORINFOEX.dwFlags.ToString()
				};
				m_displayInfoList.Add(item);
			}
			return true;
		}, IntPtr.Zero);
		return m_displayInfoList;
	}

	private static void MoveToDisplay(int newWidth, int newHeight, int targetX, int targetY)
	{
		MoveWindow(GetActiveWindow(), targetX, targetY, newWidth, newHeight, bRepaint: true);
	}
}
