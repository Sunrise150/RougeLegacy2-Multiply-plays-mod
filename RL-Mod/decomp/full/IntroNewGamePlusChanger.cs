using UnityEngine;

public class IntroNewGamePlusChanger : MonoBehaviour
{
	[SerializeField]
	private IntroSkyColourChanger m_introSkyColourChanger;

	[SerializeField]
	private GameObject m_castleGO;

	[SerializeField]
	private GameObject m_moonGO;

	[SerializeField]
	private GameObject[] m_crownGOArray;

	private bool m_changedToNewGamePlus;

	private void Awake()
	{
		m_castleGO.SetActive(value: false);
		m_moonGO.SetActive(value: true);
		UpdateCrowns(setToDefault: true);
	}

	public void UpdateMainMenu()
	{
		if (SaveManager.PlayerSaveData.HighestNGPlusBeaten > -1)
		{
			if (!m_changedToNewGamePlus)
			{
				if (HolidayLookController.IsHoliday(HolidayType.Christmas) || HolidayLookController.IsHoliday(HolidayType.Halloween))
				{
					m_introSkyColourChanger.SetSkyColor(m_introSkyColourChanger.PatchTypeToUse);
				}
				else
				{
					m_introSkyColourChanger.SetSkyColor(PatchType.NewGamePlus_BlueSky);
				}
				m_castleGO.SetActive(value: true);
				m_moonGO.SetActive(value: false);
				UpdateCrowns(setToDefault: false);
				m_changedToNewGamePlus = true;
			}
		}
		else if (m_changedToNewGamePlus)
		{
			m_introSkyColourChanger.SetSkyColor(m_introSkyColourChanger.PatchTypeToUse);
			m_castleGO.SetActive(value: false);
			m_moonGO.SetActive(value: true);
			UpdateCrowns(setToDefault: false);
			m_changedToNewGamePlus = false;
		}
	}

	private void UpdateCrowns(bool setToDefault)
	{
		int num = m_crownGOArray.Length;
		int num2 = Mathf.Clamp(SaveManager.PlayerSaveData.HighestNGPlusBeaten + 1, 0, num + 1);
		if (setToDefault)
		{
			num2 = 0;
		}
		for (int i = 0; i < num; i++)
		{
			if (i < num2)
			{
				m_crownGOArray[i].SetActive(value: true);
			}
			else
			{
				m_crownGOArray[i].SetActive(value: false);
			}
		}
	}
}
