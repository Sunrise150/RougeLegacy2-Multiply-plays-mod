using System;
using System.Collections.Generic;
using UnityEngine;

public class RLTimeScale
{
	private static Dictionary<TimeScaleType, float> m_timeScaleDict;

	private static TimeScaleType[] m_typeArray;

	public static TimeScaleType[] TypeArray
	{
		get
		{
			if (m_typeArray == null)
			{
				m_typeArray = Enum.GetValues(typeof(TimeScaleType)) as TimeScaleType[];
			}
			return m_typeArray;
		}
	}

	public static bool IsInitialized { get; private set; }

	public static float TimeScale => Time.timeScale;

	public static TimeScaleType GetAvailableSlowTimeStack()
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		int num = 1;
		int num2 = 5;
		for (int i = num; i < num + num2; i++)
		{
			TimeScaleType timeScaleType = (TimeScaleType)i;
			if (m_timeScaleDict[timeScaleType] == 1f)
			{
				return timeScaleType;
			}
		}
		Debug.Log("No available SlowTimeScale available.  Returning first on stack.");
		return TimeScaleType.SlowTimeEffectStack1;
	}

	private static void Initialize()
	{
		m_timeScaleDict = new Dictionary<TimeScaleType, float>(TypeArray.Length);
		TimeScaleType[] typeArray = TypeArray;
		foreach (TimeScaleType key in typeArray)
		{
			m_timeScaleDict.Add(key, 1f);
		}
		IsInitialized = true;
	}

	public static void SetTimeScale(TimeScaleType timeScaleType, float value)
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		m_timeScaleDict[timeScaleType] = value;
		float num = float.MaxValue;
		bool flag = false;
		TimeScaleType[] typeArray = TypeArray;
		foreach (TimeScaleType key in typeArray)
		{
			float num2 = m_timeScaleDict[key];
			if (num2 != 1f && num2 < num)
			{
				num = m_timeScaleDict[key];
				flag = true;
			}
		}
		if (!flag && Time.timeScale != 1f)
		{
			Time.timeScale = 1f;
			Time.fixedDeltaTime = 1f / 60f;
			Time.maximumDeltaTime = 1f / 30f;
		}
		else if (flag && Time.timeScale != num)
		{
			Time.timeScale = num;
			if (num != 0f)
			{
				Time.fixedDeltaTime = 1f / 60f * Time.timeScale;
				return;
			}
			Time.fixedDeltaTime = 1f / 60f;
			Time.maximumDeltaTime = 1f / 30f;
		}
	}

	public static float GetTimeScale(TimeScaleType timeScaleType)
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		return m_timeScaleDict[timeScaleType];
	}

	public static void SetAllTimeScale(float value)
	{
		if (!IsInitialized)
		{
			Initialize();
		}
		TimeScaleType[] typeArray = TypeArray;
		for (int i = 0; i < typeArray.Length; i++)
		{
			SetTimeScale(typeArray[i], value);
		}
	}

	public static void Reset()
	{
		SetAllTimeScale(1f);
	}
}
