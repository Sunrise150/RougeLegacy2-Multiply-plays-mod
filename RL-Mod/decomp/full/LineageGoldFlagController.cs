using TMPro;
using UnityEngine;

public class LineageGoldFlagController : MonoBehaviour
{
	[SerializeField]
	private TMP_Text m_text;

	[SerializeField]
	private GameObject m_goldBannerGO;

	[SerializeField]
	private GameObject m_greyBannerGO;

	[SerializeField]
	private GameObject m_goldCoin;

	public void SetGoldGain(float goldGain)
	{
		if (goldGain != 0f)
		{
			m_text.gameObject.SetActive(value: true);
			m_goldCoin.SetActive(value: true);
			if (goldGain > 0f)
			{
				m_text.text = "+" + string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), Mathf.RoundToInt(goldGain * 100f));
				m_goldBannerGO.SetActive(value: true);
				m_greyBannerGO.SetActive(value: false);
			}
			else
			{
				m_text.text = string.Format(LocalizationManager.GetString("LOC_ID_GENERAL_UI_PERCENT_1", isFemale: false), Mathf.RoundToInt(goldGain * 100f));
				m_goldBannerGO.SetActive(value: false);
				m_greyBannerGO.SetActive(value: true);
			}
		}
		else
		{
			m_text.gameObject.SetActive(value: false);
			m_goldBannerGO.SetActive(value: false);
			m_greyBannerGO.SetActive(value: false);
			m_goldCoin.SetActive(value: false);
		}
	}
}
