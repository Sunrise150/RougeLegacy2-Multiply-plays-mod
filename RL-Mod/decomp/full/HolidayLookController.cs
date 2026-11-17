using System;
using UnityEngine;

public class HolidayLookController : MonoBehaviour
{
	[SerializeField]
	private HolidayType m_holidayType;

	[SerializeField]
	private Material m_materialToApply;

	[SerializeField]
	private Renderer m_rendererToChange;

	private static bool m_holidayTypeIsCached;

	private static HolidayType m_cachedHolidayType;

	private Material m_storedMat;

	private bool m_holidayLookApplied;

	private const int LEAP_YEAR_DAY = 58;

	private void Awake()
	{
		m_storedMat = m_rendererToChange.sharedMaterial;
	}

	private void OnEnable()
	{
		if (IsHoliday(m_holidayType))
		{
			m_rendererToChange.sharedMaterial = m_materialToApply;
			m_holidayLookApplied = true;
		}
	}

	private void OnDisable()
	{
		if (m_holidayLookApplied)
		{
			m_rendererToChange.sharedMaterial = m_storedMat;
			m_holidayLookApplied = false;
		}
	}

	public static bool IsHoliday(HolidayType holidayType)
	{
		switch (holidayType)
		{
		case HolidayType.Halloween:
			if (TraitManager.IsInitialized && TraitManager.IsTraitActive(TraitType.HalloweenHoliday))
			{
				return true;
			}
			break;
		case HolidayType.Christmas:
			if (TraitManager.IsInitialized && TraitManager.IsTraitActive(TraitType.ChristmasHoliday))
			{
				return true;
			}
			break;
		}
		return holidayType == GetHolidayType();
	}

	private static HolidayType GetHolidayType()
	{
		if (!m_holidayTypeIsCached)
		{
			DateTime now = DateTime.Now;
			int dayOfYear = now.DayOfYear;
			bool flag = DateTime.IsLeapYear(now.Year);
			int num = (flag ? 366 : 365);
			int num2 = 999;
			int num3 = 0;
			HolidayType[] array = Enum.GetValues(typeof(HolidayType)) as HolidayType[];
			foreach (HolidayType holidayType in array)
			{
				if (holidayType != 0)
				{
					switch (holidayType)
					{
					case HolidayType.Halloween:
						num2 = 301;
						num3 = 7;
						break;
					case HolidayType.Christmas:
						num2 = 354;
						num3 = 18;
						break;
					}
					if (flag && dayOfYear > 58)
					{
						num2++;
					}
					int num4 = num2;
					int num5 = num2 + num3;
					bool flag2 = false;
					if ((num5 <= num) ? (dayOfYear >= num4 && dayOfYear <= num5) : (dayOfYear >= num4 || dayOfYear <= num5 - num))
					{
						m_cachedHolidayType = holidayType;
						break;
					}
				}
			}
			m_holidayTypeIsCached = true;
		}
		return m_cachedHolidayType;
	}
}
