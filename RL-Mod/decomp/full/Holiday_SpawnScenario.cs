using UnityEngine;

public class Holiday_SpawnScenario : SpawnScenario
{
	[SerializeField]
	private HolidayType m_holidayType;

	[SerializeField]
	private bool m_isNot;

	private string m_description = string.Empty;

	private HolidayType m_previousHolidayType;

	private bool m_previousIsNot;

	public override string GizmoDescription
	{
		get
		{
			if (m_previousHolidayType != m_holidayType || m_previousIsNot != m_isNot)
			{
				m_previousHolidayType = m_holidayType;
				m_previousIsNot = m_isNot;
				m_description = string.Format("HoTy: {0}{1}", m_isNot ? "!" : "", m_holidayType.ToString());
			}
			return m_description;
		}
	}

	public override SpawnScenarioType Type => SpawnScenarioType.Holiday;

	public HolidayType HolidayType
	{
		get
		{
			return m_holidayType;
		}
		set
		{
			m_holidayType = value;
		}
	}

	public bool IsNot
	{
		get
		{
			return m_isNot;
		}
		set
		{
			m_isNot = value;
		}
	}

	public override void RunIsTrueCheck(BaseRoom room)
	{
		IsTrue = HolidayLookController.IsHoliday(HolidayType) == !m_isNot;
	}

	public override void RunIsTrueCheck(GridPointManager gridPointManager)
	{
		IsTrue = HolidayLookController.IsHoliday(HolidayType) == !m_isNot;
	}
}
